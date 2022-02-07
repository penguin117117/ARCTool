using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARCTool.FileSys.Yaz0Encoder
{
    public class NintendoEncoder
    {
        private static uint numByte1;
        private static uint matchPos;
        private static int prevFlag = 0;

        public static uint Start(BinaryWriter bw,MemoryStream ms,out uint refMatchPos)
        {
            int startPos = (int)ms.Position - 0x1000;
            uint numBytes = 1;

            if (prevFlag == 1)
            {
                refMatchPos = matchPos;
                prevFlag = 0;
                return numByte1;
            }

            prevFlag = 0;
            numBytes = SimpleEncoder.SinpleEnc(bw,ms,ref matchPos);
            refMatchPos = matchPos;

            if (numBytes >= 3) 
            {
                numByte1 = SimpleEncoder.SinpleEnc(bw, ms, ref matchPos);
                if (numByte1 >= numBytes + 2) 
                {
                    numBytes = 1;
                    prevFlag = 1;
                }
            }

            return numBytes;
        }
    }
}
