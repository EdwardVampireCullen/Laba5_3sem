using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace Laba5_3sem
{
    public class DatabaseHelper
    {
        private string connectionString = @"Server=DESKTOP-3VS575J\MSSQLSERVER1;Database=Laba5DB;Trusted_Connection=True;";

        public bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void CreateTableIfNotExists()
        {
            string query = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                CREATE TABLE Users (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    FullName NVARCHAR(100) NOT NULL,
                    Age INT NOT NULL CHECK (Age >= 0 AND Age <= 150),
                    Weight DECIMAL(5,2) NOT NULL CHECK (Weight > 0 AND Weight <= 300),
                    Height DECIMAL(5,2) NOT NULL CHECK (Height > 0 AND Height <= 250),
                    BMI DECIMAL(5,2)
                )";

            ExecuteNonQuery(query);
        }

        public List<Person> GetAllPersons()
        {
            List<Person> persons = new List<Person>();
            string query = "SELECT * FROM Users";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    persons.Add(new Person
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        FullName = reader["FullName"].ToString(),
                        Age = Convert.ToInt32(reader["Age"]),
                        Weight = Convert.ToDouble(reader["Weight"]),
                        Height = Convert.ToDouble(reader["Height"]),
                        BMI = Convert.ToDouble(reader["BMI"])
                    });
                }
            }

            return persons;
        }

        // НОВЫЙ МЕТОД: Добавление с проверкой (возвращает true/false и сообщение об ошибке)
        public bool AddPerson(Person person, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                // Проверка перед добавлением в БД
                if (person.Age < 10 || person.Age > 150)
                {
                    errorMessage = "Возраст должен быть от 10 до 90 лет";
                    return false;
                }

                if (person.Weight <= 29 || person.Weight > 300)
                {
                    errorMessage = "Вес должен быть от 30 до 300 кг";
                    return false;
                }

                if (person.Height <= 119 || person.Height > 250)
                {
                    errorMessage = "Рост должен быть от 120 до 250 см";
                    return false;
                }

                string query = "INSERT INTO Users (FullName, Age, Weight, Height, BMI) VALUES (@Name, @Age, @Weight, @Height, @BMI)";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", person.FullName);
                    cmd.Parameters.AddWithValue("@Age", person.Age);
                    cmd.Parameters.AddWithValue("@Weight", person.Weight);
                    cmd.Parameters.AddWithValue("@Height", person.Height);
                    cmd.Parameters.AddWithValue("@BMI", person.BMI);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (SqlException ex)
            {
                // Парсим SQL ошибку
                if (ex.Message.Contains("CHECK") && ex.Message.Contains("Age"))
                    errorMessage = "Ошибка: Возраст должен быть от 10 до 90 лет";
                else if (ex.Message.Contains("CHECK") && ex.Message.Contains("Weight"))
                    errorMessage = "Ошибка: Вес должен быть от 30 до 300 кг";
                else if (ex.Message.Contains("CHECK") && ex.Message.Contains("Height"))
                    errorMessage = "Ошибка: Рост должен быть от 120 до 250 см";
                else
                    errorMessage = $"Ошибка БД: {ex.Message}";

                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Ошибка: {ex.Message}";
                return false;
            }
        }

        // СТАРЫЙ МЕТОД для обратной совместимости
        public void AddPerson(Person person)
        {
            if (AddPerson(person, out string error))
            {
                MessageBox.Show($"Пользователь '{person.FullName}' добавлен в БД", "Успех");
            }
            else
            {
                MessageBox.Show($"{error}", "Ошибка добавления");
            }
        }

        public void DeletePerson(int id)
        {
            string query = "DELETE FROM Users WHERE Id = @Id";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void SaveToFile(string filePath)
        {
            var persons = GetAllPersons();
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("ФИО,Возраст,Вес,Рост,ИМТ");
                foreach (var person in persons)
                {
                    writer.WriteLine($"{person.FullName},{person.Age},{person.Weight},{person.Height},{person.BMI}");
                }
            }
            MessageBox.Show("Сохранено в файл: " + filePath);
        }

        public void LoadFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не найден", "Ошибка");
                    return;
                }

                // Спрашиваем пользователя что делать
                DialogResult result = MessageBox.Show(
                    "Как загрузить данные из файла?\n\n" +
                    "Да - добавить к существующим данным\n" +
                    "Нет - очистить таблицу и загрузить новые данные\n" +
                    "Отмена - не загружать",
                    "Выбор режима загрузки",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                    return;

                // Если выбрано "Нет" - очищаем таблицу
                if (result == DialogResult.No)
                {
                    ExecuteNonQuery("DELETE FROM Users");
                }
                // Если "Да" - не очищаем, просто добавляем

                int count = 0;
                using (StreamReader reader = new StreamReader(filePath, System.Text.Encoding.UTF8))
                {
                    // Пропускаем заголовок если есть
                    string firstLine = reader.ReadLine();

                    // Проверяем, есть ли заголовок
                    bool hasHeader = firstLine?.Contains("ФИО") == true ||
                                    firstLine?.Contains("FullName") == true ||
                                    firstLine?.Contains("Age") == true;

                    // Если первая строка - заголовок, начинаем со следующей строки
                    // Если нет заголовка, обрабатываем первую строку как данные
                    if (!hasHeader && !string.IsNullOrEmpty(firstLine))
                    {
                        ProcessLineForLoad(firstLine, ref count);
                    }

                    // Читаем остальные строки
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        ProcessLineForLoad(line, ref count);
                    }
                }

                MessageBox.Show($"Загружено {count} записей из файла", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки файла: {ex.Message}", "Ошибка");
            }
        }

        // Новый метод для обработки строк при загрузке (без проверки на дубликаты)
        private void ProcessLineForLoad(string line, ref int count)
        {
            if (string.IsNullOrWhiteSpace(line)) return;

            string[] parts = line.Split(',');
            if (parts.Length >= 4)
            {
                try
                {
                    Person person = new Person(
                        parts[0].Trim(),
                        int.Parse(parts[1].Trim()),
                        double.Parse(parts[2].Trim()),
                        double.Parse(parts[3].Trim())
                    );

                    // Просто добавляем без проверки существования
                    string query = "INSERT INTO Users (FullName, Age, Weight, Height, BMI) VALUES (@Name, @Age, @Weight, @Height, @BMI)";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Name", person.FullName);
                        cmd.Parameters.AddWithValue("@Age", person.Age);
                        cmd.Parameters.AddWithValue("@Weight", person.Weight);
                        cmd.Parameters.AddWithValue("@Height", person.Height);
                        cmd.Parameters.AddWithValue("@BMI", person.BMI);
                        cmd.ExecuteNonQuery();
                    }

                    count++;
                }
                catch (SqlException ex)
                {
                    // Если дубликат или другая ошибка SQL
                    if (ex.Message.Contains("PRIMARY KEY") || ex.Message.Contains("duplicate"))
                    {
                        // Просто пропускаем дубликат
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка при добавлении строки '{line}': {ex.Message}", "Ошибка");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обработки строки '{line}': {ex.Message}", "Ошибка");
                }
            }
        }

        private void ExecuteNonQuery(string query)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}