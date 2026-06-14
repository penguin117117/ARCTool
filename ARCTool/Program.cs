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
using System.Diagnostics;


namespace ARCTool
{
    class Program
    {
        static bool isUseOtherExefile = false;

        private static string[] all_path_strings;
        private static string[] arc_path_strings;
        private static string[] dir_path_strings;
        private static Yaz0.UseStatus yaz0EncodeStatus = Yaz0.UseStatus.UnUse;

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

            Debug.WriteLine("全パス数"+ all_path_strings.Count());

            Yaz0 yaz0 = new();

            var isFirstTime = true;
            foreach (var path in all_path_strings)
            {

                //Console.WriteLine("圧縮フォルダパス" + path);
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
                        yaz0EncodeStatus = Yaz0.Use_Yaz0_Encode();
                        isFirstTime = false;
                    }

                    //Console.WriteLine("圧縮フォルダパス"+PathReplace);
                    var DirStrs = DirectoryFileEdit.DirectoryNameSort(PathReplace);
                    var FileStrs = DirectoryFileEdit.FileNameSort(PathReplace);

                    Console.WriteLine($"ファイル数{FileStrs.Length}");
                    if (DirStrs.Length < 1) continue;
                    if (FileStrs.Length < 1) continue;


                    var arcfile = Path.GetFileName(PathReplace);
                    var arcfolder = Path.GetDirectoryName(PathReplace);

                    RARC rarc = new();

                    var ArcExtractPath = arcfolder + @"\" + arcfile;
                    if (yaz0EncodeStatus is Yaz0.UseStatus.UseNew)
                    {
                        MemoryStream memst = new();

                        memst.Seek(0, SeekOrigin.Begin);
                        rarc.Archive(new BinaryWriter(memst), DirStrs, FileStrs);
                        Console.WriteLine("yaz0処理に入りました");
                        Console.WriteLine("圧縮中・・・");

                        memst.Seek(0, SeekOrigin.Begin);

                        yaz0.EncodeOptimizeV2(Path.ChangeExtension(ArcExtractPath, "arc"), new BinaryReader(memst));

                        memst.Close();

                        Console.WriteLine("Yaz0圧縮できました");
                        continue;
                    }
                    else if (yaz0EncodeStatus is Yaz0.UseStatus.Use)
                    {
                        rarc.Archive(ArcExtractPath + ".rarc", DirStrs, FileStrs);
                        Console.WriteLine("yaz0処理に入りました");
                        Console.WriteLine("圧縮中・・・");
                        AppExecuter.Start(arcfolder + @"\" + arcfile + ".rarc");
                        Console.WriteLine("Yaz0圧縮できました");
                        continue;
                    }

                    rarc.Archive(ArcExtractPath + ".arc", DirStrs, FileStrs);
                    Console.WriteLine("Yaz0処理をしていません");
                    Console.ReadKey();
                }
            }
        }
    }
}
