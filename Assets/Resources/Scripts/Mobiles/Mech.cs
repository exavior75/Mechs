using UnityEngine;
using System.Collections.Generic;

public class Mech : Mobile {
	public static List<string> Limbs = new List<string>() {"head", "left arm", "right arm", "left leg", "right leg", "left torso", "right torso", "center torso"};
	public Dictionary<string,Dictionary<string,int>> HitTable = new Dictionary<string,Dictionary<string,int>>();
	public Dictionary<string,Dictionary<string,float>> Proportion = new Dictionary<string,Dictionary<string,float>>();
	public Dictionary<string,List<Component>> Components = new Dictionary<string,List<Component>>();
	public Dictionary<string,Dictionary<string,Armor>> Armors = new Dictionary<string,Dictionary<string,Armor>>();
	public Dictionary<string,int> Speed = new Dictionary<string,int>() {{"jump", 0}, {"walk", 0}, {"run", 0}, {"momentum", 0}, {"moved", 0}};
	private string Posture = "stand";
	public int Size = 0;//1: infantry, 2: suit, 3: car, 4: tank, 5: light mech, 6: medium mech, 7: heavy mech, 8: small structure, 9: large structure, 10: tile
	private float Mass = 0.0f;
	private Chassis InternalStructure;
	public Pilot PilotOb;
	public Weapon SelectedWeapon;
	public Vector3 Pos;

	public Mech() 
	{
		foreach(KeyValuePair<string,List<Component>> type in Components)
		{
			foreach(Component item in type.Value)
				Debug.Log(type.Key+": "+item);
			Debug.Log(Proportion[type.Key]["mass"]+"/"+Proportion[type.Key]["max mass"]);
		}
		//Hardcode movement temporarily
		Speed["jump"] = 1;//Actually jump jet based
		Speed["walk"] = 3;//Energy/reactor based
		Speed["run"] = 5;
	}

	public void SetPilot(Pilot pilot)
	{
		PilotOb = pilot;
	}

	public void SetMass(float mass, Chassis chassis)
	{
		if(mass % 0.25f > 0.0f)
			Debug.LogError("Mech mass must be in increments of 0.25.");
		float totalMassRem = 100.0f;//Total mech mass
		float totalIntRem = totalMassRem * chassis.Internal;//Total internal structure portion
		totalIntRem -= totalIntRem%0.25f;//Round down to 0.25.
		InternalStructure = chassis;//Set internal
		Mass = mass;//Set mass
		if(mass < 10.0f)
			Debug.LogError("Mech mass must be at least 10.0.");
		else if(mass <= 40.0f)
			Size = 5;
		else if(mass <= 70.0f)
			Size = 6;
		else if(mass <= 100.0f)
			Size = 7;
		else
			Debug.LogError("Mech mass must be at most 100.0.");
		foreach(string item in Proportion.Keys)
		{
			float tmp = Proportion[item]["ratio"] * mass;//Multiply total by limb ratio
			tmp -= tmp%0.25f;//Round down to minimums of 0.25
			Proportion[item]["max mass"] = tmp;//Set limb ratio
			totalMassRem -= tmp;//Reduce total accordingly
			tmp *= chassis.Internal;//Get the portion of internal structure
			tmp -= tmp%0.25f;//Round down to minimums of 0.25
			totalIntRem -= tmp;//Reduce total accordingly
			Armors[item]["internal"] = new Armor(tmp);
			Proportion[item]["mass"] = tmp;//Set mass equal to internal structure as initial
		}
		Proportion["center torso"]["max mass"] += totalMassRem;//Add excess to center torso
		Armors["center torso"]["internal"].AddArmor(totalIntRem);
		Proportion["center torso"]["mass"] += totalIntRem;//Add excess to center torso
	}

	public float AddComponent(string limb, Component part)
	{
		Proportion[limb]["mass"] += part.GetMass();//Increment used mass
		Components[limb].Add(part);//Add to inventory
		part.EventAttach(this, limb);//Attach to mech
		return Proportion[limb]["max mass"] - Proportion[limb]["mass"];
	}

	public void AddArmor(string limb, string loc, float mass)
	{
		if(Armors[limb][loc] == null)
		{
			Armors[limb][loc] = new Armor(mass);
			Proportion[limb]["mass"] += mass;
		}
		else
		{
			Armors[limb][loc].AddArmor(mass);
			Proportion[limb]["mass"] += mass;
		}
	}

	public string GetClass()
	{
		if(Mass < 10.0f)
			return "Ultra-Light";
		else if(Mass <= 20.0f)
			return "Super-Light";
		else if(Mass <= 40.0f)
			return "Light";
		else if(Mass <= 70.0f)
			return "Medium";
		else if(Mass <= 90.0f)
			return "Heavy";
		else// if(Mass <= 100.0f)
			return "Super-Heavy";
	}

	private void Interval()
	{
		Speed["momentum"] = 0;
		Speed["moved"] = 0;
		//Reset firing
	}

	public void OrderMove(GameObject target)
	{
		Debug.Log(isReady);
		Debug.Log(Speed);
		if((isReady == false) || (Speed["moved"] >= Speed["run"]))
			return;//Can't move anymore
		else
		{
			Speed["moved"]++;
			isReady = false;
		}
		base.OrderMove(target);
	}

	public void OrderFire(GameObject target)
	{
		//This needs to set and follow a route, checking targets along the way.
		base.OrderFire(target, SelectedWeapon.Loaded);
	}

	private void EventCharge()
	{

	}

	private void EventCrush()
	{
		
	}

	public void EventMeleeAttack(Mech target, Component limb)
	{
		float result;
		int accuracy = PilotOb.Piloting + limb.MeleePenalty;//kick +0; punch push +1; charge +2
	}

	//MeleeDamage = 1 per 10 tons for punch 1 per 5 tons for kick; reduce by broken actuators
	//Charge is 1 per 10 tons MULTIPLIED by momentum; chargee gets hit back for the same amount depending on relative momentum (factor in angle of hit)
	//... damage is reduced into 5 point clusters (base this on self mass, more for bigger less for smaller)
	//... crushing is the same except attacker hits on top table defender hits on bottom
	//change top and bottom to deal with left and right hits as well... 


	private Ammunition EventRangedAttack(Mech target, Ammunition ammo)
	{
		float result;
		int accuracy = PilotOb.Gunnery;//Initialize at skill
		accuracy += GetRangePenalty();
		accuracy += GetMovementPenalty();
		accuracy += GetAccuracyPenalty();//Lost actuators or other circumstances
		if(target.tag != "Tile")
			accuracy += target.GetComponent<Mech>().GetDodge();//not yet set
		if(accuracy > 11)
			accuracy = 11;
		else if(accuracy < 0)
			accuracy = 0;
		result = Random.Range(0.1f, 100.0f);
		if(result < Engine.Random[accuracy])
			return target.EventDamage(this, ammo);//Might penetrate and hit something past
		else
			return ammo;//Might hit something past
	}

	public Ammunition EventDamage(Mech attacker, Ammunition ammo)
	{
		audio.Play();//TEMP: this should be moved to render
		string table = GetHitTable(attacker);//Get hit table
		string location = "none";
		string side = "external";//Flag to take from ordinary external
		int result, damage, damageR, hardness, crit;
		if(table == "rear")
		{
			side = "rear";//Flag to take from rear armor
			table = "front";//Rear table is same as front in most cases, just need to set the above flag to ensure rear armor is hit
		}
		result = Random.Range(1, 36);
		foreach(KeyValuePair<string,int> item in HitTable[table])
		{//Determine where hit on table
			result -= item.Value;
			if(result <= 0)
			{
				location = item.Key;
				break;//Done
			}
		}
		hardness = Armors[location][side].Hardness[ammo.DamageType];
		if(ammo.Damage < Armors[location][side].Hardness[ammo.DamageType])
		{
			ammo.Damage = 0;
			return ammo;//Armor absorbs the blow, but its ineffective.
		}
		damage = ammo.Damage / hardness;//Convert to relative damage based on armor hardness
		damageR = ammo.Damage % hardness;//The remainder does not necessary truncate
		result = Random.Range(1, hardness);//Chance of it applying depends on how high
		if(result <= damageR)//Check to see if extra damage
			damage++;//Add 1 extra damage
		if(Armors[location][side].HP >= damage)
		{//External armor soaks
			Armors[location][side].HP -= damage;
			ammo.Damage = 0;//Armor absorbs the blow
			return ammo;
		}
		else
		{
			damage -= Armors[location][side].HP;
			ammo.Damage -= Armors[location][side].HP * hardness;//Absorm some of the blow
			Armors[location][side].HP = 0;//Shear off all armor
			if(Armors[location]["internal"].HP >= damage)
			{//Internal armor soaks
				crit = damage;
				Armors[location]["internal"].HP -= damage;
				ammo.Damage = 0;
				return ammo;
			}
			else
			{
				crit = Armors[location]["internal"].HP;
				damage -= Armors[location]["internal"].HP;
				ammo.Damage -= Armors[location]["internal"].HP * hardness;//Absorm some of the blow
				Armors[location]["internal"].HP = 0;//Shear off all armor
				//TEMP: Damage transfer into mech?
			}
		}
		//TEMP: Can fall from damage?
		EventCritical(location, crit);//Check for crits last because of possible ammo explosions
		return ammo;
	}

	private string GetHitTable(Mech other)
	{
		float angle = Vector3.Angle(other.transform.position, transform.position);
		if(angle < 90.0f || angle > 270.0f)
			return "front";
		else if(angle >= 90.0f && angle <= 150.0f)
			return "right";
		else if(angle > 150.0f && angle < 210.0f)
			return "rear";
		else if(angle >= 210.0f && angle <= 270.0f)
			return "left";
		else
			return "none";
	}

	private void EventCritical(string location, int damage)
	{
		float possible = Proportion[location]["max mass"];
		float result;
		for(int i = 0; i < damage; i++)
		{
			result = Random.Range(0.0f, possible);
			foreach(Component item in Components[location])
			{
				result -= item.GetMass();
				if(result <= 0)
					item.EventDamage();
			}
		}
	}

	public void EventManeuver()
	{
		//try is piloting
		//apply actuator issues and other conditionals
		//if success, yay
		//if fail
		//eventFall
	}

	private void EventFall()
	{
		//determine fall FACING
		//determine hull up or down
		//eventDamage from fall

	}

	public int GetDodge()
	{
		if(Speed["momentum"] <= 1)
			return 0;
		else if(Speed["momentum"] <= 3)
			return 1;
		else if(Speed["momentum"] <= 6)
			return 2;
		else if(Speed["momentum"] <= 10)
			return 3;
		else if(Speed["momentum"] <= 15)
			return 4;
		else if(Speed["momentum"] <= 21)
			return 5;
		else if(Speed["momentum"] <= 28)
			return 6;
		else if(Speed["momentum"] <= 36)
			return 7;
		else if(Speed["momentum"] <= 45)
			return 8;
		else
			return 9;
	}

	public int GetAccuracyPenalty()
	{
		//if(Speed["moved"] == 0)
			return 0;//no penalty
	}

	public int GetRangePenalty()
	{
		//if(Speed["moved"] == 0)
			return 0;//no penalty
	}

	public int GetMovementPenalty()
	{
		if(Speed["moved"] == 0)
			return 0;//no penalty
		else if(Speed["moved"] <= Speed["walk"])
			return 1;//moved
		else if(Posture == "jump")
			return 3;//in midair
		else
			return 2;//is running
	}
}
