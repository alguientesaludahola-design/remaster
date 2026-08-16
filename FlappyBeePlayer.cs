using System;
using UnityEngine;

// Token: 0x02000025 RID: 37
public class FlappyBeePlayer : MonoBehaviour
{
	// Token: 0x060003BC RID: 956 RVA: 0x00027490 File Offset: 0x00025690
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (this.main != null)
		{
			if (other.transform == this.main.item.transform)
			{
				this.main.UseItem();
				return;
			}
			this.main.StartCoroutine(this.main.Dead(other.transform));
		}
	}

	// Token: 0x060003BD RID: 957 RVA: 0x000274F1 File Offset: 0x000256F1
	private void OnCollisionEnter2D(Collision2D other)
	{
		if (this.main != null)
		{
			this.main.StartCoroutine(this.main.Dead(other.transform));
		}
	}

	// Token: 0x0400033A RID: 826
	public FlappyBee main;
}
