using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fasolib.Managers;
using Infogare.Classes;
using Infogare.Classes.Presenters;
using Infogare.Classes.Providers;
using InfoGare.Classes.Models;
using Newtonsoft.Json;

namespace InfoGare.Classes.Helpers
{
    internal static class GareHelper
    {
        internal static async Task<Dictionary<string, Gare>> GetGareStack(this Gare gare)
        {
            var gareList = new Dictionary<string, Gare>();

            var client = new HttpClient();
            var response = await client.GetAsync("http://ressources.data.sncf.com/explore/dataset/sncf-lignes-par-gares-idf/download/?format=json&timezone=Europe/Berlin").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync();

            // Parser via Provider
            var jsonStack = JsonConvert.DeserializeObject<List<RatpProvider>>(content);

            // Assemblage gare par gare
            foreach (var element in jsonStack)
            {
                var UIC = element.Fields.CodeUic;
                var info = Converter.UicToGareInformation(UIC.Remove(UIC.Count() - 1, 1));
                var passages = GetPassages(element.Fields);

                if (info != null && !gareList.ContainsKey(UIC)) gareList.Add((element.Fields.CodeUic), new Gare { Name = info.Item2, Trigramme = info.Item1, Uic = UIC, Passages = passages });
            }

            // Création du dictionnaire
            await ObjectManager.SerializeToFile(gareList, "listeGare.bin");
            return gareList;
        }

        private static Dictionary<string, bool> GetPassages(Fields fields)
        {
            var dico = new Dictionary<string, bool>
            {
                {"A", (fields.A != null)},
                {"B", (fields.B != null)},
                {"C", (fields.C != null)},
                {"D", (fields.D != null)},
                {"E", (fields.E != null)},
                {"H", (fields.H != null)},
                {"J", (fields.J != 0)},
                {"K", (fields.K != null)},
                {"L", (fields.L != null)},
                {"N", (fields.N != null)},
                {"P", (fields.P != null)},
                {"R", (fields.R != null)},
                {"U", (fields.U != null)},
                {"T4", (fields.T4 != null)},
                {"RER", (fields.Rer != null)},
                {"Bus", (fields.Bus != null)},
                {"Train", (fields.Train != 0)},
                {"Tram", (fields.Tram != null)}
            };
            return dico;
        }

        internal static ObservableCollection<GareSuggestionPresenter> GetStationsSuggestion(Dictionary<string, Gare> gares)
        {
            if (gares == null) return null;
            var gareSuggestion = new ObservableCollection<GareSuggestionPresenter>();
            foreach (var element in gares.Values)
            {
                var passages = ExtractLinesFromMatrix(element.Passages);
                foreach (var item in passages)
                    gareSuggestion.Add(new GareSuggestionPresenter(item, element.Name, element.Trigramme));
            }

            return gareSuggestion;
        }

        private static IEnumerable<string> ExtractLinesFromMatrix(Dictionary<string, bool> dictionary)
        {
            var displayableValue = new ObservableCollection<string>();
            var dico = new Dictionary<string, short> { { "ter", 0 }, { "train", 0 }, { "rer", 0 }, { "tram", 0 }, { "bus", 0 } };

            foreach (var element in dictionary.Where(element => element.Value && !dico.ContainsKey(element.Key.ToLower())))
            {
                displayableValue.Add("ms-appx:///Assets/Lines/" + element.Key + ".png");
            }

            return displayableValue;
        }

        internal static async Task<MonRerProvider> GetNextTrains(string trigramme)
        {
            var trainList = new ObservableCollection<MonRerProvider>();
            MonRerProvider JsonStack;

            var client = new HttpClient();
            var response = await client.GetAsync("http://monrer.fr/json?s=" + trigramme).ConfigureAwait(false);
            try
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                JsonStack = JsonConvert.DeserializeObject<MonRerProvider>(content);
            }
            catch (Exception)
            {
                return null;
            }

            return JsonStack;
        }
    }
}
