using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Content;

namespace Merge.Infrastructure.Data.Configurations.Content;

public class KnowledgeBaseArticleConfiguration : IEntityTypeConfiguration<KnowledgeBaseArticle>
{
    public void Configure(EntityTypeBuilder<KnowledgeBaseArticle> builder)
    {
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.HasIndex(e => e.Slug).IsUnique();
    }
}
