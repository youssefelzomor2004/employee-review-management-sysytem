using System;

namespace PerformanceReview.Models
{
    public class PeerReview
    {
        public const int MIN_SCORE = 1;
        public const int MAX_SCORE = 5;

        public User Reviewer { get; set; }
        public User Reviewed { get; set; }
        public int CollaborationScore { get; set; }
        public int CommunicationScore { get; set; }
        public int TeamworkScore { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedDate { get; set; }

        public PeerReview(User reviewer, User reviewed, int collaboration, int communication,
            int teamwork, string? comments)
        {
            Reviewer = reviewer;
            Reviewed = reviewed;
            CollaborationScore = ValidateScore(collaboration);
            CommunicationScore = ValidateScore(communication);
            TeamworkScore = ValidateScore(teamwork);
            Comments = comments;
            CreatedDate = DateTime.Now;
        }

        private int ValidateScore(int score)
        {
            if (score < MIN_SCORE) return MIN_SCORE;
            if (score > MAX_SCORE) return MAX_SCORE;
            return score;
        }

        public double CalculateAverageScore()
        {
            return (CollaborationScore + CommunicationScore + TeamworkScore) / 3.0;
        }

        public static bool AreValidPeers(User user1, User user2)
        {
            if (user1 == null || user2 == null) return false;
            if (user1.Manager == null || user2.Manager == null) return false;
            if (user1.Username == user2.Username) return false;
            return user1.Manager.Username == user2.Manager.Username;
        }

        public bool IsAbout(User user)
        {
            return Reviewed != null && Reviewed.Username == user.Username;
        }

        public override string ToString()
        {
            return $"Peer Review for {Reviewed?.Name ?? "Unknown"} [Score: {CalculateAverageScore():F1}/5]";
        }
    }
}
