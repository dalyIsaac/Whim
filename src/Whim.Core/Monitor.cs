using System.Windows.Forms;

namespace Whim.Core
{
    public class Monitor : IMonitor
    {
        internal Screen Screen { get;  }

        public Monitor(Screen screen)
        {
            Screen = screen;
        }

        public string Name => Screen.DeviceName;
        public int Width => Screen.WorkingArea.Width;
        public int Height => Screen.WorkingArea.Height;
        public int X => Screen.WorkingArea.X;
        public int Y => Screen.WorkingArea.Y;
     }
}
