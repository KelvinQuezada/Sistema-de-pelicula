# Guía para el informe PDF y borrador de las preguntas de reflexión

Complete con sus datos y capturas reales. Cada figura: número, título, imagen legible y una descripción.

## Capturas necesarias (en este orden)
1. Diagrama de la base de datos (SSMS → Diagramas) con film, inventory, rental, customer y staff.
2. Las 3 consultas SQL (1, 3 y 7 en `Database/02_consultas_informe.sql`) con su resultado y una explicación.
3. Login; navegación como admin y como cliente (dos capturas).
4. Cliente escribiendo `/Peliculas/Crear` → pantalla "Acceso denegado".
5. Catálogo con disponibilidad; detalle; alquiler; Mis alquileres; devolución.
6. Intento de alquilar una película sin copias (mensaje de error).
7. Reporte; CRUD admin; validaciones (formulario con errores); eliminación lógica (película en SSMS con `eliminado = 1`).
8. Tabla `usuarios` en SSMS mostrando `password_hash`.
9. Fragmentos de código: `AutorizarAttribute`, `SessionExtensions`, `AlquilerRepository.Alquilar`, `ProtectorContrasena`.

## Reto 1: contraseña protegida
Se usa PBKDF2 (SHA-256, 100 000 iteraciones) con sal aleatoria de 16 bytes. Se guarda `iteraciones.sal.hash`. En el login se busca el usuario por nombre, se recalcula el hash con la sal almacenada y se compara en tiempo constante (`FixedTimeEquals`). Nunca se guarda ni se compara la contraseña original.

## Reto 2: eliminación lógica
Se aplica en `film` e `inventory` mediante la columna `eliminado`. Se justifica porque las películas y copias tienen alquileres históricos: borrarlas físicamente rompería las llaves foráneas y perdería el historial de los clientes.

## Respuestas (borrador, adáptelas con sus palabras)
1. **Autenticación** verifica quién es el usuario (login con hash). **Session** guarda en el servidor su identidad y rol (`UsuarioSesion`) mientras navega. **Autorización** decide, en cada petición, si ese rol puede ejecutar la acción: el filtro `[Autorizar]` lee la sesión; sin sesión redirige a Login y con rol no permitido a Acceso denegado, aun si se escribe la URL directamente.
2. Un campo `Stock` en `film` no distingue copias individuales ni dónde están, y se desincroniza con facilidad. En Sakila `film` describe la película, `inventory` cada copia física y `rental` si una copia está prestada (`return_date IS NULL`). La disponibilidad se calcula a partir de esas tablas.
3. El `customer_id` se toma de `UsuarioSesion.CustomerId`, almacenado en el servidor al iniciar sesión. Permitir elegirlo en el formulario dejaría a cualquiera alquilar o devolver a nombre de otro cliente modificando la petición.
4. `Alquilar` busca una copia activa sin alquiler pendiente (`NOT EXISTS` sobre `rental` con `return_date IS NULL`) dentro de una transacción serializable con `UPDLOCK`. Si no hay ninguna, hace rollback y muestra "No hay copias disponibles", sin insertar nada.
5. Conserva el historial y la integridad referencial, y permite recuperar registros. Se implementó con `eliminado BIT`; las consultas del catálogo, detalle, edición, disponibilidad y reporte filtran `eliminado = 0`, y la eliminación se bloquea si hay copias alquiladas.
