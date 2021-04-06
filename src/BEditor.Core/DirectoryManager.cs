﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BEditor
{
    /// <summary>
    /// Represents a class that manages directories.
    /// </summary>
    public class DirectoryManager
    {
        /// <summary>
        /// Gets a default <see cref="DirectoryManager"/> instance.
        /// </summary>
        public static readonly DirectoryManager Default = new();
        private readonly Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryManager"/> class.
        /// </summary>
        public DirectoryManager() : this(new())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryManager"/> class.
        /// </summary>
        public DirectoryManager(List<string> directories)
        {
            Directories = directories;
            _timer = new()
            {
                Interval = 2500,
            };
            _timer.Elapsed += Timer_Elapsed;
        }

        /// <summary>
        /// Gets the directories to manage.
        /// </summary>
        public List<string> Directories { get; }

        /// <summary>
        /// Gets the running status of <see cref="DirectoryManager"/>.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Run the <see cref="DirectoryManager"/>.
        /// </summary>
        public void Run()
        {
            if (!IsRunning)
            {
                foreach (var dir in Directories)
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }

                _timer.Start();

                IsRunning = true;
            }
        }

        /// <summary>
        /// Stop the <see cref="DirectoryManager"/>.
        /// </summary>
        public void Stop()
        {
            if (IsRunning)
            {
                _timer.Stop();

                IsRunning = false;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach(var dir in Directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }
    }
}
