function manejarClickAgregarPaso() {

    //Para que no se agregue más de un campo de Descripción para un nuevo paso
    const indice = tareaEditarViewModel.pasos().findIndex(p => p.esNuevo());

    if (indice !== -1) {
        return;
    }
    //

    tareaEditarViewModel.pasos.push(new pasoViewModel({
        modoEdicion: true,
        realizado: false
    }));

    $("[name=txtPasoDescripcion]:visible").focus();
}

function manejarCancelarPaso(paso) {
    if (paso.esNuevo()) {
        tareaEditarViewModel.pasos.pop();
    } else {
        paso.modoEdicion(false);
        paso.descripcion(paso.descripcionAnterior);
    }
}

async function manejarGuardarPaso(paso) {
    paso.modoEdicion(false);
    const esNuevo = paso.esNuevo();
    const idTarea = tareaEditarViewModel.id;
    const data = obtenerCuerpoPeticionPaso(paso);

    const descripcion = paso.descripcion();

    if (!descripcion) {
        paso.descripcion(paso.descripcionAnterior);

        if (esNuevo) {
            tareaEditarViewModel.pasos.pop();
        }

        return;
    }

    if (esNuevo) {
        await insertarPaso(paso, data, idTarea);
    } else {
        await actualizarPaso(data, paso.id());
    }
}

function obtenerCuerpoPeticionPaso(paso) {
    return JSON.stringify({
        descripcion: paso.descripcion(),
        realizado: paso.realizado()
    });
}

async function insertarPaso(paso, data, idTarea) {
    const respuesta = await fetch(`${urlPasos}/${idTarea}`, {
        method: 'POST',
        body: data,
        headers: { 'Content-Type': 'application/json' }
    });

    if (respuesta.ok) {
        const json = await respuesta.json();
        paso.id(json.id);

        //Le agregamos virtualmente un paso a la tarea para que se actualice en el front
        const tarea = obtenerTareaEnEdicion();
        tarea.pasosTotal(tarea.pasosTotal() + 1);

        //Y si el paso estaba realizado, entonces actualizamos los pasos realizados
        if (paso.realizado()) {
            tarea.pasosRealizados(tarea.pasosRealizados() + 1);
        }

    } else {
        await manejarErrorApi(respuesta);
    }
}

function manejarClickDescripcionPaso(paso) {
    paso.modoEdicion(true);
    paso.descripcionAnterior = paso.descripcion();
    $("[name=txtPasoDescripcion]:visible").focus();
}

async function actualizarPaso(data, id) {
    const respuesta = await fetch(`${urlPasos}/${id}`, {
        method: 'PUT',
        body: data,
        headers: { 'Content-Type': 'application/json' }
    });

    if (!respuesta.ok) {
        await manejarErrorApi(respuesta);
    }
}

function manejarClickCheckboxPaso(paso) {

    if (paso.esNuevo()) {
        return true;
    }

    const data = obtenerCuerpoPeticionPaso(paso);
    actualizarPaso(data, paso.id());

    //Actualizamos el total de pasos realizados
    const tarea = obtenerTareaEnEdicion();
    let pasosRealizadosActual = tarea.pasosRealizados();

    if (paso.realizado()) {
        pasosRealizadosActual++;
    } else {
        pasosRealizadosActual--;
    }
    tarea.pasosRealizados(pasosRealizadosActual);
    //

    return true;
}

function manejarClickBorrarPaso(paso) {
    modalEditarTareaBootstrap.hide();
    confirmarAccion({
        callbackAceptar: () => {
            borrarPaso(paso);
            modalEditarTareaBootstrap.show();
        },
        callbackCancelar: () => {
            modalEditarTareaBootstrap.show();
        },
        titulo: `¿Desea borrar este paso?`
    })
}

async function borrarPaso(paso) {
    const respuesta = await fetch(`${urlPasos}/${paso.id()}`, {
        method: 'DELETE'
    });

    if (!respuesta.ok) {
        await manejarErrorApi(respuesta);
        return;
    }

    tareaEditarViewModel.pasos.remove(function (item) { return item.id() == paso.id() });

    //Actualizamos el total de pasos
    const tarea = obtenerTareaEnEdicion();
    tarea.pasosTotal(tarea.pasosTotal() - 1);

    if (paso.realizado()) {
        tarea.pasosRealizados(tarea.pasosRealizados() - 1);
    }
    //
}

async function actualizarOrdenPasos() {
    const ids = obtenerIdsPasos();
    await enviarIdsPasosAlBackend(ids);

    const arregloOrganizado = tareaEditarViewModel.pasos.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    });

    tareaEditarViewModel.pasos([]);
    tareaEditarViewModel.pasos(arregloOrganizado);
}

function obtenerIdsPasos() {
    const ids = $("[name=chbPaso]").map(function () {
        return $(this).attr('data-id')
    }).get();
    return ids;
}

async function enviarIdsPasosAlBackend(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${urlPasos}/ordenar/${tareaEditarViewModel.id}`, {
        method: 'POST',
        body: data,
        headers: { 'Content-Type': 'application/json' }
    });
}

//Para reordenar los Pasos
$(function () {
    $("#reordenable-pasos").sortable({
        axis: 'y',
        stop: async function () {
            await actualizarOrdenPasos();
        }
    });
});