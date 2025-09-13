using CommandLine;

namespace LinuxBuddy.Verbs
{
    /// <summary>
    /// Command-line options for the 'model' verb.
    /// Used to set the model for AI completion.
    /// </summary>
    [Verb("model", HelpText = "Set the model to use for completion.")]
    public class ModelOptions
    {
        [Value(0, MetaName = "name", HelpText = "Model name.", Required = true)]
        public string Model { get; set; } = string.Empty;
    }
}
