using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using AttendanceSystem.Models;

namespace AttendanceSystem.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<AttendanceSystem.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(AttendanceSystem.Models.ApplicationDbContext context)
        {
            // Create roles
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // Create Admin role
            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }

            // Create Teacher role
            if (!roleManager.RoleExists("Teacher"))
            {
                roleManager.Create(new IdentityRole("Teacher"));
            }

            // Create Student role
            if (!roleManager.RoleExists("Student"))
            {
                roleManager.Create(new IdentityRole("Student"));
            }

            // Create default admin user
            var adminUser = userManager.FindByEmail("admin@attendance.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@attendance.com",
                    Email = "admin@attendance.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = "Admin",
                    EmailConfirmed = true
                };

                var result = userManager.Create(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    userManager.AddToRole(adminUser.Id, "Admin");
                }
            }

            // Create sample teacher
            var teacherUser = userManager.FindByEmail("teacher@attendance.com");
            if (teacherUser == null)
            {
                teacherUser = new ApplicationUser
                {
                    UserName = "teacher@attendance.com",
                    Email = "teacher@attendance.com",
                    FirstName = "John",
                    LastName = "Teacher",
                    Role = "Teacher",
                    EmailConfirmed = true
                };

                var result = userManager.Create(teacherUser, "Teacher@123");
                if (result.Succeeded)
                {
                    userManager.AddToRole(teacherUser.Id, "Teacher");
                }
            }

            // Create sample student
            var studentUser = userManager.FindByEmail("student@attendance.com");
            if (studentUser == null)
            {
                studentUser = new ApplicationUser
                {
                    UserName = "student@attendance.com",
                    Email = "student@attendance.com",
                    FirstName = "Jane",
                    LastName = "Student",
                    Role = "Student",
                    EmailConfirmed = true
                };

                var result = userManager.Create(studentUser, "Student@123");
                if (result.Succeeded)
                {
                    userManager.AddToRole(studentUser.Id, "Student");
                }
            }

            context.SaveChanges();

            // Create sample class
            if (!context.Classes.Any())
            {
                var sampleClass = new Class
                {
                    Name = "Computer Science 101",
                    Subject = "Computer Science",
                    Description = "Introduction to Programming",
                    Room = "Lab-A1",
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(10, 30, 0),
                    TeacherId = teacherUser.Id,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                context.Classes.Add(sampleClass);
                context.SaveChanges();

                // Create sample student record
                var studentRecord = new Student
                {
                    FirstName = "Jane",
                    LastName = "Student",
                    StudentId = "STU001",
                    Email = "student@attendance.com",
                    PhoneNumber = "123-456-7890",
                    ClassId = sampleClass.Id,
                    UserId = studentUser.Id,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                context.Students.Add(studentRecord);
                context.SaveChanges();
            }
        }
    }
}
