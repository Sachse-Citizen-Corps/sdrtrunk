namespace SDRTrunk.Tuners.RtlSdr;

/// <summary>
/// Constants for RTL-SDR USB communication
/// Based on librtlsdr library
/// </summary>
public static class RtlSdrConstants
{
    // USB Vendor and Product IDs for RTL2832U devices
    public const int VendorId = 0x0bda;  // Realtek

    public static readonly (int VendorId, int ProductId, string Name)[] SupportedDevices =
    {
        (0x0bda, 0x2832, "Generic RTL2832U"),
        (0x0bda, 0x2838, "Generic RTL2832U OEM"),
        (0x0413, 0x6680, "DigitalNow Quad DVB-T PCI-E card"),
        (0x0413, 0x6f0f, "Leadtek WinFast DTV Dongle mini D"),
        (0x0458, 0x707f, "Genius TVGo DVB-T03 USB dongle"),
        (0x0ccd, 0x00a9, "Terratec Cinergy T Stick Black"),
        (0x0ccd, 0x00b3, "Terratec NOXON DAB/DAB+ USB dongle"),
        (0x0ccd, 0x00d7, "Terratec T Stick PLUS"),
        (0x0ccd, 0x00e0, "Terratec NOXON DAB/DAB+ USB dongle"),
        (0x1554, 0x5020, "PixelView PV-DT235U(RN)"),
        (0x15f4, 0x0131, "HanfTek DAB+FM+DVB-T"),
        (0x185b, 0x0620, "Compro Videomate U620F"),
        (0x185b, 0x0650, "Compro Videomate U650F"),
        (0x1b80, 0xd393, "GIGABYTE GT-U7300"),
        (0x1b80, 0xd394, "DEXATEK Technology Ltd."),
        (0x1b80, 0xd395, "Peak 102569AGPK"),
        (0x1b80, 0xd397, "KWorld KW-UB450-T USB DVB-T Pico TV"),
        (0x1b80, 0xd398, "Zaapa ZT-MINDVBZP"),
        (0x1b80, 0xd39d, "SVEON STV20"),
        (0x1d19, 0x1101, "Dexatek DK DVB-T Dongle"),
        (0x1d19, 0x1102, "Dexatek DK DVB-T Dongle"),
        (0x1d19, 0x1103, "Dexatek Technology Ltd."),
        (0x1f4d, 0xa803, "Sweex DVB-T USB"),
        (0x1f4d, 0xb803, "GTek T803"),
        (0x1f4d, 0xc803, "Lifeview LV5TDeluxe"),
        (0x1f4d, 0xd286, "MyGica TD312"),
        (0x1f4d, 0xd803, "PROlectrix DV107669")
    };

    // RTL2832U USB control commands
    public const byte CTRL_IN = 0xC0;
    public const byte CTRL_OUT = 0x40;

    // USB control request types
    public const byte USB_SYSCTL = 0x2000;
    public const byte USB_CTRL = 0x2001;
    public const byte USB_BULK = 0x2002;

    // Register blocks
    public const byte BLOCK_DEMOD = 0;
    public const byte BLOCK_USB = 1;
    public const byte BLOCK_SYS = 2;

    // Sample rates
    public const int DEFAULT_SAMPLE_RATE = 2048000;  // 2.048 MHz
    public const int MIN_SAMPLE_RATE = 225001;       // 225 kHz
    public const int MAX_SAMPLE_RATE = 3200000;      // 3.2 MHz

    // Frequency range
    public const long MIN_FREQUENCY_HZ = 24000000;     // 24 MHz
    public const long MAX_FREQUENCY_HZ = 1766000000;   // 1766 MHz

    // Gain values (in tenths of dB)
    public static readonly int[] SupportedGains =
    {
        0, 9, 14, 27, 37, 77, 87, 125, 144, 157,
        166, 197, 207, 229, 254, 280, 297, 328,
        338, 364, 372, 386, 402, 421, 434, 439,
        445, 480, 496
    };

    // Tuner types
    public enum TunerChip
    {
        Unknown = 0,
        E4000 = 1,
        FC0012 = 2,
        FC0013 = 3,
        FC2580 = 4,
        R820T = 5,
        R828D = 6
    }

    // USB endpoint for bulk transfer (sample data)
    public const byte BULK_ENDPOINT = 0x81;

    // Buffer sizes
    public const int DEFAULT_BUFFER_LENGTH = 16384;  // 16KB
    public const int BUFFER_COUNT = 15;
}
