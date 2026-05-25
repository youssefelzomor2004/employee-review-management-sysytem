using PerformanceReview.Models;
using PerformanceReview.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceReview.Services
{
    public class PerformanceCalculator
    {
        public class ScoreResult
        {
            public double Punctuality { get; set; }
            public double Teamwork { get; set; }
            public double Productivity { get; set; }
            public double Overall { get; set; }
            public string Breakdown { get; set; }
            public List<string> MissingCategories { get; set; } = new();

            public ScoreResult(double punct, double team, double prod, string breakdown)
            {
                Punctuality = punct; Teamwork = team; Productivity = prod;
                Overall = (punct + team + prod) / 3.0;
                Breakdown = breakdown;
            }
        }

        public static ScoreResult CalculateScore(User user)
        {
            int tier = user.TierLevel;
            bool hasDirectReports = user.DirectReports?.Count > 0;

            if (tier == DataStore.MAX_TIERS) return CalculateHighboardScore(user);
            else if (tier == 1 && !hasDirectReports) return CalculateEmployeeScore(user);
            else return CalculateManagerEmployeeScore(user);
        }

        private static ScoreResult CalculateEmployeeScore(User user)
        {
            var reviews = GetReviewsFor(user);
            var managerReviews = new List<Review>();
            var peerReviews = new List<Review>();

            foreach (var r in reviews)
            {
                var reviewer = r.Manager;
                if (reviewer != null)
                {
                    if (reviewer.Equals(user.Manager)) managerReviews.Add(r);
                    else if (reviewer.TierLevel == user.TierLevel) peerReviews.Add(r);
                }
            }

            double mw = 0.70, pw = 0.30;
            var missing = new List<string>();

            if (managerReviews.Count == 0 && peerReviews.Count == 0)
                return new ScoreResult(0, 0, 0, "No reviews available");

            if (managerReviews.Count == 0) { missing.Add("Manager"); pw = 1.0; mw = 0; }
            else if (peerReviews.Count == 0) { missing.Add("Peers"); mw = 1.0; pw = 0; }

            var ma = AverageScores(managerReviews);
            var pa = AverageScores(peerReviews);

            double punct = ma[0] * mw + pa[0] * pw;
            double team = ma[1] * mw + pa[1] * pw;
            double prod = ma[2] * mw + pa[2] * pw;

            var sb = new StringBuilder();
            sb.AppendLine($"Manager ({(int)(mw * 100)}%): {managerReviews.Count} reviews");
            sb.Append($"Peers ({(int)(pw * 100)}%): {peerReviews.Count} reviews");
            if (missing.Count > 0) sb.Append($"\n* Missing: {string.Join(", ", missing)}");

            var result = new ScoreResult(punct, team, prod, sb.ToString());
            result.MissingCategories = missing;
            return result;
        }

        private static ScoreResult CalculateManagerEmployeeScore(User user)
        {
            var reviews = GetReviewsFor(user);
            var subReviews = new List<Review>();
            var peerReviews = new List<Review>();
            var upperReviews = new List<Review>();

            var drUsernames = new HashSet<string>();
            if (user.DirectReports != null)
                foreach (var dr in user.DirectReports) drUsernames.Add(dr.Username);

            foreach (var r in reviews)
            {
                var reviewer = r.Manager;
                if (reviewer != null)
                {
                    if (drUsernames.Contains(reviewer.Username)) subReviews.Add(r);
                    else if (reviewer.TierLevel == user.TierLevel) peerReviews.Add(r);
                    else if (reviewer.TierLevel > user.TierLevel) upperReviews.Add(r);
                }
            }

            double sw = 0.40, pw = 0.20, uw = 0.40;
            var missing = new List<string>();
            int active = 3;

            if (subReviews.Count == 0) { missing.Add("Subordinates"); active--; sw = 0; }
            if (peerReviews.Count == 0) { missing.Add("Peers"); active--; pw = 0; }
            if (upperReviews.Count == 0) { missing.Add("Upper Managers"); active--; uw = 0; }

            if (active == 0) return new ScoreResult(0, 0, 0, "No reviews available");

            double total = sw + pw + uw;
            if (total > 0 && total < 1.0) { double f = 1.0 / total; sw *= f; pw *= f; uw *= f; }

            var sa = AverageScores(subReviews);
            var pa = AverageScores(peerReviews);
            var ua = AverageScores(upperReviews);

            double punct = sa[0] * sw + pa[0] * pw + ua[0] * uw;
            double team = sa[1] * sw + pa[1] * pw + ua[1] * uw;
            double prod = sa[2] * sw + pa[2] * pw + ua[2] * uw;

            var sb = new StringBuilder();
            sb.AppendLine($"Subordinates ({(int)(sw * 100)}%): {subReviews.Count} reviews");
            sb.AppendLine($"Peers ({(int)(pw * 100)}%): {peerReviews.Count} reviews");
            sb.Append($"Upper Managers ({(int)(uw * 100)}%): {upperReviews.Count} reviews");
            if (missing.Count > 0) sb.Append($"\n* Missing: {string.Join(", ", missing)}");

            var result = new ScoreResult(punct, team, prod, sb.ToString());
            result.MissingCategories = missing;
            return result;
        }

        private static ScoreResult CalculateHighboardScore(User user)
        {
            var reviews = GetReviewsFor(user);
            var adminReviews = new List<Review>();
            var advisoryReviews = new List<Review>();

            foreach (var r in reviews)
            {
                if (r.Manager is Admin) adminReviews.Add(r);
                else advisoryReviews.Add(r);
            }

            if (adminReviews.Count == 0)
            {
                var sb2 = new StringBuilder("No admin reviews available");
                if (advisoryReviews.Count > 0) sb2.Append($"\nAdvisory reviews from lower tiers: {advisoryReviews.Count}");
                var result = new ScoreResult(0, 0, 0, sb2.ToString());
                result.MissingCategories.Add("Admin");
                return result;
            }

            var aa = AverageScores(adminReviews);
            var sb = new StringBuilder($"Admin (100%): {adminReviews.Count} reviews");
            if (advisoryReviews.Count > 0) sb.Append($"\nAdvisory (not counted): {advisoryReviews.Count} reviews");
            return new ScoreResult(aa[0], aa[1], aa[2], sb.ToString());
        }

        private static List<Review> GetReviewsFor(User user) =>
            DataStore.GetInstance().Reviews.Where(r => r.Employee?.Username == user.Username).ToList();

        private static double[] AverageScores(List<Review> reviews)
        {
            if (reviews.Count == 0) return new double[] { 0, 0, 0 };
            double sp = 0, st = 0, spr = 0;
            foreach (var r in reviews) { sp += r.PunctualityScore; st += r.TeamworkScore; spr += r.ProductivityScore; }
            int c = reviews.Count;
            return new double[] { sp / c, st / c, spr / c };
        }

        public static List<Review> GetAdvisoryReviews(User highboardManager) =>
            DataStore.GetInstance().Reviews.Where(r =>
                r.Employee?.Username == highboardManager.Username && r.Manager is not Admin).ToList();
    }
}
