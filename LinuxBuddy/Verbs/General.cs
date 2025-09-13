using CommandLine;

namespace LinuxBuddy.Verbs
{
    /// <summary>
    /// Command-line options for the 'general' verb.
    /// Used to ask a general question to the AI.
    /// </summary>
    [Verb("general", HelpText = "Ask the AI a general question.")]
    public class General : IOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
        [Value(0, Required = true, HelpText = "Prompt for the AI.")]
        public string Prompt { get; set; } = string.Empty;
    }
}
