using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystem.Models
{
    public enum AttendanceStatus
    {
        Present = 1,
        Left = 2,
        Returned = 3,
        Absent = 4
    }

    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [Required]
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        public DateTime? LeftAt { get; set; }

        public DateTime? ReturnedAt { get; set; }

        public DateTime? MarkedAbsentAt { get; set; }

        public string MarkedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [NotMapped]
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case AttendanceStatus.Present:
                        return "Present";
                    case AttendanceStatus.Left:
                        return $"Left at {LeftAt:HH:mm}";
                    case AttendanceStatus.Returned:
                        return $"Returned at {ReturnedAt:HH:mm}";
                    case AttendanceStatus.Absent:
                        return $"Absent (marked at {MarkedAbsentAt:HH:mm})";
                    default:
                        return "Unknown";
                }
            }
        }

        [NotMapped]
        public string StatusClass
        {
            get
            {
                switch (Status)
                {
                    case AttendanceStatus.Present:
                        return "success";
                    case AttendanceStatus.Left:
                        return "warning";
                    case AttendanceStatus.Returned:
                        return "info";
                    case AttendanceStatus.Absent:
                        return "danger";
                    default:
                        return "secondary";
                }
            }
        }
    }
}
