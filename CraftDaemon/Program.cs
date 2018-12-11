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
            sdk.Connect();

            Console.WriteLine("Connection ok");
            if (sdk.TryRegister(Process.GetCurrentProcess(), ApplicationId))
            {
                Console.WriteLine("Application registered");
                sdk.CrownTouched += OnCrownTouched;
                sdk.CrownTurned += OnCrownTurned;
            }

            Console.WriteLine("Press escape to quit");
            Console.WriteLine("Use the Craft keyboard Crownd");
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey();
                switch(key.Key)
                {
                    case ConsoleKey.A:
                        sdk.ChangeTool("ToolA");
                        //sdk.GiveToolFeedback("Tool A", "quickLayout", "1");
                        break;
                    case ConsoleKey.B:
                        sdk.ChangeTool("ToolB");
                        //sdk.GiveToolFeedback("Tool B", "quickLayout", "2");
                        break;
                }
            }
            while (key.Key != ConsoleKey.Escape);

            sdk.Disconnect();
        }

        private static void OnCrownTurned(CrownRootObject obj)
        {
            Console.WriteLine($"Turned: {obj.delta} - {obj.touch_state}");
        }

        private static void OnCrownTouched(CrownRootObject obj)
        {
            Console.WriteLine($"Touched: {obj.delta} - {obj.touch_state}");
        }
    }
}
