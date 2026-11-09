# RaceDay Web (Part 3 – ASP.NET Core MVC front end)

MVC application that consumes the RaceDay REST API from Part 2.
It contains **no database access and no duplicated business logic** – every data
operation goes through `Services/ApiClient.cs`.

## Run locally

1. Run the **Part 2 API** first (profile `https`, it listens on `https://localhost:7236`).
2. Check `appsettings.json` → `ApiSettings:BaseUrl` matches the API address.
3. Run this project:

```bash
dotnet run --launch-profile https
```

Open https://localhost:7100.

> The API and the MVC app both run on `localhost`, so the MVC session cookie has its own name
> (`.Race.MVC.Session`) to avoid clashing with the API's `.AspNetCore.Session`.

## Roles
* **Organiser** – dashboard, events, categories, enrolments, results entry.
* **Participant** – browse/filter events, enrol, my enrolments, my results, profile.

## Docker / Azure Blob Storage
Added in the next step.
