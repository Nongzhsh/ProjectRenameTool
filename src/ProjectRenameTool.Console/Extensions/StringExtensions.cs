using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectRenameTool.Console.Extensions
{
    /// <summary>
    /// <see cref="string"/> 的扩展方法。
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 如果不是以给定的字符结尾则添加它
        /// </summary>
        public static string EnsureEndsWith(this string str, char c, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if(str.EndsWith(c.ToString(), comparisonType))
            {
                return str;
            }

            return str + c;
        }

        /// <summary>
        /// 如果不是以给定的字符开头则添加它
        /// </summary>
        public static string EnsureStartsWith(this string str, char c, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if(str.StartsWith(c.ToString(), comparisonType))
            {
                return str;
            }

            return c + str;
        }

        /// <summary>
        /// <see cref="string.IsNullOrEmpty"/> 的快捷方式
        /// </summary>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// <see cref="string.IsNullOrWhiteSpace"/> 的快捷方式
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 从字符串的开头获取字符串的子字符串。
        /// </summary>
        public static string Left(this string str, int len)
        {
            if(str.Length < len)
            {
                throw new ArgumentException($"{nameof(len)} 参数不能大于给定字符串的长度!");
            }

            return str.Substring(0, len);
        }

        /// <summary>
        /// 替换换行符为 <see cref="Environment.NewLine"/>。
        /// </summary>
        public static string NormalizeLineEndings(this string str)
        {
            return str.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// 获取字符第n次出现的索引。
        /// </summary>
        public static int NthIndexOf(this string str, char c, int n)
        {
            var count = 0;
            for(var i = 0; i < str.Length; i++)
            {
                if(str[i] != c)
                {
                    continue;
                }

                if((++count) == n)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 从字符串的结尾删除首次匹配字符之后的字符串。
        /// </summary>
        public static string RemovePostFix(this string str, params string[] postFixes)
        {
            return str.RemovePostFix(StringComparison.Ordinal, postFixes);
        }

        /// <summary>
        /// 从字符串的结尾删除首次匹配字符之后的字符串。
        /// </summary>
        public static string RemovePostFix(this string str, StringComparison comparisonType, params string[] postFixes)
        {
            if(str.IsNullOrEmpty())
            {
                return null;
            }

            if(postFixes == null || postFixes.Length <= 0)
            {
                return str;
            }

            foreach(var postFix in postFixes)
            {
                if(str.EndsWith(postFix, comparisonType))
                {
                    return str.Left(str.Length - postFix.Length);
                }
            }

            return str;
        }

        /// <summary>
        /// 从字符串的开头删除首次匹配字符之后的字符串。
        /// </summary>
        public static string RemovePreFix(this string str, params string[] preFixes)
        {
            return str.RemovePreFix(StringComparison.Ordinal, preFixes);
        }

        /// <summary>
        /// 从字符串的开头删除首次匹配字符之后的字符串。
        /// </summary>
        public static string RemovePreFix(this string str, StringComparison comparisonType, params string[] preFixes)
        {
            if(str.IsNullOrEmpty())
            {
                return null;
            }

            if(preFixes == null || preFixes.Length <= 0)
            {
                return str;
            }

            foreach(var preFix in preFixes)
            {
                if(str.StartsWith(preFix, comparisonType))
                {
                    return str.Right(str.Length - preFix.Length);
                }
            }

            return str;
        }

        /// <summary>
        /// 替换第一次出现的字符
        /// </summary>
        public static string ReplaceFirst(this string str, string search, string replace, StringComparison comparisonType = StringComparison.Ordinal)
        {
            var pos = str.IndexOf(search, comparisonType);
            if(pos < 0)
            {
                return str;
            }

            return str.Substring(0, pos) + replace + str.Substring(pos + search.Length);
        }

        /// <summary>
        /// 从字符串末尾获取字符串的子字符串。
        /// </summary>
        public static string Right(this string str, int len)
        {
            if(str.Length < len)
            {
                throw new ArgumentException($"{nameof(len)} 参数不能大于给定字符串的长度!");
            }

            return str.Substring(str.Length - len, len);
        }

        /// <summary>
        /// 分割字符串
        /// </summary>
        public static string[] Split(this string str, string separator)
        {
            return str.Split(new[] { separator }, StringSplitOptions.None);
        }

        /// <summary>
        /// 分割字符串
        /// </summary>
        public static string[] Split(this string str, string separator, StringSplitOptions options)
        {
            return str.Split(new[] { separator }, options);
        }

        /// <summary>
        /// 使用 <see cref="Environment.NewLine"/> 分割字符串
        /// </summary>
        public static string[] SplitToLines(this string str)
        {
            return str.Split(Environment.NewLine);
        }

        /// <summary>
        /// 使用 <see cref="Environment.NewLine"/> 分割字符串
        /// </summary>
        public static string[] SplitToLines(this string str, StringSplitOptions options)
        {
            return str.Split(Environment.NewLine, options);
        }

        /// <summary>
        /// 转换成 CamelCase 形式
        /// </summary>
        public static string ToCamelCase(this string str, bool useCurrentCulture = false)
        {
            if(string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if(str.Length == 1)
            {
                return useCurrentCulture ? str.ToLower() : str.ToLowerInvariant();
            }

            return (useCurrentCulture ? char.ToLower(str[0]) : char.ToLowerInvariant(str[0])) + str.Substring(1);
        }

        /// <summary>
        /// 以空格分词
        /// 如: "ThisIsSampleSentence" 将转换为 "This is a sample sentence"
        /// </summary>
        public static string ToSentenceCase(this string str, bool useCurrentCulture = false)
        {
            if(string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            return useCurrentCulture
                ? Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1]))
                : Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLowerInvariant(m.Value[1]));
        }

        /// <summary>
        /// 转换为枚举值
        /// </summary>
        public static T ToEnum<T>(this string value)
            where T : struct
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        /// <summary>
        /// 转换为枚举值
        /// </summary>
        public static T ToEnum<T>(this string value, bool ignoreCase)
            where T : struct
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        /// <summary>
        /// Md5加密
        /// </summary>
        public static string ToMd5(this string str)
        {
            using(var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(str);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach(var hashByte in hashBytes)
                {
                    sb.Append(hashByte.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// 转换成 PascalCase 形式
        /// </summary>
        public static string ToPascalCase(this string str, bool useCurrentCulture = false)
        {
            if(string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if(str.Length == 1)
            {
                return useCurrentCulture ? str.ToUpper() : str.ToUpperInvariant();
            }

            return (useCurrentCulture ? char.ToUpper(str[0]) : char.ToUpperInvariant(str[0])) + str.Substring(1);
        }

        /// <summary>
        /// 如果字符串超过给定的长度, 则从字符串的开头获取该子字符串。
        /// </summary>
        public static string Truncate(this string str, int maxLength)
        {
            if(str == null)
            {
                return null;
            }

            if(str.Length <= maxLength)
            {
                return str;
            }

            return str.Left(maxLength);
        }

        /// <summary>
        /// 如果字符串超过给定的长度, 则从字符串的结尾获取子字符串。
        /// </summary>
        public static string TruncateFromBeginning(this string str, int maxLength)
        {
            if(str == null)
            {
                return null;
            }

            if(str.Length <= maxLength)
            {
                return str;
            }

            return str.Right(maxLength);
        }

        /// <summary>
        /// 如果字符串超过给定的长度, 则在结尾添加 "..."
        /// </summary>
        public static string TruncateWithPostfix(this string str, int maxLength)
        {
            return TruncateWithPostfix(str, maxLength, "...");
        }

        /// <summary>
        /// 如果字符串超过给定的长度, 则在结尾添加特定字符
        /// </summary>
        public static string TruncateWithPostfix(this string str, int maxLength, string postfix)
        {
            if(str == null)
            {
                return null;
            }

            if(str == string.Empty || maxLength == 0)
            {
                return string.Empty;
            }

            if(str.Length <= maxLength)
            {
                return str;
            }

            if(maxLength <= postfix.Length)
            {
                return postfix.Left(maxLength);
            }

            return str.Left(maxLength - postfix.Length) + postfix;
        }

        /// <summary>
        /// 转换为 <see cref="byte"/> 数组（<see cref="Encoding.UTF8"/>编码）
        /// </summary>
        public static byte[] GetBytes(this string str)
        {
            return str.GetBytes(Encoding.UTF8);
        }

        /// <summary>
        /// 按照指定编码转换为 <see cref="byte"/> 数组
        /// </summary>
        public static byte[] GetBytes(this string str, Encoding encoding)
        {
            return encoding.GetBytes(str);
        }

        /// <summary>
        /// 将路径转换为 unix 样式的路径 (使用/分隔目录)
        /// </summary>
        public static string NormalizedPath(this string path)
        {
            if (path.IsNullOrWhiteSpace())
            {
                return "/";
            }

            path = path.Replace(":", string.Empty);
            return path.Replace(Path.DirectorySeparatorChar.ToString(), "/").Trim();
        }
    }
}