using System;
using System.IO;
using System.Windows.Media;

namespace RGBSelcer.Helpers
{
    public static class AudioService
    {
        private static MediaPlayer? _player;
        private static string? _tempFilePath;

        public static void StartBackgroundMusic()
        {
            try
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SelcerRoyalityPRM");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                _tempFilePath = Path.Combine(dir, "ambient.wav");
                GenerateAmbientWav(_tempFilePath);

                _player = new MediaPlayer();
                _player.Open(new Uri(_tempFilePath));
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
            }
        }

        public static void StopBackgroundMusic()
        {
            try
            {
                _player?.Stop();
                _player?.Close();
                _player = null;

                if (_tempFilePath != null && File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);
            }
            catch
            {
            }
        }

        private static void GenerateAmbientWav(string path)
        {
            int sampleRate = 44100;
            int durationSec = 8;
            int numSamples = sampleRate * durationSec;
            short[] samples = new short[numSamples];

            double[] freqs = { 130.81, 164.81, 196.0, 261.63 };

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double sample = 0;

                foreach (var freq in freqs)
                {
                    double lfo = 1.0 + 0.003 * Math.Sin(2 * Math.PI * 0.2 * t);
                    sample += Math.Sin(2 * Math.PI * freq * lfo * t);
                }

                sample /= freqs.Length;

                double fadeIn = Math.Min(t / 1.5, 1.0);
                double fadeOut = Math.Min((durationSec - t) / 1.5, 1.0);
                sample *= fadeIn * fadeOut * 0.5;

                samples[i] = (short)(sample * short.MaxValue);
            }

            using var fs = new FileStream(path, FileMode.Create);
            using var bw = new BinaryWriter(fs);
            int byteRate = sampleRate * 2;
            int dataSize = numSamples * 2;

            bw.Write(new[] { 'R', 'I', 'F', 'F' });
            bw.Write(36 + dataSize);
            bw.Write(new[] { 'W', 'A', 'V', 'E' });
            bw.Write(new[] { 'f', 'm', 't', ' ' });
            bw.Write(16);
            bw.Write((short)1);
            bw.Write((short)1);
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write((short)2);
            bw.Write((short)16);
            bw.Write(new[] { 'd', 'a', 't', 'a' });
            bw.Write(dataSize);

            foreach (var s in samples)
                bw.Write(s);
        }
    }
}
