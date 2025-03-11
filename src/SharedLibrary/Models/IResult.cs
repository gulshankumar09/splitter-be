using System.Net;
using System.Text.Json.Serialization;

namespace SharedLibrary.Models;

/// <summary>
/// Represents the result of an operation
/// </summary>
public interface IResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets the error information if the operation failed
    /// </summary>
    object? Error { get; }

    /// <summary>
    /// Gets a message associated with the result
    /// </summary>
    string? Message { get; }

    /// <summary>
    /// Gets the HTTP status code associated with the result
    /// </summary>
    HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the headers associated with the result
    /// </summary>
    [JsonIgnore]
    IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>
/// Represents the result of an operation that returns data
/// </summary>
/// <typeparam name="T">The type of data returned by the operation</typeparam>
public interface IResult<out T> : IResult
{
    /// <summary>
    /// Gets the data if the operation was successful
    /// </summary>
    T? Data { get; }
}