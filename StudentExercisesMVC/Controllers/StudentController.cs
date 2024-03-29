﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentExercisesMVC.Models;
using StudentExercisesMVC.Models.ViewModels;

namespace StudentExercisesMVC.Controllers
{
    public class StudentController : Controller
    {
        private string _connectionString;
        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_connectionString);
            }
        }

        public StudentController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // GET: Student
        public ActionResult Index()
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
                                     ORDER BY c.name, s.lastName, s.firstName";
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
                    return View(students);
                }
            }
        }

        // GET: Student/Details/5
        public ActionResult Details(int id)
        {
            Student student = GetStudentById(id);
            return View(student);
        }

        // GET: Student/Create
        public ActionResult Create()
        {
            var viewModel = new StudentCreateViewModel()
            {
                Cohorts = GetAllCohorts()
            };
            return View(viewModel);
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentCreateViewModel viewModel)
        {
            try
            {
                var newStudent = viewModel.Student;

                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @" INSERT INTO Student (FirstName, LastName, SlackHandle, CohortId)
                                                VALUES (@firstName, @lastName, @slackHandle, @cohortId);";
                        cmd.Parameters.Add(new SqlParameter("@firstName", newStudent.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", newStudent.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", newStudent.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", newStudent.CohortId));

                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Student/Edit/5
        public ActionResult Edit(int id)
        {
            var student = GetStudentById(id);
            var viewModel = new StudentEditViewModel()
            {
                Student = student,
                AllCohorts = GetAllCohorts(),
                AllExercises = GetAllExercises(),
                SelectedExerciseIds = student.Exercises.Select(e => e.Id).ToList()
            };

            return View(viewModel);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, StudentEditViewModel viewModel)
        {
            var updatedStudent = viewModel.Student;
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            DELETE FROM StudentExercise WHERE StudentId = @id;

                            UPDATE Student 
                               SET FirstName = @firstName, 
                                   LastName = @lastName, 
                                   SlackHandle = @slackHandle, 
                                   CohortId = @cohortId
                            WHERE id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@firstName", updatedStudent.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", updatedStudent.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", updatedStudent.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", updatedStudent.CohortId));

                        cmd.ExecuteNonQuery();

                        cmd.CommandText = @"
                            INSERT INTO StudentExercise (StudentId, ExerciseId)
                                VALUES (@studentId, @exerciseId)";
                        foreach (var exerciseId in viewModel.SelectedExerciseIds)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add(new SqlParameter("@studentId", id));
                            cmd.Parameters.Add(new SqlParameter("@exerciseId", exerciseId));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                viewModel = new StudentEditViewModel()
                {
                    Student = updatedStudent,
                    AllCohorts = GetAllCohorts()
                };

                return View(viewModel);
            }
        }

        // GET: Student/Delete/5
        public ActionResult Delete(int id)
        {
            var student = GetStudentById(id);
            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            DELETE FROM StudentExercise WHERE StudentId = @id;
                            DELETE FROM Student WHERE id = @id;";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private List<Cohort> GetAllCohorts()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name FROM Cohort";
                    var reader = cmd.ExecuteReader();

                    var cohorts = new List<Cohort>();
                    while (reader.Read())
                    {
                        cohorts.Add(
                                new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                }
                            );
                    }

                    reader.Close();

                    return cohorts;
                }
            }
        }
        private List<Exercise> GetAllExercises()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, Language FROM Exercise";
                    var reader = cmd.ExecuteReader();

                    var exercises = new List<Exercise>();
                    while (reader.Read())
                    {
                        exercises.Add(
                                new Exercise()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Language = reader.GetString(reader.GetOrdinal("Language")),
                                }
                            );
                    }

                    reader.Close();

                    return exercises;
                }
            }
        }


        private Student GetStudentById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT s.id, s.firstname, s.lastname, 
                               s.slackhandle, s.cohortId,
                               c.name as cohortname,
                               se.ExerciseId, e.Name AS ExerciseName, e.Language
                          FROM Student s INNER JOIN Cohort c on c.id = s.cohortid
                               LEFT JOIN StudentExercise se on s.id = se.StudentId
                               LEFT JOIN Exercise e on e.id = se.ExerciseId
                         WHERE s.id = @id;";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    var reader = cmd.ExecuteReader();

                    Student student = null;
                    while (reader.Read())
                    {
                        if (student == null)
                        {
                            student = new Student
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
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                        {
                            student.Exercises.Add(
                                new Exercise()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    Language = reader.GetString(reader.GetOrdinal("Language")),
                                });
                        }
                    }
                    reader.Close();
                    return student;
                }
            }
        }
    }
}