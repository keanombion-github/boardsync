# Deployment runbook

For a no-cost demo, host the Next.js frontend on Netlify, the API on a Render
Free web service, and PostgreSQL on Neon Free. The frontend proxies
`/backend/*` requests to the API so the refresh cookie stays on the frontend
origin. The checked-in `render.yaml` is an alternative that provisions Render
Free Postgres, which expires after 30 days; do not use it for a lasting demo.

## 1. Pre-deployment checks

Run the backend build/tests and frontend lint/build from the repository root.
The GitHub repository is already public, and an earlier commit contains an old
local PostgreSQL password. It was rotated on 2026-10-01; the historic credential
was confirmed invalid. Removing it from the current `appsettings.json` does not
erase Git history. The new credential is in .NET user-secrets. Other local
projects using the shared `postgres` role need their saved connection strings
updated.

The API, test project, CI workflow, and Docker image all target .NET 10 LTS.
The repository `global.json` pins SDK feature band 10.0.4xx while allowing newer
patches in that band.
Local Docker is optional: Render builds the API image from
`Boardsync.Api/Dockerfile` when the web service deploys.

## 2. Create PostgreSQL and deploy the backend

1. Create a Neon Free PostgreSQL project. Copy its connection string to a
   private password manager; never add it to source control.
2. Push the application changes to GitHub.
3. In Render, create a **Web Service** from the repository using Docker, the
   repository root as build context, and `Boardsync.Api/Dockerfile` as Dockerfile
   path. The Free web-service plan is enough for a small demo.
4. Set `DATABASE_URL` to the Neon connection string, `ASPNETCORE_ENVIRONMENT` to
   `Production`, `RefreshCookiePath` to `/backend/api/auth`, and set
   `Jwt__SigningKey` to a freshly generated strong secret. Set `Jwt__Issuer` to
   `Boardsync.Api` and `Jwt__Audience` to `Boardsync.Web`.
5. Set `FrontendOrigin` to the exact Netlify origin if it already exists;
   otherwise update it after Netlify assigns the URL.
6. Wait for `/health` to report a healthy deployment. Startup applies DbUp
   migrations before the service begins accepting requests.
7. Copy the API URL, such as `https://boardsync-api.onrender.com`.

The API converts PostgreSQL URIs into Npgsql connection strings at its
configuration boundary. Keep `DATABASE_URL` and `Jwt__SigningKey` in Render
environment settings, never in Git.

## 3. Deploy the frontend on Netlify

1. Import the same GitHub repository into Netlify as a **new site**. Keep the
   existing portfolio site as a separate project and link to Boardsync from it.
2. Set the base directory to `boardsync-web`. Use Netlify's detected Next.js
   build settings and `npm run build`.
3. Add the server-side `API_PROXY_TARGET` environment variable with the Render
   API URL and no trailing slash. Do not set `NEXT_PUBLIC_API_URL` in production.
4. Deploy, then copy the stable production URL.
5. In Render, set `FrontendOrigin` to that exact origin, including `https://`
   and without a trailing slash, then redeploy the API.

Production browser requests use `/backend/*`, which Next.js rewrites to Render.
This gives the refresh cookie the same site as the frontend and avoids dependence
on third-party cookie support. The rewrite target is read at build time, so
changing it requires a new Netlify deployment. Verify login and refresh cookies
on the hosted site because the local proxy check does not prove Netlify behavior.

## 4. Production smoke test

Use a fresh browser session:

1. Register and confirm the dashboard loads.
2. Create a board, two columns, and two cards.
3. Edit a card and drag it within and across columns.
4. Refresh the page and confirm ordering persisted.
5. Log out and confirm protected routes return to login.
6. Log in again and confirm the session restores after a page refresh.
7. Register a second account and confirm it cannot open the first account's
   board URL.

Check Render logs using the response `X-Correlation-ID` if a request fails.

## No-cost database option

The checked-in `render.yaml` creates a **Render Free Postgres** database, which
expires after 30 days. Do not create a Blueprint from it when following the
Neon route above. Verify the Neon connection and TLS in a hosted smoke test
before switching public traffic.

Neon Free has a small storage and compute allowance. Monitor both in the Neon
dashboard and make occasional `pg_dump` backups; free hosting is not a backup
strategy. Keep credentials in Render environment settings, never in Git or
Netlify browser variables.

## Hosting limitation

Render's free PostgreSQL instance expires after 30 days and has no backups.
Public registration is currently open,
and the API only rate-limits registration/login/refresh. Before promoting the
site widely, add per-account resource quotas and bound large list reads, then
monitor database size and request volume.
