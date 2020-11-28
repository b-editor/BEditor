﻿using System.Collections.Generic;
using System.Runtime.Serialization;

using BEditor.Core.Command;
using BEditor.Core.Data.Primitive.Properties;
using BEditor.Core.Data.Property;
using BEditor.Core.Extensions;
using BEditor.Core.Media;
using BEditor.Core.Properties;

namespace BEditor.Core.Data.Primitive.Effects.PrimitiveImages
{
    [DataContract(Namespace = "")]
    public class GaussBlur : ImageEffect
    {
        public GaussBlur()
        {
            Size = new(BoxFilter.SizeMetadata);
            Resize = new(BoxFilter.ResizeMetadata);
        }

        public override string Name => Resources.Gauss;
        public override IEnumerable<PropertyElement> Properties => new PropertyElement[]
        {
            Size,
            Resize
        };
        [DataMember(Order = 0)]
        public EaseProperty Size { get; private set; }
        [DataMember(Order = 1)]
        public CheckProperty Resize { get; private set; }

        public override void Render(ref Image source, EffectRenderArgs args) => 
            source.ToRenderable().GaussianBlur((int)Size.GetValue(args.Frame), Resize.IsChecked);
        public override void PropertyLoaded()
        {
            Size.ExecuteLoaded(BoxFilter.SizeMetadata);
            Resize.ExecuteLoaded(BoxFilter.ResizeMetadata);
        }
    }
}