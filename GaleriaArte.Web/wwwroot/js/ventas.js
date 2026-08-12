// =====================================================
// REGISTRO DE VENTA
// Arma el resumen de la compra y los campos que se envian
// al servidor. La cantidad solo se habilita cuando el
// producto esta marcado, y nunca supera las existencias.
// =====================================================

(function () {

    const checks =
        document.querySelectorAll(".producto-check");

    const filas =
        document.querySelectorAll(".producto-row");

    const buscar =
        document.getElementById("buscarProducto");

    const sinResultados =
        document.getElementById("sinResultados");

    const detalle =
        document.getElementById("detalleSeleccionado");

    const mensajeVacio =
        document.getElementById("mensajeVacio");

    const subtotalTexto =
        document.getElementById("subtotalTexto");

    const impuestoTexto =
        document.getElementById("impuestoTexto");

    const totalTexto =
        document.getElementById("totalTexto");

    const campos =
        document.getElementById("camposProductos");

    const btnRegistrar =
        document.getElementById("btnRegistrar");

    if (!detalle || !campos) {
        return;
    }

    // Sin tildes y en minusculas, igual que el filtro del servidor
    function normalizar(texto) {
        return (texto || "")
            .toLowerCase()
            .normalize("NFD")
            .replace(/[̀-ͯ]/g, "")
            .trim();
    }

    function formatoMoneda(valor) {
        return new Intl.NumberFormat("es-CR", {
            style: "currency",
            currency: "CRC"
        }).format(valor);
    }

    // =====================================================
    // FILTRAR LA TABLA DE PRODUCTOS
    // =====================================================

    function filtrar() {
        const termino = normalizar(buscar.value);
        let visibles = 0;

        filas.forEach(function (fila) {

            const coincide =
                normalizar(fila.dataset.busqueda).includes(termino);

            fila.style.display = coincide ? "" : "none";

            if (coincide) {
                visibles++;
            }
        });

        if (sinResultados) {
            sinResultados.style.display =
                visibles === 0 ? "" : "none";
        }
    }

    // =====================================================
    // RECALCULAR EL RESUMEN Y LOS CAMPOS A ENVIAR
    // =====================================================

    function actualizar() {

        const marcados =
            Array.prototype.filter.call(
                checks,
                function (check) { return check.checked; });

        detalle.innerHTML = "";
        campos.innerHTML = "";

        let subtotal = 0;

        if (marcados.length === 0) {
            detalle.appendChild(mensajeVacio);
            btnRegistrar.disabled = true;
        }
        else {
            marcados.forEach(function (check, indice) {

                const cantidadCampo =
                    check.closest("tr")
                         .querySelector(".producto-cantidad");

                const cantidad =
                    Math.max(1, Number(cantidadCampo.value) || 1);

                const precio =
                    Number(check.dataset.precio);

                const importe = precio * cantidad;

                subtotal += importe;

                const linea =
                    document.createElement("div");

                linea.className = "resumen-linea";

                const nombre =
                    document.createElement("span");

                nombre.textContent =
                    cantidad + " x " + check.dataset.nombre;

                const monto =
                    document.createElement("strong");

                monto.textContent = formatoMoneda(importe);

                linea.appendChild(nombre);
                linea.appendChild(monto);
                detalle.appendChild(linea);

                campos.appendChild(
                    campoOculto(
                        "Productos[" + indice + "].IdProducto",
                        check.dataset.id));

                campos.appendChild(
                    campoOculto(
                        "Productos[" + indice + "].Cantidad",
                        cantidad));
            });

            btnRegistrar.disabled = false;
        }

        const impuesto = subtotal * 0.13;

        subtotalTexto.textContent = formatoMoneda(subtotal);
        impuestoTexto.textContent = formatoMoneda(impuesto);
        totalTexto.textContent = formatoMoneda(subtotal + impuesto);
    }

    function campoOculto(nombre, valor) {
        const campo = document.createElement("input");
        campo.type = "hidden";
        campo.name = nombre;
        campo.value = valor;
        return campo;
    }

    // =====================================================
    // EVENTOS
    // =====================================================

    checks.forEach(function (check) {

        const cantidad =
            check.closest("tr")
                 .querySelector(".producto-cantidad");

        check.addEventListener("change", function () {

            cantidad.disabled = !check.checked;

            if (check.checked && Number(cantidad.value) < 1) {
                cantidad.value = 1;
            }

            actualizar();
        });

        cantidad.addEventListener("input", function () {

            const maximo = Number(check.dataset.stock);

            if (Number(cantidad.value) > maximo) {
                cantidad.value = maximo;
            }

            actualizar();
        });
    });

    if (buscar) {
        buscar.addEventListener("input", filtrar);
    }

    actualizar();

})();
