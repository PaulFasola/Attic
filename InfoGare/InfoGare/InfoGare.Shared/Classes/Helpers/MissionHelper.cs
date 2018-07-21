using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Infogare.Classes;
using Infogare.Classes.Helpers;
using Infogare.Classes.Models;

namespace InfoGare.Classes.Helpers
{
    public static class MissionHelper
    {
        public static async Task<InfoGareHelper> GetBoard(this Mission instance, string trigramme, string line)
        {
            var gare = new InfoGareHelper() { MissionStack = new ObservableCollection<Mission>() };
            var trains = await GareHelper.GetNextTrains(trigramme);
            var traintype = Converter.LineToTrainType(line);
            foreach (var element in trains.trains)
            {
                var desserteFormated = element.dessertes;
                desserteFormated = desserteFormated.Replace("&bull;", "·");
                Debug.WriteLine((short)gare.MissionStack.Count);
                gare.MissionStack.Add(new Mission("ms-appx:///Assets/Lines/" + line + ".png", "ms-appx:///Assets/Lines/" + traintype + ".png", element.mission, element.time, element.destination, element.retard, desserteFormated, (short)gare.MissionStack.Count));
            }
            return gare;
        }

        public static async Task<ObservableCollection<Mission>> CollectMissions(int currentCount, string line, string trigramme)
        {
            var toRetrieve = 6 - currentCount;
            var board = await new Mission().GetBoard(trigramme, line);
            var ret = new ObservableCollection<Mission>();
            const int i = 0;

            foreach (var item in board.MissionStack)
            {
                ret.Add(item);
                if (i == toRetrieve) break;
            }
            return ret;
        }

        public static string DetermineLineType(string line)
        {
            if (line == "T4") return "tram.png";
            var dico = new Dictionary<string, char> { { "A", 'm' }, { "b", 'm' }, { "c", 'm' }, { "d", 'm' }, { "e", 'm' } };
            return dico.ContainsKey(line) ? "rer.png" : "train.png";
        }
    }
}
