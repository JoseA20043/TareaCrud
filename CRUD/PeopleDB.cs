using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CRUD
{
    // Esta clase se encarga SOLO de hablar con SQL Server (ADO.NET).
    // No tiene nada de formularios ni controles: así separamos la
    // presentación (pantallas) del acceso a datos.
    public class PeopleDB
    {
        // CADENA DE CONEXIÓN: le dice a C# cómo llegar a la base de datos.
        //   Data Source            -> nombre del servidor. IMPORTANTE: cámbielo por
        //                             el MISMO nombre que usa en SSMS
        //                             (ej.: localhost, .\SQLEXPRESS, (localdb)\MSSQLLocalDB)
        //   Initial Catalog        -> nombre de la base de datos (CRUD).
        //   Integrated Security    -> usa su usuario de Windows (sin contraseña).
        //   TrustServerCertificate -> evita errores de certificado en servidores locales.
        private readonly string connectionString =
            @"Data Source=localhost;Initial Catalog=CRUD;Integrated Security=True;TrustServerCertificate=True;";

        // Convierte la fila actual del SqlDataReader en un objeto People.
        // Se usa en las consultas SELECT para no repetir el mismo código.
        // Los números son la posición de la columna en el SELECT:
        // 0 = Id, 1 = Name, 2 = Age (los índices empiezan en cero).
        private People LeerPersona(SqlDataReader reader)
        {
            return new People
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Age = reader.GetInt32(2)
            };
        }

        // Prueba que la conexión funcione. Si falla, lanza una excepción.
        public bool TestConnection()
        {
            // "using" cierra y libera la conexión automáticamente al terminar.
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                return true;
            }
        }

        // ============================================================
        // READ (LEER): devuelve TODAS las personas
        // ============================================================
        public List<People> Get()
        {
            // Lista donde guardaremos los registros leídos.
            List<People> people = new List<People>();

            // Consulta SQL: se escriben las columnas (no SELECT *).
            const string query = "SELECT Id, Name, Age FROM dbo.People ORDER BY Id;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();

                // ExecuteReader se usa cuando la consulta DEVUELVE filas.
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    // Read() avanza a la siguiente fila; devuelve false cuando ya no hay más.
                    while (reader.Read())
                    {
                        // Un objeto NUEVO por cada fila (si no, todas las filas
                        // de la lista serían el mismo objeto repetido).
                        people.Add(LeerPersona(reader));
                    }
                }
            }
            return people;
        }

        // ============================================================
        // READ (LEER): devuelve UNA persona según su Id (o null si no existe)
        // ============================================================
        public People GetById(int id)
        {
            // @Id es un PARÁMETRO: el valor se envía separado del texto SQL.
            const string query = "SELECT Id, Name, Age FROM dbo.People WHERE Id = @Id;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    // Si no hay ninguna fila, la persona no existe.
                    if (!reader.Read())
                        return null;

                    return LeerPersona(reader);
                }
            }
        }

        // ============================================================
        // READ (BUSCAR): personas cuyo nombre CONTIENE el texto indicado
        // ============================================================
        public List<People> Search(string text)
        {
            List<People> people = new List<People>();

            // LIKE busca coincidencias parciales; el texto va en un parámetro.
            const string query =
                "SELECT Id, Name, Age FROM dbo.People WHERE Name LIKE @Text ORDER BY Id;";

            // Si el usuario escribe los símbolos especiales de LIKE (% _ [ ])
            // los "escapamos" para que se busquen como texto normal.
            string safe = (text ?? "")
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("_", "[_]");

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // Los % de los extremos significan "cualquier cosa antes/después".
                command.Parameters.Add("@Text", SqlDbType.VarChar, -1).Value = "%" + safe + "%";
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        people.Add(LeerPersona(reader));
                }
            }
            return people;
        }

        // ============================================================
        // CREATE (CREAR): inserta una persona nueva
        // Devuelve cuántas filas se insertaron (debería ser 1).
        // ============================================================
        public int Add(string name, int age)
        {
            // No se inserta el Id: SQL Server lo genera automáticamente.
            const string query =
                "INSERT INTO dbo.People (Name, Age) VALUES (@Name, @Age);";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // Parámetros con su tipo explícito (igual que en la tabla).
                // Nunca se concatenan los datos dentro del SQL (evita inyección SQL
                // y permite nombres con apóstrofo como O'Connor).
                command.Parameters.Add("@Name", SqlDbType.VarChar, 100).Value = name;
                command.Parameters.Add("@Age", SqlDbType.Int).Value = age;
                connection.Open();

                // ExecuteNonQuery se usa para INSERT/UPDATE/DELETE:
                // devuelve el número de filas afectadas.
                return command.ExecuteNonQuery();
            }
        }

        // ============================================================
        // UPDATE (ACTUALIZAR): modifica una persona existente
        // Devuelve cuántas filas se modificaron (debería ser 1).
        // ============================================================
        public int Update(int id, string name, int age)
        {
            // OJO: el WHERE Id = @Id es obligatorio; sin él se
            // modificarían TODOS los registros de la tabla.
            const string query =
                "UPDATE dbo.People SET Name = @Name, Age = @Age WHERE Id = @Id;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                command.Parameters.Add("@Name", SqlDbType.VarChar, 100).Value = name;
                command.Parameters.Add("@Age", SqlDbType.Int).Value = age;
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        // ============================================================
        // DELETE (ELIMINAR): borra una persona por su Id
        // Devuelve cuántas filas se eliminaron (debería ser 1).
        // ============================================================
        public int Delete(int id)
        {
            // Igual que en UPDATE: el WHERE evita borrar toda la tabla.
            const string query = "DELETE FROM dbo.People WHERE Id = @Id;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
    }
}
