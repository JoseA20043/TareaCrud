using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CRUD
{
    // Formulario principal: muestra la lista de personas y los botones
    // para Nuevo, Editar, Eliminar y Actualizar.
    public partial class FrmPersonas : Form
    {
        // Objeto que hace todas las consultas a SQL Server.
        private readonly PeopleDB db = new PeopleDB();

        // Texto gris que se ve en el cuadro de búsqueda cuando está vacío.
        private const string TextoBusqueda = "Buscar por nombre...";

        // Colores del cuadro de búsqueda: gris (sugerencia) y oscuro (texto real).
        private readonly Color colorSugerencia = Color.FromArgb(100, 116, 139);
        private readonly Color colorTexto = Color.FromArgb(30, 41, 59);

        public FrmPersonas()
        {
            // Crea y coloca todos los controles dibujados en el diseñador.
            InitializeComponent();
        }

        // ------------------------------------------------------------
        // Se ejecuta una vez cuando el formulario se abre.
        // ------------------------------------------------------------
        private void FrmPersonas_Load(object sender, EventArgs e)
        {
            // El cuadro de búsqueda empieza mostrando la sugerencia en gris.
            txtBuscar.ForeColor = colorSugerencia;

            // Carga los registros de la base de datos en la cuadrícula.
            RefreshPeople();
        }

        // ------------------------------------------------------------
        // Devuelve el texto que el usuario realmente escribió para buscar
        // (cadena vacía si solo está la sugerencia gris).
        // ------------------------------------------------------------
        private string ObtenerFiltro()
        {
            string texto = txtBuscar.Text.Trim();
            return texto == TextoBusqueda ? "" : texto;
        }

        // ------------------------------------------------------------
        // Lee la base de datos y muestra los datos en el DataGridView.
        // Se llama al abrir, al buscar y después de cada Nuevo/Editar/Eliminar.
        // ------------------------------------------------------------
        private void RefreshPeople()
        {
            try
            {
                // Todos los registros (se usan para el total).
                List<People> todos = db.Get();

                // Si hay texto de búsqueda se filtra por nombre; si no, se muestran todos.
                string filtro = ObtenerFiltro();
                List<People> mostrar = filtro == "" ? todos : db.Search(filtro);

                // Asigna la lista a la cuadrícula: crea las columnas Id, Name y Age.
                dgvPeople.DataSource = mostrar;

                // Títulos de columna en español (solo cambia lo que se ve).
                if (dgvPeople.Columns["Name"] != null)
                    dgvPeople.Columns["Name"].HeaderText = "Nombre";
                if (dgvPeople.Columns["Age"] != null)
                    dgvPeople.Columns["Age"].HeaderText = "Edad";

                // Mensaje de estado en la parte de abajo.
                lblStatus.Text = "Mostrando " + mostrar.Count + " de " + todos.Count
                                 + " registros • Total: " + todos.Count;
            }
            catch (Exception ex)
            {
                // Si algo falla (por ejemplo la conexión) se muestra el motivo.
                MessageBox.Show("Error al consultar: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------
        // Devuelve el Id de la persona seleccionada en la cuadrícula,
        // o null si no hay ninguna fila seleccionada.
        // (int? significa "entero que también puede ser null").
        // ------------------------------------------------------------
        private int? GetSelectedId()
        {
            if (dgvPeople.CurrentRow == null)
                return null;

            // DataBoundItem es el objeto People que está detrás de la fila.
            People person = dgvPeople.CurrentRow.DataBoundItem as People;
            if (person == null)
                return null;

            return person.Id;
        }

        // ------------------------------------------------------------
        // BÚSQUEDA: eventos del cuadro de texto txtBuscar
        // ------------------------------------------------------------

        // Al hacer clic dentro: quita la sugerencia gris para poder escribir.
        private void txtBuscar_Enter(object sender, EventArgs e)
        {
            if (txtBuscar.Text == TextoBusqueda)
            {
                txtBuscar.Text = "";
                txtBuscar.ForeColor = colorTexto;
            }
        }

        // Al salir del cuadro estando vacío: vuelve a poner la sugerencia.
        private void txtBuscar_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                txtBuscar.Text = TextoBusqueda;
                txtBuscar.ForeColor = colorSugerencia;
            }
        }

        // Cada vez que cambia el texto: se vuelve a consultar con el filtro.
        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            RefreshPeople();
        }

        // ------------------------------------------------------------
        // BOTÓN ACTUALIZAR: vuelve a leer la base de datos
        // ------------------------------------------------------------
        private void btnActualizar_Click(object sender, EventArgs e)
        {
            RefreshPeople();
        }

        // ------------------------------------------------------------
        // BOTÓN NUEVO: abre FrmNuevo vacío para INSERTAR una persona
        // ------------------------------------------------------------
        private void btnNuevo_Click(object sender, EventArgs e)
        {
            using (FrmNuevo form = new FrmNuevo())
            {
                // ShowDialog espera a que se cierre la ventana.
                // Si el usuario guardó (OK), se refresca la lista.
                if (form.ShowDialog(this) == DialogResult.OK)
                    RefreshPeople();
            }
        }

        // ------------------------------------------------------------
        // BOTÓN EDITAR: abre FrmNuevo con los datos de la persona
        // seleccionada para MODIFICARLA
        // ------------------------------------------------------------
        private void btnEditar_Click(object sender, EventArgs e)
        {
            int? id = GetSelectedId();

            // HasValue es false cuando no hay selección.
            if (!id.HasValue)
            {
                MessageBox.Show("Seleccione una persona.");
                return;
            }

            // Se pasa el Id al formulario: así sabe que debe EDITAR y no insertar.
            using (FrmNuevo form = new FrmNuevo(id.Value))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                    RefreshPeople();
            }
        }

        // ------------------------------------------------------------
        // BOTÓN ELIMINAR: borra la persona seleccionada (con confirmación)
        // ------------------------------------------------------------
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            int? id = GetSelectedId();
            if (!id.HasValue)
            {
                MessageBox.Show("Seleccione una persona.");
                return;
            }

            // Pregunta antes de borrar para evitar accidentes.
            DialogResult answer = MessageBox.Show(
                "¿Desea eliminar la persona con Id " + id.Value + "?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            // Si no responde "Sí", no se hace nada.
            if (answer != DialogResult.Yes)
                return;

            try
            {
                // Delete devuelve las filas eliminadas: debe ser exactamente 1.
                int affected = db.Delete(id.Value);
                if (affected != 1)
                    MessageBox.Show("El registro ya no existe.");

                RefreshPeople();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
