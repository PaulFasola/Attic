using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media; 

namespace Infoécran.Classes
{
    public class Mission : INotifyPropertyChanged
    {

        public string TransportType { get; private set; }
        public string TransportLogo { get; private set; }
        public string CodeMission { get; private set; }


        private string _AT;
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
        public string Desserte { get; private set; }
        public string BgColor { get; private set; }
        public string Height { get; set; }
        public string Width { get; set; }
        public Thickness CalibrationMargin { get; set; }
        public string VisibilityElement { get; set; }
        public string DesserteWidth { get; private set; }
        public Thickness TransportTypeMargin { get; set; }
        public Thickness TransportLogoMargin { get; set; }
        public string StationCheckIn { get; private set; }
        public bool TransitionState { get; set; }
        public double DisplaySize { get; set; }
        public string TansitionStart { get; set; }
        public string ProppelerWidth { get; private set; }
        public VerticalAlignment VAlign { get; private set; }

        private short _stackPosition;

        public Mission() { }
      
        public Mission(string transportType, string transportLogo, string codeMission, string arrivalTime, string terminus, string desserte, short stackPosition)
        {
            TransportType = transportType;
            TransportLogo = transportLogo;
            CodeMission = codeMission;
            ArrivalTime = arrivalTime;
            StationCheckIn = arrivalTime;
            Terminus = terminus;
            Desserte = desserte;

#if WINDOWS_APP
            ProppelerWidth = CodeMission == "MONA" ? "85" : "72";
            CalibrationMargin = new Thickness(35, 0, 0, 0);
#else
            ProppelerWidth = CodeMission == "MONA" ? "66" : "63";
#endif
            TransitionState = false;

            _stackPosition = (short)(stackPosition + 1);
            Initialize();
        }

        private void Initialize()
        {
                BgColor = ((_stackPosition % 2) == 0) ? "#043a6b" : "#0c5da5";
                //BgColor = ((_stackPosition % 2) == 0) ? "#ffffff" : "#000000";
                Width = Window.Current.Bounds.Width.ToString();
                DesserteWidth = (Desserte.Length * 10).ToString(); 
#if WINDOWS_PHONE_APP
                Height = (_stackPosition == 1 || _stackPosition == 2) ? "65" : "40";
                TransportTypeMargin =  (_stackPosition == 1 || _stackPosition == 2) ? new Thickness(4, 5, 0, 0) :new Thickness(4, 0, 0, 0);
                TransportLogoMargin = (_stackPosition == 1 || _stackPosition == 2) ?  new Thickness(4, 5, 0, 0) : new Thickness(4, 0, 0, 0);
#else
                Height = (_stackPosition == 1 || _stackPosition == 2) ? "110" : "75";
                TransportTypeMargin = (_stackPosition == 1 || _stackPosition == 2) ? new Thickness(4, 5, 0, 0) : new Thickness(4, 0, 0, 0);
                TransportLogoMargin = (_stackPosition == 1 || _stackPosition == 2) ? new Thickness(4, 5, 0, 0) : new Thickness(4, 0, 0, 0);
#endif


               // VAlign = (_stackPosition == 1 || _stackPosition == 2) ? VerticalAlignment.Top : VerticalAlignment.Center; 
                VisibilityElement = (_stackPosition == 1 || _stackPosition == 2) ? "Visible" : "Collapsed";


                


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
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

 