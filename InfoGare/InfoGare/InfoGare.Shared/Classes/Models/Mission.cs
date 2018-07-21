using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Infogare.Classes.Models
{
    public class Mission : INotifyPropertyChanged
    {
        public string TransportType { get; set; }
        public string TransportLogo { get; set; }
        public string CodeMission { get; set; }

        public string ArrivalTime
        {
            get { return _AT; }
            set
            {
                _AT = value;
                OnPropertyChanged("ArrivalTime");
            }
        }

        public string Terminus { get; set; }
        public string Desserte { get; set; }
        public string BgColor { get; set; }
        public string Height { get; set; }
        public string Width { get; set; }
        public Thickness CalibrationMargin { get; set; }
        public string VisibilityElement { get; set; }
        public string DesserteWidth { get; set; }
        public Thickness TransportTypeMargin { get; set; }
        public Thickness TransportLogoMargin { get; set; }

        public string StationCheckIn { get; set; }

        public string StationCheckSet
        {
            get { return StationCheckIn; }
            set
            {
                StationCheckIn = value;
                OnPropertyChanged("StationCheckIn");
            }
        }

        public bool TransitionState { get; set; }
        public double DisplaySize { get; set; }
        public string TansitionStart { get; set; }
        public string ProppelerWidth { get; private set; }
        public VerticalAlignment VAlign { get; set; }
        public short StackPosition { get; set; }
        public string Retard { get; set; }

        private string _AT;

#if WINDOWS_PHONE_APP
        private long _stackPosition;
#endif

        public Mission() { }

        public Mission(string transportType, string transportLogo, string codeMission, string arrivalTime, string terminus, string retard, string desserte, short stackPosition)
        {
            TransportType = transportType;
            TransportLogo = transportLogo;
            CodeMission = codeMission;
            ArrivalTime = arrivalTime;
            StationCheckIn = arrivalTime;
            StackPosition = stackPosition;
            Terminus = terminus;
            Desserte = desserte;
            Retard = retard;

            Debug.WriteLine(arrivalTime);
#if WINDOWS_APP
            ProppelerWidth = CodeMission == "MONA" ? "85" : "72";
            CalibrationMargin = new Thickness(35, 0, 0, 0);
#else
            ProppelerWidth = CodeMission == "MONA" ? "66" : "63";
#endif
            TransitionState = false;
            StackPosition = (short)(StackPosition + 1);

            Initialize();
        }

        internal void Initialize()
        {
            BgColor = ((StackPosition % 2) == 0) ? "#043a6b" : "#0c5da5";
            Width = Window.Current.Bounds.Width.ToString();
            DesserteWidth = (Desserte.Length * 10).ToString();
#if WINDOWS_PHONE_APP
                Height = (_stackPosition == 1 || _stackPosition == 2) ? "65" : "40";
                TransportTypeMargin =  (_stackPosition == 1 || _stackPosition == 2) ? new Thickness(4, 5, 0, 0) :new Thickness(4, 0, 0, 0);
                TransportLogoMargin = (_stackPosition == 1 || _stackPosition == 2) ?  new Thickness(4, 5, 0, 0) : new Thickness(4, 0, 0, 0);
#else
            Height = (StackPosition == 1 || StackPosition == 2) ? "110" : "75";
            TransportTypeMargin = (StackPosition == 1 || StackPosition == 2) ? new Thickness(4, 5, 0, 0) : new Thickness(4, 0, 0, 0);
            TransportLogoMargin = (StackPosition == 1 || StackPosition == 2) ? new Thickness(4, 5, 0, 0) : new Thickness(4, 0, 0, 0);
#endif 
            VisibilityElement = (StackPosition == 1 || StackPosition == 2) ? "Visible" : "Collapsed";

            var tbl = new TextBlock
            {
                Text = CodeMission,
                FontFamily = new FontFamily("ms-appx:/Assets/Fonts/helvetica.otf#Helvetica LT Std Cond")
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
         
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

