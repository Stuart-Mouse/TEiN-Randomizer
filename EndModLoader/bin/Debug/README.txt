------------------------------------------------------------------

INSTALLATION:

Simply extract the zip folder you downloaded and leave everything 
inside the "Release" folder, no need to go dumping eveything in 
your TEiN folder and get things mixed up.

------------------------------------------------------------------

PLAYING THE RANDOMIZER:

Run the TEiNRandomizer.exe

=== The Menu ===
The menu is broken up into several tabs.
The most important of these are the Level Pools tab and the Randomization tab.

Level Pools:
  These are the different packs of levels that will be used when generating your randomized world.
  Each pool can be turned on and off, so you can tailor the difficulty to your level of experience.
  On the right side is the settings panel, which I will go over in more detail below.

Settings:
  On the left side of this tab are the more basic settings. 
  Seed - This is the current seed that will be used to generate the randomized world. This will only produce the same results if the other settings are also the same. Clicking the "New Seed" button will generate a new seed. Do this between consecutive runs, because it won't give you a new one automatically.
  Levels - The number of levels per area.
  Areas - The number of areas or worlds that will be generated.
  Area Type - The value that will be assigned to the area_type variable in the generated worlds.
  One Tileset Per Area - If this is enabled, each area will use a contiguous tileset. That means using the same palette, particles, etc. This makes the area feel much more connected.
  Use Default Music - Music is applied on a per-area basis by default. Enabling this option will apply the default music to every level.
  Use Default Palettes - Enabling this option will apply the default palettes to every level.
  Randomize Shaders - If enabled, random shaders will be applied to every area or level. Checking the box will open a dropdown menu that allows you to select which shaders you want to enable/disable.
  Randomize Particles - If enabled, random particles will be applied to every area or level.
  Randomize Overlays - If enabled, random overlays will be applied to every area or level.
  Randomize Tile Graphics - If enabled, random tile graphics will be applied to every area or level.
  Randomize NPCs - This setting currently does nothing, but random NPCs are planned as an option for a future update.
  Randomize Art Alts - This setting allows for the control of how art assets are randomized. There are a few different levels of randomization.
      None - Art alt randomization is disabled.
      Safe - Only art alts that were present in the base game are possible.
      Extended - Allows for a bit more randomization of art alts, but nothing that could cause confusion or inhibit your ability to play the game. This is my recommended setting.
      Crazy - Art assets can be swapped with any object of roughly the same size.
      Insane - Every art asset can be swapped with any other art asset. Ash can become a signpost and bullets can be stevens. Play at your own risk.
  Can Apply Nevermore Tilt - If enabled, areas or levels have a random chance to use the Nevermore tilt effect.
  Can Apply Exodus Wobble - If enabled, areas or levels have a random chance to use the SS Exodus wobble effect.
  Tilesets Options - Anything you put in this text box will be applied to the tilesets.txt info for every single level. Use responsibly.
  Refresh - The refresh button in the bottom left corner will become usable once the game is running. If you want to get a new set of levels without restarting the game, you can click this button and it will do just that. Note: This will only work if level caching is disabled in the TEiN settings. You also will not get new tilesets unless you also run "refresh_particles" from the debug console.

  On the right side of the settings tab are settings that are more in depth and will only apply if certain basic settings are enabled.
  Physics Settings - Here you can enable and disable randomization of different aspects of the physics.
  Shader Settings - This is a list of all the available shader effects. They can be turned on and off individually.
  Generate Custom Particles - If enabled, completely unique, customized particles effects will be generated for each area/level.
  Max Particles - The max number of particles allowed by each particle effect.
  Max Particle Effects - The max number of different particle effects that can be applied to an area/level. Value can be from 0 to 3.

Program:
  The Program tag has a couple of settings related to the function of the program itself.
  Game Directory is the directory of your copy of The End is Nigh. It is important to set this properly if you want the game to run automatically when you press "Randomize".
  The Mod Save Directory is the folder that randomized runs will be saved to when you click the "Save Mod" button.
  Manual Load will make the randomizer not try to boot the game automatically. if this is enabled you will need to run the game yourself after pressing "Randomize"

Ready to Play?:
  Once you have your settings the way you like them, click the "Randomize" button at the bottom left.
  This should automatically boot the game. (Unless you have manual load on)
  Once in game, go to 1-1. There should be a new hole in the floor. Go down it to begin playing the randomizer.

If you get any crashes just hmu (stuart_mouse@protonmail.com)

------------------------------------------------------------------

CREDITS:

Stuart Mouse   - main developer of the randomizer
portal-chan    - created the endmodloader (which this program is built upon)
ButcherBerries - playtesting
Aurielle       - SWF modding
Hapax          - come on and finish the endonighzer already, geez

~ and ~
the rest of the TEiN modding community

------------------------------------------------------------------