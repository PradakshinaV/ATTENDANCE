// Attendance System JavaScript Functions

$(document).ready(function() {
    // Initialize tooltips and popovers
    if (typeof $().tooltip === 'function') {
        $('[data-toggle="tooltip"]').tooltip();
    }
    
    // Auto-refresh functionality for attendance pages
    if (window.location.pathname.includes('MarkAttendance')) {
        setInterval(function() {
            location.reload();
        }, 30000); // Refresh every 30 seconds
    }
    
    // Initialize date pickers
    $('input[type="date"]').each(function() {
        if (!$(this).val()) {
            $(this).val(new Date().toISOString().split('T')[0]);
        }
    });
});

// Mark attendance function
function markAttendance(studentId, classId, status) {
    // Show loading state
    $('button[onclick*="' + studentId + '"]').prop('disabled', true).html('<span class="loading"></span> Processing...');
    
    $.ajax({
        url: '/Attendance/MarkAttendance',
        type: 'POST',
        data: {
            studentId: studentId,
            classId: classId,
            status: status,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                showAlert(response.message, 'success');
                setTimeout(function() {
                    location.reload();
                }, 1000);
            } else {
                showAlert(response.message, 'danger');
                resetButtons(studentId);
            }
        },
        error: function(xhr, status, error) {
            showAlert('An error occurred while marking attendance: ' + error, 'danger');
            resetButtons(studentId);
        }
    });
}

// Reset button states
function resetButtons(studentId) {
    $('button[onclick*="' + studentId + '"]').prop('disabled', false).each(function() {
        var originalText = $(this).data('original-text');
        if (!originalText) {
            originalText = $(this).html();
            $(this).data('original-text', originalText);
        }
        $(this).html(originalText);
    });
}

// Show alert messages
function showAlert(message, type) {
    var alertClass = 'alert-' + type;
    var alertHtml = '<div class="alert ' + alertClass + ' alert-dismissible fade show" role="alert">' +
        message +
        '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
        '</div>';
    
    // Remove existing alerts
    $('.alert').remove();
    
    // Add new alert
    $('.main-content').prepend(alertHtml);
    
    // Auto-dismiss after 5 seconds
    setTimeout(function() {
        $('.alert').fadeOut();
    }, 5000);
}

// Filter functions for reports
function filterByDate(fromDate, toDate) {
    var url = new URL(window.location.href);
    url.searchParams.set('fromDate', fromDate);
    url.searchParams.set('toDate', toDate);
    window.location.href = url.toString();
}

// Export functions
function exportAttendance(classId, fromDate, toDate) {
    var url = '/Attendance/Export?classId=' + classId;
    if (fromDate) url += '&fromDate=' + fromDate;
    if (toDate) url += '&toDate=' + toDate;
    window.open(url, '_blank');
}

function exportStudentHistory(fromDate, toDate) {
    var url = '/Student/Export';
    if (fromDate) url += '?fromDate=' + fromDate;
    if (toDate) url += (fromDate ? '&' : '?') + 'toDate=' + toDate;
    window.open(url, '_blank');
}

// Modal functions
function showModal(modalId) {
    $('#' + modalId).modal('show');
}

function hideModal(modalId) {
    $('#' + modalId).modal('hide');
}

// Form validation
function validateForm(formId) {
    var form = $('#' + formId);
    var isValid = true;
    
    form.find('input[required], select[required]').each(function() {
        if (!$(this).val()) {
            $(this).addClass('is-invalid');
            isValid = false;
        } else {
            $(this).removeClass('is-invalid');
        }
    });
    
    return isValid;
}

// Password strength indicator
function checkPasswordStrength(password) {
    var strength = 0;
    if (password.length >= 8) strength++;
    if (password.match(/[a-z]/)) strength++;
    if (password.match(/[A-Z]/)) strength++;
    if (password.match(/[0-9]/)) strength++;
    if (password.match(/[^a-zA-Z0-9]/)) strength++;
    
    var strengthText = ['Very Weak', 'Weak', 'Fair', 'Good', 'Strong'];
    var strengthClass = ['danger', 'warning', 'info', 'primary', 'success'];
    
    return {
        score: strength,
        text: strengthText[strength - 1] || 'Very Weak',
        class: strengthClass[strength - 1] || 'danger'
    };
}

// Real-time clock
function updateClock() {
    var now = new Date();
    var timeString = now.toLocaleTimeString();
    $('.current-time').text(timeString);
}

// Update clock every second
setInterval(updateClock, 1000);

// Confirm delete actions
function confirmDelete(itemName, deleteUrl) {
    if (confirm('Are you sure you want to delete ' + itemName + '? This action cannot be undone.')) {
        window.location.href = deleteUrl;
    }
}

// Search functionality
function filterTable(inputId, tableId) {
    var input = document.getElementById(inputId);
    var filter = input.value.toLowerCase();
    var table = document.getElementById(tableId);
    var rows = table.getElementsByTagName('tr');
    
    for (var i = 1; i < rows.length; i++) {
        var row = rows[i];
        var cells = row.getElementsByTagName('td');
        var found = false;
        
        for (var j = 0; j < cells.length; j++) {
            if (cells[j].textContent.toLowerCase().indexOf(filter) > -1) {
                found = true;
                break;
            }
        }
        
        row.style.display = found ? '' : 'none';
    }
}

// Auto-save form data
function autoSaveForm(formId) {
    var form = $('#' + formId);
    var formData = {};
    
    form.find('input, select, textarea').each(function() {
        formData[$(this).attr('name')] = $(this).val();
    });
    
    localStorage.setItem('autosave_' + formId, JSON.stringify(formData));
}

// Restore form data
function restoreForm(formId) {
    var savedData = localStorage.getItem('autosave_' + formId);
    if (savedData) {
        var formData = JSON.parse(savedData);
        var form = $('#' + formId);
        
        $.each(formData, function(name, value) {
            form.find('[name="' + name + '"]').val(value);
        });
    }
}

// Clear auto-saved data
function clearAutoSave(formId) {
    localStorage.removeItem('autosave_' + formId);
}

// Print functionality
function printReport() {
    window.print();
}

// Keyboard shortcuts
$(document).keydown(function(e) {
    // Ctrl+S to save forms
    if (e.ctrlKey && e.which === 83) {
        e.preventDefault();
        $('form').submit();
    }
    
    // Ctrl+P to print
    if (e.ctrlKey && e.which === 80) {
        e.preventDefault();
        printReport();
    }
    
    // Escape to close modals
    if (e.which === 27) {
        $('.modal').modal('hide');
    }
});

// Mobile-friendly functions
function isMobile() {
    return window.innerWidth <= 768;
}

// Responsive table handling
function makeTablesResponsive() {
    if (isMobile()) {
        $('table').wrap('<div class="table-responsive"></div>');
    }
}

// Initialize responsive features
$(window).resize(function() {
    makeTablesResponsive();
});

// Touch-friendly enhancements for mobile
if ('ontouchstart' in window) {
    $('body').addClass('touch-device');
}

// Notification system
function showNotification(message, type, duration) {
    duration = duration || 5000;
    
    var notification = $('<div class="notification notification-' + type + '">' + message + '</div>');
    $('body').append(notification);
    
    setTimeout(function() {
        notification.fadeOut(function() {
            notification.remove();
        });
    }, duration);
}

// Initialize application
$(document).ready(function() {
    // Make tables responsive
    makeTablesResponsive();
    
    // Initialize auto-save for forms
    $('form').each(function() {
        var formId = $(this).attr('id');
        if (formId) {
            restoreForm(formId);
            
            $(this).find('input, select, textarea').change(function() {
                autoSaveForm(formId);
            });
            
            $(this).submit(function() {
                clearAutoSave(formId);
            });
        }
    });
});

// Global error handler
window.onerror = function(msg, url, lineNo, columnNo, error) {
    console.error('JavaScript Error:', {
        message: msg,
        source: url,
        line: lineNo,
        column: columnNo,
        error: error
    });
    
    // Show user-friendly error message
    showAlert('An unexpected error occurred. Please refresh the page and try again.', 'warning');
    
    return false;
};
