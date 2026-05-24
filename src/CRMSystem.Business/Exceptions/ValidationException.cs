namespace CRMSystem.Business.Exceptions;

/// <summary>
/// Thrown when business validation rules are violated.
/// </summary>
public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(string error) : base(error)
    {
        Errors = new List<string> { error };
    }

    public ValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList();
    }
}