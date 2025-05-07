namespace PhrasePulse.Logic;

internal class PhrasePulseException : Exception
{
    public PhrasePulseException(string? message)
        : base(message) { }
    public PhrasePulseException(string? message, Exception innerException)
        : base(message, innerException) { }
}
