using PerformanceReview.Models;
using PerformanceReview.Services;
using PerformanceReview.Utilities;
using System;
using System.Linq;
using System.Windows;

namespace PerformanceReview.Views
{
    public partial class ManagerDashboard : Window
    {
        private readonly User _currentUser;
        private readonly bool _isHighboard;

        public ManagerDashboard(User user, bool isHighboard)
        {
            InitializeComponent();
            _currentUser = user;
            _isHighboard = isHighboard;
            WelcomeLabel.Text = $"Welcome, {user.Name}";
            RoleLabel.Text = $"Tier {user.TierLevel} • {(_isHighboard ? "Highboard Manager" : UserService.GetInstance().CalculateRole(user))}";
            LoadData();
        }

        private void LoadData()
        {
            var descendants = UserService.GetInstance().GetAllDescendants(_currentUser);
            TeamGrid.ItemsSource = descendants;
            ReviewEmployeeCombo.ItemsSource = descendants;
            GoalEmployeeCombo.ItemsSource = descendants;
            TrainEmployeeCombo.ItemsSource = descendants;
            TrainCourseCombo.ItemsSource = TrainingService.GetInstance().GetAvailableCourses();
            TeamGoalsGrid.ItemsSource = GoalService.GetInstance().GetTeamGoals(_currentUser);

            var recs = TrainingService.GetInstance().GetRecommendationsBy(_currentUser);
            TrainingRecsGrid.ItemsSource = recs;

            UpdateMyScore();
        }

        private void UpdateMyScore()
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
            MyScoreDisplay.Text = sb.ToString();
        }

        private void SubmitReview_Click(object sender, RoutedEventArgs e)
        {
            if (ReviewEmployeeCombo.SelectedItem is not User employee) 
            { MessageBox.Show("Select an employee.", "Error"); return; }

            if (!int.TryParse(RevPunctBox.Text, out int p) ||
                !int.TryParse(RevTeamBox.Text, out int t) ||
                !int.TryParse(RevProdBox.Text, out int pr))
            { MessageBox.Show("Enter valid scores (1-10).", "Error"); return; }

            int.TryParse(RevLeaderBox.Text, out int l);
            int.TryParse(RevVisionBox.Text, out int v);
            int.TryParse(RevMgmtBox.Text, out int m);

            var review = ReviewService.GetInstance().CreateReview(
                employee, _currentUser, p, t, pr, l, v, m, RevCommentsBox.Text, false);

            if (review == null) MessageBox.Show("Duplicate review (within 30 days).", "Error");
            else
            {
                DatabaseManager.GetInstance().SaveReview(review);
                RevPunctBox.Clear(); RevTeamBox.Clear(); RevProdBox.Clear();
                RevLeaderBox.Clear(); RevVisionBox.Clear(); RevMgmtBox.Clear();
                RevCommentsBox.Clear();
                MessageBox.Show("Review submitted!", "Success");
            }
        }

        private void AssignTeamGoal_Click(object sender, RoutedEventArgs e)
        {
            if (GoalEmployeeCombo.SelectedItem is not User employee) return;
            string desc = TeamGoalBox.Text.Trim();
            if (string.IsNullOrEmpty(desc)) return;

            var goal = GoalService.GetInstance().CreateGoal(employee, desc, _currentUser);
            if (goal != null)
            {
                DatabaseManager.GetInstance().SaveGoal(goal);
                TeamGoalBox.Clear();
                TeamGoalsGrid.ItemsSource = GoalService.GetInstance().GetTeamGoals(_currentUser);
                MessageBox.Show("Goal assigned!", "Success");
            }
        }

        private void RecommendTraining_Click(object sender, RoutedEventArgs e)
        {
            if (TrainEmployeeCombo.SelectedItem is not User employee) return;
            if (TrainCourseCombo.SelectedItem is not string course) return;

            var tr = TrainingService.GetInstance().CreateRecommendation(_currentUser, employee, course);
            if (tr != null)
            {
                DatabaseManager.GetInstance().SaveTrainingRecommendation(tr);
                TrainingRecsGrid.ItemsSource = TrainingService.GetInstance().GetRecommendationsBy(_currentUser);
                MessageBox.Show("Training recommended!", "Success");
            }
            else MessageBox.Show("Cannot recommend training.", "Error");
        }

        private void TopPerformers_Click(object sender, RoutedEventArgs e) =>
            ReportDisplay.Text = new ReportGenerator().GenerateTopPerformersReport(_currentUser);

        private void SkillGap_Click(object sender, RoutedEventArgs e) =>
            ReportDisplay.Text = new ReportGenerator().GenerateSkillGapAnalysis(_currentUser);

        private void TeamReport_Click(object sender, RoutedEventArgs e) =>
            ReportDisplay.Text = ReportGenerator.GenerateTeamReport(_currentUser);

        private void ExportTeamReport_Click(object sender, RoutedEventArgs e) =>
            ReportExporter.ExportTeamReport(_currentUser, this);

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }
    }
}
