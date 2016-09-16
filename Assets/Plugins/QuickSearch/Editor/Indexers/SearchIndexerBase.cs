using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickSearch {

	public abstract class SearchIndexerBase {

		public void NotifyOnStartup () {
			OnStartup();
		}

		public void NotifyOnOpen () {
			OnOpen();
		}

		public void NotifyOnQuery (string query) {
			OnQuery(query);
		}

		public List<ISearchableElement> RequestElements () {
			if (IsActive())
				return GetElements();
			return null;
		}

		protected virtual void OnStartup () {
		}

		protected virtual void OnOpen () {
		}

		protected virtual void OnQuery (string query) {
		}

		protected abstract List<ISearchableElement> GetElements ();

		protected virtual bool IsActive () {
			return true;
		}
	}
}
