using PerformanceReview.Services;
using System.Linq;
using System.Windows;

namespace PerformanceReview.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow() => InitializeComponent();

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();
            string username = RegUsernameBox.Text.Trim();
            string password = RegPasswordBox.Password;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("All fields required"); return;
            }

            if (!IsValidPassword(password))
            {
                ShowError("Password doesn't meet requirements"); return;
            }

            if (UserService.GetInstance().FindByUsername(username) != null)
            {
                ShowError("Username already taken"); return;
            }

            UserService.GetInstance().RegisterUser(username, password, name);
            MessageBox.Show("Your account is pending. Admin will assign you to the hierarchy.",
                "Registration Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void ShowError(string msg)
        {
            RegErrorLabel.Text = msg;
            RegErrorLabel.Visibility = Visibility.Visible;
        }

        private bool IsValidPassword(string password)
        {
            if (password.Length < 8) return false;
            return password.Any(char.IsUpper) && password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) && password.Any(c => !char.IsLetterOrDigit(c));
        }
    }
}
