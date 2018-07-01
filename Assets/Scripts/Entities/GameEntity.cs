using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoWorldUnity3D;
using UnityEngine;

public abstract class GameEntity : GoWorldUnity3D.ClientEntity
{
    private double targetMoveTime;
    private bool firstUpdatePos = true;

    protected override void OnUpdatePosition(Vector3 pos)
    {
        GoWorldLogger.Info("GameEntity", "{0} is moving to {1}", this, pos);
        this.targetMoveTime = Time.time + 0.1;
        if (firstUpdatePos)
        {
            base.OnUpdatePosition(pos);
            firstUpdatePos = false;
        }
    }

    void Update()
    {
        this.updateTransformPosition();
    }

    private void updateTransformPosition()
    {
        float leftTime = (float)(this.targetMoveTime - Time.time);
        if (leftTime < 0)
        {
            leftTime = 0;
        }

        transform.position = Vector3.Lerp(transform.position, this.Position, Time.deltaTime / (Time.deltaTime + leftTime));
    }

    protected override void Tick()
    {
        this.updateTransformPosition();
    }
}