using System;

namespace AttendanceSystem.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string ErrorMessage { get; set; }
        public string ErrorDetails { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class ValidationErrorViewModel
    {
        public string Field { get; set; }
        public string Message { get; set; }
        public object AttemptedValue { get; set; }
    }
}
