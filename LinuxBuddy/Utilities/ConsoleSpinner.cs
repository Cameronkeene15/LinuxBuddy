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
        /// The background task responsible for updating the spinner.
        /// </summary>
        private Task? _task;

        /// <summary>
        /// Starts the spinner animation in the console.
        /// </summary>
        /// <remarks>
        /// The spinner runs asynchronously and updates every 100 milliseconds.
        /// </remarks>
        public void Start()
        {
            _active = true;
            _task = Task.Run(async () =>
            {
                while (_active)
                {
                    Console.Write($"\rThinking... {_sequence[_counter++ % _sequence.Length]}");
                    await Task.Delay(100);
                }
                // Clear the spinner line when stopping
                Console.Write("\r                     \r");
            });
        }

        /// <summary>
        /// Stops the spinner animation and waits for the background task to complete.
        /// </summary>
        public void Stop()
        {
            _active = false;
            _task?.Wait();
        }
    }
}
