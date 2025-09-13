namespace LinuxBuddy.Verbs
{
    /// <summary>
    /// Interface for command-line options that support verbose output and a prompt.
    /// </summary>
    public interface IOptions
    {
        bool Verbose { get; set; }
        string Prompt { get; set; }
    }
}
