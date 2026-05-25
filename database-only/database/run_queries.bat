@echo off
REM ============================================
REM MySQL Query Runner - No Extensions Needed
REM ============================================
REM Run this batch file to execute MySQL queries
REM 
REM Usage:
REM   run_queries.bat                    - Interactive mode (enter password)
REM   run_queries.bat -p yourpassword    - With password
REM
REM Prerequisites:
REM   - MySQL must be installed and in your PATH
REM   - Or update the MYSQL_PATH variable below
REM ============================================

REM === CONFIGURATION ===
REM Update these settings for your MySQL server
SET MYSQL_USER=root
SET MYSQL_HOST=localhost
SET MYSQL_PORT=3306
SET MYSQL_DATABASE=performance

REM If MySQL is not in PATH, set the full path here:
REM SET MYSQL_PATH="C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"
SET MYSQL_PATH=mysql

REM === MENU ===
:menu
echo.
echo =============================================
echo    Performance Review System - Query Runner
echo =============================================
echo.
echo  1. Run schema.sql (Create tables)
echo  2. Run sample_data.sql (Insert data)
echo  3. Run queries.sql (Execute all queries)
echo  4. Run a specific query file
echo  5. Open MySQL interactive shell
echo  6. Quick test - Show all users
echo  7. Quick test - Show system stats
echo  0. Exit
echo.
set /p choice="Enter your choice: "

if "%choice%"=="1" goto run_schema
if "%choice%"=="2" goto run_sample
if "%choice%"=="3" goto run_queries
if "%choice%"=="4" goto run_custom
if "%choice%"=="5" goto interactive
if "%choice%"=="6" goto quick_users
if "%choice%"=="7" goto quick_stats
if "%choice%"=="0" goto end

echo Invalid choice. Please try again.
goto menu

:run_schema
echo.
echo Running schema.sql...
%MYSQL_PATH% -h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% -p < schema.sql
echo Done!
goto menu

:run_sample
echo.
echo Running sample_data.sql...
%MYSQL_PATH% -h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% -p %MYSQL_DATABASE% < sample_data.sql
echo Done!
goto menu

:run_queries
echo.
echo Running queries.sql...
%MYSQL_PATH% -h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% -p %MYSQL_DATABASE% < queries.sql
echo Done!
goto menu

:run_custom
echo.
set /p sqlfile="Enter SQL file name (e.g., myquery.sql): "
if exist "%sqlfile%" (
    echo Running %sqlfile%...
    %MYSQL_PATH% -h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% -p %MYSQL_DATABASE% < "%sqlfile%"
) else (
    echo File not found: %sqlfile%
)
goto menu

:interactive
echo.
echo Opening MySQL interactive shell...
echo Type 'exit' to return to this menu.
%MYSQL_PATH% -h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% -p %MYSQL_DATABASE%
goto menu

:quick_users
echo.
echo Showing all users...
echo SELECT id, username, name, role, tier_level, status FROM users ORDER BY tier_level DESC; | %MYSQL_PATH% -h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% -p %MYSQL_DATABASE% -t
goto menu

:quick_stats
echo.
echo Showing system statistics...
echo SELECT 'Users' as metric, COUNT(*) as count FROM users WHERE role != 'ADMIN' UNION ALL SELECT 'Active Users', COUNT(*) FROM users WHERE status = 'ACTIVE' AND role != 'ADMIN' UNION ALL SELECT 'Reviews', COUNT(*) FROM reviews WHERE status = 'SUBMITTED' UNION ALL SELECT 'Active Goals', COUNT(*) FROM goals WHERE status = 'ACTIVE' UNION ALL SELECT 'Training Recs', COUNT(*) FROM training_recommendations; | %MYSQL_PATH% -h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% -p %MYSQL_DATABASE% -t
goto menu

:end
echo Goodbye!
exit /b 0
