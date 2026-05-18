using System;
using System.IO;
using System.Windows.Media;

namespace RGBSelcer.Helpers
{
    public static class AudioService
    {
        private static MediaPlayer? _player;
        private static string? _audioFilePath;

        public static void StartBackgroundMusic()
        {
            try
            {
                _audioFilePath = GenerateAmbientWav();
                _player = new MediaPlayer();
                _player.Open(new Uri(_audioFilePath, UriKind.Absolute));
                _player.Volume = 0.15;
                _player.MediaEnded += (s, e) =>
                {
                    _player.Position = TimeSpan.Zero;
                    _player.Play();
                };
                _player.Play();
            }
            catch
            {
                // Silently ignore audio errors
            }
        }

        public static void StopBackgroundMusic()
        {
            try
            {
                _player?.Stop();
                _player?.Close();
                _player = null;

                if (_audioFilePath != null && File.Exists(_audioFilePath))
                    File.Delete(_audioFilePath);
            }
            catch
            {
                // Silently ignore
            }
        }

        public static void SetVolume(double volume)
        {
            if (_player != null)
                _player.Volume = Math.Clamp(volume, 0.0, 1.0);
        }

        private static string GenerateAmbientWav()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RGBSelcer");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var filePath = Path.Combine(dir, "ambient.wav");

            int sampleRate = 22050;
            int durationSec = 8;
            int totalSamples = sampleRate * durationSec;
            short[] samples = new short[totalSamples];

            var rng = new Random(42);

            for (int i = 0; i < totalSamples; i++)
            {
                double t = (double)i / sampleRate;

                double wave1 = Math.Sin(2 * Math.PI * 130.81 * t) * 0.2;
                double wave2 = Math.Sin(2 * Math.PI * 164.81 * t) * 0.15;
                double wave3 = Math.Sin(2 * Math.PI * 196.00 * t) * 0.12;
                double wave4 = Math.Sin(2 * Math.PI * 261.63 * t) * 0.08;

                double lfo = 0.7 + 0.3 * Math.Sin(2 * Math.PI * 0.2 * t);

                double sample = (wave1 + wave2 + wave3 + wave4) * lfo;

                double fadeIn = Math.Min(t / 1.0, 1.0);
                double fadeOut = Math.Min((durationSec - t) / 1.0, 1.0);
                sample *= fadeIn * fadeOut;

                samples[i] = (short)(sample * short.MaxValue * 0.5);
            }

            using var fs = new FileStream(filePath, FileMode.Create);
            using var writer = new BinaryWriter(fs);

            int byteRate = sampleRate * 2;
            int dataSize = totalSamples * 2;

            writer.Write(new[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize);
            writer.Write(new[] { 'W', 'A', 'V', 'E' });
            writer.Write(new[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write(new[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);

            foreach (var s in samples)
                writer.Write(s);

            return filePath;
        }
    }
}
