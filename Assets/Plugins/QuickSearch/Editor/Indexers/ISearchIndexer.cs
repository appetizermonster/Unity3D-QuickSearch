using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickSearch {

	public interface ISearchIndexer {

		void OnStartup ();

		void OnOpen ();

		void OnQuery (string query);

		List<ISearchableElement> GetElements ();
	}
}
