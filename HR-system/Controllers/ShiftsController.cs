using HR_system.DTOs.Shift;
using HR_system.Models.Enums;
using HR_system.Services.Interfaces;
using HR_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HR_system.Controllers
{
    public class ShiftsController : Controller
    {
        private readonly IShiftService _shiftService;

        public ShiftsController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        // GET: Shifts
        public async Task<IActionResult> Index(int? editId)
        {
            var shifts = await _shiftService.GetAllAsync();
            
            var viewModel = new ShiftsIndexViewModel
            {
                Shifts = shifts
            };

            if (editId.HasValue)
            {
                var shift = await _shiftService.GetByIdAsync(editId.Value);
                if (shift != null)
                {
                    viewModel.EditingShiftId = editId;
                    viewModel.EditShift = new UpdateShiftDto
                    {
                        Shift_name = shift.Shift_name,
                        Start_time = shift.Start_time,
                        End_time = shift.End_time,
                        Minutes_allow_attendence = shift.Minutes_allow_attendence,
                        Minutes_allow_departure = shift.Minutes_allow_departure,
                        StandardHours = shift.StandardHours,
                        IsFlexible = shift.IsFlexible,
                        SalaryCalculationType = shift.SalaryCalculationType
                    };
                }
            }

            return View(viewModel);
        }

        // GET: Shifts/Create
        public IActionResult Create()
        {
            var model = new CreateShiftDto
            {
                Start_time = new TimeSpan(9, 0, 0),
                End_time = new TimeSpan(17, 0, 0),
                Minutes_allow_attendence = 15,
                Minutes_allow_departure = 15,
                StandardHours = 8
            };
            return View(model);
        }

        // POST: Shifts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateShiftDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Shift_name))
            {
                TempData["Error"] = "اسم الوردية مطلوب";
                return View(model);
            }

            if (model.End_time <= model.Start_time)
            {
                TempData["Error"] = "وقت الانتهاء يجب أن يكون بعد وقت البداية";
                return View(model);
            }

            if (!await _shiftService.IsNameUniqueAsync(model.Shift_name))
            {
                TempData["Error"] = "يوجد وردية بهذا الاسم بالفعل";
                return View(model);
            }

            await _shiftService.CreateAsync(model);
            TempData["Success"] = "تم إضافة الوردية بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // POST: Shifts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShiftsIndexViewModel model)
        {
            if (model.EditShift == null || string.IsNullOrWhiteSpace(model.EditShift.Shift_name))
            {
                TempData["Error"] = "اسم الوردية مطلوب";
                return RedirectToAction(nameof(Index));
            }

            if (model.EditShift.End_time <= model.EditShift.Start_time)
            {
                TempData["Error"] = "وقت الانتهاء يجب أن يكون بعد وقت البداية";
                return RedirectToAction(nameof(Index), new { editId = id });
            }

            if (!await _shiftService.IsNameUniqueAsync(model.EditShift.Shift_name, id))
            {
                TempData["Error"] = "يوجد وردية بهذا الاسم بالفعل";
                return RedirectToAction(nameof(Index), new { editId = id });
            }

            var result = await _shiftService.UpdateAsync(id, model.EditShift);
            if (result == null)
            {
                TempData["Error"] = "الوردية غير موجودة";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "تم تحديث الوردية بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // POST: Shifts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _shiftService.DeleteAsync(id);
                if (!result)
                {
                    TempData["Error"] = "الوردية غير موجودة";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "تم حذف الوردية بنجاح";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
