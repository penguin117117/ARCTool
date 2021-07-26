using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using ARCTool.FileSys;
using System.Reflection;



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
                Console.WriteLine("");
                Console.WriteLine("製作者：ぺんぐいん");
                Console.WriteLine("Created by penguin117117");
                Console.WriteLine("https://github.com/penguin117117/ARCTool");
                Console.WriteLine("バグなどの報告は下記URLへ");
                Console.WriteLine("https://github.com/penguin117117/ARCTool/issues");
                Console.WriteLine("終了するには何かキーを押してください");
                Console.ReadKey();
                Environment.Exit(0);
                
            }

            //先頭の配列が空白なのでスキップした配列を作成
            IEnumerable<string> aps = all_path_strings.Skip(1);
            all_path_strings = aps.ToArray();

            foreach (var path in all_path_strings) {
                if (File.Exists(path)){
                    Console.WriteLine(path);
                    Console.WriteLine("//////////以下解凍処理//////////");
                    Format_Checker.Type_Check(path);
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
                    Console.WriteLine("");
                    Console.WriteLine("//////////以下圧縮処理//////////");
                    RARC rarc = new RARC();
                    var arcfile = Path.GetFileName(path);
                    var arcfolder = Path.GetDirectoryName(path);
                    Console.WriteLine(arcfolder+@"\"+arcfile+".arc");
                    rarc.Write(arcfolder + @"\" + arcfile + ".arc",DirStrs.ToArray(),FileStrs.ToArray());

                }
                else {
                    Console.WriteLine(path);
                    Console.WriteLine("上記のパスはファイルでも、フォルダでもありません");
                }
            }

            

        }
    }
}
