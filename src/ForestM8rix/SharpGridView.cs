using System;
using System.Windows.Controls;
using System.Windows;

namespace ForestM8rix
{
	/// <summary>
	///		GridView para el árbol
	/// </summary>
	public class SharpGridView : GridView
	{
		static SharpGridView()
		{
			ItemContainerStyleKey = new ComponentResourceKey(typeof(SharpTreeView), "GridViewItemContainerStyleKey");
		}

		/// <summary>
		///		Clave de recurso del estilo del contenedor del elemento
		/// </summary>
		public static ResourceKey ItemContainerStyleKey { get; private set; }

		/// <summary>
		///		Clave de estilo predeterminada
		/// </summary>
		protected override object ItemContainerDefaultStyleKey => ItemContainerStyleKey;
	}
}
