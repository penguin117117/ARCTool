using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CS = ARCTool.FileSys.Calculation_System;

namespace ARCTool.FileSys
{
    class Format_Checker
    {
        public static void Type_Check(string rarc_path) { 
            FileStream fs = new(rarc_path, FileMode.Open);
            BinaryReader br = new(fs);
            RARC RARC = new();
            Yaz0 Yaz0 = new();

            var Magic = CS.Byte2Char(br);
            fs.Close();
            br.Close();

            Console.WriteLine(BitConverter.ToString(Encoding.GetEncoding("utf-16").GetBytes(Magic)));

            if (Magic == "RARC")
            {
                Console.WriteLine("RARCです");
                RARC.Extract(rarc_path);
            }
            else if (Magic == "Yaz0")
            {
                //Console.WriteLine("Yaz0が含まれたRARCはまだ未対応です");
                Yaz0.Decord(rarc_path);
                Console.WriteLine("Yaz0End");
                var savedirectory = rarc_path.Substring(0, rarc_path.LastIndexOf(@"\"));
                var savefilename = Path.GetFileNameWithoutExtension(rarc_path) + ".rarc";
                var Yaz0_Path = Path.Combine(savedirectory, savefilename);
                if (File.Exists(Yaz0_Path) == false)
                {
                    Console.WriteLine("指定されたyaz0decrarcファイルが存在しません。");
                    Console.WriteLine("アプリケーションを終了します。");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                Console.WriteLine("Yaz0_Path" + Yaz0_Path);
                RARC.Extract(Yaz0_Path);
                //File.Delete(savefilename);
            }
            else
            {
                Console.WriteLine("未対応のファイルです");
                Console.ReadKey();
            }

        }
    }
}
