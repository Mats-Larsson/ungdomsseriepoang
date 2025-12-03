using System.Text.RegularExpressions;

namespace Results;

public partial class ClassFilter
{
    private readonly HashSet<string> include;
    private readonly HashSet<string> exclude;
    private readonly Regex standardClassesRegex;

    [GeneratedRegex("^(H1[0246]|D1[0246]|U[1-4]|Insk)[0-9A-ZÅÄÖ ]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "sv-SE")]
    private static partial Regex StandardClassesRegex();
    
    public ClassFilter(Configuration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        include = (configuration.IncludeClasses ?? new HashSet<string>()).Select(c => c.ToUpperInvariant()).ToHashSet();
        exclude = (configuration.ExcludeClasses ?? new HashSet<string>()).Select(c => c.ToUpperInvariant()).ToHashSet();

        standardClassesRegex = StandardClassesRegex();
    }

    public bool IsIncluded(string className)
    {
        if (string.IsNullOrEmpty(className)) return false;
        var upper = className.ToUpperInvariant();
        return !exclude.Contains(upper) && (include.Contains(upper) || standardClassesRegex.IsMatch(className));
    }

}
