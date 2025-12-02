<strong>**DO NOT DISTRIBUTE OR PUBLICLY POST SOLUTIONS TO THESE LABS. MAKE ALL FORKS OF THIS REPOSITORY WITH SOLUTION CODE PRIVATE. PLEASE REFER TO THE STUDENT CODE OF CONDUCT AND ETHICAL EXPECTATIONS FOR COLLEGE OF INFORMATION TECHNOLOGY STUDENTS FOR SPECIFICS. **</strong>

# Family Medication Management System (FMMS)

## WESTERN GOVERNORS UNIVERSITY 
### D424 – SOFTWARE ENGINEERING CAPSTONE - Task 3

A full-stack cross-platform application for managing family medications, schedules, and medication tracking built with .NET MAUI.

## Project Overview

The Family Medication Management System (FMMS) is a comprehensive medication management application that allows users to:
- Manage family members and their medications
- Create and manage medication schedules (Daily, Interval, Weekly, As-Needed)
- Search across medications, people, and schedules
- Generate comprehensive reports
- Track medication supply levels and expiration dates

## Technology Stack

- **Framework:** .NET 9.0 MAUI (Multi-platform App UI)
- **Database:** SQLite with SQLite-net ORM
- **Testing:** xUnit
- **Platforms:** Windows, Android, iOS, macOS

## Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 (17.8 or later) with MAUI workload, OR
- Visual Studio Code with .NET extensions
- Git (for cloning the repository)

## Getting Started

### Clone the Repository

```bash
git clone <GitLab-Repository-URL>
cd d424-software-engineering-capstone
```

### Build the Project

```bash
dotnet restore
dotnet build
```

### Run the Application

```bash
cd FMMS
dotnet run
```

Or open the solution in Visual Studio and run from the IDE.

### Run Unit Tests

```bash
cd FMMS.Tests
dotnet test
```

## Project Structure

```
FMMS/
├── Models/          # Domain models (Person, Medication, ScheduleRule, etc.)
├── Services/        # Business logic (DatabaseService, SearchService, etc.)
├── ViewModels/      # MVVM ViewModels
├── Views/           # MAUI XAML views
├── Helpers/          # Utility classes (ValidationHelper, etc.)
├── Reports/          # Report generation service
└── Platforms/        # Platform-specific code

FMMS.Tests/
└── *.cs              # Unit test files
```

## Key Features

- **Object-Oriented Design:** Inheritance, polymorphism, and encapsulation
- **Search Functionality:** Multi-entity search with multiple row results
- **Database Operations:** Secure CRUD operations with validation
- **Report Generation:** CSV reports with multiple columns, rows, and timestamps
- **Validation:** Comprehensive input validation
- **Security:** Input sanitization, SQL injection prevention, soft deletes
- **Scalable Architecture:** Service layer, dependency injection, async operations

## Documentation

For detailed documentation including:
- Design documents with class and architecture diagrams
- User guides (setup/maintenance and end-user)
- Test plans and results
- GitLab repository information

Please refer to `task3.docx` in the repository root.

## Testing

The project includes comprehensive unit tests covering:
- Inheritance and polymorphism (ScheduleRule hierarchy)
- Database operations
- Validation logic
- Entity models

Run tests with:
```bash
dotnet test
```

## License

This project is part of the WGU Software Engineering Capstone course. See WGU Academic Authenticity policies.

## Support

For course-specific support, please navigate to the course page and reach out to your course instructor.
