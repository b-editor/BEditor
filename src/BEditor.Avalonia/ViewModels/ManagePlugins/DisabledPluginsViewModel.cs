﻿using System;
using System.Linq;
using System.Reactive.Linq;

using Reactive.Bindings;

namespace BEditor.ViewModels.ManagePlugins
{
    public class DisabledPluginsViewModel
    {
        public DisabledPluginsViewModel()
        {
            Enable.Where(_ => SelectName.Value is not null)
                .Subscribe(_ =>
            {
                BEditor.Settings.Default.EnablePlugins.Add(SelectName.Value);
                BEditor.Settings.Default.DisablePlugins.Remove(SelectName.Value);

                BEditor.Settings.Default.Save();
            });
            IsSelected = SelectName.Select(n => n is not null).ToReadOnlyReactivePropertySlim();
        }

        public ReactivePropertySlim<string> SelectName { get; } = new();
        public ReadOnlyReactivePropertySlim<bool> IsSelected { get; }
        public ReactiveCommand Enable { get; } = new();
    }
}