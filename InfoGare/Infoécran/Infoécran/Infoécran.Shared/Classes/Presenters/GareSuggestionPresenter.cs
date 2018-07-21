using System.Runtime.Serialization;

namespace Infoécran.Classes.Presenters
{
    [DataContract]
    public class GareSuggestionPresenter
    {
        [DataMember]
        public string Logo { get; private set; }
        [DataMember]
        public string GareName { get; private set; }
        [DataMember]
        public string Trigramme { get; private set; }

        public GareSuggestionPresenter(string logoPath, string stationName, string trigramme)
        {
            Logo = logoPath;
            GareName = stationName;
            Trigramme = trigramme;
        }

        public override string ToString()
        {
            return GareName;
        }
    }
}
