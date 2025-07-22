using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using System.Globalization;
using AuctionSite.Resources;

namespace AuctionSite.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger, IStringLocalizer<SharedResources> localizer)
        {
            _configuration = configuration;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var currentCulture = CultureInfo.CurrentUICulture; 
            var smtpHost = _configuration["SmtpSettings:Host"];
            var smtpPort = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
            var smtpUser = _configuration["SmtpSettings:Username"];
            var smtpPass = _configuration["SmtpSettings:Password"];
            var senderEmail = _configuration["SmtpSettings:SenderEmail"];
            var enableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"] ?? "true");

            var actualSenderEmail = string.IsNullOrEmpty(senderEmail) ? smtpUser : senderEmail;

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                _logger.LogError(SharedResources.ResourceManager.GetString("SmtpSettingsMissing", currentCulture) ?? "");
                return;
            }

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = enableSsl;
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                try
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(actualSenderEmail!, SharedResources.ResourceManager.GetString("AuctionSiteSupport", currentCulture) ?? ""),
                        To = { email },
                        Subject = subject,
                        Body = message,
                        IsBodyHtml = true
                    };

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation(string.Format(SharedResources.ResourceManager.GetString("EmailSentSuccessfully", currentCulture) ?? "", subject, email));
                }
                catch (SmtpException ex)
                {
                    _logger.LogError(ex, string.Format(SharedResources.ResourceManager.GetString("SmtpErrorSendingEmail", currentCulture) ?? "", ex.StatusCode, ex.Message));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, string.Format(SharedResources.ResourceManager.GetString("UnexpectedErrorSendingEmail", currentCulture) ?? "", ex.Message));
                }
            }
        }
    }
}
