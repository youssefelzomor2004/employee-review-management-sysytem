using System;

namespace PerformanceReview.Models
{
    public class UpwardReview
    {
        public const int MIN_SCORE = 1;
        public const int MAX_SCORE = 5;

        public User Reviewer { get; set; }
        public User Manager { get; set; }
        public int PunctualityScore { get; set; }
        public int TeamworkScore { get; set; }
        public int ProductivityScore { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedDate { get; set; }

        public UpwardReview(User reviewer, User manager, int punctualityScore, int teamworkScore,
            int productivityScore, string? comments)
        {
            Reviewer = reviewer;
            Manager = manager;
            PunctualityScore = ValidateScore(punctualityScore);
            TeamworkScore = ValidateScore(teamworkScore);
            ProductivityScore = ValidateScore(productivityScore);
            Comments = comments;
            CreatedDate = DateTime.Now;
        }

        private int ValidateScore(int score)
        {
            if (score < MIN_SCORE) return MIN_SCORE;
            if (score > MAX_SCORE) return MAX_SCORE;
            return score;
        }

        public static bool IsValidScore(int score) => score >= MIN_SCORE && score <= MAX_SCORE;

        public int CalculateTotalScore()
        {
            return (PunctualityScore + TeamworkScore + ProductivityScore) / 3;
        }

        public double CalculateAverageScore()
        {
            return (PunctualityScore + TeamworkScore + ProductivityScore) / 3.0;
        }

        public string GetFeedbackRating()
        {
            double avg = CalculateAverageScore();
            if (avg >= 4.5) return "Excellent";
            if (avg >= 3.5) return "Good";
            if (avg >= 2.5) return "Average";
            if (avg >= 1.5) return "Below Average";
            return "Poor";
        }

        public bool HasComments() => !string.IsNullOrWhiteSpace(Comments);

        public long GetDaysSinceCreation() => (long)(DateTime.Now - CreatedDate).TotalDays;

        public bool IsRecent() => GetDaysSinceCreation() <= 30;

        public override string ToString()
        {
            return $"Upward Review for {Manager?.Name ?? "Unknown"} [Score: {CalculateTotalScore()}/5]";
        }
    }
}
