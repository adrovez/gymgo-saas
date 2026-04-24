namespace GymGo.Application.Common.Models;

/// <summary>
/// Result pattern liviano para operaciones que pueden fallar sin
/// recurrir a excepciones (validaciones de negocio, búsquedas que
/// devuelven not-found "esperado", etc.). Las excepciones siguen
/// siendo válidas para errores realmente excepcionales.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Un Result exitoso no puede tener error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Un Result fallido debe tener error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No se puede acceder a Value de un Result fallido.");
}

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("ERROR_NULL_VALUE", "Se proporcionó un valor nulo.");
}
