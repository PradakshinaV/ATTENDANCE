using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystem.Models
{
    public class Class
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Subject { get; set; }

        [StringLength(20)]
        public string Room { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual ApplicationUser Teacher { get; set; }

        public virtual ICollection<Student> Students { get; set; }

        public virtual ICollection<Attendance> Attendances { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        [NotMapped]
        public string ClassTime => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    }
}
