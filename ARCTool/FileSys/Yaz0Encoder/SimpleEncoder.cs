using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ARCTool.FileSys.Yaz0Encoder
{
    public class SimpleEncoder
    {
        

        public static uint SinpleEnc(BinaryWriter bw, MemoryStream ms, ref uint refMatchPos)
        {
            int pos = (int)bw.BaseStream.Position;
            int startPos = pos - 0x1000;
            uint numBytes = 1;
            uint matchPos = 0;

            if (startPos < 0)
                startPos = 0;

            for (int writePos = startPos; writePos < pos; writePos++)
            {
                
                for (int originReadPos = 0; originReadPos < (int)ms.Length - pos; originReadPos++)
                {
                    if (Yaz0Encode.OriginFileBuffer[writePos + originReadPos] != Yaz0Encode.OriginFileBuffer[originReadPos + pos])
                        break;
                    if (originReadPos > numBytes)
                    {
                        numBytes = (uint)originReadPos;
                        matchPos = (uint)writePos;
                    }
                }
            }

            refMatchPos = matchPos;
            if (numBytes == 2)
                numBytes = 1;
            return numBytes;
        }

        
    }
}
