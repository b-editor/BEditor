﻿using System;
using System.Runtime.InteropServices;

namespace BEditor.Media.PCM
{
    /// <summary>
    /// Represents the audio data in 32-bit float format.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct PCMFloat : IPCM<PCMFloat>, IPCMConvertable<PCM16>, IPCMConvertable<PCM32>
    {
        /// <summary>
        /// Represents the audio data.
        /// </summary>
        public float Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="PCMFloat"/> class.
        /// </summary>
        /// <param name="value">The audio data.</param>
        public PCMFloat(float value)
        {
            Value = value;
        }

        /// <inheritdoc/>
        public PCMFloat Add(PCMFloat s)
        {
            return new(Value + s.Value);
        }

        /// <inheritdoc/>
        public void ConvertFrom(PCM16 src)
        {
            src.ConvertTo(out this);
        }

        /// <inheritdoc/>
        public void ConvertFrom(PCM32 src)
        {
            src.ConvertTo(out this);
        }

        /// <inheritdoc/>
        public void ConvertTo(out PCM16 dst)
        {
            dst = new((short)(Value * short.MaxValue));
        }

        /// <inheritdoc/>
        public void ConvertTo(out PCM32 dst)
        {
            dst = new((int)(Value * int.MaxValue));
        }

        /// <summary>
        /// Converts the <see cref="PCMFloat"/> to a 32-bit float.
        /// </summary>
        /// <param name="value">The Pcm data.</param>
        public static implicit operator float(PCMFloat value)
        {
            return value.Value;
        }

        /// <summary>
        /// Converts the 32-bit float to a <see cref="PCMFloat"/>.
        /// </summary>
        /// <param name="value">The 32-bit float.</param>
        public static implicit operator PCMFloat(float value)
        {
            return new(value);
        }
    }
}