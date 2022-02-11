using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CS = ARCTool.FileSys.Calculation_System;

namespace ARCTool.FileSys
{
    public class RARC_NodeSection
    {
        public BinaryWriter bw { get; set; }
        public string[] dirstrs { get; set; }
        public List<int> nameoffset { get; set; }
        public List<string> nameoffsetstr { get; set; }
        public List<int> IDIC { get; set; }

        public int Write() {
            var indexcount = 0;
            var nodeitemcounter = 0;
            for (var i = 0; i < dirstrs.Count(); i++)
            {
                var dirName = Path.GetFileName(dirstrs[i]);
                var olddirName = dirName;
                if (i == 0)
                {
                    CS.String_Writer(bw, "ROOT");
                }
                else
                {
                    var shortageCount = 4 - dirName.Count();
                    if (shortageCount > -1)
                    {
                        for (var j = 0; j < shortageCount; j++)
                            dirName += (char)0x20;
                    }
                    var dirShortName = dirName.Substring(0, 4);
                    CS.String_Writer(bw, dirShortName.ToUpper());
                }
                var findnameindex = nameoffsetstr.IndexOf(olddirName + (char)0, indexcount);
                indexcount = findnameindex + 1;
                CS.String_Writer_Int(bw, nameoffset[findnameindex]);
                CS.String_Writer_Int(bw, (short)CS.ARC_Hash(olddirName));
                CS.String_Writer_Int(bw, (short)IDIC[i]);

                CS.String_Writer_Int(bw, nodeitemcounter);
                nodeitemcounter += IDIC[i];
            }
            return nodeitemcounter;
        }
    }
}
