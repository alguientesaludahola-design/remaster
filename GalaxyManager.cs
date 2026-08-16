using System;
using System.Collections;
using Galaxy.Api;
using UnityEngine;

// Token: 0x0200002C RID: 44
public class GalaxyManager : MonoBehaviour
{
	// Token: 0x17000001 RID: 1
	// (get) Token: 0x060003E2 RID: 994 RVA: 0x000289FD File Offset: 0x00026BFD
	public GalaxyID MyGalaxyID
	{
		get
		{
			return GalaxyManager.myGalaxyID;
		}
	}

	// Token: 0x17000002 RID: 2
	// (get) Token: 0x060003E3 RID: 995 RVA: 0x00028A04 File Offset: 0x00026C04
	public bool GalaxyFullyInitialized
	{
		get
		{
			return this.galaxyFullyInitialized;
		}
	}

	// Token: 0x060003E4 RID: 996 RVA: 0x00028A0C File Offset: 0x00026C0C
	private void Awake()
	{
		if (GalaxyManager.Instance == null)
		{
			GalaxyManager.Instance = this;
			return;
		}
		Object.Destroy(this);
	}

	// Token: 0x060003E5 RID: 997 RVA: 0x00028A28 File Offset: 0x00026C28
	private void OnEnable()
	{
		this.Init();
		this.ListenersInit();
		this.SignIn();
	}

	// Token: 0x060003E6 RID: 998 RVA: 0x00028A3C File Offset: 0x00026C3C
	private void Update()
	{
		GalaxyInstance.ProcessData();
	}

	// Token: 0x060003E7 RID: 999 RVA: 0x00028A43 File Offset: 0x00026C43
	private void OnDisable()
	{
		this.ShutdownStatsAndAchievements();
		this.ListenersDispose();
	}

	// Token: 0x060003E8 RID: 1000 RVA: 0x00028A51 File Offset: 0x00026C51
	private void OnDestroy()
	{
		GalaxyInstance.Shutdown(true);
		GalaxyManager.Instance = null;
		Object.Destroy(this);
	}

	// Token: 0x060003E9 RID: 1001 RVA: 0x00028A65 File Offset: 0x00026C65
	private void ListenersInit()
	{
		if (this.authListener == null)
		{
			this.authListener = new GalaxyManager.AuthenticationListener();
		}
	}

	// Token: 0x060003EA RID: 1002 RVA: 0x00028A7A File Offset: 0x00026C7A
	private void ListenersDispose()
	{
		if (this.authListener != null)
		{
			this.authListener.Dispose();
		}
	}

	// Token: 0x060003EB RID: 1003 RVA: 0x00028A8F File Offset: 0x00026C8F
	public void StartStatsAndAchievements()
	{
		if (this.StatsAndAchievements == null)
		{
			this.StatsAndAchievements = base.gameObject.AddComponent<StatsAndAchievements>();
		}
	}

	// Token: 0x060003EC RID: 1004 RVA: 0x00028AB0 File Offset: 0x00026CB0
	public void ShutdownStatsAndAchievements()
	{
		if (this.StatsAndAchievements != null)
		{
			Object.Destroy(this.StatsAndAchievements);
		}
	}

	// Token: 0x060003ED RID: 1005 RVA: 0x00028ACC File Offset: 0x00026CCC
	private void Init()
	{
		InitParams initpParams = new InitParams(this.clientID, this.clientSecret);
		try
		{
			GalaxyInstance.Init(initpParams);
			this.galaxyFullyInitialized = true;
		}
		catch (GalaxyInstance.Error arg)
		{
			Debug.LogWarning("Init failed for reason " + arg);
			this.galaxyFullyInitialized = false;
		}
	}

	// Token: 0x060003EE RID: 1006 RVA: 0x00028B24 File Offset: 0x00026D24
	private void SignIn()
	{
		try
		{
			GalaxyInstance.User().SignInGalaxy();
		}
		catch (GalaxyInstance.Error arg)
		{
			Debug.LogWarning("SignIn failed for reason " + arg);
		}
	}

	// Token: 0x060003EF RID: 1007 RVA: 0x00028B60 File Offset: 0x00026D60
	public bool IsSignedIn()
	{
		bool result = false;
		try
		{
			result = GalaxyInstance.User().SignedIn();
		}
		catch (GalaxyInstance.Error arg)
		{
			Debug.LogWarning("Could not check user signed in status for reason " + arg);
		}
		return result;
	}

	// Token: 0x060003F0 RID: 1008 RVA: 0x00028BA0 File Offset: 0x00026DA0
	public bool IsLoggedOn()
	{
		bool result = false;
		try
		{
			result = GalaxyInstance.User().IsLoggedOn();
		}
		catch (GalaxyInstance.Error arg)
		{
			Debug.LogWarning("Could not check user logged on status for reason " + arg);
		}
		return result;
	}

	// Token: 0x060003F1 RID: 1009 RVA: 0x00028BE0 File Offset: 0x00026DE0
	public void SetAchievement(string apiKey)
	{
		base.StartCoroutine(this._SetAchievement(apiKey));
	}

	// Token: 0x060003F2 RID: 1010 RVA: 0x00028BF0 File Offset: 0x00026DF0
	private IEnumerator _SetAchievement(string apiKey)
	{
		float startTime = Time.time;
		while (!this.AchievementsReady())
		{
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			if (Time.time - startTime > 120f)
			{
				break;
			}
		}
		if (this.AchievementsReady())
		{
			this.StatsAndAchievements.SetAchievement(apiKey);
		}
		yield break;
	}

	// Token: 0x060003F3 RID: 1011 RVA: 0x00028C06 File Offset: 0x00026E06
	private bool AchievementsReady()
	{
		return this.IsSignedIn() && this.StatsAndAchievements != null && this.StatsAndAchievements.Ready();
	}

	// Token: 0x04000380 RID: 896
	private readonly string clientID = "53006155234260529";

	// Token: 0x04000381 RID: 897
	private readonly string clientSecret = "acabeb887eccaccb46262ff65156ecd4316aba697305811ae37966296c5077d4";

	// Token: 0x04000382 RID: 898
	public static GalaxyManager Instance;

	// Token: 0x04000383 RID: 899
	private StatsAndAchievements StatsAndAchievements;

	// Token: 0x04000384 RID: 900
	private static GalaxyID myGalaxyID;

	// Token: 0x04000385 RID: 901
	private bool galaxyFullyInitialized;

	// Token: 0x04000386 RID: 902
	public GalaxyManager.AuthenticationListener authListener;

	// Token: 0x04000387 RID: 903
	private const float ACHIEVEMENT_TIMEOUT = 120f;

	// Token: 0x020001F3 RID: 499
	public class AuthenticationListener : GlobalAuthListener
	{
		// Token: 0x0600106B RID: 4203 RVA: 0x00184013 File Offset: 0x00182213
		public override void OnAuthSuccess()
		{
			GalaxyManager.myGalaxyID = GalaxyInstance.User().GetGalaxyID();
			GalaxyManager.Instance.StartStatsAndAchievements();
		}

		// Token: 0x0600106C RID: 4204 RVA: 0x0018402E File Offset: 0x0018222E
		public override void OnAuthFailure(IAuthListener.FailureReason failureReason)
		{
			Debug.LogWarning("Failed to sign in for reason " + failureReason);
		}

		// Token: 0x0600106D RID: 4205 RVA: 0x00184045 File Offset: 0x00182245
		public override void OnAuthLost()
		{
			Debug.LogWarning("Authorization lost");
		}
	}
}
