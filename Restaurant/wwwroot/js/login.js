

let isLoggedIn = false;
let currentUser = {
    name: '',
    type: '',
    email: ''
};

// Configuración de opciones por tipo de usuario
const userMenuOptions = {
    cliente: [
        { icon: 'fas fa-calendar-alt', text: 'Mis Reservas', href: '#reservas' },
        { icon: 'fas fa-user-edit', text: 'Mi Perfil', href: '#perfil' },
        { icon: 'fas fa-history', text: 'Historial', href: '#historial' }
    ],
    empleado: [
        { icon: 'fas fa-clipboard-list', text: 'Panel Empleado', href: '#panel-empleado' },
        { icon: 'fas fa-calendar-check', text: 'Reservas del día', href: '#reservas-dia' },
        { icon: 'fas fa-user-edit', text: 'Mi Perfil', href: '#perfil' }
    ],
    admin: [
        { icon: 'fas fa-tachometer-alt', text: 'Dashboard', href: '#dashboard' },
        { icon: 'fas fa-users', text: 'Usuarios', href: '#usuarios' },
        { icon: 'fas fa-chart-bar', text: 'Reportes', href: '#reportes' },
        { icon: 'fas fa-cog', text: 'Configuración', href: '#config' }
    ]
};

// Toggle menús desktop
function toggleLoginMenu() {
    const menu = document.getElementById('loginMenu');
    const userMenu = document.getElementById('userMenu');

    userMenu.classList.add('opacity-0', 'invisible', 'translate-y-2');

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

    loginMenu.classList.add('opacity-0', 'invisible', 'translate-y-2');

    if (menu.classList.contains('opacity-0')) {
        menu.classList.remove('opacity-0', 'invisible', 'translate-y-2');
        menu.classList.add('opacity-100', 'visible', 'translate-y-0');
    } else {
        menu.classList.add('opacity-0', 'invisible', 'translate-y-2');
        menu.classList.remove('opacity-100', 'visible', 'translate-y-0');
    }
}

// Toggle menús mobile
function toggleMobileLoginMenu() {
    const menu = document.getElementById('mobileLoginMenu');
    const userMenu = document.getElementById('mobileUserMenu');

    userMenu.classList.add('hidden');
    menu.classList.toggle('hidden');
}

function toggleMobileUserMenu() {
    const menu = document.getElementById('mobileUserMenu');
    const loginMenu = document.getElementById('mobileLoginMenu');

    loginMenu.classList.add('hidden');
    menu.classList.toggle('hidden');
}

// Funciones principales
function showLoginForm(type) {
    //alert(`Redirigiendo al formulario de login para: ${type.charAt(0).toUpperCase() + type.slice(1)}`);

    window.location.href = `/Cuenta/Login?tipo=${type}`;
    closeAllMenus();
}

function showRegisterForm() {
    alert('Redirigiendo al formulario de registro de cliente');
    // window.location.href = '/Cuenta/Register';
    closeAllMenus();
}

function updateLoginState() {
    // Desktop
    if (isLoggedIn) {
        document.getElementById('loginBtn').classList.add('hidden');
        document.getElementById('loggedInBtn').classList.remove('hidden');
        document.getElementById('loggedInBtn').classList.add('flex');

        // Actualizar info usuario
        document.getElementById('userName').textContent = currentUser.name;
        document.getElementById('userType').textContent = currentUser.type.charAt(0).toUpperCase() + currentUser.type.slice(1);

        // Actualizar icono según tipo
        const iconMap = {
            cliente: 'fas fa-user',
            empleado: 'fas fa-id-badge',
            admin: 'fas fa-crown'
        };
        document.getElementById('userIcon').className = iconMap[currentUser.type] + ' text-white';

        // Cargar opciones del menú
        loadUserMenuOptions();
    } else {
        document.getElementById('loggedInBtn').classList.add('hidden');
        document.getElementById('loginBtn').classList.remove('hidden');
    }

    // Mobile
    if (isLoggedIn) {
        document.getElementById('mobileLoginBtn').classList.add('hidden');
        document.getElementById('mobileLoggedInBtn').classList.remove('hidden');
        document.getElementById('mobileUserName').textContent = currentUser.name;
        document.getElementById('mobileUserNameDisplay').textContent = currentUser.name;
        document.getElementById('mobileUserType').textContent = currentUser.type.charAt(0).toUpperCase() + currentUser.type.slice(1);

        loadMobileUserMenuOptions();
    } else {
        document.getElementById('mobileLoggedInBtn').classList.add('hidden');
        document.getElementById('mobileLoginBtn').classList.remove('hidden');
    }
}

function loadUserMenuOptions() {
    const container = document.getElementById('userMenuOptions');
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
    const options = userMenuOptions[currentUser.type] || [];

    container.innerHTML = options.map(option => `
                <a href="${option.href}" class="flex items-center space-x-3 p-2 rounded-lg hover:bg-gray-200 transition-colors duration-200">
                    <i class="${option.icon} text-restaurant-accent w-4"></i>
                    <span class="text-gray-700 text-sm">${option.text}</span>
                </a>
            `).join('');
}

function closeAllMenus() {
    // Desktop
    document.getElementById('loginMenu').classList.add('opacity-0', 'invisible', 'translate-y-2');
    document.getElementById('userMenu').classList.add('opacity-0', 'invisible', 'translate-y-2');

    // Mobile
    document.getElementById('mobileLoginMenu').classList.add('hidden');
    document.getElementById('mobileUserMenu').classList.add('hidden');
}

function updateStatus() {
    const status = isLoggedIn ? `Logueado como ${currentUser.type.charAt(0).toUpperCase() + currentUser.type.slice(1)} (${currentUser.name})` : 'No logueado';
    document.getElementById('currentStatus').textContent = status;
}

// Mobile menu toggle (funcionalidad existente)
document.getElementById('mobile-menu-button').addEventListener('click', function () {
    const mobileMenu = document.getElementById('mobile-menu');
    const menuOpen = document.getElementById('menu-open');
    const menuClose = document.getElementById('menu-close');

    mobileMenu.classList.toggle('hidden');
    menuOpen.classList.toggle('hidden');
    menuClose.classList.toggle('hidden');
});

// Cerrar menús al hacer clic fuera
document.addEventListener('click', function (event) {
    const dropdown = document.getElementById('loginDropdown');
    if (!dropdown.contains(event.target)) {
        closeAllMenus();
    }
});

// Prevenir cierre del menú al hacer clic dentro
document.getElementById('loginMenu').addEventListener('click', function (e) {
    e.stopPropagation();
});

document.getElementById('userMenu').addEventListener('click', function (e) {
    e.stopPropagation();
});

// Inicializar estado
updateStatus();
