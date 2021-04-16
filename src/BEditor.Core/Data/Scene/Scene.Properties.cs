﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BEditor.Audio;
using BEditor.Graphics;
using BEditor.Media;

namespace BEditor.Data
{
    /// <summary>
    /// Represents a scene to be included in the <see cref="Project"/>.
    /// </summary>
    public partial class Scene : IParent<ClipElement>, IChild<Project>, IHasName, IHasId
    {
        /// <summary>
        /// Gets the width of the frame buffer.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height of the frame buffer.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets or sets the name of this <see cref="Scene"/>.
        /// </summary>
        public virtual string SceneName
        {
            get => _sceneName;
            set => SetValue(value, ref _sceneName, _SceneNameArgs);
        }

        /// <summary>
        /// Gets or sets the total frame.
        /// </summary>
        public Frame TotalFrame
        {
            get => _totalframe;
            set => SetValue(value, ref _totalframe, _TotalFrameArgs);
        }

        /// <summary>
        /// Gets the number of the hidden layer.
        /// </summary>
        public List<int> HideLayer { get; private set; } = new List<int>();

        /// <summary>
        /// Gets the <see cref="ClipElement"/> contained in this <see cref="Scene"/>.
        /// </summary>
        public ObservableCollection<ClipElement> Datas { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ClipElement"/>.
        /// </summary>
        public ClipElement? SelectItem
        {
            get => _selectItem ??= SelectItems.FirstOrDefault();
            set
            {
                _selectItem = value;
                RaisePropertyChanged(_SelectItemArgs);
            }
        }

        /// <summary>
        /// Gets the selected <see cref="ClipElement"/>.
        /// </summary>
        public ObservableCollection<ClipElement> SelectItems
        {
            get
            {
                if (_selectItems is null)
                {
                    _selectItems = new();

                    _selectItems.CollectionChanged += (s, e) =>
                    {
                        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                        {
                            if (SelectItems.Count == 0)
                            {
                                SelectItem = null;
                            }
                        }
                    };
                }

                return _selectItems;
            }
        }

        /// <summary>
        /// Gets graphic context.
        /// </summary>
        public GraphicsContext? GraphicsContext { get; private set; }

        /// <summary>
        /// Gets audio context.
        /// </summary>
        public AudioContext? AudioContext { get; private set; }

        /// <summary>
        /// Gets a player to play this <see cref="Scene"/>.
        /// </summary>
        public IPlayer Player =>
            _player ??= new ScenePlayer(this);

        /// <summary>
        /// Gets or sets the frame number during preview.
        /// </summary>
        public Frame PreviewFrame
        {
            get => _previewframe;
            set => SetValue(value, ref _previewframe, _PrevireFrameArgs);
        }

        #region TimeLineの状態

        /// <summary>
        /// Gets or sets the scale of the timeline.
        /// </summary>
        public float TimeLineZoom
        {
            get => _timeLineZoom;
            set => SetValue(value, ref _timeLineZoom, _ZoomArgs);
        }

        /// <summary>
        /// Gets or sets the horizontal scrolling offset of the timeline.
        /// </summary>
        public double TimeLineHorizonOffset
        {
            get => _timeLineHorizonOffset;
            set => SetValue(value, ref _timeLineHorizonOffset, _HoffsetArgs);
        }

        /// <summary>
        /// Gets or sets the vertical scrolling offset of the timeline.
        /// </summary>
        public double TimeLineVerticalOffset
        {
            get => _timeLineVerticalOffset;
            set => SetValue(value, ref _timeLineVerticalOffset, _VoffsetArgs);
        }

        #endregion

        /// <inheritdoc/>
        public IEnumerable<ClipElement> Children => Datas;

        /// <inheritdoc/>
        public Project Parent
        {
            get
            {
                _parent ??= new(null!);

                if (_parent.TryGetTarget(out var p))
                {
                    return p;
                }

                return null!;
            }
            set
            {
                (_parent ??= new(null!)).SetTarget(value);

                foreach (var prop in Children)
                {
                    prop.Parent = this;
                }
            }
        }

        /// <inheritdoc/>
        public string Name => (SceneName ?? string.Empty).Replace('.', '_');

        /// <inheritdoc/>
        public int Id => Parent?.SceneList?.IndexOf(this) ?? -1;

        /// <summary>
        /// Gets or sets the settings for this scene.
        /// </summary>
        public SceneSettings Settings
        {
            get => new(Width, Height, Name);
            set
            {
                Width = value.Width;
                Height = value.Height;
                SceneName = value.Name;

                GraphicsContext?.Dispose();
                GraphicsContext = new(Width, Height);
            }
        }

        /// <summary>
        /// Gets the new id.
        /// </summary>
        internal int NewId
        {
            get
            {
                var count = Datas.Count;

                if (count > 0)
                {
                    var tmp = new List<int>();

                    Parallel.For(0, count, i => tmp.Add(Datas[i].Id));

                    return tmp.Max() + 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}