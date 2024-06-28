using System.Diagnostics;

namespace ViciousGPT;

class CommandRunner
{
    public string Command { get; set; } = "";

    public double DelaySeconds
    {
        get => delaySeconds;
        set => delaySeconds = Math.Max(0, value);
    }
    private double delaySeconds;

    public async Task Run()
    {
        if (string.IsNullOrWhiteSpace(Command))
        {
            return;
        }
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        try
        {
            await RunComandUnsafe();
        }
        catch (Exception e)
        {
            Trace.TraceError($"Error running command '{Command}':\n{e}");
        }
    }

    public void Start()
    {
        _ = Run();
    }

    private async Task RunComandUnsafe()
    {
        Trace.TraceInformation($"Running command: '{Command}'");
        Process process = new()
        {
            StartInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                ArgumentList = { "/C", Command },
            }
        };
        process.Start();
        await process.WaitForExitAsync();
        Trace.TraceInformation($"Command '{Command}' exited with code {process.ExitCode}");
        string output = await process.StandardOutput.ReadToEndAsync();
        Trace.TraceInformation($"Command '{Command}' output:\n{output}");
    }
}
