# Documentación Técnica de Migración

## Migración: CreateInventarioVentasTables

**Fecha de creación:** 2024-12-04  
**Versión de Entity Framework:** 8.0.19  
**Plataforma:** .NET 8  
**Proyecto:** PRUEBATECNICA.STEFFANINIGROUP.INVENTARIO

---

## ?? Descripción General

Esta migración crea las tablas necesarias para el sistema de inventario y ventas, dividido en dos APIs principales:

| API | Tablas | Descripción |
|-----|--------|-------------|
| **API-PRODUCTOS** | `Productos` | Gestión del inventario de productos |
| **API-VENTAS** | `EncabezadoVentas`, `DetalleVentas` | Gestión de ventas con encabezado y detalle |

---

## ??? Estructura de Tablas

### 1. Tabla: `Productos`

**Descripción:** Almacena el catálogo de productos disponibles en el inventario.

| Campo | Tipo SQL | Tipo .NET | Nullable | Descripción |
|-------|----------|-----------|----------|-------------|
| `idpro` | `int IDENTITY(1,1)` | `int` | NO | Clave primaria, autoincremental |
| `producto` | `nvarchar(200)` | `string` | NO | Nombre del producto |
| `precio` | `decimal(18,2)` | `decimal` | NO | Precio unitario del producto |

**Constraints:**
- `PK_Productos` - Clave primaria en `idpro`

**Modelo C#:**
```csharp
[Table("Productos")]
public class Producto
{
    [Key]
    [Column("idpro")]
    public int IdPro { get; set; }

    [Required]
    [StringLength(200)]
    [Column("producto")]
    public string ProductoNombre { get; set; }

    [Required]
    [Column("precio", TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    public virtual ICollection<DetalleVenta> DetallesVenta { get; set; }
}
```

---

### 2. Tabla: `EncabezadoVentas`

**Descripción:** Almacena la información general de cada venta realizada.

| Campo | Tipo SQL | Tipo .NET | Nullable | Descripción |
|-------|----------|-----------|----------|-------------|
| `idventa` | `int IDENTITY(1,1)` | `int` | NO | Clave primaria, autoincremental |
| `fecha` | `datetime2` | `DateTime` | NO | Fecha de la venta |
| `vendedor` | `nvarchar(150)` | `string` | NO | Nombre del vendedor |
| `total` | `decimal(18,2)` | `decimal` | NO | Total de la venta |

**Constraints:**
- `PK_EncabezadoVentas` - Clave primaria en `idventa`

**Modelo C#:**
```csharp
[Table("EncabezadoVentas")]
public class EncabezadoVenta
{
    [Key]
    [Column("idventa")]
    public int IdVenta { get; set; }

    [Required]
    [Column("fecha")]
    public DateTime Fecha { get; set; }

    [Required]
    [StringLength(150)]
    [Column("vendedor")]
    public string Vendedor { get; set; }

    [Required]
    [Column("total", TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    public virtual ICollection<DetalleVenta> DetallesVenta { get; set; }
}
```

---

### 3. Tabla: `DetalleVentas`

**Descripción:** Almacena el detalle de cada línea de producto vendido en una venta.

| Campo | Tipo SQL | Tipo .NET | Nullable | Descripción |
|-------|----------|-----------|----------|-------------|
| `idde` | `int IDENTITY(1,1)` | `int` | NO | Clave primaria, autoincremental |
| `fecha` | `datetime2` | `DateTime` | NO | Fecha del detalle |
| `idventa` | `int` | `int` | NO | FK a EncabezadoVentas |
| `idpro` | `int` | `int` | NO | FK a Productos |
| `cantidad` | `int` | `int` | NO | Cantidad vendida |
| `precio` | `decimal(18,2)` | `decimal` | NO | Precio unitario al momento de la venta |
| `iva` | `decimal(18,2)` | `decimal` | NO | Impuesto IVA aplicado |
| `total` | `decimal(18,2)` | `decimal` | NO | Total de la línea (cantidad × precio + iva) |

**Constraints:**
- `PK_DetalleVentas` - Clave primaria en `idde`
- `FK_DetalleVentas_EncabezadoVentas_idventa` - Foreign key a `EncabezadoVentas(idventa)` con `CASCADE DELETE`
- `FK_DetalleVentas_Productos_idpro` - Foreign key a `Productos(idpro)` con `NO ACTION` (Restrict)

**Índices:**
- `IX_DetalleVentas_idpro` - Índice en columna `idpro`
- `IX_DetalleVentas_idventa` - Índice en columna `idventa`

**Modelo C#:**
```csharp
[Table("DetalleVentas")]
public class DetalleVenta
{
    [Key]
    [Column("idde")]
    public int IdDe { get; set; }

    [Required]
    [Column("fecha")]
    public DateTime Fecha { get; set; }

    [Required]
    [Column("idventa")]
    public int IdVenta { get; set; }

    [Required]
    [Column("idpro")]
    public int IdPro { get; set; }

    [Required]
    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Required]
    [Column("precio", TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    [Required]
    [Column("iva", TypeName = "decimal(18,2)")]
    public decimal Iva { get; set; }

    [Required]
    [Column("total", TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [ForeignKey(nameof(IdVenta))]
    public virtual EncabezadoVenta EncabezadoVenta { get; set; }

    [ForeignKey(nameof(IdPro))]
    public virtual Producto Producto { get; set; }
}
```

---

## ?? Diagrama de Relaciones (ERD)

```
???????????????????       ???????????????????????       ???????????????????
?    Productos    ?       ?    DetalleVentas    ?       ? EncabezadoVentas?
???????????????????       ???????????????????????       ???????????????????
? PK idpro        ????????? FK idpro            ?       ? PK idventa      ?
?    producto     ?   1:N ? FK idventa          ?????????    fecha        ?
?    precio       ?       ? PK idde             ?  N:1  ?    vendedor     ?
???????????????????       ?    fecha            ?       ?    total        ?
                          ?    cantidad         ?       ???????????????????
                          ?    precio           ?
                          ?    iva              ?
                          ?    total            ?
                          ???????????????????????
```

### Relaciones:

| Relación | Tipo | Comportamiento Delete |
|----------|------|----------------------|
| `EncabezadoVentas` ? `DetalleVentas` | 1:N | CASCADE (eliminar encabezado elimina detalles) |
| `Productos` ? `DetalleVentas` | 1:N | RESTRICT (no permite eliminar producto con ventas) |

---

## ?? Archivos de la Migración

| Archivo | Ubicación | Descripción |
|---------|-----------|-------------|
| Migración | `Data/Migrations/20251204042323_CreateInventarioVentasTables.cs` | Código de migración Up/Down |
| Designer | `Data/Migrations/20251204042323_CreateInventarioVentasTables.Designer.cs` | Metadata de la migración |
| Snapshot | `Data/Migrations/ApplicationDbContextModelSnapshot.cs` | Estado actual del modelo |

---

## ?? Archivos de Modelos

| Archivo | Ubicación | Descripción |
|---------|-----------|-------------|
| Producto | `Models/Producto.cs` | Entidad de productos |
| EncabezadoVenta | `Models/EncabezadoVenta.cs` | Entidad de encabezado de ventas |
| DetalleVenta | `Models/DetalleVenta.cs` | Entidad de detalle de ventas |
| DbContext | `Data/ApplicationDbContext.cs` | Contexto de base de datos |

---

## ?? Comandos de Migración

### Aplicar migración:
```bash
dotnet ef database update
```

### Revertir migración:
```bash
dotnet ef database update 00000000000000_CreateIdentitySchema
```

### Generar script SQL:
```bash
dotnet ef migrations script 00000000000000_CreateIdentitySchema 20251204042323_CreateInventarioVentasTables -o script.sql
```

---

## ?? Script SQL Generado

```sql
-- Crear tabla Productos
CREATE TABLE [Productos] (
    [idpro] int NOT NULL IDENTITY,
    [producto] nvarchar(200) NOT NULL,
    [precio] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Productos] PRIMARY KEY ([idpro])
);

-- Crear tabla EncabezadoVentas
CREATE TABLE [EncabezadoVentas] (
    [idventa] int NOT NULL IDENTITY,
    [fecha] datetime2 NOT NULL,
    [vendedor] nvarchar(150) NOT NULL,
    [total] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_EncabezadoVentas] PRIMARY KEY ([idventa])
);

-- Crear tabla DetalleVentas
CREATE TABLE [DetalleVentas] (
    [idde] int NOT NULL IDENTITY,
    [fecha] datetime2 NOT NULL,
    [idventa] int NOT NULL,
    [idpro] int NOT NULL,
    [cantidad] int NOT NULL,
    [precio] decimal(18,2) NOT NULL,
    [iva] decimal(18,2) NOT NULL,
    [total] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_DetalleVentas] PRIMARY KEY ([idde]),
    CONSTRAINT [FK_DetalleVentas_EncabezadoVentas_idventa] 
        FOREIGN KEY ([idventa]) REFERENCES [EncabezadoVentas] ([idventa]) ON DELETE CASCADE,
    CONSTRAINT [FK_DetalleVentas_Productos_idpro] 
        FOREIGN KEY ([idpro]) REFERENCES [Productos] ([idpro]) ON DELETE NO ACTION
);

-- Crear índices
CREATE INDEX [IX_DetalleVentas_idpro] ON [DetalleVentas] ([idpro]);
CREATE INDEX [IX_DetalleVentas_idventa] ON [DetalleVentas] ([idventa]);

-- Registrar migración
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251204042323_CreateInventarioVentasTables', N'8.0.19');
```

---

## ?? Registro de Migración

La migración queda registrada en la tabla `__EFMigrationsHistory`:

| MigrationId | ProductVersion |
|-------------|----------------|
| 00000000000000_CreateIdentitySchema | 8.0.19 |
| 20251204042323_CreateInventarioVentasTables | 8.0.19 |

---

## ? Validación Post-Migración

Para verificar que la migración se aplicó correctamente, ejecutar:

```sql
-- Verificar tablas creadas
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Productos', 'EncabezadoVentas', 'DetalleVentas');

-- Verificar foreign keys
SELECT 
    fk.name AS FK_Name,
    tp.name AS Parent_Table,
    tr.name AS Referenced_Table
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
WHERE tp.name = 'DetalleVentas';
```

---

## ?? Autor

**Proyecto:** PRUEBATECNICA.STEFFANINIGROUP.INVENTARIO  
**Tipo:** Razor Pages - ASP.NET Core 8.0
