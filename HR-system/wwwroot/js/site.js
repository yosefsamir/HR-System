// HR System - Main JavaScript
// ============================================

document.addEventListener('DOMContentLoaded', function () {
    
    // ============================================
    // Sidebar Toggle
    // ============================================
    const sidebar = document.getElementById('sidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const mainContent = document.querySelector('.main-content');
    
    // Create overlay for mobile
    const overlay = document.createElement('div');
    overlay.className = 'sidebar-overlay';
    document.body.appendChild(overlay);
    
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function () {
            // Check if mobile view
            if (window.innerWidth < 992) {
                sidebar.classList.toggle('show');
                overlay.classList.toggle('show');
            } else {
                sidebar.classList.toggle('collapsed');
            }
        });
    }
    
    // Close sidebar when clicking overlay
    overlay.addEventListener('click', function () {
        sidebar.classList.remove('show');
        overlay.classList.remove('show');
    });
    
    // Handle window resize
    window.addEventListener('resize', function () {
        if (window.innerWidth >= 992) {
            sidebar.classList.remove('show');
            overlay.classList.remove('show');
        }
    });
    
    // ============================================
    // Auto-dismiss alerts after 5 seconds
    // ============================================
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
    
    // ============================================
    // Form validation styling
    // ============================================
    const forms = document.querySelectorAll('.needs-validation');
    forms.forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
    
    // ============================================
    // Confirm delete dialogs
    // ============================================
    const deleteButtons = document.querySelectorAll('[data-confirm-delete]');
    deleteButtons.forEach(function (button) {
        button.addEventListener('click', function (event) {
            const message = button.getAttribute('data-confirm-delete') || 'هل أنت متأكد من الحذف؟';
            if (!confirm(message)) {
                event.preventDefault();
            }
        });
    });
    
    // ============================================
    // Toggle Is Absent checkbox behavior
    // ============================================
    const isAbsentCheckbox = document.getElementById('Is_absent');
    const checkInTimeInput = document.getElementById('Check_In_time');
    const checkOutTimeInput = document.getElementById('Check_out_time');
    
    if (isAbsentCheckbox && checkInTimeInput && checkOutTimeInput) {
        function toggleTimeInputs() {
            const isAbsent = isAbsentCheckbox.checked;
            checkInTimeInput.disabled = isAbsent;
            checkOutTimeInput.disabled = isAbsent;
            
            if (isAbsent) {
                checkInTimeInput.value = '';
                checkOutTimeInput.value = '';
            }
        }
        
        isAbsentCheckbox.addEventListener('change', toggleTimeInputs);
        toggleTimeInputs(); // Initial state
    }
    
    // ============================================
    // Tooltips initialization
    // ============================================
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipTriggerList.forEach(function (tooltipTriggerEl) {
        new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // ============================================
    // Active nav link highlighting
    // ============================================
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll('.sidebar-nav .nav-link');
    
    navLinks.forEach(function (link) {
        const href = link.getAttribute('href');
        if (href && currentPath.includes(href.toLowerCase())) {
            link.classList.add('active');
        }
    });
    
});
