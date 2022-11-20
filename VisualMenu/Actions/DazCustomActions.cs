using BarRaider.SdTools;
using BarRaider.SdTools.Events;
using BarRaider.SdTools.Wrappers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CustomShortcut.Support;


namespace VisualMenu
{
    //todo Make error message visible in PI when unable to communicate with Daz Plugin

    [PluginActionId("com.windamyre.daztools.dazcustomactions")]

    public class DazCustomActions : PluginBase
    {
        #region PluginSettingsClass
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "Creating Default Settings");
                PluginSettings instance = new PluginSettings();

                instance.ActionName = string.Empty;
                instance.ShowActionIcon = false;
                instance.ShowActionTitle = false;
                return instance;
            }

            private ActionItem _curentAction = new ActionItem();
            public ActionItem CurrentAction
            {
                get { return _curentAction; }
                set { _curentAction = value; }
            }

            [JsonProperty(PropertyName = "customIcon")]
            public string CustomIcon { get; set; }

            [JsonProperty(PropertyName = "showActionTitle")]
            public bool ShowActionTitle { get; set; }

            [JsonProperty(PropertyName = "showActionIcon")]
            public bool ShowActionIcon { get; set; }

            [JsonProperty(PropertyName = "DazLoaded")]
            public bool IsDazLoaded { get; set; }

            [JsonProperty(PropertyName = "actionName")]
            public string ActionName
            {
                get
                {
                    return CurrentAction.Name;
                }
                set
                {
                    CurrentAction = Actions.FirstOrDefault(x => x.Name == value);
                    if (CurrentAction is null) CurrentAction = new ActionItem();
                }
            }

            [JsonProperty(PropertyName = "iconName")]
            public string IconName { get; set; }

            [JsonProperty(PropertyName = "actionItemList")]
            public List<ActionItem> Actions
            {
                get; set;
            } = new List<ActionItem>();
        }
        #endregion

        #region Private Members

        static private DazCustomList ActionListing = new DazCustomList();
        private PluginSettings settings;
        static private System.Drawing.Bitmap DefaultImage;
        private bool isButtonDisplaySet = false;
        #endregion

        #region StreamDeckEvents

        public DazCustomActions(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Constructor Loaded. ");
            
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
                //ActionListing.LoadActions();  
                SaveSettings();
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
            if (System.IO.File.Exists("./Images/pluginIcon.png")) DefaultImage = new System.Drawing.Bitmap("./Images/dazCustomActions.png");
            ActionListing.LoadActions();
            SaveSettings();
            SetButtonDisplay();
            Connection.OnPropertyInspectorDidAppear += Connection_OnPropertyInspectorDidAppear;
        }

        private void Connection_OnPropertyInspectorDidAppear(object sender, SDEventReceivedEventArgs<PropertyInspectorDidAppear> e)
        {
            ActionListing.LoadActions();
            this.settings.Actions = ActionListing.Actions;
            SaveSettings();
        }

        public override void Dispose()
        {
            Connection.OnPropertyInspectorDidAppear -= Connection_OnPropertyInspectorDidAppear;
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Destructor called");
        }

        public override void KeyPressed(KeyPayload payload)
        {

            //TODO provide feedback based on the returning data
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");
            string RequestString = string.Empty;
            // Custom Actions are named with Guid so we check for that first
            if (Guid.TryParse(settings.ActionName, out Guid guid))
            {
                RequestString = $"http://localhost:8080/action/{settings.ActionName}";
            }
            else if (settings.ActionName.StartsWith("Dz"))
            {
                RequestString = $"http://localhost:8080/dazaction/{settings.ActionName}";
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, "Unhandled action name in KeyPress event");
            }
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(RequestString);
                var response = req.GetResponse();
                string webcontent;
                using (var strm = new StreamReader(response.GetResponseStream()))
                {
                    webcontent = strm.ReadToEnd();
                }
                this.settings.IsDazLoaded = true;
            }
            catch (WebException wEx)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "Cannot communicate with Daz Studio or plug-in");
                Logger.Instance.LogMessage(TracingLevel.DEBUG, wEx.Message);
                this.settings.IsDazLoaded = false;
            }
            catch
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, "Unhandled error in KeyPress event");
            }
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick()
        {
            if (!isButtonDisplaySet)
            {
                // SetButtonDisplay();
                //isButtonDisplaySet = true;
            }
        }

        public async override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, "Received Settings");
            Tools.AutoPopulateSettings(settings, payload.Settings);
            settings.CustomIcon = LoadImageData();
            await SaveSettings();
            SetButtonDisplay();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #endregion

        #region Private Methods

        private void CallCustomAction(string ActionName) { }


        private async Task SetButtonDisplay()
        {

            if (settings.ShowActionTitle)
            {
                string title = settings.CurrentAction.Text;  // settings.Actions.Find(x => x.Name.Equals(settings.ActionName)).Text;
                await Connection.SetTitleAsync(Utilities.WordWrapString(title));
            }
            else
            {
                await Connection.SetTitleAsync(string.Empty);
            }

            if (settings.ShowActionIcon)
            {

                await Connection.SetImageAsync(settings.CustomIcon);
            }
            else
            {
                await Connection.SetImageAsync(DefaultImage);
            }
        }


        private Task SaveSettings()
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, "Saved Settings");
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private string LoadImageData()
        {
            string imageFileName = string.Empty;
            string retvalue = string.Empty;

            ActionItem actionItem = settings.CurrentAction; // settings.Actions.Find(x => x.Name.Equals(settings.ActionName));

            if (actionItem.IsCustom)
            {
                imageFileName = actionItem.Icon;
            }
            else
            {
                imageFileName = $"./CustomIcons/{actionItem.Name}.png";
            }
            if (System.IO.File.Exists(imageFileName))
            {
                retvalue = Utilities.BitmapToBase64(new System.Drawing.Bitmap(imageFileName), System.Drawing.Imaging.ImageFormat.Bmp);
            }
            return retvalue;
        }









    }

    #endregion
}
