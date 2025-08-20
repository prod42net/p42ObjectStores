using System.Collections.Generic;

namespace p42ObjectStores;

public class FixedSizeQueue<T>(int maxSize)
{
    private readonly Queue<T> _queue = new();

    public void Enqueue(T item)
    {
        _queue.Enqueue(item);
        while (_queue.Count > maxSize)
        {
            _queue.Dequeue();
        }
    }

    public int Count => _queue.Count;

    public IEnumerable<T> Items => _queue;
}
