using BarRaider.SdTools;
using System.Net;
using BarRaider.SdTools.Events;
using BarRaider.SdTools.Wrappers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace VisualMenu
{
    interface IActionList
    {
        List<ActionItem> Actions
        {
            get;
        }
        DateTime LastLoaded
        {
            get;
        }
        bool LoadActions();
    }
    public class DazStudioList : IActionList
    {
        public DazStudioList()
        {

        }
        private List<ActionItem> _actions = new List<ActionItem>();

        public List<ActionItem> Actions
        {
            get { return _actions; }
        }
        private DateTime _lastLoaded;

        public DateTime LastLoaded
        {
            get { return _lastLoaded; }
        }
        public bool LoadActions()
        {
            TimeSpan ts = DateTime.Now.Subtract(_lastLoaded);
            if (ts.Seconds > 30 || _actions.Count < 1)
            {
                try
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://localhost:8080/enumerate/");
                    var resp = req.GetResponse();
                    string webContent = string.Empty;
                    using (var strm = new StreamReader(resp.GetResponseStream()))
                    {
                        webContent = strm.ReadToEnd();
                    }
                    _actions = JsonConvert.DeserializeObject<List<ActionItem>>((webContent));
                    _actions.Sort(ActionItem.CompareActions);
                    _lastLoaded = DateTime.Now;

                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

    }
    public class DazCustomList : IActionList
    {
        public DazCustomList()
        {

        }
        private List<ActionItem> _actions = new List<ActionItem>();

        public List<ActionItem> Actions
        {
            get { return _actions; }
        }
        private DateTime _lastLoaded;

        public DateTime LastLoaded
        {
            get { return _lastLoaded; }
        }
        public bool LoadActions()
        {
            TimeSpan ts = DateTime.Now.Subtract(_lastLoaded);
            if (ts.Seconds > 30 || _actions.Count < 1)
            {
                try
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://localhost:8080/enumerate/");
                    var resp = req.GetResponse();
                    string webContent = string.Empty;
                    using (var strm = new StreamReader(resp.GetResponseStream()))
                    {
                        webContent = strm.ReadToEnd();
                    }
                    _actions = JsonConvert.DeserializeObject<List<ActionItem>>(webContent);
                    _actions.RemoveAll(x => !x.IsCustom);
                    _actions.Sort(ActionItem.CompareActions);
                    _lastLoaded = DateTime.Now;

                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
    }
    public class DazDefinedList : IActionList
    {
        private List<ActionItem> _actions;

        public List<ActionItem> Actions
        {
            get { return _actions; }
        }
        private DateTime _lastLoaded;

        public DateTime LastLoaded
        {
            get { return _lastLoaded; }
        }
        private string _sourceFile;

        public string SourceFile
        {
            get { return _sourceFile; }
            set { _sourceFile = value; }
        }

        public DazDefinedList(string sourceFile)
        {
            _sourceFile = sourceFile;
        }

        public bool LoadActions()
        {
            //todo add appropriate try-catch
            if (string.IsNullOrEmpty(SourceFile)) return false;
            if (!File.Exists(SourceFile)) return false;
            string fileConent = File.ReadAllText(SourceFile);
            _actions = JsonConvert.DeserializeObject<List<ActionItem>>(fileConent);
            _actions.Sort(ActionItem.CompareActions);
            _lastLoaded = DateTime.Now;

            return true;
        }

    }
    public class ActionItem
    {
        public ActionItem()
        {

        }
        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = string.Empty;
    
        private string _text;
        [JsonProperty(PropertyName = "text")]
        public string Text
        {
            get { return _text; }
            set 
            {
                _text = value;
                _text = _text.Replace("&", "");
            }
        }

        [JsonProperty(PropertyName = "custom")]
        public bool IsCustom
        {
            get
            {
                return Guid.TryParse(Name, out Guid guid);
            }
        }

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
                    if (x.IsCustom)
                    {
                        if (!y.IsCustom)
                        {
                            return -11;
                        }
                        else
                        {
                            return (x.Text.CompareTo(y.Text));
                        }
                    }
                    else
                    {
                        if (y.IsCustom)
                        {
                            return 1;
                        }
                        else
                        {
                            return (x.Text.CompareTo(y.Text));
                        }
                    }
                }
            }
        }
    }

}
