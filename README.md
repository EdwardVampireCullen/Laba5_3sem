📁 Структура проекта
Laba5_3sem/ # Корневая папка
├── Laba5_3sem.sln # Файл решения (Solution)
├── Laba5_3sem/ # Основная папка проекта
│ ├── Form1.cs # Главная форма (выбор режима)
│ ├── Form2.cs # Форма управления пользователями
│ ├── DatabaseHelper.cs # Работа с базой данных
│ ├── Person.cs # Модель пользователя
│ ├── Form1.Designer.cs # Дизайн Form1
│ ├── Form2.Designer.cs # Дизайн Form2
│ └── Program.cs # Точка входа
├── DatabaseBackup/
│ └── Laba5DB_Backup.sql # Скрипт восстановления БД
└── README.md # Этот файл

2. Настройте базу данных
Установите SQL Server (Express версия бесплатна)

Создайте базу данных:
CREATE DATABASE Laba5DB;
GO
USE Laba5DB;
GO
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Age INT NOT NULL CHECK (Age BETWEEN 10 AND 150),
    Weight DECIMAL(5,2) NOT NULL CHECK (Weight BETWEEN 30 AND 300),
    Height DECIMAL(5,2) NOT NULL CHECK (Height BETWEEN 120 AND 250),
    BMI DECIMAL(5,2)
);

3. Откройте проект
-Перейдите в папку Laba5_3sem
-Откройте файл Laba5_3sem.sln в Visual Studio

4. Настройте подключение
В файле DatabaseHelper.cs измените строку подключения:
private string connectionString = @"Server=ВАШ_СЕРВЕР\SQLEXPRESS;Database=Laba5DB;Trusted_Connection=True;";

5. Запустите проект
-Нажмите F5 для запуска
-Проверьте подключение через кнопку "Тест подключения"
