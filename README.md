# 🏪 KoneMoshapoRetail - Azure Cloud Storage Solution

![Azure](https://img.shields.io/badge/Azure-0078D4?style=for-the-badge&logo=microsoft-azure&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![GitHub](https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white)

## 📋 Project Overview

KoneMoshapoRetail is a cloud-native web application built for **ABC Retail** to modernize their order processing system. The application leverages **Azure Storage Services** to provide scalable, reliable, and cost-effective solutions for managing customer profiles, product information, order processing, inventory management, and application logging.

### 🎯 Key Features

| Feature | Technology | Description |
|---------|------------|-------------|
| 👤 **Customer Management** | Azure Table Storage | Store and manage customer profiles with CRUD operations |
| 📦 **Product Management** | Azure Table Storage | Manage product inventory with image support |
| 🖼️ **Image Storage** | Azure Blob Storage | Upload and display product images |
| 📨 **Order Processing** | Azure Queue Storage | Asynchronous order processing via queues |
| 📊 **Inventory Updates** | Azure Queue Storage | Track inventory changes in real-time |
| 📝 **Application Logs** | Azure File Storage | Store and manage log files |
| 🗄️ **Data Persistence** | Azure SQL Database | Relational data storage for orders and transactions |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      KoneMoshapoRetail                         │
│                    ASP.NET Core MVC Application                │
└─────────────────────────────────────────────────────────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        │                       │                       │
        ▼                       ▼                       ▼
┌───────────────┐     ┌─────────────────┐     ┌─────────────┐
│ Azure Table   │     │  Azure Blob     │     │ Azure Queue │
│ Storage       │     │  Storage        │     │ Storage     │
│               │     │                 │     │             │
│ • Customers   │     │ • Product       │     │ • Orders    │
│ • Products    │     │   Images        │     │ • Inventory │
└───────────────┘     └─────────────────┘     └─────────────┘
        │                       │                       │
        └───────────────────────┼───────────────────────┘
                                │
                                ▼
                    ┌─────────────────────┐
                    │  Azure File Storage │
                    │  • Application Logs │
                    └─────────────────────┘
                                │
                                ▼
                    ┌─────────────────────┐
                    │  Azure SQL Database │
                    │  • Orders           │
                    │  • OrderItems       │
                    │  • Transactions     │
                    └─────────────────────┘
```

---

## 🚀 Live Demo

🌐 **Web Application:** [https://konemoshaporetail20260813090115-hvetdjaxazh9c9g0.southafricanorth-01.azurewebsites.net](https://konemoshaporetail20260813090115-hvetdjaxazh9c9g0.southafricanorth-01.azurewebsites.net)

### Live Application Statistics

| Statistic | Count |
|-----------|-------|
| 👤 Total Customers | **5** ✅ |
| 📦 Total Products | **4** ✅ |
| 📋 Pending Orders | **4** ✅ |
| 📝 Log Files | **23** ✅ |

---

## 📂 Project Structure

```
KoneMoshapoRetail/
├── Controllers/
│   ├── HomeController.cs          # Dashboard and home page
│   ├── CustomersController.cs     # Customer CRUD operations
│   ├── ProductsController.cs      # Product CRUD operations
│   ├── OrdersController.cs        # Order processing
│   └── LogsController.cs          # Log management
├── Models/
│   ├── CustomerProfile.cs         # Customer entity (Table Storage)
│   ├── ProductInfo.cs             # Product entity (Table Storage)
│   ├── OrderMessage.cs            # Order message (Queue Storage)
│   └── LogEntry.cs                # Log entry (File Storage)
├── Services/
│   ├── ITableStorageService.cs    # Table Storage interface
│   ├── TableStorageService.cs     # Table Storage implementation
│   ├── IBlobStorageService.cs     # Blob Storage interface
│   ├── BlobStorageService.cs      # Blob Storage implementation
│   ├── IQueueStorageService.cs    # Queue Storage interface
│   ├── QueueStorageService.cs     # Queue Storage implementation
│   ├── IFileStorageService.cs     # File Storage interface
│   └── FileStorageService.cs      # File Storage implementation
├── Views/
│   ├── Home/                      # Dashboard views
│   ├── Customers/                 # Customer views
│   ├── Products/                  # Product views
│   ├── Orders/                    # Order views
│   └── Logs/                      # Log views
├── wwwroot/
│   ├── css/                       # Stylesheets
│   └── js/                        # JavaScript files
├── Program.cs                     # Application entry point
├── appsettings.json               # Application configuration
└── KoneMoshapoRetail.csproj       # Project file
```

---

## 🛠️ Technologies Used

### Backend
- **Framework:** ASP.NET Core 8.0
- **Language:** C#
- **ORM:** Dapper / ADO.NET
- **Cloud Provider:** Microsoft Azure

### Azure Services

| Service | Purpose |
|---------|---------|
| **Azure Table Storage** | Customer profiles, product information |
| **Azure Blob Storage** | Product images, multimedia content |
| **Azure Queue Storage** | Order processing, inventory updates |
| **Azure File Storage** | Application logs |
| **Azure SQL Database** | Orders, transactions, relational data |
| **Azure App Service** | Web application hosting |

### Frontend
- **Razor Views** with Bootstrap 4
- **JavaScript/jQuery** for dynamic interactions
- **Font Awesome** icons
- **Custom CSS** for professional styling

---

## 📋 Prerequisites

- [Visual Studio 2022+](https://visualstudio.microsoft.com/)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Azure Subscription](https://azure.microsoft.com/en-us/free/) (Student credits available)
- [Git](https://git-scm.com/) for version control

---

## 🔧 Setup Instructions

### 1. Clone the Repository
```bash
git clone https://github.com/Noshy321/CloudPROJECT1-KoneMoshapoRetail.git
cd CloudPROJECT1-KoneMoshapoRetail
```

### 2. Update Configuration
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:YOUR_SERVER.database.windows.net,1433;Database=YOUR_DB;User ID=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;"
  },
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=YOUR_ACCOUNT;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net"
  }
}
```

### 3. Run the Application
```bash
dotnet restore
dotnet build
dotnet run
```

### 4. Access the Application
```
https://localhost:44365/
```

---

## 📸 Screenshots

### Dashboard
![Dashboard](https://via.placeholder.com/600x400/2c3e50/ffffff?text=Dashboard)

### Customer Management
![Customers](https://via.placeholder.com/600x400/2c3e50/ffffff?text=Customer+Management)

### Product Management
![Products](https://via.placeholder.com/600x400/2c3e50/ffffff?text=Product+Management)

### Order Queue
![Orders](https://via.placeholder.com/600x400/2c3e50/ffffff?text=Order+Queue)

### Log Management
![Logs](https://via.placeholder.com/600x400/2c3e50/ffffff?text=Log+Management)

---

## 🚀 Deployment

### Deploy to Azure App Service

1. **Create App Service** in Azure Portal
2. **Publish** from Visual Studio:
   ```
   Right-click project → Publish → Azure App Service
   ```
3. **Configure App Settings**:
   ```
   AzureStorage__ConnectionString: [Your Storage Connection String]
   ConnectionStrings__DefaultConnection: [Your SQL Connection String]
   ASPNETCORE_ENVIRONMENT: Production
   ```

4. **Access your app**:
   ```
   https://konemoshaporetail20260813090115-hvetdjaxazh9c9g0.southafricanorth-01.azurewebsites.net
   ```

---

## 📊 Database Schema

### Tables Created

| Table | Purpose |
|-------|---------|
| `Customers` | Customer profiles and contact information |
| `Products` | Product details, pricing, inventory |
| `Orders` | Order headers, totals, status |
| `OrderItems` | Individual order line items |
| `InventoryTransactions` | Stock movement tracking |
| `AuditLogs` | System activity logs |
| `ShoppingCart` | Customer cart items |
| `Wishlist` | Customer wishlist items |
| `ProductReviews` | Product ratings and reviews |
| `Categories` | Product categories |
| `SystemConfig` | System configuration settings |
| `Notifications` | Customer notifications |
| `DiscountCoupons` | Promotional coupons |
| `PaymentTransactions` | Payment processing records |
| `UserActivityLog` | User activity tracking |

---

## 📝 API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/dashboard/stats` | GET | Get dashboard statistics |
| `/Customers` | GET/POST | Customer management |
| `/Products` | GET/POST | Product management |
| `/Orders` | GET/POST | Order processing |
| `/Logs` | GET/POST | Log management |

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is for educational purposes as part of the **IIE Cloud Development (BCLDV7112/w)** module.

---

## 👤 Author

**Student Number:** ST10365593  
**Module:** BCLDV7112/w  
**Institution:** IIE Rosebank College  
**Project:** Project 1 - Azure Storage Solution

---

## 🙏 Acknowledgments

- Microsoft Azure Documentation
- IIE Module Content (Learning Units 1-3)
- ABC Retail Case Study

---

## 📞 Contact

- **GitHub:** [Noshy321](https://github.com/Noshy321)
- **Email:** KONEMOSHAPO@gmail.com

---

## 🔗 Links

| Resource | URL |
|----------|-----|
| **Web Application** | https://konemoshaporetail20260813090115-hvetdjaxazh9c9g0.southafricanorth-01.azurewebsites.net |
| **GitHub Repository** | https://github.com/Noshy321/CloudPROJECT1-KoneMoshapoRetail |
| **Azure Portal** | https://portal.azure.com |

---

## 📊 Project Status

| Component | Status |
|-----------|--------|
| Azure Table Storage | ✅ Completed |
| Azure Blob Storage | ✅ Completed |
| Azure Queue Storage | ✅ Completed |
| Azure File Storage | ✅ Completed |
| Azure SQL Database | ✅ Completed |
| Web Application | ✅ Completed |
| Deployment | ✅ Completed |
| Documentation | ✅ Completed |

---

## 🎯 Future Enhancements

- [ ] Add user authentication (Microsoft Entra ID)
- [ ] Implement real-time notifications with SignalR
- [ ] Add reporting and analytics dashboard
- [ ] Integrate with Azure Functions for background processing
- [ ] Implement CI/CD pipeline with Azure DevOps
- [ ] Add multi-language support
- [ ] Implement payment gateway integration

---

> **Built with ❤️ using Microsoft Azure and .NET 8**

---

*Last Updated: August 13, 2026*
