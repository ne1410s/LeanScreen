// <copyright file="CollationOptions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Abstractions.Imaging
{
    /// <summary>
    /// Collation options.
    /// </summary>
    public record CollationOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to override the order as
        /// present in the frames list according to frame chronology.
        /// </summary>
        public bool ForceChronology { get; set; } = true;

        /// <summary>
        /// Gets or sets the total number of columns in the collation.
        /// </summary>
        public int Columns { get; set; } = 4;

        /// <summary>
        /// Gets or sets the number of pixels separating each column.
        /// </summary>
        public int SpaceX { get; set; } = 10;

        /// <summary>
        /// Gets or sets the number of pixels separating each row.
        /// </summary>
        public int SpaceY { get; set; } = 10;

        /// <summary>
        /// Gets or sets the number of pixels before the first column.
        /// </summary>
        public int Top { get; set; } = 50;

        /// <summary>
        /// Gets or sets the number of pixels after the last column.
        /// </summary>
        public int Bottom { get; set; } = 10;

        /// <summary>
        /// Gets or sets the number of pixels before the first row and after the
        /// last row.
        /// </summary>
        public int Sides { get; set; } = 10;
    }
}
