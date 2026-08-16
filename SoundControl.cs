using System;
using UnityEngine;

// Token: 0x02000053 RID: 83
public class SoundControl : MonoBehaviour
{
	// Token: 0x06000766 RID: 1894 RVA: 0x000663F8 File Offset: 0x000645F8
	private void Start()
	{
		if (this.source == null)
		{
			this.source = base.GetComponent<AudioSource>();
		}
		this.source.volume = 0f;
	}

	// Token: 0x06000767 RID: 1895 RVA: 0x00066424 File Offset: 0x00064624
	private void LateUpdate()
	{
		if (!this.started && MainManager.instance.globalcooldown <= 0f)
		{
			this.source.volume = this.startvolume * (this.musicvolume ? MainManager.musicvolume : MainManager.soundvolume);
			this.started = true;
		}
	}

	// Token: 0x0400078D RID: 1933
	public float startvolume;

	// Token: 0x0400078E RID: 1934
	public AudioSource source;

	// Token: 0x0400078F RID: 1935
	public bool musicvolume;

	// Token: 0x04000790 RID: 1936
	private bool started;
}
