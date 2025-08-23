using System.Net;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using p42BaseLib;

namespace p42ObjectStores;

public class GOObjectStore : BaseStore
{
    readonly string _accessKey;
    readonly string _bucketName;
    AmazonS3Client? _client;
    readonly P42Logger _logger = new();
    readonly string _secretKey;
    readonly string _serviceUrl;


    public GOObjectStore(string accessKey, string secretKey, string serviceUrl, string bucketName)
    {
        _accessKey = accessKey;
        _secretKey = secretKey;
        _serviceUrl = serviceUrl;
        _bucketName = bucketName;
        createClient();
    }

    void createClient()
    {
        _client = new AmazonS3Client(_accessKey, _secretKey, new AmazonS3Config
        {
            ServiceURL = _serviceUrl
        });
        if (_client == null)
            _logger.Info("GOObjectStore creation failed");
        else
            _logger.Info("GOObjectStore created");
    }

    public override int NumberOfObject(string? prefix = null)
    {
        if (_client == null) return 0;

        try
        {
            int total = 0;
            ListObjectsV2Request request = new()
            {
                BucketName = _bucketName,
                Prefix = prefix
            };

            ListObjectsV2Response response;
            do
            {
                response = _client.ListObjectsV2Async(request).GetAwaiter().GetResult();
                if (response?.S3Objects != null) total += response.S3Objects.Count;

                request.ContinuationToken = response?.NextContinuationToken;
            } while ((bool)response.IsTruncated!);

            return total;
        }
        catch (Exception ex)
        {
            _logger.Info($"GOObjectStore.NumberOfObject failed: {ex.Message}");
            return 0;
        }
    }

    public override async Task<List<T>> GetAll<T>(string name = "", string? prefix = null)
    {
        List<T> results = new();

        try
        {
            if (_client == null) return results;

            // Build listing prefix (treating 'name' as a subfolder or key prefix under the optional 'prefix')
            string listPrefix;
            if (String.IsNullOrWhiteSpace(prefix))
                listPrefix = String.IsNullOrWhiteSpace(name) ? String.Empty : name;
            else
                listPrefix = String.IsNullOrWhiteSpace(name)
                    ? $"{prefix.TrimEnd('/')}/"
                    : $"{prefix.TrimEnd('/')}/{name}";

            ListObjectsV2Request request = new()
            {
                BucketName = _bucketName,
                Prefix = listPrefix
                //MaxKeys = 1000 //this is the default 
            };

            ListObjectsV2Response response;
            do
            {
                response = await _client.ListObjectsV2Async(request);

                if (response?.S3Objects != null)
                    foreach (S3Object? s3Obj in response.S3Objects)
                        try
                        {
                            using GetObjectResponse? getResp = await _client.GetObjectAsync(new GetObjectRequest
                            {
                                BucketName = _bucketName,
                                Key = s3Obj.Key
                            });
                            T? model = await JsonSerializer.DeserializeAsync<T>(getResp.ResponseStream);
                            if (model != null)
                                results.Add(model);
                        }
                        catch (Exception exObj)
                        {
                            _logger.Info($"GOObjectStore.GetAll skipped key '{s3Obj.Key}': {exObj.Message}");
                        }

                request.ContinuationToken = response?.NextContinuationToken;
            } while (response != null && (bool)response.IsTruncated!);

            return results;
        }
        catch (Exception ex)
        {
            _logger.Info($"GOObjectStore.GetAll failed: {ex.Message}");
            return results;
        }
    }

    public override async Task<T?> Get<T>(string name, string? prefix = null) where T : class
    {
        if (String.IsNullOrWhiteSpace(name) || _client == null)
            return null;
        try
        {
            GetObjectRequest request = new()
            {
                BucketName = _bucketName,
                Key = GetPath(name, "", prefix)
            };
            using GetObjectResponse? getResponse = await _client.GetObjectAsync(request);
            // If we got here, the object exists. Optionally ensure 2xx.
            if ((int)getResponse.HttpStatusCode < 200 || (int)getResponse.HttpStatusCode >= 300)
                return null;

            using StreamReader reader = new(getResponse.ResponseStream);
            //string content = reader.ReadToEnd();
            T? model = JsonSerializer.Deserialize<T>(await reader.ReadToEndAsync());
            return model;
        }
        catch (Exception e)
        {
            _logger.Info($"GOObjectStore.Get failed: {e.Message}");
            return null;
        }
    }

    public override async Task<T?> Add<T>(T model, string name, string? prefix = null) where T : class
    {
        try
        {
            JsonSerializerOptions? jsonOptions = new()
            {
                WriteIndented = true
            };

            string fn = GetPath(name, "", prefix);
            PutObjectRequest request = new()
            {
                BucketName = _bucketName,
                Key = fn,
                ContentBody = JsonSerializer.Serialize(model, jsonOptions)
            };
            PutObjectResponse response = await _client.PutObjectAsync(request);
            if ((int)response.HttpStatusCode >= 200 && (int)response.HttpStatusCode < 300) return model;
        }
        catch (Exception e)
        {
            _logger.Info($"GOObjectStore.Add failed: {e.Message}");
        }

        return null;
    }

    public override bool Delete(string name, string? prefix = null)
    {
        try
        {
            if (String.IsNullOrWhiteSpace(name) || _client == null)
                return false;
            DeleteObjectRequest request = new()
            {
                BucketName = _bucketName,
                Key = GetPath(name, "", prefix)
            };
            _logger.Info($"Delete object [{name}] ");
            DeleteObjectResponse? response = _client.DeleteObjectAsync(request).GetAwaiter().GetResult();
            return (int)response.HttpStatusCode >= 200 && (int)response.HttpStatusCode < 300;
        }
        catch (Exception ex)
        {
            _logger.Info($"GOObjectStore.Delete failed: {ex.Message}");
            return false;
        }
    }

    bool IsObjectExisting(string name, string? prefix = null)
    {
        if (String.IsNullOrWhiteSpace(name) || _client == null)
            return false;
        try
        {
            GetObjectMetadataRequest metaRequest = new()
            {
                BucketName = _bucketName,
                Key = GetPath(name, "", prefix)
            };
            GetObjectMetadataResponse res = _client.GetObjectMetadataAsync(metaRequest).GetAwaiter().GetResult();
            return res.HttpStatusCode == HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.Info($"GOObjectStore.Update (GetObjectMetadata) failed: {ex.Message}");
            return false;
        }
    }

    public override bool Update<T>(string name, T model, string? prefix = null) where T : class
    {
        try
        {
            if (!IsObjectExisting(name, prefix))
            {
                _logger.Info($"GOObjectStore.Update: object [{name}] does not exist - adding");
                _ = Add<T>(model, name, prefix);
            }

            _logger.Info($"GOObjectStore.Update: object [{name}] is existing");

            JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
            PutObjectRequest putRequest = new()
            {
                BucketName = _bucketName,
                Key = GetPath(name, "", prefix),
                ContentBody = JsonSerializer.Serialize(model, jsonOptions)
            };

            PutObjectResponse? response = _client.PutObjectAsync(putRequest).GetAwaiter().GetResult();
            return (int)response.HttpStatusCode >= 200 && (int)response.HttpStatusCode < 300;
        }
        catch (Exception ex)
        {
            _logger.Info($"GOObjectStore.Update failed: {ex.Message}");
            return false;
        }
    }
}