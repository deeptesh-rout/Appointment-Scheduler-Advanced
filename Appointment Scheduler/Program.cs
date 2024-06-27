using System;
using System.Collections.Generic;
using System.Linq;

public class Appointment
{
    public string Title { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    public Appointment(string title, DateTime startTime, DateTime endTime)
    {
        Title = title;
        StartTime = startTime;
        EndTime = endTime;
    }

    public override string ToString()
    {
        return $"Title: {Title}, Start Time: {StartTime:yyyy-MM-dd HH:mm}, End Time: {EndTime:yyyy-MM-dd HH:mm}";
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        Appointment other = (Appointment)obj;
        return Title == other.Title && StartTime == other.StartTime && EndTime == other.EndTime;
    }

    public override int GetHashCode()
    {
        return Title.GetHashCode() + StartTime.GetHashCode() + EndTime.GetHashCode();
    }
}

public class Scheduler
{
    private List<Appointment> appointments = new List<Appointment>();

    public bool AddAppointment(Appointment appointment)
    {
        foreach (Appointment a in appointments)
        {
            if (appointment.StartTime < a.EndTime && appointment.EndTime > a.StartTime)
            {
                return false; // Conflict detected
            }
        }
        appointments.Add(appointment);
        return true;
    }

    public bool RemoveAppointment(Appointment appointment)
    {
        return appointments.Remove(appointment);
    }

    public List<Appointment> GetAppointments(DateTime startDate, DateTime endDate)
    {
        List<Appointment> result = new List<Appointment>();
        foreach (Appointment appointment in appointments)
        {
            if (appointment.StartTime <= endDate && appointment.EndTime >= startDate)
            {
                result.Add(appointment);
            }
        }
        return result;
    }

    public bool EditAppointment(Appointment oldAppointment, Appointment newAppointment)
    {
        if (RemoveAppointment(oldAppointment))
        {
            if (AddAppointment(newAppointment))
            {
                return true;
            }
            else
            {
                // If adding the new appointment fails, re-add the old appointment
                AddAppointment(oldAppointment);
            }
        }
        return false;
    }

    public bool IsAvailable(DateTime startTime, DateTime endTime)
    {
        foreach (Appointment appointment in appointments)
        {
            if (startTime < appointment.EndTime && endTime > appointment.StartTime)
            {
                return false;
            }
        }
        return true;
    }

    public DateTime FindNextAvailableSlot(DateTime startTime, int durationMinutes)
    {
        DateTime potentialStart = startTime;
        DateTime potentialEnd = startTime.AddMinutes(durationMinutes);

        while (true)
        {
            if (IsAvailable(potentialStart, potentialEnd))
            {
                return potentialStart;
            }
            potentialStart = potentialStart.AddMinutes(1);
        }
    }

    public int CountAppointments(DateTime startDate, DateTime endDate)
    {
        return GetAppointments(startDate, endDate).Count;
    }

    public void CancelAllAppointmentsForDay(DateTime date)
    {
        DateTime startOfDay = date.Date;
        DateTime endOfDay = date.Date.AddHours(23).AddMinutes(59);
        appointments.RemoveAll(appointment => appointment.StartTime > startOfDay && appointment.EndTime < endOfDay);
    }

    public Dictionary<string, int> GetAppointmentSummaryByDay(DateTime startDate, DateTime endDate)
    {
        Dictionary<string, int> summary = new Dictionary<string, int>();
        DateTime currentDate = startDate.Date;
        while (currentDate <= endDate.Date)
        {
            DateTime startOfDay = currentDate.Date;
            DateTime endOfDay = currentDate.Date.AddHours(23).AddMinutes(59);
            int count = GetAppointments(startOfDay, endOfDay).Count;
            summary.Add(currentDate.ToString("yyyy-MM-dd"), count);
            currentDate = currentDate.AddDays(1);
        }
        return summary;
    }

    public KeyValuePair<DateTime, DateTime>? FindLongestFreeSlot(DateTime startDate, DateTime endDate)
    {
        List<Appointment> sortedAppointments = GetAppointments(startDate, endDate)
            .OrderBy(a => a.StartTime)
            .ToList();

        KeyValuePair<DateTime, DateTime>? longestFreeSlot = null;
        TimeSpan maxDuration = TimeSpan.Zero;
        DateTime previousEnd = startDate;

        foreach (Appointment appointment in sortedAppointments)
        {
            DateTime currentStart = appointment.StartTime;
            if (previousEnd < currentStart)
            {
                TimeSpan freeSlotDuration = currentStart - previousEnd;
                if (freeSlotDuration > maxDuration)
                {
                    maxDuration = freeSlotDuration;
                    longestFreeSlot = new KeyValuePair<DateTime, DateTime>(previousEnd, currentStart);
                }
            }
            previousEnd = appointment.EndTime;
        }

        // Check the period between the last appointment and the end date
        if (previousEnd < endDate)
        {
            TimeSpan freeSlotDuration = endDate - previousEnd;
            if (freeSlotDuration > maxDuration)
            {
                longestFreeSlot = new KeyValuePair<DateTime, DateTime>(previousEnd, endDate);
            }
        }

        return longestFreeSlot;
    }

    public List<Appointment> GetAllAppointments()
    {
        return new List<Appointment>(appointments);
    }
}

public class AppointmentScheduler
{
    public static void Main(string[] args)
    {
        Scheduler scheduler = new Scheduler();

        // Add some sample appointments
        scheduler.AddAppointment(new Appointment("Meeting", new DateTime(2024, 3, 1, 10, 0, 0), new DateTime(2024, 3, 1, 11, 0, 0)));
        scheduler.AddAppointment(new Appointment("Lunch", new DateTime(2024, 3, 1, 12, 0, 0), new DateTime(2024, 3, 1, 13, 0, 0)));
        scheduler.AddAppointment(new Appointment("Presentation", new DateTime(2024, 3, 1, 14, 0, 0), new DateTime(2024, 3, 1, 15, 0, 0)));
        scheduler.AddAppointment(new Appointment("Review", new DateTime(2024, 3, 1, 16, 0, 0), new DateTime(2024, 3, 1, 17, 0, 0)));
        scheduler.AddAppointment(new Appointment("Workshop", new DateTime(2024, 3, 1, 17, 30, 0), new DateTime(2024, 3, 1, 18, 30, 0)));

        // Get appointments for a specific date range (user input)
        DateTime startDate = ReadDateTime("Enter start date and time (yyyy-MM-dd HH:mm): ");
        DateTime endDate = ReadDateTime("Enter end date and time (yyyy-MM-dd HH:mm): ");
        List<Appointment> appointments = scheduler.GetAppointments(startDate, endDate);

        // Print the appointments
        Console.WriteLine($"Appointments for {startDate} to {endDate}:");
        foreach (Appointment appointment in appointments)
        {
            Console.WriteLine(appointment);
        }

        // Edit an appointment (user input)
        Appointment oldAppointment = new Appointment("Lunch", new DateTime(2024, 3, 1, 12, 0, 0), new DateTime(2024, 3, 1, 13, 0, 0));
        Console.WriteLine("Enter details for the new appointment:");
        string title = Console.ReadLine();
        DateTime newStartTime = ReadDateTime("Enter new start time (yyyy-MM-dd HH:mm): ");
        DateTime newEndTime = ReadDateTime("Enter new end time (yyyy-MM-dd HH:mm): ");
        Appointment newAppointment = new Appointment(title, newStartTime, newEndTime);

        if (scheduler.EditAppointment(oldAppointment, newAppointment))
        {
            Console.WriteLine("Appointment edited successfully.");
        }
        else
        {
            Console.WriteLine("Failed to edit appointment due to conflict.");
        }

        // Print the updated appointments
        appointments = scheduler.GetAppointments(startDate, endDate);
        Console.WriteLine($"Updated appointments for {startDate} to {endDate}:");
        foreach (Appointment appointment in appointments)
        {
            Console.WriteLine(appointment);
        }

        // Check availability for a specific time slot (user input)
        DateTime checkStartTime = ReadDateTime("Enter start time to check availability (yyyy-MM-dd HH:mm): ");
        DateTime checkEndTime = ReadDateTime("Enter end time to check availability (yyyy-MM-dd HH:mm): ");
        if (scheduler.IsAvailable(checkStartTime, checkEndTime))
        {
            Console.WriteLine($"The time slot from {checkStartTime} to {checkEndTime} is available.");
        }
        else
        {
            Console.WriteLine($"The time slot from {checkStartTime} to {checkEndTime} is not available.");
        }

        // Find the next available time slot of 60 minutes (user input)
        DateTime nextAvailableSlotStart = ReadDateTime("Enter start time to find next available slot (yyyy-MM-dd HH:mm): ");
        int durationMinutes = ReadInteger("Enter duration of the slot in minutes: ");
        DateTime nextAvailableSlot = scheduler.FindNextAvailableSlot(nextAvailableSlotStart, durationMinutes);
        if (nextAvailableSlot != DateTime.MinValue)
        {
            Console.WriteLine($"The next available time slot is at {nextAvailableSlot}.");
        }
        else
        {
            Console.WriteLine("No available time slot found.");
        }

        // Count the number of appointments on a specific date (user input)
        DateTime countAppointmentsDate = ReadDateTime("Enter date to count appointments (yyyy-MM-dd): ");
        int appointmentCount = scheduler.CountAppointments(countAppointmentsDate.Date, countAppointmentsDate.Date.AddDays(1).AddSeconds(-1));
        Console.WriteLine($"Number of appointments on {countAppointmentsDate.Date}: {appointmentCount}");

        // Cancel all appointments for a specific day (user input)
        DateTime cancelAppointmentsDate = ReadDateTime("Enter date to cancel all appointments (yyyy-MM-dd): ");
        scheduler.CancelAllAppointmentsForDay(cancelAppointmentsDate.Date);
        Console.WriteLine($"All appointments for {cancelAppointmentsDate.Date} have been canceled.");

        // Get a summary of appointments by day (user input)
        DateTime summaryStartDate = ReadDateTime("Enter start date to get appointment summary (yyyy-MM-dd): ");
        DateTime summaryEndDate = ReadDateTime("Enter end date to get appointment summary (yyyy-MM-dd): ");
        Dictionary<string, int> summary = scheduler.GetAppointmentSummaryByDay(summaryStartDate.Date, summaryEndDate.Date.AddDays(1).AddSeconds(-1));
        Console.WriteLine("Appointment summary by day:");
        foreach (KeyValuePair<string, int> entry in summary)
        {
            Console.WriteLine($"{entry.Key}: {entry.Value} appointments");
        }

        // Find the longest free slot available (user input)
        DateTime longestFreeSlotStartDate = ReadDateTime("Enter start date to find longest free slot (yyyy-MM-dd HH:mm): ");
        DateTime longestFreeSlotEndDate = ReadDateTime("Enter end date to find longest free slot (yyyy-MM-dd HH:mm): ");
        KeyValuePair<DateTime, DateTime>? longestFreeSlot = scheduler.FindLongestFreeSlot(longestFreeSlotStartDate, longestFreeSlotEndDate);
        if (longestFreeSlot != null)
        {
            Console.WriteLine($"The longest free slot is from {longestFreeSlot.Value.Key} to {longestFreeSlot.Value.Value}.");
        }
        else
        {
            Console.WriteLine("No free slot found.");
        }

        // Get all appointments
        List<Appointment> allAppointments = scheduler.GetAllAppointments();
        Console.WriteLine("All appointments:");
        foreach (Appointment appointment in allAppointments)
        {
            Console.WriteLine(appointment);
        }

        // Keep the console window open in debug mode.
        Console.WriteLine("Press any key to exit...");
        Console.ReadLine();
    }

    // Helper method to read DateTime from user input
    private static DateTime ReadDateTime(string message)
    {
        DateTime result;
        while (true)
        {
            Console.Write(message);
            if (DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd HH:mm", null, System.Globalization.DateTimeStyles.None, out result))
            {
                break;
            }
            else
            {
                Console.WriteLine("Invalid date/time format. Please enter again.");
            }
        }
        return result;
    }

    // Helper method to read integer from user input
    private static int ReadInteger(string message)
    {
        int result;
        while (true)
        {
            Console.Write(message);
            if (int.TryParse(Console.ReadLine(), out result))
            {
                break;
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid integer.");
            }
        }
        return result;
    }
}
