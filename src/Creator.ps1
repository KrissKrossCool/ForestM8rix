# Запускать внутри папки /src/
$corePath = "ForestM8rix.Core"
$benchPath = "ForestM8rix.Benchmarks"

# 1. Создаем только новые подпапки
New-Item -ItemType Directory -Force -Path "$corePath/Interfaces", "$corePath/Services", "$benchPath/Benchmarks"

# 2. Генерируем интерфейс
$interfaceCode = @"
namespace ForestM8rix.Core.Interfaces
{
    public interface IForestM8rixNode
    {
        string DisplayName { get; }
        System.Collections.Generic.IEnumerable<IForestM8rixNode>? Children { get; }
        bool IsVisible { get; set; }
        bool IsExpanded { get; set; }
    }
}
"@
$interfaceCode | Out-File -FilePath "$corePath/Interfaces/IForestM8rixNode.cs" -Encoding utf8

# 3. Генерируем заглушку сервиса
$serviceCode = @"
using ForestM8rix.Core.Interfaces;
using System.Collections.Generic;

namespace ForestM8rix.Core.Services
{
    public class ForestFilterService
    {
        public void ApplyFilter(IEnumerable<IForestM8rixNode> nodes, string query)
        {
            // Логика будет здесь
        }
    }
}
"@
$serviceCode | Out-File -FilePath "$corePath/Services/ForestFilterService.cs" -Encoding utf8

Write-Host "Папки Interfaces и Services добавлены в ForestM8rix.Core" -ForegroundColor Cyan