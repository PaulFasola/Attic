using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Fasolib.Exceptions;
using Fasolib.Managers;
using InfoGare.UserControl;

namespace InfoGare.Classes
{
    /// <summary>
    ///    Constructor
    /// </summary>  
    public class VirtualWindowsManager
    {
        private readonly Dictionary<string, UserControl.VirtualWindow> _openedWindows;
        private readonly int _windowsLimit;
        private Fasolib.Interfaces.IVirtualWindowElement _pageState;

        /// <summary>
        ///    Constructor
        /// </summary> 
        /// <param name="maxWindows">The max. amount of displayed windows</param> 
        public VirtualWindowsManager(int maxWindows)
        {
            _windowsLimit = maxWindows;
            _openedWindows = new Dictionary<string, VirtualWindow>();
        }

        /// <summary>
        ///    Create a new virtual window with specified params
        /// </summary> 
        /// <param name="content">The content of the window (must implement IVirtualWindow)</param>
        /// <param name="title">The name of the window</param>
        /// <param name="valing">Specify the vertical position (relative to the parent)</param>
        /// <param name="halign">Specify the horizontal position (relative to the parent)</param>
        /// <returns>A new instance of VirtualWindow</returns>
        public VirtualWindow Create(object content, string title, VerticalAlignment valing, HorizontalAlignment halign)
        {
            if (_openedWindows.Count > _windowsLimit) return null;
            var state = content as Fasolib.Interfaces.IVirtualWindowElement;
            if (state != null)
                _pageState = state;
            else
                throw new UnhandledTypeException();

            var vb = new VirtualWindow(this) { HorizontalAlignment = halign, VerticalAlignment = valing };
            vb.SetContent(content, title);

            vb.Id = Register(vb);

            return vb;
        }

        /// <summary>
        ///    Register a virtual window
        /// </summary>
        /// <param name="window">The virtual window to register for display</param>
        /// <returns>The Guid of the diplayed window</returns> 
        private string Register(UserControl.VirtualWindow window)
        {
            var guid = Guid.NewGuid().ToString();
            _openedWindows.Add(guid, window);
            return guid;
        }

        /// <summary>
        ///    Unregister a displayed virtual window 
        /// </summary>
        /// <param name="window">The registered virtual window</param>
        /// <param name="shouldDestroy">If the windows must be destroyed or kept in cache</param> 
        public void UnRegister(UserControl.VirtualWindow window)
        {
            if (window.Id == null)
                throw new VirtualWindowNotFoundException();

            if (_openedWindows.ContainsKey(window.Id))
                _openedWindows.Remove(window.Id);
        }
    }
}
