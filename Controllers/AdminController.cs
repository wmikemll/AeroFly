using AeroFly.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Numerics;
using System.Reflection;

namespace AeroFly.Controllers
{
    public class AdminController : Controller
    {
        private readonly string _connectionString = "Server=sql5113.site4now.net;Database=db_aa81fd_wmikemll;User Id=db_aa81fd_wmikemll_admin;Password=Granta797;";
        private List<Races> races;
        private List<Ticket> tickets;

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult AddRace()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddRace(string from, string where)
        {
            string query = $"INSERT INTO Races (RaceId, [Where], [From]) VALUES ('{Guid.NewGuid()}', '{where}', '{from}')";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                        reader.Close();
                }
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult AddTicket()
        {
            FillLists();
            return View(races);
        }
        [HttpPost]
        public ActionResult AddTicket(string date, string time, string type, string raceId, string price, string plane, string count)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                for (int i = 0; i < int.Parse(count); i++) 
                {
                    string query = $"INSERT INTO Tickets (RaceId, [Date], [Time], Type, Price, Plane, TicketId) VALUES ('{raceId}', '{date}', '{time}', '{type}', '{price}', '{plane}', '{Guid.NewGuid()}')";
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader()) 
                            reader.Close();
                    }
                }
            }
            FillLists();
            return View(races);
        }
        public void FillLists()
        {
            races = new List<Races>();
            tickets = new List<Ticket>();
            string first_query = $"SELECT * FROM Races";
            string second_query = $"SELECT * FROM Tickets";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(first_query, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Races race = new Races();
                        race.Id = reader["RaceId"].ToString();
                        race.From = reader["From"].ToString();
                        race.Where = reader["Where"].ToString();
                        races.Add(race);
                    }
                    reader.Close ();
                }
                using (var command = new SqlCommand(second_query, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Ticket ticket = new Ticket();
                        ticket.Id = reader["TicketId"].ToString();
                        ticket.Time = Convert.ToDateTime(reader["Time"]);
                        ticket.Date = Convert.ToDateTime(reader["Date"]);
                        ticket.Type = reader["Type"].ToString();
                        ticket.Plane = reader["Plane"].ToString();
                        ticket.ClientId = reader["ClientId"].ToString();
                        ticket.Races = races.Find(race => race.Id == reader["RaceId"].ToString());
                        tickets.Add(ticket);
                    }
                    reader.Close();
                }
            }

        }
    }
}
