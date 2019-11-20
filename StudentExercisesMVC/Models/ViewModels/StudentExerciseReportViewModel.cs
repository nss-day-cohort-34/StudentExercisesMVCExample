using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models.ViewModels
{
    public class StudentExerciseReportViewModel
    {
        public List<Cohort> Cohorts { get; set; }
        public List<SelectListItem> CohortOptions
        {
            get
            {
                List<SelectListItem> options = new List<SelectListItem>()
                {
                    new SelectListItem("Select a cohort...", "0")
                };

                if (Cohorts != null)
                {
                    options.AddRange(
                        Cohorts.Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                    );
                }

                return options;
            }
        }
    }
}
