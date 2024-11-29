// Written by Kiara Vaz for CS4385.0W1, , Senior Design Project, Started October 3, 2024
// Net ID: KMV200000

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
