namespace BlogApi.Services;

public class ServiceResponse<T>
{
    public T? Data { get; set; } = default;
    public bool IsSuccessful { get; set; } = true;
    public string ErrorMessage { get; set; } = string.Empty;
}