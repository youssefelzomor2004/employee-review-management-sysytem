using PerformanceReview.Models;
using PerformanceReview.Services;
using PerformanceReview.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PerformanceReview.Views
{
    public partial class AdminDashboard : Window
    {
        public AdminDashboard()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            RefreshUsers();
            ReviewsGrid.ItemsSource = ReviewService.GetInstance().GetAllReviews();
            ApprovalsGrid.ItemsSource = ApprovalService.GetInstance().GetAllRequests();
            CoursesListBox.ItemsSource = null;
            CoursesListBox.ItemsSource = TrainingService.GetInstance().GetAvailableCourses();
            LoadHierarchyComboBoxes();
        }

        private void RefreshUsers()
        {
            UsersGrid.ItemsSource = null;
            UsersGrid.ItemsSource = UserService.GetInstance().GetAllUsers();
        }

        private void LoadHierarchyComboBoxes()
        {
            var nonAdmins = UserService.GetInstance().GetNonAdminUsers();
            HierarchyEmployeeCombo.ItemsSource = nonAdmins.Select(u => new { u.Username, DisplayName = $"{u.Name} ({u.Username})", User = u }).ToList();
            HierarchyManagerCombo.ItemsSource = nonAdmins.Select(u => new { u.Username, DisplayName = $"{u.Name} ({u.Username})", User = u }).ToList();

            HierarchyTierCombo.Items.Clear();
            for (int i = 1; i <= DataStore.MAX_TIERS; i++) HierarchyTierCombo.Items.Add(i);
            if (HierarchyTierCombo.Items.Count > 0) HierarchyTierCombo.SelectedIndex = 0;

            UpdateHierarchyDisplay();
        }

        private void UpdateHierarchyDisplay()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== ORGANIZATIONAL HIERARCHY ===\n");
            var users = UserService.GetInstance().GetNonAdminUsers();
            var topLevel = users.Where(u => u.Manager == null && u.Status == UserStatus.ACTIVE).ToList();

            foreach (var u in topLevel) AppendHierarchy(sb, u, 0);

            sb.AppendLine("\n=== UNASSIGNED USERS ===");
            foreach (var u in users.Where(u => u.TierLevel == 0 || u.Status == UserStatus.PENDING))
                sb.AppendLine($"  {u.Name} ({u.Username}) [{u.Status}]");

            HierarchyDisplay.Text = sb.ToString();
        }

        private void AppendHierarchy(System.Text.StringBuilder sb, User user, int depth)
        {
            string indent = new string(' ', depth * 4);
            string role = UserService.GetInstance().CalculateRole(user);
            sb.AppendLine($"{indent}├─ {user.Name} ({user.Username}) [Tier {user.TierLevel}] - {role} [{user.Status}]");
            if (user.DirectReports != null)
                foreach (var dr in user.DirectReports) AppendHierarchy(sb, dr, depth + 1);
        }

        // Event handlers
        private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void ApproveUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is User user && user.Status == UserStatus.PENDING)
            {
                UserService.GetInstance().ApproveUser(user);
                RefreshUsers();
                MessageBox.Show($"User '{user.Username}' approved.", "Success");
            }
        }

        private void ToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is User user)
            {
                if (!UserService.GetInstance().ToggleUserStatus(user))
                    MessageBox.Show("Cannot toggle status. User may have direct reports.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                RefreshUsers();
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is User user)
            {
                if (MessageBox.Show($"Delete user '{user.Username}'?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    UserService.GetInstance().DeleteUser(user);
                    DatabaseManager.GetInstance().DeleteUser(user.Username);
                    RefreshUsers();
                }
            }
        }

        private void RefreshUsers_Click(object sender, RoutedEventArgs e) => LoadData();

        private void AssignHierarchy_Click(object sender, RoutedEventArgs e)
        {
            if (HierarchyEmployeeCombo.SelectedItem == null || HierarchyTierCombo.SelectedItem == null)
            {
                MessageBox.Show("Select employee and tier.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic empItem = HierarchyEmployeeCombo.SelectedItem;
            dynamic? mgrItem = HierarchyManagerCombo.SelectedItem;
            int tier = (int)HierarchyTierCombo.SelectedItem;

            User employee = empItem.User;
            User? manager = mgrItem?.User;

            var validation = UserService.GetInstance().ValidateHierarchyAssignment(employee, manager, tier);
            if (validation != null)
            {
                MessageBox.Show(validation, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UserService.GetInstance().AssignHierarchy(employee, manager, tier);
            DatabaseManager.GetInstance().SaveUser(employee);
            LoadData();
            MessageBox.Show($"Assigned {employee.Name} to Tier {tier}.", "Success");
        }

        private void SummaryReport_Click(object sender, RoutedEventArgs e) =>
            ReportDisplay.Text = ReportGenerator.GenerateSummaryReport();

        private void TrainingReport_Click(object sender, RoutedEventArgs e) =>
            ReportDisplay.Text = ReportGenerator.GenerateTrainingReport();

        private void ExportAll_Click(object sender, RoutedEventArgs e) =>
            ReportExporter.ExportAllUsersReport(this);

        private void ApproveAction_Click(object sender, RoutedEventArgs e)
        {
            if (ApprovalsGrid.SelectedItem is PendingAction action)
            {
                var admin = DataStore.GetInstance().Users.FirstOrDefault(u => u is Admin);
                if (admin != null && ApprovalService.GetInstance().ApproveRequest(action, admin))
                {
                    LoadData();
                    MessageBox.Show("Request approved.", "Success");
                }
            }
        }

        private void RejectAction_Click(object sender, RoutedEventArgs e)
        {
            if (ApprovalsGrid.SelectedItem is PendingAction action)
            {
                var admin = DataStore.GetInstance().Users.FirstOrDefault(u => u is Admin);
                if (admin != null && ApprovalService.GetInstance().RejectRequest(action, admin, "Rejected by admin"))
                {
                    LoadData();
                    MessageBox.Show("Request rejected.", "Success");
                }
            }
        }

        private void AddCourse_Click(object sender, RoutedEventArgs e)
        {
            string name = NewCourseBox.Text.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                if (TrainingService.GetInstance().AddCourse(name))
                {
                    DatabaseManager.GetInstance().SaveCourse(name);
                    NewCourseBox.Clear();
                    CoursesListBox.ItemsSource = null;
                    CoursesListBox.ItemsSource = TrainingService.GetInstance().GetAvailableCourses();
                }
                else MessageBox.Show("Course already exists.", "Warning");
            }
        }

        private void RemoveCourse_Click(object sender, RoutedEventArgs e)
        {
            if (CoursesListBox.SelectedItem is string course)
            {
                TrainingService.GetInstance().RemoveCourse(course);
                CoursesListBox.ItemsSource = null;
                CoursesListBox.ItemsSource = TrainingService.GetInstance().GetAvailableCourses();
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }
    }
}
