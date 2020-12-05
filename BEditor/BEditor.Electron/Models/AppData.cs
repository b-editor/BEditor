﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BEditor.Core;
using BEditor.Core.Data;
using BEditor.Core.Plugin;
using BEditor.Core.Service;

using Reactive.Bindings;

namespace BEditor.Models
{
    public class AppData : BasePropertyChanged, IApplication
    {
        private AppData()
        {

        }

        public static AppData Current { get; } = new();
        public Status AppStatus { get; set; }
        public List<IPlugin> LoadedPlugins { get; }
        public ReactiveProperty<Project> Project { get; } = new();
    }
}