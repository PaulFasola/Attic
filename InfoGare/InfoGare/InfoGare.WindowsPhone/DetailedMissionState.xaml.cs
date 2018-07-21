using System;
using System.IO;
using Windows.Phone.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
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
        public bool BackScrollTranslationRunning { get; set; }
        public bool BackScrollTranslationInitialized { get; set; }
        public bool AnimationStarted { get; set; }
        public DateTime currentDt { get; set; }

        public DetailedMissionState()
        {
            this.InitializeComponent();
            this.SizeChanged += Current_SizeChanged;

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
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

        private void Current_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var currentViewState = ApplicationView.GetForCurrentView().Orientation.ToString();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Seconds.Visibility = HourMinute.Visibility = Visibility.Collapsed;
            var inbound = e.Parameter as Tuple<Mission, InfoTrafficPresenter>;
            InfoContainer.Visibility = Visibility.Collapsed;
            if (inbound == null || inbound.Item1 == null)
            {
                this.Frame.GoBack();
                return;
            }
            var mission = inbound.Item1;
            currentDt = DateTime.Now;
            AnimationStarted = false;

            var imgtype = (Path.GetFileName(mission.TransportType) == "rer.png") ? "rer_blue.png" : mission.TransportType;

            Trigramme.Text = mission.CodeMission;
            LogoLine.Source = new BitmapImage(new Uri(mission.TransportLogo));
            LogoType.Source = new BitmapImage(new Uri("ms-appx:///Assets/" + imgtype));
            Terminus.Text = mission.Terminus;

            DDP.UpdateLayout();

            DDP.Init(mission, DDP.ActualHeight);

            ArrivalDt = mission.ArrivalTime;
            DisplayDateDiff(mission.ArrivalTime);


            if (inbound.Item2 != null && inbound.Item2.Description != "")
            {
                InfoContent.Text = inbound.Item2.Description;
            }
            else
            {
                InfoAnimation.Visibility = Visibility.Collapsed;
            }

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            _timer.Tick += TimerOnTick;
            _timer.Start();
        }

        private void TimerOnTick(object sender, object o)
        {
            HourMinute.Text = DateTime.Now.ToString("HH:mm");
            Seconds.Text = DateTime.Now.ToString("ss");
            Seconds.Visibility = HourMinute.Visibility = Visibility.Visible;

            if (DateTime.Now.Second == 0)
                DisplayDateDiff(ArrivalDt);

            if (!BackScrollTranslationInitialized) StartAnimation();

        }

        private void StartAnimation()
        {
            if (InfoContent != null && InfoContent.ActualWidth > Window.Current.Bounds.Width)
            {
                InfoContainer.Visibility = Visibility.Visible;
                InfoAnimation.RenderTransform = new CompositeTransform();
                var measure = InfoAnimation.ActualWidth + Window.Current.Bounds.Width;
                InfoAnimation.Margin = new Thickness(Window.Current.Bounds.Width + measure, InfoAnimation.Margin.Top, 0, InfoAnimation.Margin.Bottom);
                ((DoubleAnimation)(BackScrollTranslation.Children[0])).To = -(measure * 2);
                ((DoubleAnimation)(BackScrollTranslation.Children[0])).RepeatBehavior = RepeatBehavior.Forever;
                ((DoubleAnimation)(BackScrollTranslation.Children[0])).Duration = new Duration(new TimeSpan((long)(measure * 6 * 23000)));
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

        public void DisplayDateDiff(string arrivalTime)
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
                        if (diff.Hours != 0)
                            finalstr += diff.Minutes + "m";
                        else
                            finalstr += diff.Minutes + " min";
                    }

                    ArrivalTime.Text = finalstr;
                }
                else
                {
                    ArrivalTime.Text = outter.Hour + "h " + outter.Minute + "min";
                }
            }
            else ArrivalTime.Text = arrivalTime;

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            if (_timer != null && _timer.IsEnabled) _timer.Stop();
        }
    }
}
