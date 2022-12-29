using Ionic.Zip;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Editor
{
    public static class ZipUtils
    {
        private static string AzureProjectPath = "/../CBS_Azure_Functions(Admin)";
        private static string ArchiveAzureProjectPath = "/CBS/AzureFunctionsProject/AzureFunctionsProject.zip";
        private static string UnzipAzureProjectPath = "/../CBSAzureFunctionsProject";

        public static void ZipAzureProject()
        {
            var fullPath = Application.dataPath + AzureProjectPath;

            using (ZipFile zip = new ZipFile())
            {
                var archivePath = Application.dataPath + ArchiveAzureProjectPath;

                zip.AddDirectory(fullPath);
                zip.Save(archivePath);
            }
        }

        public static void UnzipAzureProject()
        {
            var fullPath = Application.dataPath + ArchiveAzureProjectPath;
            var unzipPath = Application.dataPath + UnzipAzureProjectPath;

            using (ZipFile zip = ZipFile.Read(fullPath))
            {
                foreach (ZipEntry e in zip)
                {
                    e.Extract(unzipPath, ExtractExistingFileAction.OverwriteSilently);  // overwrite == true
                }
            }
        }
    }
}
