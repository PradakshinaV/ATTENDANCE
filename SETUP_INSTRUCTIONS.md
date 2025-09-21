# 🛠️ Complete Setup Instructions

## Step-by-Step Setup Guide

### 1. Prerequisites Installation

#### Install Visual Studio
- Download Visual Studio 2019 or later
- Include ASP.NET and web development workload
- Include .NET Framework 4.8 development tools

#### Install MySQL Server
```bash
# Download MySQL 8.0+ from official website
# During installation, note your root password
# Create a new database user if needed
```

### 2. Database Configuration

#### Create Database
```sql
-- Connect to MySQL as root
CREATE DATABASE AttendanceSystem CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create a dedicated user (optional but recommended)
CREATE USER 'attendance_user'@'localhost' IDENTIFIED BY 'your_secure_password';
GRANT ALL PRIVILEGES ON AttendanceSystem.* TO 'attendance_user'@'localhost';
FLUSH PRIVILEGES;
```

#### Update Connection String
Edit `Web.config` file:
```xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Server=localhost;Database=AttendanceSystem;Uid=attendance_user;Pwd=your_secure_password;SslMode=none;" 
         providerName="MySql.Data.MySqlClient" />
</connectionStrings>
```

### 3. Project Setup in Visual Studio

#### Open the Project
1. Launch Visual Studio
2. Click "Open a project or solution"
3. Navigate to the folder and select `AttendanceSystem.csproj`

#### Restore NuGet Packages
```
Right-click on Solution → Restore NuGet Packages
```
Or in Package Manager Console:
```
Update-Package -reinstall
```

#### Enable Entity Framework Migrations
Open Package Manager Console (Tools → NuGet Package Manager → Package Manager Console):
```
Enable-Migrations -EnableAutomaticMigrations
Add-Migration InitialCreate
Update-Database
```

### 4. First Run

#### Build and Run
1. Press `Ctrl + Shift + B` to build the solution
2. Press `F5` to run the application
3. Browser should open automatically to `https://localhost:port/`

#### Default Admin Account
- **Email**: admin@attendance.com
- **Password**: Admin@123

### 5. Initial Data Setup

#### Create Sample Users
1. Login as admin
2. Navigate to "Manage Users"
3. Create teacher accounts:
   - Email: teacher1@school.com, Password: Teacher@123, Role: Teacher
   - Email: teacher2@school.com, Password: Teacher@123, Role: Teacher

4. Create student accounts:
   - Email: student1@school.com, Password: Student@123, Role: Student
   - Email: student2@school.com, Password: Student@123, Role: Student

#### Create Classes
1. Navigate to "Manage Classes"
2. Create sample classes:
   - Name: "Mathematics 101", Subject: "Math", Room: "A101", Time: 09:00-10:30
   - Name: "English Literature", Subject: "English", Room: "B201", Time: 11:00-12:30

#### Add Students to Classes
1. Navigate to "Manage Students"
2. Create student records and assign them to classes

### 6. Testing the System

#### Test Teacher Functionality
1. Logout and login as teacher
2. Select a class
3. Mark attendance for students:
   - Mark as "Present"
   - Mark as "Left" and wait 5+ minutes to see auto-absent
   - Mark as "Returned" within 5 minutes

#### Test Student Functionality
1. Logout and login as student
2. View dashboard with today's status
3. Check attendance history
4. Export attendance data

### 7. Production Deployment (Optional)

#### IIS Configuration
```xml
<!-- Add to Web.config for production -->
<system.web>
    <compilation debug="false" targetFramework="4.8" />
    <customErrors mode="On" defaultRedirect="~/Error" />
</system.web>
```

#### Database Backup
```sql
-- Create regular backups
mysqldump -u attendance_user -p AttendanceSystem > backup_$(date +%Y%m%d).sql
```

## 🔧 Troubleshooting

### Common Issues and Solutions

#### "Connection string not found"
- Verify Web.config has correct connectionStrings section
- Check that connection string name matches code

#### "Login failed for user"
- Verify MySQL credentials are correct
- Check if MySQL server is running
- Test connection with MySQL Workbench

#### "The provider name 'MySql.Data.MySqlClient' was not found"
- Reinstall MySQL.Data.EntityFramework NuGet package
- Check machine.config for MySQL provider registration

#### "Background service not working"
- Verify application is running (not just debugging)
- Check Application_Start in Global.asax.cs
- Look for exceptions in debug output

#### "Authentication not working"
- Clear browser cache and cookies
- Check if AspNetUsers tables were created
- Verify Startup.cs configuration

### Development Tips

#### Enable Detailed Errors
```xml
<!-- In Web.config for development -->
<system.web>
    <compilation debug="true" targetFramework="4.8" />
    <customErrors mode="Off" />
</system.web>
```

#### Database Recreation
If you need to recreate the database:
```
Drop-Database (in Package Manager Console)
Update-Database
```

#### Custom Configuration
You can modify these settings in Web.config:
- Authentication timeout
- Session timeout
- Database connection timeout
- Background service interval

## 📊 Performance Optimization

### Database Optimization
```sql
-- Add indexes for better performance
CREATE INDEX idx_attendance_date ON Attendances(Date);
CREATE INDEX idx_attendance_student ON Attendances(StudentId);
CREATE INDEX idx_attendance_class ON Attendances(ClassId);
```

### Application Settings
```xml
<!-- Add to Web.config for better performance -->
<appSettings>
    <add key="vs:EnableBrowserLink" value="false" />
    <add key="webpages:Enabled" value="false" />
</appSettings>
```

## 🔐 Security Recommendations

### Production Security
1. Change default admin password immediately
2. Use strong passwords for all accounts
3. Enable SSL/HTTPS in production
4. Regular database backups
5. Monitor application logs
6. Keep framework and dependencies updated

### Database Security
```sql
-- Remove unnecessary privileges
REVOKE ALL PRIVILEGES ON *.* FROM 'attendance_user'@'localhost';
GRANT SELECT, INSERT, UPDATE, DELETE ON AttendanceSystem.* TO 'attendance_user'@'localhost';
```

This completes the full setup process for your Time-Based Attendance System! 🎉
