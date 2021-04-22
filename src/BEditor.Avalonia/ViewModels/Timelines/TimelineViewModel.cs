﻿using System;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Input;

using BEditor.Data;
using BEditor.Extensions;
using BEditor.Media;
using BEditor.Models;
using BEditor.Properties;

using Microsoft.Extensions.DependencyInjection;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BEditor.ViewModels.Timelines
{
    public class TimelineViewModel
    {
        public int ClickedLayer;
        public Frame ClickedFrame;
        public int PointerLayer;
        public Frame PointerFrame;
        public bool SeekbarIsMouseDown;
        public bool ClipMouseDown;
        public bool KeyframeToggle = true;
        public byte ClipLeftRight = 0;
        public ClipElement? SelectedClip;
        public double ClipMovement;
        public Point ClipStart;
        public bool ClipTimeChange;

        public TimelineViewModel(Scene scene)
        {
            Scene = scene;

            TrackWidth.Value = scene.ToPixel(scene.TotalFrame);

            scene.ObserveProperty(s => s.PreviewFrame)
                .Where(_ => Tool.PreviewIsEnabled)
                .Subscribe(f =>
                {
                    SeekbarMargin.Value = new Thickness(Scene.ToPixel(f), 0, 0, 0);

                    var type = AppModel.Current.AppStatus is Status.Playing ? RenderType.VideoPreview : RenderType.Preview;

                    Scene.Parent.PreviewUpdate(type);
                });

            scene.ObserveProperty(s => s.TotalFrame)
                .Subscribe(_ =>
                {
                    TrackWidth.Value = Scene.ToPixel(Scene.TotalFrame);

                    // Todo: 目盛り追加
                    ResetScale?.Invoke(Scene.TimeLineZoom, Scene.TotalFrame, Scene.Parent.Framerate);
                });

            AddClip.Subscribe(meta =>
            {
                if (!Scene.InRange(ClickedFrame, ClickedFrame + 180, ClickedLayer))
                {
                    Scene.ServiceProvider?.GetService<IMessage>()?.Snackbar(Strings.ClipExistsInTheSpecifiedLocation);

                    return;
                }

                Scene.AddClip(ClickedFrame, ClickedLayer, meta, out _).Execute();
            });
        }

        public Func<PointerEventArgs, Point>? GetLayerMousePosition { get; set; }
        public double TrackHeight { get; } = ConstantSettings.ClipHeight;
        public ReactiveCommand<ObjectMetadata> AddClip { get; } = new();
        public ReactiveCommand Paste { get; } = new();
        public ReactiveCommand ShowSettings { get; } = new();
        public ReactiveProperty<StandardCursorType> LayerCursor { get; } = new();
        public ReactiveProperty<double> TrackWidth { get; } = new();
        public ReactivePropertySlim<Thickness> SeekbarMargin { get; } = new();
        public Scene Scene { get; }
        public Action<float, int, int>? ResetScale { get; set; }
        public Action<ClipElement, int>? ClipLayerMoveCommand { get; set; }

        public static int ToLayer(double pixel)
        {
            pixel -= 32;
            return (int)(pixel / ConstantSettings.ClipHeight) + 1;
        }

        public static double ToLayerPixel(int layer)
        {
            return ((layer - 1) * ConstantSettings.ClipHeight) + 32;
        }

        public void PointerMoved(Point point)
        {
            // point: マウスの現在フレーム

            PointerFrame = Scene.ToFrame(point.X);
            PointerLayer = ToLayer(point.Y);

            if (SeekbarIsMouseDown && KeyframeToggle)
            {
                Scene.PreviewFrame = PointerFrame + 1;
            }
            else if (ClipMouseDown)
            {
                if (SelectedClip is null) return;
                var selectviewmodel = SelectedClip.GetCreateClipViewModel();
                if (selectviewmodel.ClipCursor.Value == StandardCursorType.Arrow && LayerCursor.Value == StandardCursorType.Arrow)
                {
                    var newframe = PointerFrame - Scene.ToFrame(ClipStart.X) + Scene.ToFrame(selectviewmodel.MarginLeft);
                    var newlayer = PointerLayer;

                    if (!Scene.InRange(SelectedClip, newframe, newframe + SelectedClip.Length, newlayer))
                    {
                        return;
                    }

                    Thickness thickness = default;

                    if (newframe < 0)
                    {
                        thickness = new(0, 0, 0, 0);
                    }
                    else
                    {
                        thickness = new Thickness(Scene.ToPixel(newframe), ToLayerPixel(newlayer), 0, 0);
                        selectviewmodel.Row = newlayer;
                    }

                    selectviewmodel.MarginProperty.Value = thickness;

                    ClipStart = point;

                    ClipTimeChange = true;
                }
                else
                {
                    ClipMovement = Scene.ToPixel(Scene.ToFrame(point.X) - Scene.ToFrame(ClipStart.X)); //一時的な移動量

                    if (ClipLeftRight == 2)
                    {
                        // 左
                        selectviewmodel.WidthProperty.Value += ClipMovement;
                    }
                    else if (ClipLeftRight == 1)
                    {
                        // 右
                        var a = ClipMovement;
                        selectviewmodel.WidthProperty.Value -= a;
                        selectviewmodel.MarginLeft = selectviewmodel.MarginProperty.Value.Left + a;
                    }

                    ClipStart = point;
                }
            }
        }

        public void PointerLeftReleased()
        {
            // マウス押下中フラグを落とす
            SeekbarIsMouseDown = false;
            LayerCursor.Value = StandardCursorType.Arrow;

            // 保存
            if (ClipLeftRight != 0 && SelectedClip != null)
            {
                var clipVm = SelectedClip.GetCreateClipViewModel();

                var start = Scene.ToFrame(clipVm.MarginLeft);
                var end = Scene.ToFrame(clipVm.WidthProperty.Value) + start;

                if (0 < start && 0 < end)
                {
                    SelectedClip.ChangeLength(start, end).Execute();
                }

                ClipLeftRight = 0;
            }
        }

        public void PointerLeftPressed()
        {
            if (ClipMouseDown || !KeyframeToggle)
            {
                return;
            }

            // フラグを"マウス押下中"にする
            SeekbarIsMouseDown = true;

            Scene.PreviewFrame = ClickedFrame + 1;
        }

        public void PointerLeaved()
        {
            SeekbarIsMouseDown = false;

            ClipMouseDown = false;
            LayerCursor.Value = StandardCursorType.Arrow;

            if (ClipTimeChange && SelectedClip is not null)
            {
                var clip = SelectedClip;
                var clipVm = clip.GetCreateClipViewModel();
                var toframe = Scene.ToFrame(clipVm.MarginLeft);

                if (Frame.Zero > toframe)
                {
                    clip.MoveFrameLayer(toframe, clipVm.Row).Execute();
                }

                ClipTimeChange = false;
            }
        }
    }
}