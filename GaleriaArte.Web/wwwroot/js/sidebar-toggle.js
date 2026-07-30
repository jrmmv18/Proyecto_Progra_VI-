document.addEventListener("DOMContentLoaded", function () {
    const toggleBtn = document.getElementById("btnToggleMenu");
    const sidebar = document.querySelector(".sidebar");
    const appContainer = document.querySelector(".app");

    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener("click", function (e) {
            e.preventDefault();

            // Toggle accessible attribute
            var expanded = toggleBtn.getAttribute('aria-expanded') === 'true';
            toggleBtn.setAttribute('aria-expanded', (!expanded).toString());

            // Use class toggle for CSS transitions
            if (sidebar.classList.contains('hidden')) {
                sidebar.classList.remove('hidden');
                if (appContainer) appContainer.classList.remove('sidebar-collapsed');
                // remove body lock
                document.body.classList.remove('no-scroll');
            } else {
                sidebar.classList.add('hidden');
                if (appContainer) appContainer.classList.add('sidebar-collapsed');
                // add body lock on mobile-sized screens
                if (window.matchMedia('(max-width: 768px)').matches) {
                    document.body.classList.add('no-scroll');
                }
            }
        });
    }
});
