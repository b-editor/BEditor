﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text.Json;

using BEditor.Command;
using BEditor.Data.Property.Easing;
using BEditor.Drawing;
using BEditor.Media;
using BEditor.Resources;

namespace BEditor.Data.Property
{
    /// <summary>
    /// Represents a property that eases the value of a <see cref="Color"/> type.
    /// </summary>
    [DebuggerDisplay("Count = {Value.Count}, Easing = {EasingData.Name}")]
    public class ColorAnimationProperty : PropertyElement<ColorAnimationPropertyMetadata>, IKeyFrameProperty
    {
        #region Fields
        private static readonly PropertyChangedEventArgs _easingFuncArgs = new(nameof(EasingType));
        private static readonly PropertyChangedEventArgs _easingDataArgs = new(nameof(EasingData));
        private EasingFunc? _easingTypeProperty;
        private EasingMetadata? _easingData;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorAnimationProperty"/> class.
        /// </summary>
        /// <param name="metadata">Metadata of this property.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metadata"/> is <see langword="null"/>.</exception>
        public ColorAnimationProperty(ColorAnimationPropertyMetadata metadata)
        {
            PropertyMetadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            var color = metadata.DefaultColor;

            Value = new() { color, color };
            Frames = new();
            EasingType = metadata.DefaultEase.CreateFunc();
        }

        /// <inheritdoc/>
        public event Action<Frame, int>? Added;

        /// <inheritdoc/>
        public event Action<int>? Removed;

        /// <inheritdoc/>
        public event Action<int, int>? Moved;

        /// <summary>
        /// Gets the <see cref="ObservableCollection{Color}"/> of the <see cref="Color"/> type value corresponding to <see cref="Frames"/>.
        /// </summary>
        public ObservableCollection<Color> Value { get; set; }

        /// <summary>
        /// Gets the <see cref="List{Frame}"/> of the frame number corresponding to <see cref="Value"/>.
        /// </summary>
        public List<Frame> Frames { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="EasingFunc"/>.
        /// </summary>
        public EasingFunc EasingType
        {
            get
            {
                if (_easingTypeProperty == null || EasingData.Type != _easingTypeProperty.GetType())
                {
                    _easingTypeProperty = EasingData.CreateFunc();
                    _easingTypeProperty.Parent = this;
                }

                return _easingTypeProperty;
            }
            set
            {
                SetValue(value, ref _easingTypeProperty, _easingFuncArgs);

                EasingData = EasingMetadata.LoadedEasingFunc.Find(x => x.Type == value.GetType())!;
            }
        }

        /// <summary>
        /// Gets or sets the metadata for <see cref="EasingType"/>.
        /// </summary>
        public EasingMetadata EasingData
        {
            get => _easingData ?? EasingMetadata.LoadedEasingFunc[0];
            set => SetValue(value, ref _easingData, _easingDataArgs);
        }

        /// <inheritdoc/>
        public override EffectElement Parent
        {
            get => base.Parent;
            set
            {
                base.Parent = value;
                EasingType.Parent = this;
            }
        }

        /// <summary>
        /// Gets the length of the clip.
        /// </summary>
        internal Frame Length => Parent?.Parent?.Length ?? default;

        /// <summary>
        /// Gets an eased value.
        /// </summary>
        /// <param name="frame">The frame of the value to get.</param>
        public Color this[Frame frame] => GetValue(frame);

        #region Methods

        /// <summary>
        /// Gets an eased value.
        /// </summary>
        /// <param name="frame">The frame of the value to get.</param>
        /// <returns>Returns an eased value.</returns>
        public Color GetValue(Frame frame)
        {
            static (int, int) GetFrame(ColorAnimationProperty property, Frame frame)
            {
                if (property.Frames.Count == 0)
                {
                    return (0, property.Length);
                }
                else if (frame >= 0 && frame <= property.Frames[0])
                {
                    return (0, property.Frames[0]);
                }
                else if (property.Frames[^1] <= frame && frame <= property.Length)
                {
                    return (property.Frames[^1], property.Length);
                }
                else
                {
                    var index = 0;
                    for (var f = 0; f < property.Frames.Count - 1; f++)
                    {
                        if (property.Frames[f] <= frame && frame <= property.Frames[f + 1])
                        {
                            index = f;
                        }
                    }

                    return (property.Frames[index], property.Frames[index + 1]);
                }

                throw new Exception();
            }
            static (Color, Color) GetValues(ColorAnimationProperty property, Frame frame)
            {
                if (property.Value.Count == 2)
                {
                    return (property.Value[0], property.Value[1]);
                }
                else if (frame >= 0 && frame <= property.Frames[0])
                {
                    return (property.Value[0], property.Value[1]);
                }
                else if (property.Frames[^1] <= frame && frame <= property.Length)
                {
                    return (property.Value[^2], property.Value[^1]);
                }
                else
                {
                    var index = 0;
                    for (var f = 0; f < property.Frames.Count - 1; f++)
                    {
                        if (property.Frames[f] <= frame && frame <= property.Frames[f + 1])
                        {
                            index = f + 1;
                        }
                    }

                    return (property.Value[index], property.Value[index + 1]);
                }

                throw new Exception();
            }

            frame -= this.GetParent2()?.Start ?? default;

            var (start, end) = GetFrame(this, frame);

            var (stval, edval) = GetValues(this, frame);

            // 相対的な現在フレーム
            int now = frame - start;

            var red = EasingType.EaseFunc(now, end - start, stval.R, edval.R);
            var green = EasingType.EaseFunc(now, end - start, stval.G, edval.G);
            var blue = EasingType.EaseFunc(now, end - start, stval.B, edval.B);
            var alpha = EasingType.EaseFunc(now, end - start, stval.A, edval.A);

            return Color.FromARGB(
                (byte)alpha,
                (byte)red,
                (byte)green,
                (byte)blue);
        }

        /// <summary>
        /// Insert a keyframe at a specific frame.
        /// </summary>
        /// <param name="frame">Frame to be added.</param>
        /// <param name="value">Value to be added.</param>
        /// <returns>Index of the added <see cref="Value"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside the scope of the parent element.</exception>
        public int InsertKeyframe(Frame frame, Color value)
        {
            if (frame <= Frame.Zero || frame >= this.GetParent2()!.Length) throw new ArgumentOutOfRangeException(nameof(frame));

            Frames.Add(frame);

            var tmp = new List<Frame>(Frames);
            tmp.Sort((a, b) => a - b);

            for (var i = 0; i < Frames.Count; i++)
            {
                Frames[i] = tmp[i];
            }

            var stindex = Frames.IndexOf(frame) + 1;

            Value.Insert(stindex, value);

            return stindex;
        }

        /// <summary>
        /// Remove a keyframe of a specific frame.
        /// </summary>
        /// <param name="frame">Frame to be removed.</param>
        /// <param name="value">Removed value.</param>
        /// <returns>Index of the removed <see cref="Value"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside the scope of the parent element.</exception>
        public int RemoveKeyframe(Frame frame, out Color value)
        {
            if (frame <= Frame.Zero || frame >= this.GetParent2()!.Length) throw new ArgumentOutOfRangeException(nameof(frame));

            // 値基準のindex
            var index = Frames.IndexOf(frame) + 1;
            value = Value[index];

            if (Frames.Remove(frame))
            {
                Value.RemoveAt(index);
            }

            return index;
        }

        /// <inheritdoc/>
        public override void GetObjectData(Utf8JsonWriter writer)
        {
            base.GetObjectData(writer);
            writer.WriteStartArray(nameof(Frames));

            foreach (var f in Frames)
            {
                writer.WriteNumberValue(f);
            }

            writer.WriteEndArray();

            writer.WriteStartArray("Values");

            foreach (var v in Value)
            {
                writer.WriteStringValue(v.ToString("#argb"));
            }

            writer.WriteEndArray();

            writer.WriteStartObject("Easing");

            var type = EasingType.GetType();
            writer.WriteString("_type", type.FullName + ", " + type.Assembly.GetName().Name);
            EasingType.GetObjectData(writer);

            writer.WriteEndObject();
        }

        /// <inheritdoc/>
        public override void SetObjectData(JsonElement element)
        {
            base.SetObjectData(element);

            var frames = element.GetProperty(nameof(Frames));
            Frames = frames.EnumerateArray().Select(i => (Frame)i.GetInt32()).ToList();

            var values = element.GetProperty("Values");
            Value = new(values.EnumerateArray().Select(i => Color.FromHTML(i.GetString())));

            var easing = element.GetProperty("Easing");
            var type = Type.GetType(easing.GetProperty("_type").GetString()!);
            if (type is null)
            {
                EasingType = EasingMetadata.LoadedEasingFunc.First().CreateFunc();
            }
            else
            {
                EasingType = (EasingFunc)FormatterServices.GetUninitializedObject(type);
                EasingType.SetObjectData(easing);
            }
        }

        /// <summary>
        /// Create a command to change the color of this <see cref="Value"/>.
        /// </summary>
        /// <param name="index">Index of colors to be changed.</param>
        /// <param name="color">New Color.</param>
        /// <returns>Created <see cref="IRecordCommand"/>.</returns>
        [Pure]
        public IRecordCommand ChangeColor(int index, Color color) => new ChangeColorCommand(this, index, color);

        /// <summary>
        /// Create a command to change the easing function.
        /// </summary>
        /// <param name="metadata">New easing function metadata.</param>
        /// <returns>Created <see cref="IRecordCommand"/>.</returns>
        [Pure]
        public IRecordCommand ChangeEase(EasingMetadata metadata) => new ChangeEaseCommand(this, metadata);

        /// <summary>
        /// Create a command to add a keyframe.
        /// </summary>
        /// <param name="frame">Frame to be added.</param>
        /// <returns>Created <see cref="IRecordCommand"/>.</returns>
        [Pure]
        public IRecordCommand AddFrame(Frame frame) => new AddCommand(this, frame);

        /// <summary>
        /// Create a command to remove a keyframe.
        /// </summary>
        /// <param name="frame">Frame to be removed.</param>
        /// <returns>Created <see cref="IRecordCommand"/>.</returns>
        [Pure]
        public IRecordCommand RemoveFrame(Frame frame) => new RemoveCommand(this, frame);

        /// <summary>
        /// Create a command to move a keyframe.
        /// </summary>
        /// <param name="fromIndex">Index of the frame to be moved from.</param>
        /// <param name="toFrame">Destination frame.</param>
        /// <returns>Created <see cref="IRecordCommand"/>.</returns>
        [Pure]
        public IRecordCommand MoveFrame(int fromIndex, Frame toFrame) => new MoveCommand(this, fromIndex, toFrame);

        /// <inheritdoc/>
        protected override void OnLoad()
        {
            EasingType.Parent = this;
            EasingType.Load();
        }

        /// <inheritdoc/>
        protected override void OnUnload()
        {
            EasingType.Unload();
        }

        #endregion

        #region Commands

        private sealed class ChangeColorCommand : IRecordCommand
        {
            private readonly WeakReference<ColorAnimationProperty> _property;
            private readonly int _index;
            private readonly Color _new;
            private readonly Color _old;

            public ChangeColorCommand(ColorAnimationProperty property, int index, Color color)
            {
                _property = new(property ?? throw new ArgumentNullException(nameof(property)));
                _index = (index < 0 || index >= property.Value.Count) ? throw new IndexOutOfRangeException($"{nameof(index)} is out of range of {nameof(Value)}") : index;

                _new = color;
                _old = property.Value[index];
            }

            public string Name => Strings.ChangeColor;

            public void Do()
            {
                if (_property.TryGetTarget(out var target))
                {
                    target.Value[_index] = _new;
                }
            }

            public void Redo()
            {
                Do();
            }

            public void Undo()
            {
                if (_property.TryGetTarget(out var target))
                {
                    target.Value[_index] = _old;
                }
            }
        }

        private sealed class ChangeEaseCommand : IRecordCommand
        {
            private readonly WeakReference<ColorAnimationProperty> _property;
            private readonly EasingFunc _new;
            private readonly EasingFunc _old;

            public ChangeEaseCommand(ColorAnimationProperty property, string type)
            {
                _property = new(property ?? throw new ArgumentNullException(nameof(property)));

                var data = EasingMetadata.LoadedEasingFunc.Find(x => x.Name == type)!;
                _new = data.CreateFunc();
                _new.Parent = property;
                _old = property.EasingType;
            }

            public ChangeEaseCommand(ColorAnimationProperty property, EasingMetadata metadata)
            {
                _property = new(property ?? throw new ArgumentNullException(nameof(property)));

                _new = metadata.CreateFunc();
                _new.Parent = property;
                _old = property.EasingType;
            }

            public string Name => Strings.ChangeEasing;

            public void Do()
            {
                if (_property.TryGetTarget(out var target))
                {
                    target.EasingType = _new;
                }
            }

            public void Redo()
            {
                Do();
            }

            public void Undo()
            {
                if (_property.TryGetTarget(out var target))
                {
                    target.EasingType = _old;
                }
            }
        }

        private sealed class AddCommand : IRecordCommand
        {
            private readonly WeakReference<ColorAnimationProperty> _property;
            private readonly Frame _frame;

            public AddCommand(ColorAnimationProperty property, Frame frame)
            {
                _property = new(property ?? throw new ArgumentNullException(nameof(property)));

                _frame = (frame <= Frame.Zero || frame >= property.GetParent2()!.Length) ? throw new ArgumentOutOfRangeException(nameof(frame)) : frame;
            }

            public string Name => Strings.AddKeyframe;

            public void Do()
            {
                if (_property.TryGetTarget(out var target))
                {
                    var index = target.InsertKeyframe(_frame, target.GetValue(_frame + target.GetParent2()?.Start ?? 0));

                    target.Added?.Invoke(_frame, index - 1);
                }
            }

            public void Redo()
            {
                Do();
            }

            public void Undo()
            {
                if (_property.TryGetTarget(out var target))
                {
                    var index = target.RemoveKeyframe(_frame, out _);

                    target.Removed?.Invoke(index - 1);
                }
            }
        }

        private sealed class RemoveCommand : IRecordCommand
        {
            private readonly WeakReference<ColorAnimationProperty> _property;
            private readonly Frame _frame;
            private Color _value;

            public RemoveCommand(ColorAnimationProperty property, Frame frame)
            {
                _property = new(property ?? throw new ArgumentNullException(nameof(property)));

                _frame = (frame <= Frame.Zero || property.GetParent2()!.Length <= frame) ? throw new ArgumentOutOfRangeException(nameof(frame)) : frame;
            }

            public string Name => Strings.RemoveKeyframe;

            public void Do()
            {
                if (_property.TryGetTarget(out var target))
                {
                    var index = target.RemoveKeyframe(_frame, out _value);

                    target.Removed?.Invoke(index - 1);
                }
            }

            public void Redo()
            {
                Do();
            }

            public void Undo()
            {
                if (_property.TryGetTarget(out var target))
                {
                    var index = target.InsertKeyframe(_frame, _value);

                    target.Added?.Invoke(_frame, index - 1);
                }
            }
        }

        private sealed class MoveCommand : IRecordCommand
        {
            private readonly WeakReference<ColorAnimationProperty> _property;
            private readonly int _fromIndex;
            private readonly Frame _toFrame;
            private int _toIndex;

            public MoveCommand(ColorAnimationProperty property, int fromIndex, Frame to)
            {
                _property = new(property ?? throw new ArgumentNullException(nameof(property)));

                _fromIndex = (fromIndex < 0 || fromIndex > property.Value.Count) ? throw new IndexOutOfRangeException() : fromIndex;

                _toFrame = (to <= Frame.Zero || property.GetParent2()!.Length <= to) ? throw new ArgumentOutOfRangeException(nameof(to)) : to;
            }

            public string Name => Strings.MoveKeyframe;

            public void Do()
            {
                if (_property.TryGetTarget(out var target))
                {
                    target.Frames[_fromIndex] = _toFrame;
                    target.Frames.Sort((a_, b_) => a_ - b_);

                    // 新しいindex
                    _toIndex = target.Frames.FindIndex(x => x == _toFrame);

                    // 値のIndexを合わせる
                    target.Value.Move(_fromIndex + 1, _toIndex + 1);

                    target.Moved?.Invoke(_fromIndex, _toIndex);
                }
            }

            public void Redo()
            {
                Do();
            }

            public void Undo()
            {
                if (_property.TryGetTarget(out var target))
                {
                    int frame = target.Frames[_toIndex];

                    target.Frames.RemoveAt(_toIndex);
                    target.Frames.Insert(_fromIndex, frame);

                    target.Value.Move(_toIndex + 1, _fromIndex + 1);

                    target.Moved?.Invoke(_toIndex, _fromIndex);
                }
            }
        }

        #endregion
    }
}
