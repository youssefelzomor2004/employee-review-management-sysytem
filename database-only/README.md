# Database Project

This project contains the SQL database files and Java database manager for the Performance Review System.

## Project Structure

```
database-only/
├── database/
│   ├── schema.sql          # Database table definitions
│   ├── queries.sql         # Common SQL queries
│   └── sample_data.sql     # Sample data for testing
├── lib/
│   └── mysql-connector-j-8.x.x.jar  # MySQL JDBC driver
├── src/
│   └── com/performance/util/
│       └── DatabaseManager.java  # Java database operations
└── README.md
```

## Files Description

### SQL Files (`database/`)

- **schema.sql**: Contains CREATE TABLE statements for:
  - `users` - User accounts and roles
  - `reviews` - Performance reviews
  - `goals` - Employee goals
  - `training_recommendations` - Training recommendations
  - `courses` - Available training courses
  - `peer_reviews` - Anonymous peer reviews
  - `upward_reviews` - Upward reviews of managers
  - `pending_actions` - Manager requests requiring admin approval
  - `skill_gaps` - Cached skill gap analysis

- **queries.sql**: Common SQL queries for data retrieval and manipulation (MySQL 8.0+ compatible)

- **sample_data.sql**: Sample INSERT statements for testing

### Java Files (`src/`)

- **DatabaseManager.java**: Singleton class that handles all database operations including:
  - Connection management
  - User CRUD operations
  - Review management
  - Goal management
  - Course management
  - Training recommendation management

## Dependencies

- MySQL Connector/J 8.x (download from https://dev.mysql.com/downloads/connector/j/)

## Setup

1. Install MySQL Server 8.0 or higher
2. Create a database named `performance`:
   ```sql
   CREATE DATABASE performance;
   ```
3. Update connection settings in `DatabaseManager.java`:
   - `DB_URL`: Your MySQL server URL (default: `jdbc:mysql://localhost:3306/performance`)
   - `DB_USER`: Your MySQL username
   - `DB_PASSWORD`: Your MySQL password
4. Run `schema.sql` to create tables
5. Optionally run `sample_data.sql` to populate test data

## Usage

Include the MySQL Connector/J driver in your classpath and use the `DatabaseManager` class to interact with the database.

