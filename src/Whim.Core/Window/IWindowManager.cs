using System;

namespace Whim.Core.Window
{
    public interface IWindowManager : ICommandable, IDisposable
    {
        public bool Initialize();

        public event WindowAddDelegate WindowAdded;
    }
}