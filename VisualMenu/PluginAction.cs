using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisualMenu;
using WindowsInput;

namespace VisualMenu
{
    /// <summary>
    /// This is the active project
    /// </summary>
    // TODO: Rework menu selection.  Instead of picking a menu's json file, set the Daz Studio Library folder and have plugin search folder for compatible menus.  Then provide dropdown list of choices.
    [PluginActionId("com.windamyre.daztools.visualmenu")]

    public class PluginAction : PluginBase
    {
        private vmMenu CurrentVM;
       
        static Dictionary<string, vmMenu> vmDict = new Dictionary<string, vmMenu>();
   

        /// <summary>
        /// Modifier Keys for Virtual Menu Shortcut.  eg. ctrl, shift, alt
        /// </summary>
        private List<WindowsInput.Native.VirtualKeyCode> virtualKeyCodes = new List<WindowsInput.Native.VirtualKeyCode>();
        /// <summary>
        /// Actual Key for Virtual Menu Shortcut.
        /// </summary>
        private WindowsInput.Native.VirtualKeyCode VirtualKeyCode;

        /// <summary>
        /// Row and Column of the StreamDeck button
        /// </summary>
        private int sdRow, sdCol;


        private class PluginSettings
        {

            public static PluginSettings CreateDefaultSettings()
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "Creating Default Settings");
                PluginSettings instance = new PluginSettings();
                instance.VMFilename = string.Empty;
                instance.KeypressDelay = "25";

                return instance;
            }

            [FilenameProperty]
            [JsonProperty(PropertyName = "vm")]
            public string VMFilename { get; set; }

            [JsonProperty(PropertyName = "keypressDelay")]
            public string KeypressDelay { get; set; }


        }

        #region Private Members

        private PluginSettings settings;

        #endregion



        public PluginAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            //set starter values
            sdRow = payload.Coordinates.Row;
            sdCol = payload.Coordinates.Column;
            LoadActionData();
        }


        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Destructor called");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed " + CurrentVM.GetButtonAt(0, 0).buttonLabel);

            InputSimulator iis = new InputSimulator();
            int keypressDelay;

            if (!int.TryParse(settings.KeypressDelay, out keypressDelay)) keypressDelay = 50; //if they keypress delay is corrupted, substitue 'safe' value.

            iis.Keyboard.ModifiedKeyStroke(virtualKeyCodes, VirtualKeyCode);  //Press key combo to bring up Virtual Menu
            Thread.Sleep(keypressDelay);  //wait for menu to load up

            int tabCount = CurrentVM.GetTabToButton(sdCol, sdRow);
            for (int i = 0; i <= tabCount; i++)  //press [TAB] with delay until we get to the correct action
            {
                iis.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.TAB);
                Thread.Sleep(keypressDelay);
            }
            iis.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);  //press [ENTER] to activate the mneu

        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public async override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Received Settings");

            Tools.AutoPopulateSettings(settings, payload.Settings);
            await SaveSettings();
            LoadActionData();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private void LoadActionData()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "LoadActionData called");
            //check if menu is in dictionary.  If not, see if file exists.  If it does exists, load it and add to dictonary.

            if (!vmDict.TryGetValue(settings.VMFilename, out CurrentVM))
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "No menu found. Loading");
                if (!System.IO.File.Exists(settings.VMFilename)) return;  //if the VM file does not exist, abort.  We can do nothing.
                CurrentVM = JsonConvert.DeserializeObject<vmMenu>(File.ReadAllText(settings.VMFilename));
                vmDict.Add(settings.VMFilename, CurrentVM);
            }

            string label = CurrentVM.GetButtonAt(sdCol, sdRow).buttonLabel ?? "no action /n assigned";  //assign label or error message as Buttton Title

            int lineLength = 0;
            StringBuilder sb = new StringBuilder(label);
            for (int i = 0; i < sb.Length; i++)
            {
                lineLength++;
                if (lineLength > 7 && char.IsWhiteSpace(sb[i]))
                {
                    sb[i] = '\n';
                    lineLength = 0;
                }
            }
            label = sb.ToString();

            this.Connection.SetTitleAsync(label);


            //parse out shortcut key
            if (CurrentVM.Shortcut.Contains("Alt")) virtualKeyCodes.Add(WindowsInput.Native.VirtualKeyCode.MENU);
            if (CurrentVM.Shortcut.Contains("Ctrl")) virtualKeyCodes.Add(WindowsInput.Native.VirtualKeyCode.CONTROL);
            if (CurrentVM.Shortcut.Contains("Shift")) virtualKeyCodes.Add(WindowsInput.Native.VirtualKeyCode.SHIFT);
            VirtualKeyCode = (WindowsInput.Native.VirtualKeyCode)CurrentVM.GetShortcutKey();

            //Check if there's an icon and assign it as Button Image
            if (!string.IsNullOrEmpty(CurrentVM.GetButtonAt(sdCol, sdRow).buttonLayout.IconPathAbsolute))
            {
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(CurrentVM.GetButtonAt(sdCol, sdRow).buttonLayout.IconPathAbsolute);
                this.Connection.SetImageAsync(bitmap);
            }
        }

        private Task SaveSettings()
        {

            Logger.Instance.LogMessage(TracingLevel.INFO, "Saved Settings");
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        #endregion
    }
}