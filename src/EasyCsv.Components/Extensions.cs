using System.Text;

namespace EasyCsv.Components;

internal static class Extensions
{
    internal static string? SplitOnCapitalLetters(this string? input)
    {
        if (input == null) return null;
        var sb = new StringBuilder();
        var i = 0;
        foreach (var character in input)
        {
            if (i == 0)
            {
                sb.Append(char.ToUpper(character));
                i++;
                continue;
            }
            if (char.IsUpper(character) && input[i - 1] != ' ')
            {
                sb.Append(' ');
            }
            sb.Append(character);
            i++;
        }
        return sb.ToString();
    }

    internal static string? Pascalize(this string? input)
    {
        if (input == null) return null;
        if (input.Length == 0) return null;
        if (char.IsUpper(input[0])) return input;
        return $"{char.ToUpper(input[0])}{input[1..]}";
    }
}