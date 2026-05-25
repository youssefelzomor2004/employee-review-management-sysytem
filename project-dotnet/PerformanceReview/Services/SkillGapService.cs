using PerformanceReview.Models;
using PerformanceReview.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceReview.Services
{
    public class SkillGapService
    {
        private static SkillGapService? _instance;
        private const double GAP_THRESHOLD = 3.0;

        private SkillGapService() { }

        public static SkillGapService GetInstance()
        {
            if (_instance == null) _instance = new SkillGapService();
            return _instance;
        }

        public class SkillGapResult
        {
            public User User { get; set; }
            public double Punctuality { get; set; }
            public double Teamwork { get; set; }
            public double Productivity { get; set; }
            public double Leadership { get; set; }
            public double Vision { get; set; }
            public double Managerial { get; set; }
            public bool ShowAllMetrics { get; set; }
            public List<string> Gaps { get; set; } = new();

            public SkillGapResult(User user, bool showAllMetrics)
            {
                User = user; ShowAllMetrics = showAllMetrics;
            }

            public bool HasGaps() => Gaps.Count > 0;
            public string GetSummary() => Gaps.Count == 0 ? "No skill gaps identified" : string.Join(", ", Gaps);
        }

        private bool ShouldShowAllMetrics(User user)
        {
            if (user == null) return false;
            bool hasReports = user.DirectReports?.Count > 0;
            bool isHighboard = user.TierLevel == DataStore.MAX_TIERS;
            return hasReports || isHighboard;
        }

        public SkillGapResult? AnalyzeUser(User user)
        {
            if (user == null) return null;

            bool showAll = ShouldShowAllMetrics(user);
            var result = new SkillGapResult(user, showAll);

            var scores = ReviewService.GetInstance().GetAggregatedScores(user);
            result.Punctuality = scores[0]; result.Teamwork = scores[1]; result.Productivity = scores[2];

            if (scores[3] > 0)
            {
                if (scores[0] < GAP_THRESHOLD) result.Gaps.Add("Punctuality");
                if (scores[1] < GAP_THRESHOLD) result.Gaps.Add("Teamwork");
                if (scores[2] < GAP_THRESHOLD) result.Gaps.Add("Productivity");
            }

            if (showAll)
            {
                var ext = GetExtendedScores(user);
                result.Leadership = ext[0]; result.Vision = ext[1]; result.Managerial = ext[2];

                if (ext[3] > 0)
                {
                    if (ext[0] < GAP_THRESHOLD) result.Gaps.Add("Leadership");
                    if (ext[1] < GAP_THRESHOLD) result.Gaps.Add("Vision");
                    if (ext[2] < GAP_THRESHOLD) result.Gaps.Add("Managerial Skills");
                }
            }
            return result;
        }

        private double[] GetExtendedScores(User user)
        {
            var reviews = DataStore.GetInstance().GetReviewsForUser(user);
            if (reviews.Count == 0) return new double[] { 0, 0, 0, 0 };

            double tL = 0, tV = 0, tM = 0; int count = 0;
            foreach (var r in reviews)
            {
                if (r.HasExtendedScores())
                {
                    tL += r.LeadershipScore; tV += r.VisionScore; tM += r.ManagerialSkillsScore;
                    count++;
                }
            }
            return count == 0 ? new double[] { 0, 0, 0, 0 } : new double[] { tL / count, tV / count, tM / count, count };
        }

        public List<SkillGapResult> AnalyzeTeam(User manager)
        {
            var results = new List<SkillGapResult>();
            if (manager == null) return results;
            foreach (var member in UserService.GetInstance().GetAllDescendants(manager))
            {
                var r = AnalyzeUser(member);
                if (r != null) results.Add(r);
            }
            return results;
        }

        public List<SkillGapResult> AnalyzeAll()
        {
            var results = new List<SkillGapResult>();
            foreach (var user in DataStore.GetInstance().Users)
            {
                if (user is Admin) continue;
                var r = AnalyzeUser(user);
                if (r != null) results.Add(r);
            }
            return results;
        }

        public string GetTeamGapSummary(User manager)
        {
            var results = AnalyzeTeam(manager);
            if (results.Count == 0) return "No team members to analyze.";

            int withGaps = 0, pG = 0, tG = 0, prG = 0, lG = 0, vG = 0, mG = 0;
            foreach (var r in results)
            {
                if (r.HasGaps()) withGaps++;
                foreach (var gap in r.Gaps)
                {
                    switch (gap)
                    {
                        case "Punctuality": pG++; break;
                        case "Teamwork": tG++; break;
                        case "Productivity": prG++; break;
                        case "Leadership": lG++; break;
                        case "Vision": vG++; break;
                        case "Managerial Skills": mG++; break;
                    }
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Team Size: {results.Count}");
            sb.AppendLine($"Members with Gaps: {withGaps}\n");
            sb.AppendLine("Gap Breakdown:");
            sb.AppendLine($"  Punctuality: {pG} members");
            sb.AppendLine($"  Teamwork: {tG} members");
            sb.AppendLine($"  Productivity: {prG} members");
            if (lG > 0 || vG > 0 || mG > 0)
            {
                sb.AppendLine($"  Leadership: {lG} members");
                sb.AppendLine($"  Vision: {vG} members");
                sb.AppendLine($"  Managerial: {mG} members");
            }
            return sb.ToString();
        }
    }
}
