namespace SDRTrunk.Tuners.Exceptions;

/// <summary>
/// Exception thrown when tuner operations fail
/// </summary>
public class TunerException : Exception
{
    public TunerException() : base()
    {
    }

    public TunerException(string message) : base(message)
    {
    }

    public TunerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when tuner is not found or cannot be opened
/// </summary>
public class TunerNotFoundException : TunerException
{
    public TunerNotFoundException() : base("Tuner device not found")
    {
    }

    public TunerNotFoundException(string message) : base(message)
    {
    }

    public TunerNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when tuner communication fails
/// </summary>
public class TunerCommunicationException : TunerException
{
    public TunerCommunicationException() : base("Tuner communication failed")
    {
    }

    public TunerCommunicationException(string message) : base(message)
    {
    }

    public TunerCommunicationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
