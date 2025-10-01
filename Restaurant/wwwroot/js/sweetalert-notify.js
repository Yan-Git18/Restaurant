document.addEventListener('DOMContentLoaded', function () {
    const mensaje = document.body.dataset.swalMensaje;
    const tipo = document.body.dataset.swalTipo || 'success';

    if (mensaje) {
        Swal.fire({
            icon: tipo,
            title: mensaje,
            confirmButtonText: 'Aceptar',
            timer: 1500,
            timerProgressBar: true
        });
    }
});
