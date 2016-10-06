using UnityEngine;
using System.Collections.Generic;
using System;

public class GrouchoAnimController
{
    public Animator grouchAnim;

    public GrouchoAnimController(Animator animIn)
    {
        grouchAnim = animIn;
    }

    public void StartRunning()
    {
        grouchAnim.Play("GrouchOrkRun");
    }

    public void StartIdle()
    {
        grouchAnim.Play("GrouchOrkIdle");
    }

    public void StartAttack()
    {
        grouchAnim.Play("GrouchOrkAttack");
    }
}


