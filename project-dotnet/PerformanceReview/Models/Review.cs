using System;

namespace PerformanceReview.Models
{
    public class Review
    {
        public const string STATUS_DRAFT = "DRAFT";
        public const string STATUS_SUBMITTED = "SUBMITTED";
        public const string STATUS_ACKNOWLEDGED = "ACKNOWLEDGED";
        public const int MIN_SCORE = 1;
        public const int MAX_SCORE = 10;

        public User Employee { get; set; }
        public User Reviewer { get; set; }
        public int PunctualityScore { get; set; }
        public int TeamworkScore { get; set; }
        public int ProductivityScore { get; set; }
        public int LeadershipScore { get; set; }
        public int VisionScore { get; set; }
        public int ManagerialSkillsScore { get; set; }
        public string? Comments { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }

        // Backward compatibility alias
        public User Manager => Reviewer;

        public Review(User employee, User reviewer)
        {
            Employee = employee;
            Reviewer = reviewer;
            Status = STATUS_SUBMITTED;
            CreatedDate = DateTime.Now;
        }

        public int CalculateTotalScore()
        {
            if (HasExtendedScores())
                return (PunctualityScore + TeamworkScore + ProductivityScore +
                        LeadershipScore + VisionScore + ManagerialSkillsScore) / 6;
            return (PunctualityScore + TeamworkScore + ProductivityScore) / 3;
        }

        public double CalculateAverageScore()
        {
            if (HasExtendedScores())
                return (PunctualityScore + TeamworkScore + ProductivityScore +
                        LeadershipScore + VisionScore + ManagerialSkillsScore) / 6.0;
            return (PunctualityScore + TeamworkScore + ProductivityScore) / 3.0;
        }

        public bool HasExtendedScores()
        {
            return LeadershipScore > 0 || VisionScore > 0 || ManagerialSkillsScore > 0;
        }

        public static bool IsValidScore(int score)
        {
            return score >= MIN_SCORE && score <= MAX_SCORE;
        }

        public bool IsDraft() => STATUS_DRAFT == Status;
        public bool IsSubmitted() => STATUS_SUBMITTED == Status;

        public bool Submit()
        {
            if (!IsDraft()) return false;
            Status = STATUS_SUBMITTED;
            return true;
        }

        public void Acknowledge() => Status = STATUS_ACKNOWLEDGED;

        public bool CanBeDeletedBy(User user)
        {
            if (user == null || Reviewer == null) return false;
            return Reviewer.Username == user.Username;
        }

        public string GetPerformanceLevel()
        {
            double avg = CalculateAverageScore();
            if (avg >= 9) return "Exceptional";
            if (avg >= 7) return "Exceeds Expectations";
            if (avg >= 5) return "Meets Expectations";
            if (avg >= 3) return "Needs Improvement";
            return "Below Expectations";
        }

        public long GetDaysSinceCreation()
        {
            return (long)(DateTime.Now - CreatedDate).TotalDays;
        }

        public override string ToString()
        {
            return $"Review for {Employee?.Name ?? "Unknown"} [Score: {CalculateTotalScore()}/10]";
        }
    }
}
