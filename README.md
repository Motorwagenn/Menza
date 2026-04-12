[readme.md](https://github.com/user-attachments/files/26660003/readme.md)
# Canteen Ordering System (UTB Minute)

Semester project for the **Application Frameworks** course.

## Team Members and Work Ratio

| Name | Role | Work Ratio |
| --- | --- | --- |
| **Matúš Šipoš** - lead | veci co robil | 1 |
| **Meno2** | veci co robil | 1 |
| **Meno3** | veci co robil | 1 |

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

* `UTB.Minute.AppHost` — Aspire orchestration. Spins up SQL Server, the WebAPI, and the DbManager. In test mode, uses a separate ephemeral SQL Server container.
* `UTB.Minute.Db` — EF Core entities (`Meal`, `MenuItem`, `Order`) and `MinuteDbContext`.
* `UTB.Minute.DbManager` — Exposes a `/reset-db` endpoint used by the Aspire HTTP Command to drop, recreate, and seed the database with test data.
* `UTB.Minute.Contracts` — Shared DTOs and the `OrderStatus` enum. Referenced by both WebAPI and Tests; entities are never exposed directly to clients.
* `UTB.Minute.WebApi` — Minimal WebAPI with all business logic. Uses `TypedResults` throughout.
* `UTB.Minute.Tests` — Integration tests running against a real SQL Server instance spun up via Aspire.

---

## Key Implementation Decisions

### 1. Database — SQL Server via Aspire

SQL Server was chosen instead of PostgreSQL. It is provisioned entirely through Aspire with a persistent data volume for development and a fresh container per test run (using `--environment=Testing`).

### 2. DTOs

All DTOs are defined exclusively in `UTB.Minute.Contracts`. The WebAPI references this project and maps entities to DTOs before returning responses, so EF entities never leak to clients.

### 3. Order Status

`OrderStatus` is defined as an enum in `UTB.Minute.Contracts` and stored as an integer in the database. Possible states: `Preparing → Ready → Cancelled / Completed`.

### 4. Portion Counting

When a student places an order, `PortionsAvailable` on the `MenuItem` is decremented immediately. If `PortionsAvailable` is already 0, the API returns `400 Bad Request` before creating the order.

### 5. Testing

Tests use `Aspire.Hosting.Testing` to start the full application stack (including a real SQL Server container) automatically, with no manual setup required. Each test resets the database via `ResetDatabaseAsync()` to ensure isolation.

---

## Notes

* **Status:** Mid-semester submission — backend and WebAPI only. Client applications, SSE notifications, and Keycloak authentication are not yet implemented.
* **Known issues:** None at time of submission.

---

## API Endpoints

**Meals**
* `GET /meals` — List all meals
* `GET /meals/{id}` — Get a single meal
* `GET /meals/active` — List active meals only
* `POST /meals` — Create a meal
* `PUT /meals/{id}` — Update a meal
* `DELETE /meals/{id}` — Deactivate a meal (soft delete)

**Menu**
* `GET /menu` — List all menu items
* `GET /menu/{id}` — Get a single menu item
* `POST /menu` — Create a menu item
* `PUT /menu/{id}` — Update a menu item
* `DELETE /menu/{id}` — Delete a menu item

**Orders**
* `GET /orders` — List all non-completed orders
* `GET /orders/{id}` — Get a single order
* `POST /orders` — Create an order (decrements available portions)
* `PUT /orders/{id}/status` — Update order status
