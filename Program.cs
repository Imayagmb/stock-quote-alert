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

Console.WriteLine($"Ativo monitorado: {symbol}");
Console.WriteLine($"Limite de venda:  {sellPrice.ToString("C", ptBr)}");
Console.WriteLine($"Limite de compra: {buyPrice.ToString("C", ptBr)}");
Console.WriteLine($"Intervalo:        {settings.IntervalSeconds} segundos");

QuoteService quoteService = new QuoteService();

try
{
    decimal price = await quoteService.GetPriceAsync(symbol);
    Console.WriteLine($"Cotação atual:    {price.ToString("C", ptBr)}");
}
catch (QuoteException ex)
{
    Console.WriteLine($"Erro na consulta: {ex.Message}");
}
EmailService emailService = new EmailService(settings);

try
{
    await emailService.SendAsync(
        "Teste stock-quote-alert",
        "Se você recebeu este e-mail, o envio está funcionando.");
    Console.WriteLine("E-mail de teste enviado.");
}
catch (EmailException ex)
{
    Console.WriteLine($"Erro no e-mail: {ex.Message}");
}
return 0;

static void ShowUsage(string error)
{
    Console.WriteLine($"Erro: {error}");
    Console.WriteLine();
    Console.WriteLine("Uso: stock-quote-alert <ATIVO> <PRECO_VENDA> <PRECO_COMPRA>");
    Console.WriteLine("Exemplo: stock-quote-alert PETR4 22.67 22.59");
}

