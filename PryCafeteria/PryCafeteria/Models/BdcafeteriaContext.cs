using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PryCafeteria.Models;

namespace PryCafeteria.Models;

public partial class BdcafeteriaContext 
    : IdentityDbContext<ApplicationUser>
{
    public BdcafeteriaContext()
    {
    }

    public BdcafeteriaContext(DbContextOptions<BdcafeteriaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Cupone> Cupones { get; set; }

    public virtual DbSet<DetallePedido> DetallePedidos { get; set; }

    public virtual DbSet<DireccionesEntrega> DireccionesEntregas { get; set; }

    public virtual DbSet<MetodosPago> MetodosPagos { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<ProductosTamanio> ProductosTamanios { get; set; }

    public virtual DbSet<Tamanio> Tamanios { get; set; }

    // HU-Stock E9: historial de movimientos de stock
    public virtual DbSet<StockMovimiento> StockMovimientos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("PK__Categori__F353C1E5EFC32C83");

            entity.HasIndex(e => e.NombreCategoria, "UQ__Categori__A21FBE9F50992B60").IsUnique();

            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Disponible).HasDefaultValue(true);

            // Criterio 6.0 — fecha de creación de categoría
            entity.Property(e => e.FechaCreacion)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Cupone>(entity =>
        {
            entity.HasKey(e => e.CuponId).HasName("PK__Cupones__C4356897BB81CEF4");

            entity.HasIndex(e => e.NombreCupon, "UQ__Cupones__DFFA581E70FE1535").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.NombreCupon)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TipoDescuento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ValorDescuento).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.HasKey(e => e.DetalleId).HasName("PK__DetalleP__6E19D6DA408176C5");

            entity.ToTable("DetallePedido");

            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.NombreProductoSnapshot).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.NombreTamanioSnapshot).HasMaxLength(30).IsUnicode(false);

            entity.HasOne(d => d.Pedido).WithMany(p => p.DetallePedidos)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallePedido_Pedidos");

            entity.HasOne(d => d.ProductoTamanio).WithMany(p => p.DetallePedidos)
                .HasForeignKey(d => d.ProductoTamanioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallePedido_ProductosTamanios");
        });

        modelBuilder.Entity<DireccionesEntrega>(entity =>
        {
            entity.HasKey(e => e.DireccionId).HasName("PK__Direccio__68906D64312744DC");

            entity.ToTable("DireccionesEntrega");

            entity.Property(e => e.Calle)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Distrito)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreDireccion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Numero)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Referencias)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("referencias");

            entity.HasOne(d => d.Usuario).WithMany(p => p.DireccionesEntregas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DireccionesEntrega_AspNetUsers");
        });

        modelBuilder.Entity<MetodosPago>(entity =>
        {
            entity.HasKey(e => e.MetodoPagoId).HasName("PK__MetodosP__A8FEAF5444056FA2");

            entity.ToTable("MetodosPago");

            entity.HasIndex(e => e.NombreMetodoPago, "UQ__MetodosP__F81E38EDF06F6AED").IsUnique();

            entity.Property(e => e.NombreMetodoPago)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Disponible).HasDefaultValue(true);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.PedidoId).HasName("PK__Pedidos__09BA1430FB8B5A3E");

            entity.Property(e => e.Descuento).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaPedido)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TipoEntrega)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Direccion).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.DireccionId)
                .HasConstraintName("FK_Pedidos_DireccionesEntrega");

            entity.HasOne(d => d.MetodoPago).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.MetodoPagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedidos_MetodosPago");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedidos_AspNetUsers");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.ProductoId).HasName("PK__Producto__A430AEA363C471C3");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Disponible).HasDefaultValue(true);
            entity.Property(e => e.Imagen)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreProducto)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Categoria).WithMany(p => p.Productos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Productos_Categorias");
        });

        modelBuilder.Entity<ProductosTamanio>(entity =>
        {
            entity.HasKey(e => e.ProductoTamanioId).HasName("PK__Producto__B64AF7683C2EBD99");

            entity.HasIndex(e => new { e.ProductoId, e.TamanioId }, "UQ_ProductosTamanios").IsUnique();

            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.Disponible).HasDefaultValue(true);

            entity.HasOne(d => d.Producto).WithMany(p => p.ProductosTamanios)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductosTamanios_Productos");

            entity.HasOne(d => d.Tamanio).WithMany(p => p.ProductosTamanios)
                .HasForeignKey(d => d.TamanioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductosTamanios_Tamanios");
        });

        modelBuilder.Entity<Tamanio>(entity =>
        {
            entity.HasKey(e => e.TamanioId).HasName("PK__Tamanios__3C536BF5A5D6817C");

            entity.HasIndex(e => e.NombreTamanio, "UQ__Tamanios__F822ECC2AE1E0233").IsUnique();

            entity.Property(e => e.NombreTamanio)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        // HU-Stock E9: tabla de auditoría de movimientos de stock
        modelBuilder.Entity<StockMovimiento>(entity =>
        {
            entity.HasKey(e => e.StockMovimientoId);

            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.UsuarioId)
                .HasMaxLength(450);

            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(e => e.ProductoTamanio)
                .WithMany()
                .HasForeignKey(e => e.ProductoTamanioId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_StockMovimientos_ProductosTamanios");

            entity.Property(e => e.Referencia)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_StockMovimientos_AspNetUsers");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
