using HR_system.Models;

namespace HR_system.Domain
{
    /// <summary>
    /// Domain service for attendance calculations
    /// </summary>
    public class AttendanceCalculationService
    {
        /// <summary>
        /// Calculate late minutes for check-in
        /// </summary>
        public static int CalculateLateMinutes(TimeSpan checkIn, Shift shift)
        {
            if (checkIn > shift.Start_time)
            {
                return (int)(checkIn - shift.Start_time).TotalMinutes;
            }
            return 0;
        }

        /// <summary>
        /// Calculate overtime minutes after shift end
        /// </summary>
        public static int CalculateOvertimeMinutes(TimeSpan checkOut, Shift shift)
        {
            if (checkOut > shift.End_time)
            {
                return (int)(checkOut - shift.End_time).TotalMinutes;
            }
            return 0;
        }

        /// <summary>
        /// Calculate early check-in minutes (before shift start)
        /// </summary>
        public static int CalculateEarlyCheckInMinutes(TimeSpan checkIn, Shift shift)
        {
            if (checkIn < shift.Start_time)
            {
                return (int)(shift.Start_time - checkIn).TotalMinutes;
            }
            return 0;
        }

        /// <summary>
        /// Calculate total overtime minutes (after shift + early check-in)
        /// </summary>
        public static int CalculateTotalOvertimeMinutes(TimeSpan checkIn, TimeSpan checkOut, Shift shift)
        {
            var overtimeAfterShift = CalculateOvertimeMinutes(checkOut, shift);
            var earlyCheckIn = CalculateEarlyCheckInMinutes(checkIn, shift);
            return overtimeAfterShift + earlyCheckIn;
        }

        /// <summary>
        /// Calculate actual worked minutes (excluding overtime, minus permission time)
        /// </summary>
        public static int CalculateWorkedMinutes(TimeSpan checkIn, TimeSpan checkOut, Shift shift, int permissionMinutes)
        {
            // Calculate total minutes from check-in to check-out
            var totalMinutes = (int)(checkOut - checkIn).TotalMinutes;

            // Handle overnight shifts
            if (totalMinutes < 0)
            {
                totalMinutes += 24 * 60;
            }

            // Standard shift duration in minutes
            var standardShiftMinutes = (int)(shift.StandardHours * 60);

            // Calculate overtime after shift (post-shift)
            int overtimeAfterShift = CalculateOvertimeMinutes(checkOut, shift);

            // Calculate early check-in minutes (pre-shift) - these should be counted as overtime, not worked hours
            int earlyCheckInMinutes = CalculateEarlyCheckInMinutes(checkIn, shift);

            // Worked minutes = total - (post-shift overtime) - (early check-in minutes), capped at standard shift
            int workedMinutes = Math.Min(totalMinutes - overtimeAfterShift - earlyCheckInMinutes, standardShiftMinutes);

            // Subtract permission time
            workedMinutes = Math.Max(0, workedMinutes - permissionMinutes);

            return workedMinutes;
        }

        /// <summary>
        /// Calculate attendance status based on check-in/out times
        /// </summary>
        public static string CalculateAttendanceStatus(bool isAbsent, TimeSpan? checkIn, TimeSpan? checkOut, Shift shift)
        {
            if (isAbsent)
                return "غائب";

            if (!checkIn.HasValue)
                return "لم يسجل دخول";

            if (!checkOut.HasValue)
                return "لم يسجل خروج";

            var lateMinutes = CalculateLateMinutes(checkIn.Value, shift);
            var overtimeMinutes = CalculateTotalOvertimeMinutes(checkIn.Value, checkOut.Value, shift);

            if (lateMinutes > 0 && overtimeMinutes > 0)
                return "متأخر + إضافي";
            else if (lateMinutes > 0)
                return "متأخر";
            else if (overtimeMinutes > 0)
                return "إضافي";
            else
                return "حاضر";
        }

        /// <summary>
        /// Calculate overtime and late time for flexible shifts based on StandardHours
        /// </summary>
        public static (int overtimeMinutes, int lateMinutes) CalculateFlexibleTimeDifferences(
            TimeSpan checkIn, TimeSpan checkOut, Shift shift, int permissionMinutes)
        {
            // Calculate actual worked hours (total time - permission)
            var totalMinutes = (int)(checkOut - checkIn).TotalMinutes;
            if (totalMinutes < 0)
            {
                totalMinutes += 24 * 60; // Handle overnight
            }

            var actualWorkedMinutes = totalMinutes - permissionMinutes;
            var actualWorkedHours = actualWorkedMinutes / 60.0m;

            // Compare with StandardHours
            var standardHours = shift.StandardHours;

            if (actualWorkedHours > standardHours)
            {
                // Overtime: worked more than standard hours
                var overtimeHours = actualWorkedHours - standardHours;
                var overtimeMinutes = (int)(overtimeHours * 60);
                return (overtimeMinutes, 0);
            }
            else if (actualWorkedHours < standardHours)
            {
                // Late: worked less than standard hours
                var lateHours = standardHours - actualWorkedHours;
                var lateMinutes = (int)(lateHours * 60);
                return (0, lateMinutes);
            }
            else
            {
                // Exactly on time
                return (0, 0);
            }
        }
    }
}