using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace ARCTool.FileSys
{
    class Calculation_System
    {
        public static string Byte2Char(BinaryReader br, int readchers = 4)
        {
            return new string(br.ReadChars(readchers));
        }

        public static int Byte2Int(BinaryReader br, int readbyte = 4)
        {
            return Int32.Parse(BitConverter.ToString(br.ReadBytes(readbyte), 0).Replace("-", "").PadLeft(readbyte, '0'), NumberStyles.HexNumber);
        }

        public static Int16 Byte2Short(BinaryReader br, int readbyte = 2)
        {
            return Int16.Parse(BitConverter.ToString(br.ReadBytes(readbyte), 0).Replace("-", "").PadLeft(readbyte, '0'), NumberStyles.HexNumber);
        }

        public static UInt16 Byte2UShort(BinaryReader br, int readbyte = 2)
        {
            return UInt16.Parse(BitConverter.ToString(br.ReadBytes(readbyte), 0).Replace("-", "").PadLeft(readbyte, '0'), NumberStyles.HexNumber);
        }

        public static byte Bytes2Byte(BinaryReader br)
        {
            return br.ReadByte();
        }

        public static byte[] StringToBytes(string str)
        {
            var bs = new List<byte>();
            for (int i = 0; i < str.Length / 2; i++)
            {
                bs.Add(Convert.ToByte(str.Substring(i * 2, 2), 16));
            }
            return bs.ToArray();
        }

        public static string Bytes2String_NullEnd(FileStream fs, BinaryReader br , long nameoffset) {
            var oldpos = fs.Position;

            fs.Seek((long)nameoffset, SeekOrigin.Begin);

            bool flag = true;

            List<byte> bits = new List<byte>();
            while (flag) {
                var bit = Bytes2Byte(br);
                bits.Add(bit);
                //Console.WriteLine(bit.ToString("X"));
                if (bit == 0x00) flag = false;
            }
            //Console.WriteLine("testes");

            fs.Seek(oldpos, SeekOrigin.Begin);

            var bitarray = bits.ToArray();
            //var bitreverse = bitarray.Reverse();
            //var bitarray2 = bitreverse.ToArray();
            //Console.WriteLine(Encoding.ASCII.GetString(bitarray));
            var str = Encoding.GetEncoding(65001).GetString(bitarray) ;
            //Console.WriteLine(str.Substring(0 , str.Count()-1));
            //Console.WriteLine(oldpos.ToString("X")+" : "+str);
            //Console.WriteLine(fs.Position.ToString("X"));

            string ret = str.Substring(0, str.Length - 1);

            //Console.WriteLine(br.BaseStream.Position.ToString("X"));
            return ret;
        }

        /// <summary>
        /// ファイルにint32型のNullデータを書き込みます<br/>
        /// <remarks>Null_Writer_Int32(進めたいバイナリライト、繰り返し回数(int型))</remarks>
        /// </summary>
        /// 
        public static void Null_Writer_Int32(BinaryWriter bw, int write_rep_num = 1)
        {

            //0以上または、int型の最大値までしか選択できないようにする。
            if (write_rep_num < 1) write_rep_num = 1;
            if (Int32.MaxValue < write_rep_num) write_rep_num = Int32.MaxValue;

            for (int i = 0; i < write_rep_num; i++)
                bw.Write(BitConverter.GetBytes(0x00000000));
        }

        public static void String_Writer(BinaryWriter bw , string str , string encoding_type = "ASCII") {
            var bits = Encoding.GetEncoding(encoding_type).GetBytes(str);
            bw.Write(bits);
        }

        public static void String_Writer_Int(BinaryWriter bw,int hexnum) {
            var hexstr = hexnum.ToString("X8");
            var bytes  = StringToBytes(hexstr);
            bw.Write(bytes);
        }

        public static void String_Writer_Int(BinaryWriter bw, Int16 hexnum)
        {
            var hexstr = hexnum.ToString("X4");
            var bytes = StringToBytes(hexstr);
            bw.Write(bytes);
        }

        public static void String_Writer_Int(BinaryWriter bw, UInt16 hexnum)
        {
            var hexstr = hexnum.ToString("X4");
            var bytes = StringToBytes(hexstr);
            bw.Write(bytes);
        }

        public static void String_Writer_Int(BinaryWriter bw, byte hexnum)
        {
            var hexstr = hexnum.ToString("X2");
            var bytes = StringToBytes(hexstr);
            bw.Write(bytes);
        }

        public static UInt16 ARC_Hash(string strs) {
            UInt16 hashvalue = 0;
            foreach (var ch in strs) {
                hashvalue *= 3;
                hashvalue += ch;
            }
            return hashvalue;
        }

        public static UInt16 ARC_Hash2(string strs , ushort length)
        {
            UInt16 hashvalue = 0;
            foreach (var ch in strs)
            {
                hashvalue *=length;
                hashvalue += ch;
            }
            return hashvalue;
        }

        public static uint MSBT_Hash(string label, int num_slots)
        {
            uint hash = 0;
            foreach (char c in label)
            {
                hash *= 0x492;
                hash += c;
            }
            return (hash & 0xFFFFFFFF) % (uint)num_slots;
        }


    }
}
