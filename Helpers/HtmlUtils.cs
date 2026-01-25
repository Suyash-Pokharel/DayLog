// Helpers/HtmlUtils.cs
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DayLog.Helpers
{
    public static class HtmlUtils
    {
        // Regex to remove script/style blocks first, then tags
        private static readonly Regex _scriptStyleRegex = new Regex(@"<(script|style)[\s\S]*?>[\s\S]*?<\/\1>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _tagRegex = new Regex(@"<[^>]+>", RegexOptions.Compiled);
        private static readonly Regex _multiSpaceRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);

        /// <summary>
        /// Strips HTML tags, decodes HTML entities and collapses whitespace.
        /// Returns plain text.
        /// </summary>
        public static string StripHtml(string? html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            // Remove script/style blocks
            var withoutScripts = _scriptStyleRegex.Replace(html, string.Empty);

            // Remove tags
            var withoutTags = _tagRegex.Replace(withoutScripts, " ");

            // Decode HTML entities (&amp; &lt; etc.)
            string decoded = WebUtility.HtmlDecode(withoutTags);

            // Collapse whitespace and trim
            var collapsed = _multiSpaceRegex.Replace(decoded, " ").Trim();

            return collapsed;
        }

        /// <summary>
        /// Produce a short preview (plain text) from HTML content.
        /// Strips tags and returns first 'maxChars' characters (safe).
        /// </summary>
        public static string Truncate(string? html, int maxChars = 300)
        {
            var plain = StripHtml(html);
            if (plain.Length <= maxChars) return plain;
            return plain.Substring(0, maxChars) + "…";
        }

        /// <summary>
        /// Alternate helper name used for clarity in some code paths.
        /// </summary>
        public static string ToPlainText(string? html) => StripHtml(html);

        /// <summary>
        /// Compute word count from HTML string (strips tags then counts words).
        /// </summary>
        public static int WordCount(string? html)
        {
            var plain = StripHtml(html);
            if (string.IsNullOrWhiteSpace(plain)) return 0;
            var parts = plain.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length;
        }
    }
}
