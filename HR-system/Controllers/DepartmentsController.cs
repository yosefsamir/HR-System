using HR_system.DTOs.Department;
using HR_system.Services.Interfaces;
using HR_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HR_system.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // GET: Departments
        public async Task<IActionResult> Index(int? editId)
        {
            var departments = await _departmentService.GetAllAsync();
            
            var viewModel = new DepartmentsIndexViewModel
            {
                Departments = departments,
                NewDepartment = new CreateDepartmentDto()
            };

            if (editId.HasValue)
            {
                var dept = await _departmentService.GetByIdAsync(editId.Value);
                if (dept != null)
                {
                    viewModel.EditingDepartmentId = editId;
                    viewModel.EditDepartment = new UpdateDepartmentDto
                    {
                        Department_name = dept.Department_name
                    };
                }
            }

            return View(viewModel);
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentsIndexViewModel model)
        {
            ModelState.Clear();
            if (string.IsNullOrWhiteSpace(model.NewDepartment.Department_name))
            {
                TempData["Error"] = "اسم القسم مطلوب";
                return RedirectToAction(nameof(Index));
            }

            if (model.NewDepartment.Department_name.Length < 2 || model.NewDepartment.Department_name.Length > 100)
            {
                TempData["Error"] = "اسم القسم يجب أن يكون بين 2 و 100 حرف";
                return RedirectToAction(nameof(Index));
            }

            if (!await _departmentService.IsNameUniqueAsync(model.NewDepartment.Department_name))
            {
                TempData["Error"] = "يوجد قسم بهذا الاسم بالفعل";
                return RedirectToAction(nameof(Index));
            }

            await _departmentService.CreateAsync(model.NewDepartment);
            TempData["Success"] = "تم إضافة القسم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentsIndexViewModel model)
        {
            if (model.EditDepartment == null || string.IsNullOrWhiteSpace(model.EditDepartment.Department_name))
            {
                TempData["Error"] = "اسم القسم مطلوب";
                return RedirectToAction(nameof(Index));
            }

            if (model.EditDepartment.Department_name.Length < 2 || model.EditDepartment.Department_name.Length > 100)
            {
                TempData["Error"] = "اسم القسم يجب أن يكون بين 2 و 100 حرف";
                return RedirectToAction(nameof(Index));
            }

            if (!await _departmentService.IsNameUniqueAsync(model.EditDepartment.Department_name, id))
            {
                TempData["Error"] = "يوجد قسم بهذا الاسم بالفعل";
                return RedirectToAction(nameof(Index), new { editId = id });
            }

            var result = await _departmentService.UpdateAsync(id, model.EditDepartment);
            if (result == null)
            {
                TempData["Error"] = "القسم غير موجود";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "تم تحديث القسم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // POST: Departments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _departmentService.DeleteAsync(id);
                if (!result)
                {
                    TempData["Error"] = "القسم غير موجود";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "تم حذف القسم بنجاح";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
