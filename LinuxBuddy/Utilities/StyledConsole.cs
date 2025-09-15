namespace LinuxBuddy.Utilities
{
    /// <summary>
    /// Provides methods for writing styled text to the console using ANSI escape codes.
    /// </summary>
    public class StyledConsole
    {
        // Default bold green text formatting using ANSI escape codes.
        private readonly string _outputStyle = "\u001b[1m\u001b[32m";
        // ANSI escape code to reset text formatting.
        private readonly string _resetStyle = "\u001b[0m";

        /// <summary>
        /// Initializes a new instance of the <see cref="StyledConsole"/> class with the default style.
        /// </summary>
        public StyledConsole(){}

        /// <summary>
        /// Initializes a new instance of the <see cref="StyledConsole"/> class with a custom output style.
        /// </summary>
        /// <param name="outputStyle">The ANSI escape code string to use for styling console output.</param>
        public StyledConsole(string outputStyle)
        {
            _outputStyle = outputStyle;
        }

        /// <summary>
        /// Writes an empty line to the console.
        /// </summary>
        public void WriteLine()
        {
            Console.WriteLine();
        }

        /// <summary>
        /// Writes a styled line of text to the console, followed by a newline.
        /// </summary>
        /// <param name="text">The text to write to the console.</param>
        public void WriteLine(string text)
        {
            Console.WriteLine($"{_outputStyle}{text}{_resetStyle}");
        }

        /// <summary>
        /// Writes styled text to the console without a trailing newline.
        /// </summary>
        /// <param name="text">The text to write to the console.</param>
        public void Write(string text)
        {
            Console.Write($"{_outputStyle}{text}{_resetStyle}");
        }
    }
}
