using System.Collections.Generic;
using Assets;
using UnityEngine;
using UnityEngine.AI;

namespace Character
{
    public class State
    {
        public enum STATE
        {
            RUN,
            DANCE,
            FALL
        };

        public enum EVENT
        {
            ENTER,
            UPDATE,
            EXIT
        };

        public STATE name;
        public EVENT stage;
        protected Animator anim;
        protected ChibiController Controller;
        protected State nextState;

        public State(Animator _anim, ChibiController controller)
        {
            anim = _anim;
            Controller = controller;
            stage = EVENT.ENTER;
        }

        public virtual void Enter() { stage = EVENT.UPDATE; }
        public virtual void Update() { stage = EVENT.UPDATE; }
        public virtual void Exit() { stage = EVENT.EXIT; }

        public State Process()
        {
            if (stage == EVENT.ENTER) Enter();
            if (stage == EVENT.UPDATE) Update();
            if (stage == EVENT.EXIT)
            {
                Exit();
                return nextState;
            }

            return this;
        }
    }
}