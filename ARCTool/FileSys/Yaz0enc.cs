//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using Syroot.NintenTools.Yaz0;

//namespace Yaz0
//{
//    class Program
//    {
//        public static byte[] ArrayReverse(byte[] array)
//        {
//            Array.Reverse(array);
//            return array;
//        }

//        public static Int32 SwapEndianness(Int32 value)
//        {
//            return BitConverter.ToInt32(ArrayReverse(BitConverter.GetBytes(value)), 0);
//        }

//        static void Main(string[] args)
//        {
//            if (args.Count() == 3)
//            {
//                switch (args[0])
//                {
//                    case "decode":
//                        if (File.Exists(args[1]))
//                        {
//                            FileInfo fi = new FileInfo(args[1]);
//                            Yaz0Compression.Decompress(fi.FullName, args[2]);
//                        }
//                        else
//                        {
//                            Console.WriteLine("The specified input file does not exist");
//                        }
//                        break;
//                    case "encode":
//                        if (File.Exists(args[1]))
//                        {
//                            FileInfo fi = new FileInfo(args[1]);
//                            byte[] decodedBuffer = File.ReadAllBytes(fi.FullName);
//                            Int32 bulkChunksCount = decodedBuffer.Length / 0x08;
//                            Int32 extraChunkSize = decodedBuffer.Length % 0x08;
//                            byte[] chunkBuffer = new byte[0x08];
//                            using (FileStream fs = new FileStream(args[2], FileMode.Create, FileAccess.Write))
//                            {
//                                fs.Write(Encoding.UTF8.GetBytes("Yaz0"), 0, 4);
//                                fs.Write(BitConverter.GetBytes(SwapEndianness((Int32)fi.Length)), 0, 4);
//                                fs.Write(BitConverter.GetBytes(SwapEndianness(0x00000000)), 0, 4); //Can be 0x00002000 sometimes, check the original Yaz0 file at offset 0x08
//                                fs.Write(BitConverter.GetBytes(SwapEndianness(0x00000000)), 0, 4);

//                                for (int i = 0; i < bulkChunksCount; i++)
//                                {
//                                    fs.Write(BitConverter.GetBytes(0xFF), 0, 1);
//                                    Array.Copy(decodedBuffer, (i * 8), chunkBuffer, 0, 8);
//                                    fs.Write(chunkBuffer, 0, 8);
//                                }

//                                if (extraChunkSize != 0)
//                                {
//                                    Array.Resize(ref chunkBuffer, extraChunkSize);
//                                    Array.Copy(decodedBuffer, (0x08 * bulkChunksCount), chunkBuffer, 0, chunkBuffer.Length);

//                                    Byte codeByte = 0x00;
//                                    switch (extraChunkSize)
//                                    {
//                                        case 1:
//                                            codeByte = 0x80;
//                                            break;
//                                        case 2:
//                                            codeByte = 0xC0;
//                                            break;
//                                        case 3:
//                                            codeByte = 0xE0;
//                                            break;
//                                        case 4:
//                                            codeByte = 0xF0;
//                                            break;
//                                        case 5:
//                                            codeByte = 0xF8;
//                                            break;
//                                        case 6:
//                                            codeByte = 0xFC;
//                                            break;
//                                        case 7:
//                                            codeByte = 0xFE;
//                                            break;
//                                        default:
//                                            break;
//                                    }

//                                    fs.Write(BitConverter.GetBytes(codeByte), 0, 1);
//                                    fs.Write(chunkBuffer, 0, chunkBuffer.Length);
//                                }
//                            }
//                        }
//                        else
//                        {
//                            Console.WriteLine("The specified input file does not exist");
//                        }
//                        break;
//                    default:
//                        break;
//                }
//                Console.WriteLine("Finished!");
//                Console.WriteLine("Press any key to exit ...");
//                Console.ReadKey();
//            }
//            else
//            {
//                //Show help
//                Console.WriteLine("Yaz0.exe decode <input_file> <output_file>");
//                Console.WriteLine("Yaz0.exe encode <input_file> <output_file>");
//                Console.WriteLine("Press any key to exit ...");
//                Console.ReadKey();
//            }
//        }
//    }
//}