using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel; 
using System.Runtime.CompilerServices;
using Windows.Storage;
using System.Threading.Tasks;
using InfoGare.Classes;

namespace Info�cran.Classes
{
    public class InfoGare : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<Mission> _missions;
        private Dictionary<string, Gare> _gares;


        public async Task Init()
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("GareListGenerated"))
            {
                _gares = await Gare.GetGareStack();
                ApplicationData.Current.LocalSettings.Values.Add("GareListGenerated", "VALID");
            }
            else
            {
                _gares = (Dictionary<string, Gare>) await ObjectManager.DeserializeToObject<Dictionary<string, Gare>>("listeGare.bin") ?? new Dictionary<string, Gare>();
            }
        }

        public ObservableCollection<Mission> MissionStack
        {
            get
            {
                return _missions;
            }
            set
            {
                _missions = value;
                this.OnPropertyChanged("MissionStack");
            }
        }

        public Dictionary<string, Gare> Gares
        {
            get
            {
                return _gares;
            }
        }

        public void UpdateMissionStack()
        {
            try
            {
                if (MissionStack[1] == null) return;
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }

            MissionStack[1].VisibilityElement = "Visible";
            MissionStack[1].Height = "65";
            OnPropertyChanged("MissionStack");
        }

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void Refresh()
        {
            OnPropertyChanged("MissionStack");
        }
    }
}
