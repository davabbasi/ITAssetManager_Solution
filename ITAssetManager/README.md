# 🖥️ IT Asset Manager
**سیستم مدیریت تجهیزات IT شرکت**

---

## ✅ پیش‌نیازها

| نرم‌افزار | نسخه |
|---|---|
| .NET SDK | 8.0 یا بالاتر |
| SQL Server | 2019 یا بالاتر (Express هم کافیه) |
| Visual Studio | 2022 یا VS Code |

---

## 🚀 راه‌اندازی - مرحله به مرحله

### مرحله ۱: رشته اتصال
فایل `appsettings.json` را باز کنید و رشته اتصال را ویرایش کنید:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ITAssetManagerDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

- اگر SQL Server روی همون کامپیوتره: `Server=.` یا `Server=localhost`
- اگر SQL Express: `Server=.\SQLEXPRESS`
- اگر SQL Server دیگه: آدرس سرور را وارد کنید

### مرحله ۲: ساخت دیتابیس (EF Migration)

در ترمینال، داخل پوشه پروژه:

```bash
# نصب EF Tools (یک بار انجام بدید)
dotnet tool install --global dotnet-ef

# ساخت Migration
dotnet ef migrations add InitialCreate

# اجرای Migration و ساخت دیتابیس
dotnet ef database update
```

> **اگر EF Migration کار نکرد:**
> فایل `Database/setup.sql` را در SQL Server Management Studio اجرا کنید.

### مرحله ۳: اجرای پروژه

```bash
dotnet run
```

یا از Visual Studio، کلید F5 را بزنید.

---

## 🔑 اطلاعات ورود پیش‌فرض

| فیلد | مقدار |
|---|---|
| ایمیل | `admin@itasset.local` |
| رمز عبور | `Admin@123` |
| نقش | Admin |

> ⚠️ بعد از اولین ورود، رمز را از پنل Admin > کاربران تغییر دهید.

---

## 📋 امکانات سیستم

### تجهیزات
- ✅ ثبت، ویرایش، جستجو و فیلتر تجهیزات
- ✅ اطلاعات کامل: برچسب اموال، سریال، بارکد، برند، مدل
- ✅ تاریخ خرید و انقضای گارانتی
- ✅ وضعیت: فعال / معیوب / در تعمیر / اسقاط / انبار
- ✅ تخصیص به کارمند و بخش

### ردیابی و جابجایی
- ✅ ثبت جابجایی تجهیزات بین کارمندان/بخش‌ها
- ✅ تاریخچه کامل جابجایی‌ها
- ✅ ثبت خودکار زمان و ثبت‌کننده

### تعمیرات
- ✅ ثبت مشکلات و تعمیرات
- ✅ انواع: تعمیر / پیشگیرانه / ارتقا / بازرسی
- ✅ ثبت هزینه و تکنسین
- ✅ آپدیت خودکار وضعیت تجهیز

### گزارش‌ها
- ✅ گزارش گارانتی (منقضی / در حال انقضا / معتبر)
- ✅ گزارش بخش‌ها
- ✅ گزارش اسقاطی‌ها و معیوب‌ها
- ✅ داشبورد با آمار کلی

### مدیریت
- ✅ سیستم Login با دو نقش Admin / User
- ✅ مدیریت کاربران (ایجاد، حذف، تغییر نقش)
- ✅ مدیریت بخش‌ها و کارمندان
- ✅ مدیریت دسته‌بندی تجهیزات

---

## 🏗️ ساختار پروژه

```
ITAssetManager/
├── Data/
│   └── ApplicationDbContext.cs     ← EF Core Context
├── Models/
│   ├── Asset.cs                    ← مدل تجهیز
│   ├── AssetAssignment.cs          ← جابجایی
│   ├── AssetCategory.cs            ← دسته‌بندی
│   ├── Department.cs               ← بخش
│   ├── Employee.cs                 ← کارمند
│   └── MaintenanceLog.cs           ← تعمیر
├── Pages/
│   ├── Assets/                     ← مدیریت تجهیزات
│   ├── Assignments/                ← جابجایی‌ها
│   ├── Maintenance/                ← تعمیرات
│   ├── Reports/                    ← گزارش‌ها
│   ├── Admin/                      ← پنل مدیریت
│   └── Account/                    ← Login/Logout
├── Database/
│   └── setup.sql                   ← اسکریپت SQL دستی
└── appsettings.json                ← تنظیمات
```

---

## 🎨 UI / تکنولوژی‌ها

- **ASP.NET Core 8** + Razor Pages
- **SQL Server** + Entity Framework Core 8
- **Bootstrap 5 RTL** (فارسی / راست به چپ)
- **Bootstrap Icons**
- **Vazirmatn** (فونت فارسی)

---

## 📞 نکات مهم

1. **برچسب اموال** از واحد مالی دریافت می‌شه و در فرم ثبت تجهیز وارد میشه
2. **جابجایی** بصورت خودکار اطلاعات تجهیز را آپدیت می‌کنه
3. **تعمیر حل‌شده** بصورت خودکار وضعیت تجهیز رو به "فعال" برمیگردونه
4. **گارانتی در خطر** در داشبورد نمایش داده میشه (کمتر از ۶۰ روز)
