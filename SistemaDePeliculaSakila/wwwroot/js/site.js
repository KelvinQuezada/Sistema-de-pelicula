// ================================================================
// Sakila Rental - JavaScript del sitio
// ================================================================

(function () {
    'use strict';

    const TEMA_KEY = 'sakila_tema';
    const btnTema = document.getElementById('btnTema');
    const iconoTema = document.getElementById('iconoTema');
    const linkTemaFooter = document.getElementById('linkTemaFooter');
    const iconoTemaFooter = document.getElementById('iconoTemaFooter');
    const textoTemaFooter = document.getElementById('textoTemaFooter');

    const ICONO_CLARO = 'bi-sun-fill';
    const ICONO_OSCURO = 'bi-moon-stars-fill';

    function obtenerTemaGuardado() {
        return localStorage.getItem(TEMA_KEY);
    }

    function guardarTema(tema) {
        localStorage.setItem(TEMA_KEY, tema);
    }

    function aplicarTema(tema) {
        const html = document.documentElement;

        if (tema === 'dark') {
            html.setAttribute('data-bs-theme', 'dark');
            if (iconoTema) iconoTema.className = 'bi bi-sun-fill';
            if (iconoTemaFooter) iconoTemaFooter.className = 'bi bi-sun-fill';
            if (textoTemaFooter) textoTemaFooter.textContent = 'Modo claro';
            if (btnTema) btnTema.setAttribute('aria-pressed', 'true');
        } else {
            html.setAttribute('data-bs-theme', 'light');
            if (iconoTema) iconoTema.className = 'bi bi-moon-stars-fill';
            if (iconoTemaFooter) iconoTemaFooter.className = 'bi bi-moon-stars-fill';
            if (textoTemaFooter) textoTemaFooter.textContent = 'Modo oscuro';
            if (btnTema) btnTema.setAttribute('aria-pressed', 'false');
        }
    }

    function alternarTema() {
        const html = document.documentElement;
        const temaActual = html.getAttribute('data-bs-theme');
        const nuevoTema = temaActual === 'dark' ? 'light' : 'dark';

        aplicarTema(nuevoTema);
        guardarTema(nuevoTema);
    }

    // Inicializar al cargar la página
    document.addEventListener('DOMContentLoaded', function () {
        const temaGuardado = obtenerTemaGuardado();
        const temaPreferidoSistema = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        const temaInicial = temaGuardado || temaPreferidoSistema;

        aplicarTema(temaInicial);

        if (btnTema) {
            btnTema.addEventListener('click', alternarTema);
        }

        if (linkTemaFooter) {
            linkTemaFooter.addEventListener('click', function (e) {
                e.preventDefault();
                alternarTema();
            });
        }
    });

    // ================================================================
    // Validación visual de formularios Bootstrap
    // ================================================================
    const forms = document.querySelectorAll('.needs-validation');

    Array.from(forms).forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
})();
