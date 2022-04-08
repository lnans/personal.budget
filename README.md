<p align="center">
    <img src="./doc/logo.png" />
</p>

# Budget. API

**Budget.** is my personal app for organize and managed my spendings and savings over years.\
This repository only contains Backend code and its based on **CQRS** and **Clean Architecture** standards.

Technical information:
- .NET 6
- SQLite
- Mediatr
- FluentValidation

# Getting Started

For running the project

```bash
git clone https://github.com/lnans/personal.budget
cd personal.budget/src/Api
dotnet run
```

# Functionalities

Implemented
---

| Method                                                         | Endpoint                      | Description                                   |
|----------------------------------------------------------------|-------------------------------|-----------------------------------------------|
| ![](https://img.shields.io/badge/-POST-49cc90?style=plastic)   | `/auth/signin`                | Sign In                                       |
| ![](https://img.shields.io/badge/-GET-61affe?style=plastic)    | `/auth`                       | Return user information                       |
| ![](https://img.shields.io/badge/-GET-61affe?style=plastic)    | `/operationTags?name=:filter` | Return all tags owned by the current user     |
| ![](https://img.shields.io/badge/-POST-49cc90?style=plastic)   | `/operationTags`              | Create a new operation tag                    |
| ![](https://img.shields.io/badge/-PATCH-50e3c2?style=plastic)  | `/operationTags/:id`          | Change Name and Color of an operation tag     |
| ![](https://img.shields.io/badge/-DELETE-f93e3e?style=plastic) | `/operationTags/:id`          | Delete an operation tag                       |
| ![](https://img.shields.io/badge/-GET-61affe?style=plastic)    | `/accounts`                   | Return all accounts owned by the current user |
| ![](https://img.shields.io/badge/-POST-49cc90?style=plastic)   | `/accounts`                   | Create a new account                          |
| ![](https://img.shields.io/badge/-PATCH-50e3c2?style=plastic)  | `/accounts/:id`               | Update account name or icon                   |
| ![](https://img.shields.io/badge/-PATCH-50e3c2?style=plastic)  | `/accounts/:id/archived`      | Archived an account                           |
| ![](https://img.shields.io/badge/-DELETE-f93e3e?style=plastic) | `/accounts/:id`               | Delete an account                             |
| ![](https://img.shields.io/badge/-POST-49cc90?style=plastic)   | `/operations`                 | Create a new operation                        |
| ![](https://img.shields.io/badge/-DELETE-f93e3e?style=plastic) | `/operations/:id`             | Delete an operation                           |
Pending
---
### User
| Method                                                        | Endpoint | Description             |
|---------------------------------------------------------------|----------|-------------------------|
| ![](https://img.shields.io/badge/-PATCH-50e3c2?style=plastic) | `/me`    | Update password         |


### Operation
| Method                                                         | Endpoint          | Description                                     |
|----------------------------------------------------------------|-------------------|-------------------------------------------------|
| ![](https://img.shields.io/badge/-GET-61affe?style=plastic)    | `/operations`     | Return all operations owned by the current user |
| ![](https://img.shields.io/badge/-PATCH-50e3c2?style=plastic)  | `/operations/:id` | Update operation description or amount          |
