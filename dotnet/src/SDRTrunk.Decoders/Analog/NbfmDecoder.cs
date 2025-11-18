using System.Numerics;
using SDRTrunk.DSP.Buffer;
using SDRTrunk.DSP.Filter;
using Microsoft.Extensions.Logging;

namespace SDRTrunk.Decoders.Analog;

/// <summary>
/// Narrowband FM (NBFM) decoder for analog voice communication.
/// Converted from Java io.github.dsheirer.module.decode.nbfm.NBFMDecoder
/// </summary>
public class NbfmDecoder
{
    private readonly ILogger<NbfmDecoder>? _logger;
    private readonly FirFilter _lowPassFilter;
    private Complex _previousSample = Complex.Zero;
    private readonly int _sampleRate;
    private readonly double _deviation;

    /// <summary>
    /// Creates a new NBFM decoder
    /// </summary>
    /// <param name="sampleRate">Input sample rate in Hz</param>
    /// <param name="deviation">FM deviation in Hz (typically 2500-5000 for NBFM)</param>
    /// <param name="logger">Optional logger</param>
    public NbfmDecoder(int sampleRate, double deviation = 2500.0, ILogger<NbfmDecoder>? logger = null)
    {
        _sampleRate = sampleRate;
        _deviation = deviation;
        _logger = logger;

        // Create low-pass filter for audio output (typically 3-4 kHz for voice)
        var audioFilterCutoff = 4000.0; // 4 kHz
        var numTaps = 51;
        _lowPassFilter = FirFilter.CreateLowPass(sampleRate, audioFilterCutoff, numTaps);

        _logger?.LogInformation("NBFM Decoder initialized: SampleRate={SampleRate}Hz, Deviation={Deviation}Hz",
            sampleRate, deviation);
    }

    /// <summary>
    /// Decode complex samples to audio samples using FM demodulation
    /// </summary>
    /// <param name="buffer">Buffer of complex I/Q samples</param>
    /// <returns>Demodulated audio samples</returns>
    public float[] Decode(ComplexSampleBuffer buffer)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));

        var samples = buffer.Samples;
        var audioSamples = new float[samples.Length];

        // FM demodulation using phase difference (polar discriminator)
        for (int i = 0; i < samples.Length; i++)
        {
            // Normalize the sample (automatic gain control)
            var current = samples[i];
            var magnitude = current.Magnitude;

            if (magnitude > 0)
            {
                current = new Complex(current.Real / magnitude, current.Imaginary / magnitude);
            }
            else
            {
                current = Complex.Zero;
            }

            // Calculate phase difference
            // FM demodulation: audio = d(phase)/dt
            var conjugate = Complex.Conjugate(_previousSample);
            var product = current * conjugate;

            // Extract instantaneous frequency from phase
            var angle = Math.Atan2(product.Imaginary, product.Real);

            // Convert phase difference to audio sample
            // Scale by sample rate and deviation
            audioSamples[i] = (float)(angle * _sampleRate / (2.0 * Math.PI * _deviation));

            _previousSample = current;
        }

        // Apply low-pass filter to remove high-frequency noise
        var filteredAudio = _lowPassFilter.Filter(audioSamples);

        return filteredAudio;
    }

    /// <summary>
    /// Reset the decoder state
    /// </summary>
    public void Reset()
    {
        _previousSample = Complex.Zero;
        _lowPassFilter.Reset();
        _logger?.LogDebug("NBFM Decoder reset");
    }

    /// <summary>
    /// Gets the expected sample rate for this decoder
    /// </summary>
    public int SampleRate => _sampleRate;

    /// <summary>
    /// Gets the FM deviation setting
    /// </summary>
    public double Deviation => _deviation;
}
