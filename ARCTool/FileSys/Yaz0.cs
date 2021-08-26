using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using CS = ARCTool.FileSys.Calculation_System;

namespace ARCTool.FileSys
{
    public class Yaz0
    {
        public string Magic { get; set; }
        public int OriginalDataSize { get; set; }
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        private readonly bool[] bit7 = {true,true,true,true,true,true,true};
        public void Decord(string filepath) {
            var savedirectory = filepath.Substring(0,filepath.LastIndexOf(@"\"));
            var savefilename =  Path.GetFileNameWithoutExtension(filepath) + ".rarc";
            var savefullpath = Path.Combine(savedirectory,savefilename);
            Console.WriteLine(savefullpath);

            

            FileStream fs = new FileStream(filepath ,FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            

            Magic            = CS.Byte2Char(br);
            OriginalDataSize = CS.Byte2Int(br);
            Unknown1         = CS.Byte2Int(br);
            Unknown2         = CS.Byte2Int(br);

            var DecFile = 0;
            List<bool> bitlist = new List<bool>();

            List<byte> Yaz0DecDeta = new List<byte>();
            while (Yaz0DecDeta.Count < OriginalDataSize) {
                byte bit =  br.ReadByte();
                byte[] bits = new byte[] { bit };
                BitArray bitArray = new BitArray(bits);
                bitlist =  BitArrayReverser(bitArray,bitlist);

                foreach (var bititem in bitlist) {
                    Console.WriteLine(bititem + "_flag______" + bitlist.Count()) ;
                    if (bititem == true) {
                        var writedata =  br.ReadByte();
                        Console.WriteLine("DATA______"+writedata.ToString("X2")+"そのまま");
                        Yaz0DecDeta.Add(writedata); 
                    }
                    else
                    {
                        var bita = br.ReadByte();
                        var bitb = br.ReadByte();

                        Console.WriteLine(bita.ToString("X"));

                        byte a_top4 = (byte)(bita >> 4); //1
                        byte a_last4 = (byte)(bita << 4);//0
                        Console.WriteLine(a_last4.ToString("X") + "＿＿＿a_last4");
                        Console.WriteLine(a_top4.ToString("X") + "＿＿＿a_top4");
                        //if (a_top4 != 0)
                        //a_top4 = (byte)(a_top4 << 4);
                        //var b_last4 = (byte)(bitb << 4);
                        //b_last4 = (byte)(b_last4>> 4);

                        //int pos_same_String = ((a_last4<<8) | bitb)+1;
                        int pos_same_String = (a_last4<<4 | bitb)+1;
                        Console.WriteLine(pos_same_String.ToString("X2"));
                       
                        int writebyteNum = 0;
                        if (a_top4 == 0)
                        {
                            writebyteNum = br.ReadByte() + 0x12;
                            Console.WriteLine("特殊");
                        }
                        else
                        {
                            writebyteNum = a_top4 + 2;
                        }


                        for (int i = 0; i < writebyteNum; i++)
                        {
                            var sameindex = Yaz0DecDeta.Count - pos_same_String;
                            Console.WriteLine("yaz0decdata "+Yaz0DecDeta.Count.ToString("X")+"  sameindex"+sameindex.ToString("X"));
                            Console.WriteLine("DATA______" + Yaz0DecDeta[sameindex].ToString("X2"));
                            Yaz0DecDeta.Add(Yaz0DecDeta[sameindex]);
                            
                        }
                    }
                    if (Yaz0DecDeta.Count == OriginalDataSize) break;
                }

                //if (ms.Length < OriginalDataSize) {
                //    Console.WriteLine("yazend");    
                //    break; 
                //}
                
                //Console.WriteLine("anykey＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿");
                //Console.ReadKey();
                //if (Yaz0DecDeta.Count() < OriginalDataSize) break;
            }

            FileStream fs2 = new FileStream(savefullpath,FileMode.Create);
            BinaryWriter bwYaz0 = new BinaryWriter(fs2);
            bwYaz0.Write(Yaz0DecDeta.ToArray());


            br.Close();
            fs.Close();
            bwYaz0.Close();
            fs2.Close();

        }


        private static List<bool> BitArrayReverser(BitArray bitArray , List<bool> bitlist) {
            bitlist = new List<bool>();
            foreach (var tes in bitArray) bitlist.Add((bool)tes);
            bitlist.Reverse();
            return bitlist;
        }

        //public void Encode(string filepath) {
        //    var Yaz0_Dir = filepath.Substring(0,filepath.LastIndexOf(@"\"));
        //    var Yaz0_file = Path.GetFileNameWithoutExtension(filepath);
        //    var Yaz0_FullPath = Path.Combine(Yaz0_Dir,Yaz0_file+".arc");

        //    FileStream fsRARC = new FileStream(filepath,FileMode.Open);
        //    BinaryReader brRARC = new BinaryReader(fsRARC);

        //    FileStream fsYaz0 = new FileStream(Yaz0_FullPath,FileMode.Create);
        //    BinaryWriter bwYaz0 = new BinaryWriter(fsYaz0);

        //    if(File.Exists(filepath) == false) return;
        //    var OriginalData = File.ReadAllBytes(filepath);
        //    OriginalDataSize = OriginalData.Length;

        //    //Yaz0ヘッダー
        //    CS.String_Writer(bwYaz0,"Yaz0");
        //    CS.String_Writer_Int(bwYaz0,OriginalDataSize);
        //    CS.Null_Writer_Int32(bwYaz0,2);

        //    //group変数
        //    byte group_head        = 0;
        //    int  group_head_length = 0;

        //    //圧縮処理
        //    while (fsRARC.Length < OriginalDataSize) {
        //        if (group_head_length == 0) { 
        //        group_head = 
        //        }

        //        //グループヘッダーバイトを「左に1シフト」
        //        group_head <<= 1;

        //        //whileループを抜ける条件
        //        break;
        //    }

        //    brRARC.Close();
        //    fsRARC.Close();
        //    bwYaz0.Close();
        //    fsYaz0.Close();
        //} 


        public static byte[] ArrayReverse(byte[] array)
        {
            Array.Reverse(array);
            return array;
        }

        public static void WriterSwapEndianness(BinaryWriter bw , Int32 value , int sindex , int eindex)
        {
            var OldBytes = BitConverter.GetBytes(value);
            var ReverseBytes = ArrayReverse(OldBytes);
            var NewInt32 = BitConverter.ToInt32(ReverseBytes, 0);
            var NewBytes = BitConverter.GetBytes(NewInt32);
            bw.Write(NewBytes, sindex, eindex);
        }

        

        public void Encode(string filepath)
        {
            var Yaz0_Dir = filepath.Substring(0, filepath.LastIndexOf(@"\"));
            var Yaz0_file = Path.GetFileNameWithoutExtension(filepath);
            var Yaz0_FullPath = Path.Combine(Yaz0_Dir, Yaz0_file + ".arc");

            FileStream fsRARC = new FileStream(filepath,FileMode.Open);
            var rarcLen = (Int32)fsRARC.Length;
            fsRARC.Close();

            byte[] Original_File = File.ReadAllBytes(filepath);

            

            Console.WriteLine(Original_File.Length.ToString("X"));
            Int32 Original_File_Chunks = Original_File.Length / 0x08;//0x44
            Int32 extraChunkSize  = Original_File.Length % 0x08;//0x00
            byte[] chunkBuffer = new byte[0x08];

            FileStream fs = new FileStream(Yaz0_FullPath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(Encoding.UTF8.GetBytes("Yaz0"), 0, 4);
            WriterSwapEndianness(bw, rarcLen, 0, 4);
            WriterSwapEndianness(bw, 0x00000000, 0, 4); //Can be 0x00002000 sometimes, check the original Yaz0 file at offset 0x08
            WriterSwapEndianness(bw, 0x00000000, 0, 4);



            //ライブラリデータの作成
            //この文字列を参照して圧縮処理を行う
            List<byte> String_Libraries = new List<byte>(); ;
            var ReadCount = 0;

            //オリジナルファイルのチャンク数分のループ
            for (var WriteCount = 0; WriteCount < Original_File_Chunks; WriteCount++) {

                List<byte> GroupByte = new List<byte>();
                GroupByte.Add(Original_File[WriteCount]);
                var pos_ReadFlag = fs.Position;
                bw.Write((byte)0xFF);

                ArraySegment<byte> AS_Original_File = new ArraySegment<byte>(Original_File,(int)pos_ReadFlag,8);
                foreach (var ASOF in AS_Original_File) { 
                    
                }




                //1バイト目の処理
                if (WriteCount == 0) {
                    String_Libraries.Add(Original_File[WriteCount]);
                    bw.Write(String_Libraries[WriteCount]);
                    var GroupByteCount = 0;
                    while (true) {
                        GroupByte.Add(String_Libraries[WriteCount + GroupByteCount]);
                        GroupByteCount++;


                    }
                    continue;
                }
                
                

                while (true) {
                    var Find_Index00 = String_Libraries.LastIndexOf(GroupByte[0]);
                    var Find_Index01 = String_Libraries.LastIndexOf(GroupByte[1]);

                    if ((Find_Index00 == -1) && (Find_Index01 == -1)){
                        bw.Write(Original_File[WriteCount]);
                    }
                    else { 
                    
                    }
                }
                
               
                
            }

            //for (int i = 0; i < bulkChunksCount; i++)
            //{
                
            //        bw.Write(BitConverter.GetBytes(0xFF), 0, 1);
            //        Array.Copy(Original_File, (i * 8), chunkBuffer, 0, 8);
            //        bw.Write(chunkBuffer, 0, 8);

            //    if (extraChunkSize != 0)
            //    {
            //        Array.Resize(ref chunkBuffer, extraChunkSize);
            //        Array.Copy(Original_File, (0x08 * bulkChunksCount), chunkBuffer, 0, chunkBuffer.Length);

            //        Byte codeByte = 0x00;
            //        switch (extraChunkSize)
            //        {
            //            case 1:
            //                codeByte = 0x80;
            //                break;
            //            case 2:
            //                codeByte = 0xC0;
            //                break;
            //            case 3:
            //                codeByte = 0xE0;
            //                break;
            //            case 4:
            //                codeByte = 0xF0;
            //                break;
            //            case 5:
            //                codeByte = 0xF8;
            //                break;
            //            case 6:
            //                codeByte = 0xFC;
            //                break;
            //            case 7:
            //                codeByte = 0xFE;
            //                break;
            //            default:
            //                break;
            //        }

            //        bw.Write(BitConverter.GetBytes(codeByte), 0, 1);
            //        bw.Write(chunkBuffer, 0, chunkBuffer.Length);
            //    }




            //}

            
            bw.Close();
            fs.Close();
            

            Console.ReadKey();
        }

        public static char Use_Yaz0_Encode() {
            Console.WriteLine("全ての項目にYaz0圧縮をしますか？");
            Console.WriteLine("※解凍時は自動でYaz0の処理を実行します。");
            Console.WriteLine("y Yaz0圧縮を使用　n Yaz0圧縮を使用しない");

            var yesnoChars = Console.ReadLine().ToCharArray();
            return yesnoChars[0];
        }
    }
}
