using System;
using System.ServiceModel.Channels;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Fasolib.Classes;
using Fasolib.Helpers;
using Fasolib.Interfaces;
using InfoGare.AppStates;
using lang = Windows.ApplicationModel;

namespace InfoGare.UserControl
{
    public sealed partial class Settings : Windows.UI.Xaml.Controls.UserControl, IVirtualWindowElement
    {
        private FirstLandingState _instance;
        private bool _intialization;

        public Settings(FirstLandingState firstLandingState)
        {
            this.InitializeComponent();

            _intialization = true;
            WelcomeAnimation.IsOn = Utilities.GetSetting<bool>("WelcomeAnimation");
            Colorblind.IsOn = Utilities.GetSetting<bool>("ColorBlindMode");
            _intialization = false;

            _instance = firstLandingState;
        }

        private void WelcomeAnimation_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_intialization) return;
            _instance.SetAnimation(WelcomeAnimation.IsOn ? "Dynamic" : "Static");
            Utilities.SetSetting("WelcomeAnimation", WelcomeAnimation.IsOn);
        }

        private void Colorblind_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Utilities.SetSetting("ColorBlindMode", Colorblind.IsOn);
        }

        private async void DelFav_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            IStorageFile file = null;

            var failure = false;
            var noFav = false;

            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync("Favorite.bin");
            }
            catch (Exception)
            {
                noFav = true;
            }

            if (noFav)
            {
                await new MessageDialog(lang.Resources.ResourceLoader.GetForCurrentView().GetString("NoFavorites")).ShowOrWaitAsync();
                return;
            }

            try
            {
                await file.DeleteAsync();
            }
            catch (Exception)
            {
                failure = true;
            }

            if (failure)
                await new MessageDialog(lang.Resources.ResourceLoader.GetForCurrentView().GetString("Error")).ShowOrWaitAsync();
            else
                await new MessageDialog(lang.Resources.ResourceLoader.GetForCurrentView().GetString("FavClearSuccess")).ShowOrWaitAsync();
        }
    }
}
