using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public static class BasicInfo
    {
        public  const string EAPlayID = "B0HFJ7PW900M";
        public  const string GamePassID = "9WNZS2ZC9L74";
    }
    public class Remediation
    {
        public string RemediationId { get; set; }
        public string Description { get; set; }
    }

    public class Affirmation
    {
        public string AffirmationId { get; set; }
        public string AffirmationProductId { get; set; }
        public string Description { get; set; }
    }

    public class EligibilityProperties
    {
        public List<Remediation> Remediations { get; set; }
        public List<Affirmation> Affirmations { get; set; }
    }

    public class Image
    {
        public string FileId { get; set; }
        public object EISListingIdentifier { get; set; }
        public string BackgroundColor { get; set; }
        public string Caption { get; set; }
        public int FileSizeInBytes { get; set; }
        public string ForegroundColor { get; set; }
        public int Height { get; set; }
        public string ImagePositionInfo { get; set; }
        public string ImagePurpose { get; set; }
        public string UnscaledImageSHA256Hash { get; set; }
        public string Uri { get; set; }
        public int Width { get; set; }
    }

    public class PreviewImage
    {
        public string FileId { get; set; }
        public object EISListingIdentifier { get; set; }
        public object BackgroundColor { get; set; }
        public string Caption { get; set; }
        public int FileSizeInBytes { get; set; }
        public object ForegroundColor { get; set; }
        public int Height { get; set; }
        public object ImagePositionInfo { get; set; }
        public string ImagePurpose { get; set; }
        public string UnscaledImageSHA256Hash { get; set; }
        public string Uri { get; set; }
        public int Width { get; set; }
    }

    public class Video
    {
        public string Uri { get; set; }
        public string VideoPurpose { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string AudioEncoding { get; set; }
        public string VideoEncoding { get; set; }
        public string VideoPositionInfo { get; set; }
        public string Caption { get; set; }
        public int FileSizeInBytes { get; set; }
        public PreviewImage PreviewImage { get; set; }
        public int SortOrder { get; set; }
    }

    public class SearchTitle
    {
        public string SearchTitleString { get; set; }
        public string SearchTitleType { get; set; }
    }

    public class LocalizedProperty
    {
        public string DeveloperName { get; set; }
        public string PublisherName { get; set; }
        public string PublisherWebsiteUri { get; set; }
        public string SupportUri { get; set; }
        public EligibilityProperties EligibilityProperties { get; set; }
        public List<object> Franchises { get; set; }
        public List<Image> Images { get; set; }
        public List<Video> Videos { get; set; }
        public string ProductDescription { get; set; }
        public string ProductTitle { get; set; }
        public string ShortTitle { get; set; }
        public string SortTitle { get; set; }
        public object FriendlyTitle { get; set; }
        public string ShortDescription { get; set; }
        public List<SearchTitle> SearchTitles { get; set; }
        public string VoiceTitle { get; set; }
        public object RenderGroupDetails { get; set; }
        public List<object> ProductDisplayRanks { get; set; }
        public object InteractiveModelConfig { get; set; }
        public bool Interactive3DEnabled { get; set; }
        public string Language { get; set; }
        public List<string> Markets { get; set; }
    }

    public class ContentRating
    {
        public string RatingSystem { get; set; }
        public string RatingId { get; set; }
        public List<string> RatingDescriptors { get; set; }
        public List<object> RatingDisclaimers { get; set; }
        public List<string> InteractiveElements { get; set; }
    }

    public class RelatedProduct
    {
        public string RelatedProductId { get; set; }
        public string RelationshipType { get; set; }
    }

    public class UsageData
    {
        public string AggregateTimeSpan { get; set; }
        public double AverageRating { get; set; }
        public double PlayCount { get; set; }
        public int RatingCount { get; set; }
        public string RentalCount { get; set; }
        public string TrialCount { get; set; }
        public string PurchaseCount { get; set; }
    }

    public class MarketProperty
    {
        public DateTime OriginalReleaseDate { get; set; }
        public int MinimumUserAge { get; set; }
        public List<ContentRating> ContentRatings { get; set; }
        public List<RelatedProduct> RelatedProducts { get; set; }
        public List<UsageData> UsageData { get; set; }
        public object BundleConfig { get; set; }
        public List<string> Markets { get; set; }
    }

    public class Attribute
    {
        public string Name { get; set; }
        public int? Minimum { get; set; }
        public int? Maximum { get; set; }
        public List<string> ApplicablePlatforms { get; set; }
        public object Group { get; set; }
    }

    public class SkuDisplayGroup
    {
        public string Id { get; set; }
        public string Treatment { get; set; }
    }

    public class Properties
    {
        public List<Attribute> Attributes { get; set; }
        public bool CanInstallToSDCard { get; set; }
        public string Category { get; set; }
        public List<string> Categories { get; set; }
        public object Subcategory { get; set; }
        public bool IsAccessible { get; set; }
        public bool IsDemo { get; set; }
        public bool IsLineOfBusinessApp { get; set; }
        public bool IsPublishedToLegacyWindowsPhoneStore { get; set; }
        public bool IsPublishedToLegacyWindowsStore { get; set; }
        public string PackageFamilyName { get; set; }
        public string PackageIdentityName { get; set; }
        public string PublisherCertificateName { get; set; }
        public string PublisherId { get; set; }
        public List<SkuDisplayGroup> SkuDisplayGroups { get; set; }
        public string XboxLiveTier { get; set; }
        public object XboxXPA { get; set; }
        public object XboxCrossGenSetId { get; set; }
        public List<string> XboxConsoleGenOptimized { get; set; }
        public List<string> XboxConsoleGenCompatible { get; set; }
        public bool XboxLiveGoldRequired { get; set; }
        public object OwnershipType { get; set; }
        public string PdpBackgroundColor { get; set; }
        public bool? HasAddOns { get; set; }
        public DateTime RevisionId { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }
    }

    public class AlternateId
    {
        public string IdType { get; set; }
        public string Value { get; set; }
    }

    public class ValidationData
    {
        public bool PassedValidation { get; set; }
        public string RevisionId { get; set; }
        public string ValidationResultUri { get; set; }
    }

    public class LegalText
    {
        public string AdditionalLicenseTerms { get; set; }
        public string Copyright { get; set; }
        public string CopyrightUri { get; set; }
        public string PrivacyPolicy { get; set; }
        public string PrivacyPolicyUri { get; set; }
        public string Tou { get; set; }
        public string TouUri { get; set; }
    }

    public class LocalizedProperty2
    {
        public List<object> Contributors { get; set; }
        public List<string> Features { get; set; }
        public string MinimumNotes { get; set; }
        public string RecommendedNotes { get; set; }
        public string ReleaseNotes { get; set; }
        public object DisplayPlatformProperties { get; set; }
        public string SkuDescription { get; set; }
        public string SkuTitle { get; set; }
        public string SkuButtonTitle { get; set; }
        public object DeliveryDateOverlay { get; set; }
        public List<object> SkuDisplayRank { get; set; }
        public object TextResources { get; set; }
        public List<object> Images { get; set; }
        public LegalText LegalText { get; set; }
        public string Language { get; set; }
        public List<string> Markets { get; set; }
    }

    public class MarketProperty2
    {
        public DateTime? FirstAvailableDate { get; set; }
        public List<string> SupportedLanguages { get; set; }
        public object PackageIds { get; set; }
        public object PIFilter { get; set; }
        public List<string> Markets { get; set; }
    }

    public class FulfillmentData
    {
        public string ProductId { get; set; }
        public string WuBundleId { get; set; }
        public string WuCategoryId { get; set; }
        public string PackageFamilyName { get; set; }
        public string SkuId { get; set; }
        public object Content { get; set; }
        public object PackageFeatures { get; set; }
    }

    public class HardwareProperties
    {
        public List<string> MinimumHardware { get; set; }
        public List<string> RecommendedHardware { get; set; }
        public string MinimumProcessor { get; set; }
        public string RecommendedProcessor { get; set; }
        public string MinimumGraphics { get; set; }
        public string RecommendedGraphics { get; set; }
    }

    public class Application
    {
        public string ApplicationId { get; set; }
        public int DeclarationOrder { get; set; }
        public List<string> Extensions { get; set; }
    }

    public class FrameworkDependency
    {
        public int MaxTested { get; set; }
        public object MinVersion { get; set; }
        public string PackageIdentity { get; set; }
    }

    public class PlatformDependency
    {
        public object MaxTested { get; set; }
        public object MinVersion { get; set; }
        public string PlatformName { get; set; }
    }

    public class PackageDownloadUri
    {
        public int Rank { get; set; }
        public string Uri { get; set; }
    }

    public class FulfillmentData2
    {
        public string ProductId { get; set; }
        public string WuBundleId { get; set; }
        public string WuCategoryId { get; set; }
        public string PackageFamilyName { get; set; }
        public string SkuId { get; set; }
        public object Content { get; set; }
        public object PackageFeatures { get; set; }
    }

    public class Package
    {
        public List<Application> Applications { get; set; }
        public List<string> Architectures { get; set; }
        public List<string> Capabilities { get; set; }
        public List<object> DeviceCapabilities { get; set; }
        public List<object> ExperienceIds { get; set; }
        public List<FrameworkDependency> FrameworkDependencies { get; set; }
        public List<object> HardwareDependencies { get; set; }
        public List<object> HardwareRequirements { get; set; }
        public string Hash { get; set; }
        public string HashAlgorithm { get; set; }
        public bool IsStreamingApp { get; set; }
        public List<string> Languages { get; set; }
        public object MaxDownloadSizeInBytes { get; set; }
        public object MaxInstallSizeInBytes { get; set; }
        public string PackageFormat { get; set; }
        public string PackageFamilyName { get; set; }
        public object MainPackageFamilyNameForDlc { get; set; }
        public string PackageFullName { get; set; }
        public string PackageId { get; set; }
        public string ContentId { get; set; }
        public string KeyId { get; set; }
        public int PackageRank { get; set; }
        public string PackageUri { get; set; }
        public List<PlatformDependency> PlatformDependencies { get; set; }
        public string PlatformDependencyXmlBlob { get; set; }
        public string ResourceId { get; set; }
        public string Version { get; set; }
        public List<PackageDownloadUri> PackageDownloadUris { get; set; }
        public List<object> DriverDependencies { get; set; }
        public FulfillmentData2 FulfillmentData { get; set; }
    }

    public class Properties2
    {
        public object EarlyAdopterEnrollmentUrl { get; set; }
        public FulfillmentData FulfillmentData { get; set; }
        public string FulfillmentType { get; set; }
        public object FulfillmentPluginId { get; set; }
        public bool HasThirdPartyIAPs { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public HardwareProperties HardwareProperties { get; set; }
        public List<object> HardwareRequirements { get; set; }
        public List<string> HardwareWarningList { get; set; }
        public string InstallationTerms { get; set; }
        public List<Package> Packages { get; set; }
        public string VersionString { get; set; }
        public List<string> SkuDisplayGroupIds { get; set; }
        public bool XboxXPA { get; set; }
        public List<object> BundledSkus { get; set; }
        public bool IsRepurchasable { get; set; }
        public int SkuDisplayRank { get; set; }
        public object DisplayPhysicalStoreInventory { get; set; }
        public List<object> VisibleToB2BServiceIds { get; set; }
        public List<object> AdditionalIdentifiers { get; set; }
        public bool IsTrial { get; set; }
        public bool IsPreOrder { get; set; }
        public bool IsBundle { get; set; }
    }

    public class Sku
    {
        public DateTime LastModifiedDate { get; set; }
        public List<LocalizedProperty2> LocalizedProperties { get; set; }
        public List<MarketProperty2> MarketProperties { get; set; }
        public string ProductId { get; set; }
        public Properties2 Properties { get; set; }
        public string SkuASchema { get; set; }
        public string SkuBSchema { get; set; }
        public string SkuId { get; set; }
        public string SkuType { get; set; }
        public object RecurrencePolicy { get; set; }
        public object SubscriptionPolicyId { get; set; }
    }

    public class AllowedPlatform
    {
        public object MaxVersion { get; set; }
        public int MinVersion { get; set; }
        public string PlatformName { get; set; }
    }

    public class ClientConditions
    {
        public List<AllowedPlatform> AllowedPlatforms { get; set; }
    }

    public class Conditions
    {
        public ClientConditions ClientConditions { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> ResourceSetIds { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class PIFilter
    {
        public List<object> ExclusionProperties { get; set; }
        public List<object> InclusionProperties { get; set; }
    }

    public class Price
    {
        public string CurrencyCode { get; set; }
        public bool IsPIRequired { get; set; }
        public double ListPrice { get; set; }
        public double MSRP { get; set; }
        public string TaxType { get; set; }
        public string WholesaleCurrencyCode { get; set; }
        public double WholesalePrice { get; set; }
    }

    public class OrderManagementData
    {
        public List<object> GrantedEntitlementKeys { get; set; }
        public PIFilter PIFilter { get; set; }
        public Price Price { get; set; }
        public string OrderManagementPolicyIdOverride { get; set; }
        public string GeofencingPolicyId { get; set; }
    }

    public class Properties3
    {
        public DateTime OriginalReleaseDate { get; set; }
    }

    public class AlternateId2
    {
        public string IdType { get; set; }
        public string Value { get; set; }
    }

    public class SatisfyingEntitlementKey
    {
        public List<string> EntitlementKeys { get; set; }
        public List<string> LicensingKeyIds { get; set; }
        public DateTime? PreOrderReleaseDate { get; set; }
    }

    public class LicensingData
    {
        public List<SatisfyingEntitlementKey> SatisfyingEntitlementKeys { get; set; }
    }

    public class Remediation2
    {
        public string RemediationId { get; set; }
        public string Type { get; set; }
        public string BigId { get; set; }
    }

    public class Availability
    {
        public List<string> Actions { get; set; }
        public string AvailabilityASchema { get; set; }
        public string AvailabilityBSchema { get; set; }
        public string AvailabilityId { get; set; }
        public Conditions Conditions { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public List<string> Markets { get; set; }
        public OrderManagementData OrderManagementData { get; set; }
        public Properties3 Properties { get; set; }
        public string SkuId { get; set; }
        public int DisplayRank { get; set; }
        public List<AlternateId2> AlternateIds { get; set; }
        public bool RemediationRequired { get; set; }
        public LicensingData LicensingData { get; set; }
        public List<Remediation2> Remediations { get; set; }
        public string AffirmationId { get; set; }
    }

    public class DisplaySkuAvailability
    {
        public Sku Sku { get; set; }
        public List<Availability> Availabilities { get; set; }
    }

    public class Product
    {
        public DateTime LastModifiedDate { get; set; }
        public List<LocalizedProperty> LocalizedProperties { get; set; }
        public List<MarketProperty> MarketProperties { get; set; }
        public string ProductASchema { get; set; }
        public string ProductBSchema { get; set; }
        public string ProductId { get; set; }
        public Properties Properties { get; set; }
        public List<AlternateId> AlternateIds { get; set; }
        public object DomainDataVersion { get; set; }
        public string IngestionSource { get; set; }
        public bool IsMicrosoftProduct { get; set; }
        public string PreferredSkuId { get; set; }
        public string ProductType { get; set; }
        public ValidationData ValidationData { get; set; }
        public List<object> MerchandizingTags { get; set; }
        public string PartD { get; set; }
        public string SandboxId { get; set; }
        public string ProductFamily { get; set; }
        public string SchemaVersion { get; set; }
        public bool IsSandboxedProduct { get; set; }
        public string ProductKind { get; set; }
        public List<DisplaySkuAvailability> DisplaySkuAvailabilities { get; set; }
    }

    public class ProductsModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
