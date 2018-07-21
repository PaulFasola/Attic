using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Infoécran.Classes
{
    [XmlRoot(ElementName = "Root")]
    public class TrigrammeStation
    {
        public string Trigramme { get; set; }
        public string StationName { get; set; }
        public string Uic { get; set; }
        public short IsTransilien { get; set; }
    }
}
