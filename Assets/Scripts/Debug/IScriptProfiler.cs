using System.Runtime.CompilerServices;

public interface IScriptProfiler {
    void StartMethod([CallerFilePath]string callerFilePath = null, [CallerMemberName] string callerMember = "");

    void EndMethod();

    void StartGroup(string groupName);

    void EndGroup();

    void Report();
}