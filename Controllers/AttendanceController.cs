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
    [Authorize(Roles = "Teacher,Admin")]
    public class AttendanceController : Controller
    {
        private ApplicationDbContext _context;
        private AttendanceService _attendanceService;

        public AttendanceController()
        {
            _context = new ApplicationDbContext();
            _attendanceService = new AttendanceService();
        }

        // GET: Attendance
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var user = await _context.Users.FindAsync(userId);

            var classes = await _context.Classes
                .Where(c => c.TeacherId == userId || user.Role == "Admin")
                .Include(c => c.Students)
                .ToListAsync();

            return View(classes);
        }

        // GET: Attendance/MarkAttendance/5
        public async Task<ActionResult> MarkAttendance(int classId)
        {
            var classInfo = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classInfo == null)
            {
                return HttpNotFound();
            }

            var todayAttendance = await _attendanceService.GetTodayAttendanceAsync(classId);
            var students = await _attendanceService.GetStudentsInClassAsync(classId);

            var viewModel = new MarkAttendanceViewModel
            {
                Class = classInfo,
                Students = students,
                TodayAttendance = todayAttendance
            };

            return View(viewModel);
        }

        // POST: Attendance/MarkAttendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAttendance(int studentId, int classId, string status)
        {
            try
            {
                var markedBy = User.Identity.GetUserId();
                AttendanceStatus attendanceStatus;

                switch (status.ToLower())
                {
                    case "present":
                        attendanceStatus = AttendanceStatus.Present;
                        break;
                    case "left":
                        attendanceStatus = AttendanceStatus.Left;
                        break;
                    case "returned":
                        attendanceStatus = AttendanceStatus.Returned;
                        break;
                    case "absent":
                        attendanceStatus = AttendanceStatus.Absent;
                        break;
                    default:
                        return Json(new { success = false, message = "Invalid status" });
                }

                await _attendanceService.MarkAttendanceAsync(studentId, classId, attendanceStatus, markedBy);

                return Json(new { success = true, message = "Attendance marked successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error marking attendance: " + ex.Message });
            }
        }

        // GET: Attendance/ViewAttendance/5
        public async Task<ActionResult> ViewAttendance(int classId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var classInfo = await _context.Classes.FindAsync(classId);
            if (classInfo == null)
            {
                return HttpNotFound();
            }

            var from = fromDate ?? DateTime.Today.AddDays(-30);
            var to = toDate ?? DateTime.Today;

            var attendance = await _attendanceService.GetClassAttendanceReportAsync(classId, from, to);
            var stats = await _attendanceService.GetAttendanceStatsAsync(classId, from, to);

            var viewModel = new ViewAttendanceViewModel
            {
                Class = classInfo,
                Attendance = attendance,
                Stats = stats,
                FromDate = from,
                ToDate = to
            };

            return View(viewModel);
        }

        // GET: Attendance/Export/5
        public async Task<ActionResult> Export(int classId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var classInfo = await _context.Classes.FindAsync(classId);
            if (classInfo == null)
            {
                return HttpNotFound();
            }

            var from = fromDate ?? DateTime.Today.AddDays(-30);
            var to = toDate ?? DateTime.Today;

            var attendance = await _attendanceService.GetClassAttendanceReportAsync(classId, from, to);

            // Create CSV content
            var csv = "Student Name,Date,Status,Left At,Returned At,Marked Absent At\n";
            foreach (var record in attendance)
            {
                csv += $"{record.Student.FullName},{record.Date:yyyy-MM-dd},{record.Status},{record.LeftAt},{record.ReturnedAt},{record.MarkedAbsentAt}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"Attendance_{classInfo.Name}_{from:yyyy-MM-dd}_to_{to:yyyy-MM-dd}.csv");
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

    public class MarkAttendanceViewModel
    {
        public Class Class { get; set; }
        public List<Student> Students { get; set; }
        public List<Attendance> TodayAttendance { get; set; }
    }

    public class ViewAttendanceViewModel
    {
        public Class Class { get; set; }
        public List<Attendance> Attendance { get; set; }
        public Dictionary<string, int> Stats { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
