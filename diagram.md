# Лабораторная работа №5: Windows Forms с БД

## Диаграмма классов проекта

```mermaid
classDiagram
    class Form2 {
        +Form2()
        +Form2(string mode)
        -LoadDataFromDatabase()
        -AddNewPerson()
        -ApplyMode(string mode)
    }

    class Person {
        +string FullName
        +int Age
        +double Weight
        +double Height
        +double BMI
        +Person(string, int, double, double)
    }

    class DatabaseHelper {
        +GetAllPersons() List~Person~
        +AddPerson(Person)
        +DeletePerson(int)
        +SaveToFile(string)
        +LoadFromFile(string)
    }

    Form2 --> DatabaseHelper
    Form2 --> Person
    DatabaseHelper --> Person
