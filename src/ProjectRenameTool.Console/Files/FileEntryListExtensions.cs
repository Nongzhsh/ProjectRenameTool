using System.IO;
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

        public static void SaveToZipFile(this FileEntryList fileEntryList, string outputZipFilePath)
        {
            var zipContent = fileEntryList.CreateZipFileFromEntries();
            using (var templateFileStream = new MemoryStream(zipContent))
            {
                using (var fileStream = new FileStream($@"{outputZipFilePath}", FileMode.Create))
                {
                    templateFileStream.Seek(0, SeekOrigin.Begin);
                    templateFileStream.CopyTo(fileStream);
                }
            }
        }
    }
}