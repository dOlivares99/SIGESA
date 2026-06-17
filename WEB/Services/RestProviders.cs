using System.Net.Http.Headers;
using System.Text;

namespace WEB.Services;

internal static class RestProviderHelpers
{
    internal static HttpClient CreateHttpClient(string endpoint)
    {
        var client = new HttpClient { BaseAddress = new Uri(endpoint) };
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    internal static StringContent CreateContent(string content) =>
        new(content, Encoding.UTF8, "application/json");

    internal static async Task<string> GetResponse(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    internal static Exception ThrowError(string endpoint, Exception ex) =>
        new ApplicationException($"Error al conectar con {endpoint}", ex);
}

public class RestProvider : IRestProvider
{
    private readonly string _baseUrl;

    public RestProvider(IConfiguration configuration)
    {
        _baseUrl = configuration["ApiSettings:BaseUrl"]
            ?? "https://localhost:7001/api";
    }

    public async Task<string> GetAsync(string endpoint)
    {
        try
        {
            var response = await RestProviderHelpers
                .CreateHttpClient(_baseUrl)
                .GetAsync(endpoint);
            return await RestProviderHelpers.GetResponse(response);
        }
        catch (Exception ex)
        {
            throw RestProviderHelpers.ThrowError(endpoint, ex);
        }
    }

    public async Task<string> PostAsync(string endpoint, string json)
    {
        try
        {
            var response = await RestProviderHelpers
                .CreateHttpClient(_baseUrl)
                .PostAsync(endpoint, RestProviderHelpers.CreateContent(json));
            return await RestProviderHelpers.GetResponse(response);
        }
        catch (Exception ex)
        {
            throw RestProviderHelpers.ThrowError(endpoint, ex);
        }
    }

    public async Task<string> PutAsync(string endpoint, string json)
    {
        try
        {
            var response = await RestProviderHelpers
                .CreateHttpClient(_baseUrl)
                .PutAsync(endpoint, RestProviderHelpers.CreateContent(json));
            return await RestProviderHelpers.GetResponse(response);
        }
        catch (Exception ex)
        {
            throw RestProviderHelpers.ThrowError(endpoint, ex);
        }
    }

    public async Task<string> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await RestProviderHelpers
                .CreateHttpClient(_baseUrl)
                .DeleteAsync(endpoint);
            return await RestProviderHelpers.GetResponse(response);
        }
        catch (Exception ex)
        {
            throw RestProviderHelpers.ThrowError(endpoint, ex);
        }
    }

    public async Task<string> PatchAsync(string endpoint, string json)
    {
        try
        {
            var response = await RestProviderHelpers
                .CreateHttpClient(_baseUrl)
                .PatchAsync(endpoint, RestProviderHelpers.CreateContent(json));
            return await RestProviderHelpers.GetResponse(response);
        }
        catch (Exception ex)
        {
            throw RestProviderHelpers.ThrowError(endpoint, ex);
        }
    }
}