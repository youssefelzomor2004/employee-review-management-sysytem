using Microsoft.Data.Sqlite;
using PerformanceReview.Models;
using System;
using System.Collections.Generic;

namespace PerformanceReview.Utilities
{
    public class DatabaseManager
    {
        private static DatabaseManager? _instance;
        private SqliteConnection _connection;
        private const string DB_URL = "Data Source=performance.db";

        private DatabaseManager()
        {
            _connection = new SqliteConnection(DB_URL);
            _connection.Open();
            CreateTables();
        }

        public static DatabaseManager GetInstance()
        {
            if (_instance == null) _instance = new DatabaseManager();
            return _instance;
        }

        private void CreateTables()
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE NOT NULL,
                    password TEXT NOT NULL,
                    name TEXT NOT NULL,
                    role TEXT DEFAULT 'EMPLOYEE',
                    status TEXT DEFAULT 'PENDING',
                    tier_level INTEGER DEFAULT 1,
                    manager_id INTEGER,
                    FOREIGN KEY (manager_id) REFERENCES users(id)
                );
                CREATE TABLE IF NOT EXISTS reviews (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    employee_id INTEGER NOT NULL,
                    reviewer_id INTEGER,
                    punctuality INTEGER DEFAULT 0,
                    teamwork INTEGER DEFAULT 0,
                    productivity INTEGER DEFAULT 0,
                    leadership INTEGER DEFAULT 0,
                    vision INTEGER DEFAULT 0,
                    managerial_skills INTEGER DEFAULT 0,
                    comments TEXT,
                    status TEXT DEFAULT 'SUBMITTED',
                    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (employee_id) REFERENCES users(id),
                    FOREIGN KEY (reviewer_id) REFERENCES users(id)
                );
                CREATE TABLE IF NOT EXISTS goals (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    employee_id INTEGER NOT NULL,
                    creator_id INTEGER,
                    description TEXT NOT NULL,
                    status TEXT DEFAULT 'ACTIVE',
                    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (employee_id) REFERENCES users(id),
                    FOREIGN KEY (creator_id) REFERENCES users(id)
                );
                CREATE TABLE IF NOT EXISTS training_recommendations (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    recipient_id INTEGER NOT NULL,
                    recommender_id INTEGER,
                    course_name TEXT NOT NULL,
                    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (recipient_id) REFERENCES users(id),
                    FOREIGN KEY (recommender_id) REFERENCES users(id)
                );
                CREATE TABLE IF NOT EXISTS courses (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT UNIQUE NOT NULL
                );";
            cmd.ExecuteNonQuery();
        }

        public void SaveUser(User user)
        {
            try
            {
                using var check = _connection.CreateCommand();
                check.CommandText = "SELECT id FROM users WHERE username = @username";
                check.Parameters.AddWithValue("@username", user.Username);
                var result = check.ExecuteScalar();

                if (result != null)
                {
                    using var update = _connection.CreateCommand();
                    update.CommandText = @"UPDATE users SET password=@p, name=@n, role=@r, status=@s, 
                        tier_level=@t, manager_id=@m WHERE id=@id";
                    update.Parameters.AddWithValue("@p", user.Password);
                    update.Parameters.AddWithValue("@n", user.Name);
                    update.Parameters.AddWithValue("@r", user.Role.ToString());
                    update.Parameters.AddWithValue("@s", user.Status.ToString());
                    update.Parameters.AddWithValue("@t", user.TierLevel);
                    update.Parameters.AddWithValue("@m", user.Manager != null ? (object)GetUserId(user.Manager.Username) : DBNull.Value);
                    update.Parameters.AddWithValue("@id", (long)result);
                    update.ExecuteNonQuery();
                }
                else
                {
                    using var insert = _connection.CreateCommand();
                    insert.CommandText = @"INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
                        VALUES (@u, @p, @n, @r, @s, @t, @m)";
                    insert.Parameters.AddWithValue("@u", user.Username);
                    insert.Parameters.AddWithValue("@p", user.Password);
                    insert.Parameters.AddWithValue("@n", user.Name);
                    insert.Parameters.AddWithValue("@r", user.Role.ToString());
                    insert.Parameters.AddWithValue("@s", user.Status.ToString());
                    insert.Parameters.AddWithValue("@t", user.TierLevel);
                    insert.Parameters.AddWithValue("@m", user.Manager != null ? (object)GetUserId(user.Manager.Username) : DBNull.Value);
                    insert.ExecuteNonQuery();
                }
            }
            catch (Exception e) { Console.Error.WriteLine(e.Message); }
        }

        public int GetUserId(string username)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT id FROM users WHERE username = @u";
            cmd.Parameters.AddWithValue("@u", username);
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            var userMap = new Dictionary<int, User>();
            var managerMap = new Dictionary<int, int>();

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM users";
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string username = reader.GetString(1);
                string password = reader.GetString(2);
                string name = reader.GetString(3);
                string role = reader.GetString(4);
                string status = reader.GetString(5);
                int tierLevel = reader.GetInt32(6);
                int managerId = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);

                User user;
                if (role == "ADMIN")
                    user = new Admin(username, password, name);
                else
                {
                    user = new Employee(username, password, name);
                    user.Role = Enum.Parse<UserRole>(role);
                }
                user.Status = Enum.Parse<UserStatus>(status);
                user.TierLevel = tierLevel;

                userMap[id] = user;
                if (managerId > 0) managerMap[id] = managerId;
                users.Add(user);
            }

            foreach (var entry in managerMap)
            {
                if (userMap.TryGetValue(entry.Key, out var user) && userMap.TryGetValue(entry.Value, out var manager))
                {
                    user.Manager = manager;
                    manager.AddDirectReport(user);
                }
            }

            return users;
        }

        public void DeleteUser(string username)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "DELETE FROM users WHERE username = @u";
            cmd.Parameters.AddWithValue("@u", username);
            cmd.ExecuteNonQuery();
        }

        public void SaveReview(Review review)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity,
                leadership, vision, managerial_skills, comments, status) VALUES (@e, @r, @p, @t, @pr, @l, @v, @ms, @c, @s)";
            cmd.Parameters.AddWithValue("@e", GetUserId(review.Employee.Username));
            cmd.Parameters.AddWithValue("@r", review.Reviewer != null ? (object)GetUserId(review.Reviewer.Username) : 0);
            cmd.Parameters.AddWithValue("@p", review.PunctualityScore);
            cmd.Parameters.AddWithValue("@t", review.TeamworkScore);
            cmd.Parameters.AddWithValue("@pr", review.ProductivityScore);
            cmd.Parameters.AddWithValue("@l", review.LeadershipScore);
            cmd.Parameters.AddWithValue("@v", review.VisionScore);
            cmd.Parameters.AddWithValue("@ms", review.ManagerialSkillsScore);
            cmd.Parameters.AddWithValue("@c", (object?)review.Comments ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@s", review.Status);
            cmd.ExecuteNonQuery();
        }

        public void SaveGoal(Goal goal)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO goals (employee_id, creator_id, description, status) VALUES (@e, @c, @d, @s)";
            cmd.Parameters.AddWithValue("@e", GetUserId(goal.Employee.Username));
            cmd.Parameters.AddWithValue("@c", goal.Creator != null ? (object)GetUserId(goal.Creator.Username) : 0);
            cmd.Parameters.AddWithValue("@d", goal.Description);
            cmd.Parameters.AddWithValue("@s", goal.Status);
            cmd.ExecuteNonQuery();
        }

        public void SaveCourse(string courseName)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO courses (name) VALUES (@n)";
            cmd.Parameters.AddWithValue("@n", courseName);
            cmd.ExecuteNonQuery();
        }

        public List<string> GetAllCourses()
        {
            var courses = new List<string>();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT name FROM courses";
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) courses.Add(reader.GetString(0));
            return courses;
        }

        public void SaveTrainingRecommendation(TrainingRecommendation tr)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO training_recommendations (recipient_id, recommender_id, course_name) VALUES (@r, @rc, @c)";
            cmd.Parameters.AddWithValue("@r", GetUserId(tr.Recipient.Username));
            cmd.Parameters.AddWithValue("@rc", tr.Recommender != null ? (object)GetUserId(tr.Recommender.Username) : 0);
            cmd.Parameters.AddWithValue("@c", tr.CourseName);
            cmd.ExecuteNonQuery();
        }

        public void Close()
        {
            _connection?.Close();
        }
    }
}
