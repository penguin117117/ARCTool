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
        private readonly bool[] bit7 = {true,true,true,true,true,true,true};
        public void Decord(string filepath) {
            var savedirectory = filepath.Substring(0,filepath.LastIndexOf(@"\"));
            var savefilename =  Path.GetFileNameWithoutExtension(filepath) + ".rarc";
            var savefullpath = Path.Combine(savedirectory,savefilename);
            Console.WriteLine(savefullpath);

            

            FileStream fs = new FileStream(filepath ,FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            

            Magic = CS.Byte2Char(br);
            OriginalDataSize = CS.Byte2Int(br);
            CS.Byte2Int(br);
            CS.Byte2Int(br);

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
                            Console.WriteLine("DATA______" + Yaz0DecDeta[sameindex].ToString("X2"));
                            Yaz0DecDeta.Add(Yaz0DecDeta[sameindex]);
                            
                        }
                    }
                    if (Yaz0DecDeta.Count() == OriginalDataSize) break;
                }

                //if (ms.Length < OriginalDataSize) {
                //    Console.WriteLine("yazend");    
                //    break; 
                //}

                Console.WriteLine("anykey＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿");
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

        
    }
}
