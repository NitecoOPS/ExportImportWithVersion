using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Internal;
using EPiServer.Core.Transfer;
using EPiServer.Core.Transfer.Internal;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.DataAccess.Internal;
using EPiServer.Framework;
using EPiServer.Security;
using EPiServer.Web;
using log4net;
using System;
using System.Globalization;
using System.Reflection;

namespace ExportImportWithVersion.DataExporter
{
    public class DefaultContentImporterInterceptor : DefaultContentImporter, IContentImporter
    {
        private static ILog Logger = LogManager.GetLogger(typeof(DefaultContentImporterInterceptor));

        private IContentImporter defaultContentImporter;
        private IPropertyImporter propertyImporter;
        private IContentRepository contentRepository;
        private ContentLanguageSettingRepository contentLanguageSettingRepository;
        private IPermanentLinkMapper permanentLinkMapper;
        private IContentProviderManager contentProviderManager;
        private IContentCacheRemover contentCacheRemover;
        private IContentCacheListingRemover contentCacheListingRemover;
        private IPageQuickSearch pageQuickSearch;
        private IContentTypeRepository contentTypeRepository;
        private IDynamicPropertiesLoader dynamicPropertiesLoader;

        public DefaultContentImporterInterceptor(IContentImporter defaultContentImporter
            , IPropertyImporter propertyImporter
            , IContentRepository contentRepository
            , ContentLanguageSettingRepository contentLanguageSettingRepository
            , IPermanentLinkMapper permanentLinkMapper
            , IContentProviderManager contentProviderManager
            , IContentCacheRemover contentCacheRemover
            , IContentCacheListingRemover contentCacheListingRemover
            , IPageQuickSearch pageQuickSearch
            , IContentTypeRepository contentTypeRepository
            , IDynamicPropertiesLoader dynamicPropertiesLoader)
            : base(propertyImporter, contentRepository, contentLanguageSettingRepository
                  , permanentLinkMapper, contentProviderManager, contentCacheRemover
                  , contentCacheListingRemover, pageQuickSearch, contentTypeRepository
                  , dynamicPropertiesLoader)
        {
            this.defaultContentImporter = defaultContentImporter;
            this.propertyImporter = propertyImporter;
            this.contentRepository = contentRepository;
            this.contentLanguageSettingRepository = contentLanguageSettingRepository;
            this.permanentLinkMapper = permanentLinkMapper;
            this.contentProviderManager = contentProviderManager;
            this.contentCacheRemover = contentCacheRemover;
            this.contentCacheListingRemover = contentCacheListingRemover;
            this.pageQuickSearch = pageQuickSearch;
            this.contentTypeRepository = contentTypeRepository;
            this.dynamicPropertiesLoader = dynamicPropertiesLoader;
        }

        protected override IContent Import(ImportedContentData importedContentData
            , AccessLevel requiredDestinationAccess, IContentTransferContext context
            , TransferImportOptions options, out Guid importedPageGuid)
        {
            if (options.TransferType != TypeOfTransfer.Importing)
            {
                return base.Import(importedContentData
                    , requiredDestinationAccess
                    , context, options, out importedPageGuid);
            }

            CultureInfo originalSelectedLanguage = null;
            if (importedContentData.SelectedLanguage != null)
                originalSelectedLanguage = CultureInfo.GetCultureInfo(importedContentData.SelectedLanguage.Name);

            var selectedLanguageAction = importedContentData.GetType().GetProperty("SelectedLanguage");
            var contentLanguage = importedContentData.GetLanguageBranch();
            if (!string.IsNullOrEmpty(contentLanguage))
            {
                selectedLanguageAction.SetValue(importedContentData, CultureInfo.GetCultureInfo(contentLanguage));
            }

            var status = importedContentData.GetStatus();
            var propSaveAction = context.GetType().GetProperty("SaveAction");
            SaveAction originalSaveActions = SaveAction.Publish | SaveAction.SkipValidation;
            if (!string.IsNullOrEmpty(status) && int.Parse(status) < (int)VersionStatus.Published)
            {
                propSaveAction.SetValue(context, SaveAction.CheckOut | SaveAction.ForceNewVersion | SaveAction.SkipValidation);
            }

            var orgPageSaveDBChangeBy = ContextCache.Current["PageSaveDB:ChangedBy"];
            var orgPageSaveDBPageSaved = ContextCache.Current["PageSaveDB:PageSaved"];
            ContextCache.Current["PageSaveDB:ChangedBy"] = string.Empty;
            ContextCache.Current["PageSaveDB:PageSaved"] = string.Empty;

            try
            {
                var importedContentGuid = new Guid(importedContentData.GetContentGuid());
                var handleContentGuidMethod = defaultContentImporter.GetType()
                    .GetMethod("HandleContentGuid", BindingFlags.NonPublic | BindingFlags.Instance);
                var guid = (Guid)handleContentGuidMethod.Invoke(
                    defaultContentImporter
                    , new object[] { importedContentGuid, context });
                PermanentLinkMap permanentLinkMap = permanentLinkMapper.Find(guid);

                var baseContent = base.Import(importedContentData
                    , requiredDestinationAccess
                    , context, options, out importedPageGuid);

                if (permanentLinkMap != null && (context.SaveAction & SaveAction.Publish) != SaveAction.Publish)
                    contentRepository.Save(baseContent, context.SaveAction, requiredDestinationAccess);

                return baseContent;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
            finally
            {
                selectedLanguageAction.SetValue(importedContentData, originalSelectedLanguage);
                propSaveAction.SetValue(context, originalSaveActions);
                ContextCache.Current["PageSaveDB:ChangedBy"] = orgPageSaveDBChangeBy;
                ContextCache.Current["PageSaveDB:PageSaved"] = orgPageSaveDBPageSaved;
            }
        }
    }
}
