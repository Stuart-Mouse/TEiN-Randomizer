﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Win32;
using System.Windows;


namespace TEiNRandomizer
{
    public static class FileSystem
    {
        public static readonly object SteamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null);
        public static readonly string[] ModFolders = { "audio", "data", "shaders", "swfs", "textures", "tilemaps" };

        private static FileSystemWatcher Watcher;
        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }

        public static IEnumerable<Mod> ReadModFolder(string path)
        {
            foreach (var dir in Directory.GetDirectories(path))
            {
                var mod = Mod.FromFolder(dir);
                if (mod != null) yield return mod;
            }
        }

        public static bool LoadModsZip(MainWindow mw) // Does not work correctly. Mods will need to be stored unzipped.
        {
            foreach(Mod mod in mw.Mods)
            {
                try  // will attempt to load all mods selected, but will return false if there is a conflict
                {
                    if (mod.Active)
                    {
                        ZipFile.ExtractToDirectory($"{mod.ModPath}", mw.RSettings.GameDirectory);
                    }
                }
                catch (System.IO.IOException ex)
                {
                    MessageBox.Show($"Error loading mod: {mod.ModPath}\n There may be a file conflict between these two mods. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return true;
        }

        public static bool LoadMods(MainWindow mw) // Does not work correctly. Mods will need to be stored unzipped.
        {
            foreach (Mod mod in mw.Mods)
            {
                try  // will attempt to load all mods selected, but will return false if there is a conflict
                {
                    if (mod.Active)
                    {
                        foreach (var folder in ModFolders)
                        {
                            string path = $"{mod.ModPath}/{folder}";
                            string dest = $"{mw.RSettings.GameDirectory}/{folder}";
                            var source = new DirectoryInfo(path);
                            var target = new DirectoryInfo(dest);
                            if (Directory.Exists(path))
                            {
                                Directory.CreateDirectory(dest);
                                CopyFilesRecursively(source, target);
                            }
                        }
                    }
                }
                catch (System.IO.IOException ex)
                {
                    MessageBox.Show($"Error loading mod: {mod.ModPath}\n There may be a file conflict between these two mods. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return true;
        }

        public static bool LoadSavedRun(MainWindow mw) // Does not work correctly. Mods will need to be stored unzipped.
        {
            Mod mod = (mw.SavedRunsList.SelectedItem as Mod);
            
            try  // will attempt to load saved run
            {
                foreach (var folder in ModFolders)
                {
                    string path = $"{mod.ModPath}/{folder}";
                    string dest = $"{mw.RSettings.GameDirectory}/{folder}";
                    var source = new DirectoryInfo(path);
                    var target = new DirectoryInfo(dest);
                    if (Directory.Exists(path))
                    {
                        Directory.CreateDirectory(dest);
                        CopyFilesRecursively(source, target);
                    }
                }
            }
            catch (System.IO.IOException ex)
            {
                MessageBox.Show($"Error loading : {mod.ModPath}\n There may be a file conflict between these two mods. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public static void EnableWatching(
            string path, 
            FileSystemEventHandler addHandler, 
            FileSystemEventHandler removeHandler,
            RenamedEventHandler renameHandler
        ) {
            Watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = "*.zip",
                NotifyFilter = NotifyFilters.LastAccess |
                               NotifyFilters.LastWrite |
                               NotifyFilters.CreationTime |
                               NotifyFilters.FileName
            };

            Watcher.Changed += addHandler;
            Watcher.Deleted += removeHandler;
            Watcher.Renamed += renameHandler;
            Watcher.EnableRaisingEvents = true;
        }

        public static void SetupDir(string path)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(path, "mods"));
                Directory.CreateDirectory(Path.Combine(path, "backup"));
            }
            catch (UnauthorizedAccessException e)
            {
                throw e;
            }
        }

        public static bool IsGamePathCorrect(string path) =>
            Directory.Exists(path) && File.Exists(Path.Combine(path, MainWindow.ExeName));

        public static void LoadMod(Mod mod, string path)
        {
            using (var zip = ZipFile.Open(mod.ModPath, ZipArchiveMode.Read))
            {
                // Only extracts directories listed in ModFolders to prevent littering the directory
                // and make it easier to delete it all afterwards.
                foreach (var entry in zip.Entries.Where(
                    e => ModFolders.Contains(Path.GetDirectoryName(e.FullName).Split('\\').First())
                    && !string.IsNullOrEmpty(e.Name)
                )) {
                    Directory.CreateDirectory(Path.Combine(path, Path.GetDirectoryName(entry.FullName)));
                    entry.ExtractToFile(Path.Combine(path, entry.FullName));
                }
            }
        }

        public static void MakeSaveBackup(string path)
        {
            // If possible, finds the users save directory and backups a save before
            // they start toying around with mods.
            var save = TryFindSaveDirectory(path);
            if (save != null)
            {
                foreach (var file in Directory.GetFiles(save))
                {
                    var backupFile = Path.Combine(path, "backup", new FileInfo(file).Name);
                    if (!File.Exists(backupFile))
                    {
                        File.Copy(file, backupFile);
                    }
                }
            }
        }

        public static void UnloadAll(string path)
        {
            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                if (ModFolders.Contains(new DirectoryInfo(dir).Name))
                {
                    Directory.Delete(dir, recursive: true);
                }
            }
        }

        public static IEnumerable<string> ContainedFolders(string path, params string[] folders)
        {
            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                if (folders.Contains(new DirectoryInfo(dir).Name))
                {
                    yield return new DirectoryInfo(dir).Name;
                }
            }
        }

        public static string DefaultGameDirectory()
        {
            if (SteamPath != null)
            {
                return Path.Combine(SteamPath as string, @"steamapps\common\theendisnigh\");
            }
            else
            {
                // Return the users Program Files folder and let them find the directory themselves.
                return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }
        }

        public static string TryFindSaveDirectory(string gamePath)
        {
            
            if (SteamPath != null)
            {
                foreach (var user in Directory.GetDirectories(Path.Combine(SteamPath as string, "userdata")))
                {
                    if (Directory.GetDirectories(user).Select(dir => new DirectoryInfo(dir).Name).Contains("583470"))
                    {
                        return Path.Combine(user, @"583470\remote");
                    }
                }
            }
            // Welp, no Steam save files, let's try something else 😼
            if (Directory.Exists(Path.Combine(gamePath, @"OfflineStorage\remote")))
            {
                return Path.Combine(gamePath, @"OfflineStorage\remote");
            }

            // No luck, no save file support.
            return null;
        }
    }
}
