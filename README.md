<p align="center">
    <img src="./doc/logo.png" />
</p>

# Budget. API

**Budget.** is my personal app for organize and managed my spendings and savings over years.\
This repository only contains Backend code and its based on **CQRS** and **Clean Architecture** standards.

Technical information:
- .NET 6
- Swagger
- Auth0
- Postgres
- Mediatr
- FluentValidation

# Getting Started

### Authentication

---
First you need to setup an account, user and application on https://auth0.com/ \
Then put your configuration in the appsettings file.

```json
{
  "Auth": {
    "Authority": "<Auth0Authority>",
    "Audience": "<Auth0Audience>"
  }
}
```

Then, have a postgres server running, and put the connection string in the appsettings file.

```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5432;Database=budget_db;Username=postgres;Password=postgrespw"
  }
}
```

Finally, running the project

```bash
cd personal.budget/src/Api
dotnet run
```

# Functionalities

Implemented
---

| Method                                                         | Endpoint                        | Description                                     |
|----------------------------------------------------------------|---------------------------------|-------------------------------------------------|
| ![](https://img.shields.io/badge/-GET-61affe?style=plastic)    | `/tags `                        | Return all tags owned by the current user       |
| ![](https://img.shields.io/badge/-POST-49cc90?style=plastic)   | `/tags`                         | Create a new tag                                |
| ![](https://img.shields.io/badge/-PATCH-50e3c2?style=plastic)  | `/tags/:id`                     | Change Name and Color of a tag                  |
| ![](https://img.shields.io/badge/-DELETE-f93e3e?style=plastic) | `/tags/:id`                     | Delete a tag                                    |
| ![](https://img.shields.io/badge/-GET-61affe?style=plastic)    | `/accounts`                     | Return all accounts owned by the current user   |
| ![](https://img.shields.io/badge/-POST-49cc90?style=plastic)   | `/accounts`                     | Create a new account                            |
| ![](https://img.shields.io/badge/-PATCH-50e3c2?style=plastic)  | `/accounts/:id`                 | Update account name or bank                     |
| ![](https://img.shields.io/badge/-PATCH-50e3c2?style=plastic)  | `/accounts/:id/archived`        | Archived an account                             |
| ![](https://img.shields.io/badge/-DELETE-f93e3e?style=plastic) | `/accounts/:id`                 | Delete an account                               |
| ![](https://img.shields.io/badge/-GET-61affe?style=plastic)    | `/operations?{operationFilter}` | Return all operations owned by the current user |
| ![](https://img.shields.io/badge/-POST-49cc90?style=plastic)   | `/operations`                   | Create new operations                           |
| ![](https://img.shields.io/badge/-DELETE-f93e3e?style=plastic) | `/operations/:id`               | Delete an operation                             |
| ![](https://img.shields.io/badge/-PATCH-50e3c2?style=plastic)  | `/operations/:id`               | Update operation description, tag or amount     |

operationFilter:
```json

{
  "description": "string",
  "accountId": "guid",
  "tagIds": ["guid"],
  "operationType": "string",
  "pageSize": 25,
  "cursor": 0
}
```