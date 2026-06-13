using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal unsafe class Yaz0Encode
{
    private const int Byte2_Additional_Length = 0x2;
    private const int Byte2_Max_Dictionary_Length = (0xF000 >> 12) + Byte2_Additional_Length;

    private const int Byte3_Additional_Length = Byte2_Max_Dictionary_Length + 1;
    private const int Byte3_Max_Dictionary_Length = 0x0000FF + Byte3_Additional_Length;

    private const int Max_Dictionary_Offset = 0x0FFF;

    private struct Chain
    {
        private const int prev_Size = 0x100_0000;
        public const int key_size = Byte2_Additional_Length + 1;

        // int = 32 ビットを基準とします。
        private int[] toPrevPos;
        private int[] prevPosChain;
        private int lastUpdatePos;

        public Chain(int srcSize)
        {
            toPrevPos = new int[prev_Size];
            prevPosChain = new int[srcSize];
            lastUpdatePos = 0;

            for (int i = 0; i < prev_Size; i++)
            {
                toPrevPos[i] = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GenKey(byte* src, int srcSize, int srcPos)
        {
            int retValue = 0;
            if (srcSize - srcPos < key_size)
                return prev_Size;

            for (int i = 0; i < key_size; i++)
                retValue |= src[srcPos + i] << (8 * i);
            return retValue;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPos(byte* src, int srcSize, int srcPos)
        {
            var key = GenKey(src, srcSize, lastUpdatePos);
            if (key >= prev_Size)
                return 0;
            return toPrevPos[key];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPrev(int pos)
        {
            return prevPosChain[pos];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateChain(byte* src, int srcSize, int srcPos)
        {
            for (; lastUpdatePos < srcPos; lastUpdatePos++)
            {
                int key = GenKey(src, srcSize, lastUpdatePos);
                if (key >= prev_Size)
                    continue;
                prevPosChain[lastUpdatePos] = toPrevPos[key];
                toPrevPos[key] = lastUpdatePos;
            }
        }
    }

    private struct Unit
    {
        public int Length;
        public int Offset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int CompressSize()
        {
            if (Length >= Byte3_Additional_Length)
                return 0x03;

            if (Length > Byte2_Additional_Length)
                return 0x02;

            return 0x01;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Score(int diffPos)
        {
            Debug.Assert(Length > 0);
            return Length - CompressSize() - diffPos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Write(int* flag, int writeFlagCount, byte* dst, int* dstPos, byte* src, in int srcPos)
        {
            if (Length > Byte2_Additional_Length)
            {
                dst[*dstPos + 0x01] = (byte)(0xFF & Offset);

                int length = 0;
                if (Length >= Byte3_Additional_Length)
                {
                    dst[*dstPos] = (byte)(0x0F & (Offset >> 0x08));
                    length = Length - Byte3_Additional_Length;
                    dst[*dstPos + 0x02] = (byte)length;
                    *dstPos += 0x03;
                    return Length;
                }

                length = Length - Byte2_Additional_Length;
                dst[*dstPos] = (byte)((0xF0 & (length << 0x04)) | (0x0F & (Offset >> 0x08)));
                *dstPos += 0x02;
                return Length;
            }

            *flag |= 0b1000_0000 >> writeFlagCount;
            dst[*dstPos] = src[srcPos];
            *dstPos += 1;
            return 0x01;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(ref Chain chain, byte* src, int srcSize, int srcPos)
        {
            chain.UpdateChain(src, srcSize, srcPos);

            Length = 2;
            Offset = 0;

            if (srcPos >= srcSize) return;

            int maxCompSize = (srcSize - srcPos < Byte3_Max_Dictionary_Length)
                                   ? srcSize - srcPos
                                   : Byte3_Max_Dictionary_Length;

            // pos - 1でoffsetが0の時の位置を計算。
            // *Max_Dictionary_Offset* > (Max_Dictionary_Offset + 1) 
            // となるように調整。
            int startDictPos = (srcPos > (Max_Dictionary_Offset - 1))
                                    ? (srcPos - 1) - (Max_Dictionary_Offset - 1)
                                    : 0;

            for (int dictPos = chain.GetPos(src, srcSize, srcPos); dictPos > startDictPos; dictPos = chain.GetPrev(dictPos))
            {
                // 早期リジェクション：現在見つかっている最大長の先の文字が一致しないならパス
                if (src[dictPos + Length - 1] != src[srcPos + Length - 1])
                    continue;

                // keyの関係上すでにkey_size分は合っている。
                int length = Chain.key_size;

                while (length < maxCompSize && src[dictPos + length] == src[srcPos + length])
                {
                    length++;
                }

                if (length > Length)
                {
                    Length = length;
                    Offset = dictPos;
                }
            }

            // ここで「相対距離」を計算して保持する（0から始まる値、1文字前なら0）
            Offset = (srcPos - 1) - Offset;
        }

    }


    private unsafe int Encode(byte* dst, byte* src, int size)
    {
        int dstPos = 0;
        int srcPos = 0;

        int flag = 0x00;
        int flagPos = dstPos;
        int writeFlagCount = 7;

        Chain chain = new(size);
        Unit currentUnit = default;
        Unit nextUnit = default;
        bool isNextUnitValid = false;

        while (srcPos < size)
        {
            writeFlagCount++;
            if (writeFlagCount == 8)
            {
                dst[flagPos] = (byte)flag;
                flag = 0x00;
                flagPos = dstPos;
                dstPos++;
                writeFlagCount = 0;
            }

            if (isNextUnitValid)
            {
                (currentUnit, nextUnit) = (nextUnit, currentUnit);
            }
            else
            {
                currentUnit.Init(ref chain, src, size, srcPos);
            }

            nextUnit.Init(ref chain, src, size, srcPos + 1);

            isNextUnitValid = nextUnit.Score(1) > currentUnit.Score(0);
            if (isNextUnitValid)
            {
                flag |= 0b1000_0000 >> writeFlagCount;
                dst[dstPos] = src[srcPos];
                dstPos++;
                srcPos++;
                continue;
            }

            srcPos += currentUnit.Write(&flag, writeFlagCount, dst, &dstPos, src, srcPos);
        }

        dst[flagPos] = (byte)flag;

        return dstPos;
    }

    public void Encode(BinaryWriter bw, BinaryReader br)
    {
        int srcLength = (int)br.BaseStream.Length;
        if (srcLength == 0) return;

        var src = new byte[srcLength];
        var dst = new byte[srcLength * 2]; // 念のためのオーバーフロー防止
        br.Read(src);

        int dstSize = 0;

        fixed (byte* pDst = dst, pSrc = src)
            dstSize = Encode(pDst, pSrc, srcLength);

        bw.Write(dst, 0, dstSize);
    }
}