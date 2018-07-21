using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Fasolib.Helpers;
using Fasolib.Interfaces;
using Fasolib.Managers;
using Infogare.Classes.Helpers;
using Infogare.Classes.Models;
using Infogare.Classes.Presenters;
using InfoGare.AppStates;
using InfoGare.Classes.Helpers;
using lang = Windows.ApplicationModel;

namespace InfoGare.UserControl
{
    public sealed partial class Favorites : IVirtualWindowElement
    {
        private Frame _frame;

        public Favorites(List<GareSuggestionPresenter> content)
        {
            this.InitializeComponent();

            FavoriteComponent.ItemsSource = content;
        }

        public void setContext(Frame frame)
        {
            _frame = frame;
        }

        public async Task GetFavorites()
        {
            var content = (List<GareSuggestionPresenter>)await ObjectManager.DeserializeToObject<List<GareSuggestionPresenter>>("Favorite.bin");

            if (content != null && content.Any())
            {
                FavoriteComponent.ItemsSource = content;
            }
            else
            {
                FavoritePane.Children.Clear();
                FavoritePane.Children.Add(new TextBlock() { Text = lang.Resources.ResourceLoader.GetForCurrentView().GetString("NoFavorites"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
            }
        }


        private async Task PreloadBoard(string trigramme, string line, GareSuggestionPresenter item)
        {
            var error = false;
            var _infoGare = new InfoGareHelper();
            var loader = new lang.Resources.ResourceLoader();
            try
            {
                _infoGare = await new Mission().GetBoard(trigramme, line);
                if (ApplicationView.GetForCurrentView().Orientation.ToString() == "Portrait") await new MessageDialog(loader.GetString("InfoAppliPopup")).ShowOrWaitAsync();
                _frame.Navigate(typeof(InfoScreenState), new Tuple<InfoGareHelper, string, GareSuggestionPresenter>(_infoGare, line, item));
            }
            catch (Exception e)
            {
                ErrorManager.Log(e);
                error = true;
            }
            if (error) await new MessageDialog(loader.GetString("NoInternet")).ShowOrWaitAsync();
        }

        private async void FavoriteComponent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            var selectedItem = FavoriteComponent.SelectedItem as GareSuggestionPresenter;
            if (selectedItem == null) return;
            var line = Path.GetFileNameWithoutExtension(selectedItem.Logo);
            await PreloadBoard(selectedItem.Trigramme, line, selectedItem);
        }
    }
}
