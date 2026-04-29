# LibraryPlatform

A full-stack web platform for browsing, managing, and reviewing online programming libraries.
Built with **ASP.NET Core 8 Minimal API** + **Entity Framework Core** + **SQL Server** + **Vanilla JS / Bootstrap**.

---

## Table of Contents

1. [Project Structure](#project-structure)
2. [Prerequisites](#prerequisites)
3. [Step-by-Step Setup](#step-by-step-setup)
4. [Architecture Overview](#architecture-overview)
5. [Implemented Features](#implemented-features)
6. [Three Complex Tasks](#three-complex-tasks)
7. [API Reference](#api-reference)
8. [Default Accounts](#default-accounts)

---

## Project Structure

```
LibraryPlatform/
├── Properties/
│   └── launchSettings.json         ← Dev launch URLs (ports 5000/5001)
├── Data/
│   └── AppDbContext.cs             ← EF Core DbContext + seed data
├── Models/
│   ├── User.cs                     ← User entity with roles (Guest/User/Admin)
│   ├── LibEntry.cs                 ← Library entry entity
│   ├── Review.cs                   ← Review with 1-5 rating
│   ├── Favorite.cs                 ← User ↔ Library favorites link
│   ├── Tag.cs                      ← Tag + LibraryTag many-to-many join
│   └── Dto/
│       └── Dtos.cs                 ← All DTOs (request/response objects)
├── Services/
│   ├── GitHubService.cs            ← GitHub REST API integration
│   └── ReportService.cs            ← PDF (QuestPDF) + Excel (ClosedXML) generation
├── Endpoints/
│   ├── AuthEndpoints.cs            ← /api/auth/* (register, login, me)
│   ├── LibraryEndpoints.cs         ← /api/libraries/* (CRUD + filtering)
│   ├── ReviewEndpoints.cs          ← /api/libraries/{id}/reviews
│   ├── FavoriteEndpoints.cs        ← /api/favorites/*
│   ├── TagEndpoints.cs             ← /api/tags
│   └── ReportEndpoints.cs          ← /api/reports/* (stats, export)
├── wwwroot/
│   ├── index.html                  ← SPA entry point
│   ├── css/
│   │   └── style.css               ← Full dark-theme custom styles
│   └── js/
│       ├── auth.js                 ← JWT token management (localStorage)
│       ├── api.js                  ← Fetch API wrapper with auth headers
│       └── app.js                  ← SPA router, all page renderers, modals
├── Program.cs                      ← App startup: DI, middleware, endpoint mapping
├── appsettings.json                ← Connection string, JWT config, GitHub token
└── LibraryPlatform.csproj          ← NuGet package references
```

---

## Prerequisites

| Tool                            | Version     | Purpose                           |
|---------------------------------|-------------|-----------------------------------|
| .NET SDK                        | 8.0+        | Runtime and build                 |
| SQL Server (or LocalDB)         | 2019+       | Database                          |
| SQL Server Management Studio    | 19+         | (Optional) Database GUI           |
| Web browser                     | Any modern  | Frontend                          |

---

## Step-by-Step Setup

### Step 1 — Clone / Copy the project

Copy the entire `LibraryPlatform/` folder to your desired location.

### Step 2 — Configure the connection string

Open **`appsettings.json`** and update the `ConnectionStrings.DefaultConnection` value
to match your SQL Server instance:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LibraryPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

- For **LocalDB** (comes with Visual Studio): the default value works as-is.
- For a **named SQL Server instance**: replace with `Server=YOUR_SERVER;Database=LibraryPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true`
- For **SQL Server with login**: use `Server=YOUR_SERVER;Database=LibraryPlatformDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True`

### Step 3 — (Optional) Add a GitHub Personal Access Token

To enable automatic fetching of GitHub stars/issues, open **`appsettings.json`** and set:

```json
"GitHub": {
    "Token": "ghp_YourPersonalAccessTokenHere"
}
```

Without a token the GitHub integration still works, but is limited to 60 requests/hour.
With a token it supports 5,000 requests/hour.

To generate a token: GitHub → Settings → Developer settings → Personal access tokens → Generate new token (classic). No special scopes are required for reading public repos.

### Step 4 — Restore NuGet packages

Open a terminal in the project folder and run:

```bash
dotnet restore
```

### Step 5 — Install EF Core Tools and Create the Migration

Install the EF Core CLI tools globally (only needed once):

```bash
dotnet tool install --global dotnet-ef
```

Then create the initial migration:

```bash
dotnet ef migrations add InitialCreate
```

This generates a `Migrations/` folder with C# code that describes the full database schema.

### Step 6 — Apply the Migration to SQL Server

You have **two options**:

**Option A — Let the app do it automatically (recommended):**

Simply run the application. `Program.cs` calls `db.Database.Migrate()` on startup,
which automatically creates the database, applies all pending migrations, and seeds data:

```bash
dotnet run
```

**Option B — Apply manually via CLI (to see it in SSMS first):**

```bash
dotnet ef database update
```

Then open **SQL Server Management Studio (SSMS)**, connect to your server,
and you will see the `LibraryPlatformDb` database with all tables and seed data.

### Step 7 — Open the application

After running, the app will be available at **https://localhost:5001** and **http://localhost:5000**.

The database will contain:
- 15 tags across 6 categories
- 1 admin user
- 50 sample libraries (React, Django, TensorFlow, Vue.js, Spring Boot, PyTorch, Angular, Laravel, etc.)

### Verifying in SSMS

1. Open SQL Server Management Studio
2. Connect to `(localdb)\mssqllocaldb` (or your configured server)
3. Expand Databases → `LibraryPlatformDb`
4. You should see tables: `Users`, `LibEntries`, `Reviews`, `Favorites`, `Tags`, `LibraryTags`, `__EFMigrationsHistory`
5. Right-click any table → "Select Top 1000 Rows" to see the seed data

---

## Architecture Overview

This project uses a **Monolithic Architecture** — all components live in a single
ASP.NET Core project. This keeps things simple for coursework while still being
well-structured with clear separation of concerns:

```
┌──────────────────────────────────────────────────────┐
│  Frontend (wwwroot/)                                 │
│  Vanilla JS SPA with hash-based routing              │
│  ┌──────────┐ ┌────────┐ ┌──────────┐              │
│  │ auth.js  │ │ api.js │ │  app.js  │              │
│  └──────────┘ └────────┘ └──────────┘              │
├──────────────────────────────────────────────────────┤
│  ASP.NET Core Minimal API (Endpoints/)               │
│  ┌──────┐ ┌──────────┐ ┌────────┐ ┌───────┐       │
│  │ Auth │ │ Library  │ │ Review │ │ Stats │ ...    │
│  └──────┘ └──────────┘ └────────┘ └───────┘       │
├──────────────────────────────────────────────────────┤
│  Services                                            │
│  ┌─────────────────┐ ┌──────────────────┐           │
│  │  GitHubService  │ │  ReportService   │           │
│  └─────────────────┘ └──────────────────┘           │
├──────────────────────────────────────────────────────┤
│  Data Layer                                          │
│  ┌──────────────┐  ┌────────────────────┐           │
│  │ AppDbContext  │  │  Entity Models     │           │
│  └──────────────┘  └────────────────────┘           │
├──────────────────────────────────────────────────────┤
│  SQL Server Database                                 │
└──────────────────────────────────────────────────────┘
```

**Key design decisions:**
- **Minimal API** — no controllers, no boilerplate. Each endpoint group is in its own file.
- **JWT Authentication** — stateless token-based auth with role claims.
- **EF Core** — code-first approach with seed data in `OnModelCreating`.
- **SPA Frontend** — single `index.html` with hash-based routing (`#catalog`, `#detail/5`, `#login`, etc.)

---

## Implemented Features

### 1. Catalog Management (CRUD)
- **Add** a new library with name, version, language, license, repo link, description, and tags
- **View** all libraries in a responsive card grid
- **Edit** existing libraries (owner or admin)
- **Delete** libraries (admin only)

### 2. User and Role System
| Role  | Permissions                                        |
|-------|----------------------------------------------------|
| Guest | View catalog, view details, view reviews            |
| User  | All Guest permissions + add libraries, write reviews, manage favorites |
| Admin | All User permissions + edit/delete any library, delete any review, export reports |

### 3. Search and Filtering (AJAX — no page reload)
- **Search** by library name (with 300ms debounce)
- **Filter** by programming language (dropdown)
- **Filter** by tag/category (dropdown)
- **Sort** by newest, most GitHub stars, or highest rating
- All filters use `fetch()` and update the grid without reloading the page

### 4. Reviews and Rating System
- Users can rate libraries 1-5 stars with a comment
- One review per user per library (enforced in DB + API)
- Average rating and review count displayed on cards

### 5. Personal Dashboard
- View current user profile
- View and manage favorited libraries

---

## Three Complex Tasks

### Complex Task 1 — GitHub API Integration (`Services/GitHubService.cs`)

When a user enters a GitHub repository URL while adding/editing a library, the system
automatically calls the GitHub REST API:

```
GET https://api.github.com/repos/{owner}/{repo}
```

It extracts:
- **Stars** (`stargazers_count`)
- **Open Issues** (`open_issues_count`)
- **Last Updated** (`updated_at`)

This data is stored in the database and displayed on the library detail page.
Admins can manually refresh GitHub data via the "Refresh GitHub Data" button.

**Implementation:** `GitHubService.cs` uses `HttpClient` with `System.Text.Json`
to parse the GitHub API response. URL parsing extracts `owner/repo` from any
valid GitHub URL format.

### Complex Task 2 — Dynamic Filtering Without Page Reload (`wwwroot/js/app.js`)

The catalog page uses **AJAX (Fetch API)** for instant filtering:

1. User changes a filter (search input, language dropdown, tag dropdown, sort)
2. JavaScript reads all current filter values
3. An async `fetch()` call hits `GET /api/libraries?search=...&language=...&tagId=...&sort=...`
4. The server-side query dynamically builds an EF Core LINQ expression
5. Only the card grid and pagination are re-rendered (the rest of the page stays intact)
6. A 300ms debounce on the search input prevents excessive API calls

**Server-side:** `LibraryEndpoints.cs` builds the query conditionally:
```csharp
if (!string.IsNullOrWhiteSpace(search))
    query = query.Where(l => l.Name.Contains(search));
if (!string.IsNullOrWhiteSpace(language))
    query = query.Where(l => l.Language == language);
```

### Complex Task 3 — Reporting and Statistics (`Services/ReportService.cs`)

The Statistics page provides:
- **Summary cards** showing total libraries, users, and reviews
- **Chart.js bar chart** showing the distribution of libraries by programming language
- **Excel export** (Admin only) — generated server-side using **ClosedXML** with formatted headers
- **PDF export** (Admin only) — generated server-side using **QuestPDF** with a styled table layout

Both exports are served as downloadable files via `Results.File()`.

---

## API Reference

| Method | Endpoint                              | Auth    | Description                           |
|--------|---------------------------------------|---------|---------------------------------------|
| POST   | /api/auth/register                    | —       | Create new account                    |
| POST   | /api/auth/login                       | —       | Login, returns JWT                    |
| GET    | /api/auth/me                          | Token   | Get current user profile              |
| GET    | /api/libraries                        | —       | List with search/filter/sort/paging   |
| GET    | /api/libraries/{id}                   | —       | Single library details                |
| POST   | /api/libraries                        | User+   | Create new library                    |
| PUT    | /api/libraries/{id}                   | Owner/Admin | Update library                    |
| DELETE | /api/libraries/{id}                   | Admin   | Delete library                        |
| GET    | /api/libraries/languages              | —       | Distinct language list                |
| POST   | /api/libraries/{id}/refresh-github    | User+   | Re-fetch GitHub stats                 |
| GET    | /api/libraries/{libId}/reviews        | —       | List reviews for a library            |
| POST   | /api/libraries/{libId}/reviews        | User+   | Submit a review                       |
| DELETE | /api/reviews/{id}                     | Author/Admin | Delete a review                  |
| GET    | /api/favorites                        | User+   | Get user's favorited libraries        |
| GET    | /api/favorites/ids                    | User+   | Get IDs only (for UI state)           |
| POST   | /api/favorites/{libId}                | User+   | Add to favorites                      |
| DELETE | /api/favorites/{libId}                | User+   | Remove from favorites                 |
| GET    | /api/tags                             | —       | All available tags                    |
| GET    | /api/reports/stats                    | —       | Platform statistics (for charts)      |
| GET    | /api/reports/export/excel             | Admin   | Download Excel report                 |
| GET    | /api/reports/export/pdf               | Admin   | Download PDF report                   |

---

## Default Accounts

| Role  | Email                        | Password   |
|-------|------------------------------|------------|
| Admin | admin@libraryplatform.com    | Admin123!  |

New users registered through the UI are assigned the **User** role.

---

## Technology Stack Summary

| Layer      | Technology              | Purpose                                  |
|------------|-------------------------|------------------------------------------|
| Runtime    | .NET 8                  | Platform                                 |
| Backend    | ASP.NET Core Minimal API| Concise, no-boilerplate endpoints         |
| ORM        | Entity Framework Core 8 | Database interaction via C# objects       |
| Database   | SQL Server / LocalDB    | Relational data storage                   |
| Auth       | JWT Bearer Tokens       | Stateless authentication                  |
| Passwords  | BCrypt.Net              | Secure password hashing                   |
| PDF Export | QuestPDF                | Server-side PDF generation                |
| Excel Export| ClosedXML              | Server-side .xlsx generation              |
| Frontend   | Vanilla JS              | Single-page application                   |
| Charts     | Chart.js 4              | Statistics visualization                  |
| Styling    | Custom CSS (dark theme) | Polished UI without framework overhead    |
