﻿using System.IO;
using System.Xml.Linq;

using BEditor.Core.Data;

namespace BEditor.Models
{
    public class Setting
    {
        public static double ClipHeight { get; } = Settings.Default.ClipHeight;
        public static float WidthOf1Frame { get; } = Settings.Default.WidthOf1Frame;
    }
}