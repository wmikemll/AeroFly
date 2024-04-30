using AeroFly.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Xml.Linq;
using System.Data.OleDb;
using System.Numerics;
using System.Reflection;
using Microsoft.Extensions.Primitives;
using System.Data;
using System.Data.SqlClient;

namespace AeroFly.Controllers
{
    public class HomeController : Controller
    {
        private static List<Races> races;
        private static List<Ticket> tickets;
        private readonly string _connectionString = "Server=sql5113.site4now.net;Database=db_aa81fd_wmikemll;User Id=db_aa81fd_wmikemll_admin;Password=Granta797;";
        private ILogger<HomeController> _logger;
        

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(); 
        }
        [HttpGet]
        public IActionResult TicketsBuy()
        {
            FillLists();
            ViewBag.Tickets = tickets;
            ViewBag.Races = races;
            return View();
        }
        [HttpPost]
        public IActionResult TicketsBuy(string mail, string phone, string passport, string last_name, string first_name, DateTime dob, string gender, string raceId)
        {
            string personId = Guid.NewGuid().ToString();
            string query = $"INSERT INTO ClientInfo (Id, Name, Surname, Email, Phone, Passport, Gender, Birthday) " +
                   $"VALUES ('{personId}', '{first_name}', '{last_name}', '{mail}', '{phone}', '{passport}', '{gender}', '{dob.ToShortDateString()}' )";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Close();
                }
            }
            return RedirectToAction("TicketChoice", new {personId = personId, raceId = raceId });
        }
        [HttpGet]
        public IActionResult TicketChoice(string personId,string raceId) 
        {
            var uniqueTickets = new Dictionary<(DateTime, DateTime, string), bool>();
            var filteredTickets = new List<Ticket>();
            foreach (var ticket in tickets.Where(ticket => ticket.Races.Id == raceId && ticket.ClientId == "").ToList())
            {
                var ticketKey = (ticket.Date, ticket.Time, ticket.Races.Id);
                if (!uniqueTickets.ContainsKey(ticketKey))
                {
                    uniqueTickets[ticketKey] = true;
                    filteredTickets.Add(ticket);
                }
            }
            ViewBag.PersonId = personId;
            return View(filteredTickets);
        }
        [HttpPost]
        public IActionResult TicketChoicePost(string personId, string ticketId) 
        {
            string query2 = $"UPDATE Tickets SET ClientId = '{personId}' Where TicketId = '{ticketId}'";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query2, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Close();
                }
            }
            UpdateTickets(ticketId, personId);
            return RedirectToAction("Ticket", new {id = personId });
        }
        [HttpGet]
        public IActionResult TicketSearch() 
        {
            return View();
        }
        [HttpPost]
        public IActionResult TicketSearch(string passport, string first_name, string last_name)
        {
            var ticket = new Ticket();
            string clientId = null;
            string first_query = $"SELECT Id FROM ClientInfo WHERE Passport = '{passport}' AND Name = '{first_name}' AND Surname = '{last_name}'";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(first_query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read()) // Проверяем, есть ли данные
                        {
                            clientId = reader["Id"].ToString();
                        }
                        else
                            return RedirectToAction("Index");
                    }
                }
            }
            return RedirectToAction("Ticket", new { id = clientId});
        }
        public IActionResult Ticket(string id)
        {
            FillLists();
            var ticket = tickets.Find(ticket => ticket.ClientId == id);
            if (ticket != null)
            {
                return View(ticket);
            }
            else
                return RedirectToAction("Index");
        }

        public void FillLists() 
        {
            if (races != null && tickets != null)
                return;
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
                    reader.Close();
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

        public void UpdateTickets(string ticketId, string clientId) 
        {
            var ticket = tickets.Find(ticket => ticket.Id == ticketId);
            tickets.Remove(ticket);
            ticket.ClientId = clientId;
            tickets.Add(ticket);
        }








    }
}
