using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualMenu
{

    /// <summary>
    /// Class that loads in JSON data from Virtual Menu.  Many variables violate UPPER/lower case rules to line up with JSON file.
    /// </summary>
    public class vmMenu
    {
        /// <summary>
        /// The displayed label of the Virtual Menu instance
        /// </summary>
        public string MenuLabel { get; set; }
        public string MenuName { get; set; }
        /// <summary>
        /// A string representing the keyboard combination used to invoke the Virtual Menu
        /// </summary>
        public string Shortcut { get; set; }
        public Template Template_Info { get; set; }
        public List<VmButton> ActionList { get; set; }
        /// <summary>
        /// Returns a VmButton object for the Virtual Menu action at a given column and row
        /// </summary>
        /// <param name="col">Integer: Zero index Columnon Virtual Menu</param>
        /// <param name="row">Integer: Zero index Row on Virtual Menu</param>
        /// <returns>VisualMenu.VmMenu.VmButton</returns>
        public VmButton GetButtonAt(int col, int row) =>  ActionList.First(x => x.Column == col.ToString() && x.Row == row.ToString());


        /// <summary>
        /// Returns a VmButton object for the Virtual Menu action at a given column and row. Converts string to int
        /// </summary>
        /// <param name="col">String: Zero index Columnon Virtual Menu</param>
        /// <param name="row">String: Zero index Columnon Virtual Menu</param>
        /// <returns>VisualMenu.VmMenu.VmButton if action exists</returns>
        public VmButton GetButtonAt(string col, string row) => ActionList.First(x => x.Column == col && x.Row == row);


        /// <summary>
        /// Returns the number of [TAB] presses required to get to particular key.  Takes into account icons.
        /// </summary>
        /// <param name="col">Integer: Zero index Columnon Virtual Menu</param>
        /// <param name="row">Integer: Zero index Columnon Virtual Menu</param>
        /// <returns>Integer: Tab count</returns>
        public int GetTabToButton(int col, int row)
        {
            int count = (row * 5) + col;  //todo: expand beyond standard steamdeck with 5 keys per row
            int retValue = count + 1;
            for (int i = 0; i <= count; i++) { if (ActionList[i].buttonLayout.IconPathAbsolute != null) retValue++; } //scan through action list to check for icons.  Each icon requires an additional [TAB] key.

            return retValue;
        }
        /// <summary>
        /// Returns the ASCII value of the key used to call the Virutal Menu.  This number corresponds to the VirtualKeyCode.  Does not include any modifier key data.
        /// </summary>
        /// <returns>ASCII value of key used if present.  Otherwise, returns 0</returns>
        public int GetShortcutKey()
        {
            char shortcutChar = this.Shortcut.Last();   //todo change from 'last character' to 'everything after last plus sign'
            if (char.IsLetterOrDigit(shortcutChar)) return ((int)shortcutChar);  //todo: expand to other keys.
            
            return 0; //return zero if the key is not a letter or number
           
        }
        public class Template
        {
            public string TemplateName { get; set; }
            public string Label { get; set; }

        }
        public class VmButton
        {
            public string buttonLabel { get; set; }
            public string buttonName { get; set; }
            public ButtonLayout buttonLayout { get; set; }
            /// <summary>
            /// Read Only. Column 'number' parsed out of the buttonName
            /// </summary>
            public string Column
            {
                get
                {
                    return buttonName.Substring(buttonName.Length - 3, 1);
                }
            }
            /// <summary>
            /// Read Only. Row 'number' parsed out of the buttonName
            /// </summary>
            public string Row
            {
                get { return buttonName.Substring(buttonName.Length - 1, 1); }
            }


            public class ButtonLayout
            {
                public string IconPathAbsolute { get; set; }
            }
        }
        /// <summary>
        /// Class structure for a Virtual Menu Shortcut Key
        /// </summary>
        public class ShortcutKey
        {
            public bool AltKey { get; set; }
            public bool CtrlKey { get; set; }
            public bool ShiftKey { get; set; }
            public int vkKeyCode { get; set; }
        }


    }
}
