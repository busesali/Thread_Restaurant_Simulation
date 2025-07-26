# Restaurant Simulation – Multithreaded C# WinForms App

## Overview

This project is a multithreaded restaurant simulation built using Windows Forms in C#. It models a simplified restaurant environment where customers arrive, waiters serve them, chefs prepare orders, and a cashier handles payments. The system is designed to simulate real-life concurrency challenges using threads, semaphores, and synchronized queues.

---

## Features

- Dynamic customer creation and queuing  
- Simulated waiters, chefs, and cashiers with controlled access  
- Thread-safe resource management with semaphores and locks  
- Real-time updates to the user interface using panels and labels  
- Customizable numbers of waiters, chefs, and cashier counters  
- Simple and visual desktop GUI with Windows Forms

---

## Technologies Used

- **C#** – Primary language  
- **Windows Forms** – UI layer  
- **Multithreading** – Thread, lock, SemaphoreSlim  
- **Data Structures** – Queue, List, Dictionary  
- **Randomized Simulation** – Randomized customer behavior

---

## Requirements

- Visual Studio 2022 or later  
- .NET 6.0 SDK or compatible  
- Windows OS (for WinForms)

---

## How It Works

- Customers are randomly generated and added to a waiting queue.  
- Waiters serve customers and forward orders to chefs.  
- Chefs process orders with a simulated delay and mark them ready.  
- The cashier processes payment once food is delivered.  
- Each unit (waiter, chef, cashier) is managed with its own `SemaphoreSlim` to control concurrency.

---

## Installation & Running

1. Clone the repository  
2. Open the `.sln` file in Visual Studio  
3. Build the solution  
4. Run the project  
5. The simulation starts automatically and you can observe the behavior on the form.

---

## Project Structure

- `Form1.cs` – Main form with core simulation logic  
- `Form2.cs` – Optional configuration or results form  
- `Program.cs` – Entry point  
- `bin/`, `obj/`, `Properties/` – Standard project folders

---

## License

This project is licensed under the **MIT License**.  
You are free to use, modify, and distribute it with attribution.

