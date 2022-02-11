using System;
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
            if (str.EndsWith(c.ToString(), comparisonType))
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
            if (str.StartsWith(c.ToString(), comparisonType))
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
        /// 从字符串的开头获取字符串的子字符串。
        /// </summary>
        public static string Left(this string str, int len)
        {
            if (str.Length < len)
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
            return Regex.Replace(str, @"\r\n?|\n", Environment.NewLine);
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
            if (str.IsNullOrEmpty())
            {
                return null;
            }

            if (postFixes == null || postFixes.Length <= 0)
            {
                return str;
            }

            foreach (var postFix in postFixes)
            {
                if (str.EndsWith(postFix, comparisonType))
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
            if (str.IsNullOrEmpty())
            {
                return null;
            }

            if (preFixes == null || preFixes.Length <= 0)
            {
                return str;
            }

            foreach (var preFix in preFixes)
            {
                if (str.StartsWith(preFix, comparisonType))
                {
                    return str.Right(str.Length - preFix.Length);
                }
            }

            return str;
        }

        /// <summary>
        /// 从字符串末尾获取字符串的子字符串。
        /// </summary>
        public static string Right(this string str, int len)
        {
            if (str.Length < len)
            {
                throw new ArgumentException($"{nameof(len)} 参数不能大于给定字符串的长度!");
            }

            return str.Substring(str.Length - len, len);
        }

    }
}