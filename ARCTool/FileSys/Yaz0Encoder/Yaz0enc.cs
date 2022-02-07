using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using Syroot.NintenTools.Yaz0;

namespace ARCTool.FileSys.Yaz0Encoder
{
    public class Yaz0Encode
    {
        private static string _filePath;
        private static int _rarcFileSize;

        public static byte[] OriginFileBuffer { get; private set; }
        private static string _yaz0_FullPath;

        public void Start(string rarcPath) 
        {
            _yaz0_FullPath = Path.ChangeExtension(rarcPath, ".arc");

            OriginFileBuffer = File.ReadAllBytes(rarcPath);
            using (FileStream fs = new FileStream(_yaz0_FullPath,FileMode.Open)) 
            {
                using (BinaryWriter bw = new BinaryWriter(fs)) 
                {
                    bw.Write("Yaz0");
                    bw.Write(OriginFileBuffer.Length);
                    bw.Write(0x00000000);
                    bw.Write(0x00000000);

                    EncodeYaz0(bw);
                }
            }

            
            if (OriginFileBuffer.Length <= 0 || OriginFileBuffer == null) 
            {
                throw new Exception("ファイルサイズが0以下です。");
            }
                

        }

        

        private void EncodeYaz0(BinaryWriter bw) 
        {
            ReturnPos returnPos = new ReturnPos(0,0);
            byte[] dst = new byte[24];
            int dstSize = 0;
            int percent = -1;

            uint validBitCount = 0;
            byte currCodeByte = 0;

            using (MemoryStream ms = new MemoryStream(OriginFileBuffer)) 
            {
                using (BinaryReader br = new BinaryReader(ms)) 
                {
                    while (ms.Position < OriginFileBuffer.Length)
                    {
                        uint numBytes;
                        //uint matchPos;
                        uint srcPosBak;

                        numBytes = NintendoEncoder.Start(bw, ms, out uint matchPos);

                        if (numBytes < 3)
                        {
                            dst[returnPos.Dst] = OriginFileBuffer[returnPos.Dst];
                            returnPos.Dst++;
                            returnPos.Src++;
                            currCodeByte |= (byte)(0x80 >> (int)validBitCount);
                        }
                        else
                        { 
                            
                        }
                    }
                }
                
            }

            
        }

        

        private uint toDWord(uint d)
        {
            byte w1 = (byte)(d & 0xFF);
            byte w2 = (byte)((d >> 8) & 0xFF);
            byte w3 = (byte)((d >> 16) & 0xFF);
            byte w4 = (byte)(d >> 24);

            return (uint)((w1 << 24) | (w2 << 16) | (w3 << 8) | w4);
        }

        public struct ReturnPos
        {
            public int Src, Dst;
            public ReturnPos(int src,int dst) 
            {
                Src = src;
                Dst = dst;
            }
        }

        unsafe uint SinpleEnc(byte* src, int size , int pos , uint* pMatchPos) 
        {
            int startPos = pos - 0x1000;
            uint numBytes = 1;
            uint matchPos = 0;

            if (startPos < 0)
                startPos = 0;

            for(int i = startPos; i < pos; i++)
            {
                for (int j = 0; j < size - pos; j++)
                {
                    if (src[i + j] != src[j + pos])
                        break;
                    if (j > numBytes)
                    {
                        numBytes = (uint)j;
                        matchPos = (uint)i;
                    }
                }
            }

            *pMatchPos = matchPos;
            if (numBytes == 2)
                numBytes = 1;
            return numBytes;
        }


        public unsafe void EncodeAll(byte* src) 
        {
            char[] dstName = new char[300];
            char[] dummy = new char[8];
            int dstSize;

            Console.WriteLine($"{dstName}:{_filePath }.yaz0");
        }

        //public static byte[] ArrayReverse(byte[] array)
        //{
        //    Array.Reverse(array);
        //    return array;
        //}

        //public static Int32 SwapEndianness(Int32 value)
        //{
        //    return BitConverter.ToInt32(ArrayReverse(BitConverter.GetBytes(value)), 0);
        //}

        //static void Main(string[] args)
        //{
        //    if (args.Count() == 3)
        //    {
        //        switch (args[0])
        //        {
        //            case "decode":
        //                if (File.Exists(args[1]))
        //                {
        //                    FileInfo fi = new FileInfo(args[1]);
        //                    Yaz0Compression.Decompress(fi.FullName, args[2]);
        //                }
        //                else
        //                {
        //                    Console.WriteLine("The specified input file does not exist");
        //                }
        //                break;
        //            case "encode":
        //                if (File.Exists(args[1]))
        //                {
        //                    FileInfo fi = new FileInfo(args[1]);
        //                    byte[] decodedBuffer = File.ReadAllBytes(fi.FullName);
        //                    Int32 bulkChunksCount = decodedBuffer.Length / 0x08;
        //                    Int32 extraChunkSize = decodedBuffer.Length % 0x08;
        //                    byte[] chunkBuffer = new byte[0x08];
        //                    using (FileStream fs = new FileStream(args[2], FileMode.Create, FileAccess.Write))
        //                    {
        //                        fs.Write(Encoding.UTF8.GetBytes("Yaz0"), 0, 4);
        //                        fs.Write(BitConverter.GetBytes(SwapEndianness((Int32)fi.Length)), 0, 4);
        //                        fs.Write(BitConverter.GetBytes(SwapEndianness(0x00000000)), 0, 4); //Can be 0x00002000 sometimes, check the original Yaz0 file at offset 0x08
        //                        fs.Write(BitConverter.GetBytes(SwapEndianness(0x00000000)), 0, 4);

        //                        for (int i = 0; i < bulkChunksCount; i++)
        //                        {
        //                            fs.Write(BitConverter.GetBytes(0xFF), 0, 1);
        //                            Array.Copy(decodedBuffer, (i * 8), chunkBuffer, 0, 8);
        //                            fs.Write(chunkBuffer, 0, 8);
        //                        }

        //                        if (extraChunkSize != 0)
        //                        {
        //                            Array.Resize(ref chunkBuffer, extraChunkSize);
        //                            Array.Copy(decodedBuffer, (0x08 * bulkChunksCount), chunkBuffer, 0, chunkBuffer.Length);

        //                            Byte codeByte = 0x00;
        //                            switch (extraChunkSize)
        //                            {
        //                                case 1:
        //                                    codeByte = 0x80;
        //                                    break;
        //                                case 2:
        //                                    codeByte = 0xC0;
        //                                    break;
        //                                case 3:
        //                                    codeByte = 0xE0;
        //                                    break;
        //                                case 4:
        //                                    codeByte = 0xF0;
        //                                    break;
        //                                case 5:
        //                                    codeByte = 0xF8;
        //                                    break;
        //                                case 6:
        //                                    codeByte = 0xFC;
        //                                    break;
        //                                case 7:
        //                                    codeByte = 0xFE;
        //                                    break;
        //                                default:
        //                                    break;
        //                            }

        //                            fs.Write(BitConverter.GetBytes(codeByte), 0, 1);
        //                            fs.Write(chunkBuffer, 0, chunkBuffer.Length);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    Console.WriteLine("The specified input file does not exist");
        //                }
        //                break;
        //            default:
        //                break;
        //        }
        //        Console.WriteLine("Finished!");
        //        Console.WriteLine("Press any key to exit ...");
        //        Console.ReadKey();
        //    }
        //    else
        //    {
        //        //Show help
        //        Console.WriteLine("Yaz0.exe decode <input_file> <output_file>");
        //        Console.WriteLine("Yaz0.exe encode <input_file> <output_file>");
        //        Console.WriteLine("Press any key to exit ...");
        //        Console.ReadKey();
        //    }
        //}
    }
}