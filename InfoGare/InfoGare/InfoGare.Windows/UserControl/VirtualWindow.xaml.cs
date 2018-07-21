using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Fasolib.Interfaces;
using InfoGare.Classes;

namespace InfoGare.UserControl
{
    public sealed partial class VirtualWindow : IVirtualWindow
    {
        internal string Guid { get; set; }
        private TranslateTransform _dragTranslation;
        public bool IsVisible { get; set; }
        public string Id { get; set; }
        private VirtualWindowsManager _virtualWindowsManager;


        public VirtualWindow(VirtualWindowsManager virtualWindowsManager)
        {
            InitializeComponent();

            VirtualWindowUC.ManipulationDelta += VirtualWindowOnManipulationDelta;
            _virtualWindowsManager = virtualWindowsManager;
            _dragTranslation = new TranslateTransform();
            VirtualWindowUC.RenderTransform = _dragTranslation;
        }

        private async void VirtualWindowOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs manipulationDeltaRoutedEventArgs)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
            () =>
            {
                _dragTranslation.X += manipulationDeltaRoutedEventArgs.Delta.Translation.X;
                _dragTranslation.Y += manipulationDeltaRoutedEventArgs.Delta.Translation.Y;
            });
        }

        internal void SetContent(object content, string title)
        {
            this.WindowBody.Children.Clear();
            Title.Text = title;

            if (content is Uri)
            {
                var webPage = new WebView();

                try
                {
                    webPage.Navigate((Uri)content);
                }
                catch (Exception)
                {
                    CloseWindow_Tapped(null, null);
                }

                this.WindowBody.Children.Add(webPage);
            }
            else if (content is IVirtualWindowElement)
            {
                if (content is UIElement)
                {
                    var child = (UIElement)content;
                    try
                    {
                        WindowBody.Children.Add(child);
                    }
                    catch (Exception)
                    { }
                }
                else
                {
                    throw new Exception("Not an UIElement.");
                }
            }
            else
            {
                throw new Exception("Content type not handled");
            }
        }

        private void CloseWindow_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Close();
        }

        public void Close()
        {
            try
            {
                _virtualWindowsManager.UnRegister(this);
                this.WindowBody.Children.RemoveAt(0);
            }
            catch (Exception)
            {

            }

            var grid = (this.Parent) as Grid;
            grid?.Children.Remove(this);
        }

        internal void Dispose()
        {
            this.WindowBody.Children.Clear();
        }

        private void Viewbox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}