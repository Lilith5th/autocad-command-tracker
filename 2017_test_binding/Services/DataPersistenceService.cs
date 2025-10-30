using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using _2017_test_binding.Models;

namespace _2017_test_binding.Services
{
    public class DataPersistenceService
    {
        private readonly string _dataDirectory;
        private readonly string _commandDataFile;
        private readonly string _settingsFile;
        private readonly string _profilesDirectory;
        private string _currentProfile = "default";

        public DataPersistenceService()
        {
            _dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoCADCommandTracker");
            _commandDataFile = Path.Combine(_dataDirectory, "command_data.json");
            _settingsFile = Path.Combine(_dataDirectory, "settings.json");
            _profilesDirectory = Path.Combine(_dataDirectory, "profiles");

            EnsureDirectoriesExist();
        }

        private void EnsureDirectoriesExist()
        {
            try
            {
                if (!Directory.Exists(_dataDirectory))
                    Directory.CreateDirectory(_dataDirectory);

                if (!Directory.Exists(_profilesDirectory))
                    Directory.CreateDirectory(_profilesDirectory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating directories: {ex.Message}");
            }
        }

        public void SaveCommandData(List<CommandData> commandData)
        {
            try
            {
                var profileFile = GetProfileFile(_currentProfile);
                var json = JsonConvert.SerializeObject(commandData, Formatting.Indented);
                File.WriteAllText(profileFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving command data: {ex.Message}");
            }
        }

        public List<CommandData> LoadCommandData()
        {
            try
            {
                var profileFile = GetProfileFile(_currentProfile);
                if (File.Exists(profileFile))
                {
                    var json = File.ReadAllText(profileFile);
                    return JsonConvert.DeserializeObject<List<CommandData>>(json) ?? new List<CommandData>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading command data: {ex.Message}");
            }

            return new List<CommandData>();
        }

        public void SaveSettings(UserSettings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(_settingsFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public UserSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFile))
                {
                    var json = File.ReadAllText(_settingsFile);
                    return JsonConvert.DeserializeObject<UserSettings>(json) ?? new UserSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }

            return new UserSettings();
        }

        public void SwitchProfile(string profileName)
        {
            _currentProfile = profileName;
        }

        public List<string> GetAvailableProfiles()
        {
            try
            {
                return Directory.GetFiles(_profilesDirectory, "*.json")
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting profiles: {ex.Message}");
                return new List<string> { "default" };
            }
        }

        private string GetProfileFile(string profileName)
        {
            return Path.Combine(_profilesDirectory, $"{profileName}.json");
        }

        public void ExportData(string filePath)
        {
            try
            {
                var data = new ExportData
                {
                    CommandData = LoadCommandData(),
                    Settings = LoadSettings(),
                    ExportDate = DateTime.Now
                };

                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting data: {ex.Message}");
                throw;
            }
        }

        public void ImportData(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeObject<ExportData>(json);

                if (data != null)
                {
                    SaveCommandData(data.CommandData);
                    SaveSettings(data.Settings);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing data: {ex.Message}");
                throw;
            }
        }
    }

    public class UserSettings
    {
        public double WindowOpacity { get; set; } = 0.6;
        public string Theme { get; set; } = "Light";
        public double WindowLeft { get; set; } = 100;
        public double WindowTop { get; set; } = 100;
        public double WindowWidth { get; set; } = 200;
        public double WindowHeight { get; set; } = 220;
        public bool IsResizable { get; set; } = true;
        public int MaxStoredValues { get; set; } = 50;
        public bool TrackTimePatterns { get; set; } = false;
        public bool TrackCommandSequences { get; set; } = true;
        public bool SynchronizeWithAutoCAD { get; set; } = true;
    }

    public class ExportData
    {
        public List<CommandData> CommandData { get; set; }
        public UserSettings Settings { get; set; }
        public DateTime ExportDate { get; set; }
    }
}