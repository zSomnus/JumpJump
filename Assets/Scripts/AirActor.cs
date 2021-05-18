using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirActor : Actor
{
    protected override void OnEnable()
    {
        base.OnEnable();
        rb.gravityScale = 0;
    }
    protected override void OnStart()
    {
        base.OnStart();
    }

    // Update is called once per frame
    protected override void Update()
    {

        base.Update();
    }

    
}
