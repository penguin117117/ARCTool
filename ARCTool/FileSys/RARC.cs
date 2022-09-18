using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CS = ARCTool.FileSys.Calculation_System;

namespace ARCTool.FileSys
{
    public class RARC
    {
        public struct DirectoryPathInfo
        {
            public int Index;
            public string Path;
            public DirectoryPathInfo(int index, string path)
            {
                Index = index;
                Path = path;
            }
        }

        public struct NodeItemInfo
        {
            public string Identifier;
            public int StringTopOffset;
            public UInt16 StringHash;
            public short ThisFolderDirectoryCount;
            public int FirstDirectoryIndex;
            public NodeItemInfo(string str, int offset, UInt16 hash, short Dcount, int Dindex)
            {
                Identifier = str;
                StringTopOffset = offset;
                StringHash = hash;
                ThisFolderDirectoryCount = Dcount;
                FirstDirectoryIndex = Dindex;
            }
        }

        public struct DirectoryItemInfo
        {
            public short FileID;
            public UInt16 FileHash;
            public UInt16 File_or_Folder;
            public UInt16 FileNameOffset;
            public int FileOffset_or_DirectoryIndex;
            public int FileSize_or_Directory10;
            public int Padding;
            public DirectoryItemInfo(short arg1, UInt16 arg2, UInt16 arg3, UInt16 arg4, int arg5, int arg6, int arg7)
            {
                FileID = arg1;
                FileHash = arg2;
                File_or_Folder = arg3;
                FileNameOffset = arg4;
                FileOffset_or_DirectoryIndex = arg5;
                FileSize_or_Directory10 = arg6;
                Padding = arg7;
            }
        }

        enum DataType : int
        {
            File = 0x1100,
            Folder = 0x0200,
            ArcFile = 0x9500
        }

        private static List<DirectoryItemInfo> Directory_Items;
        private static List<DirectoryPathInfo> DirectoryPaths;

        public class HeaderData 
        {
            public string Magic { get; set; }
            public int FileSize { get; set; }
            public int Unknown1 { get; set; }
            public int FileDataOffset { get; set; }
            public int FileDataLength_1 { get; set; }
            public int FileDataLength_2 { get; set; }
            public int Unknown2 { get; set; }
            public int Unknown3 { get; set; }

            public const int HeaderSize = 0x20;
            
            public void Read(BinaryReader br) 
            {
                Magic = CS.Byte2Char(br);
                FileSize = CS.Byte2Int(br);
                Unknown1 = CS.Byte2Int(br);
                FileDataOffset = CS.Byte2Int(br);
                FileDataLength_1 = CS.Byte2Int(br);
                FileDataLength_2 = CS.Byte2Int(br);
                Unknown2 = CS.Byte2Int(br);
                Unknown3 = CS.Byte2Int(br);
            }
        }

        public class InformationData 
        {
            public int NodeLength { get; set; }
            public int FirstNodeOffset { get; set; }
            public int TotalDirectoryCount { get; set; }
            public int FirstDirectoryOffset { get; set; }
            public int StringTableLength { get; set; }
            public int StringTableOffset { get; set; }
            public short DirectoryFileNum { get; set; }
            public short Unknown4 { get; set; }
            public int Unknown5 { get; set; }

            public void Read(BinaryReader br) 
            {
                NodeLength = CS.Byte2Int(br);
                FirstNodeOffset = CS.Byte2Int(br);
                TotalDirectoryCount = CS.Byte2Int(br);
                FirstDirectoryOffset = CS.Byte2Int(br);
                StringTableLength = CS.Byte2Int(br);
                StringTableOffset = CS.Byte2Int(br);
                DirectoryFileNum = CS.Byte2Short(br);
                Unknown4 = CS.Byte2Short(br);
                Unknown5 = CS.Byte2Int(br);
            }
        }

        public class FileSectionDataPosition 
        {
            public long BaseAddress { get; private set; }
            public long EndAddress { get; private set; }

            public void Set_BaseAddress(int fileDataOffset) 
            {
                BaseAddress = fileDataOffset + HeaderData.HeaderSize;
            }

            public void Set_EndAddress(int fileDataLength_1) 
            {
                EndAddress = BaseAddress + fileDataLength_1;
            }
        }

        public class NodeInformation 
        {
            public string ID { get; private set; }
            public int StringTopOffset { get; private set; }
            public ushort StringHash { get; private set; }
            public short FolderDirectoryCount { get; private set; }
            public int FirstDirectoryIndex { get; private set; }

            public static NodeInformation Read(BinaryReader br) 
            {
                return new NodeInformation
                {
                    ID = CS.Byte2Char(br),
                    StringTopOffset = CS.Byte2Int(br),
                    StringHash = CS.Byte2UShort(br),
                    FolderDirectoryCount = CS.Byte2Short(br),
                    FirstDirectoryIndex = CS.Byte2Int(br)
                };
            }
        }

        public class NodeSectionData 
        {
            public List<NodeInformation> Data;

            public NodeSectionData() 
            {
                Data = new List<NodeInformation>();
            }

            public void ReadData(BinaryReader br,InformationData information) 
            {
                for (int i = 0; i < information.NodeLength; i++)
                {
                    Data.Add(NodeInformation.Read(br));
                }

                br.BaseStream.Position = PaddingSkipEndPosition(br.BaseStream.Position);
            }
        }

        HeaderData Header = new HeaderData();
        InformationData Information = new InformationData();
        FileSectionDataPosition FileDataPosition = new FileSectionDataPosition();
        NodeSectionData NodeSection = new NodeSectionData();

        public void Extract(string rarc_path)
        {
            FileStream fs = new(rarc_path, FileMode.Open);
            BinaryReader br = new(fs);

            DirectoryPaths = new List<DirectoryPathInfo>();

            Header.Read(br);
            Information.Read(br);

            FileDataPosition.Set_BaseAddress(Header.FileDataOffset);
            FileDataPosition.Set_EndAddress(Header.FileDataLength_1);

            NodeSection.ReadData(br,Information);

            //fs.Position = PaddingSkipEndPosition(fs.Position);

            //Directoryセクション
            var RootDirectoryName = Path.GetDirectoryName(rarc_path);
            var ARCFileName = Path.GetFileNameWithoutExtension(rarc_path).ToString();
            var RootName = @"" + RootDirectoryName + "\\" + @"" + ARCFileName.ToString();

            //作業フォルダの作成
            DirectoryInfo workingDir = new DirectoryInfo(RootName);
            workingDir.Create();
            var RootFolder = "";

            var AllNodeItemsTotal = NodeSection.Data.Sum(x => x.FolderDirectoryCount);
            var FileEntryEnd = (AllNodeItemsTotal * 0x14) + fs.Position;
            FileEntryEnd = PaddingSkipEndPosition(FileEntryEnd);

            //Console.WriteLine("FileEntryEnd: " + FileEntryEnd.ToString("X"));

            RootFolder = CS.Bytes2String_NullEnd(fs, br, FileEntryEnd + (long)NodeSection.Data[0].StringTopOffset);
            //RootFolder = CS.Bytes2String_NullEnd(fs, br, FileEntryEnd + (long)Node_Items[0].StringTopOffset);
            //Console.WriteLine(RootFolder);


            //Rootフォルダの作成
            DirectoryInfo parentDir = workingDir.CreateSubdirectory(RootFolder);
            DirectoryPaths.Add(new DirectoryPathInfo(0, RootFolder));

            List<List<DirectoryItemInfo>> dirItemsList = new List<List<DirectoryItemInfo>>();
            //Console.WriteLine("Node");
            //foreach (var node_item in Node_Items.Select((Value, Index) => new { Value, Index }))
            foreach (var nodeItem in NodeSection.Data.Select((Value, Index) => new { Value, Index }))
            {


                //var nodeitemcounter = 0;

                Directory_Items = new List<DirectoryItemInfo>();
                for (var count = 0; count < nodeItem.Value.FolderDirectoryCount; count++)
                {
                    var FileID = CS.Byte2Short(br);
                    var FileHash = CS.Byte2UShort(br);
                    var File_or_Folder = (UInt16)CS.Byte2Short(br);
                    var FileNameOffset = CS.Byte2UShort(br);
                    var FileOffset_or_DirectoryIndex = CS.Byte2Int(br);
                    var FileSize_or_Directory10 = CS.Byte2Int(br);
                    var Padding = CS.Byte2Int(br);
                    Directory_Items.Add(new DirectoryItemInfo(FileID, FileHash, File_or_Folder, FileNameOffset, FileOffset_or_DirectoryIndex, FileSize_or_Directory10, Padding));

                    //Console.Write(Directory_Items[count].FileID.ToString("X4") + " ");
                    //Console.Write(Directory_Items[count].FileHash.ToString("X4") + " ");
                    //Console.Write(Directory_Items[count].File_or_Folder.ToString("X4") + " ");
                    //Console.Write(Directory_Items[count].FileOffset_or_DirectoryIndex.ToString("X8") + " ");
                    //Console.Write(Directory_Items[count].FileSize_or_Directory10.ToString("X8") + " ");
                    //Console.WriteLine(Directory_Items[count].Padding.ToString("X8"));

                    //nodeitemcounter++;
                }

                Directory_Items.Reverse();

                dirItemsList.Add(Directory_Items);



            }
            fs.Position = PaddingSkipEndPosition(fs.Position);
            //Console.WriteLine(fs.Position.ToString("X"));
            //Console.ReadKey();
            //List<string> DebugFileName = new();
            //DirIndexAndPath = new List<directoryindex_and_pathname>();
            foreach (var dirItemInfo in dirItemsList.Select((Value, Index) => new { Value, Index }))
            {

                var RootFolderFullpath = workingDir.FullName;
                foreach (var subitem in dirItemInfo.Value)
                {
                    var test = subitem.FileNameOffset + FileEntryEnd;
                    var itemname = CS.Bytes2String_NullEnd(fs, br, test);
                    switch (itemname)
                    {
                        case "..":
                            if (subitem.FileOffset_or_DirectoryIndex == -1)
                            {
                                //Console.WriteLine("parent_none");
                            }
                            else
                            {
                                var dipfinder = DirectoryPaths.FirstOrDefault(dirPathInfo => dirPathInfo.Index == subitem.FileOffset_or_DirectoryIndex);
                                var testes = NodeSection.Data[subitem.FileOffset_or_DirectoryIndex].StringTopOffset;
                                //Console.WriteLine("parent " + CS.Bytes2String_NullEnd(fs, br, testes + FileEntryEnd));

                                if (dipfinder.Index == default || dipfinder.Index == 0)
                                {
                                    RootFolderFullpath += (@"\" + CS.Bytes2String_NullEnd(fs, br, testes + FileEntryEnd));
                                    //Console.WriteLine("___");
                                }
                                else
                                {
                                    RootFolderFullpath = (@"" + dipfinder.Path);
                                    //Console.WriteLine();
                                }
                                //Console.ReadKey();
                                //subdi = di.CreateSubdirectory(CS.Bytes2String_NullEnd(fs, br, testes + FileEntryEnd));
                            }
                            break;
                        case ".":
                            if (subitem.FileOffset_or_DirectoryIndex == -1)
                            {
                                //Console.WriteLine("Now_Directory");
                            }
                            else
                            {
                                //Console.WriteLine(subitem.FileID.ToString("X"));
                                //var z = diitemsList.Find(findList => findList.Find(finditem => finditem.FileOffset_or_DirectoryIndex.Equals(subitem.FileID)).FileID.Equals(subitem.FileID));

                                //Console.WriteLine((z[testitem.Index].FileNameOffset+FileEntryEnd).ToString("X"));
                                var testes = NodeSection.Data[subitem.FileOffset_or_DirectoryIndex].StringTopOffset;
                                //Console.WriteLine("Now_Directory " + CS.Bytes2String_NullEnd(fs, br, testes + FileEntryEnd));
                                RootFolderFullpath += (@"\" + CS.Bytes2String_NullEnd(fs, br, testes + FileEntryEnd));
                                //Console.WriteLine(".: else");
                            }
                            break;
                        default:
                            if ((subitem.FileOffset_or_DirectoryIndex <= -1))
                            {
                                //Console.WriteLine("folder or file");
                            }
                            else
                            {
                                var filefoldername = CS.Bytes2String_NullEnd(fs, br, subitem.FileNameOffset + FileEntryEnd);

                                if (subitem.File_or_Folder == 0x0200)
                                {
                                    var dirNameoffset = NodeSection.Data[subitem.FileOffset_or_DirectoryIndex].StringTopOffset;
                                    filefoldername = CS.Bytes2String_NullEnd(fs, br, dirNameoffset + FileEntryEnd);

                                    var thisdir = new DirectoryPathInfo(subitem.FileOffset_or_DirectoryIndex, RootFolderFullpath + @"\" + filefoldername);
                                    var finddirectory = DirectoryPaths.IndexOf(thisdir);
                                    if ((finddirectory == -1) && (subitem.FileID != -1))
                                    {
                                        DirectoryPaths.Add(thisdir);
                                        Directory.CreateDirectory(thisdir.Path);
                                    }
                                    else 
                                    {
                                        //thisdir.Path = Directory.GetParent(thisdir.Path).FullName;
                                        DirectoryPaths.Add(thisdir);
                                        Directory.CreateDirectory(thisdir.Path);
                                    }

                                }
                                else if ((subitem.File_or_Folder == 0x1100) || (subitem.File_or_Folder == 0x9500))
                                {
                                    //DebugFileName.Add(filefoldername);
                                    var FileCreatePath = RootFolderFullpath + @"\" + filefoldername;
                                    var pos_old_fs = fs.Position;
                                    fs.Seek(FileDataPosition.BaseAddress + subitem.FileOffset_or_DirectoryIndex, SeekOrigin.Begin);
                                    byte[] FileData = br.ReadBytes(subitem.FileSize_or_Directory10);

                                    //var fileWriteDir = Directory.GetParent(FileCreatePath).FullName;
                                    //if (!File.Exists(fileWriteDir)) 
                                    //{
                                    //    Console.WriteLine(fileWriteDir);
                                    //    Directory.CreateDirectory(fileWriteDir);
                                    //}

                                    File.WriteAllBytes(FileCreatePath, FileData);
                                    fs.Seek(pos_old_fs, SeekOrigin.Begin);
                                    //Console.WriteLine("fileNo." + subitem.FileID.ToString("X"));
                                }
                                else
                                {
                                    //Console.WriteLine("__________________");
                                }
                            }
                            break;
                    }

                }
                //Console.WriteLine("OK");
            }
            //DebugFileName.Reverse();
            //foreach (var te in DebugFileName) Console.WriteLine(te);
            //Console.ReadKey();
            //16byte0
            CS.Byte2Int(br);
            CS.Byte2Int(br);
            CS.Byte2Int(br);
            CS.Byte2Int(br);

            //パディングを挿入した際に0x10で割り切れない場合の計算
            if (fs.Position % 32f != 0)
            {
                bool flag = true;
                while (flag)
                {
                    if (fs.Position % 32f == 0) break;
                    br.ReadByte();
                }
            }

            var testpos = fs.Position;
            //Console.WriteLine(testpos.ToString("X"));

            //names
            //Console.ReadKey();


            //end
            fs.Close();
            br.Close();
        }


        /// <summary>
        /// RARCのファイルエントリーオフセットを計算します。<br/>
        /// <return>戻り値：ファイルエントリーオフセットの先頭</return>
        /// </summary>
        /// <remarks>圧縮専用</remarks>
        public static int RARC_FEO(string[] DirectoryStrings)
        {
            var feo = 0x40 + (DirectoryStrings.Count() * 0x10);
            if (feo % 32f != 0)
            {
                bool flag = true;
                while (flag)
                {
                    if (feo % 32f == 0) break;
                    feo++;
                }
            }
            return feo - 0x20;
        }

        public static short DirectoryDepthCheck(string[] FileStrings, string[] DirectoryStrings, out short DirectoryDepthType) {
            DirectoryDepthType = 0x0000;
            short FSL = (short)FileStrings.Length;
            short DSL = (short)DirectoryStrings.Length;
            short AllFileCount = FSL;
            var HasTwoDepth = true;
            Console.WriteLine($"FSL{FSL}");
            //var TmpDir = Directory.GetDirectories(DirectoryStrings[0], "*", SearchOption.TopDirectoryOnly);
            //foreach (var TmpTmpDir in TmpDir) {
            //    var Tmp3Dir = Directory.GetDirectories(TmpTmpDir, "*", SearchOption.TopDirectoryOnly).Length;
            //    if (Tmp3Dir != 0) {
            //        HasTwoDepth = false;
            //        break;
            //    } 
            //}

            foreach (var dirName in DirectoryStrings) {
                var dir = Directory.GetDirectories(dirName, "*", SearchOption.TopDirectoryOnly).Length;
                var file = Directory.GetFiles(dirName, "*", SearchOption.TopDirectoryOnly).Length;
                Console.WriteLine($"dir{dir}:file{file}");
                if ((dir + file) < 3) {
                    
                    HasTwoDepth = false;
                    break;
                }
            }


            //Console.ReadKey();
            //ディレクトリの深さが3以上の場合の処理
            if (HasTwoDepth)
            {
                DirectoryDepthType = 0x0100;
                AllFileCount = (short)(FSL + (DSL - (short)1) + (DSL * (short)2));
                return AllFileCount;
            }

            Console.WriteLine($"Has not TwoDepth{AllFileCount}");
            Console.ReadKey();
            return AllFileCount;
        }

        /// <summary>
        /// RARCで圧縮する全てのファイルを数えます<br/>
        /// <return>戻り値：ファイルの総数、またはファイルとフォルダの総数</return>
        /// </summary>
        /// <remarks>圧縮専用「条件によってフォルダの数を加算する場合があります」</remarks>
        public static short TotalFileCount(string[] FileStrings, string[] DirectoryStrings, out short DirectoryDepthType)
        {
            DirectoryDepthType = 0x0000;
            short FSL = (short)FileStrings.Length;
            short DSL = (short)DirectoryStrings.Length;
            short AllFileCount = FSL;
            var HasOneDepth = false;
            var RootDirectoryPath = DirectoryStrings[0];
            //Console.WriteLine(RootDirectoryPath + "_RootPath");
            foreach (var DirectoryPath in DirectoryStrings.Select((Value, Index) => (Value, Index)))
            {
                if (DirectoryPath.Value == RootDirectoryPath) {
                    if (DSL == 1) {
                        //Console.ReadKey();
                        HasOneDepth = true;
                        //break;
                        
                    } 
                    continue;
                }
                
                var ParentDirectory1Upper = Directory.GetParent(DirectoryPath.Value);
                var ParentDirectory2Upper = Directory.GetParent(ParentDirectory1Upper.FullName);
                bool HasDepth1 = ParentDirectory1Upper.FullName == RootDirectoryPath;
                bool HasDepth2 = ParentDirectory2Upper.FullName == RootDirectoryPath;

                if ((HasDepth1 || HasDepth2)) { 
                    HasOneDepth = true;
                    ////break;
                }
                

                Console.WriteLine(DirectoryPath.Value);
                Console.WriteLine("↓↓↓↓↓↓↓↓↓");
                Console.WriteLine(ParentDirectory1Upper.FullName);
                Console.WriteLine(ParentDirectory2Upper);
            }
            Console.ReadKey();
            //ディレクトリの深さが3以上の場合の処理
            if (HasOneDepth)
            {
                DirectoryDepthType = 0x0100;
                AllFileCount = (short)(FSL + (DSL - 1) + (DSL * 2));
            }
            return AllFileCount;
        }

        public static IOrderedEnumerable<string> Sum_StrinArray_Sort(string[] dirstrs, string[] filestrs)
        {
            var dirandfilearray = dirstrs.Concat(filestrs).ToArray();
            var SortedDirFileArray = dirandfilearray.OrderBy(sort => sort);



            return SortedDirFileArray;
        }

        public static string[] Test(string[] dirstrs)
        {
            var SortedDir = dirstrs.OrderBy(sort => sort);
            List<string> SortedDirFileList = new();
            foreach (var path in SortedDir)
            {
                SortedDirFileList.Add(path);

                var DirInFile = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).OrderBy(sort => sort);
                foreach (var FilePath in DirInFile)
                {
                    SortedDirFileList.Add(FilePath);

                    var testpath = Path.GetFileName(FilePath);
                    //var exlengthnum = CS.ARC_Hash(Path.GetExtension(FilePath)+'\0');
                    //Console.WriteLine(testpath);
                    //Console.Write((CS.ARC_Hash(testpath)+exlengthnum)+"   ");
                    //Console.Write(CS.ARC_Hash2(testpath,(ushort)(testpath.Length + exlengthnum))+"   ");
                    //Console.WriteLine(CS.MSBT_Hash(testpath, exlengthnum));
                }


                //var DirInDir = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly).OrderBy(sort => sort);
                //foreach (var DirPath in DirInDir) SortedDirFileList.Add(DirPath);
            }
            //Console.ReadKey();
            return SortedDirFileList.ToArray();
        }

        public static string[] File_And_Dir_Null_Appender(IOrderedEnumerable<string> SortedDirFileArray)
        {
            //var dirandfilearray = dirstrs.Concat(filestrs).ToArray();
            //var SortedDirFileArray = dirandfilearray.OrderBy(sort => sort);
            var SDFA = SortedDirFileArray.Select(s => s + (char)0);
            var SDFAA = SDFA.ToArray();
            return SDFAA;
        }

        public static string[] File_And_Dir_Null_Appender(string[] SortedDirFileArray)
        {
            //var dirandfilearray = dirstrs.Concat(filestrs).ToArray();
            //var SortedDirFileArray = dirandfilearray.OrderBy(sort => sort);
            var SDFA = SortedDirFileArray.Select(s => s + (char)0);
            var SDFAA = SDFA.ToArray();
            return SDFAA;
        }

        public void Archive(string extractPath, string[] dirstrs, string[] filestrs)
        {
            FileStream fs = new(extractPath, FileMode.Create);
            BinaryWriter bw = new(fs);
            Directory_Items = new List<DirectoryItemInfo>();

            long pos_File_Size = 0x4;
            long pos_FileDataSection = 0xC;
            //long pos_FileDataSectionLength = 0x10;

            var IDIC = In_DirectoryItemCount(dirstrs);
            var Sum_IDIC = IDIC.Sum();
            var FileEntryOffset = RARC_FEO(dirstrs);


            //ディレクトリの深さが2を超える → 0x0000
            //ディレクトリの深さが2以下     → 0x0100
            //※カレントディレクトリをカウントしない
            short DirectoryDepth_LessThan2 = 0x0000;
            //short AllFileCount = TotalFileCount(filestrs, dirstrs, out DirectoryDepth_LessThan2);
            short AllFileCount = DirectoryDepthCheck(filestrs, dirstrs, out DirectoryDepth_LessThan2);

            //RARC Header
            CS.String_Writer(bw, "RARC");
            CS.Null_Writer_Int32(bw);
            CS.String_Writer_Int(bw, 0x00000020);
            CS.Null_Writer_Int32(bw, 5);
            //0x20
            CS.String_Writer_Int(bw, dirstrs.Count());
            CS.String_Writer_Int(bw, 0x00000020);
            CS.String_Writer_Int(bw, Sum_IDIC);
            CS.String_Writer_Int(bw, FileEntryOffset);
            //0x30
            var pos_StringTable = fs.Position;
            CS.Null_Writer_Int32(bw, 2);
            CS.String_Writer_Int(bw, AllFileCount);
            CS.String_Writer_Int(bw, DirectoryDepth_LessThan2);//
            CS.Null_Writer_Int32(bw);
            //0x40


            var SDFA = Test(dirstrs);
            var SDFAA = File_And_Dir_Null_Appender(SDFA);

            MemoryStream ms = new();
            BinaryWriter bw2 = new(ms);


            RARC_StringName.MemoryWrite(ms, bw2, SDFAA);
            var nameoffset = RARC_StringName.NameOffset;
            var nameoffsetstr = RARC_StringName.NameOffsetStr;

            //Node Section
            RARC_NodeSection RARCNS = new()
            {
                nameoffset = nameoffset,
                nameoffsetstr = nameoffsetstr,
                dirstrs = dirstrs,
                IDIC = IDIC,
                bw = bw
            };
            var FileEntrySectionItemNum = RARCNS.Write();
            PaddingWriter(fs, bw);

            var pos_nulldataold = fs.Position;
            for (var i = 0; i < FileEntrySectionItemNum; i++)
                NullDataWriter(bw);
            PaddingWriter(fs, bw);

            var pos_StringDataTop = fs.Position;
            bw.Write(ms.ToArray());
            PaddingWriter(fs, bw);
            var pos_FileDataTop = fs.Position;



            fs.Seek(pos_nulldataold, SeekOrigin.Begin);


            //FileEntrySection
            var currentfolder = extractPath.Substring(0, extractPath.Count() - 4);
            //Console.WriteLine(currentfolder);
            string[] dirnode;
            string[] filenode;
            var filesizetotal = 0;
            //foreach (var tesitem in SDFA) Console.WriteLine(tesitem);
            //filestrs.Reverse();
            var filenum = 0;
            var testfilenum = 0;
            foreach (var node in dirstrs.Select((Value, Index) => (Value, Index)))
            {
                dirnode = Directory.GetDirectories(node.Value, "*", SearchOption.TopDirectoryOnly).OrderBy(sort => sort).ToArray();
                filenode = Directory.GetFiles(node.Value, "*", SearchOption.TopDirectoryOnly).OrderBy(sort => sort).ToArray();
                //filenum = 0;

                var dirConcatfile = filenode.Concat(dirnode);
                var sorted_dirConcatfile = dirConcatfile.ToArray();
                var diraddfile = sorted_dirConcatfile.Count();
                for (var i = 0; i < diraddfile; i++)
                {

                    if (File.Exists(sorted_dirConcatfile[i]))
                    {
                        //var listdirfile = SDFA.ToList();

                        var listdirfile = SDFA.ToList();
                        var fileindex = listdirfile.IndexOf(sorted_dirConcatfile[i]);

                        //ディレクトリの数をファイルのインデックス番号に含めない
                        if (DirectoryDepth_LessThan2 != 0x0100) filenum = testfilenum;

                        //Console.WriteLine(filenum);
                        //Console.WriteLine(sorted_dirConcatfile[i]);
                        //Console.ReadKey();
                        var filename = Path.GetFileName(sorted_dirConcatfile[i]);
                        FileInfo fileinfo = new FileInfo(sorted_dirConcatfile[i]);
                        var filesize = Convert.ToInt32(fileinfo.Length);

                        var sdcfList = SDFA.ToList();
                        var sdcfListIndex = sdcfList.IndexOf(sorted_dirConcatfile[i]);
                        //Console.WriteLine(sdcfListIndex.ToString("X") + "_sdcfListIndex_" + sorted_dirConcatfile[i]);
                        CS.String_Writer_Int(bw, (short)/*fileindex*/filenum);
                        CS.String_Writer_Int(bw, (short)CS.ARC_Hash(filename));
                        switch (Path.GetExtension(sorted_dirConcatfile[i]))
                        {
                            case ".arc":
                                CS.String_Writer_Int(bw, (UInt16)0x9500);
                                break;
                            default:
                                CS.String_Writer_Int(bw, (short)0x1100);
                                break;
                        }

                        //Console.WriteLine(nameoffset[sdcfListIndex].ToString("X"));
                        CS.String_Writer_Int(bw, (short)nameoffset[sdcfListIndex]);
                        CS.String_Writer_Int(bw, filesizetotal);
                        CS.String_Writer_Int(bw, filesize);
                        CS.String_Writer_Int(bw, 0x00000000);

                        //fileWriter
                        var pos_before_filedata_write = fs.Position;
                        fs.Seek(pos_FileDataTop + filesizetotal, SeekOrigin.Begin);
                        bw.Write(File.ReadAllBytes(sorted_dirConcatfile[i]));
                        fs.Seek(pos_before_filedata_write, SeekOrigin.Begin);

                        testfilenum++;

                        filesizetotal += filesize;
                        if (filesizetotal % 32f != 0)
                        {
                            bool flag = true;
                            while (flag)
                            {
                                if (filesizetotal % 32f == 0) break;
                                filesizetotal++;
                            }
                        }
                    }
                    else if (Directory.Exists(sorted_dirConcatfile[i]))
                    {
                        //filenum = 0;
                        CS.String_Writer_Int(bw, (short)-1);
                        var foldaname = Path.GetFileName(sorted_dirConcatfile[i]);

                        CS.String_Writer_Int(bw, (short)CS.ARC_Hash(foldaname));

                        CS.String_Writer_Int(bw, (short)0x0200);
                        var listdirfile = dirstrs.ToList();
                        var dirindex = listdirfile.IndexOf(sorted_dirConcatfile[i]);

                        var sdcfList = SDFA.ToList();
                        var sdcfListIndex = sdcfList.IndexOf(sorted_dirConcatfile[i]);
                        //Console.WriteLine(sdcfListIndex.ToString("X") + "_sdcfListIndex_" + sorted_dirConcatfile[i]);
                        //Console.WriteLine(nameoffset[sdcfListIndex].ToString("X"));
                        CS.String_Writer_Int(bw, (short)nameoffset[sdcfListIndex]);
                        //Console.WriteLine(dirindex.ToString("X8"));
                        CS.String_Writer_Int(bw, dirindex);
                        CS.String_Writer_Int(bw, 0x00000010);
                        CS.String_Writer_Int(bw, 0x00000000);
                    }
                    filenum++;
                }

                RARC_Directory_Structure RARCDS = new RARC_Directory_Structure();
                RARCDS.bw = bw;
                RARCDS.dirstrs = dirstrs;
                RARCDS.node = node.Value;
                RARCDS.Writer();
                filenum += 2;
                //}
            }

            fs.Seek(fs.Length, SeekOrigin.Begin);
            PaddingWriter(fs, bw);

            //Console.WriteLine(CS.ARC_Hash(".."));

            //ファイルサイズを書き込む
            fs.Seek(pos_File_Size, SeekOrigin.Begin);
            CS.String_Writer_Int(bw, (int)fs.Length);

            //ファイルデータセクションのオフセットとサイズを書き込む
            fs.Seek(pos_FileDataSection, SeekOrigin.Begin);
            CS.String_Writer_Int(bw, (int)(pos_FileDataTop - 0x20));
            CS.String_Writer_Int(bw, (int)(fs.Length - pos_FileDataTop));
            CS.String_Writer_Int(bw, (int)(fs.Length - pos_FileDataTop));

            //文字列テーブルのセクションサイズとオフセット
            fs.Seek(pos_StringTable, SeekOrigin.Begin);
            CS.String_Writer_Int(bw, (int)(pos_FileDataTop - pos_StringDataTop));
            CS.String_Writer_Int(bw, (int)(pos_StringDataTop - 0x20));

            //Console.WriteLine("RARC_End");

            //fs,bw終了処理
            fs.Close();
            bw.Close();
        }


        /// <summary>
        /// RARCのフォルダの数を計算します。<br/>
        /// <return>戻り値：フォルダの個数</return>
        /// </summary>
        /// <remarks>圧縮専用</remarks>
        public static List<int> In_DirectoryItemCount(string[] DirStrs)
        {
            List<int> item = new();
            foreach (var diritem in DirStrs)
            {
                var dirindir = Directory.GetDirectories(diritem, "*", SearchOption.TopDirectoryOnly);
                var dirinfile = Directory.GetFiles(diritem, "*", SearchOption.TopDirectoryOnly);
                var dirinitemCount = dirindir.Count() + (dirinfile.Count() + 2);
                //Console.WriteLine(dirinitemCount);
                item.Add(dirinitemCount);
            }
            return item;
        }

        /// <summary>
        /// 0x20で割り切れるまでパディングデータを書き込みます。<br/>
        /// <return>戻り値：なし</return>
        /// </summary>
        /// <remarks>圧縮専用</remarks>
        public static void PaddingWriter(FileStream fs, BinaryWriter bw)
        {
            if (fs.Position % 32f != 0)
            {
                bool flag = true;
                while (flag)
                {
                    if (fs.Position % 32f == 0) break;
                    CS.String_Writer_Int(bw, (byte)0);
                }
            }
        }

        public static long PaddingSkipEndPosition(long position)
        {
            if (position % 32f != 0)
            {
                while (true)
                {
                    if (position % 32f == 0) break;
                    position++;
                }
            }
            return position;
        }

        public static void NullDataWriter(BinaryWriter bw, int DirFileTotalNum = 0x14)
        {
            byte[] bit = new byte[DirFileTotalNum];
            for (var i = 0; i < DirFileTotalNum; i++) bit[i] = 0x00;
            bw.Write(bit);
        }
    }
}
