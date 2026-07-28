# ================================================================
# BDIP - Basarnas Digital Identity Platform
# ================================================================

Sprint      : Sprint 06.01
Feature     : Create User (End-to-End)
Project     : Project Garuda
Date        : 30 Juni 2026
Author      : Dityo Mahendro & ChatGPT

==================================================================

# Tujuan Sprint

Menyelesaikan implementasi fitur Create User sehingga administrator
dapat membuat user LDAP langsung dari Dashboard BDIP tanpa
menggunakan phpLDAPadmin.

==================================================================

# Arsitektur

Frontend
(Next.js 16)

↓

API Layer

↓

ASP.NET Core 9

↓

LDAP Service

↓

OpenLDAP

==================================================================

# Backend

## 1. IUserService

Ditambahkan method

Task CreateUserAsync(CreateUserRequest request)

==================================================================

## 2. UserService

Method CreateUserAsync() diimplementasikan.

Fungsi:

- Generate uidNumber otomatis
- Menentukan gidNumber
- Membuat DN user
- Menambahkan objectClass

ObjectClass

- top
- person
- organizationalPerson
- inetOrgPerson
- posixAccount
- shadowAccount

Attribute

uid
cn
sn
givenName
displayName
mail
userPassword
uidNumber
gidNumber
homeDirectory
loginShell

==================================================================

## 3. Password

Awalnya menggunakan

slappasswd

Namun kemudian diputuskan menggunakan

userPassword

berupa plain text.

Alasan:

OpenLDAP Docker (osixia/openldap)

secara otomatis melakukan hashing
ketika bind authentication.

Pendekatan ini lebih sederhana
dan kompatibel dengan Synology,
Mikrotik LDAP,
FreeRADIUS,
Keycloak.

==================================================================

## 4. API

Endpoint

GET

/api/users

POST

/api/users

POST berhasil membuat user LDAP.

==================================================================

## 5. Docker

Backend tidak langsung berubah
karena image docker masih menggunakan
image lama.

Dilakukan:

docker compose build --no-cache backend

docker compose up -d backend

Setelah rebuild,
endpoint POST berhasil aktif.

==================================================================

# Frontend

## UserToolbar

Ditambahkan

Create User Button

==================================================================

## UserDialog

Ditambahkan

- Save Button
- Cancel Button
- Loading State

==================================================================

## UserForm

Ditambahkan

Username

Full Name

Email

Unit

Password

Confirm Password

Active User

Seluruh field menggunakan state.

==================================================================

## UsersClient

Ditambahkan

formData

saving

handleCreateUser()

Integrasi dengan API.

==================================================================

## API Layer

Dibuat folder baru

frontend/lib/api

File

users.ts

Method pertama

createUser()

Menggunakan axios.

==================================================================

# UI Components

Karena project belum memiliki
Label dan Checkbox.

Ditambahkan

components/ui/label.tsx

components/ui/checkbox.tsx

Komponen dibuat reusable
untuk seluruh BDIP.

==================================================================

# Bug yang ditemukan

1.

POST /api/users

405 Method Not Allowed

Penyebab

Container backend
masih menggunakan image lama.

Solusi

Rebuild Docker Image.

------------------------------------------------------------

2.

Module not found

checkbox

Penyebab

Komponen checkbox belum ada.

Solusi

Membuat reusable component.

------------------------------------------------------------

3.

ESLint

no-empty-object-type

Penyebab

Interface kosong.

Solusi

Menggunakan

type

daripada

interface

==================================================================

# Hasil Pengujian

Backend

GET User

SUCCESS

POST User

SUCCESS

LDAP

SUCCESS

Dashboard

SUCCESS

phpLDAPadmin

SUCCESS

User baru

SUCCESS

==================================================================

# Hasil Akhir

Administrator dapat:

✔ Melihat daftar user LDAP

✔ Membuat user baru

✔ User langsung masuk ke OpenLDAP

✔ Dashboard langsung menampilkan user

Tanpa menggunakan phpLDAPadmin.

==================================================================

# Screenshot

Disarankan menambahkan

1.
Dashboard User

2.
Dialog Create User

3.
phpLDAPadmin

4.
LDAP Search

==================================================================

# Sprint Berikutnya

Sprint 06.02

Edit User

Sprint 06.03

Delete User

Sprint 06.04

Reset Password

Sprint 07

Group Membership

Sprint 08

Synology Home Folder

Sprint 09

Mikrotik LDAP

Sprint 10

Keycloak SSO

==================================================================

Status

COMPLETED

✔ END TO END CREATE USER BERHASIL
✔ FRONTEND
✔ BACKEND
✔ API
✔ OPENLDAP
✔ DOCKER

==================================================================