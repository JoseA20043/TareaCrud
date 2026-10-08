using System;
using System.Windows.Forms;

namespace CRUD
{
    // Formulario de captura. Se reutiliza para dos cosas:
    //   new FrmNuevo()    -> INSERTAR una persona nueva
    //   new FrmNuevo(id)  -> EDITAR la persona con ese Id
    public partial class FrmNuevo : Form
    {
        // Objeto que hace las consultas a SQL Server.
        private readonly PeopleDB db = new PeopleDB();

        // Id de la persona a editar. Si es null, estamos insertando.
        private readonly int? id;

        // Constructor. "int? id = null" hace que el parámetro sea opcional:
        // si no se envía nada, el formulario trabaja en modo "Nueva persona".
        public FrmNuevo(int? id = null)
        {
            InitializeComponent();
            this.id = id;

            // Título de la ventana y del encabezado según el modo.
            string titulo = id.HasValue ? "Editar persona" : "Nueva persona";
            Text = titulo;
            lblTitulo.Text = titulo;
        }

        // ------------------------------------------------------------
        // Valida lo escrito. Devuelve true si todo es correcto y entrega
        // el nombre y la edad ya limpios mediante los parámetros "out".
        // ------------------------------------------------------------
        private bool ValidateInput(out string name, out int age)
        {
            // Trim() quita los espacios al inicio y al final.
            name = txtName.Text.Trim();
            age = 0;

            // El nombre no puede estar vacío ni pasar de 100 caracteres.
            if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            {
                MessageBox.Show("Escriba un nombre de 1 a 100 caracteres.");
                txtName.Focus();
                return false;
            }

            // TryParse intenta convertir el texto a número SIN lanzar error
            // si el usuario escribe letras (por ejemplo "abc").
            // La edad debe estar entre 0 y 120 (regla del ejercicio).
            if (!int.TryParse(txtEdad.Text.Trim(), out age) || age < 0 || age > 120)
            {
                MessageBox.Show("Escriba una edad entera entre 0 y 120.");
                txtEdad.Focus();
                return false;
            }
            return true;
        }

        // ------------------------------------------------------------
        // Al abrirse: si estamos EDITANDO, carga los datos actuales
        // de la persona en los cuadros de texto.
        // ------------------------------------------------------------
        private void FrmNuevo_Load(object sender, EventArgs e)
        {
            // Modo "nuevo": no hay nada que cargar.
            if (!id.HasValue)
                return;

            try
            {
                People person = db.GetById(id.Value);

                // Puede que otra persona/ventana ya la haya eliminado.
                if (person == null)
                {
                    MessageBox.Show("El registro ya no existe.");
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                // Muestra los datos actuales para poder modificarlos.
                txtName.Text = person.Name;
                txtEdad.Text = person.Age.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar: " + ex.Message);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        // ------------------------------------------------------------
        // BOTÓN GUARDAR: inserta o actualiza según el modo.
        // ------------------------------------------------------------
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string name;
            int age;

            // Si los datos no son válidos, se detiene aquí (no se toca la BD).
            if (!ValidateInput(out name, out age))
                return;

            try
            {
                // Si hay id -> UPDATE; si no hay id -> INSERT.
                // Ambos métodos devuelven las filas afectadas.
                int affected = id.HasValue
                    ? db.Update(id.Value, name, age)
                    : db.Add(name, age);

                // Debe afectarse exactamente 1 fila.
                if (affected != 1)
                {
                    MessageBox.Show("No se guardó el registro. Actualice la lista y revise si todavía existe.");
                    return;
                }

                // OK le avisa al formulario principal que debe refrescar la lista.
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }

        // ------------------------------------------------------------
        // BOTÓN CANCELAR: cierra sin guardar nada.
        // ------------------------------------------------------------
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
