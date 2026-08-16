using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using NWN_Timer.Models;

namespace NWN_Timer.Services;

public class ProfileService
{
    private const string ProfilePrefix = "timerprofile_";

    private readonly string _profileDirectory;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public ProfileService()
    {
        // Profiles are deliberately stored beside the executable.
        _profileDirectory = AppContext.BaseDirectory;
    }

    public bool Exists(string profileName)
    {
        return File.Exists(GetProfilePath(profileName));
    }

    public IReadOnlyList<string> GetProfileNames()
    {
        List<string> profiles = new();

        // Default always appears first.
        if (File.Exists(GetProfilePath("Default")))
        {
            profiles.Add("Default");
        }

        IEnumerable<string> namedProfiles =
            Directory
                .EnumerateFiles(
                    _profileDirectory,
                    $"{ProfilePrefix}*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name =>
                    name![ProfilePrefix.Length..])
                .OrderBy(
                    name => name,
                    StringComparer.OrdinalIgnoreCase);

        profiles.AddRange(namedProfiles);

        return profiles;
    }

    public string Save(
        string profileName,
        TimerProfile profile)
    {
        string safeName = SanitizeProfileName(profileName);

        string path = GetProfilePath(safeName);

        string json =
            JsonSerializer.Serialize(profile, _jsonOptions);

        File.WriteAllText(path, json);

        return safeName;
    }

    public TimerProfile? Load(string profileName)
    {
        string path = GetProfilePath(profileName);

        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<TimerProfile>(json);
    }

    private string GetProfilePath(string profileName)
    {
        string safeName = SanitizeProfileName(profileName);

        if (string.Equals(
            safeName,
            "Default",
            StringComparison.OrdinalIgnoreCase))
        {
            return Path.Combine(
                _profileDirectory,
                "default.json");
        }

        return Path.Combine(
            _profileDirectory,
            $"{ProfilePrefix}{safeName}.json");
    }

    private static string SanitizeProfileName(
        string profileName)
    {
        string cleaned = new string(
            profileName
                .Trim()
                .Select(c =>
                    char.IsLetterOrDigit(c) ||
                    c == ' ' ||
                    c == '-' ||
                    c == '_'
                        ? c
                        : '_')
                .ToArray());

        return string.IsNullOrWhiteSpace(cleaned)
            ? "Profile"
            : cleaned;
    }
}