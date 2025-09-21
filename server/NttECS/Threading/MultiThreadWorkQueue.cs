using System;
using System.Collections.Concurrent;
using System.Threading;

namespace server.Threading;

/// <summary>
/// Thread-safe work queue that distributes tasks across multiple background worker threads.
/// Uses BlockingCollection for efficient producer-consumer pattern with automatic load balancing.
/// </summary>
/// <typeparam name="T">Type of work items to process</typeparam>
public class MultiThreadWorkQueue<T>
{
    /// <summary>
    /// Gets the current number of items in the queue waiting to be processed.
    /// </summary>
    public int QueueSize => Queue.Count;

    private readonly BlockingCollection<T> Queue = [];
    private readonly Thread[] workerThreads;
    private readonly Action<T> OnExec;

    /// <summary>
    /// Initializes a new multi-threaded work queue with the specified executor and thread count.
    /// Worker threads are started immediately as background threads.
    /// </summary>
    /// <param name="exec">Action to execute for each work item</param>
    /// <param name="threadCount">Number of worker threads (defaults to 1)</param>
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

    /// <summary>
    /// Adds a work item to the queue for processing by worker threads.
    /// This method blocks if the underlying collection has reached capacity.
    /// </summary>
    /// <param name="item">Work item to process</param>
    public void Enqueue(T item) => Queue.Add(item);

    /// <summary>
    /// Worker thread loop that continuously processes items from the queue until shutdown.
    /// Each worker thread competes for items using BlockingCollection's thread-safe enumeration.
    /// </summary>
    private void WorkLoop()
    {
        foreach (var item in Queue.GetConsumingEnumerable())
            OnExec.Invoke(item);
    }

    /// <summary>
    /// Signals the queue to stop accepting new work and waits for all worker threads to complete.
    /// This method blocks until all worker threads have finished processing their current items.
    /// </summary>
    public void Stop()
    {
        Queue.CompleteAdding();
        foreach (var thread in workerThreads)
            thread.Join();
    }
}