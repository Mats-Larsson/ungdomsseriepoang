using System.Text.RegularExpressions;

namespace Results;

public class ClassFilter
{
    private readonly Configuration configuration;
    private readonly ISet<string> include;
    private readonly ISet<string> exclude;
    private readonly Regex regex;

    public ClassFilter(Configuration configuration)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        include = (configuration.IncludeClasses ?? new HashSet<string>()).Select(c => c.ToUpperInvariant()).ToHashSet();
        exclude = (configuration.ExcludeClasses ?? new HashSet<string>()).Select(c => c.ToUpperInvariant()).ToHashSet();

        regex = new Regex("^(H1[0246]|D1[0246]|U[1-4]|Insk)[A-ZÅÄÖ]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    public bool IsIncluded(string className)
    {
        if (className == null) return false;
        var upper = className.ToUpperInvariant();
        return !exclude.Contains(upper) && (include.Contains(upper) || regex.IsMatch(className));
    }
}
