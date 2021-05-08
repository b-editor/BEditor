﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BEditor.Data;
using BEditor.Data.Primitive;
using BEditor.Data.Property;
using BEditor.Drawing;
using BEditor.Drawing.Pixel;
using BEditor.Primitive.Resources;

namespace BEditor.Primitive.Effects
{
    /// <summary>
    /// Represents the <see cref="ImageEffect"/> that corrects the brightness of an image.
    /// </summary>
    public sealed class BrightnessCorrection : ImageEffect
    {
        /// <summary>
        /// Dedines the <see cref="Brightness"/> property.
        /// </summary>
        public static readonly EditingProperty<EaseProperty> BrightnessProperty = EditingProperty.RegisterSerializeDirect<EaseProperty, BrightnessCorrection>(
            nameof(Brightness),
            owner => owner.Brightness,
            (owner, obj) => owner.Brightness = obj,
            new EasePropertyMetadata(Strings.Brightness, 0, 255, -255));

        /// <summary>
        /// Initializes a new instance of the <see cref="BrightnessCorrection"/> class.
        /// </summary>
        public BrightnessCorrection()
        {
        }

        /// <inheritdoc/>
        public override string Name => Strings.BrightnessCorrection;

        /// <summary>
        /// Gets the brightness.
        /// </summary>
        [AllowNull]
        public EaseProperty Brightness { get; set; }

        /// <inheritdoc/>
        public override void Apply(EffectApplyArgs<Image<BGRA32>> args)
        {
            args.Value.Brightness((short)Brightness[args.Frame], Parent.Parent.DrawingContext);
        }

        /// <inheritdoc/>
        public override IEnumerable<PropertyElement> GetProperties()
        {
            yield return Brightness;
        }
    }
}