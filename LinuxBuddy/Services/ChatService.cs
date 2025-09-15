using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Text;
using System.Text.Json;

namespace LinuxBuddy.Services
{
    /// <summary>
    /// Provides chat-based AI assistance for Linux and general questions using Semantic Kernel and Ollama models.
    /// </summary>
    public class ChatService
    {
        /// <summary>
        /// Service for accessing application settings, such as verbosity and model configuration.
        /// </summary>
        private readonly SettingsService _settingsService;

        /// <summary>
        /// Console output utility for styled text display.
        /// </summary>
        private readonly Utilities.StyledConsole _styledConsole;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatService"/> class.
        /// </summary>
        /// <param name="settingsService">The settings service to use for configuration.</param>
        public ChatService(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _styledConsole = new Utilities.StyledConsole();
        }

        /// <summary>
        /// Asks a Linux/bash-related question and streams the AI's response as bash commands.
        /// The response is streamed to the console, with special handling for verbose mode and spinner display.
        /// </summary>
        /// <param name="question">The user's bash-related question.</param>
        /// <param name="model">The AI model to use.</param>
        /// <param name="context">Optional context to provide to the AI.</param>
        public async Task AskBashQuestion(string question, string model, string context)
        {
            LogVerbose(context, question);
            await SmartHelpAsync(
                question,
                model,
                context,
                "You are an expert on Linux systems and bash commands. Help the user write a bash command. Only output bash command or commands. Do not explain yourself.",
                "Bash"
            );
        }

        /// <summary>
        /// Asks a general question and streams the AI's response as text.
        /// The response is streamed to the console, with special handling for verbose mode and spinner display.
        /// </summary>
        /// <param name="question">The user's general question.</param>
        /// <param name="model">The AI model to use.</param>
        /// <param name="context">Optional context to provide to the AI.</param>
        public async Task AskGeneralQuestion(string question, string model, string context)
        {
            LogVerbose(context, question);
            await SmartHelpAsync(
                question,
                model,
                context,
                "You are a helpful assistant.",
                "Text"
            );
        }

        /// <summary>
        /// Logs the context and question to the console if verbose mode is enabled.
        /// Displays context and question in separate sections for clarity.
        /// </summary>
        /// <param name="context">Optional context to display.</param>
        /// <param name="question">The question to display.</param>
        private void LogVerbose(string context, string question)
        {
            if (_settingsService.Verbose)
            {
                if (!string.IsNullOrEmpty(context))
                {
                    _styledConsole.WriteLine("---------------------Context--------------------------");
                    _styledConsole.WriteLine($"{context}");
                    _styledConsole.WriteLine("------------------------------------------------------");
                    _styledConsole.WriteLine();
                }
                _styledConsole.WriteLine("---------------------Question-------------------------");
                _styledConsole.WriteLine($"{question}");
                _styledConsole.WriteLine("------------------------------------------------------");
                _styledConsole.WriteLine();
            }
        }

        /// <summary>
        /// Handles the interaction with the AI chat completion service, sending user requests and context, and streaming the response.
        /// In verbose mode, streams the response directly. Otherwise, displays a spinner during AI thinking and suppresses output between "think" tags.
        /// </summary>
        /// <param name="userRequest">User's prompt.</param>
        /// <param name="model">Model name to use.</param>
        /// <param name="context">Optional context from piped input.</param>
        /// <param name="instructions">System instructions for the AI.</param>
        /// <param name="allowedResponseType">Type of response expected ("Bash" or "Text").</param>
        private async Task SmartHelpAsync(
            string userRequest,
            string model,
            string context,
            string instructions,
            string allowedResponseType)
        {
            // Create and configure the chat completion service for the specified model.
            IChatCompletionService _chatCompletionService = CreateChatCompletionService(model);

            // Build system prompt with user and environment details.
            Models.SystemPrompt systemPrompt = new Models.SystemPrompt
            {
                Username = Environment.UserName,
                CurrentDateTime = DateTime.Now.ToString(),
                CurrentWorkingDirectory = Environment.CurrentDirectory,
                Instructions = instructions,
                AllowedResponseType = allowedResponseType
            };

            var chatHistory = new ChatHistory($"{JsonSerializer.Serialize(systemPrompt)}");

            // Add context and initial assistant message if context is provided.
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

            // Configure prompt execution settings for bash commands.
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

            // Stream and display the AI's response.
            _styledConsole.WriteLine("---------------------Response-------------------------");

            if (_settingsService.Verbose)
            {
                // Stream the output directly to the console.
                await foreach (var chatMessageContent in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, settings))
                {
                    _styledConsole.Write(chatMessageContent.ToString());
                }
                _styledConsole.WriteLine("\n------------------------------------------------------");
                _styledConsole.WriteLine();
                return;
            }

            // In non-verbose mode, show spinner during AI "thinking" and suppress output between <think> tags.
            bool isThinking = false;
            StringBuilder buffer = new();
            var spinner = new Utilities.ConsoleSpinner(_styledConsole);

            await foreach (var chatMessageContent in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, settings))
            {
                string chunk = chatMessageContent.ToString();
                if (chunk.Contains("<think>"))
                {
                    isThinking = true;
                    spinner.Start();
                    continue;
                }
                if (chunk.Contains("</think>"))
                {
                    isThinking = false;
                    spinner.Stop();
                    buffer.Clear();
                    continue;
                }
                if (isThinking)
                {
                    continue;
                }
                else
                {
                    _styledConsole.Write(chunk);
                }
            }
            _styledConsole.WriteLine("\n------------------------------------------------------");
        }

        /// <summary>
        /// Creates and configures the chat completion service for the specified model and Ollama URL from settings.
        /// </summary>
        /// <param name="model">Model name to use for chat completion.</param>
        /// <returns>Configured <see cref="IChatCompletionService"/> instance for streaming AI responses.</returns>
        private IChatCompletionService CreateChatCompletionService(string model)
        {
            var services = new ServiceCollection();
            services.AddOllamaChatCompletion(model, _settingsService.OllamaUrl);
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IChatCompletionService>();
        }
    }
}
