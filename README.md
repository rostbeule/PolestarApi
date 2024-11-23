# Polestar API

[![Work in Progress](https://img.shields.io/badge/status-WIP-orange)](https://github.com/)  
A modern **ASP.NET 9.0** application built with **C#**, designed to provide a simplified and 
accessible wrapper around the **unofficial Polestar API**. This project is currently a 
**work in progress** and aims to make integration with Polestar's systems easier for developers.


## 🚀 Features

- **ASP.NET Core 9.0**: Built with the latest version of .NET for high performance and scalability.
- **Scalar Integration**: Provides interactive API documentation using Scalar.
- **RESTful Architecture**: Designed to follow REST principles for clean and maintainable endpoints.
- **Authentication Module**: Includes an initial implementation of user authentication with 
  token-based responses.
- **API Versioning**: Supports multiple API versions for backward compatibility.
- **CORS Support**: Configured to allow cross-origin requests for flexible client integration.
- **OpenAPI/Swagger Documentation**: Automatically generates OpenAPI specifications for better 
  developer experience.


## 📂 Project Structure

The project follows a clean architecture to ensure modularity and maintainability:

```
PolestarApi/
├── Contracts/          # Interfaces and shared models (e.g., AuthRequest, AuthResponse)
├── Authentication/     # Authentication logic (e.g., AuthService)
├── Controllers/        # API controllers (e.g., AuthController)
├── App/                # Application configuration (e.g., Startup.cs)
└── README.md           # Project documentation
```


## 🛠️ Technologies Used

- **C# 12**: The latest version of C# for modern, type-safe programming.
- **.NET 9.0**: A cutting-edge framework for building high-performance APIs.
- **Scalar.AspNetCore**: For interactive API documentation using Scalar.
- **Swashbuckle.AspNetCore**: For generating OpenAPI specifications.
- **Microsoft.AspNetCore.Mvc.Versioning**: To support API versioning.
- **Dependency Injection**: Built-in DI container for managing services.


## ⚙️ Getting Started

### Prerequisites

Ensure you have the following installed:
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- A code editor like [Visual Studio](https://visualstudio.microsoft.com/) or 
  [JetBrains Rider](https://www.jetbrains.com/rider/)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/<your-username>/polestar-api.git
   cd polestar-api
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

5. Access the API in your browser or Postman:
   - Scalar Documentation: `http://localhost:5000/scalar/v1`
   - Example Endpoint: `http://localhost:5000/api/v1/auth/token`


## ⚠️ Disclaimer

This application is not affiliated with Polestar or its official APIs. It is based on a 
reverse-engineered, unofficial Polestar API and is provided as-is for educational and integration 
purposes. Use this application responsibly and ensure compliance with any applicable terms of service.


## 🤝 Contributing

Contributions are welcome! Please follow the [Contribution Guidelines](CONTRIBUTING.md) when 
submitting changes.


## 📄 License

This project is licensed under the MIT License. See the `LICENSE` file for details.
