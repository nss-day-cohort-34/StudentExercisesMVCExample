using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StudentExercisesMVC.Models;
using StudentExercisesMVC.Models.ViewModels;

namespace StudentExercisesMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private string _connectionString;
        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_connectionString);
            }
        }

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IActionResult StudentExerciseReport()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM Cohort";
                    var reader = cmd.ExecuteReader();

                    var allCohorts = new List<Cohort>();
                    while (reader.Read())
                    {
                        allCohorts.Add(new Cohort()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                        });
                    }

                    reader.Close();
                    var viewModel = new StudentExerciseReportViewModel()
                    {
                        Cohorts = allCohorts
                    };
                    return View(viewModel);
                }
            }
        }
 
        public IActionResult Index()
        {
            var students = new List<Student>()
            {
                new Student()
                {
                    FirstName = "Roy",
                    LastName = "Batty"
                },
                new Student()
                {
                    FirstName = "Lisa",
                    LastName = "Bactericidal"
                },
                new Student()
                {
                    FirstName = "Tony",
                    LastName = "Dances"
                }
            };
            return View(students);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
