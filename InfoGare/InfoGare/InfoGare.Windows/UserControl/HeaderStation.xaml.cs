using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Fasolib.Helpers;
using Fasolib.Managers;
using Infogare.Classes.Presenters;
using InfoGare.AppStates;
using InfoGare.Classes;
using lang = Windows.ApplicationModel;

namespace InfoGare.UserControl
{
    public sealed partial class HeaderStation
    {
        private Frame _frame;
        private GareSuggestionPresenter _gare;

        public HeaderStation()
        {
            this.InitializeComponent();
        }

        internal void Initialize(string stationName, bool isSNCF, Frame frame)
        {
            _frame = frame;
            //Sncf.Visibility = isSNCF ? Visibility.Visible : Visibility.Collapsed; 
            StationName.Text = stationName;

            this.Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                Favorite.Fill = await IsInFavorite(_gare)
                    ? new SolidColorBrush(Colors.YellowGreen)
                    : new SolidColorBrush(Colors.White);
            };
        }

        private void backButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _frame.Navigate(typeof(FirstLandingState));
        }


        internal void AttachStation(GareSuggestionPresenter gareSuggestionPresenter)
        {
            _gare = gareSuggestionPresenter;
        }

        private async void Viewbox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!StoreHelper.IsPremium())
            {
                await new MessageDialog(lang.Resources.ResourceLoader.GetForCurrentView().GetString("NotPremium")).ShowOrWaitAsync();
                return;
            }

            var data = (List<GareSuggestionPresenter>)await ObjectManager.DeserializeToObject<List<GareSuggestionPresenter>>("Favorite.bin") ??
                       new List<GareSuggestionPresenter>();

            if (await IsInFavorite(_gare))
            {
                data.RemoveAll(x => x.GareName == _gare.GareName);
                Favorite.Fill = new SolidColorBrush(Colors.White);
            }
            else
            {
                data.Add(_gare);
                Favorite.Fill = new SolidColorBrush(Colors.Yellow);
            }

            await ObjectManager.SerializeToFile(data, "Favorite.bin");
        }


        private async Task<bool> IsInFavorite(GareSuggestionPresenter gare)
        {
            var data = (List<GareSuggestionPresenter>)await ObjectManager.DeserializeToObject<List<GareSuggestionPresenter>>("Favorite.bin");

            if (data == null || !data.Any())
                return false;
            else
            {
                if (data.Exists(x => x.GareName == _gare.GareName))
                    return true;
                else
                    return false;
            }
        }
    }
}
