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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static string ExeName { get => "TheEndIsNigh.exe"; }
        public static string WindowTitle { get => "The End is Nigh Randomizer BETA"; }

        private string ModPath { get => Path.Combine(EndIsNighPath, "mods"); }
        private string PoolPath { get => "data/levelpools"; }

        public event PropertyChangedEventHandler PropertyChanged;

        public List<Shader> ShadersList { get; set; } = Randomizer.GetShadersList();

        public UInt32 GameSeed { get; set; }
        public int PrevRuns { get; set; }
        //public List<Level> AnalysisLevelList = new List<Level> { };
        //public List<string> AnalysisPaletteList = new List<string> { };
        //public List<string> AnalysisMusicList = new List<string> { };
        //public List<string> AnalysisTileList = new List<string> { };
        //public List<string> AnalysisOverlayList = new List<string> { };
        //public List<string> AnalysisParticlesList = new List<string> { };
        //public List<string> AnalysisShaderList = new List<string> { };
        //public List<string> AnalysisFullSetList = new List<string> { };
        //public UInt32 ProgressCounter;
        //public string ProgressMessage;
        public bool ShowAdvancedSettings { get; set; } = false;

        public RandomizerSettings RSettings { get; set; } = new RandomizerSettings();

        protected void NotifyPropertyChanged(string property) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        public ObservableCollection<Mod> Mods { get; private set; } = new ObservableCollection<Mod>();
        //public ObservableCollection<Pool> Pools { get; private set; } = new ObservableCollection<Pool>();
        public ObservableCollection<PoolCategory> PoolCats { get; private set; } = new ObservableCollection<PoolCategory>();

        private AppState _appState;
        public AppState AppState
        {
            get => _appState;
            private set
            {
                _appState = value;
                NotifyPropertyChanged(nameof(AppState));
            }
        }

        private string _endIsNighPath;
        public string EndIsNighPath
        {
            // Amazing way to display the path properly.
            get => _endIsNighPath?.Replace('\\', '/') ?? "";
            private set
            {
                _endIsNighPath = value;
                RSettings.GameDirectory = value;
                NotifyPropertyChanged(nameof(EndIsNighPath));
                RSettings.Save("default");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            AppState = AppState.NoModSelected;

            EndIsNighPath = RSettings.GameDirectory;
            if (string.IsNullOrWhiteSpace(EndIsNighPath))
            {
                EndIsNighPath = FileSystem.DefaultGameDirectory();
            }
            ReadyEndIsNighPath();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.Save();

            if (AppState == AppState.InGame)
            {
                var result = MessageBox.Show(
                    "It's recommended to quit the game before closing the MOD loader. Are you sure you want to quit anyways?",
                    "Warning",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No
                );

                e.Cancel = result != MessageBoxResult.Yes;
            }
        }

        private void ReadyEndIsNighPath()
        {
            try
            {
                if (FileSystem.IsGamePathCorrect(EndIsNighPath))
                {
                    FileSystem.SetupDir(EndIsNighPath);
                    FileSystem.MakeSaveBackup(EndIsNighPath);
                    //LoadModList(FileSystem.ReadModFolder(ModPath).OrderBy(m => m));
                    LoadPoolList(Randomizer.PoolLoader(PoolPath).OrderBy(p => p));
                    GameSeed = Randomizer.myRNG.GetUInt32();
                    FileSystem.EnableWatching(ModPath, OnAdd, OnRemove, OnRename);
                }
                else
                {
                    AppState = AppState.IncorrectPath;
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    "Could not create/open mods directory. Try running the program as Administrator.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Environment.Exit(1);
            }
        }

        private void OnAdd(object sender, FileSystemEventArgs e)
        {
            var added = Mod.FromZip(e.FullPath);
            if (added != null)
            {
                Dispatcher.Invoke(() =>
                {
                    // Due to ObservableCollection not firing a notify event when it's sorted,
                    // it's simpler to insert the new mod at it's sorted index.
                    int i = 0;
                    while (i < Mods.Count && Mods[i].CompareTo(added) < 0) ++i;
                    Mods.Insert(i, added);
                });
            }
        }

        private void OnRemove(object sender, FileSystemEventArgs e)
        {
            var find = Mods.Where(m => m.ModPath == e.FullPath).FirstOrDefault();
            if (find != null)
            {
                Dispatcher.Invoke(() => Mods.Remove(find));
            }
        }

        private void OnRename(object sender, RenamedEventArgs e)
        {
            var find = Mods.Where(m => m.ModPath == e.OldFullPath).FirstOrDefault();
            var renamed = Mod.FromZip(e.FullPath);

            if (find != null && renamed != null)
            {
                Dispatcher.Invoke(() =>
                {
                    Mods.Remove(find);

                    // Same.
                    int i = 0;
                    while (i < Mods.Count && Mods[i].CompareTo(renamed) < 0) ++i;
                    Mods.Insert(i, renamed);
                });
            }
        }

        private async Task PlayRandomizer()
        {
            if (AppState == AppState.ReadyToPlay)
            {
                if (File.Exists(EndIsNighPath + "backup/TheEndIsNigh -backup.exe")) // If backup exe exists restore regular from it
                {
                    File.Delete(Path.Combine(EndIsNighPath + ExeName));
                    File.Copy(Path.Combine(EndIsNighPath + "backup/TheEndIsNigh -backup.exe"), Path.Combine(EndIsNighPath + ExeName));
                }
                else File.Copy(Path.Combine(EndIsNighPath + ExeName), Path.Combine(EndIsNighPath + "backup/TheEndIsNigh -backup.exe")); //Creates backup exe

                var contains = FileSystem.ContainedFolders(EndIsNighPath, FileSystem.ModFolders).ToList();
                if (contains.Count != 0)
                {
                    // FINALLY an excuse to use tuples!
                    var (isOrAre, a, folderOrFolders, itOrThem) = contains.Count == 1 ?
                        ("is", "a ", "folder", "it") :
                        ("are", "", "folders", "them");

                    var result = MessageBox.Show(
                        $"There {isOrAre} currently {a}modified {String.Join(", ", contains.Select(f => $"\"{f}\""))} {folderOrFolders} present in your game directory. " +
                        $"Delete {itOrThem} to play the Randomizer?",
                        "Warning",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning,
                        MessageBoxResult.No
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            FileSystem.UnloadAll(EndIsNighPath);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show(
                                "Could not delete the modified folders because one or more of them are open in an another process.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );
                            return;
                        }
                    }
                    else
                    {
                        // Let's not delete any more Cities of Tethys.
                        return;
                    }
                }

                AppState = AppState.InGame;
                Randomizer.Randomize(this);

                if (RSettings.AutoRefresh)
                    AutoRefresh();

                if (!RSettings.ManualLoad)
                {
                    Process.Start(Path.Combine(EndIsNighPath, ExeName));

                    await HookGameExit("TheEndIsNigh", (s, ev) =>
                    {
                        AppState = AppState.ReadyToPlay;
                        FileSystem.UnloadAll(EndIsNighPath);
                    });
                }
                else await WaitForUnload();
            }
        }

        private async Task HookGameExit(string process, EventHandler hook)
        {
            // Since Steam's "launching..." exits and starts the games process,
            // we can't simply hook the Process.Start return value and instead
            // have to wait 5 seconds hoping the real process launches and hook
            // that instead.
            await Task.Delay(7000);
            var end = Process.GetProcessesByName(process).First();
            end.EnableRaisingEvents = true;
            end.Exited += hook;
        }
        private async Task AutoRefresh()
        {
            while (AppState == AppState.InGame)
            {
                GameSeed++;
                Randomizer.myRNG.SeedMe((int)GameSeed);
                SeedTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                Randomizer.Randomize(this);
                await Task.Delay(7000);
            }
        }
        private async Task WaitForUnload()
        {
            while (AppState == AppState.InGame)
            {
                await Task.Delay(1000);
            }
            FileSystem.UnloadAll(EndIsNighPath);
            AppState = AppState.ReadyToPlay;
        }

        private void UnloadButton_Click(object sender, RoutedEventArgs e)
        {
            AppState = AppState.NoModsFound;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            ParticleRanger(); // this function is stupid and I should get rid of it later
            Randomizer.myRNG.SeedMe((int)GameSeed);
            await PlayRandomizer();
        }

        private void SeedButton_Click(object sender, RoutedEventArgs e)
        {
            GameSeed = Randomizer.myRNG.GetUInt32();
            Randomizer.myRNG.SeedMe((int)GameSeed);
            SeedTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        }

        private void PoolCat_Click(object sender, RoutedEventArgs e)
        {
            (sender as PoolCategory).Enabled = !(sender as PoolCategory).Enabled;
            //PoolCatList.GetBindingExpression(ListBox.VisibilityProperty).UpdateTarget();
        }

        private void AltLevelInc(object sender, RoutedEventArgs e)
        {
            if (RSettings.AltLevel < AltLevels.Insane)
                RSettings.AltLevel++;
            AltLevelTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        }
        private void AltLevelDec(object sender, RoutedEventArgs e)
        {
            if (RSettings.AltLevel > AltLevels.None)
                RSettings.AltLevel--;
            AltLevelTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        }

        private void AreaTypeInc(object sender, RoutedEventArgs e)
        {
            if (RSettings.AreaType < AreaTypes.glitch)
                RSettings.AreaType++;
            AreaTypeTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        }
        private void AreaTypeDec(object sender, RoutedEventArgs e)
        {
            if (RSettings.AreaType > AreaTypes.normal)
                RSettings.AreaType--;
            AreaTypeTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        }


        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            ParticleRanger();
            RSettings.Save("default");
            Randomizer.SaveShadersList(ShadersList);
            foreach (PoolCategory cat in PoolCatList.Items)
            foreach (Pool pool in cat.Pools)
            {
                pool.Save();
            }
            MessageBox.Show(
                        "Save successful.",
                        "FYI",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information,
                        MessageBoxResult.OK
                    );
        }

        private void NoClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        private void ClearCache(object sender, RoutedEventArgs e)
        {
            if (File.Exists("cache.xml"))
                File.Delete("cache.xml");
            MessageBox.Show(
                        "Cache was cleared.",
                        "FYI",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information,
                        MessageBoxResult.OK
                    );
        }

        private void LoadPoolList(IOrderedEnumerable<PoolCategory> cats)
        {
            PoolCats.Clear();
            foreach (var cat in cats.OrderBy(m => m))
            {
                PoolCats.Add(cat);
            }

            //if (Pools.Count == 0) AppState = AppState.NoModsFound;
            //else if (ModList.SelectedIndex == -1) AppState = AppState.NoModSelected;
            AppState = AppState.ReadyToPlay;
        }

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
                EndIsNighPath = dialog.FileName + "/";
                Mods.Clear();
            }

            ReadyEndIsNighPath();
        }

        //public class ItemCounter : IComparable<ItemCounter>
        //{
        //    public string Name;
        //    public int Occurences;

        //    public int CompareTo(ItemCounter other) => Occurences.CompareTo(other.Occurences);
        //}

        //public List<ItemCounter> TilesetAnalysis(List<string> TSList)
        //{
        //    var TSCounter = new List<ItemCounter> { };
        //    bool found;
        //    foreach (var item in TSList)
        //    {
        //        found = false;
        //        for (int i = 0; i < TSCounter.Count; i++)
        //        {
        //            if (item == TSCounter[i].Name)
        //            {
        //                TSCounter[i].Occurences++;
        //                found = true;
        //            }
        //        }
        //        if (found == false)
        //        {
        //            TSCounter.Add(new ItemCounter() { Name = item, Occurences = 1 });
        //        }
        //    }
        //    return TSCounter;
        //}
        
        private void ParticleRanger() // this function is stupid and I should get rid of it later
        {
            if (RSettings.MaxParticleEffects < 0) RSettings.MaxParticleEffects = 0;
            else if (RSettings.MaxParticleEffects > 3) RSettings.MaxParticleEffects = 3;
            MaxParticleEffectsTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        }

        //private void AdvancedSettings_Click(object sender, RoutedEventArgs e)
        //{
        //    ShowAdvancedSettings = !ShowAdvancedSettings;
        //    AdvancedSettingsList.GetBindingExpression(ListBox.VisibilityProperty).UpdateTarget();
        //}

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            //GameSeed = Randomizer.myRNG.GetUInt32();
            PrevRuns++;
            GameSeed += 500;
            Randomizer.myRNG.SeedMe((int)GameSeed);
            SeedTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            Randomizer.Randomize(this);
        }
    }
}
