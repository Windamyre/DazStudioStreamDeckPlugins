using BarRaider.SdTools;
using BarRaider.SdTools.Events;
using BarRaider.SdTools.Wrappers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace VisualMenu
{
    //todo Fix PI weblink to Daz Plugin
    [PluginActionId("com.windamyre.daztools.customshortcut")]

    public class CustomShortcut : PluginBase
    {
        #region PluginSettingsClass
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "Creating Default Settings");
                PluginSettings instance = new PluginSettings();
                instance.actionList = new ActionList();
                instance.ActionName = string.Empty;
                instance.ShowActionIcon = false;
                instance.ShowActionTitle = false;
                return instance;
            }

            [JsonProperty(PropertyName = "showActionTitle")]
            public bool ShowActionTitle { get; set; }

            [JsonProperty(PropertyName = "showActionIcon")]
            public bool ShowActionIcon { get; set; }

            [JsonProperty(PropertyName = "DazLoaded")]
            public bool IsDazLoaded { get; set; }

            [JsonProperty(PropertyName = "actionName")]
            public string ActionName { get; set; }

            // TODO: combine actionList and ActionItems into one variable.
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

        #endregion

        #region Private Members
        private PluginSettings settings;
        private System.Drawing.Bitmap CustomImage;
        private System.Drawing.Bitmap DefaultImage;
        #endregion

        #region StreamDeckEvents

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
            if (System.IO.File.Exists("./Images/pluginIcon.png")) DefaultImage = new System.Drawing.Bitmap("./Images/pluginIcon.png");
            LoadActionData();
            SaveSettings();
            Connection.OnPropertyInspectorDidAppear += Connection_OnPropertyInspectorDidAppear;

        }

        private void Connection_OnPropertyInspectorDidAppear(object sender, SDEventReceivedEventArgs<PropertyInspectorDidAppear> e)
        {
            LoadActionData();
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
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create($"http://localhost:8080/action/{settings.ActionName}");
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

        public override void OnTick() { }

        public async override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, "Received Settings");

            Tools.AutoPopulateSettings(settings, payload.Settings);
            await SaveSettings();

            if (settings.ShowActionTitle)
            {
                string title = settings.ActionItems.Find(x => x.name.Equals(settings.ActionName)).text;
                await Connection.SetTitleAsync(WordWrapString(title));
            }
            else
            {
                await Connection.SetTitleAsync(string.Empty);
            }

            if (settings.ShowActionIcon)
            {
                string imageFileName = settings.ActionItems.Find(x => x.name.Equals(settings.ActionName)).icon;
                if (System.IO.File.Exists(imageFileName))
                {
                    CustomImage = new System.Drawing.Bitmap(imageFileName);
                    await Connection.SetImageAsync(CustomImage);
                }
            }
            else
            {
                await Connection.SetImageAsync(DefaultImage);
            }

        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #endregion

        #region Private Methods

        private void LoadActionData()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Loading Action Data");
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://localhost:8080/enumerate/");

                var response = req.GetResponse();
                string webcontent;
                using (var strm = new StreamReader(response.GetResponseStream()))
                {
                    webcontent = strm.ReadToEnd();
                }
                webcontent = "{\"actionItems\" : " + webcontent + "}";
                this.settings.actionList = JsonConvert.DeserializeObject<ActionList>((webcontent));

                this.settings.IsDazLoaded = true;
            }
            catch (WebException wEx)
            {
                Logger.Instance.LogMessage(TracingLevel.DEBUG, wEx.Message);
                this.settings.IsDazLoaded = false;
            }
            catch
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, "Unhandled error in LoadActionData");
            }
        }

        private Task SaveSettings()
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, "Saved Settings");
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        /// <summary>
        ///  Word wraps a single-line string based upon character count.  For strings exceeding the maxLineLimit it returns the first several lines, followed by the last line with ellipsis inserted.
        ///  For very long single words, it will truncate the word and add ellipsis.
        /// </summary>
        /// <param name="rawString">String: string to be formatted.</param>
        /// <param name="maxLineCount">Int: The maximum number of lines allowed.</param>
        /// <param name="maxLineLength">Int: the maximum number of characters per line.</param>
        /// <param name="insertEllipsis">Boolean: insert ellipsis in last line if maxLineCount </param>
        /// <returns>String: Formated with \n between lines.</returns>
        private string WordWrapString(string rawString, int maxLineCount = 4, int maxLineLength = 9, bool insertEllipsis = true)
        {
            List<string> lines = new List<string>();
            StringBuilder newLine = new StringBuilder();
            char[] seperators = { ' ', '\f', '\t', '\n', '\r' };

            string[] words = rawString.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string l in words)
            {
                if (newLine.Length != 0 && newLine.Length + l.Length > maxLineLength) //if the new word is too long for the line
                {
                    lines.Add(newLine.ToString().Trim());
                    newLine = new StringBuilder();
                }
                if (l.Length > (maxLineLength * 1.5))  //truncate really long single words
                {
                    newLine.Append(l.Substring(0, maxLineLength) + "…");
                }
                else
                {
                    newLine.Append(l + " ");
                }
            }

            lines.Add(newLine.ToString().Trim());  //slide in that last line

            if (lines.Count >= maxLineCount) //cut out the stuff in the middle if we're over maxLineCount
            {
                lines.RemoveRange(maxLineCount - 1, lines.Count - maxLineCount);
                if (insertEllipsis)
                {
                    lines[lines.Count - 1] = lines[lines.Count - 1].Insert(0, "…");
                }
                return string.Join("\n", lines.ToArray());
            }
            else
            {
                return string.Join("\n", lines.ToArray());
            }
        }

    }

    #endregion
}
