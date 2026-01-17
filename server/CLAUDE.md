# MERGE E-COMMERCE BACKEND - ULTRA-COMPREHENSIVE PROJECT CONTEXT

> **Bu dosya Claude Code'un projeyi TAM OLARAK anlaması için MEGA-KAPSAMLI referans kaynağıdır.**
> **Her conversation başında otomatik olarak yüklenir ve STRICT RULES olarak uygulanır.**
> **Toplam 4,262 C# dosyası, ~208,800 satır kod analiz edilmiştir.**

---

## TABLE OF CONTENTS

1. [Project Overview](#project-overview)
2. [Architecture Layers](#architecture-layers)
3. [Complete Directory Structure](#complete-directory-structure)
4. [All Domain Modules](#all-domain-modules)
5. [All Entities Reference](#all-entities-reference)
6. [All Value Objects](#all-value-objects)
7. [CQRS Pattern Implementation](#cqrs-pattern-implementation)
8. [Domain-Driven Design Patterns](#domain-driven-design-patterns)
9. [All Controllers & Endpoints](#all-controllers--endpoints)
10. [All Services Reference](#all-services-reference)
11. [Database & EF Core](#database--ef-core)
12. [Middleware Pipeline](#middleware-pipeline)
13. [Naming Conventions](#naming-conventions)
14. [Development Commands](#development-commands)
15. [Critical Rules](#critical-rules)
16. [Security Requirements](#security-requirements)
17. [Performance Guidelines](#performance-guidelines)
18. [Testing Standards](#testing-standards)
19. [Current Status & Tech Stack](#current-status--tech-stack)
20. [Key Files Reference](#key-files-reference)

---

## PROJECT OVERVIEW

| Property | Value |
|----------|-------|
| **Project Name** | Merge E-Commerce Backend API |
| **Framework** | .NET 9.0 / C# 12 |
| **Database** | PostgreSQL 16 |
| **Cache** | Redis 7.x (Distributed Cache) |
| **Architecture** | Clean Architecture + DDD + CQRS |
| **Total Files** | 4,262 C# files |
| **Lines of Code** | ~208,800 |
| **Domain Modules** | 13 core + 8 advanced = 21 modules |
| **Entities** | 150+ domain entities |
| **Commands** | 400+ CQRS commands |
| **Queries** | 391+ CQRS queries |
| **Controllers** | 97+ API controllers |
| **Domain Events** | 595+ events |
| **DbContexts** | 12 specialized contexts |

---

## ARCHITECTURE LAYERS

```
Merge.sln
├── Merge.Domain/           # Domain Layer (Business Logic - NO DEPENDENCIES)
├── Merge.Application/      # Application Layer (CQRS Use Cases)
├── Merge.Infrastructure/   # Infrastructure Layer (EF Core, External Services)
├── Merge.API/              # Presentation Layer (REST API Controllers)
└── Merge.Tests/            # Test Layer (xUnit, Integration Tests)
```

### Clean Architecture Dependency Rule (STRICT!)

```
        ┌─────────────────────────────────────────┐
        │              Merge.API                  │
        │         (Presentation Layer)            │
        └────────────────┬────────────────────────┘
                         │ depends on
        ┌────────────────▼────────────────────────┐
        │          Merge.Application              │
        │        (Application Layer)              │
        └────────────────┬────────────────────────┘
                         │ depends on
        ┌────────────────▼────────────────────────┐
        │            Merge.Domain                 │
        │          (Domain Layer)                 │
        │         (NO DEPENDENCIES!)              │
        └─────────────────────────────────────────┘
                         ▲
                         │ implements interfaces from
        ┌────────────────┴────────────────────────┐
        │        Merge.Infrastructure             │
        │       (Infrastructure Layer)            │
        └─────────────────────────────────────────┘

❌ NEVER: Domain depending on Infrastructure
❌ NEVER: Application depending on API
❌ NEVER: Domain depending on Application
✅ ALWAYS: Dependencies flow inward toward Domain
```

---

## COMPLETE DIRECTORY STRUCTURE

### Domain Layer (`Merge.Domain/`)

```
Merge.Domain/
├── Modules/                           # DDD Bounded Contexts (21 modules)
│   ├── Catalog/                      # Product, Category, Review, Wishlist, Variant, Bundle
│   │   ├── Product.cs                # Aggregate Root (SKU, Price, Stock, Category)
│   │   ├── Category.cs               # Aggregate Root (Name, Slug, Parent, SubCategories)
│   │   ├── ProductVariant.cs         # Entity (VariantName, Value, Price, Stock)
│   │   ├── ProductBundle.cs          # Entity (BundlePrice, Items, Discount)
│   │   ├── BundleItem.cs             # Entity (ProductId, Quantity, SortOrder)
│   │   ├── ProductTemplate.cs        # Entity (DefaultSKU, DefaultPrice, Attributes)
│   │   ├── ProductQuestion.cs        # Entity (Question, Answers, HelpfulCount)
│   │   ├── ProductAnswer.cs          # Entity (Answer, IsSellerAnswer)
│   │   ├── ProductComparison.cs      # Entity (UserId, Products, ShareCode)
│   │   ├── Review.cs                 # Entity (Rating, Comment, Media, Helpful)
│   │   ├── ReviewMedia.cs            # Entity (MediaUrl, Type)
│   │   ├── ReviewHelpfulness.cs      # Entity (IsHelpful)
│   │   ├── Wishlist.cs               # Entity (Name, Items, IsPublic)
│   │   ├── SharedWishlist.cs         # Entity (SharedWith, Permission)
│   │   ├── RecentlyViewedProduct.cs  # Entity (ViewedAt)
│   │   ├── SearchHistory.cs          # Entity (Query, ResultCount)
│   │   ├── PopularSearch.cs          # Entity (SearchCount)
│   │   ├── SizeGuide.cs              # Entity (Name, Type, Entries)
│   │   ├── SizeGuideEntry.cs         # Entity (Size, Measurements)
│   │   ├── ProductTranslation.cs     # Entity (LanguageCode, Name, Description)
│   │   ├── CategoryTranslation.cs    # Entity (LanguageCode, Name, Description)
│   │   ├── ProductTrustBadge.cs      # Entity (BadgeName, ValidFrom, ValidUntil)
│   │   └── VirtualTryOn.cs           # Entity (TryOnUrl, Dimensions)
│   │
│   ├── Identity/                     # User, Role, Address, Auth, 2FA
│   │   ├── User.cs                   # Aggregate Root (extends IdentityUser<Guid>)
│   │   ├── Address.cs                # Entity (AddressLine, City, Country)
│   │   ├── B2BUser.cs                # Entity (CompanyName, TaxId)
│   │   ├── OAuthAccount.cs           # Entity (Provider, AccessToken)
│   │   ├── OAuthProvider.cs          # Entity (ClientId, AuthorizeUrl)
│   │   ├── Organization.cs           # Entity (Name, Owner, Teams)
│   │   ├── Team.cs                   # Entity (Name, Members)
│   │   ├── TeamMember.cs             # Entity (Role, JoinedAt)
│   │   ├── RefreshToken.cs           # Entity (Token, ExpiryDate, IsRevoked)
│   │   ├── SecurityAlert.cs          # Entity (AlertType, Severity)
│   │   ├── AccountSecurityEvent.cs   # Entity (EventType, IPAddress)
│   │   ├── TwoFactorAuth.cs          # Entity (IsEnabled, BackupCodes)
│   │   ├── TwoFactorCode.cs          # Entity (Code, ExpiryTime)
│   │   ├── UserPreference.cs         # Entity (Key, Value)
│   │   ├── UserLanguagePreference.cs # Entity (LanguageCode)
│   │   ├── UserCurrencyPreference.cs # Entity (CurrencyCode)
│   │   └── UserActivityLog.cs        # Entity (Action, EntityType, Timestamp)
│   │
│   ├── Ordering/                     # Order, Cart, Shipping, Returns
│   │   ├── Order.cs                  # Aggregate Root (Items, Status, Payment)
│   │   ├── OrderItem.cs              # Entity (ProductId, Quantity, UnitPrice)
│   │   ├── Cart.cs                   # Entity (Items, ExpiresAt)
│   │   ├── CartItem.cs               # Entity (ProductId, Quantity)
│   │   ├── Shipping.cs               # Entity (TrackingNumber, Carrier)
│   │   ├── ShippingAddress.cs        # Entity (Address details)
│   │   ├── SavedCartItem.cs          # Entity (SavedAt, ExpiresAt)
│   │   ├── Invoice.cs                # Entity (InvoiceNumber, Amount, DueDate)
│   │   ├── OrderVerification.cs      # Entity (VerificationToken)
│   │   ├── PreOrder.cs               # Entity (EstimatedDelivery, Status)
│   │   ├── ReturnRequest.cs          # Entity (Reason, Status, RefundAmount)
│   │   ├── PurchaseOrder.cs          # B2B Entity (VendorId, Items)
│   │   ├── PurchaseOrderItem.cs      # Entity (ProductId, Quantity)
│   │   ├── LiveStreamOrder.cs        # Entity (LiveStreamId, OrderId)
│   │   ├── OrderSplit.cs             # Entity (OriginalOrder, NewOrders)
│   │   ├── OrderSplitItem.cs         # Entity (ProductId, Quantity)
│   │   ├── CustomsDeclaration.cs     # Entity (HSCode, Value)
│   │   ├── InternationalShipping.cs  # Entity (Country, HSCodes)
│   │   └── DeliveryTimeEstimation.cs # Entity (Origin, Destination, Days)
│   │
│   ├── Payment/                      # Payment, Subscriptions, Gift Cards
│   │   ├── Payment.cs                # Entity (Amount, Status, GatewayRef)
│   │   ├── PaymentMethod.cs          # Entity (Type, CardNumber, IsDefault)
│   │   ├── Currency.cs               # Entity (Code, Symbol, ExchangeRate)
│   │   ├── GiftCard.cs               # Entity (Code, Balance, ExpiryDate)
│   │   ├── GiftCardTransaction.cs    # Entity (Amount, Type)
│   │   ├── SubscriptionPlan.cs       # Entity (Name, Price, Features)
│   │   ├── UserSubscription.cs       # Entity (PlanId, NextBillingDate)
│   │   ├── SubscriptionPayment.cs    # Entity (Amount, DueDate)
│   │   ├── SubscriptionUsage.cs      # Entity (UsageType, Amount)
│   │   ├── CreditTerm.cs             # B2B Entity (MaxCredit, PaymentDays)
│   │   ├── TaxRule.cs                # Entity (CountryCode, TaxRate)
│   │   ├── VolumeDiscount.cs         # Entity (MinQuantity, Discount)
│   │   ├── WholesalePrice.cs         # B2B Entity (MinQuantity, Price)
│   │   ├── FraudDetectionRule.cs     # Entity (RuleType, Threshold)
│   │   ├── FraudAlert.cs             # Entity (AlertType, Severity)
│   │   └── PaymentFraudPrevention.cs # Entity (RiskScore, Status)
│   │
│   ├── Marketing/                    # Coupons, Campaigns, Flash Sales
│   │   ├── Coupon.cs                 # Entity (Code, DiscountType, UsageLimit)
│   │   ├── CouponUsage.cs            # Entity (UserId, UsedAt)
│   │   ├── EmailCampaign.cs          # Entity (Subject, TargetAudience)
│   │   ├── EmailCampaignRecipient.cs # Entity (SentAt, OpenedAt)
│   │   ├── EmailSubscriber.cs        # Entity (Email, IsActive)
│   │   ├── EmailVerification.cs      # Entity (VerificationToken)
│   │   ├── FlashSale.cs              # Entity (StartDate, EndDate, Discount)
│   │   ├── FlashSaleProduct.cs       # Entity (DiscountedPrice, Quantity)
│   │   ├── LiveStream.cs             # Entity (Title, IsLive, Viewers)
│   │   ├── LiveStreamProduct.cs      # Entity (SpecialPrice, Quantity)
│   │   ├── LiveStreamViewer.cs       # Entity (JoinedAt, Duration)
│   │   ├── LoyaltyAccount.cs         # Entity (Points, Tier)
│   │   ├── LoyaltyTier.cs            # Entity (MinPoints, Benefits)
│   │   ├── LoyaltyTransaction.cs     # Entity (Type, Points)
│   │   ├── LoyaltyRule.cs            # Entity (Condition, PointsReward)
│   │   ├── Referral.cs               # Entity (ReferralCode, Discount)
│   │   ├── ReferralCode.cs           # Entity (MaxUses, ExpiryDate)
│   │   └── AbandonedCartEmail.cs     # Entity (SentAt, ConversionAt)
│   │
│   ├── Marketplace/                  # Seller Stores, Commissions
│   │   ├── Store.cs                  # Aggregate Root (Name, Slug, Owner)
│   │   ├── SellerProfile.cs          # Entity (BankAccount, CommissionRate)
│   │   ├── SellerApplication.cs      # Entity (Status, RejectionReason)
│   │   ├── SellerDocument.cs         # Entity (DocumentType, DocumentUrl)
│   │   ├── SellerCommission.cs       # Entity (OrderId, Amount, Rate)
│   │   ├── SellerCommissionSettings.cs # Entity (BaseRate, CategoryRates)
│   │   ├── SellerTransaction.cs      # Entity (Type, Amount, Status)
│   │   ├── SellerInvoice.cs          # Entity (InvoiceNumber, Amount)
│   │   ├── CommissionPayout.cs       # Entity (Amount, Status)
│   │   ├── CommissionPayoutItem.cs   # Entity (CommissionId, Amount)
│   │   ├── CommissionTier.cs         # Entity (MinSales, Rate)
│   │   ├── TrustBadge.cs             # Entity (Name, Requirements)
│   │   └── SellerTrustBadge.cs       # Entity (BadgeId, AchievedAt)
│   │
│   ├── Inventory/                    # Stock, Warehouses
│   │   ├── Inventory.cs              # Entity (ProductId, Quantity, Reserved)
│   │   ├── Warehouse.cs              # Entity (Name, Location, Capacity)
│   │   ├── PickPack.cs               # Entity (OrderId, Status)
│   │   ├── PickPackItem.cs           # Entity (OrderItemId, Quantity)
│   │   └── StockMovement.cs          # Entity (Type, Quantity, Reason)
│   │
│   ├── Content/                      # Blog, CMS, Pages
│   │   ├── BlogPost.cs               # Entity (Title, Content, Status)
│   │   ├── BlogCategory.cs           # Entity (Name, Slug)
│   │   ├── BlogComment.cs            # Entity (Comment, Status)
│   │   ├── BlogPostView.cs           # Entity (ViewedAt, IpAddress)
│   │   ├── CMSPage.cs                # Entity (Title, Content, IsPublished)
│   │   ├── Banner.cs                 # Entity (ImageUrl, LinkUrl, IsActive)
│   │   ├── LandingPage.cs            # Entity (Name, Content)
│   │   ├── KnowledgeBaseArticle.cs   # Entity (Title, Content)
│   │   ├── KnowledgeBaseCategory.cs  # Entity (Name, Slug)
│   │   ├── KnowledgeBaseView.cs      # Entity (ViewedAt)
│   │   ├── Language.cs               # Entity (Code, Name)
│   │   ├── PageBuilder.cs            # Entity (Name, Content JSON)
│   │   ├── Policy.cs                 # Entity (Type, Title, Content)
│   │   ├── PolicyAcceptance.cs       # Entity (AcceptedAt)
│   │   ├── SitemapEntry.cs           # Entity (Priority, ChangeFrequency)
│   │   ├── SEOSettings.cs            # Entity (MetaTitle, MetaDescription)
│   │   └── StaticTranslation.cs      # Entity (Key, Language, Value)
│   │
│   ├── Notifications/                # Alerts, Push, Email
│   │   ├── Notification.cs           # Entity (Type, Title, IsRead)
│   │   ├── NotificationPreference.cs # Entity (NotificationType, IsEnabled)
│   │   ├── NotificationTemplate.cs   # Entity (Subject, Body, Variables)
│   │   ├── PushNotification.cs       # Entity (Title, Body, Data)
│   │   ├── PushNotificationDevice.cs # Entity (DeviceToken, DeviceType)
│   │   ├── EmailTemplate.cs          # Entity (Name, Subject, Body)
│   │   └── EmailAutomation.cs        # Entity (Trigger, Template)
│   │
│   ├── Support/                      # Tickets, Live Chat, FAQ
│   │   ├── SupportTicket.cs          # Entity (Subject, Priority, Status)
│   │   ├── TicketMessage.cs          # Entity (Message, IsFromSupport)
│   │   ├── TicketAttachment.cs       # Entity (AttachmentUrl)
│   │   ├── LiveChatSession.cs        # Entity (AgentId, Status)
│   │   ├── LiveChatMessage.cs        # Entity (Message, Timestamp)
│   │   ├── FAQ.cs                    # Entity (Question, Answer, Category)
│   │   ├── CustomerCommunication.cs  # Entity (Subject, Message, Type)
│   │   ├── QuestionHelpfulness.cs    # Entity (IsHelpful)
│   │   └── AnswerHelpfulness.cs      # Entity (IsHelpful)
│   │
│   ├── Analytics/                    # Reports, Dashboards
│   │   ├── Report.cs                 # Entity (Name, Type, Data JSON)
│   │   ├── ReportSchedule.cs         # Entity (Frequency, NextRunAt)
│   │   ├── DashboardMetric.cs        # Entity (Name, Value, Category)
│   │   ├── DataPipeline.cs           # Entity (Name, Status)
│   │   ├── DataQualityCheck.cs       # Entity (Name, CheckType)
│   │   ├── DataQualityRule.cs        # Entity (Rule, Severity)
│   │   ├── DataWarehouse.cs          # Entity (Name, LastRefresh)
│   │   ├── ETLProcess.cs             # Entity (Name, Status, RowCount)
│   │   └── ExchangeRateHistory.cs    # Entity (FromCurrency, Rate)
│   │
│   ├── B2B/                          # B2B Orders, Credit
│   │   └── (Entities in Identity and Payment modules)
│   │
│   └── LiveCommerce/                 # Live Shopping
│       └── (Entities in Marketing module)
│
├── ValueObjects/                      # 10 Value Objects
│   ├── Money.cs                      # Monetary value (Amount, Currency)
│   ├── SKU.cs                        # Stock Keeping Unit (3-50 chars)
│   ├── Email.cs                      # Email validation
│   ├── PhoneNumber.cs                # Phone validation
│   ├── IBAN.cs                       # Bank account validation
│   ├── Rating.cs                     # 0-5 range
│   ├── Percentage.cs                 # 0-100 percentage
│   ├── Slug.cs                       # URL-friendly text
│   ├── Url.cs                        # URL validation
│   └── Address.cs                    # Composite address
│
├── SharedKernel/                      # Base classes
│   ├── BaseEntity.cs                 # Id, CreatedAt, UpdatedAt, IsDeleted
│   ├── BaseAggregateRoot.cs          # IAggregateRoot + DomainEvents
│   ├── IDomainEvent.cs               # MediatR INotification
│   ├── Guard.cs                      # Validation guards
│   └── DomainEvents/                 # 595+ domain events
│       ├── ProductCreatedEvent.cs
│       ├── ProductUpdatedEvent.cs
│       ├── ProductDeletedEvent.cs
│       ├── ProductPriceChangedEvent.cs
│       ├── ProductStockReducedEvent.cs
│       ├── ProductOutOfStockEvent.cs
│       ├── OrderCreatedEvent.cs
│       ├── OrderConfirmedEvent.cs
│       ├── OrderShippedEvent.cs
│       ├── OrderDeliveredEvent.cs
│       ├── OrderCancelledEvent.cs
│       ├── UserCreatedEvent.cs
│       ├── PaymentProcessedEvent.cs
│       └── ... (595+ total events)
│
├── Specifications/                    # Query specifications
│   ├── Specification.cs              # Base specification
│   └── OrdersByUserSpec.cs           # Example specification
│
├── Interfaces/                        # Repository interfaces
│   ├── IRepository.cs                # Generic repository
│   ├── IUnitOfWork.cs                # Unit of Work
│   └── IDbContext.cs                 # DbContext interface
│
├── Enums/                             # 76+ business enums
│   ├── OrderStatus.cs                # Pending, Processing, Shipped, Delivered...
│   ├── PaymentStatus.cs              # Pending, Completed, Failed, Refunded
│   ├── DiscountType.cs               # Fixed, Percentage
│   ├── NotificationType.cs           # Email, Push, SMS
│   └── ... (76+ enums)
│
└── Exceptions/                        # Domain exceptions
    └── DomainException.cs            # Custom domain exception
```

### Application Layer (`Merge.Application/`)

```
Merge.Application/
├── [Module]/                          # Per-module organization
│   ├── Commands/
│   │   └── [CommandName]/
│   │       ├── [CommandName]Command.cs           # IRequest<TResponse>
│   │       ├── [CommandName]CommandHandler.cs    # IRequestHandler<,>
│   │       └── [CommandName]CommandValidator.cs  # FluentValidation
│   ├── Queries/
│   │   └── [QueryName]/
│   │       ├── [QueryName]Query.cs
│   │       ├── [QueryName]QueryHandler.cs
│   │       └── [QueryName]QueryValidator.cs
│   └── EventHandlers/                 # INotificationHandler<TEvent>
│
├── DTOs/                              # Record-based DTOs
│   ├── ProductDto.cs
│   ├── CategoryDto.cs
│   ├── OrderDto.cs
│   └── ... (200+ DTOs)
│
├── Mappings/                          # AutoMapper profiles
│   ├── CatalogMappingProfile.cs
│   ├── OrderingMappingProfile.cs
│   ├── IdentityMappingProfile.cs
│   └── ... (20+ profiles)
│
├── Services/                          # Application services
│   ├── CacheService.cs               # Redis distributed cache
│   ├── Product/
│   │   ├── ProductService.cs
│   │   ├── ProductRecommendationService.cs
│   │   └── ProductComparisonService.cs
│   ├── Order/
│   │   ├── OrderService.cs
│   │   ├── OrderCalculationService.cs
│   │   └── CartService.cs
│   ├── Payment/
│   │   ├── PaymentService.cs
│   │   └── FraudDetectionService.cs
│   ├── Seller/
│   │   ├── SellerDashboardService.cs
│   │   ├── CommissionService.cs
│   │   └── PayoutService.cs
│   └── Notification/
│       ├── EmailService.cs
│       ├── SmsService.cs
│       └── PushNotificationService.cs
│
├── Interfaces/                        # Service interfaces
│   ├── ICacheService.cs
│   ├── IEmailService.cs
│   └── ... (30+ interfaces)
│
├── Common/                            # Shared utilities
│   ├── TokenHasher.cs                # SHA256 token hashing
│   ├── RiskScoreConstants.cs         # Fraud detection constants
│   └── PagedResult.cs                # Pagination wrapper
│
├── Behaviors/                         # MediatR pipeline behaviors
│   └── ValidationBehavior.cs         # FluentValidation integration
│
└── Configuration/                     # IOptions<T> settings (25+ classes)
    ├── JwtSettings.cs
    ├── OrderSettings.cs
    ├── ShippingSettings.cs
    └── ... (25+ settings classes)
```

### Infrastructure Layer (`Merge.Infrastructure/`)

```
Merge.Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs       # Main DbContext (Identity)
│   ├── OrderingDbContext.cs          # Orders, Carts
│   ├── CatalogDbContext.cs           # Products, Categories
│   ├── PaymentDbContext.cs           # Payments
│   ├── MarketingDbContext.cs         # Coupons, Campaigns
│   ├── InventoryDbContext.cs         # Stock management
│   ├── MarketplaceDbContext.cs       # Sellers, Stores
│   ├── SupportDbContext.cs           # Tickets, FAQs
│   ├── ContentDbContext.cs           # Blogs, Pages
│   ├── AnalyticsDbContext.cs         # Reports, Dashboards
│   ├── NotificationDbContext.cs      # Notifications
│   └── Configurations/               # 94+ IEntityTypeConfiguration<T>
│       ├── ProductConfiguration.cs
│       ├── OrderConfiguration.cs
│       ├── UserConfiguration.cs
│       └── ... (94+ configurations)
│
├── Repositories/
│   └── Repository.cs                 # Generic repository implementation
│
├── UnitOfWork/
│   └── UnitOfWork.cs                 # Transaction + Outbox pattern
│
├── BackgroundServices/
│   └── OutboxMessagePublisher.cs     # Domain event processor
│
├── ExternalServices/
│   ├── PaymentGateways/
│   │   ├── StripeGateway.cs          # Stripe integration
│   │   ├── IyzicoGateway.cs          # Iyzico integration
│   │   └── PayTRGateway.cs           # PayTR integration
│   ├── ShippingProviders/
│   │   ├── YurticiProvider.cs
│   │   ├── ArasProvider.cs
│   │   └── MNGProvider.cs
│   ├── EmailProviders/
│   │   ├── SendGridProvider.cs
│   │   └── SmtpProvider.cs
│   └── SmsProviders/
│       └── TwilioProvider.cs
│
└── DependencyInjection.cs            # Service registration
```

### API Layer (`Merge.API/`)

```
Merge.API/
├── Controllers/                       # 97+ controllers
│   ├── Catalog/
│   │   ├── CategoriesController.cs
│   │   ├── InventoryController.cs
│   │   ├── ProductQuestionsController.cs
│   │   ├── ReviewsController.cs
│   │   ├── SizeGuidesController.cs
│   │   └── BundlesController.cs
│   ├── Product/
│   │   ├── ProductsController.cs
│   │   └── ProductTemplatesController.cs
│   ├── Cart/
│   │   └── CartController.cs
│   ├── Order/
│   │   ├── OrdersController.cs
│   │   ├── ReturnsController.cs
│   │   └── PreOrdersController.cs
│   ├── Payment/
│   │   ├── PaymentsController.cs
│   │   ├── PaymentMethodsController.cs
│   │   ├── SubscriptionsController.cs
│   │   └── GiftCardsController.cs
│   ├── Seller/
│   │   ├── StoresController.cs
│   │   ├── DashboardController.cs
│   │   ├── CommissionsController.cs
│   │   └── FinanceController.cs
│   ├── Marketing/
│   │   ├── CouponsController.cs
│   │   ├── FlashSalesController.cs
│   │   ├── CampaignsController.cs
│   │   ├── LoyaltyController.cs
│   │   └── ReferralsController.cs
│   ├── Content/
│   │   ├── BlogsController.cs
│   │   ├── PagesController.cs
│   │   ├── BannersController.cs
│   │   └── KnowledgeBaseController.cs
│   ├── Identity/
│   │   ├── AuthController.cs
│   │   ├── UsersController.cs
│   │   ├── AddressesController.cs
│   │   └── OrganizationsController.cs
│   ├── Support/
│   │   ├── TicketsController.cs
│   │   ├── FaqsController.cs
│   │   └── LiveChatController.cs
│   ├── B2B/
│   │   └── B2BController.cs          # (1,078 lines - needs split)
│   ├── Analytics/
│   │   ├── ReportsController.cs
│   │   └── DashboardController.cs
│   └── ...
│
├── Middleware/
│   ├── GlobalExceptionHandlerMiddleware.cs  # RFC 7807 ProblemDetails
│   ├── RateLimitingMiddleware.cs            # Per-endpoint rate limiting
│   ├── IdempotencyKeyMiddleware.cs          # Duplicate request prevention
│   └── IpWhitelistMiddleware.cs             # Admin protection
│
├── Helpers/
│   ├── HateoasHelper.cs              # HATEOAS links
│   └── HttpResponseExtensions.cs     # ETag, Cache-Control
│
├── Program.cs                        # Application startup
└── appsettings.json                  # Configuration
```

---

## ALL DOMAIN MODULES

### Module Overview (21 Total)

| # | Module | Entities | Description |
|---|--------|----------|-------------|
| 1 | **Catalog** | 25 | Products, Categories, Variants, Bundles, Reviews |
| 2 | **Identity** | 17 | Users, Roles, Auth, 2FA, Organizations |
| 3 | **Ordering** | 19 | Orders, Carts, Shipping, Returns |
| 4 | **Payment** | 16 | Payments, Subscriptions, Gift Cards, Fraud |
| 5 | **Marketing** | 17 | Coupons, Campaigns, Flash Sales, Loyalty |
| 6 | **Marketplace** | 13 | Stores, Commissions, Payouts |
| 7 | **Inventory** | 5 | Stock, Warehouses, Pick/Pack |
| 8 | **Content** | 17 | Blog, CMS, Pages, SEO |
| 9 | **Notifications** | 7 | Alerts, Push, Email Templates |
| 10 | **Support** | 9 | Tickets, Live Chat, FAQ |
| 11 | **Analytics** | 9 | Reports, Dashboards, ETL |
| 12 | **B2B** | 5 | Wholesale, Credit Terms |
| 13 | **LiveCommerce** | 4 | Live Shopping, Streams |

---

## ALL ENTITIES REFERENCE

### Core Aggregate Roots

```csharp
// Product - Main catalog aggregate
public class Product : BaseAggregateRoot
{
    // Properties
    public string Name { get; private set; }
    public string Description { get; private set; }
    public SKU SKU { get; private set; }           // Value Object
    public Money Price { get; private set; }        // Value Object
    public Money? DiscountPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public string? Brand { get; private set; }
    public string? ImageUrl { get; private set; }
    public IReadOnlyList<string> ImageUrls { get; private set; }
    public decimal Rating { get; private set; }
    public int ReviewCount { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid? SellerId { get; private set; }
    public Guid? StoreId { get; private set; }
    public byte[]? RowVersion { get; private set; } // Concurrency

    // Collections
    public IReadOnlyCollection<ProductVariant> Variants { get; }
    public IReadOnlyCollection<Review> Reviews { get; }
    public IReadOnlyCollection<OrderItem> OrderItems { get; }

    // Factory Method - ALWAYS USE THIS
    public static Product Create(
        string name, string description, SKU sku, Money price,
        int stockQuantity, Guid categoryId) { ... }

    // Domain Methods
    public void SetPrice(Money newPrice) { ... }
    public void SetDiscountPrice(Money? discountPrice) { ... }
    public void ReduceStock(int quantity) { ... }
    public void IncreaseStock(int quantity) { ... }
    public void ReserveStock(int quantity) { ... }
    public void Activate() { ... }
    public void Deactivate() { ... }
    public void UpdateRating(decimal newRating, int newCount) { ... }
    public void AddVariant(ProductVariant variant) { ... }
    public void RemoveVariant(Guid variantId) { ... }
    public void MarkAsDeleted() { ... }

    // Domain Events
    // ProductCreatedEvent, ProductUpdatedEvent, ProductDeletedEvent,
    // ProductPriceChangedEvent, ProductStockReducedEvent, ProductOutOfStockEvent
}

// Order - Order aggregate
public class Order : BaseAggregateRoot
{
    // Properties
    public Guid UserId { get; private set; }
    public Guid AddressId { get; private set; }
    public string OrderNumber { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal Tax { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal CouponDiscount { get; private set; }
    public decimal GiftCardDiscount { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public string? PaymentMethod { get; private set; }
    public DateTime? ShippedDate { get; private set; }
    public DateTime? DeliveredDate { get; private set; }
    public Guid? CouponId { get; private set; }
    public byte[]? RowVersion { get; private set; }

    // Collections
    public IReadOnlyCollection<OrderItem> OrderItems { get; }
    public IReadOnlyCollection<ReturnRequest> ReturnRequests { get; }

    // Factory Method
    public static Order Create(Guid userId, Guid addressId) { ... }

    // Domain Methods
    public void AddItem(Product product, int quantity) { ... }
    public void RemoveItem(Guid orderItemId) { ... }
    public void UpdateItemQuantity(Guid orderItemId, int quantity) { ... }
    public void ApplyCoupon(Coupon coupon) { ... }
    public void RemoveCoupon() { ... }
    public void ApplyGiftCardDiscount(decimal amount) { ... }
    public void Confirm() { ... }
    public void Ship(string trackingNumber) { ... }
    public void Deliver() { ... }
    public void Cancel(string reason) { ... }
    public void Refund() { ... }
    public void PutOnHold() { ... }
    public void SetPaymentStatus(PaymentStatus status) { ... }
    public void RecalculateTotals() { ... }

    // State Machine
    // Pending → Processing/Cancelled/OnHold → Shipped → Delivered/Refunded
}

// Category - Category aggregate
public class Category : BaseAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Slug Slug { get; private set; }          // Value Object
    public string? ImageUrl { get; private set; }
    public Guid? ParentCategoryId { get; private set; }

    public IReadOnlyCollection<Category> SubCategories { get; }
    public IReadOnlyCollection<Product> Products { get; }

    public static Category Create(string name, string description, Slug slug) { ... }

    public void UpdateName(string name) { ... }
    public void UpdateSlug(Slug slug) { ... }
    public void SetParentCategory(Guid? parentId) { ... }
    public void AddSubCategory(Category subCategory) { ... }
}

// Store - Seller store aggregate
public class Store : BaseAggregateRoot
{
    public string Name { get; private set; }
    public Slug Slug { get; private set; }
    public Guid OwnerId { get; private set; }
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? BannerUrl { get; private set; }
    public bool IsActive { get; private set; }

    public static Store Create(string name, Slug slug, Guid ownerId) { ... }

    public void UpdateDetails(string name, string description) { ... }
    public void UpdateBranding(string logoUrl, string bannerUrl) { ... }
    public void Activate() { ... }
    public void Deactivate() { ... }
}
```

---

## ALL VALUE OBJECTS

### Money (Monetary Values)

```csharp
public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    // Constructor with validation
    public Money(decimal amount, string currency = "TRY")
    {
        Guard.AgainstNegative(amount, nameof(amount));
        Guard.AgainstNullOrEmpty(currency, nameof(currency));
        Guard.AgainstInvalidLength(currency, 3, 3, nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    // Factory methods
    public static Money Zero(string currency = "TRY") => new(0, currency);

    // Operations
    public Money Add(Money other)
    {
        Guard.AgainstDifferentCurrency(Currency, other.Currency);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        Guard.AgainstDifferentCurrency(Currency, other.Currency);
        Guard.AgainstNegative(Amount - other.Amount, "result");
        return new Money(Amount - other.Amount, Currency);
    }

    public static implicit operator decimal(Money money) => money.Amount;
}
```

### SKU (Stock Keeping Unit)

```csharp
public record SKU
{
    public string Value { get; }

    // Constructor with validation
    public SKU(string value)
    {
        Guard.AgainstNullOrEmpty(value, nameof(value));

        // Normalize to uppercase
        value = value.ToUpperInvariant().Trim();

        // Validate format: 3-50 alphanumeric with hyphens/underscores
        if (!Regex.IsMatch(value, @"^[A-Z0-9][A-Z0-9\-_]{1,48}[A-Z0-9]$|^[A-Z0-9]{3}$"))
            throw new ArgumentException("Invalid SKU format", nameof(value));

        Value = value;
    }

    // Factory methods
    public static SKU Generate(string prefix, int id)
        => new($"{prefix}-{id:D6}");

    public static SKU Generate(string category, string brand, int sequence)
        => new($"{category.ToUpperInvariant()}-{brand.ToUpperInvariant()}-{sequence:D6}");

    // Convenience methods
    public bool StartsWith(string prefix) => Value.StartsWith(prefix);
    public bool Contains(string segment) => Value.Contains(segment);

    public static implicit operator string(SKU sku) => sku.Value;
    public override string ToString() => Value;
}
```

### Slug (URL-Friendly Text)

```csharp
public record Slug
{
    public string Value { get; }

    public Slug(string value)
    {
        Guard.AgainstNullOrEmpty(value, nameof(value));

        // Normalize: lowercase, Turkish chars, special chars
        value = NormalizeSlug(value);

        // Validate format
        if (!Regex.IsMatch(value, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            throw new ArgumentException("Invalid slug format", nameof(value));

        Value = value;
    }

    public static Slug FromString(string text) => new(text);

    private static string NormalizeSlug(string text)
    {
        // Turkish character normalization
        text = text.ToLowerInvariant()
            .Replace('ı', 'i').Replace('ğ', 'g').Replace('ü', 'u')
            .Replace('ş', 's').Replace('ö', 'o').Replace('ç', 'c');

        // Replace spaces/underscores with hyphens
        text = Regex.Replace(text, @"[\s_]+", "-");

        // Remove special characters
        text = Regex.Replace(text, @"[^a-z0-9\-]", "");

        // Remove leading/trailing/duplicate hyphens
        text = Regex.Replace(text, @"-+", "-").Trim('-');

        return text;
    }

    public static implicit operator string(Slug slug) => slug.Value;
}
```

### Email (Email Validation)

```csharp
public record Email
{
    public string Value { get; }

    public Email(string value)
    {
        Guard.AgainstNullOrEmpty(value, nameof(value));

        // Normalize to lowercase
        value = value.ToLowerInvariant().Trim();

        // Validate format
        if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainException("Invalid email format");

        Value = value;
    }

    public static implicit operator string(Email email) => email.Value;
}
```

### Other Value Objects

```csharp
// Rating (0-5 stars)
public record Rating
{
    public int Value { get; }

    public Rating(int value)
    {
        if (value < 0 || value > 5)
            throw new ArgumentException("Rating must be between 0 and 5");
        Value = value;
    }

    public bool IsFiveStars => Value == 5;
    public bool IsFourStarsOrHigher => Value >= 4;
}

// Percentage (0-100)
public record Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentException("Percentage must be between 0 and 100");
        Value = value;
    }

    public decimal CalculateAmount(decimal baseAmount) => baseAmount * Value / 100;
}

// PhoneNumber
public record PhoneNumber
{
    public string Value { get; }
    public string CountryCode { get; }

    public PhoneNumber(string value, string countryCode = "TR") { ... }
}

// IBAN
public record IBAN
{
    public string Value { get; }

    public IBAN(string value)
    {
        // Validate IBAN format and checksum
        ...
    }
}

// Address (Composite)
public record Address
{
    public string AddressLine1 { get; }
    public string? AddressLine2 { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public string GetFullAddress() => ...;
}

// Url
public record Url
{
    public string Value { get; }

    public Url(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out _))
            throw new DomainException("Invalid URL format");
        Value = value;
    }
}
```

---

## CQRS PATTERN IMPLEMENTATION

### Command Pattern (State-Changing Operations)

```csharp
// Command Definition
// File: Merge.Application/Catalog/Commands/CreateCategory/CreateCategoryCommand.cs
public record CreateCategoryCommand(
    string Name,
    string Description,
    string Slug,
    string? ImageUrl,
    Guid? ParentCategoryId
) : IRequest<CategoryDto>;

// Command Handler
// File: Merge.Application/Catalog/Commands/CreateCategory/CreateCategoryCommandHandler.cs
public class CreateCategoryCommandHandler(
    IRepository<Category> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICacheService cache,
    ILogger<CreateCategoryCommandHandler> logger
) : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        // 1. Create domain entity via factory method
        var category = Category.Create(
            request.Name,
            request.Description,
            Slug.FromString(request.Slug)
        );

        if (request.ParentCategoryId.HasValue)
            category.SetParentCategory(request.ParentCategoryId.Value);

        if (!string.IsNullOrEmpty(request.ImageUrl))
            category.UpdateImageUrl(request.ImageUrl);

        // 2. Persist (no SaveChanges in repository!)
        await repository.AddAsync(category, ct);

        // 3. Commit transaction + persist OutboxMessages
        await unitOfWork.SaveChangesAsync(ct);

        // 4. Invalidate cache
        await cache.RemoveByPrefixAsync("categories_", ct);

        logger.LogInformation("Category created: {CategoryId} {Name}", category.Id, category.Name);

        return mapper.Map<CategoryDto>(category);
    }
}

// Command Validator
// File: Merge.Application/Catalog/Commands/CreateCategory/CreateCategoryCommandValidator.cs
public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be URL-friendly (lowercase, hyphens only)");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");
    }
}
```

### Query Pattern (Read-Only Operations)

```csharp
// Query Definition
// File: Merge.Application/Catalog/Queries/GetCategoryById/GetCategoryByIdQuery.cs
public record GetCategoryByIdQuery(Guid CategoryId) : IRequest<CategoryDto?>;

// Query Handler
// File: Merge.Application/Catalog/Queries/GetCategoryById/GetCategoryByIdQueryHandler.cs
public class GetCategoryByIdQueryHandler(
    CatalogDbContext context,
    IMapper mapper,
    ICacheService cache,
    ILogger<GetCategoryByIdQueryHandler> logger
) : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken ct)
    {
        var cacheKey = $"category_{request.CategoryId}";

        // Try cache first
        var cached = await cache.GetAsync<CategoryDto>(cacheKey, ct);
        if (cached != null)
            return cached;

        // Query database with optimizations
        var category = await context.Categories
            .AsNoTracking()                        // Read-only
            .AsSplitQuery()                        // Multiple includes optimization
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDeleted, ct);

        if (category == null)
            return null;

        var dto = mapper.Map<CategoryDto>(category);

        // Cache result
        await cache.SetAsync(cacheKey, dto, CacheExpiration, ct);

        return dto;
    }
}
```

### CQRS Command Templates

| Operation | Command Name | Return Type |
|-----------|--------------|-------------|
| Create | Create{Entity}Command | {Entity}Dto |
| Update (Full) | Update{Entity}Command | {Entity}Dto |
| Update (Partial) | Patch{Entity}Command | {Entity}Dto |
| Delete | Delete{Entity}Command | bool |
| State Change | {Action}{Entity}Command | {Entity}Dto |

### CQRS Query Templates

| Operation | Query Name | Return Type |
|-----------|------------|-------------|
| Get by ID | Get{Entity}ByIdQuery | {Entity}Dto? |
| Get all (paged) | GetAll{Entities}Query | PagedResult<{Entity}Dto> |
| Search | Search{Entities}Query | PagedResult<{Entity}Dto> |
| Get by filter | Get{Entities}By{Filter}Query | List<{Entity}Dto> |

---

## DOMAIN-DRIVEN DESIGN PATTERNS

### Factory Methods (REQUIRED for Entity Creation)

```csharp
// ✅ CORRECT - Always use factory methods
var product = Product.Create(
    name: "iPhone 15 Pro",
    description: "Latest Apple smartphone",
    sku: new SKU("APPLE-IPHONE15PRO-001"),
    price: new Money(59999.99m, "TRY"),
    stockQuantity: 100,
    categoryId: electronicsId
);

// ❌ WRONG - Never use constructors directly
var product = new Product
{
    Name = "iPhone 15 Pro",
    Price = 59999.99m  // Wrong! Should be Money value object
};
```

### Domain Methods (REQUIRED for State Changes)

```csharp
// ✅ CORRECT - Use domain methods
product.SetPrice(new Money(54999.99m, "TRY"));
product.ReduceStock(5);
order.ApplyCoupon(coupon);
order.Ship("YK123456789TR");

// ❌ WRONG - Never set properties directly
product.Price = 54999.99m;
product.StockQuantity -= 5;
order.Status = OrderStatus.Shipped;
```

### Guard Clauses (Domain Invariants)

```csharp
// Guard.cs - Validation utilities
public static class Guard
{
    public static void AgainstNull(object? value, string parameterName)
    {
        if (value == null)
            throw new ArgumentNullException(parameterName);
    }

    public static void AgainstNullOrEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be null or empty", parameterName);
    }

    public static void AgainstNegative(decimal value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentException($"{parameterName} cannot be negative", parameterName);
    }

    public static void AgainstNegativeOrZero(decimal value, string parameterName)
    {
        if (value <= 0)
            throw new ArgumentException($"{parameterName} must be positive", parameterName);
    }

    public static void AgainstInvalidLength(string value, int min, int max, string parameterName)
    {
        if (value.Length < min || value.Length > max)
            throw new ArgumentException($"{parameterName} must be between {min} and {max} characters", parameterName);
    }
}
```

### Domain Events

```csharp
// Raise domain events in entity methods
public void SetPrice(Money newPrice)
{
    Guard.AgainstNull(newPrice, nameof(newPrice));
    Guard.AgainstNegativeOrZero(newPrice.Amount, "price");

    var oldPrice = Price;
    Price = newPrice;
    UpdatedAt = DateTime.UtcNow;

    // Raise domain event
    AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
}

public void ReduceStock(int quantity)
{
    Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

    if (StockQuantity < quantity)
        throw new DomainException($"Insufficient stock. Available: {StockQuantity}, Requested: {quantity}");

    StockQuantity -= quantity;
    UpdatedAt = DateTime.UtcNow;

    AddDomainEvent(new ProductStockReducedEvent(Id, quantity, StockQuantity));

    if (StockQuantity == 0)
        AddDomainEvent(new ProductOutOfStockEvent(Id));
}
```

### Specification Pattern

```csharp
// Specification interface
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int Take { get; }
    int Skip { get; }
    bool IsNoTracking { get; }
}

// Example specification
public class OrdersByUserSpec : Specification<Order>
{
    public OrdersByUserSpec(Guid userId)
    {
        Criteria = order => order.UserId == userId && !order.IsDeleted;

        AddInclude(o => o.User);
        AddInclude(o => o.Address);
        AddInclude(o => o.OrderItems);
        AddIncludeString("OrderItems.Product");

        ApplyOrderByDescending(o => o.CreatedAt);
        IsNoTracking = true;
    }
}

// Usage in handler
var spec = new OrdersByUserSpec(userId);
var orders = await repository.ListAsync(spec, ct);
```

### Unit of Work with Outbox Pattern

```csharp
// UnitOfWork.cs
public class UnitOfWork(ApplicationDbContext context, IDomainEventDispatcher? dispatcher) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // 1. Extract domain events from tracked aggregates
        var domainEvents = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        // 2. Convert to OutboxMessages (same transaction = durability)
        foreach (var domainEvent in domainEvents)
        {
            context.Set<OutboxMessage>().Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = domainEvent.GetType().AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                OccurredOnUtc = domainEvent.OccurredOn,
                CreatedAt = DateTime.UtcNow
            });
        }

        // 3. Clear domain events from entities
        foreach (var entry in context.ChangeTracker.Entries<IAggregateRoot>())
            entry.Entity.ClearDomainEvents();

        // 4. Persist everything in single transaction
        return await context.SaveChangesAsync(ct);
    }
}
```

---

## ALL CONTROLLERS & ENDPOINTS

### API Versioning

```
Base Route: /api/v{version:apiVersion}/{controller}
Current Version: 1.0
Example: /api/v1/catalog/categories
```

### Categories Controller

```
Route: /api/v1/catalog/categories

GET    /                    # List all categories (paginated)
GET    /main                # List main categories (no parent)
GET    /{id}                # Get category by ID
POST   /                    # Create category [Admin]
PUT    /{id}                # Update category [Admin]
PATCH  /{id}                # Partial update [Admin]
DELETE /{id}                # Delete category [Admin]
GET    /{id}/subcategories  # List subcategories
GET    /{id}/products       # List products in category
```

### Products Controller

```
Route: /api/v1/products

GET    /                    # List products (paginated, filtered)
GET    /{id}                # Get product by ID
POST   /                    # Create product [Seller]
PUT    /{id}                # Update product [Seller]
PATCH  /{id}                # Partial update [Seller]
DELETE /{id}                # Delete product [Seller]
GET    /{id}/variants       # List product variants
POST   /{id}/variants       # Add variant [Seller]
GET    /{id}/reviews        # List product reviews
GET    /search              # Search products
GET    /trending            # Trending products
GET    /recommended         # Recommended for user
```

### Orders Controller

```
Route: /api/v1/orders

GET    /                    # List user orders (paginated)
POST   /                    # Create order
GET    /{id}                # Get order details
PUT    /{id}/items          # Update order items
DELETE /{id}/items/{itemId} # Remove order item
POST   /{id}/coupon         # Apply coupon
DELETE /{id}/coupon         # Remove coupon
POST   /{id}/gift-card      # Apply gift card
POST   /{id}/confirm        # Confirm order
POST   /{id}/ship           # Ship order [Admin/Seller]
POST   /{id}/deliver        # Mark delivered [Admin/Seller]
POST   /{id}/cancel         # Cancel order
POST   /{id}/refund         # Request refund
GET    /{id}/tracking       # Get tracking info
GET    /{id}/invoice        # Get/Generate invoice
```

### Cart Controller

```
Route: /api/v1/cart

GET    /                    # Get user cart
POST   /items               # Add item to cart
PUT    /items/{productId}   # Update item quantity
DELETE /items/{productId}   # Remove item from cart
DELETE /                    # Clear cart
POST   /validate            # Validate cart (stock, prices)
POST   /save                # Save cart for later
GET    /saved               # Get saved cart items
```

### Payment Controller

```
Route: /api/v1/payments

POST   /                    # Process payment
GET    /{id}                # Get payment details
POST   /{id}/refund         # Refund payment
GET    /methods             # List payment methods
POST   /methods             # Add payment method
DELETE /methods/{id}        # Remove payment method
GET    /invoices            # List invoices
```

### Seller Controllers

```
Route: /api/v1/seller

# Dashboard
GET    /dashboard           # Dashboard stats
GET    /dashboard/metrics   # Performance metrics
GET    /dashboard/trends    # Sales trends

# Stores
GET    /stores              # List seller stores
POST   /stores              # Create store
PUT    /stores/{id}         # Update store
GET    /stores/{id}/stats   # Store statistics

# Commissions
GET    /commissions         # List commissions
GET    /commissions/{id}    # Commission details
GET    /commission-settings # Commission settings
PUT    /commission-settings # Update settings

# Finance
GET    /payouts             # List payouts
POST   /payouts             # Request payout
GET    /balance             # Available balance
GET    /invoices            # Seller invoices
```

### Marketing Controllers

```
Route: /api/v1/marketing

# Coupons
GET    /coupons             # List coupons
POST   /coupons             # Create coupon [Admin]
PUT    /coupons/{id}        # Update coupon [Admin]
DELETE /coupons/{id}        # Delete coupon [Admin]
GET    /coupons/validate    # Validate coupon code

# Flash Sales
GET    /flash-sales         # List flash sales
POST   /flash-sales         # Create flash sale [Admin]
GET    /flash-sales/active  # Active flash sales

# Loyalty
GET    /loyalty             # User loyalty account
GET    /loyalty/points      # Points balance
POST   /loyalty/redeem      # Redeem points
GET    /loyalty/tiers       # Loyalty tiers
```

### Auth Controller

```
Route: /api/v1/auth

POST   /register            # Register new user
POST   /login               # Login
POST   /logout              # Logout
POST   /refresh-token       # Refresh access token
POST   /forgot-password     # Request password reset
POST   /reset-password      # Reset password
POST   /change-password     # Change password [Auth]
POST   /confirm-email       # Confirm email
POST   /2fa/enable          # Enable 2FA [Auth]
POST   /2fa/verify          # Verify 2FA code
POST   /2fa/backup-codes    # Generate backup codes [Auth]
```

### Rate Limiting

```csharp
// Rate limit attributes
[RateLimit(60, 60)]   // 60 requests per minute (read operations)
[RateLimit(10, 60)]   // 10 requests per minute (write operations)
[RateLimit(20, 60)]   // 20 requests per minute (update operations)

// Response headers
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 55
X-RateLimit-Reset: 1705500000

// When exceeded
HTTP 429 Too Many Requests
Retry-After: 30
```

### Response Format (RFC 7807 Problem Details)

```json
// Success Response
{
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Electronics",
    "description": "Electronic devices and accessories",
    "slug": "electronics",
    ...
}

// Error Response
{
    "type": "https://api.merge.com/errors/validation-error",
    "title": "Validation Error",
    "status": 400,
    "detail": "One or more validation errors occurred.",
    "instance": "/api/v1/catalog/categories",
    "traceId": "00-abc123...",
    "timestamp": "2026-01-17T12:00:00Z",
    "errors": {
        "Name": ["Name is required"],
        "Slug": ["Slug must be URL-friendly"]
    }
}
```

---

## ALL SERVICES REFERENCE

### Cache Service

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default);
}

// Implementation features:
// - Redis distributed cache
// - Double-check locking (cache stampede protection)
// - Per-key semaphore (thundering herd protection)
// - Default TTL: 1 hour
// - ConfigureAwait(false) for efficiency
```

### Product Services

```csharp
// ProductService
GetProductAsync(Guid productId)
GetProductsByCategoryAsync(Guid categoryId, int page, int pageSize)
CreateProductAsync(CreateProductCommand)
UpdateProductAsync(UpdateProductCommand)
DeleteProductAsync(Guid productId)
SearchProductsAsync(string query, int page, int pageSize)

// ProductRecommendationService
GetRecommendedProductsAsync(Guid userId, int count)
GetSimilarProductsAsync(Guid productId, int count)
GetTrendingProductsAsync(int count)

// ProductComparisonService
CreateComparisonAsync(Guid userId, List<Guid> productIds)
GetComparisonAsync(Guid comparisonId)
AddProductToComparisonAsync(Guid comparisonId, Guid productId)
```

### Order Services

```csharp
// OrderService
CreateOrderAsync(CreateOrderCommand)
GetOrderAsync(Guid orderId)
GetUserOrdersAsync(Guid userId, int page, int pageSize)
UpdateOrderAsync(UpdateOrderCommand)
CancelOrderAsync(Guid orderId, string reason)
ApplyCouponAsync(Guid orderId, string couponCode)
ApplyGiftCardAsync(Guid orderId, Guid giftCardId, decimal amount)
ConfirmOrderAsync(Guid orderId)
ShipOrderAsync(Guid orderId, string trackingNumber)
DeliverOrderAsync(Guid orderId)
RefundOrderAsync(Guid orderId)

// CartService
GetCartAsync(Guid userId)
AddToCartAsync(Guid userId, Guid productId, int quantity)
RemoveFromCartAsync(Guid userId, Guid productId)
UpdateCartItemAsync(Guid userId, Guid productId, int quantity)
ClearCartAsync(Guid userId)
ValidateCartAsync(Guid userId)

// InvoiceService
GenerateInvoiceAsync(Guid orderId)
GetInvoiceAsync(Guid invoiceId)
SendInvoiceEmailAsync(Guid invoiceId, string email)
```

### Payment Services

```csharp
// PaymentService
ProcessPaymentAsync(ProcessPaymentCommand)
RefundPaymentAsync(Guid paymentId)
GetPaymentAsync(Guid paymentId)
VerifyPaymentAsync(Guid paymentId, string transactionId)

// PaymentGateway (Strategy Pattern)
interface IPaymentGateway
{
    Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    Task<RefundResponse> RefundAsync(string transactionId, decimal amount)
    Task<bool> VerifyPaymentAsync(string transactionId)
}
// Implementations: StripeGateway, IyzicoGateway, PayTRGateway

// FraudDetectionService
EvaluateRiskAsync(Payment payment)
CheckFraudRulesAsync(Payment payment)
IsHighRiskAsync(Payment payment)
```

### Seller Services

```csharp
// SellerDashboardService
GetDashboardStatsAsync(Guid sellerId, DateRange dateRange)
GetSalesTrendsAsync(Guid sellerId, DateRange dateRange)
GetTopProductsAsync(Guid sellerId, int count)
GetCategoryPerformanceAsync(Guid sellerId)

// CommissionService
CalculateCommissionAsync(Guid orderId)
GetSellerCommissionsAsync(Guid sellerId, int page)
GetCommissionSettingsAsync(Guid sellerId)
UpdateCommissionSettingsAsync(UpdateCommissionSettingsCommand)

// PayoutService
RequestPayoutAsync(RequestPayoutCommand)
GetAvailableBalanceAsync(Guid sellerId)
ProcessPayoutAsync(ProcessPayoutCommand)
GetPayoutHistoryAsync(Guid sellerId, int page)
```

### Notification Services

```csharp
// EmailService
SendEmailAsync(EmailRequest)
SendTemplateEmailAsync(string templateName, string to, Dictionary<string, string> variables)
SendBulkEmailAsync(List<EmailRequest>)

// PushNotificationService
SendPushNotificationAsync(PushNotificationRequest)
SendBulkPushAsync(List<PushNotificationRequest>)

// SmsService
SendSmsAsync(SmsRequest)
SendOtpAsync(string phoneNumber, string otp)
```

---

## DATABASE & EF CORE

### 12 Specialized DbContexts

| DbContext | Schema | Purpose |
|-----------|--------|---------|
| ApplicationDbContext | identity | Users, Roles, Auth |
| CatalogDbContext | catalog | Products, Categories |
| OrderingDbContext | ordering | Orders, Carts |
| PaymentDbContext | payment | Payments, Subscriptions |
| MarketingDbContext | marketing | Coupons, Campaigns |
| MarketplaceDbContext | marketplace | Stores, Commissions |
| InventoryDbContext | inventory | Stock, Warehouses |
| ContentDbContext | content | Blogs, Pages |
| SupportDbContext | support | Tickets, FAQs |
| NotificationDbContext | notifications | Notifications |
| AnalyticsDbContext | analytics | Reports, Dashboards |
| LiveCommerceDbContext | live | Live Streams |

### Entity Configuration Pattern

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Table
        builder.ToTable("Products", schema: "catalog");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        // Value Object Conversion
        builder.Property(p => p.SKU)
            .HasConversion(sku => sku.Value, value => new SKU(value))
            .HasMaxLength(50)
            .IsRequired();

        // Concurrency Token
        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.SKU).IsUnique();
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.StoreId);

        // Global Query Filter (Soft Delete)
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
```

### Query Optimizations (ALWAYS USE)

```csharp
// ✅ CORRECT - Read-only queries
var products = await context.Products
    .AsNoTracking()                    // No change tracking
    .AsSplitQuery()                    // Separate queries for includes
    .Include(p => p.Category)
    .Include(p => p.Variants)
    .Where(p => p.IsActive)
    .OrderByDescending(p => p.CreatedAt)
    .ToListAsync(ct);

// ❌ WRONG - Tracking enabled, single query cartesian explosion
var products = await context.Products
    .Include(p => p.Category)
    .Include(p => p.Variants)
    .Include(p => p.Reviews)           // Multiple collections = cartesian product!
    .ToListAsync(ct);
```

---

## MIDDLEWARE PIPELINE

### Registration Order (Program.cs)

```csharp
// 1. HTTPS Redirection
app.UseHttpsRedirection();

// 2. Security Headers
app.UseHsts();

// 3. CORS
app.UseCors("Production");

// 4. IP Whitelist (Production)
if (app.Environment.IsProduction())
    app.UseMiddleware<IpWhitelistMiddleware>();

// 5. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6. Rate Limiting
app.UseMiddleware<RateLimitingMiddleware>();

// 7. Idempotency Key
app.UseMiddleware<IdempotencyKeyMiddleware>();

// 8. Global Exception Handler (LAST)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 9. Controllers
app.MapControllers();
```

### Global Exception Handler

```csharp
// Exception → HTTP Status Code mapping
ValidationException      → 400 Bad Request (with errors array)
ArgumentNullException    → 400 Bad Request
ArgumentException        → 400 Bad Request
DomainException         → 400 Bad Request
UnauthorizedAccessException → 401 Unauthorized
NotFoundException       → 404 Not Found
KeyNotFoundException    → 404 Not Found
DbUpdateConcurrencyException → 409 Conflict
InvalidStateTransitionException → 409 Conflict
DbUpdateException       → 400 Bad Request
Exception (unhandled)   → 500 Internal Server Error
```

### Idempotency Key Middleware

```csharp
// Required header for POST/PUT/PATCH/DELETE
Idempotency-Key: {uuid}

// Cached response returned for duplicate requests
// Cache TTL: 24 hours
```

---

## NAMING CONVENTIONS

### C# Naming Standards

| Element | Convention | Example |
|---------|-----------|---------|
| **Class** | PascalCase | `ProductService`, `CreateProductCommand` |
| **Interface** | IPascalCase | `IRepository<T>`, `IProductService` |
| **Method** | PascalCase | `GetProductById()`, `CreateAsync()` |
| **Async Method** | PascalCaseAsync | `GetProductByIdAsync()`, `SaveChangesAsync()` |
| **Property** | PascalCase | `ProductId`, `CreatedAt` |
| **Private Field** | _camelCase | `_repository`, `_domainEvents` |
| **Parameter** | camelCase | `productId`, `cancellationToken` |
| **Constant** | PascalCase | `MaxPageSize`, `DefaultCurrency` |
| **Enum** | PascalCase | `OrderStatus.Completed` |

### CQRS Naming

| Element | Pattern | Example |
|---------|---------|---------|
| **Command** | [Action][Entity]Command | `CreateProductCommand`, `ApplyCouponCommand` |
| **Query** | Get[Entity/List]Query | `GetProductByIdQuery`, `GetAllProductsQuery` |
| **Handler** | [Command/Query]Handler | `CreateProductCommandHandler` |
| **Validator** | [Command/Query]Validator | `CreateProductCommandValidator` |
| **DTO** | [Entity]Dto | `ProductDto`, `CreateProductDto` |
| **Event** | [Entity][PastTense]Event | `ProductCreatedEvent`, `OrderShippedEvent` |
| **Specification** | [Description]Spec | `ActiveProductsSpec`, `OrdersByUserSpec` |

### File Organization

```
Commands/
└── CreateProduct/
    ├── CreateProductCommand.cs
    ├── CreateProductCommandHandler.cs
    └── CreateProductCommandValidator.cs

Queries/
└── GetProductById/
    ├── GetProductByIdQuery.cs
    └── GetProductByIdQueryHandler.cs
```

---

## DEVELOPMENT COMMANDS

### Build & Run

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run API (development - https://localhost:5001)
dotnet run --project Merge.API

# Run with hot reload
dotnet watch run --project Merge.API

# View Swagger: https://localhost:5001/swagger
```

### Database (EF Core + PostgreSQL)

```bash
# Create new migration
dotnet ef migrations add [MigrationName] \
    --project Merge.Infrastructure \
    --startup-project Merge.API

# Apply migrations
dotnet ef database update \
    --project Merge.Infrastructure \
    --startup-project Merge.API

# Remove last migration (not applied)
dotnet ef migrations remove \
    --project Merge.Infrastructure \
    --startup-project Merge.API

# Generate SQL script
dotnet ef migrations script \
    --project Merge.Infrastructure \
    --startup-project Merge.API \
    -o migration.sql

# Reset database (DANGEROUS!)
dotnet ef database drop --force
dotnet ef database update
```

### Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific project
dotnet test Merge.Tests/Merge.Tests.csproj

# Run with filter
dotnet test --filter "FullyQualifiedName~ProductTests"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Docker

```bash
# Build and run all services
docker-compose up -d

# Build only
docker build -t merge-api .

# View logs
docker-compose logs -f api

# Stop all
docker-compose down

# Clean up
docker system prune -af
```

---

## CRITICAL RULES

### ALWAYS DO

1. **Factory Methods**: Use `Entity.Create()` for entity creation
2. **Domain Methods**: Use methods like `product.SetPrice()` instead of `product.Price =`
3. **Specification Pattern**: Use for complex queries
4. **AsNoTracking()**: Use for read-only queries
5. **AsSplitQuery()**: Use with multiple Includes
6. **CancellationToken**: Pass through all async methods
7. **FluentValidation**: Validate all commands
8. **Guard Clauses**: Validate domain invariants
9. **Domain Events**: Raise for significant state changes
10. **IOptions<T>**: Use for configuration
11. **Structured Logging**: `_logger.LogInformation("Created {ProductId}", id)`
12. **ArgumentNullException.ThrowIfNull()**: Use C# 10+ null checks
13. **Primary Constructors**: Use C# 12 syntax for handlers
14. **Record DTOs**: All DTOs should be immutable records
15. **Value Objects**: Use for validated domain values

### NEVER DO

1. **NEVER** let Domain depend on Infrastructure/API
2. **NEVER** expose entities directly in API responses (use DTOs)
3. **NEVER** call SaveChanges() in repository (UnitOfWork only)
4. **NEVER** use `new Entity()` directly (use factory methods)
5. **NEVER** hardcode secrets (use environment variables)
6. **NEVER** commit .env or appsettings.*.json with secrets
7. **NEVER** use `any` type - always explicit types
8. **NEVER** skip validation on commands
9. **NEVER** log sensitive data (tokens, passwords, PII)
10. **NEVER** use string interpolation in logs: `$"User {email}"`
11. **NEVER** use `First()` without null check - use `FirstOrDefault()` + null check
12. **NEVER** use null-forgiving operator `!` without validation
13. **NEVER** use HMACSHA1 - use HMACSHA256
14. **NEVER** log tokens in plaintext
15. **NEVER** return 500 errors without proper handling

---

## SECURITY REQUIREMENTS

### Authentication & Authorization

```csharp
// JWT Configuration
JwtSettings:
  Key: {min 32 chars from ENV}
  Issuer: "MergeECommerce"
  Audience: "MergeECommerceUsers"
  AccessTokenExpirationMinutes: 15
  RefreshTokenExpirationDays: 7

// Always hash tokens
public static string HashToken(string token)
{
    using var sha256 = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(token);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToBase64String(hash);
}

// NEVER log tokens
logger.LogInformation("Token validation attempt"); // ✅
logger.LogInformation("Token: {Token}", token);   // ❌ NEVER!
```

### OWASP Top 10 Compliance

```csharp
// A01: Broken Access Control
[Authorize(Roles = "Admin,Seller")]
[HttpGet("{id}")]
public async Task<IActionResult> GetResource(Guid id)
{
    // Always verify ownership
    if (resource.OwnerId != GetUserId() && !User.IsInRole("Admin"))
        return Forbid();
    ...
}

// A02: Cryptographic Failures
// ✅ CORRECT
using var hmac = new HMACSHA256(keyBytes);
// ❌ WRONG
using var hmac = new HMACSHA1(keyBytes);

// A03: Injection (FluentValidation)
RuleFor(x => x.Name)
    .NotEmpty()
    .MaximumLength(200)
    .Matches(@"^[a-zA-Z0-9\s\-_]+$");

// A05: Security Misconfiguration
// Production CORS
options.AddPolicy("Production", policy =>
{
    policy.WithOrigins(allowedOrigins)
          .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
          .WithHeaders("Content-Type", "Authorization")
          .AllowCredentials();
});
```

### Webhook Signature Validation

```csharp
// Always validate webhook signatures
public bool ValidateWebhookSignature(string payload, string signature, string secret)
{
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
    var expectedSignature = Convert.ToBase64String(hash);

    return CryptographicOperations.FixedTimeEquals(
        Encoding.UTF8.GetBytes(expectedSignature),
        Encoding.UTF8.GetBytes(signature)
    );
}
```

---

## PERFORMANCE GUIDELINES

### Query Optimization

```csharp
// ✅ ALWAYS use these patterns
.AsNoTracking()           // Read-only queries
.AsSplitQuery()           // Multiple includes
.Select(x => new Dto())   // Project only needed fields
.Take(pageSize)           // Limit results
.Skip((page - 1) * size)  // Pagination

// ❌ AVOID
.ToList()                 // Load all into memory
.Include().Include()      // Cartesian explosion without AsSplitQuery
```

### Cache Patterns

```csharp
// Cache with stampede protection
public async Task<T?> GetOrCreateAsync<T>(
    string key,
    Func<Task<T>> factory,
    TimeSpan? expiration = null,
    CancellationToken ct = default)
{
    var cached = await GetAsync<T>(key, ct);
    if (cached != null) return cached;

    // Per-key semaphore prevents thundering herd
    var semaphore = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    await semaphore.WaitAsync(ct);
    try
    {
        // Double-check after acquiring lock
        cached = await GetAsync<T>(key, ct);
        if (cached != null) return cached;

        var value = await factory();
        await SetAsync(key, value, expiration, ct);
        return value;
    }
    finally { semaphore.Release(); }
}

// Cache keys
$"category_{id}"
$"categories_all_paged_{page}_{pageSize}"
$"product_{id}"
$"products_category_{categoryId}_{page}_{pageSize}"
```

### ConfigureAwait Usage

```csharp
// ✅ ALWAYS in library/infrastructure code
await _cache.GetAsync(key, ct).ConfigureAwait(false);
await _repository.AddAsync(entity, ct).ConfigureAwait(false);

// In controllers/handlers: optional (ASP.NET Core handles context)
```

---

## TESTING STANDARDS

### Test Structure

```csharp
// Unit Test Pattern (Arrange-Act-Assert)
[Fact]
public async Task CreateProduct_WithValidData_ShouldReturnProductDto()
{
    // Arrange
    var command = new CreateProductCommand(
        Name: "Test Product",
        Description: "Test Description",
        SKU: "TEST-001",
        Price: 99.99m,
        StockQuantity: 100,
        CategoryId: _categoryId
    );

    _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be(command.Name);
    result.SKU.Should().Be(command.SKU);

    _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}

// Integration Test Pattern
[Fact]
public async Task CreateCategory_ShouldPersistToDatabase()
{
    // Arrange
    await using var context = new CatalogDbContext(_options);
    var command = new CreateCategoryCommand("Electronics", "Electronic devices", "electronics");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    var saved = await context.Categories.FindAsync(result.Id);
    saved.Should().NotBeNull();
    saved!.Name.Should().Be("Electronics");
}
```

### Test Naming Convention

```
MethodName_StateUnderTest_ExpectedBehavior

Examples:
CreateProduct_WithValidData_ShouldReturnProductDto
CreateProduct_WithNullName_ShouldThrowValidationException
ApplyCoupon_WhenExpired_ShouldReturnError
ReduceStock_WhenInsufficientStock_ShouldThrowDomainException
```

---

## CURRENT STATUS & TECH STACK

### Overall Score: 7.6/10

| Category | Score | Status |
|----------|-------|--------|
| Performance & Caching | 9.0/10 | 🟢 Excellent |
| Security (OWASP) | 8.5/10 | 🟢 Good |
| SOLID Principles | 5.0/10 | 🔴 Needs Refactoring |
| Clean Architecture | 9.5/10 | 🟢 Excellent |
| API Design | 7.0/10 | 🟡 Improved |
| Database & EF Core | 8.5/10 | 🟢 Good |
| Error Handling | 7.5/10 | 🟢 Good |
| Logging & Observability | 7.0/10 | 🟢 Good |
| Modern .NET 9 | 8.0/10 | 🟢 Good |
| Code Quality | 6.0/10 | 🟡 Needs Work |
| Testing | 0.8/10 | 🔴 Critical |
| DevOps | 1.5/10 | 🔴 Critical |

### Completed Fixes

- ✅ HMACSHA1 → SHA256 (security)
- ✅ Token logging removed (security)
- ✅ Webhook signature validation
- ✅ Health checks enabled
- ✅ Outbox publisher enabled
- ✅ OpenTelemetry enabled
- ✅ Cache stampede protection
- ✅ CSRF protection
- ✅ HttpClient resilience
- ✅ PATCH endpoints added
- ✅ Idempotency key middleware
- ✅ ETag/Cache-Control headers

### Remaining Critical Issues

1. **Testing Coverage**: 0.8% (target: 60%+)
2. **CI/CD Pipeline**: None
3. **God Classes**: 3 files need splitting
4. **DbContext Consolidation**: 12 → 4

### Tech Stack

| Category | Technology | Version |
|----------|------------|---------|
| **Framework** | .NET | 9.0 |
| **Language** | C# | 12 |
| **ORM** | Entity Framework Core | 9.0 |
| **Database** | PostgreSQL | 16 |
| **Cache** | Redis | 7.x |
| **Mediator** | MediatR | 12.4.1 |
| **Mapping** | AutoMapper | 12.0.1 |
| **Validation** | FluentValidation | 11.9.2 |
| **API Docs** | Swashbuckle | 7.2.0 |
| **Auth** | JWT Bearer | 8.2.1 |
| **Password** | BCrypt.Net-Next | 4.0.3 |
| **Resilience** | Microsoft.Extensions.Http.Resilience | 9.0.0 |
| **Observability** | OpenTelemetry | 1.9.0 |
| **Testing** | xUnit | 2.6.2 |
| **Mocking** | Moq | 4.20.70 |
| **Assertions** | FluentAssertions | 6.12.0 |

---

## KEY FILES REFERENCE

### Domain Core
- `@Merge.Domain/SharedKernel/BaseEntity.cs`
- `@Merge.Domain/SharedKernel/BaseAggregateRoot.cs`
- `@Merge.Domain/SharedKernel/Guard.cs`
- `@Merge.Domain/ValueObjects/Money.cs`
- `@Merge.Domain/ValueObjects/SKU.cs`
- `@Merge.Domain/ValueObjects/Slug.cs`

### Infrastructure Core
- `@Merge.Infrastructure/Data/ApplicationDbContext.cs`
- `@Merge.Infrastructure/Repositories/Repository.cs`
- `@Merge.Infrastructure/UnitOfWork/UnitOfWork.cs`
- `@Merge.Infrastructure/DependencyInjection.cs`

### API Core
- `@Merge.API/Program.cs`
- `@Merge.API/Controllers/BaseController.cs`
- `@Merge.API/Middleware/GlobalExceptionHandlerMiddleware.cs`

### Configuration
- `@Merge.API/appsettings.json`
- `@docker-compose.yml`
- `@Dockerfile`

---

## ADDITIONAL CONTEXT

For detailed modular rules, see:
- `@.claude/rules/code-style.md` - Code formatting & style
- `@.claude/rules/security.md` - Security requirements
- `@.claude/rules/testing.md` - Test conventions
- `@.claude/rules/api-design.md` - REST API patterns
- `@.claude/rules/database.md` - EF Core & PostgreSQL
- `@.claude/rules/cqrs.md` - CQRS patterns
- `@.claude/rules/ddd.md` - Domain-Driven Design
- `@.claude/rules/performance.md` - Performance guidelines

For project architecture analysis, see:
- `@arch.md` - Complete 15-dimension analysis report (MEGA-COMPREHENSIVE)

---

*Bu dosya Claude Code'un projeyi TAM olarak anlaması için hazırlanmıştır.*
*Son Güncelleme: 2026-01-17*
*Analiz Edilen Dosya: 4,262 C# dosyası*
*Toplam Kod Satırı: ~208,800*
