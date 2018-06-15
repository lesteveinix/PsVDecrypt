using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace rohankapoor.AutoPrompt
{
    public class AutoPrompt
    {
        #region Public APIs

        /// <summary>
        /// Used to get path from command prompt. Starts by showing available directories
        /// Press:
        ///      Up Arrow to go to next file or directory
        ///      Down Arrow to go to previous file or directory
        ///      '\' to navigate into current directory
        ///      Any key to list matching files and directory. The search criteria is 'begins with'. Eg, When 'W' is pressed in C:\, the prompt will auto fill with 'Windows' as the closest match. The search is case sensitive
        ///     Enter to return with the current value
        /// </summary>
        /// <param name="message">Message that appears on the prompt. For eg, Enter Value:</param>
        /// <returns>Full path of the file or directory selected</returns>
        public static string GetPath(string message)
        {
            string[] options = Environment.GetLogicalDrives();

            for (int optionsIndex = 0; optionsIndex < options.Length; optionsIndex++)
            {
                options[optionsIndex] = options[optionsIndex].Remove(options[optionsIndex].Length - 1);
            }

            StringBuilder result = new StringBuilder(options[0]);
            int currentOptionDisplayedIndex = 0;

            ConsoleKeyInfo KeyInfo;
            Console.Write(message + options[0]);

            KeyInfo = Console.ReadKey(true);
            while (KeyInfo.Key != ConsoleKey.Enter)
            {
                if (Char.IsLetterOrDigit(KeyInfo.KeyChar) || Char.IsSymbol(KeyInfo.KeyChar)
                    || (Char.IsPunctuation(KeyInfo.KeyChar) && KeyInfo.KeyChar != '\\')
                    || (Char.IsPunctuation(KeyInfo.KeyChar) && KeyInfo.KeyChar != '\\')
                    || Char.IsWhiteSpace(KeyInfo.KeyChar))
                {
                    Console.Write(KeyInfo.KeyChar);

                    result.Append(KeyInfo.KeyChar);

                    // Performing Closest Search
                    int foundIndex = -1;
                    int optionIndex = 0;
                    foreach (string Option in options)
                    {
                        if (Option.ToLower().StartsWith(result.ToString().ToLower()))
                        {
                            foundIndex = optionIndex;

                            // Erase old option and display new
                            EraseInput(result.Length);

                            // Display new
                            result.Clear();
                            result.Append(options[foundIndex]);
                            
                            currentOptionDisplayedIndex = foundIndex;
                            Console.Write(options[currentOptionDisplayedIndex]);

                            break;
                        }
                        optionIndex++;
                    }
                }
                else if (result.Length>0 && KeyInfo.Key == ConsoleKey.Backspace)
                {
                    // User hit back space. Erase console and input string by 1 char
                    EraseInput(1);
                    result.Remove(result.Length - 1, 1);
                }
                else if (result.Length>0 && KeyInfo.KeyChar == '\\')
                {
                    options = GetFilesDirs(result + @"\").ToArray();
                    if (options.Length>0)
                    {
                        EraseInput(result.Length);
                        Console.Write(options[0]);

                        result.Clear();
                        result.Append(options[0]);
                        currentOptionDisplayedIndex = 0;
                    }
                    else
                    {
                        // Not a directory or no files in directory. No action required
                    }
                }
                else if (result.Length>0 && KeyInfo.Key == ConsoleKey.Delete)
                {
                    // Erase everything on console and clear user input
                    EraseInput(result.Length);

                    // Clear user input
                    result.Clear();
                }
                else if (KeyInfo.Key == ConsoleKey.UpArrow || KeyInfo.Key == ConsoleKey.DownArrow)
                {
                    // Need to display next option

                    EraseInput(result.Length);

                    // Display new option
                    if (KeyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if (currentOptionDisplayedIndex > 0)
                        {
                            currentOptionDisplayedIndex--;
                        }
                        else
                        {
                            currentOptionDisplayedIndex = options.Length - 1;
                        }
                    }
                    else if (KeyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if (currentOptionDisplayedIndex < options.Length - 1)
                        {
                            currentOptionDisplayedIndex++;
                        }
                        else
                        {
                            currentOptionDisplayedIndex = 0;
                        }
                    }

                    // Show new option
                    result.Clear();
                    if (options.Length > 0)
                    {
                        Console.Write(options[currentOptionDisplayedIndex]);
                        // Set new user input
                        result.Append(options[currentOptionDisplayedIndex]);
                    }
                }

                KeyInfo = Console.ReadKey(true);
            }

            Console.WriteLine();

            return result.ToString();
        }

        /// <summary>
        /// Asks the user for input, with a default input value already set
        /// </summary>
        /// <param name="message">Message that appears on the prompt. For eg, Enter Value:</param>
        /// <param name="initialInput">The value prefilled. Eg, "22 Jump Street" prefilled if prompt is to accept address</param>
        /// <returns>User entered string or 'initialInput' if no edits were made</returns>
        public static string PromptForInput(string message, string initialInput)
        {
            StringBuilder result = new StringBuilder(initialInput);

            ConsoleKeyInfo KeyInfo;

            Console.Write(message);
            Console.Write(initialInput);

            KeyInfo = Console.ReadKey(true);
            while (KeyInfo.Key != ConsoleKey.Enter)
            {
                if (Char.IsLetterOrDigit(KeyInfo.KeyChar) || Char.IsSymbol(KeyInfo.KeyChar)
                    || Char.IsPunctuation(KeyInfo.KeyChar) || Char.IsPunctuation(KeyInfo.KeyChar)
                    || Char.IsWhiteSpace(KeyInfo.KeyChar))
                {
                    // If its a Letter or Digit, echo to screen, add to Input String
                    Console.Write(KeyInfo.KeyChar);

                    result.Append(KeyInfo.KeyChar);
                }
                else if (result.Length>0 && KeyInfo.Key == ConsoleKey.Backspace)
                {
                    // User hit back space. Erase console and input string by 1 char
                    EraseInput(1);
                    result.Remove(result.Length - 1, 1);
                }

                // Getting a next key from console
                KeyInfo = Console.ReadKey(true);
            }
            Console.WriteLine();

            return result.ToString().Trim();
        }

        /// <summary>
        /// Accepts user input by displaying options. The input is editable as well
        /// Press:
        ///     Up Arrow to go to next option
        ///     Down Arrow to go to previous option
        ///     Any character to edit the current option
        ///     Enter to return with the current value
        /// </summary>
        /// <param name="message">Message that appears on the prompt. For eg, Enter Value:</param>
        /// <param name="options">List of values. Eg {Mr, Mrs, Ms} can be passed if prompt if for salutation. User can switch between these values by pressing up/down arrow OR enter something like Dr or Er and hit enter</param>
        /// <returns>User entered string or one of the string in 'options' choosen by the user</returns>
        public static string PromptForInput(string message, string[] options)
        {
            return PromptForInput(message, options, true);
        }

        /// <summary>
        /// Accepts user input by displaying options. The input is not editable and the user must choose among the options available
        /// Press:
        ///     Up Arrow to go to next option
        ///     Down Arrow to go to previous option
        ///     Enter to return with the current value
        /// </summary>
        /// <param name="message">Message that appears on the prompt. For eg, Enter Value:</param>
        /// <param name="options">List of values. Eg {Mr, Mrs, Ms} can be passed if prompt is for salutation. User can switch between these values by pressing up/down arrow or type another value if edits are allowed</param>
        /// <param name="allowEdits">If set to false, keys for characters, backspace, delete will not work. Only Up down can be used to select the options</param>
        /// <returns>User entered string or one of the string in 'options' choosen by the user</returns>
        public static string PromptForInput(string message, string[] options, bool allowEdits)
        {
            int CurrentOptionDisplayedIndex = 0;
            StringBuilder result = new StringBuilder(options[0]);

            ConsoleKeyInfo KeyInfo;

            Console.Write(message + options[0]);

            KeyInfo = Console.ReadKey(true);
            while (KeyInfo.Key != ConsoleKey.Enter)
            {
                if ((Char.IsLetterOrDigit(KeyInfo.KeyChar) || Char.IsSymbol(KeyInfo.KeyChar)
                    || Char.IsPunctuation(KeyInfo.KeyChar) || Char.IsPunctuation(KeyInfo.KeyChar)
                    || Char.IsWhiteSpace(KeyInfo.KeyChar))
                    && allowEdits)
                {
                    Console.Write(KeyInfo.KeyChar);

                    result.Append(KeyInfo.KeyChar);
                }
                else if (result.Length > 0 && KeyInfo.Key == ConsoleKey.Backspace && allowEdits)
                {
                    // User hit back space. Erase console and input string by 1 char
                    EraseInput(1);
                    result.Remove(result.Length - 1, 1);
                }
                else if (KeyInfo.Key == ConsoleKey.UpArrow || KeyInfo.Key == ConsoleKey.DownArrow)
                {
                    // We have to display another option

                    // Erase on screen and in UserInput string
                    EraseInput(result.Length);

                    // Display new option
                    if (KeyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if (CurrentOptionDisplayedIndex > 0)
                        {
                            CurrentOptionDisplayedIndex--;
                        }
                        else
                        {
                            CurrentOptionDisplayedIndex = options.Length - 1;
                        }
                    }
                    else if (KeyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if (CurrentOptionDisplayedIndex < options.Length - 1)
                        {
                            CurrentOptionDisplayedIndex++;
                        }
                        else
                        {
                            CurrentOptionDisplayedIndex = 0;
                        }
                    }

                    // Show the new option
                    Console.Write(options[CurrentOptionDisplayedIndex]);

                    // Set new user input
                    result.Clear();
                    result.Append(options[CurrentOptionDisplayedIndex]);
                }
                KeyInfo = Console.ReadKey(true);
            }
            Console.WriteLine();

            return result.ToString().Trim();
        }

        /// <summary>
        /// Accepts user input by displaying options. The options are autofilled with matches from key press. This is like auto suggest for a text box
        /// Press:
        ///     Up Arrow to go to next option
        ///     Down Arrow to go to previous option
        ///     Any key to list closest matching option. The search criteria is 'begins with'. Eg, When 'C' is pressed and options is a list of US states, then 'California' is set to as choosen option. The search is case sensitive
        ///     Enter to return with the current value
        /// </summary>
        /// <param name="message">Message that appears on the prompt. For eg, Enter Value:</param>
        /// <param name="options">Input options that the user can choose from using up-down arrow or type to get to</param>
        /// <returns>User entered string or one of the string in 'options' choosen by the user</returns>
        public static string PromptForInput_Searchable(string message, string[] options)
        {
            StringBuilder result = new StringBuilder(options[0]);
            int CurrentOptionDisplayedIndex = 0;

            ConsoleKeyInfo KeyInfo;

            Console.Write(message + options[0]);

            KeyInfo = Console.ReadKey(true);
            while (KeyInfo.Key != ConsoleKey.Enter)
            {

                if (Char.IsLetterOrDigit(KeyInfo.KeyChar) || Char.IsSymbol(KeyInfo.KeyChar)
                    || Char.IsPunctuation(KeyInfo.KeyChar) || Char.IsPunctuation(KeyInfo.KeyChar)
                    || Char.IsWhiteSpace(KeyInfo.KeyChar))
                {
                    Console.Write(KeyInfo.KeyChar);

                    result.Append(KeyInfo.KeyChar);

                    // Performing Closest Search
                    int foundIndex = -1;
                    int OptionIndex = 0;
                    foreach (string Option in options)
                    {
                        if (Option.ToLower().StartsWith(result.ToString().ToLower()))
                        {
                            foundIndex = OptionIndex;

                            // Erase displayed option and show new option
                            EraseInput(result.Length);

                            result.Clear();
                            result.Append(options[foundIndex]);
                            CurrentOptionDisplayedIndex = foundIndex;

                            Console.Write(options[CurrentOptionDisplayedIndex]);

                            break;
                        }
                        OptionIndex++;
                    }
                    // Closest search ends
                }
                else if (result.Length>0 && KeyInfo.Key == ConsoleKey.Backspace)
                {
                    // User hit back space. Erase console and input string by 1 char
                    EraseInput(1);
                    result.Remove(result.Length - 1, 1);
                }
                else if (result.Length > 0 && KeyInfo.Key == ConsoleKey.Delete)
                {
                    // Erase everything and set user input to empty
                    EraseInput(result.Length);
                    result.Clear();
                }
                else if (KeyInfo.Key == ConsoleKey.UpArrow || KeyInfo.Key == ConsoleKey.DownArrow)
                {
                    // Remove current option and Display another option

                    EraseInput(result.Length);

                    // Display new option
                    if (KeyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if (CurrentOptionDisplayedIndex > 0)
                            CurrentOptionDisplayedIndex--;
                        else
                            CurrentOptionDisplayedIndex = options.Length - 1;

                    }
                    else if (KeyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if (CurrentOptionDisplayedIndex < options.Length - 1)
                            CurrentOptionDisplayedIndex++;
                        else
                            CurrentOptionDisplayedIndex = 0;
                    }

                    // Show new option
                    Console.Write(options[CurrentOptionDisplayedIndex]);

                    // Set new user input
                    result.Clear();
                    result.Append(options[CurrentOptionDisplayedIndex]);
                }

                KeyInfo = Console.ReadKey(true);
            }
            Console.WriteLine();

            return result.ToString();
        }

        /// <summary>
        /// Gets a password without displaying it on screen. Input is masked by *s
        /// </summary>
        /// <param name="message">Message that appears on the prompt. For eg, Enter Value:</param>
        /// <returns>The password entered by the user</returns>
        public static string GetPassword(string message)
        {
            StringBuilder result = new StringBuilder();

            Console.Write(message);

            ConsoleKeyInfo KeyInfo;

            KeyInfo = Console.ReadKey(true);
            while (KeyInfo.Key != ConsoleKey.Enter)
            {
                if (Char.IsLetterOrDigit(KeyInfo.KeyChar))
                {
                    // If its a Letter or Digit, echo * to screen, add to Input String
                    Console.Write("*");
                    result.Append(KeyInfo.KeyChar);
                }
                else if (result.Length>0 && KeyInfo.Key == ConsoleKey.Backspace)
                {
                    // User hit back space. Erase console and input string by 1 char
                    EraseInput(1);
                    result.Remove(result.Length - 1, 1);
                }

                KeyInfo = Console.ReadKey(true);
            }
            Console.WriteLine();

            return result.ToString();
        }

        #endregion

        #region Utils

        private static void EraseInput(int lengthToErase)
        {
            for (int i = 0; i<lengthToErase; i++)
            {
                Console.Write("\b" + " " + "\b");
            }
        }

        private static List<string> GetFilesDirs(string userInput)
        {
            List<string> result = new List<string>();
            if (Directory.Exists(userInput))
            {
                try
                {
                    var directories = Directory.GetDirectories(userInput);
                    result.AddRange(directories);
                    var files = Directory.GetFiles(userInput);
                    result.AddRange(files);
                }
                catch (Exception e)
                {

                }
            }

            return result;
        }

        #endregion
    }
}
