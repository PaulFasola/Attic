using System;
using System.Collections.Generic; 
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
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
using Infogare.Classes.Managers;
using Infogare.Classes.Models;
using Infogare.Classes.Presenters;
using InfoGare.Classes;
using lang = Windows.ApplicationModel; 

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
        public Tuple<InfoGareHelper, string, GareSuggestionPresenter> OriginalData { get; set; }
        private bool _scrollableTransitionRunning, _scrollableTransitionRunningRelayer, 
                    _backScrollTranslationRunning, _backScrollTranslationRelayerRunning,
                    _backScrollTranslationInitialized, _backScrollTranslationRelayerInitialized;

        private Thickness _lastStMargin;

        private Tuple<DependencyObject, DependencyObject> _ret = null;
        private GareSuggestionPresenter _currentGareSuggestion;
        private InfoTrafficPresenter _infotraffic;
        private long _delayer = 0;
        private bool _retard = true;

        public InfoScreenState()
        {
            this.InitializeComponent();
            this.SizeChanged += Current_SizeChanged;

            MainPane.BorderThickness = new Thickness(0); 
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

            InformativeText.Width = Window.Current.Bounds.Width - border.ActualWidth - 15; 

            var infogare = e.Parameter as Tuple<InfoGareHelper, string, GareSuggestionPresenter>;

            if(infogare == null ||  infogare.Item1 == null) this.Frame.GoBack();

            if (infogare == null)
            {
                this.Frame.GoBack();
            }
            else
            {
                OriginalData = infogare;
                _infoGare = infogare.Item1;
                _currentGareSuggestion = infogare.Item3;
                StationHeader.AttachStation(infogare.Item3);


                StationHeader.Initialize(infogare.Item3.GareName, false, this.Frame);

                _infotraffic = StoreHelper.IsPremium() ? await InfoTraffic.RetrieveInformationromLine(infogare.Item2) : null;
             

                if (StoreHelper.IsPremium())
                {
                    InformativePanel.Visibility = Visibility.Visible;
                    secondaryWinRTAd.Visibility = Visibility.Collapsed;
                }
                else
                {
                    InformativePanel.Visibility = Visibility.Collapsed;
                    secondaryWinRTAd.Visibility = Visibility.Visible;
                }

                if (_infotraffic != null)
                {
                    InformativePanel.Background = new SolidColorBrush(Utilities.GetColorFromHexString(_infotraffic.ImportanceColor));
                    InformativeText.Tag = _infotraffic.Link;
                    Header.Text = _infotraffic.Title + _infotraffic.Description;
                }
                else
                {
                    InformativePanel.Visibility = Visibility.Collapsed;
                }
            }

            if (_infoGare == null || _infoGare.MissionStack.Count == 0)
            {
                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("NoData"))
                {
                    var loader = new lang.Resources.ResourceLoader();
                    ApplicationData.Current.LocalSettings.Values.Add("NoData", loader.GetString("NoData"));
                }
                this.Frame.GoBack();
            } 
        }


        private void StartScrollingAnimation()
        {
            _scrollInitialized = true;
            MainPane.UpdateLayout();
            var inull = FindChildControl<Grid>(MainPane, "Dessertes");

            var element = new List<DependencyObject> {_ret.Item1, _ret.Item2};
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
                            ScrollableTransitionRelayer.Completed += delegate(object sender, object o)
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
                                 
                            }
                            
                            ScrollableTransition.Begin();
                            _scrollableTransitionRunning = true;
                            ScrollableTransition.Completed += delegate(object sender, object o) { ScrollableTransitionOnCompleted(sender, elem); };
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
                _lastStMargin = new Thickness(element.Margin.Left - 100, element.Margin.Top, element.Margin.Right, element.Margin.Bottom);
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
                _backScrollTranslationRelayerRunning = true;
                BackScrollTranslationRelayer.Completed += delegate(object altsender, object a) { BackScrollTranslationRelayerOnCompleted(altsender, element); };
            }  
        }
         
        private void ScrollableTransitionOnCompleted(object sender, object o)
        { 
            _scrollableTransitionRunning = false;

            Task.Delay(3000);
            var element = o as Grid; 

            if (element != null )
            { 
                var measure = element.ActualWidth + Window.Current.Bounds.Width;
                element.Margin = new Thickness(measure + 100, element.Margin.Top, 0, element.Margin.Bottom); 
                ((DoubleAnimation)(BackScrollTranslation.Children[0])).To = -(measure * 1.8);
                ((DoubleAnimation) (BackScrollTranslation.Children[0])).RepeatBehavior = RepeatBehavior.Forever;
                ((DoubleAnimation)(BackScrollTranslation.Children[0])).Duration = new Duration(new TimeSpan((long)(measure * 6 * 20000)));
                if (!_backScrollTranslationInitialized)
                {
                    Storyboard.SetTarget(BackScrollTranslation, element);
                    _backScrollTranslationInitialized = true;
                }
                BackScrollTranslation.Begin();
                _backScrollTranslationRunning = true;
                BackScrollTranslation.Completed += delegate(object altsender, object a) { BackScrollTranslationOnCompleted(altsender, element); };
              }  
        }

        private void BackScrollTranslationRelayerOnCompleted(object sender, object o)
        {
            var element = o as Grid;  
             _backScrollTranslationRelayerRunning = false;
             ScrollableTransitionRelayerOnCompleted(sender, element); 
        }

        private void BackScrollTranslationOnCompleted(object sender, object o)
        {
            var element = o as Grid; 
            if (element == null) return;
            element.Margin = _lastStMargin;
            _scrollableTransitionRunning = false;
            ScrollableTransitionOnCompleted(sender, element);
        }

        private void TimerOnTick(object sender, object o)
        {
            var eov = true;

            HourMinute.Text = DateTime.Now.ToString("HH:mm");
            Seconds.Text = DateTime.Now.ToString("ss");

            if (!_scrollInitialized && _logPageOpened.AddSeconds(4) < DateTime.Now) 
                StartScrollingAnimation();

            // Actionne le changement d'affichage des infos (heure/éventuel retard/heure etc...)
            ToggleInfoSwitch();

            
            while (eov && _infoGare.MissionStack != null && _infoGare.MissionStack.Count > 0 && (DateTime.Now < DateTime.Parse("23:50")))
            {
                Mission currentLine;
                try
                {
                     currentLine = _infoGare.MissionStack[0];
                }
                catch (Exception e)
                {
                    ErrorManager.Log(e);
                    currentLine = null;
                    this.Frame.GoBack();
                } 

                DateTime nulldt;
                if (currentLine != null && DateTime.TryParse(currentLine.StationCheckIn, out nulldt) && DateTime.Parse(currentLine.StationCheckIn) < DateTime.Now)
                {
                    if(_scrollableTransitionRunning)  ScrollableTransition.Stop();
                    if(_backScrollTranslationRunning) BackScrollTranslation.Stop();

                    _infoGare.MissionStack.RemoveAt(0);
                    _infoGare.UpdateMissionStack();

                    MainPane.ItemsSource = null;
                    MainPane.ItemsSource = _infoGare.MissionStack;
                     
                    StartScrollingAnimation();
                }
                else
                {
                    eov = false;
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

                    if (DateTime.Parse(currentLine.StationCheckIn).AddMinutes(- 1.80) < DateTime.Now)
                    {
                        _infoGare.MissionStack[i].Terminus = "à l'approche";

                        if (currentLine.TansitionStart == null || DateTime.Parse(currentLine.TansitionStart).AddSeconds(4) < DateTime.Now)
                        {
                            currentLine.TransitionState = !currentLine.TransitionState;
                            //var st = FindChildControl<TextBlock>("TimeStation")
                            currentLine.TansitionStart = DateTime.Now.ToString(); 
                        } 
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

        private void ToggleInfoSwitch()
        {
            if (_delayer < 3)
            {
                _delayer++;
                return;
            }

            if (_retard)
            {
                foreach (var mission in _infoGare.MissionStack)
                {
                    mission.StationCheckSet = (mission.Retard != null && mission.Retard != "à l'heure")
                        ? "retardé (" + mission.Retard + ")"
                        : mission.StationCheckSet; 
                }
            }
            else
            {
                foreach (var mission in _infoGare.MissionStack)
                {
                    mission.StationCheckSet = mission.ArrivalTime;
                }
            }
            _delayer = 0;
            _retard = !_retard; 
        }


        private Tuple<DependencyObject, DependencyObject> FindChildControl<T>(DependencyObject control, string ctrlName)
        {
            if(_ret == null) _ret = new Tuple<DependencyObject, DependencyObject>(null,null);
            var childNumber = VisualTreeHelper.GetChildrenCount(control);
            for (var i = 0; i < childNumber && _ret.Item2 == null; i++)
            {
                var child = VisualTreeHelper.GetChild(control, i);
                var fe = child as FrameworkElement; 

                if (fe == null) return null;

                if (child is T && fe.Name == ctrlName)
                {
                    if (_ret.Item1 == null) _ret = new Tuple<DependencyObject, DependencyObject>(child, null);
                    else
                    {
                        if(_ret.Item2 == null) 
                            _ret = new Tuple<DependencyObject, DependencyObject>(_ret.Item1, child);
                        else
                        {
                            return _ret;
                        }
                    } 
                }
                else
                {
                    // Not found it - search children
                    var nextLevel = FindChildControl<T>(child, ctrlName);
                    if (nextLevel.Item2 != null)
                        return nextLevel;
                }
            }
            return _ret;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {  
            if(_backScrollTranslationRelayerRunning) BackScrollTranslationRelayer.Stop();
            if(_backScrollTranslationRunning)        BackScrollTranslation.Stop();
            if(_scrollableTransitionRunning)         ScrollableTransition.Stop();
            if(_scrollableTransitionRunningRelayer)  ScrollableTransitionRelayer.Stop();

            if(_timer.IsEnabled) _timer.Stop(); 
        }

        private async void InformativePanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var link = InformativeText.Tag as string;
            if (link == null) return;
            var status = await Launcher.LaunchUriAsync(new Uri(link));
            if (!status) await new MessageDialog("Erreur lors de la tentative d'ouverture de la page").ShowOrWaitAsync();
        }

        private async void Favorite_Click(object sender, RoutedEventArgs e)
        { 
            await BinaryManager.Insert("Favorite.bin", _currentGareSuggestion);
        }

        private async void MainPane_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainPane.SelectedIndex == -1) return;
            if (!StoreHelper.IsPremium())
            {
                await new MessageDialog(lang.Resources.ResourceLoader.GetForCurrentView().GetString("NotPremium")).ShowOrWaitAsync();
                MainPane.SelectedIndex = -1;
                return;
            }

            var item = MainPane.SelectedItem as Mission;
            this.Frame.Navigate(typeof(DetailedMissionState), new Tuple<Mission, InfoTrafficPresenter, Tuple<InfoGareHelper, string, GareSuggestionPresenter>>(item, _infotraffic, OriginalData));
        } 
    }
}
 