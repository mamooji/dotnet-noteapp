using FluentValidation.Results;

namespace Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        var failureGroups = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage);

        foreach (var failureGroup in failureGroups)
        {
            var propertyName = failureGroup.Key;
            var propertyFailures = failureGroup.ToArray();

            Errors.Add(propertyName, propertyFailures);
        }
    }

    public IDictionary<string, string[]> Errors { get; }

    public override string ToString()
    {
        var errorString = "";

        foreach (var keyValuePair in Errors)
        {
            errorString += string.Join(", ", keyValuePair.Value);
            errorString += "\n";
        }

        return base.ToString() + errorString;
    }

    public static List<ValidationFailure> GetReason(string property, string message)
    {
        var reason = new ValidationFailure(property, message);
        var reasons = new List<ValidationFailure> { reason };
        return reasons;
    }
}