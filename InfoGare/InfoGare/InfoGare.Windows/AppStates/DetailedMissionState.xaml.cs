using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Infogare.Classes.Helpers;
using Infogare.Classes.Models;
using Infogare.Classes.Presenters;

namespace InfoGare.AppStates
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailedMissionState : Page
    {
        private DispatcherTimer _timer;
        private string ArrivalDt; 
        private Tuple<InfoGareHelper, string, GareSuggestionPresenter> _BackData;
        public bool BackScrollTranslationRunning { get; set; } 
        public bool BackScrollTranslationInitialized { get; set; }

        public DetailedMissionState()
        {
            this.InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape |
                                                         DisplayOrientations.LandscapeFlipped;
             
            Seconds.Width = Window.Current.Bounds.Width/2;
 
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var inbound = e.Parameter as Tuple<Mission, InfoTrafficPresenter, Tuple<InfoGareHelper, string, GareSuggestionPresenter>>;
             
            if (inbound == null || inbound.Item1 == null)
            {
                this.Frame.GoBack();
                return;
            }

            _BackData = inbound.Item3;

            var mission = inbound.Item1; 
            var imgtype = (Path.GetFileName(mission.TransportType) == "rer.png") ? "rer_blue.png" : mission.TransportType;
     
                Trigramme.Text = mission.CodeMission;
                LogoLine.Source = new BitmapImage(new Uri(mission.TransportLogo));
                LogoType.Source = new BitmapImage(new Uri("ms-appx:///Assets/" + imgtype));
                Terminus.Text = mission.Terminus; 
               
                var obs = new ObservableCollection<DetailedDessertePresenter>();
                var stack = mission.Desserte.Split('·');
                foreach (var item in stack)
                { 
                    obs.Add(new DetailedDessertePresenter(item, false));  
                }

                //obs.Last().Weight = FontWeights.Bold;
                obs.Last().IsTerminus = true;
             
                ArrivalDt = mission.ArrivalTime; 
                DisplayDateDiff(mission.ArrivalTime);

                DDP.Height = Window.Current.Bounds.Height - (BottomDetails.ActualHeight + InfoLine.ActualHeight);
                DDP.UpdateLayout(); 
                DDP.Init(mission, DDP.ActualHeight);

            if (inbound.Item2 != null && inbound.Item2.Description != "")
            {
                InfoContent.Text = inbound.Item2.Description; 
            }
            else
            {
                InfoAnimation.Visibility = Visibility.Collapsed;
            } 

            _timer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 1)};
            _timer.Tick += TimerOnTick;
            _timer.Start(); 
        }

        private void TimerOnTick(object sender, object o)
        {
            HourMinute.Text = DateTime.Now.ToString("HH:mm");
            Seconds.Text = DateTime.Now.ToString("ss");

            if (DateTime.Now.Second == 0)
                DisplayDateDiff(ArrivalDt);

            if (!BackScrollTranslationInitialized)
                StartAnimation();
        }

        private void StartAnimation()
        {
            if (InfoContent != null && InfoContent.ActualWidth > Window.Current.Bounds.Width)
            { 
                InfoAnimation.RenderTransform = new CompositeTransform();
                var measure = InfoAnimation.ActualWidth + Window.Current.Bounds.Width;
                InfoAnimation.Margin = new Thickness(Window.Current.Bounds.Width + measure, InfoAnimation.Margin.Top, 0, InfoAnimation.Margin.Bottom);
                ((DoubleAnimation)(BackScrollTranslation.Children[0])).To = -(measure * 1.8);
                ((DoubleAnimation)(BackScrollTranslation.Children[0])).RepeatBehavior = RepeatBehavior.Forever;
                ((DoubleAnimation)(BackScrollTranslation.Children[0])).Duration = new Duration(new TimeSpan((long)(measure * 6 * 20000)));
                if (!BackScrollTranslationInitialized)
                {
                    Storyboard.SetTarget(BackScrollTranslation, InfoAnimation);
                    BackScrollTranslationInitialized = true;
                }
                BackScrollTranslation.Begin();
                BackScrollTranslationRunning = true;
            }
            BackScrollTranslationInitialized = true;
        }

        public  void DisplayDateDiff(string arrivalTime)
        {
            DateTime outter;
            var finalstr = "";

            if (DateTime.TryParse(arrivalTime, out outter))
            {
                if (outter > DateTime.Now)
                {
                    var diff = outter.Subtract(DateTime.Now);

                    if (diff.Hours != 0)
                        finalstr = diff.Hours + "h";
                    if (diff.Minutes != 0)
                    {
                        if (diff.Minutes < 0) finalstr += "0";
                        finalstr += diff.Minutes + "m";
                    } 
                    ArrivalTime.Text = finalstr;
                }
                else
                {
                    ArrivalTime.Text = arrivalTime + "m";
                }
            }
            else ArrivalTime.Text = arrivalTime + "m"; 
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
           if(_timer.IsEnabled) _timer.Stop();
           DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape | DisplayOrientations.LandscapeFlipped
                                                        | DisplayOrientations.Portrait | DisplayOrientations.PortraitFlipped;
        }  

        private void backButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(InfoScreenState), _BackData);
        }
    }
}
