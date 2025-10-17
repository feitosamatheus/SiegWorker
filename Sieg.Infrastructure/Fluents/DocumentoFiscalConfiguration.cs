using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sieg.Domain.Entities;
using Sieg.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Infrastructure.Fluents;

public class DocumentoFiscalConfiguration : IEntityTypeConfiguration<DocumentoFiscal>
{
    public void Configure(EntityTypeBuilder<DocumentoFiscal> builder)
    {
        builder.HasKey(df => df.Id);

        builder.Property(df => df.CnpjEmitente)
           .HasConversion(cnpj => cnpj.Value, v => new Cnpj(v))
           .HasColumnName("CnpjEmitente")
           .HasColumnType("varchar(14)")
           .IsUnicode(false)
           .IsRequired();

        builder.Property(df => df.UfEmitente)
            .HasConversion(uf => uf.Value, v => new Uf(v))
            .HasColumnName("UfEmitente")
            .HasColumnType("char(2)")
            .IsUnicode(false)
            .IsRequired();

        builder.Property(df => df.TipoDocumento).IsRequired();
        builder.Property(df => df.DataEmissao).IsRequired();
        builder.Property(df => df.ValorTotal).IsRequired().HasPrecision(18, 2);
        builder.HasIndex(df => new { df.CnpjEmitente, df.DataEmissao }).HasDatabaseName("IX_DocFiscal_Cnpj_DataEmissao");
        builder.HasIndex(df => new { df.UfEmitente, df.DataEmissao }).HasDatabaseName("IX_DocFiscal_Uf_DataEmissao");
        builder.HasOne(df => df.Documento).WithMany(d => d.DocumentosFiscais).HasForeignKey(df => df.DocumentoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(df => df.DataEmissao).HasDatabaseName("IX_DocFiscal_DataEmissao");

        builder.HasQueryFilter(df => !df.Excluido);
    }
}
