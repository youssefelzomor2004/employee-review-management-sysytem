using PerformanceReview.Models;
using PerformanceReview.Services;
using PerformanceReview.Utilities;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PerformanceReview.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            CheckFirstRun();
        }

        private void CheckFirstRun()
        {
            var ds = DataStore.GetInstance();
            if (ds.Users.Count == 0)
            {
                var admin = new Admin("admin", "admin123", "Super Admin");
                ds.AddUser(admin);
                DatabaseManager.GetInstance().SaveUser(admin);

                string[] defaultCourses = { "Leadership Fundamentals", "Time Management",
                    "Team Communication", "Project Management", "Conflict Resolution" };
                foreach (var c in defaultCourses) { ds.AddCourse(c); DatabaseManager.GetInstance().SaveCourse(c); }

                MessageBox.Show("Username: admin\nPassword: admin123", "Default Admin Created",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e) => HandleLogin();

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) HandleLogin();
        }

        private void HandleLogin()
        {
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter both username and password");
                return;
            }

            var user = UserService.GetInstance().Authenticate(username, password);
            if (user == null) { ShowError("Invalid username or password"); return; }
            if (user.Status == UserStatus.PENDING) { ShowError("Account pending approval"); return; }
            if (user.Status == UserStatus.SUSPENDED) { ShowError("Account suspended"); return; }

            Logger.Info($"Login successful: {username}");
            NavigateToDashboard(user);
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.Visibility = Visibility.Visible;
        }

        private void NavigateToDashboard(User user)
        {
            Window dashboard;
            if (user is Admin)
            {
                dashboard = new AdminDashboard();
            }
            else
            {
                int tier = user.TierLevel;
                bool hasReports = user.DirectReports?.Count > 0;

                if (tier == UserService.GetInstance().GetMaxTiers())
                    dashboard = new ManagerDashboard(user, true);
                else if (tier == 1 && !hasReports)
                    dashboard = new EmployeeDashboard(user);
                else
                    dashboard = new ManagerDashboard(user, false);
            }

            dashboard.Show();
            Close();
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            var regWindow = new RegisterWindow();
            regWindow.Owner = this;
            regWindow.ShowDialog();
        }
    }
}
