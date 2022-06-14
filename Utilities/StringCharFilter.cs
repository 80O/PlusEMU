using System.Text.RegularExpressions;

namespace Plus.Utilities;

internal static class StringCharFilter
{
    private static readonly Regex _allowedChars = new(@"^[a-zA-Z0-9-.]+$");
    private static readonly Regex _allowedAlphaNum = new(@"^[a-zA-Z0-9]+$");
    private static readonly Regex _scapesRegex = new(@"[\u0001-\u0008\u000B-\u000C\u000E-\u001F\u007F-\u009F]");
    private static readonly Regex _breakLinesRegex = new(@"[\r\n]");

    public static bool IsValid(string input)
    {
        return _allowedChars.IsMatch(input);
    }

    public static bool IsValidAlphaNumeric(char input)
    {
        return _allowedAlphaNum.IsMatch(input.ToString());
    }

    public static bool IsValidAlphaNumeric(string input)
    {
        return _allowedAlphaNum.IsMatch(input);
    }


    /// <summary>
    /// Escapes the characters used for injecting special chars from a user input.
    /// </summary>
    /// <param name="str">The string/text to escape.</param>
    /// <param name="allowBreaks">Allow line breaks to be used (\r\n).</param>
    /// <returns></returns>
    public static string Escape(string str, bool allowBreaks = false)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return string.Empty;
        }

        if (!allowBreaks)
        {
            str = _breakLinesRegex.Replace(str, " ");
        }

        return _scapesRegex.Replace(str, string.Empty);
    }
}