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
    /// Represents the <see cref="ImageEffect"/> that expands the area of an image.
    /// </summary>
    public sealed class AreaExpansion : ImageEffect
    {
        /// <summary>
        /// Defines the <see cref="Top"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<AreaExpansion, EaseProperty> TopProperty = Clipping.TopProperty.WithOwner<AreaExpansion>(
            owner => owner.Top,
            (owner, obj) => owner.Top = obj);

        /// <summary>
        /// Defines the <see cref="Bottom"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<AreaExpansion, EaseProperty> BottomProperty = Clipping.BottomProperty.WithOwner<AreaExpansion>(
            owner => owner.Bottom,
            (owner, obj) => owner.Bottom = obj);

        /// <summary>
        /// Defines the <see cref="Left"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<AreaExpansion, EaseProperty> LeftProperty = Clipping.LeftProperty.WithOwner<AreaExpansion>(
            owner => owner.Left,
            (owner, obj) => owner.Left = obj);

        /// <summary>
        /// Defines the <see cref="Right"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<AreaExpansion, EaseProperty> RightProperty = Clipping.RightProperty.WithOwner<AreaExpansion>(
            owner => owner.Right,
            (owner, obj) => owner.Right = obj);

        /// <summary>
        /// Defines the <see cref="AdjustCoordinates"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<AreaExpansion, CheckProperty> AdjustCoordinatesProperty = Clipping.AdjustCoordinatesProperty.WithOwner<AreaExpansion>(
            owner => owner.AdjustCoordinates,
            (owner, obj) => owner.AdjustCoordinates = obj);

        /// <summary>
        /// Initializes a new instance of the <see cref="AreaExpansion"/> class.
        /// </summary>
        public AreaExpansion()
        {
        }

        /// <inheritdoc/>
        public override string Name => Strings.AreaExpansion;

        /// <summary>
        /// Get an <see cref="EaseProperty"/> that represents the number of pixels to add
        /// </summary>
        [AllowNull]
        public EaseProperty Top { get; private set; }

        /// <summary>
        /// Get an <see cref="EaseProperty"/> that represents the number of pixels to add
        /// </summary>
        [AllowNull]
        public EaseProperty Bottom { get; private set; }

        /// <summary>
        /// Get an <see cref="EaseProperty"/> that represents the number of pixels to add
        /// </summary>
        [AllowNull]
        public EaseProperty Left { get; private set; }

        /// <summary>
        /// Get an <see cref="EaseProperty"/> that represents the number of pixels to add
        /// </summary>
        [AllowNull]
        public EaseProperty Right { get; private set; }

        /// <summary>
        /// Get the <see cref="CheckProperty"/> to adjust the coordinates.
        /// </summary>
        [AllowNull]
        public CheckProperty AdjustCoordinates { get; private set; }

        /// <inheritdoc/>
        public override IEnumerable<PropertyElement> GetProperties()
        {
            yield return Top;
            yield return Bottom;
            yield return Left;
            yield return Right;
            yield return AdjustCoordinates;
        }

        /// <inheritdoc/>
        public override void Apply(EffectApplyArgs<Image<BGRA32>> args)
        {
            int top = (int)Top.GetValue(args.Frame);
            int bottom = (int)Bottom.GetValue(args.Frame);
            int left = (int)Left.GetValue(args.Frame);
            int right = (int)Right.GetValue(args.Frame);

            if (AdjustCoordinates.Value && Parent!.Effect[0] is ImageObject image)
            {
                image.Coordinate.CenterX.Optional = (right / 2) - (left / 2);
                image.Coordinate.CenterY.Optional = (top / 2) - (bottom / 2);
            }

            var img = args.Value.MakeBorder(top, bottom, left, right);

            args.Value.Dispose();

            args.Value = img;
        }
    }
}