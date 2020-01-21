using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Transfer;
using EPiServer.Core.Transfer.Internal;
using EPiServer.DataAbstraction;
using EPiServer.Enterprise;
using EPiServer.Enterprise.Internal;
using EPiServer.Enterprise.Transfer.Internal;
using EPiServer.Framework.Cache;
using EPiServer.Security;
using EPiServer.Web;

namespace GrantThornton.Interface.Web.Business.DataExporter
{
    internal class DefaultDataImporterInterceptor : DefaultDataImporter, IDataImporter
    {
        private IDataImporter defaultDataImporter;
        private IDataExportEventsRaiser dataExportEventsRaiser;
        private IDataExportEvents dataExportEvents;
        private IContentCacheKeyCreator contentCacheKeyCreator;
        private ISynchronizedObjectInstanceCache synchronizedObjectInstanceCache;
        private IContentRepository contentRepository;
        private IPermanentLinkMapper permanentLinkMapper;
        private IContentTypeRepository contentTypeRepository;
        private IContentProviderManager contentProviderManager;
        private IAvailableSettingsRepository availableSettingsRepository;
        private IContentExporter contentExporter;
        private PropertyCategoryTransform propertyCategoryTransform;
        private ContentRootRepository contentRootRepository;
        private ISiteDefinitionRepository siteDefinitionRepository;
        private IMimeTypeResolver mimeTypeResolver;
        private IContentCacheRemover contentCacheRemover;
        private IPrincipalAccessor principalAccessor;
        private ContentTypeAvailabilityService contentTypeAvailabilityService;
        private IContentImporter contentImporter;
        private ContentOptions contentOptions;
        private ISiteDefinitionResolver siteDefinitionResolver;
        private IDataImportEvents dataImportEvents;
        private IDataImportEventsRaiser dataImportEventsRaiser;
        private IContentTransferValidator contentTransferValidator;

        public DefaultDataImporterInterceptor(IDataImporter defaultDataImporter
            , IContentCacheRemover contentCacheRemover
            , IPrincipalAccessor principalAccessor
            , IDataImportEvents dataImportEvents
            , IDataImportEventsRaiser dataImportEventsRaiser
            , IContentRepository contentRepository
            , IPermanentLinkMapper permanentLinkMapper
            , IContentTypeRepository contentTypeRepository
            , ContentTypeAvailabilityService contentTypeAvailabilityService
            , IAvailableSettingsRepository availableSettingsRepository
            , IContentImporter contentImporter
            , IContentTransferValidator contentTransferValidator
            , PropertyCategoryTransform propertyCategoryTransform
            , ContentRootRepository contentRootRepository
            , ISiteDefinitionRepository siteDefinitionRepository
            , ContentOptions contentOptions
            , ISiteDefinitionResolver siteDefinitionResolver)
            : base(contentCacheRemover, principalAccessor, dataImportEvents
                  , dataImportEventsRaiser, contentRepository
                  , permanentLinkMapper, contentTypeRepository
                  , contentTypeAvailabilityService, availableSettingsRepository
                  , contentImporter, contentTransferValidator
                  , propertyCategoryTransform, contentRootRepository
                  , siteDefinitionRepository, contentOptions, siteDefinitionResolver)
        {
            this.defaultDataImporter = defaultDataImporter;
            this.contentCacheRemover = contentCacheRemover;
            this.principalAccessor = principalAccessor;
            this.dataImportEvents = dataImportEvents;
            this.dataImportEventsRaiser = dataImportEventsRaiser;
            this.contentRepository = contentRepository;
            this.permanentLinkMapper = permanentLinkMapper;
            this.contentTypeRepository = contentTypeRepository;
            this.contentTypeAvailabilityService = contentTypeAvailabilityService;
            this.availableSettingsRepository = availableSettingsRepository;
            this.contentImporter = contentImporter;
            this.contentTransferValidator = contentTransferValidator;
            this.propertyCategoryTransform = propertyCategoryTransform;
            this.contentRootRepository = contentRootRepository;
            this.siteDefinitionRepository = siteDefinitionRepository;
            this.contentOptions = contentOptions;
            this.siteDefinitionResolver = siteDefinitionResolver;
        }
    }
}