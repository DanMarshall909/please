#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Interactive setup script for Please v6 environment configuration
.DESCRIPTION
    Guides users through configuring AI provider API keys and settings for Please v6.
    Supports OpenAI, Anthropic, Gemini, OpenRouter, and Ollama providers.
.PARAMETER Provider
    Optional provider to configure directly (OpenAI, Anthropic, Gemini, OpenRouter, Ollama)
.PARAMETER Permanent
    Set environment variables permanently (default: temporary for current session)
.EXAMPLE
    .\setup-environment.ps1
    .\setup-environment.ps1 -Provider OpenAI -Permanent
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("OpenAI", "Anthropic", "Gemini", "OpenRouter", "Ollama", "")]
    [string]$Provider = "",

    [Parameter(Mandatory=$false)]
    [switch]$Permanent = $false
)

# Color functions for better UX
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Header { param($Message) Write-Host "`n🚀 $Message" -ForegroundColor Magenta -BackgroundColor Black }

# Provider configuration templates
$ProviderConfigs = @{
    "OpenAI" = @{
        DisplayName = "OpenAI (GPT-4, GPT-3.5-turbo)"
        Variables = @{
            "OPENAI_API_KEY" = @{ Description = "OpenAI API Key (starts with sk-)"; Required = $true; Default = "" }
            "OPENAI_DEFAULT_MODEL" = @{ Description = "Default model"; Required = $false; Default = "gpt-4o-mini" }
            "OPENAI_BASE_URL" = @{ Description = "Base URL"; Required = $false; Default = "https://api.openai.com/v1" }
        }
        TestUrl = "https://api.openai.com/v1/models"
        GetApiKeyUrl = "https://platform.openai.com/api-keys"
    }
    "Anthropic" = @{
        DisplayName = "Anthropic (Claude)"
        Variables = @{
            "ANTHROPIC_API_KEY" = @{ Description = "Anthropic API Key (starts with sk-ant-)"; Required = $true; Default = "" }
            "ANTHROPIC_DEFAULT_MODEL" = @{ Description = "Default model"; Required = $false; Default = "claude-3-haiku-20240307" }
            "ANTHROPIC_BASE_URL" = @{ Description = "Base URL"; Required = $false; Default = "https://api.anthropic.com/v1" }
        }
        TestUrl = "https://api.anthropic.com/v1/messages"
        GetApiKeyUrl = "https://console.anthropic.com/"
    }
    "Gemini" = @{
        DisplayName = "Google Gemini"
        Variables = @{
            "GEMINI_API_KEY" = @{ Description = "Gemini API Key"; Required = $true; Default = "" }
            "GEMINI_DEFAULT_MODEL" = @{ Description = "Default model"; Required = $false; Default = "gemini-pro" }
            "GEMINI_BASE_URL" = @{ Description = "Base URL"; Required = $false; Default = "https://generativelanguage.googleapis.com/v1beta" }
        }
        TestUrl = "https://generativelanguage.googleapis.com/v1beta/models"
        GetApiKeyUrl = "https://makersuite.google.com/app/apikey"
    }
    "OpenRouter" = @{
        DisplayName = "OpenRouter (Multiple models)"
        Variables = @{
            "OPENROUTER_API_KEY" = @{ Description = "OpenRouter API Key (starts with sk-or-)"; Required = $true; Default = "" }
            "OPENROUTER_DEFAULT_MODEL" = @{ Description = "Default model"; Required = $false; Default = "microsoft/wizardlm-2-8x22b" }
            "OPENROUTER_BASE_URL" = @{ Description = "Base URL"; Required = $false; Default = "https://openrouter.ai/api/v1" }
        }
        TestUrl = "https://openrouter.ai/api/v1/models"
        GetApiKeyUrl = "https://openrouter.ai/keys"
    }
    "Ollama" = @{
        DisplayName = "Ollama (Local)"
        Variables = @{
            "OLLAMA_BASE_URL" = @{ Description = "Ollama server URL"; Required = $true; Default = "http://localhost:11434" }
            "OLLAMA_DEFAULT_MODEL" = @{ Description = "Default model"; Required = $false; Default = "llama2" }
        }
        TestUrl = "http://localhost:11434/api/tags"
        GetApiKeyUrl = "https://ollama.ai/"
    }
}

function Show-Banner {
    Write-Host @"

██████╗ ██╗     ███████╗ █████╗ ███████╗███████╗    ██╗   ██╗ ██████╗
██╔══██╗██║     ██╔════╝██╔══██╗██╔════╝██╔════╝    ██║   ██║██╔════╝
██████╔╝██║     █████╗  ███████║███████╗█████╗      ██║   ██║██████╗
██╔═══╝ ██║     ██╔══╝  ██╔══██║╚════██║██╔══╝      ╚██╗ ██╔╝██╔═══╝
██║     ███████╗███████╗██║  ██║███████║███████╗     ╚████╔╝ ██████╗
╚═╝     ╚══════╝╚══════╝╚═╝  ╚═╝╚══════╝╚══════╝      ╚═══╝  ╚═════╝

Environment Setup Script
"@ -ForegroundColor Cyan
}

function Show-CurrentConfig {
    Write-Header "Current Environment Configuration"

    $allProviders = @("OPENAI", "ANTHROPIC", "GEMINI", "OPENROUTER", "OLLAMA")
    $configuredCount = 0

    foreach ($providerPrefix in $allProviders) {
        $apiKeyVar = "${providerPrefix}_API_KEY"
        $baseUrlVar = "${providerPrefix}_BASE_URL"

        $apiKey = [Environment]::GetEnvironmentVariable($apiKeyVar)
        $baseUrl = [Environment]::GetEnvironmentVariable($baseUrlVar)

        if ($apiKey -or $baseUrl) {
            $configuredCount++
            $status = "✅ Configured"
            if ($apiKey) {
                $maskedKey = $apiKey.Substring(0, [Math]::Min(8, $apiKey.Length)) + "..." + $apiKey.Substring([Math]::Max(0, $apiKey.Length - 4))
                Write-Host "  $providerPrefix - $status (Key: $maskedKey)" -ForegroundColor Green
            } else {
                Write-Host "  $providerPrefix - $status (URL: $baseUrl)" -ForegroundColor Green
            }
        } else {
            Write-Host "  $providerPrefix - ❌ Not configured" -ForegroundColor Red
        }
    }

    if ($configuredCount -eq 0) {
        Write-Warning "No providers are currently configured"
    } else {
        Write-Success "$configuredCount provider(s) currently configured"
    }
}

function Show-ProviderMenu {
    Write-Header "Select AI Provider to Configure"

    $menuItems = @()
    $index = 1

    foreach ($key in $ProviderConfigs.Keys) {
        $config = $ProviderConfigs[$key]
        Write-Host "$index. $($config.DisplayName)" -ForegroundColor White
        $menuItems += $key
        $index++
    }

    Write-Host "$index. Configure Multiple Providers" -ForegroundColor Yellow
    Write-Host "0. Exit" -ForegroundColor Red

    do {
        $selection = Read-Host "`nEnter your choice (0-$index)"
        if ($selection -eq "0") { return $null }
        if ($selection -eq $index.ToString()) { return "Multiple" }
        if ($selection -match '^\d+$' -and [int]$selection -ge 1 -and [int]$selection -lt $index) {
            return $menuItems[[int]$selection - 1]
        }
        Write-Warning "Invalid selection. Please choose 0-$index."
    } while ($true)
}

function Validate-ApiKey {
    param($Provider, $ApiKey)

    switch ($Provider) {
        "OpenAI" { return $ApiKey -match '^sk-[a-zA-Z0-9]{48,}$' }
        "Anthropic" { return $ApiKey -match '^sk-ant-[a-zA-Z0-9\-_]{95,}$' }
        "OpenRouter" { return $ApiKey -match '^sk-or-[a-zA-Z0-9\-_]{50,}$' }
        "Gemini" { return $ApiKey -match '^[a-zA-Z0-9\-_]{39}$' }
        default { return $ApiKey.Length -gt 10 }
    }
}

function Test-ProviderConnection {
    param($Provider, $Config)

    Write-Info "Testing connection to $Provider..."

    try {
        if ($Provider -eq "Ollama") {
            $testUrl = $Config.Variables["OLLAMA_BASE_URL"].Value + "/api/tags"
            $response = Invoke-RestMethod -Uri $testUrl -Method Get -TimeoutSec 5
            return $true
        } else {
            # For API-based providers, just test if the URL is reachable
            $testUrl = $Config.TestUrl
            $response = Invoke-WebRequest -Uri $testUrl -Method Head -TimeoutSec 5 -ErrorAction SilentlyContinue
            return $response.StatusCode -lt 400
        }
    } catch {
        Write-Warning "Connection test failed: $($_.Exception.Message)"
        return $false
    }
}

function Configure-Provider {
    param($ProviderName)

    $config = $ProviderConfigs[$ProviderName]
    Write-Header "Configuring $($config.DisplayName)"

    if ($ProviderName -ne "Ollama") {
        Write-Info "Get your API key from: $($config.GetApiKeyUrl)"
    }

    $variables = @{}

    foreach ($varName in $config.Variables.Keys) {
        $varConfig = $config.Variables[$varName]
        $currentValue = [Environment]::GetEnvironmentVariable($varName)

        if ($currentValue) {
            Write-Info "Current value for $varName is set"
            $useExisting = Read-Host "Keep existing value? (y/n) [y]"
            if ($useExisting -eq "" -or $useExisting -eq "y") {
                $variables[$varName] = $currentValue
                continue
            }
        }

        do {
            if ($varConfig.Required) {
                $prompt = "$($varConfig.Description) (required)"
            } else {
                $prompt = "$($varConfig.Description) [default: $($varConfig.Default)]"
            }

            if ($varName -like "*API_KEY*") {
                $value = Read-Host -Prompt $prompt -AsSecureString
                $value = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($value))
            } else {
                $value = Read-Host $prompt
            }

            if ([string]::IsNullOrWhiteSpace($value)) {
                if ($varConfig.Required) {
                    Write-Warning "This field is required."
                    continue
                } else {
                    $value = $varConfig.Default
                }
            }

            # Validate API key format
            if ($varName -like "*API_KEY*" -and -not (Validate-ApiKey $ProviderName $value)) {
                Write-Warning "API key format appears invalid for $ProviderName. Continue anyway? (y/n)"
                $continue = Read-Host
                if ($continue -ne "y") { continue }
            }

            $variables[$varName] = $value
            break
        } while ($true)
    }

    # Store values back to config for testing
    foreach ($varName in $variables.Keys) {
        $config.Variables[$varName].Value = $variables[$varName]
    }

    return $variables
}

function Set-EnvironmentVariables {
    param($Variables, $Permanent)

    Write-Header "Setting Environment Variables"

    foreach ($varName in $Variables.Keys) {
        $value = $Variables[$varName]

        # Set for current session
        [Environment]::SetEnvironmentVariable($varName, $value, "Process")

        if ($Permanent) {
            # Set permanently for current user
            [Environment]::SetEnvironmentVariable($varName, $value, "User")
            Write-Success "Set $varName permanently"
        } else {
            Write-Success "Set $varName for current session"
        }
    }

    if (-not $Permanent) {
        Write-Warning "Variables are set for current session only. Use -Permanent to make them persistent."
    }
}

function Test-Configuration {
    param($ProviderName, $Config)

    Write-Header "Testing Configuration"

    $success = Test-ProviderConnection $ProviderName $Config

    if ($success) {
        Write-Success "$ProviderName configuration test passed!"
    } else {
        Write-Warning "$ProviderName configuration test failed. Check your settings."
    }

    return $success
}

function Show-Summary {
    param($ConfiguredProviders)

    Write-Header "Configuration Complete"

    Write-Success "Successfully configured the following providers:"
    foreach ($provider in $ConfiguredProviders) {
        Write-Host "  ✅ $provider" -ForegroundColor Green
    }

    Write-Info "`nNext steps:"
    Write-Host "  1. Test your configuration:" -ForegroundColor White
    Write-Host "     cd src/Presentation/Please.Console/bin/Debug/net8.0/win-x64" -ForegroundColor Gray
    Write-Host "     .\Please.Console.exe 'echo hello world'" -ForegroundColor Gray
    Write-Host "`n  2. Build the application if needed:" -ForegroundColor White
    Write-Host "     dotnet build src/Presentation/Please.Console" -ForegroundColor Gray
}

# Main execution
function Main {
    Show-Banner
    Show-CurrentConfig

    $configuredProviders = @()

    if ($Provider) {
        # Direct provider configuration
        $selectedProvider = $Provider
    } else {
        # Interactive menu
        $selectedProvider = Show-ProviderMenu
        if (-not $selectedProvider) {
            Write-Info "Setup cancelled."
            return
        }
    }

    if ($selectedProvider -eq "Multiple") {
        Write-Header "Multiple Provider Configuration"
        foreach ($providerName in $ProviderConfigs.Keys) {
            $configure = Read-Host "Configure $($ProviderConfigs[$providerName].DisplayName)? (y/n) [n]"
            if ($configure -eq "y") {
                $variables = Configure-Provider $providerName
                Set-EnvironmentVariables $variables $Permanent
                Test-Configuration $providerName $ProviderConfigs[$providerName]
                $configuredProviders += $providerName
            }
        }
    } else {
        # Single provider configuration
        $variables = Configure-Provider $selectedProvider
        Set-EnvironmentVariables $variables $Permanent
        Test-Configuration $selectedProvider $ProviderConfigs[$selectedProvider]
        $configuredProviders += $selectedProvider
    }

    if ($configuredProviders.Count -gt 0) {
        Show-Summary $configuredProviders
    } else {
        Write-Info "No providers were configured."
    }
}

# Run the script
try {
    Main
} catch {
    Write-Error "An error occurred: $($_.Exception.Message)"
    Write-Info "Please report this issue if it persists."
}
