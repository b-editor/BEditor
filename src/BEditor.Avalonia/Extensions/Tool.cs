﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

using BEditor.Data;
using BEditor.Media;
using BEditor.Models;
using BEditor.Properties;
using BEditor.ViewModels;

namespace BEditor.Extensions
{
    public static class Tool
    {
        public static bool PreviewIsEnabled { get; set; } = true;

        public static void PreviewUpdate(this Project project, ClipElement clipData, RenderType type = RenderType.Preview)
        {
            if (project is null) return;
            var now = project.PreviewScene.PreviewFrame;
            if (clipData.Start <= now && now <= clipData.End)
            {
                project.PreviewUpdate(type);
            }
        }

        public static void PreviewUpdate(this Project project, RenderType type = RenderType.Preview)
        {
            if (project is null || project.PreviewScene.GraphicsContext is null || !PreviewIsEnabled) return;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                PreviewIsEnabled = false;

                try
                {
                    using var img = project.PreviewScene.Render(type);
                    var viewmodel = MainWindowViewModel.Current.Previewer;

                    viewmodel.PreviewImage.Value?.Dispose();
                    viewmodel.PreviewImage.Value = img.ToBitmapSource();

                    PreviewIsEnabled = true;
                }
                catch
                {
                    var app = AppModel.Current;

                    if (app.AppStatus is Status.Playing)
                    {
                        app.AppStatus = Status.Edit;
                        app.Project!.PreviewScene.Player.Stop();
                        app.IsNotPlaying = true;

                        app.Message.Snackbar(Strings.An_exception_was_thrown_during_rendering);

                        PreviewIsEnabled = true;
                    }
                    else
                    {
                        PreviewIsEnabled = false;

                        app.Message.Snackbar(Strings.An_exception_was_thrown_during_rendering_preview);

                        Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(5));

                            PreviewIsEnabled = true;
                        });
                    }
                }
            });
        }

        public static double ToPixel(this Scene scene, Frame frame)
        {
            return ConstantSettings.WidthOf1Frame * (scene.TimeLineZoom / 200) * frame;
        }
        public static Frame ToFrame(this Scene scene, double pixel)
        {
            return (int)(pixel / (ConstantSettings.WidthOf1Frame * (scene.TimeLineZoom / 200)));
        }
        public static bool Clamp(this Scene self, ClipElement? clip_, ref Frame start, ref Frame end, int layer)
        {
            var array = self.GetLayer(layer).ToArray();

            for (var i = 0; i < array.Length; i++)
            {
                var clip = array[i];

                if (clip != clip_ && clip.InRange(start, end, out var type))
                {
                    if (type == RangeType.StartEnd)
                    {
                        return false;
                    }
                    else if (type == RangeType.Start)
                    {
                        start = clip.End;

                        return true;
                    }
                    else if (type == RangeType.End)
                    {
                        end = clip.Start;

                        return true;
                    }

                    return false;
                }
            }

            return true;
        }
        public static bool InRange(this Scene self, Frame start, Frame end, int layer)
        {
            foreach (var clip in self.GetLayer(layer))
            {
                if (clip.InRange(start, end))
                {
                    return false;
                }
            }

            return true;
        }
        public static bool InRange(this Scene self, ClipElement clip_, Frame start, Frame end, int layer)
        {
            var array = self.GetLayer(layer).ToArray();

            for (var i = 0; i < array.Length; i++)
            {
                var clip = array[i];

                if (clip != clip_ && clip.InRange(start, end, out _))
                {
                    return false;
                }
            }

            return true;
        }
        // このクリップと被る場合はtrue
        public static bool InRange(this ClipElement self, Frame start, Frame end)
        {
            if (self.Start <= start && end <= self.End)
            {
                return true;
            }
            else if (self.Start <= start && start < self.End)
            {
                return true;
            }
            else if (self.Start < end && end <= self.End)
            {
                return true;
            }
            else if (start <= self.Start && self.End <= end)
            {
                return true;
            }

            return false;
        }
        public static bool InRange(this ClipElement self, Frame start, Frame end, out RangeType type)
        {
            if (self.Start <= start && end <= self.End)
            {
                type = RangeType.StartEnd;

                return true;
            }
            else if (self.Start <= start && start < self.End)
            {
                type = RangeType.Start;

                return true;
            }
            else if (self.Start < end && end <= self.End)
            {
                type = RangeType.End;

                return true;
            }
            else if (start <= self.Start && self.End <= end)
            {
                type = RangeType.StartEnd;

                return true;
            }
            type = default;
            return false;
        }

        public enum RangeType
        {
            StartEnd,
            Start,
            End
        }
    }
}