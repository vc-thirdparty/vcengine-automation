using System.Collections.Generic;

namespace VcEngineAutomation.Models
{
    public class ComponentFeature
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Dictionary<string,string> Properties { get; set; }
    }
}