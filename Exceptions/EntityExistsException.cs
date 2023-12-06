namespace BlogApi.Exceptions;

public class EntityExistsException : Exception
{
    public EntityExistsException()
    {
        
    }

    public EntityExistsException(string message) : base(message)
    {
        
    }
}