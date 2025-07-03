using Assets;
using Character;
using Game_Management;
using UnityEngine;

public class Run : State
{
    public Run(Animator _anim, ChibiController controller) : base(_anim, controller)
    {
        name = STATE.RUN;
    }

    public override void Enter()
    {
        anim.SetTrigger("isRunning");
        base.Enter();
    }

    public override void Update()
    {

        if (Controller.IsNearFinishLine())
        {
            nextState = new Dance(anim, Controller);
            Actions.LevelFinished?.Invoke();
            stage = EVENT.EXIT;
        }
        else
            Controller.MoveForward();
    }

    public override void Exit()
    {
        anim.ResetTrigger("isRunning");
        base.Exit();
    }
}