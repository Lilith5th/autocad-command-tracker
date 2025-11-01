using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCadCommandTracker.Services
{
    public class CommandAliasService
    {
        private static readonly Dictionary<string, string> _commandAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Drawing Commands
            ["PLINE"] = "PL",
            ["POLYLINE"] = "PL2",
            ["LINE"] = "L",
            ["CIRCLE"] = "C",
            ["ARC"] = "A",
            ["ELLIPSE"] = "EL",
            ["RECTANGLE"] = "REC",
            ["POLYGON"] = "POL",
            ["POINT"] = "PO",
            ["SPLINE"] = "SPL",
            ["HATCH"] = "H",
            ["GRADIENT"] = "GD",
            ["REGION"] = "REG",
            ["BOUNDARY"] = "BO",
            ["WIPEOUT"] = "WI",
            
            // Modify Commands
            ["OFFSET"] = "O",
            ["TRIM"] = "TR",
            ["EXTEND"] = "EX",
            ["FILLET"] = "F",
            ["CHAMFER"] = "CHA",
            ["SCALE"] = "SC",
            ["ROTATE"] = "RO",
            ["MOVE"] = "M",
            ["COPY"] = "CP",
            ["MIRROR"] = "MI",
            ["ARRAY"] = "AR",
            ["STRETCH"] = "S",
            ["BREAK"] = "BR",
            ["JOIN"] = "J",
            ["ERASE"] = "E",
            ["EXPLODE"] = "X",
            ["PEDIT"] = "PE",
            ["DIVIDE"] = "DIV",
            ["MEASURE"] = "ME",
            
            // Annotation Commands
            ["TEXT"] = "T",
            ["MTEXT"] = "MT",
            ["LEADER"] = "LE",
            ["QLEADER"] = "QLE",
            ["MLEADER"] = "MLD",
            ["DIMENSION"] = "DIM",
            ["DIMLINEAR"] = "DLI",
            ["DIMANGULAR"] = "DAN",
            ["DIMRADIUS"] = "DRA",
            ["DIMDIAMETER"] = "DDI",
            ["DIMCONTINUE"] = "DCO",
            ["DIMBASELINE"] = "DBA",
            ["DIMEDIT"] = "DED",
            ["DIMTEDIT"] = "DIMTED",
            ["TABLE"] = "TB",
            
            // Layer Commands
            ["LAYER"] = "LA",
            ["LAYCUR"] = "LAYCUR",
            ["LAYISO"] = "LAYISO",
            ["LAYUNISO"] = "LAYUNISO",
            ["LAYOFF"] = "LAYOFF",
            ["LAYON"] = "LAYON",
            ["LAYFRZ"] = "LAYFRZ",
            ["LAYTHW"] = "LAYTHW",
            ["LAYLCK"] = "LAYLCK",
            ["LAYULK"] = "LAYULK",
            ["LAYMCH"] = "LAYMCH",
            ["LAYDEL"] = "LAYDEL",
            
            // View Commands
            ["ZOOM"] = "Z",
            ["PAN"] = "P",
            ["REGEN"] = "RE",
            ["REGENALL"] = "REA",
            ["REDRAW"] = "R",
            ["REDRAWALL"] = "RA",
            ["VIEW"] = "V",
            ["VPOINT"] = "VP",
            ["PLAN"] = "PLAN",
            ["UCSICON"] = "UCS",
            
            // Block Commands
            ["BLOCK"] = "B",
            ["INSERT"] = "I",
            ["WBLOCK"] = "W",
            ["BMAKE"] = "BM",
            ["BEDIT"] = "BE",
            ["BCLOSE"] = "BC",
            ["BATTMAN"] = "BAT",
            ["BURST"] = "BURST",
            
            // Inquiry Commands
            ["LIST"] = "LI",
            ["DISTANCE"] = "DI",
            ["AREA"] = "AA",
            ["ID"] = "ID",
            ["TIME"] = "TIME",
            ["STATUS"] = "STAT",
            ["PROPERTIES"] = "PR",
            ["MATCHPROP"] = "MA",
            
            // Selection Commands
            ["QSELECT"] = "QSEL",
            ["SELECT"] = "SELECT",
            ["SELECTSIMILAR"] = "SELECTSIMILAR",
            
            // 3D Commands
            ["BOX"] = "BOX",
            ["CYLINDER"] = "CYL",
            ["SPHERE"] = "SPHERE",
            ["CONE"] = "CONE",
            ["WEDGE"] = "WE",
            ["TORUS"] = "TOR",
            ["EXTRUDE"] = "EXT",
            ["REVOLVE"] = "REV",
            ["SWEEP"] = "SWEEP",
            ["LOFT"] = "LOFT",
            ["UNION"] = "UNI",
            ["SUBTRACT"] = "SU",
            ["INTERSECT"] = "IN",
            ["SLICE"] = "SL",
            
            // File Commands
            ["NEW"] = "NEW",
            ["OPEN"] = "OPEN",
            ["SAVE"] = "SAVE",
            ["SAVEAS"] = "SAVEAS",
            ["PLOT"] = "PLOT",
            ["PREVIEW"] = "PRE",
            ["PUBLISH"] = "PUBLISH",
            ["EXPORT"] = "EXP",
            ["IMPORT"] = "IMP",
            
            // Tools Commands
            ["OPTIONS"] = "OP",
            ["CUSTOMIZE"] = "CUI",
            ["TOOLPALETTES"] = "TP",
            ["CALCULATOR"] = "CAL",
            ["QUICKCALC"] = "QC",
            ["SPELL"] = "SP",
            ["FIND"] = "FIND",
            
            // Common Abbreviations
            ["PURGE"] = "PU",
            ["UNDO"] = "U",
            ["REDO"] = "REDO",
            ["QUIT"] = "Q",
            ["EXIT"] = "EXIT"
        };

        // Reverse mapping for display purposes
        private static readonly Dictionary<string, string> _aliasToCommand = 
            _commandAliases.ToDictionary(kvp => kvp.Value, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the alias for a command. If no alias exists, returns first 3 letters.
        /// </summary>
        public static string GetAlias(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
                return commandName;

            // Check if we have a predefined alias
            if (_commandAliases.TryGetValue(commandName, out string alias))
            {
                return alias;
            }

            // Fallback: use first 3 letters (or less if command is shorter)
            return commandName.Length >= 3 ? commandName.Substring(0, 3).ToUpper() : commandName.ToUpper();
        }

        /// <summary>
        /// Gets the full command name from an alias
        /// </summary>
        public static string GetCommandFromAlias(string alias)
        {
            if (string.IsNullOrEmpty(alias))
                return alias;

            if (_aliasToCommand.TryGetValue(alias, out string command))
            {
                return command;
            }

            return alias; // Return as-is if no mapping found
        }

        /// <summary>
        /// Checks if a given string is a known alias
        /// </summary>
        public static bool IsKnownAlias(string alias)
        {
            return _aliasToCommand.ContainsKey(alias);
        }

        /// <summary>
        /// Gets all available aliases
        /// </summary>
        public static IEnumerable<string> GetAllAliases()
        {
            return _commandAliases.Values;
        }

        /// <summary>
        /// Gets all command-alias pairs
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> GetAllCommandAliases()
        {
            return _commandAliases;
        }

        /// <summary>
        /// Adds or updates a custom alias
        /// </summary>
        public static void AddCustomAlias(string command, string alias)
        {
            if (!string.IsNullOrEmpty(command) && !string.IsNullOrEmpty(alias))
            {
                _commandAliases[command] = alias.ToUpper();
            }
        }

        /// <summary>
        /// Gets display text showing both command and alias
        /// </summary>
        public static string GetDisplayText(string commandName)
        {
            var alias = GetAlias(commandName);
            if (alias.Equals(commandName, StringComparison.OrdinalIgnoreCase))
            {
                return commandName; // No alias, just show command
            }
            return $"{commandName} ({alias})"; // Show both: "POLYLINE (PL)"
        }

        /// <summary>
        /// Parses input to determine if it's a command or alias and returns the appropriate execution string
        /// </summary>
        public static string GetExecutionCommand(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // If it's a known alias, use it directly
            if (IsKnownAlias(input))
            {
                return input;
            }

            // If it's a full command name, get its alias
            return GetAlias(input);
        }
    }
}