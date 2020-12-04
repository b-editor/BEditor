﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Runtime.Serialization;

using BEditor.Core.Command;
using BEditor.Core.Data.Primitive.Properties;
using BEditor.Core.Data.Property;
using BEditor.Core.Properties;
using BEditor.Drawing;

namespace BEditor.Core.Data.Primitive.Objects.PrimitiveImages
{
    [DataContract(Namespace = "")]
    public class Image : ImageObject
    {
        public static readonly FilePropertyMetadata FileMetadata = new(Resources.File, "", "png,jpeg,jpg,bmp", Resources.ImageFile);
        private Image<BGRA32> source;

        public Image() => File = new(FileMetadata);


        #region Properties
        public override IEnumerable<PropertyElement> Properties => new PropertyElement[]
        {
            Coordinate,
            Zoom,
            Blend,
            Angle,
            Material,
            File
        };


        [DataMember(Order = 0)]
        public FileProperty File { get; private set; }

        public Image<BGRA32> Source
        {
            get
            {
                if (source == null && System.IO.File.Exists(File.File))
                {
                    source = Drawing.Image.FromFile(File.File);
                }

                return source;
            }
            set => source = value;
        }

        #endregion


        public override Image<BGRA32> OnRender(EffectRenderArgs args) => Source?.Clone();

        public override void PropertyLoaded()
        {
            base.PropertyLoaded();
            File.ExecuteLoaded(FileMetadata);

            File.Subscribe(filename =>
            {
                if (System.IO.File.Exists(filename))
                {
                    source = Drawing.Image.FromFile(filename);
                }
            });
        }
    }
}
