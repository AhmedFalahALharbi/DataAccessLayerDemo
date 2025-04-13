UserOrderApp - Data Access Layer with Unit Tests
Overview

This project implements a Data Access Layer (DAL) for managing User and Order entities using Entity Framework Core (EF Core) with an in-memory database for unit testing. The DAL includes repositories for CRUD operations, and the unit tests validate these operations using NUnit. The project demonstrates best practices in unit testing, including test isolation, exception handling, and edge-case coverage.
Prerequisites

Before setting up the project, ensure you have the following installed:

    .NET SDK: Version 6.0 or later
    IDE: Visual Studio 2022 (or another IDE like Rider or VS Code)
    NuGet Packages:
        Microsoft.EntityFrameworkCore (for EF Core)
        Microsoft.EntityFrameworkCore.InMemory (for in-memory database)
        NUnit and NUnit3TestAdapter (for unit testing)

Project Structure

The solution consists of two projects:

    UserOrderApp.Data (Class Library):
        Contains the data models, database context, repository interfaces, and implementations.
    UserOrderApp.Tests (Unit Test Project):
        Contains unit tests for the repositories using NUnit and EF Core's in-memory database.

Setup Instructions
Step 1: Create the Solution and Projects

    Open a terminal or command prompt.
    Create the solution and projects:
    bash

dotnet new sln -n UserOrderApp
dotnet new classlib -n UserOrderApp.Data
dotnet new nunit -n UserOrderApp.Tests
dotnet sln UserOrderApp.sln add UserOrderApp.Data/UserOrderApp.Data.csproj
dotnet sln UserOrderApp.sln add UserOrderApp.Tests/UserOrderApp.Tests.csproj
Add a reference to the data project from the test project:
bash

    cd UserOrderApp.Tests
    dotnet add reference ../UserOrderApp.Data/UserOrderApp.Data.csproj

Step 2: Install Required NuGet Packages

In the UserOrderApp.Data project:
bash
dotnet add package Microsoft.EntityFrameworkCore --version 6.0.0
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 6.0.0

In the UserOrderApp.Tests project:
bash
dotnet add package NUnit --version 3.13.3
dotnet add package NUnit3TestAdapter --version 4.5.0
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 6.0.0
Step 3: Implement Data Models and Database Context

    Data Models:
        User: Properties include Id, FirstName, LastName, Email.
        Order: Properties include OrderId, UserId, Product, Quantity, Price.
    Database Context:
        AppDbContext: Inherits from DbContext and includes DbSet<User> and DbSet<Order>.

Step 4: Implement Repositories

    Interfaces:
        IUserRepository: Defines CRUD operations for User.
        IOrderRepository: Defines CRUD operations for Order.
    Implementations:
        UserRepository: Implements IUserRepository using AppDbContext.
        OrderRepository: Implements IOrderRepository using AppDbContext.

Unit Testing
Overview

Unit tests are implemented in the UserOrderApp.Tests project using NUnit. The tests validate CRUD operations for both UserRepository and OrderRepository, including edge cases such as non-existent IDs and invalid inputs.
Test Setup

Each test uses a unique in-memory database instance to ensure isolation. The Setup method initializes a new AppDbContext with a unique database name for each test, and the TearDown method disposes of the context.
Tests Implemented

    Create:
        Verifies that a new entity is correctly added to the database.
        Ensures that passing null throws an ArgumentNullException.
    Read:
        Verifies that an entity can be retrieved by its ID.
        Ensures that a non-existent ID returns null.
    Update:
        Verifies that changes to an entity are persisted correctly.
        Ensures that updating a non-existent entity throws an EntityNotFoundException.
        Ensures that passing null throws an ArgumentNullException.
    Delete:
        Verifies that an entity can be deleted by its ID.
        Ensures that deleting a non-existent entity throws an EntityNotFoundException.

Running the Tests

    Navigate to the test project directory:
    bash

cd UserOrderApp.Tests
Run the tests:
bash

    dotnet test

All tests should pass, confirming the correctness of the CRUD operations and edge-case handling.
Additional Notes
MSSQL Integration

While this project uses an in-memory database for unit testing, to connect to a real MSSQL database in a production environment:

    Install the Microsoft.EntityFrameworkCore.SqlServer package:
    bash

    dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 6.0.0
    Configure the AppDbContext with a connection string in the host application (e.g., a console or web app).

Best Practices Demonstrated

    Test Isolation: Each test uses a unique in-memory database to prevent interference.
    Exception Handling: Repositories throw meaningful exceptions for invalid inputs and non-existent entities.
    Separation of Concerns: Clear separation between models, context, repositories, and tests.
    Comprehensive Testing: Tests cover both successful operations and edge cases, ensuring robustness.
