# Лабораторная работа №5: Windows Forms с БД

## Диаграмма классов проекта

```mermaid
classDiagram
    class Form1 {
        -Form2 form2
        +Form1()
        +Button1_Click(sender, e)
    }

    class Form2 {
        -DatabaseHelper dbHelper
        -List~Person~ originalPersons
        -List~Person~ displayedPersons
        -bool databaseMode
        -string currentSortColumn
        -bool sortAscending
        -Button btnViewMode
        -Button btnFullAccess
        -Label lblModeStatus
        
        +Form2()
        +Form2(string mode)
        +Form2(string mode, Color backgroundColor)
        +Form2(string mode, Person selectedPerson)
        
        -SetupForm()
        -CreateModeButtons()
        -ApplyMode(string mode)
        -SetReadOnlyMode(bool readOnly)
        -LoadDataFromDatabase()
        -DataGridView1_ColumnHeaderMouseClick(sender, e)
        -SortPersons(List~Person~ data, string columnName, bool ascending) List~Person~
        -UpdateSortIndicator(int columnIndex)
        -AddNewPerson()
        -IsValidFullName(string fullName) bool
        -SelectPersonInGrid(Person person)
        -BtnViewMode_Click(sender, e)
        -BtnFullAccess_Click(sender, e)
        -BtnAdd_Click(sender, e)
        -BtnDelete_Click(sender, e)
        -BtnSaveToFile_Click(sender, e)
        -BtnLoadFromFile_Click(sender, e)
        -BtnMaxHeight_Click(sender, e)
        -BtnSort_Click(sender, e)
        -BtnSearch_Click(sender, e)
        -TxtSearch_TextChanged(sender, e)
        -DataGridView1_CellDoubleClick(sender, e)
    }

    class Person {
        +int Id
        +string FullName
        +int Age
        +double Weight
        +double Height
        +double BMI
        
        +Person()
        +Person(string fullName, int age, double weight, double height)
        -CalculateBMI()
        +ToString() string
    }

    class DatabaseHelper {
        -string connectionString
        
        +DatabaseHelper()
        +TestConnection() bool
        +CreateTableIfNotExists()
        +GetAllPersons() List~Person~
        +AddPerson(Person person) bool
        +AddPerson(Person person, out string errorMessage) bool
        +DeletePerson(int id)
        +SaveToFile(string filePath)
        +LoadFromFile(string filePath)
        +SearchByName(string searchText) List~Person~
        
        -ExecuteNonQuery(string query)
        -ProcessLine(string line, ref int count)
        -ProcessLineForLoad(string line, ref int count)
        -PersonExists(string fullName) bool
    }

    class SqlConnection {
        +SqlConnection(string connectionString)
        +Open()
        +Close()
        +CreateCommand() SqlCommand
    }

    class SqlCommand {
        +SqlCommand(string query, SqlConnection connection)
        +Parameters SqlParameterCollection
        +ExecuteNonQuery() int
        +ExecuteReader() SqlDataReader
        +ExecuteScalar() object
    }

    class SqlDataReader {
        +Read() bool
        +GetInt32(int index) int
        +GetString(int index) string
        +GetDouble(int index) double
        +Close()
    }

    class OpenFileDialog {
        +Filter string
        +FileName string
        +ShowDialog() DialogResult
    }

    class SaveFileDialog {
        +Filter string
        +FileName string
        +ShowDialog() DialogResult
    }

    class DataGridView {
        +Columns DataGridViewColumnCollection
        +DataSource object
        +SelectedRows DataGridViewSelectedRowCollection
        +CellDoubleClick event
        +ColumnHeaderMouseClick event
    }

    class MessageBox {
        +Show(string text) DialogResult
        +Show(string text, string caption) DialogResult
        +Show(string text, string caption, MessageBoxButtons buttons) DialogResult
    }

    %% Связи между классами
    Form1 --> Form2 : создает и показывает
    Form2 --> DatabaseHelper : использует для работы с БД
    Form2 --> Person : управляет коллекцией
    Form2 --> OpenFileDialog : использует для загрузки
    Form2 --> SaveFileDialog : использует для сохранения
    Form2 --> DataGridView : отображает данные
    Form2 --> MessageBox : показывает сообщения
    
    DatabaseHelper --> SqlConnection : создает и использует
    DatabaseHelper --> SqlCommand : создает и выполняет
    DatabaseHelper --> SqlDataReader : читает данные
    DatabaseHelper --> Person : создает объекты
    
    SqlCommand --> SqlConnection : привязывается к соединению
    SqlDataReader --> SqlCommand : получает от команды
