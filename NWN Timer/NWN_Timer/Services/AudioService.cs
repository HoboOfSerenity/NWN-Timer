using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace NWN_Timer.Services;

public class AudioService : IDisposable
{
    private readonly AudioFormat _format = AudioFormat.Dvd;

    private MiniAudioEngine? _engine;
    private AudioPlaybackDevice? _playbackDevice;

    private readonly Dictionary<string, SoundPlayer> _players =
        new(StringComparer.OrdinalIgnoreCase);


    public IReadOnlyList<string> GetAvailableSoundFiles()
    {
        List<string> sounds = new()
        {
            "None"
        };

        IEnumerable<string> wavFiles =
            Directory
                .EnumerateFiles(AppContext.BaseDirectory)
                .Where(file =>
                    string.Equals(
                        Path.GetExtension(file),
                        ".wav",
                        StringComparison.OrdinalIgnoreCase))
                .Select(Path.GetFileName)
                .Where(name =>
                    !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .OrderBy(
                    name => name,
                    StringComparer.OrdinalIgnoreCase);

        sounds.AddRange(wavFiles);

        return sounds;
    }


    public bool Play(string? soundFile)
    {
        if (string.IsNullOrWhiteSpace(soundFile))
            return false;

        if (string.Equals(
            soundFile,
            "None",
            StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string safeFileName =
            Path.GetFileName(soundFile);

        string path =
            Path.Combine(
                AppContext.BaseDirectory,
                safeFileName);

        if (!File.Exists(path))
            return false;

        try
        {
            EnsureInitialized();

            if (_engine == null ||
                _playbackDevice == null)
            {
                return false;
            }

            if (!_players.TryGetValue(
                path,
                out SoundPlayer? player))
            {
                FileStream stream =
                    new(
                        path,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read);

                StreamDataProvider provider =
                    new(
                        _engine,
                        _format,
                        stream);

                player =
                    new SoundPlayer(
                        _engine,
                        _format,
                        provider);

                _playbackDevice
                    .MasterMixer
                    .AddComponent(player);

                _players[path] = player;
            }

            // Stop() returns the sound to its beginning,
            // so pressing Test repeatedly works correctly.
            player.Stop();
            player.Play();

            return true;
        }
        catch
        {
            // A missing/bad audio device or damaged WAV
            // should never crash the timer program.
            return false;
        }
    }


    private void EnsureInitialized()
    {
        if (_engine != null &&
            _playbackDevice != null)
        {
            return;
        }

        _engine = new MiniAudioEngine();

        // null means use the system's default playback device.
        _playbackDevice =
            _engine.InitializePlaybackDevice(
                null,
                _format);

        _playbackDevice.Start();
    }


    public void Dispose()
    {
        if (_playbackDevice != null)
        {
            foreach (SoundPlayer player
                     in _players.Values)
            {
                _playbackDevice
                    .MasterMixer
                    .RemoveComponent(player);

                player.Dispose();
            }

            _players.Clear();

            _playbackDevice.Stop();
            _playbackDevice.Dispose();
        }

        _engine?.Dispose();
    }
}