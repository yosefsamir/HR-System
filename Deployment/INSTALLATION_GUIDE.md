# دليل تثبيت نظام الموارد البشرية
# HR System Installation Guide

---

## المتطلبات | Requirements

### 1. البرامج المطلوبة | Required Software

| البرنامج | الوصف | رابط التحميل |
|---------|-------|-------------|
| .NET 9.0 Runtime | لتشغيل التطبيق | [تحميل](https://dotnet.microsoft.com/download/dotnet/9.0) |
| SQL Server Express | قاعدة البيانات | [تحميل](https://www.microsoft.com/sql-server/sql-server-downloads) |
| SQL Server Management Studio | إدارة قاعدة البيانات (اختياري) | [تحميل](https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) |

### 2. متطلبات الجهاز | Hardware Requirements
- معالج: Intel Core i3 أو أعلى
- ذاكرة: 4 GB RAM على الأقل
- مساحة: 500 MB متاحة
- نظام التشغيل: Windows 10/11

---

## خطوات التثبيت | Installation Steps

### الخطوة 1: تثبيت .NET Runtime

1. افتح الرابط: https://dotnet.microsoft.com/download/dotnet/9.0
2. اختر **ASP.NET Core Runtime 9.0** (Windows x64)
3. قم بتحميل وتثبيت الملف
4. للتحقق من التثبيت، افتح CMD واكتب:
   ```
   dotnet --list-runtimes
   ```
   يجب أن ترى `Microsoft.AspNetCore.App 9.x.x`

### الخطوة 2: تثبيت SQL Server Express

1. افتح الرابط: https://www.microsoft.com/sql-server/sql-server-downloads
2. اختر **Express** (مجاني)
3. اختر **Basic** installation
4. أثناء التثبيت، سجل:
   - اسم السيرفر (عادة: `localhost` أو `.\SQLEXPRESS`)
   - كلمة مرور `sa` (إذا اخترت Mixed Mode Authentication)

### الخطوة 3: تحضير قاعدة البيانات

#### الخيار أ: إنشاء قاعدة البيانات تلقائياً
النظام سيقوم بإنشاء قاعدة البيانات تلقائياً عند أول تشغيل.

#### الخيار ب: إنشاء يدوياً (اختياري)
1. افتح SQL Server Management Studio
2. اتصل بالسيرفر
3. انقر بزر الماوس الأيمن على Databases
4. اختر New Database
5. سمها: `HRSystemDB`

### الخطوة 4: تثبيت النظام

1. انسخ مجلد `publish` إلى الجهاز المستهدف
2. ضعه في مكان دائم مثل `C:\HRSystem`
3. افتح ملف `appsettings.Production.json` بالمفكرة
4. عدّل إعدادات الاتصال:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=HRSystemDB;User Id=sa;Password=كلمة_المرور;Encrypt=True;TrustServerCertificate=True;"
     }
   }
   ```
5. احفظ الملف

### الخطوة 5: تشغيل التثبيت

1. انقر بزر الماوس الأيمن على `Install.bat`
2. اختر **Run as administrator**
3. اتبع التعليمات على الشاشة
4. سيتم إنشاء اختصار على سطح المكتب

### الخطوة 6: تشغيل النظام

1. انقر مرتين على اختصار **نظام الموارد البشرية** على سطح المكتب
2. سيفتح المتصفح تلقائياً على العنوان: http://localhost:5000
3. ابدأ باستخدام النظام!

---

## هيكل الملفات | File Structure

```
C:\HRSystem\
├── HRSystem.exe              # ملف التطبيق الرئيسي
├── appsettings.json          # إعدادات عامة
├── appsettings.Production.json # إعدادات الإنتاج
├── StartHRSystem.bat         # تشغيل النظام
├── StopHRSystem.bat          # إيقاف النظام
├── UpdateHRSystem.bat        # تحديث النظام
├── wwwroot/                  # ملفات الويب
├── Backup/                   # نسخ احتياطية
├── Update/                   # ملفات التحديث
└── Logs/                     # سجلات النظام
```

---

## التحديث | Update Process

### كيفية تحديث النظام:

1. **احصل على ملفات التحديث الجديدة من المطور**
2. **انسخ الملفات الجديدة إلى مجلد `Update`**
   ```
   C:\HRSystem\Update\
   ├── HRSystem.exe
   ├── (باقي الملفات الجديدة)
   ```
3. **شغّل `UpdateHRSystem.bat`**
   - سيقوم بإيقاف النظام
   - نسخ احتياطي للإصدار الحالي
   - نسخ الملفات الجديدة
   - تحديث قاعدة البيانات إذا لزم الأمر
4. **شغّل النظام من جديد**

### ملاحظات مهمة:
- ✅ البيانات لن تتأثر بالتحديث
- ✅ يتم حفظ نسخة احتياطية تلقائياً
- ✅ قاعدة البيانات يتم تحديثها تلقائياً
- ⚠️ لا تحذف مجلد `Backup` لحين التأكد من عمل الإصدار الجديد

---

## حل المشاكل | Troubleshooting

### المشكلة: النظام لا يعمل

**الحل:**
1. تأكد من تثبيت .NET Runtime
2. تأكد من تشغيل SQL Server
3. تحقق من إعدادات الاتصال في `appsettings.Production.json`

### المشكلة: لا يمكن الاتصال بقاعدة البيانات

**الحل:**
1. افتح Services (اكتب `services.msc` في Run)
2. ابحث عن SQL Server
3. تأكد أنه Running
4. إذا لم يكن، انقر بزر الماوس الأيمن واختر Start

### المشكلة: الصفحة لا تفتح في المتصفح

**الحل:**
1. تأكد من أن المنفذ 5000 غير مستخدم
2. جرب فتح http://localhost:5000 يدوياً
3. تحقق من جدار الحماية (Firewall)

### المشكلة: خطأ في التحديث

**الحل:**
1. استعد الإصدار السابق من مجلد `Backup`
2. انسخ الملف الاحتياطي إلى المجلد الرئيسي
3. أعد تسميته إلى `HRSystem.exe`
4. شغّل النظام

---

## النسخ الاحتياطي | Backup

### نسخ قاعدة البيانات:

#### باستخدام SQL Server Management Studio:
1. افتح SSMS
2. انقر بزر الماوس الأيمن على `HRSystemDB`
3. Tasks → Back Up
4. اختر مكان الحفظ
5. انقر OK

#### باستخدام الأوامر:
```sql
BACKUP DATABASE HRSystemDB 
TO DISK = 'C:\Backup\HRSystemDB.bak'
WITH FORMAT, COMPRESSION;
```

### استعادة قاعدة البيانات:

```sql
RESTORE DATABASE HRSystemDB 
FROM DISK = 'C:\Backup\HRSystemDB.bak'
WITH REPLACE;
```

---

## الدعم الفني | Technical Support

للحصول على المساعدة:
- البريد الإلكتروني: support@yourcompany.com
- الهاتف: xxx-xxxx-xxxx

---

**الإصدار:** 1.0.0  
**تاريخ التحديث:** ديسمبر 2024

---

## ملاحظة للمطور | Developer Note

### البناء من Linux لـ Windows

إذا كنت تطور على Linux وتريد النشر على Windows:

```bash
# من مجلد Deployment
./build-for-windows.sh
```

هذا السكربت يقوم بـ:
1. بناء التطبيق لـ Windows x64
2. تحويل ملفات `.bat` لتعمل على Windows
3. نسخ جميع الملفات المطلوبة

### الملفات المنتجة
```
Deployment/publish/
├── HRSystem.exe          # التطبيق الرئيسي (Windows)
├── *.dll                 # المكتبات المطلوبة
├── *.bat                 # سكربتات التشغيل
├── wwwroot/              # ملفات الويب
└── appsettings.*.json    # ملفات الإعدادات
```

### نقل الملفات للعميل
يمكنك:
- نسخ مجلد `publish` على USB
- ضغط المجلد وإرساله عبر الإنترنت
- استخدام أي طريقة نقل ملفات
