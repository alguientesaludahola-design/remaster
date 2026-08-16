using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x0200003B RID: 59
public class MapControl : MonoBehaviour
{
	// Token: 0x06000641 RID: 1601 RVA: 0x00043E28 File Offset: 0x00042028
	private void Start()
	{
		ScrewPlatform.camischanging = false;
		MainManager.FixSamira();
		DeadLanderOmega.state = 0;
		DeadLanderOmega.detected = false;
		DeadLanderOmega.activeid = -1;
		DeadLanderOmega.hand = null;
		if (RenderSettings.skybox != null && MainManager.instance.insideid == -1)
		{
			RenderSettings.skybox.SetColor("_Tint", Color.gray);
		}
		if (this.camoffset.magnitude > 0.2f)
		{
			MainManager.instance.camoffset = this.camoffset;
		}
		else
		{
			MainManager.instance.camoffset = MainManager.defaultcamoffset;
		}
		if (this.camangle.magnitude > 0.2f)
		{
			MainManager.instance.camangleoffset = this.camangle;
		}
		else
		{
			MainManager.instance.camangleoffset = MainManager.defaultcamangle;
		}
		if (this.mainmesh == null)
		{
			this.mainmesh = base.transform.GetChild(0);
		}
		if (this.mainrender == null)
		{
			this.mainrender = this.mainmesh.GetComponentInChildren<Renderer>();
		}
		this.musicpreload = new List<AudioClip>();
		if (this.areaid != (MainManager.Areas)MainManager.instance.areaid)
		{
			MainManager.UpdateArea((int)this.areaid);
		}
		this.actualcenter = this.centralpoint;
		if (this.canfollowID != null && this.canfollowID.Length != 0)
		{
			this.tempfollowers = new List<EntityControl>();
		}
		MainManager.instance.battlestage = (int)this.battlemap;
		TextAsset textAsset = Resources.Load<TextAsset>((this.mapid == MainManager.Maps.TestRoom) ? "Data/TestRoom" : string.Concat(new object[]
		{
			"Data/Dialogues",
			MainManager.languageid,
			"/Maps/",
			((this.readdatafromothermap == MainManager.Maps.TestRoom) ? this.mapid : this.readdatafromothermap).ToString()
		}));
		if (textAsset != null)
		{
			this.dialogues = textAsset.ToString().Replace("\r\n", "\n").Split(new char[]
			{
				'\n'
			});
		}
		if (this.useglobalcommand)
		{
			this.commandlines = Resources.Load<TextAsset>("Data/Commands/" + ((this.readdatafromothermap == MainManager.Maps.TestRoom) ? this.mapid : this.readdatafromothermap).ToString()).ToString().Split(new char[]
			{
				'\n'
			});
		}
		this.CreateEntities();
		if (this.skyboxmat != null)
		{
			RenderSettings.skybox = this.skyboxmat;
			if (MainManager.instance.insideid == -1)
			{
				RenderSettings.skybox.SetColor("_Tint", Color.gray);
			}
		}
		else
		{
			RenderSettings.skybox = null;
			MainManager.MainCamera.backgroundColor = Color.black;
		}
		this.GetSkyColor();
		this.originallimitneg = this.camlimitneg;
		this.originallimitpos = this.camlimitpos;
		if (!MainManager.instance.inevent && !this.keepmusic)
		{
			this.Music();
		}
		base.Invoke("CheckDisc", 1f);
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].entity.flowerbed != null)
			{
				Object.Destroy(MainManager.instance.playerdata[i].entity.flowerbed);
			}
		}
		this.render = this.mainmesh.GetComponentsInChildren<MeshRenderer>();
		if (this.mapflags.Length == 0)
		{
			this.mapflags = new bool[10];
		}
		this.GetDigWalls();
		RenderSettings.fogEndDistance = this.fogend;
		RenderSettings.fogColor = this.fogcolor;
		RenderSettings.ambientLight = this.globallight;
		this.SetScreenEffect();
		this.RefreshInsides(false, null);
		MainManager.CheckQuests();
		MainManager.RefreshEntities(true, true);
		MainManager.instance.CheckAchievement();
		this.AreaSpecific();
		MapControl.HelperMedalCheck();
		MainManager.UpdateShops();
		if (this.insidetypes.Length != this.insides.Length)
		{
			this.insidetypes = new MapControl.InsideType[this.insides.Length];
		}
		Shader.SetGlobalFloat("GlobalIceRadius", 0f);
		if (this.mapid == MainManager.Maps.BugariaResidential)
		{
			base.Invoke("CombineMesh", 0.1f);
		}
	}

	// Token: 0x06000642 RID: 1602 RVA: 0x00044240 File Offset: 0x00042440
	public static void HelperMedalCheck()
	{
		if (!MainManager.instance.flags[716] && (MainManager.instance.flags[514] || MainManager.instance.flags[498] || MainManager.instance.flags[610] || MainManager.instance.flags[135] || MainManager.instance.flags[704] || MainManager.instance.flags[391] || MainManager.instance.flags[298] || MainManager.instance.flags[709]))
		{
			MainManager.instance.badgeshops[0].Add(85);
			MainManager.instance.flags[716] = true;
		}
	}

	// Token: 0x06000643 RID: 1603 RVA: 0x00044318 File Offset: 0x00042518
	private void CombineMesh()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Merge");
		if (array.Length != 0)
		{
			MeshFilter[] array2 = new MeshFilter[array.Length];
			CombineInstance[] array3 = new CombineInstance[array2.Length];
			Material[] materials = null;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = array[i].GetComponent<MeshFilter>();
				array3[i].mesh = array2[i].mesh;
				array3[i].transform = array[i].transform.localToWorldMatrix;
				if (i > 0)
				{
					Object.Destroy(array[i]);
				}
				else
				{
					MeshRenderer component = array2[i].GetComponent<MeshRenderer>();
					if (component != null)
					{
						materials = component.materials;
						Object.Destroy(component);
					}
				}
			}
			MeshFilter meshFilter = new GameObject("merged mesh").AddComponent<MeshFilter>();
			meshFilter.mesh = new Mesh();
			meshFilter.mesh.CombineMeshes(array3);
			meshFilter.transform.parent = array2[0].transform;
			meshFilter.transform.localEulerAngles = new Vector3(-90f, 180f);
			MeshRenderer meshRenderer = meshFilter.gameObject.AddComponent<MeshRenderer>();
			meshRenderer.materials = materials;
			meshRenderer.gameObject.isStatic = true;
			List<MeshRenderer> list = new List<MeshRenderer>(this.render);
			meshRenderer.gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
			meshFilter.gameObject.layer = 8;
			list.Add(meshRenderer);
			this.render = list.ToArray();
		}
	}

	// Token: 0x06000644 RID: 1604 RVA: 0x00044494 File Offset: 0x00042694
	private void CheckDisc()
	{
		if (this.discoveryids.Length != 0 && MainManager.BadgeIsEquipped(2) && (this.mapid != MainManager.Maps.TermiteIndustrial || MainManager.player.transform.position.z < 20f))
		{
			for (int i = 0; i < this.discoveryids.Length; i++)
			{
				if (!MainManager.instance.librarystuff[0, this.discoveryids[i]])
				{
					this.hiddenitem = new int?(100);
				}
			}
		}
	}

	// Token: 0x06000645 RID: 1605 RVA: 0x00044514 File Offset: 0x00042714
	public Transform FindByName(string name)
	{
		for (int i = 0; i < this.mainmesh.childCount; i++)
		{
			if (name == this.mainmesh.GetChild(i).name)
			{
				return this.mainmesh.GetChild(i);
			}
		}
		return null;
	}

	// Token: 0x06000646 RID: 1606 RVA: 0x00044560 File Offset: 0x00042760
	public void Music()
	{
		if (this.music.Length == 0)
		{
			MainManager.ChangeMusic(null, 0.1f);
			return;
		}
		if (this.musicflags.Length != 0)
		{
			this.musicid = -1;
			for (int i = this.musicflags.Length - 1; i >= 0; i--)
			{
				if (this.musicflags[i].x == -1 || MainManager.instance.flags[this.musicflags[i].x])
				{
					this.musicid = this.musicflags[i].y;
					break;
				}
			}
		}
		if (this.musicid > -1)
		{
			MainManager.ChangeMusic(this.music[this.musicid], 0.1f);
			MainManager.CheckSamira(this.music[this.musicid]);
			return;
		}
		MainManager.ChangeMusic();
	}

	// Token: 0x06000647 RID: 1607 RVA: 0x00044630 File Offset: 0x00042830
	private void SetPlayerColliders()
	{
		List<Collider> list = new List<Collider>();
		GameObject[] array = GameObject.FindGameObjectsWithTag("EntityOnly");
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				list.Add(array[i].GetComponent<Collider>());
			}
			this.entityonly = list.ToArray();
			for (int j = 0; j < this.entityonly.Length; j++)
			{
				this.entityonly[j].material.staticFriction = 0f;
				this.entityonly[j].material.dynamicFriction = 0f;
			}
			for (int k = 0; k < MainManager.instance.playerdata.Length; k++)
			{
				for (int l = 0; l < array.Length; l++)
				{
					EntityControl.IgnoreColliders(MainManager.instance.playerdata[k].entity, this.entityonly[l], true);
				}
			}
			EntityControl[] array2 = this.tempfollowers.ToArray();
			for (int m = 0; m < array2.Length; m++)
			{
				for (int n = 0; n < array.Length; n++)
				{
					EntityControl.IgnoreColliders(array2[m], this.entityonly[n], true);
				}
			}
		}
	}

	// Token: 0x06000648 RID: 1608 RVA: 0x00044758 File Offset: 0x00042958
	private void GetSkyColor()
	{
		this.skycolor = new Color(this.skycolor.r, this.skycolor.g, this.skycolor.b, 1f);
	}

	// Token: 0x06000649 RID: 1609 RVA: 0x0004478C File Offset: 0x0004298C
	private void AreaSpecific()
	{
		this.Test();
		bool flag = false;
		if (MainManager.instance.areaid != 19 && MainManager.instance.areaid != 15)
		{
			MainManager.instance.flags[401] = false;
		}
		switch (MainManager.instance.areaid)
		{
		case 1:
			if (MainManager.instance.flags[67] && this.canfollowID != null && this.canfollowID.Length != 0 && this.canfollowID.Contains(46))
			{
				List<int> list = new List<int>(this.canfollowID);
				list.Remove(46);
				this.canfollowID = list.ToArray();
			}
			break;
		case 3:
			MainManager.Insert(406, ref this.canfollowID);
			if (!MainManager.instance.librarystuff[0, 30])
			{
				MainManager.UpdateJounal(MainManager.Library.Discovery, 30);
			}
			break;
		case 7:
			if (!MainManager.instance.librarystuff[0, 39])
			{
				MainManager.UpdateJounal(MainManager.Library.Discovery, 39);
			}
			break;
		case 8:
			if (this.mapid != MainManager.Maps.WizardTowerAttic && this.mapid != MainManager.Maps.WizardTowerBasement && this.mapid != MainManager.Maps.WizardTowerStairs && this.mapid != MainManager.Maps.FGCave && this.mapid != MainManager.Maps.FarGrasslandsWizard && this.mapid != MainManager.Maps.FGClearing)
			{
				MainManager.Insert(406, ref this.canfollowID);
				MainManager.Insert(380, ref this.canfollowID);
			}
			RenderSettings.fogColor = Color.Lerp(Color.white, Color.green, 0.15f);
			if (!MainManager.instance.librarystuff[0, 34] && this.mapid != MainManager.Maps.BroodmotherLair && this.mapid != MainManager.Maps.FGCave)
			{
				MainManager.UpdateJounal(MainManager.Library.Discovery, 34);
			}
			break;
		case 9:
			MainManager.Insert(380, ref this.canfollowID);
			if (!MainManager.instance.librarystuff[0, 37])
			{
				MainManager.UpdateJounal(MainManager.Library.Discovery, 37);
			}
			break;
		case 10:
			if (!MainManager.instance.librarystuff[0, 28])
			{
				MainManager.UpdateJounal(MainManager.Library.Discovery, 28);
			}
			break;
		case 11:
			this.nocolorchange = true;
			break;
		case 14:
			MainManager.Insert(406, ref this.canfollowID);
			if (!MainManager.instance.librarystuff[0, 47])
			{
				MainManager.UpdateJounal(MainManager.Library.Discovery, 47);
			}
			break;
		case 15:
			MainManager.instance.flags[401] = this.closemove;
			if (!MainManager.instance.librarystuff[0, 48])
			{
				MainManager.UpdateJounal(MainManager.Library.Discovery, 48);
			}
			break;
		case 19:
		{
			MainManager.Insert(380, ref this.canfollowID);
			MainManager.Maps maps = MainManager.CurrentMap();
			this.closemove = (maps != MainManager.Maps.WaspKingdomOutside && maps != MainManager.Maps.WaspKingdomJayde && maps != MainManager.Maps.WaspKingdomMainHall && maps != MainManager.Maps.WaspKingdomPrison && maps != MainManager.Maps.WaspKingdom5 && maps != MainManager.Maps.WaspKingdomQueen && maps != MainManager.Maps.WaspKingdomThrone);
			this.cantcompass = this.closemove;
			if (MainManager.ColorMagnitude(this.skycolor) > 0.9f)
			{
				this.skycolor = Color.gray;
			}
			if (this.closemove)
			{
				MainManager.instance.flags[401] = true;
			}
			if (!MainManager.instance.librarystuff[0, 38])
			{
				MainManager.UpdateJounal(MainManager.Library.Discovery, 38);
			}
			break;
		}
		case 20:
			this.render[0].material.color = new Color(0.9843137f, 1f, 0.25490198f);
			this.nocolorchange = true;
			flag = true;
			break;
		case 21:
			if (!MainManager.instance.librarystuff[0, 33])
			{
				MainManager.UpdateJounal(MainManager.Library.Discovery, 33);
			}
			break;
		case 22:
			this.render[0].material.color = Color.Lerp(Color.red, Color.yellow, 0.65f);
			this.nocolorchange = true;
			flag = true;
			break;
		}
		MainManager.Maps maps2 = MainManager.CurrentMap();
		if (maps2 <= MainManager.Maps.ChomperCave1)
		{
			if (maps2 <= MainManager.Maps.UndergroundBar)
			{
				if (maps2 <= MainManager.Maps.BugariaOutskirtsSnakemouthCorridor2)
				{
					if (maps2 != MainManager.Maps.AntTunnels)
					{
						if (maps2 == MainManager.Maps.BugariaOutskirtsSnakemouthCorridor2)
						{
							if (MainManager.instance.flags[41])
							{
								MainManager.instance.flags[652] = true;
							}
						}
					}
					else if (!MainManager.instance.librarystuff[0, 21])
					{
						MainManager.UpdateJounal(MainManager.Library.Discovery, 21);
					}
				}
				else if (maps2 != MainManager.Maps.SnakemouthTreasureRoom)
				{
					if (maps2 == MainManager.Maps.UndergroundBar)
					{
						if (!MainManager.instance.librarystuff[0, 20])
						{
							MainManager.UpdateJounal(MainManager.Library.Discovery, 20);
						}
					}
				}
				else
				{
					for (int i = 0; i < MainManager.instance.regionalflags.Length; i++)
					{
						MainManager.instance.regionalflags[i] = false;
					}
				}
			}
			else if (maps2 <= MainManager.Maps.DesertDRSouthEntrance)
			{
				if (maps2 != MainManager.Maps.BeehiveOutside)
				{
					if (maps2 == MainManager.Maps.DesertDRSouthEntrance)
					{
						MainManager.instance.flags[201] = true;
						MainManager.instance.flags[170] = true;
					}
				}
				else if (!MainManager.instance.librarystuff[0, 25])
				{
					MainManager.UpdateJounal(MainManager.Library.Discovery, 25);
				}
			}
			else if (maps2 != MainManager.Maps.FishingVillage)
			{
				if (maps2 == MainManager.Maps.ChomperCave1)
				{
					if (!MainManager.instance.librarystuff[0, 22])
					{
						MainManager.UpdateJounal(MainManager.Library.Discovery, 22);
					}
				}
			}
			else if (!MainManager.instance.librarystuff[0, 35])
			{
				MainManager.UpdateJounal(MainManager.Library.Discovery, 35);
			}
		}
		else if (maps2 <= MainManager.Maps.StreamMountain4)
		{
			if (maps2 <= MainManager.Maps.WizardTowerStairs)
			{
				if (maps2 != MainManager.Maps.SnakemouthTop)
				{
					if (maps2 == MainManager.Maps.WizardTowerStairs)
					{
						if (!MainManager.instance.librarystuff[0, 36])
						{
							MainManager.UpdateJounal(MainManager.Library.Discovery, 36);
						}
					}
				}
				else if (!MainManager.instance.librarystuff[0, 18])
				{
					MainManager.UpdateJounal(MainManager.Library.Discovery, 18);
				}
			}
			else if (maps2 != MainManager.Maps.BarrenLandsRock)
			{
				if (maps2 == MainManager.Maps.StreamMountain4)
				{
					MainManager.instance.flags[316] = false;
				}
			}
			else
			{
				MainManager.instance.flags[452] = true;
			}
		}
		else if (maps2 <= MainManager.Maps.UpperSnekMiddleRoom)
		{
			if (maps2 != MainManager.Maps.StreamMountain5)
			{
				if (maps2 == MainManager.Maps.UpperSnekMiddleRoom)
				{
					if (!MainManager.instance.librarystuff[0, 17])
					{
						MainManager.UpdateJounal(MainManager.Library.Discovery, 17);
					}
				}
			}
			else
			{
				MainManager.instance.flags[492] = false;
			}
		}
		else if (maps2 != MainManager.Maps.RubberPrisonGym)
		{
			if (maps2 != MainManager.Maps.GiantLairBeforeBoss)
			{
				if (maps2 == MainManager.Maps.GiantLairBeforeBoss2)
				{
					MainManager.instance.flags[666] = true;
				}
			}
			else if (MainManager.instance.flags[667] && MainManager.instance.flags[668])
			{
				RenderSettings.fogColor = new Color(0.2f, 0.1f, 0.1f);
			}
		}
		else
		{
			MainManager.instance.flags[535] = false;
		}
		if (this.mapid != MainManager.Maps.WaspKingdom3 && MainManager.instance.flags[364])
		{
			MainManager.instance.flags[657] = true;
		}
		if (flag)
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("CopyMainColor");
			for (int j = 0; j < array.Length; j++)
			{
				Renderer component = array[j].GetComponent<Renderer>();
				if (component != null)
				{
					component.material.color = this.render[0].material.color;
				}
			}
		}
	}

	// Token: 0x0600064A RID: 1610 RVA: 0x00044FA8 File Offset: 0x000431A8
	private void SetScreenEffect()
	{
		MapControl.ScreenEffects screenEffects = this.screeneffect;
		if (screenEffects == MapControl.ScreenEffects.SunRaysTopRight)
		{
			GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Particles/SunRay")) as GameObject;
			gameObject.transform.parent = MainManager.GUICamera.transform;
			gameObject.transform.localPosition = new Vector3(9f, 7f, 10f);
			gameObject.transform.localEulerAngles = Vector3.zero;
		}
	}

	// Token: 0x0600064B RID: 1611 RVA: 0x00045018 File Offset: 0x00043218
	private void GetDigWalls()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("DigWall");
		this.digwall = new Collider[array.Length];
		for (int i = 0; i < this.digwall.Length; i++)
		{
			this.digwall[i] = array[i].GetComponent<Collider>();
		}
		array = GameObject.FindGameObjectsWithTag("Respawn");
		for (int j = 0; j < array.Length; j++)
		{
			array[j].gameObject.layer = 0;
			BoxCollider component = array[j].GetComponent<BoxCollider>();
			if (component != null)
			{
				component.isTrigger = true;
			}
		}
	}

	// Token: 0x0600064C RID: 1612 RVA: 0x000450A4 File Offset: 0x000432A4
	private void SetParticles()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Particable");
		for (int i = 0; i < array.Length; i++)
		{
			if (MainManager.particlelevel == 0)
			{
				array[i].SetActive(false);
			}
		}
	}

	// Token: 0x0600064D RID: 1613 RVA: 0x000450DC File Offset: 0x000432DC
	private void RefreshWind()
	{
		for (int i = 0; i < this.windobjects.Length; i++)
		{
			if (this.windspeed > 0f && this.windintensity > 0f)
			{
				MainManager.RefreshWind(this.windobjects[i]);
			}
		}
	}

	// Token: 0x0600064E RID: 1614 RVA: 0x00045123 File Offset: 0x00043323
	public void RefreshSoundVolume()
	{
		this.RefreshSoundVolume(MainManager.soundvolume);
	}

	// Token: 0x0600064F RID: 1615 RVA: 0x00045130 File Offset: 0x00043330
	public void RefreshSoundVolume(float value)
	{
		SoundControl[] array = Object.FindObjectsOfType<SoundControl>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null && array[i].source != null)
			{
				array[i].source.volume = array[i].startvolume * value;
			}
		}
	}

	// Token: 0x06000650 RID: 1616 RVA: 0x00045184 File Offset: 0x00043384
	private void PreloadSprites()
	{
		this.entitysprite = new List<Texture2D>();
		int num = 0;
		for (int i = 0; i < this.entities.Length; i++)
		{
			try
			{
				if (this.entities[i] != null && !this.entities[i].dead && this.entities[i].originalid > -1 && (this.entities[i].npcdata == null || this.entities[i].npcdata.disguiseobj == null) && this.entities[i].sprite != null && this.entities[i].sprite.sprite.texture != null)
				{
					this.entitysprite.Add(this.entities[i].sprite.sprite.texture);
				}
			}
			catch
			{
				num++;
			}
		}
	}

	// Token: 0x06000651 RID: 1617 RVA: 0x00045290 File Offset: 0x00043490
	private void LateUpdate()
	{
		if (MainManager.player != null)
		{
			if (!this.latestart)
			{
				if (base.name == "0")
				{
					for (int i = 0; i < MainManager.instance.extrafollowers.ToArray().Length; i++)
					{
						MainManager.AddFollower(null, MainManager.instance.extrafollowers.ToArray()[i]);
					}
				}
				else if (this.canfollowID != null && this.canfollowID.Length != 0)
				{
					for (int j = 0; j < this.canfollowID.Length; j++)
					{
						if (MainManager.instance.extrafollowers.Contains(this.canfollowID[j]))
						{
							MainManager.AddFollower(null, this.canfollowID[j]);
							this.tempfollowers.ToArray()[this.tempfollowers.Count - 1].tempfollowerid = j;
							MainManager.AnimIDs animIDs = this.canfollowID[j] + MainManager.AnimIDs.Bee;
							if (animIDs == MainManager.AnimIDs.Maki)
							{
								this.tempfollowers.ToArray()[this.tempfollowers.Count - 1].ccol.height = 3f;
								this.tempfollowers.ToArray()[this.tempfollowers.Count - 1].ccol.center = new Vector3(0f, 1.5f);
							}
							this.tempfollowers.ToArray()[this.tempfollowers.Count - 1].onground = false;
						}
					}
				}
				if (MainManager.instance.flags[402] && !MainManager.player.submarine)
				{
					MainManager.AddFollower(null, 169);
					this.chompy = this.tempfollowers.ToArray()[this.tempfollowers.Count - 1];
					this.chompy.tempfollowerid = this.tempfollowers.Count - 1;
					this.chompy.onground = false;
				}
				this.SetParticles();
				base.Invoke("PreloadSprites", 0.1f);
				if (this.faderchange)
				{
					this.faders = Object.FindObjectsOfType<Fader>();
					this.fss = new bool[this.faders.Length];
					for (int k = 0; k < this.fss.Length; k++)
					{
						this.fss[k] = this.faders[k].enabled;
					}
				}
				base.Invoke("SetPlayerColliders", 0.2f);
				this.latestart = true;
			}
			else
			{
				if (!MainManager.instance.minipause && !MainManager.instance.pause && !MainManager.instance.inevent && !MainManager.instance.message)
				{
					if (this.autoevent.Length != 0)
					{
						for (int l = 0; l < this.autoevent.Length; l++)
						{
							if (!MainManager.instance.flags[(int)this.autoevent[l].x])
							{
								MainManager.events.StartEvent((int)this.autoevent[l].y, null);
								MainManager.instance.flags[(int)this.autoevent[l].x] = true;
							}
						}
					}
					if (this.hiddenitem != null)
					{
						MainManager.player.entity.emoticonid = 4;
						MainManager.player.entity.emoticoncooldown = 100f;
						this.hiddenitem = null;
					}
				}
				this.CheckStencilSwitch();
			}
			if (MainManager.player.entity.emoticonid == 4 && MainManager.player.entity.emoticoncooldown > 0f && !MainManager.sounds[11].isPlaying)
			{
				MainManager.PlaySound("Select1", 11, 1.1f, 0.25f);
			}
			if (this.digwall != null && this.digwall.Length != 0)
			{
				for (int m = 0; m < this.digwall.Length; m++)
				{
					if (this.digwall[m] != null)
					{
						this.digwall[m].enabled = !MainManager.player.digging;
					}
				}
			}
			if (this.alivetime > 0f)
			{
				this.alivetime -= 1f;
				return;
			}
			if (MainManager.player != null && Time.frameCount % 2 == 0)
			{
				for (int n = 0; n < this.entities.Length; n++)
				{
					if (this.entities[n] != null && this.entities[n].npcdata != null && !MainManager.CheckIfCanExist(this.entities[n].npcdata.requires, this.entities[n].npcdata.limit, this.entities[n].npcdata.regionalflag))
					{
						NPCControl.NPCType entitytype = this.entities[n].npcdata.entitytype;
						if (entitytype != NPCControl.NPCType.NPC)
						{
							if (entitytype == NPCControl.NPCType.Object)
							{
								NPCControl.ObjectTypes objecttype = this.entities[n].npcdata.objecttype;
								if (objecttype == NPCControl.ObjectTypes.DoorOtherMap)
								{
									bool flag = MainManager.GetDistance(this.entities[n].transform.position, MainManager.player.transform.position, true) < 15f;
									if (this.entities[n].gameObject.activeSelf != flag)
									{
										this.entities[n].gameObject.SetActive(flag);
										this.entities[n].emoticon.gameObject.SetActive(false);
									}
								}
							}
						}
						else if (this.entities[n].originalid == -1 && !this.keepobjectsactive)
						{
							bool flag2 = MainManager.GetDistance(this.entities[n].transform.position, MainManager.player.transform.position, true) < this.entities[n].npcdata.radius * 2f;
							if (this.entities[n].gameObject.activeSelf != flag2)
							{
								this.entities[n].gameObject.SetActive(flag2);
								if (this.entities[n].npcdata.interacttype != NPCControl.Interaction.Talk)
								{
									this.entities[n].emoticon.gameObject.SetActive(false);
								}
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06000652 RID: 1618 RVA: 0x000458D8 File Offset: 0x00043AD8
	private void CheckStencilSwitch()
	{
		bool flag = false;
		for (int i = 0; i < this.entities.Length; i++)
		{
			if (this.entities[i] != null && !this.entities[i].iskill && this.entities[i].npcdata != null && this.entities[i].npcdata.entitytype == NPCControl.NPCType.Object && this.entities[i].npcdata.objecttype == NPCControl.ObjectTypes.StencilSwitch && this.entities[i].npcdata.hit)
			{
				flag = true;
				Shader.SetGlobalFloat("GlobalIceRadius", this.entities[i].npcdata.internaltransform[0].localScale.magnitude / 2f);
				Shader.SetGlobalVector("CentralIcePoint", this.entities[i].transform.position);
				this.stencilid = i;
				break;
			}
		}
		if (!flag)
		{
			this.stencilid = -1;
			Shader.SetGlobalFloat("GlobalIceRadius", Mathf.Lerp(Shader.GetGlobalFloat("GlobalIceRadius"), 0f, MainManager.TieFramerate(0.05f)));
		}
	}

	// Token: 0x06000653 RID: 1619 RVA: 0x00045A10 File Offset: 0x00043C10
	private void FixedUpdate()
	{
		if (this.tieYtoplayer && MainManager.instance.camtarget != null)
		{
			this.actualcenter = new Vector3(this.centralpoint.x, MainManager.instance.camtarget.position.y, this.centralpoint.z);
			if (this.tetherYLerp.x > 0f)
			{
				this.tetherdistance = Mathf.Lerp(this.tetherYLerp.x, this.tetherYLerp.y, MainManager.instance.camtarget.position.y / this.tetherYLerp.z);
			}
		}
		if (!this.overrideskybox && RenderSettings.skybox != null)
		{
			if (!this.nocolorchange)
			{
				this.UpdateInsideColor((float)((MainManager.instance.insideid == -1) ? 1 : 0));
			}
			RenderSettings.skybox.SetFloat("_Rotation", 180f + MainManager.MainCamera.transform.position.x);
		}
		if (this.rotatecam && this.roundways.Length != 0 && MainManager.instance.insideid == -1)
		{
			this.roundways[0].transform.position = new Vector3(-MainManager.MainCamera.transform.forward.x * 3f, 5f, -MainManager.MainCamera.transform.forward.z * this.lightoffset);
		}
	}

	// Token: 0x06000654 RID: 1620 RVA: 0x00045B94 File Offset: 0x00043D94
	private void UpdateInsideColor(float targetalpha)
	{
		if (MainManager.GetDistance(this.fadeammount, targetalpha) > 0.025f)
		{
			if (MainManager.instance.insideid > -1)
			{
				this.fadeammount = Mathf.Lerp(this.fadeammount, 0f, MainManager.TieFramerate(this.fadingspeed));
			}
			else
			{
				this.fadeammount = Mathf.Lerp(this.fadeammount, 1f, MainManager.TieFramerate(this.fadingspeed));
			}
			float num = Mathf.Clamp(this.fadeammount, 0f, 0.5f);
			RenderSettings.skybox.SetColor("_Tint", new Color(num, num, num));
			if (this.insidedim == null)
			{
				this.insidedim = new MaterialPropertyBlock();
			}
			if (this.render != null)
			{
				for (int i = 0; i < this.render.Length; i++)
				{
					if (this.render[i] != null)
					{
						if (!this.render[i].CompareTag("AlwaysShow"))
						{
							for (int j = 0; j < this.render[i].sharedMaterials.Length; j++)
							{
								if (this.render[i].sharedMaterials[j].shader != MainManager.emptymat.shader && this.render[i].sharedMaterials[j].shader != MainManager.outlinemain.shader && this.render[i].sharedMaterials[j].shader != MainManager.fakelight && !this.render[i].CompareTag("NoMapColor") && this.render[i].sharedMaterials[j].HasProperty(MapControl.tint))
								{
									this.insidedim.SetColor(MapControl.tint, new Color(this.fadeammount, this.fadeammount, this.fadeammount, this.render[i].sharedMaterials[j].color.a));
									this.render[i].SetPropertyBlock(this.insidedim);
								}
							}
							this.render[i].enabled = (this.fadeammount > 0.15f);
						}
						else
						{
							this.render[i].enabled = true;
						}
					}
				}
			}
		}
	}

	// Token: 0x06000655 RID: 1621 RVA: 0x00045DD5 File Offset: 0x00043FD5
	public IEnumerator MoveInside(NPCControl caller)
	{
		base.StartCoroutine(this.MoveInside(caller, true));
		yield break;
	}

	// Token: 0x06000656 RID: 1622 RVA: 0x00045DEC File Offset: 0x00043FEC
	public Transform[] GetChilds(int[] index)
	{
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < index.Length; i++)
		{
			list.Add(this.mainmesh.GetChild(index[i]));
		}
		return list.ToArray();
	}

	// Token: 0x06000657 RID: 1623 RVA: 0x00045E27 File Offset: 0x00044027
	public IEnumerator MoveInside(NPCControl caller, bool move)
	{
		if (this.samira != null)
		{
			MainManager.events.SamiraStop(this.samira, true);
			this.samira = null;
		}
		MainManager.player.entity.emoticoncooldown = 0f;
		MainManager.player.entity.emoticon.Play("-1");
		MainManager.player.npc = new List<NPCControl>();
		float tempfriction = MainManager.player.entity.ccol.material.dynamicFriction;
		MainManager.player.entity.ccol.material.dynamicFriction = 0f;
		MainManager.player.CancelAction();
		MainManager.instance.minipause = true;
		Animator component = this.insides[caller.data[0]].GetComponent<Animator>();
		if (MainManager.instance.insideid == -1)
		{
			if (!MainManager.instance.inevent)
			{
				MainManager.PlaySound(this.insidetypes[caller.data[0]] + "DoorEnter");
			}
			if (move)
			{
				MainManager.TeleportFollowers(2f, MainManager.TPDir.Center, MainManager.player.transform);
				for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
				{
					MainManager.instance.playerdata[i].entity.transform.parent = null;
					MainManager.instance.playerdata[i].entity.rigid.velocity = Vector3.zero;
				}
				if (this.chompy != null)
				{
					this.chompy.transform.parent = base.transform;
					this.chompy.rigid.velocity = Vector3.zero;
				}
			}
			if (caller.data.Length >= 2 && caller.data[1] > -1)
			{
				MainManager.Musics musics = (MainManager.Musics)caller.data[1];
				string text = musics.ToString();
				MainManager.ChangeMusic(text, 0.2f);
				musics = (MainManager.Musics)Enum.Parse(typeof(MainManager.Musics), text);
				if (musics <= MainManager.Musics.Title)
				{
					if (musics == MainManager.Musics.Calm || musics == MainManager.Musics.Title)
					{
						goto IL_291;
					}
				}
				else if (musics - MainManager.Musics.Wind <= 1 || musics == MainManager.Musics.MachineHum || musics == MainManager.Musics.Breathing)
				{
					goto IL_291;
				}
				MainManager.CheckSamira(text);
			}
			IL_291:
			if (component != null)
			{
				component.Play("Open");
			}
			if (caller.data[0] != -1)
			{
				MainManager.lastinside = MainManager.instance.insideid;
			}
			MainManager.instance.insideid = caller.data[0];
			this.RefreshInsides(true, caller);
			if (move)
			{
				MainManager.player.entity.MoveTowards(caller.vectordata[0], 1f, 1, 0);
			}
			caller.vectordata[4] = MainManager.instance.camoffset;
			caller.vectordata[5] = MainManager.instance.camangleoffset;
			if (caller.vectordata[2].magnitude < 0.1f)
			{
				MainManager.instance.camoffset = MainManager.defaultcamoffset;
			}
			else
			{
				MainManager.instance.camoffset = caller.vectordata[2];
			}
			if (caller.vectordata[3].magnitude < 0.1f)
			{
				MainManager.instance.camangleoffset = MainManager.defaultcamangle;
			}
			else
			{
				MainManager.instance.camangleoffset = caller.vectordata[3];
			}
			if (this.tieinsidedoorentities)
			{
				for (int j = 0; j < this.entities.Length; j++)
				{
					if (this.entities[j] != null && this.entities[j].transform != caller.transform && this.entities[j].npcdata.objecttype == NPCControl.ObjectTypes.DoorSameMap)
					{
						this.entities[j].npcdata.vectordata[4] = caller.vectordata[4];
						this.entities[j].npcdata.vectordata[5] = caller.vectordata[5];
					}
				}
			}
			while (MainManager.player.entity.forcemove)
			{
				yield return null;
			}
			for (int k = 0; k < MainManager.instance.playerdata.Length; k++)
			{
				MainManager.instance.playerdata[k].entity.transform.parent = null;
				MainManager.instance.playerdata[k].entity.rigid.velocity = Vector3.zero;
			}
			if (this.chompy != null)
			{
				this.chompy.transform.parent = base.transform;
				this.chompy.rigid.velocity = Vector3.zero;
			}
		}
		else
		{
			if (!MainManager.instance.inevent)
			{
				MainManager.PlaySound(this.insidetypes[caller.data[0]] + "DoorExit");
			}
			if (this.music.Length != 0)
			{
				MainManager.ChangeMusic(this.music[0], 0.2f);
			}
			if (component != null)
			{
				component.Play("Close");
			}
			MainManager.instance.insideid = -1;
			this.RefreshInsides(false, caller);
			if (move)
			{
				MainManager.player.entity.MoveTowards(caller.vectordata[1], 1f, 1, 0);
			}
			MainManager.instance.camoffset = caller.vectordata[4];
			MainManager.instance.camangleoffset = caller.vectordata[5];
			while (MainManager.player.entity.forcemove)
			{
				yield return null;
			}
		}
		yield return null;
		MainManager.player.lastpos = MainManager.player.transform.position;
		MainManager.player.entity.lastpos = MainManager.player.lastpos;
		MainManager.player.entity.ccol.material.dynamicFriction = tempfriction;
		MainManager.player.entity.DetectIgnoreSphere(true);
		Caravan caravan = Object.FindObjectOfType<Caravan>();
		if (caravan != null)
		{
			caravan.Refresh();
		}
		while (MainManager.musiccoroutine != null)
		{
			yield return null;
		}
		MainManager.instance.minipause = false;
		yield break;
	}

	// Token: 0x06000658 RID: 1624 RVA: 0x00045E44 File Offset: 0x00044044
	public void SetTCLimits(NPCControl caller)
	{
		this.tcpos = new Vector3?(this.camlimitpos);
		this.tcneg = new Vector3?(this.camlimitneg);
		caller.vectordata[4] = MainManager.instance.camoffset;
		caller.vectordata[5] = MainManager.instance.camangleoffset;
	}

	// Token: 0x06000659 RID: 1625 RVA: 0x00045EA0 File Offset: 0x000440A0
	public void RefreshInsides(bool inside, NPCControl caller)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("DelAftBtl");
		if (array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				Object.Destroy(array[i]);
			}
		}
		if (MainManager.player != null && MainManager.player.beemerang != null)
		{
			Object.Destroy(MainManager.player.beemerang.gameObject);
		}
		if (inside)
		{
			if (caller != null)
			{
				for (int j = 0; j < this.entities.Length; j++)
				{
					if (this.entities[j] != null && this.entities[j].npcdata != null && this.entities[j].npcdata.insideid != -2)
					{
						if (this.entities[j].npcdata.insideid != caller.data[0])
						{
							this.entities[j].gameObject.SetActive(false);
						}
						else if (this.entities[j].hideinside && this.entities[j].npcdata.insideid == caller.data[0])
						{
							this.entities[j].gameObject.SetActive(true);
						}
					}
				}
				if (caller.vectordata != null && caller.vectordata.Length > 6)
				{
					if (caller.vectordata[6].magnitude > 0.1f)
					{
						this.tcpos = new Vector3?(this.camlimitpos);
						this.camlimitpos = caller.vectordata[6];
					}
					if (caller.vectordata[7].magnitude > 0.1f)
					{
						this.tcneg = new Vector3?(this.camlimitneg);
						this.camlimitneg = caller.vectordata[7];
					}
				}
			}
			else
			{
				for (int k = 0; k < this.entities.Length; k++)
				{
					if (this.entities[k] != null && this.entities[k].npcdata.insideid != MainManager.instance.insideid)
					{
						this.entities[k].gameObject.SetActive(false);
					}
				}
			}
			if (this.hideinsides)
			{
				for (int l = 0; l < this.insides.Length; l++)
				{
					this.insides[l].SetActive(l == MainManager.instance.insideid);
				}
			}
			else if (caller != null)
			{
				for (int m = 0; m < this.insides.Length; m++)
				{
					if (m != caller.data[0])
					{
						this.insides[m].SetActive(false);
					}
					else if (this.insides[m].GetComponent<Fader>() != null)
					{
						this.insides[m].GetComponent<Fader>().enabled = false;
					}
				}
			}
			if (this.setinsidecenter)
			{
				MainManager.instance.camtarget = this.insides[MainManager.instance.insideid].transform;
			}
		}
		else
		{
			if (this.tcpos != null)
			{
				this.camlimitpos = this.tcpos.Value;
			}
			else
			{
				this.camlimitpos = this.originallimitpos;
			}
			if (this.tcneg != null)
			{
				this.camlimitneg = this.tcneg.Value;
			}
			else
			{
				this.camlimitneg = this.originallimitneg;
			}
			if (this.hideinsides)
			{
				for (int n = 0; n < this.entities.Length; n++)
				{
					if (this.entities[n] != null && this.entities[n].npcdata.objecttype != NPCControl.ObjectTypes.DoorOtherMap && this.entities[n].npcdata.objecttype != NPCControl.ObjectTypes.DoorSameMap)
					{
						this.entities[n].gameObject.SetActive(!this.entities[n].hideinside && this.entities[n].npcdata.insideid == -1);
					}
				}
				for (int num = 0; num < this.insides.Length; num++)
				{
					this.insides[num].SetActive(false);
				}
			}
			else
			{
				for (int num2 = 0; num2 < this.entities.Length; num2++)
				{
					if (this.entities[num2] != null && !this.entities[num2].iskill)
					{
						this.entities[num2].gameObject.SetActive(!this.entities[num2].hideinside);
						this.entities[num2].oldstate = -1;
						if (this.entities[num2].anim != null)
						{
							this.entities[num2].anim.speed = 1f;
						}
					}
				}
				for (int num3 = 0; num3 < this.insides.Length; num3++)
				{
					this.insides[num3].SetActive(true);
					if (this.insides[num3].GetComponent<Fader>() != null)
					{
						this.insides[num3].GetComponent<Fader>().enabled = true;
					}
				}
			}
			if (this.setinsidecenter && MainManager.player != null)
			{
				MainManager.instance.camtarget = MainManager.player.transform;
			}
		}
		if (MainManager.player != null)
		{
			MainManager.player.pausecooldown = 7f;
		}
		MainManager.RefreshEntities(false, true);
	}

	// Token: 0x0600065A RID: 1626 RVA: 0x000463F9 File Offset: 0x000445F9
	public void StopMovingEntities()
	{
		this.StopMovingEntities(null, -1);
	}

	// Token: 0x0600065B RID: 1627 RVA: 0x00046404 File Offset: 0x00044604
	public void HideRenders()
	{
		for (int i = 0; i < this.render.Length; i++)
		{
			if (this.render[i] != null)
			{
				this.render[i].enabled = false;
			}
		}
	}

	// Token: 0x0600065C RID: 1628 RVA: 0x00046444 File Offset: 0x00044644
	public void StopMovingEntities(EntityControl exception, int state)
	{
		for (int i = 0; i < this.entities.Length; i++)
		{
			if (this.entities[i] != exception)
			{
				int targetstate = this.entities[i].animstate;
				if (state > -1)
				{
					targetstate = state;
				}
				this.entities[i].StopMoving(targetstate);
				if (this.entities[i].npcdata != null)
				{
					this.entities[i].npcdata.StopForceBehavior();
				}
			}
		}
	}

	// Token: 0x0600065D RID: 1629 RVA: 0x000464C0 File Offset: 0x000446C0
	public void RemoveLimit(bool settemp)
	{
		if (settemp)
		{
			this.tempcneg = this.camlimitneg;
			this.tempcpos = this.camlimitpos;
		}
		this.camlimitneg = new Vector3(-999f, -999f, -999f);
		this.camlimitpos = new Vector3(999f, 999f, 999f);
	}

	// Token: 0x0600065E RID: 1630 RVA: 0x0004651C File Offset: 0x0004471C
	public void RestoreLimit(bool restoretemp)
	{
		if (restoretemp)
		{
			this.camlimitneg = this.tempcneg;
			this.camlimitpos = this.tempcpos;
			return;
		}
		this.camlimitneg = this.originallimitneg;
		this.camlimitpos = this.originallimitpos;
	}

	// Token: 0x0600065F RID: 1631 RVA: 0x00046554 File Offset: 0x00044754
	private void CreateEntities()
	{
		int num = (this.readdatafromothermap == MainManager.Maps.TestRoom) ? Convert.ToInt32(base.name) : ((int)this.readdatafromothermap);
		if (Resources.Load<TextAsset>("Data/EntityData/" + num) != null)
		{
			string[] array = Resources.Load<TextAsset>("Data/EntityData/" + num).ToString().Split(new char[]
			{
				'\n'
			});
			string[] array2 = Resources.Load<TextAsset>("Data/EntityData/Names/" + num + "names").ToString().Split(new char[]
			{
				'\n'
			});
			List<EntityControl> list = new List<EntityControl>();
			List<EntityControl> list2 = new List<EntityControl>();
			if (array.Length != 0)
			{
				for (int i = 0; i < array.Length - 1; i++)
				{
					if (!array2[i].Contains("debug") || MainManager.debugenalbed)
					{
						string[] array3 = array[i].Split(new char[]
						{
							'}'
						});
						EntityControl entityControl = EntityControl.CreateNewEntity(array2[i]);
						if (MainManager.player != null)
						{
							entityControl.transform.position = new Vector3(MainManager.player.transform.position.x, MainManager.player.transform.position.y + 2f, MainManager.player.transform.position.z);
						}
						entityControl.name = array2[i];
						entityControl.npcdata = entityControl.gameObject.AddComponent<NPCControl>();
						entityControl.npcdata.entitytype = (NPCControl.NPCType)Enum.Parse(typeof(NPCControl.NPCType), array3[0]);
						entityControl.npcdata.objecttype = (NPCControl.ObjectTypes)Enum.Parse(typeof(NPCControl.ObjectTypes), array3[1]);
						entityControl.npcdata.behaviors = new NPCControl.ActionBehaviors[]
						{
							(NPCControl.ActionBehaviors)Enum.Parse(typeof(NPCControl.ActionBehaviors), array3[2]),
							(NPCControl.ActionBehaviors)Enum.Parse(typeof(NPCControl.ActionBehaviors), array3[3])
						};
						entityControl.npcdata.interacttype = (NPCControl.Interaction)Enum.Parse(typeof(NPCControl.Interaction), array3[4]);
						entityControl.destroytype = (NPCControl.DeathType)Enum.Parse(typeof(NPCControl.DeathType), array3[5]);
						entityControl.animid = Convert.ToInt32(array3[9]);
						entityControl.flip = Convert.ToBoolean(array3[10]);
						entityControl.ccol.height = Convert.ToSingle(array3[11]) / 2f;
						entityControl.npcdata.colliderheight = Convert.ToSingle(array3[11]);
						entityControl.ccol.radius = Convert.ToSingle(array3[12]);
						entityControl.npcdata.radius = Convert.ToSingle(array3[13]);
						entityControl.npcdata.timer = Convert.ToSingle(array3[14]);
						entityControl.speed = Convert.ToSingle(array3[15]);
						entityControl.npcdata.actionfrequency = new float[]
						{
							Convert.ToSingle(array3[16]),
							Convert.ToSingle(array3[17])
						};
						entityControl.npcdata.speedmultiplier = Convert.ToSingle(array3[18]);
						entityControl.npcdata.radiuslimit = Convert.ToSingle(array3[19]);
						entityControl.npcdata.wanderradius = Convert.ToSingle(array3[20]);
						entityControl.npcdata.teleportradius = Convert.ToSingle(array3[21]);
						if (entityControl.npcdata.entitytype == NPCControl.NPCType.Object && Convert.ToBoolean(array3[22]))
						{
							entityControl.npcdata.boxcol = entityControl.gameObject.AddComponent<BoxCollider>();
							entityControl.npcdata.boxcol.isTrigger = Convert.ToBoolean(array3[23]);
							entityControl.npcdata.boxcol.size = new Vector3(Convert.ToSingle(array3[24]), Convert.ToSingle(array3[25]), Convert.ToSingle(array3[26]));
							entityControl.npcdata.boxcol.center = new Vector3(Convert.ToSingle(array3[27]), Convert.ToSingle(array3[28]), Convert.ToSingle(array3[29]));
						}
						entityControl.npcdata.freezetime = Convert.ToSingle(array3[30]);
						entityControl.freezesize = new Vector3(Convert.ToSingle(array3[31]), Convert.ToSingle(array3[32]), Convert.ToSingle(array3[33]));
						entityControl.freezeoffset = new Vector3(Convert.ToSingle(array3[34]), Convert.ToSingle(array3[35]), Convert.ToSingle(array3[36]));
						entityControl.npcdata.eventid = Convert.ToInt32(array3[37]);
						int num2 = 39;
						int num3 = Convert.ToInt32(array3[38]);
						if (num3 > 0)
						{
							entityControl.npcdata.requires = new int[num3];
							for (int j = 0; j < entityControl.npcdata.requires.Length; j++)
							{
								entityControl.npcdata.requires[j] = Convert.ToInt32(array3[num2 + j]);
							}
						}
						else
						{
							entityControl.npcdata.requires = new int[]
							{
								-1
							};
						}
						num2 += 10;
						num3 = Convert.ToInt32(array3[num2]);
						if (num3 > 0)
						{
							entityControl.npcdata.limit = new int[num3];
							num2++;
							for (int k = 0; k < entityControl.npcdata.limit.Length; k++)
							{
								entityControl.npcdata.limit[k] = Convert.ToInt32(array3[num2 + k]);
							}
						}
						else
						{
							entityControl.npcdata.limit = new int[]
							{
								-1
							};
							num2++;
						}
						num2 += 10;
						num3 = Convert.ToInt32(array3[num2]);
						if (num3 > 0)
						{
							entityControl.npcdata.data = new int[num3];
							num2++;
							for (int l = 0; l < entityControl.npcdata.data.Length; l++)
							{
								entityControl.npcdata.data[l] = Convert.ToInt32(array3[num2 + l]);
							}
						}
						else
						{
							num2++;
						}
						num2 += 10;
						num3 = Convert.ToInt32(array3[num2]);
						if (num3 > 0)
						{
							entityControl.npcdata.vectordata = new Vector3[num3];
							num2++;
							for (int m = 0; m < entityControl.npcdata.vectordata.Length; m++)
							{
								entityControl.npcdata.vectordata[m] = new Vector3(Convert.ToSingle(array3[num2 + m * 3]), Convert.ToSingle(array3[num2 + m * 3 + 1]), Convert.ToSingle(array3[num2 + m * 3 + 2]));
							}
						}
						else
						{
							num2++;
						}
						num2 += 30;
						num3 = Convert.ToInt32(array3[num2]);
						if (num3 > 0)
						{
							if (entityControl.npcdata.interacttype == NPCControl.Interaction.Shop)
							{
								entityControl.npcdata.dialogues = new Vector3[20];
								num2++;
								for (int n = 0; n < 20; n++)
								{
									entityControl.npcdata.dialogues[n] = new Vector3(Convert.ToSingle(array3[num2 + n * 3]), Convert.ToSingle(array3[num2 + n * 3 + 1]), Convert.ToSingle(array3[num2 + n * 3 + 2]));
								}
							}
							else
							{
								entityControl.npcdata.dialogues = new Vector3[num3];
								num2++;
								for (int num4 = 0; num4 < entityControl.npcdata.dialogues.Length; num4++)
								{
									entityControl.npcdata.dialogues[num4] = new Vector3(Convert.ToSingle(array3[num2 + num4 * 3]), Convert.ToSingle(array3[num2 + num4 * 3 + 1]), Convert.ToSingle(array3[num2 + num4 * 3 + 2]));
								}
							}
						}
						else
						{
							entityControl.npcdata.dialogues = new Vector3[]
							{
								new Vector3(-1f, 0f, 0f)
							};
							num2++;
						}
						num2 += 60;
						if (entityControl.name.Contains("ROT"))
						{
							base.StartCoroutine(MainManager.LateAngle(entityControl.transform, new Vector3(Convert.ToSingle(array3[num2]), Convert.ToSingle(array3[num2 + 1]), Convert.ToSingle(array3[num2 + 2])), false, EventControl.quartersec));
						}
						else
						{
							entityControl.transform.eulerAngles = new Vector3(Convert.ToSingle(array3[num2]), Convert.ToSingle(array3[num2 + 1]), Convert.ToSingle(array3[num2 + 2]));
						}
						num2 += 3;
						num3 = Convert.ToInt32(array3[num2]);
						if (num3 > 0)
						{
							entityControl.npcdata.battleids = new int[num3];
							num2++;
							for (int num5 = 0; num5 < entityControl.npcdata.battleids.Length; num5++)
							{
								entityControl.npcdata.battleids[num5] = Convert.ToInt32(array3[num2 + num5]);
							}
						}
						else
						{
							num2++;
							entityControl.npcdata.battleids = new int[1];
						}
						num2 += 4;
						entityControl.npcdata.tagcolor = new Color(Convert.ToSingle(array3[num2]), Convert.ToSingle(array3[num2 + 1]), Convert.ToSingle(array3[num2 + 2]), Convert.ToSingle(array3[num2 + 3]));
						num2 += 4;
						entityControl.emoticonoffset = new Vector3(Convert.ToSingle(array3[num2]), Convert.ToSingle(array3[num2 + 1]), Convert.ToSingle(array3[num2 + 2]));
						num2 += 3;
						entityControl.npcdata.insideid = Convert.ToInt32(array3[num2]);
						num2++;
						entityControl.npcdata.emoticonflag = new Vector2[10];
						for (int num6 = 0; num6 < 10; num6++)
						{
							string[] array4 = array3[num2].Split(new char[]
							{
								','
							});
							entityControl.npcdata.emoticonflag[num6] = new Vector2((float)Convert.ToInt32(array4[0]), (float)Convert.ToInt32(array4[1]));
							num2++;
						}
						entityControl.npcdata.tattleid = Convert.ToInt32(array3[num2]);
						num2++;
						entityControl.npcdata.regionalflag = Convert.ToInt32(array3[num2]);
						num2++;
						entityControl.initialheight = Convert.ToSingle(array3[num2]);
						num2++;
						entityControl.bobrange = Convert.ToSingle(array3[num2]);
						num2++;
						entityControl.bobspeed = Convert.ToSingle(array3[num2]);
						num2++;
						entityControl.npcdata.activationflag = Convert.ToInt32(array3[num2]);
						num2++;
						if (array3[num2].Length > 1)
						{
							entityControl.npcdata.returntoheight = Convert.ToBoolean(array3[num2]);
						}
						else
						{
							entityControl.npcdata.returntoheight = Convert.ToBoolean(Convert.ToInt32(array3[num2]));
						}
						num2++;
						entityControl.startpos = new Vector3?(new Vector3(Convert.ToSingle(array3[6]), Convert.ToSingle(array3[7]), Convert.ToSingle(array3[8])));
						entityControl.transform.position = entityControl.startpos.Value;
						entityControl.npcdata.mapid = i;
						entityControl.transform.parent = base.transform;
						entityControl.npcdata.entity = entityControl;
						entityControl.npcdata.entity.iskill = MainManager.CheckIfCanExist(entityControl.npcdata.requires, entityControl.npcdata.limit, entityControl.npcdata.regionalflag);
						entityControl.height = entityControl.initialheight;
						if (entityControl.npcdata.entitytype == NPCControl.NPCType.Object)
						{
							if (entityControl.npcdata.objecttype == NPCControl.ObjectTypes.DoorOtherMap || entityControl.npcdata.objecttype == NPCControl.ObjectTypes.DoorSameMap)
							{
								entityControl.npcdata.insideid = -2;
							}
							if (entityControl.npcdata.objecttype == NPCControl.ObjectTypes.MusicRange)
							{
								this.musicrangemain = i;
							}
							list.Add(entityControl);
							NPCControl.ObjectTypes objecttype = entityControl.npcdata.objecttype;
							if (objecttype == NPCControl.ObjectTypes.Item)
							{
								entityControl.animstate = entityControl.animid;
								entityControl.animid = entityControl.npcdata.data[0];
								entityControl.item = true;
								entityControl.sprite.transform.localPosition = new Vector3(0f, 0.5f);
							}
						}
						else
						{
							list.Add(entityControl);
							if (entityControl.npcdata.entitytype == NPCControl.NPCType.NPC && entityControl.npcdata.insideid == -1 && entityControl.npcdata.interacttype == NPCControl.Interaction.ShopKeeper && entityControl.npcdata.interacttype != NPCControl.Interaction.Shop && entityControl.npcdata.behaviors != null && entityControl.npcdata.behaviors.Length != 0)
							{
								bool flag = entityControl.name.Contains("FxdCol") || entityControl.name.Contains("Fixed");
								if (MapControl.FixedActions.Contains(entityControl.npcdata.behaviors[0]) && !flag)
								{
									entityControl.name = entityControl.name.Insert(0, "FxdCol");
								}
							}
						}
						if (entityControl.npcdata.entitytype == NPCControl.NPCType.Object && entityControl.npcdata.objecttype == NPCControl.ObjectTypes.DoorSameMap && entityControl.npcdata.data.Length >= 2 && entityControl.npcdata.data[1] > -1)
						{
							List<AudioClip> list3 = this.musicpreload;
							string str = "Audio/Music/";
							MainManager.Musics musics = (MainManager.Musics)entityControl.npcdata.data[1];
							list3.Add(Resources.Load<AudioClip>(str + musics.ToString()));
						}
						entityControl.lastpos = entityControl.startpos.Value;
						if (entityControl.npcdata.interacttype == NPCControl.Interaction.Shop && !MainManager.CheckIfCanExist(entityControl.npcdata.requires, entityControl.npcdata.limit, entityControl.npcdata.regionalflag))
						{
							entityControl.npcdata.interacttype = NPCControl.Interaction.ShopKeeper;
							if ((int)entityControl.npcdata.dialogues[10].x != 1)
							{
								for (int num7 = 0; num7 < entityControl.npcdata.data.Length; num7++)
								{
									EntityControl entityControl2 = EntityControl.CreateNewEntity("Fixedshop" + num7);
									entityControl2.startpos = new Vector3?(entityControl.npcdata.vectordata[num7]);
									entityControl2.animid = (int)entityControl.npcdata.dialogues[10].x;
									if (entityControl2.animid == 0)
									{
										entityControl2.animstate = entityControl.npcdata.data[num7];
									}
									else
									{
										int animid = entityControl2.animid;
									}
									entityControl2.item = true;
									entityControl2.hasshadow = false;
									entityControl2.npcdata = entityControl2.gameObject.AddComponent<NPCControl>();
									entityControl2.npcdata.entitytype = NPCControl.NPCType.SemiNPC;
									entityControl2.npcdata.interacttype = NPCControl.Interaction.Shop;
									entityControl2.emoticonoffset = new Vector3(0f, -1000f, 0f);
									entityControl2.npcdata.shopkeeper = entityControl.npcdata;
									entityControl2.npcdata.radius = entityControl.npcdata.dialogues[8].x / 10f;
									if (entityControl2.npcdata.radius < 0.1f)
									{
										entityControl2.npcdata.radius = 1.5625f;
									}
									entityControl2.npcdata.insideid = entityControl.npcdata.insideid;
									entityControl2.npcdata.colliderheight = 0.5f;
									list2.Add(entityControl2);
								}
							}
						}
					}
				}
			}
			if (list2.ToArray().Length != 0)
			{
				for (int num8 = 0; num8 < list2.ToArray().Length; num8++)
				{
					list.Add(list2.ToArray()[num8]);
					list2.ToArray()[num8].transform.parent = base.transform;
				}
			}
			this.entities = list.ToArray();
		}
	}

	// Token: 0x06000660 RID: 1632 RVA: 0x000475A8 File Offset: 0x000457A8
	private void Test()
	{
		if (MainManager.CurrentMap() == MainManager.Maps.TestRoom && !Application.isEditor)
		{
			((GameObject)null).transform.position = default(Vector3);
		}
	}

	// Token: 0x06000661 RID: 1633 RVA: 0x000475D8 File Offset: 0x000457D8
	public void AddInEntity(EntityControl en)
	{
		List<EntityControl> list = new List<EntityControl>();
		list.AddRange(this.entities);
		list.Add(en);
		this.entities = list.ToArray();
		en.transform.parent = base.transform;
	}

	// Token: 0x04000559 RID: 1369
	private static readonly List<NPCControl.ActionBehaviors> FixedActions = new List<NPCControl.ActionBehaviors>(new NPCControl.ActionBehaviors[]
	{
		NPCControl.ActionBehaviors.None,
		NPCControl.ActionBehaviors.FaceAhead,
		NPCControl.ActionBehaviors.FaceAwayFromPlayer,
		NPCControl.ActionBehaviors.FaceBehind,
		NPCControl.ActionBehaviors.FaceDown,
		NPCControl.ActionBehaviors.FacePlayer,
		NPCControl.ActionBehaviors.FaceUp,
		NPCControl.ActionBehaviors.TurnFixedInterval,
		NPCControl.ActionBehaviors.TurnRandomly
	});

	// Token: 0x0400055A RID: 1370
	public MainManager.Maps mapid;

	// Token: 0x0400055B RID: 1371
	public MapControl.InsideType[] insidetypes;

	// Token: 0x0400055C RID: 1372
	public MapControl.ScreenEffects screeneffect;

	// Token: 0x0400055D RID: 1373
	public int tattleid;

	// Token: 0x0400055E RID: 1374
	public int musicrangemain = -1;

	// Token: 0x0400055F RID: 1375
	public int musicid;

	// Token: 0x04000560 RID: 1376
	public MapControl.BattleLeafType battleleaftype;

	// Token: 0x04000561 RID: 1377
	public MainManager.Areas areaid;

	// Token: 0x04000562 RID: 1378
	public MainManager.BattleMaps battlemap;

	// Token: 0x04000563 RID: 1379
	public GameObject[] preloadobjs;

	// Token: 0x04000564 RID: 1380
	public GameObject[] eventPointers;

	// Token: 0x04000565 RID: 1381
	public Vector3 camlimitneg = new Vector3(-999f, 0f, -999f);

	// Token: 0x04000566 RID: 1382
	public Vector3 camlimitpos = new Vector3(999f, 999f, 999f);

	// Token: 0x04000567 RID: 1383
	public Vector3 centralpoint;

	// Token: 0x04000568 RID: 1384
	public Vector3 camangle;

	// Token: 0x04000569 RID: 1385
	private Vector3 originallimitneg;

	// Token: 0x0400056A RID: 1386
	private Vector3 originallimitpos;

	// Token: 0x0400056B RID: 1387
	private Vector3 tempcpos;

	// Token: 0x0400056C RID: 1388
	private Vector3 tempcneg;

	// Token: 0x0400056D RID: 1389
	private Vector3? tcpos;

	// Token: 0x0400056E RID: 1390
	private Vector3? tcneg;

	// Token: 0x0400056F RID: 1391
	public Vector3 tetherYLerp;

	// Token: 0x04000570 RID: 1392
	public EntityControl[] entities;

	// Token: 0x04000571 RID: 1393
	public EntityControl chompy;

	// Token: 0x04000572 RID: 1394
	public Material skyboxmat;

	// Token: 0x04000573 RID: 1395
	public AudioClip[] music;

	// Token: 0x04000574 RID: 1396
	public string[] dialogues;

	// Token: 0x04000575 RID: 1397
	public float ylimit = -50f;

	// Token: 0x04000576 RID: 1398
	public float transferspeed = 0.2f;

	// Token: 0x04000577 RID: 1399
	public float baseoutline = 20f;

	// Token: 0x04000578 RID: 1400
	public float tetherdistance = -1f;

	// Token: 0x04000579 RID: 1401
	public float fadingspeed = 0.2f;

	// Token: 0x0400057A RID: 1402
	public float windspeed = 0.2f;

	// Token: 0x0400057B RID: 1403
	public float windintensity = 0.2f;

	// Token: 0x0400057C RID: 1404
	public float followerylimit = 20f;

	// Token: 0x0400057D RID: 1405
	public float fogend = 300f;

	// Token: 0x0400057E RID: 1406
	public float expmulti = 1f;

	// Token: 0x0400057F RID: 1407
	public float alivetime = 20f;

	// Token: 0x04000580 RID: 1408
	public GameObject[] insides;

	// Token: 0x04000581 RID: 1409
	public Color skycolor = Color.white;

	// Token: 0x04000582 RID: 1410
	public Color battleleafcolor = Color.green;

	// Token: 0x04000583 RID: 1411
	public Color fogcolor = Color.white;

	// Token: 0x04000584 RID: 1412
	public Color globallight = Color.gray;

	// Token: 0x04000585 RID: 1413
	public bool overrideskybox;

	// Token: 0x04000586 RID: 1414
	public bool rotatecam;

	// Token: 0x04000587 RID: 1415
	public bool hideinsides;

	// Token: 0x04000588 RID: 1416
	public bool setinsidecenter;

	// Token: 0x04000589 RID: 1417
	public bool cantcompass;

	// Token: 0x0400058A RID: 1418
	public bool tieinsidedoorentities;

	// Token: 0x0400058B RID: 1419
	public bool keepmusic;

	// Token: 0x0400058C RID: 1420
	public bool closemove;

	// Token: 0x0400058D RID: 1421
	public bool tieYtoplayer;

	// Token: 0x0400058E RID: 1422
	public bool nobattlemusic;

	// Token: 0x0400058F RID: 1423
	public bool icemap;

	// Token: 0x04000590 RID: 1424
	public bool faderchange;

	// Token: 0x04000591 RID: 1425
	public bool limitbehavior;

	// Token: 0x04000592 RID: 1426
	public bool keepobjectsactive;

	// Token: 0x04000593 RID: 1427
	public bool useglobalcommand;

	// Token: 0x04000594 RID: 1428
	public bool englishbreakfix;

	// Token: 0x04000595 RID: 1429
	public Transform mainmesh;

	// Token: 0x04000596 RID: 1430
	private Fader[] faders;

	// Token: 0x04000597 RID: 1431
	private bool[] fss;

	// Token: 0x04000598 RID: 1432
	private MeshRenderer[] render;

	// Token: 0x04000599 RID: 1433
	public Collider[] entityonly;

	// Token: 0x0400059A RID: 1434
	public int[] canfollowID;

	// Token: 0x0400059B RID: 1435
	public int[] discoveryids;

	// Token: 0x0400059C RID: 1436
	public float lightoffset = 5f;

	// Token: 0x0400059D RID: 1437
	public float insidecamspeed = 1f;

	// Token: 0x0400059E RID: 1438
	public Transform[] roundways;

	// Token: 0x0400059F RID: 1439
	public Vector2[] autoevent;

	// Token: 0x040005A0 RID: 1440
	public Vector3 actualcenter;

	// Token: 0x040005A1 RID: 1441
	public Vector3 camoffset;

	// Token: 0x040005A2 RID: 1442
	public int? hiddenitem;

	// Token: 0x040005A3 RID: 1443
	private float fadeammount = 1f;

	// Token: 0x040005A4 RID: 1444
	private const float defaultmusicfade = 0.1f;

	// Token: 0x040005A5 RID: 1445
	private Renderer[] windobjects;

	// Token: 0x040005A6 RID: 1446
	public bool[] mapflags;

	// Token: 0x040005A7 RID: 1447
	public List<EntityControl> tempfollowers;

	// Token: 0x040005A8 RID: 1448
	private List<AudioClip> musicpreload;

	// Token: 0x040005A9 RID: 1449
	private Collider[] digwall;

	// Token: 0x040005AA RID: 1450
	[HideInInspector]
	public Renderer mainrender;

	// Token: 0x040005AB RID: 1451
	public NPCControl samira;

	// Token: 0x040005AC RID: 1452
	public List<Texture2D> entitysprite;

	// Token: 0x040005AD RID: 1453
	public MainManager.Maps readdatafromothermap;

	// Token: 0x040005AE RID: 1454
	public Vector2Int[] musicflags;

	// Token: 0x040005AF RID: 1455
	[HideInInspector]
	public Hazards waterfloat;

	// Token: 0x040005B0 RID: 1456
	[HideInInspector]
	public Hazards lastwater;

	// Token: 0x040005B1 RID: 1457
	private MaterialPropertyBlock insidedim;

	// Token: 0x040005B2 RID: 1458
	public static int tint = Shader.PropertyToID("_Color");

	// Token: 0x040005B3 RID: 1459
	public static int emission = Shader.PropertyToID("_EmissionColor");

	// Token: 0x040005B4 RID: 1460
	private bool nocolorchange;

	// Token: 0x040005B5 RID: 1461
	[HideInInspector]
	public bool latestart;

	// Token: 0x040005B6 RID: 1462
	public int stencilid = -1;

	// Token: 0x040005B7 RID: 1463
	public int currentline = -1;

	// Token: 0x040005B8 RID: 1464
	public string[] commandlines;

	// Token: 0x0200024A RID: 586
	public enum ScreenEffects
	{
		// Token: 0x04001F91 RID: 8081
		None,
		// Token: 0x04001F92 RID: 8082
		SunRaysTopRight
	}

	// Token: 0x0200024B RID: 587
	public enum InsideType
	{
		// Token: 0x04001F94 RID: 8084
		Stretch,
		// Token: 0x04001F95 RID: 8085
		Slide,
		// Token: 0x04001F96 RID: 8086
		Twist
	}

	// Token: 0x0200024C RID: 588
	public enum BattleLeafType
	{
		// Token: 0x04001F98 RID: 8088
		Common,
		// Token: 0x04001F99 RID: 8089
		GoldenHills,
		// Token: 0x04001F9A RID: 8090
		Snakemouth,
		// Token: 0x04001F9B RID: 8091
		Desert,
		// Token: 0x04001F9C RID: 8092
		Bee,
		// Token: 0x04001F9D RID: 8093
		BarrenLands,
		// Token: 0x04001F9E RID: 8094
		FarGrasslands,
		// Token: 0x04001F9F RID: 8095
		Swamp,
		// Token: 0x04001FA0 RID: 8096
		MetalLake
	}
}
