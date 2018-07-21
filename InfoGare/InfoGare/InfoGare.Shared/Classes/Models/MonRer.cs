using System.Collections.Generic;

namespace Infogare.Classes.Providers
{
    public class Train
    {
        public string retard { get; set; }
        public string destination { get; set; }
        public string mission { get; set; }
        public string ligne { get; set; }
        public string dessertes { get; set; }
        public string time { get; set; }
        public string numero { get; set; }
        public string col2class { get; set; }
        public string trainclass { get; set; }
        public object platform { get; set; }
    }

    public class MonRerProvider
    {
        public List<Train> trains { get; set; }
        public List<object> info { get; set; }
        public List<string> lines { get; set; }
    }
}
