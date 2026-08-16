using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000010 RID: 16
public class DeadLanderOmega : MonoBehaviour
{
	// Token: 0x0600019A RID: 410 RVA: 0x00013780 File Offset: 0x00011980
	private void Start()
	{
		this.ogpoints = this.points;
		this.start = base.transform.position;
		base.transform.position = DeadLanderOmega.offscreen;
		if (this.extras.Length != 0)
		{
			this.expos = new Vector3[this.extras.Length];
			for (int i = 0; i < this.extras.Length; i++)
			{
				this.expos[i] = this.extras[i].position;
				this.extras[i].position = DeadLanderOmega.offscreen;
			}
		}
		base.gameObject.isStatic = false;
		if (DeadLanderOmega.hand == null)
		{
			DeadLanderOmega.hand = (Object.Instantiate(Resources.Load("Prefabs/Objects/DeadHand")) as GameObject).GetComponent<Animator>();
			DeadLanderOmega.hand.transform.parent = MainManager.map.transform;
			DeadLanderOmega.hand.transform.position = DeadLanderOmega.offscreen;
		}
		this.enemy = MainManager.GetEntity(this.enemyentityid).npcdata;
		this.enemy.entity.overrridejump = true;
		this.enemy.entity.onground = false;
		this.enemy.entity.alwaysactive = true;
		this.enemy.entity.LockRigid(true);
		this.enemy.entity.killonfall = true;
		this.eye = new Transform[4];
		this.eye[0] = (Object.Instantiate(Resources.Load("Prefabs/Objects/Eye")) as GameObject).transform;
		this.eye[0].parent = base.transform;
		this.eye[0].transform.localPosition = Vector3.zero;
		this.eye[0].transform.localScale = Vector3.one * 2f;
		this.eye[1] = this.eye[0].GetChild(0);
		this.eye[2] = this.eye[0].GetChild(1);
		this.eye[3] = this.eye[0].GetChild(2);
		for (int j = 0; j < 2; j++)
		{
			this.eye[1 + j].GetComponent<Renderer>().material.color = MainManager.map.fogcolor;
			this.eye[1 + j].localEulerAngles = DeadLanderOmega.eyeclose * (float)((j == 0) ? 1 : -1);
		}
		Renderer[] componentsInChildren = this.eye[3].GetComponentsInChildren<Renderer>();
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			componentsInChildren[k].material.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, componentsInChildren[k].material.color.a);
		}
		this.eye[3].gameObject.SetActive(false);
		this.eyerender = this.eye[0].GetComponent<Renderer>();
		this.rigid = base.gameObject.AddComponent<Rigidbody>();
		this.rigid.isKinematic = true;
		this.rigid.useGravity = false;
		this.cooldowns = new float[3];
		this.cooldowns[1] = this.framedelay;
		this.cooldowns[2] = 225f;
		this.LookAt();
	}

	// Token: 0x0600019B RID: 411 RVA: 0x00013AC8 File Offset: 0x00011CC8
	private void Update()
	{
		if (!MainManager.IsPaused() && MainManager.battle == null && !this.enemy.entity.iskill)
		{
			if (DeadLanderOmega.state == 5)
			{
				if (MainManager.musiccoroutine == null)
				{
					DeadLanderOmega.state = 0;
					this.cooldowns[0] = 0f;
					this.closetime = 0f;
					this.activated = false;
					this.eyerender.material.mainTextureOffset = Vector2.zero;
					this.cooldowns[1] = this.framedelay;
					DeadLanderOmega.detected = false;
					return;
				}
			}
			else if (DeadLanderOmega.state == 4)
			{
				if (this.enemy.entity.iskill || !MainManager.InCameraRange(this.enemy.entity.campos) || this.enemy.transform.position.y < MainManager.map.ylimit || MainManager.player.transform.position.y > 15f)
				{
					DeadLanderOmega.state = 5;
					this.enemy.entity.SetPosition(DeadLanderOmega.offscreen);
					this.enemy.entity.LockRigid(true);
					MainManager.ChangeMusic();
					return;
				}
			}
			else if (DeadLanderOmega.detected)
			{
				if (DeadLanderOmega.activeid != this.thisid)
				{
					this.Close();
					return;
				}
				switch (DeadLanderOmega.state)
				{
				case 0:
					for (int i = 0; i < 2; i++)
					{
						this.eye[1 + i].localEulerAngles = MainManager.LerpVectorAngle(this.eye[1 + i].localEulerAngles, DeadLanderOmega.found * (float)((i == 0) ? 1 : -1), MainManager.framestep * 0.15f);
					}
					if (this.cooldowns[0] < 30f)
					{
						this.cooldowns[0] += MainManager.framestep;
						return;
					}
					this.cooldowns[0] = 0f;
					DeadLanderOmega.state = 1;
					this.HideExtras(true);
					return;
				case 1:
					this.eye[3].gameObject.SetActive(false);
					for (int j = 0; j < 2; j++)
					{
						this.eye[1 + j].localEulerAngles = MainManager.LerpVectorAngle(this.eye[1 + j].localEulerAngles, DeadLanderOmega.eyeclose * (float)((j == 0) ? 1 : -1), MainManager.framestep * 0.25f);
					}
					if (this.cooldowns[0] < 20f)
					{
						this.cooldowns[0] += MainManager.framestep;
						return;
					}
					this.eyerender.material.color = MainManager.map.fogcolor;
					this.cooldowns[0] = 0f;
					DeadLanderOmega.state = 2;
					DeadLanderOmega.hand.Play("0");
					MainManager.PlaySound("OmegaMove");
					this.TieEnemy();
					return;
				case 2:
					this.closetime = 0f;
					this.Close();
					DeadLanderOmega.hand.transform.position = MainManager.player.transform.position + MainManager.SmoothLerp(DeadLanderOmega.handstart, DeadLanderOmega.handoffset, Mathf.Clamp(this.cooldowns[0], 0f, 50f) / 50f);
					if (this.cooldowns[0] < 80f)
					{
						this.cooldowns[0] += MainManager.framestep;
						return;
					}
					MainManager.PlaySound("OmegaDrop", -1, 1.2f, 1f);
					DeadLanderOmega.hand.Play("1");
					this.enemy.overridebehavior = false;
					this.enemy.entity.LockRigid(false);
					this.enemy.entity.onground = false;
					this.enemy.entity.startpos = new Vector3?(MainManager.player.transform.position);
					this.enemy.transform.parent = MainManager.map.transform;
					this.cooldowns[0] = 0f;
					DeadLanderOmega.state = 3;
					return;
				case 3:
					if (this.cooldowns[0] < 20f)
					{
						this.cooldowns[0] += MainManager.framestep;
						return;
					}
					this.enemy.entity.onground = false;
					base.StartCoroutine(MainManager.MoveTowards(DeadLanderOmega.hand.transform, DeadLanderOmega.hand.transform.position + DeadLanderOmega.handhide, 40f, true, false));
					DeadLanderOmega.hand.Play("0");
					DeadLanderOmega.state = 4;
					return;
				default:
					return;
				}
			}
			else if (DeadLanderOmega.activeid == this.thisid)
			{
				if (!this.activated)
				{
					base.transform.position = this.start;
					this.currentpoint = 0;
					this.forward = true;
					this.closetime = 0f;
					if (this.returnonpatrol)
					{
						this.forcelook = false;
						this.points = this.ogpoints;
					}
					MainManager.ShakeScreen(0.1f, 0.25f, true);
					MainManager.PlaySound("OmegaEye");
					if (this.extras.Length != 0)
					{
						for (int k = 0; k < this.extras.Length; k++)
						{
							this.extras[k].position = this.expos[k];
						}
					}
					this.activated = true;
					this.LookAt();
				}
				DeadLanderOmega.hand.transform.position = DeadLanderOmega.offscreen;
				if (!this.eye[3].gameObject.activeSelf)
				{
					this.eyerender.material.color = Color.white;
					this.eye[3].gameObject.SetActive(true);
				}
				if (this.cooldowns[1] < this.framedelay)
				{
					this.cooldowns[0] = 0f;
					this.cooldowns[1] += MainManager.framestep;
					if (!this.setpoint && this.points.Length > 1)
					{
						if (this.forward)
						{
							this.currentpoint++;
							if (this.currentpoint + 1 >= this.points.Length)
							{
								this.forward = false;
							}
						}
						else
						{
							this.currentpoint--;
							if (this.currentpoint - 1 < 0)
							{
								this.forward = true;
							}
						}
						this.setpoint = true;
					}
				}
				else
				{
					this.setpoint = false;
					if (this.cooldowns[0] < this.framespeed)
					{
						this.cooldowns[0] += MainManager.framestep;
					}
					else
					{
						this.cooldowns[1] = 0f;
					}
					if (this.looking == null)
					{
						this.LookAt();
					}
				}
				if (this.cooldowns[2] <= 0f)
				{
					for (int l = 0; l < 2; l++)
					{
						this.eye[1 + l].localEulerAngles = DeadLanderOmega.eyeclose * (float)((l == 0) ? 1 : -1);
					}
					this.closetime = 0f;
					this.cooldowns[2] = 225f;
				}
				else
				{
					this.cooldowns[2] -= MainManager.framestep;
				}
				this.eyerender.material.mainTextureOffset = Vector2.zero;
				for (int m = 0; m < 2; m++)
				{
					this.eye[1 + m].localEulerAngles = MainManager.LerpVectorAngle(this.eye[1 + m].localEulerAngles, DeadLanderOmega.searching * (float)((m == 0) ? 1 : -1), this.closetime / 30f);
				}
				if (this.closetime < 30f)
				{
					this.closetime += MainManager.framestep;
					return;
				}
			}
			else
			{
				if (this.activated)
				{
					this.closetime = 30f;
					this.activated = false;
					this.eye[3].gameObject.SetActive(false);
					this.cooldowns[0] = 0f;
					this.currentpoint = 0;
					this.forward = true;
					this.HideExtras(true);
				}
				this.Close();
			}
		}
	}

	// Token: 0x0600019C RID: 412 RVA: 0x000142A0 File Offset: 0x000124A0
	private void HideExtras(bool move)
	{
		if (move)
		{
			if (this.extras.Length != 0)
			{
				for (int i = 0; i < this.extras.Length; i++)
				{
					base.StartCoroutine(MainManager.MoveTowards(this.extras[i], this.extras[i].transform.position + DeadLanderOmega.behind, 10f, true, false));
				}
				return;
			}
			if (this.extras.Length != 0)
			{
				for (int j = 0; j < this.extras.Length; j++)
				{
					this.extras[j].position = DeadLanderOmega.offscreen;
				}
			}
		}
	}

	// Token: 0x0600019D RID: 413 RVA: 0x00014338 File Offset: 0x00012538
	private void Close()
	{
		if (this.closetime > 0f)
		{
			this.closetime -= MainManager.framestep;
		}
		else if (this.closetime > -5f)
		{
			this.eyerender.material.color = MainManager.map.fogcolor;
			this.closetime = -5.5f;
		}
		else
		{
			this.HideExtras(false);
			base.transform.position = MainManager.SmoothLerp(base.transform.position, this.start + DeadLanderOmega.behind, MainManager.framestep * 0.3f);
		}
		for (int i = 0; i < 2; i++)
		{
			this.eye[1 + i].localEulerAngles = MainManager.LerpVectorAngle(this.eye[1 + i].localEulerAngles, DeadLanderOmega.eyeclose * (float)((i == 0) ? 1 : -1), 1f - this.closetime / 30f);
		}
	}

	// Token: 0x0600019E RID: 414 RVA: 0x0001442C File Offset: 0x0001262C
	private void OnTriggerStay(Collider other)
	{
		if (!DeadLanderOmega.detected && DeadLanderOmega.activeid == this.thisid && MainManager.FreePlayer(false) && !MainManager.player.digging && MainManager.battle == null && MainManager.player.transform == other.transform)
		{
			DeadLanderOmega.detected = true;
			this.activated = false;
			MainManager.ChangeMusic("Alert", 0.15f);
			this.cooldowns[0] = 0f;
			MainManager.instance.flags[585] = true;
			MainManager.ShakeScreen(0.15f, 0.35f, true);
			MainManager.PlaySound("Wam");
			this.eyerender.material.mainTextureOffset = DeadLanderOmega.texoffset;
			this.TieEnemy();
		}
	}

	// Token: 0x0600019F RID: 415 RVA: 0x00014504 File Offset: 0x00012704
	private void TieEnemy()
	{
		this.enemy.RespawnEnemy(this.enemy, DeadLanderOmega.offscreen);
		this.enemy.entity.LockRigid(true);
		this.enemy.transform.parent = DeadLanderOmega.hand.transform;
		this.enemy.transform.localPosition = new Vector3(0f, -6f, -0.15f);
		this.enemy.overridebehavior = true;
		this.enemy.entity.animstate = 11;
	}

	// Token: 0x060001A0 RID: 416 RVA: 0x00014594 File Offset: 0x00012794
	private void LookAt()
	{
		if (this.points.Length == 1)
		{
			base.transform.eulerAngles = this.points[0];
			return;
		}
		base.transform.eulerAngles = MainManager.LerpVectorAngle(this.points[this.currentpoint], this.points[this.currentpoint + (this.forward ? 1 : -1)], Mathf.SmoothStep(0f, 1f, this.cooldowns[0] / this.framespeed));
	}

	// Token: 0x060001A1 RID: 417 RVA: 0x00014624 File Offset: 0x00012824
	public static DeadLanderOmega GetOmega(int id)
	{
		DeadLanderOmega[] array = Object.FindObjectsOfType<DeadLanderOmega>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].thisid == id)
			{
				return array[i];
			}
		}
		return null;
	}

	// Token: 0x060001A2 RID: 418 RVA: 0x00014655 File Offset: 0x00012855
	public void ForceLook(Vector3 position)
	{
		if (this.activated)
		{
			this.looking = base.StartCoroutine(this.ForceLookAt(position));
			return;
		}
		this.points = new Vector3[]
		{
			position
		};
	}

	// Token: 0x060001A3 RID: 419 RVA: 0x00014687 File Offset: 0x00012887
	private IEnumerator ForceLookAt(Vector3 pos)
	{
		this.forcelook = true;
		float a = 0f;
		Vector3 p = base.transform.eulerAngles;
		for (;;)
		{
			if (MainManager.FreePlayer(false))
			{
				this.points = new Vector3[]
				{
					MainManager.LerpVectorAngle(p, pos, Mathf.SmoothStep(0f, 1f, a / this.framespeed))
				};
				this.LookAt();
				a += MainManager.framestep;
				yield return null;
				if (a >= this.framespeed + 1f)
				{
					break;
				}
			}
			else
			{
				yield return null;
			}
		}
		this.points = new Vector3[]
		{
			pos
		};
		this.looking = null;
		yield break;
	}

	// Token: 0x04000127 RID: 295
	public static Animator hand;

	// Token: 0x04000128 RID: 296
	private Transform[] eye;

	// Token: 0x04000129 RID: 297
	public int enemyentityid;

	// Token: 0x0400012A RID: 298
	public int thisid;

	// Token: 0x0400012B RID: 299
	public float framespeed;

	// Token: 0x0400012C RID: 300
	public float framedelay;

	// Token: 0x0400012D RID: 301
	public Vector3[] points;

	// Token: 0x0400012E RID: 302
	public Transform[] extras;

	// Token: 0x0400012F RID: 303
	private Vector3[] ogpoints;

	// Token: 0x04000130 RID: 304
	private NPCControl enemy;

	// Token: 0x04000131 RID: 305
	private float[] cooldowns;

	// Token: 0x04000132 RID: 306
	private float closetime;

	// Token: 0x04000133 RID: 307
	private bool forward = true;

	// Token: 0x04000134 RID: 308
	private bool setpoint;

	// Token: 0x04000135 RID: 309
	private bool activated;

	// Token: 0x04000136 RID: 310
	private int currentpoint;

	// Token: 0x04000137 RID: 311
	private Vector3 start;

	// Token: 0x04000138 RID: 312
	public bool forcelook;

	// Token: 0x04000139 RID: 313
	public bool returnonpatrol;

	// Token: 0x0400013A RID: 314
	private const float closedtime = 30f;

	// Token: 0x0400013B RID: 315
	private const float blinkdelay = 225f;

	// Token: 0x0400013C RID: 316
	private Rigidbody rigid;

	// Token: 0x0400013D RID: 317
	private Renderer eyerender;

	// Token: 0x0400013E RID: 318
	private Collider detector;

	// Token: 0x0400013F RID: 319
	private Coroutine looking;

	// Token: 0x04000140 RID: 320
	public static int state;

	// Token: 0x04000141 RID: 321
	public static int activeid = -1;

	// Token: 0x04000142 RID: 322
	public static bool detected;

	// Token: 0x04000143 RID: 323
	private Vector3[] expos;

	// Token: 0x04000144 RID: 324
	private static Vector3 searching = new Vector3(0f, 0f, 35f);

	// Token: 0x04000145 RID: 325
	private static Vector3 found = new Vector3(0f, 0f, 50f);

	// Token: 0x04000146 RID: 326
	private static Vector3 handoffset = new Vector3(0f, 11f, -0.1f);

	// Token: 0x04000147 RID: 327
	private static Vector3 handstart = new Vector3(0f, 15f, -0.1f);

	// Token: 0x04000148 RID: 328
	private static Vector3 offscreen = new Vector3(0f, 999f);

	// Token: 0x04000149 RID: 329
	private static Vector3 handhide = new Vector3(0f, 200f);

	// Token: 0x0400014A RID: 330
	private static Vector3 eyeclose = new Vector3(0f, 0f, -2f);

	// Token: 0x0400014B RID: 331
	private static Vector3 behind = new Vector3(0f, 3f, 20f);

	// Token: 0x0400014C RID: 332
	private static Vector2 texoffset = new Vector2(0.0625f, 0f);
}
