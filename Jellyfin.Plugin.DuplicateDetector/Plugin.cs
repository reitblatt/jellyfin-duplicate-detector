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
    public class DuplicateDetectorPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        private static readonly string DebugLog = "/tmp/jellyfin_plugin_debug.txt";

        /// <summary>
        /// Gets the plugin instance.
        /// </summary>
        public static DuplicateDetectorPlugin Instance { get; private set; } = null!;

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
        /// Initializes a new instance of the <see cref="DuplicateDetectorPlugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
        public DuplicateDetectorPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {         
            Instance = this;
            LogDebug("🚀 DuplicateDetectorPlugin constructor called");
LogDebug($"📁 Application paths: {applicationPaths?.ProgramDataPath}");
        }


        /// <summary>
        /// Gets the plugin pages.
        /// </summary>
        /// <returns>An enumerable of plugin page information.</returns>
        public IEnumerable<PluginPageInfo> GetPages() {
            LogDebug("📄 GetPages() method called");
            
try {
  var assembly = GetType().Assembly;
  var resourcePath = "Jellyfin.Plugin.DuplicateDetector.Configuration.duplicatedetector.html";

  LogDebug($"🔍 Looking for resource: {resourcePath}");

  // List all embedded resources
  var allResources = assembly.GetManifestResourceNames();
  LogDebug($"📋 Found {allResources.Length} embedded resources:");
  foreach(var resource in allResources) {
    LogDebug($"  - {resource}");
  }

  // Test if we can read the HTML resource
  using(var stream = assembly.GetManifestResourceStream(resourcePath)) {
    if (stream == null) {
      LogDebug("❌ Resource stream is null!");
      return new PluginPageInfo[0];
    }

    using(var reader = new StreamReader(stream)) {
      var content = reader.ReadToEnd();
      LogDebug($"✅ Resource loaded: {content.Length} characters");
      LogDebug($"📝 Content preview: {content.Substring(0, Math.Min(100, content.Length))}...");
    }
  }

  var pageInfo = new PluginPageInfo {
    Name = this.Name,
    EmbeddedResourcePath = resourcePath
};

LogDebug($"📃 Created PluginPageInfo:");
LogDebug($"  Name: '{pageInfo.Name}'");
LogDebug($"  EmbeddedResourcePath: '{pageInfo.EmbeddedResourcePath}'");

var pages = new[]{pageInfo};
LogDebug($"✅ Returning {pages.Length} page(s) to Jellyfin");

return pages;
} catch (Exception ex) {
LogDebug($"💥 ERROR in GetPages(): {ex}");
return new PluginPageInfo[0];
}
        }
// return new[]{
//                 new
//                 PluginPageInfo
//                 {
//                 Name = "DuplicateDetector",
//                 EmbeddedResourcePath = "Jellyfin.Plugin.DuplicateDetector.Configuration.duplicatedetector.html",
//                 EnableInMainMenu = true,
//                 DisplayName = "Duplicate Detector",
//                 MenuSection = "server", // Try adding menu section
//                 MenuIcon = "settings" // Try adding an icon
//                 }
//             };
//             }


        private static void LogDebug(string message) {
        try {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logLine = $"[{timestamp}] {message}\n";
            File.AppendAllText(DebugLog, logLine);
        } catch  {
            // Ignore logging errors
        }
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