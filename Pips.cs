using System;
using UnityEngine;

// Token: 0x02000047 RID: 71
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(SphereCollider))]
public class Pips : MonoBehaviour
{
	// Token: 0x06000718 RID: 1816 RVA: 0x00060CA4 File Offset: 0x0005EEA4
	private void Start()
	{
		this.collider = base.GetComponent<BoxCollider>();
		this.trigger = base.GetComponent<SphereCollider>();
		this.collider.enabled = false;
		this.rigidbody = base.GetComponent<Rigidbody>();
		Physics.IgnoreCollision(this.collider, MainManager.player.entity.ccol, true);
		Physics.IgnoreCollision(this.collider, MainManager.player.entity.detect, true);
		this.transform = base.gameObject.transform;
		this.rigidbody.velocity = MainManager.RandomItemBounce(4f, this.jump);
		this.transform.localScale = Vector3.one * 0.65f;
	}

	// Token: 0x06000719 RID: 1817 RVA: 0x00060D60 File Offset: 0x0005EF60
	private void Update()
	{
		if (this.getPos != null)
		{
			this.transform.localScale = Vector3.Lerp(this.scale, Vector3.zero, this.anim / 60f);
			this.transform.position = MainManager.BeizierCurve3(this.getPos.Value, MainManager.player.entity.transform.position + Vector3.up, 3f, this.anim / 60f);
			this.anim += MainManager.framestep;
			if (this.anim >= 60f)
			{
				Object.Destroy(base.gameObject);
			}
			return;
		}
		if (!MainManager.instance.pause)
		{
			if (this.velocity != null)
			{
				this.rigidbody.velocity = this.velocity.Value;
				this.rigidbody.isKinematic = false;
				this.rigidbody.useGravity = true;
				this.velocity = null;
			}
			if (MainManager.FreePlayer(true))
			{
				this.aliveTime -= MainManager.framestep;
			}
			if (this.bounces > 0)
			{
				this.collider.enabled = (this.groundCD <= 0f);
				if (this.groundCD > 0f)
				{
					this.groundCD -= MainManager.framestep;
					RaycastHit raycastHit;
					if (Physics.Linecast(this.transform.position + Vector3.up * 2f, this.transform.position, out raycastHit, 8448) && this.transform.position.y < raycastHit.point.y)
					{
						this.transform.position = raycastHit.point + Vector3.up * 0.3f;
					}
				}
				else if (Physics.BoxCast(this.transform.position + Vector3.down * 0.5f, Pips.feetSize, Vector3.zero, Quaternion.identity, 0f, 8448))
				{
					this.groundCD = 45f;
					this.bounces--;
					MainManager.PlaySound("ItemBounce" + (int)this.type);
					if (this.bounces == 0)
					{
						this.collider.enabled = false;
						this.rigidbody.isKinematic = true;
						RaycastHit raycastHit2;
						if (Physics.Linecast(this.transform.position + Vector3.up, this.transform.position + Vector3.down * 2f, out raycastHit2, 8448))
						{
							this.transform.position = raycastHit2.point;
						}
					}
					else
					{
						this.jump *= 0.85f;
						this.rigidbody.velocity = new Vector3(this.rigidbody.velocity.x, this.jump, this.rigidbody.velocity.z);
					}
				}
			}
			else
			{
				this.rigidbody.velocity = Vector3.zero;
			}
			if (this.aliveTime < 100f)
			{
				this.ChangeRenderers(null);
			}
			if (this.aliveTime <= 0f)
			{
				Object.Destroy(base.gameObject);
				return;
			}
		}
		else if (this.velocity == null)
		{
			this.velocity = new Vector3?(this.rigidbody.velocity);
			this.rigidbody.isKinematic = true;
			this.rigidbody.useGravity = false;
			this.rigidbody.velocity = Vector3.zero;
			this.ChangeRenderers(new bool?(true));
		}
	}

	// Token: 0x0600071A RID: 1818 RVA: 0x0006112C File Offset: 0x0005F32C
	private void ChangeRenderers(bool? state = null)
	{
		for (int i = 0; i < this.renderers.Length; i++)
		{
			this.renderers[i].enabled = ((state == null) ? (!this.renderers[i].enabled) : state.Value);
		}
	}

	// Token: 0x0600071B RID: 1819 RVA: 0x0006117C File Offset: 0x0005F37C
	private void OnTriggerEnter(Collider other)
	{
		if (this.groundCD <= 0f && other.transform == MainManager.player.transform)
		{
			Pips.Type type = this.type;
			if (type != Pips.Type.HP)
			{
				if (type == Pips.Type.TP)
				{
					MainManager.instance.tp = Mathf.Clamp(MainManager.instance.tp + 1, 0, MainManager.instance.maxtp);
				}
			}
			else
			{
				for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
				{
					MainManager.instance.playerdata[i].hp = Mathf.Clamp(MainManager.instance.playerdata[i].hp + 1, 0, MainManager.instance.playerdata[i].maxhp);
				}
			}
			for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
			{
				MainManager.HealParticle(MainManager.instance.playerdata[j].entity.transform, Vector3.one, Vector3.up);
			}
			this.collider.enabled = false;
			this.trigger.enabled = false;
			MainManager.PlaySound("ItemBounce" + (int)this.type);
			MainManager.instance.hudcooldown = 300f;
			MainManager.instance.RefreshPlayer(false);
			this.scale = this.transform.localScale;
			this.getPos = new Vector3?(this.transform.position);
		}
	}

	// Token: 0x040006DA RID: 1754
	public Pips.Type type;

	// Token: 0x040006DB RID: 1755
	private Rigidbody rigidbody;

	// Token: 0x040006DC RID: 1756
	public BoxCollider collider;

	// Token: 0x040006DD RID: 1757
	private SphereCollider trigger;

	// Token: 0x040006DE RID: 1758
	private float aliveTime = 600f;

	// Token: 0x040006DF RID: 1759
	private float groundCD = 30f;

	// Token: 0x040006E0 RID: 1760
	private float jump = 12.5f;

	// Token: 0x040006E1 RID: 1761
	private float anim;

	// Token: 0x040006E2 RID: 1762
	public SpriteRenderer[] renderers;

	// Token: 0x040006E3 RID: 1763
	private Vector3? getPos;

	// Token: 0x040006E4 RID: 1764
	private Vector3? velocity;

	// Token: 0x040006E5 RID: 1765
	private Vector3 scale;

	// Token: 0x040006E6 RID: 1766
	private int bounces = 3;

	// Token: 0x040006E7 RID: 1767
	private const float maxLife = 600f;

	// Token: 0x040006E8 RID: 1768
	private new Transform transform;

	// Token: 0x040006E9 RID: 1769
	private static readonly Vector3 feetSize = new Vector3(0.35f, 0.5f, 0.35f);

	// Token: 0x040006EA RID: 1770
	private const float time = 60f;

	// Token: 0x02000271 RID: 625
	public enum Type
	{
		// Token: 0x040020D5 RID: 8405
		HP,
		// Token: 0x040020D6 RID: 8406
		TP
	}
}
