// Funcionalidad del formulario de reservas
const reservationForm = document.getElementById('reservation-form');
const fechaInput = document.getElementById('fecha');

if (reservationForm && fechaInput) {
    // Establecer fecha mínima como hoy
    const today = new Date().toISOString().split('T')[0];
    fechaInput.min = today;

    // Establecer fecha máxima como 30 días desde hoy
    const maxDate = new Date();
    maxDate.setDate(maxDate.getDate() + 30);
    fechaInput.max = maxDate.toISOString().split('T')[0];

    reservationForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        const formData = new FormData(reservationForm);
        const reservationData = {
            nombre: formData.get('nombre'),
            telefono: formData.get('telefono'),
            email: formData.get('email'),
            fecha: formData.get('fecha'),
            hora: formData.get('hora'),
            personas: formData.get('personas'),
        }; // ← FALTABA CERRAR ESTA LLAVE Y ESTE PUNTO Y COMA

        // Validaciones adicionales
        if (!validateReservation(reservationData)) {
            return;
        }

        try {
            const response = await fetch(reservationForm.action, {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                const reservationData = Object.fromEntries(formData.entries());
                showReservationSuccess(reservationData);
                reservationForm.reset();
            } else {
                showAlert("Ocurrió un error al guardar tu reserva.", "error");
            }
        } catch (err) {
            console.error(err);
            showAlert("Error de conexión con el servidor.", "error");
        } finally {
            // Mostrar loading en el botón
            const submitBtn = reservationForm.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerHTML;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Procesando...';
            submitBtn.disabled = true;

            setTimeout(() => {
                showReservationSuccess(reservationData);
                reservationForm.reset();
                submitBtn.innerHTML = originalText;
                submitBtn.disabled = false;
            }, 1000); 
        } 
    }); 
} 


function validateReservation(data) {
    const selectedDate = new Date(data.fecha);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (selectedDate < today) {
        showAlert('La fecha seleccionada debe ser hoy o posterior', 'error');
        return false;
    }

    if (!data.nombre.trim() || !data.telefono.trim() || !data.email.trim()) {
        showAlert('Por favor completa todos los campos obligatorios', 'error');
        return false;
    }

    if (!isValidEmail(data.email)) {
        showAlert('Por favor ingresa un email válido', 'error');
        return false;
    }

    return true;
}

function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function showReservationSuccess(data) {
    const successModal = `
        <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4" id="success-modal">
            <div class="bg-white rounded-3xl p-8 max-w-md w-full text-center animate-fade-in-up">
                <div class="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
                    <i class="fas fa-check text-green-500 text-2xl"></i>
                </div>
                <h3 class="text-2xl font-bold text-gray-800 mb-2">¡Reserva Confirmada!</h3>
                <p class="text-gray-600 mb-4">
                    Hemos recibido tu reserva para <strong>${data.personas} persona${data.personas > 1 ? 's' : ''}</strong> 
                    el <strong>${formatDate(data.fecha)}</strong> a las <strong>${data.hora}</strong>.
                </p>
                <p class="text-sm text-gray-500 mb-6">
                    Te enviaremos un email de confirmación a <strong>${data.email}</strong> 
                    en los próximos minutos.
                </p>
                <button onclick="closeModal('success-modal')" 
                        class="bg-restaurant-accent hover:bg-restaurant-secondary text-white px-8 py-3 rounded-xl font-semibold transition-colors duration-200">
                    Perfecto
                </button>
            </div>
        </div>
    `;
    document.body.insertAdjacentHTML('beforeend', successModal);
}

function showAlert(message, type = 'info') {
    const alertClass = type === 'error' ? 'bg-red-100 border-red-500 text-red-700' : 'bg-blue-100 border-blue-500 text-blue-700';
    const alertModal = `
        <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4" id="alert-modal">
            <div class="bg-white rounded-3xl p-8 max-w-md w-full text-center animate-fade-in-up">
                <p class="${alertClass} p-4 rounded-xl border mb-4">${message}</p>
                <button onclick="closeModal('alert-modal')" 
                        class="bg-restaurant-accent hover:bg-restaurant-secondary text-white px-8 py-3 rounded-xl font-semibold">
                    Cerrar
                </button>
            </div>
        </div>
    `;
    document.body.insertAdjacentHTML('beforeend', alertModal);
}

function formatDate(dateString) {
    const date = new Date(dateString);
    const options = { weekday: "long", year: "numeric", month: "long", day: "numeric" };
    return date.toLocaleDateString("es-ES", options);
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.remove();
    }
}

// Smooth scrolling for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const targetId = this.getAttribute('href').substring(1);
        const targetSection = document.getElementById(targetId);

        if (targetSection) {
            const offsetTop = targetSection.offsetTop - 80;
            window.scrollTo({
                top: offsetTop,
                behavior: 'smooth'
            });
        }

        const mobileMenu = document.getElementById('mobile-menu');
        const menuOpen = document.getElementById('menu-open');
        const menuClose = document.getElementById('menu-close');

        if (mobileMenu && !mobileMenu.classList.contains('hidden')) {
            mobileMenu.classList.add('hidden');
            if (menuOpen) menuOpen.classList.remove('hidden');
            if (menuClose) menuClose.classList.add('hidden');
        }
    });
});

// Navbar scroll effect
window.addEventListener('scroll', () => {
    const navbar = document.getElementById('navbar');
    if (navbar) {
        if (window.scrollY > 50) {
            navbar.classList.add('bg-white', 'shadow-xl');
            navbar.classList.remove('bg-white/95');
        } else {
            navbar.classList.add('bg-white/95');
            navbar.classList.remove('bg-white', 'shadow-xl');
        }
    }
});
