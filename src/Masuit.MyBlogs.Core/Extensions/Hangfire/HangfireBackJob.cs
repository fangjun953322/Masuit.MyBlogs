﻿using FreeRedis;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Common.Mails;
using Masuit.Tools.Logging;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Masuit.MyBlogs.Core.Extensions.Hangfire;

/// <summary>
/// hangfire后台任务
/// </summary>
public sealed class HangfireBackJob : Disposable, IHangfireBackJob
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IWebHostEnvironment _hostEnvironment;
	private readonly IServiceScope _serviceScope;
	private readonly IRedisClient _redisClient;
	private readonly IConfiguration _configuration;
	private readonly IMailSender _mailSender;

	/// <summary>
	/// hangfire后台任务
	/// </summary>
	public HangfireBackJob(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, IWebHostEnvironment hostEnvironment, IRedisClient redisClient, IConfiguration configuration, IMailSender mailSender)
	{
		_httpClientFactory = httpClientFactory;
		_hostEnvironment = hostEnvironment;
		_redisClient = redisClient;
		_configuration = configuration;
		_mailSender = mailSender;
		_serviceScope = serviceProvider.CreateScope();
	}

	/// <summary>
	/// 登录记录
	/// </summary>
	/// <param name="userInfo"></param>
	/// <param name="ip"></param>
	/// <param name="type"></param>
	public void LoginRecord(UserInfoDto userInfo, string ip, LoginType type)
	{
		var record = new LoginRecord()
		{
			IP = ip,
			LoginTime = DateTime.Now,
			LoginType = type,
			PhysicAddress = ip.GetIPLocation()
		};
		var userInfoService = _serviceScope.ServiceProvider.GetRequiredService<IUserInfoService>();
		var settingService = _serviceScope.ServiceProvider.GetRequiredService<ISystemSettingService>();
		var u = userInfoService.GetByUsername(userInfo.Username);
		u.LoginRecord.Add(record);
		userInfoService.SaveChanges();
		var content = new Template(File.ReadAllText(Path.Combine(_hostEnvironment.WebRootPath, "template", "login.html")))
			.Set("name", u.Username)
			.Set("time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
			.Set("ip", record.IP)
			.Set("address", record.PhysicAddress).Render();
		_mailSender.Send(settingService.Get(s => s.Name.Equals("Title")).Value + "账号登录通知", content, settingService.Get(s => s.Name.Equals("ReceiveEmail")).Value, "127.0.0.1");
	}

	/// <summary>
	/// 文章定时发布
	/// </summary>
	/// <param name="p"></param>
	public void PublishPost(Post p)
	{
		p.Status = Status.Published;
		p.PostDate = DateTime.Now;
		p.ModifyDate = DateTime.Now;
		var postService = _serviceScope.ServiceProvider.GetRequiredService<IPostService>();
		var post = postService.GetById(p.Id);
		if (post is null)
		{
			postService.AddEntitySaved(p);
		}
		else
		{
			post.Status = Status.Published;
			post.PostDate = DateTime.Now;
			post.ModifyDate = DateTime.Now;
			postService.SaveChanges();
		}
	}

	/// <summary>
	/// 文章访问记录
	/// </summary>
	/// <param name="pid"></param>
	/// <param name="ip"></param>
	/// <param name="refer"></param>
	/// <param name="url"></param>
	public void RecordPostVisit(int pid, string ip, string refer, string url)
	{
		var lastQuarter = DateTime.Now.AddMonths(-6);
		var last3Year = DateTime.Now.AddYears(-3);
		var recordService = _serviceScope.ServiceProvider.GetRequiredService<IPostVisitRecordService>();
		var recordStatsService = _serviceScope.ServiceProvider.GetRequiredService<IPostVisitRecordStatsService>();
		var postService = _serviceScope.ServiceProvider.GetRequiredService<IPostService>();
		recordService.GetQuery(b => b.Time < lastQuarter).DeleteFromQuery();
		recordStatsService.GetQuery(b => b.Date < last3Year).DeleteFromQuery();
		var post = postService.GetById(pid);
		if (post == null)
		{
			return;
		}

		post.TotalViewCount += 1;
		var count = recordService.Count(e => e.PostId == pid);
		if (count > 0)
		{
			post.AverageViewCount = count / (recordService.GetQuery(e => e.PostId == pid).MaxOrDefault(e => e.Time) - recordService.GetQuery(e => e.PostId == pid).MinOrDefault(e => e.Time)).TotalDays;
		}
		else
		{
			post.AverageViewCount = 1;
		}

		recordService.AddEntity(new PostVisitRecord()
		{
			IP = ip,
			Referer = refer,
			Location = ip.GetIPLocation(),
			Time = DateTime.Now,
			RequestUrl = url,
			PostId = pid
		});
		var stats = recordStatsService.Get(e => e.PostId == pid && e.Date >= DateTime.Today);
		if (stats != null)
		{
			stats.Count = recordService.Count(e => e.PostId == pid & e.Time >= DateTime.Today) + 1;
			stats.UV = recordService.GetQuery(e => e.PostId == pid & e.Time >= DateTime.Today).Select(e => e.IP).Distinct().Count() + 1;
		}
		else
		{
			recordStatsService.AddEntity(new PostVisitRecordStats()
			{
				Count = 1,
				UV = 1,
				Date = DateTime.Today,
				PostId = pid
			});
		}

		postService.SaveChanges();
	}

	/// <summary>
	/// 每天的任务
	/// </summary>
	public void EverydayJob()
	{
		CommonHelper.IPErrorTimes.RemoveWhere(kv => kv.Value < 100); //将访客访问出错次数少于100的移开
		var time = DateTime.Now.AddMonths(-1);
		var searchDetailsService = _serviceScope.ServiceProvider.GetRequiredService<ISearchDetailsService>();
		var advertisementService = _serviceScope.ServiceProvider.GetRequiredService<IAdvertisementService>();
		var noticeService = _serviceScope.ServiceProvider.GetRequiredService<INoticeService>();
		var postService = _serviceScope.ServiceProvider.GetRequiredService<IPostService>();
		searchDetailsService.DeleteEntitySaved(s => s.SearchTime < time);
		TrackData.DumpLog();
		advertisementService.GetQuery(a => DateTime.Now >= a.ExpireTime).ExecuteUpdate(s => s.SetProperty(a => a.Status, Status.Unavailable));
		noticeService.GetQuery(n => n.NoticeStatus == NoticeStatus.UnStart && n.StartTime < DateTime.Now).ExecuteUpdate(s => s.SetProperty(e => e.NoticeStatus, NoticeStatus.Normal).SetProperty(e => e.PostDate, DateTime.Now).SetProperty(e => e.ModifyDate, DateTime.Now));
		noticeService.GetQuery(n => n.NoticeStatus == NoticeStatus.Normal && n.EndTime < DateTime.Now).ExecuteUpdate(s => s.SetProperty(e => e.NoticeStatus, NoticeStatus.Expired).SetProperty(e => e.ModifyDate, DateTime.Now));
		postService.GetQuery(p => p.ExpireAt < DateTime.Now && p.Status == Status.Published).ExecuteUpdate(s => s.SetProperty(p => p.Status, Status.Takedown));
	}

	/// <summary>
	/// 每月的任务
	/// </summary>
	public void EverymonthJob()
	{
		var advertisementService = _serviceScope.ServiceProvider.GetRequiredService<IAdvertisementService>();
		advertisementService.GetAll().ExecuteUpdate(s => s.SetProperty(a => a.DisplayCount, 0));
	}

	public void CheckAdvertisements()
	{
		var advertisementService = _serviceScope.ServiceProvider.GetRequiredService<IAdvertisementService>();
		var client = _httpClientFactory.CreateClient();
		client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36 Edg/93.0.961.47");
		client.DefaultRequestHeaders.Add("X-Forwarded-For", "114.114.114.114");
		client.DefaultRequestHeaders.Add("X-Forwarded-Host", "114.114.114.114");
		client.DefaultRequestHeaders.Add("X-Real-IP", "114.114.114.114");
		client.DefaultRequestHeaders.Referrer = new Uri("https://baidu.com");
		client.Timeout = TimeSpan.FromSeconds(10);
		var baseAddr = bool.Parse(_configuration["Https:Enabled"]) ? $"https://127.1:{_configuration["Https:Port"]}" : $"http://127.1:{_configuration["Port"]}";
		advertisementService.GetQuery(a => a.Status == Status.Available).AsParallel().ForAll(e =>
		{
			var url = e.Url;
			if (e.Url.StartsWith("/"))
			{
				url = baseAddr + e.Url;
			}

			using var cts = new CancellationTokenSource(client.Timeout);
			client.GetAsync(url, cts.Token).ContinueWith(t =>
			{
				if (t.IsCanceled || t.IsFaulted)
				{
					LogManager.Info($"广告【[{e.Id}] {e.Title}】因访问超时被自动下架！");
					e.Status = Status.Unavailable;
				}

				if (t.Result.StatusCode == HttpStatusCode.NotFound)
				{
					LogManager.Info($"广告【[{e.Id}] {e.Title}】因广告链接404被自动下架！");
					e.Status = Status.Unavailable;
				}
			}).Wait();
		});
		advertisementService.SaveChanges();
	}

	/// <summary>
	/// 检查友链
	/// </summary>
	public void CheckLinks()
	{
		var client = _httpClientFactory.CreateClient();
		client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36 Edg/93.0.961.47");
		client.DefaultRequestHeaders.Add("X-Forwarded-For", "1.1.1.1");
		client.DefaultRequestHeaders.Add("X-Forwarded-Host", "1.1.1.1");
		client.DefaultRequestHeaders.Add("X-Real-IP", "1.1.1.1");
		client.DefaultRequestHeaders.Referrer = new Uri("https://google.com");
		client.Timeout = TimeSpan.FromSeconds(10);
		var linksService = _serviceScope.ServiceProvider.GetRequiredService<ILinksService>();
		linksService.GetQuery(l => !l.Except).AsParallel().ForAll(link =>
		{
			var prev = link.Status;
			using var cts = new CancellationTokenSource(client.Timeout);
			client.GetStringAsync(_configuration["HttpClientProxy:UriPrefix"] + link.Url, cts.Token).ContinueWith(t =>
			{
				if (t.IsCanceled || t.IsFaulted)
				{
					link.Status = Status.Unavailable;
				}
				else
				{
					link.Status = !t.Result.Contains(CommonHelper.SystemSettings["Domain"].Split("|")) ? Status.Unavailable : Status.Available;
				}

				if (link.Status != prev)
				{
					link.UpdateTime = DateTime.Now;
				}
			}).Wait();
		});
		linksService.SaveChanges();
	}

	/// <summary>
	/// 更新友链权重
	/// </summary>
	/// <param name="referer"></param>
	/// <param name="ip"></param>
	public void UpdateLinkWeight(string referer, string ip)
	{
		var linksService = _serviceScope.ServiceProvider.GetRequiredService<ILinksService>();
		var loopbackService = _serviceScope.ServiceProvider.GetRequiredService<ILinkLoopbackService>();
		var list = linksService.GetQuery(l => referer.Contains(l.UrlBase)).ToList();
		foreach (var link in list)
		{
			link.Loopbacks.Add(new LinkLoopback()
			{
				IP = ip,
				Referer = referer,
				Time = DateTime.Now
			});
		}
		var time = DateTime.Now.AddMonths(-1);
		loopbackService.GetQuery(b => b.Time < time).DeleteFromQuery();
		linksService.SaveChanges();
	}

	/// <summary>
	/// 重建Lucene索引库
	/// </summary>
	public void CreateLuceneIndex()
	{
		var searchEngine = _serviceScope.ServiceProvider.GetRequiredService<ISearchEngine<DataContext>>();
		var postService = _serviceScope.ServiceProvider.GetRequiredService<IPostService>();
		searchEngine.LuceneIndexer.DeleteAll();
		searchEngine.CreateIndex(new List<string>()
		{
			nameof(DataContext.Post),
		});
		var list = postService.GetQuery(p => p.Status != Status.Published || p.LimitMode == RegionLimitMode.OnlyForSearchEngine).ToList();
		searchEngine.LuceneIndexer.Delete(list);
	}

	/// <summary>
	/// 搜索统计
	/// </summary>
	public void StatisticsSearchKeywords()
	{
		var searchDetailsService = _serviceScope.ServiceProvider.GetRequiredService<ISearchDetailsService>();
		_redisClient.Set("SearchRank:Month", searchDetailsService.GetRanks(DateTime.Today.AddMonths(-1)));
		_redisClient.Set("SearchRank:Week", searchDetailsService.GetRanks(DateTime.Today.AddDays(-7)));
		_redisClient.Set("SearchRank:Today", searchDetailsService.GetRanks(DateTime.Today));
	}

	/// <summary>
	/// 释放
	/// </summary>
	/// <param name="disposing"></param>
	public override void Dispose(bool disposing)
	{
		_serviceScope.Dispose();
	}
}