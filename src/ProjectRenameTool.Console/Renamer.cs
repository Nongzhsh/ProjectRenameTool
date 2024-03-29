﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IgnoreParser _ignoreCopyParser = new("CopyParser");
        private readonly IgnoreParser _ignoreReplaceParser = new("ReplaceParser");

        private string _outputFolderPath;
        private readonly FileEntryList _outputFileEntryList = new(new List<FileEntry>());

        public Renamer(IWritableOptions<ReplacementOptions> options)
        {
            _replacementOptions = options.Value;
            _outputFolderPath = _replacementOptions.OutputFolderPath;
        }

        public async Task RunAsync()
        {
            LoadIgnoreGlobRules();
           await ReplaceAllAsync();
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

        private async Task ReplaceAllAsync()
        {
            try
            {
                if (_replacementOptions.SourcePath.IsZipFile())
                {
                 await   ReplaceInZipFileAsync();
                }
                else
                {
                  await  ReplaceInDirectoryAsync();
                }
            }
            catch
            {
                if (Directory.Exists(_outputFolderPath))
                {
                    Directory.Delete(_outputFolderPath);
                }

                if (File.Exists(_outputFolderPath))
                {
                    File.Delete(_outputFolderPath);
                }
                throw;
            }
        }

        private async Task ReplaceInZipFileAsync()
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

            if (!_outputFolderPath.EndsWith(newFileName))
            {
                _outputFolderPath = Path.Combine(_outputFolderPath, $"{newFileName}");
            }

            await _outputFileEntryList.SaveToZipFileAsync(_outputFolderPath);
        }

        private async Task ReplaceInDirectoryAsync()
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

            await CopyAndReplace(rootFolder);

            //TODO:保存 zip？
        }

        private async Task CopyAndReplace(DirectoryInfo source)
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

                    var entry = new FileEntry(entryName, await fileStream.GetAllBytesAsync(), false);

                    entry = Replace(file.FullName, entry);

                    var newFilePath = Path.Combine(_outputFolderPath, entry.Name.TrimStart(Path.DirectorySeparatorChar));
                    if (!Path.GetDirectoryName(newFilePath).IsNullOrEmpty())
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
                    }

                    await File.WriteAllBytesAsync(newFilePath, entry.Bytes);

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

                await CopyAndReplace(dir);
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
                        WriteLine($"{entry}");
                    }
                }
                else
                {
                    ReplacementHelper.Replace(entry, _replacementOptions.Rules);
                    WriteLine($"{entry}");
                }
            }

            return entry;
        }
    }
}