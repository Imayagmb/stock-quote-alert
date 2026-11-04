using System.Text.Json;

namespace StockQuoteAlert;

public class Settings
{
    public EmailSettings Email { get; set; } = new();
    public SmtpSettings Smtp { get; set; } = new();
    public int IntervalSeconds { get; set; } = 300;

    public static Settings Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                $"Arquivo de configuração não encontrado: {path}\n" +
                "Copie o appsettings.example.json para appsettings.json e preencha os dados.");
        }

        string json = File.ReadAllText(path);

        Settings? settings;
        try
        {
            settings = JsonSerializer.Deserialize<Settings>(json);
        }
        catch (JsonException ex)
        {

            throw new InvalidOperationException($"O appsettings.json está com formato inválido: {ex.Message}");
        }

        if (settings == null)
        {
            throw new InvalidOperationException("O appsettings.json está vazio.");
        }

        settings.Validate();
        return settings;
    }

    private void Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Email.To))
            errors.Add("Email.To (destinatário) não informado.");
        if (string.IsNullOrWhiteSpace(Email.From))
            errors.Add("Email.From (remetente) não informado.");
        if (string.IsNullOrWhiteSpace(Smtp.Host))
            errors.Add("Smtp.Host não informado.");
        if (Smtp.Port <= 0)
            errors.Add("Smtp.Port deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(Smtp.User))
            errors.Add("Smtp.User não informado.");
        if (string.IsNullOrWhiteSpace(Smtp.Password))
            errors.Add("Smtp.Password não informado.");
        if (IntervalSeconds <= 0)
            errors.Add("IntervalSeconds deve ser maior que zero.");


        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Configuração inválida no appsettings.json:\n - " + string.Join("\n - ", errors));
        }
    }
}

public class EmailSettings
{
    public string To { get; set; } = "";
    public string From { get; set; } = "";
}

public class SmtpSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
}