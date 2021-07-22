using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CS = ARCTool.FileSys.Calculation_System;

namespace ARCTool.FileSys
{
    class RARC
    {
        public struct directory_index_path {
            public int Index;
            public string Path;
            public directory_index_path(int index , string path) {
                this.Index = index;
                this.Path  = path;
            }
        }

        public struct node_items {
            public string Identifier;
            public int StringTopOffset;
            public UInt16 StringHash;
            public short ThisFolderDirectoryCount;
            public int FirstDirectoryIndex;
            public node_items(string str,int offset,UInt16 hash,short Dcount,int Dindex) {
                this.Identifier = str;
                this.StringTopOffset = offset;
                this.StringHash = hash;
                this.ThisFolderDirectoryCount = Dcount;
                this.FirstDirectoryIndex = Dindex;
            }
        }

        public struct directory_items {
            public short FileID;
            public UInt16 FileHash;
            public short File_or_Folder;
            public short FileNameOffset;
            public int FileOffset_or_DirectoryIndex;
            public int FileSize_or_Directory10;
            public int Padding;
            public directory_items(short arg1, UInt16 arg2 , short arg3 , short arg4,int arg5,int arg6 ,int arg7) {
                this.FileID = arg1;
                this.FileHash = arg2;
                this.File_or_Folder = arg3;
                this.FileNameOffset = arg4;
                this.FileOffset_or_DirectoryIndex = arg5;
                this.FileSize_or_Directory10 = arg6;
                this.Padding = arg7;
            }
        }

        public struct directoryindex_and_pathname {
            public int index;
            public string path;
            public directoryindex_and_pathname(int arg1 , string arg2) {
                this.index = arg1;
                this.path = arg2;
            }
        
        }

        private static List<node_items> Node_Items;
        private static List<directory_items> Directory_Items;
        private static List<directoryindex_and_pathname> DirIndexAndPath;
        private static List<directory_index_path> Directory_Index_Path;

        

        public string Magic { get; set; }
        public int FileSize { get; set; }
        public int Unknown1 { get; set; }
        public int FileDataOffset { get; set; }
        public int FileDataLength01 { get; set; }
        public int FileDataLength02 { get; set; }
        public int Unknown2 { get; set; }
        public int Unknown3 { get; set; }
        public int NodeNum { get; set; }
        public int FirstNodeOffset { get; set; }
        public int TotalDirectoryCount { get; set; }
        public int FirstDirectoryOffset { get; set; }
        public int StringTableLength { get; set; }
        public int StringTableOffset { get; set; }
        public short DirectoryFileNum { get; set; }
        public short Unknown4 { get; set; }
        public int Unknown5 { get; set; }
    public void Read(string rarc_path)
        {
            FileStream fs = new FileStream(rarc_path, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            Node_Items = new List<node_items>();
            Directory_Index_Path = new List<directory_index_path>();

            //File Header
            Magic            = CS.Byte2Char(br);
            FileSize         = CS.Byte2Int(br);
            Unknown1         = CS.Byte2Int(br);
            FileDataOffset   = CS.Byte2Int(br);
            FileDataLength01 = CS.Byte2Int(br);
            FileDataLength02 = CS.Byte2Int(br);
            Unknown2         = CS.Byte2Int(br);
            Unknown3         = CS.Byte2Int(br);

            //Info セクション
            NodeNum              = CS.Byte2Int(br);
            FirstNodeOffset      = CS.Byte2Int(br);
            TotalDirectoryCount  = CS.Byte2Int(br);
            FirstDirectoryOffset = CS.Byte2Int(br);
            StringTableLength    = CS.Byte2Int(br);
            StringTableOffset    = CS.Byte2Int(br);
            DirectoryFileNum     = CS.Byte2Short(br);
            Unknown4             = CS.Byte2Short(br);
            Unknown5             = CS.Byte2Int(br);

            long pos_FileDataTop = FileDataOffset + 0x20;
            long pos_FileDataEnd = pos_FileDataTop + FileDataLength01;
            Console.WriteLine("//////////Infoセクション終了//////////");

            //Node
            for(int i = 0; i<NodeNum; i++)
            {
                var Identifier = CS.Byte2Char(br);
                var StringTopOffset = CS.Byte2Int(br);
                var StringHash = CS.Byte2UShort(br);
                var ThisFolderDirectoryCount = CS.Byte2Short(br);
                var FirstDirectoryIndex = CS.Byte2Int(br);
                Node_Items.Add(new node_items(Identifier,StringTopOffset,StringHash,ThisFolderDirectoryCount,FirstDirectoryIndex));
                Console.WriteLine(Identifier);
            }

            //16byte0
            CS.Byte2Int(br);
            CS.Byte2Int(br);
            CS.Byte2Int(br);
            CS.Byte2Int(br);
            Console.WriteLine("//////////nodeend//////////");
            //Directory
            
            var RootDirectoryName = Path.GetDirectoryName(rarc_path);
            //Console.WriteLine("rootdirectoryname"+RootDirectoryName);

            var ARCFileName = Path.GetFileNameWithoutExtension(rarc_path).ToString();
            //Console.WriteLine("arcfilename"+ARCFileName);

            var RootName = @"" + RootDirectoryName + "\\" + @""+ARCFileName.ToString();
            //Console.WriteLine("rootname"+RootName);

            //作業フォルダの作成
            DirectoryInfo di = new DirectoryInfo(RootName);
            di.Create();
            var RootFolder = "";
            var FileEntryEnd = (Node_Items.Sum(x => x.ThisFolderDirectoryCount)*0x14)  + fs.Position;

            //パディングを挿入した際に0x10で割り切れない場合の計算
            if (FileEntryEnd % 32f != 0) {
                bool flag = true;
                while (flag) {
                    if (FileEntryEnd % 32f == 0) break;
                    FileEntryEnd++;
                }
            }
            Console.WriteLine("fileentryend"+FileEntryEnd.ToString("X"));

            Console.WriteLine("stringtopoffset"+ Node_Items[0].StringTopOffset);
            RootFolder = CS.Bytes2String_NullEnd(fs, br, FileEntryEnd + (long)Node_Items[0].StringTopOffset);

            //ルートフォルダを作成
            //Console.WriteLine("hash num         " + CS.ARC_Hash(RootFolder).ToString("X")) ;
            DirectoryInfo diSub = di.CreateSubdirectory(RootFolder);
            Directory_Index_Path.Add(new directory_index_path(0,RootFolder));

            Console.WriteLine("");
            List<List<directory_items>> diitemsList = new List<List<directory_items>>();  
            foreach (var node_item in Node_Items.Select((Value, Index) => new { Value, Index })) {
                var nodeitemcounter = 0;

                Directory_Items = new List<directory_items>();
                for (var count = 0; count < node_item.Value.ThisFolderDirectoryCount; count++) {
                    var FileID = CS.Byte2Short(br);
                    var FileHash = CS.Byte2UShort(br);
                    var File_or_Folder = CS.Byte2Short(br);
                    var FileNameOffset = CS.Byte2Short(br);
                    var FileOffset_or_DirectoryIndex = CS.Byte2Int(br);
                    var FileSize_or_Directory10 = CS.Byte2Int(br);
                    var Padding = CS.Byte2Int(br);
                    Directory_Items.Add(new directory_items(FileID,FileHash,File_or_Folder,FileNameOffset,FileOffset_or_DirectoryIndex,FileSize_or_Directory10,Padding));
                    Console.WriteLine(Directory_Items[nodeitemcounter].FileID.ToString("X4"));
                    Console.WriteLine(Directory_Items[nodeitemcounter].File_or_Folder.ToString("X4"));
                    Console.WriteLine(Directory_Items[nodeitemcounter].FileOffset_or_DirectoryIndex.ToString("X8"));
                    nodeitemcounter++;
                }

                Directory_Items.Reverse();

                diitemsList.Add(Directory_Items);
                
                Console.WriteLine("");
                Console.WriteLine("");
            }

            //Directory_Items.Reverse();
            //DirectoryInfo subdi , sub2di;

            Console.WriteLine("di fullpath");
            Console.WriteLine(di.FullName);
            //List<int> aaa = new 
            DirIndexAndPath = new List<directoryindex_and_pathname>();
            foreach (var testitem in diitemsList.Select((Value, Index) => new { Value, Index })) {
                //if (testitem.Index == 0) {
                //    //RootFolderにフォルダやファイルを生成する
                //    DirectoryInfo sub2di = diSub.CreateSubdirectory();
                //    continue;
                //}
                var RootFolderFullpath = di.FullName;
                foreach (var subitem in testitem.Value) {
                    var test = subitem.FileNameOffset + FileEntryEnd;
                    var itemname = CS.Bytes2String_NullEnd(fs, br, test);
                    switch (itemname) {
                        case "..":
                            if (subitem.FileOffset_or_DirectoryIndex == -1){
                                Console.WriteLine("parent_none");
                            }
                            else {
                                //var z = diitemsList.Find(findList => findList.Find(finditem => finditem.FileOffset_or_DirectoryIndex.Equals(subitem.FileID)).FileID.Equals(subitem.FileID));
                                var dipfinder = Directory_Index_Path.FirstOrDefault(x => x.Index == subitem.FileOffset_or_DirectoryIndex);
                                Console.WriteLine(subitem.FileOffset_or_DirectoryIndex.ToString("X"));
                                //Console.WriteLine((z[testitem.Index].FileNameOffset+FileEntryEnd).ToString("X"));
                                var testes = Node_Items[subitem.FileOffset_or_DirectoryIndex].StringTopOffset;
                                Console.WriteLine("parent "+CS.Bytes2String_NullEnd(fs, br, testes+FileEntryEnd));

                                if (dipfinder.Index == default || dipfinder.Index == 0) {
                                    Console.WriteLine(RootFolderFullpath += (@"\" + CS.Bytes2String_NullEnd(fs, br, testes + FileEntryEnd)));
                                }
                                else {
                                    Console.WriteLine(RootFolderFullpath = (@"" + dipfinder.Path));
                                }
                                
                                //subdi = di.CreateSubdirectory(CS.Bytes2String_NullEnd(fs, br, testes + FileEntryEnd));
                            }
                            break;
                        case ".":
                            if (subitem.FileOffset_or_DirectoryIndex == -1)
                            {
                                Console.WriteLine("Now_Directory");
                            }
                            else
                            {
                                Console.WriteLine(subitem.FileID.ToString("X"));
                                //var z = diitemsList.Find(findList => findList.Find(finditem => finditem.FileOffset_or_DirectoryIndex.Equals(subitem.FileID)).FileID.Equals(subitem.FileID));

                                //Console.WriteLine((z[testitem.Index].FileNameOffset+FileEntryEnd).ToString("X"));
                                var testes = Node_Items[subitem.FileOffset_or_DirectoryIndex].StringTopOffset;
                                Console.WriteLine("Now_Directory " + CS.Bytes2String_NullEnd(fs, br, testes + FileEntryEnd));
                                Console.WriteLine( RootFolderFullpath += (@"\" + CS.Bytes2String_NullEnd(fs, br, testes + FileEntryEnd)));
                            }
                            break;
                        default:
                            if (subitem.FileOffset_or_DirectoryIndex == -1)
                            {
                                Console.WriteLine("folder or file");
                            }
                            else
                            {
                                var filefoldername = CS.Bytes2String_NullEnd(fs, br, subitem.FileNameOffset + FileEntryEnd);
                                var testes3 = subitem.FileSize_or_Directory10;
                                Console.WriteLine(subitem.FileID.ToString("X"));
                                var testes2 = subitem.FileOffset_or_DirectoryIndex;

                                var testes = subitem.File_or_Folder;
                                var type = "File " + testes2.ToString("X");
                                if (testes == 0x0200) {
                                    type = "SubDirectory↓ " + testes2.ToString("X") + "_" + testes3.ToString("X");
                                    var thisdir = new directory_index_path(testes2, RootFolderFullpath + @"\" + filefoldername);
                                    var finddirectory = Directory_Index_Path.IndexOf(thisdir);
                                    if (finddirectory == -1) {
                                        Directory_Index_Path.Add(thisdir);
                                        //Console.WriteLine("作成↓");
                                        //Console.WriteLine(thisdir.Path);
                                        Directory.CreateDirectory(thisdir.Path);
                                    }

                                } else if (testes == 0x1100) {
                                    var FileCreatePath = RootFolderFullpath + @"\" + filefoldername;
                                    var pos_old_fs = fs.Position;
                                    fs.Seek(pos_FileDataTop + subitem.FileOffset_or_DirectoryIndex, SeekOrigin.Begin);
                                    byte[] FileData = br.ReadBytes(subitem.FileSize_or_Directory10);
                                    File.WriteAllBytes(FileCreatePath,FileData);
                                    fs.Seek(pos_old_fs,SeekOrigin.Begin);
                                    Console.WriteLine("fileNo."+subitem.FileID.ToString("X"));
                                } else { 
                                
                                }
                                Console.WriteLine(type);

                                //var z = diitemsList.Find(findList => findList.Find(finditem => finditem.FileOffset_or_DirectoryIndex.Equals(subitem.FileID)).FileID.Equals(subitem.FileID));

                                //Console.WriteLine((z[testitem.Index].FileNameOffset+FileEntryEnd).ToString("X"));
                                //var testes = Node_Items[subitem.FileOffset_or_DirectoryIndex].StringTopOffset;
                                Console.WriteLine("folder or file " + filefoldername);

                            }
                            break;
                    }
                }
                Console.WriteLine("\n\r");
            }

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
            Console.WriteLine(testpos.ToString("X"));

            //names



            //end
            fs.Close();
            br.Close();
        }

        public void Write(string rarc_path , string[] dirstrs ,string[] filestrs) {
            FileStream fs = new FileStream(rarc_path, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            long pos_File_Size = 0x8;
            long pos_FileDataSection = 0xC;
            long pos_FileDataSectionLength = 0x10;

            var RARC = Encoding.ASCII.GetBytes("RARC");
            var IDIC = In_DirectoryItemCount(dirstrs);
            var Sum_IDIC = IDIC.Sum();
            var FileEntryOffset = 0x20 + (dirstrs.Count() * 0x10) + 0x10;
            var IntHexString20 = CS.StringToBytes((0x00000020).ToString("X8"));

            short dirindirOne = 0x0000;
            short AllFileCount =(short)filestrs.Count();
            if (dirstrs.Count() == 1) {
                dirindirOne = 0x0100;
                AllFileCount = (short)(filestrs.Count() + dirstrs.Count() + 1);
            }

            //RARC Header
            CS.String_Writer(bw,"RARC");
            CS.Null_Writer_Int32(bw);
            CS.String_Writer_Int(bw,0x00000020);
            CS.Null_Writer_Int32(bw,5);
            CS.String_Writer_Int(bw,dirstrs.Count());
            CS.String_Writer_Int(bw, 0x00000020);
            CS.String_Writer_Int(bw,Sum_IDIC);
            CS.String_Writer_Int(bw,FileEntryOffset);
            var pos_StringTable = fs.Position;
            CS.Null_Writer_Int32(bw,2);
            CS.String_Writer_Int(bw,AllFileCount);
            CS.String_Writer_Int(bw,dirindirOne);
            CS.Null_Writer_Int32(bw);


            var dirandfilearray = dirstrs.Concat(filestrs).ToArray();
            var sortedDirFileArray = dirandfilearray.OrderBy(sort=>sort);
            var a= sortedDirFileArray.Select(g => g +(char)0);
            var b = a.ToArray();
            List<int> nameoffset = new List<int>();
            List<string> nameffsetstr = new List<string>();

            Console.WriteLine("//tes//");
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw2 = new BinaryWriter(ms);
            CS.String_Writer(bw2, "." + (char)0);
            CS.String_Writer(bw2,".."+(char)0);
            foreach (var tes in b) {
                var testes = Path.GetFileName(tes);
                nameoffset.Add((int)ms.Position);
                nameffsetstr.Add(testes);
                CS.String_Writer(bw2,testes);
                Console.WriteLine(testes+"testes");
            }

            

            Console.WriteLine("//tesend//");

            //Node Section
            var indexcount = 0;
            var nodeitemcounter = 0;
            for (var i = 0; i < dirstrs.Count(); i++) {
                var dirName = Path.GetFileName(dirstrs[i]);
                var olddirName = dirName;
                if (i==0) {
                    CS.String_Writer(bw,"ROOT");
                    
                }
                else {
                    

                    var shortageCount = 4 - dirName.Count();

                    if (shortageCount > -1) {
                        for(var j = 0; j<shortageCount; j++)
                        dirName += (char)0x20;
                    } 
                    var dirShortName = dirName.Substring(0,4);
                    CS.String_Writer(bw,dirShortName.ToUpper());
                }
                var findnameindex = nameffsetstr.IndexOf(olddirName+(char)0,indexcount);
                indexcount = findnameindex +1;
                Console.WriteLine(findnameindex.ToString("X")+"TESTESTES");
                CS.String_Writer_Int(bw, nameoffset[findnameindex]);
                CS.String_Writer_Int(bw, (short)CS.ARC_Hash(olddirName));
                CS.String_Writer_Int(bw,(short)IDIC[i]);
                
                CS.String_Writer_Int(bw,nodeitemcounter);
                nodeitemcounter += IDIC[i];
            }

            //パディングを挿入した際に0x20で割り切れない場合の計算
            if (fs.Position % 32f != 0)
            {
                bool flag = true;
                while (flag)
                {
                    if (fs.Position % 32f == 0) break;
                    CS.String_Writer_Int(bw, (byte)0);
                }
            }

            Console.WriteLine(CS.ARC_Hash(".."));
            Console.WriteLine("RARC_End");

            //fs,bw終了処理
            fs.Close();
            bw.Close();
        }

        public static List<int> In_DirectoryItemCount(string[] DirStrs) {
            List<int> item = new List<int>();
            foreach (var diritem in DirStrs)
            {
                var dirindir = Directory.GetDirectories(diritem, "*", SearchOption.TopDirectoryOnly);
                var dirinfile = Directory.GetFiles(diritem, "*", SearchOption.TopDirectoryOnly);
                var dirinitemCount = dirindir.Count() + dirinfile.Count() + 2;
                Console.WriteLine(dirinitemCount);
                item.Add(dirinitemCount);
            }
            return item;
        }
        
    }
}
