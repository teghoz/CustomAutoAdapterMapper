using System.Collections.Generic;
using System.Linq;

namespace CustomAutoAdapterMapper
{
    public class Option
    {
        public string RootKey { get; set; }
        public string ItemKey { get; set; }
        public Dictionary<string, string> Mappings { get; set; }
        public List<string> MappingKeys 
        {
            get
            {
                if(Mappings == null)
                {
                    return new List<string>();
                }
                else
                {
                    return Mappings
                        .Select(k => k.Key.ToString())
                        .ToList();
                }
            }
        }
    }
}
