using System.IO;

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
        public static byte[] GetAllBytes(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
