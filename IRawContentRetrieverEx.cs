using EPiServer.Core;
using EPiServer.Core.Transfer.Internal;

namespace ExportImportWithVersion.DataExporter
{
    public interface IRawContentRetrieverEx : IRawContentRetriever
    {
        RawContent CreateRawContent(IContent content);
    }
}
