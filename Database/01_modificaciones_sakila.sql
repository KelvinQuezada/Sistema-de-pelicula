-- =====================================================================
-- Modificaciones sobre Sakila para el sistema de renta de películas
-- Ejecutar en SQL Server Management Studio. Es seguro ejecutarlo más de una vez.
-- =====================================================================
USE sakila;
GO

-- Reto 2: eliminación lógica en film e inventory.
IF COL_LENGTH('dbo.film', 'eliminado') IS NULL
    ALTER TABLE dbo.film ADD eliminado BIT NOT NULL CONSTRAINT DF_film_eliminado DEFAULT 0;
GO
IF COL_LENGTH('dbo.inventory', 'eliminado') IS NULL
    ALTER TABLE dbo.inventory ADD eliminado BIT NOT NULL CONSTRAINT DF_inventory_eliminado DEFAULT 0;
GO

-- Tabla de usuarios para el login. La contraseña se guarda como hash PBKDF2 (nunca en texto plano).
-- customer_id es INT y staff_id es TINYINT, igual que en Sakila.
IF OBJECT_ID('dbo.usuarios', 'U') IS NULL
CREATE TABLE dbo.usuarios (
    usuario_id    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    username      NVARCHAR(50)  NOT NULL,
    password_hash NVARCHAR(200) NOT NULL,
    rol           NVARCHAR(20)  NOT NULL,
    customer_id   INT NULL,
    staff_id      TINYINT NULL,
    activo        BIT NOT NULL CONSTRAINT DF_usuarios_activo DEFAULT 1,
    fecha_registro DATETIME NOT NULL CONSTRAINT DF_usuarios_fecha DEFAULT GETDATE(),
    CONSTRAINT UQ_usuarios_username UNIQUE (username),
    CONSTRAINT CK_usuarios_rol CHECK (rol IN ('Administrador', 'Cliente')),
    CONSTRAINT CK_usuarios_vinculo CHECK ((rol = 'Cliente' AND customer_id IS NOT NULL)
                                       OR (rol = 'Administrador' AND staff_id IS NOT NULL)),
    CONSTRAINT FK_usuarios_customer FOREIGN KEY (customer_id) REFERENCES dbo.customer(customer_id),
    CONSTRAINT FK_usuarios_staff    FOREIGN KEY (staff_id)    REFERENCES dbo.staff(staff_id)
);
GO

-- Registro (bitácora) de inicios de sesión, exitosos y fallidos.
IF OBJECT_ID('dbo.login_log', 'U') IS NULL
CREATE TABLE dbo.login_log (
    log_id   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    username NVARCHAR(50) NOT NULL,
    exitoso  BIT NOT NULL,
    ip       NVARCHAR(45) NULL,
    fecha    DATETIME NOT NULL CONSTRAINT DF_login_log_fecha DEFAULT GETDATE()
);
GO

-- Dirección genérica para los clientes que se registran desde la aplicación
-- (customer.address_id es obligatorio en Sakila).
IF NOT EXISTS (SELECT 1 FROM dbo.address WHERE address = 'Direccion no registrada')
    INSERT INTO dbo.address (address, district, city_id, phone)
    VALUES ('Direccion no registrada', 'N/D', (SELECT MIN(city_id) FROM dbo.city), 'N/D');
GO

-- Los usuarios de demostración (admin, cliente1, cliente2) los crea la aplicación
-- al iniciar por primera vez, generando el hash de cada contraseña.
