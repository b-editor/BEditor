using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using BEditor.Properties;

using Reactive.Bindings;

using static BEditor.IMessage;

namespace BEditor.Views.DialogContent
{
    public class ProgressDialog : Window
    {
        public ProgressDialog()
        {
            DataContext = this;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public ProgressDialog(ButtonType[] buttons)
        {
            DataContext = this;

            InitializeComponent();

            var stack = this.FindControl<VirtualizingStackPanel>("stack");

            foreach (var button in buttons)
            {
                var text = button switch
                {
                    ButtonType.Ok => Strings.OK,
                    ButtonType.Yes => Strings.Yes,
                    ButtonType.No => Strings.No,
                    ButtonType.Cancel => Strings.Cancel,
                    ButtonType.Retry => Strings.Retry,
                    ButtonType.Close => Strings.Close,
                    _ => string.Empty,
                };

                var button_ = new Button
                {
                    Background = Brushes.Transparent,
                    Content = text,
                    CommandParameter = button
                };

                stack.Children.Add(button_);
            }

            for (var i = 0; i < stack.Children.Count; i++)
            {
                var b = (Button)stack.Children[i];
                b.Click += (sender, e) =>
                {
                    Close(b.CommandParameter);
                };
            }
        }

        public ReactiveProperty<string> Text { get; } = new();
        public ReactiveProperty<bool> IsIndeterminate { get; } = new() { Value = false };

        public ReactiveProperty<int> Maximum { get; } = new() { Value = 0 };
        public ReactiveProperty<int> Minimum { get; } = new() { Value = 0 };
        public ReactiveProperty<int> NowValue { get; } = new() { Value = 0 };

        public ButtonType DialogResult { get; private set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
           (Width, Height) = (480, 128);
        }
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            var screen = Screens.ScreenFromVisual(this).Bounds;
            var x = (screen.Width - Width) / 2;
            var y = (screen.Height - Height) / 2;

            Position = new((int)x, (int)y);
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
