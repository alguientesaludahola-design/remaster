using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000031 RID: 49
public class Hidder : MonoBehaviour
{
	// Token: 0x06000416 RID: 1046 RVA: 0x0002A248 File Offset: 0x00028448
	private void Start()
	{
		List<Renderer[]> list = new List<Renderer[]>();
		for (int i = 0; i < this.objs.Length; i++)
		{
			list.Add(this.objs[i].GetComponentsInChildren<Renderer>());
		}
		this.renders = list.ToArray();
	}

	// Token: 0x06000417 RID: 1047 RVA: 0x0002A290 File Offset: 0x00028490
	private void LateUpdate()
	{
		if (Time.frameCount % 2 == 0 && MainManager.player != null)
		{
			this.show = ((this.left == MainManager.Directions.Right && MainManager.player.transform.position.x > this.hidepos) || (this.left == MainManager.Directions.Left && MainManager.player.transform.position.x < this.hidepos) || (this.left == MainManager.Directions.Down && MainManager.player.transform.position.z < this.hidepos) || (this.left == MainManager.Directions.Up && MainManager.player.transform.position.z > this.hidepos));
			if (this.show != this.lateshow)
			{
				for (int i = 0; i < this.renders.Length; i++)
				{
					for (int j = 0; j < this.renders[i].Length; j++)
					{
						this.renders[i][j].enabled = this.show;
					}
				}
				this.lateshow = this.show;
			}
		}
	}

	// Token: 0x040003B9 RID: 953
	public float hidepos;

	// Token: 0x040003BA RID: 954
	public MainManager.Directions left;

	// Token: 0x040003BB RID: 955
	private bool show = true;

	// Token: 0x040003BC RID: 956
	private bool lateshow = true;

	// Token: 0x040003BD RID: 957
	public GameObject[] objs;

	// Token: 0x040003BE RID: 958
	private Renderer[][] renders;
}
