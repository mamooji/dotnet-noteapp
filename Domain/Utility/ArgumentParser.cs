namespace Domain.Utility;

public class ArgumentParser
{
    public string GetArgumentValue(string flag, string[] args)
    {
        var fields = args.Where(x => x.StartsWith(flag, StringComparison.InvariantCultureIgnoreCase)).ToList();

        return fields.Count == 1 ? fields.First().Substring(flag.Length).Trim() : string.Empty;
    }

    public bool FlagExists(string flag, string[] args)
    {
        return args.Any(x => x.Equals(flag, StringComparison.CurrentCultureIgnoreCase));
    }
}