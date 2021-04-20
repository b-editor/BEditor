﻿using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;

using BEditor.Data;
using BEditor.Data.Primitive;
using BEditor.Data.Property;
using BEditor.Drawing;
using BEditor.Drawing.Pixel;
using BEditor.Primitive.Resources;

using Reactive.Bindings;

namespace BEditor.Primitive.Objects
{
    /// <summary>
    /// Represents an <see cref="ImageObject"/> that references an image file.
    /// </summary>
    public sealed class ImageFile : ImageObject
    {
        /// <summary>
        /// Defines the <see cref="File"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<ImageFile, FileProperty> FileProperty = EditingProperty.RegisterSerializeDirect<FileProperty, ImageFile>(
            nameof(File),
            owner => owner.File,
            (owner, obj) => owner.File = obj,
            new FilePropertyMetadata(Strings.File, "", new(Strings.ImageFile, new FileExtension[]
            {
                new("png"),
                new("jpeg"),
                new("jpg"),
                new("bmp"),
            })));

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFile"/> class.
        /// </summary>
#pragma warning disable CS8618
        public ImageFile()
#pragma warning restore CS8618
        {
        }

        /// <inheritdoc/>
        public override string Name => Strings.Image;

        /// <inheritdoc/>
        public override IEnumerable<PropertyElement> Properties
        {
            get
            {
                yield return Coordinate;
                yield return Scale;
                yield return Blend;
                yield return Rotate;
                yield return Material;
                yield return File;
            }
        }

        /// <summary>
        /// Get the <see cref="FileProperty"/> to select the image file to reference.
        /// </summary>
        public FileProperty File { get; private set; }

        private ReactiveProperty<Image<BGRA32>?>? Source { get; set; }

        /// <inheritdoc/>
        protected override Image<BGRA32>? OnRender(EffectRenderArgs args)
        {
            return Source?.Value?.Clone();
        }

        /// <inheritdoc/>
        protected override void OnLoad()
        {
            base.OnLoad();

            Source = File.Where(file => System.IO.File.Exists(file))
                .Select(f =>
                {
                    Source?.Value?.Dispose();

                    using var stream = new FileStream(f, FileMode.Open);
                    return Image.Decode(stream);
                })
                .ToReactiveProperty();

        }

        /// <inheritdoc/>
        protected override void OnUnload()
        {
            base.OnUnload();

            Source?.Value?.Dispose();
            Source?.Dispose();
        }
    }
}