using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Text.Json;

namespace LinuxBuddy
{
    internal class Program
    {
        static bool verbose = false;
        static string model = "deepseek-r1:1.5b";
        static string ollamaUrl = "http://192.168.25.26:11434";
        static async Task Main(string[] args)
        {
            // Get the path to the user's home directory
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Define the path to the settings file
            string settingsPath = Path.Combine(homePath, ".linuxBuddySettings");

            // check if the file exists
            if (File.Exists(settingsPath))
            {
                // Read the model name from the settings file
                model = File.ReadAllText(settingsPath);
            }
            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<Bash, General, ModelOptions>(args);

            // Check if there is data in stdin (i.e., data has been piped into the program)
            string pipedData = string.Empty;
            if (Console.IsInputRedirected)
            {
                pipedData = await Console.In.ReadToEndAsync();
            }

            parserResult.MapResult(
                (Bash opts) => RunAndReturnExitCode(opts, pipedData, AskBashQuestion),
                (General opts) => RunAndReturnExitCode(opts, pipedData, AskGeneralQuestion),
                (ModelOptions opts) => RunModelAndReturnExitCode(opts),
                errs => HandleParseError(parserResult, errs));
        }

        private static int RunAndReturnExitCode<T>(T opts, string context, Func<string, string, string, Task> askQuestionFunc) where T : IOptions
        {
            if (opts.Verbose)
            {
                verbose = true;
            }
            if (!string.IsNullOrEmpty(opts.Prompt))
            {
                // Since we can't use async in this method, we'll use .GetAwaiter().GetResult() to run the async method synchronously
                askQuestionFunc(opts.Prompt, model, context).GetAwaiter().GetResult();
            }
            return 0;
        }

        private static int RunModelAndReturnExitCode(ModelOptions opts)
        {
            // trim the model name and remove any quotes
            opts.Model = opts.Model.Trim().Trim('"');

            // Get the path to the user's home directory
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // Define the path to the settings file
            string settingsPath = Path.Combine(homePath, ".linuxBuddySettings");

            // Save the model to a settings file
            File.WriteAllText(settingsPath, opts.Model);
            return 0;
        }

        private static int HandleParseError(ParserResult<object> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
            return -1;
        }

        private static async Task AskBashQuestion(string question, string model, string context)
        {
            LogVerbose(context, question);
            await SmartHelpAsync(question, model, context, "You are an expert on Linux systems and bash commands. Help the user write a bash command. Only output bash command or commands. Do not explain yourself.", "Bash");
        }

        private static async Task AskGeneralQuestion(string question, string model, string context)
        {
            LogVerbose(context, question);
            await SmartHelpAsync(question, model, context, "You are a helpful assistant.", "Text");
        }

        private static void LogVerbose(string context, string question)
        {
            if (verbose)
            {
                if (!string.IsNullOrEmpty(context))
                {
                    Console.WriteLine("---------------------Context--------------------------");
                    Console.WriteLine($"{context}");
                    Console.WriteLine("------------------------------------------------------");
                    Console.WriteLine();
                }
                Console.WriteLine("---------------------Question-------------------------");
                Console.WriteLine($"{question}");
                Console.WriteLine("------------------------------------------------------");
                Console.WriteLine();
            }
        }

        private static async Task SmartHelpAsync(string userRequest, string model, string context, string instructions, string allowedResponseType)
        {
            IChatCompletionService _chatCompletionService = CreateChatCompletionService(model);

            SystemPrompt systemPrompt = new SystemPrompt
            {
                Username = Environment.UserName,
                CurrentDateTime = DateTime.Now.ToString(),
                CurrentWorkingDirectory = Environment.CurrentDirectory,
                Instructions = instructions,
                AllowedResponseType = allowedResponseType
            };

            var chatHistory = new ChatHistory($"{JsonSerializer.Serialize(systemPrompt)}");

            if (!string.IsNullOrEmpty(context))
            {
                chatHistory.AddUserMessage(context);
                if (allowedResponseType == "Bash")
                {
                    chatHistory.AddAssistantMessage("Thank you for the additional context. What bash command can I provide for you?");
                }
                else if (allowedResponseType == "Text")
                {
                    chatHistory.AddAssistantMessage("Thank you for the additional context. What would you like to know about the data you provided?");
                }
            }
            chatHistory.AddUserMessage(userRequest);
            OllamaPromptExecutionSettings settings = new OllamaPromptExecutionSettings();
            if (allowedResponseType == "Bash")
            {
                settings = new OllamaPromptExecutionSettings
                {
                    Temperature = 0,
                    TopP = 0.5f,
                    TopK = 0
                };
            }
            Console.WriteLine("---------------------Response-------------------------");
            await foreach (var chatMessageContent in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, settings))
            {
                Console.Write(chatMessageContent);
            }
            Console.WriteLine("\n------------------------------------------------------");
            Console.WriteLine();
        }

        private static IChatCompletionService CreateChatCompletionService(string model)
        {
            var services = new ServiceCollection();
            services.AddOllamaChatCompletion(model, new Uri(ollamaUrl));
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IChatCompletionService>();
        }

        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
        }
    }



    public class SystemPrompt
    {
        public string Username { get; set; } = string.Empty;
        public string CurrentDateTime { get; set; } = string.Empty;
        public string CurrentWorkingDirectory { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string AllowedResponseType { get; set; } = string.Empty;
    }

    [Verb("bash", HelpText = "Ask the AI for a bash command.")]
    public class Bash : IOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Value(0, Required = true, HelpText = "Prompt for the AI.")]
        public string Prompt { get; set; } = string.Empty;
    }

    [Verb("model", HelpText = "Set the model to use for completion.")]
    public class ModelOptions
    {
        [Value(0, MetaName = "name", HelpText = "Model name.", Required = true)]
        public string Model { get; set; } = string.Empty;
    }

    [Verb("general", HelpText = "Ask the AI a general question.")]
    public class General : IOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
        [Value(0, Required = true, HelpText = "Prompt for the AI.")]
        public string Prompt { get; set; } = string.Empty;
    }
    public interface IOptions
    {
        bool Verbose { get; set; }
        string Prompt { get; set; }
    }
}
