# Infrastructure Deployment

This directory contains Terraform configuration for deploying the Lift Tracker application to Azure.

## Prerequisites

1. [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed and configured
2. [Terraform](https://www.terraform.io/downloads.html) installed (version >= 1.0)
3. Azure subscription with appropriate permissions

## Setup

1. Login to Azure:
   ```bash
   az login
   ```

2. Set the subscription (if you have multiple):
   ```bash
   az account set --subscription "your-subscription-id"
   ```

3. Copy the example variables file and configure it:
   ```bash
   cp terraform.tfvars.example terraform.tfvars
   ```

4. Edit `terraform.tfvars` and provide your specific values:
   - SQL admin password
   - Google OAuth client ID and secret
   - Other configuration as needed

## Deployment

1. Initialize Terraform:
   ```bash
   terraform init
   ```

2. Plan the deployment:
   ```bash
   terraform plan
   ```

3. Apply the configuration:
   ```bash
   terraform apply
   ```

## Resources Created

- **Resource Group**: Container for all resources
- **App Service Plan**: Hosting plan for the web application
- **App Service**: Web application hosting the API
- **SQL Server**: Database server
- **SQL Database**: Application database
- **Key Vault**: Secure storage for connection strings and secrets

## Configuration

After deployment, the following secrets will be available in Key Vault:
- `sql-connection-string`: Database connection string
- `google-client-id`: Google OAuth client ID
- `google-client-secret`: Google OAuth client secret

## Cleanup

To destroy all resources:
```bash
terraform destroy
```

**Warning**: This will permanently delete all resources and data!
