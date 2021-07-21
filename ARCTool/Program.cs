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
        private static string[] dir_path_strings;

        public static void Main(string[] args)
        {
            //キー入力後


            //exeファイルにドラッグ＆ドロップしたファイルパスを配列に入れる。
            all_path_strings = Environment.GetCommandLineArgs();
            if (all_path_strings.Count() == 1){
                Console.WriteLine("exeファイルをダブルクリックせずに");
                Console.WriteLine("exeファイルにドラッグアンドドロップしてください。");
                Environment.Exit(0); 
            }

            //先頭の配列が空白なのでスキップした配列を作成
            IEnumerable<string> aps = all_path_strings.Skip(1);
            all_path_strings = aps.ToArray();


            //Console.WriteLine(all_path_strings[0]);
            //Console.ReadKey();

            //for (int i = 0; i < all_path_strings.Count(); i++)
            //{
            //    Console.WriteLine();
            //    var test2 = Directory.GetFiles(all_path_strings[i], "*", SearchOption.AllDirectories).OrderBy(sort => sort);

            //    foreach (var item in test2) Console.WriteLine(item);
            //}
            foreach (var path in all_path_strings) {
                if (File.Exists(path)){
                    
                    Format_Checker.Type_Check(path);
                    Console.WriteLine(path);
                    Console.WriteLine("ファイルです");
                }
                else if (Directory.Exists(path)){
                    var DirStrs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories).OrderBy(sort => sort);
                    var FileStrs = Directory.GetFiles(path, "*", SearchOption.AllDirectories).OrderBy(sort => sort);
                    if (DirStrs.Count() < 1) {
                        Console.WriteLine(path);
                        Console.WriteLine("圧縮できないフォルダです、サブフォルダが1つもありません");
                        continue;
                    }
                    if (FileStrs.Count() < 1) {
                        Console.WriteLine(path);
                        Console.WriteLine("圧縮できないフォルダです、ファイルが1つもありません");
                        continue;
                    }
                    Console.WriteLine(path);
                    Console.WriteLine("圧縮可能なフォルダです");
                }
                else {
                    Console.WriteLine(path);
                    Console.WriteLine("上記のパスはファイルでも、フォルダでもありません");
                }
            }

            

            ////folderの場合
            //IEnumerable<string> dirWhere = all_path_strings.Where(o => Directory.GetDirectories(o).Count() > 0);
            //dir_path_strings = dirWhere.ToArray();
            //foreach (var testwritepath in dir_path_strings) Console.WriteLine(Path.GetFileName( testwritepath));
            ////Console.ReadKey();

            ////.arcの拡張子が付くもののみ新しい配列に入れる。
            //IEnumerable<string> arcWhere = all_path_strings.Where(o => Path.GetExtension(o) == ".arc");
            //arc_path_strings = arcWhere.ToArray();

            //if (arc_path_strings.Count() == 0) return;
            ////.arcファイルのみのパスを出力
            //foreach (var arcfile in arcWhere)
            //{
            //    Console.WriteLine(arcfile);
            //    Format_Checker.Type_Check(arcfile);
            //}

            //Folderの場合

            Console.WriteLine("終了するには何かキーを押してください");
            Console.ReadKey();

        }
    }
}
