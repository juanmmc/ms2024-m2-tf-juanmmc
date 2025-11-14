[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/rVhxZd2x)

# Microservicio "Logística y Entregas"

Microservicio para gestionar operaciones de reparto (entregas) de paquetes alimenticios. Modela el ciclo de vida de un paquete, la actividad de repartidores y su planificación de rutas.

## Funcionalidades principales:

### Gestión de repartidores (Drivers)
- Crear un repartidor.
- Registrar ubicación del repartidor.
- Listar repartidores.
- Obtener repartidor.

### Gestión de paquetes (Package)
- Crear paquete con información del paciente y dirección de entrega.

### Gestión de repartidores (Delivery)
- Cambiar estado: marcar en tránsito (si tiene orden asignado), entregado (con evidencia), fallido, cancelar.
- Registrar incidentes de entrega (solo si el paquete está en estado Failed).
- Registrar orden para planificación de entrega.
- Listar entregas asignadas a un repartidor por fecha y ordenados según el orden definido.

### Coordinación entre agregados
- Emisión de eventos de dominio (ej. paquete creado) y handlers que realizan asignaciones en el agregado Delivery (desacoplamiento entre agregados).

## Ejecución del entorno con Docker Compose

Para levantar el entorno de trabajo completo:

```bash
docker compose --project-name logisticsanddeliveries_webapi up -d
```

Para detener el entorno:

```bash
docker compose --project-name logisticsanddeliveries_webapi down
```

# Diagrama de Clases

![Class diagram](./img/class_diagram.png)