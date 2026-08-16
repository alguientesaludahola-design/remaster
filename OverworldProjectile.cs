using System;
using UnityEngine;

// Token: 0x02000044 RID: 68
public class OverworldProjectile : MonoBehaviour
{
	// Token: 0x060006F0 RID: 1776 RVA: 0x000595E4 File Offset: 0x000577E4
	private void Start()
	{
		base.tag = "Projectile";
		this.startpos = base.transform.position;
		this.starttime = this.time;
		this.time = 0f;
		BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
		boxCollider.size = Vector3.one;
		boxCollider.isTrigger = true;
		if (this.particleondeath == "Rock")
		{
			this.sprite.enabled = false;
		}
	}

	// Token: 0x060006F1 RID: 1777 RVA: 0x00059660 File Offset: 0x00057860
	private void Update()
	{
		if (!MainManager.instance.minipause && !MainManager.instance.pause && !MainManager.instance.inevent)
		{
			if (this.arc > 0.1f)
			{
				base.transform.position = MainManager.BeizierCurve3(this.startpos, this.target, this.arc, this.time / this.starttime);
			}
			else
			{
				base.transform.position = Vector3.Lerp(this.startpos, this.target, this.time / this.starttime);
			}
			this.spinamt += this.spin * MainManager.TieFramerate(1f);
			if (this.sprite != null)
			{
				this.sprite.transform.eulerAngles = this.angle + this.spinamt;
			}
			this.time += MainManager.TieFramerate(1f);
			if (this.time >= this.starttime)
			{
				this.DestroyThis();
			}
		}
	}

	// Token: 0x060006F2 RID: 1778 RVA: 0x00059784 File Offset: 0x00057984
	public void DestroyThis()
	{
		if (this.particleondeath != null)
		{
			string a = this.particleondeath;
			if (!(a == "Rock"))
			{
				if (a == "explosionsmall")
				{
					if (MainManager.GetDistance(base.transform.position, MainManager.player.transform.position) < 15f)
					{
						MainManager.PlaySound("Explosion", -1, 1f, 0.75f);
						MainManager.ShakeScreen(0.1f, 0.5f, true);
					}
				}
				MainManager.PlayParticle(this.particleondeath, base.transform.position);
			}
			else
			{
				if (MainManager.GetDistance(base.transform.position, MainManager.player.transform.position) < 15f)
				{
					MainManager.PlaySound("RockBreak", -1, 1f, 0.75f);
					MainManager.ShakeScreen(0.1f, 0.5f, true);
				}
				MeshRenderer componentInChildren = base.transform.GetComponentInChildren<MeshRenderer>();
				if (componentInChildren != null)
				{
					componentInChildren.transform.parent = null;
					MainManager.CrackRock(componentInChildren.transform, true);
				}
			}
		}
		Object.Destroy(base.gameObject);
	}

	// Token: 0x060006F3 RID: 1779 RVA: 0x000598B0 File Offset: 0x00057AB0
	private void OnTriggerStay(Collider other)
	{
		if (MainManager.player != null && other.transform == MainManager.player.transform && !MainManager.instance.inevent && !MainManager.instance.minipause && !MainManager.instance.pause && !MainManager.instance.inbattle)
		{
			if (this.parent != null && !MainManager.player.shield && !this.parent.entity.iskill && this.parent.dizzytime <= 0f)
			{
				this.parent.StartBattle(true);
			}
			this.DestroyThis();
			return;
		}
		if (other.gameObject.layer == 8 || other.gameObject.layer == 13)
		{
			this.DestroyThis();
		}
	}

	// Token: 0x060006F4 RID: 1780 RVA: 0x0005998B File Offset: 0x00057B8B
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 8 || other.gameObject.layer == 13)
		{
			this.DestroyThis();
		}
	}

	// Token: 0x060006F5 RID: 1781 RVA: 0x000599B0 File Offset: 0x00057BB0
	public static OverworldProjectile NewProjectile(NPCControl parent, int spriteindex, Vector3 startpos, Vector3 target, Vector3 spin, Vector3 angle, Vector3 size, string particleondeath, float arc, float time, float shadowsize)
	{
		OverworldProjectile overworldProjectile = new GameObject("proj").AddComponent<OverworldProjectile>();
		overworldProjectile.time = time;
		overworldProjectile.transform.position = startpos;
		overworldProjectile.particleondeath = particleondeath;
		overworldProjectile.arc = arc;
		overworldProjectile.target = target;
		overworldProjectile.parent = parent;
		overworldProjectile.angle = angle;
		overworldProjectile.transform.parent = MainManager.map.transform;
		overworldProjectile.sprite = new GameObject("tempproj").AddComponent<SpriteRenderer>();
		overworldProjectile.sprite.transform.parent = overworldProjectile.transform;
		overworldProjectile.sprite.transform.localPosition = Vector3.zero;
		overworldProjectile.gameObject.layer = 14;
		overworldProjectile.sprite.transform.eulerAngles = angle;
		overworldProjectile.sprite.material = MainManager.spritemat;
		overworldProjectile.sprite.transform.position = startpos;
		overworldProjectile.sprite.transform.localScale = size;
		overworldProjectile.spin = spin;
		if (spriteindex < 0)
		{
			overworldProjectile.sprite.sprite = MainManager.itemsprites[0, Mathf.Abs(spriteindex)];
		}
		else
		{
			overworldProjectile.sprite.sprite = MainManager.instance.projectilepsrites[spriteindex];
		}
		if (shadowsize > 0f)
		{
			overworldProjectile.gameObject.AddComponent<ShadowLite>().SetUp(0.3f, shadowsize);
		}
		return overworldProjectile;
	}

	// Token: 0x04000687 RID: 1671
	private NPCControl parent;

	// Token: 0x04000688 RID: 1672
	private Vector3 target;

	// Token: 0x04000689 RID: 1673
	private Vector3 spin;

	// Token: 0x0400068A RID: 1674
	private Vector3 angle;

	// Token: 0x0400068B RID: 1675
	private Vector3 spinamt;

	// Token: 0x0400068C RID: 1676
	private float time;

	// Token: 0x0400068D RID: 1677
	private float arc;

	// Token: 0x0400068E RID: 1678
	private float starttime;

	// Token: 0x0400068F RID: 1679
	private string particleondeath;

	// Token: 0x04000690 RID: 1680
	private SpriteRenderer sprite;

	// Token: 0x04000691 RID: 1681
	private Vector3 startpos;
}
