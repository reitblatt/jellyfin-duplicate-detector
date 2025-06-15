using System;
using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.DuplicateDetector.Configuration
{
    /// <summary>
    /// Plugin configuration.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            // Default configuration values
            EnableRuntimeComparison = true;
            EnableFileSizeComparison = true;
            EnableThumbnailComparison = false;
            RuntimeThresholdSeconds = 30;
            FileSizeThresholdPercent = 10;
            ThumbnailSimilarityThreshold = 0.85;
        }

        /// <summary>
        /// Gets or sets a value indicating whether runtime comparison is enabled.
        /// </summary>
        public bool EnableRuntimeComparison { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether file size comparison is enabled.
        /// </summary>
        public bool EnableFileSizeComparison { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether thumbnail comparison is enabled.
        /// </summary>
        public bool EnableThumbnailComparison { get; set; }

        /// <summary>
        /// Gets or sets the runtime threshold in seconds.
        /// </summary>
        public int RuntimeThresholdSeconds { get; set; }

        /// <summary>
        /// Gets or sets the file size threshold percentage.
        /// </summary>
        public int FileSizeThresholdPercent { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail similarity threshold (0.0 to 1.0).
        /// </summary>
        public double ThumbnailSimilarityThreshold { get; set; }
    }
} 