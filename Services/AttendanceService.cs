using AttendanceSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceSystem.Services
{
    public class AttendanceService
    {
        private ApplicationDbContext _context;

        public AttendanceService()
        {
            _context = new ApplicationDbContext();
        }

        public async Task<Attendance> MarkAttendanceAsync(int studentId, int classId, AttendanceStatus status, string markedBy)
        {
            var today = DateTime.Today;
            
            // Check if attendance already exists for today
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == studentId && 
                                         a.ClassId == classId && 
                                         DbFunctions.TruncateTime(a.Date) == today);

            if (existingAttendance != null)
            {
                // Update existing attendance
                existingAttendance.Status = status;
                existingAttendance.UpdatedDate = DateTime.Now;
                existingAttendance.MarkedBy = markedBy;

                switch (status)
                {
                    case AttendanceStatus.Left:
                        existingAttendance.LeftAt = DateTime.Now;
                        break;
                    case AttendanceStatus.Returned:
                        existingAttendance.ReturnedAt = DateTime.Now;
                        break;
                    case AttendanceStatus.Absent:
                        existingAttendance.MarkedAbsentAt = DateTime.Now;
                        break;
                }

                await _context.SaveChangesAsync();
                return existingAttendance;
            }
            else
            {
                // Create new attendance record
                var attendance = new Attendance
                {
                    StudentId = studentId,
                    ClassId = classId,
                    Date = DateTime.Now,
                    Status = status,
                    MarkedBy = markedBy,
                    CreatedDate = DateTime.Now
                };

                switch (status)
                {
                    case AttendanceStatus.Left:
                        attendance.LeftAt = DateTime.Now;
                        break;
                    case AttendanceStatus.Returned:
                        attendance.ReturnedAt = DateTime.Now;
                        break;
                    case AttendanceStatus.Absent:
                        attendance.MarkedAbsentAt = DateTime.Now;
                        break;
                }

                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();
                return attendance;
            }
        }

        public async Task<List<Attendance>> GetTodayAttendanceAsync(int classId)
        {
            var today = DateTime.Today;
            
            return await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.ClassId == classId && 
                           DbFunctions.TruncateTime(a.Date) == today)
                .OrderBy(a => a.Student.FirstName)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetStudentAttendanceHistoryAsync(int studentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Attendances
                .Include(a => a.Class)
                .Where(a => a.StudentId == studentId);

            if (fromDate.HasValue)
                query = query.Where(a => a.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Date <= toDate.Value);

            return await query.OrderByDescending(a => a.Date).ToListAsync();
        }

        public async Task<List<Attendance>> GetClassAttendanceReportAsync(int classId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.ClassId == classId);

            if (fromDate.HasValue)
                query = query.Where(a => a.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Date <= toDate.Value);

            return await query.OrderByDescending(a => a.Date).ToListAsync();
        }

        public async Task<List<Student>> GetStudentsInClassAsync(int classId)
        {
            return await _context.Students
                .Where(s => s.ClassId == classId && s.IsActive)
                .OrderBy(s => s.FirstName)
                .ToListAsync();
        }

        public async Task AutoMarkAbsentAsync()
        {
            var fiveMinutesAgo = DateTime.Now.AddMinutes(-5);
            
            var leftStudents = await _context.Attendances
                .Where(a => a.Status == AttendanceStatus.Left && 
                           a.LeftAt <= fiveMinutesAgo)
                .ToListAsync();

            foreach (var attendance in leftStudents)
            {
                attendance.Status = AttendanceStatus.Absent;
                attendance.MarkedAbsentAt = DateTime.Now;
                attendance.UpdatedDate = DateTime.Now;
            }

            if (leftStudents.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<string, int>> GetAttendanceStatsAsync(int classId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Attendances.Where(a => a.ClassId == classId);

            if (fromDate.HasValue)
                query = query.Where(a => a.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Date <= toDate.Value);

            var attendances = await query.ToListAsync();

            return new Dictionary<string, int>
            {
                { "Present", attendances.Count(a => a.Status == AttendanceStatus.Present) },
                { "Left", attendances.Count(a => a.Status == AttendanceStatus.Left) },
                { "Returned", attendances.Count(a => a.Status == AttendanceStatus.Returned) },
                { "Absent", attendances.Count(a => a.Status == AttendanceStatus.Absent) }
            };
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
