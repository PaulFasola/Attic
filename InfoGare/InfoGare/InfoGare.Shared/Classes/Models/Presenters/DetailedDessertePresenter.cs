using System;
using Windows.UI.Text;

namespace Infogare.Classes.Presenters
{
    public class DetailedDessertePresenter
    {
        private readonly string _stationName; 

        public string StationName
        { 
            get { return IsTerminus ? "" : _stationName; }
        }

        public FontWeight Weight
        {
            get { return IsTerminus ? FontWeights.Bold : FontWeights.Normal; }
            set { throw new NotImplementedException(); }
        }

        public string StationTerminus
        {
            get { return IsTerminus ? _stationName : ""; }
        }

        public bool IsTerminus { get; set; }

        public DetailedDessertePresenter(string stationName, bool isTerminus)
        {
            _stationName = stationName;  
            IsTerminus = isTerminus;
        }
    }
}
