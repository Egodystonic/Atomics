// (c) Egodystonic Studios 2018


using System;
using System.Threading;

namespace Egodystonic.Atomics.Tests.Harness {
	sealed partial class ConcurrentTestCaseRunner<T> {
		class ThreadEntryPointParametersWrapper {
			public const string ExceptionSourceKey = "Exception Source";
			readonly string _threadDescription;
			readonly T _contextObject;
			readonly Action<T> _setUpAction;
			readonly Action<T> _executionAction;
			readonly Action<T> _tearDownAction;
			readonly Barrier _stageSyncBarrier;
			readonly CancellationToken _cancellationToken;
			readonly Action<Exception> _errorReportAction;

			public ThreadEntryPointParametersWrapper(string threadDescription, T contextObject, Action<T> setUpAction, Action<T> executionAction, Action<T> tearDownAction, Barrier stageSyncBarrier, CancellationToken cancellationToken, Action<Exception> errorReportAction) {
				_threadDescription = threadDescription;
				_contextObject = contextObject;
				_setUpAction = setUpAction;
				_executionAction = executionAction;
				_tearDownAction = tearDownAction;
				_stageSyncBarrier = stageSyncBarrier;
				_cancellationToken = cancellationToken;
				_errorReportAction = errorReportAction;
			}

			public void Execute() {
				try {
					var continueExecution = true;
					try {
						_setUpAction?.Invoke(_contextObject);
					}
					catch (Exception e) {
						e.Data.Add(ExceptionSourceKey, $"Thread '{_threadDescription}' during setup");
						_errorReportAction(e);
						continueExecution = false;
					}
					finally {
						_stageSyncBarrier.SignalAndWait(_cancellationToken);
					}

					try {
						if (continueExecution) _executionAction?.Invoke(_contextObject);
					}
					catch (Exception e) {
						e.Data.Add(ExceptionSourceKey, $"Thread '{_threadDescription}' during execution");
						_errorReportAction(e);
						continueExecution = false;
					}
					finally {
						_stageSyncBarrier.SignalAndWait(_cancellationToken);
					}

					try {
						if (continueExecution) _tearDownAction?.Invoke(_contextObject);
					}
					catch (Exception e) {
						e.Data.Add(ExceptionSourceKey, $"Thread '{_threadDescription}' during teardown");
						_errorReportAction(e);
					}
					finally {
						_stageSyncBarrier.SignalAndWait(_cancellationToken);
					}
				}
				catch (OperationCanceledException) { }
				catch (Exception e) {
					e.Data.Add(ExceptionSourceKey, $"Thread '{_threadDescription}' during unknown time");
					_errorReportAction(e);
				}
			}
		}
	}
}