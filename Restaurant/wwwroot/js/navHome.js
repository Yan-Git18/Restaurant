document.addEventListener("DOMContentLoaded", () => {
    // ===== ELEMENTOS DEL DOM =====
    // Desktop
    const loginBtn = document.getElementById("loginBtn");
    const loggedInBtn = document.getElementById("loggedInBtn");
    const loginMenu = document.getElementById("loginMenu");
    const userMenu = document.getElementById("userMenu");
    const userNameEl = document.getElementById("userName");
    const userTypeEl = document.getElementById("userType");
    const userMenuOptions = document.getElementById("userMenuOptions");

    // Mobile
    const mobileMenuButton = document.getElementById("mobile-menu-button");
    const mobileMenu = document.getElementById("mobile-menu");
    const menuOpen = document.getElementById("menu-open");
    const menuClose = document.getElementById("menu-close");
    const mobileLoginMenu = document.getElementById("mobileLoginMenu");
    const mobileLoggedInBtn = document.getElementById("mobileLoggedInBtn");
    const mobileUserMenu = document.getElementById("mobileUserMenu");
    const mobileUserNameDisplay = document.getElementById("mobileUserNameDisplay");
    const mobileUserName = document.getElementById("mobileUserName");
    const mobileUserType = document.getElementById("mobileUserType");
    const mobileUserMenuOptions = document.getElementById("mobileUserMenuOptions");

    const user = window.userData || { isAuthenticated: false };

    // ===== FUNCIONES AUXILIARES =====
    const toggleMenu = (menu) => {
        const isVisible = !menu.classList.contains("invisible");
        if (isVisible) {
            menu.classList.add("opacity-0", "invisible", "translate-y-2");
        } else {
            menu.classList.remove("opacity-0", "invisible", "translate-y-2");
        }
    };

    const generateMenuOptions = (role) => {
        let menuHtml = "";

        switch (role.toLowerCase()) {
            case "cajero":
            case "mesero":
                menuHtml = `
                    <a href="/Admin/Index" class="flex items-center gap-3 p-3 hover:bg-gray-100 rounded-lg transition-colors duration-200">
                        <i class="fas fa-briefcase text-restaurant-accent"></i>
                        <span>Ir al panel</span>
                    </a>`;
                break;
            case "administrador":
                menuHtml = `
                    <a href="/Admin/Index" class="flex items-center gap-3 p-3 hover:bg-gray-100 rounded-lg transition-colors duration-200">
                        <i class="fas fa-cogs text-restaurant-accent"></i>
                        <span>Administración</span>
                    </a>`;
                break;
            case "cliente":
                menuHtml = `
                    <a href="/Reservas/MisReservas" class="flex items-center gap-3 p-3 hover:bg-gray-100 rounded-lg transition-colors duration-200">
                        <i class="fas fa-calendar-check text-restaurant-accent"></i>
                        <span>Mis Reservas</span>
                    </a>
                    <a href="/Pedidos/MisPedidos" class="flex items-center gap-3 p-3 hover:bg-gray-100 rounded-lg transition-colors duration-200">
                        <i class="fas fa-shopping-bag text-restaurant-accent"></i>
                        <span>Mis Pedidos</span>
                    </a>`;
                break;
        }

        return menuHtml;
    };

    // ===== CONFIGURACIÓN INICIAL =====
    if (user.isAuthenticated) {
        // DESKTOP - Usuario autenticado
        loginBtn?.classList.add("hidden");
        loggedInBtn?.classList.remove("hidden");
        loginMenu?.classList.add("hidden");

        if (userNameEl) userNameEl.textContent = user.name || "Usuario";
        if (userTypeEl) userTypeEl.textContent = user.role || "Usuario";

        const menuHtml = generateMenuOptions(user.role) + `
            <form action="/Cuenta/Logout" method="post" class="mt-2 border-t border-gray-200 pt-2">
                <button type="submit" class="flex w-full items-center gap-3 p-3 text-left hover:bg-red-50 rounded-lg text-red-600 transition-colors duration-200">
                    <i class="fas fa-sign-out-alt"></i>
                    <span>Cerrar sesión</span>
                </button>
            </form>`;

        if (userMenuOptions) userMenuOptions.innerHTML = menuHtml;

        // MOBILE - Usuario autenticado
        mobileLoginMenu?.classList.add("hidden");
        mobileLoggedInBtn?.classList.remove("hidden");

        if (mobileUserNameDisplay) mobileUserNameDisplay.textContent = user.name || "Usuario";
        if (mobileUserName) mobileUserName.textContent = user.name || "Usuario";
        if (mobileUserType) mobileUserType.textContent = user.role || "Usuario";

        if (mobileUserMenuOptions) {
            mobileUserMenuOptions.innerHTML = generateMenuOptions(user.role);
        }
    } else {
        // Usuario NO autenticado
        loginBtn?.classList.remove("hidden");
        loggedInBtn?.classList.add("hidden");
        mobileLoginMenu?.classList.remove("hidden");
        mobileLoggedInBtn?.classList.add("hidden");
    }

    // ===== EVENTOS DESKTOP =====
    loginBtn?.addEventListener("click", (e) => {
        e.stopPropagation();
        toggleMenu(loginMenu);
    });

    loggedInBtn?.addEventListener("click", (e) => {
        e.stopPropagation();
        toggleMenu(userMenu);
    });

    // Cerrar menús al hacer clic fuera (Desktop)
    document.addEventListener("click", (e) => {
        if (loginMenu && !loginMenu.contains(e.target) && !loginBtn?.contains(e.target)) {
            loginMenu.classList.add("opacity-0", "invisible", "translate-y-2");
        }
        if (userMenu && !userMenu.contains(e.target) && !loggedInBtn?.contains(e.target)) {
            userMenu.classList.add("opacity-0", "invisible", "translate-y-2");
        }
    });

    // ===== EVENTOS MOBILE =====
    // Toggle menú hamburguesa
    mobileMenuButton?.addEventListener("click", () => {
        const isHidden = mobileMenu.classList.contains("hidden");

        if (isHidden) {
            mobileMenu.classList.remove("hidden");
            menuOpen.classList.add("hidden");
            menuClose.classList.remove("hidden");
        } else {
            mobileMenu.classList.add("hidden");
            menuOpen.classList.remove("hidden");
            menuClose.classList.add("hidden");
        }
    });

    // Cerrar menú móvil al hacer clic en un enlace
    const mobileLinks = mobileMenu?.querySelectorAll("a[href^='#']");
    mobileLinks?.forEach(link => {
        link.addEventListener("click", () => {
            mobileMenu.classList.add("hidden");
            menuOpen.classList.remove("hidden");
            menuClose.classList.add("hidden");
        });
    });

    // ===== FUNCIÓN GLOBAL PARA MOBILE USER MENU =====
    window.toggleMobileUserMenu = () => {
        const isHidden = mobileUserMenu.classList.contains("hidden");

        if (isHidden) {
            mobileUserMenu.classList.remove("hidden");
        } else {
            mobileUserMenu.classList.add("hidden");
        }
    };

    // ===== FUNCIÓN GLOBAL PARA LOGOUT =====
    window.logout = () => {
        const form = document.createElement("form");
        form.method = "post";
        form.action = "/Cuenta/Logout";
        document.body.appendChild(form);
        form.submit();
    };

    // ===== RESPONSIVE: Cerrar menú móvil al cambiar a desktop =====
    const handleResize = () => {
        if (window.innerWidth >= 1024) { // lg breakpoint
            mobileMenu?.classList.add("hidden");
            menuOpen?.classList.remove("hidden");
            menuClose?.classList.add("hidden");
            mobileUserMenu?.classList.add("hidden");
        }
    };

    window.addEventListener("resize", handleResize);
});