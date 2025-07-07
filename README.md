# PlanPro

**PlanPro** is a comprehensive ASP.NET MVC web application designed to streamline personal planning and productivity. It integrates multiple modules — including travel planning, to-do task management, financial tracking, image galleries, motivational quotes, and health reminders — into one powerful and user-friendly platform.

## 🌟 Features

- 🧳 **Travel Planning** – Create and manage itineraries with day-wise plans.
- ✅ **To-Do List** – Organize daily tasks with deadlines and completion status.
- 💰 **Financial Tracking** – Record incomes, expenses, and manage budgets.
- 🖼️ **Gallery** – Upload and view memorable travel images and albums.
- 💡 **Motivation** – Display rotating motivational quotes and wellness tips on the homepage.
- 👤 **User Management** – Register, log in, and manage your personal account securely.

---

## 🛠️ Technologies Used

- ASP.NET MVC 5
- Entity Framework
- SQL Server
- HTML5, CSS3, JavaScript, jQuery
- Bootstrap 4/5 (optional for styling)

---

🚀 Getting Started
Prerequisites
Visual Studio 2019 or newer

SQL Server (LocalDB or Express)

.NET Framework 4.7.2 or later

Setup Instructions
Clone the repository:

git clone https://github.com/nethmi2024/PlanPro.git
cd PlanPro

Set up the database connection:

Open the Web.config file and update the <connectionStrings> section with your own SQL Server credentials:

<connectionStrings>
  <add name="DefaultConnection" 
       connectionString="Data Source=YOUR_SERVER_NAME;Initial Catalog=PlanProDB;Integrated Security=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>

Build and run the application:

Open the solution in Visual Studio.
Build the project to restore dependencies.
Run the application using F5 or the green Start button.
