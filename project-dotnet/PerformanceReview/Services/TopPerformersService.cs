using PerformanceReview.Models;
using PerformanceReview.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerformanceReview.Services
{
    public class TopPerformersService
    {
        private static TopPerformersService? _instance;
        private const int DEFAULT_TOP_COUNT = 3;

        private TopPerformersService() { }

        public static TopPerformersService GetInstance()
        {
            if (_instance == null) _instance = new TopPerformersService();
            return _instance;
        }

        public class PerformerResult
        {
            public User User { get; set; }
            public double Score { get; set; }
            public int ReviewCount { get; set; }
            public int Rank { get; set; }

            public PerformerResult(User user, double score, int reviewCount, int rank)
            {
                User = user; Score = score; ReviewCount = reviewCount; Rank = rank;
            }
        }

        public class TeamPerformers
        {
            public string TeamName { get; set; }
            public User? Manager { get; set; }
            public List<PerformerResult> TopPerformersList { get; set; }
            public int TotalMembers { get; set; }

            public TeamPerformers(string teamName, User? manager, List<PerformerResult> topPerformers, int totalMembers)
            {
                TeamName = teamName; Manager = manager; TopPerformersList = topPerformers; TotalMembers = totalMembers;
            }
        }

        private List<PerformerResult> GetTopPerformers(List<User> users, int topCount)
        {
            var results = new List<PerformerResult>();
            foreach (var user in users)
            {
                var reviews = DataStore.GetInstance().GetReviewsForUser(user);
                double score = reviews.Count > 0 ? reviews.Average(r => r.CalculateAverageScore()) : 0;
                results.Add(new PerformerResult(user, score, reviews.Count, 0));
            }

            results.Sort((a, b) => b.Score.CompareTo(a.Score));

            var topResults = new List<PerformerResult>();
            for (int i = 0; i < Math.Min(topCount, results.Count); i++)
            {
                results[i].Rank = i + 1;
                topResults.Add(results[i]);
            }
            return topResults;
        }

        public TeamPerformers GetSystemTopPerformers(int topCount)
        {
            var allUsers = DataStore.GetInstance().Users
                .Where(u => u is not Admin && u.Status == UserStatus.ACTIVE).ToList();
            return new TeamPerformers("System-Wide Top Performers", null,
                GetTopPerformers(allUsers, topCount), allUsers.Count);
        }

        public TeamPerformers GetDirectTeamTopPerformers(User manager, int topCount)
        {
            var dr = manager.DirectReports;
            if (dr == null || dr.Count == 0)
                return new TeamPerformers($"Direct Reports of {manager.Name}", manager, new(), 0);
            return new TeamPerformers($"Direct Reports of {manager.Name}", manager,
                GetTopPerformers(dr, topCount), dr.Count);
        }

        public TeamPerformers GetHierarchyTopPerformers(User manager, int topCount)
        {
            var hierarchy = UserService.GetInstance().GetAllDescendants(manager);
            if (hierarchy.Count == 0)
                return new TeamPerformers($"Hierarchy of {manager.Name}", manager, new(), 0);
            return new TeamPerformers($"Full Hierarchy of {manager.Name}", manager,
                GetTopPerformers(hierarchy, topCount), hierarchy.Count);
        }

        public List<TeamPerformers> GetAdminTopPerformersAnalysis()
        {
            var results = new List<TeamPerformers> { GetSystemTopPerformers(DEFAULT_TOP_COUNT) };
            var managers = UserService.GetInstance().GetNonAdminUsers()
                .Where(u => u.Status == UserStatus.ACTIVE && (u.DirectReports?.Count > 0 || u.TierLevel == DataStore.MAX_TIERS))
                .OrderByDescending(u => u.TierLevel).ToList();

            foreach (var mgr in managers)
            {
                var tp = GetHierarchyTopPerformers(mgr, DEFAULT_TOP_COUNT);
                if (tp.TotalMembers > 0) results.Add(tp);
            }
            return results;
        }

        public List<TeamPerformers> GetManagerTopPerformersAnalysis(User manager)
        {
            var results = new List<TeamPerformers> { GetHierarchyTopPerformers(manager, DEFAULT_TOP_COUNT) };
            if (manager.DirectReports?.Count > 0)
                results.Add(GetDirectTeamTopPerformers(manager, DEFAULT_TOP_COUNT));

            var subManagers = UserService.GetInstance().GetAllDescendants(manager)
                .Where(u => u.DirectReports?.Count > 0).ToList();
            foreach (var sm in subManagers)
            {
                var tp = GetHierarchyTopPerformers(sm, DEFAULT_TOP_COUNT);
                if (tp.TotalMembers > 0) results.Add(tp);
            }
            return results;
        }
    }
}
