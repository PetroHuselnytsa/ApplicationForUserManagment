using Microsoft.EntityFrameworkCore;
using TestFirstProject.Configurations;
using TestFirstProject.Models;

namespace TestFirstProject.Contexts
{
    public class PersonsContext : DbContext
    {
        public DbSet<Person> Persons { get; set; } = null!;

        // Messaging & notification entities
        public DbSet<AppUser> AppUsers { get; set; } = null!;
        public DbSet<Conversation> Conversations { get; set; } = null!;
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;

        // E-Commerce entities
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
        public DbSet<ProductReview> ProductReviews { get; set; } = null!;
        public DbSet<PriceHistory> PriceHistories { get; set; } = null!;
        public DbSet<Warehouse> Warehouses { get; set; } = null!;
        public DbSet<StockEntry> StockEntries { get; set; } = null!;
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;
        public DbSet<RestockRequest> RestockRequests { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<PaymentAttempt> PaymentAttempts { get; set; } = null!;
        public DbSet<Wallet> Wallets { get; set; } = null!;
        public DbSet<Shipment> Shipments { get; set; } = null!;
        public DbSet<Coupon> Coupons { get; set; } = null!;
        public DbSet<CouponUsage> CouponUsages { get; set; } = null!;
        public DbSet<FlashSale> FlashSales { get; set; } = null!;
        public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; } = null!;
        public DbSet<ShippingZone> ShippingZones { get; set; } = null!;
        public DbSet<BundleDeal> BundleDeals { get; set; } = null!;

        public PersonsContext(DbContextOptions<PersonsContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Existing entity configuration
            modelBuilder.ApplyConfiguration(new PersonConfiguration());

            // Messaging & notification configurations
            modelBuilder.ApplyConfiguration(new AppUserConfiguration());
            modelBuilder.ApplyConfiguration(new ConversationConfiguration());
            modelBuilder.ApplyConfiguration(new ConversationParticipantConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationPreferenceConfiguration());

            // E-Commerce configurations
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new EcomProductConfiguration());
            modelBuilder.ApplyConfiguration(new ProductVariantConfiguration());
            modelBuilder.ApplyConfiguration(new ProductReviewConfiguration());
            modelBuilder.ApplyConfiguration(new PriceHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new WarehouseConfiguration());
            modelBuilder.ApplyConfiguration(new StockEntryConfiguration());
            modelBuilder.ApplyConfiguration(new InventoryTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new RestockRequestConfiguration());
            modelBuilder.ApplyConfiguration(new CartConfiguration());
            modelBuilder.ApplyConfiguration(new CartItemConfiguration());
            modelBuilder.ApplyConfiguration(new OrderConfiguration());
            modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
            modelBuilder.ApplyConfiguration(new OrderStatusHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentAttemptConfiguration());
            modelBuilder.ApplyConfiguration(new WalletConfiguration());
            modelBuilder.ApplyConfiguration(new ShipmentConfiguration());
            modelBuilder.ApplyConfiguration(new CouponConfiguration());
            modelBuilder.ApplyConfiguration(new CouponUsageConfiguration());
            modelBuilder.ApplyConfiguration(new FlashSaleConfiguration());
            modelBuilder.ApplyConfiguration(new LoyaltyTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new ShippingZoneConfiguration());
            modelBuilder.ApplyConfiguration(new BundleDealConfiguration());
        }
    }
}
