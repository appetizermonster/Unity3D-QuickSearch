using System.Collections;
using UnityEngine;

namespace QuickSearch {

	public interface ISearchableElement {
		string PrimaryContents { get; }
		string SecondaryContents { get; }
		float Priority { get; }

		Texture2D Icon { get; }
		string Title { get; }
		string Description { get; }

		void Select ();

		void Execute ();
	}
}
