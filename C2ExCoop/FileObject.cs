using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RM2ExCoop.C2ExCoop
{
    internal class FileObject
    {
        readonly string _path;
        readonly string[] _lines;
        readonly List<Func<string, string>> _operations;

        public FileObject(string path)
        {
            _path = path;
            _lines = File.ReadAllLines(path);
            _operations = new();
        }

        public FileObject Replace(Regex pattern, string replacement)
        {
            _operations.Add(line => pattern.Replace(line, replacement));

            return this;
        }

        public void ApplyAndSave(string? newPath = null)
        {
            string[] newLines = new string[_lines.Length];

            for (int i = 0; i < _lines.Length; ++i)
            {
                newLines[i] = _lines[i];
                foreach (var func in _operations)
                    newLines[i] = func(newLines[i]);
            }

            File.Delete(newPath ?? _path);
            File.WriteAllLines(newPath ??  _path, newLines);
        }
    }
}
