# Library Management System

A full-featured, role-based **Library Management System** built with **ASP.NET Core MVC (.NET 10)** and **SQL Server**. It covers the day-to-day work of a library: cataloguing books, managing members and staff, borrowing and returns, fines, book purchases, and online reading access, with an AI-powered chat assistant and AI-assisted borrowing approval.

## Features

- **Catalogue management**: books (with cover images), authors, and categories
- **Role-based access control** with four roles: `Admin`, `Librarian`, `Staff`, `Member`
- **Member registration with approval**: new sign-ups wait for a Librarian/Admin to approve them
- **Borrowing workflow**: members request books, staff approve and issue them, returns are tracked
- **AI-assisted approval**: borrowing requests are evaluated using the member's history (active loans, overdue books, unpaid fines)
- **Fines**: overdue tracking and fine payment
- **Purchases & receipts**: members can buy books, with printable receipts
- **Online reading**: time-limited online access to books
- **AI chat assistant** and **AI book suggestions** (Groq API)
- **Reports** for Admin/Librarian
- Profile management and password change

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC, .NET 10 |
| Database | SQL Server, Entity Framework Core (Code First migrations) |
| Auth | ASP.NET Core Identity (role-based) |
| Frontend | Razor views, Bootstrap, jQuery |
| AI | Groq API |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server or SQL Server Express
- (Optional) a [Groq API key](https://console.groq.com) for the AI features

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/MashrubaTasnim/LibraryManagementSystem.git
   cd LibraryManagementSystem
   ```

2. **Set the database connection string** in `appsettings.json` (default is `localhost\SQLEXPRESS`), or keep it out of the repo with user-secrets:
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=LibraryManagementSystemDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   ```

3. **Add your Groq API key** (never commit it):
   ```bash
   dotnet user-secrets set "Groq:ApiKey" "YOUR_GROQ_API_KEY"
   ```
   Without a key the app still runs; the AI features are skipped.

4. **Create the database**
   ```bash
   dotnet ef database update
   ```

5. **Run**
   ```bash
   dotnet run
   ```
   Open `https://localhost:7030` (or `http://localhost:5288`).

### First admin account

The four roles are created automatically on first run, but no admin user is seeded. Register a normal account, then promote it in SQL Server:

```sql
-- 1. find the user and the Admin role
SELECT Id, Email FROM AspNetUsers;
SELECT Id FROM AspNetRoles WHERE Name = 'Admin';

-- 2. assign the role and approve the account
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('<user-id>', '<admin-role-id>');
UPDATE AspNetUsers SET IsApproved = 1 WHERE Id = '<user-id>';
```

After that, the Admin can create Librarian, Staff, and Member accounts from the app.

## Project Structure

```
Controllers/   MVC controllers (Books, Borrowings, Fines, Purchases, Reports, Chat, ...)
Models/        Entity models (Book, Author, Borrowing, Fine, Purchase, ...)
ViewModels/    View-specific models
Services/      AI approval and book suggestion services
Data/          EF Core DbContext
Migrations/    Database migrations
Views/         Razor views
wwwroot/       Static files (CSS, JS, images)
```

## Screenshots

<!-- Add screenshots to a `screenshots/` folder, then uncomment and edit:
![Home](screenshots/home.png)
![Borrowing](screenshots/borrowing.png)
-->

_Screenshots coming soon._

## Author

**Mashruba Tasnem**
GitHub: [@MashrubaTasnim](https://github.com/MashrubaTasnim)  
Email: mashruba.cse@gmail.com
