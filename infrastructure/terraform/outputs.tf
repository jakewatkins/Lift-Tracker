# Outputs for Terraform configuration

output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "app_service_url" {
  description = "URL of the App Service"
  value       = "https://${azurerm_linux_web_app.api.default_hostname}"
}

output "sql_server_fqdn" {
  description = "Fully qualified domain name of the SQL Server"
  value       = azurerm_mssql_server.main.fully_qualified_domain_name
  sensitive   = true
}

output "sql_database_name" {
  description = "Name of the SQL Database"
  value       = azurerm_mssql_database.main.name
}

output "key_vault_uri" {
  description = "URI of the Key Vault"
  value       = azurerm_key_vault.main.vault_uri
  sensitive   = true
}

output "connection_string_secret_name" {
  description = "Name of the connection string secret in Key Vault"
  value       = azurerm_key_vault_secret.sql_connection_string.name
}

output "google_client_id_secret_name" {
  description = "Name of the Google Client ID secret in Key Vault"
  value       = azurerm_key_vault_secret.google_client_id.name
}

output "google_client_secret_secret_name" {
  description = "Name of the Google Client Secret secret in Key Vault"
  value       = azurerm_key_vault_secret.google_client_secret.name
}