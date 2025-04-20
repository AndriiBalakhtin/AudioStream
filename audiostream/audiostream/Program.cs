using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.WriteLine("Select Audio Action:");
        Console.WriteLine(" [1] Capture System Audio (Loopback)");
        Console.WriteLine(" [2] Specific Browser Audio (Experimental)");
        Console.WriteLine(" [3] List All Audio Devices");
        Console.Write("Enter your choice: ");
        string choice = Console.ReadLine();

        if (choice == "1")
        {
            SelectAndCaptureLoopback();
        }
        else if (choice == "2")
        {
            SelectInputOrOutputForBrowser();
        }
        else if (choice == "3")
        {
            ListAllAudioDevices();
        }
        else
        {
            Console.WriteLine("Invalid choice. Exiting.");
        }
    }

    static void SelectAndCaptureLoopback()
    {
        var enumerator = new MMDeviceEnumerator();
        var renderDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();

        Console.WriteLine("\nAvailable Output Devices:");
        if (!renderDevices.Any())
        {
            Console.WriteLine("No active output devices found. Exiting.");
            return;
        }
        for (int i = 0; i < renderDevices.Count; i++)
        {
            Console.WriteLine($" [{i}] {renderDevices[i].FriendlyName}");
        }

        Console.Write("Select the OUTPUT device number for loopback: ");
        if (!int.TryParse(Console.ReadLine(), out int outputIndex) || outputIndex < 0 || outputIndex >= renderDevices.Count)
        {
            Console.WriteLine("Invalid output device number. Exiting.");
            return;
        }
        var selectedOutputDevice = renderDevices[outputIndex];

        using var capture = new WasapiLoopbackCapture(selectedOutputDevice);
        using var output = new WasapiOut(selectedOutputDevice, AudioClientShareMode.Shared, true, 100);
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

        Console.WriteLine($"\nAudio loopback started from '{selectedOutputDevice.FriendlyName}'.");
        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();

        capture.StopRecording();
        output.Stop();
    }

    static void SelectInputOrOutputForBrowser()
    {
        Console.WriteLine("\nSelect Input or Output for Browser Audio (Experimental):");
        Console.WriteLine(" [1] Select Output Device (Capture from Browser)");
        Console.WriteLine(" [2] Select Input Device (Send audio to Browser - Less Common)");
        Console.Write("Enter your choice: ");
        string browserChoice = Console.ReadLine();

        if (browserChoice == "1")
        {
            SelectOutputDeviceForBrowser();
        }
        else if (browserChoice == "2")
        {
            SelectInputDeviceForBrowser();
        }
        else
        {
            Console.WriteLine("Invalid choice. Returning to main menu.");
        }
    }

    static void SelectOutputDeviceForBrowser()
    {
        var enumerator = new MMDeviceEnumerator();
        var renderDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();

        Console.WriteLine("\nAvailable Output Devices:");
        if (!renderDevices.Any())
        {
            Console.WriteLine("No active output devices found. Exiting.");
            return;
        }
        for (int i = 0; i < renderDevices.Count; i++)
        {
            Console.WriteLine($" [{i}] {renderDevices[i].FriendlyName}");
        }

        Console.Write("Select the OUTPUT device number for browser audio capture: ");
        if (!int.TryParse(Console.ReadLine(), out int outputIndex) || outputIndex < 0 || outputIndex >= renderDevices.Count)
        {
            Console.WriteLine("Invalid output device number. Exiting.");
            return;
        }
        var selectedOutputDevice = renderDevices[outputIndex];

        using var output = new WasapiOut(selectedOutputDevice, AudioClientShareMode.Shared, true, 100);

        Console.WriteLine("\nAttempting to capture audio from Chrome or Edge from '{0}'...", selectedOutputDevice.FriendlyName);
        Console.WriteLine("This is an experimental feature and might not work correctly.");
        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();
    }

    static void SelectInputDeviceForBrowser()
    {
        var enumerator = new MMDeviceEnumerator();
        var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

        Console.WriteLine("\nAvailable Input Devices:");
        if (!captureDevices.Any())
        {
            Console.WriteLine("No active input devices found. Exiting.");
            return;
        }
        for (int i = 0; i < captureDevices.Count; i++)
        {
            Console.WriteLine($" [{i}] {captureDevices[i].FriendlyName}");
        }

        Console.Write("Select the INPUT device number for browser audio input (experimental): ");
        if (!int.TryParse(Console.ReadLine(), out int inputIndex) || inputIndex < 0 || inputIndex >= captureDevices.Count)
        {
            Console.WriteLine("Invalid input device number. Exiting.");
            return;
        }
        var selectedInputDevice = captureDevices[inputIndex];

        // Note: Sending audio to a browser typically involves more complex mechanisms
        // than simply selecting an input device here. This part is highly conceptual.
        Console.WriteLine("\nAttempting to send audio to Chrome or Edge using input '{0}'...", selectedInputDevice.FriendlyName);
        Console.WriteLine("This is an experimental feature and likely requires further implementation.");
        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();
    }

    static void ListAllAudioDevices()
    {
        var enumerator = new MMDeviceEnumerator();

        Console.WriteLine("\n--- Output Devices ---");
        var renderDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
        if (renderDevices.Any())
        {
            for (int i = 0; i < renderDevices.Count; i++)
            {
                Console.WriteLine($" [{i}] Output: {renderDevices[i].FriendlyName}");
            }
        }
        else
        {
            Console.WriteLine("No active output devices found.");
        }

        Console.WriteLine("\n--- Input Devices ---");
        var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();
        if (captureDevices.Any())
        {
            for (int i = 0; i < captureDevices.Count; i++)
            {
                Console.WriteLine($" [{i}] Input: {captureDevices[i].FriendlyName}");
            }
        }
        else
        {
            Console.WriteLine("No active input devices found.");
        }

        Console.WriteLine();
    }
}