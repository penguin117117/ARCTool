using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using ARCTool.FileSys;
using System.Reflection;
using System.Text.RegularExpressions;



namespace ARCTool
{
    class Program
    {
        private static string[] all_path_strings;
        private static string[] arc_path_strings;
        private static string[] dir_path_strings;
        private static char yesno = 'n';

        public static void Main(string[] args)
        {
            //exeファイルにドラッグ＆ドロップしたファイルパスを配列に入れる。
            all_path_strings = Environment.GetCommandLineArgs();

            if (all_path_strings.Count() == 1)
            {
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


            //Console.WriteLine("全ての項目にYaz0圧縮をしますか？");
            //Console.WriteLine("※解凍時は自動でYaz0の処理を実行します。");
            //Console.WriteLine("y Yaz0圧縮を使用　n Yaz0圧縮を使用しない");

            //var yesnoChars = Console.ReadLine().ToCharArray();
            //yesno = yesnoChars[0];

            
            //Console.WriteLine("全パス2"+all_path_strings.Count());

            var isFirstTime = true;
            foreach (var path in all_path_strings)
            {
                


                Console.WriteLine("圧縮フォルダパス" + path);
                if (File.Exists(path))
                {
                    Format_Checker.Type_Check(path);
                    continue;
                }

                var PathReplace = path;
                
                if (Directory.Exists(PathReplace))
                {
                    if (isFirstTime)
                    {
                        yesno = Yaz0.Use_Yaz0_Encode();
                        isFirstTime = false;
                    }

                    Console.WriteLine("圧縮フォルダパス"+PathReplace);
                    var DirStrs = DirectoryFileEdit.DirectoryNameSort(PathReplace);
                    var FileStrs = DirectoryFileEdit.FileNameSort(PathReplace);
                    if (DirStrs.Length < 1) continue;
                    if (FileStrs.Length < 1) continue;

                    
                    var arcfile = Path.GetFileName(PathReplace);
                    var arcfolder = Path.GetDirectoryName(PathReplace);

                    RARC rarc = new();

                    var ArcExtractPath = arcfolder + @"\" + arcfile;
                    if (yesno == 'y' || yesno == 'Y' || yesno == 'ｙ' || yesno == 'Ｙ')
                    {
                        rarc.Archive(ArcExtractPath + ".rarc", DirStrs, FileStrs);
                        Console.WriteLine("yaz0処理に入りました");
                        Console.WriteLine("圧縮中・・・");
                        AppExecuter.Start(arcfolder + @"\" + arcfile + ".rarc");
                        //Yaz0 yaz0 = new();
                        //yaz0.Encode2(arcfolder + @"\" + arcfile + ".rarc");



                        Console.WriteLine("Yaz0圧縮できました");
                        //Console.ReadKey();
                        continue;
                    }

                    rarc.Archive(ArcExtractPath + ".arc", DirStrs, FileStrs);
                    Console.WriteLine("Yaz0処理をしていません");
                }
            }
        }
    }
}
