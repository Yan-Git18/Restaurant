// Variables de estado del usuario
let isLoggedIn = false;
let currentUser = {
    name: '',
    type: '',
    email: ''
};

// Configuración de opciones por tipo de usuario
const userMenuOptions = {
    cliente: [
        { icon: 'fas fa-calendar-alt', text: 'Mis Reservas', href: '/Cliente/Reservas' },
        { icon: 'fas fa-user-edit', text: 'Mi Perfil', href: '/Cliente/Perfil' },
        { icon: 'fas fa-history', text: 'Historial', href: '/Cliente/Historial' }
    ],
    empleado: [
        { icon: 'fas fa-clipboard-list', text: 'Panel Empleado', href: '/Empleado/Index' },
        { icon: 'fas fa-calendar-check', text: 'Reservas del día', href: '/Empleado/Reservas' },
        { icon: 'fas fa-user-edit', text: 'Mi Perfil', href: '/Empleado/Perfil' }
    ],
    admin: [
        { icon: 'fas fa-tachometer-alt', text: 'Dashboard', href: '/Admin/Index' },
        { icon: 'fas fa-users', text: 'Usuarios', href: '/Admin/Usuarios' },
        { icon: 'fas fa-chart-bar', text: 'Reportes', href: '/Admin/Reportes' },
        { icon: 'fas fa-cog', text: 'Configuración', href: '/Admin/Configuracion' }
    ]
};

// Toggle menús desktop
function toggleLoginMenu() {
    const menu = document.getElementById('loginMenu');
    const userMenu = document.getElementById('userMenu');

    if (userMenu) {
        userMenu.classList.add('opacity-0', 'invisible', 'translate-y-2');
    }

    if (menu.classList.contains('opacity-0')) {
        menu.classList.remove('opacity-0', 'invisible', 'translate-y-2');
        menu.classList.add('opacity-100', 'visible', 'translate-y-0');
    } else {
        menu.classList.add('opacity-0', 'invisible', 'translate-y-2');
        menu.classList.remove('opacity-100', 'visible', 'translate-y-0');
    }
}

function toggleUserMenu() {
    const menu = document.getElementById('userMenu');
    const loginMenu = document.getElementById('loginMenu');

    if (loginMenu) {
        loginMenu.classList.add('opacity-0', 'invisible', 'translate-y-2');
    }

    if (menu.classList.contains('opacity-0')) {
        menu.classList.remove('opacity-0', 'invisible', 'translate-y-2');
        menu.classList.add('opacity-100', 'visible', 'translate-y-0');
    } else {
        menu.classList.add('opacity-0', 'invisible', 'translate-y-2');
        menu.classList.remove('opacity-100', 'visible', 'translate-y-0');
    }
}

// Toggle menú de usuario logueado en móvil
window.toggleMobileUserMenu = function () {
    const menu = document.getElementById('mobileUserMenu');
    if (menu) {
        menu.classList.toggle('hidden');
    }
};

// Logout
window.logout = function () {
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Cuenta/Logout';
    document.body.appendChild(form);
    form.submit();
};

// Cargar opciones del menú
function loadUserMenuOptions() {
    const container = document.getElementById('userMenuOptions');
    if (!container) return;

    const options = userMenuOptions[currentUser.type] || [];
    container.innerHTML = options.map(option => `
        <a href="${option.href}" class="flex items-center space-x-3 p-3 rounded-lg hover:bg-gray-50 transition-colors duration-200">
            <i class="${option.icon} text-restaurant-accent w-5"></i>
            <span class="text-gray-700">${option.text}</span>
        </a>
    `).join('');
}

function loadMobileUserMenuOptions() {
    const container = document.getElementById('mobileUserMenuOptions');
    if (!container) return;

    const options = userMenuOptions[currentUser.type] || [];
    container.innerHTML = options.map(option => `
        <a href="${option.href}" class="flex items-center space-x-3 p-2 rounded-lg hover:bg-gray-200 transition-colors duration-200">
            <i class="${option.icon} text-restaurant-accent w-4"></i>
            <span class="text-gray-700 text-sm">${option.text}</span>
        </a>
    `).join('');
}

// Actualizar estado de login
function updateLoginState() {
    const loginBtn = document.getElementById('loginBtn');
    const loggedInBtn = document.getElementById('loggedInBtn');
    const mobileLoginBtn = document.getElementById('mobileLoginBtn');
    const mobileLoggedInBtn = document.getElementById('mobileLoggedInBtn');

    if (isLoggedIn) {
        // Desktop
        if (loginBtn) loginBtn.classList.add('hidden');
        if (loggedInBtn) {
            loggedInBtn.classList.remove('hidden');
            loggedInBtn.classList.add('flex');
        }

        const userName = document.getElementById('userName');
        const userType = document.getElementById('userType');
        const userIcon = document.getElementById('userIcon');

        if (userName) userName.textContent = currentUser.name;
        if (userType) userType.textContent = currentUser.type.charAt(0).toUpperCase() + currentUser.type.slice(1);

        const iconMap = {
            cliente: 'fas fa-user',
            empleado: 'fas fa-id-badge',
            admin: 'fas fa-crown'
        };
        if (userIcon) userIcon.className = iconMap[currentUser.type] + ' text-white';

        loadUserMenuOptions();

        // Mobile
        if (mobileLoginBtn) mobileLoginBtn.classList.add('hidden');
        if (mobileLoggedInBtn) mobileLoggedInBtn.classList.remove('hidden');

        const mobileUserName = document.getElementById('mobileUserName');
        const mobileUserNameDisplay = document.getElementById('mobileUserNameDisplay');
        const mobileUserType = document.getElementById('mobileUserType');

        if (mobileUserName) mobileUserName.textContent = currentUser.name;
        if (mobileUserNameDisplay) mobileUserNameDisplay.textContent = currentUser.name;
        if (mobileUserType) mobileUserType.textContent = currentUser.type.charAt(0).toUpperCase() + currentUser.type.slice(1);

        loadMobileUserMenuOptions();
    } else {
        // Desktop
        if (loggedInBtn) loggedInBtn.classList.add('hidden');
        if (loginBtn) loginBtn.classList.remove('hidden');

        // Mobile
        if (mobileLoggedInBtn) mobileLoggedInBtn.classList.add('hidden');
        if (mobileLoginBtn) mobileLoginBtn.classList.remove('hidden');
    }
}

// Cerrar menús
function closeAllMenus() {
    const loginMenu = document.getElementById('loginMenu');
    const userMenu = document.getElementById('userMenu');
    const mobileUserMenu = document.getElementById('mobileUserMenu');

    if (loginMenu) loginMenu.classList.add('opacity-0', 'invisible', 'translate-y-2');
    if (userMenu) userMenu.classList.add('opacity-0', 'invisible', 'translate-y-2');
    if (mobileUserMenu) mobileUserMenu.classList.add('hidden');
}

// Inicialización
document.addEventListener('DOMContentLoaded', function () {
    // Mobile menu hamburger toggle
    const mobileMenuButton = document.getElementById('mobile-menu-button');
    if (mobileMenuButton) {
        mobileMenuButton.addEventListener('click', function () {
            const mobileMenu = document.getElementById('mobile-menu');
            const menuOpen = document.getElementById('menu-open');
            const menuClose = document.getElementById('menu-close');

            if (mobileMenu) mobileMenu.classList.toggle('hidden');
            if (menuOpen) menuOpen.classList.toggle('hidden');
            if (menuClose) menuClose.classList.toggle('hidden');
        });
    }

    // Cerrar menús al hacer clic fuera
    document.addEventListener('click', function (event) {
        const dropdown = document.getElementById('loginDropdown');
        if (dropdown && !dropdown.contains(event.target)) {
            closeAllMenus();
        }
    });

    // Prevenir cierre del menú al hacer clic dentro
    const loginMenu = document.getElementById('loginMenu');
    if (loginMenu) {
        loginMenu.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    }

    const userMenu = document.getElementById('userMenu');
    if (userMenu) {
        userMenu.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    }

    // Tabs del menú
    const menuTabs = document.querySelectorAll('.menu-tab');
    const menuItems = document.querySelectorAll('.menu-item');

    menuTabs.forEach(tab => {
        tab.addEventListener('click', () => {
            const category = tab.getAttribute('data-category');

            menuTabs.forEach(t => {
                t.classList.remove('active', 'bg-restaurant-accent', 'text-white');
                t.classList.add('bg-white/10', 'text-white');
            });

            tab.classList.remove('bg-white/10');
            tab.classList.add('active', 'bg-restaurant-accent', 'text-white');

            menuItems.forEach(item => {
                const itemCategory = item.getAttribute('data-category');

                if (category === 'all' || itemCategory === category) {
                    item.style.display = 'block';
                } else {
                    item.style.display = 'none';
                }
            });
        });
    });

    // Inicializar estado
    updateLoginState();
});