namespace ViciousGPT.ApiClient;

internal abstract class GoogleApiClient : ApiClient
{
    private const string GOOGLE_API_KEY_FILE = "google-credentials.json";

    protected GoogleApiClient()
    {
        string jsonKeyPath = GetKeyPath(GOOGLE_API_KEY_FILE);
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonKeyPath);
    }
}
