using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Fasolib.Classes;
using Fasolib.Helpers;
using Fasolib.Interfaces;
using Fasolib.Managers;
using Infogare.Classes;
using Infogare.Classes.Animations;
using Infogare.Classes.Helpers;
using Infogare.Classes.Managers;
using Infogare.Classes.Models;
using Infogare.Classes.Presenters;
using InfoGare.Classes;
using InfoGare.Classes.Helpers;
using InfoGare.UserControl;
using Newtonsoft.Json;
using lang = Windows.ApplicationModel;

namespace InfoGare.AppStates
{
    public sealed partial class FirstLandingState : Page
    {
        private InfoGareHelper _infoGare;
        private ObservableCollection<GareSuggestionPresenter> _suggestionItems;
        public SideScrollAnimation _animation { get; set; }
        private VirtualWindowsManager _virtualWindow;
        private About _aboutuc;
        private bool loading;

        public FirstLandingState()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            this.SizeChanged += Current_SizeChanged;

            SuggestionBoxProposer.Visibility = Visibility.Collapsed;
            SuggestionBoxProposer.Visibility = Visibility.Collapsed;

            _virtualWindow = new VirtualWindowsManager(3);
            _aboutuc = new About();

            CheckPremium();
            CheckAnimation();
            
        }


        private void CheckPremium()
        {
            if (StoreHelper.IsPremium())
            {
                principalWinRTAd.Visibility = Visibility.Collapsed;
                Favorite.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                principalWinRTAd.Visibility = Visibility.Visible;
                Favorite.Foreground = new SolidColorBrush(Colors.LightCoral);
            }
        }

        void Current_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var currentViewState = ApplicationView.GetForCurrentView().Orientation.ToString();

            Media.Width = Window.Current.Bounds.Width;
            Media.Height = Window.Current.Bounds.Height;

            VisualStateManager.GoToState(this, GetState(e.NewSize.Width), true);
            // ScreenWidth.Text = e.NewSize.Width.ToString();
            textBlock.Text = GetState(e.NewSize.Width);
        }

        private string GetState(double width)
        {
            if (width <= 500)
                return "Snapped";
            if (width <= 660)
                return "Extra_Small";
            if (width <= 755)
                return "Small";
            if (width <= 992)
                return "Reduced";
            return "Default";
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var failure = false;

            this.Frame.BackStack.Clear();
            var loader = new lang.Resources.ResourceLoader();
            SuggestionBoxProposer.Visibility = Visibility.Collapsed;

            if (!Utilities.ExistSetting("WelcomeAnimation"))
            {
                Media.AutoPlay = true;
                Media.Play();
            }

            Searchbox.Focus(FocusState.Keyboard);
            Searchbox.Text = "";
            _infoGare = new InfoGareHelper();

            if (Utilities.ExistSetting("NoData"))
            {
                await new MessageDialog(loader.GetString("NoData")).ShowOrWaitAsync();
                ApplicationData.Current.LocalSettings.Values.Remove("NoData");
                loading = false;
            }

            try
            {
                await _infoGare.Init();
            }
            catch (Exception)
            {
                failure = true;
            }

            if (failure)
            {
                layergrid.Visibility = Visibility.Visible;
                layertext.Text = lang.Resources.ResourceLoader.GetForCurrentView().GetString("CrucialDataError");
                layertext.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                layertext.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                await new MessageDialog(lang.Resources.ResourceLoader.GetForCurrentView().GetString("NoData")).ShowOrWaitAsync();
                layergrid.Visibility = Visibility.Collapsed;
                loading = false;
                return;
            }

            _suggestionItems = GareHelper.GetStationsSuggestion(_infoGare.Gares);

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("AppJustLaunched"))
            {
                try
                {
                    Searchbox.Text = await Localize();
                }
                catch (Exception)
                {

                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("AppJustLaunched"))
                {
                    Searchbox.Focus(FocusState.Keyboard);
                }
                else
                {
                    this.Focus(FocusState.Programmatic);
                }

                ApplicationData.Current.LocalSettings.Values.Remove("AppJustLaunched");
            }
            Grid_Tapped(this, null);
        }

        private void CheckAnimation()
        {
            if (Utilities.ExistSetting("WelcomeAnimation"))
            {
                SetAnimation(Utilities.GetSetting<bool>("WelcomeAnimation") ? "Dynamic" : "Static");
            }
        }

        private string Filter(string gare)
        {
            var stack = gare.Split();
            var mainCaption = "";
            foreach (var element in stack)
            {
                var str = element.Trim();
                str = str.Replace("gare", "");
                str = str.Replace("de", "");
                str = str.Replace("le", "");
                str = str.Replace("la", "");
                str = str.Replace("les", "");

                if (str != "" && str.Length > 2) mainCaption += str;
            }
            return mainCaption;
        }

        public async Task PreloadBoard(string trigramme, string line, GareSuggestionPresenter item)
        {
            var error = false;
            _infoGare = new InfoGareHelper();
            var loader = new lang.Resources.ResourceLoader();
            try
            {
                _infoGare = await new Mission().GetBoard(trigramme, line);
                if (ApplicationView.GetForCurrentView().Orientation.ToString() == "Portrait") await new MessageDialog(loader.GetString("InfoAppliPopup")).ShowOrWaitAsync();

                loading = false;
                layergrid.Visibility = Visibility.Collapsed;

                this.Frame.Navigate(typeof(InfoScreenState), new Tuple<InfoGareHelper, string, GareSuggestionPresenter>(_infoGare, line, item));
            }
            catch (Exception)
            {
                error = true;
            }
            if (error) await new MessageDialog(loader.GetString("NoInternet")).ShowOrWaitAsync();
            layergrid.Visibility = Visibility.Collapsed;
            loading = false;
        }

        private async Task<string> Localize()
        {
            Geoposition position = null;
            double lat, lon, tempDistance = 99;
            string str = "", rcity = "";
            var counter = 0;
            var failure = false;
            var loader = new lang.Resources.ResourceLoader();

            try
            {
                var geolocator = new Geolocator { DesiredAccuracy = PositionAccuracy.Default };
                position = await geolocator.GetGeopositionAsync(new TimeSpan(0, 3, 20), new TimeSpan(0, 0, 35));
            }
            catch (Exception)
            {
                failure = true;
            }

            if (failure)
            {
                await new MessageDialog(loader.GetString("CantLocalize")).ShowOrWaitAsync();
                return null;
            }

            var coordinate = position.Coordinate;
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/GeoProvider.json")).AsTask().ConfigureAwait(false);
            var text = new StreamReader(await file.OpenStreamForReadAsync().ConfigureAwait(false)).ReadToEnd();

            GeoProvider stack;

            try
            {
                stack = JsonConvert.DeserializeObject<GeoProvider>(text);
            }
            catch (Exception)
            {
                return null;
            }


            lat = coordinate.Point.Position.Latitude;
            lon = coordinate.Point.Position.Longitude;


            foreach (var item in stack.elements)
            {
                if (item.tags == null || item.tags.typeRATP != "rer" || item.lat.ToString() == "" || item.lon.ToString() == "") continue;
                var latJson = double.Parse(item.lat.ToString());
                var lonJson = double.Parse(item.lon.ToString());
                var distance = MathHelper.CoodinateToDistance(lat, lon, latJson, lonJson);
                if (item.tags.name != null && tempDistance >= distance)
                {
                    str = item.tags.name;
                    tempDistance = distance;
                }
            }

            var city = str.Replace("-", " ");
            var citySplitted = city.Split(' ');

            foreach (var element in citySplitted)
            {
                counter++;
                rcity += " " + element;
                if (counter == 3) break;
            }

            if (rcity.Length < 1 && !ApplicationData.Current.LocalSettings.Values.ContainsKey("AppJustLaunched"))
                await new MessageDialog(loader.GetString("CantLocalize")).ShowOrWaitAsync();

            return rcity;
        }


        private async Task FillWithHistory()
        {
            var content = (ObservableCollection<GareSuggestionPresenter>)await ObjectManager.DeserializeToObject<ObservableCollection<GareSuggestionPresenter>>("history.bin");
            var loader = new lang.Resources.ResourceLoader();

            if (content != null && content.Any())
            {
                content.Insert(0, new GareSuggestionPresenter(null, loader.GetString("ShowAllStations"), ""));
                SuggestionBoxProposer.ItemsSource = content;
            }
        }

        private async void SuggestionBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Searchbox.Text.Length == 0)
                await FillWithHistory();
        }


        private void SuggestionBoxProposer_LostFocus(object sender, RoutedEventArgs e)
        {
            SuggestionBoxProposer.Visibility = Visibility.Collapsed;
        }

        private async void Searchbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suggestionItems == null || loading) return;
            if (Searchbox.Text == "") await FillWithHistory();

            var displayableGare = new ObservableCollection<GareSuggestionPresenter>();
            var lastStack = new ObservableCollection<GareSuggestionPresenter>();
            foreach (var element in _suggestionItems)
            {
                var userEntry = element.GareName.Replace('-', ' ');
                userEntry = Searchbox.Text.Replace('\'', ' ');
                userEntry = userEntry.ToLower();
                userEntry = userEntry.RemoveAccents();
                userEntry = Filter(userEntry);

                var gare = element.GareName.Replace('-', ' ');
                gare = gare.Replace('\'', ' ');
                gare = gare.ToLower();
                gare = gare.RemoveAccents();
                gare = Filter(gare);


                if (userEntry == gare)
                {
                    displayableGare.Insert(0, element);
                }
                else
                {
                    if (!displayableGare.Contains(element))
                    {
                        if (gare.StartsWith(userEntry) && displayableGare.Contains(element))
                            displayableGare.Add(element);
                        else
                        {
                            var stack = userEntry.Split();
                            foreach (var item in stack)
                            {
                                if (item.Length < 2) continue;
                                if (gare.StartsWith(item.Trim()) && !displayableGare.Contains(element))
                                    displayableGare.Add(element);
                            }
                        }
                    }
                }
            }
            if (lastStack.Count > 0)
            {
                displayableGare = new ObservableCollection<GareSuggestionPresenter>(displayableGare.Concat(lastStack));
                lastStack = null;
            }
            SuggestionBoxProposer.ItemsSource = displayableGare;
            SuggestionBoxProposer.Visibility = Visibility.Visible;
        }

        private async void TryCreateWindow(object content, string title)
        {
            var state = content as IVirtualWindow;
            if (state != null)
            {
                if (state.IsVisible) return;
                state.IsVisible = true;
            }

            var wb = _virtualWindow.Create(content, title, VerticalAlignment.Center, HorizontalAlignment.Center);
            if (wb != null)
            {
                MainPane.Children.Add(wb);
            }
            else
            {
                await new MessageDialog("Warning: too much windows openned. Please, close one of to go !").ShowOrWaitAsync();
            }
        }

        private async void SuggestionBoxProposer_SelectionChanged(object sender, SelectionChangedEventArgs arg)
        {
            if (SuggestionBoxProposer.SelectedIndex == -1) return;
            SuggestionBoxProposer.Visibility = Visibility.Collapsed;

            loading = true;
            layergrid.Visibility = Visibility.Visible;
            layertext.Text = lang.Resources.ResourceLoader.GetForCurrentView().GetString("Loading");

            var loader = new lang.Resources.ResourceLoader();
            this.Focus(FocusState.Programmatic);
            var selectedItem = SuggestionBoxProposer.SelectedItem as GareSuggestionPresenter;

            if (_suggestionItems.IndexOf(selectedItem) == -1)
            {
                SuggestionBoxProposer.ItemsSource = _suggestionItems;
                SuggestionBoxProposer.Focus(FocusState.Programmatic);
                Searchbox.Text = "";
            }

            if (selectedItem == null || (selectedItem.Logo == null && selectedItem.Trigramme == ""))
            {
                Searchbox.Text = "";
                return;
            }

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                await new MessageDialog(lang.Resources.ResourceLoader.GetForCurrentView().GetString("NoInternet")).ShowOrWaitAsync();
                layergrid.Visibility = Visibility.Collapsed;
                loading = false;
                return;
            }

            var line = Path.GetFileNameWithoutExtension(selectedItem.Logo);
            Searchbox.Text = selectedItem.GareName + " (Ligne " + line + ")";

            await BinaryManager.Insert("history.bin", selectedItem); 
            await PreloadBoard(selectedItem.Trigramme, line, selectedItem);
        }


        private async void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                Searchbox.Text = await Localize();
            }
            catch (Exception)
            {

            }

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("AppJustLaunched"))
            {
                Searchbox.Focus(FocusState.Keyboard);
            }
            else
            {
                this.Focus(FocusState.Programmatic);
            }
        }

        private async void About_Btn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await TryCreateWindow(new About());
        }

        private async Task TryCreateWindow(object o)
        {
            var window = _virtualWindow.Create(o, "About", VerticalAlignment.Center, HorizontalAlignment.Center);

            if (window != null)
            {
                MainPane.Children.Add(window);
            }
            else
            {
                await new MessageDialog(lang.Resources.ResourceLoader.GetForCurrentView().GetString("WindowError")).ShowOrWaitAsync();
            }
        }

        private async void Settings_Btn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await TryCreateWindow(new Settings(this));
        }

        private async void Favorite_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!StoreHelper.IsPremium())
            {
                await new MessageDialog(lang.Resources.ResourceLoader.GetForCurrentView().GetString("NotPremium")).ShowOrWaitAsync();
                return;
            }
            var content = (List<GareSuggestionPresenter>)await ObjectManager.DeserializeToObject<List<GareSuggestionPresenter>>("Favorite.bin");

            if (content != null && content.Any())
            {
                var favorite = new Favorites(content);
                await favorite.GetFavorites();
                favorite.setContext(this.Frame);
                TryCreateWindow(favorite, lang.Resources.ResourceLoader.GetForCurrentView().GetString("FavStations"));
            }
            else
            {
                await new MessageDialog(lang.Resources.ResourceLoader.GetForCurrentView().GetString("NoFavorites")).ShowOrWaitAsync();
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SuggestionBoxProposer.Visibility = Visibility.Collapsed;
        }

        public void SetAnimation(string animation)
        {
            if (animation == "Dynamic")
            {
                Media.Visibility = Visibility.Visible;
                Media.AutoPlay = true;
                Media.Play();
            }
            else
            {
                Media.Stop();
                Media.Visibility = Visibility.Collapsed;
                var rand = Utilities.GetRandom(1, 4);

                MainPane.Background = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Slide/" + rand + ".jpg"))
                };
            }
        }
    }
}
