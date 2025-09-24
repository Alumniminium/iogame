using System;
using System.Threading;
using server.Helpers;

namespace server.ECS;

/// <summary>
/// High-performance thread pool for parallel execution of ECS system operations.
/// Maintains a pool of background threads with high priority for efficient multi-threaded entity processing.
/// </summary>
public static class ThreadedWorker
{
    /// <summary>Pool of worker threads, one per processor core</summary>
    private static readonly Thread[] _threads;
    /// <summary>Synchronization events to wake up specific worker threads</summary>
    private static readonly AutoResetEvent[] _blocks;
    /// <summary>Event signaled when all worker threads complete their tasks</summary>
    private static readonly AutoResetEvent _allReady = new(false);
    /// <summary>Atomic counter tracking number of threads that have completed work</summary>
    private static volatile int _readyThreads;
    /// <summary>Number of threads currently being used for work execution</summary>
    private static volatile int _numThreadsUsed;
    /// <summary>Current action being executed by worker threads</summary>
    private static Action<int, int> Action;


    /// <summary>
    /// Initializes the thread pool with one high-priority background thread per processor core.
    /// </summary>
    static ThreadedWorker()
    {
        Action = (i, j) => { };
        _threads = new Thread[Environment.ProcessorCount];
        _blocks = new AutoResetEvent[Environment.ProcessorCount];
        for (var i = 0; i < Environment.ProcessorCount; i++)
        {
            _blocks[i] = new AutoResetEvent(false);
            _threads[i] = new Thread(ThreadLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            _threads[i].Start(i);
        }
    }

    /// <summary>
    /// Executes an action across multiple threads with automatic thread count management.
    /// Falls back to single-threaded execution for small workloads or when only one thread is requested.
    /// </summary>
    /// <param name="action">Action to execute (receives thread index and total thread count)</param>
    /// <param name="threads">Number of threads to use for execution</param>
    public static void Run(Action<int, int> action, int threads)
    {
        if (threads <= 1)
        {
            action(0, 1);
            return;
        }

        if (threads > Environment.ProcessorCount)
            threads = Environment.ProcessorCount;

        _numThreadsUsed = threads;
        Action = action;

        _allReady.Reset();
        Interlocked.Exchange(ref _readyThreads, 0);

        for (var i = 0; i < threads; i++)
            _blocks[i].Set();

        _allReady.WaitOne();
    }


    /// <summary>
    /// Main loop for worker threads that waits for work and executes assigned actions.
    /// Runs continuously until application shutdown, using synchronization events for coordination.
    /// </summary>
    /// <param name="id">Thread identifier (index in thread pool)</param>
    public static void ThreadLoop(object id)
    {
        if (id is null)
            throw new ArgumentNullException(nameof(id));

        var idx = (int)id;
        while (true)
        {
            _blocks[idx].WaitOne();

            try
            {
                Action.Invoke(idx, _numThreadsUsed);
            }
            finally
            {
                if (Interlocked.Increment(ref _readyThreads) == _numThreadsUsed)
                    _allReady.Set();
            }
        }
    }
}