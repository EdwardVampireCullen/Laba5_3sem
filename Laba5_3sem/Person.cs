using System;

namespace Laba5_3sem
{
    public class Person
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public double BMI { get; set; }

        public Person() { }

        public Person(string fullName, int age, double weight, double height)
        {
            // Проверки при создании объекта
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("ФИО не может быть пустым");

            if (age < 10 || age > 90)
                throw new ArgumentException($"Возраст должен быть от 10 до 90 лет. Указано: {age}");

            if (weight <= 29 || weight > 300)
                throw new ArgumentException($"Вес должен быть от 30 до 300 кг. Указано: {weight}");

            if (height <= 119 || height > 250)
                throw new ArgumentException($"Рост должен быть от 120 до 250 см. Указано: {height}");

            FullName = fullName;
            Age = age;
            Weight = weight;
            Height = height;
            CalculateBMI();
        }

        private void CalculateBMI()
        {
            if (Height > 0)
            {
                double heightMeters = Height / 100.0;
                BMI = Math.Round(Weight / (heightMeters * heightMeters), 2);
            }
        }
    }
}