using System;
using UnityEngine;

// Token: 0x02000020 RID: 32
[CreateAssetMenu(fileName = "Fish Data", menuName = "Bug Fables/Fishing Data")]
public class FishData : ScriptableObject
{
	// Token: 0x040002C6 RID: 710
	public FishingMain.FishIDs id;

	// Token: 0x040002C7 RID: 711
	public float startDepth;

	// Token: 0x040002C8 RID: 712
	public float maxDepth = 100f;

	// Token: 0x040002C9 RID: 713
	public float radius = 10f;

	// Token: 0x040002CA RID: 714
	public float strength = 1f;

	// Token: 0x040002CB RID: 715
	public float speed = 0.025f;

	// Token: 0x040002CC RID: 716
	public float size = 1f;

	// Token: 0x040002CD RID: 717
	public float musicPitch = 1f;

	// Token: 0x040002CE RID: 718
	public float cmSize = 1f;

	// Token: 0x040002CF RID: 719
	public int music;

	// Token: 0x040002D0 RID: 720
	public int weight = 10;

	// Token: 0x040002D1 RID: 721
	public int reqCombo;

	// Token: 0x040002D2 RID: 722
	public int reqFlag = -1;

	// Token: 0x040002D3 RID: 723
	public int money = 1;

	// Token: 0x040002D4 RID: 724
	public bool explodable = true;

	// Token: 0x020001E0 RID: 480
	public enum SpriteIndex
	{
		// Token: 0x0400161D RID: 5661
		Normal,
		// Token: 0x0400161E RID: 5662
		Caught,
		// Token: 0x0400161F RID: 5663
		Hurt
	}
}
