using System;
using UnityEngine;

// Token: 0x02000021 RID: 33
[CreateAssetMenu(fileName = "Fishing Items", menuName = "Bug Fables/Fishing Items")]
public class FishingItems : ScriptableObject
{
	// Token: 0x040002D5 RID: 725
	public FishingItems.Group[] groups;

	// Token: 0x020001E1 RID: 481
	public enum Groups
	{
		// Token: 0x04001621 RID: 5665
		None = -1,
		// Token: 0x04001622 RID: 5666
		Bombs,
		// Token: 0x04001623 RID: 5667
		Poison,
		// Token: 0x04001624 RID: 5668
		Freeze,
		// Token: 0x04001625 RID: 5669
		Numb,
		// Token: 0x04001626 RID: 5670
		Sleep
	}

	// Token: 0x020001E2 RID: 482
	[Serializable]
	public struct Group
	{
		// Token: 0x04001627 RID: 5671
		public FishingItems.Groups type;

		// Token: 0x04001628 RID: 5672
		public MainManager.Items[] items;
	}
}
