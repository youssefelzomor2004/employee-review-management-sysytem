using PerformanceReview.Models;
using PerformanceReview.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace PerformanceReview.Services
{
    public class TrainingService
    {
        private static TrainingService? _instance;
        private readonly DataStore _dataStore;

        private TrainingService() { _dataStore = DataStore.GetInstance(); }

        public static TrainingService GetInstance()
        {
            if (_instance == null) _instance = new TrainingService();
            return _instance;
        }

        public List<string> GetAvailableCourses() => _dataStore.AvailableCourses;

        public bool AddCourse(string courseName)
        {
            if (string.IsNullOrWhiteSpace(courseName)) return false;
            string trimmed = courseName.Trim();
            if (_dataStore.AvailableCourses.Contains(trimmed))
            {
                Logger.Warn($"Course addition failed: '{courseName}' already exists.");
                return false;
            }
            _dataStore.AddCourse(trimmed);
            _dataStore.SaveData();
            Logger.Info($"Course added: {courseName}");
            return true;
        }

        public void RemoveCourse(string courseName)
        {
            _dataStore.RemoveCourse(courseName);
            _dataStore.SaveData();
            Logger.Info($"Course removed: {courseName}");
        }

        public TrainingRecommendation? CreateRecommendation(User recommender, User recipient, string courseName)
        {
            bool isAdmin = recommender is Admin;
            bool isManager = false;

            if (!isAdmin)
            {
                var descendants = UserService.GetInstance().GetAllDescendants(recommender);
                isManager = descendants.Any(u => u.Username == recipient.Username);
            }

            if (!isAdmin && !isManager)
            {
                Logger.Warn($"Training recommendation denied: {recommender.Username} tried to assign to {recipient.Username}");
                return null;
            }

            var tr = new TrainingRecommendation(recommender, recipient, courseName);
            _dataStore.AddTrainingRecommendation(tr);
            _dataStore.SaveData();
            Logger.Info($"Training recommended: '{courseName}' for {recipient.Username} by {recommender.Username}");
            return tr;
        }

        public bool DeleteRecommendation(TrainingRecommendation recommendation, User requester)
        {
            if (recommendation.Recommender?.Username == requester.Username)
            {
                _dataStore.RemoveTrainingRecommendation(recommendation);
                _dataStore.SaveData();
                Logger.Info($"Training recommendation deleted by {requester.Username}");
                return true;
            }
            return false;
        }

        public List<TrainingRecommendation> GetAllRecommendations() => _dataStore.TrainingRecommendations;

        public List<TrainingRecommendation> GetRecommendationsFor(User recipient) =>
            _dataStore.TrainingRecommendations.Where(tr => tr.Recipient?.Username == recipient.Username).ToList();

        public List<TrainingRecommendation> GetRecommendationsBy(User recommender) =>
            _dataStore.TrainingRecommendations.Where(tr => tr.Recommender?.Username == recommender.Username).ToList();
    }
}
