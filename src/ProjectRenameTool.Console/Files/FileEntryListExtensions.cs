using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace ProjectRenameTool.Console.Files
{
    /// <summary>
    /// <see cref="FileEntryList"/> 扩展方法
    /// </summary>
    public static class FileEntryListExtensions
    {
        public static byte[] CreateZipFileFromEntries(this FileEntryList entries)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipOutputStream = new ZipOutputStream(memoryStream))
                {
                    zipOutputStream.SetLevel(3); //0-9, 9 being the highest level of compression

                    foreach (var entry in entries)
                    {
                        zipOutputStream.PutNextEntry(new ZipEntry(entry.Name)
                        {
                            Size = entry.Bytes.Length
                        });
                        zipOutputStream.Write(entry.Bytes, 0, entry.Bytes.Length);
                    }

                    zipOutputStream.CloseEntry();
                    zipOutputStream.IsStreamOwner = false;
                }

                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        public static void SaveToZipFile(this FileEntryList fileEntryList, string outputFolderPath)
        {
            var zipContent = fileEntryList.CreateZipFileFromEntries();
            using (var templateFileStream = new MemoryStream(zipContent))
            {
                using (var zipInputStream = new ZipInputStream(templateFileStream))
                {
                    var zipEntry = zipInputStream.GetNextEntry();
                    while (zipEntry != null)
                    {
                        var fullZipToPath = Path.Combine(outputFolderPath, zipEntry.Name);
                        var directoryName = Path.GetDirectoryName(fullZipToPath);

                        if (!string.IsNullOrEmpty(directoryName))
                        {
                            Directory.CreateDirectory(directoryName);
                        }

                        var fileName = Path.GetFileName(fullZipToPath);
                        if (fileName.Length == 0)
                        {
                            zipEntry = zipInputStream.GetNextEntry();
                            continue;
                        }

                        var buffer = new byte[4096]; // 4K is optimum
                        using (var streamWriter = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                        }

                        zipEntry = zipInputStream.GetNextEntry();
                    }
                }
            }
        }
    }
}