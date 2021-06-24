﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BEditor.Graphics.Platform;

namespace BEditor.Graphics.Mock
{
    public sealed class MockLineImpl : MockDrawableImpl, ILineImpl
    {
        public float[] Vertices { get; } = Array.Empty<float>();

        public Vector3 Start { get; }

        public Vector3 End { get; }

        public float Width { get; }
    }
}
