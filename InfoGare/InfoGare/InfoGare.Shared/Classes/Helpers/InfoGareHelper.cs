using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Fasolib.Managers;
using Infogare.Classes.Models;
using InfoGare.Classes.Helpers;
using InfoGare.Classes.Models;

namespace Infogare.Classes.Helpers
{
    public class InfoGareHelper : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<Mission> _missions;

        public async Task Init()
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("GareListGenerated"))
            {
                Gares = await new Gare().GetGareStack();
                ApplicationData.Current.LocalSettings.Values.Add("GareListGenerated", "VALID");
            }
            else
            {
                Gares = (Dictionary<string, Gare>)await ObjectManager.DeserializeToObject<Dictionary<string, Gare>>("listeGare.bin") ?? new Dictionary<string, Gare>();
            }

            foreach (var element in Gares)
            {
                element.Value.Name = element.Value.Name.Replace("RER E", "");
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

        public Dictionary<string, Gare> Gares { get; private set; }

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
            MissionStack[1].Height = MissionStack[0].Height;
            OnPropertyChanged("MissionStack");
        }

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void Refresh()
        {
            OnPropertyChanged("MissionStack");
        }
    }
}
