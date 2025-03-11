using System.Net;
using System.Text.Json.Serialization;

namespace SharedLibrary.Models;

/// <summary>
/// Represents a basic result without data, only success/failure status
/// </summary>
public class Result : IResult
{
    public bool IsSuccess { get; private set; }
    public object? Error { get; private set; }
    public string? Message { get; private set; }
    public HttpStatusCode StatusCode { get; private set; }

    [JsonIgnore]
    private Dictionary<string, string> SetHeaders = new Dictionary<string, string>();

    [JsonIgnore]
    public IReadOnlyDictionary<string, string> Headers => SetHeaders;

    private Result(bool isSuccess, object? error = null, string? message = null, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        IsSuccess = isSuccess;
        Error = error;
        Message = message;
        StatusCode = statusCode;
    }

    public void AddHeader(string key, string value)
    {
        SetHeaders[key] = value;
    }

    public IReadOnlyDictionary<string, string> GetHeaders()
    {
        return Headers;
    }

    public static Result Success() => new(true);

    public static Result Success(string message) => new(true, null, message);

    public static Result Failure(object error) => new(false, error);

    public static Result Failure(object error, string message) => new(false, error, message);

    public static Result Failure(object error, HttpStatusCode statusCode) =>
        new(false, error, null, statusCode);

    public static Result Failure(object error, string message, HttpStatusCode statusCode) =>
        new(false, error, message, statusCode);

    public Result<T> ToTyped<T>(T? data = default) => IsSuccess
        ? Result<T>.Success(data)
        : Result<T>.Failure(Error ?? "Unknown error");

    /// <summary>
    /// Converts the result to a different type using a mapping function
    /// </summary>
    public Result<TResult> Map<TResult>(Func<TResult> mapper)
    {
        if (!IsSuccess)
            return Result<TResult>.Failure(Error ?? "Unknown error");

        try
        {
            return Result<TResult>.Success(mapper());
        }
        catch (Exception ex)
        {
            return Result<TResult>.Failure(ex);
        }
    }
}

/// <summary>
/// Represents a result with data of type T
/// </summary>
public class Result<T> : IResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public object? Error { get; private set; }
    public string? Message { get; private set; }
    public HttpStatusCode StatusCode { get; private set; }

    [JsonIgnore]
    private Dictionary<string, string> SetHeaders = new Dictionary<string, string>();

    [JsonIgnore]
    public IReadOnlyDictionary<string, string> Headers => SetHeaders;

    private Result(bool isSuccess, T? data, object? error, string? message = null, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        Message = message;
        StatusCode = statusCode;
    }

    public void AddHeader(string key, string value)
    {
        SetHeaders[key] = value;
    }

    public IReadOnlyDictionary<string, string> GetHeaders()
    {
        return Headers;
    }

    public static Result<T> Success(T? data) => new(true, data, null);

    public static Result<T> Success(T data, string message) => new(true, data, null, message);

    public static Result<T> Failure(object error) => new(false, default, error);

    public static Result<T> Failure(object error, string message) => new(false, default, error, message);

    public static Result<T> Failure(object error, HttpStatusCode statusCode) =>
        new(false, default, error, null, statusCode);

    public static Result<T> Failure(object error, string message, HttpStatusCode statusCode) =>
        new(false, default, error, message, statusCode);

    /// <summary>
    /// Creates a Result from another Result with a different data type
    /// </summary>
    public static Result<T> From<TSource>(Result<TSource> result, Func<TSource, T> mapper)
    {
        if (!result.IsSuccess)
            return Failure(result.Error ?? "Unknown error");

        if (result.Data == null)
            return Failure("Source data is null");

        try
        {
            return Success(mapper(result.Data));
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Maps the result to a new result with a different data type
    /// </summary>
    public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        if (!IsSuccess)
            return Result<TResult>.Failure(Error ?? "Unknown error");

        if (Data == null)
            return Result<TResult>.Failure("Data is null");

        try
        {
            return Result<TResult>.Success(mapper(Data));
        }
        catch (Exception ex)
        {
            return Result<TResult>.Failure(ex);
        }
    }

    /// <summary>
    /// Binds the result to a new result with a different data type
    /// </summary>
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder)
    {
        if (!IsSuccess)
            return Result<TResult>.Failure(Error ?? "Unknown error");

        if (Data == null)
            return Result<TResult>.Failure("Data is null");

        try
        {
            return binder(Data);
        }
        catch (Exception ex)
        {
            return Result<TResult>.Failure(ex);
        }
    }

    /// <summary>
    /// Executes an action if the result is successful
    /// </summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess && Data != null)
        {
            try
            {
                action(Data);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        return this;
    }

    /// <summary>
    /// Executes an action if the result is a failure
    /// </summary>
    public Result<T> OnFailure(Action<object> action)
    {
        if (!IsSuccess && Error != null)
        {
            try
            {
                action(Error);
            }
            catch
            {
                // Ignore exceptions in the failure handler
            }
        }

        return this;
    }
}