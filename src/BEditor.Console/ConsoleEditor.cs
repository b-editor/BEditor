﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.CommandLineUtils;
using Microsoft.CodeAnalysis.CSharp.Scripting;

using BEditor.Data;
using BEditor.Properties;
using System.Text.RegularExpressions;
using System.IO;
using BEditor.Data.Property;
using Microsoft.CodeAnalysis.Scripting;

namespace BEditor
{
    public class ConsoleEditor
    {
        private readonly ScriptContext _context;

        public ConsoleEditor(string file)
        {
            Project = Project.FromFile(file, App.Current) ?? throw new Exception();
            Project.Load();

            _context = new(Project);
        }

        public Project Project { get; }

        public void Execute()
        {
            while (true)
            {
                var line = Console.ReadLine();

                if (line is not null)
                {
                    if (line is "exit") return;

                    try
                    {
                        CSharpScript.RunAsync(line, globals: _context).Wait();
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("スクリプトを実行出来ませんでした");

                        Console.ResetColor();

                        throw;
                    }
                }
            }
        }
    }
}