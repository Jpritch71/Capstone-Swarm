using System;


public enum ID_Status { started, paused, stopped }

public interface I_State
{
    ID_Status currentStatus { get; }

    void OnStart();
    void OnPaused();
    void Execute();
    void OnStopped();
}

public interface I_GlobalState : I_State
{
    bool IsStateValid();
}

