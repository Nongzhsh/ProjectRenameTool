using System.IO;
using System.Threading.Tasks;

namespace ProjectRenameTool.Console.Extensions
{
    /// <summary>
    /// <see cref="Stream"/> 的扩展方法
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// 获取全部字节
        /// </summary>
        public static async Task<byte[]> GetAllBytesAsync(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
