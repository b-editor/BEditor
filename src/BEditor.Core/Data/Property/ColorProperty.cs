﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Reactive.Disposables;
using System.Runtime.Serialization;

using BEditor.Core.Command;
using BEditor.Core.Data.Bindings;
using BEditor.Core.Data.Property;
using BEditor.Drawing;

namespace BEditor.Core.Data.Property
{
    /// <summary>
    /// 色を選択するプロパティを表します
    /// </summary>
    [DataContract]
    public class ColorProperty : PropertyElement<ColorPropertyMetadata>, IEasingProperty, IBindable<Color>
    {
        #region Fields
        private static readonly PropertyChangedEventArgs _ColorArgs = new(nameof(Color));
        private Color _Color;
        private List<IObserver<Color>>? _List;

        private IDisposable? _BindDispose;
        private IBindable<Color>? _Bindable;
        private string? _BindHint;
        #endregion


        /// <summary>
        /// <see cref="ColorProperty"/> クラスの新しいインスタンスを初期化します
        /// </summary>
        /// <param name="metadata">このプロパティの <see cref="ColorPropertyMetadata"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="metadata"/> が <see langword="null"/> です</exception>
        public ColorProperty(ColorPropertyMetadata metadata)
        {
            PropertyMetadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            Color = metadata.DefaultColor;
        }


        private List<IObserver<Color>> Collection => _List ??= new();
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public Color Color
        {
            get => _Color;
            set => SetValue(value, ref _Color, _ColorArgs, this, state =>
            {
                foreach (var observer in state.Collection)
                {
                    try
                    {
                        observer.OnNext(state._Color);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                }
            });
        }
        /// <inheritdoc/>
        public Color Value => _Color;
        /// <inheritdoc/>
        [DataMember]
        public string? BindHint
        {
            get => _Bindable?.GetString();
            private set => _BindHint = value;
        }


        #region Methods

        /// <inheritdoc/>
        public override string ToString() => $"(R:{_Color.R} G:{_Color.G} B:{_Color.B} A:{_Color.A} Name:{PropertyMetadata?.Name})";
        /// <inheritdoc/>
        protected override void OnLoad()
        {
            if (_BindHint is not null && this.GetBindable(_BindHint, out var b))
            {
                Bind(b);
            }
            _BindHint = null;
        }

        /// <summary>
        /// 色を変更するコマンドを作成します
        /// </summary>
        [Pure]
        public IRecordCommand ChangeColor(Color color) => new ChangeColorCommand(this, color);

        #region IBindable

        public void Bind(IBindable<Color>? bindable)
        {
            _BindDispose?.Dispose();
            _Bindable = bindable;

            if (bindable is not null)
            {
                Color = bindable.Value;

                // bindableが変更時にthisが変更
                _BindDispose = bindable.Subscribe(this);
            }
        }

        public IDisposable Subscribe(IObserver<Color> observer)
        {
            if (observer is null) throw new ArgumentNullException(nameof(observer));

            Collection.Add(observer);
            return Disposable.Create((observer, this), state =>
             {
                 state.observer.OnCompleted();
                 state.Item2.Collection.Remove(state.observer);
             });
        }

        public void OnCompleted() { }
        public void OnError(Exception error) { }
        public void OnNext(Color value)
        {
            Color = value;
        }

        #endregion

        #endregion


        /// <summary>
        /// 色を変更するコマンド
        /// </summary>
        /// <remarks>このクラスは <see cref="CommandManager.Do(IRecordCommand)"/> と併用することでコマンドを記録できます</remarks>
        private sealed class ChangeColorCommand : IRecordCommand
        {
            private readonly ColorProperty _Property;
            private readonly Color _New;
            private readonly Color _Old;

            /// <summary>
            /// <see cref="ChangeColorCommand"/> クラスの新しいインスタンスを初期化します
            /// </summary>
            /// <param name="property">対象の <see cref="ColorProperty"/></param>
            /// <param name="color"></param>
            /// <exception cref="ArgumentNullException"><paramref name="property"/> が <see langword="null"/> です</exception>
            public ChangeColorCommand(ColorProperty property, Color color)
            {
                _Property = property ?? throw new ArgumentNullException(nameof(property));
                _New = color;
                _Old = property.Value;
            }

            public string Name => CommandName.ChangeColor;

            /// <inheritdoc/>
            public void Do()
            {
                _Property.Color = _New;
            }

            /// <inheritdoc/>
            public void Redo() => Do();

            /// <inheritdoc/>
            public void Undo()
            {
                _Property.Color = _Old;
            }
        }
    }

    /// <summary>
    /// <see cref="BEditor.Core.Data.Property.ColorProperty"/> のメタデータを表します
    /// </summary>
    public record ColorPropertyMetadata(string Name, Color DefaultColor = default, bool UseAlpha = false) : PropertyElementMetadata(Name);
}