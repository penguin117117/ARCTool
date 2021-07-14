using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using ARCTool.FileSys;

namespace ARCTool
{
    class Program
    {
        private static string[] all_path_strings;
        private static string[] arc_path_strings;
        
         public static void Main(string[] args)
        {
            //exeファイルにドラッグ＆ドロップしたファイルパスを配列に入れる。
            all_path_strings =  Environment.GetCommandLineArgs();

            //.arcの拡張子が付くもののみ新しい配列に入れる。
            IEnumerable<string> testWhere = all_path_strings.Where(o => Path.GetExtension(o) == ".arc");
            arc_path_strings = testWhere.ToArray();

            //.arcファイルのみのパスを出力
            foreach (var arcfile in testWhere) { 
                Console.WriteLine(arcfile);
                Format_Checker.Type_Check(arcfile);
            }
            


            //キー入力後
            Console.WriteLine("終了するには何かキーを押してください");
            Console.ReadKey();
        }
    }
}
