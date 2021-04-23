﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEditor.Compute
{
    public abstract class ComputeObject : IDisposable
    {
        ~ComputeObject()
        {
            Dispose(false);
        }

        public bool IsDisposed { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Dispose(disposing: true);

                IsDisposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}