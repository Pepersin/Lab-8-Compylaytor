using System;
using System.Windows.Forms;

namespace Blocknot
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Настройка для работы с UI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Запуск формы
            Application.Run(new Form1());
        }
    }
}
