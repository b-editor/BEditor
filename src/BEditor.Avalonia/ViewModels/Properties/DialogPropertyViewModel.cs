﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BEditor.Data.Property;

using Reactive.Bindings;

namespace BEditor.ViewModels.Properties
{
    public sealed class DialogPropertyViewModel
    {
        public DialogPropertyViewModel(DialogProperty property)
        {
            Property = property;
            ClickCommand.Subscribe(Property.Show);
        }

        public DialogProperty Property { get; }
        public ReactiveCommand ClickCommand { get; } = new();
    }
}