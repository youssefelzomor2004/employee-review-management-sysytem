using System;

namespace PerformanceReview.Models
{
    public class TrainingRecommendation
    {
        public const string STATUS_PENDING = "PENDING";
        public const string STATUS_IN_PROGRESS = "IN_PROGRESS";
        public const string STATUS_COMPLETED = "COMPLETED";
        public const string STATUS_CANCELLED = "CANCELLED";

        public User? Recommender { get; set; }
        public User Recipient { get; set; }
        public string CourseName { get; set; }
        public DateTime DateRecommended { get; set; }
        public string Status { get; set; }
        public DateTime? CompletedDate { get; set; }

        public TrainingRecommendation(User? recommender, User recipient, string courseName)
        {
            Recommender = recommender;
            Recipient = recipient;
            CourseName = courseName;
            DateRecommended = DateTime.Now;
            Status = STATUS_PENDING;
        }

        public bool CanBeDeletedBy(User user)
        {
            if (user == null || Recommender == null) return false;
            return Recommender.Username == user.Username;
        }

        public bool CanStart() => STATUS_PENDING == Status;

        public bool Start()
        {
            if (!CanStart()) return false;
            Status = STATUS_IN_PROGRESS;
            return true;
        }

        public bool CanComplete() => STATUS_IN_PROGRESS == Status || STATUS_PENDING == Status;

        public bool Complete()
        {
            if (!CanComplete()) return false;
            Status = STATUS_COMPLETED;
            CompletedDate = DateTime.Now;
            return true;
        }

        public bool Cancel()
        {
            if (STATUS_COMPLETED == Status) return false;
            Status = STATUS_CANCELLED;
            return true;
        }

        public bool IsPending() => STATUS_PENDING == Status;
        public bool IsInProgress() => STATUS_IN_PROGRESS == Status;
        public bool IsCompleted() => STATUS_COMPLETED == Status;

        public long GetDaysSinceRecommended()
        {
            return (long)(DateTime.Now - DateRecommended).TotalDays;
        }

        public bool IsOverdue()
        {
            return IsPending() && GetDaysSinceRecommended() > 30;
        }

        public string RecipientName => Recipient?.Name ?? "Unknown";
        public string RecommenderName => Recommender?.Name ?? "Unknown";

        public override string ToString()
        {
            return $"{CourseName} (Recommended by: {RecommenderName} on {DateRecommended:yyyy-MM-dd})";
        }
    }
}
