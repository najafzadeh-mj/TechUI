using System.Text.Json;
using TechUI.SampleAdmin.ViewModels;

namespace TechUI.SampleAdmin.Services;

public sealed class ProfileService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _path;
    private readonly object _sync = new();

    public ProfileService(IWebHostEnvironment environment)
    {
        _path = Path.Combine(environment.ContentRootPath, "App_Data", "profile.json");
    }

    public AdminProfile Load()
    {
        lock (_sync)
        {
            if (!File.Exists(_path))
            {
                return new AdminProfile();
            }

            try
            {
                var json = File.ReadAllText(_path);

                return JsonSerializer.Deserialize<AdminProfile>(json, JsonOptions)
                    ?? new AdminProfile();
            }
            catch (JsonException)
            {
                return new AdminProfile();
            }
        }
    }

    public void Save(AdminProfile profile)
    {
        lock (_sync)
        {
            var directory = Path.GetDirectoryName(_path)!;

            Directory.CreateDirectory(directory);

            var temp = _path + ".tmp";

            File.WriteAllText(temp, JsonSerializer.Serialize(profile, JsonOptions));
            File.Move(temp, _path, overwrite: true);
        }
    }
}
