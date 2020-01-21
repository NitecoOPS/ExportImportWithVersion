using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Transfer;
using EPiServer.Core.Transfer.Internal;
using EPiServer.DataAbstraction;
using EPiServer.Enterprise;
using EPiServer.Enterprise.Internal;
using EPiServer.Framework.Cache;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace ExportImportWithVersion.DataExporter
{
    public class DefaultDataExporterInterceptor : DefaultDataExporter, IDataExporter
    {
        private readonly IDataExporter _defaultDataExporter;
        private readonly ITransferExportOptionsEx _transferExportOptionsEx;
        private readonly IContentVersionRepository _contentVersionRepository;
        private readonly IContentExporter _contentExporter;
        private readonly IRawContentRetriever _rawContentRetiever;
        private readonly IContentLoader _contentLoader;
        private readonly IPropertyExporter _propertyExporter;

        private TransferExportOptions Options { get; set; }

        public DefaultDataExporterInterceptor(IDataExporter defaultDataExporter
            , ITransferExportOptionsEx transferExportOptionsEx
            , IContentVersionRepository contentVersionRepository
            , IRawContentRetriever rawContentRetriever
            , IContentLoader contentLoader
            , IPropertyExporter propertyExporter
            , IDataExportEventsRaiser eventRegister
            , IDataExportEvents exportEvents
            , IContentCacheKeyCreator contentCacheKeyCreator
            , ISynchronizedObjectInstanceCache cacheInstance
            , IContentRepository contentRepository
            , IPermanentLinkMapper permanentLinkMapper
            , IContentTypeRepository contentTypeRepository
            , IContentProviderManager contentProviderManager
            , ContentTypeAvailabilityService contentTypeAvailabilityService
            , IAvailableSettingsRepository availableSettingsRepository
            , IContentExporter contentExporter
            , PropertyCategoryTransform categoryTransform
            , ContentRootRepository contentRootRepository
            , ISiteDefinitionRepository siteDefinitionRepository
            , IMimeTypeResolver mimeTypeResolver)
            : base(eventRegister, exportEvents, contentCacheKeyCreator
                  , cacheInstance, contentRepository
                  , permanentLinkMapper, contentTypeRepository
                  , contentProviderManager, contentTypeAvailabilityService
                  , availableSettingsRepository, contentExporter
                  , categoryTransform, contentRootRepository
                  , siteDefinitionRepository, mimeTypeResolver)
        {
            _defaultDataExporter = defaultDataExporter;
            _transferExportOptionsEx = transferExportOptionsEx;
            _contentVersionRepository = contentVersionRepository;
            _contentExporter = contentExporter;
            _rawContentRetiever = rawContentRetriever;
            _contentLoader = contentLoader;
            _propertyExporter = propertyExporter;
        }

        public new ITransferLog Export(Stream stream, IEnumerable<ExportSource> sourceRoots, ExportOptions options)
        {
            Options = options;
            _transferExportOptionsEx.MergeWithEPi(options);

            return base.Export(stream, sourceRoots, options);
        }

        protected override void ExportContent(XmlTextWriter xml
            , ContentReference contentToExport
            , IContentTransferContext context
            , ITransferContentData transferContent)
        {
            if (_transferExportOptionsEx.ExportVersion)
            {
                ExportContentWithVerion(xml, contentToExport, context, transferContent, base.ExportContent);
            }
            else
            {
                base.ExportContent(xml, contentToExport, context, transferContent);
            }
        }

        protected virtual void ExportContentWithVerion(XmlTextWriter xml
            , ContentReference contentToExport
            , IContentTransferContext context
            , ITransferContentData transferContent
            , Action<XmlTextWriter, ContentReference, IContentTransferContext, ITransferContentData> callBackAction)
        {
            var rawContentInfo = _rawContentRetiever.CreateRawContentInfo(contentToExport, string.Empty);

            ContentVersion latestPublishedMasterVersion;
            var versions = GetContentVersions(contentToExport
                , rawContentInfo
                , out latestPublishedMasterVersion).ToList();

            var rawContentRetieverEx = _rawContentRetiever as IRawContentRetrieverEx;

            if (versions == null || versions.Any() == false || latestPublishedMasterVersion == null)
            {
                callBackAction.Invoke(xml, contentToExport, context, transferContent);
                return;
            }

            foreach (var version in versions)
            {
                var versionTransferData = BuildRawTransferContent(context
                    , null
                    , rawContentRetieverEx
                    , version);

                new XmlSerializer(typeof(TransferContentData)).Serialize(xml, versionTransferData, null);
            }
        }

        protected virtual TransferContentData BuildRawTransferContent(IContentTransferContext context
            , List<ContentLanguageSetting> contentLanguageSettings
            , IRawContentRetrieverEx rawContentRetieverEx, IContent version)
        {
            var transferVersionContent = new TransferContentData()
            {
                RawContentData = rawContentRetieverEx.CreateRawContent(version)
            };
            if (contentLanguageSettings != null)
                transferVersionContent.ContentLanguageSettings = contentLanguageSettings;

            PropertyExportContext propertyExportContext = new PropertyExportContext
            {
                TransferContext = context,
                TransferOptions = Options,
                Output = transferVersionContent.RawContentData,
                Source = version
            };
            _propertyExporter.ExportProperties(version
                , transferVersionContent.RawContentData.Property
                , propertyExportContext);

            return transferVersionContent;
        }

        /// <summary>
        /// Get all content version of current exported content in all languages.
        /// </summary>
        /// <param name="contentToExport"></param>
        /// <returns></returns>
        protected virtual IEnumerable<IContent> GetContentVersions(
            ContentReference contentToExport, RawContentInfo rawContentInfo, out ContentVersion latestPublishedMasterVersion)
        {
            var versionFilter = new VersionFilter
            {
                ContentLink = contentToExport,
                Languages = rawContentInfo.Languages.Select(x => CultureInfo.GetCultureInfo(x)),
                ExcludeDeleted = true
            };

            if (_transferExportOptionsEx.VersionStatuses != null)
                versionFilter.Statuses = _transferExportOptionsEx.VersionStatuses;

            int totalCount;
            var versions = _contentVersionRepository
                .List(versionFilter, 0, int.MaxValue, out totalCount);

            latestPublishedMasterVersion = versions.FirstOrDefault(x =>
                x.Status == VersionStatus.Published
                && x.LanguageBranch == rawContentInfo.MasterLanguage);

            versions = FilterVersions(versions);

            return versions
                .OrderBy(x => x.LanguageBranch == rawContentInfo.MasterLanguage ? 1 : 2)
                .ThenBy(x => x.Saved)
                .Select(x => GetContent(x))
                .Where(x => x != null);
        }

        private IEnumerable<ContentVersion> FilterVersions(IEnumerable<ContentVersion> versions)
        {
            if (versions == null || !versions.Any()) return versions;

            var versionGroupsByLanguage = versions.GroupBy(x => x.LanguageBranch).Where(g => g != null && g.Any());
            var filterdVersions = new List<ContentVersion>();

            foreach (var group in versionGroupsByLanguage)
            {
                var orderedSavedDateGroup = group.OrderBy(x => x.Saved);
                var firstPublished = orderedSavedDateGroup.FirstOrDefault(x => x.Status >= VersionStatus.Published);
                if (firstPublished == null)
                {
                    filterdVersions.Add(orderedSavedDateGroup.Last());
                    continue;
                }

                var validVersions = orderedSavedDateGroup.Where(x => x.Saved >= firstPublished.Saved);
                if (validVersions != null && validVersions.Any())
                    filterdVersions.AddRange(validVersions);
            }

            return filterdVersions;
        }

        protected virtual IContent GetContent(ContentVersion x)
        {
            try
            {
                return _contentLoader.Get<IContent>(x.ContentLink, CultureInfo.GetCultureInfo(x.LanguageBranch));
            }
            catch
            {
                // ContentNotFound
                return null;
            }
        }
    }
}