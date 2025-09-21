using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace AttendanceSystem.Services
{
    public class BackgroundService : IRegisteredObject
    {
        private readonly object _lock = new object();
        private bool _shuttingDown;
        private Timer _timer;

        public BackgroundService()
        {
            HostingEnvironment.RegisterObject(this);
            StartTimer();
        }

        private void StartTimer()
        {
            // Run every minute to check for students who should be marked absent
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        private void DoWork(object state)
        {
            lock (_lock)
            {
                if (_shuttingDown)
                    return;

                try
                {
                    // Auto-mark students as absent if they left more than 5 minutes ago
                    using (var attendanceService = new AttendanceService())
                    {
                        attendanceService.AutoMarkAbsentAsync().Wait();
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception (you might want to use a proper logging framework)
                    System.Diagnostics.Debug.WriteLine($"Background service error: {ex.Message}");
                }
            }
        }

        public void Stop(bool immediate)
        {
            lock (_lock)
            {
                _shuttingDown = true;
            }

            _timer?.Dispose();
            HostingEnvironment.UnregisterObject(this);
        }
    }
}
