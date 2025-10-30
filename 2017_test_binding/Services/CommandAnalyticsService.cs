using System;
using System.Collections.Generic;
using System.Linq;
using _2017_test_binding.Models;

namespace _2017_test_binding.Services
{
    public class CommandAnalyticsService
    {
        private readonly List<CommandData> _commandHistory;
        private CommandData _previousCommand;
        private readonly Dictionary<int, List<CommandData>> _timeBasedPatterns;

        public CommandAnalyticsService()
        {
            _commandHistory = new List<CommandData>();
            _timeBasedPatterns = new Dictionary<int, List<CommandData>>();
        }

        public void TrackCommand(string commandName, List<CommandData> allCommands)
        {
            var command = allCommands.FirstOrDefault(c => c.CommandName == commandName);
            
            if (command != null)
            {
                // Track command sequence
                if (_previousCommand != null)
                {
                    _previousCommand.AddFollowedCommand(commandName);
                }

                // Track time-based patterns
                var hour = DateTime.Now.Hour;
                if (!_timeBasedPatterns.ContainsKey(hour))
                {
                    _timeBasedPatterns[hour] = new List<CommandData>();
                }
                _timeBasedPatterns[hour].Add(command);

                _previousCommand = command;
                _commandHistory.Add(command);
                
                // Limit history size
                if (_commandHistory.Count > 1000)
                {
                    _commandHistory.RemoveAt(0);
                }
            }
        }

        public List<string> GetPredictedCommands(string currentCommand, DateTime currentTime)
        {
            var predictions = new List<string>();

            // Get commands that usually follow the current command
            var currentCmd = _commandHistory.LastOrDefault(c => c.CommandName == currentCommand);
            if (currentCmd != null && currentCmd.FollowedByCommands.Any())
            {
                predictions.AddRange(currentCmd.FollowedByCommands.Take(3));
            }

            // Get time-based predictions
            var hour = currentTime.Hour;
            if (_timeBasedPatterns.ContainsKey(hour))
            {
                var timeBasedCommands = _timeBasedPatterns[hour]
                    .GroupBy(c => c.CommandName)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => g.Key);
                
                predictions.AddRange(timeBasedCommands);
            }

            return predictions.Distinct().Take(5).ToList();
        }

        public CommandStatistics GetStatistics(List<CommandData> commands)
        {
            if (!commands.Any())
                return new CommandStatistics();

            return new CommandStatistics
            {
                TotalCommands = commands.Sum(c => c.UsageCount),
                UniqueCommands = commands.Count,
                MostUsedCommand = commands.OrderByDescending(c => c.UsageCount).FirstOrDefault()?.CommandName,
                AverageCommandsPerHour = CalculateAverageCommandsPerHour(),
                PeakUsageHour = GetPeakUsageHour(),
                CommandSequences = GetTopCommandSequences()
            };
        }

        private double CalculateAverageCommandsPerHour()
        {
            if (!_commandHistory.Any())
                return 0;

            var timeSpan = DateTime.Now - _commandHistory.First().LastUsed;
            return _commandHistory.Count / Math.Max(timeSpan.TotalHours, 1);
        }

        private int GetPeakUsageHour()
        {
            return _timeBasedPatterns
                .OrderByDescending(kvp => kvp.Value.Count)
                .Select(kvp => kvp.Key)
                .FirstOrDefault();
        }

        private List<CommandSequence> GetTopCommandSequences()
        {
            var sequences = new Dictionary<string, int>();
            
            for (int i = 0; i < _commandHistory.Count - 1; i++)
            {
                var sequence = $"{_commandHistory[i].CommandName} → {_commandHistory[i + 1].CommandName}";
                if (sequences.ContainsKey(sequence))
                    sequences[sequence]++;
                else
                    sequences[sequence] = 1;
            }

            return sequences
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp => new CommandSequence { Sequence = kvp.Key, Count = kvp.Value })
                .ToList();
        }
    }

    public class CommandStatistics
    {
        public int TotalCommands { get; set; }
        public int UniqueCommands { get; set; }
        public string MostUsedCommand { get; set; }
        public double AverageCommandsPerHour { get; set; }
        public int PeakUsageHour { get; set; }
        public List<CommandSequence> CommandSequences { get; set; }
    }

    public class CommandSequence
    {
        public string Sequence { get; set; }
        public int Count { get; set; }
    }
}