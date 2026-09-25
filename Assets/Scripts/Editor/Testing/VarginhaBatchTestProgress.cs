using System;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Game.Editor.Testing
{
    /// <summary>Progress in batch logs, including the test responsible for a timeout.</summary>
    public sealed class VarginhaBatchTestProgress : ICallbacks
    {
        [InitializeOnLoadMethod]
        private static void Register()
        {
            if (Application.isBatchMode && Array.Exists(Environment.GetCommandLineArgs(), arg => arg == "-runTests"))
                TestRunnerApi.RegisterTestCallback(new VarginhaBatchTestProgress());
        }

        public void RunStarted(ITestAdaptor testsToRun) => Debug.Log("QA run started: " + testsToRun.TestCaseCount);
        public void RunFinished(ITestResultAdaptor result) => Debug.Log("QA run finished: " + result.ResultState);
        public void TestStarted(ITestAdaptor test)
        {
            if (!test.IsSuite) Debug.Log("QA test: " + test.FullName);
        }
        public void TestFinished(ITestResultAdaptor result)
        {
            if (!result.Test.IsSuite) Debug.Log("QA result: " + result.ResultState + " " + result.Test.FullName);
        }
    }
}
