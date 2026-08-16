using System;
using UnityEngine;

// Token: 0x0200004E RID: 78
public class RayDetector : MonoBehaviour
{
	// Token: 0x06000750 RID: 1872 RVA: 0x00065393 File Offset: 0x00063593
	private void Start()
	{
		this.entity = base.GetComponentInParent<EntityControl>();
		this.player = (this.entity.tag == "Player");
	}

	// Token: 0x06000751 RID: 1873 RVA: 0x000653BC File Offset: 0x000635BC
	private bool CheckWall(Vector3 n)
	{
		return (n.y >= 0.95f && Mathf.Abs(n.x) < 0.05f && Mathf.Abs(n.z) < 0.05f) || (n.x >= 0.95f && Mathf.Abs(n.y) < 0.05f && Mathf.Abs(n.z) < 0.05f) || (n.z >= 0.95f && Mathf.Abs(n.x) < 0.05f && Mathf.Abs(n.y) < 0.05f);
	}

	// Token: 0x06000752 RID: 1874 RVA: 0x00065460 File Offset: 0x00063660
	private void OnTriggerStay(Collider other)
	{
		if ((other.gameObject.layer == 8 || other.gameObject.layer == 10 || other.gameObject.layer == 13) && this.entity != null && other.tag != "Respawn")
		{
			if (this.player && !MainManager.instance.inevent)
			{
				RaycastHit raycastHit;
				if (this.entity.animstate != 116 && Physics.Raycast(base.transform.position, base.transform.forward, out raycastHit, 1f, 9472) && raycastHit.transform != null)
				{
					this.entity.hitwall = !this.CheckWall(raycastHit.normal);
					return;
				}
				this.entity.hitwall = true;
				return;
			}
			else
			{
				this.entity.hitwall = true;
			}
		}
	}

	// Token: 0x06000753 RID: 1875 RVA: 0x00065554 File Offset: 0x00063754
	private void OnTriggerExit(Collider other)
	{
		if ((other.gameObject.layer == 8 || other.gameObject.layer == 10 || other.gameObject.layer == 13) && this.entity != null)
		{
			this.entity.hitwall = false;
		}
	}

	// Token: 0x0400075A RID: 1882
	private EntityControl entity;

	// Token: 0x0400075B RID: 1883
	private bool player;
}
