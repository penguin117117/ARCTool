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

        private static List<node_items> Node_Items;
        private static List<directory_items> Directory_Items;
        
        public static void Read(string rarc_path)
        {
            FileStream fs = new FileStream(rarc_path, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            Node_Items = new List<node_items>();
            //Directory_Items = new List<directory_items>();

            //File Header
            var Magic = CS.Byte2Char(br);
            var FileSize = CS.Byte2Int(br);
            CS.Byte2Int(br);
            var FileDataOffset = CS.Byte2Int(br);
            var FileDataLength01 = CS.Byte2Int(br);
            var FileDataLength02 = CS.Byte2Int(br);
            CS.Byte2Int(br);
            CS.Byte2Int(br);

            //Info セクション
            var NodeNum = CS.Byte2Int(br);
            var FirstNodeOffset = CS.Byte2Int(br);
            var TotalDirectoryCount = CS.Byte2Int(br);
            var FirstDirectoryOffset = CS.Byte2Int(br);
            var StringTableLength = CS.Byte2Int(br);
            var StringTableOffset = CS.Byte2Int(br);
            var DirectoryFileNum = CS.Byte2Short(br);
            CS.Byte2Short(br);
            CS.Byte2Int(br);

            Console.WriteLine("//////////infoend//////////");

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

            //Console.WriteLine(RootFolder);
            DirectoryInfo diSub = di.CreateSubdirectory(RootFolder);

            Console.WriteLine("");
            
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
                    var a = Directory_Items[nodeitemcounter].FileNameOffset + FileEntryEnd;
                    var b = CS.Bytes2String_NullEnd(fs,br,a);
                    //Console.WriteLine(b);
                    nodeitemcounter++;
                }

                Directory_Items.Reverse();
                foreach (var item in Directory_Items.Select((Value, Index) => new { Value, Index })) {
                    var a = Directory_Items[nodeitemcounter].FileNameOffset + FileEntryEnd;
                    var b = CS.Bytes2String_NullEnd(fs, br, a);
                    switch (b) {
                        case "..":
                            break;
                        case ".":
                            break;
                        default:
                            break;
                    }
                }
                Console.WriteLine("");
                Console.WriteLine("");
            }

            //Directory_Items.Reverse();
            //Console.WriteLine((Directory_Items[0].FileOffset_or_DirectoryIndex));

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

        public static void ParentNode(List<directory_items> Directory_Items , DirectoryInfo di) { 
            
        }
        
    }
}
