using PerformanceReview.Models;
using PerformanceReview.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace PerformanceReview.Services
{
    public class GoalService
    {
        private static GoalService? _instance;
        private readonly DataStore _dataStore;

        private GoalService() { _dataStore = DataStore.GetInstance(); }

        public static GoalService GetInstance()
        {
            if (_instance == null) _instance = new GoalService();
            return _instance;
        }

        public Goal? CreateGoal(User employee, string description, User creator)
        {
            bool isSelf = employee.Username == creator.Username;
            bool isAdmin = creator is Admin;
            bool isManager = false;

            if (!isSelf && !isAdmin)
            {
                var descendants = UserService.GetInstance().GetAllDescendants(creator);
                isManager = descendants.Any(u => u.Username == employee.Username);
            }

            if (!isSelf && !isAdmin && !isManager)
            {
                Logger.Warn($"Goal creation denied: {creator.Username} tried to assign goal to {employee.Username}");
                return null;
            }

            var goal = new Goal(employee, description, creator);
            _dataStore.AddGoal(goal);
            _dataStore.SaveData();
            Logger.Info($"Goal created for {employee.Username} by {creator.Username}");
            return goal;
        }

        public bool DeleteGoal(Goal goal, User requester)
        {
            if (goal.Creator == null)
            {
                if (goal.Employee?.Username == requester.Username)
                {
                    _dataStore.RemoveGoal(goal);
                    _dataStore.SaveData();
                    Logger.Info($"Goal deleted by {requester.Username}");
                    return true;
                }
            }
            if (goal.Creator?.Username == requester.Username)
            {
                _dataStore.RemoveGoal(goal);
                _dataStore.SaveData();
                Logger.Info($"Goal deleted by {requester.Username}");
                return true;
            }
            return false;
        }

        public void UpdateGoalStatus(Goal goal, string status)
        {
            goal.Status = status;
            _dataStore.SaveData();
            Logger.Info($"Goal status updated to '{status}' for {goal.Employee.Username}");
        }

        public void CompleteGoal(Goal goal)
        {
            goal.Status = "COMPLETED";
            _dataStore.SaveData();
            Logger.Info($"Goal completed for {goal.Employee.Username}");
        }

        public List<Goal> GetAllGoals() => _dataStore.Goals;

        public List<Goal> GetGoalsForUser(User user) =>
            _dataStore.Goals.Where(g => g.Employee?.Username == user.Username).ToList();

        public List<Goal> GetGoalsCreatedBy(User creator) =>
            _dataStore.Goals.Where(g => g.Creator?.Username == creator.Username).ToList();

        public List<Goal> GetTeamGoals(User manager)
        {
            var team = DataStore.GetInstance().GetAllDescendants(manager);
            var teamGoals = new List<Goal>();
            foreach (var emp in team) teamGoals.AddRange(GetGoalsForUser(emp));
            return teamGoals;
        }

        public void DeleteGoalAdmin(Goal goal)
        {
            _dataStore.RemoveGoal(goal);
            _dataStore.SaveData();
            Logger.Info("Goal deleted by Admin");
        }
    }
}
