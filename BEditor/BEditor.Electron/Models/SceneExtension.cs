﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BEditor.Core.Data;

namespace BEditor.Models
{
    public static class SceneExtension
    {
        const int width = 10;
        public static double ToPixel(this Scene self, int number)
            => width * (self.TimeLineZoom / 200) * number;
        public static int ToFrame(this Scene self, double pixel)
            => (int)(pixel / (width * (self.TimeLineZoom / 200)));
    }
}
