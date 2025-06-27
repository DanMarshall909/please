# 🪟 Windows Setup for `pls` Command

The `pls` alias is now available for Windows! You can use it in both Command Prompt and PowerShell.

## ✅ Ready to Use

The `pls` command is already set up and available in:
- **Command Prompt** (`cmd.exe`)
- **PowerShell** (both Windows PowerShell and PowerShell 7+)
- **Windows Terminal**

## 🚀 Usage Examples

Open any Windows terminal and try:

```cmd
pls get current time
pls list running services
pls create backup script for my documents
pls find files older than 7 days
pls show disk usage
pls restart print spooler service
```

## 📁 Files Created

- `C:\Code\please\pls.cmd` - Batch file for Command Prompt
- `C:\Code\please\pls.ps1` - PowerShell script 
- `C:\Code\please` is already in your Windows PATH

## 🔧 How It Works

Both scripts:
1. Navigate to the Please project directory
2. Run `dotnet run --project src/Presentation/Please.Console -- [your request]`
3. Return to your original directory

## 🐛 Troubleshooting

If `pls` command is not found:
1. **Restart your terminal** - PATH changes require a new session
2. **Check PATH** - Run: `echo %PATH%` (cmd) or `$env:PATH` (PowerShell)
3. **Verify location** - Ensure `C:\Code\please` is in your PATH
4. **Run directly** - Try: `C:\Code\please\pls.cmd get current time`

## 🎉 Success!

You now have the `pls` command available across all your Windows terminals. Enjoy using natural language to generate PowerShell scripts!