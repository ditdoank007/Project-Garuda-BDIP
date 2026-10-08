# BDIP Infrastructure & CLI Checkpoint

> **Purpose:** This file is the authoritative operational reference for BDIP infrastructure, database connectivity, containers, LDAP, Radius, repository/branch, and CLI conventions.
>
> **Rule:** Before giving or executing a CLI command for BDIP, check this file first. Do not guess service names, container names, database hosts, ports, or paths.

## 1. Core Architecture

BDIP PostgreSQL is the **Source of Truth / highest authority**.

The downstream authentication architecture is:

```
BDIP PostgreSQL
      |
      +----> LDAP
      |        |
      |        +----> user authentication/password
      |
      +----> FreeRADIUS (downstream state)
```

Business rule:

- A valid and enabled employee/user in BDIP must remain authoritative even if LDAP is missing or stale.
- LDAP and FreeRADIUS must follow BDIP.
- Do not treat LDAP or FreeRADIUS as the master database for employee identity.
- Password authority follows **BDIP -> LDAP**. FreeRADIUS must not become an independent password authority.

## 2. Repository

- GitHub repository: `ditdoank007/Project-Garuda-BDIP`
- Active operational branch on BDIP server: `CT124`
- Application root on server: `/opt/bdip`

Current known HEAD at checkpoint creation:

```text
690f60e fix(ldap): reprovision missing identities from BDIP
```

Important recent commits:

```text
babd516 fix(users): make password reset BDIP source of truth
f290b0b fix(ldap): allow BDIP identity reprovisioning
690f60e fix(ldap): reprovision missing identities from BDIP
```

## 3. BDIP Server / Container Runtime

BDIP application runs in Docker.

Known containers:

| Container | Purpose | Image |
|---|---|---|
| `bdip-backend` | ASP.NET 9 API | `docker-backend` |
| `bdip-frontend` | Next.js frontend | `docker-frontend` |
| `bdip-nginx` | Reverse proxy | `docker-nginx` |
| `openldap` | LDAP container present on host | `osixia/openldap:1.5.0` |

### IMPORTANT: Docker Compose service names

The Compose service names are **not** the same as container names.

Current `docker/compose.yml`:

```yaml
services:
  frontend:
    container_name: bdip-frontend

  backend:
    container_name: bdip-backend
```

Therefore:

- Compose service for backend = `backend`
- Container name for backend = `bdip-backend`
- Compose service for frontend = `frontend`
- Container name for frontend = `bdip-frontend`

Correct examples:

```bash
cd /opt/bdip

docker compose -f docker/compose.yml build backend

docker compose -f docker/compose.yml up -d --force-recreate backend

docker logs --tail 80 bdip-backend
```

**Never use:**

```bash
docker compose -f docker/compose.yml build bdip-backend
docker compose -f docker/compose.yml up -d bdip-backend
```

because `bdip-backend` is a container name, not the Compose service name.

## 4. BDIP PostgreSQL Database

BDIP PostgreSQL is **remote**, not a PostgreSQL Docker container on SERVER-BDIP.

Known database endpoint:

- Host: `db-bdip.sarsurabaya.id`
- Resolved IP: `192.168.100.122`
- Port: `5432`
- Database: `bdip`
- PostgreSQL user: `bdip`

Do not assume a local PostgreSQL container exists.

### Database credentials

Credentials are stored in the backend container environment / deployment configuration.

**Never put database passwords, LDAP passwords, Radius passwords, or other secrets in this file.**

For diagnostic access, obtain the password from the running deployment without printing it to the terminal or chat. Prefer environment-variable handling such as:

```bash
DB_PASS="$(docker inspect bdip-backend --format '{{range .Config.Env}}{{println .}}{{end}}' | sed -n 's/^ApplicationDb__Password=//p')"
# use "$DB_PASS" only where necessary
unset DB_PASS
```

Do not echo `DB_PASS`.

## 5. LDAP

Backend configuration currently points to:

- LDAP host: `ldap.sarsurabaya.id`
- LDAP IP: `192.168.205.100`
- Port: `389`
- SSL: disabled
- Base DN: `dc=sarsurabaya,dc=id`
- People DN: `ou=People,dc=sarsurabaya,dc=id`
- Groups DN: `ou=Groups,dc=sarsurabaya,dc=id`
- Bind DN: `cn=admin,dc=sarsurabaya,dc=id`

The configured LDAP admin password is a secret and must never be written into this file or pasted into chat.

### LDAP operational rule

If a user exists and is enabled in BDIP but the LDAP identity is missing:

- BDIP remains authoritative.
- Password reset must be able to reprovision the LDAP identity from BDIP.
- Do not manually create an unrelated LDAP identity with guessed attributes.
- Prefer the application provisioning flow so LDAP remains consistent with BDIP.

## 6. FreeRADIUS

FreeRADIUS is downstream of BDIP/LDAP.

Current password model:

- BDIP is authoritative for user identity.
- LDAP owns the authentication password used by the current login flow.
- FreeRADIUS does not own an independent user password.
- Existing password-reset provisioning removes stale `Cleartext-Password` state from Radius when appropriate.

Do not invent a Radius database host, container, schema, or password. Check the application configuration/source before giving a Radius CLI.

## 7. Important BDIP Database Table

The main user table is:

```sql
public.users
```

Known important columns:

```text
username
nip
finger_id
full_name
email
unit_id
enabled
password_changed_at
created_at
updated_at
```

Example canonical user lookup:

```sql
SELECT
    username,
    nip,
    finger_id,
    full_name,
    email,
    unit_id,
    enabled,
    password_changed_at,
    created_at,
    updated_at
FROM public.users
WHERE LOWER(username) = LOWER('muhammad.zaenal')
   OR nip = '198109152002121003';
```

## 8. Known Zaenal Incident / Verified Fix

User:

- Username: `muhammad.zaenal`
- NIP: `198109152002121003`
- Full name: `M. Zaenal Arifin`
- Email: `zaenal842@gmail.com`
- Finger ID: `202120001`

Incident:

- User existed and was enabled in BDIP PostgreSQL.
- LDAP identity for `muhammad.zaenal` was missing.
- Password reset therefore previously produced API 500 because LDAP reset expected an existing LDAP identity.
- The code was changed so BDIP is authoritative and missing LDAP identity can be reprovisioned from BDIP.
- The backend was rebuilt/recreated successfully.
- Zaenal's password was reset successfully.
- Zaenal successfully logged in afterward.

This incident is the reference case for the **BDIP -> LDAP authority rule**.

## 9. Password Reset Code Path

Relevant backend registrations:

```csharp
builder.Services.AddScoped<IUserService, PostgreSqlUserService>();
builder.Services.AddScoped<ILdapProvisioningService, UserService>();
```

Relevant endpoint:

```text
POST /api/users/{username}/reset-password
```

Current intended flow:

```text
1. Resolve canonical user from BDIP PostgreSQL
2. Reject if user does not exist
3. Reject if user is disabled
4. Build provisioning data from BDIP
5. Reset existing LDAP identity OR create missing LDAP identity
6. Synchronize downstream Radius state
7. Update password_changed_at in BDIP
```

Relevant source files:

```text
backend/API/Controllers/UsersController.cs
backend/Infrastructure/Users/PostgreSqlUserService.cs
backend/Infrastructure/Users/UserService.cs
backend/Application/Provisioning/ILdapProvisioningService.cs
```

## 10. Safe CLI Rules for Future Chatty Sessions

Before giving a BDIP CLI:

1. Confirm the server: `SERVER-BDIP`.
2. Confirm application root: `/opt/bdip`.
3. Confirm repository branch: `CT124`, unless the user explicitly changes it.
4. Check `docker/compose.yml` before assuming a Compose service name.
5. Distinguish **service name** from **container name**.
6. Check application configuration/source before assuming a database or LDAP endpoint.
7. Never expose secrets in output.
8. Do not use destructive Docker flags such as `--remove-orphans` unless explicitly required and confirmed.
9. Do not delete existing `_backup/` or `.bak` files merely because Git reports them as untracked.
10. Prefer read-only verification commands first.
11. For database changes, show the exact SQL and identify the target database/table before execution.
12. For code changes, use the GitHub repository as the source for the intended code state, then have the user pull/build/restart on the server.
13. After deployment, verify container status and startup logs before functional testing.

## 11. Standard BDIP Verification Commands

### Git

```bash
cd /opt/bdip

git status --short
git branch --show-current
git log -1 --oneline
git remote -v
```

### Docker

```bash
docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Image}}'
```

### Backend logs

```bash
docker logs --tail 80 bdip-backend 2>&1
```

### Compose service discovery

When uncertain, do this before build/recreate:

```bash
docker compose -f docker/compose.yml config --services
```

Expected current services:

```text
frontend
backend
```

### Database connectivity

Use the deployment's configured credentials and never print the password. The canonical endpoint is:

```text
db-bdip.sarsurabaya.id:5432 / database bdip
```

## 12. Do Not Guess

If a future task involves any of these and this file does not contain enough information:

- FreeRADIUS host/database/schema
- another LDAP server
- another PostgreSQL database
- a new Docker service
- a new production server
- a new deployment path
- a new environment variable
- a new API endpoint

**Stop and inspect the repository/server configuration first. Do not invent a CLI.**

---

_Last updated: 2026-10-08 after successful BDIP/LDAP password-reset remediation for `muhammad.zaenal`._
