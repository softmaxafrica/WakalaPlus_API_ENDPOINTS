using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WakalaPlus.Migrations
{
    /// <inheritdoc />
    public partial class wakalaMigr1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    AGENT_CODE = table.Column<string>(type: "varchar(255)", nullable: false),
                    PASSWORD = table.Column<string>(type: "longtext", nullable: false),
                    NIDA = table.Column<string>(type: "longtext", nullable: false),
                    AGENT_FULL_NAME = table.Column<string>(type: "longtext", nullable: false),
                    AGENT_PHONE = table.Column<string>(type: "longtext", nullable: false),
                    NETWORKS_OPERATING = table.Column<string>(type: "longtext", nullable: false),
                    SERVICE_GROUP_CODE = table.Column<string>(type: "longtext", nullable: false),
                    REGISTRATION_STATUS = table.Column<string>(type: "longtext", nullable: false),
                    ADDRESS = table.Column<string>(type: "longtext", nullable: true),
                    LONGITUDE = table.Column<double>(type: "double", nullable: true),
                    LATITUDE = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.AGENT_CODE);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomerTickets",
                columns: table => new
                {
                    TRANSACTION_ID = table.Column<string>(type: "varchar(255)", nullable: false),
                    AGENT_CODE = table.Column<string>(type: "longtext", nullable: true),
                    PHONE_NUMBER = table.Column<string>(type: "longtext", nullable: true),
                    DESCRIPTION = table.Column<string>(type: "longtext", nullable: true),
                    CUSTOMER_LONGITUDE = table.Column<double>(type: "double", nullable: true),
                    CUSTOMER_LATITUDE = table.Column<double>(type: "double", nullable: true),
                    AGENT_LONGITUDE = table.Column<double>(type: "double", nullable: true),
                    AGENT_LATITUDE = table.Column<double>(type: "double", nullable: true),
                    SERVICE_REQUESTED = table.Column<string>(type: "longtext", nullable: true),
                    NETWORK = table.Column<string>(type: "longtext", nullable: true),
                    TICKET_STATUS = table.Column<string>(type: "longtext", nullable: false),
                    TICKET_CREATION_DATE_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TICKET_LAST_RESPONSE_DATE_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerTickets", x => x.TRANSACTION_ID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    customerId = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.customerId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeviceDetails",
                columns: table => new
                {
                    DEVICE_ID = table.Column<string>(type: "varchar(255)", nullable: false),
                    IDENTITY = table.Column<string>(type: "longtext", nullable: false),
                    LAST_CONNECTION_DATE = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CREATED_DATE = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CONNECTION_ID = table.Column<string>(type: "longtext", nullable: false),
                    LAST_ACTION = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceDetails", x => x.DEVICE_ID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeneralTranslations",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "varchar(255)", nullable: false),
                    VALUE = table.Column<string>(type: "longtext", nullable: true),
                    DESCRIPTION = table.Column<string>(type: "longtext", nullable: false),
                    TABLE_NAME = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralTranslations", x => x.CODE);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NotificationMessages",
                columns: table => new
                {
                    CONNECTION_ID = table.Column<string>(type: "varchar(255)", nullable: false),
                    SENDER_IDENTITY = table.Column<string>(type: "longtext", nullable: false),
                    RECEIVER_CONNECTION_ID = table.Column<string>(type: "longtext", nullable: false),
                    RECEIVER_IDENTITY = table.Column<string>(type: "longtext", nullable: false),
                    MESSAGE = table.Column<string>(type: "longtext", nullable: false),
                    STATUS = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationMessages", x => x.CONNECTION_ID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OnlineOfflineAgent",
                columns: table => new
                {
                    TRANSACTION_ID = table.Column<string>(type: "varchar(255)", nullable: false),
                    AGENT_CODE = table.Column<string>(type: "longtext", nullable: true),
                    AGENT_FULL_NAME = table.Column<string>(type: "longtext", nullable: true),
                    AGENT_PHONE = table.Column<string>(type: "longtext", nullable: true),
                    ADDRESS = table.Column<string>(type: "longtext", nullable: true),
                    LONGITUDE = table.Column<double>(type: "double", nullable: true),
                    LATITUDE = table.Column<double>(type: "double", nullable: true),
                    SERVICE_GROUP_CODE = table.Column<string>(type: "longtext", nullable: true),
                    NETWORKS_OPERATING = table.Column<string>(type: "longtext", nullable: true),
                    STATUS = table.Column<string>(type: "longtext", nullable: true),
                    LAST_UPDATE = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineOfflineAgent", x => x.TRANSACTION_ID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    TRANSACTION_ID = table.Column<string>(type: "varchar(255)", nullable: false),
                    SERVICE_CODE = table.Column<string>(type: "longtext", nullable: false),
                    AGENT_CODE = table.Column<string>(type: "longtext", nullable: false),
                    COMMENTS = table.Column<string>(type: "longtext", nullable: false),
                    CUSTOMER_PHONE_NUMBER = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.TRANSACTION_ID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    SERVICE_CODE = table.Column<string>(type: "varchar(255)", nullable: false),
                    SERVICE_TYPE = table.Column<string>(type: "longtext", nullable: false),
                    SERVICE_NAME = table.Column<string>(type: "longtext", nullable: false),
                    SERVICE_DESCRIPTION = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.SERVICE_CODE);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketDetails",
                columns: table => new
                {
                    TRANSACTION_ID = table.Column<string>(type: "varchar(255)", nullable: false),
                    TICKET_REF = table.Column<string>(type: "longtext", nullable: false),
                    AGENT_CODE = table.Column<string>(type: "longtext", nullable: false),
                    TICKET_STATUS = table.Column<string>(type: "longtext", nullable: false),
                    TICKET_CREATION_DATE_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TICKET_LAST_RESPONSE_DATE_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketDetails", x => x.TRANSACTION_ID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "CustomerTickets");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "DeviceDetails");

            migrationBuilder.DropTable(
                name: "GeneralTranslations");

            migrationBuilder.DropTable(
                name: "NotificationMessages");

            migrationBuilder.DropTable(
                name: "OnlineOfflineAgent");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "TicketDetails");
        }
    }
}
