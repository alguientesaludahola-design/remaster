using System;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x0200000A RID: 10
public class Caravan : MonoBehaviour
{
	// Token: 0x0600016A RID: 362 RVA: 0x00010BA0 File Offset: 0x0000EDA0
	private void Start()
	{
		this.snail = base.GetComponentInChildren<Animator>();
		this.s = this.snail.GetComponent<SpriteRenderer>();
		this.s.shadowCastingMode = ShadowCastingMode.TwoSided;
		if (!MainManager.instance.flags[41] || Random.Range(0, 3) == 0)
		{
			this.snail.Play("Sleep");
			this.isslep = true;
		}
		if (this.facingright)
		{
			this.offset = new Vector3(0.25f, 0f);
		}
	}

	// Token: 0x0600016B RID: 363 RVA: 0x00010C24 File Offset: 0x0000EE24
	private void LateUpdate()
	{
		if (!this.isslep && MainManager.player != null && Time.frameCount % 3 == 0)
		{
			if (Vector3.Distance(base.transform.position + this.offset, MainManager.ClampMagnitude(MainManager.player.transform.position, float.PositiveInfinity, 4.5f)) < 4f && this.PlayerRightDirection() && MainManager.player.transform.position.z < base.transform.position.z)
			{
				this.snail.Play(string.Concat(this.GetAngle()));
			}
			else
			{
				this.snail.Play("Idle");
			}
		}
		this.s.enabled = (this.insideid == MainManager.instance.insideid);
	}

	// Token: 0x0600016C RID: 364 RVA: 0x00010D10 File Offset: 0x0000EF10
	private int GetAngle()
	{
		if (this.facingright)
		{
			return Mathf.Clamp(4 - Mathf.FloorToInt(Vector3.Angle(base.transform.position, MainManager.player.transform.position)), 0, 4);
		}
		return Mathf.Clamp(Mathf.FloorToInt(Vector3.Angle(base.transform.position, MainManager.player.transform.position)), 0, 4);
	}

	// Token: 0x0600016D RID: 365 RVA: 0x00010D80 File Offset: 0x0000EF80
	private bool PlayerRightDirection()
	{
		if (this.facingright)
		{
			return MainManager.player.transform.position.x > base.transform.position.x;
		}
		return MainManager.player.transform.position.x < base.transform.position.x;
	}

	// Token: 0x0600016E RID: 366 RVA: 0x00010DE2 File Offset: 0x0000EFE2
	public void Refresh()
	{
		if (this.isslep)
		{
			this.snail.Play("Sleep");
			return;
		}
		this.snail.Play("Idle");
	}

	// Token: 0x040000D7 RID: 215
	private Animator snail;

	// Token: 0x040000D8 RID: 216
	public bool isslep;

	// Token: 0x040000D9 RID: 217
	public bool facingright;

	// Token: 0x040000DA RID: 218
	public int insideid = -1;

	// Token: 0x040000DB RID: 219
	private SpriteRenderer s;

	// Token: 0x040000DC RID: 220
	private Vector3 offset = new Vector3(-0.25f, 0f);
}
