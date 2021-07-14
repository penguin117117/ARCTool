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
            FileStream fs = new FileStream(rarc_path, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            var Magic = CS.Byte2Char(br);

            if (Magic == "RARC")
            {
                Console.WriteLine("RARCです");
                fs.Close();
                br.Close();
                RARC.Read(rarc_path);
            }
            else if (Magic == "Yaz0")
            {
                Console.WriteLine("Yaz0です");
                fs.Close();
                br.Close();
            }
            else {
                Console.WriteLine("未対応です");
                fs.Close();
                br.Close();
            }

            //fs.Close();
            //br.Close();
        }
    }
}
