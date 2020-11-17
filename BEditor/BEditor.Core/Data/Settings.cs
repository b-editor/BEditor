﻿using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;

namespace BEditor.Core.Data
{
    [DataContract(Namespace = "")]
    public class Settings : BasePropertyChanged, INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs clipHeightArgs = new(nameof(ClipHeight));
        private static readonly PropertyChangedEventArgs darkModeArgs = new(nameof(UseDarkMode));
        private static readonly PropertyChangedEventArgs autoBackUpArgs = new(nameof(AutoBackUp));
        private static readonly PropertyChangedEventArgs lastTimeFolderArgs = new(nameof(LastTimeFolder));
        private static readonly PropertyChangedEventArgs lastTimeNumArgs = new(nameof(LastTimeNum));
        private static readonly PropertyChangedEventArgs widthOf1FrameArgs = new(nameof(WidthOf1Frame));
        private static readonly PropertyChangedEventArgs enableErrorLogArgs = new(nameof(EnableErrorLog));
        private int clipHeight = 25;
        private bool darkMode = true;
        private bool autoBackUp = true;
        private string lastTimeFolder = "";
        private int lastTimeNum = 0;
        private int widthOf1Frame = 5;
        private bool enableErrorLog = false;

        public static Settings Default { get; }

        static Settings()
        {
            var path = $"{Component.Path}\\user\\settings.json";
            if (!File.Exists(path))
            {
                Default = new Settings();
                Serialize.SaveToFile(Default, path);
            }
            else
            {
                Default = Serialize.LoadFromFile<Settings>(path);
            }
        }
        private Settings() { }
        public void Save() => Serialize.SaveToFile(this, $"{Component.Path}\\user\\settings.json");

        [DataMember]
        public int ClipHeight
        {
            get => clipHeight;
            set => SetValue(value, ref clipHeight, clipHeightArgs);
        }
        [DataMember]
        public bool UseDarkMode
        {
            get => darkMode;
            set => SetValue(value, ref darkMode, darkModeArgs);
        }
        [DataMember]
        public bool AutoBackUp
        {
            get => autoBackUp;
            set => SetValue(value, ref autoBackUp, autoBackUpArgs);
        }
        [DataMember]
        public string LastTimeFolder
        {
            get => lastTimeFolder;
            set => SetValue(value, ref lastTimeFolder, lastTimeFolderArgs);
        }
        [DataMember]
        public int LastTimeNum
        {
            get => lastTimeNum;
            set => SetValue(value, ref lastTimeNum, lastTimeNumArgs);
        }
        [DataMember]
        public int WidthOf1Frame
        {
            get => widthOf1Frame;
            set => SetValue(value, ref widthOf1Frame, widthOf1FrameArgs);
        }
        [DataMember]
        public bool EnableErrorLog
        {
            get => enableErrorLog;
            set => SetValue(value, ref enableErrorLog, enableErrorLogArgs);
        }
    }
}
