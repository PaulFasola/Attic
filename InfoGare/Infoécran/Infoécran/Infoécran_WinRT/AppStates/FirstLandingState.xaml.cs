using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; 
using System.IO;
using System.Linq;
using System.Net.Http;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Fasolib.Controls;
using Fasolib.Interfaces;
using Fasolib.Managers;
using Infoécran.Classes;
using Infoécran.Classes.Animations;
using Infoécran.Classes.Animations.Helpers;
using Infoécran.Classes.Presenters;
using Infoécran.Helpers;
using Infoécran.UserControl;
using Infoécran.Windows.UserControl;
using Infoécran_WinRT.UserControl;
using Newtonsoft.Json;
using ErrorManager = Infoécran.Classes.ErrorManager;
using lang = Windows.ApplicationModel; 

namespace Infoécran.AppStates
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FirstLandingState : Page
    {
        private InfoGare _infoGare;
        private ObservableCollection<GareSuggestionPresenter> _suggestionItems;
        private DispatcherTimer _timer;
        private int _geoApiCallTentative = 0;
        private bool PaneOpened = false;
        public SideScrollAnimation _animation { get; set; }
        private VirtualWindowsManager _virtualWindow;
        private About _aboutuc; 

        public FirstLandingState()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.SizeChanged += Current_SizeChanged;
           
            SuggestionBoxProposer.Visibility = Visibility.Collapsed;
            GeoLoadingModule.Visibility = Visibility.Collapsed;
            _virtualWindow = new VirtualWindowsManager();

            _aboutuc = new About(); 
        }

        void Current_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var currentViewState = ApplicationView.GetForCurrentView().Orientation.ToString();
     
            if (currentViewState == "Portrait")
            {
                SPContainer.Orientation = Orientation.Vertical;
                logo.Visibility = Visibility.Visible;
                logo.Height = (Window.Current.Bounds.Height);
                 
                Maingrid.Margin = new Thickness(0, Window.Current.Bounds.Width / 2.8, 0, 0);

                var cmove = SPContainer.Children[0] as Grid;
                AdaptBackgroundImage(Stretch.UniformToFill);

                if (cmove != null && (string)cmove.Tag == "InfoAppli")
                {
                    SPContainer.Children.RemoveAt(0);
                    SPContainer.Children.Insert(1, cmove);
                }

                MainPane.Width = Window.Current.Bounds.Width;
                MainPane.Height = Window.Current.Bounds.Height;
                 
            }

            if (currentViewState == "Landscape")
            {
                logo.Visibility = Visibility.Collapsed; 
                AdaptBackgroundImage(Stretch.UniformToFill);
                 

                SPContainer.Orientation = Orientation.Horizontal;
                var cmove = SPContainer.Children[0] as Grid;

                if (cmove != null && (string)cmove.Tag != "InfoAppli")
                {
                    SPContainer.Children.RemoveAt(0);
                    SPContainer.Children.Insert(1, cmove);
                } 

                MainPane.Width = (Window.Current.Bounds.Width / 3) * 2;
                MainPane.Height = Window.Current.Bounds.Height;

                Searchbox.Width = ((Window.Current.Bounds.Width / 3) * 2) - 85;  
            }

            VisualStateManager.GoToState(this, currentViewState, true);
        }

        private void AdaptBackgroundImage(Stretch stretch)
        {
            var img = MainPane.Background as ImageBrush;
            if (img == null) return;

            var source = img.ImageSource as BitmapImage;

            MainPane.Background = new ImageBrush() { ImageSource = source, Stretch = stretch };
        }

        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Frame.BackStack.Clear();
            var loader = new lang.Resources.ResourceLoader();

            Searchbox.Text = "";
            LoadingText.Text = loader.GetString("LoadingStations");

            LoadingModule.Visibility = Visibility.Visible;
            Maingrid.Visibility = Visibility.Collapsed;
            _infoGare = new InfoGare();
            MainLoader.Visibility = Visibility.Collapsed;

            var rand = new Random().Next(1, 5);
            if (rand == 4) rand = 99;
            MainPane.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Slide/" + rand + ".jpg"))
            };

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("GeoStation"))
            {
                GeoLoadingText.Text = loader.GetString("LoadingGeoStation");
                GeoLoadingModule.Visibility = Visibility.Visible;
                await LoadGeoStations();
                GeoLoadingModule.Visibility = Visibility.Collapsed;
            }

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("NoData"))
            {
                await new MessageDialog(loader.GetString("NoData")).ShowOrWaitAsync();
                ApplicationData.Current.LocalSettings.Values.Remove("NoData");
            }

            await _infoGare.Init(); 

            _suggestionItems = Gare.GetStationsSuggestion(_infoGare.Gares);
            LoadingModule.Visibility = Visibility.Collapsed;
            Maingrid.Visibility = Visibility.Visible;

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("AppJustLaunched"))
            {
                Localise_Click(null, null);
                ApplicationData.Current.LocalSettings.Values.Remove("AppJustLaunched");
            }
        } 

        private string Filter(string gare)
        {
            var stack = gare.Split();
            string str = "", mainCaption = "";
            foreach (var element in stack)
            {
                str = element.Trim();
                str = str.Replace("gare", "");
                str = str.Replace("pont", "");
                str = str.Replace("de", "");
                str = str.Replace("du", "");
                str = str.Replace("le", "");
                str = str.Replace("la", "");
                str = str.Replace("les", "");

                if (str != "" && str.Length > 2) mainCaption += str;
            }
            return mainCaption;
        }

     
        private async Task PreloadBoard(string trigramme, string line, GareSuggestionPresenter item)
        {
            var error = false;
            _infoGare = new InfoGare();
            var loader = new lang.Resources.ResourceLoader();
            try
            {
                _infoGare = await new Mission().GetBoard(trigramme, line);
                LoadingModule.Visibility = Visibility.Collapsed;
                if (ApplicationView.GetForCurrentView().Orientation.ToString() == "Portrait") await new MessageDialog(loader.GetString("InfoAppliPopup")).ShowOrWaitAsync();
                this.Frame.Navigate(typeof(InfoScreenState), new Tuple<InfoGare, string, GareSuggestionPresenter>(_infoGare, line, item));
            }
            catch (Exception e)
            {
                ErrorManager.Log(e);
                error = true;
                LoadingModule.Visibility = Visibility.Collapsed;
            }
            if (error) await new MessageDialog(loader.GetString("NoInternet")).ShowOrWaitAsync();
        }

         
         
        private async void Localise_Click(object sender, RoutedEventArgs e)
        {
            double lat, lon;

            var loader = new lang.Resources.ResourceLoader();
            GeoLoadingText.Text = loader.GetString("Localizing");
            GeoLoadingModule.Visibility = Visibility.Visible;

            Geoposition position;
            var str = "";
            try
            {
                var geolocator = new Geolocator { DesiredAccuracy = PositionAccuracy.Default };
                position = await geolocator.GetGeopositionAsync(new TimeSpan(0, 3, 20), new TimeSpan(0, 0, 35));
            }
            catch (Exception)
            {
                new MessageDialog(loader.GetString("CantLocalize")).ShowOrWaitAsync();
                return;
            }

            var coordinate = position.Coordinate; 
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("GeoStations.json", CreationCollisionOption.OpenIfExists);
            var text = await FileIO.ReadTextAsync(file);

            if (text.Length < 20)
            {
                LoadingText.Text = loader.GetString("LoadingGeoStation");
                var load = await LoadGeoStations();
                if (load)
                {
                    file = await ApplicationData.Current.LocalFolder.CreateFileAsync("GeoStations.json", CreationCollisionOption.OpenIfExists);
                    text = await FileIO.ReadTextAsync(file);
                }
                else
                {
                    await new MessageDialog(loader.GetString("CantLocalize")).ShowOrWaitAsync();
                }

            }

            var stack = JsonConvert.DeserializeObject<GeoProvider>(text);

            lat = coordinate.Latitude;
            lon = coordinate.Longitude;

            double tempDistance = 999;

            LoadingText.Text = loader.GetString("LookGare");

            foreach (var item in stack.elements)
            {
                if (item.tags == null || item.tags.typeRATP != "rer" || item.lat.ToString() == "" || item.lon.ToString() == "") continue;
                var latJson = double.Parse(item.lat.ToString());
                var lonJson = double.Parse(item.lon.ToString());
                var distance = Converter.CoodinateToDistance(lat, lon, latJson, lonJson);
                if (item.tags.name != null && tempDistance >= distance)
                {
                    str = item.tags.name;
                    tempDistance = distance;
                }
            }

            var city = str.Replace("-", " ");
            var rcity = "";
            var citySplitted = city.Split(' ');
            var counter = 0;
            foreach (var element in citySplitted)
            {
                counter++;
                rcity += " " + element;
                if (counter == 3) break;
            }

            if (rcity.Length < 1 && !ApplicationData.Current.LocalSettings.Values.ContainsKey("AppJustLaunched")) await new MessageDialog(loader.GetString("CantLocalize")).ShowOrWaitAsync();

            GeoLoadingModule.Visibility = Visibility.Collapsed;
            Searchbox.Text = rcity;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("AppJustLaunched"))
            {
                Searchbox.Focus(FocusState.Keyboard); 
            }
            else
            {
                this.Focus(FocusState.Programmatic);
            }
        }

        private async Task<bool> LoadGeoStations()
        {
            var file =
                await
                    ApplicationData.Current.LocalFolder.CreateFileAsync("GeoStations.json",
                        CreationCollisionOption.ReplaceExisting);

            var httpclient = new HttpClient();
            string content;
            try
            {
                var response =
                await
                   httpclient.GetAsync(
                       "http://oapi-fr.openstreetmap.fr/oapi/interpreter?data=[out:json];node[%22type:RATP%22~%22rer%22];out;way[%22type:RATP%22~%22metro|rer|tram%22];out;%3E;out%20skel;");
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                ErrorManager.Log(e);
                return false;
            }

            await FileIO.WriteTextAsync(file, content);
            file = null;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("GeoStation"))
            {
                ApplicationData.Current.LocalSettings.Values.Add("GeoStation", true);
            }
            return true;
        }
         


        private async Task FillWitHistory()
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
                await FillWitHistory();
        }

        private void MainPane_Tapped(object sender, TappedRoutedEventArgs e)
        {
        }
 

        private void SuggestionBoxProposer_LostFocus(object sender, RoutedEventArgs e)
        {
            SuggestionBoxProposer.Visibility = Visibility.Collapsed;
        }

        private async void Searchbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Searchbox.Text == "")
            {
                await FillWitHistory();
            }

            var displayableGare = new ObservableCollection<GareSuggestionPresenter>();
            var lastStack = new ObservableCollection<GareSuggestionPresenter>();
            foreach (var element in _suggestionItems)
            {
                var userEntry = element.GareName.Replace('-', ' ');
                userEntry = Searchbox.Text.Replace('\'', ' ');
                userEntry = userEntry.ToLower();
                userEntry.RemoveAccents();
                userEntry = Filter(userEntry);


                var gare = element.GareName.Replace('-', ' ');
                gare = gare.Replace('\'', ' ');
                gare = gare.ToLower();
                gare.RemoveAccents();
                gare = Filter(gare);


                if (userEntry == gare)
                {
                    displayableGare.Insert(0, element);
                }
                else
                {
                    if (!displayableGare.Contains(element))
                    {
                        if (gare.StartsWith(userEntry) && !displayableGare.Contains(element))
                            displayableGare.Add(element);
                        else
                        {
                            var stack = userEntry.Split();
                            foreach (var item in stack)
                            {
                                if (item.Length < 2) continue;
                                var titem = item.Trim();
                                if (gare.StartsWith(titem) && !displayableGare.Contains(element))
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
                ContentRoot.Children.Add(wb);
            }
            else
            {
                await
                    new MessageDialog("Warning: too much windows openned. Please, close one of to go !").ShowOrWaitAsync();
            }
        }

        private async void SuggestionBoxProposer_SelectionChanged(object sender, SelectionChangedEventArgs arg)
        {
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
                return;
            }


            LoadingText.Text = loader.GetString("LoadingInfoGare");
            LoadingModule.Visibility = Visibility.Visible;


            var line = Path.GetFileNameWithoutExtension(selectedItem.Logo);
            Searchbox.Text = selectedItem.GareName + " (Ligne " + line + ")";

            await ObjectManager.Insert("history.bin", selectedItem);
            await PreloadBoard(selectedItem.Trigramme, line, selectedItem);
        }

        private void About_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TryCreateWindow(_aboutuc, "About");
        }

    }
}
