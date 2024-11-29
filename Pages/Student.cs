// Written by Kiara Vaz for CS4385.0W1, Senior Design Project, Started October 3, 2024
// Net ID: KMV200000
// This file defines the `Student` class, which models the essential information for a student in the system.
// The class includes properties for the student's identification, personal information,
// login credentials, course section, and team assignment.

using System.ComponentModel.DataAnnotations;
    public class Student
    {
        public string NetId { get; set; } = string.Empty;
        public string UtdId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string TeamNum { get; set; } = string.Empty;
    }
