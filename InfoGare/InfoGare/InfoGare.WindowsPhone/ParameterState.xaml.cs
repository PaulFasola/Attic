using System;
using System.Globalization;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Fasolib.Helpers;
using Infogare.Classes.Helpers;
using InfoGare.Classes;

namespace InfoGare.AppStates
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ParameterState : Page
    {
        private IPropertySet _appData = ApplicationData.Current.LocalSettings.Values;
        public ParameterState()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
            {
                if (rootFrame.CanGoBack)
                {
                    e.Handled = true;
                    rootFrame.GoBack();
                } 
            }
        } 

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (StoreHelper.IsPremium()) RemoveAds.Visibility = Visibility.Collapsed;
            DidUKnow.IsOn = _appData.ContainsKey("DidUKnowToggled") && (bool)_appData["DidUKnowToggled"];
            Localiser.IsOn = _appData.ContainsKey("CanLocalise") && !(bool)_appData["CanLocalise"];
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        private async void RemoveAds_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await StoreHelper.TryBuy("RemoveAds");
            await new MessageDialog(ResourceLoader.GetForCurrentView().GetString("ActionTerminated")).ShowOrWaitAsync();
        }

        private async void ClearFav_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IStorageFile file = null;
            try
            {
                 file = await ApplicationData.Current.LocalFolder.GetFileAsync("Favorite.bin");
            }
            catch (Exception)
            {
                return; 
            } 
            await file.DeleteAsync();
            await new MessageDialog(ResourceLoader.GetForCurrentView().GetString("ActionTerminated")).ShowOrWaitAsync();
        }

        private async void ClearHist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IStorageFile file = null;
            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync("History.bin");
            }
            catch (Exception)
            {
                return;
            }
            await file.DeleteAsync();
            await new MessageDialog(ResourceLoader.GetForCurrentView().GetString("ActionTerminated")).ShowOrWaitAsync();
        }

        private void DidUKnow_Toggled(object sender, RoutedEventArgs e)
        {
            if (!DidUKnow.IsOn)
            {
                if (_appData.ContainsKey("DidUKnowToggled")) _appData.Remove("DidUKnowToggled");
            }
            else
            {
                if (!_appData.ContainsKey("DidUKnowToggled")) 
                    _appData.Add("DidUKnowToggled", true);
                else
                {
                    _appData["DidUKnowToggled"] = true;
                }
            }
        }

        private void Localiser_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Localiser.IsOn)
            {
                if (_appData.ContainsKey("CanLocalise")) _appData.Remove("CanLocalise");
            }
            else
            {
                if (!_appData.ContainsKey("CanLocalise"))
                    _appData.Add("CanLocalise", true);
                else
                {
                    _appData["CanLocalise"] = true;
                }
            }
        }

        private async void Privacy_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var status = await Launcher.LaunchUriAsync(new Uri("http://winapps.paulfasola.fr/Infogare/privacy?lang=" + CultureInfo.CurrentCulture));
            if (!status) await new MessageDialog("Erreur lors de la tentative d'ouverture de la page").ShowAsync();
        }
    }
}
