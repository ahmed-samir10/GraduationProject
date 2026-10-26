<div align="center">

# 🤝 Kon Sanad (كن سند)

**A charity and aid management REST API that connects Donors, Beneficiaries, Volunteers, and Managers, with campaigns, donations, field missions, stock control, real-time notifications, and PDF reports.**

[![.NET](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF%20Core-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Redis](https://img.shields.io/badge/Redis-DC382D?logo=redis&logoColor=white)](https://redis.io/)
[![SignalR](https://img.shields.io/badge/SignalR-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core/signalr/)
[![JWT](https://img.shields.io/badge/JWT-000000?logo=jsonwebtokens&logoColor=white)](https://jwt.io/)

[Features](#-features) · [Architecture](#-architecture) · [API Overview](#-api-overview) · [Getting Started](#-getting-started) · [Configuration](#%EF%B8%8F-configuration)

</div>

---

## 📖 Overview

**Kon Sanad** ("Be a Support") is a multi-actor charity management system built as my **graduation project**. It digitizes the full aid cycle: beneficiaries submit help requests, volunteers visit and verify them in the field, donors give cash or in-kind items, and managers review everything, control the warehouse stock, and run campaigns.

The backend is an **ASP.NET Core Web API** with **JWT authentication**, **role-based access**, **real-time notifications over SignalR**, **Redis-backed carts**, and **bilingual-ready PDF reports**. It was designed with a focus on **layered architecture**, **clear business workflows**, and **clean separation between identity data and domain data**.

## 👥 Actors & Roles

| Role | What they do |
|---|---|
| 🙋 **Beneficiary** | Browses campaigns, requests needed supplies through a cart, joins campaigns, edits or cancels their own pending requests |
| 💝 **Donor** | Donates cash (to a campaign or a help order), existing supplies, or custom items, and tracks their donations |
| 🦺 **Volunteer** | Applies to join, receives field missions, submits visit reports, and joins campaigns |
| 🛡 **Manager** | Approves volunteers, assigns missions, reviews reports and donations item by item, manages supplies and campaigns, and exports reports |

## ✨ Features

### 🛒 Cart & Checkout (Redis)
- Role-aware carts stored in **Redis** (`cart:{role}:{userId}`), so they are fast and never touch the database until checkout.
- **Beneficiary cart**: supplies with quantity and reason.
- **Donor cart**: existing supply items, **cash donations**, and **custom items** (with image upload).
- Add, update, remove, and clear operations for every cart item type.
- **Strategy pattern at checkout**: `DonorSubmitStrategy` and `BeneficiarySubmitStrategy` each own their own submit logic behind `ICartSubmitStrategy`.
- Donations to an **expired campaign are rejected**.

### 🙋 Help Requests (Beneficiary)
- Submitting a cart creates a **Help Order** with its items and an associated **field Mission**.
- Managers, donors, and volunteers are notified instantly about the new need.
- Beneficiaries can view, update the quantity of, or delete their own requests while pending.
- Profile management and a list of joined campaigns with their status.

### 💝 Donations (Donor)
- **Cash donations** are recorded together with an automatic **Receipt**.
- **Item donations** and **custom items** go to the manager for review.
- Donations tied to a campaign update the **Campaign Result** (total cash collected, donor count) automatically.
- When a campaign reaches its **cash goal**, it is marked **Completed** and managers are notified.
- Donors can view their own item and cash donations, edit amounts or quantities, and see **low-stock supplies** that need support.

### 🦺 Volunteers & Field Missions
- Registration creates a **pending** volunteer that a manager must approve or reject (with a reason).
- Managers **assign a volunteer** to a help order with start and end dates (an order can only be assigned once).
- The volunteer can **start** a mission, **submit a report file**, update or delete the report, or **reject** the assignment with a reason.
- The manager reviews the report and **accepts or rejects each requested item**:
  - Accepting an item **deducts stock** and notifies the beneficiary.
  - The help order status is recalculated from its items (all approved, all rejected, or pending).
- A volunteer can be promoted to **Manager**.

### 📣 Campaigns
- Managers create, update, and soft-delete campaigns with title, category, cash goal, supplies goal, dates, picture, and **maximum beneficiaries**.
- Volunteers and beneficiaries **request to join** a campaign; managers accept or reject the request.
- Campaign categories: food, clothes, education, volunteers, cleaning, housing, devices, winter, and more.

### 📦 Supplies & Stock
- Supply catalog with **stock quantity**, **minimum level**, unit, image, and category.
- **Visibility control** per supply: shown to donors, to beneficiaries, or to both.
- Accepting a donated item **increases stock** and records a **Supply Movement**.
- **Low-stock PDF report** for supplies below their minimum level.

### 🔔 Real-Time Notifications (SignalR)
- Hub at `/hubs/notifications`. Each connection is added to a **per-user group** and **per-role groups**, so messages can target one user, a whole role, or all managers.
- JWT is accepted from the `access_token` query string for WebSocket connections.
- Notifications are **persisted in Redis** (capped per channel) so users get their history after reconnecting.
- REST endpoints to list notifications, mark one as read, or mark all as read.
- **20 notification types**: donation submitted/approved/rejected, campaign created and goal reached, help request events, mission events, volunteer registration and campaign join events, and more.

### ⏰ Background Campaign Monitor
- A `BackgroundService` (`CampaignNotificationJob`) checks campaigns and notifies the campaign manager when:
  - a campaign is **about to end** (within 2 days),
  - a campaign **has ended**,
  - the **cash goal** is reached,
  - the **maximum number of beneficiaries** is reached,
  - the **required number of volunteers** is reached.
- Each alert has its own **sent flag** on the campaign, so it is only ever sent once.

### 📊 Dashboard & PDF Reports
- **Dashboard**: total cash donations, in-kind donations, donors, volunteers, beneficiaries, active campaigns, pending and completed requests, and this month's donations.
- **PDF reports** generated with **QuestPDF**:
  - Donors: last month, top donors, donations between two dates
  - Beneficiaries: all and top beneficiaries
  - Volunteers: all and top fastest volunteers
  - Campaigns: active, not active, expired
  - Requests: pending and completed
  - Low-stock supplies

## 🏗 Architecture

The solution is split into four projects with a one-way dependency flow:

```
┌──────────────────────────────────────────────┐
│  KonSanad.APIS                               │
│  Controllers · Program.cs · JWT setup ·      │
│  Swagger · AutoMapper profiles · Filters     │
└──────────────────────┬───────────────────────┘
                       │ depends on
┌──────────────────────▼───────────────────────┐
│  KonSanad.Services                           │
│  Business services · Cart strategies ·       │
│  SignalR hub · Background job · File service │
└──────────────────────┬───────────────────────┘
                       │ depends on
┌──────────────────────▼───────────────────────┐
│  KonSanad.Repository                         │
│  DbContexts · Configurations · Migrations ·  │
│  Generic repository · Unit of Work · Seeding │
└──────────────────────┬───────────────────────┘
                       │ depends on
┌──────────────────────▼───────────────────────┐
│  KonSanad.Core                               │
│  Entities · Enums · DTOs · Service contracts │
└──────────────────────────────────────────────┘
```

**Patterns & practices**

| Pattern | Where |
|---|---|
| Generic Repository + Unit of Work | `Core/IRepositery`, `Repository/Repository` |
| Strategy | `ICartSubmitStrategy` with `DonorSubmitStrategy` / `BeneficiarySubmitStrategy` |
| Dependency Injection | Service registrations in `Program.cs` |
| AutoMapper + custom value resolvers | `MappingProfiles`, `CampaignUrlResolver`, `ImageUrlsResolver` |
| Options pattern | `Jwt` configuration section |
| Entity base class | `BaseEntity<TKey>`: `Id`, `CreatedAt`, `UpdatedAt`, `Status`, `IsDeleted` (soft delete), `RowVersion` |
| Fluent API configurations | One `IEntityTypeConfiguration` per entity in `Repository/Data/Configurations` |
| Two DbContexts | `KonSanadDbContext` (domain) and `AppIdentityDbContext` (ASP.NET Identity), each with its own migrations |
| Hosted service | `CampaignNotificationJob` |
| Real-time messaging | SignalR hub with user and role groups |
| Standard error responses | `ApiResponse`, `ApiValidationErrorResponse` |

## 🛠 Tech Stack

| Area | Technology |
|---|---|
| Framework | ASP.NET Core Web API on **.NET 9** |
| Data | Entity Framework Core, SQL Server |
| Identity | ASP.NET Core Identity (users and roles) |
| Authentication | JWT Bearer tokens |
| Cache / Carts / Notifications | Redis (StackExchange.Redis) |
| Real-time | SignalR |
| Mapping | AutoMapper |
| PDF | QuestPDF |
| API docs | Swagger / Swashbuckle (with Bearer auth support) |

## 🔒 Security

- **JWT Bearer authentication** with validated issuer, audience, signing key, and lifetime (zero clock skew).
- **ASP.NET Core Identity** for user management and password hashing.
- **Role-based authorization** using `[Authorize(Roles = "...")]` for Manager, Donor, Beneficiary, and Volunteer actions.
- Public self-registration is **limited to Donor, Beneficiary, and Volunteer**; the Manager role can only be granted by an existing manager.
- Volunteers stay **pending** until a manager approves them.
- SignalR connections are authenticated, and notifications are delivered only to the matching user or role group.
- Soft delete on all entities, so records are hidden but never destroyed.

## 📁 Project Structure

```
KonSanad/
├── KonSanad.APIS/
│   ├── Controllers/         # Account, Campaign, Cart, Beneficiary, Donor, Volunteer, Manager, Reports, Notifications
│   ├── Errors/              # ApiResponse, ApiValidationErrorResponse
│   ├── Extenaction/         # JWT authentication setup, UserManager extensions
│   ├── Filters/             # Swagger operation filter
│   ├── Helpers/             # MappingProfiles, resolvers, CachedAttribute
│   └── Program.cs
├── KonSanad.Services/
│   ├── Services/            # Campaign, Cart, Donation, Donor, HelpRequest, Volunteer, Manager, Report, Token, File
│   ├── Realtime/            # NotificationHub, SignalRNotificationService
│   └── Helpers/             # Campaign metrics, URL resolver, date helpers
├── KonSanad.Repository/
│   ├── Data/
│   │   ├── Persistence/     # KonSanadDbContext + seed
│   │   ├── Identity/        # AppIdentityDbContext + seed
│   │   └── Configurations/  # Entity configurations
│   ├── Migrations/          # Domain and Identity migrations
│   └── Repository/          # GenericRepository, UnitOfWork
└── KonSanad.Core/
    ├── Entity/              # Campaign, Donor, Volunteer, Beneficiary, Mission, Supply, Receipt, ...
    │   ├── Enums/
    │   └── Identity/        # AppUser
    ├── Dtos/
    ├── IRepositery/         # Service and repository contracts
    └── Converters/
```

## 🗺 API Overview

All routes are under `/api`. Swagger UI is served at the application root (`/`).

| Area | Base route | Highlights |
|---|---|---|
| **Account** | `/api/Account` | `POST login`, `POST register`, manager profile |
| **Campaigns** | `/api/Campaign` | Public read; create, update, and delete for managers |
| **Cart** | `/api/Cart` | Get cart, add items / donation items / cash / custom items, update, delete, clear, **`POST SubmitCart`** |
| **Beneficiary** | `/api/Beneficiery` | Campaigns, join campaign, my requests, update or delete a request, profile |
| **Donor** | `/api/Donor` | My item and cash donations, low-stock supplies, update or delete donation items, profile |
| **Volunteer** | `/api/Volunteer` | Missions, start mission, submit, update, or delete a report, reject mission, join campaign, profile |
| **Manager** | `/api/Manger` | Help orders, volunteers, assign mission, view or download reports, accept or reject items, volunteer and campaign requests, donations, supplies, visibility, low-stock report |
| **Reports** | `/api/reports` | Dashboard (JSON) and PDF reports for donors, beneficiaries, volunteers, campaigns, and requests |
| **Notifications** | `/api/Notifications` | My notifications, types, connection info, mark as read, mark all as read |
| **Hub** | `/hubs/notifications` | SignalR real-time channel (`ReceiveNotification`) |

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or full)
- [Redis](https://redis.io/) (local instance or Docker)

### Run locally

```bash
# 1. Clone
git clone https://github.com/Islam3mar/KonSanad.git
cd KonSanad/KonSanad.APIS

# 2. Start Redis (example using Docker)
docker run -d --name redis -p 6379:6379 redis

# 3. Set your configuration (never commit these)
dotnet user-secrets set "ConnectionStrings:DefaultConnection"  "Server=.;Database=KonSanadDb;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "ConnectionStrings:IdentityConnection" "Server=.;Database=KonSanadIdentityDb;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "ConnectionStrings:Redis"              "localhost:6379"
dotnet user-secrets set "Jwt:Key"      "<a-long-random-secret-at-least-32-chars>"
dotnet user-secrets set "Jwt:Issuer"   "KonSanad"
dotnet user-secrets set "Jwt:Audience" "KonSanadUsers"

# 4. Run (migrations and seeding are applied automatically on startup)
dotnet run
```

On startup the app:
- applies the migrations of **both databases**,
- seeds categories and beneficiaries from optional `Categories.json` / `Beneficiaries.json` files if they exist,
- creates the four roles (`Manager`, `Donor`, `Beneficiary`, `Volunteer`),
- creates a **default Manager account** (see `AppIdentityDbContextSeed`). **Change its credentials before deploying.**

Then open Swagger at the root URL, call `POST /api/Account/login`, and paste the returned token into **Authorize**.

### Connect to the real-time hub

```js
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:<port>/hubs/notifications", {
    accessTokenFactory: () => token
  })
  .withAutomaticReconnect()
  .build();

connection.on("ReceiveNotification", (n) => console.log(n));
await connection.start();
```

## ⚙️ Configuration

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string for the domain database |
| `ConnectionStrings:IdentityConnection` | SQL Server connection string for the Identity database |
| `ConnectionStrings:Redis` | Redis connection string (carts and notifications) |
| `Jwt:Key` | Symmetric signing key for JWT tokens |
| `Jwt:Issuer` / `Jwt:Audience` | Expected token issuer and audience |

> In production, supply secrets through environment variables or your host's secret store, not through files in the repository.

## 🧪 Core Business Rules

- Self-registration is allowed only for Donor, Beneficiary, and Volunteer; a new volunteer is **pending** until approved by a manager.
- Only the **Manager** role can create, update, or delete campaigns and manage supplies.
- Donating to a campaign that has **ended** is not allowed.
- A help order can be **assigned to one volunteer only**.
- Accepting a requested item **deducts** it from stock; accepting a donated item **adds** it to stock and logs a supply movement.
- A help order is **approved** only when all its items are approved, **rejected** only when all are rejected, and stays **pending** otherwise.
- A campaign becomes **Completed** when its cash goal is reached, and each campaign alert is sent **only once**.
- Records are **soft-deleted**, so history is always preserved.

## 🗺 Screenshots

> _Add screenshots here: Swagger UI, dashboard response, a sample PDF report, and the ERD._

```md
![Swagger](docs/screenshots/swagger.png)
```

## 👤 Author

**Islam Omar**, Back-End Developer (.NET)

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0A66C2?logo=linkedin&logoColor=white)](https://linkedin.com/in/islam-omar-3ba867266)
[![GitHub](https://img.shields.io/badge/GitHub-181717?logo=github&logoColor=white)](https://github.com/Islam3mar)
[![Portfolio](https://img.shields.io/badge/Portfolio-1F5F7A?logoColor=white)](https://islam3mar.github.io/portfolio/)
[![Email](https://img.shields.io/badge/Email-D14836?logo=gmail&logoColor=white)](mailto:islam3marahmed@gmail.com)

Built as my graduation project.
