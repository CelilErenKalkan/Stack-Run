using Assets;
using Character;
using UnityEngine;

public class Dance : State
{
    public Dance(Animator _anim, ChibiController controller) : base(_anim, controller)
    {
        name = STATE.DANCE;
    }

    public override void Enter()
    {
        anim.SetTrigger("isDancing");
        base.Enter();
    }

    public override void Update()
    {
        if (GameManager.Instance.isLevelStarted)
        {
            nextState = new Run(anim, Controller);
            stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        anim.ResetTrigger("isDancing");
        base.Exit();
    }
}