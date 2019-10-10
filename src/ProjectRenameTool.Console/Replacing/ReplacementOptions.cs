using System;
using System.Collections.Generic;

namespace ProjectRenameTool.Console.Replacing
{
    /// <summary>
    /// 文件名/内容替换配置
    /// </summary>
    public class ReplacementOptions
    {
        private string _outputFolderPath = string.Empty;

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
        public string SourcePath { get; set; } = "可以是.zip文件或已解压的原项目文件夹的路径";

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
        public List<ReplacementRule> Rules { get; set; } = new List<ReplacementRule>
        {
            new ReplacementRule
            {
                MatchCase = true,
                NewValue = "NewCompanyName",
                OldValue = "OldCompanyName"
            }
        };
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
    }
}