using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using BEditor.Data.Property;
using BEditor.ViewModels.Properties;

namespace BEditor.Views.Properties
{
    public class ColorPropertyView : UserControl, IDisposable
    {
        public ColorPropertyView()
        {
            InitializeComponent();
        }

        public ColorPropertyView(ColorProperty property)
        {
            DataContext = new ColorPropertyViewModel(property);
            InitializeComponent();
        }

        ~ColorPropertyView()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            DataContext = null;
            GC.SuppressFinalize(this);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}