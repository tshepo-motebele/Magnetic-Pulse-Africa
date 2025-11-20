using MagneticPulseAfrica.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace MagneticPulseAfrica.Services
{
    public interface ISmsService
    {
        Task SendConsultationNotificationAsync(Consultation consultation);
    }

    public class TwilioSmsService : ISmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;
        private readonly string _toNumber;

        public TwilioSmsService(IConfiguration configuration)
        {
            _accountSid = configuration["Twilio:AccountSid"];
            _authToken = configuration["Twilio:AuthToken"];
            _fromNumber = configuration["Twilio:FromNumber"];
            _toNumber = configuration["Twilio:ToNumber"]; // Your number to receive notifications

            TwilioClient.Init(_accountSid, _authToken);
        }

        public async Task SendConsultationNotificationAsync(Consultation consultation)
        {
            // Make message shorter to accommodate trial prefix
            var messageBody = $"Booking: {consultation.FirstName} {consultation.LastName}" +
                             $"\nContact: {consultation.ContactNumber}" +
                             $"\nReason: {consultation.ConsultationReason}";

            try
            {
                var message = await MessageResource.CreateAsync(
                    body: messageBody,
                    from: new PhoneNumber(_fromNumber),
                    to: new PhoneNumber(_toNumber)
                );

                // You might want to log successful sends
                Console.WriteLine($"SMS sent with SID: {message.Sid}");
            }
            catch (Exception ex)
            {
                // Common trial account errors:
                // - Attempting to send to unverified numbers
                // - Exceeding trial balance
                Console.WriteLine($"Failed to send SMS: {ex.Message}");
            }
        }
    }
}