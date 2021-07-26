using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CS = ARCTool.FileSys.Calculation_System;

namespace ARCTool.FileSys
{
    public class RARC_Directory_Structure
    {
        public BinaryWriter bw { get; set; }
        //public List<string> listdirfile { get; set; }
        //public string parentString { get; set; }
        public string[] dirstrs { get; set; }
        public string node { get; set; }
        public void Writer() {
            var listdirfile = dirstrs.ToList();
            var parentString = node.Substring(0, node.LastIndexOf(@"\"));
            //var dirparent = listdirfile2.IndexOf();
            var parentDir = listdirfile.IndexOf(parentString);
            var nowDir = listdirfile.IndexOf(node);



            Console.WriteLine(nowDir + "_____________________index番号");
            Console.WriteLine(parentDir + "_____________________親index番号");
            CS.String_Writer_Int(bw, (short)-1);
            CS.String_Writer_Int(bw, (short)CS.ARC_Hash("."));
            CS.String_Writer_Int(bw, (short)0x0200);
            CS.String_Writer_Int(bw, (short)0x0000);
            CS.String_Writer_Int(bw, nowDir);
            CS.String_Writer_Int(bw, 0x00000010);
            CS.String_Writer_Int(bw, 0x00000000);

            CS.String_Writer_Int(bw, (short)-1);
            CS.String_Writer_Int(bw, (short)CS.ARC_Hash(".."));
            CS.String_Writer_Int(bw, (short)0x0200);
            CS.String_Writer_Int(bw, (short)0x0002);
            CS.String_Writer_Int(bw, parentDir);
            CS.String_Writer_Int(bw, 0x00000010);
            CS.String_Writer_Int(bw, 0x00000000);
        }
    }
}
