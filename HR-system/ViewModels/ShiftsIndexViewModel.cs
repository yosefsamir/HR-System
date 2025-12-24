using HR_system.DTOs.Shift;

namespace HR_system.ViewModels
{
    /// <summary>
    /// ViewModel for Shifts Index page - displays list and allows inline add/edit
    /// </summary>
    public class ShiftsIndexViewModel
    {
        public IEnumerable<ShiftDto> Shifts { get; set; } = new List<ShiftDto>();
        public CreateShiftDto NewShift { get; set; } = new CreateShiftDto();
        public int? EditingShiftId { get; set; }
        public UpdateShiftDto? EditShift { get; set; }
    }
}
