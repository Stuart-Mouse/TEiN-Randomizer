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
        private void WriteSettingsCodeForMe_Click(object sender, RoutedEventArgs e)
        {
            RSettings.WriteNewSaveFunc();
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            //GameSeed = RNG.GetUInt32();
            GameSeed += 500;
            RNG.SeedMe((int)GameSeed);
            SeedTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            Randomizer.Randomize(this);
        }   // Refresh button is no longer in use.
        private void SaveModButton_Click(object sender, RoutedEventArgs e)
        {
            Randomizer.Randomize(this, "savemod");
            LoadSavedRuns(FileSystem.ReadModFolder(SavedRunsPath).OrderBy(p => p));
            MessageBox.Show($"Mod saved successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ModList_Click(object sender, RoutedEventArgs e)
        {
            var selected = (sender as ListView).SelectedItem;
            (selected as Mod).Active = !(selected as Mod).Active;
        }
        private void OpenTilesetsOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists("data/AttachToTS.txt"))
                    Process.Start("notepad.exe", "data/AttachToTS.txt");
                else { File.Create("data/AttachToTS.txt"); Process.Start("data/AttachToTS.txt"); }
            }
            catch (Exception)
            {
                MessageBox.Show($"Error opening or creating AttachToTS.txt.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ReloadTilesetsOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            RSettings.AttachToTS = File.ReadAllText("data/AttachToTS.txt");
            MessageBox.Show($"Tilesets options succesfully reloaded.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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

                    if (level.data.active[0] == TileID.Solid)               // checks top left tile for solid block
                        piece.SetAttributeValue("ceilingEn", "True");
                    if (level.data.active[lw - 1] == TileID.Solid)          // checks top right tile for solid block
                        piece.SetAttributeValue("ceilingEx", "True");

                    index = (enCoord.First + 1) * lw + enCoord.Second;
                    if (index < lh * lw)
                    {
                        if (level.data.active[index] == TileID.Solid)       // checks tile underneath the entrance for solid block
                            piece.SetAttributeValue("floorEn", "True");
                    }
                    index = (exCoord.First + 1) * lw + exCoord.Second;
                    if (index < lh * lw)
                    {
                        if (level.data.active[index] == TileID.Solid)       // checks tile underneath the exit for solid block
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
            Randomizer.saveDir = RSettings.ToolsOutDirectory;
            for (int i = 0; i < 50; i++)
            {
                ParticleGenerator.GetParticle(RSettings);
            }
            MessageBox.Show($"Successfully Generated Particles", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
                Corruptors.SmartCorruptActive(ref level);
                Corruptors.SmartCorruptOverlay(ref level);
                string savepath = RSettings.ToolsOutDirectory + filename;
                LevelManip.Save(level, savepath);
            }
            MessageBox.Show($"Successfully Corrupted Levels", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
