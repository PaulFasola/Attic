using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Infoécran.AppStates;
using Infoécran.Classes;
using Infoécran.Classes.Presenters;
using Infoécran.Helpers;
using lang=Windows.ApplicationModel;
 
namespace Infoécran.UserControl
{
    public sealed partial class Favorites : global::Windows.UI.Xaml.Controls.UserControl
    {
        private Frame _frame;

        public Favorites()
        {
            this.InitializeComponent();
        }

        public void setContext(Frame frame)
        {
            _frame = frame;
        }

        private async Task GetFavorites()
        {
            var content = (List<GareSuggestionPresenter>)await ObjectManager.DeserializeToObject<List<GareSuggestionPresenter>>("Favorite.bin");

            if (content != null && content.Any())
            {
                FavoriteComponent.ItemsSource = content;
            }
        }

        
        private async Task PreloadBoard(string trigramme, string line, GareSuggestionPresenter item)
        {
            var error = false;
            var _infoGare = new InfoGare();
            var loader = new lang.Resources.ResourceLoader();
            try
            { 
                _infoGare = await new Mission().GetBoard(trigramme, line); 
                if (ApplicationView.GetForCurrentView().Orientation.ToString() == "Portrait") await new MessageDialog(loader.GetString("InfoAppliPopup")).ShowOrWaitAsync();
                _frame.Navigate(typeof(InfoScreenState), new Tuple<InfoGare, string, GareSuggestionPresenter>(_infoGare, line, item));
            }
            catch (Exception e)
            {
                ErrorManager.Log(e);
                error = true; 
            }
            if (error) await new MessageDialog(loader.GetString("NoInternet")).ShowOrWaitAsync();
        }

        private void FavoriteComponent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


    }
}
