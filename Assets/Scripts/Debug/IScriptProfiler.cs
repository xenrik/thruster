using System.Runtime.CompilerServices;

public enum ReportMode { Tree, Method }
public interface IScriptProfiler {
    
    void StartMethod([CallerFilePath]string callerFilePath = null, [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLineNo = 0);

    void EndMethod();

    void StartGroup(string groupName, [CallerFilePath]string callerFilePath = null, [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLineNo = 0);

    void EndGroup();

    void Report(ReportMode mode = ReportMode.Tree);
}