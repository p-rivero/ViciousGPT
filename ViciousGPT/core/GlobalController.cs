using ViciousGPT.AudioProcessing;

namespace ViciousGPT;

class GlobalController
{
    private readonly ViciousGptController controller = new();
    private readonly AudioPlayer audioPlayer = new();
    private readonly SubtitleWindow subtitleWindow = new();

    private State state = State.Idle;

    public string CharacterName
    {
        get => controller.UserCharatcterName;
        set => controller.UserCharatcterName = value;
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
        state = State.Recording;
    }

    private void StopRecording()
    {
        state = State.Idle;
        controller.CancelRecording();
    }

    private async Task AcceptRecording()
    {
        state = State.Processing;
        subtitleWindow.Show(controller.UserCharatcterName + " is thinking...");
        var (response, audio) = await controller.AcceptRecording();
        state = State.Speaking;
        subtitleWindow.Show(response);
        await audioPlayer.Play(audio);
        state = State.Idle;
    }

    private void StopSpeaking()
    {
        state = State.Idle;
        audioPlayer.Stop();
    }

    private enum State
    {
        Idle,
        Recording,
        Processing,
        Speaking
    }
}
