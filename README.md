README.md

# 🇨🇭 SwissLohnSystem
A modular Swiss payroll system with accurate AHV/ALV/BVG/UVG/FAK and Quellensteuer (QST) calculations.  
Includes a structured REST Web API, AdminLTE 3 + Bootstrap 4 UI, and a domain-driven core architecture.

---

## 📌 Overview

SwissLohnSystem is a full-stack payroll platform designed for Swiss companies.

It provides:

- Monthly salary calculations  
- AHV / ALV / BVG / UVG / FAK deductions  
- Quellensteuer tariff management  
- Company & employee management  
- AdminLTE 3–based management UI  
- Standardized API with ApiResponse<T>  

---

## 🏗️ Project Structure

```
SwissLohnSystem/
│
├── SwissLohnSystem.sln
├── .gitattributes
│
├── SwissLohnSystem/          → Domain & Business Logic
├── SwissLohnSystem.Web/      → REST API (Controllers, DTOs, Swagger)
└── SwissLohnSystem.UI/       → AdminLTE 3 MVC UI (Companies, Employees, Löhne)
```

---

## ⚙️ Technologies

**Backend**
- .NET / ASP.NET Core  
- Entity Framework Core  
- SQL Server  
- Swagger / OpenAPI  
- ApiResponse<T> wrapper  

**Frontend**
- AdminLTE 3  
- Bootstrap 4  
- ASP.NET MVC  
- jQuery  
- Toastr notifications  

---

## 🚀 Features

### ✔ Implemented
- Base architecture (UI + API + Core)
- Companies module
- Employees module (initial)
- Domain entities for payroll
- CRLF-normalized `.gitattributes`

### 🛠 In Progress
- `/api/Lohn/by-company/{companyId}` endpoint  
- Employee Create/Edit pages  
- Settings module for AHV/ALV/BVG/etc.  
- QST tariff schema (canton + tariff code)  
- Salary calculation engine cleanup  

### 🎯 Planned
- WorkDays / Overtime module  
- Lohn list: filtering, pagination, CSV/PDF export  
- Localization (German format)  
- Authentication (Cookie/JWT)  
- Deployment / HSTS / Reverse Proxy config  

---

## 📥 Installation

### 1. Clone
```bash
git clone https://github.com/IsmailEkincii/SwissLohnSystem.git
cd SwissLohnSystem
```

### 2. Restore
```bash
dotnet restore
```

### 3. Database Setup
- Edit connection string in `appsettings.json`
- Run migrations (or create new database)

### 4. Run API
```bash
cd SwissLohnSystem.Web
dotnet run
```

### 5. Run UI
```bash
cd SwissLohnSystem.UI
dotnet run
```

Default ports:

- API: https://localhost:5001  
- UI:  https://localhost:5002  

Ensure UI → API BaseUrl matches.

---

## 🧪 Tests
```bash
dotnet test
```

---

## 🗺 Roadmap
- [ ] Settings Module  
- [ ] QST Tariff Table  
- [ ] Employee Create/Edit Pages  
- [ ] Lohn Calculation Engine  
- [ ] Overtime & WorkDays Module  
- [ ] Export (CSV/PDF)  
- [ ] UI Cleanup (CSS, Toastr order)  
- [ ] Localization  
- [ ] Authentication  

---

## 🤝 Contributing
1. Fork  
2. Create a new branch  
3. Commit changes  
4. Open Pull Request  

---

## 📄 License
To be added.

