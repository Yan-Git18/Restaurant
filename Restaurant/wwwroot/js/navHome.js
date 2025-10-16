document.addEventListener("DOMContentLoaded", () => {
    const loginBtn = document.getElementById("loginBtn");
    const loggedInBtn = document.getElementById("loggedInBtn");
    const loginMenu = document.getElementById("loginMenu");
    const userMenu = document.getElementById("userMenu");
    const userNameEl = document.getElementById("userName");
    const userTypeEl = document.getElementById("userType");
    const userMenuOptions = document.getElementById("userMenuOptions");
    const user = window.userData;

    // Función para mostrar/ocultar menú
    const toggleMenu = (menu) => {
        const isVisible = !menu.classList.contains("invisible");
        if (isVisible) {
            menu.classList.add("opacity-0", "invisible", "translate-y-2");
        } else {
            menu.classList.remove("opacity-0", "invisible", "translate-y-2");
        }
    };

    // Mostrar menú correcto según autenticación
    if (user.isAuthenticated) {
        loginBtn.classList.add("hidden");
        loggedInBtn.classList.remove("hidden");
        loginMenu.classList.add("hidden");

        userNameEl.textContent = user.name || "Usuario";
        userTypeEl.textContent = user.role;

        let menuHtml = "";

        switch (user.role.toLowerCase()) {
            case "cajero":
                menuHtml = `
                    <a href="/Admin/Index" class="flex items-center gap-3 p-3 hover:bg-gray-100 rounded-lg">
                        <i class="fas fa-briefcase text-restaurant-accent"></i> Ir al panel
                    </a>`;
                break;

            case "mesero":
                menuHtml = `
                    <a href="/Admin/Index" class="flex items-center gap-3 p-3 hover:bg-gray-100 rounded-lg">
                        <i class="fas fa-briefcase text-restaurant-accent"></i> Ir al panel
                    </a>`;
                break;

            case "administrador":
                menuHtml = `
                    <a href="/Admin/Index" class="flex items-center gap-3 p-3 hover:bg-gray-100 rounded-lg">
                        <i class="fas fa-cogs text-restaurant-accent"></i> Administración
                    </a>`;
                break;
        }

        menuHtml += `
            <form action="/Cuenta/Logout" method="post">
                <button type="submit" class="flex w-full items-center gap-3 p-3 text-left hover:bg-gray-100 rounded-lg text-red-600">
                    <i class="fas fa-sign-out-alt"></i> Cerrar sesión
                </button>
            </form>`;

        userMenuOptions.innerHTML = menuHtml;
    } else {
        loginBtn.classList.remove("hidden");
        loggedInBtn.classList.add("hidden");
    }

    // Eventos de clic
    loginBtn?.addEventListener("click", (e) => {
        e.stopPropagation();
        toggleMenu(loginMenu);
    });

    loggedInBtn?.addEventListener("click", (e) => {
        e.stopPropagation();
        toggleMenu(userMenu);
    });

    // Cerrar menús al hacer clic fuera
    document.addEventListener("click", (e) => {
        if (!loginMenu.contains(e.target) && !loginBtn.contains(e.target)) {
            loginMenu.classList.add("opacity-0", "invisible", "translate-y-2");
        }
        if (!userMenu.contains(e.target) && !loggedInBtn.contains(e.target)) {
            userMenu.classList.add("opacity-0", "invisible", "translate-y-2");
        }
    });
});
