using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Awesome.Net.IgnoreParser;
using Awesome.Net.WritableOptions;
using ProjectRenameTool.Console.Extensions;
using ProjectRenameTool.Console.Files;
using ProjectRenameTool.Console.Replacing;
using ProjectRenameTool.Console.Zipping;
using static System.Console;

namespace ProjectRenameTool.Console
{
    public class Renamer
    {
        private readonly ReplacementOptions _replacementOptions;
        private readonly IgnoreParser _ignoreCopyParser = new IgnoreParser("CopyParser");
        private readonly IgnoreParser _ignoreReplaceParser = new IgnoreParser("ReplaceParser");

        private string _outputFolderPath;
        private readonly FileEntryList _outputFileEntryList = new FileEntryList(new List<FileEntry>());

        public Renamer(IWritableOptions<ReplacementOptions> options)
        {
            _replacementOptions = options.Value;
            _outputFolderPath = _replacementOptions.OutputFolderPath;
        }

        public void Run()
        {
            LoadIgnoreGlobRules();
            ReplaceAll();
        }

        private void LoadIgnoreGlobRules()
        {
            //忽略替换规则
            var sourceRootPath = _replacementOptions.SourcePath;
            if (_replacementOptions.SourcePath.IsZipFile())
            {
                sourceRootPath = Path.GetFileNameWithoutExtension(_replacementOptions.SourcePath);
            }

            _ignoreCopyParser.AddCustomIgnoreRules(sourceRootPath, _replacementOptions.IgnoreCopyGlobRules);

            _ignoreReplaceParser.AddCustomIgnoreRules(sourceRootPath, _replacementOptions.IgnoreReplaceGlobRules);

        }

        private void ReplaceAll()
        {
            WriteLine($"正在读取 {_replacementOptions.SourcePath} 文件列表");
            if (_replacementOptions.SourcePath.IsZipFile())
            {
                ReplaceInZipFile();
            }
            else
            {
                ReplaceInDirectory();
            }
        }

        private void ReplaceInZipFile()
        {
            var entries = _replacementOptions.SourcePath.ToFileEntryList();
            var ignoreFiles = entries.Where(x => x.Name.EndsWith(".gitignore"));

            foreach (var fileEntry in ignoreFiles)
            {
                _ignoreCopyParser.AddIgnoreFile(fileEntry.Name, fileEntry.GetLines());
            }

            foreach (var fileEntry in entries)
            {
                if (_ignoreCopyParser.IsIgnore(fileEntry.Name))
                {
                    continue;
                }

                var newEntry = Replace(fileEntry.Name, fileEntry);
                _outputFileEntryList.Add(newEntry);
            }

            var sourceFile = new FileInfo(_replacementOptions.SourcePath);
            var newFileName = ReplacementHelper.ReplaceText(sourceFile.Name, _replacementOptions.Rules);

            var outputZipFilePath = newFileName;
            if (!_outputFolderPath.EndsWith(newFileName))
            {
                outputZipFilePath = Path.Combine(_outputFolderPath, $"{newFileName}");
            }

            _outputFileEntryList.SaveToZipFile(outputZipFilePath);
        }

        private void ReplaceInDirectory()
        {
            var rootFolder = new DirectoryInfo(_replacementOptions.SourcePath);
            var newFolderName = ReplacementHelper.ReplaceText(rootFolder.Name, _replacementOptions.Rules);

            if (!_outputFolderPath.TrimEnd(Path.DirectorySeparatorChar).EndsWith(newFolderName))
            {
                _outputFolderPath = Path.Combine(_outputFolderPath, $"{newFolderName}");
            }

            Directory.CreateDirectory(_outputFolderPath);

            var ignoreFiles = rootFolder.GetFiles(".gitignore", SearchOption.AllDirectories);
            _ignoreCopyParser.AddIgnoreFiles(ignoreFiles);

            WriteLine($"{Environment.NewLine}正在替换文件名称/内容");

            CopyAndReplace(rootFolder);

            //TODO:保存 zip？
        }

        private void CopyAndReplace(DirectoryInfo source)
        {
            foreach (var file in source.GetFiles())
            {
                if (_ignoreCopyParser.IsIgnore(file.FullName))
                {
                    continue;
                }

                using (var fileStream = file.OpenRead())
                {
                    // 相对原解决方案的路径
                    var entryName = file.FullName.RemovePreFix(_replacementOptions.SourcePath);

                    var entry = new FileEntry(entryName, fileStream.GetAllBytes(), false);

                    entry = Replace(file.FullName, entry);

                    var newFilePath = Path.Combine(_outputFolderPath, entry.Name.TrimStart(Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));

                    File.WriteAllBytes(newFilePath, entry.Bytes);

                    _outputFileEntryList.Add(entry);
                }
            }

            foreach (var dir in source.GetDirectories())
            {
                if (_ignoreCopyParser.IsIgnore(dir.FullName))
                {
                    continue;
                }

                var entryName = dir.FullName.RemovePreFix(_replacementOptions.SourcePath);
                var entry = new FileEntry(entryName, Array.Empty<byte>(), true);

                entry = Replace(dir.FullName, entry);

                var newDirPath = Path.Combine(_outputFolderPath, entry.Name.TrimStart(Path.DirectorySeparatorChar));
                Directory.CreateDirectory(newDirPath);

                _outputFileEntryList.Add(entry);

                CopyAndReplace(dir);
            }
        }

        private FileEntry Replace(string sourcePath, FileEntry entry)
        {
            if (_replacementOptions.Rules.Any())
            {
                if (_ignoreReplaceParser.IsIgnore(sourcePath))
                {
                    // 它的路径还是要替换的

                    var oldPath = entry.Name.Replace('/', Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);
                    var postName = oldPath.Split(Path.DirectorySeparatorChar).Last();
                    var newPrePath = ReplacementHelper.ReplaceText(oldPath.RemovePostFix(postName), _replacementOptions.Rules);

                    if (newPrePath != Path.DirectorySeparatorChar.ToString())
                    {
                        var newName = Path.Combine(newPrePath, postName);

                        if (entry.Name.EndsWith(Path.DirectorySeparatorChar))
                        {
                            newName = newName.EnsureEndsWith(Path.DirectorySeparatorChar);
                        }

                        entry.SetName(newName);
                        WriteLine($"已处理：{entry}");
                    }
                }
                else
                {
                    ReplacementHelper.Replace(entry, _replacementOptions.Rules);
                    WriteLine($"已处理：{entry}");
                }
            }

            return entry;
        }
    }
}