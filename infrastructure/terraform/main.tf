# Configure Terraform and required providers
terraform {
  required_version = ">= 1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

# Configure the Microsoft Azure Provider
provider "azurerm" {
  features {}
}

# Resource Group
resource "azurerm_resource_group" "lift_tracker" {
  name     = var.resource_group_name
  location = var.location

  tags = {
    Environment = var.environment
    Project     = "LiftTracker"
    ManagedBy   = "Terraform"
  }
}

# App Service Plan
resource "azurerm_service_plan" "lift_tracker" {
  name                = "${var.app_name}-plan"
  resource_group_name = azurerm_resource_group.lift_tracker.name
  location            = azurerm_resource_group.lift_tracker.location
  os_type             = "Linux"
  sku_name            = var.app_service_sku

  tags = {
    Environment = var.environment
    Project     = "LiftTracker"
    ManagedBy   = "Terraform"
  }
}

# App Service for API
resource "azurerm_linux_web_app" "lift_tracker_api" {
  name                = "${var.app_name}-api"
  resource_group_name = azurerm_resource_group.lift_tracker.name
  location            = azurerm_service_plan.lift_tracker.location
  service_plan_id     = azurerm_service_plan.lift_tracker.id

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
    always_on = true
  }

  app_settings = {
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE" = "false"
    "ConnectionStrings__DefaultConnection" = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.db_connection.id})"
    "Authentication__Google__ClientId"     = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.google_client_id.id})"
    "Authentication__Google__ClientSecret" = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.google_client_secret.id})"
  }

  identity {
    type = "SystemAssigned"
  }

  tags = {
    Environment = var.environment
    Project     = "LiftTracker"
    ManagedBy   = "Terraform"
  }
}

# App Service for Client (Blazor WASM)
resource "azurerm_linux_web_app" "lift_tracker_client" {
  name                = "${var.app_name}-client"
  resource_group_name = azurerm_resource_group.lift_tracker.name
  location            = azurerm_service_plan.lift_tracker.location
  service_plan_id     = azurerm_service_plan.lift_tracker.id

  site_config {
    application_stack {
      node_version = "18-lts"
    }
    always_on = true
  }

  app_settings = {
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE" = "false"
    "API_BASE_URL" = "https://${azurerm_linux_web_app.lift_tracker_api.default_hostname}"
  }

  tags = {
    Environment = var.environment
    Project     = "LiftTracker"
    ManagedBy   = "Terraform"
  }
}

# SQL Server
resource "azurerm_mssql_server" "lift_tracker" {
  name                         = "${var.app_name}-sql-server"
  resource_group_name          = azurerm_resource_group.lift_tracker.name
  location                     = azurerm_resource_group.lift_tracker.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password

  tags = {
    Environment = var.environment
    Project     = "LiftTracker"
    ManagedBy   = "Terraform"
  }
}

# SQL Database
resource "azurerm_mssql_database" "lift_tracker" {
  name           = "${var.app_name}-db"
  server_id      = azurerm_mssql_server.lift_tracker.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  license_type   = "LicenseIncluded"
  max_size_gb    = 2
  sku_name       = var.sql_sku_name

  tags = {
    Environment = var.environment
    Project     = "LiftTracker"
    ManagedBy   = "Terraform"
  }
}

# SQL Firewall Rule for Azure Services
resource "azurerm_mssql_firewall_rule" "azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.lift_tracker.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Key Vault
resource "azurerm_key_vault" "lift_tracker" {
  name                = "${var.app_name}-kv"
  location            = azurerm_resource_group.lift_tracker.location
  resource_group_name = azurerm_resource_group.lift_tracker.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"

  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    secret_permissions = [
      "Get",
      "List",
      "Set",
      "Delete",
      "Recover",
      "Backup",
      "Restore"
    ]
  }

  # Access policy for App Service API
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = azurerm_linux_web_app.lift_tracker_api.identity[0].principal_id

    secret_permissions = [
      "Get",
      "List"
    ]
  }

  tags = {
    Environment = var.environment
    Project     = "LiftTracker"
    ManagedBy   = "Terraform"
  }
}

# Key Vault Secrets
resource "azurerm_key_vault_secret" "db_connection" {
  name         = "DbConnectionString"
  value        = "Server=tcp:${azurerm_mssql_server.lift_tracker.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.lift_tracker.name};Persist Security Info=False;User ID=${var.sql_admin_username};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = azurerm_key_vault.lift_tracker.id

  depends_on = [azurerm_key_vault.lift_tracker]
}

resource "azurerm_key_vault_secret" "google_client_id" {
  name         = "GoogleClientId"
  value        = var.google_client_id
  key_vault_id = azurerm_key_vault.lift_tracker.id

  depends_on = [azurerm_key_vault.lift_tracker]
}

resource "azurerm_key_vault_secret" "google_client_secret" {
  name         = "GoogleClientSecret"
  value        = var.google_client_secret
  key_vault_id = azurerm_key_vault.lift_tracker.id

  depends_on = [azurerm_key_vault.lift_tracker]
}

# Data source for current Azure client configuration
data "azurerm_client_config" "current" {}