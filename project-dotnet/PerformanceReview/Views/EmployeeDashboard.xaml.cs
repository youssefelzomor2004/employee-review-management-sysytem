using PerformanceReview.Models;
using PerformanceReview.Services;
using PerformanceReview.Utilities;
using System;
using System.Linq;
using System.Windows;

namespace PerformanceReview.Views
{
    public partial class EmployeeDashboard : Window
    {
        private readonly User _currentUser;

        public EmployeeDashboard(User user)
        {
            InitializeComponent();
            _currentUser = user;
            WelcomeLabel.Text = $"Welcome, {user.Name}";
            RoleLabel.Text = $"Tier {user.TierLevel} • {UserService.GetInstance().CalculateRole(user)}";
            LoadData();
        }

        private void LoadData()
        {
            MyReviewsGrid.ItemsSource = ReviewService.GetInstance().GetReviewsForUser(_currentUser);
            GoalsGrid.ItemsSource = GoalService.GetInstance().GetGoalsForUser(_currentUser);
            TrainingGrid.ItemsSource = TrainingService.GetInstance().GetRecommendationsFor(_currentUser);
            PeerReviewsGrid.ItemsSource = ReviewService.GetInstance().GetPeerReviewsGivenBy(_currentUser);
            PeerCombo.ItemsSource = _currentUser.GetPeers();
            UpdateScoreDisplay();
            UpdateUpwardReview();
        }

        private void UpdateScoreDisplay()
        {
            var score = PerformanceCalculator.CalculateScore(_currentUser);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"═══ PERFORMANCE SCORE: {_currentUser.Name} ═══\n");
            sb.AppendLine($"  Punctuality:  {score.Punctuality:F1} / 10");
            sb.AppendLine($"  Teamwork:     {score.Teamwork:F1} / 10");
            sb.AppendLine($"  Productivity: {score.Productivity:F1} / 10");
            sb.AppendLine($"  ─────────────────");
            sb.AppendLine($"  OVERALL:      {score.Overall:F1} / 10\n");
            sb.AppendLine($"Calculation:\n{score.Breakdown}");
            if (score.MissingCategories.Count > 0)
                sb.AppendLine($"\nMissing: {string.Join(", ", score.MissingCategories)}");
            ScoreDisplay.Text = sb.ToString();
        }

        private void UpdateUpwardReview()
        {
            if (_currentUser.Manager != null)
                UpwardManagerLabel.Text = $"Review Manager: {_currentUser.Manager.Name}";
            else
                UpwardManagerLabel.Text = "No manager assigned";
        }

        private void ExportReport_Click(object sender, RoutedEventArgs e) =>
            ReportExporter.ExportUserReport(_currentUser, this);

        private void AddGoal_Click(object sender, RoutedEventArgs e)
        {
            string desc = NewGoalBox.Text.Trim();
            if (string.IsNullOrEmpty(desc)) return;
            GoalService.GetInstance().CreateGoal(_currentUser, desc, _currentUser);
            DatabaseManager.GetInstance().SaveGoal(new Goal(_currentUser, desc, _currentUser));
            NewGoalBox.Clear();
            GoalsGrid.ItemsSource = GoalService.GetInstance().GetGoalsForUser(_currentUser);
        }

        private void CompleteGoal_Click(object sender, RoutedEventArgs e)
        {
            if (GoalsGrid.SelectedItem is Goal goal && goal.IsActive())
            {
                GoalService.GetInstance().CompleteGoal(goal);
                GoalsGrid.ItemsSource = null;
                GoalsGrid.ItemsSource = GoalService.GetInstance().GetGoalsForUser(_currentUser);
            }
        }

        private void SubmitPeerReview_Click(object sender, RoutedEventArgs e)
        {
            if (PeerCombo.SelectedItem is not User peer) return;
            if (!int.TryParse(PeerCollabBox.Text, out int collab) ||
                !int.TryParse(PeerCommBox.Text, out int comm) ||
                !int.TryParse(PeerTeamBox.Text, out int team))
            {
                MessageBox.Show("Enter valid scores (1-5).", "Error"); return;
            }
            var pr = ReviewService.GetInstance().CreatePeerReview(_currentUser, peer, collab, comm, team, null);
            if (pr == null) MessageBox.Show("Cannot submit peer review. Invalid or duplicate.", "Error");
            else
            {
                PeerReviewsGrid.ItemsSource = ReviewService.GetInstance().GetPeerReviewsGivenBy(_currentUser);
                PeerCollabBox.Clear(); PeerCommBox.Clear(); PeerTeamBox.Clear();
                MessageBox.Show("Peer review submitted!", "Success");
            }
        }

        private void SubmitUpwardReview_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Manager == null) { MessageBox.Show("No manager assigned.", "Error"); return; }
            if (!int.TryParse(UpPunctBox.Text, out int p) ||
                !int.TryParse(UpTeamBox.Text, out int t) ||
                !int.TryParse(UpProdBox.Text, out int pr))
            {
                MessageBox.Show("Enter valid scores (1-5).", "Error"); return;
            }
            var ur = ReviewService.GetInstance().CreateUpwardReview(_currentUser, _currentUser.Manager, p, t, pr, UpCommentsBox.Text);
            if (ur == null) MessageBox.Show("Duplicate upward review.", "Error");
            else
            {
                UpPunctBox.Clear(); UpTeamBox.Clear(); UpProdBox.Clear(); UpCommentsBox.Clear();
                MessageBox.Show("Upward review submitted!", "Success");
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }
    }
}
