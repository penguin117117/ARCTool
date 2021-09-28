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
        public void Decord(string filepath)
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
                        s_debug.Add(bita.ToString("X2"));
                        s_debug.Add(bitb.ToString("X2"));
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
                            s_debug.Add(ByteC.ToString("X2"));
                        }
                        else
                        {
                            writebyteNum = a_top4 + 2;
                        }

                        //s_debug.Add("\n");
                        for (int i = 0; i < writebyteNum; i++)
                        {
                            var sameindex = Yaz0DecDeta.Count - pos_same_String;
                            //Console.WriteLine("yaz0decdata "+Yaz0DecDeta.Count.ToString("X")+"  sameindex"+sameindex.ToString("X"));
                            //Console.WriteLine("DATA______" + Yaz0DecDeta[sameindex].ToString("X2"));
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

            FileStream fs2 = new(savefullpath, FileMode.Create);
            BinaryWriter bwYaz0 = new(fs2);
            bwYaz0.Write(Yaz0DecDeta.ToArray());


            br.Close();
            fs.Close();
            bwYaz0.Close();
            fs2.Close();

            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetEntryAssembly();
            string path = myAssembly.Location;
            path = Path.GetDirectoryName(path);
            foreach(var str in s_debug)
            File.WriteAllText(path + "\\DebugByte.txt", str);


        }


        private static List<bool> BitArrayReverser(BitArray bitArray, List<bool> bitlist)
        {
            bitlist = new List<bool>();
            foreach (var tes in bitArray) bitlist.Add((bool)tes);
            bitlist.Reverse();
            return bitlist;
        }

        //public void Encode(string filepath)
        //{
        //    var Yaz0_Dir = filepath.Substring(0, filepath.LastIndexOf(@"\"));
        //    var Yaz0_file = Path.GetFileNameWithoutExtension(filepath);
        //    var Yaz0_FullPath = Path.Combine(Yaz0_Dir, Yaz0_file + ".arc");

        //    FileStream fsRARC = new(filepath, FileMode.Open);
        //    BinaryReader brRARC = new(fsRARC);

        //    FileStream fsYaz0 = new FileStream(Yaz0_FullPath, FileMode.Create);
        //    BinaryWriter bwYaz0 = new BinaryWriter(fsYaz0);

        //    if (File.Exists(filepath) == false) return;
        //    var OriginalData = File.ReadAllBytes(filepath);
        //    OriginalDataSize = OriginalData.Length;

        //    //Yaz0ヘッダー
        //    CS.String_Writer(bwYaz0, "Yaz0");
        //    CS.String_Writer_Int(bwYaz0, OriginalDataSize);
        //    CS.Null_Writer_Int32(bwYaz0, 2);

        //    //group変数
        //    byte group_head = 0;
        //    int group_head_length = 0;

        //    //圧縮処理
        //    while (fsRARC.Length < OriginalDataSize)
        //    {
        //        if (group_head_length == 0)
        //        {
        //            group_head =
        //            }

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

        public static void WriterSwapEndianness(BinaryWriter bw, Int32 value, int sindex, int eindex)
        {
            var OldBytes = BitConverter.GetBytes(value);
            var ReverseBytes = ArrayReverse(OldBytes);
            var NewInt32 = BitConverter.ToInt32(ReverseBytes, 0);
            var NewBytes = BitConverter.GetBytes(NewInt32);
            bw.Write(NewBytes, sindex, eindex);
        }

        public void Encode(string encodeFilePath)
        {
            var Yaz0_FullPath = Path.ChangeExtension(encodeFilePath, ".arc");

            Console.WriteLine(Yaz0_FullPath);
            Console.ReadKey();

            var Original_File           = File.ReadAllBytes(encodeFilePath);
            var OriginalFileLength      = Original_File.Length;
            var DictionaryList          = new List<byte>();

            FileStream fsRARC   = new(encodeFilePath, FileMode.Open);
            BinaryReader brRARC = new(fsRARC);
            MemoryStream msYaz0 = new();
            BinaryWriter bwYaz0 = new(msYaz0);

            //ヘッダー情報の書き込み
            CS.String_Writer(bwYaz0,Magic);
            CS.String_Writer_Int(bwYaz0,OriginalFileLength);
            CS.Null_Writer_Int32(bwYaz0,2);

            //時間を計測する
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            //検索したいデータのサイズを決定する。
            for (var ReadingPosition = fsRARC.Position; ReadingPosition < OriginalFileLength; /* なし */)
            {
                if (fsRARC.Position >= OriginalFileLength) break ;

                //チャンク先頭の読み込みフラグ
                byte NormalRead = 0b_0000_0000;             

                //読み込みフラグのダミーデータを入れる
                var Pos_DummyData = msYaz0.Position;
                bwYaz0.Write((byte)NormalRead);


                List <List<byte>> ChunkList = new();
                byte ReadingByte;

                for (var ChunkIndex = 0; ChunkIndex < 8; ChunkIndex++)
                {
                    if (fsRARC.Position >= OriginalFileLength) break;
                    List<byte> ChunkInsideBytes = new();

                    Console.WriteLine("");
                    Console.WriteLine("チャンクTopFS " + ReadingPosition.ToString("X"));
                    Console.WriteLine("チャンク " + ChunkIndex.ToString());
                    Console.WriteLine("fsRARC Pos " + fsRARC.Position.ToString("X"));

                    //3バイト目までは必ず書き込む
                    if (fsRARC.Position == 0)
                    {
                        for (var FirstIndex = 0; FirstIndex < 3; FirstIndex++) 
                        {
                            ChunkIndex = FirstIndex;
                            NormalRead += IsNormalRead[ChunkIndex];
                            ReadingByte = brRARC.ReadByte();
                            ChunkInsideBytes.Add(ReadingByte);
                            DictionaryList.Add(ReadingByte);
                            bwYaz0.Write(ReadingByte);
                        }
                        Console.WriteLine("1バイト目");
                        //Console.ReadKey();
                        continue;
                    }

                    
                    

                    //ファイル末尾まで残り3バイト以下の処理
                    if (OriginalFileLength < fsRARC.Position + Read_3Byte) 
                    {
                        //符号反転して残りのビットフラグをすべて通常読み込みにする。
                        byte ReversNormalRead = (byte)~NormalRead;
                        NormalRead += ReversNormalRead;
                        
                        //ファイル末尾までデータをそのまま読み込む
                        for (var t = fsRARC.Position; t < OriginalFileLength; t++) 
                        {
                            bwYaz0.Write(brRARC.ReadByte());
                        }
                        Console.WriteLine("End");
                        sw.Stop();
                        Console.WriteLine(sw.Elapsed.ToString());
                        Console.ReadKey();
                        break;
                    }

                    var pos_Byte2nd = fsRARC.Position + 1;

                    //★通常読み込みの処理開始
                    byte Byte1st = brRARC.ReadByte();
                    byte Byte2nd = brRARC.ReadByte();
                    byte Byte3rd = brRARC.ReadByte();

                    ChunkInsideBytes.Add(Byte1st);
                    ChunkInsideBytes.Add(Byte2nd);
                    ChunkInsideBytes.Add(Byte3rd);

                    DictionaryList.Add(Byte1st);
                    DictionaryList.Add(Byte2nd);
                    DictionaryList.Add(Byte3rd);

                    //fsRARC.Length現在地から、-0x000～-0xFFFまでの範囲を辞書ファイルとする。0x1003 = 0x003
                    UInt16 DictionaryRange = (UInt16)(fsRARC.Position - 1);
                    if (Dictionary_MaxRange < fsRARC.Position)
                    {
                        DictionaryRange = Dictionary_MaxRange;
                    }

                    //var OriginalDictionaryData = new byte[DictionaryRange];

                    //for (var inside = 0; inside < DictionaryRange; inside++)
                    //{
                    //    var pos = fsRARC.Position - DictionaryRange;
                    //    OriginalDictionaryData[inside] = Original_File[pos + inside];
                    //}

                    //Console.WriteLine(OriginalDictionaryData.Length.ToString("X"));


                    //Console.WriteLine(Original_File.Length.ToString("X"));
                    var ResizedDictionaryData = DictionaryList.TakeLast(DictionaryRange).ToArray()/*OriginalDictionaryData*//*Original_File.Take((UInt16)fsRARC.Position - DictionaryRange).ToArray()*/;

                    UInt16 MatchByteCount;
                    UInt16 MatchByteIndex;
                    bool OldFound = DictionaryFinder_3Byte(ChunkInsideBytes,ResizedDictionaryData,out MatchByteCount);
                    

                    //3バイトが見つからなかった場合(通常の処理フラグは1)
                    if (OldFound == false /*|| (pos_Byte2nd % 0x0F == 0.0000F)*/) 
                    {
                        Console.WriteLine("NormalReading");
                        fsRARC.Seek(pos_Byte2nd, SeekOrigin.Begin);
                        ChunkInsideBytes.RemoveAt(ChunkInsideBytes.Count - 1);
                        ChunkInsideBytes.RemoveAt(ChunkInsideBytes.Count - 1);
                        DictionaryList.RemoveAt(DictionaryList.Count - 1);
                        DictionaryList.RemoveAt(DictionaryList.Count - 1);
                        NormalRead += IsNormalRead[ChunkIndex];
                        bwYaz0.Write(Byte1st);
                        
                        continue;
                    }
                    //File.WriteAllBytes(Yaz0_FullPath, msYaz0.ToArray());
                    //ここまでチェック済み

                    if (fsRARC.Position + Dictionary_ReadLength >= OriginalFileLength) break;

                    for (var tmp = 0; tmp < (Dictionary_ReadLength + 1); tmp++) 
                    {
                        byte byteTmp = brRARC.ReadByte();
                        ChunkInsideBytes.Add(byteTmp);
                        DictionaryList.Add(byteTmp);
                    }

                    if (fsRARC.Position >= OriginalFileLength) break;

                    OldFound = DictionaryFinder_FByte(ChunkInsideBytes, ResizedDictionaryData, out MatchByteCount,out MatchByteIndex);
                    
                    //Fバイトが見つからなかった場合(処理フラグは0)
                    if (OldFound == false)
                    {
                        Console.WriteLine(((UInt16)(MatchByteIndex + MatchByteCount)).ToString("X2"));
                        Console.WriteLine(ChunkInsideBytes.Count.ToString("X2"));
                        
                        var TestIndex01 = 1 + (MatchByteIndex + MatchByteCount);
                        var TestIndex02 = (0x12) - (MatchByteIndex + MatchByteCount);

                        Console.WriteLine("TestIndexXX");
                        Console.WriteLine(TestIndex01.ToString("X"));
                        Console.WriteLine(TestIndex02.ToString("X"));

                        //Console.WriteLine("dc-mbi-mbc "+((1+(MatchByteIndex + MatchByteCount))).ToString("X"));
                        //Console.WriteLine((DictionaryList.Count - (0x12) + (MatchByteIndex - MatchByteCount)).ToString("X"));
                        //ChunkInsideBytes.RemoveRange(ChunkInsideBytes.Count - (MatchByteIndex - MatchByteCount), ChunkInsideBytes.Count - (ChunkInsideBytes.Count - (MatchByteIndex - MatchByteCount)));

                        DictionaryList.RemoveRange(TestIndex01,DictionaryList.Count - TestIndex01 - 1);
                        //DictionaryList.RemoveRange(DictionaryList.Count - (0x12) + (MatchByteIndex - MatchByteCount), DictionaryList.Count - (DictionaryList.Count - (0x12) + (MatchByteIndex - MatchByteCount)));
                        ////Console.ReadKey();
                        Console.WriteLine("pos set");
                        Console.WriteLine(fsRARC.Position.ToString("X"));
                        Console.WriteLine(DictionaryList.Count.ToString("X"));
                        //long pos_Old = TestIndex01;
                        long pos_Old = fsRARC.Position - (0x12) + (MatchByteIndex - MatchByteCount);
                        Console.WriteLine("pos " + pos_Old.ToString("X"));
                        fsRARC.Seek(pos_Old, SeekOrigin.Begin);
                        Console.WriteLine("OKKKKKKKKKKKKKKKKK");
                        //Console.ReadKey();
                        //NormalRead += IsNormalRead[ChunkIndex];
                        bwYaz0.Write((byte)(MatchByteCount<<4));
                        bwYaz0.Write((byte)(MatchByteIndex));
                        Console.WriteLine((MatchByteCount).ToString("X"));
                        Console.WriteLine((MatchByteIndex).ToString("X"));
                        //Console.ReadKey();
                        File.WriteAllBytes(Yaz0_FullPath, msYaz0.ToArray());
                        continue;
                    }
                    Console.WriteLine("おーばー");
                    Console.ReadKey();
                    continue;
                    //continue;
                    //Console.ReadKey();
                    //File.WriteAllBytes(Yaz0_FullPath, msYaz0.ToArray());
                    //Console.WriteLine(fsRARC.Position);
                    //brRARC.Close();
                    //fsRARC.Close();
                    //bwYaz0.Close();
                    //msYaz0.Close();
                    //return;

                    //var FindFromDictionary = Array.LastIndexOf(ResizedDictionaryData, ChunkInsideBytes.ToArray());

                    //Console.WriteLine(FindFromDictionary);
                    //if (FindFromDictionary == -1)
                    //{
                    //    //Console.WriteLine("3ByteNotFound");
                    //    fsRARC.Seek(pos_Byte2nd, SeekOrigin.Begin);
                    //    ChunkInsideBytes.RemoveAt(ChunkInsideBytes.Count()-1);
                    //    NormalRead += IsNormalRead[ChunkIndex];
                    //    bwYaz0.Write(Byte1st);
                    //    continue;
                    //}

                    //var FindFromDictionary = 0;

                    //Console.WriteLine("3ByteFind");
                    //0xfループByte1st_Top4Byteのサイズ
                    //byte NewResult1stByte , NewResult2ndByte;
                    //long pos_OldRARC = fsRARC.Position;
                    //for (UInt16 l = 0; l < Dictionary_ReadLength; l++) 
                    //{
                    //    var TempByte = brRARC.ReadByte();
                    //    ChunkInsideBytes.Add(TempByte);
                    //    ResizedDictionaryData = Original_File.TakeLast(DictionaryRange).ToArray();
                    //    FindFromDictionary = Array.LastIndexOf(ResizedDictionaryData, ChunkInsideBytes);
                    //    //Console.WriteLine();
                    //    if (FindFromDictionary == -1) 
                    //    {
                    //        fsRARC.Seek(pos_OldRARC,SeekOrigin.Begin);
                    //        ChunkInsideBytes.RemoveAt(ChunkInsideBytes.Count()-1);
                    //        var FindByte1stTop4 = (byte)((UInt16)l<<4);
                    //        var FindByte1stLast4 = (byte)((UInt16)ChunkInsideBytes.Count()>>4);
                    //        var FindByte2nd = (byte)((UInt16)ChunkInsideBytes.Count() << 4);
                    //        bwYaz0.Write((byte) FindByte1stTop4 + FindByte1stLast4);
                    //        bwYaz0.Write((byte) FindByte2nd);
                    //        Console.WriteLine(((byte)FindByte1stTop4 + FindByte1stLast4).ToString("X"));
                    //        Console.WriteLine(((byte)FindByte2nd).ToString("X"));
                    //        break;
                    //    }
                    //    pos_OldRARC = fsRARC.Position;

                    //}

                    //if (FindFromDictionary == -1) 
                    //{
                    //    //NormalRead += IsNormalRead[ChunkIndex];
                    //    break;
                    //}

                    //Console.WriteLine("Find_FD  " + FindFromDictionary.ToString("X"));
                    //File.WriteAllBytes(Yaz0_FullPath, msYaz0.ToArray());
                    //Console.WriteLine(fsRARC.Position);
                    brRARC.Close();
                    fsRARC.Close();
                    bwYaz0.Close();
                    msYaz0.Close();
                    
                    Console.ReadKey();
                    return;

                    //while (FindFromDictionary == -1) 
                    //{
                    //    if (OriginalFileLength < fsRARC.Position) break;

                        

                    //    //var pos_DictionaryBegin = (fsRARC.Position - 1) - DictionaryRange;

                    //    //Array.Copy(
                    //    //    Original_File,
                    //    //    pos_DictionaryBegin,
                    //    //    OriginalDictionaryData,
                    //    //    0,
                    //    //    pos_DictionaryEnd
                    //    //    );

                    //    ResizedDictionaryData = Original_File.TakeLast(DictionaryRange).ToArray();

                    //    FindFromDictionary = Array.LastIndexOf(ResizedDictionaryData,ChunkInsideBytes);


                    //    if (FindFromDictionary == -1) {

                    //        NormalRead += IsNormalRead[ChunkIndex];
                    //        break;
                    //    }

                    //    //ResizedDictionaryData.ToList().IndexOf(ChunkList);

                    //    ReadingByte = brRARC.ReadByte();
                    //    ChunkInsideBytes.Add(ReadingByte);
                    //    bwYaz0.Write(ReadingByte);
                    //}


                    
                    File.WriteAllBytes(Yaz0_FullPath, msYaz0.ToArray());
                    Console.WriteLine(fsRARC.Position);
                    Console.ReadKey();
                    brRARC.Close();
                    fsRARC.Close();
                    bwYaz0.Close();
                    msYaz0.Close();
                    return;
                }

                msYaz0.Seek(Pos_DummyData, SeekOrigin.Begin);
                bwYaz0.Write(NormalRead);
                msYaz0.Seek(msYaz0.Length,SeekOrigin.Begin);
                continue;
                //var Byte2nd = brRARC.ReadByte();

                ////ChunkList.Add(Byte1st);
                //ChunkList.Add(Byte2nd);
                //ファイル末尾のための処理
                long SearchDataSize = 0x0F;
                long DistanceFromEnd = OriginalFileLength - fsRARC.Position;
                if (SearchDataSize > DistanceFromEnd)
                {
                    SearchDataSize = DistanceFromEnd;
                }
                //else if (SearchDataSize > fsRARC.Position)
                //{
                //    SearchDataSize = fsRARC.Position;
                //}
                //else 
                //{
                //    SearchDataSize = 0xF;
                //}
                //byte[] BinaryDataToLookFor = new byte[SearchDataSize];

                var DictionaryData = new List<byte>();
                int FindDistance;
                int MatchCount;
                for (var j = 0; j < SearchDataSize; j++) 
                {
                    //3バイト目以降
                    var ByteX = brRARC.ReadByte();
                    //ChunkList.Add(ByteX);
                    long Pos_Now = fsRARC.Position;

                    var Find = Array.LastIndexOf(Original_File,ChunkList,(int)ReadingPosition,j+ (int)Read_3Byte);
                    if (Find == -1) 
                    {
                        ChunkList.RemoveAt((int)Pos_Now - 1);
                        fsRARC.Seek(fsRARC.Position - 1, SeekOrigin.Begin);
                        break;
                    }
                    FindDistance = Find;
                    MatchCount = ChunkList.Count();
                    if (FindDistance > 0xF) 
                    {
                        FindDistance = 0; 
                    }
                }






                ////検索したいデータを作成
                //UInt16 FindDictonaryIndex = 0xFFFF;
                //for (var j = 0; j < SearchDataSize; j++)
                //{
                //    BinaryDataToLookFor[j] = brRARC.ReadByte();
                //    //BinaryDataToLookFor[j] = Original_File[i+j];
                //    Array.Copy(Original_File, fsRARC.Position - DictionaryReadSize, OriginalDictionaryData, 0, DictionaryReadSize);
                //    UInt16 TempFindDictonaryIndex = (UInt16)Array.LastIndexOf(OriginalDictionaryData, BinaryDataToLookFor);
                //    if (TempFindDictonaryIndex == 0xFFFF) continue;
                //    FindDictonaryIndex = TempFindDictonaryIndex;
                //}

                //if (FindDictonaryIndex == 0xFFFF) 
                //{
                //    for(var k = 0; k < 8; k++) 
                //    {
                //        chunkDatas[k].IsRead = true;
                //        chunkDatas[k].ByteList = new List<byte>();
                //    }

                //}

                //Array.Copy(Original_File, fsRARC.Position - DictionaryReadSize, OriginalDictionaryData, 0, DictionaryReadSize);
                //FindDictonaryIndex = (UInt16)Array.LastIndexOf(OriginalDictionaryData, BinaryDataToLookFor);
                if (fsRARC.Position >= OriginalFileLength) break;


            }

            File.WriteAllBytes(Yaz0_FullPath, msYaz0.ToArray());
            Console.WriteLine(fsRARC.Position);
            brRARC.Close();
            fsRARC.Close();
            bwYaz0.Close();
            msYaz0.Close();
            //fsRARC.Close();
            //brRARC.Close();
        }



        public static bool DictionaryFinder_3Byte(List<byte> ChunkInsideBytes,byte[] ResizedDictionaryData,out UInt16 findLastIndex) 
        {
            //int tes = 0;
            
            const byte OffByOne = 1;
            bool OldFound = false;
            findLastIndex = 0xFFFF;
            //bool[] Founds = new bool[Read_3Byte];
            for (var Index = ResizedDictionaryData.Length - (Read_3Byte + OffByOne); Index >= 0; Index--)
            {
                //for (var Index2 = 0; Index2 < Read_3Byte; Index2++)
                //{
                //    Founds[Index2] = ResizedDictionaryData[Index + Index2] == ChunkInsideBytes[Index2];
                //}

                //var IsFound = Founds.Where((value, index) => value == false).ToArray();
                //if (IsFound.Length == 0) OldFound = true;

                //高速
                //Console.WriteLine("");
                //Console.Write((ResizedDictionaryData[Index]).ToString("X"));
                //Console.Write(" == " + ChunkInsideBytes[0].ToString("X"));
                //Console.WriteLine("");
                //Console.Write((ResizedDictionaryData[(Index + 1)]).ToString("X"));
                //Console.Write(" == " + ChunkInsideBytes[1].ToString("X"));
                //Console.WriteLine("");
                //Console.Write((ResizedDictionaryData[(Index + 2)]).ToString("X"));
                //Console.Write(" == " + ChunkInsideBytes[2].ToString("X"));
                //Console.WriteLine("");
                //Console.WriteLine("");

                var IsFind1 = ResizedDictionaryData[Index] == ChunkInsideBytes[0];
                var isFind2 = ResizedDictionaryData[Index + 1] == ChunkInsideBytes[1];
                var isFind3 = ResizedDictionaryData[Index + 2] == ChunkInsideBytes[2];
                bool IsFound = (IsFind1 && isFind2 && isFind3);
                if (IsFound) 
                { 
                    OldFound = true; 
                    findLastIndex = (UInt16)Index; 
                    break; 
                } 

            }
            

            return OldFound;
        }

        public static bool DictionaryFinder_FByte(List<byte> ChunkInsideBytes, byte[] ResizedDictionaryData, out UInt16 matchByteCount , out UInt16 matchBtyeIndex) 
        {
            
            const byte OffByOne = 1;
            bool OldFound = false;
            matchByteCount = (UInt16)0x000F;
            matchBtyeIndex = (UInt16)0x0FFF;
            
            for (var Index0 = 0; Index0 < (int)(Dictionary_ReadLength + (Read_3Byte + OffByOne)); Index0++)
            {
                bool[] Founds = new bool[Index0 + OffByOne];
                for (var Index = 0; Index < ResizedDictionaryData.Length - (Index0 + Read_3Byte); Index++)
                {
                    for (var Index2 = 0; Index2 < Index0; Index2++)
                    {
                        Founds[Index2] = ResizedDictionaryData[Index + Index2] == ChunkInsideBytes[Index2];
                    }
                    //falseが含まれる場合
                    var IsFound = Founds.Where((value) => value == false).ToArray();
                    Console.WriteLine(IsFound.Length == 0);
                    if (IsFound.Length == 0) continue;

                    Console.WriteLine(Index0.ToString("X"));
                    Console.WriteLine("辞書" + ResizedDictionaryData.Length.ToString("X"));
                    Console.WriteLine(Index.ToString("X"));

                    OldFound = false;
                    matchByteCount = (UInt16)(Index0 + 1);
                    matchBtyeIndex = (UInt16)(/*ResizedDictionaryData.Length -*/ (Index + 3));
                    Console.WriteLine("ok 4");
                    //Console.ReadKey();
                    return OldFound;

                }
                //for (var Index = ResizedDictionaryData.Length - (Index0 + Read_3Byte + OffByOne); Index >= 0; Index--)
                //{
                //    for (var Index2 = 0; Index2 < Index0; Index2++)
                //    {
                //        Founds[Index2] = ResizedDictionaryData[Index + Index2] == ChunkInsideBytes[Index2];
                //    }
                //    //falseが含まれる場合
                //    var IsFound = Founds.Where((value) => value == false).ToArray();
                //    Console.WriteLine(IsFound.Length == 0);
                //    if (IsFound.Length == 0) continue;

                //    Console.WriteLine(Index0.ToString("X"));
                //    Console.WriteLine("辞書" + ResizedDictionaryData.Length.ToString("X"));
                //    Console.WriteLine(Index.ToString("X"));

                //    OldFound = false;
                //    matchByteCount = (UInt16)(Index0 + 1);
                //    matchBtyeIndex = (UInt16)(ResizedDictionaryData.Length - (Index + 1));
                //    Console.WriteLine("ok 4");
                //    //Console.ReadKey();
                //    return OldFound;

                //}

            }
            return OldFound;
        }

        //public void Encode(string filepath)
        //{
        //    var Yaz0_Dir = filepath.Substring(0, filepath.LastIndexOf(@"\"));
        //    var Yaz0_file = Path.GetFileNameWithoutExtension(filepath);
        //    var Yaz0_FullPath = Path.Combine(Yaz0_Dir, Yaz0_file + ".arc");

        //    FileStream fsRARC = new(filepath,FileMode.Open);
        //    var rarcLen = (Int32)fsRARC.Length;
        //    fsRARC.Close();

        //    byte[] Original_File = File.ReadAllBytes(filepath);



        //    //Console.WriteLine(Original_File.Length.ToString("X"));
        //    Int32 Original_File_Chunks = Original_File.Length / 0x08;//0x44
        //    Int32 extraChunkSize  = Original_File.Length % 0x08;//0x00
        //    byte[] chunkBuffer = new byte[0x08];

        //    FileStream fs = new(Yaz0_FullPath, FileMode.Create, FileAccess.Write);
        //    BinaryWriter bw = new(fs);

        //    bw.Write(Encoding.UTF8.GetBytes("Yaz0"), 0, 4);
        //    WriterSwapEndianness(bw, rarcLen, 0, 4);
        //    WriterSwapEndianness(bw, 0x00000000, 0, 4); //Can be 0x00002000 sometimes, check the original Yaz0 file at offset 0x08
        //    WriterSwapEndianness(bw, 0x00000000, 0, 4);



        //    //ライブラリデータの作成
        //    //この文字列を参照して圧縮処理を行う
        //    List<byte> String_Libraries = new List<byte>(); ;
        //    var ReadCount = 0;

        //    //オリジナルファイルのチャンク数分のループ
        //    for (var WriteCount = 0; WriteCount < Original_File_Chunks; WriteCount++) {

        //        List<byte> GroupByte = new()
        //        {
        //            Original_File[WriteCount]
        //        };
        //        var pos_ReadFlag = fs.Position;
        //        //bw.Write((byte)0xFF);

        //        //ArraySegment<byte> AS_Original_File = new(Original_File,(int)pos_ReadFlag,8);
        //        //foreach (var ASOF in AS_Original_File) { 

        //        //}




        //        //1バイト目の処理
        //        if (WriteCount == 0) {
        //            String_Libraries.Add(Original_File[WriteCount]);
        //            bw.Write(String_Libraries[WriteCount]);
        //            var GroupByteCount = 0;
        //            while (true) {
        //                GroupByte.Add(String_Libraries[WriteCount + GroupByteCount]);
        //                GroupByteCount++;


        //            }
        //            continue;
        //        }



        //        while (true) {
        //            var Find_Index00 = String_Libraries.LastIndexOf(GroupByte[0]);
        //            var Find_Index01 = String_Libraries.LastIndexOf(GroupByte[1]);

        //            if ((Find_Index00 == -1) && (Find_Index01 == -1)){
        //                bw.Write(Original_File[WriteCount]);
        //            }
        //            else { 

        //            }
        //        }



        //    }

        //    //for (int i = 0; i < bulkChunksCount; i++)
        //    //{

        //    //        bw.Write(BitConverter.GetBytes(0xFF), 0, 1);
        //    //        Array.Copy(Original_File, (i * 8), chunkBuffer, 0, 8);
        //    //        bw.Write(chunkBuffer, 0, 8);

        //    //    if (extraChunkSize != 0)
        //    //    {
        //    //        Array.Resize(ref chunkBuffer, extraChunkSize);
        //    //        Array.Copy(Original_File, (0x08 * bulkChunksCount), chunkBuffer, 0, chunkBuffer.Length);

        //    //        Byte codeByte = 0x00;
        //    //        switch (extraChunkSize)
        //    //        {
        //    //            case 1:
        //    //                codeByte = 0x80;
        //    //                break;
        //    //            case 2:
        //    //                codeByte = 0xC0;
        //    //                break;
        //    //            case 3:
        //    //                codeByte = 0xE0;
        //    //                break;
        //    //            case 4:
        //    //                codeByte = 0xF0;
        //    //                break;
        //    //            case 5:
        //    //                codeByte = 0xF8;
        //    //                break;
        //    //            case 6:
        //    //                codeByte = 0xFC;
        //    //                break;
        //    //            case 7:
        //    //                codeByte = 0xFE;
        //    //                break;
        //    //            default:
        //    //                break;
        //    //        }

        //    //        bw.Write(BitConverter.GetBytes(codeByte), 0, 1);
        //    //        bw.Write(chunkBuffer, 0, chunkBuffer.Length);
        //    //    }




        //    //}


        //    bw.Close();
        //    fs.Close();


        //    Console.ReadKey();
        //}

        public static char Use_Yaz0_Encode()
        {
            Console.WriteLine("全ての項目にYaz0圧縮をしますか？");
            Console.WriteLine("※解凍時は自動でYaz0の処理を実行します。");
            Console.WriteLine("y Yaz0圧縮を使用　n Yaz0圧縮を使用しない");

            var yesnoChars = Console.ReadLine().ToCharArray();
            return yesnoChars[0];
        }
    }
}
