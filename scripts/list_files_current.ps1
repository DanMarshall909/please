# Get all files in current directory
try {
    Get-ChildItem -Path . | Where-Object { ! $_.PSIsContainer } 
} catch {
    Write-Error "Failed to list files. Error: $_"
}