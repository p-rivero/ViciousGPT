namespace ViciousGPT;

internal class Throttle(int interval)
{
    private readonly int interval = interval;
    private DateTime lastCall = DateTime.MinValue;

    public Action? Action { get; set; }

    public void Invoke()
    {
        if (CanCall())
        {
            Action?.Invoke();
        }
    }

    public bool CanCall()
    {
        if (DateTime.Now - lastCall > TimeSpan.FromMilliseconds(interval))
        {
            lastCall = DateTime.Now;
            return true;
        }
        return false;
    }
}
