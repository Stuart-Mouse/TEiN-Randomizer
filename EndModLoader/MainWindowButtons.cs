using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using TEiNRandomizer.Properties;

namespace TEiNRandomizer
{
    partial class MainWindow
    {
        // Tools Buttons
        private void DoLatestThing_Click(object sender, RoutedEventArgs e)
        {
            // generate all directional connector levels from template (level pool data)
            StreamWriter sw = File.CreateText("data/level_pools/.mapgen/NewConnectors.gon");

            sw.Write("header {\nenabled true\norder 1\nauthor \"Tyler & Edmund\"\nsource \"The End is Nigh\"\npath \"data/level_pools/.mapgen/tilemaps/\"\ntype connector\n}\n");
            sw.Write("levels {\n");

            // iterate over all combinations of directions
            for (Directions dirs = Directions.None; dirs < Directions.All; dirs++)
            {
                // create level file
                LevelFile level = LevelManip.Load("data/level_pools/.mapgen/tilemaps/template.lvl");
                LevelCorruptors.CleanLevels(ref level, Collectables.None, ~dirs);
                LevelManip.Save(level, $"data/level_pools/.mapgen/tilemaps/{(int)dirs}.lvl");
                
                // write data to file
                sw.Write((int)dirs + " {\n");

                sw.Write("\tconnections {\n");
                DirectionsEnum.ActOnEach(dirs, delegate(Directions dir)
                    { sw.Write("\t\t" + DirectionsEnum.SingleToString(dir) + " EX\n"); }
                );
                sw.Write("\t}\n");

                sw.Write("}\n");
            }

            sw.Write("}\n");
            sw.Close();
        }
        private void TestGon_Click(object sender, RoutedEventArgs e)
        {
            Utility.TilesetTest();
        }
        private void TestMapGen_Click(object sender, RoutedEventArgs e)
        {
            //MapGenerator.GenerateMap();
        }
        private void WriteSettingsCodeForMe_Click(object sender, RoutedEventArgs e)
        {
            //RSettings.WriteNewSaveFunc();
        }
        private void LevelGenTestButton_Click(object sender, RoutedEventArgs e)
        {
            LevelGenerator.LoadPieces(this);
            //RNG.SeedMe(0);

            for (int i = 0; i < 40; i++)
            {
                LevelManip.Save(LevelGenerator.CreateLevel(), /*$"C:\\Program Files (x86)\\Steam\\steamapps\\common\\theendisnigh\\tilemaps/1-{i}.lvl"*/$"{RSettings.ToolsOutDirectory}/1-{i}.lvl");
            }

            MessageBox.Show($"level gen test complete", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void CreatePiecePools(object sender, RoutedEventArgs e)
        {
            foreach (var folder in Directory.GetDirectories("data/levelpieces"))
            {
                string folderName = Path.GetFileNameWithoutExtension(folder); // get folder name
                if (folderName == "GEN") continue; // ignore GEN folder

                XDocument doc = new XDocument();
                XElement pool = new XElement("pool");
                pool.SetAttributeValue("enabled", "True");
                pool.SetAttributeValue("source", folderName);
                pool.SetAttributeValue("author", "Uzerro");
                pool.SetAttributeValue("order", "0");

                foreach (var file in Directory.GetFiles(folder, "*.lvl", SearchOption.TopDirectoryOnly))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file); // get file name
                    XElement piece = new XElement("piece");
                    piece.SetAttributeValue("name", fileName);
                    // create XElement for piece
                    LevelFile level = LevelManip.Load(file);                  // load the associated level file

                    Pair enCoord = LevelGenerator.GetEntryCoord(ref level); // get entry coord
                    Pair exCoord = LevelGenerator.GetExitCoord(ref level);  // get exit coord

                    // check for ceilings and floors
                    piece.SetAttributeValue("ceilingEn", "False");
                    piece.SetAttributeValue("ceilingEx", "False");
                    piece.SetAttributeValue("floorEn", "False");
                    piece.SetAttributeValue("floorEx", "False");

                    int index, lw = level.header.width, lh = level.header.height;

                    if (level.data[LevelFile.ACTIVE, 0] == TileID.Solid)               // checks top left tile for solid block
                        piece.SetAttributeValue("ceilingEn", "True");
                    if (level.data[LevelFile.ACTIVE, lw - 1] == TileID.Solid)          // checks top right tile for solid block
                        piece.SetAttributeValue("ceilingEx", "True");

                    index = (enCoord.First + 1) * lw + enCoord.Second;
                    if (index < lh * lw)
                    {
                        if (level.data[LevelFile.ACTIVE, index] == TileID.Solid)       // checks tile underneath the entrance for solid block
                            piece.SetAttributeValue("floorEn", "True");
                    }
                    index = (exCoord.First + 1) * lw + exCoord.Second;
                    if (index < lh * lw)
                    {
                        if (level.data[LevelFile.ACTIVE, index] == TileID.Solid)       // checks tile underneath the exit for solid block
                            piece.SetAttributeValue("floorEx", "True");
                    }

                    // FullHeight, AllowBuildingAbove/Below, Margin will need to be set manually.

                    pool.Add(piece);
                }

                doc.Add(pool);
                doc.Save($"data/piecepools/{folderName}.xml");
            }

            MessageBox.Show($"creating piece pools complete", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void GenerateParticles_Click(object sender, RoutedEventArgs e)
        {
            Randomizer.SaveDir = RSettings.ToolsOutDirectory;
            for (int i = 0; i < 50; i++)
            {
                ParticleGenerator.GetParticle();
            }
            MessageBox.Show($"Successfully Generated Particles", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ColorLevels_Click(object sender, RoutedEventArgs e)
        {
            //var level1 = LevelManip.Load("C:\\Users\\Noah\\Documents\\GitHub\\TEiN-Randomizer\\EndModLoader\\bin\\Debug\\tools\\input\\_BGTILES.lvl");
            //CSV.LevelToCSV(ref level1, RSettings.ToolsOutDirectory + "level.csv");
            //CSV.RotateCSV("oldmap.csv");

            foreach (var file in Directory.GetFiles(RSettings.ToolsInDirectory, "*.lvl", SearchOption.TopDirectoryOnly))
            {
                var level = LevelManip.Load(file);
                string filename = Path.GetFileName(file);

                LevelCorruptors.ReplaceColorTiles(ref level);
                //AutoDecorator.DecorateMachine(ref level);

                string savepath = RSettings.ToolsOutDirectory + filename;
                LevelManip.Save(level, savepath);
            }

            MessageBox.Show($"Successfully De-Colored Levels", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ColorLevelsAndDecorateMachine_Click(object sender, RoutedEventArgs e)
        {
            //var level1 = LevelManip.Load("C:\\Users\\Noah\\Documents\\GitHub\\TEiN-Randomizer\\EndModLoader\\bin\\Debug\\tools\\input\\_BGTILES.lvl");
            //CSV.LevelToCSV(ref level1, RSettings.ToolsOutDirectory + "level.csv");
            //CSV.RotateCSV("oldmap.csv");

            foreach (var file in Directory.GetFiles(RSettings.ToolsInDirectory, "*.lvl", SearchOption.TopDirectoryOnly))
            {
                var level = LevelManip.Load(file);
                string filename = Path.GetFileName(file);

                LevelCorruptors.ReplaceColorTiles(ref level);
                AutoDecorator.DecorateMachine(ref level);

                string savepath = RSettings.ToolsOutDirectory + filename;
                LevelManip.Save(level, savepath);
            }

            MessageBox.Show($"Successfully De-Colored Levels", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void RotateLevels_Click(object sender, RoutedEventArgs e)
        {
            //var level1 = LevelManip.Load("C:\\Users\\Noah\\Documents\\GitHub\\TEiN-Randomizer\\EndModLoader\\bin\\Debug\\tools\\input\\_BGTILES.lvl");
            //CSV.LevelToCSV(ref level1, RSettings.ToolsOutDirectory + "level.csv");
            //CSV.RotateCSV("oldmap.csv");

            foreach (var file in Directory.GetFiles(RSettings.ToolsInDirectory, "*.lvl", SearchOption.TopDirectoryOnly))
            {
                var level = LevelManip.Load(file);
                string filename = Path.GetFileName(file);

                int loop = Convert.ToInt32(((MenuItem)sender).Tag);
                for (int i = 0; i < loop; i++)
                {
                    level = LevelManip.RotateLevel(ref level);
                }
                level = LevelManip.FixAspect(ref level);

                string savepath = RSettings.ToolsOutDirectory + filename;
                LevelManip.Save(level, savepath);
            }

            MessageBox.Show($"Successfully Rotated Levels", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void FlipLevelsH_Click(object sender, RoutedEventArgs e)
        {
            foreach (var file in Directory.GetFiles(RSettings.ToolsInDirectory, "*.lvl", SearchOption.TopDirectoryOnly))
            {
                var level = LevelManip.Load(file);
                string filename = Path.GetFileName(file);
                LevelManip.FlipLevelH(ref level);
                string savepath = RSettings.ToolsOutDirectory + filename;
                LevelManip.Save(level, savepath);
            }
            MessageBox.Show($"Successfully Flipped Levels", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void SmartCorrupt_Click(object sender, RoutedEventArgs e)
        {
            foreach (var file in Directory.GetFiles(RSettings.ToolsInDirectory, "*.lvl", SearchOption.TopDirectoryOnly))
            {
                var level = LevelManip.Load(file);
                string filename = Path.GetFileName(file);
                LevelCorruptors.SmartCorruptActive(ref level);
                LevelCorruptors.SmartCorruptOverlay(ref level);
                string savepath = RSettings.ToolsOutDirectory + filename;
                LevelManip.Save(level, savepath);
            }
            MessageBox.Show($"Successfully Corrupted Levels", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        // Main Interface Buttons
        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select The End Is Nigh Folder"
            };

            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                RSettings.GameDirectory = dialog.FileName + "\\";
                Mods.Clear();
            }

            ReadyEndIsNighPath();
        }
        private void UnloadButton_Click(object sender, RoutedEventArgs e)
        {
            AppState = AppState.NoModsFound;
        }
        private async void RandomizeButton_Click(object sender, RoutedEventArgs e)
        {
            RNG.SeedMe((int)GameSeed);
            await PlayRandomizer();
        }
        private async void PlayModButton_Click(object sender, RoutedEventArgs e)
        {
            RNG.SeedMe((int)GameSeed);
            bool arg = (sender as Button).Name.ToString() == "RandomizeModButton";
            await PlayMod(arg);
        }
        private void SeedButton_Click(object sender, RoutedEventArgs e)
        {
            GameSeed = RNG.GetUInt32();
            RNG.SeedMe((int)GameSeed);
            SeedTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            ModSeedTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        }
        private void PoolCat_Click(object sender, RoutedEventArgs e)
        {
            (sender as LevelPoolCategory).Enabled = !(sender as LevelPoolCategory).Enabled;
            //PoolCatList.GetBindingExpression(ListBox.VisibilityProperty).UpdateTarget();
        }
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            RSettings.Save();
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            //GameSeed = RNG.GetUInt32();
            GameSeed += 500;
            RNG.SeedMe((int)GameSeed);
            SeedTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            Randomizer.Randomize();
        }   // Refresh button is no longer in use.
        private void SaveModButton_Click(object sender, RoutedEventArgs e)
        {
            Randomizer.Randomize("savemod");
            LoadSavedRuns(FileSystem.ReadModFolder(SavedRunsPath).OrderBy(p => p));
            MessageBox.Show($"Mod saved successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ModList_Click(object sender, RoutedEventArgs e)
        {
            var selected = (sender as ListView).SelectedItem;
            (selected as Mod).Enabled = !(selected as Mod).Enabled;
        }
        private void SavedRunsList_Click(object sender, RoutedEventArgs e)
        {
            var selected = (sender as ListView).SelectedItem;
            foreach (var mod in SavedRuns)
            {
                mod.Enabled = false;
            }
            (selected as Mod).Enabled = true;
        }
        private void PieceList_Click(object sender, RoutedEventArgs e)
        {
            var selected = (sender as ListView).SelectedItem;
            (selected as PiecePool).Active = !(selected as PiecePool).Active;
        }
        private void OpenTilesetsOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists("data/text/AttachToTS.txt"))
                    Process.Start("notepad.exe", "data/text/AttachToTS.txt");
                else { File.Create("data/text/AttachToTS.txt"); Process.Start("data/text/AttachToTS.txt"); }
            }
            catch (Exception)
            {
                MessageBox.Show($"Error opening or creating text/AttachToTS.txt.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ReloadTilesetsOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            AttachToTS = new ObservableCollection<string>(File.ReadAllLines("data/text/AttachToTS.txt"));
            AttachToTSPreview.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget();
            //MessageBox.Show($"Tilesets options succesfully reloaded.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
