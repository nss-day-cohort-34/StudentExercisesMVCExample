using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models.ViewModels
{
    public class StudentEditViewModel
    {
        public Student Student { get; set; }
        public List<Cohort> AllCohorts { get; set; } = new List<Cohort>();

        public List<Exercise> AllExercises { get; set; } = new List<Exercise>();
        public List<int> SelectedExerciseIds { get; set; } = new List<int>();

        public List<SelectListItem> CohortOptions
        {
            get
            {
                if (AllCohorts == null) return null;

                return AllCohorts
                    .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                    .ToList();
            }
        }

        public List<SelectListItem> ExerciseOptions
        {
            get
            {
                if (AllExercises == null) return null;

                return AllExercises
                    .Select(e => new SelectListItem($"{e.Name} ({e.Language})", e.Id.ToString()))
                    .ToList();
            }
        }
     }
}
