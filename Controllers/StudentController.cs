using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using AttendanceSystem.Models;
using AttendanceSystem.Services;

namespace AttendanceSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private ApplicationDbContext _context;
        private AttendanceService _attendanceService;

        public StudentController()
        {
            _context = new ApplicationDbContext();
            _attendanceService = new AttendanceService();
        }

        // GET: Student/Dashboard
        public async Task<ActionResult> Dashboard()
        {
            var userId = User.Identity.GetUserId();
            var user = await _context.Users.FindAsync(userId);

            // Find student record
            var student = await _context.Students
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                TempData["Error"] = "Student record not found. Please contact administrator.";
                return RedirectToAction("Index", "Home");
            }

            // Get today's attendance
            var todayAttendance = await _context.Attendances
                .Include(a => a.Class)
                .Where(a => a.StudentId == student.Id && 
                           DbFunctions.TruncateTime(a.Date) == DateTime.Today)
                .OrderByDescending(a => a.CreatedDate)
                .FirstOrDefaultAsync();

            // Get recent attendance (last 10 days)
            var recentAttendance = await _context.Attendances
                .Include(a => a.Class)
                .Where(a => a.StudentId == student.Id)
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ToListAsync();

            // Get attendance statistics for current month
            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var monthEnd = DateTime.Today;
            var monthlyStats = await _attendanceService.GetAttendanceStatsAsync(student.ClassId, monthStart, monthEnd);

            var viewModel = new StudentDashboardViewModel
            {
                Student = student,
                TodayAttendance = todayAttendance,
                RecentAttendance = recentAttendance,
                MonthlyStats = monthlyStats
            };

            return View(viewModel);
        }

        // GET: Student/History
        public async Task<ActionResult> History(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var userId = User.Identity.GetUserId();
            var user = await _context.Users.FindAsync(userId);

            var student = await _context.Students
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                TempData["Error"] = "Student record not found. Please contact administrator.";
                return RedirectToAction("Index", "Home");
            }

            var from = fromDate ?? DateTime.Today.AddDays(-30);
            var to = toDate ?? DateTime.Today;

            var attendanceHistory = await _attendanceService.GetStudentAttendanceHistoryAsync(student.Id, from, to);

            var viewModel = new StudentHistoryViewModel
            {
                Student = student,
                AttendanceHistory = attendanceHistory,
                FromDate = from,
                ToDate = to
            };

            return View(viewModel);
        }

        // GET: Student/Profile
        public async Task<ActionResult> Profile()
        {
            var userId = User.Identity.GetUserId();
            var user = await _context.Users.FindAsync(userId);

            var student = await _context.Students
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                TempData["Error"] = "Student record not found. Please contact administrator.";
                return RedirectToAction("Index", "Home");
            }

            return View(student);
        }

        // GET: Student/Export
        public async Task<ActionResult> Export(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var userId = User.Identity.GetUserId();
            var user = await _context.Users.FindAsync(userId);

            var student = await _context.Students
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return HttpNotFound();
            }

            var from = fromDate ?? DateTime.Today.AddDays(-30);
            var to = toDate ?? DateTime.Today;

            var attendanceHistory = await _attendanceService.GetStudentAttendanceHistoryAsync(student.Id, from, to);

            // Create CSV content
            var csv = "Date,Class,Status,Left At,Returned At,Marked Absent At\n";
            foreach (var record in attendanceHistory)
            {
                csv += $"{record.Date:yyyy-MM-dd},{record.Class.Name},{record.Status},{record.LeftAt},{record.ReturnedAt},{record.MarkedAbsentAt}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"MyAttendance_{from:yyyy-MM-dd}_to_{to:yyyy-MM-dd}.csv");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
                _attendanceService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class StudentDashboardViewModel
    {
        public Student Student { get; set; }
        public Attendance TodayAttendance { get; set; }
        public List<Attendance> RecentAttendance { get; set; }
        public Dictionary<string, int> MonthlyStats { get; set; }
    }

    public class StudentHistoryViewModel
    {
        public Student Student { get; set; }
        public List<Attendance> AttendanceHistory { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
