using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace TestFirstProject.Migrations
{
    /// <inheritdoc />
    public partial class AddEcommerceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // === Independent tables (no FKs to other new tables) ===

            // --- categories table ---
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    parent_category_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_categories_categories_parent_category_id",
                        column: x => x.parent_category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_categories_name", table: "categories", column: "name");
            migrationBuilder.CreateIndex(name: "IX_categories_parent_category_id", table: "categories", column: "parent_category_id");

            // --- warehouses table ---
            migrationBuilder.CreateTable(
                name: "warehouses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouses", x => x.id);
                });

            // --- coupons table ---
            migrationBuilder.CreateTable(
                name: "coupons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    min_order_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    max_uses = table.Column<int>(type: "integer", nullable: true),
                    current_uses = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupons", x => x.id);
                });

            migrationBuilder.CreateIndex(name: "IX_coupons_code", table: "coupons", column: "code", unique: true);
            migrationBuilder.CreateIndex(name: "IX_coupons_is_active", table: "coupons", column: "is_active");

            // --- wallets table ---
            migrationBuilder.CreateTable(
                name: "wallets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallets", x => x.id);
                    table.ForeignKey(
                        name: "FK_wallets_auth_users_user_id",
                        column: x => x.user_id,
                        principalTable: "auth_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_wallets_user_id", table: "wallets", column: "user_id", unique: true);

            // --- loyalty_transactions table ---
            migrationBuilder.CreateTable(
                name: "loyalty_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    points = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loyalty_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_loyalty_transactions_auth_users_user_id",
                        column: x => x.user_id,
                        principalTable: "auth_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_loyalty_transactions_user_id", table: "loyalty_transactions", column: "user_id");
            migrationBuilder.CreateIndex(name: "IX_loyalty_transactions_order_id", table: "loyalty_transactions", column: "order_id");

            // === Tables dependent on categories ===

            // --- products table ---
            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    base_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    weight = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_urls = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_products_sku", table: "products", column: "sku", unique: true);
            migrationBuilder.CreateIndex(name: "IX_products_category_id", table: "products", column: "category_id");
            migrationBuilder.CreateIndex(name: "IX_products_base_price", table: "products", column: "base_price");
            migrationBuilder.CreateIndex(name: "IX_products_is_active", table: "products", column: "is_active");
            migrationBuilder.CreateIndex(name: "IX_products_is_active_category_id_base_price", table: "products", columns: new[] { "is_active", "category_id", "base_price" });

            // --- product_variants table ---
            migrationBuilder.CreateTable(
                name: "product_variants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    size = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    color = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    material = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    price_delta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_variants", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_variants_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_product_variants_sku", table: "product_variants", column: "sku", unique: true);
            migrationBuilder.CreateIndex(name: "IX_product_variants_product_id", table: "product_variants", column: "product_id");

            // --- price_history table ---
            migrationBuilder.CreateTable(
                name: "price_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    new_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    changed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_price_history_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_price_history_product_id", table: "price_history", column: "product_id");
            migrationBuilder.CreateIndex(name: "IX_price_history_changed_at", table: "price_history", column: "changed_at");

            // --- product_reviews table ---
            migrationBuilder.CreateTable(
                name: "product_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    review_text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_verified_purchase = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_reviews_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_reviews_auth_users_user_id",
                        column: x => x.user_id,
                        principalTable: "auth_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_product_reviews_product_id_user_id", table: "product_reviews", columns: new[] { "product_id", "user_id" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_product_reviews_product_id", table: "product_reviews", column: "product_id");

            // --- flash_sales table ---
            migrationBuilder.CreateTable(
                name: "flash_sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sale_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flash_sales", x => x.id);
                    table.ForeignKey(
                        name: "FK_flash_sales_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_flash_sales_product_id", table: "flash_sales", column: "product_id");
            migrationBuilder.CreateIndex(name: "IX_flash_sales_is_active_start_time_end_time", table: "flash_sales", columns: new[] { "is_active", "start_time", "end_time" });

            // --- stock_entries table ---
            migrationBuilder.CreateTable(
                name: "stock_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity_on_hand = table.Column<int>(type: "integer", nullable: false),
                    quantity_reserved = table.Column<int>(type: "integer", nullable: false),
                    low_stock_threshold = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_entries_product_variants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stock_entries_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_stock_entries_product_variant_id_warehouse_id", table: "stock_entries", columns: new[] { "product_variant_id", "warehouse_id" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_stock_entries_warehouse_id", table: "stock_entries", column: "warehouse_id");

            // --- inventory_transactions table ---
            migrationBuilder.CreateTable(
                name: "inventory_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    quantity_change = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    performed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_inventory_transactions_stock_entries_stock_entry_id",
                        column: x => x.stock_entry_id,
                        principalTable: "stock_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_inventory_transactions_stock_entry_id", table: "inventory_transactions", column: "stock_entry_id");
            migrationBuilder.CreateIndex(name: "IX_inventory_transactions_order_id", table: "inventory_transactions", column: "order_id");
            migrationBuilder.CreateIndex(name: "IX_inventory_transactions_created_at", table: "inventory_transactions", column: "created_at");

            // --- restock_requests table ---
            migrationBuilder.CreateTable(
                name: "restock_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_quantity = table.Column<int>(type: "integer", nullable: false),
                    fulfilled_quantity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fulfilled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restock_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_restock_requests_product_variants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_restock_requests_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_restock_requests_status", table: "restock_requests", column: "status");

            // --- carts table ---
            migrationBuilder.CreateTable(
                name: "carts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    applied_coupon_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_activity_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carts", x => x.id);
                    table.ForeignKey(
                        name: "FK_carts_auth_users_user_id",
                        column: x => x.user_id,
                        principalTable: "auth_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_carts_coupons_applied_coupon_id",
                        column: x => x.applied_coupon_id,
                        principalTable: "coupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(name: "IX_carts_user_id", table: "carts", column: "user_id", unique: true);
            migrationBuilder.CreateIndex(name: "IX_carts_last_activity_at", table: "carts", column: "last_activity_at");

            // --- cart_items table ---
            migrationBuilder.CreateTable(
                name: "cart_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cart_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cart_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_cart_items_carts_cart_id",
                        column: x => x.cart_id,
                        principalTable: "carts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cart_items_product_variants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_cart_items_cart_id_product_variant_id", table: "cart_items", columns: new[] { "cart_id", "product_variant_id" }, unique: true);

            // --- orders table ---
            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    shipping_address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    billing_address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    sub_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    shipping_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    shipping_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    coupon_id = table.Column<Guid>(type: "uuid", nullable: true),
                    loyalty_points_used = table.Column<int>(type: "integer", nullable: false),
                    loyalty_discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    row_version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_orders_auth_users_user_id",
                        column: x => x.user_id,
                        principalTable: "auth_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_coupons_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "coupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(name: "IX_orders_user_id", table: "orders", column: "user_id");
            migrationBuilder.CreateIndex(name: "IX_orders_status", table: "orders", column: "status");
            migrationBuilder.CreateIndex(name: "IX_orders_created_at", table: "orders", column: "created_at");
            migrationBuilder.CreateIndex(name: "IX_orders_user_id_created_at", table: "orders", columns: new[] { "user_id", "created_at" });

            // --- order_items table ---
            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    variant_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_items_product_variants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_order_items_order_id", table: "order_items", column: "order_id");

            // --- order_status_history table ---
            migrationBuilder.CreateTable(
                name: "order_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    changed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_status_history_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_order_status_history_order_id", table: "order_status_history", column: "order_id");
            migrationBuilder.CreateIndex(name: "IX_order_status_history_changed_at", table: "order_status_history", column: "changed_at");

            // --- payments table ---
            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    refunded_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    transaction_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    captured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_payments_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_payments_order_id", table: "payments", column: "order_id");
            migrationBuilder.CreateIndex(name: "IX_payments_status", table: "payments", column: "status");

            // --- payment_attempts table ---
            migrationBuilder.CreateTable(
                name: "payment_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_successful = table.Column<bool>(type: "boolean", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    gateway_response = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    attempted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_attempts", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_attempts_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_payment_attempts_payment_id", table: "payment_attempts", column: "payment_id");

            // --- shipments table ---
            migrationBuilder.CreateTable(
                name: "shipments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tracking_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    carrier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shipping_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    weight_kg = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    distance_zone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    shipped_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estimated_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipments", x => x.id);
                    table.ForeignKey(
                        name: "FK_shipments_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_shipments_order_id", table: "shipments", column: "order_id", unique: true);
            migrationBuilder.CreateIndex(name: "IX_shipments_tracking_number", table: "shipments", column: "tracking_number");

            // --- coupon_usages table ---
            migrationBuilder.CreateTable(
                name: "coupon_usages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    coupon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupon_usages", x => x.id);
                    table.ForeignKey(
                        name: "FK_coupon_usages_coupons_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "coupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_coupon_usages_coupon_id_user_id", table: "coupon_usages", columns: new[] { "coupon_id", "user_id" });
            migrationBuilder.CreateIndex(name: "IX_coupon_usages_order_id", table: "coupon_usages", column: "order_id");

            // === Seed Data ===
            SeedData(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "coupon_usages");
            migrationBuilder.DropTable(name: "shipments");
            migrationBuilder.DropTable(name: "payment_attempts");
            migrationBuilder.DropTable(name: "payments");
            migrationBuilder.DropTable(name: "order_status_history");
            migrationBuilder.DropTable(name: "order_items");
            migrationBuilder.DropTable(name: "orders");
            migrationBuilder.DropTable(name: "cart_items");
            migrationBuilder.DropTable(name: "carts");
            migrationBuilder.DropTable(name: "restock_requests");
            migrationBuilder.DropTable(name: "inventory_transactions");
            migrationBuilder.DropTable(name: "stock_entries");
            migrationBuilder.DropTable(name: "flash_sales");
            migrationBuilder.DropTable(name: "product_reviews");
            migrationBuilder.DropTable(name: "price_history");
            migrationBuilder.DropTable(name: "product_variants");
            migrationBuilder.DropTable(name: "products");
            migrationBuilder.DropTable(name: "loyalty_transactions");
            migrationBuilder.DropTable(name: "wallets");
            migrationBuilder.DropTable(name: "coupons");
            migrationBuilder.DropTable(name: "warehouses");
            migrationBuilder.DropTable(name: "categories");
        }

        private static void SeedData(MigrationBuilder migrationBuilder)
        {
            var now = DateTime.UtcNow;

            // --- Seed Categories ---
            var electronicsId = Guid.NewGuid();
            var clothingId = Guid.NewGuid();
            var homeId = Guid.NewGuid();
            var phonesId = Guid.NewGuid();
            var laptopsId = Guid.NewGuid();
            var mensId = Guid.NewGuid();
            var womensId = Guid.NewGuid();

            migrationBuilder.InsertData(table: "categories", columns: new[] { "id", "name", "description", "parent_category_id" },
                values: new object[] { electronicsId, "Electronics", "Electronic devices and accessories", null! });
            migrationBuilder.InsertData(table: "categories", columns: new[] { "id", "name", "description", "parent_category_id" },
                values: new object[] { clothingId, "Clothing", "Apparel and fashion items", null! });
            migrationBuilder.InsertData(table: "categories", columns: new[] { "id", "name", "description", "parent_category_id" },
                values: new object[] { homeId, "Home & Garden", "Home decor and garden supplies", null! });
            migrationBuilder.InsertData(table: "categories", columns: new[] { "id", "name", "description", "parent_category_id" },
                values: new object[] { phonesId, "Smartphones", "Mobile phones and accessories", electronicsId });
            migrationBuilder.InsertData(table: "categories", columns: new[] { "id", "name", "description", "parent_category_id" },
                values: new object[] { laptopsId, "Laptops", "Portable computers", electronicsId });
            migrationBuilder.InsertData(table: "categories", columns: new[] { "id", "name", "description", "parent_category_id" },
                values: new object[] { mensId, "Men's Clothing", "Clothing for men", clothingId });
            migrationBuilder.InsertData(table: "categories", columns: new[] { "id", "name", "description", "parent_category_id" },
                values: new object[] { womensId, "Women's Clothing", "Clothing for women", clothingId });

            // --- Seed Warehouses ---
            var warehouse1Id = Guid.NewGuid();
            var warehouse2Id = Guid.NewGuid();
            var warehouse3Id = Guid.NewGuid();

            migrationBuilder.InsertData(table: "warehouses", columns: new[] { "id", "name", "location", "is_active" },
                values: new object[] { warehouse1Id, "Main Warehouse", "New York, NY", true });
            migrationBuilder.InsertData(table: "warehouses", columns: new[] { "id", "name", "location", "is_active" },
                values: new object[] { warehouse2Id, "West Coast Hub", "Los Angeles, CA", true });
            migrationBuilder.InsertData(table: "warehouses", columns: new[] { "id", "name", "location", "is_active" },
                values: new object[] { warehouse3Id, "Midwest Center", "Chicago, IL", true });

            // --- Seed Products ---
            var phone1Id = Guid.NewGuid();
            var laptop1Id = Guid.NewGuid();
            var tshirtId = Guid.NewGuid();

            migrationBuilder.InsertData(table: "products",
                columns: new[] { "id", "name", "description", "sku", "base_price", "weight", "category_id", "image_urls", "is_active", "created_at", "updated_at" },
                values: new object[] { phone1Id, "Galaxy Pro Max", "Latest flagship smartphone with 6.7-inch display", "PHONE-001", 999.99m, 0.2m, phonesId, "https://example.com/images/galaxy-pro.jpg", true, now, now });

            migrationBuilder.InsertData(table: "products",
                columns: new[] { "id", "name", "description", "sku", "base_price", "weight", "category_id", "image_urls", "is_active", "created_at", "updated_at" },
                values: new object[] { laptop1Id, "UltraBook Air", "Lightweight 14-inch laptop with 16GB RAM", "LAPTOP-001", 1299.99m, 1.5m, laptopsId, "https://example.com/images/ultrabook.jpg", true, now, now });

            migrationBuilder.InsertData(table: "products",
                columns: new[] { "id", "name", "description", "sku", "base_price", "weight", "category_id", "image_urls", "is_active", "created_at", "updated_at" },
                values: new object[] { tshirtId, "Classic Cotton Tee", "Comfortable 100% cotton t-shirt", "SHIRT-001", 29.99m, 0.3m, mensId, "https://example.com/images/cotton-tee.jpg", true, now, now });

            // --- Seed Product Variants ---
            var phone1BlackId = Guid.NewGuid();
            var phone1WhiteId = Guid.NewGuid();
            var laptop1SilverId = Guid.NewGuid();
            var laptop1SpaceId = Guid.NewGuid();
            var tshirtSId = Guid.NewGuid();
            var tshirtMId = Guid.NewGuid();
            var tshirtLId = Guid.NewGuid();

            migrationBuilder.InsertData(table: "product_variants",
                columns: new[] { "id", "product_id", "sku", "size", "color", "material", "price_delta", "is_active", "created_at" },
                values: new object[] { phone1BlackId, phone1Id, "PHONE-001-BLK", null!, "Black", null!, 0m, true, now });
            migrationBuilder.InsertData(table: "product_variants",
                columns: new[] { "id", "product_id", "sku", "size", "color", "material", "price_delta", "is_active", "created_at" },
                values: new object[] { phone1WhiteId, phone1Id, "PHONE-001-WHT", null!, "White", null!, 50m, true, now });

            migrationBuilder.InsertData(table: "product_variants",
                columns: new[] { "id", "product_id", "sku", "size", "color", "material", "price_delta", "is_active", "created_at" },
                values: new object[] { laptop1SilverId, laptop1Id, "LAPTOP-001-SLV", null!, "Silver", "Aluminum", 0m, true, now });
            migrationBuilder.InsertData(table: "product_variants",
                columns: new[] { "id", "product_id", "sku", "size", "color", "material", "price_delta", "is_active", "created_at" },
                values: new object[] { laptop1SpaceId, laptop1Id, "LAPTOP-001-SGR", null!, "Space Gray", "Aluminum", 0m, true, now });

            migrationBuilder.InsertData(table: "product_variants",
                columns: new[] { "id", "product_id", "sku", "size", "color", "material", "price_delta", "is_active", "created_at" },
                values: new object[] { tshirtSId, tshirtId, "SHIRT-001-S-BLU", "S", "Blue", "Cotton", 0m, true, now });
            migrationBuilder.InsertData(table: "product_variants",
                columns: new[] { "id", "product_id", "sku", "size", "color", "material", "price_delta", "is_active", "created_at" },
                values: new object[] { tshirtMId, tshirtId, "SHIRT-001-M-BLU", "M", "Blue", "Cotton", 0m, true, now });
            migrationBuilder.InsertData(table: "product_variants",
                columns: new[] { "id", "product_id", "sku", "size", "color", "material", "price_delta", "is_active", "created_at" },
                values: new object[] { tshirtLId, tshirtId, "SHIRT-001-L-BLU", "L", "Blue", "Cotton", 2m, true, now });

            // --- Seed Stock Entries (Inventory) ---
            migrationBuilder.InsertData(table: "stock_entries",
                columns: new[] { "id", "product_variant_id", "warehouse_id", "quantity_on_hand", "quantity_reserved", "low_stock_threshold", "last_updated" },
                values: new object[] { Guid.NewGuid(), phone1BlackId, warehouse1Id, 100, 0, 10, now });
            migrationBuilder.InsertData(table: "stock_entries",
                columns: new[] { "id", "product_variant_id", "warehouse_id", "quantity_on_hand", "quantity_reserved", "low_stock_threshold", "last_updated" },
                values: new object[] { Guid.NewGuid(), phone1WhiteId, warehouse1Id, 75, 0, 10, now });
            migrationBuilder.InsertData(table: "stock_entries",
                columns: new[] { "id", "product_variant_id", "warehouse_id", "quantity_on_hand", "quantity_reserved", "low_stock_threshold", "last_updated" },
                values: new object[] { Guid.NewGuid(), laptop1SilverId, warehouse2Id, 50, 0, 5, now });
            migrationBuilder.InsertData(table: "stock_entries",
                columns: new[] { "id", "product_variant_id", "warehouse_id", "quantity_on_hand", "quantity_reserved", "low_stock_threshold", "last_updated" },
                values: new object[] { Guid.NewGuid(), laptop1SpaceId, warehouse2Id, 45, 0, 5, now });
            migrationBuilder.InsertData(table: "stock_entries",
                columns: new[] { "id", "product_variant_id", "warehouse_id", "quantity_on_hand", "quantity_reserved", "low_stock_threshold", "last_updated" },
                values: new object[] { Guid.NewGuid(), tshirtSId, warehouse3Id, 200, 0, 20, now });
            migrationBuilder.InsertData(table: "stock_entries",
                columns: new[] { "id", "product_variant_id", "warehouse_id", "quantity_on_hand", "quantity_reserved", "low_stock_threshold", "last_updated" },
                values: new object[] { Guid.NewGuid(), tshirtMId, warehouse3Id, 250, 0, 20, now });
            migrationBuilder.InsertData(table: "stock_entries",
                columns: new[] { "id", "product_variant_id", "warehouse_id", "quantity_on_hand", "quantity_reserved", "low_stock_threshold", "last_updated" },
                values: new object[] { Guid.NewGuid(), tshirtLId, warehouse3Id, 180, 0, 20, now });

            // --- Seed Coupons ---
            migrationBuilder.InsertData(table: "coupons",
                columns: new[] { "id", "code", "type", "discount_value", "min_order_value", "max_uses", "current_uses", "expires_at", "is_active", "created_at" },
                values: new object[] { Guid.NewGuid(), "WELCOME10", "Percentage", 10m, 50m, 1000, 0, now.AddMonths(6), true, now });
            migrationBuilder.InsertData(table: "coupons",
                columns: new[] { "id", "code", "type", "discount_value", "min_order_value", "max_uses", "current_uses", "expires_at", "is_active", "created_at" },
                values: new object[] { Guid.NewGuid(), "FLAT20", "Fixed", 20m, 100m, 500, 0, now.AddMonths(3), true, now });
            migrationBuilder.InsertData(table: "coupons",
                columns: new[] { "id", "code", "type", "discount_value", "min_order_value", "max_uses", "current_uses", "expires_at", "is_active", "created_at" },
                values: new object[] { Guid.NewGuid(), "FREESHIP", "FreeShipping", 0m, 75m, null!, 0, now.AddMonths(12), true, now });
        }
    }
}
