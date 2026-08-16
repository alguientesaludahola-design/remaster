using System;
using UnityEngine;

// Token: 0x02000041 RID: 65
public class MusicSpinner : MonoBehaviour
{
	// Token: 0x0600069E RID: 1694 RVA: 0x0004AACD File Offset: 0x00048CCD
	private void Start()
	{
		Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
		rigidbody.isKinematic = true;
		rigidbody.useGravity = false;
		if (!this.HasItem() && MainManager.BadgeIsEquipped(2))
		{
			MainManager.map.hiddenitem = new int?(100);
		}
	}

	// Token: 0x0600069F RID: 1695 RVA: 0x0004AB08 File Offset: 0x00048D08
	private void LateUpdate()
	{
		float num = Mathf.Clamp(this.spin, 0f, this.maxspin);
		if (this.spin > 0f && !MainManager.instance.pause && !MainManager.instance.inevent)
		{
			this.time += num / this.maxspin * 2f * MainManager.framestep;
			if (this.playedindex < this.notes.Length && this.time / 60f > this.notes[this.playedindex].x)
			{
				MainManager.PlaySound(this.soundclip, -1, this.notes[this.playedindex].y);
				this.playedindex++;
			}
			if (this.time > this.maxtime)
			{
				this.time -= this.maxtime;
				this.playedindex = 0;
			}
			this.spin -= MainManager.framestep * this.spinstop;
			for (int i = 0; i < this.spinner.Length; i++)
			{
				if (this.spinner[i].gameObject.isStatic)
				{
					this.spinner[i].gameObject.isStatic = false;
				}
				this.spinner[i].Rotate(this.rotation[i] * MainManager.framestep * num);
			}
		}
		else if (MainManager.instance.inevent)
		{
			this.spin = 0f;
		}
		if (MainManager.music[0].isPlaying && MainManager.musiccoroutine == null)
		{
			MainManager.music[0].volume = Mathf.Lerp(MainManager.musicvolume, MainManager.musicvolume * this.musiclower, num / this.maxspin);
		}
	}

	// Token: 0x060006A0 RID: 1696 RVA: 0x0004ACDC File Offset: 0x00048EDC
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("BeetleHorn"))
		{
			MainManager.HitPart(other.transform.position + Vector3.up);
			MainManager.PlaySound("Damage0", -1, 0.7f, 1f);
			this.spin = Mathf.Clamp(this.spin + this.spinhit, 0f, this.spinlimit);
			if (!this.itemspitout && this.spin + this.spinhit > this.spinlimit && !this.HasItem())
			{
				MainManager.DeathSmoke(this.itempos);
				this.itemspitout = true;
				MainManager.PlaySound("PingDown");
				NPCControl npccontrol = EntityControl.CreateItem(this.itempos, this.itemtype, this.itemid, this.bouncepos, this.itemtime);
				npccontrol.insideid = MainManager.instance.insideid;
				if (this.itemtype != 3)
				{
					npccontrol.activationflag = this.flag;
				}
			}
		}
	}

	// Token: 0x060006A1 RID: 1697 RVA: 0x0004ADDC File Offset: 0x00048FDC
	private bool HasItem()
	{
		return this.flag == -1 || (this.itemtype == 3 && MainManager.instance.crystalbflags[this.flag]) || (this.itemtype != 3 && MainManager.instance.flags[this.flag]);
	}

	// Token: 0x040005F9 RID: 1529
	public Vector2[] notes;

	// Token: 0x040005FA RID: 1530
	public float maxtime;

	// Token: 0x040005FB RID: 1531
	public float spinlimit = 10f;

	// Token: 0x040005FC RID: 1532
	public float maxspin = 5f;

	// Token: 0x040005FD RID: 1533
	public float spinstop = 0.25f;

	// Token: 0x040005FE RID: 1534
	public float spinhit = 1.25f;

	// Token: 0x040005FF RID: 1535
	public float musiclower = 0.5f;

	// Token: 0x04000600 RID: 1536
	public int flag = -1;

	// Token: 0x04000601 RID: 1537
	public int itemtype;

	// Token: 0x04000602 RID: 1538
	public int itemid;

	// Token: 0x04000603 RID: 1539
	public int itemtime = -1;

	// Token: 0x04000604 RID: 1540
	private float time;

	// Token: 0x04000605 RID: 1541
	private float spin;

	// Token: 0x04000606 RID: 1542
	private float startmusicvol;

	// Token: 0x04000607 RID: 1543
	private int playedindex;

	// Token: 0x04000608 RID: 1544
	public Vector3 itempos;

	// Token: 0x04000609 RID: 1545
	public Vector3 bouncepos;

	// Token: 0x0400060A RID: 1546
	public Transform[] spinner;

	// Token: 0x0400060B RID: 1547
	public Vector3[] rotation;

	// Token: 0x0400060C RID: 1548
	public AudioClip soundclip;

	// Token: 0x0400060D RID: 1549
	public bool musicvolume;

	// Token: 0x0400060E RID: 1550
	private bool itemspitout;
}
