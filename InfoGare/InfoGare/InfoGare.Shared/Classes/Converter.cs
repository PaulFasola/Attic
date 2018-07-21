using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace Infogare.Classes
{
    public static class Converter
    {
        private static async Task<Dictionary<string, Tuple<string, string>>> ParseXml()
        {
            var liste = new Dictionary<string, Tuple<string, string>>();

            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/TrigrammeData.xml")).AsTask().ConfigureAwait(false);
            var content = new StreamReader(await file.OpenStreamForReadAsync().ConfigureAwait(false)).ReadToEnd();
            var doc = XDocument.Parse(content);
            var items = new List<TrigrammeStation>();
            if (doc.Root != null)
            {
                items = (from r in doc.Root.Elements("Gare")
                         select new TrigrammeStation
                         {
                             Trigramme = (string)r.Element("Trigramme"),
                             StationName = (string)r.Element("StationName"),
                             Uic = (string)r.Element("Uic"),
                             IsTransilien = (short)r.Element("IsTransilien")
                         }).ToList();
            }

            foreach (var element in items)
            {

                if (liste.ContainsKey(element.Uic)) continue;
                liste.Add(element.Uic, new Tuple<string, string>(element.Trigramme, element.StationName));
            }
            return liste;
        }

        public static string UicToTrigramme(string Uic)
        {
            var dico = ParseXml().GetAwaiter().GetResult();
            if (dico.ContainsKey(Uic)) return dico[Uic].Item1;
            return "ERR";
        }

        public static Tuple<string, string> UicToGareInformation(string Uic)
        {
            var dico = ParseXml().GetAwaiter().GetResult();
            return dico.ContainsKey(Uic) ? dico[Uic] : null;
        }

        internal static string LineToTrainType(string line)
        {
            return (line == "A" || line == "B" || line == "C" || line == "D") ? "rer" : "train";
        }
    }
}
