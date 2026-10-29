# Refactor: Cajero cliente "tonto" + servidor central

## Qué cambió

**Cliente (`CajeroAutomatico`, WinForms)**
- Se eliminaron por completo los forms, lógica y vistas de administrador:
  `FormMenuAdmin`, `FormControlUsuarios`, `FormCrearUsuario`, `FormModificarTarjeta`,
  `FormModificarLimite`, `FormConsultaUsuario`, `FormInicializarCajero`,
  `FormAgregarEfectivo`, `FormModificarHuella`.
- Se eliminó todo acceso local a datos: `Datos/RepositorioArchivos.cs` (CSV) y
  `Negocio/CajeroService.cs` (reglas de negocio) ya no existen en el cliente.
- `Entidades/Usuario.cs` quedó reducido a un DTO de pantalla: solo
  `NumeroTarjeta`, `Nombre`, `Bloqueado`, `RequiereHuella`. Ya no guarda PIN,
  saldo, límite, huella ni historial — esos datos nunca tocan el cliente,
  se piden al servidor cada vez que una pantalla los necesita.
- Nuevo namespace `Comunicacion/`:
  - `Protocolo.cs`: define `SolicitudRed` / `RespuestaRed` y las operaciones.
  - `ServidorClient.cs`: único punto de entrada a la red. Abre un `TcpClient`,
    manda un JSON, lee la respuesta. Los Forms solo llaman a `Program.Servidor.*`.
- `Program.cs` ya no crea un `CajeroService`; crea un `ServidorClient` (red) y,
  por separado, el `ArduinoService` (lector físico, que solo captura datos de
  hardware — no decide nada ni guarda estado de negocio).

**Servidor (`ServidorCajero`, consola, nuevo)**
- Implementa el mismo protocolo y concentra TODA la lógica que antes estaba en
  `CajeroService`: validar tarjeta/PIN/huella, saldo, límite diario, desglose
  de billetes, transacciones, cambio de PIN, bloqueo por intentos fallidos.
- Es una referencia funcional y deliberadamente simple (datos en memoria) para
  que puedas correr el flujo completo ya mismo y después reemplazar el
  almacenamiento por una base de datos real sin tocar el protocolo.
- Usuario de prueba: tarjeta `1234567890123456`, PIN `1234`.

## Protocolo

Transporte: **TCP** (`TcpClient`/`TcpListener`), un mensaje JSON por línea
(`\n`-delimited). El cliente abre conexión → envía una `Solicitud` → espera
una `Respuesta` → cierra. Una conexión nueva por operación (simple y robusto
para un cajero que hace una operación a la vez).

```json
// Cliente -> Servidor
{ "Operacion": "VALIDAR_PIN", "Datos": { "NumeroTarjeta": "123...", "Pin": "1234" } }

// Servidor -> Cliente
{ "Exito": true, "Mensaje": "Ingreso correcto.", "Datos": { "Token": "abc123..." } }
```

Operaciones (`Comunicacion/Protocolo.cs` → `Operaciones`):

| Operación | Datos que manda el cliente | Datos que devuelve el servidor |
|---|---|---|
| `VALIDAR_TARJETA` | `NumeroTarjeta` | `Nombre`, `Bloqueado`, `RequiereHuella` |
| `VALIDAR_PIN` | `NumeroTarjeta`, `Pin` | `Token` (si Exito) / `IntentosRestantes`, `Bloqueado` |
| `VALIDAR_HUELLA` | `NumeroTarjeta`, `Huella` | igual que `VALIDAR_PIN` |
| `CONSULTAR_SALDO` | `Token` | `SaldoActual`, `LimiteMaximo`, `DisponibleHoy` |
| `RETIRAR` | `Token`, `Monto`, `Desglose` | `SaldoActual` |
| `DEPOSITAR` | `Token`, `Desglose` | `SaldoActual` |
| `CAMBIAR_PIN` | `Token`, `PinActual`, `PinNuevo`, `ConfirmacionPinNuevo` | — |
| `ULTIMAS_TRANSACCIONES` | `Token`, `Cantidad` | `Transacciones: [...]` |
| `ESTADO_CAJERO` | — | `Estado` ("Activo"/"Inactivo") |
| `CERRAR_SESION` | `Token` | — |

`Token` es un identificador de sesión que el servidor entrega al validar PIN
o huella; el cliente lo guarda en memoria (`ServidorClient.Token`) y lo manda
en cada operación siguiente. El servidor es la única autoridad: decide si una
tarjeta existe, si el PIN es correcto, si hay saldo, si hay billetes
suficientes, etc. El cliente arma la solicitud y muestra la respuesta, nunca
decide por su cuenta.

## Cómo probarlo

1. Abre `ServidorCajero.sln`/proyecto y ejecútalo (consola). Escucha en el
   puerto `9000` (configurable, ver abajo).
2. Abre `CajeroAutomatico` y ejecútalo. En el login usa la tarjeta
   `1234567890123456` y PIN `1234`.
3. La IP/puerto del servidor se configuran en
   `CajeroAutomatico/App.config` → `appSettings` (`ServidorIP`, `ServidorPuerto`,
   `ServidorTimeoutMs`). Si el cliente corre en otra máquina, cambia
   `ServidorIP` a la IP del servidor.

Ya probé el protocolo de punta a punta (socket real, ida y vuelta) contra el
servidor de referencia: validar tarjeta, PIN correcto/incorrecto, consultar
saldo, retirar, depositar, últimas transacciones, cambiar PIN, estado del
cajero y cerrar sesión. Todo funcionando.

## Siguientes pasos sugeridos

- Cambiar el almacenamiento en memoria de `ServidorCajero` por una base de
  datos (el protocolo no cambia, solo la implementación de cada operación).
- Agregar TLS si el cajero y el servidor no están en una red físicamente
  segura (hoy el socket va en texto plano).
- Mover la administración (crear usuarios, inicializar cajero, agregar
  efectivo, bloquear/desbloquear, reportes) a una aplicación de servidor
  separada que hable directo con la base de datos — ya no al cliente ATM.
- Si quieres reautenticación más estricta, puedes expirar el `Token` del
  lado del servidor después de cierto tiempo de inactividad.
