using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Drawing;
using System.Security.Cryptography;
using Jellyfin.Data.Enums;

namespace Jellyfin.Plugin.DuplicateDetector;

  /// <summary>
  /// Service for detecting duplicate videos in a Jellyfin library.
  /// </summary>
  public class DuplicateDetectionService {
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<DuplicateDetectionService> _logger;
    private readonly double _similarityThreshold = 0.85; // 85% similarity threshold

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateDetectionService"/> class.
    /// </summary>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{DuplicateDetectionService}"/> interface.</param>
    public DuplicateDetectionService(ILibraryManager libraryManager, ILogger<DuplicateDetectionService> logger) {
      _libraryManager = libraryManager;
      _logger = logger;
    }

    /// <summary>
    /// Finds duplicate videos in the specified library.
    /// </summary>
    /// <param name="library">The library to scan for duplicates.</param>
    /// <returns>A list of duplicate groups.</returns>
    public async Task<List<List<VideoInfo>>> FindDuplicatesAsync(BaseItem library) {
      _logger.LogInformation("Starting duplicate detection for library: {LibraryName}", library.Name);

      var videos = await GetVideosFromLibraryAsync(library);
      var duplicateGroups = new List<List<VideoInfo>>();

      for (int i = 0; i < videos.Count; i++) {
        var currentVideo = videos[i];
        var currentGroup = new List<VideoInfo> { currentVideo };

        for (int j = i + 1; j < videos.Count; j++) {
          var compareVideo = videos[j];
          if (AreVideosSimilar(currentVideo, compareVideo)) {
            currentGroup.Add(compareVideo);
            videos.RemoveAt(j);
            j--;
          }
        }

        if (currentGroup.Count > 1) {
          duplicateGroups.Add(currentGroup);
        }
      }

      _logger.LogInformation("Found {Count} groups of duplicate videos", duplicateGroups.Count);
      return duplicateGroups;
    }

    private async Task<List<VideoInfo>> GetVideosFromLibraryAsync(BaseItem library) {
      var videos = new List<VideoInfo>();
      var query = new InternalItemsQuery {
        Parent = library,
        IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode },
        Recursive = true
      };

      var items = _libraryManager.GetItemList(query);
      foreach (var item in items) {
        if (item is Movie movie) {
          videos.Add(new VideoInfo {
            Name = movie.Name,
            Path = movie.Path,
            Resolution = GetVideoResolution(movie),
            Runtime = movie.RunTimeTicks.HasValue ? TimeSpan.FromTicks(movie.RunTimeTicks.Value).TotalSeconds : 0,
            FileSize = GetFileSize(movie.Path)
          });
        }
        else if (item is Episode episode) {
          videos.Add(new VideoInfo {
            Name = $"{episode.SeriesName} - S{episode.ParentIndexNumber:D2}E{episode.IndexNumber:D2}",
            Path = episode.Path,
            Resolution = GetVideoResolution(episode),
            Runtime = episode.RunTimeTicks.HasValue ? TimeSpan.FromTicks(episode.RunTimeTicks.Value).TotalSeconds : 0,
            FileSize = GetFileSize(episode.Path)
          });
        }
      }

      return videos;
    }

    private string GetVideoResolution(BaseItem video) {
      // This is a placeholder - you'll need to implement actual resolution detection
      return "1080p";
    }

    private long GetFileSize(string path) {
      try {
        var fileInfo = new FileInfo(path);
        return fileInfo.Exists ? fileInfo.Length : 0;
      }
      catch (Exception ex) {
        _logger.LogError(ex, "Error getting file size for {Path}", path);
        return 0;
      }
    }

    private bool AreVideosSimilar(VideoInfo video1, VideoInfo video2) {
      var similarityScore = CalculateSimilarity(video1, video2);
      return similarityScore >= _similarityThreshold;
    }

    private double CalculateSimilarity(VideoInfo video1, VideoInfo video2) {
      var scores = new List<double>();

      // Compare names
      if (video1.Name == video2.Name) {
        scores.Add(1.0);
      }
      else {
        scores.Add(0.0);
      }

      // Compare resolutions
      if (video1.Resolution == video2.Resolution) {
        scores.Add(1.0);
      }
      else {
        scores.Add(0.0);
      }

      // Compare runtimes (with tolerance)
      if (Math.Abs(video1.Runtime - video2.Runtime) <= 30) { // 30 seconds tolerance
        scores.Add(1.0);
      }
      else {
        scores.Add(0.0);
      }

      // Compare file sizes (with percentage tolerance)
      var sizeDiff = Math.Abs(video1.FileSize - video2.FileSize);
      var avgSize = (video1.FileSize + video2.FileSize) / 2.0;
      var sizeDiffPercent = (sizeDiff / avgSize) * 100;
      if (sizeDiffPercent <= 10) { // 10% tolerance
        scores.Add(1.0);
      }
      else {
        scores.Add(0.0);
      }

      return scores.Average();
    }
  }

  /// <summary>
  /// Represents information about a video file.
  /// </summary>
  public class VideoInfo {
    /// <summary>
    /// Gets or sets the video name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the video file path.
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// Gets or sets the video resolution.
    /// </summary>
    public required string Resolution { get; set; }

    /// <summary>
    /// Gets or sets the video runtime in seconds.
    /// </summary>
    public double Runtime { get; set; }

    /// <summary>
    /// Gets or sets the video file size in bytes.
    /// </summary>
    public long FileSize { get; set; }
  }