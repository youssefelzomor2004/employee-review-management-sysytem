using System.Windows;
using PerformanceReview.Utilities;
using PerformanceReview.Models;

namespace PerformanceReview
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Logger.Info("=== Performance Review System (.NET) ===");
            Logger.Info("Initializing application...");

            // Initialize DataStore and load data from DB
            InitializeData();
        }

        private void InitializeData()
        {
            var ds = DataStore.GetInstance();
            var dbm = DatabaseManager.GetInstance();

            // Load users from DB
            var users = dbm.GetAllUsers();
            foreach (var u in users) ds.Users.Add(u);

            // Load courses from DB  
            var courses = dbm.GetAllCourses();
            foreach (var c in courses) ds.AvailableCourses.Add(c);

            // If no users exist, create default admin
            if (ds.Users.Count == 0)
            {
                var admin = new Admin("admin", "admin123", "Super Admin");
                ds.AddUser(admin);
                dbm.SaveUser(admin);

                // Add default courses
                string[] defaultCourses = { "Leadership Fundamentals", "Time Management", 
                    "Team Communication", "Project Management", "Conflict Resolution" };
                foreach (var c in defaultCourses) { ds.AddCourse(c); dbm.SaveCourse(c); }

                Logger.Info("Default admin and courses created.");
            }
        }
    }
}
