# Expense Tracker (ASP.NET Core MVC)

A simple ASP.NET Core MVC application for tracking personal expenses.
Users have an overview of the current month with key summary information, followed by a section for adding transactions and a table with filtering options for transaction history.
The last item in the navigation bar is a Settings section, which currently serves as a management area for transaction categories.

## What to expect
- CRUD operations for transactions
- CRUD management of expense categories
- MVC architecture with separation of concerns
- Entity Framework Core
- Local SQLite database

## Tech stack
- ASP.NET Core MVC
- Entity Framework Core
- SQLite

## Project status
Work in progress. The core functionality is implemented and stable, UI refinement, and feature expansion.

### Current focus
- UI and UX improvements on main index views accessible from the navigation bar  
- Improved table density, typography, and visual hierarchy  
- Subtle styling enhancements for dashboard and navigation elements  

*(Detail and preview views are intentionally out of scope for the current iteration.)*

### Short-term goals
- Authentication (ASP.NET Core Identity)
- Basic overview charts and enhanced statistics on the Home dashboard
- Multi-currency support

### Mid / long-term goals
- Per-user transactions (OwnerId)
- Export to CSV / PDF
- Recurring income and expense items
- Automatic currency conversion to a primary currency
- Continued UI/UX improvements
