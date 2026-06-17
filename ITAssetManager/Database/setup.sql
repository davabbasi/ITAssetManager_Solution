-- =====================================================
-- IT Asset Manager - Database Setup Script
-- این اسکریپت را فقط اگر EF Migration کار نکرد اجرا کنید
-- =====================================================

-- ساخت دیتابیس
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ITAssetManagerDB')
    CREATE DATABASE ITAssetManagerDB;
GO

USE ITAssetManagerDB;
GO

-- جدول بخش‌ها
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Departments')
CREATE TABLE Departments (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive    BIT NOT NULL DEFAULT 1
);

-- جدول کارمندان
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
CREATE TABLE Employees (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    FullName       NVARCHAR(100) NOT NULL,
    EmployeeCode   NVARCHAR(50)  NULL,
    Position       NVARCHAR(100) NULL,
    DepartmentId   INT NOT NULL REFERENCES Departments(Id),
    IsActive       BIT NOT NULL DEFAULT 1
);

-- جدول دسته‌بندی تجهیزات
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AssetCategories')
CREATE TABLE AssetCategories (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Icon        NVARCHAR(50)  NULL DEFAULT 'bi-box',
    Description NVARCHAR(500) NULL
);

-- جدول تجهیزات
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Assets')
CREATE TABLE Assets (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    Name           NVARCHAR(200) NOT NULL,
    Brand          NVARCHAR(100) NULL,
    Model          NVARCHAR(100) NULL,
    SerialNumber   NVARCHAR(100) NULL,
    Barcode        NVARCHAR(100) NULL,
    PropertyTag    NVARCHAR(100) NULL,
    CategoryId     INT NOT NULL REFERENCES AssetCategories(Id),
    Status         INT NOT NULL DEFAULT 1,
    StatusNote     NVARCHAR(500) NULL,
    PurchaseDate   DATE NULL,
    WarrantyExpiry DATE NULL,
    PurchasePrice  DECIMAL(18,0) NULL,
    DepartmentId   INT NULL REFERENCES Departments(Id),
    EmployeeId     INT NULL REFERENCES Employees(Id),
    Location       NVARCHAR(200) NULL,
    Specs          NVARCHAR(MAX) NULL,
    Notes          NVARCHAR(MAX) NULL,
    CreatedAt      DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt      DATETIME NULL
);

-- جدول تاریخچه جابجایی
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AssetAssignments')
CREATE TABLE AssetAssignments (
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    AssetId          INT NOT NULL REFERENCES Assets(Id),
    FromEmployeeId   INT NULL REFERENCES Employees(Id),
    FromDepartmentId INT NULL REFERENCES Departments(Id),
    FromLocation     NVARCHAR(200) NULL,
    ToEmployeeId     INT NULL REFERENCES Employees(Id),
    ToDepartmentId   INT NULL REFERENCES Departments(Id),
    ToLocation       NVARCHAR(200) NULL,
    AssignedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    Reason           NVARCHAR(500) NULL,
    AssignedBy       NVARCHAR(200) NULL,
    Notes            NVARCHAR(MAX) NULL
);

-- جدول تعمیرات
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenanceLogs')
CREATE TABLE MaintenanceLogs (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    AssetId         INT NOT NULL REFERENCES Assets(Id),
    Type            INT NOT NULL DEFAULT 1,
    Description     NVARCHAR(MAX) NOT NULL,
    TechnicianName  NVARCHAR(100) NULL,
    Cost            DECIMAL(18,0) NULL,
    StartDate       DATETIME NOT NULL DEFAULT GETDATE(),
    EndDate         DATETIME NULL,
    IsResolved      BIT NOT NULL DEFAULT 0,
    Resolution      NVARCHAR(MAX) NULL,
    Notes           NVARCHAR(MAX) NULL,
    LoggedBy        NVARCHAR(200) NULL
);

-- درج دسته‌بندی‌های پیش‌فرض
IF NOT EXISTS (SELECT 1 FROM AssetCategories)
BEGIN
    INSERT INTO AssetCategories (Name, Icon) VALUES
    (N'لپ‌تاپ',          'bi-laptop'),
    (N'کامپیوتر رومیزی', 'bi-pc-display'),
    (N'مانیتور',          'bi-display'),
    (N'پرینتر',           'bi-printer'),
    (N'ماوس',             'bi-mouse'),
    (N'کیبورد',           'bi-keyboard'),
    (N'سوئیچ شبکه',      'bi-hdd-network'),
    (N'روتر',             'bi-router'),
    (N'سرور',             'bi-server'),
    (N'UPS',              'bi-battery-charging'),
    (N'هدست',             'bi-headset'),
    (N'سایر',             'bi-box');
END

PRINT 'Setup complete!';
GO
