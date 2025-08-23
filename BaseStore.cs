using p42BaseLib;
using p42BaseLib.Interfaces;

namespace p42ObjectStores;

public class BaseStore : IP42ObjectModelStore
{
    readonly IP42Logger _logger = new P42Logger();

    public virtual int NumberOfObject(string? prefix = null)
    {
        _logger.Debug($"NumberOfObject from prefix [{prefix}] not implemented in base class");
        throw new NotImplementedException("CRUD not implemented in base class");
    }

    public virtual Task<T?> Get<T>(string name, string? prefix = null) where T : class
    {
        _logger.Debug($"Get from name [{name}] prefix [{prefix}] not implemented in base class");
        throw new NotImplementedException("CRUD not implemented in base class");
    }

    public virtual Task<List<T>> GetAll<T>(string name = "", string? prefix = null)
    {
        _logger.Debug($"GetAll from name [{name}] prefix [{prefix}] not implemented in base class");
        throw new NotImplementedException("CRUD not implemented in base class");
    }

    public virtual Task<T?> Add<T>(T model, string name, string? prefix = null) where T : class
    {
        _logger.Debug($"Add from model [{model}] name [{name}] prefix [{prefix}] not implemented in base class");
        throw new NotImplementedException("CRUD not implemented in base class");
    }

    public virtual bool Delete(string name, string? prefix = null)
    {
        _logger.Debug($"Delete from name [{name}] prefix [{prefix}] not implemented in base class");
        throw new NotImplementedException("CRUD not implemented in base class");
    }

    public virtual bool Update<T>(string name, T model, string? prefix = null) where T : class
    {
        _logger.Debug($"Update from model [{model}] name [{name}] prefix [{prefix}] not implemented in base class");
        throw new NotImplementedException("CRUD not implemented in base class");
    }

    protected string GetPath(string name, string ext = "", string? prefix = null)
    {
        _logger.Debug($"GetPath from name [{name}] ext [{ext}] prefix [{prefix}]");
        string path = !String.IsNullOrWhiteSpace(prefix) ? $"{prefix}" : "";
        string extension = !String.IsNullOrWhiteSpace(ext) ? $".{ext}" : "";
        if (!String.IsNullOrWhiteSpace(path) && !path.EndsWith("/"))
            path += "/";
        _logger.Debug($"GetPath returning [{path}{name}{extension}]");
        return $"{path}{name}{extension}";
    }
}