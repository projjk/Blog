using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Blog.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly string _fromName;
    private readonly string _fromEmail;

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _logger = logger;
        _apiKey = config["MailJet:ApiKey"];
        _apiSecret = config["MailJet:ApiSecret"];
        _fromName = config["MailJet:FromName"];
        _fromEmail = config["MailJet:FromEmail"];
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var client = new MailjetClient(_apiKey, _apiSecret);
        var request = new MailjetRequest { Resource = Send.Resource };
        var email = new TransactionalEmailBuilder()
            .WithFrom(new SendContact("noreply@projectcode9.com", _fromName))
            .WithSubject(subject)
            .WithHtmlPart(message)
            .WithTo(new SendContact(toEmail))
            .Build();
        var response = await client.SendTransactionalEmailAsync(email);

        foreach (var mem in response.Messages)
        {
            if (mem.Errors != null)
            {
                _logger.LogWarning($"ErrorMessage: {mem.Errors[0].ErrorMessage}");
            }
        }
    }
}