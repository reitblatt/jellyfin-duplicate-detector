using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using MediaBrowser.Model.Entities;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Jellyfin.Plugin.DuplicateDetector.Configuration;
using Microsoft.AspNetCore.Authorization;


namespace Jellyfin.Plugin.DuplicateDetector;

    /// <summary>
    /// The duplicate detection controller.
    /// </summary>
    [ApiController]
    [Route("DuplicateDetector")]
    [Authorize(Policy = "DefaultAuthorization")]
    public class DuplicateDetectionController : ControllerBase
    {
        private readonly ILibraryManager _libraryManager;
        private readonly DuplicateDetectionService _duplicateService;
        private readonly ILogger<DuplicateDetectionController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateDetectionController"/> class.
        /// </summary>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="duplicateService">Instance of the <see cref="DuplicateDetectionService"/> class.</param>
        /// <param name="logger">Instance of the <see cref="ILogger{DuplicateDetectionController}"/> interface.</param>
        public DuplicateDetectionController(
            ILibraryManager libraryManager,
            DuplicateDetectionService duplicateService,
            ILogger<DuplicateDetectionController> logger)
        {
            _libraryManager = libraryManager;
            _duplicateService = duplicateService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the list of libraries.
        /// </summary>
        /// <returns>A list of library information.</returns>
        [HttpGet("Libraries")]
        public List<LibraryInfo> GetLibraries()
        {
            var libraries = _libraryManager.GetVirtualFolders();
            var result = new List<LibraryInfo>();

            foreach (var lib in libraries)
            {
                if (Guid.TryParse(lib.ItemId, out var id))
                {
                    result.Add(new LibraryInfo
                    {
                        Id = id,
                        Name = lib.Name ?? "Unknown",
                        Type = lib.CollectionType?.ToString() ?? "Unknown"
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the current plugin configuration.
        /// </summary>
        /// <returns>The plugin configuration.</returns>
        [HttpGet("Configuration")]
        public PluginConfiguration GetConfiguration()
        {
            return DuplicateDetectorPlugin.Instance.Configuration;
        }

        /// <summary>
        /// Updates the plugin configuration.
        /// </summary>
        /// <param name="config">The new configuration.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPost("Configuration")]
        public async Task UpdateConfiguration([FromBody] PluginConfiguration config)
        {
            await DuplicateDetectorPlugin.Instance.UpdateConfigurationAsync(config);
        }

        /// <summary>
        /// Scans a library for duplicates.
        /// </summary>
        /// <param name="libraryId">The library ID to scan.</param>
        /// <returns>A list of duplicate groups.</returns>
        [HttpGet("Scan/{libraryId}")]
        public async Task<List<DuplicateGroup>> ScanLibrary(string libraryId)
        {
            var library = _libraryManager.GetItemById(libraryId);
            if (library == null)
            {
                throw new ArgumentException($"Library with ID {libraryId} not found");
            }

            var duplicates = await _duplicateService.FindDuplicatesAsync(library).ConfigureAwait(false);
            return duplicates.Select(group => new DuplicateGroup
            {
                Videos = group.Select(video => new VideoInfo
                {
                    Name = video.Name,
                    Path = video.Path,
                    Resolution = video.Resolution,
                    Runtime = video.Runtime,
                    FileSize = video.FileSize
                }).ToList(),
                Reason = "Similar video content detected",
                SimilarityScore = 1.0
            }).ToList();
        }
    }

    /// <summary>
    /// Represents information about a library.
    /// </summary>
    public class LibraryInfo
    {
        /// <summary>
        /// Gets or sets the library ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the library name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the library type.
        /// </summary>
        public required string Type { get; set; }
    }
