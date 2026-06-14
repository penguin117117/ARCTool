using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using CS = ARCTool.FileSys.Calculation_System;
using System.Buffers.Binary;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace ARCTool.FileSys
{
    public class Yaz0
    {
        const byte Read_3Byte               = 0x03;         //最初に読み込む3バイト
        const UInt16 Dictionary_MaxRange    = 0x0FFF;       //辞書の最大サイズ
        const UInt16 Dictionary_ReadLength  = 0xF000 >> 12; //辞書サイズ

        private static string   s_magic ;
        private static int      s_unknown1, s_unknown2;
        public string Magic
        {
            set => s_magic = value;
            get
            {
                s_magic = "Yaz0";
                return s_magic;
            }
        }
        public int OriginalDataSize { get; set; }
        public int Unknown1
        {
            set
            {
                if (value != 0x00000000)
                {
                    Console.WriteLine("Yaz0のUnknown1プロパティで例外が発生しました");
                    Console.WriteLine("下記のエラー内容を最下段のURLに報告してください。");
                    Console.WriteLine("エラー：Unknown1「"+value.ToString("X8")+"」");
                    Console.WriteLine("https://github.com/penguin117117/ARCTool/issues");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                s_unknown1 = value;
            }
            get
            {
                s_unknown1 = 0x00000000;
                return s_unknown1;
            }

        }
        public int Unknown2
        {
            set
            {
                if (value != 0x00000000)
                {
                    Console.WriteLine("Yaz0のUnknown1プロパティで例外が発生しました");
                    Console.WriteLine("下記のエラー内容を最下段のURLに報告してください。");
                    Console.WriteLine("エラー：Unknown1「" + value.ToString("X8") + "」");
                    Console.WriteLine("https://github.com/penguin117117/ARCTool/issues");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                s_unknown2 = value;
            }
            get
            {
                s_unknown2 = 0x00000000;
                return s_unknown2;
            }
        }

        private readonly Byte[] IsNormalRead = {
            0b_1000_0000 ,
            0b_0100_0000 ,
            0b_0010_0000 ,
            0b_0001_0000 ,
            0b_0000_1000 ,
            0b_0000_0100 ,
            0b_0000_0010 ,
            0b_0000_0001
        };
        //private readonly bool[] IsNormalRead = { true, true, true, true, true, true, true };

        public struct ChunkData {
            public bool IsNormalRead;
            public List<byte> ByteList;
            public ChunkData(bool isRead , List<byte> byteList) {
                IsNormalRead = isRead;
                ByteList = new List<byte>(byteList);

            }
        }

        private ChunkData[] ChunkDatas = new ChunkData[8];

        private static List<string> s_debug = new List<string>();
        public void Decode(string filepath)
        {

            List<bool> bitlist = new();
            List<byte> Yaz0DecDeta = new();
            //var DecFile = 0;
            var savedirectory = filepath.Substring(0, filepath.LastIndexOf(@"\"));
            var savefilename = Path.GetFileNameWithoutExtension(filepath) + ".rarc";
            var savefullpath = Path.Combine(savedirectory, savefilename);

            FileStream fs = new(filepath, FileMode.Open);
            BinaryReader br = new(fs);

            //Yaz0ヘッダー
            Magic = CS.Byte2Char(br);
            OriginalDataSize = CS.Byte2Int(br);
            Unknown1 = CS.Byte2Int(br);
            Unknown2 = CS.Byte2Int(br);

            //解凍処理
            while (Yaz0DecDeta.Count < OriginalDataSize)
            {

                byte StrReadType = br.ReadByte();
                byte[] bits = new byte[] { StrReadType };


                //ビット反転
                BitArray bitArray = new(bits);
                bitlist = BitArrayReverser(bitArray, bitlist);



                foreach (var bititem in bitlist)
                {

                    if (bititem == true)
                    {

                        //ビットが1の場合1バイトをそのまま読み込む
                        var writedata = br.ReadByte();
                        Yaz0DecDeta.Add(writedata);
                    }
                    else
                    {
                        //ビットが0の場合の処理
                        var bita = br.ReadByte();
                        var bitb = br.ReadByte();
                        //s_debug.Add(bita.ToString("X2"));
                        //s_debug.Add(bitb.ToString("X2"));
                        byte a_top4 = (byte)(bita >> 4);
                        byte a_last4 = (byte)(bita << 4);
                        int pos_same_String = (a_last4 << 4 | bitb) + 1;
                        int writebyteNum;

                        //a_top4が0の場合と0以外の場合で読み込み方法が変わる。
                        if (a_top4 == 0)
                        {
                            //a_top4のサイズが0xFが最大なのでそれよりも大きい場合の処理
                            byte ByteC = br.ReadByte();
                            writebyteNum = ByteC + 0x12;
                            //s_debug.Add(ByteC.ToString("X2"));
                        }
                        else
                        {
                            writebyteNum = a_top4 + 2;
                        }

                        //s_debug.Add("\n");
                        for (int i = 0; i < writebyteNum; i++)
                        {
                            var sameindex = Yaz0DecDeta.Count - pos_same_String;
                            Yaz0DecDeta.Add(Yaz0DecDeta[sameindex]);
                            s_debug.Add(Yaz0DecDeta[sameindex].ToString("X2") + " ");

                        }
                        //s_debug.Add("\n");
                    }
                    if (Yaz0DecDeta.Count == OriginalDataSize) break;
                }
            }

            FileStream fs2 = new(savefullpath, FileMode.Create);
            BinaryWriter bwYaz0 = new(fs2);
            bwYaz0.Write(Yaz0DecDeta.ToArray());


            br.Close();
            fs.Close();
            bwYaz0.Close();
            fs2.Close();

#if DEBUG
            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetEntryAssembly();
            string path = myAssembly.Location;
            path = Path.GetDirectoryName(path);
            File.WriteAllText(path + "\\DebugByte.txt", string.Empty);

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var str in s_debug)
            {
                stringBuilder.Append(str);

            }
            File.WriteAllText(path + "\\DebugByte.txt", stringBuilder.ToString());
#endif

        }


        private static List<bool> BitArrayReverser(BitArray bitArray, List<bool> bitlist)
        {
            bitlist = new List<bool>();
            foreach (var tes in bitArray) bitlist.Add((bool)tes);
            bitlist.Reverse();
            return bitlist;
        }

        private void HeaderWriter(BinaryWriter bw, long RawSize)
        {
            CS.String_Writer(bw, Magic);
            var buf = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(buf, (uint)RawSize);
            bw.Write(buf);
            CS.Null_Writer_Int32(bw, 2);
        }

        private static void PaddingWriter(BinaryWriter bw)
        {
            const int Pack_Length = sizeof(Int32) * 4;
            var mod = bw.BaseStream.Length % Pack_Length;
            byte[] padding = new byte[Pack_Length - mod];

            if (padding.Length != 0)
            {
                padding[0] = 0xFF;
                bw.Write(padding);
            }
        }
        public void EncodeOptimizeV2(BinaryWriter bw, BinaryReader br)
        {
            //ヘッダー情報の書き込み
            //Console.WriteLine("write header");
            HeaderWriter(bw, br.BaseStream.Length);

            //Console.WriteLine("write chunk");

            Yaz0Encode encoder = new();
            encoder.Encode(bw, br);

            //Console.WriteLine("write padding");
            PaddingWriter(bw);

            //Console.WriteLine($"Stream End Pos: {br.BaseStream.Position}");
        }
        public void EncodeOptimizeV2(string encodeFilePath, BinaryReader br)
        {
            FileStream fs = new(encodeFilePath, FileMode.Create);
            EncodeOptimizeV2(new BinaryWriter(fs), br);
            fs.Close();
        }

        public void EncodeOptimize(BinaryWriter bw, BinaryReader br)
        {
            //ヘッダー情報の書き込み
            //Console.WriteLine("write header");
            HeaderWriter(bw, br.BaseStream.Length);

            //Console.WriteLine("write chunk");

            Yaz0Chunk chunk = new Yaz0ChunkRawEncode(br);
            bw.Write(chunk.GetValue());

            List<Yaz0Unit> buffer = new();
            //チャンクデータの読み込み方法を設定
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                buffer = Yaz0ChunkEncode.PreprocessUnit(br, buffer);
                chunk = new Yaz0ChunkEncode(ref buffer);
                bw.Write(chunk.GetValue());
            }
            if (buffer.Count > 0)
            {
                chunk = new Yaz0ChunkEncode(ref buffer);
                bw.Write(chunk.GetValue());
            }
            //チャンクデータの読み込み方法を設定_END

            //Console.WriteLine("write padding");
            PaddingWriter(bw);

            //Console.WriteLine($"Stream End Pos: {br.BaseStream.Position}");
        }
        public void EncodeOptimize(string encodeFilePath, BinaryReader br)
            {
            FileStream fs = new(encodeFilePath, FileMode.Create);
            EncodeOptimize(new BinaryWriter(fs), br);
            fs.Close();
        }

        public void Encode(BinaryWriter bw, BinaryReader br)
        {
            //Console.WriteLine("write header");
            HeaderWriter(bw, br.BaseStream.Length);

            //Console.WriteLine("write chunk");
            Yaz0Chunk chunk = new Yaz0ChunkRawEncode(br);
            bw.Write(chunk.GetValue());

            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                chunk = new Yaz0ChunkEncode(br);
                bw.Write(chunk.GetValue());
            }

            //Console.WriteLine("write padding");
            PaddingWriter(bw);

            //Console.WriteLine($"Stream End Pos: {br.BaseStream.Position}");
            }

        public void Encode(string encodeFilePath, BinaryReader br)
        {
            FileStream fs = new(encodeFilePath, FileMode.Create);
            Encode(new BinaryWriter(fs), br);
            fs.Close();
        }

        public enum UseStatus
        {
            UnUse,
            Use,
            UseNew,
        }

        public static UseStatus Use_Yaz0_Encode()
        {
            Console.WriteLine("全ての項目にYaz0圧縮をしますか？");
            Console.WriteLine("※解凍時は自動でYaz0の処理を実行します。");
            Console.WriteLine("下記の4つの中から実行するキーを入力してEnterキーを押してください");
            Console.WriteLine("y：RARCアーカイブ化 + Yaz0圧縮「高速、高圧縮」を使用");
            Console.WriteLine("Y：RARCアーカイブ化 + Yaz0圧縮「安定版」を使用");
            Console.WriteLine("n：RARCアーカイブ化のみ実行");
            Console.WriteLine("e：アプリを終了する");

            char input = Console.ReadLine().ToCharArray()[0];

            /*
             * Note:switch文で入力されたキーがdefaultの場合再帰処理またはgotoが必要だったので
             * while文で入力されたキーが正しいものになるまでループするようにした。
            */
            while (true) 
            {
                switch (input)
                {
                    case 'y':
                    case 'ｙ':
                        return UseStatus.UseNew;
                    case 'Y':
                    case 'Ｙ':
                        return UseStatus.Use;
                    case 'n':
                    case 'ｎ':
                        return UseStatus.UnUse;
                    case 'e':
                        Console.WriteLine("アプリを終了します。");
                        Environment.Exit(0);
                        throw new Exception("AppExit");

                    default:
                        Console.WriteLine($"{input}キーが入力されましたが");
                        Console.WriteLine("入力が正しくありません。");
                        Console.ReadKey();
                        break;

                }
            }

        }
    }
}
