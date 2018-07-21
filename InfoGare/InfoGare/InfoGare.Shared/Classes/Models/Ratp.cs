using Newtonsoft.Json;

namespace Infogare.Classes
{
    internal class Parameters
    {

        [JsonProperty("rows")]
        public int Rows { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("facet")]
        public string[] Facet { get; set; }

        [JsonProperty("dataset")]
        public string[] Dataset { get; set; }
    }

    internal class Fields
    {
        [JsonProperty("code_uic")]
        public string CodeUic { get; set; }

        [JsonProperty("train")]
        public int Train { get; set; }

        [JsonProperty("libelle_point_arret")]
        public string LibellePointArret { get; set; }

        [JsonProperty("j")]
        public int J { get; set; }

        [JsonProperty("rer")]
        public int? Rer { get; set; }

        [JsonProperty("c")]
        public int? C { get; set; }

        [JsonProperty("n")]
        public int? N { get; set; }

        [JsonProperty("tram")]
        public int? Tram { get; set; }

        [JsonProperty("t4")]
        public int? T4 { get; set; }

        [JsonProperty("e")]
        public int? E { get; set; }

        [JsonProperty("l")]
        public int? L { get; set; }

        [JsonProperty("d")]
        public int? D { get; set; }

        [JsonProperty("h")]
        public int? H { get; set; }

        [JsonProperty("a")]
        public int? A { get; set; }

        [JsonProperty("ter")]
        public int? Ter { get; set; }

        [JsonProperty("r")]
        public int? R { get; set; }

        [JsonProperty("p")]
        public int? P { get; set; }

        [JsonProperty("bus")]
        public int? Bus { get; set; }

        [JsonProperty("b")]
        public int? B { get; set; }

        [JsonProperty("k")]
        public int? K { get; set; }

        [JsonProperty("u")]
        public string U { get; set; }
    }

    internal class RatpProvider
    {

        [JsonProperty("datasetid")]
        public string Datasetid { get; set; }

        [JsonProperty("recordid")]
        public string Recordid { get; set; }

        [JsonProperty("fields")]
        public Fields Fields { get; set; }

        [JsonProperty("record_timestamp")]
        public string RecordTimestamp { get; set; }
    }

    internal class Facet
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    internal class FacetGroup
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("facets")]
        public Facet[] Facets { get; set; }
    }

}

namespace Infogare.Classes
{

    internal class SampleResponse1
    {

        [JsonProperty("nhits")]
        public int Nhits { get; set; }

        [JsonProperty("parameters")]
        public Parameters Parameters { get; set; }

        [JsonProperty("records")]
        public RatpProvider[] Records { get; set; }

        [JsonProperty("facet_groups")]
        public FacetGroup[] FacetGroups { get; set; }
    }

}
