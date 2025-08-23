namespace p42ObjectStores;

public class FixedSizeQueue<T>(int maxSize)
{
    readonly Queue<T> _queue = new();

    public int Count => _queue.Count;

    public IEnumerable<T> Items => _queue;

    public void Enqueue(T item)
    {
        _queue.Enqueue(item);
        while (_queue.Count > maxSize) _queue.Dequeue();
    }
}