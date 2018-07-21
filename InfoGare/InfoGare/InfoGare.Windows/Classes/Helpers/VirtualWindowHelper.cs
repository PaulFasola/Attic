using Fasolib.Enums;
using VirtualWindow = InfoGare.UserControl.VirtualWindow;

namespace InfoGare.Classes.Helpers
{
    public static class VirtualWindowHelper
    {
        public static void Show(this VirtualWindow window)
        {
            if (window.IsVisible) return; 
                window.IsVisible = true;
        }

        public static void Hide(this VirtualWindow window, CacheMode mode = CacheMode.Normal)
        {
            if (!window.IsVisible) return;

            window.IsVisible = false;

            if (mode == CacheMode.None)
                window.Close();
            if (mode == CacheMode.Persistent)
            {
                //@TODO
            }
        }

        public static void Destroy(this VirtualWindow window)
        {
            window.Close();
        }
    }
}