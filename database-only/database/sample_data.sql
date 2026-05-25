-- ============================================
-- Sample Data for Performance Review System
-- MySQL Database
-- Run after schema.sql
-- ============================================
-- NOTE: Tier structure is DYNAMIC (MAX_TIERS can be changed)
-- Default MAX_TIERS = 3
--   Tier 1: Regular Employees
--   Tier 2: Manager & Employee (middle managers)
--   Tier 3 (MAX_TIERS): Highboard Manager (top of hierarchy)
--   Tier 4 (MAX_TIERS + 1): Admin (system owner, above all)
-- ============================================

-- Select database
USE performance;

-- ============================================
-- SAMPLE USERS (14 users across all tiers)
-- Note: Admin user is already created in schema.sql
-- ============================================

-- Highboard Manager (Tier MAX_TIERS, default = 3)
INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('ceo', 'ceo123', 'John Williams', 'HIGHBOARD_MANAGER', 'ACTIVE', 3, NULL);

-- Middle Managers (Tier 2 - MANAGER_EMPLOYEE)
INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('sarah_vp', 'pass123', 'Sarah Johnson', 'MANAGER_EMPLOYEE', 'ACTIVE', 2, 
        (SELECT id FROM (SELECT id FROM users WHERE username = 'ceo') AS temp));

INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('mike_dir', 'pass123', 'Michael Chen', 'MANAGER_EMPLOYEE', 'ACTIVE', 2, 
        (SELECT id FROM (SELECT id FROM users WHERE username = 'ceo') AS temp));

INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('lisa_mgr', 'pass123', 'Lisa Rodriguez', 'MANAGER_EMPLOYEE', 'ACTIVE', 2, 
        (SELECT id FROM (SELECT id FROM users WHERE username = 'ceo') AS temp));

-- Team Leads (Tier 1 with direct reports)
INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('david_lead', 'pass123', 'David Miller', 'EMPLOYEE', 'ACTIVE', 1, 
        (SELECT id FROM (SELECT id FROM users WHERE username = 'sarah_vp') AS temp));

INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('emma_lead', 'pass123', 'Emma Thompson', 'EMPLOYEE', 'ACTIVE', 1, 
        (SELECT id FROM (SELECT id FROM users WHERE username = 'mike_dir') AS temp));

-- Regular Employees (Tier 1)
INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('alice_emp', 'pass123', 'Alice Brown', 'EMPLOYEE', 'ACTIVE', 1, 
        (SELECT id FROM (SELECT id FROM users WHERE username = 'sarah_vp') AS temp));

INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('bob_emp', 'pass123', 'Bob Smith', 'EMPLOYEE', 'ACTIVE', 1, 
        (SELECT id FROM (SELECT id FROM users WHERE username = 'sarah_vp') AS temp));

INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('carol_emp', 'pass123', 'Carol Davis', 'EMPLOYEE', 'ACTIVE', 1, 
        (SELECT id FROM (SELECT id FROM users WHERE username = 'mike_dir') AS temp));

INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('derek_emp', 'pass123', 'Derek Wilson', 'EMPLOYEE', 'ACTIVE', 1, 
        (SELECT id FROM (SELECT id FROM users WHERE username = 'mike_dir') AS temp));

INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('eva_emp', 'pass123', 'Eva Martinez', 'EMPLOYEE', 'ACTIVE', 1, 
        (SELECT id FROM (SELECT id FROM users WHERE username = 'lisa_mgr') AS temp));

INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('frank_emp', 'pass123', 'Frank Lee', 'EMPLOYEE', 'ACTIVE', 1, 
        (SELECT id FROM (SELECT id FROM users WHERE username = 'lisa_mgr') AS temp));

-- Pending Employee (not yet activated)
INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('new_hire', 'pass123', 'New Hire Pending', 'EMPLOYEE', 'PENDING', 1, NULL);

-- Suspended Employee
INSERT INTO users (username, password, name, role, status, tier_level, manager_id) 
VALUES ('suspended_user', 'pass123', 'Suspended User', 'EMPLOYEE', 'SUSPENDED', 1, 
        (SELECT id FROM (SELECT id FROM users WHERE username = 'sarah_vp') AS temp));

-- ============================================
-- SAMPLE COURSES (10 courses)
-- ============================================
INSERT INTO courses (name, description) VALUES 
    ('Leadership Fundamentals', 'Core leadership skills and principles'),
    ('Time Management', 'Strategies for effective time management'),
    ('Team Communication', 'Building effective communication within teams'),
    ('Project Management', 'Introduction to project management methodologies'),
    ('Conflict Resolution', 'Handling workplace conflicts professionally'),
    ('Public Speaking', 'Presentation and public speaking skills'),
    ('Data Analysis', 'Basic data analysis and reporting'),
    ('Customer Service Excellence', 'Best practices for customer interactions'),
    ('Agile Methodology', 'Agile and Scrum fundamentals'),
    ('Technical Writing', 'Writing clear technical documentation');

-- ============================================
-- SAMPLE REVIEWS (20+ reviews)
-- ============================================

-- CEO reviews managers (with extended scores)
INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, leadership, vision, managerial_skills, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    (SELECT id FROM users WHERE username = 'ceo'),
    9, 8, 9, 8, 7, 9,
    'Excellent leadership and strategic thinking. Consistently delivers results.',
    'SUBMITTED';

INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, leadership, vision, managerial_skills, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'mike_dir'),
    (SELECT id FROM users WHERE username = 'ceo'),
    8, 9, 8, 7, 8, 8,
    'Great team builder. Needs to work on long-term vision.',
    'SUBMITTED';

INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, leadership, vision, managerial_skills, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    (SELECT id FROM users WHERE username = 'ceo'),
    7, 8, 9, 8, 8, 7,
    'Highly productive. Could improve on delegation.',
    'SUBMITTED';

-- Admin reviews CEO
INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, leadership, vision, managerial_skills, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'ceo'),
    (SELECT id FROM users WHERE username = 'admin'),
    10, 9, 10, 9, 10, 9,
    'Outstanding performance across all metrics. Exemplary leader.',
    'SUBMITTED';

-- Managers review employees
INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'alice_emp'),
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    9, 8, 7,
    'Great team player, always on time. Could improve productivity.',
    'SUBMITTED';

INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'bob_emp'),
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    6, 7, 8,
    'Good productivity but needs to improve punctuality.',
    'SUBMITTED';

INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'david_lead'),
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    8, 9, 9,
    'Excellent team lead. Ready for more responsibility.',
    'SUBMITTED';

INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'carol_emp'),
    (SELECT id FROM users WHERE username = 'mike_dir'),
    9, 9, 8,
    'Outstanding teamwork and communication skills.',
    'SUBMITTED';

INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'derek_emp'),
    (SELECT id FROM users WHERE username = 'mike_dir'),
    7, 6, 9,
    'Very productive but needs to collaborate more with team.',
    'SUBMITTED';

INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'emma_lead'),
    (SELECT id FROM users WHERE username = 'mike_dir'),
    8, 8, 8,
    'Consistent performer. Good leadership potential.',
    'SUBMITTED';

INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'eva_emp'),
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    10, 8, 9,
    'Always punctual and delivers high-quality work.',
    'SUBMITTED';

INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'frank_emp'),
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    5, 7, 6,
    'Performance needs improvement. Setting up a development plan.',
    'SUBMITTED';

-- Draft review (not yet submitted)
INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'alice_emp'),
    (SELECT id FROM users WHERE username = 'mike_dir'),
    7, 7, 7,
    'Cross-team review in progress...',
    'DRAFT';

-- Multiple reviews for same employee (historical)
INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'alice_emp'),
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    8, 8, 8,
    'Q1 Review: Showing consistent improvement.',
    'SUBMITTED';

INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'bob_emp'),
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    7, 8, 9,
    'Q2 Review: Great improvement in productivity!',
    'SUBMITTED';

-- Low performer for skill gap testing
INSERT INTO reviews (employee_id, reviewer_id, punctuality, teamwork, productivity, comments, status)
SELECT 
    (SELECT id FROM users WHERE username = 'frank_emp'),
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    4, 5, 4,
    'Struggling with deadlines. Needs immediate support.',
    'SUBMITTED';

-- ============================================
-- SAMPLE GOALS (12 goals)
-- ============================================

-- Self-created goals
INSERT INTO goals (employee_id, creator_id, description, status)
SELECT 
    (SELECT id FROM users WHERE username = 'alice_emp'),
    (SELECT id FROM users WHERE username = 'alice_emp'),
    'Complete Java certification by Q2 2024',
    'ACTIVE';

INSERT INTO goals (employee_id, creator_id, description, status)
SELECT 
    (SELECT id FROM users WHERE username = 'bob_emp'),
    (SELECT id FROM users WHERE username = 'bob_emp'),
    'Improve code review turnaround time to under 24 hours',
    'ACTIVE';

INSERT INTO goals (employee_id, creator_id, description, status)
SELECT 
    (SELECT id FROM users WHERE username = 'carol_emp'),
    (SELECT id FROM users WHERE username = 'carol_emp'),
    'Lead at least one team presentation this quarter',
    'COMPLETED';

-- Manager-assigned goals
INSERT INTO goals (employee_id, creator_id, description, status)
SELECT 
    (SELECT id FROM users WHERE username = 'david_lead'),
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    'Mentor 2 junior developers by end of quarter',
    'ACTIVE';

INSERT INTO goals (employee_id, creator_id, description, status)
SELECT 
    (SELECT id FROM users WHERE username = 'emma_lead'),
    (SELECT id FROM users WHERE username = 'mike_dir'),
    'Reduce team bug rate by 25%',
    'ACTIVE';

INSERT INTO goals (employee_id, creator_id, description, status)
SELECT 
    (SELECT id FROM users WHERE username = 'frank_emp'),
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    'Complete Time Management training course',
    'ACTIVE';

-- Highboard manager goals for middle managers
INSERT INTO goals (employee_id, creator_id, description, status)
SELECT 
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    (SELECT id FROM users WHERE username = 'ceo'),
    'Improve team productivity metrics by 15%',
    'ACTIVE';

INSERT INTO goals (employee_id, creator_id, description, status)
SELECT 
    (SELECT id FROM users WHERE username = 'mike_dir'),
    (SELECT id FROM users WHERE username = 'ceo'),
    'Reduce employee turnover rate in department',
    'ACTIVE';

INSERT INTO goals (employee_id, creator_id, description, status)
SELECT 
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    (SELECT id FROM users WHERE username = 'ceo'),
    'Implement new onboarding process for team',
    'COMPLETED';

-- Team-wide goal
INSERT INTO goals (employee_id, creator_id, description, status)
SELECT 
    (SELECT id FROM users WHERE username = 'eva_emp'),
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    'Participate in cross-functional project',
    'ACTIVE';

INSERT INTO goals (employee_id, creator_id, description, status)
SELECT 
    (SELECT id FROM users WHERE username = 'derek_emp'),
    (SELECT id FROM users WHERE username = 'mike_dir'),
    'Improve collaboration skills - attend Team Communication course',
    'ACTIVE';

INSERT INTO goals (employee_id, creator_id, description, status)
SELECT 
    (SELECT id FROM users WHERE username = 'alice_emp'),
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    'Take on project lead role for Q3 initiative',
    'ACTIVE';

-- ============================================
-- SAMPLE TRAINING RECOMMENDATIONS (10 records)
-- ============================================

INSERT INTO training_recommendations (recipient_id, recommender_id, course_name)
SELECT 
    (SELECT id FROM users WHERE username = 'david_lead'),
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    'Leadership Fundamentals';

INSERT INTO training_recommendations (recipient_id, recommender_id, course_name)
SELECT 
    (SELECT id FROM users WHERE username = 'emma_lead'),
    (SELECT id FROM users WHERE username = 'mike_dir'),
    'Leadership Fundamentals';

INSERT INTO training_recommendations (recipient_id, recommender_id, course_name)
SELECT 
    (SELECT id FROM users WHERE username = 'bob_emp'),
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    'Time Management';

INSERT INTO training_recommendations (recipient_id, recommender_id, course_name)
SELECT 
    (SELECT id FROM users WHERE username = 'frank_emp'),
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    'Time Management';

INSERT INTO training_recommendations (recipient_id, recommender_id, course_name)
SELECT 
    (SELECT id FROM users WHERE username = 'derek_emp'),
    (SELECT id FROM users WHERE username = 'mike_dir'),
    'Team Communication';

INSERT INTO training_recommendations (recipient_id, recommender_id, course_name)
SELECT 
    (SELECT id FROM users WHERE username = 'alice_emp'),
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    'Project Management';

INSERT INTO training_recommendations (recipient_id, recommender_id, course_name)
SELECT 
    (SELECT id FROM users WHERE username = 'carol_emp'),
    (SELECT id FROM users WHERE username = 'mike_dir'),
    'Public Speaking';

INSERT INTO training_recommendations (recipient_id, recommender_id, course_name)
SELECT 
    (SELECT id FROM users WHERE username = 'eva_emp'),
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    'Data Analysis';

-- Admin recommending to managers
INSERT INTO training_recommendations (recipient_id, recommender_id, course_name)
SELECT 
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    (SELECT id FROM users WHERE username = 'admin'),
    'Conflict Resolution';

INSERT INTO training_recommendations (recipient_id, recommender_id, course_name)
SELECT 
    (SELECT id FROM users WHERE username = 'mike_dir'),
    (SELECT id FROM users WHERE username = 'ceo'),
    'Agile Methodology';

-- ============================================
-- SAMPLE PEER REVIEWS (12 records)
-- Anonymous reviews from teammates with same manager
-- ============================================

-- Peers under sarah_vp reviewing each other
INSERT INTO peer_reviews (reviewer_id, reviewed_id, collaboration_score, communication_score, teamwork_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'alice_emp'),
    (SELECT id FROM users WHERE username = 'bob_emp'),
    4, 3, 4,
    'Great collaborator, could improve communication on blockers.';

INSERT INTO peer_reviews (reviewer_id, reviewed_id, collaboration_score, communication_score, teamwork_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'bob_emp'),
    (SELECT id FROM users WHERE username = 'alice_emp'),
    5, 5, 5,
    'Excellent teammate, always willing to help.';

INSERT INTO peer_reviews (reviewer_id, reviewed_id, collaboration_score, communication_score, teamwork_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'david_lead'),
    (SELECT id FROM users WHERE username = 'alice_emp'),
    4, 4, 5,
    'Reliable team member with strong technical skills.';

INSERT INTO peer_reviews (reviewer_id, reviewed_id, collaboration_score, communication_score, teamwork_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'alice_emp'),
    (SELECT id FROM users WHERE username = 'david_lead'),
    5, 4, 5,
    'Great leader, provides clear guidance.';

-- Peers under mike_dir reviewing each other
INSERT INTO peer_reviews (reviewer_id, reviewed_id, collaboration_score, communication_score, teamwork_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'carol_emp'),
    (SELECT id FROM users WHERE username = 'derek_emp'),
    3, 2, 3,
    'Tends to work in isolation. Could benefit from more collaboration.';

INSERT INTO peer_reviews (reviewer_id, reviewed_id, collaboration_score, communication_score, teamwork_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'derek_emp'),
    (SELECT id FROM users WHERE username = 'carol_emp'),
    5, 5, 5,
    'Amazing communicator and team player.';

INSERT INTO peer_reviews (reviewer_id, reviewed_id, collaboration_score, communication_score, teamwork_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'emma_lead'),
    (SELECT id FROM users WHERE username = 'carol_emp'),
    4, 5, 4,
    'Strong communication skills and proactive.';

INSERT INTO peer_reviews (reviewer_id, reviewed_id, collaboration_score, communication_score, teamwork_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'carol_emp'),
    (SELECT id FROM users WHERE username = 'emma_lead'),
    5, 4, 5,
    'Great leadership potential and team coordination.';

-- Peers under lisa_mgr reviewing each other
INSERT INTO peer_reviews (reviewer_id, reviewed_id, collaboration_score, communication_score, teamwork_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'eva_emp'),
    (SELECT id FROM users WHERE username = 'frank_emp'),
    2, 3, 2,
    'Needs to participate more in team activities.';

INSERT INTO peer_reviews (reviewer_id, reviewed_id, collaboration_score, communication_score, teamwork_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'frank_emp'),
    (SELECT id FROM users WHERE username = 'eva_emp'),
    5, 5, 5,
    'Very supportive colleague, always helpful.';

-- Cross-reference reviews (same manager different reports)
INSERT INTO peer_reviews (reviewer_id, reviewed_id, collaboration_score, communication_score, teamwork_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'bob_emp'),
    (SELECT id FROM users WHERE username = 'david_lead'),
    5, 5, 5,
    'Excellent mentor and leader.';

INSERT INTO peer_reviews (reviewer_id, reviewed_id, collaboration_score, communication_score, teamwork_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'derek_emp'),
    (SELECT id FROM users WHERE username = 'emma_lead'),
    4, 4, 4,
    'Good at coordinating team efforts.';

-- ============================================
-- SAMPLE UPWARD REVIEWS (10 records)
-- Employees reviewing their managers (anonymous)
-- ============================================

-- Employees reviewing sarah_vp
INSERT INTO upward_reviews (reviewer_id, manager_id, punctuality_score, teamwork_score, productivity_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'alice_emp'),
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    5, 5, 5,
    'Very supportive manager, always available for guidance.';

INSERT INTO upward_reviews (reviewer_id, manager_id, punctuality_score, teamwork_score, productivity_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'bob_emp'),
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    4, 5, 4,
    'Great at team building, could improve on meeting timeliness.';

INSERT INTO upward_reviews (reviewer_id, manager_id, punctuality_score, teamwork_score, productivity_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'david_lead'),
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    5, 4, 5,
    'Provides clear direction and empowers team members.';

-- Employees reviewing mike_dir
INSERT INTO upward_reviews (reviewer_id, manager_id, punctuality_score, teamwork_score, productivity_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'carol_emp'),
    (SELECT id FROM users WHERE username = 'mike_dir'),
    5, 4, 4,
    'Very organized and reliable. Good mentor.';

INSERT INTO upward_reviews (reviewer_id, manager_id, punctuality_score, teamwork_score, productivity_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'derek_emp'),
    (SELECT id FROM users WHERE username = 'mike_dir'),
    4, 4, 4,
    'Fair and consistent in feedback.';

INSERT INTO upward_reviews (reviewer_id, manager_id, punctuality_score, teamwork_score, productivity_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'emma_lead'),
    (SELECT id FROM users WHERE username = 'mike_dir'),
    5, 5, 5,
    'Excellent manager, very approachable.';

-- Employees reviewing lisa_mgr
INSERT INTO upward_reviews (reviewer_id, manager_id, punctuality_score, teamwork_score, productivity_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'eva_emp'),
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    5, 4, 5,
    'Highly productive and sets clear expectations.';

INSERT INTO upward_reviews (reviewer_id, manager_id, punctuality_score, teamwork_score, productivity_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'frank_emp'),
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    4, 3, 4,
    'Good manager but could provide more regular feedback.';

-- Managers reviewing CEO
INSERT INTO upward_reviews (reviewer_id, manager_id, punctuality_score, teamwork_score, productivity_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    (SELECT id FROM users WHERE username = 'ceo'),
    5, 5, 5,
    'Visionary leader with clear strategic direction.';

INSERT INTO upward_reviews (reviewer_id, manager_id, punctuality_score, teamwork_score, productivity_score, comments)
SELECT 
    (SELECT id FROM users WHERE username = 'mike_dir'),
    (SELECT id FROM users WHERE username = 'ceo'),
    5, 4, 5,
    'Great communicator, inspires the entire organization.';

-- ============================================
-- SAMPLE PENDING ACTIONS (6 records)
-- Manager requests requiring admin approval
-- ============================================

-- Pending suspension request
INSERT INTO pending_actions (id, requested_by_id, target_user_id, action_type, new_value, status)
SELECT 
    'REQ-001',
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    (SELECT id FROM users WHERE username = 'frank_emp'),
    'SUSPEND',
    NULL,
    'PENDING';

-- Pending tier change request
INSERT INTO pending_actions (id, requested_by_id, target_user_id, action_type, new_value, status)
SELECT 
    'REQ-002',
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    (SELECT id FROM users WHERE username = 'david_lead'),
    'CHANGE_TIER',
    '2',
    'PENDING';

-- Approved request (historical)
INSERT INTO pending_actions (id, requested_by_id, target_user_id, action_type, new_value, status, processed_by_id, processed_date)
SELECT 
    'REQ-003',
    (SELECT id FROM users WHERE username = 'mike_dir'),
    (SELECT id FROM users WHERE username = 'emma_lead'),
    'CHANGE_TIER',
    '2',
    'APPROVED',
    (SELECT id FROM users WHERE username = 'admin'),
    DATE_SUB(NOW(), INTERVAL 7 DAY);

-- Rejected request (historical)
INSERT INTO pending_actions (id, requested_by_id, target_user_id, action_type, new_value, status, rejection_reason, processed_by_id, processed_date)
SELECT 
    'REQ-004',
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    (SELECT id FROM users WHERE username = 'eva_emp'),
    'DELETE',
    NULL,
    'REJECTED',
    'Employee is valuable and shows potential for improvement.',
    (SELECT id FROM users WHERE username = 'admin'),
    DATE_SUB(NOW(), INTERVAL 14 DAY);

-- Another pending suspension
INSERT INTO pending_actions (id, requested_by_id, target_user_id, action_type, new_value, status)
SELECT 
    'REQ-005',
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    (SELECT id FROM users WHERE username = 'suspended_user'),
    'SUSPEND',
    NULL,
    'APPROVED';

-- Pending deletion request
INSERT INTO pending_actions (id, requested_by_id, target_user_id, action_type, new_value, status)
SELECT 
    'REQ-006',
    (SELECT id FROM users WHERE username = 'ceo'),
    (SELECT id FROM users WHERE username = 'new_hire'),
    'DELETE',
    NULL,
    'PENDING';

-- ============================================
-- SAMPLE SKILL GAPS (8 records)
-- Cached skill gap analysis results
-- ============================================

-- Employees with skill gaps (scores below 3.0 threshold)
INSERT INTO skill_gaps (user_id, punctuality_score, teamwork_score, productivity_score, leadership_score, vision_score, managerial_score, gaps)
SELECT 
    (SELECT id FROM users WHERE username = 'frank_emp'),
    4.5, 6.0, 5.0, 0, 0, 0,
    'Punctuality (below threshold), Productivity (below threshold)';

INSERT INTO skill_gaps (user_id, punctuality_score, teamwork_score, productivity_score, leadership_score, vision_score, managerial_score, gaps)
SELECT 
    (SELECT id FROM users WHERE username = 'bob_emp'),
    6.5, 7.5, 8.5, 0, 0, 0,
    NULL;  -- No gaps (all above threshold)

INSERT INTO skill_gaps (user_id, punctuality_score, teamwork_score, productivity_score, leadership_score, vision_score, managerial_score, gaps)
SELECT 
    (SELECT id FROM users WHERE username = 'derek_emp'),
    7.0, 6.0, 9.0, 0, 0, 0,
    'Teamwork (needs improvement)';

-- Managers with extended scores
INSERT INTO skill_gaps (user_id, punctuality_score, teamwork_score, productivity_score, leadership_score, vision_score, managerial_score, gaps)
SELECT 
    (SELECT id FROM users WHERE username = 'sarah_vp'),
    9.0, 8.0, 9.0, 8.0, 7.0, 9.0,
    NULL;  -- No gaps

INSERT INTO skill_gaps (user_id, punctuality_score, teamwork_score, productivity_score, leadership_score, vision_score, managerial_score, gaps)
SELECT 
    (SELECT id FROM users WHERE username = 'mike_dir'),
    8.0, 9.0, 8.0, 7.0, 8.0, 8.0,
    NULL;  -- No gaps

INSERT INTO skill_gaps (user_id, punctuality_score, teamwork_score, productivity_score, leadership_score, vision_score, managerial_score, gaps)
SELECT 
    (SELECT id FROM users WHERE username = 'lisa_mgr'),
    7.0, 8.0, 9.0, 8.0, 8.0, 7.0,
    NULL;  -- No gaps

-- Top performers
INSERT INTO skill_gaps (user_id, punctuality_score, teamwork_score, productivity_score, leadership_score, vision_score, managerial_score, gaps)
SELECT 
    (SELECT id FROM users WHERE username = 'alice_emp'),
    8.5, 8.0, 7.5, 0, 0, 0,
    NULL;  -- No gaps

INSERT INTO skill_gaps (user_id, punctuality_score, teamwork_score, productivity_score, leadership_score, vision_score, managerial_score, gaps)
SELECT 
    (SELECT id FROM users WHERE username = 'eva_emp'),
    10.0, 8.0, 9.0, 0, 0, 0,
    NULL;  -- No gaps - top performer
