using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using BEditor.Command;
using BEditor.Data;
using BEditor.Models;
using BEditor.Properties;
using BEditor.ViewModels;
using BEditor.ViewModels.DialogContent;
using BEditor.Views.DialogContent;

using OpenTK.Audio.OpenAL;

namespace BEditor
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            var vm = MainWindowViewModel.Current;
            AddHandler(KeyDownEvent, Window_KeyDown, RoutingStrategies.Tunnel);
            vm.New.Subscribe(CreateProjectClick);

            InitializeComponent();
            // Windows�����ƕ\�����o�O��̂ő΍�
            MainWindowViewModel.Current.IsOpened
                .ObserveOn(AvaloniaScheduler.Instance)
                .Where(_ => OperatingSystem.IsWindows())
                .Subscribe(isopened =>
            {
                var content = (Grid)Content!;
                if (isopened)
                {
                    content.Margin = new(0, 0, 8, 0);
                }
                else
                {
                    content.Margin = default;
                }
            });
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void Window_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Source != this) return;

            for (var i = 0; i < KeyBindingModel.Bindings.Count; i++)
            {
                var kb = KeyBindingModel.Bindings[i];
                if (kb.ToKeyGesture().Matches(e))
                {
                    kb.Command?.Command.Execute(null);
                }
            }
        }

        public void ObjectsPopupOpen(object s, RoutedEventArgs e)
        {
            this.FindControl<Popup>("ObjectsPopup").Open();
        }

        public void ObjectStartDrag(object s, PointerPressedEventArgs e)
        {
            this.FindControl<Popup>("ObjectsPopup").Close();
            if (s is Control ctr && ctr.DataContext is ObjectMetadata metadata)
            {
                var data = new DataObject();
                data.Set("ObjectMetadata", metadata);
                DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
            }
        }

        public async void CreateProjectClick(object s)
        {
            var viewmodel = new CreateProjectViewModel();
            var dialog = new CreateProject
            {
                DataContext = viewmodel
            };

            await dialog.ShowDialog((Window)VisualRoot);
        }

        protected override async void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            await CheckOpenALAsync();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private static async Task CheckOpenALAsync()
        {
            try
            {
                _ = AL.GetError();
            }
            catch
            {
                await AppModel.Current.Message.DialogAsync(Strings.OpenALNotFound);
                App.Shutdown(1);
            }
        }
    }
}