# Project Garuda Engineering Handbook

## Volume 01 — Project Structure

**Project:** Basarnas Digital Identity Platform (BDIP)
**Repository:** Project Garuda
**Version:** 0.5 Alpha

---

# 1. Purpose

Dokumen ini menjelaskan struktur repository Project Garuda agar seluruh pengembang memahami fungsi setiap folder dan menjaga konsistensi pengembangan.

---

# 2. Repository Structure

```
/opt/bdip

backend/
frontend/
docker/
compose/
config/
database/
data/
backup/
logs/
docs/
scripts/
archive/
```

---

# 3. Folder Description

## backend/

Seluruh source code .NET Clean Architecture.

Berisi:

* API
* Application
* Domain
* Infrastructure
* Persistence

---

## frontend/

Seluruh source code Next.js.

Berisi:

* app/
* components/
* hooks/
* services/
* types/
* utils/
* lib/

---

## docker/

Dockerfile dan konfigurasi image.

---

## compose/

Docker Compose untuk service pendukung seperti LDAP, PostgreSQL, Keycloak, Radius, Monitoring, dan Nextcloud.

---

## config/

Konfigurasi aplikasi.

---

## database/

Script database dan migrasi.

---

## data/

Persistent volume Docker.

Folder ini **tidak** dikelola melalui Git.

---

## backup/

Hasil backup sistem.

Folder ini **tidak** dikelola melalui Git.

---

## logs/

Log aplikasi.

Folder ini **tidak** dikelola melalui Git.

---

## docs/

Dokumentasi proyek.

Berisi:

* Architecture
* Deployment
* API
* Engineering Handbook

---

## scripts/

Script operasional.

Contoh:

* backup.sh
* restore.sh
* deploy.sh
* healthcheck.sh

Ke depan akan ditambahkan:

* doctor.sh
* check.sh
* lint.sh
* release.sh

---

## archive/

Menyimpan source code lama yang tidak lagi aktif.

Folder ini hanya digunakan sebagai referensi historis.

---

# 4. Engineering Rules

1. Tidak membuat folder baru tanpa alasan yang jelas.
2. Selalu gunakan komponen reusable sebelum membuat komponen baru.
3. Jalankan lint sebelum commit.
4. Dokumentasi diperbarui bersamaan dengan perubahan kode.
5. Semua perubahan arsitektur dicatat dalam Architecture Decision Record (ADR).

---

# 5. Future Growth

Repository ini dirancang untuk mendukung modul tambahan seperti:

* HRIS
* Monitoring
* Asset Management
* OpenVPN
* Mikrotik Hotspot
* Synology Drive
* Single Sign-On (SSO)

---

# Document History

| Version   | Date | Description                  |
| --------- | ---- | ---------------------------- |
| 0.5 Alpha | 2026 | Initial Engineering Handbook |

---

**Project Garuda**

Basarnas Digital Identity Platform (BDIP)

Developed by

Dityo Mahendro
&
Chatty (OpenAI)
