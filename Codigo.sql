USE RockDeveloper;
GO

CREATE TABLE usuario (
	id INT IDENTITY(1,1) PRIMARY KEY,
	cedula VARCHAR(50),
	nombres VARCHAR(50),
	apellidos VARCHAR(50),
	clave VARCHAR(50)
);
GO

CREATE TABLE proveedor (
	id INT IDENTITY(1,1) PRIMARY KEY,
	nombre VARCHAR(50)
);
GO

CREATE TABLE producto (
	id INT IDENTITY(1,1) PRIMARY KEY,
	codigo VARCHAR(20),
	nombre VARCHAR(50),
	precio DECIMAL(10,2),
	proveedor INT,
	CONSTRAINT FK_producto_proveedor 
	FOREIGN KEY (proveedor) REFERENCES proveedor(id)
);
GO
INSERT INTO usuario (cedula, nombres, apellidos, clave)
VALUES ('0911222334', 'Andrea', 'Castillo', '123456');

INSERT INTO usuario (cedula, nombres, apellidos, clave)
VALUES ('0911222311', 'Luis', 'Morejon', '123456');
GO
SELECT * FROM usuario;
GO

INSERT INTO proveedor (nombre) VALUES ('La Favorita');
GO
INSERT INTO proveedor (nombre) VALUES ('GMO');
GO

INSERT INTO producto (codigo, nombre, precio, proveedor) 
VALUES ('SA100', 'Aceite 1lt', 3.45, 1);
GO
INSERT INTO producto (codigo, nombre, precio, proveedor) 
VALUES ('38105', 'Lentes bifocales', 48.75, 2);
GO
SELECT 
    pr.codigo,
    pr.nombre,
    pr.precio,
    po.nombre AS proveedor
FROM producto pr
INNER JOIN proveedor po ON po.id = pr.proveedor;
Go
GO
CREATE OR ALTER PROCEDURE GetUsuario
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10);
    DECLARE @Leyenda VARCHAR(50);
    DECLARE @cedulaT VARCHAR(50);
    DECLARE @claveT VARCHAR(50);

    BEGIN TRY

        -- CONSULTAR TODOS LOS USUARIOS
        IF (@iTransaccion = 'CONSULTAR_USUARIO')
        BEGIN
            SELECT * FROM usuario;

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Consulta Exitosa';
        END

        -- VALIDAR USUARIO
        ELSE IF (@iTransaccion = 'VALIDAR_USUARIO')
        BEGIN
            SET @cedulaT = @iXML.value('(/Usuario/Cedula)[1]', 'VARCHAR(50)');
            SET @claveT  = @iXML.value('(/Usuario/Clave)[1]', 'VARCHAR(50)');

            SELECT *
            FROM usuario
            WHERE cedula = @cedulaT
              AND clave  = @claveT;

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Validación Exitosa';
        END

    END TRY
    BEGIN CATCH
        SET @Respuesta = 'Error';
        SET @Leyenda = 'Error al ejecutar el comando en la BD: ' + ERROR_MESSAGE();
    END CATCH

    SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
END;
GO
GO
CREATE OR ALTER PROCEDURE SetUsuario
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10);
    DECLARE @Leyenda VARCHAR(100);
    DECLARE @cedula VARCHAR(50);
    DECLARE @nombres VARCHAR(50);
    DECLARE @apellidos VARCHAR(50);
    DECLARE @clave VARCHAR(50);

    BEGIN TRY
        BEGIN TRANSACTION TRX_DATOS;

        -- INSERTAR USUARIO
        IF (@iTransaccion = 'INSERTAR_USUARIO')
        BEGIN
            SET @cedula    = LTRIM(RTRIM(@iXML.value('(/Usuario/Cedula)[1]', 'VARCHAR(50)')));
            SET @nombres   = LTRIM(RTRIM(@iXML.value('(/Usuario/Nombres)[1]', 'VARCHAR(50)')));
            SET @apellidos = LTRIM(RTRIM(@iXML.value('(/Usuario/Apellidos)[1]', 'VARCHAR(50)')));
            SET @clave     = LTRIM(RTRIM(@iXML.value('(/Usuario/Clave)[1]', 'VARCHAR(50)')));

            INSERT INTO usuario (cedula, nombres, apellidos, clave)
            VALUES (@cedula, @nombres, @apellidos, @clave);

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Usuario insertado correctamente. Cédula: ' + @cedula;
        END

        -- ELIMINAR USUARIO
        ELSE IF (@iTransaccion = 'ELIMINAR_USUARIO')
        BEGIN
            SET @cedula = LTRIM(RTRIM(@iXML.value('(/Usuario/Cedula)[1]', 'VARCHAR(50)')));

            DELETE FROM usuario WHERE cedula = @cedula;

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Usuario eliminado correctamente. Cédula: ' + @cedula;
        END

        -- ACTUALIZAR CLAVE
        ELSE IF (@iTransaccion = 'ACTUALIZAR_CLAVE')
        BEGIN
            SET @cedula = LTRIM(RTRIM(@iXML.value('(/Usuario/Cedula)[1]', 'VARCHAR(50)')));
            SET @clave  = LTRIM(RTRIM(@iXML.value('(/Usuario/Clave)[1]', 'VARCHAR(50)')));

            UPDATE usuario
            SET clave = @clave
            WHERE cedula = @cedula;

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Clave actualizada correctamente. Cédula: ' + @cedula;
        END

        COMMIT TRANSACTION TRX_DATOS;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION TRX_DATOS;

        SET @Respuesta = 'Error';
        SET @Leyenda = 'Inconvenientes en la transacción: ' 
                        + @iTransaccion + ' - ' + ERROR_MESSAGE();
    END CATCH

    SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
END;
GO





GO
CREATE OR ALTER PROCEDURE GetProducto
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10);
    DECLARE @Leyenda VARCHAR(50);
    DECLARE @codigoT VARCHAR(20);

    BEGIN TRY

        -- CONSULTAR TODOS LOS PRODUCTOS
        IF (@iTransaccion = 'CONSULTAR_PRODUCTO')
        BEGIN
            SELECT 
                p.id,
                p.codigo,
                p.nombre,
                p.precio,
                p.proveedor,
                pr.nombre AS nombreProveedor
            FROM producto p
            INNER JOIN proveedor pr ON pr.id = p.proveedor;

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Consulta Exitosa';
        END

        -- BUSCAR PRODUCTO POR CÓDIGO
        ELSE IF (@iTransaccion = 'BUSCAR_PRODUCTO')
        BEGIN
            SET @codigoT = @iXML.value('(/Producto/Codigo)[1]', 'VARCHAR(20)');

            SELECT 
                p.id,
                p.codigo,
                p.nombre,
                p.precio,
                p.proveedor,
                pr.nombre AS nombreProveedor
            FROM producto p
            INNER JOIN proveedor pr ON pr.id = p.proveedor
            WHERE p.codigo = @codigoT;

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Consulta Exitosa';
        END

    END TRY
    BEGIN CATCH
        SET @Respuesta = 'Error';
        SET @Leyenda = 'Error al ejecutar el comando en la BD: ' + ERROR_MESSAGE();
    END CATCH

    SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
END;
GO

-- PROCEDIMIENTO SetProducto
CREATE OR ALTER PROCEDURE SetProducto
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10);
    DECLARE @Leyenda VARCHAR(100);
    DECLARE @codigo VARCHAR(20);
    DECLARE @nombre VARCHAR(50);
    DECLARE @precio DECIMAL(10,2);
    DECLARE @proveedorId INT;

    BEGIN TRY
        BEGIN TRANSACTION TRX_DATOS;

        -- INSERTAR PRODUCTO
        IF (@iTransaccion = 'INSERTAR_PRODUCTO')
        BEGIN
            SET @codigo = LTRIM(RTRIM(@iXML.value('(/Producto/Codigo)[1]', 'VARCHAR(20)')));
            SET @nombre = LTRIM(RTRIM(@iXML.value('(/Producto/Nombre)[1]', 'VARCHAR(50)')));
            SET @precio = @iXML.value('(/Producto/Precio)[1]', 'DECIMAL(10,2)');
            SET @proveedorId = @iXML.value('(/Producto/Proveedor/Id)[1]', 'INT');

            INSERT INTO producto (codigo, nombre, precio, proveedor)
            VALUES (@codigo, @nombre, @precio, @proveedorId);

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Producto insertado correctamente. Código: ' + @codigo;
        END

        -- ACTUALIZAR PRODUCTO
        ELSE IF (@iTransaccion = 'ACTUALIZAR_PRODUCTO')
        BEGIN
            SET @codigo = LTRIM(RTRIM(@iXML.value('(/Producto/Codigo)[1]', 'VARCHAR(20)')));
            SET @nombre = LTRIM(RTRIM(@iXML.value('(/Producto/Nombre)[1]', 'VARCHAR(50)')));
            SET @precio = @iXML.value('(/Producto/Precio)[1]', 'DECIMAL(10,2)');
            SET @proveedorId = @iXML.value('(/Producto/Proveedor/Id)[1]', 'INT');

            UPDATE producto
            SET nombre = @nombre,
                precio = @precio,
                proveedor = @proveedorId
            WHERE codigo = @codigo;

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Producto actualizado correctamente. Código: ' + @codigo;
        END

        -- ELIMINAR PRODUCTO
        ELSE IF (@iTransaccion = 'ELIMINAR_PRODUCTO')
        BEGIN
            SET @codigo = LTRIM(RTRIM(@iXML.value('(/Producto/Codigo)[1]', 'VARCHAR(20)')));

            DELETE FROM producto WHERE codigo = @codigo;

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Producto eliminado correctamente. Código: ' + @codigo;
        END

        COMMIT TRANSACTION TRX_DATOS;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION TRX_DATOS;

        SET @Respuesta = 'Error';
        SET @Leyenda = 'Inconvenientes en la transacción: ' 
                        + @iTransaccion + ' - ' + ERROR_MESSAGE();
    END CATCH

    SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
END;
GO
SELECT * FROM usuario;
SELECT * FROM producto;
SELECT * FROM proveedor;
GO
