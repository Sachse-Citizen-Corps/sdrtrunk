using System.Numerics;

namespace SDRTrunk.DSP.Filter;

/// <summary>
/// Finite Impulse Response (FIR) filter for signal processing.
/// Converted from Java DSP filter implementations.
/// </summary>
public class FirFilter
{
    private readonly float[] _coefficients;
    private readonly float[] _buffer;
    private int _bufferIndex;

    /// <summary>
    /// Creates a new FIR filter with the specified coefficients
    /// </summary>
    /// <param name="coefficients">Filter coefficients</param>
    public FirFilter(float[] coefficients)
    {
        _coefficients = coefficients ?? throw new ArgumentNullException(nameof(coefficients));
        _buffer = new float[coefficients.Length];
        _bufferIndex = 0;
    }

    /// <summary>
    /// Gets the filter order (number of coefficients)
    /// </summary>
    public int Order => _coefficients.Length;

    /// <summary>
    /// Process a single sample through the filter
    /// </summary>
    /// <param name="sample">Input sample</param>
    /// <returns>Filtered output</returns>
    public float Filter(float sample)
    {
        // Add new sample to circular buffer
        _buffer[_bufferIndex] = sample;
        _bufferIndex = (_bufferIndex + 1) % _buffer.Length;

        // Compute convolution
        float result = 0;
        int idx = _bufferIndex;
        for (int i = 0; i < _coefficients.Length; i++)
        {
            idx = (idx == 0) ? _buffer.Length - 1 : idx - 1;
            result += _coefficients[i] * _buffer[idx];
        }

        return result;
    }

    /// <summary>
    /// Process an array of samples
    /// </summary>
    /// <param name="samples">Input samples</param>
    /// <returns>Filtered samples</returns>
    public float[] Filter(float[] samples)
    {
        if (samples == null) throw new ArgumentNullException(nameof(samples));

        var result = new float[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            result[i] = Filter(samples[i]);
        }
        return result;
    }

    /// <summary>
    /// Process complex samples (filters both I and Q components)
    /// </summary>
    public Complex[] Filter(Complex[] samples)
    {
        if (samples == null) throw new ArgumentNullException(nameof(samples));

        var result = new Complex[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            var filteredReal = Filter((float)samples[i].Real);
            var filteredImag = Filter((float)samples[i].Imaginary);
            result[i] = new Complex(filteredReal, filteredImag);
        }
        return result;
    }

    /// <summary>
    /// Reset the filter state
    /// </summary>
    public void Reset()
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        _bufferIndex = 0;
    }

    /// <summary>
    /// Create a low-pass filter
    /// </summary>
    /// <param name="sampleRate">Sample rate in Hz</param>
    /// <param name="cutoffFreq">Cutoff frequency in Hz</param>
    /// <param name="numTaps">Number of filter taps</param>
    /// <returns>Low-pass FIR filter</returns>
    public static FirFilter CreateLowPass(int sampleRate, double cutoffFreq, int numTaps)
    {
        if (numTaps % 2 == 0)
            numTaps++; // Ensure odd number of taps for symmetry

        var coefficients = new float[numTaps];
        var normalizedCutoff = cutoffFreq / sampleRate;
        var center = (numTaps - 1) / 2.0;

        // Generate sinc function coefficients
        for (int i = 0; i < numTaps; i++)
        {
            if (i == center)
            {
                coefficients[i] = (float)(2.0 * normalizedCutoff);
            }
            else
            {
                var x = i - center;
                coefficients[i] = (float)(Math.Sin(2.0 * Math.PI * normalizedCutoff * x) / (Math.PI * x));
            }

            // Apply Hamming window
            coefficients[i] *= (float)(0.54 - 0.46 * Math.Cos(2.0 * Math.PI * i / (numTaps - 1)));
        }

        // Normalize coefficients
        var sum = coefficients.Sum();
        for (int i = 0; i < numTaps; i++)
        {
            coefficients[i] /= sum;
        }

        return new FirFilter(coefficients);
    }

    /// <summary>
    /// Create a high-pass filter
    /// </summary>
    public static FirFilter CreateHighPass(int sampleRate, double cutoffFreq, int numTaps)
    {
        // Create low-pass and invert
        var lowPass = CreateLowPass(sampleRate, cutoffFreq, numTaps);
        var coefficients = lowPass._coefficients;

        // Spectral inversion
        for (int i = 0; i < coefficients.Length; i++)
        {
            coefficients[i] = -coefficients[i];
        }

        // Add 1.0 to center tap
        coefficients[coefficients.Length / 2] += 1.0f;

        return new FirFilter(coefficients);
    }

    /// <summary>
    /// Create a band-pass filter
    /// </summary>
    public static FirFilter CreateBandPass(int sampleRate, double lowCutoff, double highCutoff, int numTaps)
    {
        var lowPass = CreateLowPass(sampleRate, highCutoff, numTaps);
        var highPass = CreateHighPass(sampleRate, lowCutoff, numTaps);

        // Convolve the two filters
        var coefficients = new float[numTaps];
        for (int i = 0; i < numTaps; i++)
        {
            for (int j = 0; j < numTaps; j++)
            {
                if (i - j >= 0 && i - j < numTaps)
                {
                    coefficients[i] += lowPass._coefficients[j] * highPass._coefficients[i - j];
                }
            }
        }

        return new FirFilter(coefficients);
    }
}
