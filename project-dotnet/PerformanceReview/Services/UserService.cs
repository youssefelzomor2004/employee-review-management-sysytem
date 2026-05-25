using PerformanceReview.Models;
using PerformanceReview.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace PerformanceReview.Services
{
    public class UserService
    {
        private static UserService? _instance;
        private readonly DataStore _dataStore;

        private UserService() { _dataStore = DataStore.GetInstance(); }

        public static UserService GetInstance()
        {
            if (_instance == null) _instance = new UserService();
            return _instance;
        }

        public User? Authenticate(string username, string password)
        {
            var user = _dataStore.Authenticate(username, password);
            if (user != null) Logger.Info($"User logged in: {username} ({user.Status})");
            else Logger.Warn($"Failed login attempt for username: {username}");
            return user;
        }

        public bool RegisterUser(string username, string password, string name)
        {
            if (_dataStore.Users.Any(u => u.Username == username))
            {
                Logger.Warn($"Registration failed: Username '{username}' already taken.");
                return false;
            }
            var newUser = new Employee(username, password, name);
            newUser.SetStatus(UserStatus.PENDING);
            newUser.TierLevel = 1;
            _dataStore.AddUser(newUser);
            _dataStore.SaveData();
            Logger.Info($"New user registered: {username} (Pending Approval)");
            return true;
        }

        public List<User> GetAllUsers() => _dataStore.Users;

        public List<User> GetNonAdminUsers() =>
            _dataStore.Users.Where(u => u is not Admin).ToList();

        public List<User> GetPendingUsers() =>
            _dataStore.Users.Where(u => u.Status == UserStatus.PENDING).ToList();

        public void ApproveUser(User user)
        {
            user.SetStatus(UserStatus.ACTIVE);
            _dataStore.SaveData();
            Logger.Info($"User approved: {user.Username}");
        }

        public bool SuspendUser(User user)
        {
            if (user is Admin) return false;
            if (user.DirectReports != null && user.DirectReports.Count > 0)
            {
                Logger.Warn($"Failed to suspend {user.Username}: Has direct reports.");
                return false;
            }
            user.SetStatus(UserStatus.SUSPENDED);
            _dataStore.SaveData();
            Logger.Info($"User suspended: {user.Username}");
            return true;
        }

        public void ActivateUser(User user)
        {
            user.SetStatus(UserStatus.ACTIVE);
            _dataStore.SaveData();
            Logger.Info($"User reactivated: {user.Username}");
        }

        public bool ToggleUserStatus(User user)
        {
            if (user is Admin) return false;
            if (user.Status != UserStatus.SUSPENDED)
            {
                if (user.DirectReports != null && user.DirectReports.Count > 0)
                {
                    Logger.Warn($"Failed to suspend {user.Username}: Has direct reports.");
                    return false;
                }
                user.SetStatus(UserStatus.SUSPENDED);
                Logger.Info($"User suspended: {user.Username}");
            }
            else
            {
                user.SetStatus(UserStatus.PENDING);
                Logger.Info($"User set to pending: {user.Username}");
            }
            _dataStore.SaveData();
            return true;
        }

        public void EditUserName(User user, string newName)
        {
            user.Name = newName;
            _dataStore.SaveData();
        }

        public bool DeleteUser(User user)
        {
            if (user is Admin) return false;
            var manager = user.Manager;
            manager?.DirectReports?.Remove(user);
            if (user.DirectReports != null)
                foreach (var report in user.DirectReports)
                    report.Manager = null;
            _dataStore.Users.Remove(user);
            _dataStore.SaveData();
            return true;
        }

        public User? FindByUsername(string username) =>
            _dataStore.Users.FirstOrDefault(u => u.Username == username);

        public void AssignHierarchy(User employee, User? manager, int tierLevel)
        {
            if (manager != null)
            {
                if (employee.Username == manager.Username) return;
                var descendants = GetAllDescendants(employee);
                if (descendants.Contains(manager)) return;
            }

            var oldManager = employee.Manager;
            oldManager?.DirectReports?.Remove(employee);

            employee.Manager = manager;
            employee.TierLevel = tierLevel;
            employee.SetStatus(UserStatus.ACTIVE);
            manager?.AddDirectReport(employee);
            _dataStore.SaveData();
        }

        public List<User> GetAllDescendants(User manager) => _dataStore.GetAllDescendants(manager);
        public List<User> GetChainOfCommand(User user) => _dataStore.GetChainOfCommand(user);

        public List<User> GetManagers() =>
            _dataStore.Users.Where(u => u.DirectReports?.Count > 0).ToList();

        public string CalculateRole(User user)
        {
            if (user is Admin) return "Admin";
            int tier = user.TierLevel;
            bool hasReports = user.DirectReports?.Count > 0;
            var mgr = user.Manager;
            if (tier == DataStore.MAX_TIERS) return "Highboard Manager";
            if (hasReports && mgr != null) return "Manager & Employee";
            if (hasReports) return "Manager";
            return "Employee";
        }

        public int CountEmployeesAtTier(int tierLevel) =>
            _dataStore.Users.Count(u => u is not Admin && u.Status == UserStatus.ACTIVE && u.TierLevel == tierLevel);

        public bool CanAssignToTier(int tierLevel)
        {
            if (tierLevel == DataStore.MAX_TIERS) return true;
            for (int tier = tierLevel + 1; tier <= DataStore.MAX_TIERS; tier++)
                if (CountEmployeesAtTier(tier) == 0) return false;
            return true;
        }

        public string? GetTierBlockedReason(int tierLevel)
        {
            if (tierLevel == DataStore.MAX_TIERS) return null;
            var missing = new List<string>();
            for (int tier = tierLevel + 1; tier <= DataStore.MAX_TIERS; tier++)
                if (CountEmployeesAtTier(tier) == 0) missing.Add(tier.ToString());
            return missing.Count > 0 ? $"Need at least one employee in tier(s): {string.Join(", ", missing)}" : null;
        }

        public bool CanChangeTier(User employee, int newTier)
        {
            if (employee == null) return false;
            if (employee.TierLevel == newTier) return true;
            if (employee.DirectReports?.Count > 0) return false;
            if (employee.Status == UserStatus.ACTIVE && CountEmployeesAtTier(employee.TierLevel) == 1) return false;
            return true;
        }

        public string? GetTierChangeBlockedReason(User employee, int newTier)
        {
            if (employee == null) return "No employee selected";
            if (employee.TierLevel == newTier) return null;
            if (employee.DirectReports?.Count > 0) return "Cannot change tier: Employee is a manager with direct reports";
            if (employee.Status == UserStatus.ACTIVE && CountEmployeesAtTier(employee.TierLevel) == 1)
                return $"Cannot change tier: Only employee at Tier {employee.TierLevel}";
            return null;
        }

        public string? ValidateHierarchyAssignment(User employee, User? manager, int tierLevel)
        {
            var reason = GetTierBlockedReason(tierLevel);
            if (reason != null) return reason;
            reason = GetTierChangeBlockedReason(employee, tierLevel);
            if (reason != null) return reason;
            if (tierLevel < DataStore.MAX_TIERS && manager == null)
                return $"Manager is required for Tier {tierLevel}";
            return null;
        }

        public void ResetSystem()
        {
            foreach (var u in _dataStore.Users.Where(u => u is not Admin))
            {
                u.SetStatus(UserStatus.PENDING);
                u.Manager = null;
                u.DirectReports.Clear();
                u.TierLevel = User.TIER_UNASSIGNED;
            }
            _dataStore.SaveData();
        }

        public int GetMaxTiers() => DataStore.MAX_TIERS;

        public void SetMaxTiers(int tiers)
        {
            int old = DataStore.MAX_TIERS;
            if (tiers != old)
            {
                DataStore.MAX_TIERS = tiers;
                foreach (var user in _dataStore.Users.Where(u => u is not Admin))
                {
                    user.SetStatus(UserStatus.PENDING);
                    user.TierLevel = 1;
                    user.Manager = null;
                    user.DirectReports?.Clear();
                }
                Logger.Info($"Max tiers changed from {old} to {tiers}. All users reset to PENDING.");
            }
            _dataStore.SaveData();
        }

        public void ResetAllEmployees()
        {
            foreach (var user in _dataStore.Users.Where(u => u is not Admin))
            {
                user.SetStatus(UserStatus.PENDING);
                user.TierLevel = 1;
                user.Manager = null;
                user.DirectReports?.Clear();
            }
            Logger.Info("All employees reset to PENDING status.");
            _dataStore.SaveData();
        }

        public void UpdateUserRole(User user)
        {
            if (user is Admin) return;
            if (user.DirectReports?.Count > 0 && user.TierLevel < 2) user.TierLevel = 2;
            _dataStore.SaveData();
        }

        public List<User> GetPeers(User user) => user.GetPeers();
        public List<User> GetTeamMembers(User manager) => _dataStore.GetTeamMembers(manager);
    }
}
