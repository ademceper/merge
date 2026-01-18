using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.DTOs.Content;
using Merge.Domain.Modules.Support;
using Merge.Domain.Modules.Content;
using System.Text.Json;

namespace Merge.Application.Mappings.Support;

public class SupportMappingProfile : Profile
{
    public SupportMappingProfile()
    {
        // Support Domain Mappings
        CreateMap<SupportTicket, SupportTicketDto>()
        .ConvertUsing((src, context) =>
        {
        var messages = src.Messages != null && src.Messages.Any()
        ? src.Messages.Select(m => new TicketMessageDto(
        m.Id,
        m.TicketId,
        m.UserId,
        m.User != null ? $"{m.User.FirstName} {m.User.LastName}" : "Unknown",
        m.Message,
        m.IsStaffResponse,
        m.IsInternal,
        m.CreatedAt,
        m.Attachments != null && m.Attachments.Any()
        ? m.Attachments.Select(a => new TicketAttachmentDto(
        a.Id,
        a.FileName,
        a.FilePath,
        a.FileType,
        a.FileSize,
        a.CreatedAt
        )).ToList().AsReadOnly()
        : Array.Empty<TicketAttachmentDto>().AsReadOnly()
        )).ToList().AsReadOnly()
        : Array.Empty<TicketMessageDto>().AsReadOnly();

        var attachments = src.Attachments != null && src.Attachments.Any()
        ? src.Attachments.Select(a => new TicketAttachmentDto(
        a.Id,
        a.FileName,
        a.FilePath,
        a.FileType,
        a.FileSize,
        a.CreatedAt
        )).ToList().AsReadOnly()
        : Array.Empty<TicketAttachmentDto>().AsReadOnly();

        return new SupportTicketDto(
        src.Id,
        src.TicketNumber,
        src.UserId,
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : "Unknown",
        src.User != null ? src.User.Email ?? string.Empty : string.Empty,
        src.Category.ToString(),
        src.Priority.ToString(),
        src.Status.ToString(),
        src.Subject,
        src.Description,
        src.OrderId,
        src.Order != null ? src.Order.OrderNumber : null,
        src.ProductId,
        src.Product != null ? src.Product.Name : null,
        src.AssignedToId,
        src.AssignedTo != null ? $"{src.AssignedTo.FirstName} {src.AssignedTo.LastName}" : null,
        src.ResolvedAt,
        src.ClosedAt,
        src.Messages != null ? src.Messages.Count : 0,
        src.Messages != null && src.Messages.Any() ? src.Messages.Max(m => m.CreatedAt) : null,
        src.CreatedAt,
        messages,
        attachments,
        null // Links will be set in controller
        );
        });

        CreateMap<TicketMessage, TicketMessageDto>()
        .ConvertUsing((src, context) =>
        {
        var attachments = src.Attachments != null && src.Attachments.Any()
        ? src.Attachments.Select(a => new TicketAttachmentDto(
        a.Id,
        a.FileName,
        a.FilePath,
        a.FileType,
        a.FileSize,
        a.CreatedAt
        )).ToList().AsReadOnly()
        : Array.Empty<TicketAttachmentDto>().AsReadOnly();

        return new TicketMessageDto(
        src.Id,
        src.TicketId,
        src.UserId,
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : "Unknown",
        src.Message,
        src.IsStaffResponse,
        src.IsInternal,
        src.CreatedAt,
        attachments
        );
        });

        CreateMap<TicketAttachment, TicketAttachmentDto>()
        .ConvertUsing(src => new TicketAttachmentDto(
        src.Id,
        src.FileName,
        src.FilePath,
        src.FileType,
        src.FileSize,
        src.CreatedAt
        ));

        CreateMap<CustomerCommunication, CustomerCommunicationDto>()
        .ConvertUsing((src, context) =>
        {
        CustomerCommunicationSettingsDto? metadata = null;
        if (!string.IsNullOrEmpty(src.Metadata))
        {
        try
        {
        metadata = JsonSerializer.Deserialize<CustomerCommunicationSettingsDto>(src.Metadata);
        }
        catch
        {
        }
        }

        return new CustomerCommunicationDto(
        src.Id,
        src.UserId,
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty,
        src.CommunicationType,
        src.Channel,
        src.Subject,
        src.Content,
        src.Direction,
        src.RelatedEntityId,
        src.RelatedEntityType,
        src.SentByUserId,
        src.SentBy != null ? $"{src.SentBy.FirstName} {src.SentBy.LastName}" : null,
        src.RecipientEmail,
        src.RecipientPhone,
        src.Status.ToString(),
        src.SentAt,
        src.DeliveredAt,
        src.ReadAt,
        src.ErrorMessage,
        metadata,
        src.CreatedAt,
        null // Links will be set in controller
        );
        });

        CreateMap<KnowledgeBaseArticle, KnowledgeBaseArticleDto>()
        .ConvertUsing((src, context) =>
        {
        var tags = !string.IsNullOrEmpty(src.Tags)
        ? src.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly()
        : Array.Empty<string>().AsReadOnly();

        return new KnowledgeBaseArticleDto(
        src.Id,
        src.Title,
        src.Slug,
        src.Content,
        src.Excerpt,
        src.CategoryId,
        src.Category != null ? src.Category.Name : null,
        src.Status.ToString(),
        src.ViewCount,
        src.HelpfulCount,
        src.NotHelpfulCount,
        src.IsFeatured,
        src.DisplayOrder,
        tags,
        src.AuthorId,
        src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : null,
        src.PublishedAt,
        src.CreatedAt,
        src.UpdatedAt,
        null // Links will be set in controller
        );
        });

        CreateMap<KnowledgeBaseCategory, KnowledgeBaseCategoryDto>()
        .ConvertUsing((src, context) =>
        {
        // Recursive mapping için helper method kullanıyoruz
        IReadOnlyList<KnowledgeBaseCategoryDto> MapSubCategories(KnowledgeBaseCategory category)
        {
        if (category.SubCategories == null || !category.SubCategories.Any())
        return Array.Empty<KnowledgeBaseCategoryDto>().AsReadOnly();

        return category.SubCategories.Select(sc => new KnowledgeBaseCategoryDto(
        sc.Id,
        sc.Name,
        sc.Slug,
        sc.Description,
        sc.ParentCategoryId,
        sc.ParentCategory != null ? sc.ParentCategory.Name : null,
        sc.DisplayOrder,
        sc.IsActive,
        sc.IconUrl,
        0, // ArticleCount will be set in handler
        MapSubCategories(sc), // Recursive
        sc.CreatedAt,
        null // Links will be set in controller
        )).ToList().AsReadOnly();
        }

        var subCategories = MapSubCategories(src);

        return new KnowledgeBaseCategoryDto(
        src.Id,
        src.Name,
        src.Slug,
        src.Description,
        src.ParentCategoryId,
        src.ParentCategory != null ? src.ParentCategory.Name : null,
        src.DisplayOrder,
        src.IsActive,
        src.IconUrl,
        0, // ArticleCount will be set in handler
        subCategories,
        src.CreatedAt,
        null // Links will be set in controller
        );
        });

        CreateMap<LiveChatSession, LiveChatSessionDto>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : src.GuestName))
        .ForMember(dest => dest.AgentName, opt => opt.MapFrom(src =>
        src.Agent != null ? $"{src.Agent.FirstName} {src.Agent.LastName}" : null))
        .ForMember(dest => dest.Tags, opt => opt.Ignore()) // Will be set in AfterMap
        .ForMember(dest => dest.RecentMessages, opt => opt.Ignore()) // Will be set in LiveChatService after batch loading
        .AfterMap((src, dest) =>
        {
        dest.Tags = !string.IsNullOrEmpty(src.Tags)
        ? src.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        : new List<string>();
        });

        CreateMap<LiveChatMessage, LiveChatMessageDto>()
        .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src =>
        src.Sender != null ? $"{src.Sender.FirstName} {src.Sender.LastName}" : null));
    }
}
