/*!
 * Bootstrap v5.1.3 (https://getbootstrap.com/)
 * Copyright 2011-2021 The Bootstrap Authors
 * Copyright 2011-2021 Twitter, Inc.
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/main/LICENSE)
 */

// Basic Bootstrap JavaScript functionality
(function() {
    'use strict';

    // Modal functionality
    function initModals() {
        const modals = document.querySelectorAll('.modal');
        modals.forEach(modal => {
            const closeButtons = modal.querySelectorAll('[data-bs-dismiss="modal"]');
            closeButtons.forEach(button => {
                button.addEventListener('click', () => {
                    modal.style.display = 'none';
                    modal.classList.remove('show');
                });
            });
        });
    }

    // Dropdown functionality
    function initDropdowns() {
        const dropdowns = document.querySelectorAll('.dropdown-toggle');
        dropdowns.forEach(dropdown => {
            dropdown.addEventListener('click', (e) => {
                e.preventDefault();
                const menu = dropdown.nextElementSibling;
                if (menu && menu.classList.contains('dropdown-menu')) {
                    menu.style.display = menu.style.display === 'block' ? 'none' : 'block';
                }
            });
        });

        // Close dropdowns when clicking outside
        document.addEventListener('click', (e) => {
            if (!e.target.closest('.dropdown')) {
                document.querySelectorAll('.dropdown-menu').forEach(menu => {
                    menu.style.display = 'none';
                });
            }
        });
    }

    // Alert dismissal
    function initAlerts() {
        const alerts = document.querySelectorAll('.alert-dismissible .btn-close');
        alerts.forEach(button => {
            button.addEventListener('click', () => {
                button.closest('.alert').remove();
            });
        });
    }

    // Initialize when DOM is loaded
    document.addEventListener('DOMContentLoaded', function() {
        initModals();
        initDropdowns();
        initAlerts();
    });

    // Export for global use
    window.bootstrap = {
        Modal: function(element) {
            return {
                show: function() {
                    element.style.display = 'block';
                    element.classList.add('show');
                },
                hide: function() {
                    element.style.display = 'none';
                    element.classList.remove('show');
                }
            };
        }
    };
})();
