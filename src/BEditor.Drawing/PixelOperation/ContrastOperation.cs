﻿
using BEditor.Drawing.Pixel;

namespace BEditor.Drawing.PixelOperation
{
    public readonly unsafe struct ContrastOperation : IPixelOperation
    {
        private readonly BGRA32* _src;
        private readonly BGRA32* _dst;
        private readonly byte* _lut;

        public ContrastOperation(BGRA32* src, BGRA32* dst, byte* lut)
        {
            _src = src;
            _dst = dst;
            _lut = lut;
        }

        public readonly void Invoke(int pos)
        {
            _dst[pos].B = _lut[_src[pos].B];
            _dst[pos].G = _lut[_src[pos].G];
            _dst[pos].R = _lut[_src[pos].R];
            _dst[pos].A = _src[pos].A;
        }
    }
}