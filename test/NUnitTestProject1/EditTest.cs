﻿//#define IsEnabled

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BEditor.Command;
using BEditor.Data;
using BEditor.Data.Property;
using BEditor.Drawing;
using BEditor.Primitive;

using NUnit.Framework;

namespace NUnitTestProject1
{
    public class EditTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test()
        {
#if IsEnabled
            using var project = new Project(1920, 1080, 60);
            using var stream = new MemoryStream();
            project.Load();
            var scene = project.PreviewScene;

            scene.AddClip(1, 1, PrimitiveTypes.FigureMetadata, out _).Execute();

            var result = scene.Render(1);

            result.Image.Encode(stream, EncodedImageFormat.Png);
#endif
        }
    }
}