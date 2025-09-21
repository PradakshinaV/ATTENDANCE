using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AttendanceSystem.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly bool _enableSsl;

        public EmailService()
        {
            _smtpServer = ConfigurationManager.AppSettings["SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            _smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"] ?? "";
            _smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "";
            _fromEmail = ConfigurationManager.AppSettings["FromEmail"] ?? "noreply@attendance.com";
            _enableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSsl"] ?? "true");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = _enableSsl;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                    var message = new MailMessage(_fromEmail, toEmail)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = isHtml
                    };

                    await client.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                // Log the error (you might want to use a proper logging framework)
                System.Diagnostics.Debug.WriteLine($"Email sending failed: {ex.Message}");
                throw;
            }
        }

        public async Task SendAttendanceNotificationAsync(string studentEmail, string studentName, string className, string status)
        {
            var subject = $"Attendance Update - {className}";
            var body = GenerateAttendanceEmailBody(studentName, className, status);
            
            await SendEmailAsync(studentEmail, subject, body);
        }

        public async Task SendAbsentNotificationAsync(string studentEmail, string studentName, string className, DateTime absentTime)
        {
            var subject = $"Absence Alert - {className}";
            var body = GenerateAbsentEmailBody(studentName, className, absentTime);
            
            await SendEmailAsync(studentEmail, subject, body);
        }

        public async Task SendWeeklyReportAsync(string recipientEmail, string reportContent)
        {
            var subject = "Weekly Attendance Report";
            var body = GenerateReportEmailBody(reportContent);
            
            await SendEmailAsync(recipientEmail, subject, body);
        }

        private string GenerateAttendanceEmailBody(string studentName, string className, string status)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c3e50;'>Attendance Update</h2>
                        <p>Dear {studentName},</p>
                        <p>Your attendance status for <strong>{className}</strong> has been updated:</p>
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p><strong>Status:</strong> {status}</p>
                            <p><strong>Time:</strong> {DateTime.Now:yyyy-MM-dd HH:mm}</p>
                        </div>
                        <p>If you have any questions, please contact your teacher or administrator.</p>
                        <hr style='margin: 30px 0;'>
                        <p style='color: #6c757d; font-size: 12px;'>
                            This is an automated message from the Attendance System.
                        </p>
                    </div>
                </body>
                </html>";
        }

        private string GenerateAbsentEmailBody(string studentName, string className, DateTime absentTime)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #dc3545;'>Absence Alert</h2>
                        <p>Dear {studentName},</p>
                        <p>You have been marked as <strong>Absent</strong> for <strong>{className}</strong>.</p>
                        <div style='background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 20px 0; border: 1px solid #f5c6cb;'>
                            <p><strong>Class:</strong> {className}</p>
                            <p><strong>Marked Absent At:</strong> {absentTime:yyyy-MM-dd HH:mm}</p>
                            <p><strong>Reason:</strong> Did not return within 5 minutes after leaving</p>
                        </div>
                        <p>If this is incorrect, please contact your teacher immediately.</p>
                        <hr style='margin: 30px 0;'>
                        <p style='color: #6c757d; font-size: 12px;'>
                            This is an automated message from the Attendance System.
                        </p>
                    </div>
                </body>
                </html>";
        }

        private string GenerateReportEmailBody(string reportContent)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 800px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c3e50;'>Weekly Attendance Report</h2>
                        <p>Please find your weekly attendance report below:</p>
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                            {reportContent}
                        </div>
                        <p>For detailed reports, please log in to the Attendance System.</p>
                        <hr style='margin: 30px 0;'>
                        <p style='color: #6c757d; font-size: 12px;'>
                            This is an automated weekly report from the Attendance System.
                        </p>
                    </div>
                </body>
                </html>";
        }
    }
}
