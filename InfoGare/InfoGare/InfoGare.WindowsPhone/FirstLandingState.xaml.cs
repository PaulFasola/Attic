using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Fasolib.Helpers;
using Infogare.Classes.Animations;
using Infogare.Classes.Helpers;
using Infogare.Classes.Models;
using Infogare.Classes.Presenters;
using InfoGare.AppStates;
using InfoGare.Classes.Helpers;
using InfoGare.Enums;
using InfoGare.UserControls;
using ErrorManager = Infogare.Classes.ErrorManager;

namespace InfoGare
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FirstLandingState : Page
    {
        public SideScrollAnimation Animation { get; set; }
        public InfoGareHelper InfoGareInstance;
        private ObservableCollection<GareSuggestionPresenter> _suggestionItems;
        private FavoritePaneUC _favoritePane;
        public SearchBar SearchBar;
        private ResourceLoader loader = new ResourceLoader();

        public FirstLandingState()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            InfoGareInstance = new InfoGareHelper();

            this.Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                await InfoGareInstance.Init(); // Handle offline mode
                SearchBar = new SearchBar(this);
                await SearchBar.LoadGeoStations();
            };
        }


        private void AdaptBackground()
        {
            if (ContentRoot.Background is ImageBrush)
            {
                var source = ((ImageBrush)(ContentRoot.Background)).ImageSource as BitmapImage;
                ContentRoot.Background = new ImageBrush() { ImageSource = source, Stretch = Stretch.Uniform };
            }

            backgroundMotion.VerticalAlignment = VerticalAlignment.Stretch;
            backgroundMotion.HorizontalAlignment = HorizontalAlignment.Stretch;
            backgroundMotion.Height = double.NaN;
            backgroundMotion.Width = double.NaN;
            backgroundMotion.Height = Window.Current.Bounds.Height;
            backgroundMotion.Width = Window.Current.Bounds.Width;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            Application.Current.Exit();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Frame.BackStack.Clear();
            var loader = new ResourceLoader();
            var currentViewState = ApplicationView.GetForCurrentView().Orientation.ToString();

            LoadingText.Text = loader.GetString("LoadingStations");

            LoadingModule.Visibility = Visibility.Visible;
            InfoGareInstance = new InfoGareHelper();
            MainLoader.Visibility = Visibility.Collapsed;

            SetBackground(BackgroundType.Video);

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("NoData"))
            {
                await new MessageDialog(loader.GetString("NoData")).ShowOrWaitAsync();
                ApplicationData.Current.LocalSettings.Values.Remove("NoData");
            }

            _suggestionItems = GareHelper.GetStationsSuggestion(InfoGareInstance.Gares);
            LoadingModule.Visibility = Visibility.Collapsed;

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("AppJustLaunched"))
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("CanLocalise"))
                {
                    if ((bool)ApplicationData.Current.LocalSettings.Values["CanLocalise"]) SearchBar.Localize();
                }
                ApplicationData.Current.LocalSettings.Values.Remove("AppJustLaunched");
            }


            _favoritePane = new FavoritePaneUC(this) { Height = Window.Current.Bounds.Height };
            SearchBar = new SearchBar(this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };

            LayoutRoot.Children.Add(_favoritePane);
            LayoutRoot.Children.Add(SearchBar);
        }

        private void SetBackground(BackgroundType type)
        {
            if (type == BackgroundType.Image)
            {
                backgroundMotion.Stop();
                backgroundMotion.Visibility = Visibility.Collapsed;

                var rand = new Random().Next(1, 5);
                if (rand == 4) rand = 99;
                ContentRoot.Background = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Slide/" + rand + ".jpg"))
                };
            }
            else
            {
                ContentRoot.Background = new SolidColorBrush(Colors.Transparent);
                backgroundMotion.Visibility = Visibility.Visible;
                backgroundMotion.Play();
            }
        }

        public async Task PreloadBoard(string trigramme, string line, GareSuggestionPresenter item)
        {
            InfoGareInstance = new InfoGareHelper();
            var loader = new ResourceLoader();
            try
            {
                if (line == "A" && trigramme == "VDF") trigramme = "VFR";
                InfoGareInstance = await new Mission().GetBoard(trigramme, line);
                LoadingModule.Visibility = Visibility.Collapsed;
                if (ApplicationView.GetForCurrentView().Orientation.ToString() == "Portrait" && !ApplicationData.Current.LocalSettings.Values.ContainsKey("Popup"))
                {
                    ApplicationData.Current.LocalSettings.Values.Add("Popup", true);
                    await new MessageDialog(loader.GetString("InfoAppliPopup")).ShowOrWaitAsync();
                }

                var param = new Tuple<InfoGareHelper, string, GareSuggestionPresenter>(InfoGareInstance, line, item);
                this.Frame.Navigate(typeof(InfoScreenState), param);
            }
            catch (Exception e)
            {
                ErrorManager.Log(e);
                LoadingModule.Visibility = Visibility.Collapsed;

                await new MessageDialog(loader.GetString("NoInternet")).ShowOrWaitAsync();
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AboutState));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        private void Parameter_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ParameterState));
        }
    }
}
