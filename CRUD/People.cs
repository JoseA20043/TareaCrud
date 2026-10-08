namespace CRUD
{
    // Representa UNA fila de la tabla dbo.People de la base de datos CRUD.
    // Cada propiedad corresponde a una columna de la tabla.
    // Al ser públicas, el DataGridView crea sus columnas automáticamente.
    public class People
    {
        // Identificador único. SQL Server lo genera solo (IDENTITY).
        public int Id { get; set; }

        // Nombre de la persona (VARCHAR(100) en SQL Server).
        public string Name { get; set; }

        // Edad de la persona (INT en SQL Server).
        public int Age { get; set; }
    }
}
