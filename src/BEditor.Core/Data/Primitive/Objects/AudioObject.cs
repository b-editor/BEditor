﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using BEditor.Core.Command;
using BEditor.Core.Data.Property;
using BEditor.Core.Properties;
using BEditor.Core.Service;

using NAudio.Wave;

namespace BEditor.Core.Data.Primitive.Objects
{
    [DataContract]
    [CustomClipUI(Color = 0xff1744)]
    public class AudioObject : ObjectElement
    {
        public static readonly FilePropertyMetadata FileMetadata = VideoFile.FileMetadata with { Filter = new("", new FileExtension[] { new("mp3"), new("wav") }) };
        public static readonly EasePropertyMetadata VolumeMetadata = new("Volume", 50, 100, 0);
        public static readonly ValuePropertyMetadata StartMetadata = new(Resources.Start + "(Milliseconds)", 0, Min: 0);
        private WaveOut? _Player;
        private AudioFileReader? _Reader;
        private IDisposable? _Disposable;

        public AudioObject()
        {
            Volume = new(VolumeMetadata);
            File = new(FileMetadata);
            Start = new(StartMetadata);
        }

        public override string Name => "Audio";
        public override IEnumerable<PropertyElement> Properties => new PropertyElement[]
        {
            Volume,
            Start,
            File
        };
        [DataMember(Order = 0)]
        public FileProperty File { get; private set; }
        [DataMember(Order = 1)]
        public EaseProperty Volume { get; private set; }
        [DataMember(Order = 2)]
        public ValueProperty Start { get; private set; }
        private WaveOut Player => _Player ??= new();
        private AudioFileReader? Reader
        {
            get
            {
                if (_Reader is null && System.IO.File.Exists(File.File))
                {
                    _Reader = new(File.File);
                }

                return _Reader;
            }
            set
            {
                _Reader?.Dispose();
                _Reader = value;
            }
        }


        public override void Render(EffectRenderArgs args)
        {
            Player.Volume = Volume[args.Frame] / 100;
            if (args.Type is not RenderType.VideoPreview) return;

            if (_Reader is null) return;


            if (args.Frame == Parent!.Start)
            {
                Task.Run(async () =>
                {
                    Reader!.CurrentTime = TimeSpan.FromMilliseconds(Start.Value);
                    Player.Init(Reader);

                    Player.Play();

                    var millis = (int)Parent.Length.ToMilliseconds(Parent!.Parent!.Parent!.Framerate);
                    await Task.Delay(millis);

                    Player.Stop();
                });
            }
        }
        protected override void OnLoad()
        {
            Volume.Load(VolumeMetadata);
            File.Load(FileMetadata);
            Start.Load(StartMetadata);

            _Disposable = File.Subscribe(file =>
            {
                if (System.IO.File.Exists(file))
                {
                    Reader = new(file);
                }
            });

            var player = Parent!.Parent.Player;
            player.Stopped += Player_Stopped;

            player.Playing += Player_PlayingAsync;
        }
        protected override void OnUnload()
        {
            _Disposable?.Dispose();
            var player = Parent!.Parent.Player;
            player.Stopped -= Player_Stopped;

            player.Playing -= Player_PlayingAsync;
        }

        private async void Player_PlayingAsync(object? sender, PlayingEventArgs e)
        {
            if (Parent!.Start <= e.StartFrame && e.StartFrame <= Parent.End)
            {
                var framerate = Parent!.Parent!.Parent!.Framerate;
                var startmsec = e.StartFrame.ToMilliseconds(framerate);

                Reader!.CurrentTime = TimeSpan.FromMilliseconds(Start.Value + startmsec);
                Player.Init(Reader);

                Player.Play();

                // クリップ基準の再生開始位置
                var hStart = startmsec - Parent.Start.ToMilliseconds(framerate);

                var millis = (int)(Parent.Length.ToMilliseconds(framerate) - hStart);
                await Task.Delay(millis);

                Player.Stop();
            }
        }
        private void Player_Stopped(object? sender, EventArgs e)
        {
            Player.Stop();
        }
    }
}