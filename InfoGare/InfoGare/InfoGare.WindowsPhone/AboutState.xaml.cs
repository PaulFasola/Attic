using System;
using System.Globalization;
using System.IO;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Store;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace InfoGare.AppStates
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutState : Page
    {
        public AboutState()
        {
            this.InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait |
                                                         DisplayOrientations.PortraitFlipped;

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
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait
                                                         | DisplayOrientations.PortraitFlipped
                                                         | DisplayOrientations.Landscape
                                                         | DisplayOrientations.LandscapeFlipped;
        }

        private async void Twitter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var status = await Launcher.LaunchUriAsync(new Uri("https://mobile.twitter.com/paulfasola"));
            if (!status) await new MessageDialog("Erreur lors de la tentative d'ouverture de la page").ShowAsync();
        }

        private async void Linkedin_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var status = await Launcher.LaunchUriAsync(new Uri("https://fr.linkedin.com/pub/paul-fasola/97/4b8/927"));
            if (!status) await new MessageDialog("Erreur lors de la tentative d'ouverture de la page").ShowAsync();
        }

        private async void Bug_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var em = new EmailMessage();
            em.To.Add(new EmailRecipient("support-wp@paulfasola.fr"));
            em.Subject = "Sujet...";
            em.Body = "Email...\n\n\n\n\n --- Don't remove below there --- \n\n ETIC : " + Path.GetRandomFileName() + " Public : " + "{aoe4-rt7-1@a}\n\n---";
            await EmailManager.ShowComposeNewEmailAsync(em);
             
        }

        private async void OtherApps_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("zune:search?publisher=Paul Fasola"));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        private async void Changelog_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var status = await Launcher.LaunchUriAsync(new Uri("http://winapps.paulfasola.fr/" + "Infogare/cl.php?lang=" + CultureInfo.CurrentCulture));
            if (!status) await new MessageDialog("Erreur lors de la tentative d'ouverture de la page").ShowAsync();
        }

        private async void RateApp_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }
    }
}
