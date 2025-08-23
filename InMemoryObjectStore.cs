using System.Collections.Concurrent;

namespace p42ObjectStores;

public class InMemoryObjectStore : BaseStore
{
    readonly ConcurrentDictionary<string, object> _reports = new();

    public override int NumberOfObject(string? prefix = null)
    {
        return _reports.Count;
    }

    public override async Task<T?> Get<T>(string name, string? prefix = null) where T : class
    {
        string id = GetPath(name, "", prefix);
        if (String.IsNullOrEmpty(id)) return null;
        _reports.TryGetValue(id, out object model);
        if (model != null && typeof(T) == model.GetType())
            return (T)model;
        return null;
    }

    public override async Task<T?> Add<T>(T model, string name, string? prefix = null) where T : class
    {
        if (model == null || String.IsNullOrWhiteSpace(name)) return null;

        string id = GetPath(name, "", prefix);
        if (_reports.TryAdd(id, model)) return model;
        return null;
    }

    public override bool Delete(string name, string? prefix = null)
    {
        return _reports.TryRemove(GetPath(name, "", prefix), out _);
    }

    public override bool Update<T>(string name, T model, string? prefix = null)
    {
        string id = GetPath(name, "", prefix);
        if (String.IsNullOrEmpty(id) || model == null) return false;
        if (!_reports.ContainsKey(id)) return false;
        _reports[id] = model;
        return true;
    }
}