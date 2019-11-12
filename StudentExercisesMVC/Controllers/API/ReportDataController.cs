using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentExercisesMVC.Models;

namespace StudentExercisesMVC.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportDataController : ControllerBase
    {
        private string _connectionString;
        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_connectionString);
            }
        }

        public ReportDataController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        [HttpGet("GetStudentsByCohortId/{id:int}", Name = "GetStudentsByCohortId")]
        public async Task<IActionResult> GetStudentsByCohortId(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.id, s.firstname, s.lastname, 
                                               s.slackhandle, s.cohortId,
                                               c.name as cohortname
                                         FROM Student s INNER JOIN Cohort c on c.id = s.cohortid
                                        WHERE c.id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    var reader = cmd.ExecuteReader();

                    var students = new List<Student>();
                    while (reader.Read())
                    {
                        students.Add(
                            new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                                LastName = reader.GetString(reader.GetOrdinal("lastname")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("slackhandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("cohortId")),
                                Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("cohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("cohortname")),
                                }
                            });
                    }

                    reader.Close();
                    return Ok(students);
                }
            }
        }
    }
}