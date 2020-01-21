using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Internal;
using EPiServer.Core.Transfer;
using EPiServer.Core.Transfer.Internal;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess.Internal;
using EPiServer.Enterprise;
using EPiServer.Enterprise.Internal;
using EPiServer.Enterprise.Transfer.Internal;
using EPiServer.Framework;
using EPiServer.Framework.Cache;
using EPiServer.Framework.Initialization;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using GrantThornton.Interface.Web.Business.DataExporter;

namespace ExportImportWithVersion.DataExporter
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class DataExporterInitializationModule : IConfigurableModule, IInitializableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.StructureMap().Configure(x =>
            {
                x.For<ITransferExportOptionsEx>().Use<DefaultTransferExportOptionsEx>().Singleton();
            });
            context.ConfigurationComplete += (o, e) =>
            {
                e.Services.Intercept<IDataImporter>((locator, defaultDataImporter) =>
                    new DefaultDataImporterInterceptor(defaultDataImporter
                        , locator.GetInstance<IContentCacheRemover>()
                        , locator.GetInstance<IPrincipalAccessor>()
                        , locator.GetInstance<IDataImportEvents>()
                        , locator.GetInstance<IDataImportEventsRaiser>()
                        , locator.GetInstance<IContentRepository>()
                        , locator.GetInstance<IPermanentLinkMapper>()
                        , locator.GetInstance<IContentTypeRepository>()
                        , locator.GetInstance<ContentTypeAvailabilityService>()
                        , locator.GetInstance<IAvailableSettingsRepository>()
                        , locator.GetInstance<IContentImporter>()
                        , locator.GetInstance<IContentTransferValidator>()
                        , locator.GetInstance<PropertyCategoryTransform>()
                        , locator.GetInstance<ContentRootRepository>()
                        , locator.GetInstance<ISiteDefinitionRepository>()
                        , locator.GetInstance<ContentOptions>()
                        , locator.GetInstance<ISiteDefinitionResolver>()
                    ));

                e.Services.Intercept<IRawContentRetriever>((locator, defaultRawContentRetriever) =>
                    new DefaultRawContentRetrieverInterceptor(defaultRawContentRetriever
                        , locator.GetInstance<IContentRepository>()
                        , locator.GetInstance<IContentTypeRepository>()
                        , locator.GetInstance<ILanguageBranchRepository>()
                        , locator.GetInstance<IContentCacheKeyCreator>()
                        , locator.GetInstance<ISynchronizedObjectInstanceCache>()
                        , locator.GetInstance<IRawPropertyRetriever>()));

                e.Services.Intercept<IContentImporter>((locator, defaultDataImporter) =>
                    new DefaultContentImporterInterceptor(defaultDataImporter
                         , locator.GetInstance<IPropertyImporter>()
                         , locator.GetInstance<IContentRepository>()
                         , locator.GetInstance<ContentLanguageSettingRepository>()
                         , locator.GetInstance<IPermanentLinkMapper>()
                         , locator.GetInstance<IContentProviderManager>()
                         , locator.GetInstance<IContentCacheRemover>()
                         , locator.GetInstance<IContentCacheListingRemover>()
                         , locator.GetInstance<IPageQuickSearch>()
                         , locator.GetInstance<IContentTypeRepository>()
                         , locator.GetInstance<IDynamicPropertiesLoader>()
                    ));

                e.Services.Intercept<IDataExporter>((locator, defaultDataExporter) =>
                    new DefaultDataExporterInterceptor(defaultDataExporter
                        , locator.GetInstance<ITransferExportOptionsEx>()
                        , locator.GetInstance<IContentVersionRepository>()
                        , locator.GetInstance<IRawContentRetriever>()
                        , locator.GetInstance<IContentLoader>()
                        , locator.GetInstance<IPropertyExporter>()
                        , locator.GetInstance<IDataExportEventsRaiser>()
                        , locator.GetInstance<IDataExportEvents>()
                        , locator.GetInstance<IContentCacheKeyCreator>()
                        , locator.GetInstance<ISynchronizedObjectInstanceCache>()
                        , locator.GetInstance<IContentRepository>()
                        , locator.GetInstance<IPermanentLinkMapper>()
                        , locator.GetInstance<IContentTypeRepository>()
                        , locator.GetInstance<IContentProviderManager>()
                        , locator.GetInstance<ContentTypeAvailabilityService>()
                        , locator.GetInstance<IAvailableSettingsRepository>()
                        , locator.GetInstance<IContentExporter>()
                        , locator.GetInstance<PropertyCategoryTransform>()
                        , locator.GetInstance<ContentRootRepository>()
                        , locator.GetInstance<ISiteDefinitionRepository>()
                        , locator.GetInstance<IMimeTypeResolver>()
                    ));
            };
        }

        public void Initialize(InitializationEngine context)
        {
            //Add initialization logic, this method is called once after CMS has been initialized
        }

        public void Uninitialize(InitializationEngine context)
        {
            //Add uninitialization logic
        }
    }
}
