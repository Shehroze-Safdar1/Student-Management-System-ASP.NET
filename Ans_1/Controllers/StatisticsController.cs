using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Ans_1.Models;
using Ans_1.Models.ViewModels;

namespace Ans_1.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly EnrollDbContext db = new EnrollDbContext();

        // Main Dashboard
        public ActionResult Index()
        {
            var stats = new StatisticsVM
            {
                TotalStudents = db.Students.Count(),
                ActiveStudents = db.Students.Count(s => !s.StudentStatus),
                WithdrawnStudents = db.Students.Count(s => s.StudentStatus),
                TotalCourses = db.Courses.Count(),
                TotalEnrollments = db.Enrolls.Count(),

                // Age distribution
                AgeGroups = GetAgeDistribution(),

                // Course enrollment data
                CourseEnrollments = GetCourseEnrollmentData(),

                // Monthly registration data
                MonthlyRegistrations = GetMonthlyRegistrationData(),

                // Student status distribution
                StudentStatusData = GetStudentStatusData(),

                // Recent activities
                RecentStudents = db.Students.OrderByDescending(s => s.StudentId)
                                          .Take(5)
                                          .Select(s => new RecentActivityVM
                                          {
                                              StudentName = s.StudentName,
                                              Action = "Registered",
                                              Date = DateTime.Now, // Replace with actual date if available
                                              Status = s.StudentStatus ? "Withdrawn" : "Active"
                                          }).ToList()
            };

            return View(stats);
        }

        // Detailed Reports
        public ActionResult StudentReport()
        {
            var students = db.Students.Include(s => s.Enrolls.Select(e => e.Course))
                                    .OrderBy(s => s.StudentName)
                                    .ToList();
            return View(students);
        }

        public ActionResult CourseReport()
        {
            var courses = db.Courses.Include(c => c.Enrolls.Select(e => e.Student))
                                   .OrderBy(c => c.CourseName)
                                   .ToList();
            return View(courses);
        }

        public ActionResult EnrollmentReport()
        {
            var enrollments = db.Enrolls.Include(e => e.Student)
                                       .Include(e => e.Course)
                                       .OrderByDescending(e => e.EnrollId)
                                       .ToList();
            return View(enrollments);
        }

        // AJAX Methods for Charts
        public JsonResult GetChartData(string chartType)
        {
            switch (chartType.ToLower())
            {
                case "agedistribution":
                    return Json(GetAgeDistribution(), JsonRequestBehavior.AllowGet);

                case "courseenrollments":
                    return Json(GetCourseEnrollmentData(), JsonRequestBehavior.AllowGet);

                case "monthlyregistrations":
                    return Json(GetMonthlyRegistrationData(), JsonRequestBehavior.AllowGet);

                case "studentstatus":
                    return Json(GetStudentStatusData(), JsonRequestBehavior.AllowGet);

                default:
                    return Json(new { error = "Invalid chart type" }, JsonRequestBehavior.AllowGet);
            }
        }

        // Export functionality
        public ActionResult ExportStudentData()
        {
            var students = db.Students.Include(s => s.Enrolls.Select(e => e.Course)).ToList();

            var csv = "Student Name,Age,Birth Date,Status,Enrolled Courses\n";
            foreach (var student in students)
            {
                var courses = string.Join("; ", student.Enrolls.Select(e => e.Course.CourseName));
                csv += $"{student.StudentName},{student.Age},{student.BirthDate:yyyy-MM-dd},{(student.StudentStatus ? "Withdrawn" : "Active")},\"{courses}\"\n";
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "StudentData.csv");
        }

        // Private helper methods
        private List<ChartDataVM> GetAgeDistribution()
        {
            var rawGroups = db.Students.GroupBy(s =>
                s.Age < 20 ? "Under 20" :
                s.Age < 25 ? "20-24" :
                s.Age < 30 ? "25-29" :
                s.Age < 35 ? "30-34" : "35+"
            ).Select(g => new
            {
                Label = g.Key,
                Value = g.Count()
            }).ToList();

            var ageGroups = rawGroups.Select(g => new ChartDataVM
            {
                Label = g.Label,
                Value = g.Value,
                Color = GetRandomColor()
            }).ToList();

            return ageGroups;
        }

        private List<ChartDataVM> GetCourseEnrollmentData()
        {
            var courseData = db.Courses.ToList().Select(c => new ChartDataVM
            {
                Label = c.CourseName,
                Value = c.Enrolls.Count,
                Color = GetRandomColor()
            }).OrderByDescending(c => c.Value).ToList();

            return courseData;
        }

        private List<ChartDataVM> GetMonthlyRegistrationData()
        {
            var monthlyData = new List<ChartDataVM>();
            var currentDate = DateTime.Now;

            for (int i = 5; i >= 0; i--)
            {
                var month = currentDate.AddMonths(-i);
                var count = db.Students.Count(); // You can filter by DateCreated in real scenarios

                monthlyData.Add(new ChartDataVM
                {
                    Label = month.ToString("MMM yyyy"),
                    Value = count / 6, // Placeholder
                    Color = GetRandomColor()
                });
            }

            return monthlyData;
        }

        private List<ChartDataVM> GetStudentStatusData()
        {
            return new List<ChartDataVM>
            {
                new ChartDataVM
                {
                    Label = "Active Students",
                    Value = db.Students.Count(s => !s.StudentStatus),
                    Color = "#28a745"
                },
                new ChartDataVM
                {
                    Label = "Withdrawn Students",
                    Value = db.Students.Count(s => s.StudentStatus),
                    Color = "#dc3545"
                }
            };
        }

        private string GetRandomColor()
        {
            var colors = new[] { "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF", "#FF9F40", "#C9CBCF" };
            var random = new Random();
            return colors[random.Next(colors.Length)];
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
