﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BEditor.Data;
using BEditor.Data.Primitive;
using BEditor.Data.Property;
using BEditor.Data.Property.PrimitiveGroup;
using BEditor.Graphics;
using BEditor.Primitive.Resources;

using GLColor = OpenTK.Mathematics.Color4;
using Material = BEditor.Data.Property.PrimitiveGroup.Material;

namespace BEditor.Primitive.Objects
{
    /// <summary>
    /// Represents an <see cref="ObjectElement"/> that draws a Cube, Ball, etc.
    /// </summary>
    public sealed class GL3DObject : ObjectElement
    {
        /// <summary>
        /// Defines the <see cref="Coordinate"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<GL3DObject, Coordinate> CoordinateProperty = ImageObject.CoordinateProperty.WithOwner<GL3DObject>(
            owner => owner.Coordinate,
            (owner, obj) => owner.Coordinate = obj);

        /// <summary>
        /// Defines the <see cref="Data.Property.PrimitiveGroup.Scale"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<GL3DObject, Scale> ScaleProperty = ImageObject.ScaleProperty.WithOwner<GL3DObject>(
            owner => owner.Scale,
            (owner, obj) => owner.Scale = obj);

        /// <summary>
        /// Defines the <see cref="Blend"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<GL3DObject, Blend> BlendProperty = ImageObject.BlendProperty.WithOwner<GL3DObject>(
            owner => owner.Blend,
            (owner, obj) => owner.Blend = obj);

        /// <summary>
        /// Defines the <see cref="Data.Property.PrimitiveGroup.Rotate"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<GL3DObject, Rotate> RotateProperty = ImageObject.RotateProperty.WithOwner<GL3DObject>(
            owner => owner.Rotate,
            (owner, obj) => owner.Rotate = obj);

        /// <summary>
        /// Defines the <see cref="Data.Property.PrimitiveGroup.Rotate"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<GL3DObject, Material> MaterialProperty = ImageObject.MaterialProperty.WithOwner<GL3DObject>(
            owner => owner.Material,
            (owner, obj) => owner.Material = obj);

        /// <summary>
        /// Defines the <see cref="Type"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<GL3DObject, SelectorProperty> TypeProperty = EditingProperty.RegisterSerializeDirect<SelectorProperty, GL3DObject>(
            nameof(Type),
            owner => owner.Type,
            (owner, obj) => owner.Type = obj,
            new SelectorPropertyMetadata(Strings.Type, new[]
            {
                Strings.Cube,
                Strings.Ball
            }));

        /// <summary>
        /// Defines the <see cref="Width"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<GL3DObject, EaseProperty> WidthProperty = EditingProperty.RegisterSerializeDirect<EaseProperty, GL3DObject>(
            nameof(Width),
            owner => owner.Width,
            (owner, obj) => owner.Width = obj,
            new EasePropertyMetadata(Strings.Width, 100, Min: 0));

        /// <summary>
        /// Defines the <see cref="Height"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<GL3DObject, EaseProperty> HeightProperty = EditingProperty.RegisterSerializeDirect<EaseProperty, GL3DObject>(
            nameof(Height),
            owner => owner.Height,
            (owner, obj) => owner.Height = obj,
            new EasePropertyMetadata(Strings.Height, 100, Min: 0));

        /// <summary>
        /// Defines the <see cref="Depth"/> property.
        /// </summary>
        public static readonly DirectEditingProperty<GL3DObject, EaseProperty> DepthProperty = EditingProperty.RegisterSerializeDirect<EaseProperty, GL3DObject>(
            nameof(Depth),
            owner => owner.Depth,
            (owner, obj) => owner.Depth = obj,
            new EasePropertyMetadata(Strings.Depth, 100, Min: 0));

        /// <summary>
        /// Initializes a new instance of the <see cref="GL3DObject"/> class.
        /// </summary>
        public GL3DObject()
        {
        }

        /// <inheritdoc/>
        public override string Name => Strings.GL3DObject;

        /// <summary>
        /// Gets the coordinates.
        /// </summary>
        [AllowNull]
        public Coordinate Coordinate { get; private set; }

        /// <summary>
        /// Gets the scale.
        /// </summary>
        [AllowNull]
        public Scale Scale { get; private set; }

        /// <summary>
        /// Gets the blend.
        /// </summary>
        [AllowNull]
        public Blend Blend { get; private set; }

        /// <summary>
        /// Gets the angle.
        /// </summary>
        [AllowNull]
        public Rotate Rotate { get; private set; }

        /// <summary>
        /// Gets the material.
        /// </summary>
        [AllowNull]
        public Material Material { get; private set; }

        /// <summary>
        /// Gets the <see cref="SelectorProperty"/> to select the object type.
        /// </summary>
        [AllowNull]
        public SelectorProperty Type { get; private set; }

        /// <summary>
        /// Gets the width of the object.
        /// </summary>
        [AllowNull]
        public EaseProperty Width { get; private set; }

        /// <summary>
        /// Gets the height of the object.
        /// </summary>
        [AllowNull]
        public EaseProperty Height { get; private set; }

        /// <summary>
        /// Gets the depth of the object.
        /// </summary>
        [AllowNull]
        public EaseProperty Depth { get; private set; }

        /// <inheritdoc/>
        public override void Apply(EffectApplyArgs args)
        {
            int frame = args.Frame;
            var color = Blend.Color[frame];
            var color4 = Drawing.Color.FromARGB((byte)(color.A * (Blend.Opacity[frame] / 100)), color.R, color.G, color.B);


            float scale = (float)(Scale.Scale1[frame] / 100);
            float scalex = (float)(Scale.ScaleX[frame] / 100) * scale;
            float scaley = (float)(Scale.ScaleY[frame] / 100) * scale;
            float scalez = (float)(Scale.ScaleZ[frame] / 100) * scale;

            var material = new Graphics.Material(Material.Ambient[frame], Material.Diffuse[frame], Material.Specular[frame], Material.Shininess[frame]);
            var trans = new Transform(
                new(Coordinate.X[frame], Coordinate.Y[frame], Coordinate.Z[frame]),
                new(Coordinate.CenterX[frame], Coordinate.CenterY[frame], Coordinate.CenterZ[frame]),
                new(Rotate.RotateX[frame], Rotate.RotateY[frame], Rotate.RotateZ[frame]),
                new(scalex, scaley, scalez));

            if (Type.Index == 0)
            {
                using var cube = new Cube(
                    Width[frame],
                    Height[frame],
                    Depth[frame],
                    color4,
                    material,
                    trans);

                Parent.Parent.GraphicsContext!.DrawCube(cube);
            }
            else
            {
                using var ball = new Ball(
                    Width[frame] * 0.5f,
                    Height[frame] * 0.5f,
                    Depth[frame] * 0.5f,
                    color4,
                    material,
                    trans);

                Parent.Parent.GraphicsContext!.DrawBall(ball);
            }

            ResetOptional();
        }

        /// <inheritdoc/>
        public override IEnumerable<PropertyElement> GetProperties()
        {
            yield return Coordinate;
            yield return Scale;
            yield return Blend;
            yield return Rotate;
            yield return Material;
            yield return Type;
            yield return Width;
            yield return Height;
            yield return Depth;
        }

        private void ResetOptional()
        {
            Coordinate.ResetOptional();
            Rotate.ResetOptional();
            Scale.ResetOptional();
            Blend.ResetOptional();
        }
    }
}