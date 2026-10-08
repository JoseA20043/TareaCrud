using System;
using System.Windows.Forms;

namespace CRUD
{
    internal static class Program
    {
        // Punto de entrada: aquí empieza a ejecutarse el programa.
        [STAThread]
        static void Main()
        {
            // Activa el aspecto visual moderno de los controles de Windows.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Abre el formulario principal (FrmPersonas) y mantiene la
            // aplicación en ejecución hasta que ese formulario se cierre.
            Application.Run(new FrmPersonas());
        }
    }
}
