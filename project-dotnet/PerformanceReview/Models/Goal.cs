using System;

namespace PerformanceReview.Models
{
    public class Goal
    {
        public const string STATUS_ACTIVE = "ACTIVE";
        public const string STATUS_COMPLETED = "COMPLETED";
        public const string STATUS_CANCELLED = "CANCELLED";

        public User Employee { get; set; }
        public User? Creator { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? CompletionNotes { get; set; }

        public Goal(User employee, string description, User? creator = null)
        {
            Employee = employee;
            Description = description;
            Creator = creator;
            Status = STATUS_ACTIVE;
            CreatedDate = DateTime.Now;
        }

        public bool CanComplete() => STATUS_ACTIVE == Status;

        public bool Complete(string notes)
        {
            if (!CanComplete()) return false;
            Status = STATUS_COMPLETED;
            CompletedDate = DateTime.Now;
            CompletionNotes = notes;
            return true;
        }

        public bool CanCancel() => STATUS_ACTIVE == Status;

        public bool Cancel()
        {
            if (!CanCancel()) return false;
            Status = STATUS_CANCELLED;
            return true;
        }

        public bool CanBeDeletedBy(User user)
        {
            if (user == null) return false;
            if (Creator == null)
                return Employee != null && Employee.Username == user.Username;
            return Creator.Username == user.Username;
        }

        public bool IsActive() => STATUS_ACTIVE == Status;
        public bool IsCompleted() => STATUS_COMPLETED == Status;

        public long GetDaysSinceCreation()
        {
            return (long)(DateTime.Now - CreatedDate).TotalDays;
        }

        public bool IsOverdue()
        {
            return IsActive() && GetDaysSinceCreation() > 90;
        }

        public override string ToString()
        {
            return $"{Description} [{Status}]";
        }
    }
}
