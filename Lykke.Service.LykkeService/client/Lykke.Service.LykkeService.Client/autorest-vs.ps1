# Run this file if your use Execute as Script command of Visual Studio's PowerShell Tools extension
autorest -Input http://localhost:5000/swagger/v1/swagger.json -CodeGenerator CSharp -OutputDirectory ./client/Lykke.Service.LykkeService.Client/AutorestClient -Namespace Lykke.Service.LykkeService.Client.AutorestClient