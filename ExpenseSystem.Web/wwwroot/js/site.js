document.addEventListener('DOMContentLoaded', function () {

    // Auto-hide flash messages after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.4s, transform 0.4s';
            alert.style.opacity = '0';
            alert.style.transform = 'translateY(-10px)';
            setTimeout(() => alert.remove(), 400);
        }, 5000);
    });

    // Confirm destructive actions (reject buttons in forms)
    document.querySelectorAll('form').forEach(form => {
        const submitBtn = form.querySelector('button[type="submit"]');
        if (submitBtn && submitBtn.classList.contains('btn-danger')) {
            form.addEventListener('submit', function (e) {
                if (!confirm('Are you sure you want to perform this action? This cannot be undone.')) {
                    e.preventDefault();
                }
            });
        }
    });

    // Disable submit buttons after click to prevent double-submit
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', function () {
            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                // Only disable if form is valid (browser will show validation errors otherwise)
                setTimeout(() => {
                    if (form.checkValidity()) {
                        submitBtn.disabled = true;
                        submitBtn.textContent = 'Processing...';
                    }
                }, 100);
            }
        });
    });
});