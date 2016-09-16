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

			var query_lower = query.ToLowerInvariant().Replace(" ", "");

			for (var i = 0; i < indexers_.Count; ++i) {
				var indexer = indexers_[i];
				indexer.NotifyOnQuery(query);

				var indexerElements = indexer.RequestElements();
				if (indexerElements == null)
					continue;

				CalculateMatchScore(indexerElements, query_lower, cachedMatchPairs_);
			}
			cachedMatchPairs_.Sort((a, b) => b.score.CompareTo(a.score));

			var resultCount = Mathf.Min(cachedMatchPairs_.Count, MAX_RESULT);
			for (var i = 0; i < resultCount; ++i)
				cachedElements_.Add(cachedMatchPairs_[i].element);
			return cachedElements_;
		}

		private void CalculateMatchScore (List<ISearchableElement> elements, string query_lower, List<MatchPair> outToAppend) {
			for (var i = 0; i < elements.Count; ++i) {
				var element = elements[i];
				var score = CalculateMatchScore(element.PrimaryContents.ToLowerInvariant(), query_lower);

				if (element.SecondaryContents != null) {
					var secondaryScore = CalculateMatchScore(element.SecondaryContents.ToLowerInvariant(), query_lower) * 0.9f;
					score = Mathf.Max(score, secondaryScore);
				}

				if (score <= 0.03f)
					continue;

				outToAppend.Add(new MatchPair(element, score));
			}
		}

		private float CalculateMatchScore (string contents_lower, string query_lower) {
			if (query_lower.Length <= 0 || contents_lower.Length <= 0)
				return 0;

			var sum = 0f;
			var score = 5f;
			var cursor = 0;

			for (var i = 0; i < contents_lower.Length; ++i) {
				// if there is no more another query character
				if (cursor >= query_lower.Length) {
					// disadvantage on early break
					sum *= 0.95f;
					break;
				}

				var contentsChr = contents_lower[i];
				var queryChr = query_lower[cursor];
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
			if (sum <= 0f || cursor < query_lower.Length)
				return CalculateMatchScoreFallback(contents_lower, query_lower);
			return sum;
		}

		private readonly List<char> fallbackChars_ = new List<char>(20);

		private float CalculateMatchScoreFallback (string contents_lower, string query_lower) {
			fallbackChars_.Clear();
			fallbackChars_.AddRange(contents_lower.ToCharArray());

			var matchCount = 0;
			for (var i = 0; i < query_lower.Length; ++i) {
				var chr = query_lower[i];
				var idx = fallbackChars_.IndexOf(chr);
				if (idx < 0)
					continue;

				fallbackChars_[idx] = (char)0;
				matchCount++;
			}
			return Mathf.Min(2f, matchCount * 0.1f);
		}
	}
}
