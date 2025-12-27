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

//[assembly: XmlnsDefinition("http://schemas.forestm8rix.com/wpf", "ForestM8rix.Core.Controls")]


  // [TAG] Сопоставление C# пространства имен с коротким URL для XAML
 [assembly: XmlnsDefinition("http://schemas.forestm8rix.com/2025/wpf", "ForestM8rix.Core")]
 [assembly: XmlnsDefinition("http://schemas.forestm8rix.com/2025/wpf", "ForestM8rix.Core.Controls")]
 
  // [TAG] Назначение префикса по умолчанию (например, fm: вместо sd:)
 [assembly: XmlnsPrefix("http://schemas.forestm8rix.com/2025/wpf", "fm")]