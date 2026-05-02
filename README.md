# 🏥 SmartCare Platform

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Entity Framework Core](https://img.shields.io/badge/EF_Core-8.0-3FA037?logo=nuget)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue)

> [!NOTE]
> This is the core backend repository for the SmartCare Platform. It handles everything from user authentication and real-time carts to complex order processing and third-party integrations.

## 🏗️ 1. System Architecture

The SmartCare Platform is built using a strict **Clean Architecture** approach combined with **CQRS** (Command Query Responsibility Segregation) and **Event-Driven Architecture**.

### 📂 Layers Overview

*   **1. SmartCare.Domain:** 
    *   **Responsibility:** Contains enterprise logic, entities, enums, exceptions, events, and repository interfaces.
    *   **Key components:** Strongly typed entities, NetTopologySuite for spatial data, Domain Events.
*   **2. SmartCare.Application:**
    *   **Responsibility:** Contains business use cases (commands, queries, event handlers).
    *   **Key components:** DTOs, Validation rules (FluentValidation), Mappers (AutoMapper), MediatR/Messaging, and external service interfaces.
*   **3. SmartCare.InfraStructure:**
    *   **Responsibility:** Implements interfaces from the Domain and Application layers.
    *   **Key components:** Entity Framework Core DbContext, Migrations, Repositories, Background Jobs (Hangfire), external service integrations (Stripe, Cloudinary, Email).
*   **4. SmartCare.API (Presentation):**
    *   **Responsibility:** The entry point to the system. Handles HTTP requests and Real-time connections.
    *   **Key components:** Controllers, SignalR Hubs, Custom Middlewares (Error Handling, Rate Limiting, Input Sanitization), Dependency Injection setup.

---

## 🛠️ 2. Technology Stack & Frameworks

### Core Technologies
*   **Framework:** .NET 8.0 (C# 12)
*   **ORM:** Entity Framework Core 8.0
*   **Database:** Microsoft SQL Server (with NetTopologySuite for Geo-Spatial data)
*   **Real-time Communication:** Microsoft ASP.NET Core SignalR
*   **Authentication & Authorization:** ASP.NET Core Identity + JWT Bearer Tokens

### Key Libraries & Packages
*   **Validation:** `FluentValidation` (Automated API validation)
*   **Object Mapping:** `AutoMapper`
*   **Background Jobs:** `Hangfire` (with SQL Server storage)
*   **Security & Hashing:** `BCrypt.Net-Next`
*   **Logging:** `Serilog` (Console and File sinks)
*   **Resilience & Fault Tolerance:** `Polly`
*   **Search/Text Matching:** `FuzzySharp`
*   **Input Sanitization:** `HtmlSanitizer` (XSS prevention)
*   **API Documentation:** `Swashbuckle` (Swagger/OpenAPI)

---

## 🔌 3. External Services

> [!IMPORTANT] 
> The system relies on the following third-party services and microservices. Ensure proper API keys and credentials are provided in `appsettings.json`.

| Service | Purpose | Configuration Key in `appsettings.json` |
| :--- | :--- | :--- |
| **Cloudinary** | Media and Image hosting/management | `"cloudinary": { "CloudName", "ApiKey", "ApiSecret" }` |
| **Stripe** | Payment Gateway processing | Initialized via DI/Stripe.net |
| **Gmail SMTP** | Email notifications & OTPs | `"emailSettings": { "host": "smtp.gmail.com" ... }` |
| **SmartCare-AI** | Dedicated Python/Flask AI microservice for semantic drug search, similarity, and contraindication detection. | Configured via AI service endpoint URL |

---

## 📡 4. Real-time Hubs (SignalR)

The platform heavily utilizes WebSockets for real-time updates.

*   `wss://[host]/hubs/payments`: Payment status updates.
*   `wss://[host]/hubs/products`: Product availability and stock alerts.
*   `wss://[host]/hubs/cart`: Live cart updates and reservation expirations.
*   `wss://[host]/hubs/orders`: Order status tracking.
*   `wss://[host]/hubs/users`: General user notifications.

---

## ⚙️ 5. Background Jobs (Hangfire)

Background processes handle time-sensitive domain logic automatically:
*   **Cart Expiration:** Items in cart expire after 7 days.
*   **Order Expiration:** Pending orders expire after 5 minutes.
*   **Payment Window:** 5 hours allowed for payment completion.
*   **PickUp Window:** 1 day allowed for order pickup.
*   **Hangfire Dashboard:** Accessible via `/hangfire` route in the API.

---

## 🛡️ 6. Security & Middlewares

> [!TIP]
> The API includes robust custom middlewares designed for security and stability.

1.  **Rate Limiting Middleware:** Prevents DDoS by limiting requests (Default: 100 requests per 60 seconds).
2.  **Input Sanitization Middleware:** Strips dangerous HTML/Script tags to prevent XSS and generic SQL injections.
3.  **Error Handler Middleware:** Catches unhandled exceptions, logs them via Serilog, and returns standardized JSON error responses.

---

## 🌐 7. API Modules Overview

The API is structured around the following core controllers:

*   **`AuthenticationController`**: Login, Registration, OTP, Password Reset, Token Refresh.
*   **`UserController`**: Profile management, user settings.
*   **`ProductsController`**: Product catalogs, search (FuzzySharp), details.
*   **`CategoryController`**: Hierarchical category management.
*   **`CartController`**: Shopping cart lifecycle, item reservations.
*   **`OrdersController`**: Order placement, status tracking, history.
*   **`PaymentsController`**: Stripe checkout sessions, webhooks, payment verification.
*   **`InventoryController`**: Stock management, warehousing.
*   **`StoreController` & `CompanyController`**: Multi-tenant/store configurations.
*   **`ClientAddressController`**: User geolocation and shipping addresses.
*   **`FavouritesController`**: Wishlist management.
*   **`RatesController`**: Reviews and ratings.
*   **`LookupsController`**: Dropdown data and general enums.

---

## 🚀 8. Setup and Run Instructions

### Prerequisites
*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
*   Visual Studio 2022 or JetBrains Rider

### Step-by-Step Setup

1.  **Clone the Repository:**
    ```bash
    git clone https://github.com/your-org/SmartCare.git
    cd SmartCare
    ```

2.  **Configure Application Settings:**
    *   Navigate to `SmartCare.API/appsettings.json`.
    *   Update the `ConnectionStrings:Local` with your local SQL Server details.
    *   Ensure proper keys are set for `cloudinary`, `JwtSettings`, and `emailSettings`.

3.  **Run Database Migrations:**
    Open the Package Manager Console or your terminal, ensure `SmartCare.InfraStructure` is the default project, and run:
    ```bash
    dotnet ef database update --project SmartCare.InfraStructure --startup-project SmartCare.API
    ```

4.  **Run the Application:**
    ```bash
    cd SmartCare.API
    dotnet run
    ```

5.  **Access Swagger UI:**
    Once running, navigate to `https://localhost:[port]/swagger` to view the interactive API documentation.
