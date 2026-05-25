namespace PerformanceReview.Models
{
    public class Employee : User
    {
        public Employee(string username, string password, string name)
            : base(username, password, name, UserRole.EMPLOYEE) { }

        public bool CanReviewManager()
        {
            return Manager != null && Status == UserStatus.ACTIVE;
        }

        public bool HasManager()
        {
            return Manager != null;
        }

        public string GetTierDescription()
        {
            if (TierLevel == 1 && (DirectReports == null || DirectReports.Count == 0))
                return "Employee (Tier 1)";
            return $"Employee & Manager (Tier {TierLevel})";
        }

        public bool IsEligibleForPromotion(double averageScore)
        {
            return averageScore >= 7.0 && Status == UserStatus.ACTIVE;
        }

        public string GetDisplayName()
        {
            return $"{Name} ({Username})";
        }

        public bool CanHaveGoals()
        {
            return Status == UserStatus.ACTIVE;
        }

        public bool CanReceiveTraining()
        {
            return Status == UserStatus.ACTIVE;
        }

        public override string ToString()
        {
            return $"{Username} ({Name})";
        }
    }
}
