package com.performance.util;

import com.performance.model.*;
import java.sql.*;
import java.util.*;

/**
 * Database Manager for MySQL database operations.
 */
public class DatabaseManager {
    // MySQL connection URL - update host, port, and database name as needed
    private static final String DB_URL = "jdbc:mysql://localhost:3306/performance";
    private static final String DB_USER = "root"; // Update with your MySQL username
    private static final String DB_PASSWORD = ""; // Update with your MySQL password

    private static DatabaseManager instance;
    private Connection connection;

    private DatabaseManager() {
        try {
            Class.forName("com.mysql.cj.jdbc.Driver");
            connection = DriverManager.getConnection(DB_URL, DB_USER, DB_PASSWORD);
            createTables();
        } catch (Exception e) {
            System.err.println("Database connection failed: " + e.getMessage());
            e.printStackTrace();
        }
    }

    public static synchronized DatabaseManager getInstance() {
        if (instance == null) {
            instance = new DatabaseManager();
        }
        return instance;
    }

    private void createTables() throws SQLException {
        Statement stmt = connection.createStatement();

        // Users table
        stmt.execute("""
                    CREATE TABLE IF NOT EXISTS users (
                        id INT PRIMARY KEY AUTO_INCREMENT,
                        username VARCHAR(100) UNIQUE NOT NULL,
                        password VARCHAR(255) NOT NULL,
                        name VARCHAR(255) NOT NULL,
                        role VARCHAR(50) DEFAULT 'EMPLOYEE',
                        status VARCHAR(50) DEFAULT 'PENDING',
                        tier_level INT DEFAULT 1,
                        manager_id INT,
                        FOREIGN KEY (manager_id) REFERENCES users(id)
                    )
                """);

        // Reviews table
        stmt.execute("""
                    CREATE TABLE IF NOT EXISTS reviews (
                        id INT PRIMARY KEY AUTO_INCREMENT,
                        employee_id INT NOT NULL,
                        reviewer_id INT,
                        punctuality INT DEFAULT 0,
                        teamwork INT DEFAULT 0,
                        productivity INT DEFAULT 0,
                        leadership INT DEFAULT 0,
                        vision INT DEFAULT 0,
                        managerial_skills INT DEFAULT 0,
                        comments TEXT,
                        status VARCHAR(50) DEFAULT 'SUBMITTED',
                        created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (employee_id) REFERENCES users(id),
                        FOREIGN KEY (reviewer_id) REFERENCES users(id)
                    )
                """);

        // Goals table
        stmt.execute("""
                    CREATE TABLE IF NOT EXISTS goals (
                        id INT PRIMARY KEY AUTO_INCREMENT,
                        employee_id INT NOT NULL,
                        creator_id INT,
                        description TEXT NOT NULL,
                        status VARCHAR(50) DEFAULT 'ACTIVE',
                        created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (employee_id) REFERENCES users(id),
                        FOREIGN KEY (creator_id) REFERENCES users(id)
                    )
                """);

        // Training recommendations table
        stmt.execute("""
                    CREATE TABLE IF NOT EXISTS training_recommendations (
                        id INT PRIMARY KEY AUTO_INCREMENT,
                        recipient_id INT NOT NULL,
                        recommender_id INT,
                        course_name VARCHAR(255) NOT NULL,
                        reason TEXT,
                        created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (recipient_id) REFERENCES users(id),
                        FOREIGN KEY (recommender_id) REFERENCES users(id)
                    )
                """);

        // Courses table
        stmt.execute("""
                    CREATE TABLE IF NOT EXISTS courses (
                        id INT PRIMARY KEY AUTO_INCREMENT,
                        name VARCHAR(255) UNIQUE NOT NULL
                    )
                """);

        stmt.close();
    }

    // === USER OPERATIONS ===

    public void saveUser(User user) {
        try {
            // Check if exists
            PreparedStatement check = connection.prepareStatement(
                    "SELECT id FROM users WHERE username = ?");
            check.setString(1, user.getUsername());
            ResultSet rs = check.executeQuery();

            if (rs.next()) {
                // Update existing
                int id = rs.getInt("id");
                PreparedStatement update = connection.prepareStatement("""
                            UPDATE users SET password=?, name=?, role=?, status=?, tier_level=?, manager_id=?
                            WHERE id=?
                        """);
                update.setString(1, user.getPassword());
                update.setString(2, user.getName());
                update.setString(3, user.getRole().toString());
                update.setString(4, user.getStatus().toString());
                update.setInt(5, user.getTierLevel());
                if (user.getManager() != null) {
                    update.setInt(6, getUserId(user.getManager().getUsername()));
                } else {
                    update.setNull(6, Types.INTEGER);
                }
                update.setInt(7, id);
                update.executeUpdate();
            } else {
                // Insert new
                PreparedStatement insert = connection.prepareStatement("""
                            INSERT INTO users (username, password, name, role, status, tier_level, manager_id)
                            VALUES (?, ?, ?, ?, ?, ?, ?)
                        """);
                insert.setString(1, user.getUsername());
                insert.setString(2, user.getPassword());
                insert.setString(3, user.getName());
                insert.setString(4, user.getRole().toString());
                insert.setString(5, user.getStatus().toString());
                insert.setInt(6, user.getTierLevel());
                if (user.getManager() != null) {
                    insert.setInt(7, getUserId(user.getManager().getUsername()));
                } else {
                    insert.setNull(7, Types.INTEGER);
                }
                insert.executeUpdate();
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
    }

    public int getUserId(String username) {
        try {
            PreparedStatement stmt = connection.prepareStatement(
                    "SELECT id FROM users WHERE username = ?");
            stmt.setString(1, username);
            ResultSet rs = stmt.executeQuery();
            if (rs.next())
                return rs.getInt("id");
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return -1;
    }

    public List<User> getAllUsers() {
        List<User> users = new ArrayList<>();
        Map<Integer, User> userMap = new HashMap<>();
        Map<Integer, Integer> managerMap = new HashMap<>();

        try {
            Statement stmt = connection.createStatement();
            ResultSet rs = stmt.executeQuery("SELECT * FROM users");

            while (rs.next()) {
                int id = rs.getInt("id");
                String username = rs.getString("username");
                String password = rs.getString("password");
                String name = rs.getString("name");
                String role = rs.getString("role");
                String status = rs.getString("status");
                int tierLevel = rs.getInt("tier_level");
                int managerId = rs.getInt("manager_id");

                User user;
                if ("ADMIN".equals(role)) {
                    user = new Admin(username, password, name);
                } else {
                    user = new Employee(username, password, name);
                    user.setRole(UserRole.valueOf(role));
                }
                user.setStatus(UserStatus.valueOf(status));
                user.setTierLevel(tierLevel);

                userMap.put(id, user);
                if (managerId > 0) {
                    managerMap.put(id, managerId);
                }
                users.add(user);
            }

            // Set managers
            for (Map.Entry<Integer, Integer> entry : managerMap.entrySet()) {
                User user = userMap.get(entry.getKey());
                User manager = userMap.get(entry.getValue());
                if (user != null && manager != null) {
                    user.setManager(manager);
                    manager.addDirectReport(user);
                }
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return users;
    }

    public void deleteUser(String username) {
        try {
            PreparedStatement stmt = connection.prepareStatement(
                    "DELETE FROM users WHERE username = ?");
            stmt.setString(1, username);
            stmt.executeUpdate();
        } catch (SQLException e) {
            e.printStackTrace();
        }
    }

    // === REVIEW OPERATIONS ===

    public void saveReview(Review review) {
        try {
            PreparedStatement stmt = connection.prepareStatement("""
                        INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity,
                            leadership, vision, managerial_skills, comments, status)
                        VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                    """);
            stmt.setInt(1, getUserId(review.getEmployee().getUsername()));
            stmt.setInt(2, review.getReviewer() != null ? getUserId(review.getReviewer().getUsername()) : 0);
            stmt.setInt(3, review.getPunctualityScore());
            stmt.setInt(4, review.getTeamworkScore());
            stmt.setInt(5, review.getProductivityScore());
            stmt.setInt(6, review.getLeadershipScore());
            stmt.setInt(7, review.getVisionScore());
            stmt.setInt(8, review.getManagerialSkillsScore());
            stmt.setString(9, review.getComments());
            stmt.setString(10, review.getStatus());
            stmt.executeUpdate();
        } catch (SQLException e) {
            e.printStackTrace();
        }
    }

    public List<Review> getAllReviews(Map<Integer, User> userMap) {
        List<Review> reviews = new ArrayList<>();
        try {
            Statement stmt = connection.createStatement();
            ResultSet rs = stmt.executeQuery("SELECT * FROM reviews");

            while (rs.next()) {
                User employee = userMap.get(rs.getInt("employee_id"));
                User reviewer = userMap.get(rs.getInt("reviewer_id"));
                if (employee != null) {
                    Review review = new Review(employee, reviewer);
                    review.setPunctualityScore(rs.getInt("punctuality"));
                    review.setTeamworkScore(rs.getInt("teamwork"));
                    review.setProductivityScore(rs.getInt("productivity"));
                    review.setLeadershipScore(rs.getInt("leadership"));
                    review.setVisionScore(rs.getInt("vision"));
                    review.setManagerialSkillsScore(rs.getInt("managerial_skills"));
                    review.setComments(rs.getString("comments"));
                    review.setStatus(rs.getString("status"));
                    reviews.add(review);
                }
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return reviews;
    }

    // === GOAL OPERATIONS ===

    public void saveGoal(Goal goal) {
        try {
            PreparedStatement stmt = connection.prepareStatement("""
                        INSERT INTO goals (employee_id, creator_id, description, status)
                        VALUES (?, ?, ?, ?)
                    """);
            stmt.setInt(1, getUserId(goal.getEmployee().getUsername()));
            stmt.setInt(2, goal.getCreator() != null ? getUserId(goal.getCreator().getUsername()) : 0);
            stmt.setString(3, goal.getDescription());
            stmt.setString(4, goal.getStatus());
            stmt.executeUpdate();
        } catch (SQLException e) {
            e.printStackTrace();
        }
    }

    public List<Goal> getAllGoals(Map<Integer, User> userMap) {
        List<Goal> goals = new ArrayList<>();
        try {
            Statement stmt = connection.createStatement();
            ResultSet rs = stmt.executeQuery("SELECT * FROM goals");

            while (rs.next()) {
                User employee = userMap.get(rs.getInt("employee_id"));
                User creator = userMap.get(rs.getInt("creator_id"));
                if (employee != null) {
                    Goal goal = new Goal(employee, rs.getString("description"), creator);
                    goal.setStatus(rs.getString("status"));
                    goals.add(goal);
                }
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return goals;
    }

    public void deleteGoal(Goal goal) {
        try {
            PreparedStatement stmt = connection.prepareStatement(
                    "DELETE FROM goals WHERE employee_id = ? AND description = ?");
            stmt.setInt(1, getUserId(goal.getEmployee().getUsername()));
            stmt.setString(2, goal.getDescription());
            stmt.executeUpdate();
        } catch (SQLException e) {
            e.printStackTrace();
        }
    }

    // === COURSE OPERATIONS ===

    public void saveCourse(String courseName) {
        try {
            PreparedStatement stmt = connection.prepareStatement(
                    "INSERT IGNORE INTO courses (name) VALUES (?)");
            stmt.setString(1, courseName);
            stmt.executeUpdate();
        } catch (SQLException e) {
            e.printStackTrace();
        }
    }

    public List<String> getAllCourses() {
        List<String> courses = new ArrayList<>();
        try {
            Statement stmt = connection.createStatement();
            ResultSet rs = stmt.executeQuery("SELECT name FROM courses");
            while (rs.next()) {
                courses.add(rs.getString("name"));
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return courses;
    }

    public void deleteCourse(String name) {
        try {
            PreparedStatement stmt = connection.prepareStatement(
                    "DELETE FROM courses WHERE name = ?");
            stmt.setString(1, name);
            stmt.executeUpdate();
        } catch (SQLException e) {
            e.printStackTrace();
        }
    }

    // === TRAINING RECOMMENDATIONS ===

    public void saveTrainingRecommendation(TrainingRecommendation tr) {
        try {
            PreparedStatement stmt = connection.prepareStatement("""
                        INSERT INTO training_recommendations (recipient_id, recommender_id, course_name)
                        VALUES (?, ?, ?)
                    """);
            stmt.setInt(1, getUserId(tr.getRecipient().getUsername()));
            stmt.setInt(2, tr.getRecommender() != null ? getUserId(tr.getRecommender().getUsername()) : 0);
            stmt.setString(3, tr.getCourseName());
            stmt.executeUpdate();
        } catch (SQLException e) {
            e.printStackTrace();
        }
    }

    public List<TrainingRecommendation> getAllTrainingRecommendations(Map<Integer, User> userMap) {
        List<TrainingRecommendation> recommendations = new ArrayList<>();
        try {
            Statement stmt = connection.createStatement();
            ResultSet rs = stmt.executeQuery("SELECT * FROM training_recommendations");

            while (rs.next()) {
                User recipient = userMap.get(rs.getInt("recipient_id"));
                User recommender = userMap.get(rs.getInt("recommender_id"));
                if (recipient != null) {
                    TrainingRecommendation tr = new TrainingRecommendation(
                            recommender, recipient, rs.getString("course_name"));
                    recommendations.add(tr);
                }
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return recommendations;
    }

    public void close() {
        try {
            if (connection != null)
                connection.close();
        } catch (SQLException e) {
            e.printStackTrace();
        }
    }
}
