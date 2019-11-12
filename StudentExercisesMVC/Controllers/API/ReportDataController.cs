using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentExercisesMVC.Models;
using StudentExercisesMVC.Models.ViewModels;

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
                    cmd.CommandText = @"SELECT s.id, s.firstname, s.lastname, s.slackhandle,
                                               COUNT(se.id) AS ExerciseCount
                                          fROM Student s
                                               left join StudentExercise se on s.id = se.StudentId
                                         WHERE s.cohortId = @id
                                      GROUP BY s.id, s.FirstName, s.LastName, s.SlackHandle";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    var reader = cmd.ExecuteReader();

                    var students = new List<StudentExerciseCount>();
                    while (reader.Read())
                    {
                        students.Add(
                            new StudentExerciseCount
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                                LastName = reader.GetString(reader.GetOrdinal("lastname")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("slackhandle")),
                                ExerciseCount = reader.GetInt32(reader.GetOrdinal("ExerciseCount")),
                            });
                    }

                    reader.Close();
                    return Ok(students);
                }
            }
        }
    }
}