using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Fasolib.Classes;
using Infogare.Classes.Models;
using Infogare.Classes.Presenters;
using WinRTXamlToolkit.AwaitableUI;
using WinRTXamlToolkit.Controls.Extensions;

namespace InfoGare.Controls
{
    public sealed partial class DetailedDessertePanel
    {
        private ScrollViewer _scrollViewer;
        private IList<IList<DetailedDessertePresenter>> _desserteParts = null;
        private DispatcherTimer _timer, _secondTimer;

        public DetailedDessertePanel()
        {
            this.InitializeComponent();
        }

        internal void Init(Mission mission, double maxHeight)
        {
            var stack = mission.Desserte.Split('·');

            Dessertes.UpdateLayout();

            if (ApplicationView.GetForCurrentView().Orientation.ToString() != "Portrait")
            {
                InitPortaitMode(stack);
            }
            else
            {
                InitPortaitMode(stack);
            }
            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 4) };
        }

        private void InitPortaitMode(IEnumerable<string> stack)
        {
            var obs = new ObservableCollection<DetailedDessertePresenter>();

            foreach (var item in stack)
            {
                obs.Add(new DetailedDessertePresenter(item.RemoveBeginningWhiteSpace(), false));
            }

            obs.Last().IsTerminus = true;
            FirstStack.ItemsSource = obs;
        }

        private void InitLandscapeMode(string[] stack, double maxSize)
        {
            _desserteParts = new List<IList<DetailedDessertePresenter>>();
            var i = 0;
            while (i < stack.Count())
            {
                var part = new List<DetailedDessertePresenter>();

                var actualSize = 0;
                foreach (var item in stack)
                {
                    actualSize += 4;
                    part.Add(new DetailedDessertePresenter(item.RemoveBeginningWhiteSpace(), false));
                    if (maxSize <= actualSize) break;
                    i++;
                }
                _desserteParts.Add(part);

            }

           // obs.Last().IsTerminus = true;
            FirstStack.ItemsSource = _desserteParts[0];

            if (1 < _desserteParts.Count)
            {
           //     SecondStack.ItemsSource = _desserteParts[1];
            //    SecondStack.Visibility = Visibility.Visible;
            }
            else
            {
               // SecondStack.Visibility = Visibility.Collapsed;
            }

            if (2 < _desserteParts.Count())
            {
                //@todo Lateral animation
            }
        }

        internal void SetOrientation(string currentViewState)
        {
            if (currentViewState == "Portrait")
            {
            }
            else
            {
                //@TODO Dispatcher les dessetes sur les 2 stackpanels 
            }
        }


        public async void ScrollToBottom()
        {
            await _scrollViewer.ScrollToVerticalOffsetWithAnimation(_scrollViewer.ScrollableHeight, 5);
        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }

        private void FirstStack_OnLoaded(object sender, RoutedEventArgs e)
        {
            PreflareAnimation();
        }

        private void PreflareAnimation()
        {
            var sVEnum = FirstStack.GetDescendantsOfType<ScrollViewer>();
            var scrollV = sVEnum.FirstOrDefault(x => x.VerticalScrollMode != ScrollMode.Disabled);
            if (scrollV != null)
            {
                // Found the scrollViewer inside the ListView
                scrollV.Loaded += (o, args) =>
                {
                    _timer.Start();
                    _timer.Tick += delegate (object sender1, object m)
                    {
                        _timer.Stop();
                        _scrollViewer = o as ScrollViewer;
                        if (_scrollViewer != null) _scrollViewer.ViewChanged += ScrollViewerOnViewChanged;
                        ScrollToBottom();
                    };
                };
            }
        }

        private void ScrollViewerOnViewChanged(object sender, ScrollViewerViewChangedEventArgs scrollViewerViewChangedEventArgs)
        {
            if (Math.Abs(_scrollViewer.VerticalOffset - _scrollViewer.ScrollableHeight) < 0.4)
            {
                // scrolled to bottom
                var dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
                dispatcherTimer.Tick += async (o, o1) =>
                {
                    await _scrollViewer.ScrollToVerticalOffsetAsync(0);

                    dispatcherTimer.Stop();
                    dispatcherTimer = null;

                    ContinueAnimation();
                };
                dispatcherTimer.Start();
            }
        }

        private void ContinueAnimation()
        {
            _secondTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
            _secondTimer.Tick += SecondTimerOnTick;
            _secondTimer.Start();
        }

        private void SecondTimerOnTick(object sender, object o)
        {
            _secondTimer.Stop();
            ScrollToBottom();
        }
    }
}
