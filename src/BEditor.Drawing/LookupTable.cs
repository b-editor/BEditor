﻿// LookupTable.cs
//
// Copyright (C) BEditor
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BEditor.Drawing.Pixel;
using BEditor.Drawing.PixelOperation;

namespace BEditor.Drawing
{
    /// <summary>
    /// Specifies the dimension of the lookup table.
    /// </summary>
    public enum LookupTableDimension
    {
        /// <summary>
        /// The lookup table is 1D.
        /// </summary>
        OneDimension = 1,

        /// <summary>
        /// The lookup table is 3D.
        /// </summary>
        ThreeDimension = 3,
    }

    /// <summary>
    /// Represents a lookup table.
    /// </summary>
    public sealed unsafe class LookupTable : IDisposable
    {
        private static readonly Regex _lutSizeReg = new("^LUT_(?<dim>.*?)_SIZE (?<size>.*?)$");
        private static readonly Regex _titleReg = new("^TITLE \"(?<text>.*?)\"$");
        private static readonly Regex _domainMinReg = new("^DOMAIN_MIN (?<red>.*?) (?<green>.*?) (?<blue>.*?)$");
        private static readonly Regex _domainMaxReg = new("^DOMAIN_MAX (?<red>.*?) (?<green>.*?) (?<blue>.*?)$");
        private readonly UnmanagedArray<float> _array;

        /// <summary>
        /// Initializes a new instance of the <see cref="LookupTable"/> class.
        /// </summary>
        /// <param name="size">The size of the <see cref="LookupTable"/>.</param>
        /// <param name="dim">The dimension of the <see cref="LookupTable"/>.</param>
        public LookupTable(int size = 256, LookupTableDimension dim = LookupTableDimension.OneDimension)
        {
            _array = new((int)Math.Pow(size, (int)dim) * (int)dim);
            Size = size;
            Dimension = dim;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="LookupTable"/> class.
        /// </summary>
        ~LookupTable()
        {
            Dispose();
        }

        /// <summary>
        /// Gets the size of this <see cref="LookupTable"/>.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets the dimension of this <see cref="LookupTable"/>.
        /// </summary>
        public LookupTableDimension Dimension { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a lookup table to adjust the contrast.
        /// </summary>
        /// <param name="contrast">The contrast [range: -255-255].</param>
        /// <returns>Returns the lookup table created by this method.</returns>
        public static LookupTable Contrast(short contrast)
        {
            contrast = Math.Clamp(contrast, (short)-255, (short)255);
            var table = new LookupTable();

            Image.PixelOperate(256, new ContrastOperation(contrast, (float*)table.GetPointer()));

            return table;
        }

        /// <summary>
        /// Creates a lookup table to adjust the contrast.
        /// </summary>
        /// <param name="gamma">The gamma. [range: 0.01-3.0].</param>
        /// <returns>Returns the lookup table created by this method.</returns>
        public static LookupTable Gamma(float gamma)
        {
            gamma = Math.Clamp(gamma, 0.01f, 3f);
            var table = new LookupTable();

            Image.PixelOperate(256, new GammaOperation(gamma, (float*)table.GetPointer()));

            return table;
        }

        /// <summary>
        /// Creates a lookup table from a Cube file.
        /// </summary>
        /// <param name="stream">The stream to create the lookup table.</param>
        /// <returns>Returns the lookup table created by this method.</returns>
        public static LookupTable FromStream(Stream stream)
        {
            using var reader = new StreamReader(stream);
            var i = 0;
            ReadInfo(reader, out _, out var dim, out var size, out _, out _);

            var table = new LookupTable(size, dim);
            var length = (int)Math.Pow(size, (int)dim);
            var data = (Float3*)table.GetPointer();

            while (i < length)
            {
                var line = reader.ReadLine();
                if (line is not null)
                {
                    var values = line.Split(' ');
                    if (values.Length is not 3) continue;

                    if (float.TryParse(values[0], out var r) &&
                        float.TryParse(values[1], out var g) &&
                        float.TryParse(values[2], out var b))
                    {
                        data[i].R = r;
                        data[i].G = g;
                        data[i].B = b;
                        i++;
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// Creates a lookup table from a Cube file.
        /// </summary>
        /// <param name="file">The Cube file to create the lookup table.</param>
        /// <returns>Returns the lookup table created by this method.</returns>
        public static LookupTable FromCube(string file)
        {
            using var stream = new FileStream(file, FileMode.Open);

            return FromStream(stream);
        }

        /// <summary>
        /// Creates a new span over a lookup table.
        /// </summary>
        /// <returns> The span representation of the lookup table.</returns>
        public Span<float> AsSpan()
        {
            return _array.AsSpan();
        }

        /// <summary>
        /// Gets the pointer.
        /// </summary>
        /// <returns>Returns the pointer.</returns>
        public IntPtr GetPointer()
        {
            return _array.Pointer;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                _array.Dispose();
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }

        private static void ReadInfo(StreamReader reader, out string title, out LookupTableDimension dim, out int size, out Vector3 min, out Vector3 max)
        {
            title = string.Empty;
            dim = LookupTableDimension.ThreeDimension;
            size = 33;
            min = new(0, 0, 0);
            max = new(1, 1, 1);
            var titleFound = false;
            var lutSizeFound = false;
            var minFound = false;
            var maxFound = false;

            while (!reader.EndOfStream)
            {
                if (titleFound && lutSizeFound && minFound && maxFound) break;
                var line = reader.ReadLine();
                if (line is not null)
                {
                    if (_lutSizeReg.IsMatch(line))
                    {
                        lutSizeFound = true;
                        var match = _lutSizeReg.Match(line);
                        size = int.Parse(match.Groups["size"].Value);
                        dim = match.Groups["dim"].Value is "3D" ? LookupTableDimension.ThreeDimension : LookupTableDimension.OneDimension;
                    }
                    else if (_titleReg.IsMatch(line))
                    {
                        titleFound = true;
                        var match = _lutSizeReg.Match(line);
                        title = match.Groups["text"].Value;
                    }
                    else if (_domainMaxReg.IsMatch(line))
                    {
                        maxFound = true;
                        var match = _domainMaxReg.Match(line);
                        var r = float.Parse(match.Groups["red"].Value);
                        var g = float.Parse(match.Groups["green"].Value);
                        var b = float.Parse(match.Groups["blue"].Value);
                        max = new(r, g, b);
                    }
                    else if (_domainMinReg.IsMatch(line))
                    {
                        minFound = true;
                        var match = _domainMinReg.Match(line);
                        var r = float.Parse(match.Groups["red"].Value);
                        var g = float.Parse(match.Groups["green"].Value);
                        var b = float.Parse(match.Groups["blue"].Value);
                        min = new(r, g, b);
                    }
                }
            }

            reader.BaseStream.Position = 0;
        }

        internal struct Float3
        {
            public float R;
            public float G;
            public float B;
        }
    }
}