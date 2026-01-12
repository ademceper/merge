namespace Merge.Application.Configuration;

/// <summary>
/// Support/Ticket islemleri icin configuration ayarlari
/// BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (ZORUNLU)
/// </summary>
public class SupportSettings
{
    public const string SectionName = "SupportSettings";

    /// <summary>
    /// Varsayilan istatistik periyodu (gun)
    /// </summary>
    public int DefaultStatsPeriodDays { get; set; } = 30;

    /// <summary>
    /// Haftalik rapor periyodu (gun)
    /// </summary>
    public int WeeklyReportDays { get; set; } = 7;

    /// <summary>
    /// SLA uyari esigi (saat)
    /// </summary>
    public int SlaWarningThresholdHours { get; set; } = 24;

    /// <summary>
    /// Pagination için maksimum sayfa boyutu
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Varsayılan sayfa boyutu
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;

    /// <summary>
    /// Ticket overdue threshold (gün) - 3 günden eski ticket'lar overdue sayılır
    /// </summary>
    public int TicketOverdueDays { get; set; } = 3;

    /// <summary>
    /// Live chat session için maksimum mesaj sayısı (recent messages)
    /// </summary>
    public int MaxRecentChatMessages { get; set; } = 50;

    /// <summary>
    /// Dashboard için gösterilecek recent ticket sayısı
    /// </summary>
    public int DashboardRecentTicketsCount { get; set; } = 10;

    /// <summary>
    /// Dashboard için gösterilecek urgent ticket sayısı
    /// </summary>
    public int DashboardUrgentTicketsCount { get; set; } = 10;

    /// <summary>
    /// Ticket number format prefix
    /// </summary>
    public string TicketNumberPrefix { get; set; } = "TKT-";

    /// <summary>
    /// Ticket number format padding (digit count)
    /// </summary>
    public int TicketNumberPadding { get; set; } = 6;

    /// <summary>
    /// Live chat session ID format prefix
    /// </summary>
    public string ChatSessionIdPrefix { get; set; } = "CHAT-";

    /// <summary>
    /// Live chat session ID format date format
    /// </summary>
    public string ChatSessionIdDateFormat { get; set; } = "yyyyMMdd";

    /// <summary>
    /// Live chat session ID format GUID substring length
    /// </summary>
    public int ChatSessionIdGuidLength { get; set; } = 8;

    /// <summary>
    /// Knowledge base article için maksimum title uzunluğu
    /// </summary>
    public int MaxArticleTitleLength { get; set; } = 200;

    /// <summary>
    /// Knowledge base article için maksimum content uzunluğu
    /// </summary>
    public int MaxArticleContentLength { get; set; } = 50000;

    /// <summary>
    /// Knowledge base article için maksimum excerpt uzunluğu
    /// </summary>
    public int MaxArticleExcerptLength { get; set; } = 500;

    /// <summary>
    /// FAQ için maksimum question uzunluğu
    /// </summary>
    public int MaxFaqQuestionLength { get; set; } = 500;

    /// <summary>
    /// FAQ için maksimum answer uzunluğu
    /// </summary>
    public int MaxFaqAnswerLength { get; set; } = 5000;

    /// <summary>
    /// Ticket message için maksimum message uzunluğu
    /// </summary>
    public int MaxTicketMessageLength { get; set; } = 10000;

    /// <summary>
    /// Live chat message için maksimum content uzunluğu
    /// </summary>
    public int MaxChatMessageLength { get; set; } = 10000;

    /// <summary>
    /// Customer communication için maksimum subject uzunluğu
    /// </summary>
    public int MaxCommunicationSubjectLength { get; set; } = 200;

    /// <summary>
    /// Customer communication için maksimum content uzunluğu
    /// </summary>
    public int MaxCommunicationContentLength { get; set; } = 10000;

    /// <summary>
    /// Ticket için maksimum subject uzunluğu
    /// </summary>
    public int MaxTicketSubjectLength { get; set; } = 200;

    /// <summary>
    /// Ticket için maksimum description uzunluğu
    /// </summary>
    public int MaxTicketDescriptionLength { get; set; } = 5000;

    /// <summary>
    /// Ticket attachment için maksimum file name uzunluğu
    /// </summary>
    public int MaxAttachmentFileNameLength { get; set; } = 255;

    /// <summary>
    /// Knowledge base article için maksimum slug uzunluğu
    /// </summary>
    public int MaxArticleSlugLength { get; set; } = 200;

    /// <summary>
    /// Knowledge base category için maksimum name uzunluğu
    /// </summary>
    public int MaxCategoryNameLength { get; set; } = 100;

    /// <summary>
    /// Knowledge base category için maksimum slug uzunluğu
    /// </summary>
    public int MaxCategorySlugLength { get; set; } = 100;

    /// <summary>
    /// Knowledge base category için maksimum description uzunluğu
    /// </summary>
    public int MaxCategoryDescriptionLength { get; set; } = 1000;

    /// <summary>
    /// FAQ için maksimum category uzunluğu
    /// </summary>
    public int MaxFaqCategoryLength { get; set; } = 50;

    /// <summary>
    /// Live chat message için maksimum content uzunluğu (alias for MaxChatMessageLength)
    /// </summary>
    public int MaxLiveChatMessageLength { get; set; } = 10000;

    // Validator için minimum/maksimum değerler
    /// <summary>
    /// Minimum mesaj içeriği uzunluğu
    /// </summary>
    public int MinMessageContentLength { get; set; } = 1;

    /// <summary>
    /// Minimum soru uzunluğu (FAQ)
    /// </summary>
    public int MinFaqQuestionLength { get; set; } = 5;

    /// <summary>
    /// Minimum cevap uzunluğu (FAQ)
    /// </summary>
    public int MinFaqAnswerLength { get; set; } = 5;

    /// <summary>
    /// Minimum kategori adı uzunluğu
    /// </summary>
    public int MinCategoryNameLength { get; set; } = 2;

    /// <summary>
    /// Minimum başlık uzunluğu (Article)
    /// </summary>
    public int MinArticleTitleLength { get; set; } = 2;

    /// <summary>
    /// Minimum içerik uzunluğu (Article)
    /// </summary>
    public int MinArticleContentLength { get; set; } = 10;

    /// <summary>
    /// Minimum konu uzunluğu (Ticket)
    /// </summary>
    public int MinTicketSubjectLength { get; set; } = 5;

    /// <summary>
    /// Minimum açıklama uzunluğu (Ticket)
    /// </summary>
    public int MinTicketDescriptionLength { get; set; } = 10;

    /// <summary>
    /// Minimum misafir adı uzunluğu (Live Chat)
    /// </summary>
    public int MinGuestNameLength { get; set; } = 2;

    /// <summary>
    /// Maksimum misafir adı uzunluğu (Live Chat)
    /// </summary>
    public int MaxGuestNameLength { get; set; } = 100;

    /// <summary>
    /// Maksimum e-posta adresi uzunluğu
    /// </summary>
    public int MaxEmailLength { get; set; } = 200;

    /// <summary>
    /// Maksimum telefon numarası uzunluğu
    /// </summary>
    public int MaxPhoneNumberLength { get; set; } = 20;

    /// <summary>
    /// Maksimum departman uzunluğu
    /// </summary>
    public int MaxDepartmentLength { get; set; } = 50;

    /// <summary>
    /// Maksimum iletişim tipi uzunluğu
    /// </summary>
    public int MaxCommunicationTypeLength { get; set; } = 100;

    /// <summary>
    /// Maksimum kanal uzunluğu
    /// </summary>
    public int MaxChannelLength { get; set; } = 50;

    /// <summary>
    /// Maksimum mesaj tipi uzunluğu
    /// </summary>
    public int MaxMessageTypeLength { get; set; } = 50;

    /// <summary>
    /// Maksimum kategori enum uzunluğu
    /// </summary>
    public int MaxCategoryEnumLength { get; set; } = 50;

    /// <summary>
    /// Maksimum öncelik enum uzunluğu
    /// </summary>
    public int MaxPriorityEnumLength { get; set; } = 20;

    /// <summary>
    /// Maksimum IP adresi uzunluğu
    /// </summary>
    public int MaxIpAddressLength { get; set; } = 45;

    /// <summary>
    /// Maksimum User Agent uzunluğu
    /// </summary>
    public int MaxUserAgentLength { get; set; } = 500;

    /// <summary>
    /// Maksimum Icon URL uzunluğu
    /// </summary>
    public int MaxIconUrlLength { get; set; } = 500;

    /// <summary>
    /// Minimum görüntüleme sırası değeri
    /// </summary>
    public int MinDisplayOrder { get; set; } = 0;
}
