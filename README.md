# InSureX - Insurance Claim Management System

## 📌 Overview

**InSureX** is a web-based **Insurance Claim Management System** built with **ASP.NET Core MVC, Entity Framework, and Identity**.  
It simplifies claim submission, processing, and management by providing dedicated dashboards for **Employees, CPD Officers, and Admins**.

The system ensures transparency, audit tracking, and efficient claim processing with role-based authentication and authorization.

---

## 🚀 Features

### 👤 Employee
- Register/Login securely
- Submit new claims with attachments
- View claim status and history
- Simple and user-friendly dashboard

### 🏢 CPD (Claims Processing Department)
- View and manage employee claims
- Approve, reject, or request more info on claims
- Track processed claims


### 🔑 Admin
- Access **Admin Dashboard** with system stats
- View and manage employee claims
- Monitor activities via Audit Logs
- Generate claim reports with CSV export

### 📝 Common
- Authentication & Authorization (ASP.NET Core Identity)
- Role-based navigation menu
- Export reports to **CSV**
- Audit logging for accountability

---

## 🛠️ Tech Stack
- **Backend:** ASP.NET Core MVC 7.0, C#
- **Database:** Microsoft SQL Server (Entity Framework Core)
- **Authentication:** ASP.NET Core Identity
- **Frontend:** Bootstrap 5, Razor Views, Bootstrap Icons
- **Other:** CSV Export, Role-based Access Control

---

## 📂 Project Structure
```

InSureX/
│── Controllers/         # MVC Controllers 
│── Models/              # Entity models 
│── Services/            # Business services 
│── Views/               # Razor Views for each role
│── wwwroot/             # Static files 
│── Data/                # DbContext and Migrations
│── InSureX.csproj       # Project file
│── appsettings.json     # Configuration 
│── README.md            # Project documentation

````

---

## ⚙️ Setup Instructions

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/InSureX.git
   cd InSureX
````

2. **Update Database Connection**

   * Open `appsettings.json`
   * Replace connection string with your SQL Server credentials

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=InSureXDB;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

3. **Run Database Migrations**

   ```bash
   dotnet ef database update
   ```

4. **Run the Project**

   ```bash
   dotnet run
   ```

---

## 🔐 Default Roles & Users

When the application starts for the first time, roles are created automatically:

* **Admin** → Full access

  * Email: `admin@insurex.com`
  * Password: `Admin@123`

* **CPD (Claims Processing Department)** → Claim management

  * Email: `cpd@insurex.com`
  * Password: `Cpd@123`

* **Employee** → Claim submission (register a new user manually)



🔐 Default Roles & Users

When the application starts for the first time, roles are created automatically:

Admin → Full access

Email: admin@insurex.com

Password: Admin@123

CPD (Claims Processing Department) → Claim management

Email: cpd@insurex.com

Password: Cpd@123

Employee → Claim submission (register a new user manually)

