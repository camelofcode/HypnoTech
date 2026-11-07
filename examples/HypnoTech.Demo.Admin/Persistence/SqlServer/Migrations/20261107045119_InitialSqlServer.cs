using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HypnoTech.Demo.Admin.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "adminusers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Roles = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adminusers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audiotracks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatedId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersistenceOrder = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audiotracks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "config",
                columns: table => new
                {
                    PersistenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Paypal_ApiClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Paypal_ApiClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Paypal_ApiUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Paypal_SalesUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Paypal_PaymentCompleteUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Paypal_PaymentCancelledUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Paypal_IsProduction = table.Column<bool>(type: "bit", nullable: true),
                    Email_EnableEmails = table.Column<bool>(type: "bit", nullable: true),
                    Email_SmtpServer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email_MailboxUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email_MailboxPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email_Sender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email_DefaultRecipient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MainSite_Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MainSite_ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_config", x => x.PersistenceId);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_countries", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "demoArticles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HeroImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Featured = table.Column<bool>(type: "bit", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demoArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "demoArtists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Discipline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Biography = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Featured = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demoArtists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "demoEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArtistId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Venue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TicketPrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgrammePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Featured = table.Column<bool>(type: "bit", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demoEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "downloadcodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedemptionUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_downloadcodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "emailtemplates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmailType = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Header = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntroLine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FooterText = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emailtemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "merchcategorytypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchcategorytypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "merches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormatId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostageProfile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DownloadCodeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "navbar",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_navbar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    PaymentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SaleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaleUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tracked = table.Column<bool>(type: "bit", nullable: false),
                    Postage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Address_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Address1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Town = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Region = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DownloadCodeIds = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.PaymentId);
                });

            migrationBuilder.CreateTable(
                name: "passwordresets",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passwordresets", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "photoalbums",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_photoalbums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "postageprofiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitedKingdom_Alone = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitedKingdom_WithOthers = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EuropeanUnion_Alone = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EuropeanUnion_WithOthers = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RestOfWorld1_Alone = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RestOfWorld1_WithOthers = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RestOfWorld2_Alone = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RestOfWorld2_WithOthers = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RestOfWorld3_Alone = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RestOfWorld3_WithOthers = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_postageprofiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "serilog",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties_SiteName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties_Path = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_serilog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "videos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YouTubeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersistenceOrder = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_videos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "demoEvents_Schedule",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Time = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Format = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    PersistenceOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demoEvents_Schedule", x => new { x.EventId, x.Id });
                    table.ForeignKey(
                        name: "FK_demoEvents_Schedule_demoEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "demoEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "downloadcodes_Codes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DownloadCodeCollectionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_downloadcodes_Codes", x => new { x.DownloadCodeCollectionId, x.Code });
                    table.ForeignKey(
                        name: "FK_downloadcodes_Codes_downloadcodes_DownloadCodeCollectionId",
                        column: x => x.DownloadCodeCollectionId,
                        principalTable: "downloadcodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "merchcategorytypes_SubcategoryTypes",
                columns: table => new
                {
                    PersistenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    MerchCategoryTypeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PersistenceOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchcategorytypes_SubcategoryTypes", x => x.PersistenceId);
                    table.ForeignKey(
                        name: "FK_merchcategorytypes_SubcategoryTypes_merchcategorytypes_MerchCategoryTypeId",
                        column: x => x.MerchCategoryTypeId,
                        principalTable: "merchcategorytypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "merches_Options",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MerchItemId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersistenceOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merches_Options", x => new { x.MerchItemId, x.Id });
                    table.ForeignKey(
                        name: "FK_merches_Options_merches_MerchItemId",
                        column: x => x.MerchItemId,
                        principalTable: "merches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders_Items",
                columns: table => new
                {
                    PersistenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Item = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    RowPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderPaymentId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders_Items", x => x.PersistenceId);
                    table.ForeignKey(
                        name: "FK_orders_Items_orders_OrderPaymentId",
                        column: x => x.OrderPaymentId,
                        principalTable: "orders",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "photoalbums_Photos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PhotoAlbumId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersistenceOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_photoalbums_Photos", x => new { x.PhotoAlbumId, x.Id });
                    table.ForeignKey(
                        name: "FK_photoalbums_Photos_photoalbums_PhotoAlbumId",
                        column: x => x.PhotoAlbumId,
                        principalTable: "photoalbums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_adminusers_Email",
                table: "adminusers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_adminusers_Username",
                table: "adminusers",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_merchcategorytypes_SubcategoryTypes_MerchCategoryTypeId",
                table: "merchcategorytypes_SubcategoryTypes",
                column: "MerchCategoryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_Items_OrderPaymentId",
                table: "orders_Items",
                column: "OrderPaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adminusers");

            migrationBuilder.DropTable(
                name: "audiotracks");

            migrationBuilder.DropTable(
                name: "config");

            migrationBuilder.DropTable(
                name: "countries");

            migrationBuilder.DropTable(
                name: "demoArticles");

            migrationBuilder.DropTable(
                name: "demoArtists");

            migrationBuilder.DropTable(
                name: "demoEvents_Schedule");

            migrationBuilder.DropTable(
                name: "downloadcodes_Codes");

            migrationBuilder.DropTable(
                name: "emailtemplates");

            migrationBuilder.DropTable(
                name: "merchcategorytypes_SubcategoryTypes");

            migrationBuilder.DropTable(
                name: "merches_Options");

            migrationBuilder.DropTable(
                name: "navbar");

            migrationBuilder.DropTable(
                name: "orders_Items");

            migrationBuilder.DropTable(
                name: "passwordresets");

            migrationBuilder.DropTable(
                name: "photoalbums_Photos");

            migrationBuilder.DropTable(
                name: "postageprofiles");

            migrationBuilder.DropTable(
                name: "serilog");

            migrationBuilder.DropTable(
                name: "videos");

            migrationBuilder.DropTable(
                name: "demoEvents");

            migrationBuilder.DropTable(
                name: "downloadcodes");

            migrationBuilder.DropTable(
                name: "merchcategorytypes");

            migrationBuilder.DropTable(
                name: "merches");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "photoalbums");
        }
    }
}
