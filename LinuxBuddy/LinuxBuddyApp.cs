using CommandLine;
using CommandLine.Text;

namespace LinuxBuddy
{
    /// <summary>
    /// Main application class for LinuxBuddy.
    /// Handles command-line parsing, dispatches commands, and manages application flow.
    /// </summary>
    public class LinuxBuddyApp
    {
        // Service for managing application settings.
        private readonly Services.SettingsService _settingsService;

        // Service for handling chat-based AI interactions.
        private readonly Services.ChatService _chatService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinuxBuddyApp"/> class.
        /// </summary>
        /// <param name="settingsService">Settings service for configuration and persistence.</param>
        /// <param name="chatService">Chat service for AI interactions.</param>
        public LinuxBuddyApp(Services.SettingsService settingsService, Services.ChatService chatService)
        {
            _settingsService = settingsService;
            _chatService = chatService;
        }

        /// <summary>
        /// Entry point for running the application.
        /// Parses command-line arguments, handles piped input, and dispatches to the appropriate command handler.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public async Task RunAsync(string[] args)
        {
            try
            {
                // Parse command-line arguments using CommandLineParser
                var parser = new Parser(with => with.HelpWriter = null);
                var parserResult = parser.ParseArguments<Verbs.Bash, Verbs.General, Verbs.ModelOptions>(args);

                // Check if there is data in stdin (i.e., data has been piped into the program)
                string pipedData = string.Empty;
                if (Console.IsInputRedirected)
                {
                    // Read all piped input asynchronously
                    pipedData = await Console.In.ReadToEndAsync();
                }

                // Dispatch to the appropriate handler based on parsed verb
                _ = await parserResult.MapResult(
                    async (Verbs.Bash opts) => await RunAndReturnExitCode(opts, pipedData, _chatService.AskBashQuestion),
                    async (Verbs.General opts) => await RunAndReturnExitCode(opts, pipedData, _chatService.AskGeneralQuestion),
                    (Verbs.ModelOptions opts) => Task.FromResult(RunModelAndReturnExitCode(opts)),
                    errs => Task.FromResult(DisplayHelp(parserResult, errs)));
            }
            catch (Exception ex)
            {
                // Display error details based on verbosity setting
                if (_settingsService.Verbose)
                {
                    Console.Error.WriteLine(ex);
                }
                else
                {
                    Console.Error.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Runs the specified question handler and returns an exit code.
        /// Sets verbosity if requested and invokes the appropriate chat service method.
        /// </summary>
        /// <typeparam name="T">Type of options implementing IOptions.</typeparam>
        /// <param name="opts">Parsed options containing prompt and verbosity.</param>
        /// <param name="context">Optional context from piped input.</param>
        /// <param name="askQuestionFunc">Async function to handle the question.</param>
        /// <returns>Exit code (0 for success).</returns>
        private async Task<int> RunAndReturnExitCode<T>(T opts, string context, Func<string, string, string, Task> askQuestionFunc) where T : Verbs.IOptions
        {
            // Enable verbose output if requested
            if (opts.Verbose)
            {
                _settingsService.Verbose = true;
            }
            if (!string.IsNullOrEmpty(opts.Prompt))
            {
                // Run the async question handler
                await askQuestionFunc(opts.Prompt, _settingsService.GetModel(), context);
            }
            return 0;
        }

        /// <summary>
        /// Handles the 'model' verb to set and persist the model name.
        /// </summary>
        /// <param name="opts">ModelOptions containing the model name.</param>
        /// <returns>Exit code (0 for success).</returns>
        private int RunModelAndReturnExitCode(Verbs.ModelOptions opts)
        {
            // Save the specified model name to settings
            _settingsService.SaveModel(opts.Model);
            return 0;
        }

        /// <summary>
        /// Displays help text for the specified parser result and errors.
        /// </summary>
        /// <typeparam name="T">Type of parser result.</typeparam>
        /// <param name="result">Parser result object.</param>
        /// <param name="errs">Enumeration of parsing errors.</param>
        /// <returns>Exit code (0 for help displayed).</returns>
        private int DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            // Build and display help text using CommandLineParser's HelpText
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
            return 0;
        }
    }
}
