using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace StockQuoteAlert;

public class EmailException : Exception
{
    public EmailException(string message) : base(message) { }
}

public class EmailService
{
    private readonly EmailSettings _email;
    private readonly SmtpSettings _smtp;

    public EmailService(Settings settings)
    {
        _email = settings.Email;
        _smtp = settings.Smtp;
    }

    public async Task SendAsync(string subject, string body)
    {
        try
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_email.From));
            message.To.Add(MailboxAddress.Parse(_email.To));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using SmtpClient client = new SmtpClient();
            client.CheckCertificateRevocation = false;
            await client.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.Auto);
            await client.AuthenticateAsync(_smtp.User, _smtp.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (AuthenticationException)
        {
            throw new EmailException("O servidor SMTP recusou o usuário ou a senha. Verifique Smtp.User e Smtp.Password.");
        }
        catch (ParseException)
        {
            throw new EmailException("Endereço de e-mail inválido em Email.From ou Email.To.");
        }
        catch (Exception ex)
        {
            throw new EmailException($"Falha ao enviar o e-mail: {ex.Message}");
        }
    }
}