namespace WEB.Services;

public interface IRestProvider
{
    Task<string> GetAsync(string endpoint);
    Task<string> PostAsync(string endpoint, string json);
    Task<string> PutAsync(string endpoint, string json);
    Task<string> DeleteAsync(string endpoint);
    Task<string> PatchAsync(string endpoint, string json);
}
