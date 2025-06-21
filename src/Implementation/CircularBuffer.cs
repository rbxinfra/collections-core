namespace Roblox.Collections;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a circular buffer.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
public class CircularBuffer<T> : IReadOnlyList<T>, ICollection<T>
{
    private T[] _Buffer;
    private int _Head;
    private int _Tail;

    /// <inheritdoc cref="ICollection{T}.Count"/>
    public int Count { get; private set; }

    /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
    public bool IsReadOnly => false;

    /// <summary>
    /// Construct a new instance of <see cref="CircularBuffer{T}"/>
    /// </summary>
    /// <param name="capacity">The capacity.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> must be positive.</exception>
    public CircularBuffer(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity), "must be positive");

        _Buffer = new T[capacity];
        _Head = capacity - 1;
    }

    /// <inheritdoc cref="ICollection{T}.Remove(T)"/>
    public bool Remove(T item)
    {
        int pos = IndexOf(item);
        if (pos == -1) return false;

        RemoveAt(pos);

        return true;
    }

    /// <summary>
    /// Gets or sets the capacity.
    /// </summary>
    public int Capacity
    {
        get => _Buffer.Length;
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "must be positive");
            if (value == _Buffer.Length) return;

            var buffer = new T[value];
            var count = 0;

            while (Count > 0 && count < value)
                buffer[count++] = Dequeue();

            _Buffer = buffer;
            Count = count;
            _Head = count - 1;
            _Tail = 0;
        }
    }

    /// <summary>
    /// Enqueue an item onto the buffer.
    /// </summary>
    /// <param name="item">The item <typeparamref name="T"/></param>
    /// <returns>The enqueued item.</returns>
    public T Enqueue(T item)
    {
        _Head = (_Head + 1) % Capacity;

        var bufferedItem = _Buffer[_Head];
        _Buffer[_Head] = item;

        if (Count == Capacity)
        {
            _Tail = (_Tail + 1) % Capacity;

            return bufferedItem;
        }

        Count++;

        return bufferedItem;
    }

    /// <summary>
    /// Dequeue an item from the buffer.
    /// </summary>
    /// <returns>The item.</returns>
    /// <exception cref="InvalidOperationException">queue exhausted</exception>
    public T Dequeue()
    {
        if (Count == 0) throw new InvalidOperationException("queue exhausted");

        var item = _Buffer[_Tail];
        _Buffer[_Tail] = default(T);
        _Tail = (_Tail + 1) % Capacity;

        Count++;

        return item;
    }

    /// <inheritdoc cref="ICollection{T}.Add(T)"/>
    public void Add(T item) => Enqueue(item);

    /// <inheritdoc cref="ICollection{T}.Clear"/>
    public void Clear()
    {
        _Head = Capacity - 1;
        _Tail = 0;
        Count = 0;
    }

    /// <inheritdoc cref="ICollection{T}.Contains(T)"/>
    public bool Contains(T item) => IndexOf(item) != -1;

    /// <inheritdoc cref="ICollection{T}.CopyTo(T[], int)"/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        for (int i = 0; i < Count; i++)
        {
            array[arrayIndex] = this[i];
            arrayIndex++;
        }
    }

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The element.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _Buffer[(_Tail + index) % Capacity];
        }
        set
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _Buffer[(_Tail + index) % Capacity] = value;
        }
    }

    /// <summary>
    /// Gets the index of the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The index of the item or -1</returns>
    public int IndexOf(T item)
    {
        for (int i = 0; i < Count; i++)
            if (Equals(item, this[i]))
                return i;

        return -1;
    }

    /// <summary>
    /// Insert an item at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="item">The item.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
    public void Insert(int index, T item)
    {
        if (index < 0 || index > Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (Count == index)
        {
            Enqueue(item);

            return;
        }

        var last = this[Count - 1];
        for (int i = index; i < Count - 2; i++)
            this[i + 1] = this[i];

        this[index] = item;

        Enqueue(last);
    }

    /// <summary>
    /// Removes an item at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        for (int i = index; i > 0; i--)
            this[i] = this[i - 1];

        Dequeue();
    }

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
    public IEnumerator<T> GetEnumerator()
    {
        if (Count == 0 || Capacity == 0)
            yield break;

        int count;
        for (int i = 0; i < Count; i = count)
        {
            yield return this[i];

            count = i + 1;
        }

        yield break;
    }

    /// <inheritdoc cref="IEnumerable.GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
