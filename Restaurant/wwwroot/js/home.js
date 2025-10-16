// Animation classes usando CSS puro
const animationCSS = `
<style>
@keyframes fade-in-up {
    from { opacity: 0; transform: translateY(30px); }
    to { opacity: 1; transform: translateY(0); }
}

@keyframes fade-in-left {
    from { opacity: 0; transform: translateX(30px); }
    to { opacity: 1; transform: translateX(0); }
}

.animate-fade-in-up { animation: fade-in-up 0.8s ease-out; }
.animate-fade-in-left { animation: fade-in-left 1s ease-out; }
.animation-delay-200 { animation-delay: 0.2s; animation-fill-mode: both; }
.animation-delay-400 { animation-delay: 0.4s; animation-fill-mode: both; }
.animation-delay-600 { animation-delay: 0.6s; animation-fill-mode: both; }
.animation-delay-800 { animation-delay: 0.8s; animation-fill-mode: both; }
.slow { animation-duration: 3s; }
</style>
`;

document.addEventListener("DOMContentLoaded", () => {
    // ===== FILTROS =====
    const tabs = document.querySelectorAll("#menu-tabs .menu-tab");
    const items = document.querySelectorAll("#menu-items .menu-item");

    tabs.forEach(tab => {
        tab.addEventListener("click", () => {
            const category = tab.getAttribute("data-category");

            // Activar/desactivar clases de tabs
            tabs.forEach(t => t.classList.remove("active", "bg-restaurant-accent", "text-white"));
            tabs.forEach(t => t.classList.add("bg-white/10", "text-white"));
            tab.classList.add("active", "bg-restaurant-accent", "text-white");
            tab.classList.remove("bg-white/10");

            // Mostrar u ocultar items
            items.forEach(item => {
                if (category === "all" || item.getAttribute("data-category") === category) {
                    item.classList.remove("hidden");
                } else {
                    item.classList.add("hidden");
                }
            });
        });
    });

    // ===== QUITAR BOTONES "AGREGAR" =====
    const addButtons = document.querySelectorAll("#menu-items button");
    addButtons.forEach(btn => btn.remove());
});

