using System;
using Newtonsoft.Json;

namespace Infoécran.Classes
{
    public class GeoProvider
    {
        public float version { get; set; }
        public string generator { get; set; }
        public Osm3s osm3s { get; set; }
        public Element[] elements { get; set; }
    }

    public class Osm3s
    {
        public DateTime timestamp_osm_base { get; set; }
        public string copyright { get; set; }
    }

    public class Element
    {
        public string type { get; set; }
        public long id { get; set; }
        public float lat { get; set; }
        public float lon { get; set; }
        public Tags tags { get; set; }
        public long[] nodes { get; set; }
    }

    public class Tags
    {
        public string STIFzone { get; set; }
        public string name { get; set; }
        public string railway { get; set; }
        [JsonProperty(PropertyName = "ref:FR:RATP")]
        public string refFRRATP { get; set; }
        [JsonProperty(PropertyName = "type:RATP")]
        public string typeRATP { get; set; }
        public string uic_ref { get; set; }
        public string wikipedia { get; set; }
        public string wheelchair { get; set; }
        public string _operator { get; set; }
        public string _ref { get; set; }
        public string official_name { get; set; }
        [JsonProperty(PropertyName = "route_ref:FR:RER")]
        public string route_refFRRER_D { get; set; }
        [JsonProperty(PropertyName = "ref:SNCF:RER")]
        public string refSNCFRER { get; set; }
        public string source { get; set; }
        public string start_date { get; set; }
        public string typeSNCF { get; set; }
        public string refSNCFTransilien { get; set; }
        public string SNCFhackesscompleteness { get; set; }
        public string notefr { get; set; }
        public string pictogram { get; set; }
        public string public_transport { get; set; }
        public string signage { get; set; }
        public string usage { get; set; }
        public string old_name { get; set; }
        public string opening_hours { get; set; }
        public string refSNCF { get; set; }
        public string namefr { get; set; }
        public string nameru { get; set; }
        public string namede { get; set; }
        public string namepl { get; set; }
        public string is_in { get; set; }
        public string platforms { get; set; }
        public string rail { get; set; }
        public string train { get; set; }
        public string alt_name { get; set; }
        public string namezh { get; set; }
        public string station { get; set; }
        public string note { get; set; }
        public string layer { get; set; }
        public string level { get; set; }
        public string short_name { get; set; }
        public string route_ref { get; set; }
        public string wikidata { get; set; }
        public string network { get; set; }
        public string comment { get; set; }
        public string namees { get; set; }
        public string nameuk { get; set; }
        public string building { get; set; }
        public string subway { get; set; }
        public string buildingpart { get; set; }
        public string website { get; set; }
        public string websiteschedules { get; set; }
        public string bench { get; set; }
        public string shelter { get; set; }
        public string wall { get; set; }
        public string uic_name { get; set; }
        public string image { get; set; }
        public string architect { get; set; }
        public string sourcerailway { get; set; }
        public string _3drtype { get; set; }
        public string buildinglevels { get; set; }
        public string buildingmaterial { get; set; }
        public string roofshape { get; set; }
    }

}