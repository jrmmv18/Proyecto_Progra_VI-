// =====================================================
// MODO OSCURO
// Guarda la preferencia en el navegador para que se
// mantenga al cambiar de pagina y entre sesiones.
// =====================================================

(function () {

    const CLAVE = "galeriaArte.tema";

    function aplicar(oscuro) {
        document.body.classList.toggle("tema-oscuro", oscuro);

        const boton = document.getElementById("btnTema");

        if (boton) {
            boton.setAttribute(
                "aria-checked",
                oscuro ? "true" : "false");
        }
    }

    document.addEventListener("DOMContentLoaded", function () {

        const boton = document.getElementById("btnTema");

        // El tema ya se aplico en el head para evitar el parpadeo,
        // aqui solo se sincroniza el estado del interruptor.
        aplicar(localStorage.getItem(CLAVE) === "oscuro");

        if (!boton) {
            return;
        }

        boton.addEventListener("click", function () {

            const oscuro =
                boton.getAttribute("aria-checked") !== "true";

            aplicar(oscuro);

            localStorage.setItem(
                CLAVE,
                oscuro ? "oscuro" : "claro");
        });
    });

})();
