using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CSImageClick
{
    internal class HandlerForMisc
    {
        public static void BeepInNewThread()
        {
            // Start a new thread to play a beep sound
            Thread beepThread = new Thread(() =>
            {
                try
                {
                    Console.Beep(); // Play a beep sound
                }
                catch(Exception beepEx)
                {
                    Console.WriteLine($"Error in beep thread: {beepEx.Message} {beepEx.StackTrace}");
                }
            });
            beepThread.Start(); // Start the beep thread
        }

        public static int? GetSubstringAsInt(string fileName, string prefix, string suffix)
        {
            int prefixIndex = fileName.IndexOf(prefix);
            int suffixIndex = fileName.IndexOf(suffix, prefixIndex + prefix.Length);

            if(prefixIndex != -1 && suffixIndex != -1)
            {
                int start = prefixIndex + prefix.Length; // Start after the prefix
                string value = fileName.Substring(start, suffixIndex - start);
                if(int.TryParse(value, out int result))
                {
                    return result; // Return the parsed integer
                }
            }

            return null; // Return null if extraction fails
        }
    }
}
