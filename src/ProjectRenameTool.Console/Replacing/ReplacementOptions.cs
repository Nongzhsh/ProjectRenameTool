using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ProjectRenameTool.Console.Replacing
{
    /// <summary>
    /// 文件名/内容替换配置
    /// </summary>
    public class ReplacementOptions
    {
        private string _outputFolderPath = string.Empty;
        private List<ReplacementRule> _rules = new List<ReplacementRule>
        {
            new ReplacementRule
            {
                NewValue = "",
                OldValue = ""
            }
        };

        /// <summary>
        /// 输出的文件夹
        /// </summary>
        public string OutputFolderPath
        {
            get => string.IsNullOrEmpty(_outputFolderPath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                : _outputFolderPath;
            set => _outputFolderPath = value;
        }

        /// <summary>
        /// 项目或解决方案路径
        /// </summary>
        public string SourcePath { get; set; } = @"可以是.zip文件或已解压的原项目文件夹的路径，注意路径分隔符为\";

        /// <summary>
        /// 自定义忽略拷贝规则（同.gitignore 格式）
        /// </summary>
        public string[] IgnoreCopyGlobRules { get; set; } = { ".github/" };

        /// <summary>
        /// 自定义忽略替换规则（同.gitignore 格式）
        /// </summary>
        public string[] IgnoreReplaceGlobRules { get; set; } = { "fonts/" };

        /// <summary>
        /// 待替换的条目列表
        /// </summary>
        public List<ReplacementRule> Rules
        {
            get => _rules/*.Where(x => x.NewValue != x.OldValue)*/.Distinct().ToList();
            set => _rules = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
        }
    }

    /// <summary>
    /// 待替换的条目
    /// </summary>
    public class ReplacementRule
    {
        /// <summary>
        /// 原字符
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// 新字符
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// 匹配大小写
        /// </summary>
        public bool MatchCase { get; set; } = true;

        /// <summary>
        /// 是否替换名称
        /// </summary>
        public bool ReplaceName { get; set; } = true;

        /// <summary>
        /// 是否替换内容
        /// </summary>
        public bool ReplaceContent { get; set; } = true;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is ReplacementRule item))
            {
                return false;
            }

            return item.MatchCase == MatchCase &&
                   item.ReplaceContent == ReplaceContent &&
                   item.ReplaceName == ReplaceName &&
                   item.NewValue == NewValue &&
                   item.OldValue == OldValue;
        }

        protected bool Equals(ReplacementRule other)
        {
            return OldValue == other.OldValue &&
                   NewValue == other.NewValue &&
                   MatchCase == other.MatchCase &&
                   ReplaceName == other.ReplaceName &&
                   ReplaceContent == other.ReplaceContent;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (OldValue != null ? OldValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NewValue != null ? NewValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ MatchCase.GetHashCode();
                hashCode = (hashCode * 397) ^ ReplaceName.GetHashCode();
                hashCode = (hashCode * 397) ^ ReplaceContent.GetHashCode();
                return hashCode;
            }
        }
    }
}