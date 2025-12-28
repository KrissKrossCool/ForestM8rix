using System.Windows;
using System.Windows.Markup;

[assembly: ThemeInfo(
	ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
									 //(used if a resource is not found in the page,
									 // or application resource dictionaries)
	ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
											  //(used if a resource is not found in the page,
											  // app, or any theme specific resource dictionaries)
)]

// Сопоставляем URL-схему с внутренним пространством имен
//// Важно: имя сборки (ForestM8rix) должно точно совпадать с Output Name проекта
//[assembly: XmlnsDefinition("http://schemas.forestm8rix.com/2025/wpf", "ForestM8rix.Controls")]
//[assembly: XmlnsDefinition("http://schemas.forestm8rix.com/2025/wpf", "ForestM8rix")]

//// Опционально: задаем префикс, который будет предлагать VS (например, <fm:SharpTreeView />)
//[assembly: XmlnsPrefix("http://schemas.forestm8rix.com/2025/wpf", "fm")]

[assembly: XmlnsDefinition("http://schemas.forestm8rix.com/2025/wpf", "ForestM8rix")]