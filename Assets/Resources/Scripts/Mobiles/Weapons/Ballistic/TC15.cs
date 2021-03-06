﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TC15 : Weapon 
{
	public string[] Compatibility = new string[] {"head", "left arm", "right arm", "left leg", "right leg", "left torso", "right torso", "center torso"};
	public TC15()
	{
		Short = "TC-15";
		Long = "A tactical cannon that fires 15cc shells.";
		Capacity = 3;
		RateOfFire = new Dictionary<string,int>() {{"max",1}, {"set",1}};
		Reload = new Dictionary<string,int>() {{"delay", 2}, {"waiting", 0}};
		Ammo = new List<string>() {"15cc Shells"};
		Energy = new Dictionary<string,float>() {{"fire",1.0f}, {"reload",3.0f}};
		SetMass(7.0f);
	}
}
