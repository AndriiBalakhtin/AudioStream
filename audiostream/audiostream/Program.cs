using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;

class Program
{
    static void Main()
    {
        using var capture = new WasapiLoopbackCapture();
        var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        Console.WriteLine("Select output device:");
        for (int i = 0; i < devices.Count; i++)
        {
            Console.WriteLine($"{i}: {devices[i].FriendlyName}");
        }

        Console.Write("Number: ");
        int index = int.Parse(Console.ReadLine() ?? "0");
        var outputDevice = devices[index];

        using var output = new WasapiOut(outputDevice, AudioClientShareMode.Shared, true, 100);
        var provider = new BufferedWaveProvider(capture.WaveFormat)
        {
            DiscardOnBufferOverflow = true
        };

        output.Init(provider);
        output.Play();

        capture.DataAvailable += (s, e) =>
        {
            provider.AddSamples(e.Buffer, 0, e.BytesRecorded);
        };

        capture.StartRecording();

        Console.WriteLine("Audio streaming started. Press Enter to exit.");
        Console.ReadLine();

        capture.StopRecording();
    }
}
