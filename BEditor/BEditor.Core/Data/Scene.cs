﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using BEditor.Core.Data.Control;
using BEditor.Core.Extensions;
using BEditor.Core.Graphics;
using BEditor.Core.Media;
using BEditor.Core.Renderings;

namespace BEditor.Core.Data
{
    /// <summary>
    /// Represents a scene to be included in the <see cref="Project"/>.
    /// </summary>
    [DataContract(Namespace = "")]
    public class Scene : ComponentObject, IParent<ClipData>, IChild<Project>, IHadName, IHadId
    {
        #region Fields

        private static readonly PropertyChangedEventArgs selectItemArgs = new(nameof(SelectItem));
        private static readonly PropertyChangedEventArgs previreFrameArgs = new(nameof(PreviewFrame));
        private static readonly PropertyChangedEventArgs totalFrameArgs = new(nameof(TotalFrame));
        private static readonly PropertyChangedEventArgs zoomArgs = new(nameof(TimeLineZoom));
        private static readonly PropertyChangedEventArgs hoffsetArgs = new(nameof(TimeLineHorizonOffset));
        private static readonly PropertyChangedEventArgs voffsetArgs = new(nameof(TimeLineVerticalOffset));
        private static readonly PropertyChangedEventArgs sceneNameArgs = new(nameof(SceneName));
        private ClipData selectItem;
        private ObservableCollection<ClipData> selectItems;
        private int previewframe;
        private int totalframe = 1000;
        private float timeLineZoom = 150;
        private double timeLineHorizonOffset;
        private double timeLineVerticalOffset;
        private string sceneName;

        #endregion


        #region Contructor

        /// <summary>
        /// <see cref="Scene"/> Initialize a new instance of the class.
        /// </summary>
        /// <param name="width">The width of the frame buffer.</param>
        /// <param name="height">The height of the frame buffer</param>
        public Scene(int width, int height)
        {
            Width = width;
            Height = height;
            Datas = new ObservableCollection<ClipData>();
            GraphicsContext = new GraphicsContext(width, height);
        }

        #endregion


        #region Properties

        /// <summary>
        /// Get or set the width of the frame buffer.
        /// </summary>
        [DataMember(Order = 0)]
        public int Width { get; private set; }
        /// <summary>
        /// Get or set the height of the frame buffer
        /// </summary>
        [DataMember(Order = 1)]
        public int Height { get; private set; }

        /// <summary>
        /// Get or set the name of this <see cref="Scene"/>.
        /// </summary>
        [DataMember(Order = 2)]
        public virtual string SceneName
        {
            get => sceneName;
            set => SetValue(value, ref sceneName, sceneNameArgs);
        }

        /// <summary>
        /// Get the names of the selected <see cref="ClipData"/>.
        /// </summary>
        [DataMember(Order = 3)]
        public List<string> SelectNames { get; private set; } = new List<string>();
        /// <summary>
        /// Get the name of the selected <see cref="ClipData"/>.
        /// </summary>
        [DataMember(Order = 4)]
        public string SelectName { get; private set; }

        /// <summary>
        /// Get the <see cref="ClipData"/> contained in this <see cref="Scene"/>.
        /// </summary>
        [DataMember(Order = 10)]
        public ObservableCollection<ClipData> Datas { get; private set; }

        /// <summary>
        /// Get the number of the hidden layer.
        /// </summary>
        [DataMember(Order = 11)]
        public List<int> HideLayer { get; private set; } = new List<int>();

        /// <summary>
        /// Get or set the selected <see cref="ClipData"/>.
        /// </summary>
        public ClipData SelectItem
        {
            get => selectItem ??= this.Find(SelectName);
            set
            {
                SelectName = selectItem?.Name;
                selectItem = value;
                RaisePropertyChanged(selectItemArgs);
            }
        }
        /// <summary>
        /// Get or set the selected <see cref="ClipData"/>.
        /// </summary>
        public ObservableCollection<ClipData> SelectItems
        {
            get
            {
                if (selectItems == null)
                {
                    selectItems = new ObservableCollection<ClipData>();

                    Parallel.ForEach(SelectNames, name => selectItems.Add(this.Find(name)));

                    selectItems.CollectionChanged += SelectItems_CollectionChanged;
                }

                return selectItems;
            }
        }


        /// <summary>
        /// Get graphic context.
        /// </summary>
        public BaseGraphicsContext GraphicsContext { get; internal set; }


        #region コントロールに関係

        /// <summary>
        /// Gets or sets the frame number during preview.
        /// </summary>
        [DataMember(Order = 5)]
        public int PreviewFrame
        {
            get => previewframe;
            set => SetValue(value, ref previewframe, previreFrameArgs);
        }

        /// <summary>
        /// Get or set the total frame.
        /// </summary>
        [DataMember(Order = 6)]
        public int TotalFrame
        {
            get => totalframe;
            set => SetValue(value, ref totalframe, totalFrameArgs);
        }

        /// <summary>
        /// Get or set the scale of the timeline.
        /// </summary>
        [DataMember(Order = 7)]
        public float TimeLineZoom
        {
            get => timeLineZoom;
            set => SetValue(value, ref timeLineZoom, zoomArgs);
        }

        #region TimeLineScrollOffset

        /// <summary>
        /// Get or set the horizontal scrolling offset of the timeline.
        /// </summary>
        [DataMember(Order = 8)]
        public double TimeLineHorizonOffset
        {
            get => timeLineHorizonOffset;
            set => SetValue(value, ref timeLineHorizonOffset, hoffsetArgs);
        }


        /// <summary>
        /// Get or set the vertical scrolling offset of the timeline.
        /// </summary>
        [DataMember(Order = 9)]
        public double TimeLineVerticalOffset
        {
            get => timeLineVerticalOffset;
            set => SetValue(value, ref timeLineVerticalOffset, voffsetArgs);
        }

        #endregion

        #endregion

        /// <inheritdoc/>
        public IEnumerable<ClipData> Children => Datas;
        /// <inheritdoc/>
        public Project Parent { get; set; }
        /// <inheritdoc/>
        public string Name => SceneName;
        /// <inheritdoc/>
        public int Id => Parent.SceneList.IndexOf(this);

        internal int NewId
        {
            get
            {
                int count = Datas.Count;
                int max;

                if (count > 0)
                {
                    var tmp = new List<int>();

                    Parallel.For(0, count, i => tmp.Add(Datas[i].Id));

                    max = tmp.Max() + 1;
                }
                else
                {
                    max = 0;
                }

                return max;
            }
        }

        #endregion

        #region Methods

        private void SelectItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                SelectNames.Insert(e.NewStartingIndex, selectItems[e.NewStartingIndex].Name);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (SelectName == SelectNames[e.OldStartingIndex] || SelectItems.Count == 0)
                {
                    SelectItem = null;
                }

                SelectNames.RemoveAt(e.OldStartingIndex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void PropertyLoaded()
        {
            Parallel.ForEach(Datas, data =>
            {
                data.Parent = this;
                data.PropertyLoaded();
            });
        }


        /// <summary>
        /// Render this <see cref="Scene"/>.
        /// </summary>
        /// <param name="frame">The frame to render</param>
        public RenderingResult Render(int frame)
        {
            var buffer = new Image(Width, Height);
            var layer = GetLayer(frame).ToList();

            GraphicsContext.MakeCurrent();
            GraphicsContext.Clear();

            var args = new ClipRenderArgs(frame, layer);

            //Preview
            layer.ForEach(clip => clip.PreviewRender(args));

            layer.ForEach(clip => clip.Render(args));

            GraphicsContext.SwapBuffers();
            GraphicsContext.MakeCurrent();

            GLTK.GetPixels(buffer);

#if DEBUG
            if (frame % Parent.Framerate * 5 == 1)
                Task.Run(GC.Collect);
#endif
            return new RenderingResult { Image = buffer };
        }
        /// <summary>
        /// Render a frame of <see cref="PreviewFrame"/>.
        /// </summary>
        public RenderingResult Render()
        {
            return Render(PreviewFrame);
        }


        /// <summary>
        /// Get and sort the clips on the specified frame.
        /// </summary>
        /// <param name="frame">Target frame number.</param>
        public IEnumerable<ClipData> GetLayer(int frame)
        {
            return Datas
                .Where(item => item.Start <= (frame) && (frame) < item.End)
                .Where(item => !HideLayer.Exists(x => x == item.Layer))
                .OrderBy(item => item.Layer);
        }

        /// <summary>
        /// Add a <see cref="ClipData"/> to this <see cref="Scene"/>.
        /// </summary>
        /// <param name="clip">A <see cref="ClipData"/> to add</param>
        public void Add(ClipData clip)
        {
            clip.Parent = this;

            Datas.Add(clip);
        }
        /// <summary>
        /// Remove certain a <see cref="ClipData"/> from this <see cref="Scene"/>.
        /// </summary>
        /// <param name="clip"><see cref="ClipData"/> to be removed.</param>
        /// <returns>
        /// <see langword="true"/> if item is successfully removed; otherwise, <see langword="false"/>. This method also returns
        /// <see langword="false"/> if item was not found in the original <see cref="Collection{T}"/>.
        /// </returns>
        public bool Remove(ClipData clip)
        {
            return Datas.Remove(clip);
        }

        /// <summary>
        /// Set the selected <see cref="ClipData"/> and add the name to <see cref="SelectNames"/> if it does not exist.
        /// </summary>
        /// <param name="data">Target <see cref="ClipData"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        public void SetCurrentClip(ClipData data)
        {
            SelectItem = data ?? throw new ArgumentNullException(nameof(data));

            if (!SelectNames.Exists(x => x == data.Name))
            {
                SelectItems.Add(data);
            }
        }

        #endregion
    }

    [DataContract(Namespace = "")]
    public class RootScene : Scene
    {
        public RootScene(int width, int height) : base(width, height) { }

        public override string SceneName { get => "root"; set { } }
    }
}