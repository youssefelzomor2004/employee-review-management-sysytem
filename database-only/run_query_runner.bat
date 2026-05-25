@echo off
REM ============================================
REM Run MySQL Query Runner - No Extensions Needed
REM ============================================
cd /d "%~dp0"

echo.
echo Compiling and running MySQL Query Runner...
echo.

REM Check if MySQL Connector exists
if not exist "lib\mysql-connector-j-8.3.0.jar" (
    if not exist "lib\mysql-connector-java*.jar" (
        echo ============================================
        echo  WARNING: MySQL Connector JAR not found!
        echo ============================================
        echo.
        echo  Please download MySQL Connector/J from:
        echo  https://dev.mysql.com/downloads/connector/j/
        echo.
        echo  Then place the JAR file in the lib folder:
        echo  %~dp0lib\
        echo.
        echo  Press any key to exit...
        pause >nul
        exit /b 1
    )
)

REM Find MySQL connector JAR
for %%f in (lib\mysql-connector*.jar) do set MYSQL_JAR=%%f

REM Compile
echo Compiling QueryRunner.java...
javac -cp "%MYSQL_JAR%" -d . src\com\performance\util\QueryRunner.java 2>nul
if errorlevel 1 (
    echo Compilation failed. Trying alternative...
    javac -cp "lib\*" -d . src\com\performance\util\QueryRunner.java
)

REM Run
echo Starting Query Runner...
echo.
java -cp ".;lib\*" com.performance.util.QueryRunner

pause
