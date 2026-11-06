using System.Text.Json;
using TechUI.SampleAdmin.ViewModels;

namespace TechUI.SampleAdmin.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _path;
    private readonly object _sync = new();

    public SettingsService(IWebHostEnvironment environment)
    {
        _path = Path.Combine(environment.ContentRootPath, "App_Data", "settings.json");
    }

    public AdminSettings Load()
    {
        lock (_sync)
        {
            if (!File.Exists(_path))
            {
                return new AdminSettings();
            }

            try
            {
                var json = File.ReadAllText(_path);

                return JsonSerializer.Deserialize<AdminSettings>(json, JsonOptions)
                    ?? new AdminSettings();
            }
            catch (JsonException)
            {
                return new AdminSettings();
            }
        }
    }

    public void Save(AdminSettings settings)
    {
        lock (_sync)
        {
            var directory = Path.GetDirectoryName(_path)!;

            Directory.CreateDirectory(directory);

            var temp = _path + ".tmp";

            File.WriteAllText(temp, JsonSerializer.Serialize(settings, JsonOptions));
            File.Move(temp, _path, overwrite: true);
        }
    }
}
