# DazStudioStreamDeckPlugins
My StreamDeck plugins for integrating with Daz Studio. 
This requires you to run the Daz Studio plug-in by esemwy.  https://github.com/esemwy/StreamDeckSocket

I'm a noob programmer.  If you have suggestions, please let me know by posting an issue on the github repo.

Thanks to esemwy for doing the heavy lifting in the Daz Studio c++ sdk. (https://github.com/esemwy)

Thanks to BarRaider for the development tools (https://github.com/BarRaider)


The Daz _Studio_ Action plug-in grabs every available SDK command possible, over 800 in all.  It includes the 'useless' ones and ones I don't even understand.  It also includes any Custom Actions you have defined.
Some commands, such as 'Viewport Drawstyle Filament', don't work.  Of course, I can't get an assigned shortcut key to work either so it may be an issue with Daz Studio.
The Daz _Custom_ Actions plug-in limits the list to only the custom actions you have defined by using "Create Custom Action..." in Daz Studio.  This can make for a more managable list
See included DazStudioActions.json for a list of Daz Studio Commands.  This can help you find the command's title

If you'd like to create custom icons that replace the default, use the following steps.  (note: Custom actions use the icon assocate with it and cannot be replaced)
  
  -Look up the 'name' in the DazStudioActions.json file.  For example, "DzHPSelectChildrenAction" for Select Children.
	
  -Save the file as "DzHPSelectChildrenAction.png"
	
  -Place the file in the Custom Icons folder of the plug in.  The default locaiton is:
		C:\Users\[username]\AppData\Roaming\Elgato\StreamDeck\Plugins\com.windamyre.daztools.sdPlugin\CustomIcons


Note: Daz Studio uses the same text for multiple actions, such as "Select Children" based on the context.  You may have to use trial-and-error to figure out which one is the right one for you.
I'm working on compiling a 'filtered' list based upon workflow and allowing others to do the same.  For example, a list that would be only items found in the Main Menu, Scene Tab menu, etc.

Download site https://github.com/Windamyre/DazStudioStreamDeckPlugins
This software is provided as-is with no warrenty expressed or implied.
GNU v3 Public license.  If you make changes you do not have to credit me but that would be nice.
Please let me know if you make a better plug-in so I can use that instead.

Thanks
