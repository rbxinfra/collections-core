namespace Roblox.Collections;

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

using EventLog;

/// <summary>
/// Base class for a buffered counter.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public abstract class BufferedCounterBase<TKey> : CounterBase<TKey>, IDisposable
{
    private ConcurrentDictionary<TKey, double> _Current;
    private Timer _CommitTimer;
    private bool _Disposed;

    private readonly ILogger _Logger;

    /// <summary>
    /// Handler for the event invoked when a commit is made.
    /// </summary>
    /// <param name="counterKeys">The counter keys.</param>
    public delegate void OnCommitHandler(IEnumerable<TKey> counterKeys);

    /// <summary>
    /// Event invoked when a commit is made.
    /// </summary>
    public event OnCommitHandler OnCommit;

    /// <summary>
    /// Gets the commit interval.
    /// </summary>
    protected abstract TimeSpan CommitInterval { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BufferedCounterBase{TKey}" /> class.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger" />.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="logger" />
    /// </exception>
    protected BufferedCounterBase(ILogger logger)
    {
        _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _Current = new ConcurrentDictionary<TKey, double>();

        InitializeTimer();
    }

    /// <inheritdoc cref="ICounter{TKey}.Increment(TKey, double)"/>
    public override void Increment(TKey counterKey, double amount = 1.0) 
        => _Current.AddOrUpdate(counterKey, amount, (k, v) => v + amount);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose of the counter.
    /// </summary>
    /// <param name="disposing">Is disposing?</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_Disposed) return;

        if (disposing)
        {
            DoCommit();

            _CommitTimer?.Dispose();
        }

        _CommitTimer = null;
        _Disposed = true;
    }

    private void InitializeTimer()
    {
        _CommitTimer = new Timer(s => PauseTimerAndCommit(), null, CommitInterval, CommitInterval);
    }

    private void PauseTimerAndCommit()
    {
        _CommitTimer.Change(-1, -1);

        DoCommit();

        _CommitTimer.Change(CommitInterval, CommitInterval);
    }

    internal void DoCommit()
    {
        var newDictionary = new ConcurrentDictionary<TKey, double>();
        var toCommit = Interlocked.Exchange(ref _Current, newDictionary);

        Commit(toCommit);

        if (OnCommit != null)
        {
            try
            {
                OnCommit(toCommit.Keys);
            }
            catch (Exception e)
            {
                _Logger.Error(e);
            }
        }
    }

    /// <summary>
    /// The dtor.
    /// </summary>
    ~BufferedCounterBase()
    {
        try
        {
            DoCommit();
        }
        catch (Exception)
        {
        }

        Dispose(false);
    }
}
