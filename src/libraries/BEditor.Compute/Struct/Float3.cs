﻿// Float3.cs
//
// Copyright (C) BEditor
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.

using System.Runtime.InteropServices;

namespace BEditor.Compute
{
    /// <summary>
    /// A vector of 3 32-bit floating-point values.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Float3
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Float3"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        public Float3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Float3"/> struct.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="z">The z.</param>
        public Float3(Float2 vector, float z)
        {
            X = vector.X;
            Y = vector.Y;
            Z = z;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Float3"/> struct.
        /// </summary>
        /// <param name="vector">The vector.</param>
        public Float3(Float3 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Float3"/> struct.
        /// </summary>
        /// <param name="vector">The vector.</param>
        public Float3(Float4 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Gets or sets the z.
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// Gets or sets the xy.
        /// </summary>
        public Float2 XY
        {
            get => new(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the xyz.
        /// </summary>
        public Float3 XYZ
        {
            get => this;
            set => this = value;
        }
    }
}
