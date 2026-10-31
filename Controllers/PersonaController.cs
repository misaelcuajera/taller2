using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using taller.Models;

namespace taller.Controllers;

public class PersonaController : Controller
{

    private readonly string _connectionString;

    public PersonaController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MariaDBConnection") ?? string.Empty;
    }
    public IActionResult Crear()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarPersona(Persona persona)
    {
        if (!ModelState.IsValid)
        {
            return View("Crear", persona);
        }

        string query = "INSERT INTO persona (nombre, apellido, edad) VALUES (@nombre, @apellido, @edad)";

        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.Add("@nombre", MySqlDbType.VarChar, 20).Value = persona.nombre;
                command.Parameters.Add("@apellido", MySqlDbType.VarChar, 20).Value = persona.apellido;
                command.Parameters.Add("@edad", MySqlDbType.Decimal).Value = persona.edad;

                await command.ExecuteNonQueryAsync();
            }
        }

        return RedirectToAction("listar", "Persona");
    }

    public async Task<IActionResult> Listar()
{
    var ListaPersonas = new List<Persona>();

    string query = "SELECT id_persona, nombre, apellido, edad FROM persona ORDER BY edad DESC;";

    using (var connection = new MySqlConnection(_connectionString))
    {
        await connection.OpenAsync();

        using (var command = new MySqlCommand(query, connection))
        {
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var person = new Persona
                    {
                        id_persona = reader.GetInt32("id_persona"),
                        nombre = reader.GetString("nombre"),
                        apellido = reader.GetString("apellido"),
                        edad = reader.GetInt32("edad")
                    };
                    ListaPersonas.Add(person);
                }
            }
        }
    }

    return View(ListaPersonas);
}
}
