﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using BEditor.Core.Data;
using BEditor.Core.Data.Property;
using BEditor.Core.Extensions;
using BEditor.Core.Properties;
using BEditor.Core.Service;

using Microsoft.Win32;

using Reactive.Bindings;

namespace BEditor.Models
{
    public class ProjectModel
    {
        public static readonly ProjectModel Current = new();

        private ProjectModel()
        {
            SaveAs.Where(_ => AppData.Current.Project is not null)
                .Select(_ => AppData.Current.Project)
                .Subscribe(p => p!.SaveAs());

            Save.Where(_ => AppData.Current.Project is not null)
                .Select(_ => AppData.Current.Project)
                .Subscribe(p => p!.Save());

            Open.Select(_ => AppData.Current).Subscribe(app =>
            {
                var dialog = new OpenFileDialog()
                {
                    Filter = $"{Resources.ProjectFile}|*.bedit|{Resources.BackupFile}|*.backup|{Resources.JsonFile}|*.json",
                    RestoreDirectory = true
                };

                if (dialog.ShowDialog() ?? false)
                {
                    try
                    {
                        app.Project?.Unload();
                        var project = new Project(dialog.FileName);
                        project.Load();
                        app.Project = project;
                        app.AppStatus = Status.Edit;

                        Settings.Default.MostRecentlyUsedList.Remove(dialog.FileName);
                        Settings.Default.MostRecentlyUsedList.Add(dialog.FileName);
                    }
                    catch
                    {
                        Debug.Assert(false);
                        Message.Snackbar(string.Format(Resources.FailedToLoad, "Project"));
                    }
                }
            });

            Close.Select(_ => AppData.Current)
                .Where(app => app.Project is not null)
                .Subscribe(app =>
            {
                app.Project?.Unload();
                app.Project = null;
                app.AppStatus = Status.Idle;
            });

            Create.Subscribe(_ =>
            {
                AppData.Current.Project?.Unload();
                CreateEvent?.Invoke(this, EventArgs.Empty);
            });
        }

        public event EventHandler? CreateEvent;

        public ReactiveCommand SaveAs { get; } = new();
        public ReactiveCommand Save { get; } = new();
        public ReactiveCommand Open { get; } = new();
        public ReactiveCommand Close { get; } = new();
        public ReactiveCommand Create { get; } = new();
    }
}