let inputArchivoTarea = document.getElementById('archivoATarea');
function manejarClickAgregarArchivoAdjunto() {
    inputArchivoTarea.click();
}

async function manejarSeleccionArchivoTarea(event) {
    const archivos = event.target.files;
    const archivosArreglo = Array.from(archivos);

    const idTarea = tareaEditarViewModel.id;
    const formData = new FormData();

    //El "archivos" es por [FromForm] IEnumerable<IFormFile> archivos -> ArchivosController
    for (var i = 0; i < archivosArreglo.length; i++) {
        formData.append("archivos", archivosArreglo[i]);
    }

    const respuesta = await fetch(`${urlArchivos}/${idTarea}`, {
        method: 'POST',
        body: formData
    });

    if (!respuesta.ok) {
        await manejarErrorApi(respuesta);
        return;
    }

    const json = await respuesta.json();
    //console.log(json);
    prepararArchivosAdjuntos(json);

    inputArchivoTarea.value = null;
}

function prepararArchivosAdjuntos(archivosAdjuntos) {

    if (archivosAdjuntos.length === 0) {
        tareaEditarViewModel.archivosAdjuntos([]);
        return;
    }

    archivosAdjuntos.forEach(archivoAdjunto => {

        if (!tareaEditarViewModel.archivosAdjuntos().find(archivo => archivo.id === archivoAdjunto.id)) {
            let fechaCreacion = archivoAdjunto.fechaCreacion;

            if (archivoAdjunto.fechaCreacion.indexOf('Z') === -1) {
                fechaCreacion += 'Z';
            }

            const fechaCreacionDT = new Date(fechaCreacion);
            archivoAdjunto.publicado = fechaCreacionDT.toLocaleString();

            tareaEditarViewModel.archivosAdjuntos.push(
                new archivoAdjuntoViewModel({ ...archivoAdjunto, modoEdicion: false }));
        }

    });
}

let tituloArchivoAdjuntoAnterior;
function manejarClickTituloArchivoAdjunto(archivoAdjunto) {
    archivoAdjunto.modoEdicion(true);
    tituloArchivoAdjuntoAnterior = archivoAdjunto.titulo();
    $("[name='txtArchivoAdjuntoTitulo']:visible").focus();
}

async function manejarFocusoutTituloArchivoAdjunto(archivoAdjunto) {
    archivoAdjunto.modoEdicion(false);
    const idTarea = archivoAdjunto.id;

    if (!archivoAdjunto.titulo()) {
        archivoAdjunto.titulo(tituloArchivoAdjuntoAnterior);
    }

    if (archivoAdjunto.titulo() === tituloArchivoAdjuntoAnterior) {
        return;
    }

    const data = JSON.stringify(archivoAdjunto.titulo());

    const respuesta = await fetch(`${urlArchivos}/${idTarea}`, {
        method: 'PUT',
        body: data,
        headers: { 'Content-Type': 'application/json' }
    });

    if (!respuesta.ok) {
        await manejarErrorApi(respuesta);
    }
}

function manejarClickBorrarArchivoAdjunto(archivoAdjunto) {
    modalEditarTareaBootstrap.hide();

    confirmarAccion({
        callbackAceptar: () => {
            borrarArchivoAdjunto(archivoAdjunto);
            modalEditarTareaBootstrap.show();
        },
        callbackCancelar: () => {
            modalEditarTareaBootstrap.show();
        },
        titulo: '¿Desea eliminar este archivo adjunto?'
    });
}

async function borrarArchivoAdjunto(archivoAdjunto) {
    const respuesta = await fetch(`${urlArchivos}/${archivoAdjunto.id}`, {
        method: 'DELETE'
    });

    if (!respuesta.ok) {
        await manejarErrorApi(respuesta);
        return;
    }

    tareaEditarViewModel.archivosAdjuntos.remove(function (item) {
        return item.id == archivoAdjunto.id
    });
}

function manejarClickDescargarArchivoAdjunto(archivoAdjunto) {
    descargarArchivo(archivoAdjunto.url, archivoAdjunto.titulo());
}

async function actualizarOrdenArchivos() {
    const ids = obtenerIdsArchivos();
    await enviarIdsArchivosAlBackend(ids);

    const arregloOrganizado = tareaEditarViewModel.archivosAdjuntos.sorted(function (a, b) {
        return ids.indexOf(a.id.toString()) - ids.indexOf(b.id.toString());
    });

    tareaEditarViewModel.archivosAdjuntos([]);
    tareaEditarViewModel.archivosAdjuntos(arregloOrganizado);
}

function obtenerIdsArchivos() {
    const ids = $("[name=txtArchivoAdjuntoTitulo]").map(function () {
        return $(this).attr('data-id')
    }).get();
    return ids;
}

async function enviarIdsArchivosAlBackend(ids) {
    var data = JSON.stringify(ids);
    const respuesta = await fetch(`${urlArchivos}/ordenar/${tareaEditarViewModel.id}`, {
        method: 'POST',
        body: data,
        headers: { 'Content-Type': 'application/json' }
    });

    if (!respuesta.ok) {
        await manejarErrorApi(respuesta);
    }
}

$(function () {
    $('#reordenable-adjuntos').sortable({
        axis: 'y',
        stop: async function () {
            await actualizarOrdenArchivos();
        }
    });
});