namespace ECommerceApi.Api.Exceptions;

// 404
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

// 400 - the request is well-formed but violates a business rule
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

// 409 - the request conflicts with the current state of a resource
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}