﻿using System;
using System.Collections.Generic;

using BEditor.Data;
using BEditor.Data.Primitive;
using BEditor.Data.Property;
using BEditor.Drawing;
using BEditor.Drawing.Pixel;
using BEditor.Primitive.Resources;

namespace BEditor.Primitive.Effects
{
#pragma warning disable CS1591
    public sealed class Negaposi : ImageEffect
    {
        /// <summary>
        /// Defines the <see cref="Red"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<Negaposi, EaseProperty> RedProperty = EditingProperty.RegisterSerializeDirect<EaseProperty, Negaposi>(
            nameof(Red),
            owner => owner.Red,
            (owner, obj) => owner.Red = obj,
            new EasePropertyMetadata(Strings.Red, 255, 255, 0));

        /// <summary>
        /// Defines the <see cref="Green"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<Negaposi, EaseProperty> GreenProperty = EditingProperty.RegisterSerializeDirect<EaseProperty, Negaposi>(
            nameof(Green),
            owner => owner.Green,
            (owner, obj) => owner.Green = obj,
            new EasePropertyMetadata(Strings.Green, 255, 255, 0));

        /// <summary>
        /// Defines the <see cref="Blue"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<Negaposi, EaseProperty> BlueProperty = EditingProperty.RegisterSerializeDirect<EaseProperty, Negaposi>(
            nameof(Blue),
            owner => owner.Blue,
            (owner, obj) => owner.Blue = obj,
            new EasePropertyMetadata(Strings.Blue, 255, 255, 0));

#pragma warning disable CS8618
        public Negaposi()
#pragma warning restore CS8618
        {
        }

        public override string Name => Strings.Negaposi;
        public override IEnumerable<PropertyElement> Properties => new PropertyElement[]
        {
            Red,
            Green,
            Blue
        };
        public EaseProperty Red { get; set; }
        public EaseProperty Green { get; set; }
        public EaseProperty Blue { get; set; }

        public override void Render(EffectRenderArgs<Image<BGRA32>> args)
        {
            args.Value.Negaposi(
                (byte)Red[args.Frame],
                (byte)Green[args.Frame],
                (byte)Blue[args.Frame],
                Parent.Parent.DrawingContext);
        }
    }
#pragma warning restore CS1591
}