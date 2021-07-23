using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using TEiNRandomizer.Properties;

namespace TEiNRandomizer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //private void AnalysisButton_Click(object sender, RoutedEventArgs e)
        //{
        //    var LevelCounter = new List<ItemCounter> { };
        //    foreach (var cat in PoolCats)
        //    {
        //        foreach(var pool in cat.Pools)
        //        {
        //            if (pool.Active)
        //            {
        //                foreach (var level in pool.Levels)
        //                {
        //                    LevelCounter.Add(new ItemCounter() { Name = level.Name, Occurences = 0 });
        //                }
        //            }
        //        }
        //    }

        //    // 4294967295
        //    // run randomizer
        //    UInt32 reached = 0;
        //    for (UInt32 i = 0; i < 1000; i++)
        //    {
        //        //GameSeed = RNG.GetSeed();
        //        try
        //        {
        //            RNG.SeedMe((int)i);
        //            Randomizer.Randomize(this, true);
        //            Console.WriteLine(i);
        //        }
        //        catch (OutOfMemoryException)
        //        {
        //            reached = i;
        //            break;
        //        }

        //    }
        //    Console.WriteLine(reached);

        //    Console.WriteLine("Counting Levels");
        //    // count up level occurences
        //    bool found;
        //    foreach (var level in AnalysisLevelList)
        //    {
        //        found = false;
        //        for (int i = 0; i < LevelCounter.Count; i++)
        //        {
        //            if (level.Name == LevelCounter[i].Name)
        //            {
        //                LevelCounter[i].Occurences++;
        //                found = true;
        //            }
        //        }
        //        if (found == false)
        //        {
        //            LevelCounter.Add(new ItemCounter() { Name = level.Name, Occurences = 1 });
        //        }
        //    }

        //    // count up tilesets ocurrences
        //    Console.WriteLine("Counting Music"); var MusicCounter = TilesetAnalysis(AnalysisMusicList);
        //    Console.WriteLine("Counting Palettes"); var PaletteCounter = TilesetAnalysis(AnalysisPaletteList);
        //    Console.WriteLine("Counting Tile Graphics"); var TileCounter = TilesetAnalysis(AnalysisTileList);
        //    Console.WriteLine("Counting Overlayss"); var OverlayCounter = TilesetAnalysis(AnalysisOverlayList);
        //    Console.WriteLine("Counting Shaders"); var ShaderCounter = TilesetAnalysis(AnalysisShaderList);
        //    Console.WriteLine("Counting Particles"); var ParticlesCounter = TilesetAnalysis(AnalysisParticlesList);
        //    //Console.WriteLine("Counting FullSets"); var FullSetCounter = TilesetAnalysis(AnalysisFullSetList);

        //    // sort 
        //    Console.WriteLine("Sorting");
        //    LevelCounter.Sort();
        //    MusicCounter.Sort();
        //    PaletteCounter.Sort();
        //    TileCounter.Sort();
        //    OverlayCounter.Sort();
        //    ShaderCounter.Sort();
        //    ParticlesCounter.Sort();
        //    //FullSetCounter.Sort();

        //    // write results to file
        //    Console.WriteLine("Writing to file");
        //    using (StreamWriter sw = File.CreateText("analysis_output.csv"))
        //    {
        //        sw.WriteLine("Level Selction Distribution");
        //        sw.WriteLine("Spread: " + (LevelCounter.Last().Occurences - LevelCounter.FirstOrDefault().Occurences).ToString());
        //        foreach (var item in LevelCounter)
        //        {
        //            sw.WriteLine(item.Name.Trim() + ";" + item.Occurences.ToString());
        //        }

        //        sw.WriteLine("\nTile Graphics Distribution");
        //        sw.WriteLine("Spread: " + (TileCounter.Last().Occurences - TileCounter.FirstOrDefault().Occurences).ToString());
        //        foreach (var item in TileCounter)
        //        {
        //            sw.WriteLine(item.Name.Trim() + ";" + item.Occurences.ToString());
        //        }
        //        sw.WriteLine("\nOverlay Graphics Distribution");
        //        sw.WriteLine("Spread: " + (OverlayCounter.Last().Occurences - OverlayCounter.FirstOrDefault().Occurences).ToString());
        //        foreach (var item in OverlayCounter)
        //        {
        //            sw.WriteLine(item.Name.Trim() + ";" + item.Occurences.ToString());
        //        }
        //        sw.WriteLine("\nMusic Distribution");
        //        sw.WriteLine("Spread: " + (MusicCounter.Last().Occurences - MusicCounter.FirstOrDefault().Occurences).ToString());
        //        foreach (var item in MusicCounter)
        //        {
        //            sw.WriteLine(item.Name.Trim() + ";" + item.Occurences.ToString());
        //        }
        //        sw.WriteLine("\nPalette Distribution");
        //        sw.WriteLine("Spread: " + (PaletteCounter.Last().Occurences - PaletteCounter.FirstOrDefault().Occurences).ToString());
        //        foreach (var item in PaletteCounter)
        //        {
        //            sw.WriteLine(item.Name.Trim() + ";" + item.Occurences.ToString());
        //        }
        //        sw.WriteLine("\nShader Distribution");
        //        sw.WriteLine("Spread: " + (ShaderCounter.Last().Occurences - ShaderCounter.FirstOrDefault().Occurences).ToString());
        //        foreach (var item in ShaderCounter)
        //        {
        //            sw.WriteLine(item.Name + ";" + item.Occurences.ToString());
        //        }
        //        sw.WriteLine("\nParticles Distribution");
        //        sw.WriteLine("Spread: " + (ParticlesCounter.Last().Occurences - ParticlesCounter.FirstOrDefault().Occurences).ToString());
        //        foreach (var item in ParticlesCounter)
        //        {
        //            sw.WriteLine(item.Name.Trim() + ";" + item.Occurences.ToString());
        //        }
        //        //sw.WriteLine("Full Tileset Distribution");
        //        //foreach (var item in FullSetCounter)
        //        //{
        //        //    sw.WriteLine(item.Name + ";" + item.Occurences.ToString());
        //        //}

        //    }

        //    MessageBox.Show(
        //                "Analysis Complete.",
        //                "FYI",
        //                MessageBoxButton.OK,
        //                MessageBoxImage.Information,
        //                MessageBoxResult.OK
        //            );
        //}
    }
}
