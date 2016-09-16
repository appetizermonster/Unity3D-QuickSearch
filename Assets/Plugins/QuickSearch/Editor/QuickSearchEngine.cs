using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public struct MatchPair {
		public ISearchableElement element;
		public float score;

		public MatchPair (ISearchableElement element, float score) {
			this.element = element;
			this.score = score;
		}
	}

	[InitializeOnLoad]
	public class QuickSearchEngine {
		private const int MAX_RESULT = 20;

		private static QuickSearchEngine instance_ = null;
		public static QuickSearchEngine Instance { get { return instance_; } }

		static QuickSearchEngine () {
			instance_ = new QuickSearchEngine();
			instance_.Initialize();
		}

		private readonly List<SearchIndexerBase> indexers_ = new List<SearchIndexerBase>();

		private void Initialize () {
			indexers_.Add(new AssetIndexer());
			indexers_.Add(new SceneIndexer());
			indexers_.Add(new MenuIndexer());

			EmitStartup();
		}

		private void EmitStartup () {
			for (var i = 0; i < indexers_.Count; ++i) {
				indexers_[i].NotifyOnStartup();
			}
		}

		public void EmitOpen () {
			for (var i = 0; i < indexers_.Count; ++i) {
				indexers_[i].NotifyOnOpen();
			}
		}

		private readonly List<ISearchableElement> cachedElements_ = new List<ISearchableElement>();
		private readonly List<MatchPair> cachedMatchPairs_ = new List<MatchPair>();

		public List<ISearchableElement> FindElements (string query) {
			cachedElements_.Clear();
			cachedMatchPairs_.Clear();

			for (var i = 0; i < indexers_.Count; ++i) {
				var indexer = indexers_[i];
				indexer.NotifyOnQuery(query);

				var indexerElements = indexer.RequestElements();
				if (indexerElements == null)
					continue;

				CalculateMatchScore(indexerElements, query, cachedMatchPairs_);
			}
			cachedMatchPairs_.Sort((a, b) => b.score.CompareTo(a.score));

			var resultCount = Mathf.Min(cachedMatchPairs_.Count, MAX_RESULT);
			for (var i = 0; i < resultCount; ++i)
				cachedElements_.Add(cachedMatchPairs_[i].element);
			return cachedElements_;
		}

		private void CalculateMatchScore (List<ISearchableElement> elements, string query, List<MatchPair> outToAppend) {
			for (var i = 0; i < elements.Count; ++i) {
				var element = elements[i];
				var score = CalculateMatchScore(element.PrimaryContents, query);

				if (element.SecondaryContents != null) {
					var secondaryScore = CalculateMatchScore(element.SecondaryContents, query) * 0.9f;
					score = Mathf.Max(score, secondaryScore);
				}

				if (score <= 0.03f)
					continue;

				outToAppend.Add(new MatchPair(element, score));
			}
		}

		private float CalculateMatchScore (string contents, string query) {
			if (query.Length <= 0 || contents.Length <= 0)
				return 0;

			var sum = 0f;
			var score = 5f;
			var cursor = 0;

			for (var i = 0; i < contents.Length; ++i) {
				// if there is no more another query character
				if (cursor >= query.Length) {
					// disadvantage on early break
					sum *= 0.95f;
					break;
				}

				var contentsChr = char.ToLowerInvariant(contents[i]);
				var queryChr = char.ToLowerInvariant(query[cursor]);

				if (queryChr == ' ') {
					cursor += 1;
					continue;
				}

				if (contentsChr == ' ')
					continue;

				if (contentsChr == queryChr) {
					score *= 2f;
					sum += score;

					cursor += 1;
				} else {
					score *= 0.8f;
					score = Mathf.Max(score, 3f);
				}
			}
			// if query has rest characters
			if (cursor < query.Length)
				return 0f;
			return sum;
		}
	}
}
