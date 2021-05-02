﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using BEditor.Data;
using BEditor.Data.Property;

namespace SourceGenerator.Tests
{
    [GenerateTarget]
    public partial class UserClass : EffectElement
    {
        public static readonly DirectEditingProperty<UserClass, ColorProperty> ColorProperty = EditingProperty.RegisterDirect<ColorProperty, UserClass>("Color",
            owner => owner.Color,
            (owner, obj) => owner.Color = obj);
        public static readonly EditingProperty<ColorProperty> ValueProperty = EditingProperty.RegisterSerialize<ColorProperty, UserClass>("Value");

        public override string Name { get; }
        public override IEnumerable<PropertyElement> Properties { get; }

        public override void Render(EffectRenderArgs args)
        {
            _ = Color;
            _ = Value;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
        }
    }
}