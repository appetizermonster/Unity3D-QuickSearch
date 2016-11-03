using System;
using System.Collections;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public sealed class EditorThreadLoop {
		private bool started_ = false;
		private readonly Action loopFunc_ = null;

		private Thread thread_ = null;
		private readonly AutoResetEvent waitHandle_ = new AutoResetEvent(false);
		private readonly int interval_ms_ = 0;

		public EditorThreadLoop (Action loopFunc, int interval_ms = 20) {
			loopFunc_ = loopFunc;
			interval_ms_ = interval_ms;
		}

		public void Start () {
			if (started_)
				return;
			started_ = true;

			EditorApplication.update += Editor_Update;

			thread_ = new Thread(ThreadFunc);
			thread_.Start();
		}

		private void Editor_Update () {
			waitHandle_.Set();
		}

		private void ThreadFunc () {
			while (true) {
				Thread.Sleep(interval_ms_);
				waitHandle_.WaitOne();

				try {
					if (loopFunc_ != null)
						loopFunc_();
				} catch (Exception ex) {
					Debug.LogException(ex);
				}
			}
		}
	}
}