namespace SharedLibrary.Configuration;

/// <summary>
/// Configuration settings for Redis
/// </summary>
public class RedisSettings
{
    /// <summary>
    /// The section name in the configuration file
    /// </summary>
    public const string SectionName = "Redis";

    /// <summary>
    /// The Redis connection string
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// The instance name (prefix for keys)
    /// </summary>
    public string? InstanceName { get; set; }

    /// <summary>
    /// Default TTL (Time To Live) for cache entries in minutes
    /// </summary>
    public int DefaultTtlMinutes { get; set; }

    /// <summary>
    /// Whether to enable SSL/TLS
    /// </summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Connection retry count
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Whether to abort connect on failure
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;

    /// <summary>
    /// Whether to allow admin operations
    /// </summary>
    public bool AllowAdmin { get; set; } = false;

    /// <summary>
    /// Database ID to use
    /// </summary>
    public int Database { get; set; } = 0;
}