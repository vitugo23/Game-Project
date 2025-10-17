# Quiz Game Application

A **full-stack quiz game** built with **ASP.NET Core Web API**, **React**, and **SQL Server**.  
Players can answer questions in real-time, track their scores, and compete in a dynamic, responsive web interface.  
The backend handles quiz logic, player sessions, and data persistence using a SQL database, while the frontend provides an interactive, modern UI.

---

## Project Overview

This project is a **Quiz Game Platform** where users can:
- Play quizzes with multiple-choice questions.
- Track their scores and performance.
- Interact with a game server that manages quiz sessions and results.
- Store questions, player info, and results in a **SQL Server database**.

The architecture follows a **client-server model**:
- **Frontend (React):** Handles the user interface and game interaction.
- **Backend (ASP.NET Core):** Processes requests, enforces game logic, and connects to the database.
- **Database (SQL Server):** Stores questions, users, and scores.

---
## Project Structure

Game-Project/
├── backend/ # ASP.NET Core Web API
│ ├── Controllers/ # API endpoints
│ ├── Models/ # Data models (e.g., Question, Player, Score)
│ ├── Repositories/ # Data access logic
│ ├── Services/ # Business logic (e.g., GameService)
│ ├── appsettings.json # Connection strings, configurations
│ └── Program.cs # Entry point for the API
│
├── frontend/ # React application
│ ├── src/ # React components and pages
│ ├── public/ # Static assets
│ ├── package.json # Frontend dependencies
│ └── ...
│
└── README.md # Project documentation

---

## Technologies Used

**Frontend**
- React (with Create React App)
- Axios for API communication
- Tailwind CSS or Bootstrap for styling

**Backend**
- ASP.NET Core Web API
- Entity Framework Core for ORM
- SQL Server for data storage
- Swagger for API documentation

**Tools & DevOps**
- Visual Studio Code
- Git & GitHub
- Docker (optional, for containerization)

---



