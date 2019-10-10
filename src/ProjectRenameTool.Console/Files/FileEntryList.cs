using System.Collections.Generic;

namespace ProjectRenameTool.Console.Files
{
    public class FileEntryList : List<FileEntry>
    {
        public FileEntryList(IEnumerable<FileEntry> entries)
            : base(entries)
        {
            
        }
    }
}