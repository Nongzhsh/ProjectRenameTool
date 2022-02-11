using System.Collections.Generic;
using Awesome.Net.IgnoreParser;

namespace ProjectRenameTool.Console.Extensions
{
    public static class IgnoreParserExtensions
    {
        public static void AddIgnoreFile(this IgnoreParser ignoreParser, string filePath, IEnumerable<string> rules)
        {
            ignoreParser.IgnoreFiles.Add(new IgnoreFile(rules, filePath, ignoreParser.Options));
        }
    }
}