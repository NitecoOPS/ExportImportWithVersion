using EPiServer.Core;
using EPiServer.Core.Transfer;
using System.Collections.Generic;

namespace ExportImportWithVersion.DataExporter
{
    public class DefaultTransferExportOptionsEx : ITransferExportOptionsEx
    {
        public bool ExcludeFiles { get; set; } = true;
        public bool ExportVersion { get; set; } = true;
        public IEnumerable<VersionStatus> VersionStatuses { get; set; }

        public bool ExportFilesOnly { get; set; } = false;

        public void MergeWithEPi(TransferExportOptions options)
        {
            ExcludeFiles = options.ExcludeFiles;
            //if (!ExcludeFiles)
            //{
            //    ExportVersion = false;
            //    ExportFilesOnly = true;
            //}

            // support exporting only
            if (options.TransferType != TypeOfTransfer.Exporting)
            {
                ExcludeFiles = false;
                ExportVersion = false;
                ExportFilesOnly = false;
            }
        }
    }
}