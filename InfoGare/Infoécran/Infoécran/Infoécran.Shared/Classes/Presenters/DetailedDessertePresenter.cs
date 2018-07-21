using Windows.UI.Text; 

namespace Infoécran.Classes.Presenters
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
            set { throw new System.NotImplementedException(); }
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
