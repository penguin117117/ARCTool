﻿using System;
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
            RARC rarc = new RARC();
            Yaz0 yaz0 = new Yaz0();

            var Magic = CS.Byte2Char(br);

            if (Magic == "RARC")
            {
                Console.WriteLine("RARCです");
                fs.Close();
                br.Close();
                rarc.Read(rarc_path);
            }
            else if (Magic == "Yaz0")
            {
                Console.WriteLine("Yaz0が含まれたRARCはまだ未対応です");
                fs.Close();
                br.Close();
                yaz0.Decord(rarc_path);
                Console.ReadKey();
            }
            else {
                Console.WriteLine("未対応のファイルです");
                fs.Close();
                br.Close();
            }

            //fs.Close();
            //br.Close();
        }
    }
}
