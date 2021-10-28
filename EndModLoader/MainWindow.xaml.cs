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
using TEiNRandomizer.Properties;

namespace TEiNRandomizer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // string values used by the Main Window
        public static string ExeName { get => "TheEndIsNigh.exe"; }
        public static string WindowTitle { get => "  The End is Nigh Randomizer BETA  "; }
        public string ModPath { get => AppResources.ModPath; }
        public string SavedRunsPath { get => AppResources.SavedRunsPath; }
        
        // shader list and settings list are references to those stored in the AppResources
        public List<Shader> ShadersList { get; set; } = AppResources.ShadersList;
        public SettingsFile RSettings { get; set; } = AppResources.MainSettings;
        // This is the seed used to generate randomized runs. Acts as a reference to the gameseed stored in AppResources
        public UInt32 GameSeed
        {
            get => AppResources.GameSeed;
            set
            {
                AppResources.GameSeed = value;
            }
        }

        // AttachToTS is copied from AppResources
        public ObservableCollection<string> AttachToTS { get; set; } = AppResources.AttachToTS;

        // Main observable collections used for lists
        public ObservableCollection<Mod> Mods { get; private set; } = new ObservableCollection<Mod>();
        public ObservableCollection<Mod> SavedRuns { get; private set; } = new ObservableCollection<Mod>();
        public ObservableCollection<LevelPoolCategory> PoolCats { get; private set; } = new ObservableCollection<LevelPoolCategory>();
        public ObservableCollection<PiecePool> PiecePools { get; private set; } = new ObservableCollection<PiecePool>();

        // these collections are basically just used as enums for the possible values for certain settings (dropdown boxes bind to them)
        public ObservableCollection<string> AltLevels { get; private set; } = new ObservableCollection<string>() { "None", "Safe", "Extended", "Crazy", "Insane" };
        public ObservableCollection<string> AreaTypes { get; private set; } = new ObservableCollection<string>() { "normal", "dark", "cart", "ironcart", "glitch" };
        public ObservableCollection<int> MaxParticleFXList { get; private set; } = new ObservableCollection<int>() { 1, 2, 3 };

        // Incorporate property change events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string property) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        private TabItem _modLoaderTabSelected;
        public TabItem ModLoaderTabSelected
        {
            get => _modLoaderTabSelected;
            set
            {
                _modLoaderTabSelected = value;
                NotifyPropertyChanged(nameof(ModLoaderTabSelected));
            }
        }

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
            AppState = AppState.InMenus;

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
                    LoadPoolList(Randomizer.LevelPoolCategories.OrderBy(p => p));
                    LoadSavedRuns(FileSystem.ReadModFolder(SavedRunsPath).OrderBy(p => p));
                    LoadPieceList(Randomizer.PiecePools.OrderBy(p => p));
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

        // FileSystemWatcher Stuff, mostly unused now
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

        // Async tasks, used while the game is running
        private async Task PlayRandomizer()
        {
            if (AppState == AppState.InMenus)
            {
                //if (File.Exists(EndIsNighPath + "backup/TheEndIsNigh -backup.exe")) // If backup exe exists restore regular from it
                //{
                //    File.Delete(Path.Combine(EndIsNighPath + ExeName));
                //    File.Copy(Path.Combine(EndIsNighPath + "backup/TheEndIsNigh -backup.exe"), Path.Combine(EndIsNighPath + ExeName));
                //}
                //else File.Copy(Path.Combine(EndIsNighPath + ExeName), Path.Combine(EndIsNighPath + "backup/TheEndIsNigh -backup.exe")); //Creates backup exe

                // Check the game directory for mod folders
                if (!CheckForModFolders()) return;

                AppState = AppState.InGame;
                Randomizer.Randomize();

                if (!RSettings.ManualLoad)
                {
                    Process.Start(Path.Combine(RSettings.GameDirectory, ExeName));
                    await HookGameExit("TheEndIsNigh", (s, ev) =>
                    {
                        AppState = AppState.InMenus;
                        FileSystem.UnloadAll(RSettings.GameDirectory);
                    });
                }
                else await WaitForUnload();
            }
        }
        private async Task PlayMod(bool randomize)
        {
            if (AppState == AppState.InMenus)
            {
                if (!CheckForModFolders()) return;

                if((ModLoaderTabs.SelectedItem as TabItem).Name == "ModsTab")
                {
                    AppState = AppState.InGame;
                    if (FileSystem.LoadMods(Mods, RSettings.GameDirectory))
                    {
                        if (randomize) Randomizer.RandomizeMod(this);
                        if (!RSettings.ManualLoad)
                        {
                            Process.Start(Path.Combine(RSettings.GameDirectory, ExeName));
                            await HookGameExit("TheEndIsNigh", (s, ev) =>
                            {
                                AppState = AppState.InMenus;
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
                        AppState = AppState.InMenus;
                    }
                }

                if ((ModLoaderTabs.SelectedItem as TabItem).Name == "SavedRunsTab")
                {
                    AppState = AppState.InGame;
                    if (FileSystem.LoadSavedRun(SavedRunsList.SelectedItem as Mod, RSettings.GameDirectory))
                    {
                        if (!RSettings.ManualLoad)
                        {
                            Process.Start(Path.Combine(RSettings.GameDirectory, ExeName));
                            await HookGameExit("TheEndIsNigh", (s, ev) =>
                            {
                                AppState = AppState.InMenus;
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
                        AppState = AppState.InMenus;

                    }
                }
            }
        }
        private bool CheckForModFolders()
        {
            // Check the game directory for mod folders
            var contains = FileSystem.ContainedFolders(RSettings.GameDirectory, FileSystem.ModFolders).ToList();
            if (contains.Count != 0)
            {
                /*// FINALLY an excuse to use tuples!
                var (isOrAre, a, folderOrFolders, itOrThem) = contains.Count == 1 ?
                    ("is", "a ", "folder", "it") :
                    ("are", "", "folders", "them");*/

                var result = MessageBox.Show(
                    $"There are currently modified {String.Join(", ", contains.Select(f => $"\"{f}\""))} folders present in your game directory. " +
                    $"Delete them to play the Randomizer?",
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
                        return false;
                    }
                }
                else
                {
                    // Let's not delete any more Cities of Tethys.
                    return false;
                }
            }
            return true;
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
        private async Task WaitForUnload()
        {
            while (AppState == AppState.InGame)
            {
                await Task.Delay(1000);
            }
            FileSystem.UnloadAll(RSettings.GameDirectory);
            AppState = AppState.InMenus;
        }
        
        // Resource loading functions
        private void LoadPoolList(IOrderedEnumerable<LevelPoolCategory> cats)
        {
            PoolCats.Clear();
            foreach (var cat in cats.OrderBy(m => m))
            {
                PoolCats.Add(cat);
            }
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
        }
        private void LoadSavedRuns(IOrderedEnumerable<Mod> mods)
        {
            SavedRuns.Clear();
            foreach (var mod in mods.OrderBy(m => m))
            {
                SavedRuns.Add(mod);
            }
        }
        
    }
}
