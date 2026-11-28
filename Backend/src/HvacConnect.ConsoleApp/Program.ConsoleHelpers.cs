
namespace HvacConnect.ConsoleApp;

public static partial class Program
{
    private static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }

    private static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {message}");
        Console.ResetColor();
    }

    private static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARNING] {message}");
        Console.ResetColor();
    }

    private static string Prompt(string label)
    {
        Console.Write(label);
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    private static string PromptValidated(string label, Func<string?, (bool isValid, string errorMessage)> validator)
    {
        while (true)
        {
            Console.Write(label);
            string? input = Console.ReadLine();
            var (isValid, errorMessage) = validator(input);
            if (isValid)
            {
                return input?.Trim() ?? string.Empty;
            }

            PrintError(errorMessage);
        }
    }

    private static bool PromptYesNo(string label)
    {
        while (true)
        {
            string answer = Prompt(label);
            if (answer.Equals("y", StringComparison.OrdinalIgnoreCase)) return true;
            if (answer.Equals("n", StringComparison.OrdinalIgnoreCase)) return false;
            PrintError("Enter Y or N.");
        }
    }
}
