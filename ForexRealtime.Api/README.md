# Forex Real-time API

Real-time Forex notification system using **SignalR**, **MediatR (CQRS)**, **PostgreSQL**, and **Finnhub** WebSocket.

## Features

- **SignalR** hub (`/hubs/forex`) with groups per symbol; clients subscribe to symbols and receive **price updates every 500ms**.
- **JWT Bearer** authentication; only authorized clients can connect to the hub and call APIs.
- **Finnhub** WebSocket ingestion: connect to Finnhub, subscribe to symbols, update in-memory cache.
- **BroadcastService**: every 500ms pushes cached ticks to SignalR groups.
- **Tick persistence**: `TickPersistenceService` writes cache to PostgreSQL every 500ms (`price_tick` table).
- **Subscription audit**: Subscribe/Unsubscribe actions are recorded in `subscription_audit` via MediatR.
- **Serilog** with correlation IDs for connection and broadcast logging.
- **MediatR** CQRS: `RecordSubscriptionCommand`, `GetRecentTicksQuery` (recent ticks retrievable via API).

## Prerequisites

- .NET 8 SDK
- PostgreSQL (e.g. PgAdmin or local Postgres)
- Finnhub API key (default in appsettings; replace if needed)

## Setup

1. **PostgreSQL**

   Create a database, e.g.:

   ```sql
   CREATE DATABASE forex_realtime;
   ```

   Update `appsettings.json` if needed:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=forex_realtime;Username=postgres;Password=YOUR_PASSWORD"
   }
   ```

2. **Apply migrations**

   ```bash
   dotnet ef database update
   ```

   Or run the app once; it will attempt to apply migrations on startup (optional).

3. **Run the API**

   ```bash
   dotnet run
   ```

   API: `https://localhost:7050` (or the port in `launchSettings.json`).

## Test Client

1. Open **https://localhost:7050/index.html** in a browser (accept the dev certificate if prompted).
2. **Login**: enter a username (e.g. `testuser`) and click **Login**.
3. **Subscribe**: click **Subscribe** (default symbol `OANDA:EUR_USD`). You should see **PriceUpdate** messages in the log every ~500ms.
4. **Unsubscribe**: click **Unsubscribe** to stop updates.

Symbols must match what the server is ingesting (see `Finnhub:Symbols` in appsettings), e.g. `OANDA:EUR_USD`, `OANDA:GBP_USD`, `BINANCE:BTCUSDT`.

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST   | `/api/auth/login` | No | Body: `{ "username": "..." }`. Returns JWT. |
| GET    | `/api/ticks?symbol=OANDA:EUR_USD&limit=100` | JWT | Recent ticks (from DB). |

## SignalR Hub

- **URL**: `https://localhost:7050/hubs/forex`
- **Auth**: JWT as query string `?access_token=TOKEN` or `Authorization: Bearer TOKEN`.
- **Methods**: `Subscribe(symbol)`, `Unsubscribe(symbol)`.
- **Server → Client**: `PriceUpdate` with `{ symbol, price, bid, ask, ts }` every 500ms for subscribed symbols.

## Data Model (PostgreSQL)

- **price_tick**: `id`, `symbol`, `price`, `bid`, `ask`, `ts`
- **subscription_audit**: `id`, `user_id`, `symbol`, `action`, `at`

## Configuration (appsettings.json)

- **ConnectionStrings:DefaultConnection** – PostgreSQL connection string.
- **Jwt:Key** – Signing key (min 32 chars).
- **Finnhub:ApiKey** – Finnhub API key.
- **Finnhub:Symbols** – Comma-separated symbols (e.g. `OANDA:EUR_USD,OANDA:GBP_USD,BINANCE:BTCUSDT`).

## Acceptance Criteria

- Authorized clients can connect and subscribe to symbols.
- Updates received every 500ms via SignalR.
- Unauthorized clients are rejected (hub and `/api/ticks` require JWT).
- Subscription actions logged to DB (`subscription_audit`).
- Recent ticks retrievable via `GET /api/ticks` (query handler).
- Logging captures connection and broadcast info (Serilog + correlation ID).
