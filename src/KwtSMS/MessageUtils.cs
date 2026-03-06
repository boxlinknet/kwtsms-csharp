using System;
using System.Text;
using System.Text.RegularExpressions;

namespace KwtSMS
{
    /// <summary>
    /// Message cleaning utilities for SMS content sanitization.
    /// </summary>
    public static class MessageUtils
    {
        private static readonly Regex HtmlTagRegex = new Regex("<[^>]*>", RegexOptions.Compiled);

        /// <summary>
        /// Clean SMS message text before sending.
        /// Strips emojis, hidden control characters, HTML tags, and converts Arabic digits to Latin.
        /// Arabic text is fully preserved.
        /// Called automatically by Send(), but can also be used manually.
        /// </summary>
        /// <param name="text">Raw message text.</param>
        /// <returns>Cleaned message text safe for SMS delivery.</returns>
        public static string CleanMessage(string? text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            var sb = new StringBuilder(text!.Length);

            // Process character by character using StringInfo to handle surrogate pairs
            int i = 0;
            while (i < text.Length)
            {
                int codePoint;
                if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    codePoint = char.ConvertToUtf32(text[i], text[i + 1]);
                    i += 2;
                }
                else
                {
                    codePoint = text[i];
                    i += 1;
                }

                // 1. Convert Arabic-Indic digits (U+0660-U+0669) to Latin
                if (codePoint >= 0x0660 && codePoint <= 0x0669)
                {
                    sb.Append((char)('0' + (codePoint - 0x0660)));
                    continue;
                }

                // 2. Convert Extended Arabic-Indic / Persian digits (U+06F0-U+06F9) to Latin
                if (codePoint >= 0x06F0 && codePoint <= 0x06F9)
                {
                    sb.Append((char)('0' + (codePoint - 0x06F0)));
                    continue;
                }

                // 3. Remove emojis
                if (IsEmoji(codePoint)) continue;

                // 4. Remove hidden invisible characters
                if (IsHiddenChar(codePoint)) continue;

                // 5. Remove directional formatting characters
                if (IsDirectionalChar(codePoint)) continue;

                // 6. Remove C0/C1 control characters (preserve \n and \t)
                if (IsControlChar(codePoint)) continue;

                // Keep the character
                if (codePoint <= 0xFFFF)
                {
                    sb.Append((char)codePoint);
                }
                else
                {
                    sb.Append(char.ConvertFromUtf32(codePoint));
                }
            }

            // 7. Strip HTML tags
            var result = HtmlTagRegex.Replace(sb.ToString(), "");

            return result;
        }

        private static bool IsEmoji(int cp)
        {
            // Mahjong tiles (U+1F000-U+1F02F)
            if (cp >= 0x1F000 && cp <= 0x1F02F) return true;
            // Playing cards (U+1F0A0-U+1F0FF)
            if (cp >= 0x1F0A0 && cp <= 0x1F0FF) return true;
            // Regional indicator symbols / flag components (U+1F1E0-U+1F1FF)
            if (cp >= 0x1F1E0 && cp <= 0x1F1FF) return true;
            // Misc symbols and pictographs (U+1F300-U+1F5FF)
            if (cp >= 0x1F300 && cp <= 0x1F5FF) return true;
            // Emoticons (U+1F600-U+1F64F)
            if (cp >= 0x1F600 && cp <= 0x1F64F) return true;
            // Transport and map (U+1F680-U+1F6FF)
            if (cp >= 0x1F680 && cp <= 0x1F6FF) return true;
            // Alchemical symbols (U+1F700-U+1F77F)
            if (cp >= 0x1F700 && cp <= 0x1F77F) return true;
            // Geometric shapes extended (U+1F780-U+1F7FF)
            if (cp >= 0x1F780 && cp <= 0x1F7FF) return true;
            // Supplemental arrows (U+1F800-U+1F8FF)
            if (cp >= 0x1F800 && cp <= 0x1F8FF) return true;
            // Supplemental symbols and pictographs (U+1F900-U+1F9FF)
            if (cp >= 0x1F900 && cp <= 0x1F9FF) return true;
            // Chess symbols (U+1FA00-U+1FA6F)
            if (cp >= 0x1FA00 && cp <= 0x1FA6F) return true;
            // Symbols and pictographs extended (U+1FA70-U+1FAFF)
            if (cp >= 0x1FA70 && cp <= 0x1FAFF) return true;
            // Misc symbols (U+2600-U+26FF)
            if (cp >= 0x2600 && cp <= 0x26FF) return true;
            // Dingbats (U+2700-U+27BF)
            if (cp >= 0x2700 && cp <= 0x27BF) return true;
            // Variation selectors (U+FE00-U+FE0F)
            if (cp >= 0xFE00 && cp <= 0xFE0F) return true;
            // Combining enclosing keycap (U+20E3)
            if (cp == 0x20E3) return true;
            // Tags block (U+E0000-U+E007F)
            if (cp >= 0xE0000 && cp <= 0xE007F) return true;

            return false;
        }

        private static bool IsHiddenChar(int cp)
        {
            return cp == 0x200B  // Zero-width space
                || cp == 0x200C  // Zero-width non-joiner
                || cp == 0x200D  // Zero-width joiner
                || cp == 0x2060  // Word joiner
                || cp == 0x00AD  // Soft hyphen
                || cp == 0xFEFF  // BOM
                || cp == 0xFFFC; // Object replacement character
        }

        private static bool IsDirectionalChar(int cp)
        {
            return cp == 0x200E  // Left-to-right mark
                || cp == 0x200F  // Right-to-left mark
                || (cp >= 0x202A && cp <= 0x202E) // LRE, RLE, PDF, LRO, RLO
                || (cp >= 0x2066 && cp <= 0x2069); // LRI, RLI, FSI, PDI
        }

        private static bool IsControlChar(int cp)
        {
            // C0 controls (U+0000-U+001F) except TAB (U+0009) and LF (U+000A)
            if (cp >= 0x0000 && cp <= 0x001F && cp != 0x0009 && cp != 0x000A)
                return true;
            // DEL (U+007F)
            if (cp == 0x007F) return true;
            // C1 controls (U+0080-U+009F)
            if (cp >= 0x0080 && cp <= 0x009F) return true;
            return false;
        }
    }
}
