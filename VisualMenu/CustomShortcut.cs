using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;


namespace VisualMenu
{
    /// <summary>
    /// This is the active project
    /// </summary>
    [PluginActionId("com.windamyre.daztools.customshortcut")]

    public class CustomShortcut : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "Creating Default Settings");
                PluginSettings instance = new PluginSettings();
                instance.actionList = new ActionList();
                return instance;
            }
            /// <summary>
            /// The currently selected action's GUID as a string.
            /// </summary>
            [JsonProperty(PropertyName = "actionName")]
            public string ActionName { get; set; }
            /// <summary>
            /// List of actions
            // TODO: combine actionList and ActionItems into one variable.
            /// </summary>
            [JsonProperty(PropertyName = "actionList")]
            public ActionList actionList
            {
                get;
                set;
            } = new ActionList();

            [JsonProperty(PropertyName = "actionItemList")]
            public List<ActionList.ActionItem> ActionItems
            {
                get
                {
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, "Settings - actionItemList");
                    return actionList.actionItems;
                }
            }
        }

        #region Private Members
        private PluginSettings settings;
        #endregion

        public CustomShortcut(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {

            Logger.Instance.LogMessage(TracingLevel.INFO, "Constructor Loaded. ");

            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
                SaveSettings();
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
            LoadActionData();
            SaveSettings();
        }


        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Destructor called");
        }

        //TODO handle Daz Studio plug-in not loaded
        //TODO provide feedback based on the returning data
        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed ");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create($"http://localhost:8080/action/{settings.ActionName}");
            var response = req.GetResponse();
            string webcontent;
            using (var strm = new StreamReader(response.GetResponseStream()))
            {
                webcontent = strm.ReadToEnd();
            }
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public async override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, "Received Settings");

            Tools.AutoPopulateSettings(settings, payload.Settings);
            LoadActionData();
            await SaveSettings();

        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        //TODO: implement Connection_OnPropertyInspectorDidAppear event to reduce calls to this method.
        //TODO: add error handling with messages such as "No connection to Daz Studio Plug-in" and the like.
        private void LoadActionData()
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, "Loading Action Data");
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://localhost:8080/enumerate/");

            var response = req.GetResponse();
            string webcontent;
            using (var strm = new StreamReader(response.GetResponseStream()))
            {
                webcontent = strm.ReadToEnd();
            }
            webcontent = "{\"actionItems\" : " + webcontent + "}";
            this.settings.actionList = JsonConvert.DeserializeObject<ActionList>((webcontent));
        }

        private Task SaveSettings()
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, "Saved Settings");
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        #endregion
    }
}