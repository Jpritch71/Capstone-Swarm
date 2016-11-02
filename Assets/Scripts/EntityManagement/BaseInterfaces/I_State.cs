using System;


public enum ID_Status { started, paused, stopped }

public interface I_State
{
    ID_Status currentStatus { get; }

    void Execute();

    void OnStart();   
    void OnStopped();
    void OnPause();
    void OnResume();
}

public interface I_GlobalState : I_State
{
    bool IsStateValid();
}

