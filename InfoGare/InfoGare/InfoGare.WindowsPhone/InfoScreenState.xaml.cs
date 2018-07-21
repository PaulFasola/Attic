using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Fasolib.Classes;
using Fasolib.Helpers;
using Infogare.Classes;
using Infogare.Classes.Helpers;
using Infogare.Classes.Models;
using Infogare.Classes.Presenters;
using InfoGare.Classes;
using InfoGare.Classes.Helpers;
using InfoGare.Classes.Managers;

namespace InfoGare.AppStates
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InfoScreenState : Page
    {
        private DispatcherTimer _timer;
        private InfoGareHelper _infoGare;
        private DateTime _logPageOpened = DateTime.Now;
        private bool _scrollInitialized = false;
        private bool _scrollableTransitionRunning, _scrollableTransitionRunningRelayer,
                    _BackScrollTranslationRunning, _BackScrollTranslationRelayerRunning,
                    _backScrollTranslationInitialized, _backScrollTranslationRelayerInitialized;

        private Thickness _lastSTMargin;

        private Tuple<DependencyObject, DependencyObject> ret = null;
        private GareSuggestionPresenter currentGareSuggestion;
        private InfoTrafficPresenter _infotraffic;
        private string Uniqid = "";
        private string _currentLine;

        public InfoScreenState()
        {
            this.InitializeComponent();
            this.SizeChanged += Current_SizeChanged;
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            CommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
            adprincipale.Visibility = StoreHelper.IsPremium() ? Visibility.Collapsed : Visibility.Visible;
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

        void Current_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var currentViewState = ApplicationView.GetForCurrentView().Orientation.ToString();

            if (currentViewState == "Portrait")
            {
                AdjustInfoSize();
                CommandBar.Visibility = Visibility.Visible;

                MainPane.ItemsSource = null;
                MainPane.ItemsSource = _infoGare.MissionStack;
            }

            if (currentViewState == "Landscape")
            {
                MainPane.Height = Window.Current.Bounds.Width;
                InformativePanel.Width = Window.Current.Bounds.Width;

                AdjustInfoSize();
                CommandBar.Visibility = Visibility.Collapsed;
                MainPane.ItemsSource = null;
                MainPane.ItemsSource = _infoGare.MissionStack;
                adprincipale.Margin = InformativePanel.Margin;
                adprincipale.HorizontalAlignment = HorizontalAlignment.Left; ;
            }
            VisualStateManager.GoToState(this, currentViewState, true);
        }

        private void AdjustInfoSize()
        {
            foreach (var element in _infoGare.MissionStack)
            {
                element.Width = (Window.Current.Bounds.Width).ToString();
            }
            MainPane.ItemsSource = null;
            MainPane.ItemsSource = _infoGare.MissionStack;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _timer = new DispatcherTimer();
            _timer.Tick += TimerOnTick;
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Start();

            if (e.Parameter is string)
                await AppStateManager.SecondaryTileOnNavigatedTo(e.Parameter.ToString(), this.Frame);

            RemoveAds.Visibility = StoreHelper.IsPremium() ? Visibility.Collapsed : Visibility.Collapsed;
            InformativeText.Width = Window.Current.Bounds.Width - border.ActualWidth - 15;

            var infogare = e.Parameter as Tuple<InfoGareHelper, string, GareSuggestionPresenter>;

            if (infogare?.Item1 == null) this.Frame.GoBack();

            _infoGare = infogare.Item1;
            currentGareSuggestion = infogare.Item3;
            _currentLine = infogare.Item2;

            var info = await InfoTraffic.RetrieveInformationromLine(_currentLine);
            _infotraffic = info;
            if (info != null)
            {
                InformativePanel.Background = new SolidColorBrush(Utilities.GetColorFromHexString(info.ImportanceColor));
                InformativeText.Tag = info.Link;
                info.Description = info.Description.Replace(":", ": ");
                info.Description = info.Description.Replace(".", ". ");
                Debug.WriteLine(info.Title + "        " + info.Description);
                Header.Text = info.Title + "        " + info.Description;
            }
            else
            {
                InformativePanel.Visibility = Visibility.Collapsed;
            }

            Uniqid = currentGareSuggestion.GareName.Replace(" ", "").ToLower() + '_' + Path.GetFileNameWithoutExtension(currentGareSuggestion.Logo) + '_' + currentGareSuggestion.Trigramme.Trim();


            if (_infoGare == null || _infoGare.MissionStack.Count == 0)
            {
                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("NoData"))
                {
                    var loader = new ResourceLoader();
                    ApplicationData.Current.LocalSettings.Values.Add("NoData", loader.GetString("NoData"));
                }
                this.Frame.GoBack();
            }

            PinStation.Content = ResourceLoader.GetForCurrentView().GetString("PinApp");
            //PinStation.Visibility = SecondaryTile.Exists(Uniqid) ? Visibility.Collapsed : Visibility.Visible;

        }


        private void StartScrollingAnimation(bool shouldReset)
        {
            _scrollInitialized = true;
            MainPane.UpdateLayout();
            var inull = FindChildControl<Grid>(MainPane, "Dessertes", shouldReset);

            var element = new List<DependencyObject> { ret.Item1, ret.Item2 };
            if (element.Count > 0)
            {
                foreach (var item in element)
                {
                    var elem = item as Grid;
                    if (elem != null && elem.ActualWidth > Window.Current.Bounds.Width)
                    {
                        elem.RenderTransform = new CompositeTransform();

                        if (_scrollableTransitionRunning)
                        {
                            ((DoubleAnimation)(ScrollableTransitionRelayer.Children[0])).To = -elem.ActualWidth;
                            ((DoubleAnimation)(ScrollableTransitionRelayer.Children[0])).Duration = new Duration(new TimeSpan((long)(elem.ActualWidth * 4 * 20000)));

                            try
                            {
                                Storyboard.SetTarget(ScrollableTransitionRelayer, elem);
                            }
                            catch (Exception)
                            {
                                return;
                            }

                            ScrollableTransitionRelayer.Begin();
                            _scrollableTransitionRunningRelayer = true;
                            ScrollableTransitionRelayer.Completed += delegate (object sender, object o)
                            {
                                ScrollableTransitionRelayerOnCompleted(sender, elem);
                            };
                        }
                        else
                        {
                            ((DoubleAnimation)(ScrollableTransition.Children[0])).To = -elem.ActualWidth;
                            ((DoubleAnimation)(ScrollableTransition.Children[0])).Duration = new Duration(new TimeSpan((long)(elem.ActualWidth * 4 * 20000)));

                            try
                            {
                                Storyboard.SetTarget(ScrollableTransition, elem);
                            }
                            catch (Exception)
                            {
                                ScrollableTransition.Stop();
                            }
                            finally
                            {
                                Storyboard.SetTarget(ScrollableTransition, elem);
                            }

                            ScrollableTransition.Begin();
                            _scrollableTransitionRunning = true;
                            ScrollableTransition.Completed += delegate (object sender, object o) { ScrollableTransitionOnCompleted(sender, elem); };
                        }
                    }
                }
            }
        }

        private void ScrollableTransitionRelayerOnCompleted(object sender, object o)
        {
            _scrollableTransitionRunningRelayer = false;
            Task.Delay(3000);
            var element = o as Grid;

            if (element != null)
            {
                _lastSTMargin = new Thickness(element.Margin.Left - 100, element.Margin.Top, element.Margin.Right, element.Margin.Bottom);
                var measure = element.ActualWidth + Window.Current.Bounds.Width;
                element.Margin = new Thickness(measure + 100, element.Margin.Top, 0, element.Margin.Bottom);
                ((DoubleAnimation)(BackScrollTranslationRelayer.Children[0])).To = -(measure * 1.8);
                ((DoubleAnimation)(BackScrollTranslationRelayer.Children[0])).RepeatBehavior = RepeatBehavior.Forever;
                ((DoubleAnimation)(BackScrollTranslationRelayer.Children[0])).Duration = new Duration(new TimeSpan((long)(measure * 6 * 20000)));
                if (!_backScrollTranslationRelayerInitialized)
                {
                    Storyboard.SetTarget(BackScrollTranslationRelayer, element);
                    _backScrollTranslationRelayerInitialized = true;
                }
                BackScrollTranslationRelayer.Begin();
                _BackScrollTranslationRelayerRunning = true;
                BackScrollTranslationRelayer.Completed += delegate (object altsender, object a) { BackScrollTranslationRelayerOnCompleted(altsender, element); };

            }
        }

        private void ScrollableTransitionOnCompleted(object sender, object o)
        {
            _scrollableTransitionRunning = false;
            Task.Delay(3000);
            var element = o as Grid;

            if (element != null)
            {
                var measure = element.ActualWidth + Window.Current.Bounds.Width;
                element.Margin = new Thickness(measure + 100, element.Margin.Top, 0, element.Margin.Bottom);
                ((DoubleAnimation)(BackScrollTranslation.Children[0])).To = -(measure * 1.8);
                ((DoubleAnimation)(BackScrollTranslation.Children[0])).RepeatBehavior = RepeatBehavior.Forever;
                ((DoubleAnimation)(BackScrollTranslation.Children[0])).Duration = new Duration(new TimeSpan((long)(measure * 6 * 20000)));
                if (!_backScrollTranslationInitialized)
                {
                    Storyboard.SetTarget(BackScrollTranslation, element);
                    _backScrollTranslationInitialized = true;
                }
                BackScrollTranslation.Begin();
                _BackScrollTranslationRunning = true;
                BackScrollTranslation.Completed += delegate (object altsender, object a) { BackScrollTranslationOnCompleted(altsender, element); };
            }
        }

        private void BackScrollTranslationRelayerOnCompleted(object sender, object o)
        {
            var element = o as Grid;
            _BackScrollTranslationRelayerRunning = false;
            ScrollableTransitionRelayerOnCompleted(sender, element);
        }

        private void BackScrollTranslationOnCompleted(object sender, object o)
        {
            var element = o as Grid;
            if (element == null) return;
            element.Margin = _lastSTMargin;
            _scrollableTransitionRunning = false;
            ScrollableTransitionOnCompleted(sender, element);
        }

        private void TimerOnTick(object sender, object o)
        {
            HourMinute.Text = DateTime.Now.ToString("HH:mm");
            Seconds.Text = DateTime.Now.ToString("ss");

            if (!_scrollInitialized && _logPageOpened.AddSeconds(4) < DateTime.Now) StartScrollingAnimation(false);

            var EOV = true;

            if (_infoGare.MissionStack.Count == 0)
            {
                this.ResetAndRetrieve();
            }

            while (EOV && _infoGare.MissionStack != null && _infoGare.MissionStack.Count > 0 && (DateTime.Now < DateTime.Parse("23:50")))
            {
                Mission currentLine = null;
                try
                {
                    currentLine = _infoGare.MissionStack[0];
                }
                catch (Exception e)
                {
                    ErrorManager.Log(e);
                    this.Frame.GoBack(); // temporary handler
                }


                DateTime nulldt;
                if ((currentLine != null && DateTime.TryParse(currentLine.StationCheckIn, out nulldt) && DateTime.Parse(currentLine.StationCheckIn).AddSeconds(29) < DateTime.Now))
                {
                    //@TODO: voir si 1.datetime > 0 => train en retard etc.

                    StopAllAnimations();
                    _infoGare.MissionStack.RemoveAt(0);
                    _infoGare.UpdateMissionStack();

                    MainPane.ItemsSource = null;
                    MainPane.ItemsSource = _infoGare.MissionStack;

                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                    timer.Tick += delegate
                    {
                        timer.Stop();
                        StopAllAnimations();
                        StartScrollingAnimation(true);
                    };
                    timer.Start();
                }
                else
                {
                    EOV = false;
                }

                var i = 0;
                while (_infoGare.MissionStack != null)
                {
                    try
                    {
                        currentLine = _infoGare.MissionStack[i];
                    }
                    catch (Exception)
                    {
                        break;
                    }

                    if (!DateTime.TryParse(currentLine.StationCheckIn, out nulldt)) return;

                    if (DateTime.Parse(currentLine.StationCheckIn).AddMinutes(-3) < DateTime.Now)
                    {
                        _infoGare.MissionStack[i].ArrivalTime = currentLine.TransitionState ? "" : "à l'approche";
                        currentLine.TransitionState = !currentLine.TransitionState;
                        currentLine.TansitionStart = DateTime.Now.ToString();
                        _infoGare.Refresh();
                    }
                    else if (DateTime.Parse(currentLine.StationCheckIn) == DateTime.Now)
                    {
                        currentLine.ArrivalTime = "à quai";
                        currentLine.TransitionState = false;
                        _infoGare.Refresh();
                    }
                    else break;
                    i++;
                }
            }
        }

        private async void ResetAndRetrieve()
        {
            return;
            /*StopAllAnimations();

            var stack =
                await
                    MissionHelper.CollectMissions(_infoGare.MissionStack.Count, _currentLine,
                        currentGareSuggestion.Trigramme);
            if (_infoGare.MissionStack != null) _infoGare.MissionStack = new ObservableCollection<Mission>(_infoGare.MissionStack.Concat(stack).ToList());
            */
        }

        private Tuple<DependencyObject, DependencyObject> FindChildControl<T>(DependencyObject control, string ctrlName, bool reset)
        {
            if (ret == null || reset) ret = new Tuple<DependencyObject, DependencyObject>(null, null);
            var childNumber = VisualTreeHelper.GetChildrenCount(control);
            for (var i = 0; i < childNumber && ret.Item2 == null; i++)
            {
                var child = VisualTreeHelper.GetChild(control, i);
                var fe = child as FrameworkElement;

                if (fe == null) return null;

                if (child is T && fe.Name == ctrlName)
                {
                    if (ret.Item1 == null) ret = new Tuple<DependencyObject, DependencyObject>(child, null);
                    else
                    {
                        if (ret.Item2 == null)
                            ret = new Tuple<DependencyObject, DependencyObject>(ret.Item1, child);
                        else
                        {
                            return ret;
                        }
                    }
                }
                else
                {
                    // Not found it - search children
                    var nextLevel = FindChildControl<T>(child, ctrlName, false);
                    if (nextLevel.Item2 != null)
                        return nextLevel;
                }
            }
            return ret;
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            StopAllAnimations();
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            if (_timer.IsEnabled) _timer.Stop();
        }

        private void StopAllAnimations()
        {
            if (_BackScrollTranslationRelayerRunning) BackScrollTranslationRelayer.Stop();
            if (_BackScrollTranslationRunning) BackScrollTranslation.Stop();
            if (_scrollableTransitionRunning) ScrollableTransition.Stop();
            if (_scrollableTransitionRunningRelayer) ScrollableTransitionRelayer.Stop();

            _scrollableTransitionRunning = false;
            _BackScrollTranslationRelayerRunning = false;
            _BackScrollTranslationRunning = false;
            _scrollableTransitionRunningRelayer = false;
        }


        private async void InformativePanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var link = InformativeText.Tag as string;
            if (link == null) return;
            var status = await Launcher.LaunchUriAsync(new Uri(link));
            if (!status) await new MessageDialog("Erreur lors de la tentative d'ouverture de la page").ShowAsync();
        }

        private async void Favorite_Click(object sender, RoutedEventArgs e)
        {
            await Insert("Favorite.bin", currentGareSuggestion);
            await new MessageDialog(ResourceLoader.GetForCurrentView().GetString("BookmarkedSuccess")).ShowOrWaitAsync();
        }

        private async void MainPane_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var loader = new ResourceLoader();
            if (StoreHelper.IsPremium())
            {
                var item = MainPane.SelectedItem as Mission;
                this.Frame.Navigate(typeof(DetailedMissionState), new Tuple<Mission, InfoTrafficPresenter>(item, _infotraffic));
            }
            else
            {
                await StoreHelper.TryBuy("UnlockDetailedItems");
            }
        }

        private void ContentRoot_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        public static async Task Insert<T>(string filename, T obj)
        {
            var stack = (List<T>)await ObjectHelper.DeserializeToObject<List<T>>(filename);
            var retstack = new List<T>();
            if (stack != null) retstack.AddRange(stack);

            if (stack != null)
            {
                bool found = false;
                foreach (var element in stack)
                {
                    found = false;
                    if (typeof(T) == typeof(GareSuggestionPresenter))
                    {
                        var target = obj as GareSuggestionPresenter;
                        var item = element as GareSuggestionPresenter;

                        if (item != null &&
                            (target != null &&
                             (target.GareName == item.GareName && target.Trigramme == item.Trigramme &&
                              target.Logo == item.Logo)))
                        {
                            found = true;
                        }
                    }


                    if (found)
                    {
                        retstack.Remove(element);
                        retstack.Insert(0, obj);
                        break;
                    }
                }
                if (!found) retstack.Insert(0, obj);
            }
            else
            {
                retstack = new List<T> { obj };
            }

            await ObjectHelper.SerializeToFile(retstack, filename);
        }


        private async void Pin_Click(object sender, RoutedEventArgs e)
        {
#if WINDOWS_APP
                this.SecondaryTileCommandBar.IsSticky = true;
#endif

            if (SecondaryTile.Exists(Uniqid))
            {
                await new MessageDialog(ResourceLoader.GetForCurrentView().GetString("CantPinApp")).ShowOrWaitAsync();
            }
            else
            {
                var name = Path.GetFileNameWithoutExtension(currentGareSuggestion.Logo) + "-150x150.png";

                var square150x150Logo = new Uri("ms-appx:///Assets/icon/" + name);
                var secondaryTile = new SecondaryTile(Uniqid,
                                                                currentGareSuggestion.GareName,
                                                                Uniqid,
                                                                square150x150Logo,
                                                                TileSize.Default);
                secondaryTile.VisualElements.Square30x30Logo = new Uri(currentGareSuggestion.Logo);
                secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                secondaryTile.VisualElements.ForegroundText = ForegroundText.Light;



#if WINDOWS_APP
                        var isPinned = await secondaryTile.RequestCreateForSelectionAsync(rect, placement); 
                        ToggleAppBarButton(!isPinned);
#endif

#if WINDOWS_PHONE_APP
                await secondaryTile.RequestCreateAsync();
                PinStation.Visibility = Visibility.Collapsed;
#endif
            }

#if WINDOWS_APP
                    this.BottomAppBar.IsSticky = false;
#endif
        }

        private async void RemoveAds_Click(object sender, RoutedEventArgs e)
        {
            await StoreHelper.TryBuy("RemoveAds");
        }

        private void CommandBar_Opened(object sender, object e)
        {

        }


    }
}
