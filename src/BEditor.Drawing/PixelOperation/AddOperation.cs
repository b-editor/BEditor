﻿
using BEditor.Drawing.Pixel;

namespace BEditor.Drawing.PixelOperation
{
    public readonly unsafe struct AddOperation<T> : IPixelOperation
        where T : unmanaged, IPixel<T>
    {
        private readonly T* _dst;
        private readonly T* _src;
        private readonly T* _mask;

        public AddOperation(T* src, T* dst, T* mask)
        {
            _dst = dst;
            _src = src;
            _mask = mask;
        }

        public readonly void Invoke(int pos)
        {
            _dst[pos] = _src[pos].Add(_mask[pos]);
        }
    }
}