using Microsoft.AspNetCore.Mvc;

namespace DarkKitchen.Testing.Http;

public static class HttpTestExtensions
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<T> ReadJsonAsync<T>(this HttpResponseMessage response)
    {
        var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return value ?? throw new InvalidOperationException("Response body was empty.");
    }

    public static async Task<string> ReadBodyAsync(this HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task AssertSuccessAsync(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.ReadBodyAsync();
        Assert.Fail($"Expected success but got {(int)response.StatusCode} {response.StatusCode}: {body}");
    }

    public static async Task AssertStatusCodeAsync(this HttpResponseMessage response, HttpStatusCode expectedStatusCode)
    {
        if (response.StatusCode == expectedStatusCode)
        {
            return;
        }

        var body = await response.ReadBodyAsync();
        Assert.Fail($"Expected {(int)expectedStatusCode} {expectedStatusCode} but got {(int)response.StatusCode} {response.StatusCode}: {body}");
    }

    public static async Task<ValidationProblemDetails> ReadValidationProblemAsync(this HttpResponseMessage response)
    {
        await response.AssertStatusCodeAsync(HttpStatusCode.BadRequest);
        return await response.ReadJsonAsync<ValidationProblemDetails>();
    }
}
