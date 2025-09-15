namespace LinuxBuddy.Utilities
{
    /// <summary>
    /// Provides a simple console spinner animation to indicate ongoing processing.
    /// </summary>
    public class ConsoleSpinner
    {
        /// <summary>
        /// The sequence of characters used to display the spinner animation.
        /// </summary>
        private readonly char[] _sequence = new[] { '|', '/', '-', '\\' };

        /// <summary>
        /// Tracks the current position in the spinner sequence.
        /// </summary>
        private int _counter = 0;

        /// <summary>
        /// Indicates whether the spinner is currently active.
        /// </summary>
        private bool _active = false;

        /// <summary>
        /// The background task responsible for updating the spinner animation.
        /// </summary>
        private Task? _task;

        /// <summary>
        /// Provides styled console output for the spinner.
        /// </summary>
        private readonly StyledConsole _styledConsole;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleSpinner"/> class using the specified <see cref="StyledConsole"/>.
        /// </summary>
        /// <param name="styledConsole">The styled console used for output.</param>
        public ConsoleSpinner(StyledConsole styledConsole)
        {
            _styledConsole = styledConsole;
        }

        /// <summary>
        /// Starts the spinner animation in the console asynchronously.
        /// </summary>
        /// <remarks>
        /// The spinner updates every 100 milliseconds and runs until <see cref="Stop"/> is called.
        /// </remarks>
        public void Start()
        {
            _active = true;
            _task = Task.Run(async () =>
            {
                while (_active)
                {
                    // Display the spinner character and advance the sequence.
                    _styledConsole.Write($"\rThinking... {_sequence[_counter++ % _sequence.Length]}");
                    await Task.Delay(100);
                }
                // Clear the spinner line when stopping.
                _styledConsole.Write("\r                     \r");
            });
        }

        /// <summary>
        /// Stops the spinner animation and waits for the background task to complete.
        /// </summary>
        /// <remarks>
        /// This method ensures the spinner is removed from the console after stopping.
        /// </remarks>
        public void Stop()
        {
            _active = false;
            _task?.Wait();
        }
    }
}
