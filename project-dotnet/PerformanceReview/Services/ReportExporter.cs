using Microsoft.Win32;
using PerformanceReview.Models;
using PerformanceReview.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace PerformanceReview.Services
{
    public static class ReportExporter
    {
        public static void ExportUserReport(User user, Window owner)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Export Performance Report",
                FileName = $"{user.Username}_report.html",
                Filter = "HTML Files|*.html"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, GenerateUserReportHTML(user));
                    MessageBox.Show($"Report saved to:\n{dialog.FileName}\n\nOpen in browser and use Print -> Save as PDF",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    Process.Start(new ProcessStartInfo { FileName = dialog.FileName, UseShellExecute = true });
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error: {e.Message}", "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public static void ExportAllUsersReport(Window owner)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Export All Users Report",
                FileName = "performance_report_all.html",
                Filter = "HTML Files|*.html"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, GenerateAllUsersReportHTML());
                    MessageBox.Show("Report saved! Open in browser and Print -> Save as PDF",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    Process.Start(new ProcessStartInfo { FileName = dialog.FileName, UseShellExecute = true });
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error: {e.Message}", "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public static void ExportTeamReport(User manager, Window owner)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Export Team Report",
                FileName = $"{manager.Username}_team_report.html",
                Filter = "HTML Files|*.html"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, GenerateTeamReportHTML(manager));
                    MessageBox.Show("Team report saved! Open in browser and Print -> Save as PDF",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    Process.Start(new ProcessStartInfo { FileName = dialog.FileName, UseShellExecute = true });
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error: {e.Message}", "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private static string GenerateUserReportHTML(User user)
        {
            var score = PerformanceCalculator.CalculateScore(user);
            var reviews = DataStore.GetInstance().GetReviewsForUser(user);
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset='UTF-8'>");
            sb.Append($"<title>Performance Report - {user.Name}</title>");
            sb.Append("<style>");
            sb.Append("body { font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 20px; }");
            sb.Append("h1 { color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px; }");
            sb.Append("h2 { color: #34495e; margin-top: 30px; }");
            sb.Append("table { width: 100%; border-collapse: collapse; margin: 15px 0; }");
            sb.Append("th, td { padding: 12px; text-align: left; border: 1px solid #ddd; }");
            sb.Append("th { background: #3498db; color: white; }");
            sb.Append(".score-box { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 10px; margin: 20px 0; }");
            sb.Append(".big-score { font-size: 48px; font-weight: bold; }");
            sb.Append(".good { background: #d4edda; } .warning { background: #fff3cd; } .poor { background: #f8d7da; }");
            sb.Append("</style></head><body>");

            sb.Append($"<h1>📊 Performance Report</h1>");
            sb.Append($"<p><strong>Employee:</strong> {user.Name}</p>");
            sb.Append($"<p><strong>Tier Level:</strong> {user.TierLevel}</p>");
            sb.Append($"<p><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm}</p>");

            sb.Append($"<div class='score-box'><h2 style='color:white'>Overall Performance Score</h2>");
            sb.Append($"<div class='big-score'>{score.Overall:F1} / 10</div></div>");

            sb.Append("<h2>📊 Score Breakdown</h2><table>");
            sb.Append("<tr><th>Category</th><th>Score</th><th>Rating</th></tr>");
            sb.Append(CreateScoreRow("Punctuality", score.Punctuality));
            sb.Append(CreateScoreRow("Teamwork", score.Teamwork));
            sb.Append(CreateScoreRow("Productivity", score.Productivity));
            sb.Append("</table>");

            sb.Append($"<h2>📝 Reviews Received ({reviews.Count})</h2>");
            if (reviews.Count > 0)
            {
                sb.Append("<table><tr><th>#</th><th>Punct</th><th>Team</th><th>Prod</th><th>Total</th></tr>");
                int i = 1;
                foreach (var r in reviews)
                    sb.Append($"<tr><td>{i++}</td><td>{r.PunctualityScore}</td><td>{r.TeamworkScore}</td><td>{r.ProductivityScore}</td><td><strong>{r.CalculateTotalScore()}</strong></td></tr>");
                sb.Append("</table>");
            }

            sb.Append("<hr><p style='text-align:center; color:#888;'>Generated by Performance Review System</p></body></html>");
            return sb.ToString();
        }

        private static string CreateScoreRow(string category, double score)
        {
            string cls = score >= 7 ? "good" : score >= 5 ? "warning" : "poor";
            string rating = score >= 7 ? "Excellent" : score >= 5 ? "Good" : "Needs Improvement";
            return $"<tr><td>{category}</td><td class='{cls}'>{score:F1}</td><td class='{cls}'>{rating}</td></tr>";
        }

        private static string GenerateTeamReportHTML(User manager)
        {
            var team = UserService.GetInstance().GetAllDescendants(manager);
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset='UTF-8'>");
            sb.Append($"<title>Team Performance Report - {manager.Name}</title>");
            sb.Append("<style>body{font-family:Arial;max-width:1000px;margin:0 auto;padding:20px}h1{color:#2c3e50;border-bottom:2px solid #3498db}");
            sb.Append("table{width:100%;border-collapse:collapse;margin:15px 0}th,td{padding:10px;border:1px solid #ddd}th{background:#3498db;color:white}");
            sb.Append(".good{background:#d4edda!important}.warning{background:#fff3cd!important}.poor{background:#f8d7da!important}</style></head><body>");

            sb.Append($"<h1>📊 Team Performance Report</h1><p><strong>Manager:</strong> {manager.Name}</p>");
            sb.Append($"<p><strong>Team Size:</strong> {team.Count} members</p>");
            sb.Append("<table><tr><th>Name</th><th>Tier</th><th>Role</th><th>Punct</th><th>Team</th><th>Prod</th><th>Overall</th></tr>");

            foreach (var u in team)
            {
                var s = PerformanceCalculator.CalculateScore(u);
                string role = GetRole(u);
                string cls = s.Overall >= 7 ? "good" : s.Overall >= 5 ? "warning" : "poor";
                sb.Append($"<tr class='{cls}'><td>{u.Name}</td><td>{u.TierLevel}</td><td>{role}</td>");
                sb.Append($"<td>{s.Punctuality:F1}</td><td>{s.Teamwork:F1}</td><td>{s.Productivity:F1}</td><td><strong>{s.Overall:F1}</strong></td></tr>");
            }
            sb.Append("</table></body></html>");
            return sb.ToString();
        }

        private static string GenerateAllUsersReportHTML()
        {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset='UTF-8'><title>All Users Performance Report</title>");
            sb.Append("<style>body{font-family:Arial;max-width:1000px;margin:0 auto;padding:20px}h1{color:#2c3e50}");
            sb.Append("table{width:100%;border-collapse:collapse}th,td{padding:10px;border:1px solid #ddd}th{background:#3498db;color:white}");
            sb.Append(".good{background:#d4edda!important}.warning{background:#fff3cd!important}.poor{background:#f8d7da!important}</style></head><body>");
            sb.Append($"<h1>📈 Organization Performance Report</h1><p><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm}</p>");
            sb.Append("<table><tr><th>Name</th><th>Username</th><th>Tier</th><th>Role</th><th>Punct</th><th>Team</th><th>Prod</th><th>Overall</th></tr>");

            foreach (var u in DataStore.GetInstance().Users.Where(u => u is not Admin))
            {
                var s = PerformanceCalculator.CalculateScore(u);
                string cls = s.Overall >= 7 ? "good" : s.Overall >= 5 ? "warning" : "poor";
                sb.Append($"<tr class='{cls}'><td>{u.Name}</td><td>{u.Username}</td><td>{u.TierLevel}</td><td>{GetRole(u)}</td>");
                sb.Append($"<td>{s.Punctuality:F1}</td><td>{s.Teamwork:F1}</td><td>{s.Productivity:F1}</td><td><strong>{s.Overall:F1}</strong></td></tr>");
            }
            sb.Append("</table></body></html>");
            return sb.ToString();
        }

        private static string GetRole(User u)
        {
            int tier = u.TierLevel;
            bool hasReports = u.DirectReports?.Count > 0;
            if (tier == DataStore.MAX_TIERS) return "Highboard Manager";
            if (tier == 1 && !hasReports) return "Employee";
            return "Manager & Employee";
        }
    }
}
