﻿using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using BEditor.Models;
using BEditor.ViewModels.Helper;
using BEditor.Core.Extensions.ViewCommand;
using BEditor.Core.Plugin;
using BEditor.Core.Data;
using System.ComponentModel;

namespace BEditor.ViewModels.SettingsControl.Plugins
{
    public class InstalledPluginsViewModel : BasePropertyChanged
    {
        private static readonly PropertyChangedEventArgs selectArgs = new(nameof(SelectPlugin));
        private IPlugin selectplugin;

        public InstalledPluginsViewModel()
        {

            SettingClick.Subscribe(_ =>
            {
                Message.Snackbar(SelectPlugin?.PluginName);

                SelectPlugin.SettingCommand();
            });
        }

        public IPlugin SelectPlugin { get => selectplugin; set => SetValue(value, ref selectplugin, selectArgs); }
        public DelegateCommand<object> SettingClick { get; } = new DelegateCommand<object>();
    }
}
