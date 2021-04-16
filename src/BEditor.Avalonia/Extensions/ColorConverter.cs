﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEditor.Extensions
{
    public static class ColorConverter
    {
        public static Avalonia.Media.Color ToAvalonia(this Drawing.Color color)
        {
            return Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        public static Drawing.Color ToDrawing(this Avalonia.Media.Color color)
        {
            return Drawing.Color.FromARGB(color.A, color.R, color.G, color.B);
        }
    }
}