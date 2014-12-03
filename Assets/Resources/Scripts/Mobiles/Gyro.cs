﻿using UnityEngine;
using System.Collections;

public class Gyro : Component {
	public float Stabilization;

    public override string GetSystem()
    {
        return "gyro";
    }

	public float GetStabilization()
	{
		return Stabilization;
	}

	public override void Interval()
	{
		if(GetStatus() == STATUS_OK)
			Installed.Master.Stabilization += Stabilization;
		base.Interval();
  	}
}
