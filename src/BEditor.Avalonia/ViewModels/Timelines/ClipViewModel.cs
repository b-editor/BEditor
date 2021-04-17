﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;

using BEditor.Data;
using BEditor.Extensions;
using BEditor.Models;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BEditor.ViewModels.Timelines
{
    public class ClipViewModel : IDisposable
    {
        private Point _mouseRightPoint;

        public ClipViewModel(ClipElement clip)
        {
            ClipElement = clip;
            WidthProperty.Value = Scene.ToPixel(ClipElement.Length);
            MarginProperty.Value = new Thickness(Scene.ToPixel(ClipElement.Start), TimelineViewModel.ToLayerPixel(clip.Layer), 0, 0);
            Row = clip.Layer;

            var color = clip.Metadata.AccentColor;
            ClipColor.Value = new SolidColorBrush(new Color(255, color.R, color.G, color.B));
            ClipText.Value = clip.Effect[0].Name;

            clip.PropertyChangedAsObservable()
                .Where(e => e.PropertyName is nameof(ClipElement.End))
                .Subscribe(_ => WidthProperty.Value = ClipElement.Parent.ToPixel(ClipElement.Length));

            clip.PropertyChangedAsObservable()
                .Where(e => e.PropertyName is nameof(ClipElement.Start))
                .Subscribe(_ =>
                {
                    MarginLeft = ClipElement.Parent.ToPixel(ClipElement.Start);
                    WidthProperty.Value = ClipElement.Parent.ToPixel(ClipElement.Length);
                });

            clip.PropertyChangedAsObservable()
                .Where(e => e.PropertyName is nameof(ClipElement.Layer))
                .Subscribe(_ => TimelineViewModel.ClipLayerMoveCommand?.Invoke(ClipElement, ClipElement.Layer));

            Copy.Subscribe(async () =>
            {
                await using var memory = new MemoryStream();
                await Serialize.SaveToStreamAsync(ClipElement, memory, SerializeMode.Json);

                var json = Encoding.Default.GetString(memory.ToArray());
                await Application.Current.Clipboard.SetTextAsync(json);
            });

            Cut.Subscribe(async () =>
            {
                ClipElement.Parent.RemoveClip(ClipElement).Execute();

                await using var memory = new MemoryStream();
                await Serialize.SaveToStreamAsync(ClipElement, memory, SerializeMode.Json);

                var json = Encoding.Default.GetString(memory.ToArray());
                await Application.Current.Clipboard.SetTextAsync(json);
            });

            Remove.Subscribe(() => ClipElement.Parent.RemoveClip(ClipElement).Execute());

            MessageLog.Subscribe(async () =>
            {
                var text =
                    $"ID : {ClipElement.Id}\n" +
                    $"Name : {ClipElement.Name}\n" +
                    $"Length : {ClipElement.Length.Value}\n" +
                    $"Layer : {ClipElement.Layer}\n" +
                    $"Start : {ClipElement.Start.Value}\n" +
                    $"End : {ClipElement.End.Value}";

                await AppModel.Current.Message.DialogAsync(text);
            });

            Split.Subscribe(() =>
            {
                var frame = ClipElement.Parent.ToFrame(_mouseRightPoint.X) + ClipElement.Start;

                ClipElement.Split(frame).Execute();
            });
        }

        ~ClipViewModel()
        {
            Dispose();
        }

        public Scene Scene => ClipElement.Parent;

        public TimelineViewModel TimelineViewModel => Scene.GetCreateTimelineViewModel();

        public ClipElement ClipElement { get; }

        public int Row { get; set; }

        public ReactivePropertySlim<string> ClipText { get; set; } = new();

        public ReactivePropertySlim<Brush> ClipColor { get; set; } = new();

        public static double TrackHeight => ConstantSettings.ClipHeight;

        public ReactivePropertySlim<double> WidthProperty { get; } = new();

        public ReactivePropertySlim<Thickness> MarginProperty { get; } = new();

        public double MarginTop
        {
            get => MarginProperty.Value.Top;
            set
            {
                var tmp = MarginProperty.Value;
                MarginProperty.Value = new(tmp.Left, value, tmp.Right, tmp.Bottom);
            }
        }

        public double MarginLeft
        {
            get => MarginProperty.Value.Left;
            set
            {
                var tmp = MarginProperty.Value;
                MarginProperty.Value = new(value, tmp.Top, tmp.Right, tmp.Bottom);
            }
        }

        public ReactivePropertySlim<StandardCursorType> ClipCursor { get; } = new();

        public ReactiveCommand Copy { get; } = new();

        public ReactiveCommand Cut { get; } = new();

        public ReactiveCommand Remove { get; } = new();

        public ReactiveCommand MessageLog { get; } = new();

        public ReactiveCommand Split { get; } = new();

        public void PointerLeftPressed(PointerEventArgs e)
        {
            var timeline = TimelineViewModel;
            var scene = ClipElement.Parent;
            timeline.ClipMouseDown = true;

            timeline.ClipStart = timeline.GetLayerMousePosition?.Invoke(e) ?? throw new Exception();

            timeline.SelectedClip = ClipElement;

            if (timeline.SelectedClip.GetCreateClipViewModel().ClipCursor.Value == StandardCursorType.SizeWestEast)
            {
                timeline.LayerCursor.Value = StandardCursorType.SizeWestEast;
            }
        }

        public void PointerRightPressed(Point point)
        {
            _mouseRightPoint = point;
        }

        public void PointerLeftReleased()
        {
            var timelinevm = TimelineViewModel;

            timelinevm.SeekbarIsMouseDown = false;
            var selectedClip = timelinevm.SelectedClip;

            if (selectedClip is null) return;

            timelinevm.ClipMouseDown = false;

            // 値の保存
            if (timelinevm.ClipLeftRight != 0)
            {
                int start = selectedClip.Parent.ToFrame(selectedClip.GetCreateClipViewModel().MarginLeft);
                int end = selectedClip.Parent.ToFrame(selectedClip.GetCreateClipViewModel().WidthProperty.Value) + start;

                if (0 < start && 0 < end)
                {
                    selectedClip.ChangeLength(start, end).Execute();
                }
            }

            if (timelinevm.ClipTimeChange)
            {
                var frame = selectedClip.Parent.ToFrame(selectedClip.GetCreateClipViewModel().MarginLeft);
                var layer = selectedClip.GetCreateClipViewModel().Row;

                selectedClip.MoveFrameLayer(frame, layer).Execute();

                timelinevm.ClipTimeChange = false;
            }

            // SelectItemに設定
            Scene.SetCurrentClip(ClipElement);

            timelinevm.ClipLeftRight = 0;
            timelinevm.LayerCursor.Value = StandardCursorType.Arrow;
        }

        public void PointerMoved(Point point)
        {
            var timeline = TimelineViewModel;
            if (timeline.ClipMouseDown) return;

            var horizon = point.X;

            // 左右 10px内 なら左右矢印↔
            if (horizon < 10)
            {
                ClipCursor.Value = StandardCursorType.SizeWestEast;
                timeline.ClipLeftRight = 1;
            }
            else if (horizon > ClipElement.Parent.ToPixel(ClipElement.Length) - 10)
            {
                ClipCursor.Value = StandardCursorType.SizeWestEast;
                timeline.ClipLeftRight = 2;
            }
            else
            {
                ClipCursor.Value = StandardCursorType.Arrow;
            }
        }

        public void Dispose()
        {
            ClipText.Dispose();
            ClipColor.Dispose();
            WidthProperty.Dispose();
            MarginProperty.Dispose();
            ClipCursor.Dispose();
            Cut.Dispose();
            Remove.Dispose();
            MessageLog.Dispose();
            Split.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}