﻿using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public sealed partial class DonateService : BaseService<Donate>, IDonateService
{
	public DonateService(IBaseRepository<Donate> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class FastShareService : BaseService<FastShare>, IFastShareService
{
	public FastShareService(IBaseRepository<FastShare> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class InternalMessageService : BaseService<InternalMessage>, IInternalMessageService
{
	public InternalMessageService(IBaseRepository<InternalMessage> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class LinksService : BaseService<Links>, ILinksService
{
	public LinksService(IBaseRepository<Links> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class LinkLoopbackService : BaseService<LinkLoopback>, ILinkLoopbackService
{
	public LinkLoopbackService(IBaseRepository<LinkLoopback> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class LoginRecordService : BaseService<LoginRecord>, ILoginRecordService
{
	public LoginRecordService(IBaseRepository<LoginRecord> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class MiscService : BaseService<Misc>, IMiscService
{
	public MiscService(IBaseRepository<Misc> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class NoticeService : BaseService<Notice>, INoticeService
{
	public NoticeService(IBaseRepository<Notice> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class PostHistoryVersionService : BaseService<PostHistoryVersion>, IPostHistoryVersionService
{
	public PostHistoryVersionService(IBaseRepository<PostHistoryVersion> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class SeminarService : BaseService<Seminar>, ISeminarService
{
	public SeminarService(IBaseRepository<Seminar> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class SystemSettingService : BaseService<SystemSetting>, ISystemSettingService
{
	public SystemSettingService(IBaseRepository<SystemSetting> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class PostMergeRequestService : BaseService<PostMergeRequest>, IPostMergeRequestService
{
	public PostMergeRequestService(IBaseRepository<PostMergeRequest> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class VariablesService : BaseService<Variables>, IVariablesService
{
	public VariablesService(IBaseRepository<Variables> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class PostVisitRecordService : BaseService<PostVisitRecord>, IPostVisitRecordService
{
	public PostVisitRecordService(IBaseRepository<PostVisitRecord> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class PostVisitRecordStatsService : BaseService<PostVisitRecordStats>, IPostVisitRecordStatsService
{
	public PostVisitRecordStatsService(IBaseRepository<PostVisitRecordStats> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class AdvertisementClickRecordService : BaseService<AdvertisementClickRecord>, IAdvertisementClickRecordService
{
	public AdvertisementClickRecordService(IBaseRepository<AdvertisementClickRecord> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}

public sealed partial class PostTagService : BaseService<PostTag>, IPostTagService
{
	public PostTagService(IBaseRepository<PostTag> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
	{
	}
}