#!/bin/bash

# Please v6 Installation Script for Linux/macOS
# Downloads and installs the latest release of Please

set -euo pipefail

# Color codes
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

# Default values
VERSION="latest"
INSTALL_PATH=""
PORTABLE=false
FORCE=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --version)
            VERSION="$2"
            shift 2
            ;;
        --install-path)
            INSTALL_PATH="$2"
            shift 2
            ;;
        --portable)
            PORTABLE=true
            shift
            ;;
        --force)
            FORCE=true
            shift
            ;;
        --help)
            echo "Please v6 Installation Script"
            echo ""
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --version VERSION     Install specific version (default: latest)"
            echo "  --install-path PATH   Custom installation path"
            echo "  --portable           Download as portable executable"
            echo "  --force              Force reinstallation"
            echo "  --help               Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0                              # Install latest version"
            echo "  $0 --version v6.0.1             # Install specific version"
            echo "  $0 --portable                   # Download to current directory"
            echo "  $0 --install-path /usr/local/bin # Install to custom path"
            exit 0
            ;;
        *)
            error "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

get_platform_info() {
    local os="unknown"
    local arch="unknown"
    
    # Detect OS
    case "$(uname -s)" in
        Linux*)     os="linux" ;;
        Darwin*)    os="macos" ;;
        *)          os="unknown" ;;
    esac
    
    # Detect architecture
    case "$(uname -m)" in
        x86_64)     arch="x64" ;;
        arm64)      arch="arm64" ;;
        aarch64)    arch="arm64" ;;
        *)          arch="x64" ;;  # Default to x64
    esac
    
    echo "$os-$arch"
}

get_latest_release() {
    if command -v curl &> /dev/null; then
        curl -s https://api.github.com/repos/DanMarshall909/please/releases/latest | grep '"tag_name"' | cut -d'"' -f4
    elif command -v wget &> /dev/null; then
        wget -qO- https://api.github.com/repos/DanMarshall909/please/releases/latest | grep '"tag_name"' | cut -d'"' -f4
    else
        error "Neither curl nor wget is available. Cannot fetch latest release."
        return 1
    fi
}

get_default_install_path() {
    echo "$HOME/.local/bin"
}

download_please() {
    local version="$1"
    local platform="$2"
    local output_path="$3"
    
    local asset_name="please-$platform"
    local download_url="https://github.com/DanMarshall909/please/releases/download/$version/$asset_name"
    
    info "Downloading $asset_name..."
    info "URL: $download_url"
    
    # Create directory if it doesn't exist
    local directory
    directory=$(dirname "$output_path")
    if [[ -n "$directory" && ! -d "$directory" ]]; then
        mkdir -p "$directory"
    fi
    
    # Download the file
    if command -v curl &> /dev/null; then
        if curl -L -o "$output_path" "$download_url"; then
            chmod +x "$output_path"
            return 0
        else
            return 1
        fi
    elif command -v wget &> /dev/null; then
        if wget -O "$output_path" "$download_url"; then
            chmod +x "$output_path"
            return 0
        else
            return 1
        fi
    else
        error "Neither curl nor wget is available for downloading"
        return 1
    fi
}

add_to_path() {
    local path="$1"
    
    # Shell profile files to check/update
    local profile_files=(
        "$HOME/.bashrc"
        "$HOME/.zshrc"
        "$HOME/.profile"
    )
    
    local export_line="export PATH=\"$path:\$PATH\""
    local added=false
    
    # Try to add to existing profile
    for profile_file in "${profile_files[@]}"; do
        if [[ -f "$profile_file" ]]; then
            if ! grep -q "$path" "$profile_file"; then
                echo "" >> "$profile_file"
                echo "$export_line" >> "$profile_file"
                success "Added to $(basename "$profile_file")"
            else
                info "Already in $(basename "$profile_file")"
            fi
            added=true
            break
        fi
    done
    
    # Create .profile if no shell config exists
    if [[ "$added" == false ]]; then
        echo "$export_line" >> "$HOME/.profile"
        success "Created .profile with PATH export"
    fi
}

main() {
    header "Please v6 Installation Script"
    
    # Get platform information
    local platform
    platform=$(get_platform_info)
    info "Detected platform: $platform"
    
    # Determine version to install
    local target_version="$VERSION"
    if [[ "$VERSION" == "latest" ]]; then
        info "Fetching latest release information..."
        target_version=$(get_latest_release)
        if [[ -z "$target_version" ]]; then
            error "Could not determine latest version"
            exit 1
        fi
    fi
    info "Target version: $target_version"
    
    # Determine installation path
    local install_dir
    local install_path
    
    if [[ "$PORTABLE" == true ]]; then
        install_dir="."
        install_path="./please"
        info "Portable mode - downloading to current directory"
    else
        install_dir="${INSTALL_PATH:-$(get_default_install_path)}"
        install_path="$install_dir/please"
        info "Installation directory: $install_dir"
    fi
    
    # Check if already installed
    if [[ -f "$install_path" && "$FORCE" != true ]]; then
        warning "Please is already installed at $install_path"
        info "Use --force to reinstall or --portable to download to current directory"
        
        # Test the existing installation
        if "$install_path" --version &>/dev/null; then
            local current_version
            current_version=$("$install_path" --version 2>/dev/null || echo "unknown")
            info "Current installation: $current_version"
        else
            warning "Existing installation may be corrupted"
        fi
        
        exit 0
    fi
    
    # Download Please
    header "Downloading Please $target_version"
    if ! download_please "$target_version" "$platform" "$install_path"; then
        error "Download failed"
        exit 1
    fi
    
    success "Downloaded Please to $install_path"
    
    # Test the installation
    info "Testing installation..."
    if version_output=$("$install_path" --version 2>&1); then
        success "Installation test passed: $version_output"
    else
        error "Installation test failed: $version_output"
        exit 1
    fi
    
    # Add to PATH if not portable
    if [[ "$PORTABLE" != true ]]; then
        header "Configuring PATH"
        add_to_path "$install_dir"
    fi
    
    # Installation complete
    header "Installation Complete!"
    success "Please $target_version installed successfully"
    
    if [[ "$PORTABLE" == true ]]; then
        info "Portable installation ready:"
        echo -e "  ${CYAN}./please get current time${NC}"
        echo -e "  ${CYAN}./please --help${NC}"
    else
        info "Please is now available system-wide:"
        echo -e "  ${CYAN}please get current time${NC}"
        echo -e "  ${CYAN}please list running services${NC}"
        echo -e "  ${CYAN}please --help${NC}"
        warning "You may need to restart your terminal for PATH changes to take effect"
    fi
    
    info "\nNext steps:"
    echo -e "  ${BLUE}1. Configure an AI provider (run: please --help)${NC}"
    echo -e "  ${BLUE}2. Start generating scripts with natural language!${NC}"
}

# Run the installer
main