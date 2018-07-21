using System;
using System.Globalization;
using Windows.ApplicationModel.Store;
using lang = Windows.ApplicationModel; 
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Fasolib.Interfaces;
using Infoécran.Classes;

namespace Infoécran.Windows.UserControl
{
    public sealed partial class About : global::Windows.UI.Xaml.Controls.UserControl, IVirtualWindow
    {
        public About()
        {
            this.InitializeComponent();

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait |
                                                         DisplayOrientations.PortraitFlipped;

            Removeads.Visibility = Utilities.IsOnePaid() ? Visibility.Collapsed : Visibility.Visible;
        }
            
        private async void Twitter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var status = await Launcher.LaunchUriAsync(new Uri("https://mobile.twitter.com/"));
            if (!status) await new MessageDialog("Erreur lors de la tentative d'ouverture de la page").ShowAsync();
        }

        private async void Linkedin_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var status = await Launcher.LaunchUriAsync(new Uri("http://touch.www.linkedin.com"));
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

        private void RateApp_Click(object sender, global::Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void Removeads_Click(object sender, global::Windows.UI.Xaml.RoutedEventArgs e)
        {

        }
    }
}
