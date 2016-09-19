using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
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
		private static QuickSearchEngine instance_ = null;
		public static QuickSearchEngine Instance { get { return instance_; } }

		static QuickSearchEngine () {
			instance_ = new QuickSearchEngine();
			instance_.Start();
		}

		private const int MAX_RESULT = 20;

		private readonly List<SearchIndexerBase> indexers_ = new List<SearchIndexerBase>();
		private Thread thread_ = null;

		public event Action OnResultUpdate = null;

		public void Start () {
			indexers_.Add(new AssetIndexer());
			indexers_.Add(new SceneIndexer());
			indexers_.Add(new MenuIndexer());

			NotifyStartup();

			thread_ = new Thread(Thread_Worker);
			thread_.Start();

			EditorApplication.update += Editor_OnUpdate;
		}

		private readonly List<ISearchableElement> lastResult_ = new List<ISearchableElement>();

		public void GetLastResult (List<ISearchableElement> outResult) {
			lock (lastResult_) {
				outResult.Clear();
				outResult.AddRange(lastResult_);
			}
		}

		private readonly object notifyLock_ = new object();
		private bool notifyResultUpdate_ = false;

		private void Editor_OnUpdate () {
			lock (notifyLock_) {
				if (!notifyResultUpdate_)
					return;

				if (OnResultUpdate != null)
					OnResultUpdate();

				notifyResultUpdate_ = false;
			}
		}

		private void NotifyStartup () {
			for (var i = 0; i < indexers_.Count; ++i) {
				indexers_[i].NotifyOnStartup();
			}
		}

		public void NotifyOpen () {
			for (var i = 0; i < indexers_.Count; ++i) {
				indexers_[i].NotifyOnOpen();
			}
		}

		public void RequestFindElements (string query) {
			lock (queryLock_) {
				requestedQuery_ = query;
			}
		}

		private readonly object queryLock_ = new object();

		private string requestedQuery_ = null;

		private void Thread_Worker () {
			while (true) {
				Thread.Sleep(20);

				var query = (string)null;
				lock (queryLock_) {
					if (requestedQuery_ == null)
						continue;
					query = requestedQuery_;
					requestedQuery_ = null;
				}

				lock (lastResult_) {
					var parsedQuery = ParseQuery(query);
					Debug.Log("parsed:" + parsedQuery);

					var result = FindElements(parsedQuery);
					lastResult_.Clear();
					lastResult_.AddRange(result);
				}

				lock (notifyLock_) {
					notifyResultUpdate_ = true;
				}
			}
		}

		private readonly Regex endQueryRule_ = new Regex(@"^\.([^\s]+)", RegexOptions.IgnoreCase);

		private string ParseQuery (string query) {
			var match = endQueryRule_.Match(query);
			if (!match.Success)
				return query;

			var newQuery = endQueryRule_.Replace(query, "");
			newQuery += match.Groups[1].Value;

			return newQuery;
		}

		private readonly List<ISearchableElement> tempResult_ = new List<ISearchableElement>();
		private readonly List<ISearchableElement> tempElements_ = new List<ISearchableElement>();

		private readonly List<MatchPair> tempMatchPairs_ = new List<MatchPair>();

		private List<ISearchableElement> FindElements (string query) {
			tempResult_.Clear();
			tempMatchPairs_.Clear();

			var query_lower = query.ToLowerInvariant().Replace(" ", "");

			for (var i = 0; i < indexers_.Count; ++i) {
				var indexer = indexers_[i];
				indexer.NotifyOnQuery(query);

				tempElements_.Clear();
				indexer.RequestElements(tempElements_);
				if (tempElements_.Count <= 0)
					continue;

				CalculateMatchScore(tempElements_, query_lower, tempMatchPairs_);
			}
			tempMatchPairs_.Sort((a, b) => b.score.CompareTo(a.score));

			var resultCount = Mathf.Min(tempMatchPairs_.Count, MAX_RESULT);
			for (var i = 0; i < resultCount; ++i)
				tempResult_.Add(tempMatchPairs_[i].element);

			return tempResult_;
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
			if (cursor < query_lower.Length)
				return sum * 0.25f;
			return sum;
		}
	}
}
