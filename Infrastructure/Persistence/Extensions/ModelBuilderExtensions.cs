using Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infrastructure.Persistence.Extensions;

public static class ModelBuilderExtensions
{
    public static void SnakeCaseModels(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            // https://github.com/dotnet/efcore/issues/18006
            if (entity.BaseType == null) entity.SetTableName(entity.DisplayName().ToSnakeCase());

            // Replace column names            
            foreach (var property in entity.GetProperties())
                property.SetColumnName(
                    property
                        .GetColumnName(
                            StoreObjectIdentifier.Table(entity.GetTableName())
                        )
                        .ToSnakeCase()
                );

            foreach (var key in entity.GetKeys()) key.SetName(key.GetName().ToSnakeCase());

            foreach (var key in entity.GetForeignKeys())
                key.SetConstraintName(key.GetConstraintName().ToSnakeCase());

            foreach (var index in entity.GetIndexes()) index.SetDatabaseName(index.GetDatabaseName().ToSnakeCase());
        }
    }
}