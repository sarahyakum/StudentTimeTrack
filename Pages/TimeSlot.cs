// Written by Kiara Vaz for CS4385.0W1, Senior Design Project, Started October 3, 2024
// Net ID: KMV200000
// This file defines the `Timeslot` class, which models the essential information for a timeslot in the system.
// The class includes properties for the timeslot such as Date, description, duration and so forth. Mimics the attributes in the DB
public class TimeSlot
{
    public string StuNetID { get; set; }    // Student's NetID
    public string StuName { get; set; }    // Student's name
    public DateTime TSDate { get; set; }   // Date of the time slot
    public string TSDescription { get; set; } // Description of the work done
    public double TSDuration { get; set; } // Duration of the time slot in minutes
}
