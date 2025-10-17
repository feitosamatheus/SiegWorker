using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sieg.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Infrastructure.Fluents;

public class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.NomeOriginalArquivo).IsRequired().HasMaxLength(255);
        builder.Property(d => d.CaminhoXml).IsRequired().HasMaxLength(500);
        builder.Property(d => d.Tamanho).IsRequired();
        builder.Property(d => d.Processado).IsRequired().HasDefaultValue(false);
        builder.HasMany(d => d.DocumentosFiscais).WithOne(df => df.Documento).HasForeignKey(df => df.DocumentoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(d => d.NomeOriginalArquivo).HasDatabaseName("IX_Documento_NomeOriginalArquivo");

        builder.HasQueryFilter(df => !df.Excluido);
    }
}