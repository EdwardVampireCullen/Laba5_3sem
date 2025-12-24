using System;
using System.Windows.Forms;

namespace Laba5_3sem
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();

            // Передаем режим работы (можно сделать выбор через RadioButtons)
            Form2 f2 = new Form2("Полный доступ"); // или "Только чтение"
            f2.ShowDialog();

            this.Show(); // Возвращаем первую форму после закрытия второй
        }
    }
}