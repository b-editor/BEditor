﻿
using BEditor.Core.Command;
using BEditor.Core.Data.Property;

using System.Reactive.Linq;
using System.Reactive;
using System;

using Reactive.Bindings;
using BEditor.Models;
using BEditor.Models.Extension;
using BEditor.Views.PropertyControls;
using BEditor.Core.Data;

namespace BEditor.ViewModels.PropertyControl
{
    public class DocumentPropertyViewModel
    {
        private string oldvalue;

        public DocumentPropertyViewModel(DocumentProperty property)
        {
            Property = property;
            oldvalue = Property.Value;
            Reset.Subscribe(() => Property.ChangeText(Property.PropertyMetadata?.DefaultText ?? "").Execute());
            Bind.Subscribe(() =>
            {
                var window = new BindSettings(new BindSettingsViewModel<string>(Property));
                window.ShowDialog();
            });
            GotFocus.Subscribe(_ => oldvalue = Property.Value);
            LostFocus.Subscribe(text =>
            {
                Property.Text = oldvalue;

                Property.ChangeText(text).Execute();
            });
            TextChanged.Subscribe(text =>
            {
                Property.Text = text;

                AppData.Current.Project!.PreviewUpdate(Property.GetParent2()!);
            });
        }

        public DocumentProperty Property { get; }
        public ReactiveCommand Reset { get; } = new();
        public ReactiveCommand Bind { get; } = new();
        public ReactiveCommand<string> GotFocus { get; } = new();
        public ReactiveCommand<string> LostFocus { get; } = new();
        public ReactiveCommand<string> TextChanged { get; } = new();
    }
}