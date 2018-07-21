using Infoécran.Classes.Presenters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Fasolib.Helpers;

namespace Infoécran.Classes
{
    public class InfoTraffic
    {
        public static InfoTrafficPresenter GetDefaultInfo()
        {
            var rInfo = new string[]
            {
                "Votre téléphone est précieux, il peut faire des envieux. Nous vous conseillons d'être vigilants si vous l'utilisez en public.",
                "Pour la sécurité de tous, nous vous invitons à surveiller vos bagages et à les garder près de vous. N'hésitez pas à nous signaler tout paquet qui vous paraîtrait abandonné.",
                "Pour ne pas tenter les pickpockets, fermez bien votre sac et surveillez vos objets personnels."
            };
            return new InfoTrafficPresenter {ImportanceColor = "FF043a6b", Title = "Attentifs, ensemble", Description = rInfo[new Random().Next(0, rInfo.Count())]};
        }

        public static async Task<InfoTrafficPresenter> RetrieveInformationromLine(string line)
        {
            var obj = new InfoTrafficPresenter();
            var client = new HttpClient();
            string content = "";
            try
            {
                var response = await client.GetAsync("http://www.transilien.com/flux/rss/traficLigne?codeLigne=" + line).ConfigureAwait(false);
                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                ErrorManager.Log(e);
                return null;
            }

            InfoTrafficPresenter[] infoTrafficPresenters  = null;
            try
            {
                  var a = from e in XElement.Parse(content).Descendants("item")
                          select new InfoTrafficPresenter{
                            Description = e.Element("description").Value,
                            Link = e.Element("link").Value,
                            ImportanceColor = "FFFABB51",
                            Title = e.Element("title").Value,
                            PubDate = e.Element("pubDate").Value
                        };
                  infoTrafficPresenters = a as InfoTrafficPresenter[] ?? a.ToArray();
            }
            catch (Exception e)
            {
                ErrorManager.Log(e);
                return null;
            }
           
            if (infoTrafficPresenters.Any())
            {
                obj = infoTrafficPresenters.First();
            }
            else
            {
                try
                {
                    var response = await client.GetAsync("http://winapps.paulfasola.fr/Infogare/news/").ConfigureAwait(false);
                    content = await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    ErrorManager.Log(e);
                    return null;
                }
             

                if (content != null)
                {
                    string[] parse = content.Split(';');
                    string link = "", color = "", txt = "";
                    var i = 1;

                    foreach (var item in parse)
                    {
                        if (i == 1) txt = item;
                        if (i == 2) color = item;
                        if (i == 3) link = item;
                        i++;
                    }

                    if (txt == "")
                    { 
                        obj = (TupleHelper.GetRandomInt(0, 2) == 2) ? GetDefaultInfo() : null;
                    }
                    else
                    { 
                        obj = new InfoTrafficPresenter() { Description = txt, Title = "", ImportanceColor = "FFFABB51", Link = link };
                    }
                
                }
                else
                { 
                        obj = (TupleHelper.GetRandomInt(0, 2) == 2) ? GetDefaultInfo() : null; 
                }
            }


            return obj;
        }
    }
}
