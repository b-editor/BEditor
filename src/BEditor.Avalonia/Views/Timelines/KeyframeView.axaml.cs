using System;
using System.Linq;
using System.Reactive.Disposables;

using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

using BEditor.Data;
using BEditor.Data.Property;
using BEditor.Extensions;
using BEditor.Properties;
using BEditor.ViewModels.Timelines;

using Reactive.Bindings.Extensions;

namespace BEditor.Views.Timelines
{
    public class KeyframeView : UserControl
    {
        private readonly Grid _grid;
        private readonly TextBlock _text;
        private readonly CompositeDisposable _disposable = new();
        private readonly Animation _anm = new()
        {
            Duration = TimeSpan.FromSeconds(0.15),
            Children =
            {
                new()
                {
                    Cue = new(0),
                    Setters =
                    {
                        new Setter(OpacityProperty, 1d)
                    }
                },
                new()
                {
                    Cue = new(1),
                    Setters =
                    {
                        new Setter(OpacityProperty, 0d)
                    }
                }
            }
        };
        private Media.Frame _startpos;
        private Shape? _select;

#pragma warning disable CS8618
        public KeyframeView()
#pragma warning restore CS8618
        {
            InitializeComponent();
        }

        public KeyframeView(IKeyframeProperty property)
        {
            var viewmodel = new KeyframeViewModel(property);

            DataContext = viewmodel;
            InitializeComponent();
            _grid = this.FindControl<Grid>("grid");
            _text = this.FindControl<TextBlock>("text");

            _grid.AddHandler(PointerPressedEvent, Grid_PointerLeftPressedTunnel, RoutingStrategies.Tunnel);
            _grid.AddHandler(PointerMovedEvent, Grid_PointerMovedTunnel, RoutingStrategies.Tunnel);
            _grid.AddHandler(PointerPressedEvent, Grid_PointerRightPressedTunnel, RoutingStrategies.Tunnel);
            _grid.AddHandler(PointerReleasedEvent, Grid_PointerReleasedTunnel, RoutingStrategies.Tunnel);

            viewmodel.AddKeyFrameIcon = (frame, index) =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var x = Scene.ToPixel(frame);
                    var icon = new Rectangle
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(x, 0, 0, 0),
                        Width = 8,
                        Height = 8,
                        RenderTransform = new RotateTransform { Angle = 45 },
                        Fill = (IBrush?)Application.Current.FindResource("SystemControlForegroundBaseMediumHighBrush")
                    };

                    Add_Handler_Icon(icon);

                    icon.ContextMenu = new ContextMenu
                    {
                        Items = new MenuItem[] { CreateMenu() }
                    };

                    _grid.Children.Insert(index, icon);
                });
            };
            viewmodel.RemoveKeyFrameIcon = (index) => Dispatcher.UIThread.InvokeAsync(() => _grid.Children.RemoveAt(index));
            viewmodel.MoveKeyFrameIcon = (from, to) =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var icon = _grid.Children[from];
                    _grid.Children.RemoveAt(from);
                    _grid.Children.Insert(to, icon);
                });
            };

            _grid.Children.Clear();

            for (var index = 0; index < Property.Frames.Count; index++)
            {
                viewmodel.AddKeyFrameIcon(Property.Frames[index], index);
            }

            var tmp = Scene.ToPixel(Property.GetParent<ClipElement>()!.Length);
            if (tmp > 0)
            {
                Width = tmp;
            }

            Scene.ObserveProperty(p => p.TimeLineZoom)
                .Subscribe(_ => ZoomChange())
                .AddTo(_disposable);

            // StoryBoard��ݒ�
            {
                PointerEnter += async (_, _) =>
                {
                    _anm.PlaybackDirection = PlaybackDirection.Normal;
                    await _anm.RunAsync(_text);

                    _text.Opacity = 0;
                };
                PointerLeave += async (_, _) =>
                {
                    _anm.PlaybackDirection = PlaybackDirection.Reverse;
                    await _anm.RunAsync(_text);

                    _text.Opacity = 1;
                };
            }
        }

        private Scene Scene => Property.GetParent<Scene>()!;
        private IKeyframePropertyViewModel ViewModel => (IKeyframePropertyViewModel)DataContext!;
        private IKeyframeProperty Property => ViewModel.Property;

        // icon�̃C�x���g��ǉ�
        private void Add_Handler_Icon(Shape icon)
        {
            icon.PointerPressed += Icon_PointerPressed;
            icon.PointerReleased += Icon_PointerReleased;
            icon.PointerMoved += Icon_PointerMoved;
            icon.PointerLeave += Icon_PointerLeave;
        }

        // icon�̃C�x���g���폜
        private void Remove_Handler_Icon(Shape icon)
        {
            icon.PointerPressed -= Icon_PointerPressed;
            icon.PointerReleased -= Icon_PointerReleased;
            icon.PointerMoved -= Icon_PointerMoved;
            icon.PointerLeave -= Icon_PointerLeave;
        }

        // �^�C�����C���̃X�P�[���ύX
        private void ZoomChange()
        {
            for (var frame = 0; frame < Property.Frames.Count; frame++)
            {
                if (_grid.Children.Count <= frame) break;

                if (_grid.Children[frame] is Shape icon)
                {
                    icon.Margin = new Thickness(Scene.ToPixel(Property.Frames[frame]), 0, 0, 0);
                }
            }

            Width = Scene.ToPixel(Property.GetParent<ClipElement>()!.Length);
        }

        // �L�[�t���[����ǉ�
        public void Add_Frame(object sender, RoutedEventArgs e)
        {
            ViewModel.AddKeyFrameCommand.Execute(_startpos);
        }

        // �L�[�t���[�����폜
        private void Remove_Click(object? sender, RoutedEventArgs e)
        {
            ViewModel.RemoveKeyFrameCommand.Execute(Property.Frames[_grid.Children.IndexOf(_select)]);
        }

        // Icon��PointerPressed�C�x���g
        // �ړ��J�n
        private void Icon_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _startpos = Scene.ToFrame(e.GetPosition(_grid).X);

            _select = (Shape)sender!;

            // �J�[�\���̐ݒ�
            if (_select.Cursor == Cursors.SizeWestEast)
            {
                _grid.Cursor = Cursors.SizeWestEast;
            }

            // �C�x���g�̍폜
            foreach (var icon in _grid.Children.OfType<Shape>().Where(i => i != _select))
            {
                Remove_Handler_Icon(icon);
            }
        }

        // Icon��PointerReleased�C�x���g
        // �ړ��I��
        private void Icon_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            // �J�[�\���̐ݒ�
            _grid.Cursor = Cursors.Arrow;
            if (_select is not null)
            {
                _select.Cursor = Cursors.Arrow;
            }

            // �C�x���g�̒ǉ�
            foreach (var icon in _grid.Children.OfType<Shape>().Where(i => i != _select))
            {
                Add_Handler_Icon(icon);
            }

            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                Icon_PointerLeftReleased(sender, e);
            }
        }

        // Icon��PointerMoved�C�x���g
        private void Icon_PointerMoved(object? sender, PointerEventArgs e)
        {
            _select = (Shape)sender!;

            // �J�[�\���̐ݒ�
            _select.Cursor = Cursors.SizeWestEast;

            // Timeline�̈ꕔ�̑���𖳌���
            Scene.GetCreateTimelineViewModel().KeyframeToggle = false;
        }

        // Icon��PointerLeave�C�x���g
        private void Icon_PointerLeave(object? sender, PointerEventArgs e)
        {
            var senderIcon = (Shape)sender!;

            // �J�[�\���̐ݒ�
            senderIcon.Cursor = Cursors.Arrow;

            // Timeline�̈ꕔ�̑����L����
            Scene.GetCreateTimelineViewModel().KeyframeToggle = true;

            // �C�x���g�̍Đݒ�
            foreach (var icon in _grid.Children.OfType<Shape>().Where(i => i != senderIcon))
            {
                Remove_Handler_Icon(icon);
                Add_Handler_Icon(icon);
            }
        }

        // Icon��PointerLeftReleased�C�x���g
        // �ړ��I��, �ۑ�
        private void Icon_PointerLeftReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_select is not null)
            {
                // �C���f�b�N�X
                var idx = _grid.Children.IndexOf(_select);
                // �N���b�v����̃t���[��
                var frame = Scene.ToFrame(_select.Margin.Left);

                ViewModel.MoveKeyFrameCommand.Execute((idx, frame));
            }
        }

        // grid��PointerMoved�C�x���g (Tunnel)
        // icon��ui��margin��ݒ�
        private void Grid_PointerMovedTunnel(object? sender, PointerEventArgs e)
        {
            if (!(_select is null) && _grid.Cursor == Cursors.SizeWestEast)
            {
                // ���݂̃}�E�X�̈ʒu (frame)
                var now = Scene.ToFrame(e.GetPosition(_grid).X);
                // �N���b�v����̃t���[��
                var a = now - _startpos + Scene.ToFrame(_select.Margin.Left);

                _select.Margin = new Thickness(Scene.ToPixel(a), 0, 0, 0);

                _startpos = now;
            }
        }

        // grid��PointerRightPressed�C�x���g (Tunnel)
        private void Grid_PointerRightPressedTunnel(object? sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint((Avalonia.VisualTree.IVisual?)sender).Properties.IsRightButtonPressed) return;

            // �E�N���b�N -> ���j���[ ->�u�L�[�t���[����ǉ��v�Ȃ̂�
            // ���݈ʒu��ۑ� (frame)
            //_nowframe = Scene.GetCreateTimeLineViewModel().ToFrame(e.GetPosition(grid).X);
            _startpos = Scene.ToFrame(e.GetPosition(_grid).X);
        }

        // grid��PointerReleased�C�x���g (Tunnel)
        private void Grid_PointerReleasedTunnel(object? sender, PointerReleasedEventArgs e)
        {
            // �J�[�\���̐ݒ�
            _grid.Cursor = Cursors.Arrow;
            if (_select is null) return;

            if (_select.Cursor == Cursors.SizeWestEast)
            {
                _grid.Cursor = Cursors.SizeWestEast;
            }
        }

        // grid��PointerLeftPressed�C�x���g (Tunnel)
        private void Grid_PointerLeftPressedTunnel(object? sender, PointerPressedEventArgs e)
        {
            if (_select is null || !e.GetCurrentPoint((Avalonia.VisualTree.IVisual?)sender).Properties.IsLeftButtonPressed) return;

            if (_select.Cursor == Cursors.SizeWestEast)
            {
                // ���݈ʒu��ۑ�
                _startpos = Scene.ToFrame(e.GetPosition(_grid).X);
            }
        }

        // grid��PointerLeave�C�x���g
        public void Grid_PointerLeave(object sender, PointerEventArgs e)
        {
            // �J�[�\���̐ݒ�
            _grid.Cursor = Cursors.Arrow;
            if (_select is null) return;

            if (_select.Cursor == Cursors.SizeWestEast)
            {
                _grid.Cursor = Cursors.SizeWestEast;
            }
        }

        // Icon�̃��j���[���쐬
        private MenuItem CreateMenu()
        {
            var removeMenu = new MenuItem
            {
                Icon = new PathIcon
                {
                    Data = (Geometry)Application.Current.FindResource("Delete20Regular")!,
                    Margin = new Thickness(5, 0, 5, 0)
                },
                Header = Strings.Remove
            };

            removeMenu.Click += Remove_Click;

            return removeMenu;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}