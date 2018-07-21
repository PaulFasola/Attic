using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Fasolib.Classes;
using Fasolib.Helpers;
using Fasolib.Managers;
using Infogare.Classes;
using Infogare.Classes.Presenters;
using InfoGare.AppStates;
using InfoGare.Classes.Helpers;
using Newtonsoft.Json;
using ErrorManager = Fasolib.Managers.ErrorManager;

namespace InfoGare.UserControls
{
    public sealed partial class SearchBar : UserControl
    {

        private ObservableCollection<GareSuggestionPresenter> _suggestionItems;
        private FirstLandingState _instance;

        public SearchBar(FirstLandingState instance)
        {
            this.InitializeComponent();
            _instance = instance;

            _suggestionItems = GareHelper.GetStationsSuggestion(_instance.InfoGareInstance.Gares);
        }

        private async void SuggestionBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SuggestionBox.Text.Length == 0)
                await FillWithHistory();
        }

        private async void SuggestionBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (SuggestionBox.Text.Length > 0)
                SuggestionBox_TextChanged(null, null);
            else
            {
                await FillWithHistory();
            }
        }

        private async Task FillWithHistory()
        {
            var content = (ObservableCollection<GareSuggestionPresenter>)await ObjectManager.DeserializeToObject<ObservableCollection<GareSuggestionPresenter>>("history.bin");

            if (content != null && content.Any())
            {
                SuggestionBox.ItemsSource = content;
            }
            else
            {
                SuggestionBox.ItemsSource = _instance.InfoGareInstance.MissionStack;
            }
        }

        private async void SuggestionBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var loader = new ResourceLoader();
            this.Focus(FocusState.Programmatic);
            var selectedItem = args.SelectedItem as GareSuggestionPresenter;
            SuggestionBox.AutoMaximizeSuggestionArea = true;

            if (selectedItem != null && selectedItem.GareName == loader.GetString("ShowAllStations"))
            {
                SuggestionBox.ItemsSource = _suggestionItems;
            }

            if (selectedItem == null || (selectedItem.Logo == null && selectedItem.Trigramme == ""))
            {
                SuggestionBox.Text = "";
                return;
            }

            var line = Path.GetFileNameWithoutExtension(selectedItem.Logo);
            SuggestionBox.Text = selectedItem.GareName + " (Ligne " + line + ")";

            await InfoScreenState.Insert("history.bin", selectedItem);
            await _instance.PreloadBoard(selectedItem.Trigramme, line, selectedItem);
        }

        private static string Filter(string gare)
        {
            var stack = gare.Split();
            var mainCaption = "";
            foreach (var element in stack)
            {
                var str = element.Trim();
                if (str != "" && str.Length > 2) mainCaption += str;
            }
            return mainCaption;
        }

        private async void SuggestionBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (SuggestionBox.Text == "")
                await FillWithHistory();

            var displayableGare = new ObservableCollection<GareSuggestionPresenter>();
            var lastStack = new ObservableCollection<GareSuggestionPresenter>();

            foreach (var element in _suggestionItems)
            {
                var userEntry = element.GareName.Replace('-', ' ');
                userEntry = SuggestionBox.Text.Replace('\'', ' ');
                userEntry = userEntry.ToLower();
                userEntry = userEntry.RemoveAccents();
                userEntry = userEntry.RemoveBeginningWhiteSpace();
                userEntry = Filter(userEntry);


                var gare = element.GareName.Replace('-', ' ');
                gare = gare.Replace('\'', ' ');
                gare = gare.ToLower();
                gare = gare.RemoveAccents();
                gare = gare.RemoveBeginningWhiteSpace();
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
            }
            SuggestionBox.ItemsSource = displayableGare;
            SuggestionBox.IsSuggestionListOpen = true;
        }

        private async void Localise_Click(object sender, RoutedEventArgs e)
        {
            double lat, lon;

            var loader = new ResourceLoader();
            _instance.GeoLoadingText.Text = loader.GetString("Localizing");
            _instance.GeoLoadingModule.Visibility = Visibility.Visible;

            Geoposition position = null;
            var error = false;
            var str = "";
            try
            {
                var geolocator = new Geolocator { DesiredAccuracy = PositionAccuracy.Default };
                position = await geolocator.GetGeopositionAsync(new TimeSpan(0, 3, 20), new TimeSpan(0, 0, 35));
            }
            catch (Exception y)
            {
                ErrorManager.Log(y);
                error = true;
                _instance.GeoLoadingModule.Visibility = Visibility.Collapsed;
            }

            if (error)
            {
                await new MessageDialog(loader.GetString("CantLocalize")).ShowOrWaitAsync();
                _instance.GeoLoadingModule.Visibility = Visibility.Collapsed;
                return;
            }

            var coordinate = position.Coordinate;
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("GeoStations.json", CreationCollisionOption.OpenIfExists);
            var text = await FileIO.ReadTextAsync(file);

            if (text.Length < 20)
            {
                _instance.LoadingText.Text = loader.GetString("LoadingGeoStation");
                var load = await LoadGeoStations();
                if (load)
                {
                    file = await ApplicationData.Current.LocalFolder.CreateFileAsync("GeoStations.json", CreationCollisionOption.OpenIfExists);
                    text = await FileIO.ReadTextAsync(file);
                }
                else
                {
                    await new MessageDialog(loader.GetString("CantLocalize")).ShowOrWaitAsync();
                    _instance.GeoLoadingModule.Visibility = Visibility.Collapsed;
                    return;

                }
            }

            var stack = JsonConvert.DeserializeObject<GeoProvider>(text);

            lat = coordinate.Point.Position.Latitude;
            lon = coordinate.Point.Position.Longitude;

            double tempDistance = 999;

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

            _instance.GeoLoadingModule.Visibility = Visibility.Collapsed;
            SuggestionBox.Text = rcity;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("AppJustLaunched"))
            {
                SuggestionBox.Focus(FocusState.Keyboard);
                SuggestionBox_TextChanged(null, null);
            }
            else
            {
                this.Focus(FocusState.Programmatic);
            }
        }

        public async Task<bool> LoadGeoStations()
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("GeoStations.json", CreationCollisionOption.ReplaceExisting);

            var httpclient = new HttpClient();
            string content;
            try
            {
                var response = await httpclient.GetAsync("http://api.openstreetmap.fr/oapi/interpreter?data=[out:json];node[%22type:RATP%22~%22rer%22];out;way[%22type:RATP%22~%22metro|rer|tram%22];");
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                ErrorManager.Log(e);
                return false;
            }

            await FileIO.WriteTextAsync(file, content);

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("GeoStation"))
            {
                ApplicationData.Current.LocalSettings.Values.Add("GeoStation", true);
            }
            return true;
        }

        internal void Localize()
        {

        }

        private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Localize();
        }
    }
}
