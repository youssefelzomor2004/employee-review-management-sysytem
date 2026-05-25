-- Performance Review System Database Schema
-- MySQL Database
-- Total Tables: 9

-- Create and select database
CREATE DATABASE IF NOT EXISTS performance;
USE performance;

-- Disable foreign key checks for clean table drops
SET FOREIGN_KEY_CHECKS = 0;

-- Drop existing tables (for clean reset)
DROP TABLE IF EXISTS skill_gaps;
DROP TABLE IF EXISTS pending_actions;
DROP TABLE IF EXISTS upward_reviews;
DROP TABLE IF EXISTS peer_reviews;
DROP TABLE IF EXISTS training_recommendations;
DROP TABLE IF EXISTS goals;
DROP TABLE IF EXISTS reviews;
DROP TABLE IF EXISTS courses;
DROP TABLE IF EXISTS users;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- ============================================
-- USERS TABLE
-- ============================================
CREATE TABLE users (
    id INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    name VARCHAR(255) NOT NULL,
    role ENUM('ADMIN', 'EMPLOYEE', 'MANAGER_EMPLOYEE', 'HIGHBOARD_MANAGER') DEFAULT 'EMPLOYEE',
    status ENUM('ACTIVE', 'PENDING', 'SUSPENDED') DEFAULT 'PENDING',
    tier_level INT DEFAULT 1,
    manager_id INT,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (manager_id) REFERENCES users(id) ON DELETE SET NULL
);

-- ============================================
-- REVIEWS TABLE
-- ============================================
CREATE TABLE reviews (
    id INT PRIMARY KEY AUTO_INCREMENT,
    employee_id INT NOT NULL,
    reviewer_id INT,
    -- Basic categories (all employees)
    punctuality INT DEFAULT 0 CHECK(punctuality BETWEEN 0 AND 10),
    teamwork INT DEFAULT 0 CHECK(teamwork BETWEEN 0 AND 10),
    productivity INT DEFAULT 0 CHECK(productivity BETWEEN 0 AND 10),
    -- Extended categories (managers only)
    leadership INT DEFAULT 0 CHECK(leadership BETWEEN 0 AND 10),
    vision INT DEFAULT 0 CHECK(vision BETWEEN 0 AND 10),
    managerial_skills INT DEFAULT 0 CHECK(managerial_skills BETWEEN 0 AND 10),
    comments TEXT,
    status VARCHAR(50) DEFAULT 'SUBMITTED',
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (employee_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (reviewer_id) REFERENCES users(id) ON DELETE SET NULL
);

-- ============================================
-- GOALS TABLE
-- ============================================
CREATE TABLE goals (
    id INT PRIMARY KEY AUTO_INCREMENT,
    employee_id INT NOT NULL,
    creator_id INT,
    description TEXT NOT NULL,
    status ENUM('ACTIVE', 'COMPLETED') DEFAULT 'ACTIVE',
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (employee_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (creator_id) REFERENCES users(id) ON DELETE SET NULL
);

-- ============================================
-- COURSES TABLE
-- ============================================
CREATE TABLE courses (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(255) UNIQUE NOT NULL,
    description TEXT,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ============================================
-- TRAINING RECOMMENDATIONS TABLE
-- ============================================
CREATE TABLE training_recommendations (
    id INT PRIMARY KEY AUTO_INCREMENT,
    recipient_id INT NOT NULL,
    recommender_id INT,
    course_name VARCHAR(255) NOT NULL,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (recipient_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (recommender_id) REFERENCES users(id) ON DELETE SET NULL
);

-- ============================================
-- PEER_REVIEWS TABLE
-- Anonymous reviews from teammates (same manager)
-- ============================================
CREATE TABLE peer_reviews (
    id INT PRIMARY KEY AUTO_INCREMENT,
    reviewer_id INT NOT NULL,
    reviewed_id INT NOT NULL,
    collaboration_score INT DEFAULT 0 CHECK(collaboration_score BETWEEN 1 AND 5),
    communication_score INT DEFAULT 0 CHECK(communication_score BETWEEN 1 AND 5),
    teamwork_score INT DEFAULT 0 CHECK(teamwork_score BETWEEN 1 AND 5),
    comments TEXT,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (reviewer_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (reviewed_id) REFERENCES users(id) ON DELETE CASCADE
);

-- ============================================
-- UPWARD_REVIEWS TABLE
-- Anonymous reviews of managers by their direct reports
-- ============================================
CREATE TABLE upward_reviews (
    id INT PRIMARY KEY AUTO_INCREMENT,
    reviewer_id INT NOT NULL,
    manager_id INT NOT NULL,
    punctuality_score INT DEFAULT 0 CHECK(punctuality_score BETWEEN 1 AND 5),
    teamwork_score INT DEFAULT 0 CHECK(teamwork_score BETWEEN 1 AND 5),
    productivity_score INT DEFAULT 0 CHECK(productivity_score BETWEEN 1 AND 5),
    comments TEXT,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (reviewer_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (manager_id) REFERENCES users(id) ON DELETE CASCADE
);

-- ============================================
-- PENDING_ACTIONS TABLE
-- Manager requests requiring admin approval
-- ============================================
CREATE TABLE pending_actions (
    id VARCHAR(50) PRIMARY KEY,
    requested_by_id INT NOT NULL,
    target_user_id INT NOT NULL,
    action_type ENUM('SUSPEND', 'CHANGE_TIER', 'DELETE') NOT NULL,
    new_value VARCHAR(255),
    requested_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    status ENUM('PENDING', 'APPROVED', 'REJECTED') DEFAULT 'PENDING',
    rejection_reason TEXT,
    processed_by_id INT,
    processed_date DATETIME,
    FOREIGN KEY (requested_by_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (target_user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (processed_by_id) REFERENCES users(id) ON DELETE SET NULL
);

-- ============================================
-- SKILL_GAPS TABLE
-- Cached skill gap analysis results
-- ============================================
CREATE TABLE skill_gaps (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL UNIQUE,
    punctuality_score DECIMAL(5,2) DEFAULT 0,
    teamwork_score DECIMAL(5,2) DEFAULT 0,
    productivity_score DECIMAL(5,2) DEFAULT 0,
    leadership_score DECIMAL(5,2) DEFAULT 0,
    vision_score DECIMAL(5,2) DEFAULT 0,
    managerial_score DECIMAL(5,2) DEFAULT 0,
    gaps TEXT,
    last_updated DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- ============================================
-- INDEXES for Performance
-- ============================================
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_manager ON users(manager_id);
CREATE INDEX idx_reviews_employee ON reviews(employee_id);
CREATE INDEX idx_reviews_reviewer ON reviews(reviewer_id);
CREATE INDEX idx_goals_employee ON goals(employee_id);
CREATE INDEX idx_training_recipient ON training_recommendations(recipient_id);
CREATE INDEX idx_peer_reviews_reviewer ON peer_reviews(reviewer_id);
CREATE INDEX idx_peer_reviews_reviewed ON peer_reviews(reviewed_id);
CREATE INDEX idx_upward_reviews_reviewer ON upward_reviews(reviewer_id);
CREATE INDEX idx_upward_reviews_manager ON upward_reviews(manager_id);
CREATE INDEX idx_pending_actions_status ON pending_actions(status);
CREATE INDEX idx_pending_actions_target ON pending_actions(target_user_id);
CREATE INDEX idx_skill_gaps_user ON skill_gaps(user_id);

-- ============================================
-- DEFAULT ADMIN USER
-- ============================================
INSERT INTO users (username, password, name, role, status, tier_level) 
VALUES ('admin', 'admin', 'System Administrator', 'ADMIN', 'ACTIVE', 0);

-- Note: Courses and sample data are in sample_data.sql
