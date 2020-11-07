﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using BEditor.Core.Data.ProjectData;
using BEditor.Core.Data.PropertyData;
using BEditor.Core.Extensions;
using BEditor.Core.Extesions.ViewCommand;
using BEditor.Core.Media.Decoder;
using BEditor.Core.Properties;

namespace BEditor.Core.Data.ObjectData {
    public static partial class DefaultData {
        [DataContract(Namespace = "")]
        public sealed class Video : DefaultImageObject {
            public static readonly EasePropertyMetadata SpeedMetadata = new EasePropertyMetadata(Properties.Resources.Speed, 100);
            public static readonly EasePropertyMetadata StartMetadata = new EasePropertyMetadata(Properties.Resources.Start, 1, float.NaN, 0);
            public static readonly FilePropertyMetadata FileMetadata = new FilePropertyMetadata(Properties.Resources.File, "", "mp4,avi,wmv,mov", Properties.Resources.VideoFile);

            private VideoDecoder videoReader;


            public Video() {
                Speed = new EaseProperty(SpeedMetadata);
                Start = new EaseProperty(StartMetadata);
                File = new FileProperty(FileMetadata);
            }



            #region DefaultImageObjectメンバー
            public override IList<PropertyElement> GroupItems => new List<PropertyElement>() {
                Speed,
                Start,
                File
            };

            public override Media.Image Load(EffectLoadArgs args) {
                float speed = Speed.GetValue(args.Frame) / 100;
                int start = (int)Start.GetValue(args.Frame);

                return VideoDecoder.Read((int)((start + args.Frame - Parent.ClipData.Start) * speed), videoReader);
            }

            public override void PropertyLoaded() {
                base.PropertyLoaded();

                if (System.IO.File.Exists(File.File)) {
                    videoReader = new FFmpegVideoDecoder(File.File);
                }

                File.PropertyChanged += (s, e) => {
                    if (e.PropertyName != nameof(FileProperty.File)) {
                        return;
                    }

                    videoReader?.Dispose();

                    try {
                        videoReader = new FFmpegVideoDecoder(File.File);
                    }
                    catch (Exception ex) {
                        Message.Snackbar(string.Format(Resources.FailedToLoad, File.File));
                        ActivityLog.ErrorLog(ex);
                    }
                };
            }

            #endregion


            [DataMember(Order = 0)]
            [PropertyMetadata("SpeedMetadata", typeof(Video))]
            public EaseProperty Speed { get; private set; }

            [DataMember(Order = 1)]
            [PropertyMetadata("StartMetadata", typeof(Video))]
            public EaseProperty Start { get; private set; }

            [DataMember(Order = 2)]
            [PropertyMetadata("FileMetadata", typeof(Video))]
            public FileProperty File { get; private set; }
        }
    }
}