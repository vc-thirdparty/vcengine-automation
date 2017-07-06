using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VcEngineAutomation.Panels;

namespace VcEngineAutomation.Models
{
    public class Component
    {
        public string Name { get; set; }
        public string Vcid { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public Position Position { get; set; }
        public Rotation Rotation { get; set; }
        public double GetPropertyAsDouble(string key)
        {
            return double.Parse(Properties[key], CultureInfo.InvariantCulture);
        }
        public int GetPropertyAsInt(string key)
        {
            return int.Parse(Properties[key], CultureInfo.InvariantCulture);
        }

        public static List<Component> ParseAsList(string str)
        {
            return str.Split(',').Select(Parse).ToList();
        }
        public static Component Parse(string str)
        {
            return new Component
            {
                Name = str
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}