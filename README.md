# Canteen Ordering System (UTB Minute)

## Team Members and Work Ratio

| Name | Role | Work Ratio |
| --- | --- | --- |
| **Šipoš** - Lead | Blazor clients, Aspire integration | 1 |
| **Novák** | WebApi, SSE | 1 |
| **Hlavička** | Tests, Cloak | 1 |

---

## Running the Project

**Requirements:** .NET 10 SDK, Docker Desktop or Podman.

1. Start Docker Desktop or Podman.
2. Open `Minute.slnx` in Visual Studio 2026 or JetBrains Rider.
3. Set `UTB.Minute.AppHost` as the **startup project**.
4. Run the project.
5. The **.NET Aspire Dashboard** will open in the browser, showing all services and their status.
6. To reset and seed the database, use the **Reset** HTTP command on the `dbmanager` service in the Aspire Dashboard.

---

## Solution Structure

* `UTB.Minute.AppHost` — Aspire orchestration. Spins up SQL Server, Keycloak, the WebAPI, DbManager, AdminClient, and CanteenClient. In test mode, uses a separate ephemeral SQL Server container and Keycloak instance.
* `UTB.Minute.ServiceDefaults` — Shared Aspire service defaults (OpenTelemetry, service discovery, health checks).
* `UTB.Minute.Db` — EF Core entities (`Meal`, `MenuItem`, `Order`) and `MinuteDbContext`.
* `UTB.Minute.DbManager` — Exposes a `/reset-db` endpoint used by the Aspire HTTP Command to drop, recreate, and seed the database with test data.
* `UTB.Minute.Contracts` — Shared DTOs and the `OrderStatus` enum. Referenced by WebAPI, clients, and tests. Entities are never exposed directly to clients.
* `UTB.Minute.WebApi` — Minimal WebAPI with all business logic. Uses `TypedResults` throughout. Secured via Keycloak JWT. Includes SSE endpoint for real-time order notifications.
* `UTB.Minute.WebApi.Tests` — Integration tests running against a real SQL Server and Keycloak instance spun up via Aspire.
* `UTB.Minute.AdminClient` — Blazor Server application for canteen management (meals, menu). Secured with Keycloak; requires `canteen-admin` role.
* `UTB.Minute.CanteenClient` — Blazor Server application for students and cooks. Students can view today's menu and place orders without logging in. Cooks log in to manage order statuses.

---

## Key Implementation Decisions

### 1. Database — SQL Server via Aspire

SQL Server was chosen instead of PostgreSQL. It is provisioned entirely through Aspire with a persistent data volume for development and a fresh ephemeral container per test run (using `--environment=Testing`).

### 2. DTOs

All DTOs are defined exclusively in `UTB.Minute.Contracts`. The WebAPI maps entities to DTOs before returning responses, so EF entities never leak to clients.

### 3. Order Status Transitions

`OrderStatus` is defined as an enum in `UTB.Minute.Contracts` and stored as an integer in the database. Valid transitions enforced on the server:

```
Preparing → Ready
Preparing → Cancelled
Ready     → Completed
Cancelled → Completed
```

Any other transition returns `400 Bad Request`.

### 4. Portion Counting and Concurrency

When a student places an order, `PortionsAvailable` on the `MenuItem` is decremented. If it is already 0, the API returns `400 Bad Request`. To handle race conditions when multiple students order the last portion simultaneously, `MenuItem` uses a `RowVersion` optimistic concurrency token. A `DbUpdateConcurrencyException` is caught and returns `400 Bad Request`.

### 5. Authentication and Authorization

Keycloak is provisioned via Aspire with a pre-imported realm (`utb-minute`). The WebAPI validates JWT tokens issued by Keycloak. Roles:
- `canteen-admin` — full access to meals, menu, and order management
- `cook` — access to order status updates
- Students access public endpoints without authentication

### 6. SSE Notifications

The `/orders/stream` endpoint uses Server-Sent Events to broadcast order status changes to all connected clients in real time, without authentication.

### 7. Testing

Tests use `Aspire.Hosting.Testing` to start the full application stack automatically, including a real SQL Server container and Keycloak. Each test resets the database via `ResetDatabaseAsync()` to ensure isolation. No manual setup is required.

---

## API Endpoints

**Meals** — `canteen-admin` required for write operations
* `GET /meals` — List all meals
* `GET /meals/{id}` — Get a single meal
* `GET /meals/active` — List active meals only
* `POST /meals` — Create a meal
* `PUT /meals/{id}` — Update a meal
* `DELETE /meals/{id}` — Deactivate a meal (soft delete)

**Menu** — `canteen-admin` required for write operations
* `GET /menu` — List all menu items
* `GET /menu/{id}` — Get a single menu item
* `POST /menu` — Create a menu item
* `PUT /menu/{id}` — Update a menu item
* `DELETE /menu/{id}` — Delete a menu item

**Orders**
* `GET /orders` — List all non-completed orders (public)
* `GET /orders/{id}` — Get a single order (`cook` or `canteen-admin`)
* `POST /orders` — Create an order, decrements available portions (public)
* `PUT /orders/{id}/status` — Update order status (`cook` or `canteen-admin`)
* `GET /orders/stream` — SSE stream of order notifications (public)

## Known Issues

* Being logged in on one client causes an error when attempting to log in on the other client.
  **Temporary fix:** Always log out of the current client before switching to the other one.
