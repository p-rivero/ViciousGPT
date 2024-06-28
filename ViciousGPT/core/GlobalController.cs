using System.Diagnostics;
using System.Windows;
using ViciousGPT.AudioProcessing;
using ViciousGPT.Properties;

namespace ViciousGPT;

class GlobalController : IDisposable
{
    private readonly ViciousGptController controller = new();
    private readonly AudioPlayer voiceAudioPlayer = new();
    private readonly AudioPlayer sfxAudioPlayer = new();
    private readonly LoopingAudioPlayer loopAudioPlayer = new();
    private readonly Window owner;
    private readonly SubtitleWindow subtitleWindow = new();

    public GlobalController(Window owner)
    {
        this.owner = owner;
        loopAudioPlayer.Start(Resources.bard_loop);
    }

    private State state = State.Idle;

    public int DelayAfterSfx { get; set; } = 200;
    public int FadeInBackgroundLoop { get; set; } = 1000;
    public int FadeOutBackgroundLoop { get; set; } = 1000;

    public static string CharacterName
    {
        get => ViciousGptController.UserCharatcterName;
    }

    public static bool OutputLogFile => ViciousGptController.OutputLogFile;

    public async void OnTriggered()
    {
        try
        {
            switch (state)
            {
                case State.Idle:
                    new CommandRunner { Command = Settings.Default.PreRequestCommand, DelaySeconds = Settings.Default.PreRequestCommandDelay }.Start();
                    StartRecording();
                    break;
                case State.Recording:
                    await AcceptRecording();
                    new CommandRunner { Command = Settings.Default.PostResponseCommand, DelaySeconds = Settings.Default.PostResponseCommandDelay }.Start();
                    break;
            }
        } catch (Exception e)
        {
            HandleError(e);
        }
    }

    public void OnCancelled()
    {
        try
        {
            switch (state)
            {
                case State.Recording:
                    StopRecording();
                    new CommandRunner { Command = Settings.Default.OnCancelCommand, DelaySeconds = Settings.Default.OnCancelCommandDelay }.Start();
                    break;
                case State.Speaking:
                    StopSpeaking();
                    break;
            }
        }
        catch (Exception e)
        {
            HandleError(e);
        }
    }

    private void StartRecording()
    {
        _ = loopAudioPlayer.FadeIn(FadeInBackgroundLoop);
        controller.StartRecording();
        subtitleWindow.Show("Thou shall speak now.");
        subtitleWindow.Owner = owner;
        state = State.Recording;
    }

    private void StopRecording()
    {
        _ = loopAudioPlayer.FadeOut(100);
        state = State.Idle;
        controller.CancelRecording();
        subtitleWindow.Hide();
    }

    private async Task AcceptRecording()
    {
        _ = loopAudioPlayer.FadeOut(FadeOutBackgroundLoop);
        state = State.Processing;
        subtitleWindow.Show($"[{CharacterName} is thinking...]");
        var (response, audio) = await controller.AcceptRecording();
        state = State.Speaking;
        subtitleWindow.Show($"{CharacterName}: {response}");
        await PlayVoice(audio);
        subtitleWindow.Hide();
        state = State.Idle;
    }

    private async Task PlayVoice(byte[] audio)
    {
        _ = sfxAudioPlayer.Play(Resources.vicious_mockery);
        await Task.Delay(DelayAfterSfx);
        await voiceAudioPlayer.Play(audio);
    }

    private void StopSpeaking()
    {
        state = State.Idle;
        sfxAudioPlayer.Stop();
        voiceAudioPlayer.Stop();
    }

    private static void HandleError(Exception e)
    {
        MessageBox.Show("An error occurred: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        Trace.TraceError(e.ToString());
        Trace.TraceError(e.StackTrace);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            sfxAudioPlayer.Dispose();
            voiceAudioPlayer.Dispose();
            subtitleWindow.Close();
        }
    }

    private enum State
    {
        Idle,
        Recording,
        Processing,
        Speaking
    }
}
