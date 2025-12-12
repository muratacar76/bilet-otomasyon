using FlightBooking.Application.DTOs;
using FlightBooking.Application.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace FlightBooking.Infrastructure.Services;

/// <summary>
/// Email service implementation
/// </summary>
public class EmailService : IEmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(
        string smtpServer = "smtp.gmail.com",
        int smtpPort = 587,
        string smtpUsername = "",
        string smtpPassword = "",
        string fromEmail = "noreply@bulutbilet.com",
        string fromName = "Bulut Bilet.com")
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _smtpUsername = smtpUsername;
        _smtpPassword = smtpPassword;
        _fromEmail = fromEmail;
        _fromName = fromName;
    }

    public async Task SendBookingConfirmationAsync(BookingDto booking)
    {
        var subject = $"Rezervasyon Onayı - {booking.BookingReference}";
        var body = GenerateBookingConfirmationHtml(booking);
        
        await SendEmailAsync(booking.User?.Email ?? "", subject, body);
    }

    public async Task SendPaymentConfirmationAsync(BookingDto booking)
    {
        var subject = $"Ödeme Onayı - {booking.BookingReference}";
        var body = GeneratePaymentConfirmationHtml(booking);
        
        await SendEmailAsync(booking.User?.Email ?? "", subject, body);
    }

    public async Task SendCancellationNotificationAsync(BookingDto booking)
    {
        var subject = $"Rezervasyon İptali - {booking.BookingReference}";
        var body = GenerateCancellationNotificationHtml(booking);
        
        await SendEmailAsync(booking.User?.Email ?? "", subject, body);
    }

    public async Task SendWelcomeEmailAsync(UserDto user)
    {
        var subject = "Bulut Bilet.com'a Hoş Geldiniz!";
        var body = GenerateWelcomeEmailHtml(user);
        
        await SendEmailAsync(user.Email, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        var subject = "Şifre Sıfırlama - Bulut Bilet.com";
        var body = GeneratePasswordResetHtml(resetToken);
        
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendBookingConfirmationAsync(string email, string pnr, Core.Entities.Flight flight, List<string> passengerNames)
    {
        var subject = $"Rezervasyon Onayı - {pnr}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #00bcd4; text-align: center;'>🎉 Rezervasyon Onayı</h2>
                    
                    <div style='background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #333; margin-bottom: 15px;'>Rezervasyon Bilgileri</h3>
                        <p><strong>PNR:</strong> {pnr}</p>
                        <p><strong>Uçuş:</strong> {flight.FlightNumber} - {flight.Airline}</p>
                        <p><strong>Rota:</strong> {flight.DepartureCity} → {flight.ArrivalCity}</p>
                        <p><strong>Kalkış:</strong> {flight.DepartureTime:dd.MM.yyyy HH:mm}</p>
                        <p><strong>Varış:</strong> {flight.ArrivalTime:dd.MM.yyyy HH:mm}</p>
                        <p><strong>Yolcular:</strong> {string.Join(", ", passengerNames)}</p>
                    </div>
                    
                    <div style='background: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <p style='margin: 0; color: #856404;'>
                            💡 Ödeme yapmak için PNR sorgulama sayfasını kullanabilirsiniz.
                        </p>
                    </div>
                    
                    <p style='text-align: center; margin-top: 30px;'>
                        <a href='https://bulutbilet.com' style='color: #00bcd4;'>BULUTBİLET.COM</a>
                    </p>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // For development, we'll just log the email instead of sending
            if (string.IsNullOrEmpty(_smtpUsername))
            {
                Console.WriteLine($"[EMAIL] To: {toEmail}, Subject: {subject}");
                Console.WriteLine($"[EMAIL] Body: {htmlBody}");
                return;
            }

            await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Log error but don't throw to prevent breaking the main flow
            Console.WriteLine($"Email sending failed: {ex.Message}");
        }
    }

    private string GenerateBookingConfirmationHtml(BookingDto booking)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <title>Rezervasyon Onayı</title>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; }}
                .header {{ text-align: center; color: #00bcd4; margin-bottom: 30px; }}
                .booking-info {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                .passenger-list {{ margin: 20px 0; }}
                .passenger {{ padding: 10px; border-bottom: 1px solid #eee; }}
                .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>🎉 Rezervasyonunuz Onaylandı!</h1>
                    <h2>Bulut Bilet.com</h2>
                </div>
                
                <div class='booking-info'>
                    <h3>📋 Rezervasyon Bilgileri</h3>
                    <p><strong>PNR Numarası:</strong> {booking.BookingReference}</p>
                    <p><strong>Uçuş:</strong> {booking.Flight?.FlightNumber} - {booking.Flight?.Route}</p>
                    <p><strong>Kalkış:</strong> {booking.Flight?.DepartureTime:dd.MM.yyyy HH:mm}</p>
                    <p><strong>Yolcu Sayısı:</strong> {booking.PassengerCount}</p>
                    <p><strong>Toplam Tutar:</strong> ₺{booking.TotalPrice:N2}</p>
                    <p><strong>Durum:</strong> {booking.StatusDisplay}</p>
                </div>

                <div class='passenger-list'>
                    <h3>👥 Yolcu Bilgileri</h3>
                    {string.Join("", booking.Passengers.Select(p => $@"
                    <div class='passenger'>
                        <strong>{p.FullName}</strong> - Koltuk: {p.SeatNumber} ({p.SeatTypeDisplay})
                    </div>"))}
                </div>

                <div class='footer'>
                    <p>Bu e-posta otomatik olarak gönderilmiştir.</p>
                    <p>Bulut Bilet.com - Hayalinizdeki Yolculuğa Başlayın</p>
                </div>
            </div>
        </body>
        </html>";
    }

    private string GeneratePaymentConfirmationHtml(BookingDto booking)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <title>Ödeme Onayı</title>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; }}
                .header {{ text-align: center; color: #4caf50; margin-bottom: 30px; }}
                .payment-info {{ background-color: #e8f5e8; padding: 20px; border-radius: 8px; margin: 20px 0; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>✅ Ödemeniz Alındı!</h1>
                    <h2>Bulut Bilet.com</h2>
                </div>
                
                <div class='payment-info'>
                    <h3>💳 Ödeme Bilgileri</h3>
                    <p><strong>PNR Numarası:</strong> {booking.BookingReference}</p>
                    <p><strong>Ödeme Tarihi:</strong> {booking.PaymentDate:dd.MM.yyyy HH:mm}</p>
                    <p><strong>Tutar:</strong> ₺{booking.TotalPrice:N2}</p>
                    <p><strong>Durum:</strong> Ödendi ✅</p>
                </div>

                <p>Biletiniz hazır! İyi yolculuklar dileriz.</p>
            </div>
        </body>
        </html>";
    }

    private string GenerateCancellationNotificationHtml(BookingDto booking)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <title>Rezervasyon İptali</title>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; }}
                .header {{ text-align: center; color: #f44336; margin-bottom: 30px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>❌ Rezervasyon İptal Edildi</h1>
                    <h2>Bulut Bilet.com</h2>
                </div>
                
                <p><strong>PNR Numarası:</strong> {booking.BookingReference}</p>
                <p><strong>İptal Tarihi:</strong> {booking.CancellationDate:dd.MM.yyyy HH:mm}</p>
                
                <p>Rezervasyonunuz başarıyla iptal edilmiştir. İade işlemleri 3-5 iş günü içinde hesabınıza yansıyacaktır.</p>
            </div>
        </body>
        </html>";
    }

    private string GenerateWelcomeEmailHtml(UserDto user)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <title>Hoş Geldiniz</title>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; }}
                .header {{ text-align: center; color: #00bcd4; margin-bottom: 30px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>🎉 Hoş Geldiniz!</h1>
                    <h2>Bulut Bilet.com</h2>
                </div>
                
                <p>Merhaba {user.FirstName},</p>
                <p>Bulut Bilet.com ailesine katıldığınız için teşekkür ederiz!</p>
                <p>Artık en uygun fiyatlarla uçak biletlerinizi rezerve edebilir, koltuk seçimi yapabilir ve rezervasyonlarınızı kolayca yönetebilirsiniz.</p>
                
                <p>İyi yolculuklar dileriz! ✈️</p>
            </div>
        </body>
        </html>";
    }

    private string GeneratePasswordResetHtml(string resetToken)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <title>Şifre Sıfırlama</title>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; }}
                .header {{ text-align: center; color: #00bcd4; margin-bottom: 30px; }}
                .reset-code {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; text-align: center; font-size: 24px; font-weight: bold; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>🔐 Şifre Sıfırlama</h1>
                    <h2>Bulut Bilet.com</h2>
                </div>
                
                <p>Şifrenizi sıfırlamak için aşağıdaki kodu kullanın:</p>
                
                <div class='reset-code'>
                    {resetToken}
                </div>
                
                <p>Bu kod 15 dakika geçerlidir.</p>
                <p>Eğer bu talebi siz yapmadıysanız, bu e-postayı görmezden gelebilirsiniz.</p>
            </div>
        </body>
        </html>";
    }
}