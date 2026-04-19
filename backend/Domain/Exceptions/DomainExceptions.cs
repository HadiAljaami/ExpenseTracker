namespace Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object id)
        : base($"{entity} with ID '{id}' was not found.") { }

    public NotFoundException(string message)
        : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message) { }
}

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
