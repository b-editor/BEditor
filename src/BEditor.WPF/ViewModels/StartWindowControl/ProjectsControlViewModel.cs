﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using BEditor.Data;
using BEditor.Models;
using BEditor.Properties;

using Reactive.Bindings;

namespace BEditor.ViewModels.StartWindowControl
{
    public sealed class ProjectsControlViewModel
    {
        public ProjectsControlViewModel()
        {
            CountIsZero = new(!Settings.Default.RecentlyUsedFiles
                .Where(i => File.Exists(i))
                .Any());
            CountIsNotZero = CountIsZero.Select(i => !i).ToReadOnlyReactivePropertySlim();

            Projects = new(Settings.Default.RecentlyUsedFiles
                .Where(i => File.Exists(i))
                .Select(i => new ProjectItem(Path.GetFileNameWithoutExtension(i), i, Click, Remove))
                .Reverse());

            Click.Subscribe(async ProjectItem =>
            {
                ProjectItem.IsLoading.Value = true;

                var app = AppData.Current;
                app.Project?.Unload();
                var project = Project.FromFile(ProjectItem.Path, app);

                if (project is null)
                {
                    ProjectItem.IsLoading.Value = false;

                    return;
                }

                await Task.Run(async () =>
                {
                    project.Load();

                    app.Project = project;
                    app.AppStatus = Status.Edit;

                    Settings.Default.RecentlyUsedFiles.Remove(ProjectItem.Path);
                    Settings.Default.RecentlyUsedFiles.Add(ProjectItem.Path);

                    await App.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var win = new MainWindow();
                        App.Current.MainWindow = win;
                        win.Show();

                        Close?.Invoke(this, EventArgs.Empty);
                    });
                });
            });

            Create.Subscribe(() =>
            {
                ProjectModel.Current.Create.Execute();

                if (AppData.Current.Project is not null)
                {
                    var win = new MainWindow();
                    App.Current.MainWindow = win;
                    win.Show();

                    Close?.Invoke(this, EventArgs.Empty);
                }
            });
            Remove.Subscribe(item =>
            {
                Projects.Remove(item);
                Settings.Default.RecentlyUsedFiles.Remove(item.Path);

                if (Projects.Count is 0)
                {
                    CountIsZero.Value = true;
                }
            });
            Add.Subscribe(() =>
            {
                var record = new OpenFileRecord()
                {
                    Filters =
                    {
                        new(Strings.ProjectFile, new FileExtension[] { new("bedit") })
                    }
                };


                if (AppData.Current.FileDialog.ShowOpenFileDialog(record))
                {
                    var f = Projects.Count is 0;
                    Projects.Insert(0, new ProjectItem(Path.GetFileNameWithoutExtension(record.FileName), record.FileName, Click, Remove));
                    Settings.Default.RecentlyUsedFiles.Add(record.FileName);

                    if (f)
                    {
                        CountIsZero.Value = false;
                    }
                }
            });

            Search.Subscribe(str =>
            {
                if (str is null) return;

                foreach (var item in Projects)
                {
                    item.Visibility.Value = Visibility.Visible;
                }

                if (string.IsNullOrWhiteSpace(str)) return;

                var regexPattern = Regex.Replace(str, ".", m =>
                {
                    string s = m.Value;
                    if (s.Equals("?"))
                    {
                        return ".";
                    }
                    else if (s.Equals("*"))
                    {
                        return ".*";
                    }
                    else
                    {
                        return Regex.Escape(s);
                    }
                });
                var regex = new Regex(regexPattern.ToLowerInvariant());

                foreach (var item in Projects.Where(item => !regex.IsMatch(item.Name.ToLowerInvariant())).ToArray())
                {
                    item.Visibility.Value = Visibility.Collapsed;
                }
            });
        }

        public ReactiveCommand<ProjectItem> Click { get; } = new();
        public ReactiveCommand<ProjectItem> Remove { get; } = new();
        public ReactiveCommand Create { get; } = new();
        public ReactiveCommand Add { get; } = new();
        public ObservableCollection<ProjectItem> Projects { get; }
        public ReactivePropertySlim<bool> CountIsZero { get; }
        public ReadOnlyReactivePropertySlim<bool> CountIsNotZero { get; }
        public ReactivePropertySlim<string> Search { get; } = new();

        public event EventHandler? Close;

        public record ProjectItem(string Name, string Path, ICommand Command, ICommand Remove)
        {
            public string? ThumbnailPath
                => System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path)!, "thumbnail.png") is var p && File.Exists(p) ? p : null;

            public ReactivePropertySlim<bool> IsLoading { get; } = new(false);

            public ReactivePropertySlim<Visibility> Visibility { get; } = new(System.Windows.Visibility.Visible);
        }
    }
}