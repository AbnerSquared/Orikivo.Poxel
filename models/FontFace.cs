﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Orikivo.Drawing
{
    /// <summary>
    /// Represents a font to be used alongside <see cref="GraphicsWriter"/> when drawing bodies of text.
    /// </summary>
    public class FontFace
    {
        private static FontTag GetTagValue(bool isMonospace, bool isUnicodeSupported)
        {
            FontTag tag = 0;

            if (isMonospace)
                tag |= FontTag.Monospace;

            if (isUnicodeSupported)
                tag |= FontTag.UnicodeSupported;

            return tag;
        }


        // TODO: Make .FromPath(string) use the generic JsonHandler.
        public static FontFace FromPath(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
                return default;
            }

            using (StreamReader stream = File.OpenText(path))
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    Formatting = Formatting.Indented
                };

                using (JsonReader reader = new JsonTextReader(stream))
                {
                    FontFace font = JsonSerializer.Create(settings).Deserialize<FontFace>(reader);
                    return font ?? (default);
                }
            }
        }

        public FontFace(FontFaceBuilder builder)
        {
            CharWidth = builder.Width;
            CharHeight = builder.Height;
            Padding = builder.Padding; // TODO: Set Default to Padding.Char at builder.
            Tag = GetTagValue(builder.IsMonospace, builder.IsUnicodeSupported);
            SheetUrls = builder.SheetUrls;
            Whitespace = builder.Whitespace;
            Customs = builder.Customs;
            HideBadUnicode = builder.HideBadUnicode;
        }

        [JsonConstructor]
        internal FontFace(int width, int height, FontTag tag, Dictionary<int, string> sheetUrls,
            Padding? padding = null, List<WhiteSpaceInfo> empties = null, List<CustomCharInfo> customs = null,
            bool hideBadUnicode = false)
        {
            CharWidth = width;
            CharHeight = height;
            Padding = padding ?? Padding.Char;
            Tag = tag;
            SheetUrls = sheetUrls;
            Whitespace = empties;
            Customs = customs;
            HideBadUnicode = hideBadUnicode;
        }

        [JsonProperty("width")]
        public int CharWidth { get; }

        [JsonProperty("height")]
        public int CharHeight { get; }

        [JsonIgnore]
        public Size Ppu => new Size(CharWidth, CharHeight);

        /// <summary>
        /// The <see cref="Padding"/> that will be used with each <see cref="char"/> sprite value.
        /// </summary>
        [JsonProperty("padding")]
        public Padding Padding { get; }

        [JsonProperty("tag")]
        public FontTag Tag { get; }

        [JsonProperty("sheets")]
        public /*IReadOnly*/Dictionary<int, string> SheetUrls { get; }

        [JsonProperty("empties")]
        public /*IReadOnly*/List<WhiteSpaceInfo> Whitespace { get; }

        [JsonProperty("customs")]
        public /*IReadOnly*/List<CustomCharInfo> Customs { get; }

        [JsonProperty("hide_bad_unicode")]
        public bool HideBadUnicode { get; }

        [JsonIgnore]
        public bool IsUnicodeSupported => Tag.HasFlag(FontTag.UnicodeSupported);

        [JsonIgnore]
        public bool IsMonospace => Tag.HasFlag(FontTag.Monospace);

        public Point GetCharOffset(char c)
            => Customs?.FirstOrDefault(x => x.Chars.Contains(c))?.Offset ?? Point.Empty;

        public int GetCharWidth(char c)
            => Customs?.FirstOrDefault(x => x.Chars.Contains(c))?.Width ?? CharWidth;

        public WhiteSpaceInfo GetWhiteSpace(char c)
            => Whitespace.FirstOrDefault(x => x.Chars.Contains(c));

        public int GetWhiteSpaceWidth(char c)
            => GetWhiteSpace(c)?.Width ?? CharWidth;
    }
}
