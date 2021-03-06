using System.Collections.Concurrent;

namespace server.Helpers
{
    // C# Console is slow, lets put it on a bg thread.
    public static class FConsole
    {
        private static readonly BlockingCollection<string> Lines;

        static FConsole()
        {
            Lines = new BlockingCollection<string>();
            var bgWorker = new Thread(ProcessingQueue) { IsBackground = true };
            bgWorker.Start();
        }

        private static void ProcessingQueue()
        {
            foreach (var line in Lines.GetConsumingEnumerable())
                Console.Write(line);
        }

        public static void WriteLine(string line) => Lines.Add($"{line}{Environment.NewLine}");
        public static void Write(string text) => Lines.Add(text);

    }
}