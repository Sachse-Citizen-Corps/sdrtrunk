using System.Numerics;

namespace SDRTrunk.DSP.Buffer;

/// <summary>
/// Buffer for complex (I/Q) samples from SDR devices.
/// Provides storage and access to complex sample data.
/// </summary>
public class ComplexSampleBuffer
{
    private readonly Complex[] _samples;
    private readonly long _timestamp;
    private int _sampleRate;

    /// <summary>
    /// Creates a new complex sample buffer
    /// </summary>
    /// <param name="samples">Array of complex samples</param>
    /// <param name="timestamp">Timestamp for this buffer</param>
    /// <param name="sampleRate">Sample rate in Hz</param>
    public ComplexSampleBuffer(Complex[] samples, long timestamp, int sampleRate)
    {
        _samples = samples ?? throw new ArgumentNullException(nameof(samples));
        _timestamp = timestamp;
        _sampleRate = sampleRate;
    }

    /// <summary>
    /// Gets the complex samples
    /// </summary>
    public Complex[] Samples => _samples;

    /// <summary>
    /// Gets the number of samples in this buffer
    /// </summary>
    public int Length => _samples.Length;

    /// <summary>
    /// Gets the timestamp for this buffer
    /// </summary>
    public long Timestamp => _timestamp;

    /// <summary>
    /// Gets or sets the sample rate in Hz
    /// </summary>
    public int SampleRate
    {
        get => _sampleRate;
        set => _sampleRate = value;
    }

    /// <summary>
    /// Get a sample at a specific index
    /// </summary>
    public Complex this[int index] => _samples[index];

    /// <summary>
    /// Create a new buffer from I and Q float arrays
    /// </summary>
    public static ComplexSampleBuffer FromIQ(float[] i, float[] q, long timestamp, int sampleRate)
    {
        if (i == null) throw new ArgumentNullException(nameof(i));
        if (q == null) throw new ArgumentNullException(nameof(q));
        if (i.Length != q.Length)
            throw new ArgumentException("I and Q arrays must have the same length");

        var samples = new Complex[i.Length];
        for (int idx = 0; idx < i.Length; idx++)
        {
            samples[idx] = new Complex(i[idx], q[idx]);
        }

        return new ComplexSampleBuffer(samples, timestamp, sampleRate);
    }

    /// <summary>
    /// Create a new buffer from interleaved I/Q data
    /// </summary>
    public static ComplexSampleBuffer FromInterleaved(float[] interleaved, long timestamp, int sampleRate)
    {
        if (interleaved == null) throw new ArgumentNullException(nameof(interleaved));
        if (interleaved.Length % 2 != 0)
            throw new ArgumentException("Interleaved array must have even length");

        var samples = new Complex[interleaved.Length / 2];
        for (int idx = 0; idx < samples.Length; idx++)
        {
            samples[idx] = new Complex(interleaved[idx * 2], interleaved[idx * 2 + 1]);
        }

        return new ComplexSampleBuffer(samples, timestamp, sampleRate);
    }

    /// <summary>
    /// Extract I (real) values
    /// </summary>
    public float[] GetI()
    {
        var result = new float[_samples.Length];
        for (int i = 0; i < _samples.Length; i++)
        {
            result[i] = (float)_samples[i].Real;
        }
        return result;
    }

    /// <summary>
    /// Extract Q (imaginary) values
    /// </summary>
    public float[] GetQ()
    {
        var result = new float[_samples.Length];
        for (int i = 0; i < _samples.Length; i++)
        {
            result[i] = (float)_samples[i].Imaginary;
        }
        return result;
    }

    /// <summary>
    /// Calculate magnitude of samples
    /// </summary>
    public float[] GetMagnitude()
    {
        var result = new float[_samples.Length];
        for (int i = 0; i < _samples.Length; i++)
        {
            result[i] = (float)_samples[i].Magnitude;
        }
        return result;
    }

    /// <summary>
    /// Calculate phase of samples
    /// </summary>
    public float[] GetPhase()
    {
        var result = new float[_samples.Length];
        for (int i = 0; i < _samples.Length; i++)
        {
            result[i] = (float)_samples[i].Phase;
        }
        return result;
    }
}
