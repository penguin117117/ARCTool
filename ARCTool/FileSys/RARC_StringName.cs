using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CS = ARCTool.FileSys.Calculation_System;

namespace ARCTool.FileSys
{
    public class RARC_StringName
    {
        public static List<int> NameOffset;
        public static List<string> NameOffsetStr;
        public static void MemoryWrite(MemoryStream ms , BinaryWriter bw ,string[] SDFAA) {

            //
            NameOffset = new List<int>();
            NameOffsetStr = new List<string>();

            //ディレクトリ階層
            CS.String_Writer(bw, "." + (char)0);
            CS.String_Writer(bw, ".." + (char)0);

            //
            foreach (var str in SDFAA)
            {
                var FileName = Path.GetFileName(str);
                NameOffset.Add((int)ms.Position);
                NameOffsetStr.Add(FileName);
                CS.String_Writer(bw, FileName);
            }
        }
    }
}
