using System;
using System.Collections.Generic;

namespace Whim.Core.Window
{
    public interface IWindow
    {
        public bool IsIgnored { get; }
        public IntPtr Handle { get; }
        public string Title { get; }
        public string Class { get; }
        public IWindowLocation Location { get; }
        public HashSet<string> Tags { get; }
    }
}