using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Merge.Application.DTOs.Search;

namespace Merge.API.Helpers;

/// <summary>
/// HATEOAS helper for generating hypermedia links in API responses
/// </summary>
public static class HateoasHelper
{
    /// <summary>
    /// Creates HATEOAS links for a product resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateProductLinks(IUrlHelper urlHelper, Guid productId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link("GetProductById", new { version, id = productId }) ?? $"/api/v{version}/products/{productId}",
                Method = "GET"
            },
            ["update"] = new LinkDto
            {
                Href = urlHelper.Link("UpdateProduct", new { version, id = productId }) ?? $"/api/v{version}/products/{productId}",
                Method = "PUT"
            },
            ["delete"] = new LinkDto
            {
                Href = urlHelper.Link("DeleteProduct", new { version, id = productId }) ?? $"/api/v{version}/products/{productId}",
                Method = "DELETE"
            },
            ["reviews"] = new LinkDto
            {
                Href = urlHelper.Link("GetProductReviews", new { version, productId }) ?? $"/api/v{version}/products/{productId}/reviews",
                Method = "GET"
            },
            ["questions"] = new LinkDto
            {
                Href = urlHelper.Link("GetProductQuestions", new { version, productId }) ?? $"/api/v{version}/products/{productId}/questions",
                Method = "GET"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a product bundle resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateProductBundleLinks(IUrlHelper urlHelper, Guid bundleId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link("GetProductBundleById", new { version, id = bundleId }) ?? $"/api/v{version}/products/bundles/{bundleId}",
                Method = "GET"
            },
            ["update"] = new LinkDto
            {
                Href = urlHelper.Link("UpdateProductBundle", new { version, id = bundleId }) ?? $"/api/v{version}/products/bundles/{bundleId}",
                Method = "PUT"
            },
            ["delete"] = new LinkDto
            {
                Href = urlHelper.Link("DeleteProductBundle", new { version, id = bundleId }) ?? $"/api/v{version}/products/bundles/{bundleId}",
                Method = "DELETE"
            },
            ["addItem"] = new LinkDto
            {
                Href = urlHelper.Link("AddProductToBundle", new { version, bundleId }) ?? $"/api/v{version}/products/bundles/{bundleId}/items",
                Method = "POST"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a product comparison resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateProductComparisonLinks(IUrlHelper urlHelper, Guid comparisonId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link("GetUserComparison", new { version, id = comparisonId }) ?? $"/api/v{version}/products/comparisons/{comparisonId}",
                Method = "GET"
            },
            ["matrix"] = new LinkDto
            {
                Href = urlHelper.Link("GetComparisonMatrix", new { version, id = comparisonId }) ?? $"/api/v{version}/products/comparisons/{comparisonId}/matrix",
                Method = "GET"
            },
            ["addProduct"] = new LinkDto
            {
                Href = urlHelper.Link("AddProductToComparison", new { version, id = comparisonId }) ?? $"/api/v{version}/products/comparisons/{comparisonId}/products",
                Method = "POST"
            },
            ["removeProduct"] = new LinkDto
            {
                Href = urlHelper.Link("RemoveProductFromComparison", new { version, id = comparisonId }) ?? $"/api/v{version}/products/comparisons/{comparisonId}/products",
                Method = "DELETE"
            },
            ["save"] = new LinkDto
            {
                Href = urlHelper.Link("SaveComparison", new { version, id = comparisonId }) ?? $"/api/v{version}/products/comparisons/{comparisonId}/save",
                Method = "POST"
            },
            ["delete"] = new LinkDto
            {
                Href = urlHelper.Link("DeleteComparison", new { version, id = comparisonId }) ?? $"/api/v{version}/products/comparisons/{comparisonId}",
                Method = "DELETE"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a product question resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateProductQuestionLinks(IUrlHelper urlHelper, Guid questionId, Guid productId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link("GetQuestion", new { version, id = questionId }) ?? $"/api/v{version}/products/questions/{questionId}",
                Method = "GET"
            },
            ["answers"] = new LinkDto
            {
                Href = urlHelper.Link("GetQuestionAnswers", new { version, questionId }) ?? $"/api/v{version}/products/questions/{questionId}/answers",
                Method = "GET"
            },
            ["markHelpful"] = new LinkDto
            {
                Href = urlHelper.Link("MarkQuestionHelpful", new { version, id = questionId }) ?? $"/api/v{version}/products/questions/{questionId}/helpful",
                Method = "POST"
            },
            ["unmarkHelpful"] = new LinkDto
            {
                Href = urlHelper.Link("UnmarkQuestionHelpful", new { version, id = questionId }) ?? $"/api/v{version}/products/questions/{questionId}/helpful",
                Method = "DELETE"
            },
            ["product"] = new LinkDto
            {
                Href = urlHelper.Link("GetProductById", new { version, id = productId }) ?? $"/api/v{version}/products/{productId}",
                Method = "GET"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a size guide resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateSizeGuideLinks(IUrlHelper urlHelper, Guid sizeGuideId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link("GetSizeGuide", new { version, id = sizeGuideId }) ?? $"/api/v{version}/products/size-guides/{sizeGuideId}",
                Method = "GET"
            },
            ["update"] = new LinkDto
            {
                Href = urlHelper.Link("UpdateSizeGuide", new { version, id = sizeGuideId }) ?? $"/api/v{version}/products/size-guides/{sizeGuideId}",
                Method = "PUT"
            },
            ["delete"] = new LinkDto
            {
                Href = urlHelper.Link("DeleteSizeGuide", new { version, id = sizeGuideId }) ?? $"/api/v{version}/products/size-guides/{sizeGuideId}",
                Method = "DELETE"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a product template resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateProductTemplateLinks(IUrlHelper urlHelper, Guid templateId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link("GetProductTemplate", new { version, id = templateId }) ?? $"/api/v{version}/products/templates/{templateId}",
                Method = "GET"
            },
            ["update"] = new LinkDto
            {
                Href = urlHelper.Link("UpdateProductTemplate", new { version, id = templateId }) ?? $"/api/v{version}/products/templates/{templateId}",
                Method = "PUT"
            },
            ["delete"] = new LinkDto
            {
                Href = urlHelper.Link("DeleteProductTemplate", new { version, id = templateId }) ?? $"/api/v{version}/products/templates/{templateId}",
                Method = "DELETE"
            },
            ["createFromTemplate"] = new LinkDto
            {
                Href = urlHelper.Link("CreateProductFromTemplate", new { version, templateId }) ?? $"/api/v{version}/products/templates/{templateId}/create",
                Method = "POST"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a product answer resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateProductAnswerLinks(IUrlHelper urlHelper, Guid questionId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link("GetQuestionAnswers", new { version, id = questionId }) ?? $"/api/v{version}/products/questions/{questionId}/answers",
                Method = "GET"
            },
            ["question"] = new LinkDto
            {
                Href = urlHelper.Link("GetQuestion", new { version, id = questionId }) ?? $"/api/v{version}/products/questions/{questionId}",
                Method = "GET"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates a simple self link
    /// </summary>
    public static Dictionary<string, LinkDto> CreateSelfLink(
        IUrlHelper urlHelper,
        string routeName,
        object? routeValues = null,
        string version = "1.0")
    {
        // Merge routeValues with version
        var mergedValues = routeValues is not null
            ? new Dictionary<string, object?> { ["version"] = version }
                .Concat(routeValues.GetType().GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(routeValues)))
                .ToDictionary(kv => kv.Key, kv => kv.Value)
            : new Dictionary<string, object?> { ["version"] = version };

        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link(routeName, mergedValues) ?? $"/api/v{version}/{routeName}",
                Method = "GET"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates pagination links for a paged result
    /// </summary>
    public static Dictionary<string, LinkDto> CreatePaginationLinks(
        IUrlHelper urlHelper,
        string routeName,
        int page,
        int pageSize,
        int totalPages,
        object? routeValues = null,
        string version = "1.0")
    {
        Dictionary<string, LinkDto> links = [];

        // Helper method to merge route values
        Dictionary<string, object?> MergeRouteValues(int? pageValue = null)
        {
            var dict = new Dictionary<string, object?> { ["version"] = version, ["pageSize"] = pageSize };
            if (pageValue.HasValue) dict["page"] = pageValue.Value;
            
            if (routeValues is not null)
            {
                foreach (var prop in routeValues.GetType().GetProperties())
                {
                    if (prop.Name != "page" && prop.Name != "pageSize" && prop.Name != "version")
                    {
                        dict[prop.Name] = prop.GetValue(routeValues);
                    }
                }
            }
            return dict;
        }

        if (page > 1)
        {
            links["first"] = new LinkDto
            {
                Href = urlHelper.Link(routeName, MergeRouteValues(1)) ?? $"/api/v{version}/{routeName}?page=1&pageSize={pageSize}",
                Method = "GET"
            };
            links["prev"] = new LinkDto
            {
                Href = urlHelper.Link(routeName, MergeRouteValues(page - 1)) ?? $"/api/v{version}/{routeName}?page={page - 1}&pageSize={pageSize}",
                Method = "GET"
            };
        }

        links["self"] = new LinkDto
        {
            Href = urlHelper.Link(routeName, MergeRouteValues(page)) ?? $"/api/v{version}/{routeName}?page={page}&pageSize={pageSize}",
            Method = "GET"
        };

        if (page < totalPages)
        {
            links["next"] = new LinkDto
            {
                Href = urlHelper.Link(routeName, MergeRouteValues(page + 1)) ?? $"/api/v{version}/{routeName}?page={page + 1}&pageSize={pageSize}",
                Method = "GET"
            };
            links["last"] = new LinkDto
            {
                Href = urlHelper.Link(routeName, MergeRouteValues(totalPages)) ?? $"/api/v{version}/{routeName}?page={totalPages}&pageSize={pageSize}",
                Method = "GET"
            };
        }

        return links;
    }

    /// <summary>
    /// Adds HATEOAS links to SearchResultDto
    /// </summary>
    public static object AddSearchLinks(SearchResultDto result, Microsoft.AspNetCore.Http.HttpRequest request)
    {
        var version = request.RouteValues["version"]?.ToString() ?? "1.0";
        var baseUrl = $"{request.Scheme}://{request.Host}/api/v{version}/search";
        
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = $"{baseUrl}?page={result.Page}&pageSize={result.PageSize}",
                Method = "GET"
            }
        };

        if (result.Page > 1)
        {
            links["first"] = new LinkDto
            {
                Href = $"{baseUrl}?page=1&pageSize={result.PageSize}",
                Method = "GET"
            };
            links["prev"] = new LinkDto
            {
                Href = $"{baseUrl}?page={result.Page - 1}&pageSize={result.PageSize}",
                Method = "GET"
            };
        }

        if (result.Page < result.TotalPages)
        {
            links["next"] = new LinkDto
            {
                Href = $"{baseUrl}?page={result.Page + 1}&pageSize={result.PageSize}",
                Method = "GET"
            };
            links["last"] = new LinkDto
            {
                Href = $"{baseUrl}?page={result.TotalPages}&pageSize={result.PageSize}",
                Method = "GET"
            };
        }

        return new
        {
            Products = result.Products,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            AvailableBrands = result.AvailableBrands,
            MinPrice = result.MinPrice,
            MaxPrice = result.MaxPrice,
            _links = links
        };
    }

    /// <summary>
    /// Creates HATEOAS links for a support ticket resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateSupportTicketLinks(IUrlHelper urlHelper, Guid ticketId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link("GetTicket", new { version, id = ticketId }) ?? $"/api/v{version}/support/tickets/{ticketId}",
                Method = "GET"
            },
            ["update"] = new LinkDto
            {
                Href = urlHelper.Link("UpdateTicket", new { version, id = ticketId }) ?? $"/api/v{version}/support/tickets/{ticketId}",
                Method = "PUT"
            },
            ["assign"] = new LinkDto
            {
                Href = urlHelper.Link("AssignTicket", new { version, id = ticketId }) ?? $"/api/v{version}/support/tickets/{ticketId}/assign",
                Method = "POST"
            },
            ["close"] = new LinkDto
            {
                Href = urlHelper.Link("CloseTicket", new { version, id = ticketId }) ?? $"/api/v{version}/support/tickets/{ticketId}/close",
                Method = "POST"
            },
            ["reopen"] = new LinkDto
            {
                Href = urlHelper.Link("ReopenTicket", new { version, id = ticketId }) ?? $"/api/v{version}/support/tickets/{ticketId}/reopen",
                Method = "POST"
            },
            ["messages"] = new LinkDto
            {
                Href = urlHelper.Link("GetTicketMessages", new { version, id = ticketId }) ?? $"/api/v{version}/support/tickets/{ticketId}/messages",
                Method = "GET"
            },
            ["addMessage"] = new LinkDto
            {
                Href = urlHelper.Link("AddMessage", new { version }) ?? $"/api/v{version}/support/tickets/messages",
                Method = "POST"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a live chat session resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateLiveChatSessionLinks(IUrlHelper urlHelper, Guid sessionId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link("GetSessionById", new { version, id = sessionId }) ?? $"/api/v{version}/support/live-chat/sessions/{sessionId}",
                Method = "GET"
            },
            ["assignAgent"] = new LinkDto
            {
                Href = urlHelper.Link("AssignAgent", new { version, sessionId }) ?? $"/api/v{version}/support/live-chat/sessions/{sessionId}/assign-agent",
                Method = "POST"
            },
            ["close"] = new LinkDto
            {
                Href = urlHelper.Link("CloseSession", new { version, sessionId }) ?? $"/api/v{version}/support/live-chat/sessions/{sessionId}/close",
                Method = "POST"
            },
            ["messages"] = new LinkDto
            {
                Href = urlHelper.Link("GetSessionMessages", new { version, sessionId }) ?? $"/api/v{version}/support/live-chat/sessions/{sessionId}/messages",
                Method = "GET"
            },
            ["sendMessage"] = new LinkDto
            {
                Href = urlHelper.Link("SendMessage", new { version, sessionId }) ?? $"/api/v{version}/support/live-chat/sessions/{sessionId}/messages",
                Method = "POST"
            },
            ["markRead"] = new LinkDto
            {
                Href = urlHelper.Link("MarkMessagesAsRead", new { version, sessionId }) ?? $"/api/v{version}/support/live-chat/sessions/{sessionId}/messages/mark-read",
                Method = "POST"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a knowledge base article resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateKnowledgeBaseArticleLinks(IUrlHelper urlHelper, Guid articleId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link("GetArticle", new { version, id = articleId }) ?? $"/api/v{version}/support/knowledge-base/articles/{articleId}",
                Method = "GET"
            },
            ["update"] = new LinkDto
            {
                Href = urlHelper.Link("UpdateArticle", new { version, id = articleId }) ?? $"/api/v{version}/support/knowledge-base/articles/{articleId}",
                Method = "PUT"
            },
            ["delete"] = new LinkDto
            {
                Href = urlHelper.Link("DeleteArticle", new { version, id = articleId }) ?? $"/api/v{version}/support/knowledge-base/articles/{articleId}",
                Method = "DELETE"
            },
            ["publish"] = new LinkDto
            {
                Href = urlHelper.Link("PublishArticle", new { version, id = articleId }) ?? $"/api/v{version}/support/knowledge-base/articles/{articleId}/publish",
                Method = "POST"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a FAQ resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateFaqLinks(IUrlHelper urlHelper, Guid faqId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link("GetById", new { version, id = faqId }) ?? $"/api/v{version}/support/faqs/{faqId}",
                Method = "GET"
            },
            ["update"] = new LinkDto
            {
                Href = urlHelper.Link("Update", new { version, id = faqId }) ?? $"/api/v{version}/support/faqs/{faqId}",
                Method = "PUT"
            },
            ["delete"] = new LinkDto
            {
                Href = urlHelper.Link("Delete", new { version, id = faqId }) ?? $"/api/v{version}/support/faqs/{faqId}",
                Method = "DELETE"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a customer communication resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateCustomerCommunicationLinks(IUrlHelper urlHelper, Guid communicationId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link("GetCommunication", new { version, id = communicationId }) ?? $"/api/v{version}/support/communications/{communicationId}",
                Method = "GET"
            },
            ["updateStatus"] = new LinkDto
            {
                Href = urlHelper.Link("UpdateStatus", new { version, id = communicationId }) ?? $"/api/v{version}/support/communications/{communicationId}/status",
                Method = "PUT"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a knowledge base category resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateKnowledgeBaseCategoryLinks(IUrlHelper urlHelper, Guid categoryId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = urlHelper.Link("GetCategory", new { version, id = categoryId }) ?? $"/api/v{version}/support/knowledge-base/categories/{categoryId}",
                Method = "GET"
            },
            ["update"] = new LinkDto
            {
                Href = urlHelper.Link("UpdateCategory", new { version, id = categoryId }) ?? $"/api/v{version}/support/knowledge-base/categories/{categoryId}",
                Method = "PUT"
            },
            ["delete"] = new LinkDto
            {
                Href = urlHelper.Link("DeleteCategory", new { version, id = categoryId }) ?? $"/api/v{version}/support/knowledge-base/categories/{categoryId}",
                Method = "DELETE"
            },
            ["articles"] = new LinkDto
            {
                Href = $"/api/v{version}/support/knowledge-base/articles?categoryId={categoryId}",
                Method = "GET"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a B2B user resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateB2BUserLinks(IUrlHelper urlHelper, Guid b2bUserId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/users/{b2bUserId}",
                Method = "GET"
            },
            ["update"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/users/{b2bUserId}",
                Method = "PUT"
            },
            ["delete"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/users/{b2bUserId}",
                Method = "DELETE"
            },
            ["approve"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/users/{b2bUserId}/approve",
                Method = "POST"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a purchase order resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreatePurchaseOrderLinks(IUrlHelper urlHelper, Guid purchaseOrderId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/purchase-orders/{purchaseOrderId}",
                Method = "GET"
            },
            ["submit"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/purchase-orders/{purchaseOrderId}/submit",
                Method = "POST"
            },
            ["cancel"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/purchase-orders/{purchaseOrderId}/cancel",
                Method = "POST"
            },
            ["approve"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/purchase-orders/{purchaseOrderId}/approve",
                Method = "POST"
            },
            ["reject"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/purchase-orders/{purchaseOrderId}/reject",
                Method = "POST"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a credit term resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateCreditTermLinks(IUrlHelper urlHelper, Guid creditTermId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/credit-terms/{creditTermId}",
                Method = "GET"
            },
            ["update"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/credit-terms/{creditTermId}",
                Method = "PUT"
            },
            ["delete"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/credit-terms/{creditTermId}",
                Method = "DELETE"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a wholesale price resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateWholesalePriceLinks(IUrlHelper urlHelper, Guid wholesalePriceId, Guid productId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/products/{productId}/wholesale-prices",
                Method = "GET"
            },
            ["update"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/wholesale-prices/{wholesalePriceId}",
                Method = "PUT"
            },
            ["delete"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/wholesale-prices/{wholesalePriceId}",
                Method = "DELETE"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a volume discount resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateVolumeDiscountLinks(IUrlHelper urlHelper, Guid volumeDiscountId, Guid? productId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/volume-discounts",
                Method = "GET"
            },
            ["update"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/volume-discounts/{volumeDiscountId}",
                Method = "PUT"
            },
            ["delete"] = new LinkDto
            {
                Href = $"/api/v{version}/b2b/volume-discounts/{volumeDiscountId}",
                Method = "DELETE"
            }
        };

        if (productId.HasValue)
        {
            links["product"] = new LinkDto
            {
                Href = $"/api/v{version}/products/{productId.Value}",
                Method = "GET"
            };
        }

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a cart resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateCartLinks(IUrlHelper urlHelper, Guid cartId, Guid userId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = $"/api/v{version}/cart",
                Method = "GET"
            },
            ["addItem"] = new LinkDto
            {
                Href = $"/api/v{version}/cart/items",
                Method = "POST"
            },
            ["clear"] = new LinkDto
            {
                Href = $"/api/v{version}/cart",
                Method = "DELETE"
            },
            ["user"] = new LinkDto
            {
                Href = $"/api/v{version}/users/{userId}",
                Method = "GET"
            }
        };

        return links;
    }

    /// <summary>
    /// Creates HATEOAS links for a cart item resource
    /// </summary>
    public static Dictionary<string, LinkDto> CreateCartItemLinks(IUrlHelper urlHelper, Guid cartItemId, Guid productId, string version = "1.0")
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = $"/api/v{version}/cart/items/{cartItemId}",
                Method = "GET"
            },
            ["update"] = new LinkDto
            {
                Href = $"/api/v{version}/cart/items/{cartItemId}",
                Method = "PUT"
            },
            ["remove"] = new LinkDto
            {
                Href = $"/api/v{version}/cart/items/{cartItemId}",
                Method = "DELETE"
            },
            ["product"] = new LinkDto
            {
                Href = $"/api/v{version}/products/{productId}",
                Method = "GET"
            }
        };

        return links;
    }
}

/// <summary>
/// Link DTO for HATEOAS
/// </summary>
public record LinkDto
{
    [JsonPropertyName("href")]
    public string Href { get; init; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; init; } = "GET";
}
