using PerformanceReview.Models;
using System.Collections.Generic;
using System.Linq;

namespace PerformanceReview.Utilities
{
    public class DataStore
    {
        private static DataStore? _instance;
        public static int MAX_TIERS = 3;

        public List<User> Users { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public List<TrainingRecommendation> TrainingRecommendations { get; set; } = new();
        public List<UpwardReview> UpwardReviews { get; set; } = new();
        public List<Goal> Goals { get; set; } = new();
        public List<string> AvailableCourses { get; set; } = new();
        public List<PendingAction> PendingActions { get; set; } = new();
        public List<PeerReview> PeerReviews { get; set; } = new();

        private DataStore() { }

        public static DataStore GetInstance()
        {
            if (_instance == null) _instance = new DataStore();
            return _instance;
        }

        public void SaveData()
        {
            // In .NET version, persistence is handled through DatabaseManager
            // This method exists for API compatibility
        }

        public User? Authenticate(string username, string password)
        {
            return Users.FirstOrDefault(u => u.Username == username && u.CheckPassword(password));
        }

        public void AddUser(User user) { Users.Add(user); SaveData(); }
        public void RemoveUser(User user) { Users.Remove(user); SaveData(); }

        public void AddReview(Review review) { Reviews.Add(review); SaveData(); }
        public void RemoveReview(Review review) { Reviews.Remove(review); SaveData(); }

        public List<Review> GetReviewsForUser(User e)
        {
            return Reviews.Where(r => r.Employee?.Username == e.Username).ToList();
        }

        public void AddTrainingRecommendation(TrainingRecommendation tr)
        {
            TrainingRecommendations.Add(tr); SaveData();
        }

        public void RemoveTrainingRecommendation(TrainingRecommendation tr)
        {
            TrainingRecommendations.Remove(tr); SaveData();
        }

        public void AddUpwardReview(UpwardReview ur) { UpwardReviews.Add(ur); SaveData(); }

        public void AddGoal(Goal goal) { Goals.Add(goal); SaveData(); }
        public void RemoveGoal(Goal goal) { Goals.Remove(goal); SaveData(); }

        public void AddCourse(string course)
        {
            if (!AvailableCourses.Contains(course)) { AvailableCourses.Add(course); SaveData(); }
        }

        public void RemoveCourse(string course) { AvailableCourses.Remove(course); SaveData(); }

        public void AddPendingAction(PendingAction action) { PendingActions.Add(action); SaveData(); }

        public void AddPeerReview(PeerReview review) { PeerReviews.Add(review); SaveData(); }

        public List<PeerReview> GetPeerReviewsFor(User user)
        {
            return PeerReviews.Where(pr => pr.IsAbout(user)).ToList();
        }

        public List<User> GetAllDescendants(User manager)
        {
            return GetAllDescendants(manager, new HashSet<User>());
        }

        private List<User> GetAllDescendants(User manager, HashSet<User> visited)
        {
            var descendants = new List<User>();
            visited.Add(manager);
            if (manager.DirectReports != null)
            {
                foreach (var e in manager.DirectReports)
                {
                    if (!visited.Contains(e))
                    {
                        descendants.Add(e);
                        descendants.AddRange(GetAllDescendants(e, visited));
                    }
                }
            }
            return descendants;
        }

        public List<User> GetChainOfCommand(User user)
        {
            var chain = new List<User>();
            var visited = new HashSet<User>();
            var current = user.Manager;
            while (current != null)
            {
                if (visited.Contains(current) || current == user) break;
                visited.Add(current);
                chain.Add(current);
                current = current.Manager;
            }
            return chain;
        }

        public List<User> GetTeamMembers(User user)
        {
            var team = new List<User>();
            if (user.DirectReports != null) team.AddRange(user.DirectReports);

            var manager = user.Manager;
            int tier = user.TierLevel;

            foreach (var u in Users)
            {
                if (u == user) continue;
                if (u is Admin) continue;
                if (u.TierLevel == tier)
                {
                    var uManager = u.Manager;
                    if ((manager == null && uManager == null) ||
                        (manager != null && manager.Equals(uManager)))
                    {
                        team.Add(u);
                    }
                }
            }
            return team;
        }
    }
}
