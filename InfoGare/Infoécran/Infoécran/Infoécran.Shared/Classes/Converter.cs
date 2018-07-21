using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage; 

namespace Infoécran.Classes
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
            if (dico.ContainsKey(Uic)) return dico[Uic];
            return null;
        }


        internal static string LineToTrainType(string line)
        {
            if (line == "A" || line == "B" || line == "C" || line == "D")
                return "rer";
            else
            {
                return "train";
            }
        }

 
              public static double CoodinateToDistance(double lat1, double lon1, double lat2, double lon2, char unit ='K') {
              double theta = lon1 - lon2;
              double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
              dist = Math.Acos(dist);
              dist = rad2deg(dist);
              dist = dist * 60 * 1.1515;
              switch (unit)
              {
                  case 'K':
                      dist = dist * 1.609344;
                      break;
                  case 'N':
                      dist = dist * 0.8684;
                      break;
              }
              return (dist);
            }

            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            //::  This function converts decimal degrees to radians             :::
            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            private static double deg2rad(double deg) {
              return (deg * Math.PI / 180.0);
            }

            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            //::  This function converts radians to decimal degrees             :::
            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            private static double rad2deg(double rad) {
              return (rad / Math.PI * 180.0);
            }
    }
}