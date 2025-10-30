using System;
using System.Collections.Generic;
using System.Linq;
using _2017_test_binding.Models;

namespace _2017_test_binding
{
    /// <summary>
    /// Base class containing the proven command tracking logic from the original implementation
    /// Consolidates the infrastructure instead of duplicating it
    /// </summary>
    public abstract class CommandTrackerBase
    {
        protected List<CommandData> _commandDataList;
        protected string _activeCommand;
        protected readonly object _lockObject = new object();

        /// <summary>
        /// Original proven logic from check_and_add_command
        /// Finds existing commands or creates new ones and increments counters
        /// </summary>
        protected internal void CheckAndAddCommand(string commandName)
        {
            lock (_lockObject)
            {
                if (_commandDataList == null)
                {
                    _commandDataList = new List<CommandData>();
                }

                // Use the same logic as the original check_and_add_command
                var existingCommand = _commandDataList.FirstOrDefault(c => c.CommandName == commandName);
                
                if (existingCommand == null)
                {
                    // Command not in list - add new command
                    _commandDataList.Add(new CommandData(commandName));
                }
                else
                {
                    // Command already in list - increment counter
                    existingCommand.IncrementUsage();
                }
            }
        }

        /// <summary>
        /// Get or create command without incrementing usage (for setup/initialization)
        /// </summary>
        protected internal CommandData GetOrCreateCommand(string commandName)
        {
            lock (_lockObject)
            {
                if (_commandDataList == null)
                {
                    _commandDataList = new List<CommandData>();
                }

                var command = _commandDataList.FirstOrDefault(c => c.CommandName == commandName);
                if (command == null)
                {
                    command = new CommandData(commandName);
                    _commandDataList.Add(command);
                }
                return command;
            }
        }

        /// <summary>
        /// Add default startup commands using the proven pattern
        /// </summary>
        protected internal void AddDefaultCommands(IEnumerable<string> commands)
        {
            foreach (var command in commands)
            {
                GetOrCreateCommand(command); // Create without incrementing for defaults
            }
        }
    }
}