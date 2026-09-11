# FleetCare Pro

FleetCare Pro is an **ASP.NET Core MVC vehicle maintenance management system** designed to help fleet managers manage vehicles, drivers, service centers, maintenance records, and service costs in one centralized application.

The project demonstrates practical use of **ASP.NET Core MVC, Entity Framework Core, SQL Server, ASP.NET Core Identity, Razor Views, ViewModels, validation, file uploads, and role-based authorization**.

---

## Features

### Vehicle Management

Fleet managers and administrators can manage the fleet's vehicles.

* Add vehicles
* Edit vehicle information
* View vehicle details
* Delete vehicles
* Assign vehicles to drivers
* Track vehicle mileage
* Track vehicle status
* Store purchase price
* Upload vehicle images
* Replace existing vehicle images
* VIN validation
* Unique VIN enforcement

Supported vehicle statuses:

* `Active`
* `InService`
* `Decommissioned`

---

### Service Category Management

Service categories represent the different types of maintenance that can be performed.

Examples:

* Oil Change
* Brake Service
* Tire Replacement
* Engine Maintenance
* Battery Replacement

Each category contains:

* Category name
* Description
* Recommended maintenance interval

---

### Service Center Management

Fleet managers can manage external service centers used for vehicle maintenance.

Each service center contains:

* Name
* Phone number
* Email
* Address
* Active/Inactive status

Service centers can be associated with supported service categories.

---

### Service Record Management

FleetCare Pro supports detailed vehicle maintenance records.

Each service record contains:

* Vehicle
* Service center
* Service date
* Current mileage
* Maintenance status
* Notes
* Invoice document
* Total maintenance cost
* Service line items

Supported service statuses:

* `Pending`
* `Approved`
* `Completed`
* `Cancelled`

---

### Master-Detail Service Records

A service record can contain multiple service line items.

For example:

```text
Service Record
│
├── Oil Change       $50
├── Brake Inspection $30
├── Air Filter       $25
└── Labor            $40
```

The application calculates the total cost on the server instead of trusting the value submitted by the browser.

---

### Invoice Upload

Service records support invoice document uploads.

Supported formats:

* PDF
* JPG
* JPEG
* PNG

Maximum file size:

```text
5 MB
```

Invoices are stored under:

```text
wwwroot/uploads/invoices/
```

The database stores the path to the uploaded document.

When an invoice is replaced, the old file is removed.

---

### Vehicle Image Upload

Vehicle images are stored under:

```text
wwwroot/uploads/vehicles/
```

The application generates a unique filename using a GUID.

Example:

```text
/uploads/vehicles/7b7d6f2a-3c8f-4d6a-9a2f-example.jpg
```

The physical file is stored in `wwwroot`, while the URL is stored in the database.

---

## User Roles

FleetCare Pro uses **ASP.NET Core Identity** for role-based access.

### Roles

The application currently supports three roles:

| Role         | Purpose                                         |
| ------------ | ----------------------------------------------- |
| Admin        | System administration and overall management    |
| FleetManager | Fleet and maintenance management                |
| Driver       | Access to assigned vehicles and service history |

Role-based authorization is implemented using:

```csharp
[Authorize(Roles = "Admin")]
```

and:

```csharp
Initial Admin

Email : admin@fleetcare.com

Password : Admin@12345
```

```
[Authorize(Roles = "Driver")]
```
---

## Application User

FleetCare Pro extends the default ASP.NET Identity user with fleet-specific information.

```csharp
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }

    public string EmployeeId { get; set; }

    public ICollection<Vehicle> AssignedVehicles { get; set; }

    public ICollection<ServiceRecord> ServiceRecordsCreated { get; set; }
}
```

This allows users to be connected directly to fleet data.

---

## Driver Features

Drivers can only access vehicles assigned to their account.

### My Vehicles

Drivers can view:

* Assigned vehicles
* Vehicle information
* Current mileage
* Vehicle status

### Vehicle Details

Drivers can view detailed information about their assigned vehicle.

### Service History

Drivers can view the maintenance history of their vehicles, including:

* Service date
* Service center
* Service status
* Service categories
* Service costs
* Maintenance notes

A driver cannot access another driver's vehicle through the controller because the vehicle query checks the logged-in user's ID.

Example:

```csharp
.FirstOrDefaultAsync(v =>
    v.Id == id &&
    v.DriverId == userId);
```

---

## VIN Validation

FleetCare Pro includes a custom:

```csharp
[ValidVIN]
```

validation attribute.

The validator checks that the VIN:

* Contains exactly 17 characters
* Does not contain `I`
* Does not contain `O`
* Does not contain `Q`
* Uses valid VIN characters
* Passes the VIN checksum validation

VINs are also unique in the database.

```csharp
modelBuilder.Entity<Vehicle>()
    .HasIndex(v => v.VIN)
    .IsUnique();
```

---

## Admin Dashboard

Administrators have access to a dashboard containing fleet and maintenance statistics.

Current dashboard statistics include:

* Total vehicles
* Total service centers
* Total service categories
* Total service records
* Pending service records
* Completed service records
* Total maintenance cost

Example:

```text
Vehicles                 25
Service Centers           8
Service Categories       12
Service Records          74
Pending Services          6
Completed Services       61
Maintenance Cost     $12,450
```

---

## Database

FleetCare Pro uses:

* **Entity Framework Core**
* **SQL Server**
* **Code First migrations**

The main database context is:

```csharp
FleetCareDbContext
```

It inherits from:

```csharp
IdentityDbContext<ApplicationUser>
```
---

---

## Project Structure

```text
C:.
│   appsettings.Development.json
│   appsettings.json
│   FleetCare Pro.csproj
│   FleetCare Pro.csproj.user
│   Program.cs
│
├───Controllers
│       AccountController.cs
│       AdminController.cs
│       DriverController.cs
│       HomeController.cs
│       ServiceCategoryController.cs
│       ServiceCenterController.cs
│       ServiceRecordController.cs
│       VehicleController.cs
│
├───Data
│       DbInitializer.cs
│       FleetCareDbContext.cs
│
├───Filters
│       AuditLogAttribute.cs
│
├───Middleware
│       MaintenanceModeMiddleware.cs
│
├───Migrations
├───Models
│   │   ErrorViewModel.cs
│   │
│   ├───Domain
│   │       AuditLog.cs
│   │       ServiceCategory.cs
│   │       ServiceCenter.cs
│   │       ServiceLineItem.cs
│   │       ServiceRecord.cs
│   │       Vehicle.cs
│   │
│   ├───Enums
│   │       ServiceStatus.cs
│   │       VehicleStatus.cs
│   │
│   ├───Identity
│   │       ApplicationUser.cs
│   │
│   ├───Validation
│   │       ValidVINAttribute.cs
│   │
│   └───ViewModels
│       ├───Account
│       │       LoginViewModel.cs
│       │       RegisterViewModel.cs
│       │
│       ├───Dashboard
│       │       DashboardViewModel.cs
│       │
│       ├───ServiceCategory
│       │       ServiceCategoryCreateViewModel.cs
│       │       ServiceCategoryEditViewModel.cs
│       │
│       ├───ServiceCenter
│       │       ServiceCenterCreateViewModel.cs
│       │       ServiceCenterEditViewModel.cs
│       │
│       ├───ServiceRecord
│       │       ServiceLineItemViewModel.cs
│       │       ServiceRecordCreateViewModel.cs
│       │       ServiceRecordEditViewModel.cs
│       │
│       └───Vehicle
│               VehicleCreateViewModel.cs
│               VehicleDetailsViewModel.cs
│               VehicleEditViewModel.cs
├───Properties
│       launchSettings.json
│
├───ViewComponents
│       FleetCostSummaryViewComponent.cs
│       OverdueMaintenanceViewComponent.cs
│
├───Views
│   │   _ViewImports.cshtml
│   │   _ViewStart.cshtml
│   │
│   ├───Account
│   │       Login.cshtml
│   │       Register.cshtml
│   │
│   ├───Admin
│   │       Index.cshtml
│   │
│   ├───Driver
│   │       Index.cshtml
│   │       ServiceHistory.cshtml
│   │       Vehicle.cshtml
│   │
│   ├───Home
│   │       Error.cshtml
│   │       Index.cshtml
│   │       Maintenance.cshtml
│   │       Privacy.cshtml
│   │       StatusCode.cshtml
│   │
│   ├───ServiceCategory
│   │       Create.cshtml
│   │       Delete.cshtml
│   │       Details.cshtml
│   │       Edit.cshtml
│   │       Index.cshtml
│   │
│   ├───ServiceCenter
│   │       Create.cshtml
│   │       Delete.cshtml
│   │       Details.cshtml
│   │       Edit.cshtml
│   │       Index.cshtml
│   │
│   ├───ServiceRecord
│   │       Create.cshtml
│   │       Delete.cshtml
│   │       Details.cshtml
│   │       Edit.cshtml
│   │       Index.cshtml
│   │       MyServices.cshtml
│   │
│   ├───Shared
│   │   │   Error.cshtml
│   │   │   _Layout.cshtml
│   │   │   _Layout.cshtml.css
│   │   │   _ValidationScriptsPartial.cshtml
│   │   │   _VehicleCard.cshtml
│   │   │
│   │   └───Components
│   │       ├───FleetCostSummary
│   │       │       Default.cshtml
│   │       │
│   │       └───OverdueMaintenance
│   │               Default.cshtml
│   │
│   └───Vehicle
│           Create.cshtml
│           Delete.cshtml
│           Details.cshtml
│           Edit.cshtml
│           Index.cshtml
│
└───wwwroot
    │   favicon.ico   │
    ├───css
    │
    ├───js
    │
    ├───lib
    │   
    │
    └───uploads
        ├───invoices
        └───vehicles
                
```

---

## ViewModels

FleetCare Pro uses ViewModels instead of directly using domain models for complex forms.

This allows form-specific validation and prevents unnecessary domain properties from being exposed directly to the UI.

---

## Configuration

The SQL Server connection string is stored in:

```text
appsettings.json
```

Update the connection string according to your SQL Server configuration.

---

## Running the Project

### 1. Clone the repository

```bash
git clone https://github.com/Ziadmohamed65/FleetCare-Pro-Vehicle-Maintenance-System.git
```

### 2. Open the project

Open:

```text
FleetCare_Pro.sln
```

using Visual Studio.

### 3. Configure the database

Update the connection string in:

```text
appsettings.json
```

### 4. Apply migrations

Using Package Manager Console:

```powershell
Update-Database
```

### 5. Run the application

Press F5 in Visual Studio.

---

FleetCare Pro was developed as a practical project for learning and demonstrating modern **ASP.NET Core MVC and Entity Framework Core development**.
