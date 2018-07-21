using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Threading.Tasks; 
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation; 
using Infoécran.Classes;
using Infoécran.Classes.Presenters;
using lang = Windows.ApplicationModel; 

namespace Infoécran.AppStates
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InfoScreenState : Page
    { 
        private DispatcherTimer _timer; 
        private InfoGare _infoGare;
        private DateTime _logPageOpened = DateTime.Now;
        private bool _scrollInitialized = false;
        private bool _scrollableTransitionRunning, _scrollableTransitionRunningRelayer, 
                    _BackScrollTranslationRunning, _BackScrollTranslationRelayerRunning,
                    _backScrollTranslationInitialized, _backScrollTranslationRelayerInitialized;

        private Thickness _lastSTMargin, _lastSTMarginRelayer;

        private Tuple<DependencyObject, DependencyObject> ret = null;
        private GareSuggestionPresenter currentGareSuggestion;
        private InfoTrafficPresenter _infotraffic;

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

            var infogare = e.Parameter as Tuple<InfoGare, string, GareSuggestionPresenter>;

            if(infogare == null ||  infogare.Item1 == null) this.Frame.GoBack();

            if (infogare == null)
            {
                this.Frame.GoBack();
            }
            else
            {
                _originalData = infogare;
                _infoGare = infogare.Item1;
                currentGareSuggestion = infogare.Item3;


                StationHeader.Initialize(infogare.Item3.GareName, false, this.Frame);

                var info = await InfoTraffic.RetrieveInformationromLine(infogare.Item2);
                   _infotraffic = info;
                if (info != null)
                {
                    InformativePanel.Background = new SolidColorBrush(Utilities.GetColorFromHexString(info.ImportanceColor));
                    InformativeText.Tag = info.Link;
                    Header.Text = info.Title + info.Description;
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

            var element = new List<DependencyObject> {ret.Item1, ret.Item2};
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
                            Storyboard.SetTarget(ScrollableTransition, elem);
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
                _BackScrollTranslationRunning = true;
                BackScrollTranslation.Completed += delegate(object altsender, object a) { BackScrollTranslationOnCompleted(altsender, element); };
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

            if (!_scrollInitialized && _logPageOpened.AddSeconds(4) < DateTime.Now) StartScrollingAnimation();

            var EOV = true;

            while (EOV && _infoGare.MissionStack != null && _infoGare.MissionStack.Count > 0 && (DateTime.Now < DateTime.Parse("23:50")))
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
                    //@TODO: voir si 1.datetime > 0 => train en retard etc.


                    if(_scrollableTransitionRunning)  ScrollableTransition.Stop();
                    if(_BackScrollTranslationRunning) BackScrollTranslation.Stop();

                    _infoGare.MissionStack.RemoveAt(0);
                    _infoGare.UpdateMissionStack();

                    MainPane.ItemsSource = null;
                    MainPane.ItemsSource = _infoGare.MissionStack;
                     
                    StartScrollingAnimation();
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

                    if (DateTime.Parse(currentLine.StationCheckIn).AddMinutes(- 1.80) < DateTime.Now)
                    {
                        _infoGare.MissionStack[i].Terminus = "à l'approche";

                        if (currentLine.TansitionStart == null || DateTime.Parse(currentLine.TansitionStart).AddSeconds(4) < DateTime.Now)
                        {
                            currentLine.TransitionState = !currentLine.TransitionState;
                            //var st = FindChildControl<TextBlock>("TimeStation")
                            currentLine.TansitionStart = DateTime.Now.ToString();
                            Debug.WriteLine("tic"); 
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
         
 
        private Tuple<DependencyObject, DependencyObject> FindChildControl<T>(DependencyObject control, string ctrlName)
        {
            if(ret == null) ret = new Tuple<DependencyObject, DependencyObject>(null,null);
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
                        if(ret.Item2 == null) 
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
                    var nextLevel = FindChildControl<T>(child, ctrlName);
                    if (nextLevel.Item2 != null)
                        return nextLevel;
                }
            }
            return ret;
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {  
            if(_BackScrollTranslationRelayerRunning) BackScrollTranslationRelayer.Stop();
            if(_BackScrollTranslationRunning)        BackScrollTranslation.Stop();
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
            await ObjectManager.Insert("Favorite.bin", currentGareSuggestion);
        }

        private void MainPane_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = MainPane.SelectedItem as Mission;
            this.Frame.Navigate(typeof(DetailedMissionState), new Tuple<Mission, InfoTrafficPresenter, Tuple<InfoGare, string, GareSuggestionPresenter>>(item, _infotraffic, _originalData));
        }

        public Tuple<InfoGare, string, GareSuggestionPresenter> _originalData { get; set; }
    }
}
