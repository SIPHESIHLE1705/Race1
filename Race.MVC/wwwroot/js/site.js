// Tiny helper: mobile menu toggle and dismissible alerts (no framework).
document.addEventListener('click', function (e) {
    var toggle = e.target.closest('[data-toggle-nav]');
    if (toggle) {
        var nav = document.getElementById(toggle.getAttribute('data-toggle-nav'));
        if (nav) nav.classList.toggle('show');
        return;
    }
    var dismiss = e.target.closest('[data-dismiss]');
    if (dismiss) {
        var alertBox = dismiss.closest('.alert');
        if (alertBox) alertBox.remove();
    }
});
