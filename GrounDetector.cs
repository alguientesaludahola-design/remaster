using System;
using UnityEngine;

// Token: 0x0200002E RID: 46
public class GroundDetector : MonoBehaviour
{
	// Token: 0x06000400 RID: 1024 RVA: 0x00028E7C File Offset: 0x0002707C
	private void Update()
	{
		if (this.overridecd > 0f)
		{
			this.overridecd -= MainManager.framestep;
		}
	}

	// Token: 0x06000401 RID: 1025 RVA: 0x00028EA0 File Offset: 0x000270A0
	private void OnTriggerStay(Collider other)
	{
		if (this.overridecd <= 0f && (other.gameObject.layer == 8 || other.gameObject.layer == 13))
		{
			if (!this.ceilingdetector)
			{
				if (this.hparent != null)
				{
					this.hparent.onground = true;
					return;
				}
				if (MainManager.player != null && this.parent.transform == MainManager.player.transform)
				{
					if (other.gameObject.layer != 13)
					{
						MainManager.player.candig = true;
					}
					else
					{
						MainManager.player.candig = false;
					}
					MainManager.player.standingon = other;
				}
				if (other.CompareTag("PushPlatform"))
				{
					if (MainManager.FreePlayer(false) && MainManager.map.lastwater != null && other.transform.parent.parent == MainManager.map.lastwater.transform)
					{
						this.parent.transform.position += MainManager.map.lastwater.riverammount * MainManager.TieFramerate(1f);
					}
				}
				else if ((other.CompareTag("Platform") || other.CompareTag("PlatformNoClock")) && this.parent.transform.parent != other.transform)
				{
					this.parent.transform.localScale = Vector3.one;
					this.parent.transform.eulerAngles = Vector3.zero;
					this.parent.transform.parent = other.transform;
					this.parent.noclock = other.CompareTag("PlatformNoClock");
					this.platform = other.transform;
				}
				if (this.parent.npcdata != null && this.parent.npcdata.startlife >= 15f && this.parent.npcdata.entitytype == NPCControl.NPCType.Object && this.parent.npcdata.objecttype == NPCControl.ObjectTypes.PushRock && !this.parent.onground && this.parent.npcdata.actioncooldown <= 0f)
				{
					MainManager.DeathSmoke(this.parent.transform.position + MainManager.instance.globalcamdir.forward * -0.35f);
					this.parent.PlaySound("Thud", 0.4f, 1.5f);
				}
				this.parent.onground = true;
				return;
			}
			else
			{
				MainManager.player.ceiling = true;
			}
		}
	}

	// Token: 0x06000402 RID: 1026 RVA: 0x00029170 File Offset: 0x00027370
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == 8 || other.gameObject.layer == 13)
		{
			if (this.ceilingdetector)
			{
				MainManager.player.ceiling = false;
				return;
			}
			if (!(this.hparent != null))
			{
				this.parent.onground = false;
				if (MainManager.player != null && this.parent.transform == MainManager.player.transform)
				{
					MainManager.player.candig = false;
					MainManager.player.standingon = null;
				}
				if (other.CompareTag("Platform") || other.CompareTag("PlatformNoClock"))
				{
					if (this.parent.transform == MainManager.player.transform || this.parent.gameObject.CompareTag("PFollower"))
					{
						this.parent.transform.parent = null;
					}
					else
					{
						this.parent.transform.parent = MainManager.map.transform;
					}
					this.parent.transform.eulerAngles = Vector3.zero;
					this.parent.transform.localScale = Vector3.one;
					this.parent.noclock = false;
					this.platform = null;
				}
			}
		}
	}

	// Token: 0x0400038B RID: 907
	public EntityControl parent;

	// Token: 0x0400038C RID: 908
	public Hornable hparent;

	// Token: 0x0400038D RID: 909
	public Transform platform;

	// Token: 0x0400038E RID: 910
	public float platformscale = 1f;

	// Token: 0x0400038F RID: 911
	public float overridecd;

	// Token: 0x04000390 RID: 912
	public bool invertplatform;

	// Token: 0x04000391 RID: 913
	public bool ceilingdetector;
}
