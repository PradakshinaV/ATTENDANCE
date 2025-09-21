using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using AttendanceSystem.Models;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace AttendanceSystem.Services
{
    public class BackupService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _backupPath;

        public BackupService()
        {
            _context = new ApplicationDbContext();
            _backupPath = ConfigurationManager.AppSettings["BackupPath"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
            
            // Ensure backup directory exists
            if (!Directory.Exists(_backupPath))
            {
                Directory.CreateDirectory(_backupPath);
            }
        }

        public async Task<string> CreateBackupAsync()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"AttendanceBackup_{timestamp}.sql";
                var filePath = Path.Combine(_backupPath, fileName);

                var backupScript = await GenerateBackupScriptAsync();
                await File.WriteAllTextAsync(filePath, backupScript);

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Backup creation failed: {ex.Message}", ex);
            }
        }

        public async Task RestoreBackupAsync(string backupFilePath)
        {
            try
            {
                if (!File.Exists(backupFilePath))
                {
                    throw new FileNotFoundException("Backup file not found.");
                }

                var script = await File.ReadAllTextAsync(backupFilePath);
                await ExecuteRestoreScriptAsync(script);
            }
            catch (Exception ex)
            {
                throw new Exception($"Backup restoration failed: {ex.Message}", ex);
            }
        }

        public async Task<string> ExportDataToCsvAsync()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"AttendanceData_{timestamp}.csv";
                var filePath = Path.Combine(_backupPath, fileName);

                var csvContent = await GenerateCsvDataAsync();
                await File.WriteAllTextAsync(filePath, csvContent);

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"CSV export failed: {ex.Message}", ex);
            }
        }

        private async Task<string> GenerateBackupScriptAsync()
        {
            var script = new StringBuilder();
            
            // Header
            script.AppendLine("-- Attendance System Database Backup");
            script.AppendLine($"-- Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            script.AppendLine("-- ");
            script.AppendLine();

            // Disable foreign key checks
            script.AppendLine("SET FOREIGN_KEY_CHECKS = 0;");
            script.AppendLine();

            // Backup Users
            var users = await _context.Users.ToListAsync();
            script.AppendLine("-- Users Table");
            script.AppendLine("DELETE FROM AspNetUsers;");
            foreach (var user in users)
            {
                script.AppendLine($"INSERT INTO AspNetUsers (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEndDateUtc, LockoutEnabled, AccessFailedCount, UserName, FirstName, LastName, Role) VALUES ('{user.Id}', '{EscapeSql(user.Email)}', {(user.EmailConfirmed ? 1 : 0)}, '{EscapeSql(user.PasswordHash)}', '{EscapeSql(user.SecurityStamp)}', '{EscapeSql(user.PhoneNumber)}', {(user.PhoneNumberConfirmed ? 1 : 0)}, {(user.TwoFactorEnabled ? 1 : 0)}, {(user.LockoutEndDateUtc.HasValue ? $"'{user.LockoutEndDateUtc:yyyy-MM-dd HH:mm:ss}'" : "NULL")}, {(user.LockoutEnabled ? 1 : 0)}, {user.AccessFailedCount}, '{EscapeSql(user.UserName)}', '{EscapeSql(user.FirstName)}', '{EscapeSql(user.LastName)}', '{EscapeSql(user.Role)}');");
            }
            script.AppendLine();

            // Backup Classes
            var classes = await _context.Classes.ToListAsync();
            script.AppendLine("-- Classes Table");
            script.AppendLine("DELETE FROM Classes;");
            foreach (var cls in classes)
            {
                script.AppendLine($"INSERT INTO Classes (Id, Name, Description, Subject, Room, StartTime, EndTime, TeacherId, CreatedDate, IsActive) VALUES ({cls.Id}, '{EscapeSql(cls.Name)}', '{EscapeSql(cls.Description)}', '{EscapeSql(cls.Subject)}', '{EscapeSql(cls.Room)}', '{cls.StartTime}', '{cls.EndTime}', '{EscapeSql(cls.TeacherId)}', '{cls.CreatedDate:yyyy-MM-dd HH:mm:ss}', {(cls.IsActive ? 1 : 0)});");
            }
            script.AppendLine();

            // Backup Students
            var students = await _context.Students.ToListAsync();
            script.AppendLine("-- Students Table");
            script.AppendLine("DELETE FROM Students;");
            foreach (var student in students)
            {
                script.AppendLine($"INSERT INTO Students (Id, FirstName, LastName, StudentId, Email, PhoneNumber, ClassId, UserId, CreatedDate, IsActive) VALUES ({student.Id}, '{EscapeSql(student.FirstName)}', '{EscapeSql(student.LastName)}', '{EscapeSql(student.StudentId)}', '{EscapeSql(student.Email)}', '{EscapeSql(student.PhoneNumber)}', {student.ClassId}, '{EscapeSql(student.UserId)}', '{student.CreatedDate:yyyy-MM-dd HH:mm:ss}', {(student.IsActive ? 1 : 0)});");
            }
            script.AppendLine();

            // Backup Attendances
            var attendances = await _context.Attendances.ToListAsync();
            script.AppendLine("-- Attendances Table");
            script.AppendLine("DELETE FROM Attendances;");
            foreach (var attendance in attendances)
            {
                script.AppendLine($"INSERT INTO Attendances (Id, StudentId, ClassId, Date, Status, LeftAt, ReturnedAt, MarkedAbsentAt, MarkedBy, CreatedDate, UpdatedDate, Notes) VALUES ({attendance.Id}, {attendance.StudentId}, {attendance.ClassId}, '{attendance.Date:yyyy-MM-dd HH:mm:ss}', {(int)attendance.Status}, {(attendance.LeftAt.HasValue ? $"'{attendance.LeftAt:yyyy-MM-dd HH:mm:ss}'" : "NULL")}, {(attendance.ReturnedAt.HasValue ? $"'{attendance.ReturnedAt:yyyy-MM-dd HH:mm:ss}'" : "NULL")}, {(attendance.MarkedAbsentAt.HasValue ? $"'{attendance.MarkedAbsentAt:yyyy-MM-dd HH:mm:ss}'" : "NULL")}, '{EscapeSql(attendance.MarkedBy)}', '{attendance.CreatedDate:yyyy-MM-dd HH:mm:ss}', {(attendance.UpdatedDate.HasValue ? $"'{attendance.UpdatedDate:yyyy-MM-dd HH:mm:ss}'" : "NULL")}, '{EscapeSql(attendance.Notes)}');");
            }
            script.AppendLine();

            // Re-enable foreign key checks
            script.AppendLine("SET FOREIGN_KEY_CHECKS = 1;");
            script.AppendLine();
            script.AppendLine("-- Backup completed successfully");

            return script.ToString();
        }

        private async Task<string> GenerateCsvDataAsync()
        {
            var csv = new StringBuilder();
            
            // Header
            csv.AppendLine("Student Name,Student ID,Class,Date,Status,Left At,Returned At,Marked Absent At,Notes");
            
            // Data
            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            foreach (var attendance in attendances)
            {
                csv.AppendLine($"\"{attendance.Student.FullName}\",\"{attendance.Student.StudentId}\",\"{attendance.Class.Name}\",\"{attendance.Date:yyyy-MM-dd}\",\"{attendance.Status}\",\"{attendance.LeftAt?.ToString("HH:mm") ?? ""}\",\"{attendance.ReturnedAt?.ToString("HH:mm") ?? ""}\",\"{attendance.MarkedAbsentAt?.ToString("HH:mm") ?? ""}\",\"{EscapeCsv(attendance.Notes)}\"");
            }

            return csv.ToString();
        }

        private async Task ExecuteRestoreScriptAsync(string script)
        {
            // This is a simplified implementation
            // In a real scenario, you'd want to parse and execute SQL commands carefully
            var commands = script.Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var command in commands)
            {
                if (!string.IsNullOrWhiteSpace(command) && !command.Trim().StartsWith("--"))
                {
                    try
                    {
                        await _context.Database.ExecuteSqlCommandAsync(command);
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue with other commands
                        System.Diagnostics.Debug.WriteLine($"Command failed: {command}. Error: {ex.Message}");
                    }
                }
            }
        }

        private string EscapeSql(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            
            return input.Replace("'", "''").Replace("\\", "\\\\");
        }

        private string EscapeCsv(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            
            return input.Replace("\"", "\"\"");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
