using System;
using System.Collections.Generic;

namespace Ans_1.Models.ViewModels
{
    public class StatisticsVM
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int WithdrawnStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }

        public List<ChartDataVM> AgeGroups { get; set; }
        public List<ChartDataVM> CourseEnrollments { get; set; }
        public List<ChartDataVM> MonthlyRegistrations { get; set; }
        public List<ChartDataVM> StudentStatusData { get; set; }
        public List<RecentActivityVM> RecentStudents { get; set; }

        public StatisticsVM()
        {
            AgeGroups = new List<ChartDataVM>();
            CourseEnrollments = new List<ChartDataVM>();
            MonthlyRegistrations = new List<ChartDataVM>();
            StudentStatusData = new List<ChartDataVM>();
            RecentStudents = new List<RecentActivityVM>();
        }
    }

    public class ChartDataVM
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public string Color { get; set; }
        public double Percentage { get; set; }
    }

    public class RecentActivityVM
    {
        public string StudentName { get; set; }
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}