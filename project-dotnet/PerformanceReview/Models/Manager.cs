using System.Collections.Generic;

namespace PerformanceReview.Models
{
    public class Manager : User
    {
        public Manager(string username, string password, string name)
            : base(username, password, name, UserRole.MANAGER_EMPLOYEE) { }

        public override bool CanReview(User employee)
        {
            if (employee == null) return false;
            if (Status != UserStatus.ACTIVE) return false;
            if (DirectReports != null && DirectReports.Contains(employee)) return true;
            return IsDescendant(employee);
        }

        public bool IsDescendant(User employee)
        {
            if (DirectReports == null) return false;
            foreach (var report in DirectReports)
            {
                if (report.Username == employee.Username) return true;
                if (report.DirectReports != null && report.DirectReports.Count > 0)
                {
                    if (IsDescendantOf(report, employee)) return true;
                }
            }
            return false;
        }

        private bool IsDescendantOf(User manager, User target)
        {
            if (manager.DirectReports == null) return false;
            foreach (var report in manager.DirectReports)
            {
                if (report.Username == target.Username) return true;
                if (IsDescendantOf(report, target)) return true;
            }
            return false;
        }

        public bool CanAcceptMoreReports()
        {
            int current = DirectReports?.Count ?? 0;
            return current < MaxDirectReports;
        }

        public int GetDirectReportCount()
        {
            return DirectReports?.Count ?? 0;
        }

        public int GetTotalTeamSize()
        {
            return CountDescendants(this);
        }

        private int CountDescendants(User user)
        {
            int count = 0;
            if (user.DirectReports != null)
            {
                count = user.DirectReports.Count;
                foreach (var report in user.DirectReports)
                    count += CountDescendants(report);
            }
            return count;
        }

        public bool CanAssignTraining(User employee)
        {
            return CanReview(employee) && employee.Status == UserStatus.ACTIVE;
        }

        public bool CanSetGoalsFor(User employee)
        {
            return CanReview(employee) && employee.Status == UserStatus.ACTIVE;
        }

        public bool IsValidScore(int score)
        {
            return score >= 1 && score <= 10;
        }

        public Review CreateValidatedReview(User employee, int punct, int team, int prod)
        {
            if (!CanReview(employee))
                throw new System.ArgumentException("Cannot review this employee");
            if (!IsValidScore(punct) || !IsValidScore(team) || !IsValidScore(prod))
                throw new System.ArgumentException("Scores must be between 1 and 10");

            var review = new Review(employee, this);
            review.PunctualityScore = punct;
            review.TeamworkScore = team;
            review.ProductivityScore = prod;
            return review;
        }

        public override string ToString()
        {
            return $"{Username} ({Name})";
        }
    }
}
