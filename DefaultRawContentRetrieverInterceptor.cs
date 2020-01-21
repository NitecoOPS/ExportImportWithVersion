using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Transfer.Internal;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Cache;

namespace ExportImportWithVersion.DataExporter
{
    public class DefaultRawContentRetrieverInterceptor : DefaultRawContentRetriever, IRawContentRetrieverEx
    {
        private IRawContentRetriever defaultRawContentRetriever;
        private IContentRepository contentRepository;
        private IContentTypeRepository contentTypeRepository;
        private ILanguageBranchRepository languageBranchRepository;
        private IContentCacheKeyCreator contentCacheKeyCreator;
        private ISynchronizedObjectInstanceCache synchronizedObjectInstanceCache;
        private IRawPropertyRetriever rawPropertyRetriever;

        public DefaultRawContentRetrieverInterceptor(IRawContentRetriever defaultRawContentRetriever
            , IContentRepository contentRepository
            , IContentTypeRepository contentTypeRepository
            , ILanguageBranchRepository languageBranchRepository
            , IContentCacheKeyCreator contentCacheKeyCreator
            , ISynchronizedObjectInstanceCache synchronizedObjectInstanceCache
            , IRawPropertyRetriever rawPropertyRetriever)
            : base(contentRepository, contentTypeRepository, languageBranchRepository
                 , contentCacheKeyCreator, synchronizedObjectInstanceCache, rawPropertyRetriever)
        {
            this.defaultRawContentRetriever = defaultRawContentRetriever;
            this.contentRepository = contentRepository;
            this.contentTypeRepository = contentTypeRepository;
            this.languageBranchRepository = languageBranchRepository;
            this.contentCacheKeyCreator = contentCacheKeyCreator;
            this.synchronizedObjectInstanceCache = synchronizedObjectInstanceCache;
            this.rawPropertyRetriever = rawPropertyRetriever;
        }
    }
}