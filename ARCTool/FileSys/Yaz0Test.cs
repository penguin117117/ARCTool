using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Reflection;


namespace ARCTool.FileSys
{
    public class AppExecuter
    {

        public static void Start(string filePath) 
        {
            var newYaz0Path = Path.ChangeExtension(filePath,".arc");

            Assembly myAssembly = Assembly.GetEntryAssembly();
            string path = myAssembly.Location;
            var appDirPath = Directory.GetCurrentDirectory();

            appDirPath = Path.Combine(appDirPath, "Resources");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = appDirPath+@"\yaz0enc.exe",
                WorkingDirectory = appDirPath,
                Arguments = filePath,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            //プロセス実行
            Process proc = Process.Start(psi);
            string ErrorCMD = proc.StandardError.ReadToEnd();
            string OutputCMD = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            //エラーが出たかを確認
            if (proc.ExitCode != 0)
            {
                throw new Exception("Yaz0のエンコードで問題が発生しました。");
            }

            if (File.Exists(filePath + ".yaz0"))
                File.Copy(filePath + ".yaz0", newYaz0Path, true);
            else
                throw new Exception("rarcファイルがありません");
        }

    }

    public class Yaz0Test
    {
        private string _yaz0FullPath;
        private byte[] _originData;
        private int _originDataSize;

        public Yaz0Test() 
        {
        
        }

        public void Encode(string rarcFullPath) 
        {
            _yaz0FullPath = Path.ChangeExtension (rarcFullPath,".arc");
            _originData   = File.ReadAllBytes    (rarcFullPath);
            _originDataSize = _originData.Length;

            using (MemoryStream ms = new MemoryStream()) 
            {
                using (BinaryWriter bw = new BinaryWriter(ms)) 
                {
                    HeaderWrite(bw,_originDataSize);




                }
            }
            
        }

        private void HeaderWrite(BinaryWriter bw,int rarcSize) 
        {
            bw.Write("Yaz0");
            bw.Write(rarcSize);
            bw.Write(0x00000000);
            bw.Write(0x00000000);
        }

        private void EncodeStart(BinaryWriter bw) 
        {
            

            using (MemoryStream ms = new MemoryStream(_originData)) 
            {
                using (BinaryReader br = new BinaryReader(ms)) 
                {
                
                }
            }
        }

        private void EncodeMain(BinaryWriter bw, BinaryReader br) 
        {
            var pos_StartYaz0Data = bw.BaseStream.Position;

        }
    }
}
