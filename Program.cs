using System.Globalization;
using StockQuoteAlert;

CultureInfo ptBr = new CultureInfo("pt-BR");

if (args.Length != 3)
{
    ShowUsage($"Informe exatamente 3 parâmetros (recebidos: {args.Length}).");
    return 1;
}

string symbol = args[0].Trim().ToUpperInvariant();

if (string.IsNullOrWhiteSpace(symbol))
{
    ShowUsage("O código do ativo não foi informado.");
    return 1;
}

if (!decimal.TryParse(args[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal sellPrice)
    || sellPrice <= 0)
{
    ShowUsage($"Preço de venda inválido: '{args[1]}'. Use um número positivo com ponto decimal (ex.: 22.67).");
    return 1;
}

if (!decimal.TryParse(args[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal buyPrice)
    || buyPrice <= 0)
{
    ShowUsage($"Preço de compra inválido: '{args[2]}'. Use um número positivo com ponto decimal (ex.: 22.59).");
    return 1;
}

if (sellPrice <= buyPrice)
{
    ShowUsage("O preço de venda deve ser maior que o preço de compra.");
    return 1;
}

string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
Settings settings;

try
{
    settings = Settings.Load(configPath);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Erro: {ex.Message}");
    return 1;
}

QuoteService quoteService = new QuoteService();
EmailService emailService = new EmailService(settings);

using CancellationTokenSource cancellation = new CancellationTokenSource();

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    cancellation.Cancel();
    Console.WriteLine("Encerrando o monitoramento...");
};

string sellText = sellPrice.ToString("C", ptBr);
string buyText = buyPrice.ToString("C", ptBr);

bool sellAlertSent = false;
bool buyAlertSent = false;

Console.WriteLine($"Monitorando {symbol} | Venda acima de {sellText} | Compra abaixo de {buyText} | Intervalo: {settings.IntervalSeconds} s");
Console.WriteLine("Pressione Ctrl+C para encerrar.");
Console.WriteLine();

while (!cancellation.IsCancellationRequested)
{
    string now = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

    try
    {
        decimal price = await quoteService.GetPriceAsync(symbol);
        string priceText = price.ToString("C", ptBr);

        if (price > sellPrice)
        {
            Console.WriteLine($"[{now}] {symbol}: {priceText} | ALERTA DE VENDA (acima de {sellText})");
            buyAlertSent = false;

            if (!sellAlertSent)
            {
                sellAlertSent = await SendAlertAsync(
                    emailService,
                    $"Alerta de venda: {symbol} acima de {sellText}",
                    $"A cotação de {symbol} está em {priceText}, acima do preço de referência para venda ({sellText}).\n\n" +
                    $"Recomendação: venda.\nConsulta realizada em {now}.");
            }
        }
        else if (price < buyPrice)
        {
            Console.WriteLine($"[{now}] {symbol}: {priceText} | ALERTA DE COMPRA (abaixo de {buyText})");
            sellAlertSent = false;

            if (!buyAlertSent)
            {
                buyAlertSent = await SendAlertAsync(
                    emailService,
                    $"Alerta de compra: {symbol} abaixo de {buyText}",
                    $"A cotação de {symbol} está em {priceText}, abaixo do preço de referência para compra ({buyText}).\n\n" +
                    $"Recomendação: compra.\nConsulta realizada em {now}.");
            }
        }
        else
        {
            Console.WriteLine($"[{now}] {symbol}: {priceText} | Dentro da faixa");
            sellAlertSent = false;
            buyAlertSent = false;
        }
    }
    catch (QuoteException ex)
    {
        Console.WriteLine($"[{now}] Erro na consulta: {ex.Message} Nova tentativa no próximo ciclo.");
    }

    try
    {
        await Task.Delay(TimeSpan.FromSeconds(settings.IntervalSeconds), cancellation.Token);
    }
    catch (OperationCanceledException)
    {
    }
}

Console.WriteLine("Monitoramento encerrado.");
return 0;

static async Task<bool> SendAlertAsync(EmailService emailService, string subject, string body)
{
    try
    {
        await emailService.SendAsync(subject, body);
        Console.WriteLine("    E-mail de alerta enviado.");
        return true;
    }
    catch (EmailException ex)
    {
        Console.WriteLine($"    {ex.Message} Nova tentativa no próximo ciclo.");
        return false;
    }
}

static void ShowUsage(string error)
{
    Console.WriteLine($"Erro: {error}");
    Console.WriteLine();
    Console.WriteLine("Uso: stock-quote-alert <ATIVO> <PRECO_VENDA> <PRECO_COMPRA>");
    Console.WriteLine("Exemplo: stock-quote-alert PETR4 22.67 22.59");
}