// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', function () {
    // Login form: disable submit button and show spinner to prevent duplicate submits
    var loginForm = document.getElementById('loginForm');
    if (loginForm) {
        var loginBtn = document.getElementById('loginBtn');
        var loginSpinner = document.getElementById('loginSpinner');
        loginForm.addEventListener('submit', function () {
            if (loginBtn) {
                loginBtn.disabled = true;
                if (loginSpinner) loginSpinner.classList.remove('d-none');
            }
        });
    }

    // Initialize flatpickr on elements with data-flatpickr attribute
    if (typeof flatpickr !== 'undefined') {
        document.querySelectorAll('[data-flatpickr]').forEach(function (el) {
            try { flatpickr(el, {}); } catch (e) { /* ignore init errors */ }
        });
    }

    // Initialize Choices.js on selects with data-choices attribute
    if (typeof Choices !== 'undefined') {
        document.querySelectorAll('[data-choices]').forEach(function (el) {
            try { new Choices(el, { shouldSort: false }); } catch (e) { /* ignore init errors */ }
        });
    }
});
