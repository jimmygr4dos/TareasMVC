function agregarNuevaTarea() {
    tareaListadoViewModel.tareas.push(new tareaElementoListadoViewModel({ id: 0, titulo: '' }));

    $("[name=titulo-tarea]").last().focus();
}

async function manejarFocusout(tarea) {
    const titulo = tarea.titulo();
    if (!titulo) {
        tareaListadoViewModel.tareas.pop();
        return;
    }

    //tarea.id(1);
    const data = JSON.stringify(titulo);
    const respuesta = await fetch(urlTareas, {
        method: 'POST',
        body: data,
        headers: { 'Content-Type': 'application/json' }
    });
    if (respuesta.ok) {
        const json = await respuesta.json();
        tarea.id(json.id);
    } else {
        manejarErrorApi(respuesta);
    }
}

async function obtenerTareas() {
    tareaListadoViewModel.cargando(true);
    const respuesta = await fetch(urlTareas, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    });
    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        return;
    }
    const json = await respuesta.json();
    tareaListadoViewModel.tareas([]);
    json.forEach(tarea => {
        tareaListadoViewModel.tareas.push(new tareaElementoListadoViewModel(tarea));
    })
    tareaListadoViewModel.cargando(false);
}

async function actualizarOrdenTareas() {
    const ids = obtenerIdsTareas();
    await enviarIdsTareasAlBackend(ids);

    const arregloOrdenado = tareaListadoViewModel.tareas.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    });

    tareaListadoViewModel.tareas([]);
    tareaListadoViewModel.tareas(arregloOrdenado);
}

function obtenerIdsTareas() {
    const ids = $("[name=titulo-tarea]").map(function () {
        return $(this).attr("data-id");
    }).get();
    return ids;
}

async function enviarIdsTareasAlBackend(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${urlTareas}/ordenar`, {
        method: 'POST',
        body: data,
        headers: { 'Content-Type': 'application/json' }
    });
}

$(function () {
    $("#reordenable").sortable({
        axis: 'y',
        stop: async function () {
            await actualizarOrdenTareas();
        }
    });
});