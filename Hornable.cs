using System;
using UnityEngine;

// Token: 0x02000032 RID: 50
public class Hornable : MonoBehaviour
{
	// Token: 0x06000419 RID: 1049 RVA: 0x0002A3C5 File Offset: 0x000285C5
	public void SetUp(Vector2 pushammount, bool pusher, bool breakondash, NPCControl createdfrom)
	{
		this.push = pushammount;
		this.pusherenabled = pusher;
		this.dashbreak = breakondash;
		this.parent = createdfrom;
	}

	// Token: 0x0600041A RID: 1050 RVA: 0x0002A3E4 File Offset: 0x000285E4
	private void Start()
	{
		this.rigid = base.GetComponent<Rigidbody>();
		this.c = base.GetComponent<Collider>();
		base.tag = "Hornable";
		this.rotater = new GameObject().transform;
		this.rotater.parent = base.transform;
		this.rotater.localPosition = Vector3.zero;
		if (this.type == Hornable.Type.IceCube)
		{
			this.pusherenabled = true;
			this.danim = base.GetComponent<DialogueAnim>();
			this.initialscale = this.danim.targetscale;
			HelpArrow.NewArrow(base.transform, Vector3.zero, Color.cyan, 2.5f, 1f);
		}
	}

	// Token: 0x0600041B RID: 1051 RVA: 0x0002A491 File Offset: 0x00028691
	private void Update()
	{
		base.transform.localScale = base.transform.localScale;
	}

	// Token: 0x0600041C RID: 1052 RVA: 0x0002A4AC File Offset: 0x000286AC
	private void LateUpdate()
	{
		if (this.ingeizer != null && !MainManager.instance.minipause && !MainManager.instance.pause)
		{
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, new Vector3(0f, 0f, 5.5f + Mathf.Abs(Mathf.Sin(Time.time * 5f) * 0.4f)), MainManager.TieFramerate(0.15f));
		}
		if (this.collisioncolldown > 0f)
		{
			this.collisioncolldown -= MainManager.framestep;
		}
		if (this.rigid.velocity.y >= -0.015f && this.rigid.velocity.y <= 0.015f && this.onground)
		{
			this.vel = Vector3.zero;
			this.c.material.dynamicFriction = 1f;
			this.c.material.staticFriction = 1f;
		}
		else
		{
			this.rigid.velocity = new Vector3(this.vel.x, this.rigid.velocity.y, this.vel.z);
			this.c.material.dynamicFriction = 0f;
			this.c.material.staticFriction = 0f;
		}
		if (!this.onground)
		{
			if (this.aircooldown < 150f)
			{
				this.aircooldown += MainManager.framestep;
				return;
			}
			this.rigid.velocity = new Vector3(0f, this.rigid.velocity.y, 0f);
			this.onground = true;
		}
	}

	// Token: 0x0600041D RID: 1053 RVA: 0x0002A67C File Offset: 0x0002887C
	public void ServerGeizer()
	{
		if (this.ingeizer != null)
		{
			base.StartCoroutine(MainManager.TempIgnoreCollision(base.transform.GetChild(0).GetComponent<Collider>(), this.ingeizer.boxcol, 5f));
			base.StartCoroutine(MainManager.TempIgnoreCollision(base.GetComponent<Collider>(), this.ingeizer.boxcol, 5f));
			base.transform.parent = MainManager.map.transform;
			this.rigid.useGravity = true;
			this.rigid.isKinematic = false;
			this.rigid.velocity = Vector3.zero;
			this.ingeizer.moveobj = null;
			this.ingeizer = null;
		}
	}

	// Token: 0x0600041E RID: 1054 RVA: 0x0002A73C File Offset: 0x0002893C
	private void OnTriggerEnter(Collider other)
	{
		if (other != null)
		{
			string tag = other.tag;
			if (!(tag == "BeetleHorn"))
			{
				if (!(tag == "IceBreak"))
				{
					if (!(tag == "Platform") && !(tag == "PlatformNoClock"))
					{
						return;
					}
					if (this.collisioncolldown <= 0f)
					{
						if (this.danim != null)
						{
							this.danim.enabled = false;
						}
						base.transform.parent = other.transform;
					}
				}
				else if (this.parent != null)
				{
					MainManager.player.entity.hitwall = true;
					this.ServerGeizer();
					this.parent.ShatterDroppletIce();
					return;
				}
			}
			else
			{
				this.rigid.useGravity = true;
				this.rigid.isKinematic = false;
				MainManager.HitPart(base.transform.position);
				MainManager.PlaySound("Damage0", -1, 0.7f, 0.25f);
				Vector3 vector = Vector3.zero;
				if (this.parent != null && this.parent.objecttype == NPCControl.ObjectTypes.Dropplet && this.parent.data.Length > 4 && this.parent.data[4] == 1)
				{
					this.rotater.LookAt(MainManager.player.transform.position);
					vector = MainManager.CardinalSnap8(this.rotater.eulerAngles, true);
				}
				else
				{
					vector = MainManager.GetDirection4(base.transform.position, MainManager.player.transform.position, true);
				}
				float d = 1f;
				if (this.ingeizer != null)
				{
					this.ServerGeizer();
					d = 2f;
				}
				this.aircooldown = 0f;
				this.rigid.velocity = new Vector3(vector.x * this.push.x, this.push.y, vector.z * this.push.x) * d;
				this.vel = this.rigid.velocity;
				this.collisioncolldown = 3f;
				this.onground = false;
				base.transform.parent = MainManager.map.transform;
				if (this.danim != null)
				{
					this.danim.enabled = true;
					return;
				}
			}
		}
	}

	// Token: 0x0600041F RID: 1055 RVA: 0x0002A998 File Offset: 0x00028B98
	private void OnTriggerStay(Collider other)
	{
		if (this.pusherenabled && other.tag == "Pusher")
		{
			MainManager.PushAway(base.transform, other.transform.position);
		}
		if (this.collisioncolldown <= 0f && (other.gameObject.layer == 8 || other.gameObject.layer == 13))
		{
			this.onground = true;
		}
	}

	// Token: 0x06000420 RID: 1056 RVA: 0x0002AA08 File Offset: 0x00028C08
	private void OnCollisionEnter(Collision other)
	{
		if (this.collisioncolldown <= 0f)
		{
			string tag = other.gameObject.tag;
			if (tag == "Platform" || tag == "PlatformNoClock")
			{
				if (this.danim != null)
				{
					this.danim.enabled = false;
				}
				base.transform.parent = other.transform;
			}
			if (other.gameObject.layer == 8 || other.gameObject.layer == 13)
			{
				this.onground = true;
			}
		}
	}

	// Token: 0x06000421 RID: 1057 RVA: 0x0002AA97 File Offset: 0x00028C97
	private void OnCollisionStay(Collision other)
	{
		if (this.collisioncolldown <= 0f && (other.gameObject.layer == 8 || other.gameObject.layer == 13))
		{
			this.onground = true;
		}
	}

	// Token: 0x06000422 RID: 1058 RVA: 0x0002AACC File Offset: 0x00028CCC
	private void OnTriggerExit(Collider other)
	{
		if (other != null && base.transform.parent != MainManager.map.transform && this.collisioncolldown <= 0f)
		{
			string tag = other.tag;
			if (tag == "Platform" || tag == "PlatformNoClock")
			{
				base.transform.parent = MainManager.map.transform;
			}
		}
	}

	// Token: 0x040003BF RID: 959
	private Vector2 push;

	// Token: 0x040003C0 RID: 960
	private bool pusherenabled;

	// Token: 0x040003C1 RID: 961
	private bool dashbreak;

	// Token: 0x040003C2 RID: 962
	private bool playerinrange;

	// Token: 0x040003C3 RID: 963
	public bool onground;

	// Token: 0x040003C4 RID: 964
	public NPCControl ingeizer;

	// Token: 0x040003C5 RID: 965
	public NPCControl parent;

	// Token: 0x040003C6 RID: 966
	private Rigidbody rigid;

	// Token: 0x040003C7 RID: 967
	private float collisioncolldown;

	// Token: 0x040003C8 RID: 968
	private float aircooldown;

	// Token: 0x040003C9 RID: 969
	public Hornable.Type type;

	// Token: 0x040003CA RID: 970
	private DialogueAnim danim;

	// Token: 0x040003CB RID: 971
	private Transform rotater;

	// Token: 0x040003CC RID: 972
	private Collider c;

	// Token: 0x040003CD RID: 973
	private Vector3 initialscale;

	// Token: 0x040003CE RID: 974
	private Vector3 vel;

	// Token: 0x020001FC RID: 508
	public enum Type
	{
		// Token: 0x04001691 RID: 5777
		IceCube
	}
}
