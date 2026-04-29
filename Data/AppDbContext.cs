using Microsoft.EntityFrameworkCore;
using LibraryPlatform.Models;

namespace LibraryPlatform.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<LibEntry> LibEntries => Set<LibEntry>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<LibraryTag> LibraryTags => Set<LibraryTag>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<LibraryTag>()
            .HasKey(lt => new { lt.LibEntryId, lt.TagId });

        mb.Entity<Favorite>()
            .HasIndex(f => new { f.UserId, f.LibEntryId })
            .IsUnique();

        mb.Entity<Review>()
            .HasIndex(r => new { r.UserId, r.LibEntryId })
            .IsUnique();

        mb.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        mb.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Web", Category = "Web" },
            new Tag { Id = 2, Name = "REST API", Category = "Web" },
            new Tag { Id = 3, Name = "Frontend", Category = "Web" },
            new Tag { Id = 4, Name = "Game Dev", Category = "Game Dev" },
            new Tag { Id = 5, Name = "2D Graphics", Category = "Game Dev" },
            new Tag { Id = 6, Name = "3D Engine", Category = "Game Dev" },
            new Tag { Id = 7, Name = "Data Science", Category = "Data Science" },
            new Tag { Id = 8, Name = "Machine Learning", Category = "Data Science" },
            new Tag { Id = 9, Name = "Visualization", Category = "Data Science" },
            new Tag { Id = 10, Name = "Utility", Category = "General" },
            new Tag { Id = 11, Name = "Testing", Category = "General" },
            new Tag { Id = 12, Name = "Security", Category = "General" },
            new Tag { Id = 13, Name = "Database", Category = "General" },
            new Tag { Id = 14, Name = "Mobile", Category = "Mobile" },
            new Tag { Id = 15, Name = "DevOps", Category = "DevOps" }
        );

        mb.Entity<User>().HasData(new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@libraryplatform.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = UserRole.Admin,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        mb.Entity<LibEntry>().HasData(
            new LibEntry { Id = 1, Name = "React", Version = "18.2.0", Language = "JavaScript", LicenseType = "MIT", RepositoryLink = "https://github.com/facebook/react", Description = "A JavaScript library for building user interfaces with a component-based architecture", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 2, Name = "Django", Version = "5.0", Language = "Python", LicenseType = "BSD-3", RepositoryLink = "https://github.com/django/django", Description = "High-level Python web framework that encourages rapid development and clean, pragmatic design", CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 3, Name = "Entity Framework Core", Version = "8.0", Language = "C#", LicenseType = "MIT", RepositoryLink = "https://github.com/dotnet/efcore", Description = "Modern object-database mapper for .NET supporting LINQ queries and migrations", CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 4, Name = "TensorFlow", Version = "2.15.0", Language = "Python", LicenseType = "Apache-2.0", RepositoryLink = "https://github.com/tensorflow/tensorflow", Description = "An end-to-end open-source platform for machine learning and neural networks", CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 5, Name = "Unity", Version = "2023.3", Language = "C#", LicenseType = "Proprietary", RepositoryLink = "https://github.com/Unity-Technologies/UnityCsReference", Description = "Cross-platform game engine for 2D and 3D game development", CreatedAt = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 6, Name = "Vue.js", Version = "3.4.15", Language = "JavaScript", LicenseType = "MIT", RepositoryLink = "https://github.com/vuejs/core", Description = "Progressive JavaScript framework for building modern web interfaces", CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 7, Name = "Spring Boot", Version = "3.2.1", Language = "Java", LicenseType = "Apache-2.0", RepositoryLink = "https://github.com/spring-projects/spring-boot", Description = "Production-grade Spring-based applications framework with embedded server support", CreatedAt = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 8, Name = "Flask", Version = "3.0.1", Language = "Python", LicenseType = "BSD-3", RepositoryLink = "https://github.com/pallets/flask", Description = "Lightweight WSGI web application framework designed for quick and easy development", CreatedAt = new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 9, Name = "PyTorch", Version = "2.1.2", Language = "Python", LicenseType = "BSD-3", RepositoryLink = "https://github.com/pytorch/pytorch", Description = "Open source machine learning framework with dynamic computational graphs", CreatedAt = new DateTime(2024, 1, 9, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 10, Name = "Express.js", Version = "4.18.2", Language = "JavaScript", LicenseType = "MIT", RepositoryLink = "https://github.com/expressjs/express", Description = "Fast, unopinionated, minimalist web framework for Node.js", CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 11, Name = "Angular", Version = "17.1.0", Language = "TypeScript", LicenseType = "MIT", RepositoryLink = "https://github.com/angular/angular", Description = "Platform for building mobile and desktop web applications using TypeScript", CreatedAt = new DateTime(2024, 1, 11, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 12, Name = "Pandas", Version = "2.1.4", Language = "Python", LicenseType = "BSD-3", RepositoryLink = "https://github.com/pandas-dev/pandas", Description = "Powerful data analysis and manipulation library for Python", CreatedAt = new DateTime(2024, 1, 12, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 13, Name = "Godot Engine", Version = "4.2.1", Language = "C++", LicenseType = "MIT", RepositoryLink = "https://github.com/godotengine/godot", Description = "Multi-platform 2D and 3D open-source game engine with intuitive scene system", CreatedAt = new DateTime(2024, 1, 13, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 14, Name = "Next.js", Version = "14.1.0", Language = "JavaScript", LicenseType = "MIT", RepositoryLink = "https://github.com/vercel/next.js", Description = "Full-stack React framework with server-side rendering and static site generation", CreatedAt = new DateTime(2024, 1, 14, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 15, Name = "Scikit-learn", Version = "1.4.0", Language = "Python", LicenseType = "BSD-3", RepositoryLink = "https://github.com/scikit-learn/scikit-learn", Description = "Machine learning library for classification, regression, clustering, and dimensionality reduction", CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 16, Name = "Gin", Version = "1.9.1", Language = "Go", LicenseType = "MIT", RepositoryLink = "https://github.com/gin-gonic/gin", Description = "High-performance HTTP web framework for Go with a Martini-like API", CreatedAt = new DateTime(2024, 1, 16, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 17, Name = "Ruby on Rails", Version = "7.1.3", Language = "Ruby", LicenseType = "MIT", RepositoryLink = "https://github.com/rails/rails", Description = "Full-stack web framework optimized for developer happiness and sustainable productivity", CreatedAt = new DateTime(2024, 1, 17, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 18, Name = "D3.js", Version = "7.8.5", Language = "JavaScript", LicenseType = "ISC", RepositoryLink = "https://github.com/d3/d3", Description = "Data-driven documents library for creating dynamic, interactive data visualizations in the browser", CreatedAt = new DateTime(2024, 1, 18, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 19, Name = "Laravel", Version = "11.0", Language = "PHP", LicenseType = "MIT", RepositoryLink = "https://github.com/laravel/laravel", Description = "Elegant PHP framework for artisans with expressive syntax and powerful tools", CreatedAt = new DateTime(2024, 1, 19, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 20, Name = "Tokio", Version = "1.35.1", Language = "Rust", LicenseType = "MIT", RepositoryLink = "https://github.com/tokio-rs/tokio", Description = "Asynchronous runtime for Rust providing event-driven, non-blocking I/O", CreatedAt = new DateTime(2024, 1, 20, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 21, Name = "Svelte", Version = "4.2.8", Language = "JavaScript", LicenseType = "MIT", RepositoryLink = "https://github.com/sveltejs/svelte", Description = "Cybernetically enhanced web apps with compile-time framework approach", CreatedAt = new DateTime(2024, 1, 21, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 22, Name = "FastAPI", Version = "0.109.0", Language = "Python", LicenseType = "MIT", RepositoryLink = "https://github.com/tiangolo/fastapi", Description = "Modern high-performance web framework for building APIs with Python type hints", CreatedAt = new DateTime(2024, 1, 22, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 23, Name = "Dapper", Version = "2.1.28", Language = "C#", LicenseType = "Apache-2.0", RepositoryLink = "https://github.com/DapperLib/Dapper", Description = "Simple lightweight high-performance micro-ORM for .NET", CreatedAt = new DateTime(2024, 1, 23, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 24, Name = "Actix Web", Version = "4.4.1", Language = "Rust", LicenseType = "MIT", RepositoryLink = "https://github.com/actix/actix-web", Description = "Powerful and fast web framework for Rust with actor-based architecture", CreatedAt = new DateTime(2024, 1, 24, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 25, Name = "Keras", Version = "3.0.4", Language = "Python", LicenseType = "Apache-2.0", RepositoryLink = "https://github.com/keras-team/keras", Description = "Deep learning API providing high-level building blocks for neural networks", CreatedAt = new DateTime(2024, 1, 25, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 26, Name = "Electron", Version = "28.1.4", Language = "JavaScript", LicenseType = "MIT", RepositoryLink = "https://github.com/electron/electron", Description = "Build cross-platform desktop apps with JavaScript, HTML, and CSS", CreatedAt = new DateTime(2024, 1, 26, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 27, Name = "Hibernate", Version = "6.4.2", Language = "Java", LicenseType = "LGPL-2.1", RepositoryLink = "https://github.com/hibernate/hibernate-orm", Description = "Object-relational mapping framework for Java with powerful query language", CreatedAt = new DateTime(2024, 1, 27, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 28, Name = "Tailwind CSS", Version = "3.4.1", Language = "JavaScript", LicenseType = "MIT", RepositoryLink = "https://github.com/tailwindlabs/tailwindcss", Description = "Utility-first CSS framework for rapidly building custom user interfaces", CreatedAt = new DateTime(2024, 1, 28, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 29, Name = "NumPy", Version = "1.26.3", Language = "Python", LicenseType = "BSD-3", RepositoryLink = "https://github.com/numpy/numpy", Description = "Fundamental package for scientific computing with multi-dimensional arrays", CreatedAt = new DateTime(2024, 1, 29, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 30, Name = "Nest.js", Version = "10.3.0", Language = "TypeScript", LicenseType = "MIT", RepositoryLink = "https://github.com/nestjs/nest", Description = "Progressive Node.js framework for building efficient server-side applications", CreatedAt = new DateTime(2024, 1, 30, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 31, Name = "Symfony", Version = "7.0.2", Language = "PHP", LicenseType = "MIT", RepositoryLink = "https://github.com/symfony/symfony", Description = "Set of reusable PHP components and a full-stack web framework", CreatedAt = new DateTime(2024, 1, 31, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 32, Name = "Unreal Engine", Version = "5.3", Language = "C++", LicenseType = "Proprietary", RepositoryLink = "https://github.com/EpicGames/UnrealEngine", Description = "Industry-leading game engine for photorealistic visuals and immersive experiences", CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 33, Name = "Matplotlib", Version = "3.8.2", Language = "Python", LicenseType = "PSF", RepositoryLink = "https://github.com/matplotlib/matplotlib", Description = "Comprehensive library for creating static, animated, and interactive visualizations in Python", CreatedAt = new DateTime(2024, 2, 2, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 34, Name = "Rocket", Version = "0.5.0", Language = "Rust", LicenseType = "MIT", RepositoryLink = "https://github.com/rwf2/Rocket", Description = "Web framework for Rust with focus on ease of use, security, and speed", CreatedAt = new DateTime(2024, 2, 3, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 35, Name = "Redux", Version = "5.0.1", Language = "JavaScript", LicenseType = "MIT", RepositoryLink = "https://github.com/reduxjs/redux", Description = "Predictable state container for JavaScript applications", CreatedAt = new DateTime(2024, 2, 4, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 36, Name = "Sinatra", Version = "4.0.0", Language = "Ruby", LicenseType = "MIT", RepositoryLink = "https://github.com/sinatra/sinatra", Description = "Lightweight DSL for quickly creating web applications in Ruby", CreatedAt = new DateTime(2024, 2, 5, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 37, Name = "Kotlin Coroutines", Version = "1.8.0", Language = "Java", LicenseType = "Apache-2.0", RepositoryLink = "https://github.com/Kotlin/kotlinx.coroutines", Description = "Library support for Kotlin coroutines with multiplatform async programming", CreatedAt = new DateTime(2024, 2, 6, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 38, Name = "SignalR", Version = "8.0", Language = "C#", LicenseType = "MIT", RepositoryLink = "https://github.com/dotnet/aspnetcore", Description = "Real-time web functionality library enabling server-side push to connected clients", CreatedAt = new DateTime(2024, 2, 7, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 39, Name = "Three.js", Version = "0.161.0", Language = "JavaScript", LicenseType = "MIT", RepositoryLink = "https://github.com/mrdoob/three.js", Description = "JavaScript 3D library for creating and displaying animated 3D graphics in the browser", CreatedAt = new DateTime(2024, 2, 8, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 40, Name = "Celery", Version = "5.3.6", Language = "Python", LicenseType = "BSD-3", RepositoryLink = "https://github.com/celery/celery", Description = "Distributed task queue for real-time processing and scheduling in Python", CreatedAt = new DateTime(2024, 2, 9, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 41, Name = "Fiber", Version = "2.52.0", Language = "Go", LicenseType = "MIT", RepositoryLink = "https://github.com/gofiber/fiber", Description = "Express-inspired web framework built on top of Fasthttp for Go", CreatedAt = new DateTime(2024, 2, 10, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 42, Name = "Prisma", Version = "5.8.1", Language = "TypeScript", LicenseType = "Apache-2.0", RepositoryLink = "https://github.com/prisma/prisma", Description = "Next-generation ORM for Node.js and TypeScript with auto-generated query builder", CreatedAt = new DateTime(2024, 2, 11, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 43, Name = "Selenium", Version = "4.17.0", Language = "Python", LicenseType = "Apache-2.0", RepositoryLink = "https://github.com/SeleniumHQ/selenium", Description = "Browser automation framework for web testing across multiple browsers", CreatedAt = new DateTime(2024, 2, 12, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 44, Name = "Phoenix", Version = "1.7.10", Language = "Ruby", LicenseType = "MIT", RepositoryLink = "https://github.com/phoenixframework/phoenix", Description = "Productive web framework for Elixir that delivers fast response times", CreatedAt = new DateTime(2024, 2, 13, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 45, Name = "OpenCV", Version = "4.9.0", Language = "C++", LicenseType = "Apache-2.0", RepositoryLink = "https://github.com/opencv/opencv", Description = "Open source computer vision and machine learning software library", CreatedAt = new DateTime(2024, 2, 14, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 46, Name = "Socket.IO", Version = "4.7.4", Language = "JavaScript", LicenseType = "MIT", RepositoryLink = "https://github.com/socketio/socket.io", Description = "Bidirectional event-based real-time communication library for web applications", CreatedAt = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 47, Name = "Blazor", Version = "8.0", Language = "C#", LicenseType = "MIT", RepositoryLink = "https://github.com/dotnet/aspnetcore", Description = "Framework for building interactive web UIs using C# instead of JavaScript", CreatedAt = new DateTime(2024, 2, 16, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 48, Name = "Mongoose", Version = "8.1.1", Language = "JavaScript", LicenseType = "MIT", RepositoryLink = "https://github.com/Automattic/mongoose", Description = "Elegant MongoDB object modeling for Node.js with schema validation", CreatedAt = new DateTime(2024, 2, 17, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 49, Name = "Scrapy", Version = "2.11.0", Language = "Python", LicenseType = "BSD-3", RepositoryLink = "https://github.com/scrapy/scrapy", Description = "Fast high-level web crawling and web scraping framework for Python", CreatedAt = new DateTime(2024, 2, 18, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 },
            new LibEntry { Id = 50, Name = "Echo", Version = "4.11.4", Language = "Go", LicenseType = "MIT", RepositoryLink = "https://github.com/labstack/echo", Description = "High performance minimalist Go web framework with extensible middleware", CreatedAt = new DateTime(2024, 2, 19, 0, 0, 0, DateTimeKind.Utc), CreatedByUserId = 1 }
        );

        mb.Entity<LibraryTag>().HasData(
            new { LibEntryId = 1, TagId = 1 }, new { LibEntryId = 1, TagId = 3 },
            new { LibEntryId = 2, TagId = 1 }, new { LibEntryId = 2, TagId = 2 },
            new { LibEntryId = 3, TagId = 13 }, new { LibEntryId = 3, TagId = 10 },
            new { LibEntryId = 4, TagId = 7 }, new { LibEntryId = 4, TagId = 8 },
            new { LibEntryId = 5, TagId = 4 }, new { LibEntryId = 5, TagId = 6 },
            new { LibEntryId = 6, TagId = 1 }, new { LibEntryId = 6, TagId = 3 },
            new { LibEntryId = 7, TagId = 1 }, new { LibEntryId = 7, TagId = 2 },
            new { LibEntryId = 8, TagId = 1 }, new { LibEntryId = 8, TagId = 2 },
            new { LibEntryId = 9, TagId = 7 }, new { LibEntryId = 9, TagId = 8 },
            new { LibEntryId = 10, TagId = 1 }, new { LibEntryId = 10, TagId = 2 },
            new { LibEntryId = 11, TagId = 1 }, new { LibEntryId = 11, TagId = 3 },
            new { LibEntryId = 12, TagId = 7 }, new { LibEntryId = 12, TagId = 9 },
            new { LibEntryId = 13, TagId = 4 }, new { LibEntryId = 13, TagId = 6 },
            new { LibEntryId = 14, TagId = 1 }, new { LibEntryId = 14, TagId = 3 },
            new { LibEntryId = 15, TagId = 7 }, new { LibEntryId = 15, TagId = 8 },
            new { LibEntryId = 16, TagId = 1 }, new { LibEntryId = 16, TagId = 2 },
            new { LibEntryId = 17, TagId = 1 }, new { LibEntryId = 17, TagId = 2 },
            new { LibEntryId = 18, TagId = 9 }, new { LibEntryId = 18, TagId = 3 },
            new { LibEntryId = 19, TagId = 1 }, new { LibEntryId = 19, TagId = 2 },
            new { LibEntryId = 20, TagId = 10 },
            new { LibEntryId = 21, TagId = 1 }, new { LibEntryId = 21, TagId = 3 },
            new { LibEntryId = 22, TagId = 1 }, new { LibEntryId = 22, TagId = 2 },
            new { LibEntryId = 23, TagId = 13 }, new { LibEntryId = 23, TagId = 10 },
            new { LibEntryId = 24, TagId = 1 }, new { LibEntryId = 24, TagId = 2 },
            new { LibEntryId = 25, TagId = 7 }, new { LibEntryId = 25, TagId = 8 },
            new { LibEntryId = 26, TagId = 3 }, new { LibEntryId = 26, TagId = 10 },
            new { LibEntryId = 27, TagId = 13 }, new { LibEntryId = 27, TagId = 10 },
            new { LibEntryId = 28, TagId = 1 }, new { LibEntryId = 28, TagId = 3 },
            new { LibEntryId = 29, TagId = 7 }, new { LibEntryId = 29, TagId = 9 },
            new { LibEntryId = 30, TagId = 1 }, new { LibEntryId = 30, TagId = 2 },
            new { LibEntryId = 31, TagId = 1 }, new { LibEntryId = 31, TagId = 2 },
            new { LibEntryId = 32, TagId = 4 }, new { LibEntryId = 32, TagId = 6 },
            new { LibEntryId = 33, TagId = 7 }, new { LibEntryId = 33, TagId = 9 },
            new { LibEntryId = 34, TagId = 1 }, new { LibEntryId = 34, TagId = 2 },
            new { LibEntryId = 35, TagId = 3 }, new { LibEntryId = 35, TagId = 10 },
            new { LibEntryId = 36, TagId = 1 }, new { LibEntryId = 36, TagId = 2 },
            new { LibEntryId = 37, TagId = 10 },
            new { LibEntryId = 38, TagId = 1 }, new { LibEntryId = 38, TagId = 10 },
            new { LibEntryId = 39, TagId = 4 }, new { LibEntryId = 39, TagId = 5 },
            new { LibEntryId = 40, TagId = 10 }, new { LibEntryId = 40, TagId = 15 },
            new { LibEntryId = 41, TagId = 1 }, new { LibEntryId = 41, TagId = 2 },
            new { LibEntryId = 42, TagId = 13 }, new { LibEntryId = 42, TagId = 10 },
            new { LibEntryId = 43, TagId = 11 }, new { LibEntryId = 43, TagId = 10 },
            new { LibEntryId = 44, TagId = 1 }, new { LibEntryId = 44, TagId = 2 },
            new { LibEntryId = 45, TagId = 8 }, new { LibEntryId = 45, TagId = 9 },
            new { LibEntryId = 46, TagId = 1 }, new { LibEntryId = 46, TagId = 10 },
            new { LibEntryId = 47, TagId = 1 }, new { LibEntryId = 47, TagId = 3 },
            new { LibEntryId = 48, TagId = 13 }, new { LibEntryId = 48, TagId = 10 },
            new { LibEntryId = 49, TagId = 10 }, new { LibEntryId = 49, TagId = 7 },
            new { LibEntryId = 50, TagId = 1 }, new { LibEntryId = 50, TagId = 2 }
        );
    }
}