using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Fasolib.Managers;
using Infogare.Classes.Presenters;
using InfoGare.AppStates;

namespace InfoGare.UserControls
{
    public sealed partial class FavoritePaneUC : Windows.UI.Xaml.Controls.UserControl
    {
        private FirstLandingState _instance;

        public FavoritePaneUC(FirstLandingState instance)
        {
            this.InitializeComponent();
            this.SizeChanged += Current_SizeChanged;
            this.Loaded += async (sender, args) =>
            {
                await GetFavorites();
            };

            _instance = instance;
        }

        private async Task GetFavorites()
        {
            var content =
                (List<GareSuggestionPresenter>)
                    await ObjectManager.DeserializeToObject<List<GareSuggestionPresenter>>("Favorite.bin");

            if (content != null && content.Any())
            {
                FavoriteComponent.ItemsSource = content;
            }
        }

        private async void FavoriteComponent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var loader = ResourceLoader.GetForCurrentView();
            var selectedItem = FavoriteComponent.SelectedItem as GareSuggestionPresenter;
            _instance.LoadingText.Text = loader.GetString("LoadingInfoGare");
            _instance.LoadingModule.Visibility = Visibility.Visible;
            if (selectedItem == null) return;
            var line = Path.GetFileNameWithoutExtension(selectedItem.Logo);
            _instance.SearchBar.SuggestionBox.Text = selectedItem.GareName + " (Ligne " + line + ")";

            await _instance.PreloadBoard(selectedItem.Trigramme, line, selectedItem);
        }


        private void Current_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var currentViewState = ApplicationView.GetForCurrentView().Orientation.ToString();

            FavoriteTitle.Foreground = currentViewState == "Portrait" ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
        }

    }
}
