using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ARCTool.FileSys
{
    public class DirectoryFileEdit
    {
        public static string[] DirectoryNameSort(string path) {
            return Directory.GetDirectories(path, "*", SearchOption.AllDirectories).OrderBy(sort => sort).ToArray();
        }

        public static string[] FileNameSort(string path) {
            return Directory.GetFiles(path, "*", SearchOption.AllDirectories).OrderBy(sort => sort).ToArray();
        }
    }
}
