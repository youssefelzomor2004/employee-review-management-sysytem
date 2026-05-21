# Employee Performance Review App

A desktop application designed for managers to evaluate employee performance, store reviews, calculate average scores, identify top performers, and generate comprehensive reports such as year-end summaries, skill gap analysis, and training recommendations.

## Features
- **Store Reviews:** Manage and store detailed employee performance evaluations.
- **Calculate Scores:** Automatically compute average performance scores based on review metrics.
- **Identify Top Performers:** Easily find the best-performing employees within the organization.
- **Generate Reports:** Produce detailed year-end summary reports.
- **Analysis & Recommendations:** Perform skill gap analysis and provide tailored training recommendations.

## Project Structure

This repository contains multiple implementations and components of the Employee Performance Review App:

- **`project/`**: The primary Java implementation utilizing JavaFX for the graphical user interface and SQLite for the database backend.
- **`project-dotnet/`**: A C# implementation built using .NET 8.0 and Windows Presentation Foundation (WPF).
- **`database-only/`**: Contains the database schema, detailed documentation (`DATABASE_DOCUMENTATION.md`), reports, and a standalone database query runner.

## Getting Started

### Java Version (JavaFX)
The Java implementation is located in the `project/` directory.

**Prerequisites:**
- JDK installed on your machine.
- *Note: JavaFX SDK and SQLite JDBC driver are included in the project folder.*

**How to run:**
1. Navigate to the `project/` directory.
2. Execute the `run_javafx.bat` script. This batch file will automatically compile the Java code and launch the application.

### .NET Version (WPF)
The C# WPF implementation is located in the `project-dotnet/PerformanceReview/` directory.

**Prerequisites:**
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed on your machine.

**How to run:**
1. Navigate to the `project-dotnet/PerformanceReview/` directory.
2. Run the application using the .NET CLI:
   ```bash
   dotnet run
   ```
3. Alternatively, you can open the project in Visual Studio and run it from there.

### Database Query Runner
The standalone database runner and related documentation are located in the `database-only/` directory.

**How to run:**
1. Navigate to the `database-only/` directory.
2. Execute the `run_query_runner.bat` script to run database scripts and queries.

## Documentation
For detailed information regarding the database schema, please refer to the `database-only/DATABASE_DOCUMENTATION.md` file or the HTML documentation at `database-only/view_documentation.html`.
