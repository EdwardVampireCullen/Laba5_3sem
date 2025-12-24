using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Laba5_3sem
{
    public partial class Form2 : Form
    {
        private DatabaseHelper dbHelper;
        private List<Person> originalPersons;
        private List<Person> displayedPersons;
        private bool databaseMode = true;

        // Переменные для сортировки
        private string currentSortColumn = "";
        private bool sortAscending = true;

        // Дополнительные элементы для переключения режимов
        private Button btnViewMode;
        private Button btnFullAccess;
        private Label lblModeStatus;

        // ========== КОНСТРУКТОРЫ (ПЕРЕГРУЗКА) ==========

        // Конструктор 1: Базовый (без параметров)
        public Form2()
        {
            InitializeComponent();
            CreateModeButtons(); // Создаем кнопки переключения режимов
            SetupForm();
        }

        // Конструктор 2: С параметром режима работы (ОСНОВНАЯ ПЕРЕГРУЗКА)
        public Form2(string mode) : this()
        {
            ApplyMode(mode);
        }

        // Конструктор 3: С параметром режима и цветом фона (ДОПОЛНИТЕЛЬНАЯ ПЕРЕГРУЗКА)
        public Form2(string mode, Color backgroundColor) : this(mode)
        {
            this.BackColor = backgroundColor;
        }

        // Конструктор 4: С параметром режима и пользователем (ДОПОЛНИТЕЛЬНАЯ ПЕРЕГРУЗКА)
        public Form2(string mode, Person selectedPerson) : this(mode)
        {
            if (selectedPerson != null)
            {
                // Выделяем пользователя в таблице
                SelectPersonInGrid(selectedPerson);
            }
        }

        // ========== СОЗДАНИЕ КНОПОК РЕЖИМОВ ==========

        private void CreateModeButtons()
        {
            // Кнопка "Только просмотр"
            btnViewMode = new Button
            {
                Text = "Только просмотр",
                Location = new Point(450, 320),
                Size = new Size(120, 30),
                BackColor = Color.LightBlue,
                ForeColor = Color.Black
            };
            btnViewMode.Click += BtnViewMode_Click;
            this.Controls.Add(btnViewMode);

            // Кнопка "Полный доступ"
            btnFullAccess = new Button
            {
                Text = "Полный доступ",
                Location = new Point(580, 320),
                Size = new Size(120, 30),
                BackColor = Color.LightGreen,
                ForeColor = Color.Black
            };
            btnFullAccess.Click += BtnFullAccess_Click;
            this.Controls.Add(btnFullAccess);

            // Label для отображения статуса режима
            lblModeStatus = new Label
            {
                Text = "Режим: Полный доступ",
                Location = new Point(450, 360),
                Size = new Size(250, 20),
                ForeColor = Color.Green,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            this.Controls.Add(lblModeStatus);
        }

        // ========== НАСТРОЙКА ФОРМЫ ==========

        private void SetupForm()
        {
            // Подключаем обработчики событий для существующих кнопок
            btnAdd.Click += BtnAdd_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSaveToFile.Click += BtnSaveToFile_Click;
            btnLoadFromFile.Click += BtnLoadFromFile_Click;
            btnMaxHeight.Click += BtnMaxHeight_Click;
            btnSort.Click += BtnSort_Click;
            btnSearch.Click += BtnSearch_Click;
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            dataGridView1.ColumnHeaderMouseClick += DataGridView1_ColumnHeaderMouseClick;

            // Инициализация БД
            dbHelper = new DatabaseHelper();
            databaseMode = dbHelper.TestConnection();

            if (databaseMode)
            {
                LoadDataFromDatabase();
            }
            else
            {
                originalPersons = new List<Person>();
                displayedPersons = new List<Person>();
                dataGridView1.DataSource = displayedPersons;
                MessageBox.Show("БД недоступна. Локальный режим.");
            }

            // По умолчанию - полный доступ
            ApplyMode("Полный доступ");
        }

        // ========== РЕЖИМЫ ДОСТУПА ==========

        // Метод применения режима
        private void ApplyMode(string mode)
        {
            this.Text = "Управление пользователями - " + mode;

            if (mode == "Только просмотр" || mode == "Только чтение")
            {
                SetReadOnlyMode(true);
                lblModeStatus.Text = "Режим: Только просмотр";
                lblModeStatus.ForeColor = Color.Blue;
            }
            else if (mode == "Полный доступ")
            {
                SetReadOnlyMode(false);
                lblModeStatus.Text = "Режим: Полный доступ";
                lblModeStatus.ForeColor = Color.Green;
            }
        }

        // Режим "Только просмотр"
        private void SetReadOnlyMode(bool readOnly)
        {
            // Блокируем основные функции
            btnAdd.Enabled = !readOnly;
            btnDelete.Enabled = !readOnly;
            btnSaveToFile.Enabled = !readOnly;
            btnLoadFromFile.Enabled = !readOnly;

            // Эти функции всегда доступны в режиме просмотра
            btnSort.Enabled = true;
            btnMaxHeight.Enabled = true;
            btnSearch.Enabled = true;
            txtSearch.Enabled = true;
            dataGridView1.Enabled = true;

            // Кнопки переключения режимов
            btnViewMode.Enabled = !readOnly; // В режиме просмотра нельзя переключить на просмотр
            btnFullAccess.Enabled = readOnly; // В режиме просмотра можно переключить на полный доступ
        }

        // ========== ОБРАБОТЧИКИ КНОПОК РЕЖИМОВ ==========

        private void BtnViewMode_Click(object sender, EventArgs e)
        {
            ApplyMode("Только просмотр");
            MessageBox.Show("Включен режим 'Только просмотр'.\nДобавление, удаление, сохранение и загрузка отключены.", "Режим изменен",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnFullAccess_Click(object sender, EventArgs e)
        {
            ApplyMode("Полный доступ");
            MessageBox.Show("Включен режим 'Полный доступ'.\nВсе функции доступны.", "Режим изменен",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========

        private void SelectPersonInGrid(Person person)
        {
            // Найти и выделить пользователя в DataGridView
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                Person p = (Person)row.DataBoundItem;
                if (p.Id == person.Id)
                {
                    row.Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;

                    // Подсветка выбранного пользователя
                    row.DefaultCellStyle.BackColor = Color.LightYellow;
                    break;
                }
            }
        }

        // ========== НОВЫЙ МЕТОД: ПРОВЕРКА ФИО НА ЦИФРЫ ==========

        private bool ContainsDigits(string text)
        {
            foreach (char c in text)
            {
                if (char.IsDigit(c))
                {
                    return true;
                }
            }
            return false;
        }

        // ========== СУЩЕСТВУЮЩИЕ МЕТОДЫ (БЕЗ ИЗМЕНЕНИЙ) ==========

        private void LoadDataFromDatabase()
        {
            originalPersons = dbHelper.GetAllPersons();
            displayedPersons = new List<Person>(originalPersons);
            dataGridView1.DataSource = displayedPersons;
        }

        private void DataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (originalPersons == null || originalPersons.Count == 0) return;

            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;

            if (currentSortColumn == columnName)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSortColumn = columnName;
                sortAscending = true;
            }

            displayedPersons = SortPersons(new List<Person>(originalPersons), columnName, sortAscending);
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = displayedPersons;
            UpdateSortIndicator(e.ColumnIndex);
        }

        private List<Person> SortPersons(List<Person> data, string columnName, bool ascending)
        {
            switch (columnName)
            {
                case "FullName":
                    return ascending
                        ? data.OrderBy(p => p.FullName).ToList()
                        : data.OrderByDescending(p => p.FullName).ToList();
                case "Age":
                    return ascending
                        ? data.OrderBy(p => p.Age).ToList()
                        : data.OrderByDescending(p => p.Age).ToList();
                case "Weight":
                    return ascending
                        ? data.OrderBy(p => p.Weight).ToList()
                        : data.OrderByDescending(p => p.Weight).ToList();
                case "Height":
                    return ascending
                        ? data.OrderBy(p => p.Height).ToList()
                        : data.OrderByDescending(p => p.Height).ToList();
                case "BMI":
                    return ascending
                        ? data.OrderBy(p => p.BMI).ToList()
                        : data.OrderByDescending(p => p.BMI).ToList();
                default:
                    return data;
            }
        }

        private void UpdateSortIndicator(int columnIndex)
        {
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.None;
            }
            dataGridView1.Columns[columnIndex].HeaderCell.SortGlyphDirection =
                sortAscending ? System.Windows.Forms.SortOrder.Ascending : System.Windows.Forms.SortOrder.Descending;
        }

        // ========== ОСТАЛЬНЫЕ МЕТОДЫ (С ДОБАВЛЕННОЙ ПРОВЕРКОЙ ФИО) ==========

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            AddNewPerson();
        }

        private void AddNewPerson()
        {
            using (var inputForm = new Form())
            {
                inputForm.Text = "Добавить пользователя";
                inputForm.Size = new Size(400, 300);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;

                // Создаем элементы
                Label lblName = new Label { Text = "ФИО:", Location = new Point(20, 20), Width = 100 };
                TextBox txtName = new TextBox { Location = new Point(130, 20), Width = 200 };

                Label lblAge = new Label { Text = "Возраст:", Location = new Point(20, 60), Width = 100 };
                TextBox txtAge = new TextBox { Location = new Point(130, 60), Width = 50 };

                Label lblWeight = new Label { Text = "Вес:", Location = new Point(20, 100), Width = 100 };
                TextBox txtWeight = new TextBox { Location = new Point(130, 100), Width = 50 };

                Label lblHeight = new Label { Text = "Рост:", Location = new Point(20, 140), Width = 100 };
                TextBox txtHeight = new TextBox { Location = new Point(130, 140), Width = 50 };

                Label lblError = new Label { Text = "", Location = new Point(20, 180), Width = 300, ForeColor = Color.Red };

                Button btnOk = new Button { Text = "Добавить", Location = new Point(80, 210), Width = 80, DialogResult = DialogResult.OK };
                Button btnCancel = new Button { Text = "Отмена", Location = new Point(180, 210), Width = 80, DialogResult = DialogResult.Cancel };

                // Валидация ввода в реальном времени
                txtAge.KeyPress += (s, ev) => {
                    if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar))
                        ev.Handled = true;
                };

                txtWeight.KeyPress += (s, ev) => {
                    if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar) && ev.KeyChar != ',')
                        ev.Handled = true;
                };

                txtHeight.KeyPress += (s, ev) => {
                    if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar) && ev.KeyChar != ',')
                        ev.Handled = true;
                };

                // Добавляем элементы
                inputForm.Controls.AddRange(new Control[] {
                    lblName, txtName,
                    lblAge, txtAge,
                    lblWeight, txtWeight,
                    lblHeight, txtHeight,
                    lblError,
                    btnOk, btnCancel
                });

                inputForm.AcceptButton = btnOk;
                inputForm.CancelButton = btnCancel;

                // Обработка нажатия OK с валидацией
                btnOk.Click += (s, ev) => {
                    lblError.Text = "";

                    // Проверка ФИО
                    string fullName = txtName.Text.Trim();

                    if (string.IsNullOrWhiteSpace(fullName))
                    {
                        lblError.Text = "Ошибка: Введите ФИО";
                        txtName.Focus();
                        inputForm.DialogResult = DialogResult.None;
                        return;
                    }

                    // НОВАЯ ПРОВЕРКА: ФИО не должно содержать цифры
                    if (ContainsDigits(fullName))
                    {
                        lblError.Text = "Ошибка: ФИО не должно содержать цифры";
                        txtName.Focus();
                        inputForm.DialogResult = DialogResult.None;
                        return;
                    }

                    // Дополнительная проверка: ФИО должно содержать минимум 2 слова
                    string[] nameParts = fullName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (nameParts.Length < 2)
                    {
                        lblError.Text = "Ошибка: Введите имя и фамилию (минимум 2 слова)";
                        txtName.Focus();
                        inputForm.DialogResult = DialogResult.None;
                        return;
                    }

                    // Проверка возраста
                    if (!int.TryParse(txtAge.Text, out int age))
                    {
                        lblError.Text = "Ошибка: Возраст должен быть числом";
                        txtAge.Focus();
                        inputForm.DialogResult = DialogResult.None;
                        return;
                    }

                    if (age < 10 || age > 150)
                    {
                        lblError.Text = $"Ошибка: Возраст должен быть от 10 до 150 лет";
                        txtAge.Focus();
                        inputForm.DialogResult = DialogResult.None;
                        return;
                    }

                    // Проверка веса
                    if (!double.TryParse(txtWeight.Text.Replace(',', '.'), out double weight))
                    {
                        lblError.Text = "Ошибка: Вес должен быть числом (используйте точку или запятую)";
                        txtWeight.Focus();
                        inputForm.DialogResult = DialogResult.None;
                        return;
                    }

                    if (weight <= 29 || weight > 300)
                    {
                        lblError.Text = $"Ошибка: Вес должен быть от 30 до 300 кг";
                        txtWeight.Focus();
                        inputForm.DialogResult = DialogResult.None;
                        return;
                    }

                    // Проверка роста
                    if (!double.TryParse(txtHeight.Text.Replace(',', '.'), out double height))
                    {
                        lblError.Text = "Ошибка: Рост должен быть числом (используйте точку или запятую)";
                        txtHeight.Focus();
                        inputForm.DialogResult = DialogResult.None;
                        return;
                    }

                    if (height <= 119 || height > 250)
                    {
                        lblError.Text = $"Ошибка: Рост должен быть от 120 до 250 см";
                        txtHeight.Focus();
                        inputForm.DialogResult = DialogResult.None;
                        return;
                    }

                    inputForm.DialogResult = DialogResult.OK;
                };

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Person person = new Person(
                            txtName.Text.Trim(),
                            int.Parse(txtAge.Text),
                            double.Parse(txtWeight.Text.Replace(',', '.')),
                            double.Parse(txtHeight.Text.Replace(',', '.'))
                        );

                        if (databaseMode)
                        {
                            dbHelper.AddPerson(person);
                            LoadDataFromDatabase();
                        }
                        else
                        {
                            originalPersons.Add(person);
                            displayedPersons.Add(person);
                            dataGridView1.DataSource = null;
                            dataGridView1.DataSource = displayedPersons;
                        }

                        MessageBox.Show($"Пользователь '{person.FullName}' успешно добавлен!\nИМТ: {person.BMI}", "Успех");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите строку для удаления");
                return;
            }

            Person selected = (Person)dataGridView1.SelectedRows[0].DataBoundItem;

            if (MessageBox.Show($"Удалить {selected.FullName}?", "Подтверждение",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (databaseMode)
                {
                    dbHelper.DeletePerson(selected.Id);
                    LoadDataFromDatabase();
                }
                else
                {
                    originalPersons.Remove(selected);
                    displayedPersons.Remove(selected);
                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = displayedPersons;
                }
            }
        }

        private void BtnSaveToFile_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Текстовые файлы (*.txt)|*.txt";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    dbHelper.SaveToFile(sfd.FileName);
                }
            }
        }

        private void BtnLoadFromFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Текстовые файлы (*.txt)|*.txt";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    dbHelper.LoadFromFile(ofd.FileName);
                    LoadDataFromDatabase();
                }
            }
        }

        private void BtnMaxHeight_Click(object sender, EventArgs e)
        {
            if (displayedPersons.Count == 0) return;

            var tallest = displayedPersons.OrderByDescending(p => p.Height).First();

            // Сброс цвета
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.DefaultCellStyle.BackColor = Color.White;
            }

            // Подсветка
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                Person p = (Person)row.DataBoundItem;
                if (p.Id == tallest.Id)
                {
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                    MessageBox.Show($"Самый высокий: {tallest.FullName}\nРост: {tallest.Height} см");
                    break;
                }
            }
        }

        private void BtnSort_Click(object sender, EventArgs e)
        {
            if (originalPersons == null || originalPersons.Count == 0) return;

            displayedPersons = SortPersons(new List<Person>(originalPersons), "FullName", true);
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = displayedPersons;

            // Убираем значки сортировки
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.None;
            }

            // Устанавливаем значок для колонки ФИО
            if (dataGridView1.Columns.Contains("FullName"))
            {
                dataGridView1.Columns["FullName"].HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.Ascending;
            }

            currentSortColumn = "FullName";
            sortAscending = true;

            MessageBox.Show("Отсортировано по ФИО (А-Я)", "Сортировка");
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(search))
            {
                displayedPersons = new List<Person>(originalPersons);
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = displayedPersons;
                return;
            }

            var filtered = originalPersons.Where(p => p.FullName.ToLower().Contains(search.ToLower())).ToList();
            displayedPersons = filtered;
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = displayedPersons;

            if (filtered.Count == 0)
            {
                MessageBox.Show("Не найдено");
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                displayedPersons = new List<Person>(originalPersons);
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = displayedPersons;
            }
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Person person = (Person)dataGridView1.Rows[e.RowIndex].DataBoundItem;
                MessageBox.Show($"ФИО: {person.FullName}\nВозраст: {person.Age}\nВес: {person.Weight}\nРост: {person.Height}\nИМТ: {person.BMI}");
            }
        }
    }
}