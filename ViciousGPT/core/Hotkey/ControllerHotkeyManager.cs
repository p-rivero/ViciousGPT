using XInputDotNetPure;

namespace ViciousGPT;

public class ControllerHotkeyManager : IDisposable
{
    private const int POLL_PERIOD_MS = 5;
    private const int THROTTLE_MS = 1000;

    private readonly PlayerIndex playerIndex;

    public Action? OnSticksPressed { get; set; }
    private readonly Throttle onSticksPressedThrottle = new(THROTTLE_MS);

    public Action? OnTriggersPressed { get; set; }
    private readonly Throttle onTriggersPressedThrottle = new(THROTTLE_MS);

    private readonly CancellationTokenSource cancellationTokenSource = new();

    public ControllerHotkeyManager(uint playerIndex)
    {
        if (playerIndex > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(playerIndex), "Player index must be between 0 and 3");
        }
        this.playerIndex = (PlayerIndex)playerIndex;
        onSticksPressedThrottle.Action += () => OnSticksPressed?.Invoke();
        onTriggersPressedThrottle.Action += () => OnTriggersPressed?.Invoke();
    }

    public void Start()
    {
        _ = ControllerLoop(cancellationTokenSource.Token);
    }

    public void Stop()
    {
        cancellationTokenSource?.Cancel();
    }

    private async Task ControllerLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            GamePadState state = GamePad.GetState(playerIndex);
            if (state.Buttons.LeftStick == ButtonState.Pressed && state.Buttons.RightStick == ButtonState.Pressed)
            {
                onSticksPressedThrottle.Invoke();
            }
            if (state.Triggers.Right > 0.5f && state.Triggers.Left > 0.5f)
            {
                onTriggersPressedThrottle.Invoke();
            }
            await Task.Delay(POLL_PERIOD_MS, cancellationToken);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            cancellationTokenSource.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
