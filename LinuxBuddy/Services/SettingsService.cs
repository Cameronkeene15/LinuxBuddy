namespace LinuxBuddy.Services
{
    /// <summary>
    /// Manages application settings such as verbosity, model selection, and Ollama service URL.
    /// </summary>
    public class SettingsService
    {
        // Indicates whether verbose logging is enabled.
        private bool _verbose = false;

        // Default AI model name used if no user preference is set.
        private readonly string _defaultModel = "deepseek-r1:1.5b";

        // Default URL for the Ollama AI service.
        private readonly string _ollamaUrl = "http://192.168.25.26:11434";

        /// <summary>
        /// Gets or sets whether verbose logging is enabled.
        /// </summary>
        public bool Verbose
        {
            get
            {
                return _verbose;
            }
            set
            {
                _verbose = value;
            }
        }

        /// <summary>
        /// Gets the configured Ollama service URL.
        /// </summary>
        public Uri OllamaUrl
        {
            get
            {
                return new Uri(_ollamaUrl);
            }
        }

        /// <summary>
        /// Retrieves the currently selected AI model name.
        /// If a user-specific model is saved in the settings file, it is returned; otherwise, the default model is used.
        /// </summary>
        /// <returns>The model name as a string.</returns>
        public string GetModel()
        {
            // Get the path to the user's application data directory.
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Define the path to the settings file.
            string settingsPath = Path.Combine(homePath, ".linuxBuddySettings");

            // Load model name from settings file if it exists.
            if (File.Exists(settingsPath))
            {
                return File.ReadAllText(settingsPath);
            }
            return _defaultModel;
        }

        /// <summary>
        /// Saves the specified AI model name to the user's settings file.
        /// </summary>
        /// <param name="model">The model name to save.</param>
        public void SaveModel(string model)
        {
            // Get the path to the user's application data directory.
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Define the path to the settings file.
            string settingsPath = Path.Combine(homePath, ".linuxBuddySettings");

            // Ensure the directory exists.
            Directory.CreateDirectory(homePath);

            // Save the trimmed model name to the settings file.
            File.WriteAllText(settingsPath, model.Trim().Trim('"'));
        }
    }
}
