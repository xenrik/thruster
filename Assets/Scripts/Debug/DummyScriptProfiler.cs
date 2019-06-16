/**
 * Dummy profiler that doesn't actually do anything (used when
 * we aren't running in the editor)
 */
public class DummyScriptProfiler : IScriptProfiler {
    public void EndGroup() {
    }

    public void EndMethod() {
    }

    public void Report() {
    }

    public void StartGroup(string groupName) {
    }

    public void StartMethod(string callerFilePath = null, string callerMember = "") {
    }
}