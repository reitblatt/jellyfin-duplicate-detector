using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Jellyfin.Plugin.DuplicateDetector.Configuration;

namespace Jellyfin.Plugin.DuplicateDetector;

    /// <summary>
    /// The main plugin class.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        /// <summary>
        /// Gets the plugin instance.
        /// </summary>
        public static Plugin Instance { get; private set; } = null!;

        /// <summary>
        /// Gets the plugin name.
        /// </summary>
        public override string Name => "Duplicate Detector";

        /// <summary>
        /// Gets the plugin description.
        /// </summary>
        public override string Description => "Detects duplicate videos in your Jellyfin library";

        /// <summary>
        /// Gets the plugin ID.
        /// </summary>
        public override Guid Id => Guid.Parse("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d");

        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        /// <summary>
        /// Gets the plugin pages.
        /// </summary>
        /// <returns>An enumerable of plugin page information.</returns>
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "Duplicate Detector",
                    EmbeddedResourcePath = "Jellyfin.Plugin.DuplicateDetector.Configuration.duplicatedetector.html",
                    EnableInMainMenu = true,
                    DisplayName = "Duplicate Detector"
                }
            };
        }

        /// <summary>
        /// Updates the plugin configuration.
        /// </summary>
        /// <param name="configuration">The new configuration.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task UpdateConfigurationAsync(PluginConfiguration configuration)
        {
            Configuration = configuration;
            return Task.CompletedTask;
        }
    }