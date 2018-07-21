using System;
using System.Globalization;
using Windows.ApplicationModel.Store;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Fasolib.Enums;
using Fasolib.Helpers;
using Fasolib.Interfaces;
using Fasolib.Managers;
using InfoGare.Classes;
using lang = Windows.ApplicationModel; 

namespace InfoGare.UserControl
{
    public sealed partial class About : global::Windows.UI.Xaml.Controls.UserControl, IVirtualWindowElement
    {
        public About()
        {
            this.InitializeComponent(); 
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait |
                                                         DisplayOrientations.PortraitFlipped; 
            Removeads.Visibility = StoreHelper.IsPremium() ? Visibility.Collapsed : Visibility.Visible;
        }
            
        private async void Twitter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var status = await Launcher.LaunchUriAsync(new Uri("https://twitter.com/paulfasola/"));
            if (!status) await new MessageDialog("Erreur lors de la tentative d'ouverture de la page").ShowAsync();
        }

        private async void Linkedin_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var status = await Launcher.LaunchUriAsync(new Uri("https://fr.linkedin.com/in/paulfasola"));
            if (!status) await new MessageDialog("Erreur lors de la tentative d'ouverture de la page").ShowAsync();
        }

        private  void Bug_Tapped(object sender, TappedRoutedEventArgs e)
        { 
        }

        private async void OtherApps_Tapped(object sender, TappedRoutedEventArgs  e)
        {
            await Launcher.LaunchUriAsync(new Uri("zune:search?publisher=Paul Fasola"));
        }

        private async void Changelog_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var status = await Launcher.LaunchUriAsync(new Uri("http://winapps.paulfasola.fr/Infogare/cl.php?lang=" + CultureInfo.CurrentCulture));
            if (!status) await new MessageDialog("Erreur lors de la tentative d'ouverture de la page").ShowAsync();
        }

        private async void RateApp_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }

        public bool IsVisible { get; set; }

        private async void RateApp_Click(object sender, global::Windows.UI.Xaml.RoutedEventArgs e)
        { 
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:PDP?PFN=91699302-516e-4ba3-b02b-7d7bd9eb5158_h1mwxa553cm64"));
        } 

        private async void RemoveAd_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var state = await StoreHelper.TryBuy("RemoveAds");
            var lg = lang.Resources.ResourceLoader.GetForCurrentView();
            switch (state)
            { 
                case StoreResponse.Bought:
                    await new MessageDialog(lg.GetString("BuySuccess")).ShowOrWaitAsync();
                    Removeads.Visibility = StoreHelper.IsPremium() ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case StoreResponse.AlreadyBought:
                    await new MessageDialog(lg.GetString("AlreadyBuy")).ShowOrWaitAsync();
                    break;
                case StoreResponse.ServerError:
                    if(NetworkingManager.IsNetworkAvailable())
                        await new MessageDialog(lg.GetString("BuyError")).ShowOrWaitAsync();
                    else
                        await new MessageDialog(lg.GetString("NoData")).ShowOrWaitAsync();
                    break;
            }
        }
    }
}
