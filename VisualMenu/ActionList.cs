using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualMenu
{
    public class ActionList
    {
        // [JsonProperty(PropertyName = "actionItems")]
        public List<ActionItem> actionItems = new List<ActionItem>();


    }
    public class ActionItem
    {
        [JsonProperty(PropertyName = "icon")]
        public string icon { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }
        [JsonProperty(PropertyName = "text")]
        public string text { get; set; }
        [JsonProperty(PropertyName = "custom")]
        public bool custom { get; set; }

    }


}
