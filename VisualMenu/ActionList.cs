using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VisualMenu
{
    public class ActionList
    {
        List<ActionItem> _actionItems = new List<ActionItem>();
        // [JsonProperty(PropertyName = "actionItems")]
        public List<ActionItem> ActionItems
        {
            get
            {
                return _actionItems;
            }
            set
            {
                value.Sort(ActionItem.CompareActions);
                _actionItems = value;
            }
        }
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;

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
        [JsonProperty(PropertyName = "optGroup")]
        public string optGroup { get; set; }


        public static int CompareActions(ActionItem x, ActionItem y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    if (x.custom)
                    {
                        if (!y.custom)
                        {
                            return -11;
                        }
                        else
                        {
                            return (x.text.CompareTo(y.text));
                        }
                    }
                    else
                    {
                        if (y.custom)
                        {
                            return 1;
                        }
                        else
                        {
                            return (x.text.CompareTo(y.text));
                        }
                    }
                }
            }
        }
    }


}
