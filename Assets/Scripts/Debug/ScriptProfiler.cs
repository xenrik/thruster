using System.Data.Common;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEngine;

public class ScriptProfiler {
    private static IScriptProfiler profiler;

    private static IScriptProfiler GetInstance() {
        if (profiler == null) {
            if (Application.isEditor) {
                profiler = new DefaultScriptProfiler();
            } else {
                profiler = new DummyScriptProfiler();
            }
        }

        return profiler;
    }

    public static void StartMethod([CallerFilePath]string callerFilePath = null, [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLineNo = 0) {
        GetInstance().StartMethod(callerFilePath, callerMember, callerLineNo);
    }

    public static void EndMethod() {
        GetInstance().EndMethod();    
    }

    public static void StartGroup(string groupName, [CallerFilePath]string callerFilePath = null, [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLineNo = 0) {
        GetInstance().StartGroup(groupName, callerFilePath, callerMember, callerLineNo);
    }

    public static void EndGroup() {
        GetInstance().EndGroup();
    }

    public static void Report(ReportMode mode = ReportMode.Tree) {
        GetInstance().Report(mode);
    }
}