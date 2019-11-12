using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Surname")]
        public string LastName { get; set; }

        [Display(Name = "Slack")]
        public string SlackHandle { get; set; }

        [Display(Name = "Cohort")]
        public int CohortId { get; set; }

        public Cohort Cohort { get; set; }
    }
}
