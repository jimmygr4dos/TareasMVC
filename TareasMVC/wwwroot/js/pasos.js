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