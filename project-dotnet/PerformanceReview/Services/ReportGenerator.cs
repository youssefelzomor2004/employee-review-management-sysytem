using PerformanceReview.Models;
using PerformanceReview.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceReview.Services
{
    public class ReportGenerator
    {
        public string GenerateTopPerformersReport(User manager)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine("       TOP PERFORMERS REPORT");
            sb.AppendLine($"       Manager: {manager.Name}");
            sb.AppendLine("═══════════════════════════════════════════\n");

            var team = DataStore.GetInstance().GetAllDescendants(manager);
            if (team.Count == 0) { sb.AppendLine("No team members found."); return sb.ToString(); }

            var scores = new List<(User user, double score, int reviewCount)>();
            foreach (var emp in team)
            {
                var reviews = DataStore.GetInstance().GetReviewsForUser(emp);
                double avg = reviews.Count > 0 ? reviews.Average(r => r.CalculateTotalScore()) : 0;
                scores.Add((emp, avg, reviews.Count));
            }
            scores.Sort((a, b) => b.score.CompareTo(a.score));

            sb.AppendLine($"{"Rank",-5} {"Employee",-25} {"Avg Score",-10} {"Reviews",-10}");
            sb.AppendLine("───────────────────────────────────────────");
            int rank = 1;
            foreach (var (user, score, reviewCount) in scores)
                sb.AppendLine($"{rank++,-5} {user.Name,-25} {score,-10:F1} {reviewCount,-10}");

            return sb.ToString();
        }

        public string GenerateYearEndSummary(User employee)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine("       YEAR-END SUMMARY REPORT");
            sb.AppendLine($"       Employee: {employee.Name}");
            sb.AppendLine("═══════════════════════════════════════════\n");

            var reviews = DataStore.GetInstance().GetReviewsForUser(employee);
            if (reviews.Count == 0) { sb.AppendLine("No reviews found for this employee."); return sb.ToString(); }

            double totalP = 0, totalT = 0, totalPr = 0;
            int count = 0;
            sb.AppendLine("INDIVIDUAL REVIEWS:");
            sb.AppendLine("───────────────────────────────────────────");

            foreach (var r in reviews)
            {
                count++;
                sb.AppendLine($"Review #{count}:");
                sb.AppendLine($"  Punctuality:  {r.PunctualityScore}/10");
                sb.AppendLine($"  Teamwork:     {r.TeamworkScore}/10");
                sb.AppendLine($"  Productivity: {r.ProductivityScore}/10");
                sb.AppendLine($"  Comments: {r.Comments ?? "None"}\n");
                totalP += r.PunctualityScore; totalT += r.TeamworkScore; totalPr += r.ProductivityScore;
            }

            sb.AppendLine("───────────────────────────────────────────");
            sb.AppendLine("YEAR-END AVERAGES:");
            sb.AppendLine($"  Punctuality:  {totalP / count:F1} / 10");
            sb.AppendLine($"  Teamwork:     {totalT / count:F1} / 10");
            sb.AppendLine($"  Productivity: {totalPr / count:F1} / 10");
            sb.AppendLine($"  OVERALL:      {(totalP + totalT + totalPr) / (3 * count):F1} / 10");
            return sb.ToString();
        }

        public string GenerateSkillGapAnalysis(User manager)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine("       SKILL GAP ANALYSIS");
            sb.AppendLine($"       Team of: {manager.Name}");
            sb.AppendLine("═══════════════════════════════════════════\n");

            var team = DataStore.GetInstance().GetAllDescendants(manager);
            if (team.Count == 0) { sb.AppendLine("No team members found."); return sb.ToString(); }

            double totalP = 0, totalT = 0, totalPr = 0; int reviewCount = 0;
            foreach (var emp in team)
                foreach (var r in DataStore.GetInstance().GetReviewsForUser(emp))
                {
                    totalP += r.PunctualityScore; totalT += r.TeamworkScore; totalPr += r.ProductivityScore;
                    reviewCount++;
                }

            if (reviewCount == 0) { sb.AppendLine("No reviews found for team members."); return sb.ToString(); }

            double avgP = totalP / reviewCount, avgT = totalT / reviewCount, avgPr = totalPr / reviewCount;
            sb.AppendLine("TEAM AVERAGE SCORES:");
            sb.AppendLine("───────────────────────────────────────────");
            sb.AppendLine($"  Punctuality:  {avgP:F1} / 10  {GetSkillBar(avgP)}");
            sb.AppendLine($"  Teamwork:     {avgT:F1} / 10  {GetSkillBar(avgT)}");
            sb.AppendLine($"  Productivity: {avgPr:F1} / 10  {GetSkillBar(avgPr)}");
            sb.AppendLine();

            sb.AppendLine("SKILL GAP ANALYSIS:");
            sb.AppendLine("───────────────────────────────────────────");
            bool hasGaps = false;
            if (avgP < 5) { sb.AppendLine($"⚠ Punctuality needs improvement ({avgP:F1})"); hasGaps = true; }
            if (avgT < 5) { sb.AppendLine($"⚠ Teamwork needs improvement ({avgT:F1})"); hasGaps = true; }
            if (avgPr < 5) { sb.AppendLine($"⚠ Productivity needs improvement ({avgPr:F1})"); hasGaps = true; }
            if (!hasGaps) sb.AppendLine("✓ All skill areas are above threshold (5.0)");

            sb.AppendLine($"\nTotal reviews analyzed: {reviewCount}");
            sb.AppendLine($"Team size: {team.Count}");
            return sb.ToString();
        }

        private string GetSkillBar(double score)
        {
            int filled = (int)score;
            var bar = new StringBuilder("[");
            for (int i = 0; i < 10; i++) bar.Append(i < filled ? "█" : "░");
            bar.Append(']');
            return bar.ToString();
        }

        public string GenerateUpwardFeedbackReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine("       UPWARD FEEDBACK REPORT");
            sb.AppendLine("       (Anonymous Aggregated Data)");
            sb.AppendLine("═══════════════════════════════════════════\n");

            var allUp = DataStore.GetInstance().UpwardReviews;
            if (allUp.Count == 0) { sb.AppendLine("No upward reviews have been submitted yet."); return sb.ToString(); }

            var byManager = allUp.GroupBy(ur => ur.Manager.Username);
            foreach (var group in byManager)
            {
                var reviews = group.ToList();
                var mgr = reviews[0].Manager;
                sb.AppendLine($"Manager: {mgr.Name}");
                sb.AppendLine("───────────────────────────────────────────");
                double tP = reviews.Sum(r => r.PunctualityScore), tT = reviews.Sum(r => r.TeamworkScore), tPr = reviews.Sum(r => r.ProductivityScore);
                int c = reviews.Count;
                sb.AppendLine($"  Avg Punctuality:  {tP / c:F1} / 5");
                sb.AppendLine($"  Avg Teamwork:     {tT / c:F1} / 5");
                sb.AppendLine($"  Avg Productivity: {tPr / c:F1} / 5");
                sb.AppendLine($"  Number of reviews: {c}\n");
                sb.AppendLine("  Anonymous Feedback:");
                foreach (var r in reviews)
                    if (!string.IsNullOrWhiteSpace(r.Comments)) sb.AppendLine($"    • {r.Comments}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string GenerateSummaryReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine("       SYSTEM SUMMARY REPORT");
            sb.AppendLine("═══════════════════════════════════════════\n");

            var ds = DataStore.GetInstance();
            sb.AppendLine("USER STATISTICS:");
            sb.AppendLine("───────────────────────────────────────────");
            sb.AppendLine($"  Total Users:     {ds.Users.Count}");
            sb.AppendLine($"  Active:          {ds.Users.Count(u => u.Status == UserStatus.ACTIVE)}");
            sb.AppendLine($"  Pending:         {ds.Users.Count(u => u.Status == UserStatus.PENDING)}");
            sb.AppendLine($"  Suspended:       {ds.Users.Count(u => u.Status == UserStatus.SUSPENDED)}\n");

            sb.AppendLine("REVIEW STATISTICS:");
            sb.AppendLine("───────────────────────────────────────────");
            sb.AppendLine($"  Total Reviews:   {ds.Reviews.Count}");
            sb.AppendLine($"  Submitted:       {ds.Reviews.Count(r => r.Status == "SUBMITTED")}");
            sb.AppendLine($"  Draft:           {ds.Reviews.Count(r => r.Status == "DRAFT")}\n");

            sb.AppendLine("TRAINING STATISTICS:");
            sb.AppendLine("───────────────────────────────────────────");
            sb.AppendLine($"  Total Recommendations: {ds.TrainingRecommendations.Count}");
            sb.AppendLine($"  Available Courses:     {ds.AvailableCourses.Count}");
            return sb.ToString();
        }

        public static string GenerateTrainingReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine("       TRAINING REPORT");
            sb.AppendLine("═══════════════════════════════════════════\n");

            var ds = DataStore.GetInstance();
            sb.AppendLine("AVAILABLE COURSES:");
            sb.AppendLine("───────────────────────────────────────────");
            foreach (var c in ds.AvailableCourses) sb.AppendLine($"  • {c}");
            sb.AppendLine();
            sb.AppendLine("RECENT RECOMMENDATIONS:");
            sb.AppendLine("───────────────────────────────────────────");
            if (ds.TrainingRecommendations.Count == 0) sb.AppendLine("  No recommendations yet.");
            else foreach (var tr in ds.TrainingRecommendations)
                    sb.AppendLine($"  {tr.Recipient.Name} - {tr.CourseName} (by {tr.Recommender?.Name ?? "Admin"}) [{tr.Status}]");
            return sb.ToString();
        }

        public static string GenerateTeamReport(User manager)
        {
            var gen = new ReportGenerator();
            return gen.GenerateTopPerformersReport(manager) + "\n\n" + gen.GenerateSkillGapAnalysis(manager);
        }
    }
}
