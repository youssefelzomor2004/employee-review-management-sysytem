using PerformanceReview.Utilities;

namespace PerformanceReview.Models
{
    public class Admin : User
    {
        public Admin(string username, string password, string name)
            : base(username, password, name, UserRole.ADMIN)
        {
            Status = UserStatus.ACTIVE;
            TierLevel = DataStore.MAX_TIERS + 1;
        }

        public new int TierLevel
        {
            get => DataStore.MAX_TIERS + 1;
            set => base.TierLevel = value;
        }

        public bool CanApprove(User user)
        {
            return user != null && user.Status == UserStatus.PENDING && !(user is Admin);
        }

        public bool ApproveUser(User user)
        {
            if (!CanApprove(user)) return false;
            user.SetStatus(UserStatus.ACTIVE);
            return true;
        }

        public bool CanSuspend(User user)
        {
            if (user == null || user is Admin) return false;
            if (user.Status == UserStatus.SUSPENDED) return false;
            return user.DirectReports == null || user.DirectReports.Count == 0;
        }

        public bool SuspendUser(User user)
        {
            if (!CanSuspend(user)) return false;
            user.SetStatus(UserStatus.SUSPENDED);
            return true;
        }

        public bool ReactivateUser(User user)
        {
            if (user == null || user is Admin) return false;
            if (user.Status != UserStatus.SUSPENDED) return false;
            user.SetStatus(UserStatus.ACTIVE);
            return true;
        }

        public bool CanDelete(User user)
        {
            if (user == null || user is Admin) return false;
            return true;
        }

        public bool CanAssignHierarchy(User employee, User? manager, int tier)
        {
            if (employee == null || employee is Admin) return false;
            if (tier < 1) return false;
            if (manager == null) return tier == 1;
            return tier < manager.TierLevel;
        }

        public bool AssignToHierarchy(User employee, User? manager, int tier)
        {
            if (!CanAssignHierarchy(employee, manager, tier)) return false;

            var oldManager = employee.Manager;
            if (oldManager?.DirectReports != null)
                oldManager.DirectReports.Remove(employee);

            employee.Manager = manager;
            employee.TierLevel = tier;
            employee.SetStatus(UserStatus.ACTIVE);

            manager?.AddDirectReport(employee);
            return true;
        }

        public bool IsValidTierConfig(int tier, int maxTiers)
        {
            return tier >= 1 && tier <= maxTiers;
        }

        public bool CanModifyMaxTiers(int newMaxTiers, int currentHighestTier)
        {
            return newMaxTiers >= currentHighestTier && newMaxTiers >= 1;
        }

        public override string ToString()
        {
            return $"Admin: {Username}";
        }
    }
}
