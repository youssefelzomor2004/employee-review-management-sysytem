package com.performance.util;

import java.io.*;
import java.sql.*;
import java.util.Scanner;

/**
 * Standalone MySQL Query Runner - No extensions needed!
 * Run SQL queries directly from the command line.
 */
public class QueryRunner {
    // MySQL connection settings - update as needed
    private static final String DB_URL = "jdbc:mysql://localhost:3306/performance";
    private static final String DB_USER = "root";
    private static final String DB_PASSWORD = ""; // Update with your password

    private Connection connection;
    private Scanner scanner;

    public QueryRunner() {
        scanner = new Scanner(System.in);
    }

    public boolean connect() {
        try {
            Class.forName("com.mysql.cj.jdbc.Driver");
            connection = DriverManager.getConnection(DB_URL, DB_USER, DB_PASSWORD);
            System.out.println("✓ Connected to MySQL database successfully!");
            return true;
        } catch (ClassNotFoundException e) {
            System.err.println("✗ MySQL JDBC Driver not found. Make sure mysql-connector-j.jar is in classpath.");
            return false;
        } catch (SQLException e) {
            System.err.println("✗ Connection failed: " + e.getMessage());
            return false;
        }
    }

    public void showMenu() {
        System.out.println("\n╔════════════════════════════════════════════════════╗");
        System.out.println("║     Performance Review System - Query Runner       ║");
        System.out.println("╠════════════════════════════════════════════════════╣");
        System.out.println("║  1. Run schema.sql (Create tables)                 ║");
        System.out.println("║  2. Run sample_data.sql (Insert sample data)       ║");
        System.out.println("║  3. Run queries.sql (Execute all queries)          ║");
        System.out.println("║  4. Run custom SQL file                            ║");
        System.out.println("║  5. Enter SQL queries manually                     ║");
        System.out.println("║  6. Quick: Show all users                          ║");
        System.out.println("║  7. Quick: Show system statistics                  ║");
        System.out.println("║  8. Quick: Show top performers                     ║");
        System.out.println("║  0. Exit                                           ║");
        System.out.println("╚════════════════════════════════════════════════════╝");
        System.out.print("\nEnter your choice: ");
    }

    public void run() {
        if (!connect()) {
            System.out.println("\nPlease check your MySQL server and connection settings.");
            return;
        }

        boolean running = true;
        while (running) {
            showMenu();
            String choice = scanner.nextLine().trim();

            switch (choice) {
                case "1" -> runSqlFile("database/schema.sql");
                case "2" -> runSqlFile("database/sample_data.sql");
                case "3" -> runSqlFile("database/queries.sql");
                case "4" -> runCustomFile();
                case "5" -> interactiveMode();
                case "6" -> quickShowUsers();
                case "7" -> quickShowStats();
                case "8" -> quickShowTopPerformers();
                case "0" -> running = false;
                default -> System.out.println("Invalid choice. Please try again.");
            }
        }

        disconnect();
        System.out.println("Goodbye!");
    }

    private void runSqlFile(String filename) {
        File file = new File(filename);
        if (!file.exists()) {
            System.out.println("✗ File not found: " + filename);
            return;
        }

        System.out.println("\n▶ Running: " + filename);
        System.out.println("─".repeat(60));

        try (BufferedReader reader = new BufferedReader(new FileReader(file))) {
            StringBuilder sql = new StringBuilder();
            String line;
            int queryCount = 0;

            while ((line = reader.readLine()) != null) {
                line = line.trim();

                // Skip comments and empty lines
                if (line.isEmpty() || line.startsWith("--")) {
                    if (line.startsWith("-- ==")) {
                        System.out.println("\n" + line);
                    }
                    continue;
                }

                sql.append(line).append(" ");

                // Execute when we hit a semicolon
                if (line.endsWith(";")) {
                    String query = sql.toString().trim();
                    query = query.substring(0, query.length() - 1); // Remove semicolon

                    if (!query.isEmpty()) {
                        executeQuery(query);
                        queryCount++;
                    }
                    sql = new StringBuilder();
                }
            }

            System.out.println("\n─".repeat(60));
            System.out.println("✓ Executed " + queryCount + " queries from " + filename);

        } catch (IOException e) {
            System.err.println("✗ Error reading file: " + e.getMessage());
        }

        pause();
    }

    private void runCustomFile() {
        System.out.print("Enter SQL file path: ");
        String filename = scanner.nextLine().trim();
        runSqlFile(filename);
    }

    private void interactiveMode() {
        System.out.println("\n┌─────────────────────────────────────────────────────┐");
        System.out.println("│             Interactive SQL Mode                     │");
        System.out.println("│  Type SQL queries and press Enter to execute.        │");
        System.out.println("│  Type 'exit' to return to main menu.                 │");
        System.out.println("└─────────────────────────────────────────────────────┘\n");

        StringBuilder multiLine = new StringBuilder();

        while (true) {
            System.out.print(multiLine.isEmpty() ? "mysql> " : "    -> ");
            String line = scanner.nextLine();

            if (line.trim().equalsIgnoreCase("exit")) {
                break;
            }

            multiLine.append(line).append(" ");

            if (line.trim().endsWith(";")) {
                String query = multiLine.toString().trim();
                query = query.substring(0, query.length() - 1);
                executeQuery(query);
                multiLine = new StringBuilder();
            }
        }
    }

    private void executeQuery(String sql) {
        try {
            Statement stmt = connection.createStatement();

            // Check if it's a SELECT query
            if (sql.trim().toUpperCase().startsWith("SELECT") ||
                    sql.trim().toUpperCase().startsWith("SHOW") ||
                    sql.trim().toUpperCase().startsWith("DESCRIBE") ||
                    sql.trim().toUpperCase().startsWith("WITH")) {

                ResultSet rs = stmt.executeQuery(sql);
                printResultSet(rs);
                rs.close();
            } else {
                int affected = stmt.executeUpdate(sql);
                System.out.println("✓ Query OK, " + affected + " row(s) affected");
            }

            stmt.close();
        } catch (SQLException e) {
            System.err.println("✗ SQL Error: " + e.getMessage());
        }
    }

    private void printResultSet(ResultSet rs) throws SQLException {
        ResultSetMetaData meta = rs.getMetaData();
        int columnCount = meta.getColumnCount();

        // Calculate column widths
        int[] widths = new int[columnCount];
        String[] headers = new String[columnCount];

        for (int i = 1; i <= columnCount; i++) {
            headers[i - 1] = meta.getColumnLabel(i);
            widths[i - 1] = Math.max(headers[i - 1].length(), 10);
        }

        // Print header
        System.out.println();
        printRow(headers, widths);
        printSeparator(widths);

        // Print rows
        int rowCount = 0;
        while (rs.next()) {
            String[] values = new String[columnCount];
            for (int i = 1; i <= columnCount; i++) {
                String value = rs.getString(i);
                values[i - 1] = value != null ? value : "NULL";
                widths[i - 1] = Math.max(widths[i - 1], Math.min(values[i - 1].length(), 40));
            }
            printRow(values, widths);
            rowCount++;
        }

        System.out.println("\n" + rowCount + " row(s) returned");
    }

    private void printRow(String[] values, int[] widths) {
        StringBuilder row = new StringBuilder("│ ");
        for (int i = 0; i < values.length; i++) {
            String value = values[i];
            if (value.length() > 40)
                value = value.substring(0, 37) + "...";
            row.append(String.format("%-" + widths[i] + "s", value)).append(" │ ");
        }
        System.out.println(row);
    }

    private void printSeparator(int[] widths) {
        StringBuilder sep = new StringBuilder("├─");
        for (int width : widths) {
            sep.append("─".repeat(width)).append("─┼─");
        }
        sep.setLength(sep.length() - 2);
        sep.append("┤");
        System.out.println(sep);
    }

    private void quickShowUsers() {
        System.out.println("\n▶ All Users");
        executeQuery("SELECT id, username, name, role, tier_level, status FROM users ORDER BY tier_level DESC, name");
        pause();
    }

    private void quickShowStats() {
        System.out.println("\n▶ System Statistics");
        executeQuery("""
                SELECT 'Total Users' as Metric, COUNT(*) as Count FROM users WHERE role != 'ADMIN'
                UNION ALL SELECT 'Active Users', COUNT(*) FROM users WHERE status = 'ACTIVE' AND role != 'ADMIN'
                UNION ALL SELECT 'Reviews', COUNT(*) FROM reviews WHERE status = 'SUBMITTED'
                UNION ALL SELECT 'Active Goals', COUNT(*) FROM goals WHERE status = 'ACTIVE'
                UNION ALL SELECT 'Training Recs', COUNT(*) FROM training_recommendations
                """);
        pause();
    }

    private void quickShowTopPerformers() {
        System.out.println("\n▶ Top 10 Performers");
        executeQuery("""
                SELECT u.name, u.role, u.tier_level,
                       ROUND((AVG(r.punctuality) + AVG(r.teamwork) + AVG(r.productivity)) / 3.0, 2) as avg_score,
                       COUNT(*) as review_count
                FROM users u
                JOIN reviews r ON u.id = r.employee_id
                WHERE r.status = 'SUBMITTED'
                GROUP BY u.id
                HAVING COUNT(*) >= 1
                ORDER BY avg_score DESC
                LIMIT 10
                """);
        pause();
    }

    private void pause() {
        System.out.print("\nPress Enter to continue...");
        scanner.nextLine();
    }

    private void disconnect() {
        try {
            if (connection != null && !connection.isClosed()) {
                connection.close();
                System.out.println("✓ Disconnected from database");
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
    }

    public static void main(String[] args) {
        System.out.println("\n");
        System.out.println("╔═══════════════════════════════════════════════════════╗");
        System.out.println("║   MySQL Query Runner - Performance Review System      ║");
        System.out.println("║   No VS Code extensions needed!                       ║");
        System.out.println("╚═══════════════════════════════════════════════════════╝");

        QueryRunner runner = new QueryRunner();
        runner.run();
    }
}
