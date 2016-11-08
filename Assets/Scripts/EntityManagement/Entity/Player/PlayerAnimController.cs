using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerAnimController
{
    public Animation playerAnim;

    public PlayerAnimController(Animation animIn)
    {
        playerAnim = animIn;
    }

    public void StartRunning()
    {
        playerAnim.Play("run");
    }

    public void StartIdle()
    {
        playerAnim.Play("idle");
    }

    public void StartAttack()
    {
        playerAnim.Play("attack");
    }
}



