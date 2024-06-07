using System.Windows;
using ViciousGPT.AudioProcessing;

namespace ViciousGPT;

class GlobalController(Window owner) : IDisposable
{
    private readonly ViciousGptController controller = new();
    private readonly AudioPlayer audioPlayer = new();
    private readonly Window owner = owner;
    private readonly SubtitleWindow subtitleWindow = new();

    private State state = State.Idle;

    public static string CharacterName
    {
        get => ViciousGptController.UserCharatcterName;
    }

    public async void OnTriggered()
    {
        switch (state)
        {
            case State.Idle:
                StartRecording();
                break;
            case State.Recording:
                await AcceptRecording();
                break;
        }
    }

    public void OnCancelled()
    {
        switch (state)
        {
            case State.Recording:
                StopRecording();
                break;
            case State.Speaking:
                StopSpeaking();
                break;
        }
    }

    private void StartRecording()
    {
        controller.StartRecording();
        subtitleWindow.Show("Thou shall speak now.");
        subtitleWindow.Owner = owner;
        state = State.Recording;
    }

    private void StopRecording()
    {
        state = State.Idle;
        controller.CancelRecording();
        subtitleWindow.Hide();
    }

    private async Task AcceptRecording()
    {
        state = State.Processing;
        subtitleWindow.Show($"[{CharacterName} is thinking...]");
        var (response, audio) = await controller.AcceptRecording();
        state = State.Speaking;
        subtitleWindow.Show($"{CharacterName}: {response}");
        await audioPlayer.Play(audio);
        subtitleWindow.Hide();
        state = State.Idle;
    }

    private void StopSpeaking()
    {
        state = State.Idle;
        audioPlayer.Stop();
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
            audioPlayer.Dispose();
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
