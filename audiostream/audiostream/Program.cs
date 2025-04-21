using NAudio.CoreAudioApi;
using NAudio.Wave;

class Program
{
    private const string AuthorGithub = "https://github.com/AndriiBalakhtin";

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("🎧 Select Audio Source 🎤");
        Console.WriteLine($"👨‍💻 Author: {AuthorGithub}");
        Console.WriteLine();

        var enumerator = new MMDeviceEnumerator();

        var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        Console.WriteLine("👂 Select the system audio capture device:");
        for (int i = 0; i < captureDevices.Count; i++)
        {
            Console.WriteLine($"[{i}] {captureDevices[i].FriendlyName}");
        }
        Console.Write("Device number: ");
        if (!int.TryParse(Console.ReadLine(), out int captureIndex) || captureIndex < 0 || captureIndex >= captureDevices.Count)
        {
            Console.WriteLine("❌ Invalid capture device number. Exiting.");
            return;
        }
        var selectedCaptureDevice = captureDevices[captureIndex];
        Console.WriteLine($"✅ Selected capture device: {selectedCaptureDevice.FriendlyName}");

        var outputVirtualCables = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
            .Where(d => d.FriendlyName.Contains("CABLE") || d.FriendlyName.Contains("VB-Audio"))
            .ToList();

        Console.WriteLine("\n📢 Select the virtual cable for audio output:");
        if (outputVirtualCables.Any())
        {
            for (int i = 0; i < outputVirtualCables.Count; i++)
            {
                Console.WriteLine($"[{i}] {outputVirtualCables[i].FriendlyName}");
            }
            Console.Write("Virtual cable number: ");
            if (!int.TryParse(Console.ReadLine(), out int outputIndex) || outputIndex < 0 || outputIndex >= outputVirtualCables.Count)
            {
                Console.WriteLine("❌ Invalid virtual cable number. Exiting.");
                return;
            }
            var selectedOutputDevice = outputVirtualCables[outputIndex];
            Console.WriteLine($"✅ Selected virtual cable: {selectedOutputDevice.FriendlyName}");

            using var capture = new WasapiLoopbackCapture(selectedCaptureDevice);
            using var output = new WasapiOut(selectedOutputDevice, AudioClientShareMode.Shared, true, 200);

            var bufferedWaveProvider = new BufferedWaveProvider(capture.WaveFormat);

            output.Init(bufferedWaveProvider);
            output.Play();

            capture.DataAvailable += (s, a) =>
            {
                bufferedWaveProvider.AddSamples(a.Buffer, 0, a.BytesRecorded);
            };

            capture.StartRecording();

            Console.WriteLine("🚀 Streaming started. Press Enter to exit.");
            Console.ReadLine();

            capture.StopRecording();
        }
        else
        {
            Console.WriteLine("⚠️ Virtual cables not found.");
        }
    }
}