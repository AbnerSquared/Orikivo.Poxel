﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Orikivo.Drawing
{
    /// <summary>
    /// A <see cref="Sprite"/> that contains multiple images, cropped according to a grid-based pattern.
    /// </summary>
    public class Sheet : Sprite
    {
        [JsonConstructor]
        public Sheet(string url, int cropWidth, int cropHeight, 
            List<SheetOverride> overrides = null, string id = null) : base(url, id)
        {
            Overrides = overrides ?? new List<SheetOverride>();
            CropHeight = cropHeight;
            CropWidth = cropWidth;

            using (Bitmap source = GetSource())
            {
                if (source.Width % CropWidth != 0 || source.Height % CropHeight != 0)
                    throw new IndexOutOfRangeException("The crop specified does not completely crop the sheet.");

                ColumnCount = source.Width / CropWidth;
                RowCount = source.Height / CropHeight;
            }
        }

        /// <summary>
        /// A collection of custom specifications for a specific crop.
        /// </summary>
        [JsonProperty("overrides")]
        private List<SheetOverride> Overrides { get; }

        /// <summary>
        /// The width of each image on the <see cref="Sheet"/>.
        /// </summary>
        [JsonProperty("crop_width")]
        public int CropWidth { get; }

        /// <summary>
        /// The height of each image on the <see cref="Sheet"/>.
        /// </summary>
        [JsonProperty("crop_height")]
        public int CropHeight { get; }

        /// <summary>
        /// The number of rows that exist on the current <see cref="Sheet"/>.
        /// </summary>
        [JsonIgnore]
        public int RowCount { get; }

        /// <summary>
        /// The number of columns that exist on the current <see cref="Sheet"/>.
        /// </summary>
        [JsonIgnore]
        public int ColumnCount { get; }


        /// <summary>
        /// Returns the image at the specified index.
        /// </summary>
        public Bitmap GetBitmap(int index) // # of total crops
        {
            if (index > ColumnCount * RowCount)
                throw new ArgumentOutOfRangeException();

            int column = 1;

            while (index - ColumnCount > 0)
            {
                column++;
                index -= ColumnCount;
            }

            return GetBitmap(index, column);
        }

        /// <summary>
        /// Returns the image at the specified row and column.
        /// </summary>
        public Bitmap GetBitmap(int row, int column)
        {
            if (row > RowCount || column > ColumnCount)
                throw new ArgumentOutOfRangeException("The specified row or column is out of range.");

            SheetOverride crop = Overrides.FirstOrDefault(x => x.Row == row && x.Column == column) ?? SheetOverride.Empty;
            
            return BitmapUtils.Crop(Path, (CropWidth * column - 1) + crop.Offset.X, (CropHeight * row - 1) + crop.Offset.Y,
                crop.Width ?? CropWidth, crop.Height ?? CropHeight);
        }

        /// <summary>
        /// Returns all of the images on the <see cref="Sheet"/> with their corresponding row and column.
        /// </summary>
        public List<(Point Index, Bitmap Bitmap)> GetBitmaps()
        {
            List<(Point Index, Bitmap Bitmap)> bitmaps = new List<(Point, Bitmap)>();

            for (int x = 1; x <= ColumnCount; x++)
                for (int y = 1; y <= RowCount; y++)
                    bitmaps.Add((new Point(y, x), GetBitmap(y, x)));

            return bitmaps;
        }
    }
}
