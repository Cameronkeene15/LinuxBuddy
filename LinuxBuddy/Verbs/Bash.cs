using CommandLine;

namespace LinuxBuddy.Verbs
{
    /// <summary>
    /// Command-line options for the 'bash' verb.
    /// Used to request a bash command from the AI.
    /// </summary>
    [Verb("bash", HelpText = "Ask the AI for a bash command.")]
    public class Bash : IOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Value(0, Required = true, HelpText = "Prompt for the AI.")]
        public string Prompt { get; set; } = string.Empty;
    }
}
