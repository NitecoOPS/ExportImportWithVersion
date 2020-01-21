using EPiServer.Core;
using EPiServer.Core.Transfer;
using System.Collections.Generic;

namespace ExportImportWithVersion.DataExporter
{
    public interface ITransferExportOptionsEx
    {
        bool ExcludeFiles { get; set; }
        bool ExportVersion { get; set; }
        IEnumerable<VersionStatus> VersionStatuses { get; set; }
        bool ExportFilesOnly { get; set; }

        void MergeWithEPi(TransferExportOptions options);
    }
}