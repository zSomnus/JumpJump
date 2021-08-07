using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuggleEnemyState : EnemyState
{
    public override void OnEnter()
    {
        base.OnEnter();
        Controller.Juggle();
    }
}
