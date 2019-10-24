using System;
using System.Collections.Generic;
using System.IO;
using ProjectRenameTool.Console.Extensions;
using ProjectRenameTool.Console.Files;

namespace ProjectRenameTool.Console.Replacing
{
    /// <summary>
    /// 文件名/内容替换 Helper
    /// </summary>
    public static class ReplacementHelper
    {
        public static string ReplaceText(string text, ReplacementRule rule)
        {
            if (!rule.OldValue.IsNullOrEmpty() && !rule.NewValue.IsNullOrEmpty())
            {
                var oldValue = rule.OldValue;
                var newValue = rule.NewValue;

                if (rule.OldValue != rule.NewValue)
                {
                    var newText = rule.MatchCase
                        ? text.Replace(oldValue, newValue)
                        : text.Replace(oldValue, newValue, StringComparison.CurrentCultureIgnoreCase);

                    return newText;
                }

                if (rule.MatchCase)
                {
                    return text.Replace(oldValue, newValue, StringComparison.CurrentCultureIgnoreCase);
                }
            }

            return text;
        }

        public static string ReplaceText(string text, List<ReplacementRule> rules)
        {
            foreach (var rule in rules)
            {
                text = ReplaceText(text, rule);
            }

            return text;
        }

        /// <summary>
        /// 处理给定的文件/夹
        /// </summary>
        /// <param name="entry">给定的文件/夹</param>
        /// <param name="rules">文件名/内容替换的配置</param>
        public static void Replace(FileEntry entry, List<ReplacementRule> rules)
        {
            foreach (var rule in rules)
            {
                if (rule.OldValue.IsNullOrEmpty() || rule.NewValue.IsNullOrEmpty())
                {
                    continue;
                }

                if (rule.ReplaceContent)
                {
                    ReplaceFileEntryContent(entry, rule);
                }

                if (rule.ReplaceName)
                {
                    ReplaceFileEntryName(entry, rule);
                }
            }
        }

        /// <summary>
        /// 全部处理给定的文件/夹
        /// </summary>
        /// <param name="entries">给定的文件/夹</param>
        /// <param name="rules">文件名/内容替换的配置</param>
        public static void ReplaceAll(IEnumerable<FileEntry> entries, List<ReplacementRule> rules)
        {
            foreach (var entry in entries)
            {
                Replace(entry, rules);
            }
        }

        private static void ReplaceFileEntryName(FileEntry entry, ReplacementRule rule)
        {
            // 合法的名称才进行更名
            if (rule.NewValue.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
            {
                var newName = ReplaceText(entry.Name, rule);
                if (newName != entry.Name)
                {
                    entry.SetName(newName);
                }
            }
            throw new InvalidOperationException("You can not use special characters to set the file name.");
        }

        private static void ReplaceFileEntryContent(FileEntry entry, ReplacementRule rule)
        {
            if (entry.IsDirectory || entry.IsBinaryFile)
            {
                return;
            }
            var newContent = ReplaceText(entry.Content, rule);
            if (newContent != entry.Content)
            {
                entry.NormalizeLineEndings();
                entry.SetContent(newContent);
            }
        }
    }
}
