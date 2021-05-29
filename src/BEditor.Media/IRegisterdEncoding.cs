﻿// IRegisterdEncoding.cs
//
// Copyright (C) BEditor
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.

using System.Collections.Generic;
using System.IO;
using System.Linq;

using BEditor.Media.Encoding;

namespace BEditor.Media
{
    /// <summary>
    /// Provides the ability to create a encoder.
    /// </summary>
    public interface IRegisterdEncoding
    {
        /// <summary>
        /// Gets the decoder name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Create the media file.
        /// </summary>
        /// <param name="file">The file name to output.</param>
        /// <returns>Returns the output container created by this method.</returns>
        public IOutputContainer? Create(string file);

        /// <summary>
        /// Gets the value if the specified media file is supported.
        /// </summary>
        /// <param name="file">File name of the media file.</param>
        /// <returns>Returns a value if the specified media file is supported.</returns>
        public bool IsSupported(string file)
        {
            return SupportExtensions().Contains(Path.GetExtension(file));
        }

        /// <summary>
        /// Gets the supported extensions.
        /// </summary>
        /// <returns>Returns the supported extensions.</returns>
        public IEnumerable<string> SupportExtensions();
    }
}