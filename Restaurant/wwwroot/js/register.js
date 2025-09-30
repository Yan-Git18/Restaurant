
// Toggle password visibility
const togglePassword = document.getElementById('toggle-password');
const passwordInput = document.getElementById('password');
const passwordIcon = document.getElementById('password-icon');

togglePassword.addEventListener('click', function () {
    const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
    passwordInput.setAttribute('type', type);

    if (type === 'text') {
        passwordIcon.classList.remove('fa-eye');
        passwordIcon.classList.add('fa-eye-slash');
    } else {
        passwordIcon.classList.remove('fa-eye-slash');
        passwordIcon.classList.add('fa-eye');
    }
});

// Toggle confirm password visibility
const toggleConfirmPassword = document.getElementById('toggle-confirm-password');
const confirmPasswordInput = document.getElementById('confirm-password');
const confirmPasswordIcon = document.getElementById('confirm-password-icon');

toggleConfirmPassword.addEventListener('click', function () {
    const type = confirmPasswordInput.getAttribute('type') === 'password' ? 'text' : 'password';
    confirmPasswordInput.setAttribute('type', type);

    if (type === 'text') {
        confirmPasswordIcon.classList.remove('fa-eye');
        confirmPasswordIcon.classList.add('fa-eye-slash');
    } else {
        confirmPasswordIcon.classList.remove('fa-eye-slash');
        confirmPasswordIcon.classList.add('fa-eye');
    }
});

// Password strength indicator
const strengthBars = [
    document.getElementById('strength-bar-1'),
    document.getElementById('strength-bar-2'),
    document.getElementById('strength-bar-3')
];
const strengthText = document.getElementById('strength-text');

passwordInput.addEventListener('input', function () {
    const password = this.value;
    let strength = 0;

    // Reset bars
    strengthBars.forEach(bar => {
        bar.classList.remove('bg-red-500', 'bg-yellow-500', 'bg-green-500');
        bar.classList.add('bg-white/20');
    });

    if (password.length === 0) {
        strengthText.textContent = 'Seguridad';
        return;
    }

    // Check password strength
    if (password.length >= 6) strength++;
    if (password.length >= 8) strength++;
    if (/[A-Z]/.test(password) && /[a-z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[^A-Za-z0-9]/.test(password)) strength++;

    // Normalize to 0-3 scale
    const normalizedStrength = Math.min(Math.floor(strength / 2), 2);

    // Update bars
    for (let i = 0; i <= normalizedStrength; i++) {
        strengthBars[i].classList.remove('bg-white/20');
        if (normalizedStrength === 0) {
            strengthBars[i].classList.add('bg-red-500');
            strengthText.textContent = 'Débil';
        } else if (normalizedStrength === 1) {
            strengthBars[i].classList.add('bg-yellow-500');
            strengthText.textContent = 'Media';
        } else {
            strengthBars[i].classList.add('bg-green-500');
            strengthText.textContent = 'Fuerte';
        }
    }
});

// Form validation
const registerForm = document.getElementById('register-form');
const generalError = document.getElementById('general-error');
const registerButton = document.getElementById('register-button');
const buttonText = document.getElementById('button-text');

registerForm.addEventListener('submit', function (e) {
    const password = passwordInput.value;
    const confirmPassword = confirmPasswordInput.value;
    const termsCheckbox = document.getElementById('terms');

    if (password !== confirmPassword) {
        e.preventDefault();
        showError('Las contraseñas no coinciden');
        return;
    }

    if (!termsCheckbox.checked) {
        e.preventDefault();
        showError('Debes aceptar los términos y condiciones');
        return;
    }

    // Show loading state
    registerButton.disabled = true;
    buttonText.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Creando cuenta...';
});

function showError(message) {
    document.getElementById('error-message').textContent = message;
    generalError.classList.remove('hidden');
    setTimeout(() => {
        generalError.classList.add('hidden');
    }, 5000);
}

// Clear error on input
const inputs = registerForm.querySelectorAll('input');
inputs.forEach(input => {
    input.addEventListener('focus', () => {
        generalError.classList.add('hidden');
    });
});
