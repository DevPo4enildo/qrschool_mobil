namespace qrschool_mobil.Services;

public static class NeonConnectionSettings
{
    public const string PreferenceKey = "neon_connection_string";

    public static string ConnectionString
    {
        get
        {
            var savedValue = Preferences.Default.Get(PreferenceKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(savedValue))
            {
                return savedValue;
            }

            return Environment.GetEnvironmentVariable("NEON_DATABASE_URL")
                ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? string.Empty;
        }
        set => Preferences.Default.Set(PreferenceKey, value.Trim());
    }

    public static bool HasConnectionString => !string.IsNullOrWhiteSpace(ConnectionString);
}
