using PerformanceReview.Models;
using PerformanceReview.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerformanceReview.Services
{
    public class ReviewService
    {
        private static ReviewService? _instance;
        private readonly DataStore _dataStore;

        private ReviewService() { _dataStore = DataStore.GetInstance(); }

        public static ReviewService GetInstance()
        {
            if (_instance == null) _instance = new ReviewService();
            return _instance;
        }

        public Review? CreateReview(User employee, User reviewer,
            int punctuality, int teamwork, int productivity,
            int leadership, int vision, int managerialSkills,
            string? comments, bool isDraft)
        {
            if (!isDraft && HasRecentReview(reviewer, employee))
            {
                Logger.Warn($"Duplicate review blocked: {reviewer.Username} -> {employee.Username}");
                return null;
            }

            var review = new Review(employee, reviewer)
            {
                PunctualityScore = punctuality,
                TeamworkScore = teamwork,
                ProductivityScore = productivity,
                LeadershipScore = leadership,
                VisionScore = vision,
                ManagerialSkillsScore = managerialSkills,
                Comments = comments,
                Status = isDraft ? "DRAFT" : "SUBMITTED"
            };

            _dataStore.AddReview(review);
            _dataStore.SaveData();
            Logger.Info($"Review {(isDraft ? "saved as draft" : "submitted")}: {reviewer.Username} -> {employee.Username}");
            return review;
        }

        public bool HasRecentReview(User reviewer, User employee)
        {
            long thirtyDays = 30L * 24 * 60 * 60 * 1000;
            return _dataStore.Reviews.Any(r =>
                r.Reviewer.Username == reviewer.Username &&
                r.Employee.Username == employee.Username &&
                r.Status == "SUBMITTED" &&
                (DateTime.Now - r.CreatedDate).TotalMilliseconds < thirtyDays);
        }

        public Review? CreateBasicReview(User employee, User reviewer,
            int punctuality, int teamwork, int productivity, string? comments)
        {
            return CreateReview(employee, reviewer, punctuality, teamwork, productivity, 0, 0, 0, comments, false);
        }

        public void SubmitReview(Review review)
        {
            review.Status = "SUBMITTED";
            _dataStore.SaveData();
            Logger.Info($"Draft review submitted: {review.Reviewer.Username} -> {review.Employee.Username}");
        }

        public bool DeleteReview(Review review, User requester)
        {
            if (review.Reviewer?.Username == requester.Username)
            {
                _dataStore.RemoveReview(review);
                _dataStore.SaveData();
                Logger.Info($"Review deleted by {requester.Username}");
                return true;
            }
            return false;
        }

        public void DeleteReviewAdmin(Review review)
        {
            _dataStore.RemoveReview(review);
            _dataStore.SaveData();
            Logger.Info("Review deleted by Admin");
        }

        public List<Review> GetReviewsForUser(User user) => _dataStore.GetReviewsForUser(user);

        public List<Review> GetReviewsGivenBy(User reviewer) =>
            _dataStore.Reviews.Where(r => r.Reviewer?.Username == reviewer.Username).ToList();

        public List<Review> GetAllReviews() => _dataStore.Reviews;

        public UpwardReview? CreateUpwardReview(User employee, User manager,
            int punctuality, int teamwork, int productivity, string? comments)
        {
            if (HasRecentUpwardReview(employee, manager))
            {
                Logger.Warn($"Duplicate upward review blocked: {employee.Username} -> {manager.Username}");
                return null;
            }
            var ur = new UpwardReview(employee, manager, punctuality, teamwork, productivity, comments);
            _dataStore.AddUpwardReview(ur);
            _dataStore.SaveData();
            Logger.Info($"Upward review submitted: {employee.Username} -> {manager.Username}");
            return ur;
        }

        public bool HasRecentUpwardReview(User employee, User manager)
        {
            long thirtyDays = 30L * 24 * 60 * 60 * 1000;
            return _dataStore.UpwardReviews.Any(r =>
                r.Reviewer.Username == employee.Username &&
                r.Manager.Username == manager.Username &&
                (DateTime.Now - r.CreatedDate).TotalMilliseconds < thirtyDays);
        }

        public List<UpwardReview> GetAllUpwardReviews() => _dataStore.UpwardReviews;

        public List<UpwardReview> GetUpwardReviewsForManager(User manager) =>
            _dataStore.UpwardReviews.Where(ur => ur.Manager.Username == manager.Username).ToList();

        public double[] GetAggregatedScores(User user)
        {
            var reviews = GetReviewsForUser(user).Where(r => r.Status == "SUBMITTED").ToList();
            if (reviews.Count == 0) return new double[] { 0, 0, 0, 0 };

            double sumP = 0, sumT = 0, sumPr = 0;
            foreach (var r in reviews) { sumP += r.PunctualityScore; sumT += r.TeamworkScore; sumPr += r.ProductivityScore; }
            int count = reviews.Count;
            return new double[] { sumP / count, sumT / count, sumPr / count, count };
        }

        public double GetOverallAverageScore(User user)
        {
            var s = GetAggregatedScores(user);
            return s[3] == 0 ? 0 : (s[0] + s[1] + s[2]) / 3.0;
        }

        public double[] GetAggregatedPeerScores(User user)
        {
            var reviews = _dataStore.GetPeerReviewsFor(user);
            if (reviews.Count == 0) return new double[] { 0, 0, 0, 0 };

            double sumC = 0, sumCo = 0, sumT = 0;
            foreach (var r in reviews) { sumC += r.CollaborationScore; sumCo += r.CommunicationScore; sumT += r.TeamworkScore; }
            int count = reviews.Count;
            return new double[] { sumC / count, sumCo / count, sumT / count, count };
        }

        public PeerReview? CreatePeerReview(User reviewer, User reviewed,
            int collaboration, int communication, int teamwork, string? comments)
        {
            if (!PeerReview.AreValidPeers(reviewer, reviewed)) return null;
            if (HasRecentPeerReview(reviewer, reviewed)) return null;

            var pr = new PeerReview(reviewer, reviewed, collaboration, communication, teamwork, comments);
            _dataStore.AddPeerReview(pr);
            _dataStore.SaveData();
            return pr;
        }

        public bool HasRecentPeerReview(User reviewer, User reviewed)
        {
            long thirtyDays = 30L * 24 * 60 * 60 * 1000;
            return _dataStore.PeerReviews.Any(r =>
                r.Reviewer.Username == reviewer.Username &&
                r.Reviewed.Username == reviewed.Username &&
                (DateTime.Now - r.CreatedDate).TotalMilliseconds < thirtyDays);
        }

        public List<PeerReview> GetPeerReviewsFor(User user) => _dataStore.GetPeerReviewsFor(user);
        public List<PeerReview> GetAllPeerReviews() => _dataStore.PeerReviews;

        public List<UpwardReview> GetUpwardReviewsGivenBy(User reviewer) =>
            _dataStore.UpwardReviews.Where(ur => ur.Reviewer?.Username == reviewer.Username).ToList();

        public List<PeerReview> GetPeerReviewsGivenBy(User reviewer) =>
            _dataStore.PeerReviews.Where(pr => pr.Reviewer?.Username == reviewer.Username).ToList();
    }
}
