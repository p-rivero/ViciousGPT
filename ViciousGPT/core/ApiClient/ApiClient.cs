namespace ViciousGPT.ApiClient;

internal class ApiClient
{
    private const string CREDENTIALS_FOLDER = "credentials";

    protected string GetKey(string fileName)
    {
        string keyPath = GetKeyPath(fileName);
        return File.ReadAllText(keyPath).Trim();
    }

    protected string GetKeyPath(string fileName)
    {
        string keyPath = Path.Combine(CREDENTIALS_FOLDER, fileName);
        string absolutePath = Path.GetFullPath(keyPath);
        if (File.Exists(absolutePath))
        {
            return absolutePath;
        }
        throw new FileNotFoundException($"Credentials file not found: {absolutePath}");
    }
}
