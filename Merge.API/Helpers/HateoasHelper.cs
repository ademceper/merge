using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Merge.API.Helpers;

// ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
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
        var mergedValues = routeValues != null
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
        var links = new Dictionary<string, LinkDto>();

        // Helper method to merge route values
        Dictionary<string, object?> MergeRouteValues(int? pageValue = null)
        {
            var dict = new Dictionary<string, object?> { ["version"] = version, ["pageSize"] = pageSize };
            if (pageValue.HasValue) dict["page"] = pageValue.Value;
            
            if (routeValues != null)
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
