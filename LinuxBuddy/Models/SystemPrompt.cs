namespace LinuxBuddy.Models
{
    /// <summary>
    /// Represents the system prompt sent to the AI, including user and environment details.
    /// </summary>
    public class SystemPrompt
    {
        public string Username { get; set; } = string.Empty;
        public string CurrentDateTime { get; set; } = string.Empty;
        public string CurrentWorkingDirectory { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string AllowedResponseType { get; set; } = string.Empty;
    }
}
