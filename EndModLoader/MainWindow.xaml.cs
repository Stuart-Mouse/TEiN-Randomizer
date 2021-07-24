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
using TEiNRandomizer.Properties;

namespace TEiNRandomizer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static string ExeName { get => "TheEndIsNigh.exe"; }
        public static string WindowTitle { get => "  The End is Nigh Randomizer BETA  "; }
        public string ModPath { get => "mods"; }
        public string SavedRunsPath { get => "saved runs"; }
        public string PoolPath { get => "data/levelpools"; }
        public string PiecePoolPath { get => "data/piecepools"; }

        public event PropertyChangedEventHandler PropertyChanged;
        public List<Shader> ShadersList { get; set; } = Randomizer.GetShadersList();
        public UInt32 GameSeed { get; set; }    // This is the seed used to generate randomized runs.
        public RandomizerSettings RSettings { get; set; } = new RandomizerSettings("default");

        protected void NotifyPropertyChanged(string property) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        public ObservableCollection<Mod> Mods { get; private set; } = new ObservableCollection<Mod>();
        public ObservableCollection<Mod> SavedRuns { get; private set; } = new ObservableCollection<Mod>();
        public ObservableCollection<PoolCategory> PoolCats { get; private set; } = new ObservableCollection<PoolCategory>();
        public ObservableCollection<PiecePool> PiecePools { get; private set; } = new ObservableCollection<PiecePool>();
        public ObservableCollection<string> AltLevels { get; private set; } = new ObservableCollection<string>() { "None", "Safe", "Extended", "Crazy", "Insane" };
        public ObservableCollection<string> AreaTypes { get; private set; } = new ObservableCollection<string>() { "normal", "dark", "cart", "ironcart", "glitch" };
        public ObservableCollection<int> MaxParticleFXList { get; private set; } = new ObservableCollection<int>() { 1, 2, 3 };


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

        //private string _endIsNighPath;
        //public string EndIsNighPath
        //{
        //    // Amazing way to display the path properly.
        //    get => _endIsNighPath?.Replace('\\', '/') ?? "";
        //    private set
        //    {
        //        _endIsNighPath = value;
        //        RSettings.GameDirectory = value;
        //        NotifyPropertyChanged(nameof(EndIsNighPath));
        //        RSettings.Save("default");
        //    }
        //}

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            AppState = AppState.NoModSelected;

            //EndIsNighPath = RSettings.GameDirectory;
            if (string.IsNullOrWhiteSpace(RSettings.GameDirectory))
            {
                RSettings.GameDirectory = FileSystem.DefaultGameDirectory();
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
                if (FileSystem.IsGamePathCorrect(RSettings.GameDirectory))
                {
                    FileSystem.SetupDir(RSettings.GameDirectory);
                    FileSystem.MakeSaveBackup(RSettings.GameDirectory);
                    LoadModList(FileSystem.ReadModFolder(ModPath).OrderBy(m => m));
                    LoadPoolList(Randomizer.PoolLoader(PoolPath).OrderBy(p => p));
                    LoadSavedRuns(FileSystem.ReadModFolder(SavedRunsPath).OrderBy(p => p));
                    LoadPieceList(Randomizer.PieceLoader(PiecePoolPath).OrderBy(p => p));
                    Randomizer.LoadNPCs();
                    GameSeed = RNG.GetUInt32();
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
                //if (File.Exists(EndIsNighPath + "backup/TheEndIsNigh -backup.exe")) // If backup exe exists restore regular from it
                //{
                //    File.Delete(Path.Combine(EndIsNighPath + ExeName));
                //    File.Copy(Path.Combine(EndIsNighPath + "backup/TheEndIsNigh -backup.exe"), Path.Combine(EndIsNighPath + ExeName));
                //}
                //else File.Copy(Path.Combine(EndIsNighPath + ExeName), Path.Combine(EndIsNighPath + "backup/TheEndIsNigh -backup.exe")); //Creates backup exe

                // Check the game directory for mod folders
                var contains = FileSystem.ContainedFolders(RSettings.GameDirectory, FileSystem.ModFolders).ToList();
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
                            FileSystem.UnloadAll(RSettings.GameDirectory);
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

                if (!RSettings.ManualLoad)
                {
                    Process.Start(Path.Combine(RSettings.GameDirectory, ExeName));
                    await HookGameExit("TheEndIsNigh", (s, ev) =>
                    {
                        AppState = AppState.ReadyToPlay;
                        FileSystem.UnloadAll(RSettings.GameDirectory);
                    });
                }
                else await WaitForUnload();
            }
        }

        private async Task PlayMod()
        {
            if (AppState == AppState.ReadyToPlay)
            {
                //if (File.Exists(EndIsNighPath + "backup/TheEndIsNigh -backup.exe")) // If backup exe exists restore regular from it
                //{
                //    File.Delete(Path.Combine(EndIsNighPath + ExeName));
                //    File.Copy(Path.Combine(EndIsNighPath + "backup/TheEndIsNigh -backup.exe"), Path.Combine(EndIsNighPath + ExeName));
                //}
                //else File.Copy(Path.Combine(EndIsNighPath + ExeName), Path.Combine(EndIsNighPath + "backup/TheEndIsNigh -backup.exe")); //Creates backup exe

                // Check the game directory for mod folders
                var contains = FileSystem.ContainedFolders(RSettings.GameDirectory, FileSystem.ModFolders).ToList();
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
                            FileSystem.UnloadAll(RSettings.GameDirectory);
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

                if((ModLoaderTabs.SelectedItem as TabItem).Name == "ModsTab")
                {
                    AppState = AppState.InGame;
                    if (FileSystem.LoadMods(this))
                    {
                        Randomizer.RandomizeMod(this);
                        if (!RSettings.ManualLoad)
                        {
                            Process.Start(Path.Combine(RSettings.GameDirectory, ExeName));
                            await HookGameExit("TheEndIsNigh", (s, ev) =>
                            {
                                AppState = AppState.ReadyToPlay;
                                FileSystem.UnloadAll(RSettings.GameDirectory);
                            });
                        }
                        else await WaitForUnload();
                    }
                    else    // if the mods fail to load, try to delete all of the files copied into the game folder
                    {
                        try
                        {
                            FileSystem.UnloadAll(RSettings.GameDirectory);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show(
                                "Could not delete the modified folders because one or more of them are open in an another process.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );
                        }
                        AppState = AppState.ReadyToPlay;
                    }
                }

                if ((ModLoaderTabs.SelectedItem as TabItem).Name == "SavedRunsTab")
                {
                    AppState = AppState.InGame;
                    if (FileSystem.LoadSavedRun(this))
                    {
                        if (!RSettings.ManualLoad)
                        {
                            Process.Start(Path.Combine(RSettings.GameDirectory, ExeName));
                            await HookGameExit("TheEndIsNigh", (s, ev) =>
                            {
                                AppState = AppState.ReadyToPlay;
                                FileSystem.UnloadAll(RSettings.GameDirectory);
                            });
                        }
                        else await WaitForUnload();
                    }
                    else    // if the mods fail to load, try to delete all of the files copied into the game folder
                    {
                        try
                        {
                            FileSystem.UnloadAll(RSettings.GameDirectory);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show(
                                "Could not delete the modified folders because one or more of them are open in an another process.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );
                        }
                        AppState = AppState.ReadyToPlay;

                    }
                }




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
                RNG.SeedMe((int)GameSeed);
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
            FileSystem.UnloadAll(RSettings.GameDirectory);
            AppState = AppState.ReadyToPlay;
        }

        private void UnloadButton_Click(object sender, RoutedEventArgs e)
        {
            AppState = AppState.NoModsFound;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            //ParticleRanger(); // this function is stupid and I should get rid of it later
            RNG.SeedMe((int)GameSeed);
            await PlayRandomizer();
        }
        private async void PlayButton2_Click(object sender, RoutedEventArgs e)
        {
            //ParticleRanger(); // this function is stupid and I should get rid of it later
            RNG.SeedMe((int)GameSeed);
            await PlayMod();
        }

        private void SeedButton_Click(object sender, RoutedEventArgs e)
        {
            GameSeed = RNG.GetUInt32();
            RNG.SeedMe((int)GameSeed);
            SeedTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        }

        private void PoolCat_Click(object sender, RoutedEventArgs e)
        {
            (sender as PoolCategory).Enabled = !(sender as PoolCategory).Enabled;
            //PoolCatList.GetBindingExpression(ListBox.VisibilityProperty).UpdateTarget();
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            //ParticleRanger();
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

            if (PoolCats.Count == 0) AppState = AppState.NoModsFound;
            //else if (ModList.SelectedIndex == -1) AppState = AppState.NoModSelected;
            else AppState = AppState.ReadyToPlay;
        }
        private void LoadPieceList(IOrderedEnumerable<PiecePool> pools)
        {
            PiecePools.Clear();
            foreach (var pool in pools.OrderBy(m => m))
            {
                PiecePools.Add(pool);
            }
        }
        private void LoadModList(IOrderedEnumerable<Mod> mods)
        {
            Mods.Clear();
            foreach (var mod in mods.OrderBy(m => m))
            {
                Mods.Add(mod);
            }

            //if (Pools.Count == 0) AppState = AppState.NoModsFound;
            //else if (ModList.SelectedIndex == -1) AppState = AppState.NoModSelected;
            //AppState = AppState.ReadyToPlay;
        }
        private void LoadSavedRuns(IOrderedEnumerable<Mod> mods)
        {
            SavedRuns.Clear();
            foreach (var mod in mods.OrderBy(m => m))
            {
                SavedRuns.Add(mod);
            }

            //if (Pools.Count == 0) AppState = AppState.NoModsFound;
            //else if (ModList.SelectedIndex == -1) AppState = AppState.NoModSelected;
            //AppState = AppState.ReadyToPlay;
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
                RSettings.GameDirectory = dialog.FileName + "/";
                Mods.Clear();
            }

            ReadyEndIsNighPath();
        }
        
        //private void ParticleRanger() // this function is stupid and I should get rid of it later
        //{
        //    if (RSettings.MaxParticleEffects < 0) RSettings.MaxParticleEffects = 0;
        //    else if (RSettings.MaxParticleEffects > 3) RSettings.MaxParticleEffects = 3;
        //    MaxParticleEffectsTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        //}

    }
}
