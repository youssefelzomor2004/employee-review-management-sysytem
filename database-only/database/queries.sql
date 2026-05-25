-- ============================================
-- Useful Queries for the Performance Review System
-- Database: MySQL (requires MySQL 8.0+ for recursive CTEs)
-- Covers all 9 database tables
-- ============================================

-- Select database
USE performance;

-- ============================================
-- USER QUERIES
-- ============================================

-- Get all users with their managers and tier info
SELECT 
    u.id, 
    u.username, 
    u.name, 
    u.role, 
    u.status,
    u.tier_level,
    m.name as manager_name,
    m.tier_level as manager_tier
FROM users u
LEFT JOIN users m ON u.manager_id = m.id
ORDER BY u.tier_level DESC, u.name;

-- Get user hierarchy (all subordinates using recursive CTE)
WITH RECURSIVE subordinates AS (
    SELECT id, username, name, tier_level, manager_id, 0 as depth
    FROM users WHERE username = 'ceo'
    UNION ALL
    SELECT u.id, u.username, u.name, u.tier_level, u.manager_id, s.depth + 1
    FROM users u
    JOIN subordinates s ON u.manager_id = s.id
)
SELECT * FROM subordinates ORDER BY depth, tier_level DESC;

-- Get chain of command for a specific user (all managers above)
WITH RECURSIVE chain AS (
    SELECT id, username, name, tier_level, manager_id, 0 as level
    FROM users WHERE username = 'alice_emp'
    UNION ALL
    SELECT u.id, u.username, u.name, u.tier_level, u.manager_id, c.level + 1
    FROM users u
    JOIN chain c ON u.id = c.manager_id
    WHERE c.manager_id IS NOT NULL
)
SELECT * FROM chain WHERE level > 0 ORDER BY level;

-- Get all direct reports for a manager
SELECT 
    u.id,
    u.username,
    u.name,
    u.role,
    u.tier_level,
    u.status
FROM users u
WHERE u.manager_id = (SELECT id FROM users WHERE username = 'sarah_vp')
ORDER BY u.name;

-- Count users by role and status
SELECT 
    role,
    status,
    COUNT(*) as user_count
FROM users
GROUP BY role, status
ORDER BY role, status;

-- Get pending users awaiting activation
SELECT id, username, name, role
FROM users
WHERE status = 'PENDING';

-- ============================================
-- REVIEW QUERIES
-- ============================================

-- Get average scores for each employee
SELECT 
    u.name as employee,
    u.role,
    u.tier_level,
    ROUND(AVG(r.punctuality), 1) as avg_punctuality,
    ROUND(AVG(r.teamwork), 1) as avg_teamwork,
    ROUND(AVG(r.productivity), 1) as avg_productivity,
    ROUND(AVG(r.leadership), 1) as avg_leadership,
    ROUND(AVG(r.vision), 1) as avg_vision,
    ROUND(AVG(r.managerial_skills), 1) as avg_managerial,
    ROUND((AVG(r.punctuality) + AVG(r.teamwork) + AVG(r.productivity)) / 3.0, 1) as overall_basic,
    COUNT(*) as review_count
FROM reviews r
JOIN users u ON r.employee_id = u.id
WHERE r.status = 'SUBMITTED'
GROUP BY r.employee_id
ORDER BY overall_basic DESC;

-- Get reviews by reviewer type (who reviewed whom)
SELECT 
    e.name as employee,
    e.tier_level as emp_tier,
    rev.name as reviewer,
    rev.role as reviewer_role,
    rev.tier_level as rev_tier,
    r.punctuality, 
    r.teamwork, 
    r.productivity,
    r.leadership,
    r.vision,
    r.managerial_skills,
    r.status,
    r.created_date
FROM reviews r
JOIN users e ON r.employee_id = e.id
JOIN users rev ON r.reviewer_id = rev.id
ORDER BY r.created_date DESC;

-- Get draft reviews (pending submission)
SELECT 
    e.name as employee,
    rev.name as reviewer,
    r.comments,
    r.created_date
FROM reviews r
JOIN users e ON r.employee_id = e.id
JOIN users rev ON r.reviewer_id = rev.id
WHERE r.status = 'DRAFT';

-- Top performers (basic scores only)
SELECT 
    u.name,
    u.role,
    u.tier_level,
    ROUND((AVG(r.punctuality) + AVG(r.teamwork) + AVG(r.productivity)) / 3.0, 2) as avg_score,
    COUNT(*) as review_count
FROM users u
JOIN reviews r ON u.id = r.employee_id
WHERE r.status = 'SUBMITTED'
GROUP BY u.id
HAVING COUNT(*) >= 1
ORDER BY avg_score DESC
LIMIT 10;

-- ============================================
-- GOAL QUERIES
-- ============================================

-- Get goals with creator info
SELECT 
    e.name as employee,
    e.tier_level,
    g.description,
    g.status,
    c.name as created_by,
    CASE 
        WHEN c.id = e.id THEN 'Self-created'
        ELSE 'Assigned'
    END as goal_type,
    g.created_date
FROM goals g
JOIN users e ON g.employee_id = e.id
LEFT JOIN users c ON g.creator_id = c.id
ORDER BY g.status, g.created_date DESC;

-- Get active goals for a specific employee
SELECT 
    g.description,
    g.status,
    c.name as assigned_by,
    g.created_date
FROM goals g
JOIN users e ON g.employee_id = e.id
LEFT JOIN users c ON g.creator_id = c.id
WHERE e.username = 'alice_emp' AND g.status = 'ACTIVE';

-- Goal completion statistics by manager
SELECT 
    m.name as manager,
    COUNT(*) as total_goals,
    SUM(CASE WHEN g.status = 'COMPLETED' THEN 1 ELSE 0 END) as completed,
    SUM(CASE WHEN g.status = 'ACTIVE' THEN 1 ELSE 0 END) as active,
    ROUND(SUM(CASE WHEN g.status = 'COMPLETED' THEN 1 ELSE 0 END) * 100.0 / COUNT(*), 1) as completion_rate
FROM goals g
JOIN users e ON g.employee_id = e.id
JOIN users m ON e.manager_id = m.id
GROUP BY m.id
ORDER BY completion_rate DESC;

-- ============================================
-- COURSE QUERIES
-- ============================================

-- List all available courses
SELECT id, name, description, created_date
FROM courses
ORDER BY name;

-- Courses with recommendation count
SELECT 
    c.name,
    c.description,
    COUNT(tr.id) as times_recommended
FROM courses c
LEFT JOIN training_recommendations tr ON c.name = tr.course_name
GROUP BY c.id
ORDER BY times_recommended DESC;

-- ============================================
-- TRAINING RECOMMENDATION QUERIES
-- ============================================

-- Get all training recommendations with names
SELECT 
    r.name as recipient,
    r.tier_level as recipient_tier,
    rec.name as recommender,
    rec.role as recommender_role,
    tr.course_name,
    tr.created_date
FROM training_recommendations tr
JOIN users r ON tr.recipient_id = r.id
LEFT JOIN users rec ON tr.recommender_id = rec.id
ORDER BY tr.created_date DESC;

-- Training recommendations by course popularity
SELECT 
    course_name,
    COUNT(*) as recommendation_count
FROM training_recommendations tr
JOIN users u ON tr.recipient_id = u.id
GROUP BY course_name
ORDER BY recommendation_count DESC;

-- Get recipients for a specific course (alternative to GROUP_CONCAT)
SELECT DISTINCT 
    tr.course_name,
    u.name as recipient
FROM training_recommendations tr
JOIN users u ON tr.recipient_id = u.id
ORDER BY tr.course_name, u.name;

-- Get recommendations for a specific user
SELECT 
    tr.course_name,
    rec.name as recommended_by,
    tr.created_date
FROM training_recommendations tr
JOIN users rec ON tr.recommender_id = rec.id
WHERE tr.recipient_id = (SELECT id FROM users WHERE username = 'david_lead');

-- ============================================
-- PEER REVIEW QUERIES
-- ============================================

-- Get all peer reviews with anonymized format (for display)
SELECT 
    reviewed.name as reviewed_employee,
    reviewed.tier_level,
    'Anonymous Peer' as reviewer,  -- Anonymized
    pr.collaboration_score,
    pr.communication_score,
    pr.teamwork_score,
    ROUND((pr.collaboration_score + pr.communication_score + pr.teamwork_score) / 3.0, 1) as avg_score,
    pr.comments,
    pr.created_date
FROM peer_reviews pr
JOIN users reviewed ON pr.reviewed_id = reviewed.id
ORDER BY pr.created_date DESC;

-- Aggregated peer review scores per employee
SELECT 
    u.name as employee,
    u.tier_level,
    COUNT(pr.id) as peer_review_count,
    ROUND(AVG(pr.collaboration_score), 1) as avg_collaboration,
    ROUND(AVG(pr.communication_score), 1) as avg_communication,
    ROUND(AVG(pr.teamwork_score), 1) as avg_teamwork,
    ROUND((AVG(pr.collaboration_score) + AVG(pr.communication_score) + AVG(pr.teamwork_score)) / 3.0, 2) as overall_peer_score
FROM users u
LEFT JOIN peer_reviews pr ON u.id = pr.reviewed_id
WHERE u.role = 'EMPLOYEE'
GROUP BY u.id
HAVING peer_review_count > 0
ORDER BY overall_peer_score DESC;

-- Peer reviews given BY a specific user (admin view only)
SELECT 
    reviewed.name as reviewed_employee,
    pr.collaboration_score,
    pr.communication_score,
    pr.teamwork_score,
    pr.comments,
    pr.created_date
FROM peer_reviews pr
JOIN users reviewed ON pr.reviewed_id = reviewed.id
WHERE pr.reviewer_id = (SELECT id FROM users WHERE username = 'alice_emp');

-- ============================================
-- UPWARD REVIEW QUERIES
-- ============================================

-- Aggregated upward review scores per manager (anonymous)
SELECT 
    m.name as manager,
    m.role,
    m.tier_level,
    COUNT(ur.id) as feedback_count,
    ROUND(AVG(ur.punctuality_score), 1) as avg_punctuality,
    ROUND(AVG(ur.teamwork_score), 1) as avg_teamwork,
    ROUND(AVG(ur.productivity_score), 1) as avg_productivity,
    ROUND((AVG(ur.punctuality_score) + AVG(ur.teamwork_score) + AVG(ur.productivity_score)) / 3.0, 2) as overall_score
FROM users m
LEFT JOIN upward_reviews ur ON m.id = ur.manager_id
WHERE m.tier_level >= 2  -- Only managers
GROUP BY m.id
HAVING feedback_count > 0
ORDER BY overall_score DESC;

-- Anonymous upward feedback for a specific manager
SELECT 
    'Anonymous Report' as reviewer,  -- Anonymized
    ur.punctuality_score,
    ur.teamwork_score,
    ur.productivity_score,
    ur.comments,
    ur.created_date
FROM upward_reviews ur
WHERE ur.manager_id = (SELECT id FROM users WHERE username = 'sarah_vp')
ORDER BY ur.created_date DESC;

-- Managers with low upward review scores (needs attention)
SELECT 
    m.name as manager,
    m.tier_level,
    COUNT(ur.id) as feedback_count,
    ROUND((AVG(ur.punctuality_score) + AVG(ur.teamwork_score) + AVG(ur.productivity_score)) / 3.0, 2) as overall_score
FROM users m
JOIN upward_reviews ur ON m.id = ur.manager_id
GROUP BY m.id
HAVING overall_score < 3.5  -- Below threshold
ORDER BY overall_score ASC;

-- ============================================
-- PENDING ACTIONS QUERIES
-- ============================================

-- Get all pending actions awaiting approval
SELECT 
    pa.id as request_id,
    req.name as requested_by,
    req.role as requester_role,
    target.name as target_user,
    target.tier_level as target_tier,
    pa.action_type,
    pa.new_value,
    pa.status,
    pa.requested_date
FROM pending_actions pa
JOIN users req ON pa.requested_by_id = req.id
JOIN users target ON pa.target_user_id = target.id
WHERE pa.status = 'PENDING'
ORDER BY pa.requested_date ASC;

-- Get action history (all processed actions)
SELECT 
    pa.id as request_id,
    req.name as requested_by,
    target.name as target_user,
    pa.action_type,
    pa.new_value,
    pa.status,
    pa.rejection_reason,
    proc.name as processed_by,
    pa.processed_date
FROM pending_actions pa
JOIN users req ON pa.requested_by_id = req.id
JOIN users target ON pa.target_user_id = target.id
LEFT JOIN users proc ON pa.processed_by_id = proc.id
WHERE pa.status IN ('APPROVED', 'REJECTED')
ORDER BY pa.processed_date DESC;

-- Pending actions by requester
SELECT 
    req.name as requester,
    pa.action_type,
    COUNT(*) as request_count
FROM pending_actions pa
JOIN users req ON pa.requested_by_id = req.id
GROUP BY req.id, pa.action_type
ORDER BY request_count DESC;

-- ============================================
-- SKILL GAP QUERIES
-- ============================================

-- Get all skill gaps with user info
SELECT 
    u.name as employee,
    u.role,
    u.tier_level,
    sg.punctuality_score,
    sg.teamwork_score,
    sg.productivity_score,
    sg.leadership_score,
    sg.vision_score,
    sg.managerial_score,
    sg.gaps,
    sg.last_updated
FROM skill_gaps sg
JOIN users u ON sg.user_id = u.id
ORDER BY u.tier_level DESC, u.name;

-- Find employees with skill gaps (any score below 6.0 threshold)
SELECT 
    u.name as employee,
    u.tier_level,
    sg.punctuality_score,
    sg.teamwork_score,
    sg.productivity_score,
    sg.gaps
FROM skill_gaps sg
JOIN users u ON sg.user_id = u.id
WHERE sg.gaps IS NOT NULL AND sg.gaps != ''
ORDER BY u.name;

-- Average scores by tier level
SELECT 
    u.tier_level,
    COUNT(DISTINCT u.id) as employee_count,
    ROUND(AVG(sg.punctuality_score), 1) as avg_punct,
    ROUND(AVG(sg.teamwork_score), 1) as avg_team,
    ROUND(AVG(sg.productivity_score), 1) as avg_prod,
    ROUND(AVG(sg.leadership_score), 1) as avg_lead,
    ROUND(AVG(sg.vision_score), 1) as avg_vision,
    ROUND(AVG(sg.managerial_score), 1) as avg_mgmt
FROM users u
JOIN skill_gaps sg ON u.id = sg.user_id
WHERE u.role != 'ADMIN'
GROUP BY u.tier_level
ORDER BY u.tier_level DESC;

-- ============================================
-- CROSS-TABLE STATISTICS QUERIES
-- ============================================

-- Overall system statistics
SELECT 
    'Users' as metric,
    COUNT(*) as count
FROM users WHERE role != 'ADMIN'
UNION ALL
SELECT 
    'Active Users' as metric,
    COUNT(*) as count
FROM users WHERE status = 'ACTIVE' AND role != 'ADMIN'
UNION ALL
SELECT 
    'Reviews' as metric,
    COUNT(*) as count
FROM reviews WHERE status = 'SUBMITTED'
UNION ALL
SELECT 
    'Goals (Active)' as metric,
    COUNT(*) as count
FROM goals WHERE status = 'ACTIVE'
UNION ALL
SELECT 
    'Training Recommendations' as metric,
    COUNT(*) as count
FROM training_recommendations
UNION ALL
SELECT 
    'Peer Reviews' as metric,
    COUNT(*) as count
FROM peer_reviews
UNION ALL
SELECT 
    'Upward Reviews' as metric,
    COUNT(*) as count
FROM upward_reviews
UNION ALL
SELECT 
    'Pending Actions' as metric,
    COUNT(*) as count
FROM pending_actions WHERE status = 'PENDING';

-- Dashboard summary query (user + their stats)
SELECT 
    u.name,
    u.role,
    u.tier_level,
    (SELECT COUNT(*) FROM reviews r WHERE r.employee_id = u.id AND r.status = 'SUBMITTED') as reviews_received,
    (SELECT COUNT(*) FROM reviews r WHERE r.reviewer_id = u.id) as reviews_given,
    (SELECT COUNT(*) FROM goals g WHERE g.employee_id = u.id AND g.status = 'ACTIVE') as active_goals,
    (SELECT COUNT(*) FROM training_recommendations tr WHERE tr.recipient_id = u.id) as training_assigned,
    (SELECT COUNT(*) FROM peer_reviews pr WHERE pr.reviewed_id = u.id) as peer_reviews_received,
    (SELECT COUNT(*) FROM upward_reviews ur WHERE ur.manager_id = u.id) as upward_reviews_received
FROM users u
WHERE u.role != 'ADMIN'
ORDER BY u.tier_level DESC, u.name;
