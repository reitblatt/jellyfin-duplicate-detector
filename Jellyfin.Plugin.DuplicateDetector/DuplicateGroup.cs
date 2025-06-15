using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.DuplicateDetector;

/// <summary>
/// Represents a group of duplicate media items.
/// </summary>
public class DuplicateGroup
{
    /// <summary>
    /// Gets or sets the list of duplicate videos.
    /// </summary>
    public required List<VideoInfo> Videos { get; set; }

    /// <summary>
    /// Gets or sets the similarity score between items.
    /// </summary>
    public double SimilarityScore { get; set; }

    /// <summary>
    /// Gets or sets the reason for considering these items duplicates.
    /// </summary>
    public required string Reason { get; set; }
} 