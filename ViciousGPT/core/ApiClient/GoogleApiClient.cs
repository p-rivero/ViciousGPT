using ViciousGPT.Properties;

namespace ViciousGPT.ApiClient;

internal abstract class GoogleApiClient
{
    protected static void SetCredentialsEnv()
    {
        string jsonKeyPath = Settings.Default.GoogleServiceAccountPath;
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonKeyPath);
    }
}
