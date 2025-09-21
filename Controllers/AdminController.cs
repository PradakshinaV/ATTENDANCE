using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using AttendanceSystem.Models;
using AttendanceSystem.Services;

namespace AttendanceSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext _context;
        private ApplicationUserManager _userManager;

        public AdminController()
        {
            _context = new ApplicationDbContext();
            _userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(_context));
        }

        // GET: Admin/Dashboard
        public async Task<ActionResult> Dashboard()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(s => s.IsActive),
                TotalTeachers = await _context.Users.CountAsync(u => u.Role == "Teacher"),
                TotalClasses = await _context.Classes.CountAsync(c => c.IsActive),
                TotalAttendanceToday = await _context.Attendances.CountAsync(a => DbFunctions.TruncateTime(a.Date) == DateTime.Today)
            };

            var recentAttendance = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Where(a => DbFunctions.TruncateTime(a.Date) == DateTime.Today)
                .OrderByDescending(a => a.CreatedDate)
                .Take(10)
                .ToListAsync();

            stats.RecentAttendance = recentAttendance;

            return View(stats);
        }

        // GET: Admin/ManageUsers
        public async Task<ActionResult> ManageUsers()
        {
            var users = await _context.Users
                .Where(u => u.Role != "Admin")
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            return View(users);
        }

        // GET: Admin/CreateUser
        public ActionResult CreateUser()
        {
            return View();
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateUser(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = model.Role
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user.Id, model.Role);
                    TempData["Success"] = "User created successfully.";
                    return RedirectToAction("ManageUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            return View(model);
        }

        // GET: Admin/ManageStudents
        public async Task<ActionResult> ManageStudents()
        {
            var students = await _context.Students
                .Include(s => s.Class)
                .Include(s => s.User)
                .Where(s => s.IsActive)
                .OrderBy(s => s.FirstName)
                .ToListAsync();

            return View(students);
        }

        // GET: Admin/CreateStudent
        public async Task<ActionResult> CreateStudent()
        {
            var classes = await _context.Classes
                .Where(c => c.IsActive)
                .ToListAsync();

            ViewBag.Classes = new SelectList(classes, "Id", "Name");
            return View();
        }

        // POST: Admin/CreateStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateStudent(Student student)
        {
            if (ModelState.IsValid)
            {
                student.CreatedDate = DateTime.Now;
                student.IsActive = true;
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Student created successfully.";
                return RedirectToAction("ManageStudents");
            }

            var classes = await _context.Classes
                .Where(c => c.IsActive)
                .ToListAsync();
            ViewBag.Classes = new SelectList(classes, "Id", "Name", student.ClassId);
            return View(student);
        }

        // GET: Admin/ManageClasses
        public async Task<ActionResult> ManageClasses()
        {
            var classes = await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Students)
                .Where(c => c.IsActive)
                .ToListAsync();

            return View(classes);
        }

        // GET: Admin/CreateClass
        public async Task<ActionResult> CreateClass()
        {
            var teachers = await _context.Users
                .Where(u => u.Role == "Teacher")
                .ToListAsync();

            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");
            return View();
        }

        // POST: Admin/CreateClass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateClass(Class classModel)
        {
            if (ModelState.IsValid)
            {
                classModel.CreatedDate = DateTime.Now;
                classModel.IsActive = true;
                _context.Classes.Add(classModel);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Class created successfully.";
                return RedirectToAction("ManageClasses");
            }

            var teachers = await _context.Users
                .Where(u => u.Role == "Teacher")
                .ToListAsync();
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName", classModel.TeacherId);
            return View(classModel);
        }

        // GET: Admin/Reports
        public async Task<ActionResult> Reports()
        {
            var classes = await _context.Classes
                .Where(c => c.IsActive)
                .ToListAsync();

            ViewBag.Classes = new SelectList(classes, "Id", "Name");
            return View();
        }

        // POST: Admin/GenerateReport
        [HttpPost]
        public async Task<ActionResult> GenerateReport(int classId, DateTime fromDate, DateTime toDate)
        {
            using (var attendanceService = new AttendanceService())
            {
                var attendance = await attendanceService.GetClassAttendanceReportAsync(classId, fromDate, toDate);
                var stats = await attendanceService.GetAttendanceStatsAsync(classId, fromDate, toDate);
                var classInfo = await _context.Classes.FindAsync(classId);

                var report = new AttendanceReportViewModel
                {
                    Class = classInfo,
                    Attendance = attendance,
                    Stats = stats,
                    FromDate = fromDate,
                    ToDate = toDate
                };

                return PartialView("_ReportResults", report);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
                _userManager?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalClasses { get; set; }
        public int TotalAttendanceToday { get; set; }
        public List<Attendance> RecentAttendance { get; set; }
    }

    public class AttendanceReportViewModel
    {
        public Class Class { get; set; }
        public List<Attendance> Attendance { get; set; }
        public Dictionary<string, int> Stats { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
