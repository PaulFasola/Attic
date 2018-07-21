using System.Collections.Generic;
using System.Runtime.Serialization;
using Infogare.Classes;
using Infogare.Enums;
using InfoGare.Enums;

namespace InfoGare.Classes.Models
{
    [DataContract]
    public class Gare
    {
        [DataMember]
        internal string Trigramme { get; set; }

        [DataMember]
        internal string Name { get; set; }

        [DataMember]
        internal string Uic { get; set; }

        [DataMember]
        internal List<VehiculeType> VehiculeTypeDesserte { get; set; }

        [DataMember]
        internal List<LineType> LineTypeDesserte { get; set; }

        [DataMember]
        internal Dictionary<string, bool> Passages { get; set; }

        internal Tags Details { get; set; }
    }
}
