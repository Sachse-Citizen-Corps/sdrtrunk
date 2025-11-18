using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;
using SDRTrunk.Models;

namespace SDRTrunk.App.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private string _statusMessage = "Ready";
    private string _tunerStatus = "No tuner connected";
    private string _decoderStatus = "No channels active";
    private Channel? _selectedChannel;

    public MainWindowViewModel()
    {
        Channels = new ObservableCollection<Channel>
        {
            new Channel
            {
                Name = "Channel 1",
                Frequency = "154.0000 MHz",
                DecoderType = "NBFM",
                Bandwidth = "12.5 kHz",
                IsActive = false
            },
            new Channel
            {
                Name = "Channel 2",
                Frequency = "460.5000 MHz",
                DecoderType = "P25",
                Bandwidth = "12.5 kHz",
                IsActive = false
            }
        };

        // Initialize commands
        OpenPlaylistCommand = ReactiveCommand.Create(OnOpenPlaylist);
        SavePlaylistCommand = ReactiveCommand.Create(OnSavePlaylist);
        ExitCommand = ReactiveCommand.Create(OnExit);
        ShowSpectrumCommand = ReactiveCommand.Create(OnShowSpectrum);
        ShowChannelEventsCommand = ReactiveCommand.Create(OnShowChannelEvents);
        ShowPreferencesCommand = ReactiveCommand.Create(OnShowPreferences);
        ShowTunerManagerCommand = ReactiveCommand.Create(OnShowTunerManager);
        ShowAboutCommand = ReactiveCommand.Create(OnShowAbout);
        StartChannelCommand = ReactiveCommand.Create(OnStartChannel, this.WhenAnyValue(x => x.CanStartChannel));
        StopChannelCommand = ReactiveCommand.Create(OnStopChannel, this.WhenAnyValue(x => x.CanStopChannel));
    }

    public ObservableCollection<Channel> Channels { get; }

    public Channel? SelectedChannel
    {
        get => _selectedChannel;
        set => this.RaiseAndSetIfChanged(ref _selectedChannel, value);
    }

    public bool ChannelSelected => SelectedChannel != null;

    public bool CanStartChannel => SelectedChannel != null && !SelectedChannel.IsActive;

    public bool CanStopChannel => SelectedChannel != null && SelectedChannel.IsActive;

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public string TunerStatus
    {
        get => _tunerStatus;
        set => this.RaiseAndSetIfChanged(ref _tunerStatus, value);
    }

    public string DecoderStatus
    {
        get => _decoderStatus;
        set => this.RaiseAndSetIfChanged(ref _decoderStatus, value);
    }

    // Commands
    public ICommand OpenPlaylistCommand { get; }
    public ICommand SavePlaylistCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand ShowSpectrumCommand { get; }
    public ICommand ShowChannelEventsCommand { get; }
    public ICommand ShowPreferencesCommand { get; }
    public ICommand ShowTunerManagerCommand { get; }
    public ICommand ShowAboutCommand { get; }
    public ICommand StartChannelCommand { get; }
    public ICommand StopChannelCommand { get; }

    private void OnOpenPlaylist()
    {
        StatusMessage = "Opening playlist...";
    }

    private void OnSavePlaylist()
    {
        StatusMessage = "Saving playlist...";
    }

    private void OnExit()
    {
        System.Environment.Exit(0);
    }

    private void OnShowSpectrum()
    {
        StatusMessage = "Showing spectrum display...";
    }

    private void OnShowChannelEvents()
    {
        StatusMessage = "Showing channel events...";
    }

    private void OnShowPreferences()
    {
        StatusMessage = "Opening preferences...";
    }

    private void OnShowTunerManager()
    {
        StatusMessage = "Opening tuner manager...";
    }

    private void OnShowAbout()
    {
        StatusMessage = "About SDRTrunk .NET";
    }

    private void OnStartChannel()
    {
        if (SelectedChannel != null)
        {
            SelectedChannel.IsActive = true;
            StatusMessage = $"Started channel: {SelectedChannel.Name}";
            this.RaisePropertyChanged(nameof(CanStartChannel));
            this.RaisePropertyChanged(nameof(CanStopChannel));
        }
    }

    private void OnStopChannel()
    {
        if (SelectedChannel != null)
        {
            SelectedChannel.IsActive = false;
            StatusMessage = $"Stopped channel: {SelectedChannel.Name}";
            this.RaisePropertyChanged(nameof(CanStartChannel));
            this.RaisePropertyChanged(nameof(CanStopChannel));
        }
    }
}
