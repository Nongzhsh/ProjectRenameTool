using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ProjectRenameTool.Console.Extensions;
using ProjectRenameTool.Console.Files;

namespace ProjectRenameTool.Console.Zipping
{
    public static class ZipFileExtensions
    {
        /// <summary>
        /// 判断是否是 .zip 文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsZipFile(this string filePath)
        {
            try
            {
                if (new FileInfo(filePath).Attributes.HasFlag(FileAttributes.Directory))
                {
                    return false;
                }

                var zipFile = new ZipFile(filePath);
                var isZip = zipFile.TestArchive(true);
                return isZip;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }
        }

        public static FileEntryList ToFileEntryList(this string zipFilePath)
        {
            var fileBytes = File.ReadAllBytes(zipFilePath);
            var fileEntries = new List<FileEntry>();

            using (var memoryStream = new MemoryStream(fileBytes))
            {
                using (var zipInputStream = new ZipInputStream(memoryStream))
                {
                    var zipEntry = zipInputStream.GetNextEntry();
                    while (zipEntry != null)
                    {
                        var buffer = new byte[4096]; // 4K is optimum

                        using (var fileEntryMemoryStream = new MemoryStream())
                        {
                            StreamUtils.Copy(zipInputStream, fileEntryMemoryStream, buffer);
                            fileEntries.Add(new FileEntry(zipEntry.Name.EnsureStartsWith('/'),
                                fileEntryMemoryStream.ToArray(), zipEntry.IsDirectory));
                        }

                        zipEntry = zipInputStream.GetNextEntry();
                    }
                }

                return new FileEntryList(fileEntries);
            }
        }
    }
}
