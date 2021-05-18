﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEditor.Extensions.FFmpeg
{
    internal class FFmpegExecutable
    {


        public static string GetExecutable()
        {
            if (OperatingSystem.IsWindows()) return Path.Combine(AppContext.BaseDirectory, "user", "plugins", "BEditor.Extensions.FFmpeg", "ffmpeg", "ffmpeg.exe");
            else if (OperatingSystem.IsLinux()) return "/usr/bin/ffmpeg";
            else if (OperatingSystem.IsMacOS()) return "/usr/local/opt/ffmpeg";
            else throw new PlatformNotSupportedException();
        }
    }
}