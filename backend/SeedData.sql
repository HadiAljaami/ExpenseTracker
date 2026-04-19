-- =====================================================
-- ExpenseTracker - Seed Data Script
-- تشغيل هذا السكربت بعد إنشاء قاعدة البيانات
-- المستخدم الموجود: hadi@gmail.com (Id = 1)
-- =====================================================

USE ExpenseTrackerDb;
GO

-- =====================================================
-- 1. Budgets - ميزانيات شهر أبريل 2026
-- =====================================================
INSERT INTO Budgets (UserId, CategoryId, MonthlyLimit, Month, Year, CreatedAt)
VALUES
    (1, NULL, 5000.00, 4, 2026, GETUTCDATE()),   -- ميزانية إجمالية
    (1, 1,    800.00,  4, 2026, GETUTCDATE()),   -- طعام
    (1, 2,    400.00,  4, 2026, GETUTCDATE()),   -- مواصلات
    (1, 3,    600.00,  4, 2026, GETUTCDATE()),   -- فواتير
    (1, 4,    300.00,  4, 2026, GETUTCDATE()),   -- ترفيه
    (1, 6,    500.00,  4, 2026, GETUTCDATE());   -- تسوق
GO

-- =====================================================
-- 2. Expenses - مصروفات أبريل 2026
-- =====================================================
INSERT INTO Expenses (UserId, CategoryId, Amount, Description, Date, CreatedAt)
VALUES
-- الأسبوع الأول
(1, 1,  45.50,  'غداء مطعم الأصيل',        '2026-04-01', GETUTCDATE()),
(1, 2,  80.00,  'بنزين',                    '2026-04-01', GETUTCDATE()),
(1, 1,  25.00,  'فطور كافيه',               '2026-04-02', GETUTCDATE()),
(1, 6,  150.00, 'ملابس',                    '2026-04-03', GETUTCDATE()),
(1, 1,  60.00,  'عشاء عائلي',               '2026-04-04', GETUTCDATE()),
(1, 4,  55.00,  'سينما',                    '2026-04-05', GETUTCDATE()),
(1, 2,  40.00,  'أوبر',                     '2026-04-06', GETUTCDATE()),
(1, 1,  35.00,  'غداء',                     '2026-04-07', GETUTCDATE()),

-- الأسبوع الثاني
(1, 3,  250.00, 'فاتورة كهرباء',            '2026-04-08', GETUTCDATE()),
(1, 1,  90.00,  'بقالة أسبوعية',            '2026-04-09', GETUTCDATE()),
(1, 5,  200.00, 'طبيب',                     '2026-04-10', GETUTCDATE()),
(1, 7,  350.00, 'كتب جامعية',               '2026-04-11', GETUTCDATE()),
(1, 1,  45.00,  'فطور + قهوة',              '2026-04-12', GETUTCDATE()),
(1, 2,  80.00,  'بنزين',                    '2026-04-13', GETUTCDATE()),
(1, 4,  120.00, 'اشتراك نتفليكس + سبوتيفاي','2026-04-14', GETUTCDATE()),

-- الأسبوع الثالث
(1, 1,  75.00,  'مطعم مع الأصدقاء',        '2026-04-15', GETUTCDATE()),
(1, 3,  180.00, 'فاتورة ماء وإنترنت',       '2026-04-16', GETUTCDATE()),
(1, 6,  220.00, 'أدوات منزلية',             '2026-04-17', GETUTCDATE()),
(1, 1,  55.00,  'بقالة',                    '2026-04-18', GETUTCDATE()),
(1, 2,  60.00,  'أوبر + كريم',              '2026-04-18', GETUTCDATE()),
(1, 4,  80.00,  'فعالية رياضية',            '2026-04-18', GETUTCDATE()),

-- الأسبوع الرابع
(1, 1,  110.00, 'عشاء عيد ميلاد',          '2026-04-19', GETUTCDATE()),
(1, 5,  150.00, 'دواء صيدلية',              '2026-04-19', GETUTCDATE()),
(1, 6,  180.00, 'تسوق إلكتروني',            '2026-04-19', GETUTCDATE());
GO

-- =====================================================
-- 3. Expenses - مصروفات مارس 2026 (للمقارنة في Insights)
-- =====================================================
INSERT INTO Expenses (UserId, CategoryId, Amount, Description, Date, CreatedAt)
VALUES
(1, 1,  400.00, 'طعام مارس',       '2026-03-15', GETUTCDATE()),
(1, 2,  200.00, 'مواصلات مارس',    '2026-03-15', GETUTCDATE()),
(1, 3,  350.00, 'فواتير مارس',     '2026-03-15', GETUTCDATE()),
(1, 4,  180.00, 'ترفيه مارس',      '2026-03-15', GETUTCDATE()),
(1, 6,  270.00, 'تسوق مارس',       '2026-03-15', GETUTCDATE()),
(1, 5,  120.00, 'صحة مارس',        '2026-03-15', GETUTCDATE()),
(1, 7,  200.00, 'تعليم مارس',      '2026-03-15', GETUTCDATE()),
(1, 8,  80.00,  'متفرقات مارس',    '2026-03-15', GETUTCDATE());
GO

-- =====================================================
-- 4. عرض ملخص البيانات المدخلة
-- =====================================================
SELECT 'Budgets' AS TableName, COUNT(*) AS TotalRows FROM Budgets WHERE UserId = 1
UNION ALL
SELECT 'Expenses April', COUNT(*) FROM Expenses WHERE UserId = 1 AND MONTH(Date) = 4
UNION ALL
SELECT 'Expenses March', COUNT(*) FROM Expenses WHERE UserId = 1 AND MONTH(Date) = 3
UNION ALL
SELECT 'Total Expenses Amount April', SUM(Amount) FROM Expenses WHERE UserId = 1 AND MONTH(Date) = 4;
GO

PRINT '✅ Seed data inserted successfully!';
GO
