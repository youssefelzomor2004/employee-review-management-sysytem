using System.Collections.Generic;
using System.Linq;

namespace PerformanceReview.Models
{
    public abstract class User
    {
        public const int TIER_UNASSIGNED = 0;
        public const int TIER_EMPLOYEE = 1;

        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public UserStatus Status { get; set; }
        public UserRole Role { get; set; }
        public int TierLevel { get; set; }
        public int MaxDirectReports { get; set; } = 3;
        public User? Manager { get; set; }
        public List<User> DirectReports { get; set; } = new List<User>();

        protected User(string username, string password, string name, UserRole role)
        {
            Username = username;
            Password = password;
            Name = name;
            Role = role;
            Status = UserStatus.PENDING;
            TierLevel = TIER_UNASSIGNED;
        }

        public bool IsAssigned()
        {
            return TierLevel > TIER_UNASSIGNED && Status == UserStatus.ACTIVE;
        }

        public bool IsManager()
        {
            return DirectReports != null && DirectReports.Count > 0;
        }

        public List<User> GetPeers()
        {
            var peers = new List<User>();
            if (Manager?.DirectReports != null)
            {
                foreach (var peer in Manager.DirectReports)
                {
                    if (peer.Username != Username)
                        peers.Add(peer);
                }
            }
            return peers;
        }

        public bool IsPeer(User other)
        {
            if (other == null || Manager == null) return false;
            if (other.Username == Username) return false;
            if (other.Manager == null) return false;
            return Manager.Username == other.Manager.Username;
        }

        public virtual bool CanReview(User target)
        {
            if (target == null) return false;
            if (DirectReports != null && DirectReports.Contains(target)) return true;
            return IsPeer(target);
        }

        public bool CheckPassword(string inputPassword)
        {
            return Password == inputPassword;
        }

        public void SetStatus(UserStatus status)
        {
            Status = status;
            if (status == UserStatus.SUSPENDED || status == UserStatus.PENDING)
                TierLevel = TIER_UNASSIGNED;
        }

        public void AddDirectReport(User u)
        {
            if (DirectReports == null) DirectReports = new List<User>();
            if (!DirectReports.Contains(u)) DirectReports.Add(u);
        }
    }
}
