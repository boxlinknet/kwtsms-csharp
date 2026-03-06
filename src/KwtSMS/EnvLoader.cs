using System;
using System.Collections.Generic;
using System.IO;

namespace KwtSMS
{
    /// <summary>
    /// Loads key=value pairs from .env files. Read-only, never modifies environment variables.
    /// </summary>
    internal static class EnvLoader
    {
        /// <summary>
        /// Parse a .env file and return key-value pairs.
        /// Returns empty dictionary if file not found. Never throws.
        /// </summary>
        internal static Dictionary<string, string> LoadEnvFile(string path)
        {
            var result = new Dictionary<string, string>();

            try
            {
                if (!File.Exists(path)) return result;

                foreach (var rawLine in File.ReadAllLines(path))
                {
                    var line = rawLine.Trim();

                    // Skip blank lines and comments
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                        continue;

                    // Find the first = sign
                    var eqIndex = line.IndexOf('=');
                    if (eqIndex < 0) continue;

                    var key = line.Substring(0, eqIndex).Trim();
                    if (string.IsNullOrEmpty(key)) continue;

                    var value = line.Substring(eqIndex + 1).Trim();

                    // Handle quoted values
                    if (value.Length >= 2)
                    {
                        if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                            (value.StartsWith("'") && value.EndsWith("'")))
                        {
                            value = value.Substring(1, value.Length - 2);
                        }
                        else
                        {
                            // Strip inline comments from unquoted values
                            var commentIndex = value.IndexOf('#');
                            if (commentIndex > 0)
                            {
                                value = value.Substring(0, commentIndex).TrimEnd();
                            }
                        }
                    }
                    else
                    {
                        // Single char value, strip inline comments
                        var commentIndex = value.IndexOf('#');
                        if (commentIndex > 0)
                        {
                            value = value.Substring(0, commentIndex).TrimEnd();
                        }
                    }

                    result[key] = value;
                }
            }
            catch
            {
                // Never crash on .env parsing
            }

            return result;
        }
    }
}
