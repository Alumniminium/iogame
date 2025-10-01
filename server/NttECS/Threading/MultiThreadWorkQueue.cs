using System;
using System.Collections.Concurrent;
using System.Threading;

namespace server.NttECS.Threading;

public class MultiThreadWorkQueue<T>
{
    public int QueueSize => Queue.Count;

    private readonly BlockingCollection<T> Queue = [];
    private readonly Thread[] workerThreads;
    private readonly Action<T> OnExec;

    public MultiThreadWorkQueue(Action<T> exec, int threadCount = 1)
    {
        OnExec = exec;
        workerThreads = new Thread[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            workerThreads[i] = new Thread(WorkLoop) { IsBackground = true };
            workerThreads[i].Start();
        }
    }

    public void Enqueue(T item) => Queue.Add(item);

    private void WorkLoop()
    {
        foreach (var item in Queue.GetConsumingEnumerable())
            OnExec.Invoke(item);
    }

    public void Stop()
    {
        Queue.CompleteAdding();
        foreach (var thread in workerThreads)
            thread.Join();
    }
}