using System;
using System.Diagnostics;

using CraftSdk;

namespace CraftDaemon
{
    class Program
    {
        private static Guid ApplicationId = new Guid("{1CF0E277-919B-4DE4-B226-B41FEF915CFC}");

        static void Main(string[] args)
        {
            var sdk = new CraftDevice();
            sdk.Connect(Process.GetCurrentProcess(), ApplicationId).Wait();
            sdk.CrownTouched += OnCrownTouched;
            sdk.CrownTurned += OnCrownTurned;

            Console.WriteLine("Press A to select Tool A");
            Console.WriteLine("Press B to select Tool B");
            Console.WriteLine("Use the Craft keyboard Crownd");
            ConsoleKeyInfo key;
            
            do
            {
                key = Console.ReadKey();
                switch(key.Key)
                {
                    case ConsoleKey.A:
                        sdk.ChangeTool("ToolA").Wait();
                        break;
                    case ConsoleKey.B:
                        sdk.ChangeTool("ToolB").Wait();
                        break;
                }
            }
            while (key.Key != ConsoleKey.Escape);

            sdk.Disconnect();
        }

        private static void OnCrownTurned(Crown crown)
        {
            Console.WriteLine($"Turned: {crown.Delta}");
        }

        private static void OnCrownTouched(Crown crown)
        {
            Console.WriteLine($"Touched: {crown.IsTouched}");
        }
    }
}
