using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

namespace ProjectRenameTool.Console.Files
{
    /// <summary>
    /// <see cref="FileEntryList"/> 扩展方法
    /// </summary>
    public static class FileEntryListExtensions
    {
        public static async Task<byte[]> CreateZipFileFromEntriesAsync(this FileEntryList entries)
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
                        await zipOutputStream.WriteAsync(entry.Bytes, 0, entry.Bytes.Length);
                    }

                    zipOutputStream.CloseEntry();
                    zipOutputStream.IsStreamOwner = false;
                }

                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        public static async Task SaveToZipFileAsync(this FileEntryList fileEntryList, string outputZipFilePath)
        {
            var zipContent = await fileEntryList.CreateZipFileFromEntriesAsync();
            using (var templateFileStream = new MemoryStream(zipContent))
            {
                using (var fileStream = new FileStream($@"{outputZipFilePath}", FileMode.Create))
                {
                    templateFileStream.Seek(0, SeekOrigin.Begin);
                    await templateFileStream.CopyToAsync(fileStream);
                }
            }
        }
    }
}