using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace StockQuoteAlert;

public class QuoteException : Exception
{
    public QuoteException(string message) : base(message) { }
}

public class QuoteService
{
    private readonly HttpClient _httpClient;

    public QuoteService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://brapi.dev/"),
            Timeout = TimeSpan.FromSeconds(15)
        };

        string? token = Environment.GetEnvironmentVariable("BRAPI_TOKEN");
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<decimal> GetPriceAsync(string symbol)
    {
        string url = $"api/v2/stocks/quote?symbols={Uri.EscapeDataString(symbol)}";
        HttpStatusCode status;
        bool success;
        string body;

        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(url);
            status = response.StatusCode;
            success = response.IsSuccessStatusCode;
            body = await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            throw new QuoteException($"Falha de conexão com a API: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            throw new QuoteException("A API não respondeu dentro do tempo limite.");
        }

        if (status == HttpStatusCode.NotFound)
        {
            throw new QuoteException($"Nenhuma cotação encontrada para o ativo {symbol}.");
        }

        if (status == HttpStatusCode.Unauthorized || status == HttpStatusCode.Forbidden)
        {
            throw new QuoteException(
                $"Acesso negado pela API (HTTP {(int)status}). Verifique o token na variável de ambiente BRAPI_TOKEN.");
        }

        if (!success)
        {
            throw new QuoteException($"A API retornou erro HTTP {(int)status}.");
        }

        return ExtractPrice(body, symbol);
    }

    private static decimal ExtractPrice(string json, string symbol)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("results", out JsonElement results)
                || results.ValueKind != JsonValueKind.Array
                || results.GetArrayLength() == 0)
            {
                throw new QuoteException($"A API não retornou cotação para o ativo {symbol}.");
            }

            JsonElement first = results[0];

            if (first.ValueKind != JsonValueKind.Object
                || !first.TryGetProperty("data", out JsonElement data)
                || data.ValueKind != JsonValueKind.Object
                || !data.TryGetProperty("regularMarketPrice", out JsonElement price)
                || price.ValueKind != JsonValueKind.Number)
            {
                throw new QuoteException($"A resposta da API não contém o preço do ativo {symbol}.");
            }

            return price.GetDecimal();
        }
        catch (JsonException)
        {
            throw new QuoteException("A API retornou uma resposta em formato inválido.");
        }
    }
}