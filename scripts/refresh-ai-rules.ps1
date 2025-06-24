# Sync .github\AiRules with C:\Code\AiRules using robocopy

$source = "C:\Code\AiRules"
$destination = ".\.github\AiRules"

# Create destination folder if it doesn't exist
if (!(Test-Path -Path $destination)) {
	New-Item -ItemType Directory -Path $destination | Out-Null
}

# Run robocopy to mirror the source to the destination
robocopy $source $destination /MIR /COPY:DAT /R:2 /W:5 /NFL /NDL /NP /LOG:robocopy.log

if ($LASTEXITCODE -le 3) {
	Write-Host "Sync completed successfully."
}
else {
	Write-Host "Robocopy reported errors. Check robocopy.log for details."
}