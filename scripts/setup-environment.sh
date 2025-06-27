#!/bin/bash

# Please v6 Environment Setup Script
# Cross-platform shell script for Linux/macOS/WSL
# Configures AI provider API keys and settings

set -euo pipefail

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Output functions
success() { echo -e "${GREEN}✅ $1${NC}"; }
info() { echo -e "${CYAN}ℹ️  $1${NC}"; }
warning() { echo -e "${YELLOW}⚠️  $1${NC}"; }
error() { echo -e "${RED}❌ $1${NC}"; }
header() { echo -e "\n${MAGENTA}🚀 $1${NC}"; }

# Provider configurations
declare -A PROVIDER_NAMES=(
    ["openai"]="OpenAI (GPT-4, GPT-3.5-turbo)"
    ["anthropic"]="Anthropic (Claude)"
    ["gemini"]="Google Gemini"
    ["openrouter"]="OpenRouter (Multiple models)"
    ["ollama"]="Ollama (Local)"
)

declare -A PROVIDER_API_KEYS=(
    ["openai"]="OPENAI_API_KEY"
    ["anthropic"]="ANTHROPIC_API_KEY"
    ["gemini"]="GEMINI_API_KEY"
    ["openrouter"]="OPENROUTER_API_KEY"
    ["ollama"]=""
)

declare -A PROVIDER_DEFAULT_MODELS=(
    ["openai"]="gpt-4o-mini"
    ["anthropic"]="claude-3-haiku-20240307"
    ["gemini"]="gemini-pro"
    ["openrouter"]="microsoft/wizardlm-2-8x22b"
    ["ollama"]="llama2"
)

declare -A PROVIDER_BASE_URLS=(
    ["openai"]="https://api.openai.com/v1"
    ["anthropic"]="https://api.anthropic.com/v1"
    ["gemini"]="https://generativelanguage.googleapis.com/v1beta"
    ["openrouter"]="https://openrouter.ai/api/v1"
    ["ollama"]="http://localhost:11434"
)

declare -A PROVIDER_TEST_URLS=(
    ["openai"]="https://api.openai.com/v1/models"
    ["anthropic"]="https://api.anthropic.com/v1/messages"
    ["gemini"]="https://generativelanguage.googleapis.com/v1beta/models"
    ["openrouter"]="https://openrouter.ai/api/v1/models"
    ["ollama"]="http://localhost:11434/api/tags"
)

declare -A PROVIDER_KEY_URLS=(
    ["openai"]="https://platform.openai.com/api-keys"
    ["anthropic"]="https://console.anthropic.com/"
    ["gemini"]="https://makersuite.google.com/app/apikey"
    ["openrouter"]="https://openrouter.ai/keys"
    ["ollama"]="https://ollama.ai/"
)

# Global variables
PERMANENT=false
INSTALL_ALIAS=false
SELECTED_PROVIDER=""
CONFIGURED_PROVIDERS=()

show_banner() {
    echo -e "${CYAN}"
    cat << 'EOF'

██████╗ ██╗     ███████╗ █████╗ ███████╗███████╗    ██╗   ██╗ ██████╗
██╔══██╗██║     ██╔════╝██╔══██╗██╔════╝██╔════╝    ██║   ██║██╔════╝
██████╔╝██║     █████╗  ███████║███████╗█████╗      ██║   ██║██████╗
██╔═══╝ ██║     ██╔══╝  ██╔══██║╚════██║██╔══╝      ╚██╗ ██╔╝██╔═══╝
██║     ███████╗███████╗██║  ██║███████║███████╗     ╚████╔╝ ██████╗
╚═╝     ╚══════╝╚══════╝╚═╝  ╚═╝╚══════╝╚══════╝      ╚═══╝  ╚═════╝

Environment Setup Script

EOF
    echo -e "${NC}"
}

show_current_config() {
    header "Current Environment Configuration"

    local all_providers=("OPENAI" "ANTHROPIC" "GEMINI" "OPENROUTER" "OLLAMA")
    local configured_count=0

    for provider_prefix in "${all_providers[@]}"; do
        local api_key_var="${provider_prefix}_API_KEY"
        local base_url_var="${provider_prefix}_BASE_URL"

        local api_key="${!api_key_var:-}"
        local base_url="${!base_url_var:-}"

        if [[ -n "$api_key" || -n "$base_url" ]]; then
            ((configured_count++))
            local status="✅ Configured"
            if [[ -n "$api_key" ]]; then
                local masked_key="${api_key:0:8}...${api_key: -4}"
                echo -e "  ${GREEN}$provider_prefix - $status (Key: $masked_key)${NC}"
            else
                echo -e "  ${GREEN}$provider_prefix - $status (URL: $base_url)${NC}"
            fi
        else
            echo -e "  ${RED}$provider_prefix - ❌ Not configured${NC}"
        fi
    done

    if [[ $configured_count -eq 0 ]]; then
        warning "No providers are currently configured"
    else
        success "$configured_count provider(s) currently configured"
    fi
}

show_provider_menu() {
    header "Select AI Provider to Configure"

    local menu_items=()
    local index=1

    for provider in "${!PROVIDER_NAMES[@]}"; do
        echo "$index. ${PROVIDER_NAMES[$provider]}"
        menu_items+=("$provider")
        ((index++))
    done

    echo -e "${YELLOW}$index. Configure Multiple Providers${NC}"
    echo -e "${RED}0. Exit${NC}"

    while true; do
        read -p $'\nEnter your choice (0-'"$index"'): ' selection

        if [[ "$selection" == "0" ]]; then
            return 1
        elif [[ "$selection" == "$index" ]]; then
            SELECTED_PROVIDER="multiple"
            return 0
        elif [[ "$selection" =~ ^[0-9]+$ ]] && [[ $selection -ge 1 && $selection -lt $index ]]; then
            SELECTED_PROVIDER="${menu_items[$((selection-1))]}"
            return 0
        else
            warning "Invalid selection. Please choose 0-$index."
        fi
    done
}

validate_api_key() {
    local provider="$1"
    local api_key="$2"

    case "$provider" in
        "openai")
            [[ "$api_key" =~ ^sk-[a-zA-Z0-9]{48,}$ ]]
            ;;
        "anthropic")
            [[ "$api_key" =~ ^sk-ant-[a-zA-Z0-9_-]{95,}$ ]]
            ;;
        "openrouter")
            [[ "$api_key" =~ ^sk-or-[a-zA-Z0-9_-]{50,}$ ]]
            ;;
        "gemini")
            [[ "$api_key" =~ ^[a-zA-Z0-9_-]{39}$ ]]
            ;;
        *)
            [[ ${#api_key} -gt 10 ]]
            ;;
    esac
}

test_provider_connection() {
    local provider="$1"
    local test_url="${PROVIDER_TEST_URLS[$provider]}"

    info "Testing connection to $provider..."

    if command -v curl &> /dev/null; then
        if curl -s --max-time 5 --head "$test_url" &> /dev/null; then
            return 0
        else
            return 1
        fi
    elif command -v wget &> /dev/null; then
        if wget --timeout=5 --spider "$test_url" &> /dev/null; then
            return 0
        else
            return 1
        fi
    else
        warning "Neither curl nor wget available for connection testing"
        return 1
    fi
}

read_secure() {
    local prompt="$1"
    local value

    echo -n "$prompt"
    read -s value
    echo
    echo "$value"
}

configure_provider() {
    local provider="$1"
    local provider_name="${PROVIDER_NAMES[$provider]}"

    header "Configuring $provider_name"

    if [[ "$provider" != "ollama" ]]; then
        info "Get your API key from: ${PROVIDER_KEY_URLS[$provider]}"
    fi

    local api_key_var="${PROVIDER_API_KEYS[$provider]}"
    local model_var="${provider^^}_DEFAULT_MODEL"
    local base_url_var="${provider^^}_BASE_URL"

    # Configure API key (if applicable)
    if [[ -n "$api_key_var" ]]; then
        local current_key="${!api_key_var:-}"

        if [[ -n "$current_key" ]]; then
            info "Current API key for $api_key_var is set"
            read -p "Keep existing API key? (y/n) [y]: " keep_existing
            keep_existing="${keep_existing:-y}"

            if [[ "$keep_existing" != "y" ]]; then
                current_key=""
            fi
        fi

        while [[ -z "$current_key" ]]; do
            current_key=$(read_secure "$api_key_var (required): ")

            if [[ -z "$current_key" ]]; then
                warning "API key is required for $provider"
                continue
            fi

            if ! validate_api_key "$provider" "$current_key"; then
                warning "API key format appears invalid for $provider. Continue anyway? (y/n)"
                read -p "> " continue_anyway
                if [[ "$continue_anyway" != "y" ]]; then
                    current_key=""
                    continue
                fi
            fi

            break
        done

        # Set the API key
        export "$api_key_var"="$current_key"
    fi

    # Configure default model
    local current_model="${!model_var:-}"
    local default_model="${PROVIDER_DEFAULT_MODELS[$provider]}"

    read -p "$model_var [default: $default_model]: " new_model
    new_model="${new_model:-$default_model}"
    export "$model_var"="$new_model"

    # Configure base URL
    local current_base_url="${!base_url_var:-}"
    local default_base_url="${PROVIDER_BASE_URLS[$provider]}"

    read -p "$base_url_var [default: $default_base_url]: " new_base_url
    new_base_url="${new_base_url:-$default_base_url}"
    export "$base_url_var"="$new_base_url"

    success "Configured $provider_name"
}

set_environment_variables() {
    local provider="$1"
    local api_key_var="${PROVIDER_API_KEYS[$provider]}"
    local model_var="${provider^^}_DEFAULT_MODEL"
    local base_url_var="${provider^^}_BASE_URL"

    header "Setting Environment Variables"

    # Determine shell profile file
    local profile_file=""
    if [[ -n "${BASH_VERSION:-}" ]]; then
        if [[ -f ~/.bash_profile ]]; then
            profile_file=~/.bash_profile
        elif [[ -f ~/.bashrc ]]; then
            profile_file=~/.bashrc
        fi
    elif [[ -n "${ZSH_VERSION:-}" ]]; then
        profile_file=~/.zshrc
    else
        # Try to detect shell from $SHELL variable
        case "${SHELL:-}" in
            */bash)
                profile_file=~/.bashrc
                ;;
            */zsh)
                profile_file=~/.zshrc
                ;;
            *)
                profile_file=~/.profile
                ;;
        esac
    fi

    # Set variables for current session (already done in configure_provider)
    local vars_to_set=("$model_var" "$base_url_var")
    if [[ -n "$api_key_var" ]]; then
        vars_to_set=("$api_key_var" "${vars_to_set[@]}")
    fi

    for var in "${vars_to_set[@]}"; do
        success "Set $var for current session"
    done

    if [[ "$PERMANENT" == true && -n "$profile_file" ]]; then
        info "Adding variables to $profile_file"

        for var in "${vars_to_set[@]}"; do
            local value="${!var}"
            local export_line="export $var=\"$value\""

            # Remove existing line if it exists
            if [[ -f "$profile_file" ]]; then
                grep -v "^export $var=" "$profile_file" > "${profile_file}.tmp" || true
                mv "${profile_file}.tmp" "$profile_file"
            fi

            # Add new line
            echo "$export_line" >> "$profile_file"
            success "Added $var to $profile_file permanently"
        done

        info "Restart your terminal or run 'source $profile_file' to load the variables"
    elif [[ "$PERMANENT" == true ]]; then
        warning "Could not determine shell profile file for permanent storage"
    else
        warning "Variables are set for current session only. Use --permanent flag to make them persistent."
    fi
}

test_configuration() {
    local provider="$1"
    local provider_name="${PROVIDER_NAMES[$provider]}"

    header "Testing Configuration"

    if test_provider_connection "$provider"; then
        success "$provider_name configuration test passed!"
        return 0
    else
        warning "$provider_name configuration test failed. Check your settings."
        return 1
    fi
}

install_pls_alias() {
    header "Installing 'pls' Alias"

    local script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    local please_path="$(dirname "$script_dir")"
    local pls_script_path="$HOME/bin/pls"

    # Create ~/bin directory if it doesn't exist
    mkdir -p "$HOME/bin"

    # Create pls script
    cat > "$pls_script_path" << EOF
#!/bin/bash
cd "$please_path"
dotnet run --project src/Presentation/Please.Console -- "\$@"
EOF

    chmod +x "$pls_script_path"
    success "Created pls script at $pls_script_path"

    # Add ~/bin to PATH if not already there
    local profile_file=""
    if [[ -n "${BASH_VERSION:-}" ]]; then
        if [[ -f ~/.bash_profile ]]; then
            profile_file=~/.bash_profile
        elif [[ -f ~/.bashrc ]]; then
            profile_file=~/.bashrc
        fi
    elif [[ -n "${ZSH_VERSION:-}" ]]; then
        profile_file=~/.zshrc
    else
        case "${SHELL:-}" in
            */bash) profile_file=~/.bashrc ;;
            */zsh) profile_file=~/.zshrc ;;
            *) profile_file=~/.profile ;;
        esac
    fi

    # Check if ~/bin is in PATH
    if [[ ":$PATH:" != *":$HOME/bin:"* ]]; then
        if [[ -n "$profile_file" ]]; then
            echo 'export PATH="$HOME/bin:$PATH"' >> "$profile_file"
            success "Added \$HOME/bin to PATH in $profile_file"
        else
            warning "Could not determine profile file. Manually add \$HOME/bin to your PATH"
        fi
        # Add to current session
        export PATH="$HOME/bin:$PATH"
    else
        success "\$HOME/bin is already in PATH"
    fi

    info "\n🎉 'pls' alias installed! Usage examples:"
    echo -e "  ${CYAN}pls get current time${NC}"
    echo -e "  ${CYAN}pls list running services${NC}"
    echo -e "  ${CYAN}pls create backup script${NC}"
    info "\n📝 Restart your terminal or run 'source $profile_file' to ensure PATH is loaded"
}

show_summary() {
    local alias_installed="$1"
    header "Configuration Complete"

    success "Successfully configured the following providers:"
    for provider in "${CONFIGURED_PROVIDERS[@]}"; do
        echo -e "  ${GREEN}✅ ${PROVIDER_NAMES[$provider]}${NC}"
    done

    if [[ "$alias_installed" == "true" ]]; then
        echo -e "\n  ${GREEN}✅ 'pls' alias installed${NC}"
    fi

    info "\nNext steps:"
    if [[ "$alias_installed" == "true" ]]; then
        echo "  1. Test your configuration with the alias:"
        echo -e "     ${CYAN}pls get current time${NC}"
        echo -e "     ${CYAN}pls echo hello world${NC}"
    else
        echo "  1. Test your configuration:"
        echo "     cd src/Presentation/Please.Console/bin/Debug/net8.0/linux-x64"
        echo "     ./Please.Console 'echo hello world'"
    fi
    echo ""
    echo "  2. Build the application if needed:"
    echo "     dotnet build src/Presentation/Please.Console"
}

show_usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  --provider PROVIDER    Configure specific provider (openai, anthropic, gemini, openrouter, ollama)"
    echo "  --permanent           Set environment variables permanently in shell profile"
    echo "  --install-alias       Install 'pls' alias for easier usage"
    echo "  --help                Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                              # Interactive setup"
    echo "  $0 --provider openai           # Configure OpenAI only"
    echo "  $0 --provider anthropic --permanent  # Configure Anthropic permanently"
    echo "  $0 --install-alias              # Just install the pls alias"
    echo "  $0 --provider ollama --permanent --install-alias  # Full setup"
}

main() {
    # Parse command line arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            --provider)
                SELECTED_PROVIDER="$2"
                shift 2
                ;;
            --permanent)
                PERMANENT=true
                shift
                ;;
            --install-alias)
                INSTALL_ALIAS=true
                shift
                ;;
            --help)
                show_usage
                exit 0
                ;;
            *)
                error "Unknown option: $1"
                show_usage
                exit 1
                ;;
        esac
    done

    show_banner
    show_current_config

    # Validate provider if specified
    if [[ -n "$SELECTED_PROVIDER" && "$SELECTED_PROVIDER" != "multiple" ]]; then
        if [[ ! -v "PROVIDER_NAMES[$SELECTED_PROVIDER]" ]]; then
            error "Invalid provider: $SELECTED_PROVIDER"
            error "Valid providers: ${!PROVIDER_NAMES[*]}"
            exit 1
        fi
    fi

    # Interactive provider selection if not specified and not just installing alias
    if [[ -z "$SELECTED_PROVIDER" && "$INSTALL_ALIAS" != true ]]; then
        if ! show_provider_menu; then
            info "Setup cancelled."
            exit 0
        fi
    fi

    # Configure providers (skip if only installing alias)
    if [[ "$INSTALL_ALIAS" != true || -n "$SELECTED_PROVIDER" ]]; then
        if [[ "$SELECTED_PROVIDER" == "multiple" ]]; then
            header "Multiple Provider Configuration"
            for provider in "${!PROVIDER_NAMES[@]}"; do
                read -p "Configure ${PROVIDER_NAMES[$provider]}? (y/n) [n]: " configure
                if [[ "$configure" == "y" ]]; then
                    configure_provider "$provider"
                    set_environment_variables "$provider"
                    if test_configuration "$provider"; then
                        CONFIGURED_PROVIDERS+=("$provider")
                    fi
                fi
            done
        elif [[ -n "$SELECTED_PROVIDER" ]]; then
            # Single provider configuration
            configure_provider "$SELECTED_PROVIDER"
            set_environment_variables "$SELECTED_PROVIDER"
            if test_configuration "$SELECTED_PROVIDER"; then
                CONFIGURED_PROVIDERS+=("$SELECTED_PROVIDER")
            fi
        fi
    fi

    # Install alias if requested
    local alias_installed="false"
    if [[ "$INSTALL_ALIAS" == true ]]; then
        install_pls_alias
        alias_installed="true"
    elif [[ ${#CONFIGURED_PROVIDERS[@]} -gt 0 ]]; then
        read -p $'\nInstall \'pls\' alias for easier usage? (y/n) [y]: ' install_alias_choice
        install_alias_choice="${install_alias_choice:-y}"
        if [[ "$install_alias_choice" == "y" ]]; then
            install_pls_alias
            alias_installed="true"
        fi
    fi

    # Show summary
    if [[ ${#CONFIGURED_PROVIDERS[@]} -gt 0 || "$alias_installed" == "true" ]]; then
        show_summary "$alias_installed"
    else
        info "No providers were configured successfully."
    fi
}

# Run the script
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi
