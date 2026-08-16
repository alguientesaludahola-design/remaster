using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using InputIOManager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Token: 0x0200003A RID: 58
public class MainManager : MonoBehaviour
{
	// Token: 0x06000439 RID: 1081 RVA: 0x0002B34C File Offset: 0x0002954C
	private void Start()
	{
		Application.runInBackground = !InputIO.IsConsole;
		Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
		if (!MainManager.basicload)
		{
			InputIO.StartUp();
		}
		Time.timeScale = 1f;
		MainManager.MainCamera = Camera.main;
		MainManager.musicresume = -1f;
		MainManager.instance = this;
		base.StartCoroutine(MainManager.LoadEverything());
		if (!InputIO.IsEditor)
		{
			Debug.Log(Application.version);
		}
	}

	// Token: 0x0600043A RID: 1082 RVA: 0x0002B3C8 File Offset: 0x000295C8
	private void FontSet()
	{
		string text = "";
		for (int i = 0; i < MainManager.fonts.Length; i++)
		{
			text = string.Concat(new object[]
			{
				text,
				"|font,",
				i,
				"|"
			});
			for (int j = 0; j < MainManager.letterPromptHelp.Length; j++)
			{
				text += Resources.Load<TextAsset>("Data/LetterPrompt" + j).ToString();
			}
		}
		base.StartCoroutine(MainManager.SetText("|single|" + text, Vector3.back, MainManager.MainCamera.transform));
	}

	// Token: 0x0600043B RID: 1083 RVA: 0x0002B46F File Offset: 0x0002966F
	private static IEnumerator LoadEverything()
	{
		StartMenu s = Object.FindObjectOfType<StartMenu>();
		s.enabled = false;
		Cursor.visible = false;
		MainManager.MainCamera.depthTextureMode = DepthTextureMode.Depth;
		RenderSettings.ambientLight = Color.gray;
		MainManager.basicload = false;
		yield return null;
		MainManager.events = MainManager.instance.gameObject.AddComponent<EventControl>();
		yield return null;
		MainManager.chaptername = MainManager.instance.StartCoroutine(MainManager.instance.LoadEssentials());
		while (MainManager.chaptername != null)
		{
			yield return null;
		}
		yield return null;
		if (MainManager.languageid > -1)
		{
			MainManager.instance.SetVariables();
		}
		else
		{
			InputIO.LoadSettings(false);
		}
		yield return null;
		MainManager.SetRenderTexture(0);
		yield return null;
		MainManager.instance.FontSet();
		yield return null;
		MainManager.basicload = true;
		s.enabled = true;
		yield break;
	}

	// Token: 0x0600043C RID: 1084 RVA: 0x0002B478 File Offset: 0x00029678
	public static void LoadLangSpecific()
	{
		if (MainManager.languageid == 4)
		{
			MainManager.guisprites[61] = MainManager.guisprites[224];
			MainManager.guisprites[109] = MainManager.guisprites[225];
			MainManager.guisprites[75] = MainManager.guisprites[226];
		}
	}

	// Token: 0x0600043D RID: 1085 RVA: 0x0002B4C8 File Offset: 0x000296C8
	public static void SetRenderTexture(int downsampleindex)
	{
		MainManager.downsample = downsampleindex;
		if (MainManager.downsample == 0)
		{
			if (MainManager.MainCamera.targetTexture != null)
			{
				MainManager.MainCamera.targetTexture = null;
				MainManager.MainCamera.rect = new Rect(0f, 0f, 1f, 1f);
				MainManager.GUICamera.transform.GetChild(0).gameObject.SetActive(false);
				MainManager.MainCamera.transform.GetChild(1).GetComponent<Camera>().targetTexture = null;
				return;
			}
		}
		else
		{
			MainManager.GUICamera.transform.GetChild(0).gameObject.SetActive(true);
			Material material = MainManager.GUICamera.transform.GetChild(0).GetComponentInChildren<MeshRenderer>().material;
			MainManager.MainCamera.rect = new Rect(0f, 0f, MainManager.downsamples[MainManager.downsample], MainManager.downsamples[MainManager.downsample]);
			RenderTexture renderTexture = new RenderTexture((int)(1920f * MainManager.downsamples[MainManager.downsample]), (int)(1080f * MainManager.downsamples[MainManager.downsample]), 24);
			MainManager.MainCamera.transform.GetChild(1).GetComponent<Camera>().targetTexture = renderTexture;
			renderTexture.filterMode = FilterMode.Bilinear;
			MainManager.MainCamera.targetTexture = renderTexture;
			material.mainTexture = renderTexture;
			MainManager.RefreshRenderTex();
		}
	}

	// Token: 0x0600043E RID: 1086 RVA: 0x0002B624 File Offset: 0x00029824
	public static void RefreshRenderTex()
	{
		MainManager.GUICamera.transform.GetChild(0).GetComponent<Renderer>().material.mainTextureScale = Vector2.one * (MainManager.MainCamera.GetComponent<FXAA>().enabled ? 1f : MainManager.downsamples[MainManager.downsample]);
	}

	// Token: 0x0600043F RID: 1087 RVA: 0x0002B680 File Offset: 0x00029880
	public static bool AllOnGround(EntityControl[] entities)
	{
		for (int i = 0; i < entities.Length; i++)
		{
			if (!entities[i].onground)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000440 RID: 1088 RVA: 0x0002B6A8 File Offset: 0x000298A8
	public static string GetDialogueFromMap(MainManager.Maps mapid, int lineid, int boxid)
	{
		string text = Resources.Load<TextAsset>(string.Concat(new object[]
		{
			"Data/Dialogues",
			MainManager.languageid,
			"/Maps/",
			mapid
		})).ToString().Replace("\r\n", "\n").Split(new char[]
		{
			'\n'
		})[lineid];
		if (boxid == -1)
		{
			return text;
		}
		return text.Split(new string[]
		{
			"|next|"
		}, StringSplitOptions.None)[boxid];
	}

	// Token: 0x06000441 RID: 1089 RVA: 0x0002B730 File Offset: 0x00029930
	public static MainManager.Entity_Data[] LoadEntityData()
	{
		string[] array = Resources.Load<TextAsset>("Data/EntityValues").ToString().Split(new char[]
		{
			'\n'
		});
		MainManager.Entity_Data[] array2 = new MainManager.Entity_Data[Enum.GetNames(typeof(MainManager.AnimIDs)).Length];
		for (int i = 0; i < array2.Length; i++)
		{
			if (i < array.Length)
			{
				string[] array3 = array[i].Split(new char[]
				{
					','
				});
				array2[i].shadowsize = Convert.ToSingle(array3[0]);
				array2[i].startscale = new Vector3(Convert.ToSingle(array3[1].Replace("(", "")), Convert.ToSingle(array3[2]), Convert.ToSingle(array3[3].Replace(")", "")));
				array2[i].bleeppitch = Convert.ToSingle(array3[4]);
				array2[i].bleepid = Convert.ToInt32(array3[5]);
				array2[i].ismodel = Convert.ToBoolean(array3[6]);
				array2[i].modelscale = new Vector3(Convert.ToSingle(array3[7].Replace("(", "")), Convert.ToSingle(array3[8]), Convert.ToSingle(array3[9].Replace(")", "")));
				array2[i].modeloffset = new Vector3(Convert.ToSingle(array3[10].Replace("(", "")), Convert.ToSingle(array3[11]), Convert.ToSingle(array3[12].Replace(")", "")));
				array2[i].freezesize = new Vector3(Convert.ToSingle(array3[13].Replace("(", "")), Convert.ToSingle(array3[14]), Convert.ToSingle(array3[15].Replace(")", "")));
				array2[i].freezeoffset = new Vector3(Convert.ToSingle(array3[16].Replace("(", "")), Convert.ToSingle(array3[17]), Convert.ToSingle(array3[18].Replace(")", "")));
				array2[i].freezeflipoffset = new Vector3(Convert.ToSingle(array3[19].Replace("(", "")), Convert.ToSingle(array3[20]), Convert.ToSingle(array3[21].Replace(")", "")));
				if (array3[22].Length > 0)
				{
					string[] array4 = array3[22].Split(new char[]
					{
						'?'
					});
					array2[i].preloaddata = new string[array4.Length];
					for (int j = 0; j < array4.Length; j++)
					{
						array2[i].preloaddata[j] = array4[j];
					}
				}
				array2[i].shakeondrop = Convert.ToBoolean(array3[23]);
				array2[i].diganim = Convert.ToBoolean(array3[24]);
				array2[i].dontoverridejump = Convert.ToBoolean(array3[25]);
				array2[i].freezenofall = Convert.ToBoolean(array3[26]);
				array2[i].noshadow = Convert.ToBoolean(array3[27]);
				array2[i].walktype = (EntityControl.WalkType)Convert.ToInt32(array3[28]);
				array2[i].basestate = Convert.ToInt32(array3[29]);
				array2[i].basewalk = Convert.ToInt32(array3[30]);
				array2[i].minheight = Convert.ToSingle(array3[31]);
				array2[i].startheight = Convert.ToSingle(array3[32]);
				array2[i].startbobspd = Convert.ToSingle(array3[33]);
				array2[i].startbobfreq = Convert.ToSingle(array3[34]);
				array2[i].hasiceanim = Convert.ToBoolean(array3[35]);
				if (array3.Length > 36)
				{
					array2[i].noflyanim = Convert.ToBoolean(array3[36]);
					array2[i].forceshadow = Convert.ToBoolean(array3[37]);
					array2[i].Object = Convert.ToBoolean(array3[38]);
				}
			}
			else
			{
				array2[i].bleeppitch = 1f;
				array2[i].startscale = Vector3.one;
				array2[i].shadowsize = 1f;
			}
		}
		return array2;
	}

	// Token: 0x06000442 RID: 1090 RVA: 0x0002BB9A File Offset: 0x00029D9A
	public static void EndMiniGame(bool antialias, int score)
	{
		MainManager.ResetCamera(true);
		MainManager.SetRenderTexture(0);
		MainManager.MainCamera.GetComponent<FXAA>().enabled = antialias;
		MainManager.GetRewardTokens(score);
		MainManager.instance.flagvar[0] = score;
	}

	// Token: 0x06000443 RID: 1091 RVA: 0x0002BBCB File Offset: 0x00029DCB
	private IEnumerator LoadEssentials()
	{
		MainManager.letterpool = new TextMesh[500];
		for (int i = 0; i < MainManager.letterpool.Length; i++)
		{
			MainManager.letterpool[i] = MainManager.NewLetter(i.ToString());
		}
		MainManager.screenshake = Vector3.zero;
		yield return null;
		MainManager.textboxsprites = Resources.LoadAll<Sprite>("Sprites/GUI/textbox");
		yield return null;
		MainManager.partfab = Resources.LoadAll<GameObject>("Prefabs/Particles");
		yield return null;
		MainManager.parttex = Resources.LoadAll<Texture>("Sprites/Particles");
		yield return null;
		MainManager.spritepart = Resources.LoadAll<Sprite>("Sprites/Particles");
		yield return null;
		MainManager.asounds = Resources.LoadAll<AudioClip>("Audio/Sounds");
		yield return null;
		if (InputIO.IsConsole)
		{
			string[] names = Enum.GetNames(typeof(MainManager.Musics));
			MainManager.msounds = new AudioClip[names.Length];
			for (int j = 0; j < names.Length; j++)
			{
				MainManager.msounds[j] = Resources.Load<AudioClip>("Audio/Music/" + names[j]);
			}
		}
		MainManager.dsounds = Resources.LoadAll<AudioClip>("Audio/Sounds/Dialogue");
		yield return null;
		MonoBehaviour.print(string.Concat(new object[]
		{
			"preloaded ",
			MainManager.partfab.Length,
			" particle prefabs + ",
			MainManager.parttex.Length + MainManager.spritepart.Length,
			" textures AND ",
			MainManager.asounds.Length + MainManager.dsounds.Length,
			" sounds"
		}));
		MainManager.GUICamera = MainManager.MainCamera.transform.GetChild(0).GetComponent<Camera>();
		MainManager.defaultpmat = Resources.Load<PhysicMaterial>("Materials/DefaultPhysis");
		MainManager.spritemat = Resources.Load<Material>("Materials/SpriteMat");
		MainManager.holosprite = Resources.Load<Material>("Materials/SpriteHologram");
		MainManager.emptymat = Resources.Load<Material>("Materials/Empty");
		MainManager.spritematlit = Resources.Load<Material>("Materials/SpriteLit");
		MainManager.outlinemain = Resources.Load<Material>("Materials/OutlineMain");
		MainManager.spritedefaultunity = Resources.Load<Material>("Materials/SpriteDefault");
		MainManager.windShader = Resources.Load<Material>("Materials/WindShader");
		MainManager.Main3D = Resources.Load<Material>("Materials/3DMain");
		MainManager.Fade3D = Resources.Load<Material>("Materials/3DFade");
		MainManager.grayscale = Resources.Load<Material>("Materials/Grayscale");
		MainManager.letters = Resources.Load<TextAsset>("Data/Letters").ToString().ToCharArray();
		MainManager.grasssprite = Resources.LoadAll<Sprite>("Sprites/Objects/grass");
		yield return null;
		MainManager.leafsprites = Resources.LoadAll<Sprite>("Sprites/GUI/battleleaves");
		yield return null;
		MainManager.fakelight = Resources.Load<Material>("Materials/FakeLight").shader;
		MainManager.shadowsprite = Resources.Load<Sprite>("Sprites/Misc/shadow");
		MainManager.mainPlane = Resources.Load<Material>("Materials/MainPlane");
		MainManager.fadePlane = Resources.Load<Material>("Materials/FadePlane");
		MainManager.languagehelp = Resources.Load<TextAsset>("Data/LanguageHelp").ToString().Split(new char[]
		{
			'\n'
		});
		MainManager.endata = MainManager.LoadEntityData();
		yield return null;
		MainManager.hitpart = (Object.Instantiate(Resources.Load("Prefabs/Particles/HitPart"), new Vector3(0f, -999f), Quaternion.identity) as GameObject).GetComponent<ParticleSystem>();
		MainManager.deathpart = (Object.Instantiate(Resources.Load("Prefabs/Particles/deathsmoke"), new Vector3(0f, -999f), Quaternion.Euler(-90f, 0f, 0f)) as GameObject).GetComponent<ParticleSystem>();
		this.globalcamdir = new GameObject("CamDir").transform;
		this.globalcamdir.transform.parent = MainManager.MainCamera.transform;
		this.globalcamdir.transform.localPosition = Vector3.zero;
		MainManager.librarysprites = Resources.LoadAll<Sprite>("Sprites/Items/EnemyPortraits");
		yield return null;
		this.extrafollowers = new List<int>();
		MainManager.sounds = new AudioSource[15];
		MainManager.music = new AudioSource[1];
		MainManager.musicids = new int[MainManager.music.Length];
		string[] array = Resources.Load<TextAsset>("Data/LeafPos").ToString().Split(new char[]
		{
			'\n'
		});
		string[] array2 = new string[1];
		MainManager.leafpos = new Vector2[array.Length];
		for (int k = 0; k < array.Length; k++)
		{
			array2 = array[k].Split(new char[]
			{
				','
			});
			MainManager.leafpos[k] = new Vector2(Convert.ToSingle(array2[0]), Convert.ToSingle(array2[1]));
		}
		List<int[]> list = new List<int[]>();
		array = Resources.Load<TextAsset>("Data/QuestChecks").ToString().Split(new char[]
		{
			'\n'
		});
		for (int l = 0; l < array.Length; l++)
		{
			array2 = array[l].Split(new char[]
			{
				'@'
			});
			List<int> list2 = new List<int>();
			for (int m = 0; m < array2.Length; m++)
			{
				list2.Add(Convert.ToInt32(array2[m]));
			}
			list.Add(list2.ToArray());
		}
		MainManager.questchecks = list.ToArray();
		yield return null;
		array = Resources.Load<TextAsset>("Data/Termacade").ToString().Split(new char[]
		{
			'\n'
		});
		MainManager.termacadeprize = new int[array.Length, array[0].Split(new char[]
		{
			','
		}).Length];
		for (int n = 0; n < array.Length; n++)
		{
			array2 = array[n].Split(new char[]
			{
				','
			});
			for (int num = 0; num < array2.Length; num++)
			{
				MainManager.termacadeprize[n, num] = Convert.ToInt32(array2[num]);
			}
		}
		array = Resources.Load<TextAsset>("Data/EnemyData").ToString().Split(new char[]
		{
			'\n'
		});
		MainManager.enemydata = new string[array.Length, array[0].Split(new char[]
		{
			','
		}).Length];
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			array2 = array[num2].Split(new char[]
			{
				','
			});
			for (int num3 = 0; num3 < array2.Length; num3++)
			{
				MainManager.enemydata[num2, num3] = array2[num3];
			}
		}
		yield return null;
		MainManager.fonts = new Font[Enum.GetNames(typeof(MainManager.Fonts)).Length];
		MainManager.fontmat = new Material[MainManager.fonts.Length];
		for (int num4 = 0; num4 < MainManager.fonts.Length; num4++)
		{
			if (num4 != 2)
			{
				MainManager.fonts[num4] = Resources.Load<Font>("Fonts/" + (MainManager.Fonts)num4);
				MainManager.fontmat[num4] = Resources.Load<Material>("Fonts/" + (MainManager.Fonts)num4);
			}
		}
		MainManager.bleeps = base.gameObject.AddComponent<AudioSource>();
		for (int num5 = 0; num5 < MainManager.sounds.Length; num5++)
		{
			MainManager.sounds[num5] = base.gameObject.AddComponent<AudioSource>();
			MainManager.sounds[num5].velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
			MainManager.sounds[num5].volume = MainManager.soundvolume;
		}
		yield return null;
		for (int num6 = 0; num6 < MainManager.music.Length; num6++)
		{
			MainManager.music[num6] = base.gameObject.AddComponent<AudioSource>();
			if (num6 > 0)
			{
				MainManager.music[num6].volume = 0f;
			}
			MainManager.music[num6].loop = true;
		}
		yield return null;
		List<Sprite> list3 = new List<Sprite>();
		list3.AddRange(Resources.LoadAll<Sprite>("Sprites/GUI/gui"));
		list3.AddRange(Resources.LoadAll<Sprite>("Sprites/GUI/gui2"));
		MainManager.guisprites = list3.ToArray();
		MainManager.cursorsprite = new Sprite[]
		{
			MainManager.guisprites[145]
		};
		if (!MainManager.basicload && Application.version.Contains("Beta"))
		{
			MainManager.NewUIObject("betawatermark", MainManager.GUICamera.transform, new Vector3(7.8f, -4.4f, 1f), Vector3.one, MainManager.guisprites[169], 9999).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		}
		yield return null;
		MainManager.letterbox = new SpriteRenderer[2];
		for (int num7 = 0; num7 < 2; num7++)
		{
			MainManager.letterbox[num7] = MainManager.NewSolidColor("letterbox" + num7, Color.black, 0.2f, default(Vector3), Vector2.one / 2f);
			MainManager.letterbox[num7].color = Color.clear;
			MainManager.letterbox[num7].gameObject.layer = 5;
			MainManager.letterbox[num7].transform.parent = MainManager.GUICamera.transform;
			MainManager.letterbox[num7].transform.localEulerAngles = Vector3.zero;
			MainManager.letterbox[num7].transform.localScale = new Vector3(5f, 0.5f, 1f);
			MainManager.letterbox[num7].sortingOrder = -40;
		}
		MainManager.letterbox[0].transform.localPosition = new Vector3(0f, 5.75f, 10f);
		MainManager.letterbox[1].transform.localPosition = new Vector3(0f, -5.75f, 10f);
		yield return null;
		array = Resources.Load<TextAsset>("Data/RecipeData").ToString().Split(new char[]
		{
			'\n'
		});
		MainManager.recipedata = new int[array.Length - 1, 3];
		for (int num8 = 0; num8 < array.Length - 1; num8++)
		{
			array2 = array[num8].Split(new char[]
			{
				','
			});
			for (int num9 = 0; num9 < array2.Length; num9++)
			{
				MainManager.recipedata[num8, num9] = Convert.ToInt32(array2[num9]);
			}
			if (MainManager.recipedata[num8, 0] > MainManager.recipedata[num8, 1] && MainManager.recipedata[num8, 1] > -1)
			{
				int num10 = MainManager.recipedata[num8, 1];
				MainManager.recipedata[num8, 1] = MainManager.recipedata[num8, 0];
				MainManager.recipedata[num8, 0] = num10;
			}
		}
		yield return null;
		MainManager.instance.projectilepsrites = Resources.LoadAll<Sprite>("Sprites/Misc/projectiles");
		yield return null;
		Physics.gravity = new Vector3(0f, -40f, 0f);
		InputIO.GetJoyButtons();
		yield return null;
		InputIO.SetDefaultKeys();
		yield return null;
		MainManager.chaptername = null;
		yield break;
	}

	// Token: 0x06000444 RID: 1092 RVA: 0x0002BBDC File Offset: 0x00029DDC
	private static void LoadItemSprites()
	{
		MainManager.itemsprites = new Sprite[2, 256];
		Sprite[] array = Resources.LoadAll<Sprite>("Sprites/Items/items0");
		for (int i = 0; i < MainManager.itemsprites.GetLength(0); i++)
		{
			int num = (i == 0) ? Enum.GetNames(typeof(MainManager.Items)).Length : Enum.GetNames(typeof(MainManager.BadgeTypes)).Length;
			for (int j = 0; j < num; j++)
			{
				if (i == 1)
				{
					int num2 = Convert.ToInt32(MainManager.badgedata[j, 8]);
					if (num2 > -1)
					{
						MainManager.itemsprites[i, j] = MainManager.GetMoreItem(num2, 0);
					}
					else
					{
						MainManager.itemsprites[i, j] = array[176 + j];
					}
				}
				else
				{
					MainManager.itemsprites[i, j] = ((j >= 176) ? MainManager.GetMoreItem(j, 176) : array[j]);
				}
			}
		}
	}

	// Token: 0x06000445 RID: 1093 RVA: 0x0002BCC1 File Offset: 0x00029EC1
	private static Sprite GetMoreItem(int i, int offset = 176)
	{
		return Resources.LoadAll<Sprite>("Sprites/Items/items1")[Mathf.Abs(offset - i)];
	}

	// Token: 0x06000446 RID: 1094 RVA: 0x0002BCD6 File Offset: 0x00029ED6
	private static TextMesh NewLetter()
	{
		return MainManager.NewLetter("");
	}

	// Token: 0x06000447 RID: 1095 RVA: 0x0002BCE2 File Offset: 0x00029EE2
	public static MainManager.Maps CurrentMap()
	{
		return MainManager.map.mapid;
	}

	// Token: 0x06000448 RID: 1096 RVA: 0x0002BCF0 File Offset: 0x00029EF0
	private static TextMesh NewLetter(string id)
	{
		TextMesh textMesh = new GameObject("letter" + id).AddComponent<TextMesh>();
		textMesh.transform.parent = MainManager.instance.transform;
		textMesh.transform.localPosition = new Vector3(0f, 0f, 10f);
		textMesh.gameObject.layer = 5;
		textMesh.richText = false;
		textMesh.fontStyle = FontStyle.Normal;
		textMesh.anchor = TextAnchor.LowerLeft;
		MeshRenderer component = textMesh.GetComponent<MeshRenderer>();
		component.shadowCastingMode = ShadowCastingMode.Off;
		component.allowOcclusionWhenDynamic = false;
		return textMesh;
	}

	// Token: 0x06000449 RID: 1097 RVA: 0x0002BD7A File Offset: 0x00029F7A
	public static IEnumerator LateSound(string sound, float delay)
	{
		yield return new WaitForSeconds(delay);
		MainManager.PlaySound(sound);
		yield break;
	}

	// Token: 0x0600044A RID: 1098 RVA: 0x0002BD90 File Offset: 0x00029F90
	public static bool IsPaused()
	{
		return MainManager.instance.minipause || MainManager.instance.pause || MainManager.instance.inevent || (MainManager.battle != null && MainManager.battle.inevent);
	}

	// Token: 0x0600044B RID: 1099 RVA: 0x0002BDDC File Offset: 0x00029FDC
	public static SpriteRenderer Dimmer(int fadetype)
	{
		return MainManager.Dimmer(fadetype, 0.1f, Color.black);
	}

	// Token: 0x0600044C RID: 1100 RVA: 0x0002BDEE File Offset: 0x00029FEE
	public static SpriteRenderer Dimmer(int fadetype, float fadespeed)
	{
		return MainManager.Dimmer(fadetype, fadespeed, Color.black);
	}

	// Token: 0x0600044D RID: 1101 RVA: 0x0002BDFC File Offset: 0x00029FFC
	public static SpriteRenderer Dimmer(int fadetype, float fadespeed, Color color)
	{
		MainManager.PlayTransition(fadetype, 0, fadespeed, color);
		return MainManager.instance.transitionobj[0].GetComponent<SpriteRenderer>();
	}

	// Token: 0x0600044E RID: 1102 RVA: 0x0002BE18 File Offset: 0x0002A018
	public static void SetCamera(Transform target, Vector3? targetpos, float speed)
	{
		MainManager.SetCamera(target, targetpos, speed, MainManager.defaultcamoffset);
	}

	// Token: 0x0600044F RID: 1103 RVA: 0x0002BE27 File Offset: 0x0002A027
	public static void SetCamera(Vector3 targetpos, float speed)
	{
		MainManager.SetCamera(null, new Vector3?(targetpos), speed, MainManager.defaultcamoffset);
	}

	// Token: 0x06000450 RID: 1104 RVA: 0x0002BE3B File Offset: 0x0002A03B
	public static void SetCamera(Vector3 targetpos, Vector3 angle, Vector3 offset, float speed)
	{
		MainManager.SetCamera(null, new Vector3?(targetpos), speed, offset, angle);
	}

	// Token: 0x06000451 RID: 1105 RVA: 0x0002BE4C File Offset: 0x0002A04C
	public static void SetCamera(Vector3 targetpos, Vector3 angle, float speed)
	{
		MainManager.SetCamera(null, new Vector3?(targetpos), speed, MainManager.defaultcamoffset, angle);
	}

	// Token: 0x06000452 RID: 1106 RVA: 0x0002BE61 File Offset: 0x0002A061
	public static void SetCamera(Transform target, Vector3? targetpos, float speed, Vector3 offset)
	{
		MainManager.SetCamera(target, targetpos, speed, offset, (MainManager.battle != null) ? new Vector3(5f, 0f) : MainManager.defaultcamangle);
	}

	// Token: 0x06000453 RID: 1107 RVA: 0x0002BE90 File Offset: 0x0002A090
	public static void SetCamera(Transform target, Vector3? targetpos, float speed, Vector3 offset, Vector3 angle)
	{
		MainManager.instance.camtarget = target;
		MainManager.instance.camtargetpos = targetpos;
		MainManager.instance.camoffset = offset;
		MainManager.instance.camangleoffset = angle;
		MainManager.instance.camspeed = speed;
		if (speed >= 1f)
		{
			if (target != null)
			{
				MainManager.MainCamera.transform.parent.position = target.transform.position;
			}
			else if (targetpos != null)
			{
				MainManager.MainCamera.transform.parent.position = targetpos.Value;
			}
			MainManager.MainCamera.transform.parent.localEulerAngles = angle;
			MainManager.MainCamera.transform.localPosition = offset;
		}
	}

	// Token: 0x06000454 RID: 1108 RVA: 0x0002BF54 File Offset: 0x0002A154
	public void SetVariables()
	{
		MainManager.battlemessage = Resources.LoadAll<Sprite>("Sprites/GUI/BattleMessage/battlem" + MainManager.languageid);
		if (MainManager.battlemessage == null || MainManager.battlemessage.Length == 0)
		{
			MainManager.battlemessage = Resources.LoadAll<Sprite>("Sprites/GUI/BattleMessage/battlem0");
		}
		MainManager.menutext = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/MenuText").ToString().Replace("\r\n", "\n").Split(new char[]
		{
			'\n'
		});
		MainManager.commondialogue = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/CommonDialogue").ToString().Split(new char[]
		{
			'\n'
		});
		MainManager.musicnames = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/MusicList").ToString().Split(new char[]
		{
			'\n'
		});
		MainManager.commandhelptext = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/ActionCommands").ToString().Split(new char[]
		{
			'\n'
		});
		MainManager.areanames = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/AreaNames").ToString().Split(new char[]
		{
			'\n'
		});
		this.samiramusics = new List<int[]>();
		this.prizeflags = new int[]
		{
			13,
			17,
			18,
			19,
			20,
			21,
			25,
			33,
			34,
			36,
			44,
			45,
			46,
			48,
			51,
			52,
			57,
			61,
			63,
			30,
			31,
			64,
			65
		};
		this.prizeids = new int[]
		{
			5,
			24,
			14,
			15,
			1,
			32,
			10,
			73,
			37,
			26,
			66,
			67,
			65,
			60,
			55,
			61,
			0,
			63,
			68,
			79,
			58,
			70,
			68
		};
		this.prizeenemyids = new int[]
		{
			2,
			0,
			24,
			3,
			15,
			31,
			46,
			49,
			54,
			69,
			76,
			42,
			41,
			51,
			34,
			0,
			-1,
			72,
			36,
			97,
			96,
			40,
			90
		};
		this.badgeshops = new List<int>[2];
		this.avaliablebadgepool = new List<int>[this.badgeshops.Length];
		for (int i = 0; i < this.badgeshops.Length; i++)
		{
			this.badgeshops[i] = new List<int>();
			this.avaliablebadgepool[i] = new List<int>();
		}
		this.items = new List<int>[3];
		for (int j = 0; j < this.items.Length; j++)
		{
			this.items[j] = new List<int>();
		}
		MainManager.itemdata = new string[1, 256, 7];
		string[] array = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/Items").ToString().Replace("\r\n", "\n").Split(new char[]
		{
			'\n'
		});
		string[] array2 = Resources.Load<TextAsset>("Data/ItemData").ToString().Split(new char[]
		{
			'\n'
		});
		for (int k = 0; k < 1; k++)
		{
			for (int l = 0; l < array.Length; l++)
			{
				string[] array3 = array[l].Split(new char[]
				{
					'@'
				});
				if (array3.Length > 1)
				{
					MainManager.itemdata[k, l, 0] = array3[0];
					MainManager.itemdata[k, l, 1] = array3[1];
					MainManager.itemdata[k, l, 2] = array3[2];
					if (array3.Length > 3)
					{
						MainManager.itemdata[k, l, 3] = array3[3];
					}
					if (k == 0)
					{
						array3 = array2[l].Split(new char[]
						{
							'@'
						});
						MainManager.itemdata[k, l, 4] = array3[0];
						MainManager.itemdata[k, l, 5] = array3[1];
						MainManager.itemdata[k, l, 6] = array3[2];
					}
				}
			}
		}
		array = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/Skills").ToString().Split(new char[]
		{
			'\n'
		});
		array2 = Resources.Load<TextAsset>("Data/SkillData").ToString().Split(new char[]
		{
			'\n'
		});
		MainManager.skilldata = new string[array.Length - 1, 13];
		for (int m = 0; m < MainManager.skilldata.GetLength(0); m++)
		{
			string[] array4 = array[m].Split(new char[]
			{
				'@'
			});
			MainManager.skilldata[m, 0] = array4[0];
			MainManager.skilldata[m, 1] = array4[1];
			array4 = array2[m].Split(new char[]
			{
				'@'
			});
			for (int n = 2; n < MainManager.skilldata.GetLength(1); n++)
			{
				MainManager.skilldata[m, n] = array4[n - 2];
			}
		}
		array = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/BadgeName").ToString().Split(new char[]
		{
			'\n'
		});
		array2 = Resources.Load<TextAsset>("Data/BadgeData").ToString().Split(new char[]
		{
			'\n'
		});
		MainManager.badgedata = new string[array.Length - 1, 9];
		for (int num = 0; num < MainManager.badgedata.GetLength(0); num++)
		{
			string[] array5 = array[num].Split(new char[]
			{
				'@'
			});
			MainManager.badgedata[num, 0] = array5[0];
			MainManager.badgedata[num, 1] = array5[1];
			MainManager.badgedata[num, 6] = array5[2];
			array5 = array2[num].Split(new char[]
			{
				'@'
			});
			MainManager.badgedata[num, 2] = array5[0];
			MainManager.badgedata[num, 3] = array5[1];
			MainManager.badgedata[num, 4] = array5[2];
			MainManager.badgedata[num, 5] = array5[3];
			MainManager.badgedata[num, 7] = array5[4];
			MainManager.badgedata[num, 8] = array5[5];
		}
		array = Resources.Load<TextAsset>("Data/BadgeOrder").ToString().Split(new char[]
		{
			'\n'
		});
		MainManager.badgeorder = new int[array.Length];
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			MainManager.badgeorder[num2] = Convert.ToInt32(array[num2]);
		}
		array = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/BoardQuests").ToString().Split(new char[]
		{
			'\n'
		});
		array2 = Resources.Load<TextAsset>("Data/BoardData").ToString().Split(new char[]
		{
			'\n'
		});
		MainManager.boardquestdata = new string[array.Length, array[0].Split(new char[]
		{
			'@'
		}).Length + array2[0].Split(new char[]
		{
			'@'
		}).Length];
		for (int num3 = 0; num3 < array.Length; num3++)
		{
			string[] array6 = array[num3].Split(new char[]
			{
				'@'
			});
			int num4 = 0;
			for (int num5 = 0; num5 < array6.Length; num5++)
			{
				MainManager.boardquestdata[num3, num5] = array6[num5];
				num4++;
			}
			array6 = array2[num3].Split(new char[]
			{
				'@'
			});
			for (int num6 = 0; num6 < array6.Length; num6++)
			{
				MainManager.boardquestdata[num3, num6 + num4] = array6[num6];
			}
		}
		MainManager.librarydata = new string[5, 256, 10];
		MainManager.libraryorder = new int[MainManager.librarydata.GetLength(0), MainManager.librarydata.GetLength(1)];
		for (int num7 = 0; num7 < MainManager.librarydata.GetLength(0); num7++)
		{
			string[] array7 = new string[]
			{
				""
			};
			switch (num7)
			{
			case 0:
			{
				array7 = Resources.Load<TextAsset>("Data/DiscoveryOrder").ToString().Split(new char[]
				{
					'\n'
				});
				array = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/Discoveries").ToString().Split(new char[]
				{
					'\n'
				});
				List<string> list = new List<string>();
				MainManager.discoveryicons = new int[MainManager.librarylimit[0]];
				for (int num8 = 0; num8 < MainManager.librarylimit[0]; num8++)
				{
					string[] array8 = array7[num8].Split(new char[]
					{
						','
					});
					MainManager.discoveryicons[num8] = Convert.ToInt32(array8[1]);
					list.Add(array8[0]);
				}
				array7 = list.ToArray();
				break;
			}
			case 1:
				array7 = Resources.Load<TextAsset>("Data/TattleList").ToString().Split(new char[]
				{
					'\n'
				});
				array = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/EnemyTattle").ToString().Split(new char[]
				{
					'\n'
				});
				break;
			case 2:
				array7 = Resources.Load<TextAsset>("Data/CookOrder").ToString().Split(new char[]
				{
					'\n'
				});
				array = Resources.Load<TextAsset>("Data/CookLibrary").ToString().Split(new char[]
				{
					'\n'
				});
				break;
			case 3:
			{
				array7 = Resources.Load<TextAsset>("Data/SynopsisOrder").ToString().Split(new char[]
				{
					'\n'
				});
				array = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/Synopsis").ToString().Split(new char[]
				{
					'\n'
				});
				List<string> list2 = new List<string>();
				MainManager.achiveicons = new int[MainManager.librarylimit[3]];
				for (int num9 = 0; num9 < MainManager.librarylimit[3]; num9++)
				{
					string[] array9 = array7[num9].Split(new char[]
					{
						','
					});
					MainManager.achiveicons[num9] = Convert.ToInt32(array9[1]);
					list2.Add(array9[0]);
				}
				array7 = list2.ToArray();
				break;
			}
			case 4:
				array7 = new string[0];
				break;
			}
			for (int num10 = 0; num10 < array.Length; num10++)
			{
				array2 = array[num10].Split(new char[]
				{
					'@'
				});
				for (int num11 = 0; num11 < array2.Length; num11++)
				{
					MainManager.librarydata[num7, num10, num11] = array2[num11];
				}
			}
			for (int num12 = 0; num12 < array7.Length; num12++)
			{
				MainManager.libraryorder[num7, num12] = Convert.ToInt32(array7[num12]);
			}
		}
		this.boardquests = new List<int>[3];
		for (int num13 = 0; num13 < this.boardquests.Length; num13++)
		{
			this.boardquests[num13] = new List<int>();
			this.boardquests[num13].Add(0);
		}
		array = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/EnemyTattle").ToString().Split(new char[]
		{
			'\n'
		});
		MainManager.enemynames = new string[array.Length];
		for (int num14 = 0; num14 < array.Length; num14++)
		{
			MainManager.enemynames[num14] = array[num14].Split(new char[]
			{
				'@'
			})[0];
		}
		this.badges = new List<int[]>();
		this.SetUpBadges();
		this.statbonus = new List<int[]>();
		MainManager.ChangeParty(new int[]
		{
			0,
			1
		}, true);
		this.enemyencounter = new int[256, 2];
		this.insideid = -1;
		this.tp = 10;
		this.maxtp = 10;
		this.basetp = 10;
		this.bp = 5;
		this.maxbp = this.bp;
		this.partylevel = 1;
		this.maxitems = 10;
		this.maxstorage = 35;
		this.camspeed = 0.1f;
		this.camanglespeed = 0.1f;
		this.flagvar = new int[70];
		this.flagvar[28] = 9500;
		this.flagvar[29] = 4500;
		this.flags = new bool[750];
		this.crystalbflags = new bool[50];
		this.vectorflags = new Vector3[10];
		this.regionalflags = new bool[100];
		this.flagstring = new string[15];
		this.camoffset = MainManager.defaultcamoffset;
		this.camoffset2 = Vector3.zero;
		this.camangleoffset = MainManager.defaultcamangle;
		this.librarystuff = new bool[MainManager.librarydata.GetLength(0), MainManager.librarydata.GetLength(1)];
		MainManager.musicvolume = 1f;
		MainManager.soundvolume = 1f;
		MainManager.halt = false;
		MainManager.instance.switchicon = MainManager.NewUIObject("SwitchIcon", MainManager.GUICamera.transform, new Vector3(-8f, 0f, 10f)).AddComponent<SpriteRenderer>();
		MainManager.instance.switchicon.transform.localScale = Vector3.one * 0.75f;
		this.promptpick = -1;
		MainManager.LoadItemSprites();
		InputIO.LoadSettings(false);
	}

	// Token: 0x06000455 RID: 1109 RVA: 0x0002CC65 File Offset: 0x0002AE65
	public static bool IsWithin(int value, int min, int max)
	{
		return value >= min && value <= max;
	}

	// Token: 0x06000456 RID: 1110 RVA: 0x0002CC74 File Offset: 0x0002AE74
	public static void ChangeParty(int[] ids, bool fromscratch)
	{
		MainManager.ChangeParty(ids, fromscratch, true);
	}

	// Token: 0x06000457 RID: 1111 RVA: 0x0002CC7E File Offset: 0x0002AE7E
	public static void AddStatBonus(MainManager.StatBonus type, int ammount, int to)
	{
		MainManager.instance.statbonus.Add(new int[]
		{
			(int)type,
			ammount,
			to
		});
	}

	// Token: 0x06000458 RID: 1112 RVA: 0x0002CCA1 File Offset: 0x0002AEA1
	public static void PushAway(Transform obj, Vector3 otherobj)
	{
		MainManager.PushAway(obj, otherobj, 0.05f);
	}

	// Token: 0x06000459 RID: 1113 RVA: 0x0002CCB0 File Offset: 0x0002AEB0
	public static void PushAway(Transform obj, Vector3 otherobj, float value)
	{
		obj.transform.position += (obj.transform.position - otherobj).normalized * value;
	}

	// Token: 0x0600045A RID: 1114 RVA: 0x0002CCF4 File Offset: 0x0002AEF4
	private static void ResetStats()
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].trueid == 1)
			{
				MainManager.instance.playerdata[i].basehp = 9;
			}
			else
			{
				MainManager.instance.playerdata[i].basehp = 7;
			}
			MainManager.instance.playerdata[i].baseatk = 2;
			MainManager.instance.playerdata[i].basedef = 0;
		}
		MainManager.instance.basetp = 10;
	}

	// Token: 0x0600045B RID: 1115 RVA: 0x0002CD9C File Offset: 0x0002AF9C
	public static void ApplyStatBonus()
	{
		MainManager.ResetStats();
		if (MainManager.instance.statbonus.Count > 0)
		{
			int[][] array = MainManager.instance.statbonus.ToArray();
			int count = MainManager.instance.statbonus.Count;
			for (int i = 0; i < count; i++)
			{
				if (array[i][2] == -1)
				{
					switch (array[i][0])
					{
					case 0:
						for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
						{
							MainManager.BattleData[] array2 = MainManager.instance.playerdata;
							int num = j;
							array2[num].basehp = array2[num].basehp + array[i][1];
						}
						break;
					case 1:
						for (int k = 0; k < MainManager.instance.playerdata.Length; k++)
						{
							MainManager.BattleData[] array3 = MainManager.instance.playerdata;
							int num2 = k;
							array3[num2].baseatk = array3[num2].baseatk + array[i][1];
						}
						break;
					case 2:
						for (int l = 0; l < MainManager.instance.playerdata.Length; l++)
						{
							MainManager.BattleData[] array4 = MainManager.instance.playerdata;
							int num3 = l;
							array4[num3].basedef = array4[num3].basedef + array[i][1];
						}
						break;
					case 3:
						MainManager.instance.basetp += array[i][1];
						break;
					}
				}
				else
				{
					for (int m = 0; m < MainManager.instance.playerdata.Length; m++)
					{
						if (MainManager.instance.playerdata[m].trueid == array[i][2])
						{
							switch (array[i][0])
							{
							case 0:
							{
								MainManager.BattleData[] array5 = MainManager.instance.playerdata;
								int num4 = m;
								array5[num4].basehp = array5[num4].basehp + array[i][1];
								break;
							}
							case 1:
							{
								MainManager.BattleData[] array6 = MainManager.instance.playerdata;
								int num5 = m;
								array6[num5].baseatk = array6[num5].baseatk + array[i][1];
								break;
							}
							case 2:
							{
								MainManager.BattleData[] array7 = MainManager.instance.playerdata;
								int num6 = m;
								array7[num6].basedef = array7[num6].basedef + array[i][1];
								break;
							}
							}
						}
					}
				}
			}
			MainManager.ApplyBadges();
		}
	}

	// Token: 0x0600045C RID: 1116 RVA: 0x0002CFB8 File Offset: 0x0002B1B8
	public static bool HasPlayer(int id)
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].trueid == id)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600045D RID: 1117 RVA: 0x0002CFF8 File Offset: 0x0002B1F8
	public static void ChangeParty(int[] ids, bool fromscratch, bool destroyoldentity)
	{
		EntityControl[] array = null;
		if (MainManager.instance.playerdata != null && MainManager.instance.playerdata.Length != 0)
		{
			array = new EntityControl[MainManager.instance.playerdata.Length];
			for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
			{
				array[i] = MainManager.instance.playerdata[i].entity;
			}
		}
		int[,] array2 = null;
		if (!fromscratch)
		{
			array2 = new int[MainManager.instance.playerdata.Length, 8];
			for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
			{
				array2[j, 0] = MainManager.instance.playerdata[j].trueid;
				array2[j, 1] = MainManager.instance.playerdata[j].hp;
				array2[j, 2] = MainManager.instance.playerdata[j].maxhp;
				array2[j, 3] = MainManager.instance.playerdata[j].basehp;
				array2[j, 4] = MainManager.instance.playerdata[j].atk;
				array2[j, 5] = MainManager.instance.playerdata[j].baseatk;
				array2[j, 6] = MainManager.instance.playerdata[j].def;
				array2[j, 7] = MainManager.instance.playerdata[j].basedef;
			}
		}
		else
		{
			MainManager.instance.partyorder = ids;
		}
		List<MainManager.BattleData> list = new List<MainManager.BattleData>();
		if (fromscratch)
		{
			for (int k = 0; k < ids.Length; k++)
			{
				list.Add(MainManager.SetDefaultStats(ids[k]));
			}
		}
		else if (array2 != null)
		{
			for (int l = 0; l < ids.Length; l++)
			{
				for (int m = 0; m < 0; m++)
				{
					MainManager.BattleData battleData = MainManager.SetDefaultStats(ids[l]);
					if (battleData.trueid == array2[m, 0])
					{
						battleData.hp = array2[m, 1];
						battleData.maxhp = array2[m, 2];
						battleData.basehp = array2[m, 3];
						battleData.atk = array2[m, 4];
						battleData.baseatk = array2[m, 5];
						battleData.def = array2[m, 6];
						battleData.basedef = array2[m, 7];
						break;
					}
					list.Add(battleData);
				}
			}
		}
		MainManager.instance.partyorder = ids;
		MainManager.instance.playerdata = list.ToArray();
		if (array != null)
		{
			for (int n = 0; n < array.Length; n++)
			{
				bool flag = false;
				for (int num = 0; num < MainManager.instance.playerdata.Length; num++)
				{
					if (array[n] != null)
					{
						if (MainManager.instance.playerdata[num].animid == array[n].animid)
						{
							MainManager.instance.playerdata[num].entity = array[n];
							if (num == 0)
							{
								MainManager.instance.playerdata[num].entity.tag = "Player";
								MainManager.instance.playerdata[num].entity.gameObject.layer = 10;
							}
							else
							{
								MainManager.instance.playerdata[num].entity.tag = "PFollower";
								MainManager.instance.playerdata[num].entity.gameObject.layer = 9;
							}
							break;
						}
						if (!flag)
						{
							if (destroyoldentity)
							{
								Object.Destroy(array[n].gameObject);
							}
							else if (MainManager.map != null && array[n] != null && MainManager.map != null)
							{
								array[n].transform.parent = MainManager.map.transform;
								array[n].tag = "Untagged";
								EntityControl[] array3 = new EntityControl[MainManager.map.entities.Length + 1];
								for (int num2 = 0; num2 < MainManager.map.entities.Length; num2++)
								{
									array3[num2] = MainManager.map.entities[num2];
								}
								array3[MainManager.map.entities.Length] = array[n];
								MainManager.map.entities = array3;
							}
						}
					}
				}
			}
			if (MainManager.instance.playerdata != null && MainManager.instance.playerdata.Length != 0 && MainManager.instance.playerdata[0].entity != null && MainManager.instance.playerdata[0].entity.GetComponent<PlayerControl>() == null)
			{
				MainManager.instance.playerdata[0].entity.gameObject.AddComponent<PlayerControl>();
				PlayerControl playerControl = MainManager.player;
				Object.Destroy(MainManager.player);
			}
		}
		MainManager.ApplyBadges();
		MainManager.ApplyStatBonus();
		if (fromscratch)
		{
			for (int num3 = 0; num3 < MainManager.instance.playerdata.Length; num3++)
			{
				MainManager.instance.playerdata[num3].hp = MainManager.instance.playerdata[num3].maxhp;
			}
		}
		MainManager.RebuildHUD();
	}

	// Token: 0x0600045E RID: 1118 RVA: 0x0002D58C File Offset: 0x0002B78C
	public static bool PartyLowHP()
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].hp <= 4)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600045F RID: 1119 RVA: 0x0002D5CC File Offset: 0x0002B7CC
	public static void RefreshWind(Renderer t)
	{
		t.material.SetFloat("_ShakeDisplacement", Random.Range(MainManager.map.windspeed / 2f, MainManager.map.windspeed));
		t.material.SetFloat("_ShakeBending", Random.Range(MainManager.map.windintensity / 2f, MainManager.map.windintensity));
		t.material.SetFloat("_ShakeTime", Random.Range(0.075f, 0.25f));
	}

	// Token: 0x06000460 RID: 1120 RVA: 0x0002D658 File Offset: 0x0002B858
	private static MainManager.BattleData SetDefaultStats(int id)
	{
		MainManager.BattleData battleData = new MainManager.BattleData
		{
			animid = id
		};
		if (battleData.animid == 1)
		{
			battleData.hp = 9;
		}
		else
		{
			battleData.hp = 7;
		}
		battleData.trueid = battleData.animid;
		battleData.atk = 2;
		battleData.maxhp = battleData.hp;
		battleData.basehp = battleData.hp;
		battleData.baseatk = battleData.atk;
		battleData.skills = new List<int>();
		battleData.cursoroffset = new Vector3(0f, 2.3f);
		battleData.entityname = MainManager.menutext[46 + battleData.animid];
		battleData.condition = new List<int[]>();
		return battleData;
	}

	// Token: 0x06000461 RID: 1121 RVA: 0x0002D712 File Offset: 0x0002B912
	public static void HurtParticle(Vector3 pos, bool playsound)
	{
		MainManager.HitPart(pos);
		if (playsound)
		{
			MainManager.PlaySound("Hurt");
		}
	}

	// Token: 0x06000462 RID: 1122 RVA: 0x0002D728 File Offset: 0x0002B928
	public static MainManager.BattleData GetPlayerData(int id, bool frombattleentity)
	{
		if (frombattleentity)
		{
			for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
			{
				if (MainManager.instance.playerdata[i].battleentity != null && MainManager.instance.playerdata[i].battleentity.battleid == id)
				{
					return MainManager.instance.playerdata[i];
				}
			}
		}
		return MainManager.GetPlayerData(id);
	}

	// Token: 0x06000463 RID: 1123 RVA: 0x0002D7A0 File Offset: 0x0002B9A0
	public static MainManager.BattleData GetPlayerData(int id)
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].trueid == id)
			{
				return MainManager.instance.playerdata[i];
			}
		}
		return MainManager.instance.playerdata[0];
	}

	// Token: 0x06000464 RID: 1124 RVA: 0x0002D800 File Offset: 0x0002BA00
	public static MainManager.BattleData? GetPlayerDataNullable(int id)
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].trueid == id)
			{
				return new MainManager.BattleData?(MainManager.instance.playerdata[i]);
			}
		}
		return null;
	}

	// Token: 0x06000465 RID: 1125 RVA: 0x0002D85C File Offset: 0x0002BA5C
	public static void AddPrizeMedal(int id)
	{
		if (MainManager.BadgeIsEquipped(11) || MainManager.instance.flags[614])
		{
			MainManager.instance.flagvar[MainManager.instance.prizeflags[id]] = 1;
			MainManager.instance.flags[56] = true;
		}
		else
		{
			MainManager.instance.flagvar[MainManager.instance.prizeflags[id]] = 2;
		}
		MainManager.instance.flagvar[55]++;
	}

	// Token: 0x06000466 RID: 1126 RVA: 0x0002D8DC File Offset: 0x0002BADC
	public static int HasBadge(int badgeid)
	{
		int num = 0;
		for (int i = 0; i < MainManager.instance.badges.Count; i++)
		{
			if (MainManager.instance.badges[i][0] == badgeid)
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x06000467 RID: 1127 RVA: 0x0002D920 File Offset: 0x0002BB20
	public void SetUpBadges()
	{
		this.badgeshops[0].AddRange(new int[]
		{
			0,
			1,
			7,
			12,
			30,
			86,
			84,
			87,
			88,
			81
		});
		this.badgeshops[1].AddRange(new int[]
		{
			19,
			6,
			9,
			43,
			42
		});
		for (int i = 0; i < this.badgeshops.Length; i++)
		{
			int[] array = this.badgeshops[i].ToArray();
			for (int j = 0; j < array.Length; j++)
			{
				this.avaliablebadgepool[i].Add(array[j]);
			}
		}
	}

	// Token: 0x06000468 RID: 1128 RVA: 0x0002D9A9 File Offset: 0x0002BBA9
	public static void AddQuest(int id)
	{
		MainManager.ChangeBoardQuest(id, 0);
	}

	// Token: 0x06000469 RID: 1129 RVA: 0x0002D9B4 File Offset: 0x0002BBB4
	public static void CheckQuests()
	{
		for (int i = 0; i < MainManager.questchecks.Length; i++)
		{
			if (!MainManager.HasQuest(i) && MainManager.questchecks[i][0] != 0)
			{
				bool[] array = new bool[MainManager.questchecks[i].Length];
				for (int j = 0; j < array.Length; j++)
				{
					if (MainManager.questchecks[i][j] > 0)
					{
						array[j] = MainManager.instance.flags[MainManager.questchecks[i][j]];
					}
					else
					{
						array[j] = MainManager.instance.librarystuff[4, Mathf.Abs(MainManager.questchecks[i][j])];
					}
				}
				if (MainManager.CheckAllBool(array, true))
				{
					MainManager.AddQuest(i);
				}
			}
		}
	}

	// Token: 0x0600046A RID: 1130 RVA: 0x0002DA5E File Offset: 0x0002BC5E
	public static IEnumerator TempIgnoreCollision(Collider a, Collider b, float seconds)
	{
		Physics.IgnoreCollision(a, b, true);
		yield return new WaitForSeconds(seconds);
		if (a != null && b != null)
		{
			Physics.IgnoreCollision(a, b, false);
		}
		yield break;
	}

	// Token: 0x0600046B RID: 1131 RVA: 0x0002DA7B File Offset: 0x0002BC7B
	public static void LaunchObject(Transform obj, Vector3 push)
	{
		MainManager.LaunchObject(obj.GetComponent<Rigidbody>(), push, true);
	}

	// Token: 0x0600046C RID: 1132 RVA: 0x0002DA8A File Offset: 0x0002BC8A
	public static void LaunchObject(Rigidbody r, Vector3 push, bool gravity)
	{
		r.velocity = push;
		r.isKinematic = false;
		r.useGravity = gravity;
	}

	// Token: 0x0600046D RID: 1133 RVA: 0x0002DAA4 File Offset: 0x0002BCA4
	public static void UpdateArea(int newarea)
	{
		if (MainManager.instance.areaid == 15 && MainManager.instance.flags[596])
		{
			MainManager.instance.flags[597] = true;
		}
		MainManager.instance.regionalflags = new bool[100];
		MainManager.instance.areaid = newarea;
		MainManager.instance.librarystuff[4, newarea] = true;
	}

	// Token: 0x0600046E RID: 1134 RVA: 0x0002DB10 File Offset: 0x0002BD10
	public static void UpdateShops()
	{
		for (int i = 0; i < MainManager.instance.avaliablebadgepool.Length; i++)
		{
			MainManager.instance.avaliablebadgepool[i] = MainManager.instance.badgeshops[i];
			MainManager.RandomSort(ref MainManager.instance.avaliablebadgepool[i]);
			if (i == 0 && MainManager.instance.flags[587] && MainManager.instance.badgeshops[i].Count > 0)
			{
				MainManager.instance.flags[587] = false;
			}
		}
	}

	// Token: 0x0600046F RID: 1135 RVA: 0x0002DB9C File Offset: 0x0002BD9C
	public static bool ObjectsAreActive(int[] objects, bool any)
	{
		List<NPCControl> list = new List<NPCControl>();
		for (int i = 0; i < objects.Length; i++)
		{
			list.Add(MainManager.map.entities[objects[i]].npcdata);
		}
		return MainManager.ObjectsAreActive(list.ToArray(), any);
	}

	// Token: 0x06000470 RID: 1136 RVA: 0x0002DBE4 File Offset: 0x0002BDE4
	public static bool ObjectsAreActive(NPCControl[] objects, bool any)
	{
		if (any)
		{
			for (int i = 0; i < objects.Length; i++)
			{
				if (objects[i].hit)
				{
					return true;
				}
			}
			return false;
		}
		for (int j = 0; j < objects.Length; j++)
		{
			if (!objects[j].hit)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000471 RID: 1137 RVA: 0x0002DC30 File Offset: 0x0002BE30
	public static void RandomSort(ref List<int> array)
	{
		int[] array2 = array.ToArray();
		for (int i = 0; i < array2.Length; i++)
		{
			int num = array2[i];
			int num2 = Random.Range(i, array2.Length);
			array2[i] = array2[num2];
			array2[num2] = num;
		}
		array = new List<int>(array2);
	}

	// Token: 0x06000472 RID: 1138 RVA: 0x0002DC74 File Offset: 0x0002BE74
	public static void RandomSort(ref int[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			int num = array[i];
			int num2 = Random.Range(i, array.Length);
			array[i] = array[num2];
			array[num2] = num;
		}
	}

	// Token: 0x06000473 RID: 1139 RVA: 0x0002DCAD File Offset: 0x0002BEAD
	public static IEnumerator FlipSpriteBool(SpriteRenderer sprite, bool x, bool y, float everyxframes, float duringxframes)
	{
		float a = 0f;
		float b = 0f;
		do
		{
			b += MainManager.framestep;
			if (b >= everyxframes)
			{
				b = 0f;
				if (x)
				{
					sprite.flipX = !sprite.flipX;
				}
				if (y)
				{
					sprite.flipY = !sprite.flipY;
				}
			}
			if (duringxframes > 0f)
			{
				a += MainManager.framestep;
			}
			yield return null;
		}
		while (duringxframes == -1f || a < duringxframes);
		yield break;
	}

	// Token: 0x06000474 RID: 1140 RVA: 0x0002DCDC File Offset: 0x0002BEDC
	public void DoClock()
	{
		if (MainManager.instance.flagvar[37] == 0)
		{
			MainManager.instance.flagvar[37] = 8;
		}
		this.clocksec++;
		if (this.clocksec % 5 == 0 && !MainManager.roomtransition)
		{
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}
		if (this.clocksec == 60)
		{
			this.clockmin++;
			this.clocksec = 0;
			if ((this.clockmin == 60 || this.clockmin == 30) && this.flags[254] && this.flagvar[26] > 0)
			{
				this.flagvar[26] = Mathf.Clamp(this.flagvar[26] + Mathf.Clamp(Mathf.FloorToInt((float)this.flagvar[26] * (this.flags[630] ? 0.04f : 0.02f)), 1, 75), 0, 10000);
			}
			if (this.clockmin == 60)
			{
				if (this.clockhour < 9999)
				{
					this.clockhour++;
				}
				this.clockmin = 0;
			}
		}
		if (MainManager.timeddemo)
		{
			if (this.demotimer >= 900 && MainManager.FreePlayer() && MainManager.battle == null)
			{
				MainManager.events.StartEvent(187, null);
			}
			else
			{
				this.demotimer++;
			}
		}
		if (MainManager.player != null && MainManager.battle == null)
		{
			if (MainManager.FreePlayer())
			{
				this.RefreshPlayer(false);
			}
			if (!MainManager.instance.flags[377] && this.items[0].Contains(121))
			{
				MainManager.instance.flags[377] = true;
			}
		}
	}

	// Token: 0x06000475 RID: 1141 RVA: 0x0002DEA0 File Offset: 0x0002C0A0
	public void RefreshPlayer(bool onlycollider = false)
	{
		MainManager.player.ceiling = false;
		if (!onlycollider)
		{
			for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
			{
				if (!MainManager.instance.playerdata[i].entity.noclock)
				{
					MainManager.instance.playerdata[i].entity.onground = false;
					MainManager.instance.playerdata[i].entity.transform.parent = null;
				}
			}
		}
		if (MainManager.battle == null && !this.inevent)
		{
			MainManager.player.entity.ForceHitWall();
		}
	}

	// Token: 0x06000476 RID: 1142 RVA: 0x0002DF50 File Offset: 0x0002C150
	public static bool IsParty(Transform obj)
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].entity.transform == obj)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000477 RID: 1143 RVA: 0x0002DF9C File Offset: 0x0002C19C
	public static bool FreePlayer(bool getfly)
	{
		return MainManager.player != null && !MainManager.instance.minipause && !MainManager.instance.inevent && !MainManager.instance.message && !MainManager.instance.pause && !MainManager.player.digging && (!getfly || !MainManager.player.flying);
	}

	// Token: 0x06000478 RID: 1144 RVA: 0x0002E006 File Offset: 0x0002C206
	public static bool FreePlayer()
	{
		return MainManager.FreePlayer(true);
	}

	// Token: 0x06000479 RID: 1145 RVA: 0x0002E010 File Offset: 0x0002C210
	public void CheckAchievement()
	{
		if (this.started)
		{
			if (!this.flags[63] && MainManager.HowManyTrue(MainManager.GetLibraryBools(3)) >= MainManager.librarylimit[3] - 1)
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 27);
				this.flags[63] = true;
			}
			if (!this.librarystuff[3, 11] && MainManager.instance.flags[410])
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 11);
			}
			if (!this.librarystuff[3, 5] && this.boardquests[2].Contains(11))
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 5);
			}
			if (!this.librarystuff[3, 12] && this.boardquests[2].Contains(12))
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 12);
			}
			if (!this.librarystuff[3, 14] && this.boardquests[2].Contains(13))
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 14);
			}
			if (!this.librarystuff[3, 16] && this.boardquests[2].Contains(14))
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 16);
			}
			if (!this.librarystuff[3, 18] && this.boardquests[2].Contains(15))
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 18);
			}
			if (!this.librarystuff[3, 20] && this.boardquests[2].Contains(16))
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 20);
			}
			if (!this.librarystuff[3, 22] && this.boardquests[2].Contains(17))
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 22);
			}
			if (!this.librarystuff[3, 29] && this.flagvar[28] > 9500 && this.flagvar[29] > 4500)
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 29);
			}
			if (!this.librarystuff[3, 26] && this.boardquests[2].Contains(26))
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 26);
			}
			if (!this.librarystuff[3, 25] && this.boardquests[2].Contains(30))
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 25);
			}
			if (!this.librarystuff[3, 1] && MainManager.CrystalBerryAmmount() >= 50)
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 1);
			}
			if (!this.librarystuff[3, 2] && this.badges.Count >= 120)
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 2);
			}
			if (!this.librarystuff[3, 3] && MainManager.SamiraGotAll())
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 3);
			}
			if (!this.librarystuff[3, 4] && MainManager.instance.partylevel >= 27)
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 4);
			}
			if (!this.librarystuff[3, 7] && this.boardquests[2].Count >= Enum.GetNames(typeof(MainManager.BoardQuests)).Length - 1)
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 7);
			}
			if (!this.librarystuff[3, 8] && MainManager.HowManyTrue(MainManager.GetLibraryBools(2)) >= MainManager.librarylimit[2])
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 8);
			}
			if (!this.librarystuff[3, 9] && MainManager.HowManyTrue(MainManager.GetLibraryBools(1)) >= MainManager.librarylimit[1])
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 9);
			}
			if (!this.librarystuff[3, 10] && MainManager.HowManyTrue(MainManager.GetLibraryBools(0)) >= MainManager.librarylimit[0])
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 10);
			}
			if (!this.librarystuff[3, 28] && MainManager.instance.flags[612])
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 28);
			}
			if (!this.librarystuff[3, 0] && MainManager.instance.flags[610])
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 0);
			}
			if (!this.librarystuff[3, 24] && MainManager.instance.flags[341])
			{
				MainManager.UpdateJounal(MainManager.Library.Logbook, 24);
			}
			if (MainManager.instance.flags[546] && !MainManager.instance.librarystuff[0, 36])
			{
				MainManager.UpdateJounal(MainManager.Library.Discovery, 36);
			}
			for (int i = 0; i < MainManager.librarylimit[3]; i++)
			{
				if (this.librarystuff[3, i])
				{
					InputIO.Achivement(i);
				}
			}
			if (this.librarystuff[3, 7])
			{
				MainManager.instance.flags[671] = true;
			}
		}
	}

	// Token: 0x0600047A RID: 1146 RVA: 0x0002E45C File Offset: 0x0002C65C
	public static bool SamiraGotAll()
	{
		return MainManager.PurchasedMusicAmmount() >= Enum.GetNames(typeof(MainManager.Musics)).Length - 8;
	}

	// Token: 0x0600047B RID: 1147 RVA: 0x0002E47C File Offset: 0x0002C67C
	public static int PurchasedMusicAmmount()
	{
		int num = 0;
		int[][] array = MainManager.instance.samiramusics.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i][1] > -1)
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x0600047C RID: 1148 RVA: 0x0002E4B8 File Offset: 0x0002C6B8
	public static bool[] GetLibraryBools(int page)
	{
		bool[] array = new bool[MainManager.instance.librarystuff.GetLength(1)];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = MainManager.instance.librarystuff[page, i];
		}
		return array;
	}

	// Token: 0x0600047D RID: 1149 RVA: 0x0002E500 File Offset: 0x0002C700
	public static int HowManyTrue(bool[] array)
	{
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i])
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x0600047E RID: 1150 RVA: 0x0002E527 File Offset: 0x0002C727
	public static bool InCameraRange(Transform obj)
	{
		return MainManager.InCameraRange(MainManager.MainCamera.WorldToViewportPoint(obj.transform.position));
	}

	// Token: 0x0600047F RID: 1151 RVA: 0x0002E544 File Offset: 0x0002C744
	public static bool InCameraRange(Vector3 t)
	{
		return t.x > -MainManager.entityactive.x && t.x < MainManager.entityactive.x + 1f && t.y > -MainManager.entityactive.y && t.y < MainManager.entityactive.y + 1f && t.z > MainManager.entityactive.z;
	}

	// Token: 0x06000480 RID: 1152 RVA: 0x0002E5BB File Offset: 0x0002C7BB
	public static AudioSource PlaySound(AudioClip soundclip, int id)
	{
		return MainManager.PlaySound(soundclip, id, 1f);
	}

	// Token: 0x06000481 RID: 1153 RVA: 0x0002E5C9 File Offset: 0x0002C7C9
	public static AudioSource PlaySound(AudioClip soundclip, int id, float pitch)
	{
		return MainManager.PlaySound(soundclip, id, pitch, 1f, false);
	}

	// Token: 0x06000482 RID: 1154 RVA: 0x0002E5D9 File Offset: 0x0002C7D9
	public static AudioSource PlaySound(AudioClip soundclip, int id, float pitch, float volume)
	{
		return MainManager.PlaySound(soundclip, id, pitch, volume, false);
	}

	// Token: 0x06000483 RID: 1155 RVA: 0x0002E5E5 File Offset: 0x0002C7E5
	public static AudioSource PlaySound(string soundclip, int id, float pitch, float volume)
	{
		return MainManager.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/" + soundclip), id, pitch, volume, false);
	}

	// Token: 0x06000484 RID: 1156 RVA: 0x0002E600 File Offset: 0x0002C800
	public static AudioSource PlaySound(string clip, float pitch, float volume)
	{
		return MainManager.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/" + clip), -1, pitch, volume, false);
	}

	// Token: 0x06000485 RID: 1157 RVA: 0x0002E61B File Offset: 0x0002C81B
	public static AudioSource PlaySound(string clip, float volume)
	{
		return MainManager.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/" + clip), -1, 1f, volume, false);
	}

	// Token: 0x06000486 RID: 1158 RVA: 0x0002E63A File Offset: 0x0002C83A
	public static AudioSource PlaySound(string soundclip, int id, float pitch, float volume, bool loop)
	{
		return MainManager.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/" + soundclip), id, pitch, volume, loop);
	}

	// Token: 0x06000487 RID: 1159 RVA: 0x0002E658 File Offset: 0x0002C858
	private static int GetFreeSound()
	{
		int result = MainManager.sounds.Length - 1;
		for (int i = 0; i < MainManager.sounds.Length - 1; i++)
		{
			if (!MainManager.sounds[i].isPlaying)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	// Token: 0x06000488 RID: 1160 RVA: 0x0002E698 File Offset: 0x0002C898
	public static AudioSource PlaySound(AudioClip soundclip, int id, float pitch, float volume, bool loop)
	{
		if (MainManager.SoundVolume())
		{
			if (id == -1)
			{
				id = MainManager.GetFreeSound();
			}
			MainManager.sounds[id].clip = soundclip;
			MainManager.sounds[id].volume = MainManager.soundvolume * volume;
			MainManager.sounds[id].pitch = pitch;
			MainManager.sounds[id].loop = loop;
			MainManager.sounds[id].Play();
			MainManager.lastsoundid = id;
			return MainManager.sounds[id];
		}
		return null;
	}

	// Token: 0x06000489 RID: 1161 RVA: 0x0002E70D File Offset: 0x0002C90D
	public static void StopSound(int clip)
	{
		MainManager.StopSound(clip, 0.1f);
	}

	// Token: 0x0600048A RID: 1162 RVA: 0x0002E71A File Offset: 0x0002C91A
	public static void StopSound(int clip, float delay)
	{
		if (MainManager.SoundVolume())
		{
			if (delay > 0f)
			{
				MainManager.instance.StartCoroutine(MainManager.FadeSound(MainManager.sounds[clip], delay));
				return;
			}
			MainManager.sounds[clip].Stop();
		}
	}

	// Token: 0x0600048B RID: 1163 RVA: 0x0002E750 File Offset: 0x0002C950
	public static bool SoundVolume()
	{
		return (MainManager.pausemenu == null && MainManager.soundvolume > 0f) || (MainManager.pausemenu != null && (MainManager.pausemenu.windowid != 4 || MainManager.pausemenu.svolume > 0f));
	}

	// Token: 0x0600048C RID: 1164 RVA: 0x0002E7A8 File Offset: 0x0002C9A8
	public static void StopSound(AudioClip clip, float delay)
	{
		if (MainManager.SoundVolume())
		{
			for (int i = 0; i < MainManager.sounds.Length; i++)
			{
				if (MainManager.sounds[i].clip == clip)
				{
					if (delay > 0f)
					{
						MainManager.instance.StartCoroutine(MainManager.FadeSound(MainManager.sounds[i], delay));
					}
					else
					{
						MainManager.sounds[i].Stop();
					}
				}
			}
		}
	}

	// Token: 0x0600048D RID: 1165 RVA: 0x0002E810 File Offset: 0x0002CA10
	public static void StopSound(AudioClip clip)
	{
		MainManager.StopSound(clip, 0f);
	}

	// Token: 0x0600048E RID: 1166 RVA: 0x0002E81D File Offset: 0x0002CA1D
	public static void StopSound(string clipname)
	{
		MainManager.StopSound(Resources.Load<AudioClip>("Audio/Sounds/" + clipname), 0f);
	}

	// Token: 0x0600048F RID: 1167 RVA: 0x0002E839 File Offset: 0x0002CA39
	public static void StopSound(string clipname, float delay)
	{
		MainManager.StopSound(Resources.Load<AudioClip>("Audio/Sounds/" + clipname), delay);
	}

	// Token: 0x06000490 RID: 1168 RVA: 0x0002E854 File Offset: 0x0002CA54
	public static Vector3 ClampVectorBox(Vector3 input, Vector3 limitspos, Vector3 limitsneg)
	{
		return new Vector3(Mathf.Clamp(input.x, limitsneg.x, limitspos.x), Mathf.Clamp(input.y, limitsneg.y, limitspos.y), Mathf.Clamp(input.z, limitsneg.z, limitspos.z));
	}

	// Token: 0x06000491 RID: 1169 RVA: 0x0002E8AB File Offset: 0x0002CAAB
	public static Vector3 ClampVectorBox(Vector3 input, Vector3 limits)
	{
		return MainManager.ClampVectorBox(input, limits, -limits);
	}

	// Token: 0x06000492 RID: 1170 RVA: 0x0002E8BA File Offset: 0x0002CABA
	public static AudioSource PlaySound(AudioClip soundclip)
	{
		return MainManager.PlaySound(soundclip, -1);
	}

	// Token: 0x06000493 RID: 1171 RVA: 0x0002E8C3 File Offset: 0x0002CAC3
	public static AudioSource PlaySound(string soundclip, int id)
	{
		return MainManager.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/" + soundclip), id);
	}

	// Token: 0x06000494 RID: 1172 RVA: 0x0002E8DB File Offset: 0x0002CADB
	public static AudioSource PlaySound(string soundclip)
	{
		return MainManager.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/" + soundclip));
	}

	// Token: 0x06000495 RID: 1173 RVA: 0x0002E8F4 File Offset: 0x0002CAF4
	public static Vector3 ClampMagnitude(Vector3 v, float max, float min)
	{
		double num = (double)v.sqrMagnitude;
		if (num > (double)max * (double)max)
		{
			return v.normalized * max;
		}
		if (num < (double)min * (double)min)
		{
			return v.normalized * min;
		}
		return v;
	}

	// Token: 0x06000496 RID: 1174 RVA: 0x0002E937 File Offset: 0x0002CB37
	public static IEnumerator LerpObject(Transform obj, Vector3 position, float speed, bool destroyonend)
	{
		Vector3 start = obj.transform.position;
		float a = 0f;
		while (a < 1f && obj != null)
		{
			obj.transform.position = Vector3.Lerp(start, position, a);
			a += MainManager.TieFramerate(speed);
			yield return null;
		}
		if (destroyonend && obj != null)
		{
			Object.Destroy(obj.gameObject);
		}
		yield break;
	}

	// Token: 0x06000497 RID: 1175 RVA: 0x0002E95B File Offset: 0x0002CB5B
	public static void ChangeMusicVolume(float frametime)
	{
		MainManager.ChangeMusicVolume(MainManager.musicchannel, MainManager.musicvolume, frametime);
	}

	// Token: 0x06000498 RID: 1176 RVA: 0x0002E96D File Offset: 0x0002CB6D
	public static void ChangeMusicVolume(float targetvolume, float frametime)
	{
		MainManager.ChangeMusicVolume(MainManager.musicchannel, targetvolume, frametime);
	}

	// Token: 0x06000499 RID: 1177 RVA: 0x0002E97B File Offset: 0x0002CB7B
	public static void ChangeMusicVolume(int clipid, float targetvolume, float frametime)
	{
		if (MainManager.musiccoroutine != null)
		{
			MainManager.instance.StopCoroutine(MainManager.musiccoroutine);
		}
		MainManager.musiccoroutine = MainManager.instance.StartCoroutine(MainManager.ChangeMVolume(clipid, targetvolume, frametime));
	}

	// Token: 0x0600049A RID: 1178 RVA: 0x0002E9AA File Offset: 0x0002CBAA
	private static IEnumerator ChangeMVolume(int clipid, float volume, float frametime)
	{
		float sv = MainManager.music[clipid].volume;
		for (float a = 0f; a < frametime; a += MainManager.TieFramerate(1f))
		{
			MainManager.music[clipid].volume = Mathf.Lerp(sv, volume, a / frametime);
			yield return null;
		}
		yield return null;
		MainManager.music[clipid].volume = volume;
		yield break;
	}

	// Token: 0x0600049B RID: 1179 RVA: 0x0002E9C7 File Offset: 0x0002CBC7
	private static IEnumerator SwitchMusic(AudioClip musicclip, float fadespeed, int id)
	{
		MainManager.musiccoroutine = MainManager.instance.StartCoroutine(MainManager.SwitchMusic(musicclip, fadespeed, id, false));
		yield return null;
		yield break;
	}

	// Token: 0x0600049C RID: 1180 RVA: 0x0002E9E4 File Offset: 0x0002CBE4
	private static IEnumerator SwitchMusic(AudioClip musicclip, float fadespeed, int id, bool seamless)
	{
		if (MainManager.music[id].clip != musicclip)
		{
			float t = 1f;
			int sid = -1;
			if (seamless)
			{
				sid = MainManager.GetFreeSound();
				MainManager.sounds[sid].clip = musicclip;
				MainManager.sounds[sid].time = MainManager.music[id].time;
				MainManager.sounds[sid].volume = 0f;
				MainManager.sounds[sid].loop = true;
				MainManager.sounds[sid].Play();
			}
			if (t > 0f && MainManager.music[id].clip != null)
			{
				float failsafe = 0f;
				while (t > 0.045f && failsafe < 500f)
				{
					t = Mathf.Lerp(t, 0f, MainManager.framestep * fadespeed);
					MainManager.music[id].volume = MainManager.musicvolume * t;
					MainManager.Musics musics = (MainManager.Musics)Enum.Parse(typeof(MainManager.Musics), MainManager.music[id].clip.name);
					if (musics <= MainManager.Musics.Battle3)
					{
						if (musics - MainManager.Musics.Theater <= 1 || musics == MainManager.Musics.Venus || musics == MainManager.Musics.Battle3)
						{
							goto IL_1AE;
						}
					}
					else if (musics == MainManager.Musics.Bee || musics == MainManager.Musics.TermiteLoop || musics == MainManager.Musics.Pier)
					{
						goto IL_1AE;
					}
					IL_1CB:
					if (seamless)
					{
						MainManager.sounds[sid].volume = MainManager.musicvolume - MainManager.music[id].volume;
						if (Mathf.Abs(MainManager.sounds[sid].time - MainManager.music[id].time) > 0.15f)
						{
							MainManager.sounds[sid].time = MainManager.music[id].time;
						}
					}
					failsafe += MainManager.framestep;
					yield return null;
					continue;
					IL_1AE:
					MainManager.music[id].volume *= 0.9f;
					goto IL_1CB;
				}
				MainManager.music[id].volume = 0f;
			}
			yield return new WaitForSeconds(0.1f);
			if (musicclip != null)
			{
				MainManager.music[id].clip = musicclip;
				if (!seamless)
				{
					MainManager.music[id].time = 0f;
					yield return null;
				}
				else
				{
					MainManager.music[id].time = MainManager.sounds[sid].time;
					MainManager.sounds[sid].Stop();
					MainManager.sounds[sid].time = 0f;
					MainManager.sounds[sid].loop = false;
				}
				if (MainManager.musicresume >= 0f)
				{
					MainManager.music[id].time = MainManager.musicresume;
					MainManager.musicresume = -1f;
				}
				MainManager.music[id].volume = MainManager.musicvolume;
				MainManager.music[id].Play();
			}
			else
			{
				MainManager.music[id].clip = null;
				MainManager.music[id].Stop();
			}
		}
		if (MainManager.battle || MainManager.instance.inevent || (MainManager.map != null && MainManager.map.musicid > -1 && MainManager.map.musicid < MainManager.musicids.Length))
		{
			MainManager.CheckSamira(MainManager.music[id].clip);
		}
		yield return null;
		MainManager.musiccoroutine = null;
		yield break;
	}

	// Token: 0x0600049D RID: 1181 RVA: 0x0002EA08 File Offset: 0x0002CC08
	public static void CheckSamira(AudioClip music)
	{
		if (music != null)
		{
			int num = (int)Enum.Parse(typeof(MainManager.Musics), music.name);
			int[][] array = MainManager.instance.samiramusics.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i][0] == num)
				{
					return;
				}
			}
			MainManager.instance.samiramusics.Add(new int[]
			{
				num,
				-1
			});
		}
	}

	// Token: 0x0600049E RID: 1182 RVA: 0x0002EA80 File Offset: 0x0002CC80
	public static void CheckSamira(string music)
	{
		if (music != null)
		{
			int num = (int)Enum.Parse(typeof(MainManager.Musics), music);
			int[][] array = MainManager.instance.samiramusics.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i][0] == num)
				{
					return;
				}
			}
			MainManager.instance.samiramusics.Add(new int[]
			{
				num,
				-1
			});
		}
	}

	// Token: 0x0600049F RID: 1183 RVA: 0x0002EAEA File Offset: 0x0002CCEA
	public static void ChangeMusic(AudioClip musicclip, float fadespeed, int id)
	{
		MainManager.ChangeMusic(musicclip, fadespeed, id, false);
	}

	// Token: 0x060004A0 RID: 1184 RVA: 0x0002EAF5 File Offset: 0x0002CCF5
	public static void ChangeMusic(string musicclip, float fadespeed, int id, bool seamless)
	{
		MainManager.ChangeMusic(Resources.Load<AudioClip>("Audio/Music/" + musicclip), fadespeed, id, seamless);
	}

	// Token: 0x060004A1 RID: 1185 RVA: 0x0002EB10 File Offset: 0x0002CD10
	public static void ChangeMusic(AudioClip musicclip, float fadespeed, int id, bool seamless)
	{
		if (MainManager.musiccoroutine != null)
		{
			MainManager.instance.StopCoroutine(MainManager.musiccoroutine);
		}
		MainManager.musiccoroutine = MainManager.instance.StartCoroutine(MainManager.SwitchMusic(musicclip, fadespeed, id, seamless));
		if (musicclip != null)
		{
			MainManager.musicids[id] = (int)Enum.Parse(typeof(MainManager.Musics), musicclip.name);
		}
		else
		{
			MainManager.musicids[id] = -1;
		}
		if (MainManager.musicids[id] > -1)
		{
			MainManager.lastmusic = MainManager.musicids[id];
		}
	}

	// Token: 0x060004A2 RID: 1186 RVA: 0x0002EB95 File Offset: 0x0002CD95
	public static void ChangeMusic(string musicclip, float fadespeed)
	{
		MainManager.ChangeMusic(Resources.Load<AudioClip>("Audio/Music/" + musicclip), fadespeed, 0);
	}

	// Token: 0x060004A3 RID: 1187 RVA: 0x0002EBB0 File Offset: 0x0002CDB0
	public static int SoundIsPlaying(string name)
	{
		for (int i = 0; i < MainManager.sounds.Length; i++)
		{
			if (MainManager.sounds[i].isPlaying && MainManager.sounds[i].clip != null && MainManager.sounds[i].clip.name == name)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x060004A4 RID: 1188 RVA: 0x0002EC0D File Offset: 0x0002CE0D
	public static void ChangeMusic(AudioClip musicclip, float fadespeed)
	{
		MainManager.ChangeMusic(musicclip, fadespeed, 0);
	}

	// Token: 0x060004A5 RID: 1189 RVA: 0x0002EC17 File Offset: 0x0002CE17
	public static void ChangeMusic(string musicclip, float fadespeed, int id)
	{
		MainManager.ChangeMusic(Resources.Load<AudioClip>("Audio/Music/" + musicclip), fadespeed, id);
	}

	// Token: 0x060004A6 RID: 1190 RVA: 0x0002EC30 File Offset: 0x0002CE30
	public static void ChangeMusic()
	{
		if (MainManager.map != null && MainManager.map.music != null && MainManager.map.music.Length != 0 && MainManager.map.musicid > -1)
		{
			MainManager.ChangeMusic(MainManager.map.music[MainManager.map.musicid]);
			return;
		}
		MainManager.ChangeMusic(null);
	}

	// Token: 0x060004A7 RID: 1191 RVA: 0x0002EC91 File Offset: 0x0002CE91
	public static void ChangeMusic(int mapid)
	{
		MainManager.ChangeMusic(MainManager.map.music[mapid]);
	}

	// Token: 0x060004A8 RID: 1192 RVA: 0x0002ECA4 File Offset: 0x0002CEA4
	public static void ChangeMusic(string musicclip)
	{
		if (musicclip == null)
		{
			MainManager.ChangeMusic(null, 0.1f);
			return;
		}
		MainManager.ChangeMusic(Resources.Load<AudioClip>("Audio/Music/" + musicclip), 0.1f, 0);
	}

	// Token: 0x060004A9 RID: 1193 RVA: 0x0002ECD0 File Offset: 0x0002CED0
	public static void ChangeMusic(AudioClip musicclip)
	{
		MainManager.ChangeMusic(musicclip, 0.075f);
	}

	// Token: 0x060004AA RID: 1194 RVA: 0x0002ECE0 File Offset: 0x0002CEE0
	private static bool AnyJoyKey(bool hold)
	{
		for (int i = 0; i < 20; i++)
		{
			if (!hold && Input.GetKeyDown(KeyCode.JoystickButton0 + i))
			{
				return true;
			}
			if (hold && Input.GetKey(KeyCode.JoystickButton0 + i))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060004AB RID: 1195 RVA: 0x0002ED21 File Offset: 0x0002CF21
	private static bool AnyJoyStick()
	{
		return InputIO.JoyStick(0) != 0f || InputIO.JoyStick(2) != 0f || InputIO.JoyStick(1) != 0f || InputIO.JoyStick(3) != 0f;
	}

	// Token: 0x060004AC RID: 1196 RVA: 0x0002ED5C File Offset: 0x0002CF5C
	private void GetJoystick()
	{
		if (MainManager.usejoystick > 0 || InputIO.IsConsole)
		{
			if (!InputIO.IsConsole && Input.anyKeyDown && !MainManager.AnyJoyKey(false) && !MainManager.AnyJoyStick())
			{
				MainManager.joystick = false;
			}
			else if (MainManager.forcecontrollerupdate || (!MainManager.joystick && (InputIO.IsConsole || MainManager.AnyJoyKey(false) || MainManager.AnyJoyStick())))
			{
				MainManager.forcecontrollerupdate = false;
				MainManager.joystick = true;
				if (MainManager.usejoystick == 4)
				{
					MainManager.joyid = MainManager.forcejoystick;
				}
				else if (MainManager.usejoystick == 2)
				{
					MainManager.joyid = 0;
				}
				else if (MainManager.usejoystick < 5)
				{
					RuntimePlatform platform = Application.platform;
					if (platform != RuntimePlatform.PS4)
					{
						if (platform != RuntimePlatform.XboxOne)
						{
							if (platform != RuntimePlatform.Switch)
							{
								string[] array = MainManager.Controllers();
								for (int i = 0; i < array.Length; i++)
								{
									if (MainManager.usejoystick < 3 || i == MainManager.forcejoystick)
									{
										if (array[i].Contains("Fight Pad Pro"))
										{
											MainManager.joyid = 7;
										}
										else if (array[i] == "Bluetooth Gamepad   " || array[i].Contains("PC/PS3/Android"))
										{
											MainManager.joyid = 6;
										}
										else if (array[i].Contains("ðñò"))
										{
											MainManager.joyid = 5;
										}
										else if (array[i] == "Wireless Gamepad")
										{
											MainManager.joyid = 2;
										}
										else if (!array[i].Contains("Xbox") && !array[i].Contains("XBOX") && (array[i].Contains("Wireless") || array[i].Contains("PS4") || array[i].Contains("Sony") || array[i].Contains("tation") || array[i].Contains("PLAYSTATION") || array[i].Contains("3 TURBO")))
										{
											MainManager.joyid = 1;
										}
										else
										{
											MainManager.joyid = 0;
										}
										MonoBehaviour.print(array[i]);
										break;
									}
								}
							}
							else
							{
								MainManager.joyid = 2;
							}
						}
						else
						{
							MainManager.joyid = 0;
						}
					}
					else
					{
						MainManager.joyid = 1;
					}
				}
				InputIO.GetJoyButtons();
			}
		}
		else
		{
			MainManager.joystick = false;
		}
		if (MainManager.joystick && !InputIO.IsConsole)
		{
			Screen.sleepTimeout = -1;
			return;
		}
		Screen.sleepTimeout = -2;
	}

	// Token: 0x060004AD RID: 1197 RVA: 0x0002EFA4 File Offset: 0x0002D1A4
	private static void ShowBackDialogue()
	{
		MainManager.DestroyAllChildren(MainManager.instance.textbox);
		string text = MainManager.diagstring[MainManager.currentdialogue];
		if (MainManager.battle != null)
		{
			text = text.Replace("   ", "").Replace("  ", "");
		}
		if (text[0] == ' ')
		{
			text = text.Remove(0);
		}
		MainManager.instance.StartCoroutine(MainManager.SetText("|color,7|" + text, Vector3.zero, MainManager.instance.textbox));
	}

	// Token: 0x060004AE RID: 1198 RVA: 0x0002F03C File Offset: 0x0002D23C
	private static bool HasSkillCost(int tpcost, int playerid)
	{
		if (MainManager.BadgeIsEquipped(72, MainManager.instance.playerdata[playerid].trueid) || tpcost < 0)
		{
			return MainManager.instance.playerdata[playerid].hp - 1 >= Mathf.Abs(tpcost);
		}
		return MainManager.instance.tp >= tpcost;
	}

	// Token: 0x060004AF RID: 1199 RVA: 0x0002F09E File Offset: 0x0002D29E
	public static IEnumerator Spin(Transform obj, Vector3 target, float frametime, bool smooth)
	{
		float a = 0f;
		Vector3 s = obj.eulerAngles;
		do
		{
			if (smooth)
			{
				obj.eulerAngles = MainManager.SmoothLerp(s, target, a / frametime);
			}
			else
			{
				obj.eulerAngles = Vector3.Lerp(s, target, a / frametime);
			}
			a += MainManager.TieFramerate(1f);
			yield return null;
		}
		while (a < frametime + 1f);
		yield break;
	}

	// Token: 0x060004B0 RID: 1200 RVA: 0x0002F0C4 File Offset: 0x0002D2C4
	public static bool KeyHold(MainManager.Directions direction)
	{
		if (MainManager.GetKey((int)direction, true))
		{
			if (MainManager.keyhold[0] > 15f)
			{
				if (MainManager.keyhold[1] >= 7f)
				{
					MainManager.keyhold[1] = 0f;
					return true;
				}
				MainManager.keyhold[1] += MainManager.framestep;
			}
			else
			{
				MainManager.keyhold[0] += MainManager.framestep;
			}
		}
		return false;
	}

	// Token: 0x060004B1 RID: 1201 RVA: 0x0002F12F File Offset: 0x0002D32F
	public static void ResetKeyHold()
	{
		if (!MainManager.GetKey(0, true) && !MainManager.GetKey(1, true) && !MainManager.GetKey(2, true) && !MainManager.GetKey(3, true))
		{
			MainManager.keyhold[0] = 0f;
			MainManager.keyhold[1] = 0f;
		}
	}

	// Token: 0x060004B2 RID: 1202 RVA: 0x0002F16D File Offset: 0x0002D36D
	private static bool MultiItem()
	{
		return (MainManager.listtype == 2 || (MainManager.listtype == 0 && MainManager.instance.flags[349]) || MainManager.listsell) && MainManager.instance.multiselect.Count > 0;
	}

	// Token: 0x060004B3 RID: 1203 RVA: 0x0002F1AB File Offset: 0x0002D3AB
	private static bool IsInMultiList()
	{
		return MainManager.listtype == 2 || (MainManager.listtype == 0 && MainManager.instance.flags[349]) || MainManager.listsell;
	}

	// Token: 0x060004B4 RID: 1204 RVA: 0x0002F1D5 File Offset: 0x0002D3D5
	public static bool IsPlayerInPos(int pos, Transform entity)
	{
		return MainManager.instance.playerdata[MainManager.battle.partypointer[pos]].battleentity.transform == entity;
	}

	// Token: 0x060004B5 RID: 1205 RVA: 0x0002F204 File Offset: 0x0002D404
	private void Update()
	{
		if (MainManager.basicload)
		{
			if (this.message)
			{
				if (!this.prompt && this.itemlist == null)
				{
					if (this.blinker != null)
					{
						if (this.waitinput)
						{
							this.blinker.enabled = true;
						}
						else
						{
							this.blinker.enabled = false;
						}
					}
					if (MainManager.GetKey(5, true))
					{
						if (this.inputcooldown <= 0f && !MainManager.noskip && !this.inlist && MainManager.currentdialogue == MainManager.diagstring.Count)
						{
							this.isholdingskip = true;
							if (this.waitinput)
							{
								MainManager.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/Confirm1"), -1, 0.6f, 0.5f);
								this.inputcooldown = 16f;
							}
							else
							{
								this.inputcooldown = 4f;
							}
							this.waitinput = false;
							this.skiptext = true;
						}
					}
					else if ((MainManager.GetKey(4, false) || MainManager.GetKey(5, false)) && !this.isholdingskip)
					{
						if (MainManager.currentdialogue == MainManager.diagstring.Count)
						{
							if (this.inputcooldown <= 0f)
							{
								if (this.waitinput)
								{
									this.waitinput = false;
									MainManager.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/Confirm1"), -1, 0.4f, 0.5f);
									this.inputcooldown = 16f;
								}
								else
								{
									this.skiptext = true;
								}
							}
						}
						else
						{
							this.inputcooldown = 16f;
							MainManager.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/Confirm1"), -1, 0.6f, 0.5f);
							MainManager.currentdialogue++;
							if (MainManager.currentdialogue == MainManager.diagstring.Count)
							{
								MainManager.DestroyText(MainManager.instance.textbox);
								this.waitinput = false;
							}
							else
							{
								MainManager.ShowBackDialogue();
							}
						}
					}
					else if (MainManager.GetKey(6, false) && !MainManager.notextbacktrack)
					{
						if (MainManager.currentdialogue > 0 && this.waitinput && this.textbox != null && this.inputcooldown <= 0f)
						{
							this.inputcooldown = 16f;
							MainManager.PlaySound("FunnyStep2", -1, 1f, 0.5f);
							if (MainManager.currentdialogue == MainManager.diagstring.Count && !MainManager.backtracking)
							{
								MainManager.diagstring.Add(MainManager.OrganizeLines(MainManager.tempdiag.Replace("  ", ""), MainManager.linebr, MainManager.fontdsize, MainManager.fontdtype));
								MainManager.ResetDiag();
								MainManager.backtracking = true;
							}
							MainManager.currentdialogue--;
							MainManager.ShowBackDialogue();
						}
					}
					else if (!MainManager.GetKey(5, true) && this.inputcooldown <= 0f)
					{
						this.isholdingskip = false;
					}
				}
				else if (this.prompt && this.inputcooldown <= 0f)
				{
					if (!this.numberprompt && this.cursor != null)
					{
						this.cursor.transform.localPosition = Vector3.Lerp(this.cursor.transform.localPosition, new Vector3(this.promptbox.GetChild(1).localPosition.x - 0.25f, this.promptbox.GetChild(1 + this.option).localPosition.y + 0.25f, 10f), MainManager.TieFramerate(0.2f));
					}
					if (this.blinker != null)
					{
						this.blinker.enabled = false;
					}
					if (MainManager.GetKey(0, false) || MainManager.KeyHold(MainManager.Directions.Up))
					{
						MainManager.PlayScrollSound();
						if (!this.numberprompt)
						{
							this.option--;
							if (this.option < 0)
							{
								this.option = this.maxoptions - 1;
							}
						}
						else
						{
							if (MainManager.instance.flagvar[0] == -555)
							{
								this.option = Mathf.Clamp(this.option - this.flagvar[1], 0, this.maxoptions - 1);
							}
							else
							{
								this.option = 0;
							}
							this.flagvar[5] = 0;
						}
					}
					else if (MainManager.GetKey(1, false) || MainManager.KeyHold(MainManager.Directions.Down))
					{
						MainManager.PlayScrollSound();
						if (!this.numberprompt)
						{
							this.option++;
							if (this.option >= this.maxoptions)
							{
								this.option = 0;
							}
						}
						else
						{
							if (MainManager.instance.flagvar[0] == -555)
							{
								this.option = Mathf.Clamp(this.option + this.flagvar[1], 0, this.maxoptions - 1);
							}
							else
							{
								this.option = this.maxoptions - 1;
							}
							this.flagvar[5] = 1;
						}
					}
					else if (!MainManager.GetKey(2, true) && !MainManager.GetKey(3, true))
					{
						MainManager.ResetKeyHold();
					}
					if (this.numberprompt)
					{
						if (MainManager.GetKey(8) && this.inputcooldown <= 0f)
						{
							this.option = this.maxoptions - 1;
						}
						if (MainManager.GetKey(6, false) && this.letterprompt > -1 && this.inputcooldown <= 0f)
						{
							this.ChangeLetterPrompt(-1);
						}
						else if (MainManager.GetKey(2, false) || MainManager.KeyHold(MainManager.Directions.Left))
						{
							MainManager.PlayScrollSound();
							if (this.option > 0)
							{
								this.option--;
								if (MainManager.instance.flagvar[0] == -555 && (this.option + 1) % this.flagvar[1] == 0 && this.option < this.maxoptions - 4)
								{
									this.option += this.flagvar[1];
								}
							}
							else if (MainManager.instance.flagvar[0] == -555)
							{
								this.option += this.flagvar[1] - 1;
							}
							this.flagvar[5] = 2;
						}
						else if (MainManager.GetKey(3, false) || MainManager.KeyHold(MainManager.Directions.Right))
						{
							MainManager.PlayScrollSound();
							if (this.option < this.maxoptions + ((this.letterprompt > -1) ? -1 : 0))
							{
								this.option++;
								if (MainManager.instance.flagvar[0] == -555 && this.option % this.flagvar[1] == 0 && this.option < this.maxoptions - 1)
								{
									this.option -= this.flagvar[1];
								}
							}
							this.flagvar[5] = 3;
						}
						else if (!MainManager.GetKey(0, true) && !MainManager.GetKey(1, true))
						{
							MainManager.ResetKeyHold();
						}
					}
					if (this.numberprompt && this.flagvar[0] == -555 && this.flagvar[5] > -1)
					{
						while (this.option < this.flagstring[1].Length - 1 && this.flagstring[1][this.option] == ' ')
						{
							switch (this.flagvar[5])
							{
							case 0:
								this.option -= this.flagvar[1];
								break;
							case 1:
								this.option += this.flagvar[1];
								break;
							case 2:
								this.option--;
								break;
							case 3:
								this.option++;
								break;
							}
							this.option = Mathf.Clamp(this.option, 0, this.maxoptions - 1);
						}
						this.flagvar[5] = -1;
					}
					else if (MainManager.GetKey(4, false))
					{
						MainManager.listcanceled = false;
						MainManager.PlaySound("Confirm", -1);
						this.inputcooldown = 5f;
						this.lastPrompt = this.option;
						if (!this.numberprompt)
						{
							this.promptpick = this.option;
							this.prompt = false;
							this.skiptext = false;
						}
						else
						{
							this.promptpick = 0;
							if (this.flagvar[0] == -555)
							{
								if (this.option < this.maxoptions - 3)
								{
									if (this.flagstring[MainManager.listtype].Length < this.flagvar[10])
									{
										if (this.letterprompt == 4)
										{
											if (this.option + 3 < MainManager.koreanLimit[this.flagvar[6]].x || this.option + 3 > MainManager.koreanLimit[this.flagvar[6]].y)
											{
												MainManager.PlayBuzzer();
											}
											else
											{
												this.flagvar[6]++;
												if (this.flagvar[6] == 3)
												{
													this.flagvar[6] = 0;
													string[] array = this.flagstring;
													int num = MainManager.listtype;
													array[num] += MainManager.GetKoreanChar(new int[]
													{
														MainManager.koreanHL[0] - MainManager.koreanLimit[0].x,
														MainManager.koreanHL[1] - MainManager.koreanLimit[1].x,
														this.option - MainManager.koreanLimit[2].x + 3
													}).ToString();
													MainManager.koreanHL = new int[]
													{
														-1,
														-1
													};
													this.option = 0;
												}
												else
												{
													MainManager.koreanHL[this.flagvar[6] - 1] = this.option + 3;
												}
												this.UpdateKoreanPrompt(this.flagvar[6]);
												this.RefreshNumberPrompt();
											}
										}
										else
										{
											string[] array2 = this.flagstring;
											int num2 = MainManager.listtype;
											array2[num2] += this.flagstring[1][this.option].ToString();
											this.RefreshNumberPrompt();
										}
									}
									else
									{
										MainManager.PlayBuzzer();
									}
								}
								else if (this.option == this.maxoptions - 3)
								{
									if (this.flagstring[MainManager.listtype].Length > 0)
									{
										if (this.letterprompt == 4 && this.flagvar[6] > 0)
										{
											this.flagvar[6] = 0;
											MainManager.koreanHL = new int[]
											{
												-1,
												-1
											};
											this.UpdateKoreanPrompt(0);
										}
										else
										{
											this.flagstring[MainManager.listtype] = this.flagstring[MainManager.listtype].Remove(this.flagstring[MainManager.listtype].Length - 1);
										}
										this.RefreshNumberPrompt();
									}
									else
									{
										MainManager.PlayBuzzer();
									}
								}
								else if (this.option == this.maxoptions - 2)
								{
									if (this.flagstring[MainManager.listtype].Length < this.flagvar[10])
									{
										if (this.letterprompt == 4 && this.flagvar[6] > 0)
										{
											MainManager.PlayBuzzer();
										}
										else
										{
											string[] array3 = this.flagstring;
											int num3 = MainManager.listtype;
											array3[num3] += " ";
											this.RefreshNumberPrompt();
										}
									}
									else
									{
										MainManager.PlayBuzzer();
									}
								}
								else if (this.option == this.maxoptions - 1)
								{
									if (this.flagstring[MainManager.listtype].Length > 0)
									{
										this.promptpointers = new int[]
										{
											MainManager.listredirect.Value
										};
										this.prompt = false;
										this.skiptext = false;
									}
									else
									{
										MainManager.PlayBuzzer();
									}
								}
							}
							else if (this.option < 10)
							{
								if (this.flagstring[0].Length < this.flagvar[10])
								{
									string[] array4 = this.flagstring;
									int num4 = 0;
									array4[num4] += this.option;
									this.RefreshNumberPrompt();
								}
								else
								{
									MainManager.PlayBuzzer();
								}
							}
							else if (this.option == 10)
							{
								if (this.flagstring[0].Length == 0)
								{
									this.promptpointers = new int[]
									{
										MainManager.listcancel
									};
								}
								else
								{
									this.flagvar[MainManager.listtype] = Convert.ToInt32(this.flagstring[0]);
									this.promptpointers = new int[]
									{
										MainManager.listredirect.Value
									};
								}
								this.prompt = false;
								this.skiptext = false;
							}
							else if (this.flagstring[0].Length > 0)
							{
								this.flagstring[0] = this.flagstring[0].Remove(this.flagstring[0].Length - 1);
								this.RefreshNumberPrompt();
							}
							else
							{
								MainManager.PlayBuzzer();
							}
						}
						MainManager.ResetDiag(false);
					}
					else if (MainManager.GetKey(5, false))
					{
						if (this.numberprompt)
						{
							MainManager.listcanceled = true;
							MainManager.PlaySound("Cancel", 10);
							if (MainManager.instance.flagvar[0] != -555)
							{
								this.promptpick = 0;
								this.promptpointers = new int[]
								{
									MainManager.listcancel
								};
								this.prompt = false;
								this.inputcooldown = 15f;
								this.skiptext = false;
								MainManager.ResetDiag(false);
							}
							else if (this.flagstring[MainManager.listtype].Length > 0)
							{
								if (this.letterprompt == 4 && this.flagvar[6] > 0)
								{
									this.flagvar[6] = 0;
									MainManager.koreanHL = new int[]
									{
										-1,
										-1
									};
									this.UpdateKoreanPrompt(0);
								}
								else
								{
									this.flagstring[MainManager.listtype] = this.flagstring[MainManager.listtype].Remove(this.flagstring[MainManager.listtype].Length - 1);
								}
								this.RefreshNumberPrompt();
							}
							else
							{
								MainManager.PlayBuzzer();
							}
						}
						else if (MainManager.listcancel > -1)
						{
							MainManager.PlaySound("Cancel", 10);
							this.skiptext = false;
							this.inputcooldown = 10f;
							this.promptpick = MainManager.listcancel;
							this.option = MainManager.listcancel;
							this.prompt = false;
							this.skiptext = false;
						}
					}
				}
			}
			if (MainManager.instance.flagvar[3] == 5353 && this.inevent && !this.message && !MainManager.noskip && MainManager.lastevent == 3 && this.skiptext && this.inputcooldown <= 0f && (MainManager.GetKey(4, true) || MainManager.GetKey(5, true)))
			{
				MainManager.sounds[5].pitch = 2.5f;
				MainManager.sounds[6].pitch = 2.5f;
				Time.timeScale = 2.5f;
				MainManager.noskip = true;
			}
			if (this.itemlist != null && ((!this.inbattle && !this.pause) || this.inbattle) && this.inputcooldown <= 0f)
			{
				if (MainManager.listsell)
				{
					this.showmoney = 10f;
				}
				if (this.cursor != null)
				{
					if (this.questboardobj == null)
					{
						if (MainManager.listtype == 20)
						{
							this.cursor.transform.localPosition = Vector3.Lerp(this.cursor.transform.localPosition, new Vector3(-2f, -((float)MainManager.listcursor * 0.7f), 10f), MainManager.TieFramerate(0.2f));
						}
						else
						{
							this.cursor.transform.localPosition = Vector3.Lerp(this.cursor.transform.localPosition, new Vector3(2f, 0.75f - (float)MainManager.listcursor * 0.7f, 10f), MainManager.TieFramerate(0.2f));
						}
					}
					else
					{
						this.cursor.transform.localPosition = Vector3.Lerp(this.cursor.transform.localPosition, new Vector3(-7.3f, 2.8f - (float)MainManager.listcursor * 0.7f, 10f), MainManager.TieFramerate(0.2f));
					}
				}
				int num5 = this.option;
				if (MainManager.GetKey(0, false) || MainManager.KeyHold(MainManager.Directions.Up))
				{
					this.UpdateList(true);
					if (this.option != num5)
					{
						MainManager.ShowItemList(MainManager.listtype, MainManager.listpos, MainManager.listdesc, MainManager.listsell);
					}
				}
				else if (MainManager.GetKey(1, false) || MainManager.KeyHold(MainManager.Directions.Down))
				{
					this.UpdateList(false);
					if (this.option != num5)
					{
						MainManager.ShowItemList(MainManager.listtype, MainManager.listpos, MainManager.listdesc, MainManager.listsell);
					}
				}
				else if (MainManager.GetKey(2, false))
				{
					if (this.questboardobj == null)
					{
						for (int i = 0; i < MainManager.listammount; i++)
						{
							this.UpdateList(MainManager.Directions.Up);
						}
						if (this.option != num5)
						{
							MainManager.ShowItemList(MainManager.listtype, MainManager.listpos, MainManager.listdesc, MainManager.listsell);
						}
					}
					else
					{
						MainManager.PlaySound("PageFlip");
						MainManager.ResetList();
						MainManager.listtype--;
						if (MainManager.listtype < 14)
						{
							MainManager.listtype = 16;
						}
						MainManager.UpdateQuestBoard();
						MainManager.ShowItemList(MainManager.listtype, MainManager.listpos, true, false);
					}
				}
				else if (MainManager.GetKey(3, false))
				{
					if (this.questboardobj == null)
					{
						for (int j = 0; j < MainManager.listammount; j++)
						{
							this.UpdateList(MainManager.Directions.Down);
						}
						if (this.option != num5)
						{
							MainManager.ShowItemList(MainManager.listtype, MainManager.listpos, MainManager.listdesc, MainManager.listsell);
						}
					}
					else
					{
						MainManager.PlaySound("PageFlip");
						MainManager.ResetList();
						MainManager.listtype++;
						if (MainManager.listtype > 16)
						{
							MainManager.listtype = 14;
						}
						MainManager.UpdateQuestBoard();
						MainManager.ShowItemList(MainManager.listtype, MainManager.listpos, true, false);
					}
				}
				else
				{
					MainManager.ResetKeyHold();
				}
				if (((MainManager.listtype == 0 && MainManager.instance.flags[349]) || MainManager.listtype == 2 || MainManager.listsell) && MainManager.GetKey(7))
				{
					bool flag = false;
					if (this.multiselect.Contains(this.option))
					{
						this.multiselect.Remove(this.option);
						MainManager.PlaySound("BadgeDequip");
						flag = true;
					}
					else if (MainManager.listsell || this.multiselect.Count < ((MainManager.listtype == 0) ? (this.maxstorage - this.items[2].Count) : (this.maxitems - this.items[0].Count)))
					{
						this.multiselect.Add(this.option);
						MainManager.PlaySound("BadgeEquip");
						flag = true;
					}
					if (flag)
					{
						MainManager.listY = -1;
						MainManager.ShowItemList(MainManager.listtype, MainManager.listpos, MainManager.listdesc, MainManager.listsell);
					}
					else
					{
						MainManager.PlayBuzzer();
					}
				}
				else if (MainManager.GetKey(4, false))
				{
					MainManager.PlaySound("Confirm", 10);
					MainManager.listcanceled = false;
					this.lastPrompt = this.option;
					if (MainManager.savelastlist)
					{
						MainManager.overridedlist = MainManager.SaveList();
						MainManager.savelastlist = false;
					}
					if (this.questboardobj != null)
					{
						if (MainManager.listtype == 14 && this.boardcaller != null)
						{
							if (MainManager.listvar[this.option] != 0)
							{
								this.CloseQuestBoard();
								EntityControl entity = MainManager.GetEntity(this.boardcaller.data[0]);
								for (int k = 0; k < MainManager.instance.playerdata.Length; k++)
								{
									MainManager.instance.playerdata[k].entity.FaceTowards(entity.transform.position);
								}
								base.StartCoroutine(MainManager.SetText("|questprompt|" + MainManager.GetDialogueText(this.boardcaller.data[1]), true, Vector3.zero, entity.transform, entity.npcdata));
								this.flagvar[0] = MainManager.listvar[this.option];
								this.DestroyList();
							}
							else
							{
								MainManager.PlayBuzzer();
							}
						}
						else if (Convert.ToInt32(MainManager.map.name) == 0)
						{
							MainManager.instance.boardquests[MainManager.listtype - 14].Remove(MainManager.listvar[this.option]);
							this.CloseQuestBoard();
							this.DestroyList();
						}
					}
					else if (MainManager.listtype == 20)
					{
						MainManager.languageid = MainManager.listvar[this.option];
						InputIO.LoadSettings(true);
						this.SetVariables();
						this.DestroyList();
						base.StartCoroutine(Object.FindObjectOfType<StartMenu>().Intro());
						MainManager.LoadLangSpecific();
					}
					else if (MainManager.listtype < 0)
					{
						int num6 = Mathf.Abs(MainManager.listtype) - 1;
						int tpcost = MainManager.GetTPCost(num6, this.option);
						MainManager.instance.flagvar[0] = tpcost;
						bool flag2 = MainManager.HasSkillCost(tpcost, num6);
						if (MainManager.battle.CanSkill(MainManager.listvar[this.option]) && flag2)
						{
							MainManager.battle.SetItem(MainManager.listvar[this.option]);
							this.DestroyList();
						}
						else
						{
							if (!flag2 && MainManager.listvar[MainManager.instance.option] != 48)
							{
								MainManager.hudsprites[MainManager.BadgeIsEquipped(72, num6) ? num6 : 3].color = Color.red;
							}
							MainManager.PlayBuzzer();
						}
					}
					else if (MainManager.listtype != 18 && MainManager.listtype != 17)
					{
						if (MainManager.instance.inbattle && MainManager.listtype != 35)
						{
							if (MainManager.battle.currentaction != BattleControl.Pick.StrategyList || (MainManager.battle.currentaction == BattleControl.Pick.StrategyList && ((this.option == 2 && !MainManager.battle.disablespy) || ((this.option == 0 || this.option == 1) && (MainManager.AllPartyFree() || MainManager.GetAlivePlayerAmmount() == 1)) || this.option == 4 || this.option == 3)))
							{
								MainManager.battle.SetItem(MainManager.listvar[this.option]);
								this.DestroyList();
							}
							else
							{
								MainManager.PlayBuzzer();
							}
						}
						else
						{
							this.flagvar[MainManager.storeid] = MainManager.listvar[this.option];
							int num7 = MainManager.listtype;
							if (num7 == 2 || num7 == 1)
							{
								num7 = 0;
							}
							if (MainManager.listvar[this.option] > -1 && MainManager.listtype < 22)
							{
								this.flagstring[0] = ((MainManager.listtype == 3 || MainManager.listtype == 32) ? MainManager.badgedata[MainManager.listvar[this.option], 0] : MainManager.itemdata[num7, MainManager.listvar[this.option], 0]);
								if (MainManager.listsell)
								{
									if (this.multiselect.Count > 0)
									{
										this.flagstring[0] = MainManager.menutext[279];
										this.flagvar[10] = 0;
										for (int l = 0; l < this.multiselect.Count; l++)
										{
											this.flagvar[10] += Mathf.Clamp(Mathf.FloorToInt((float)(Convert.ToInt32(MainManager.itemdata[num7, MainManager.listvar[this.multiselect[l]], 4]) / 2)), 1, 999);
										}
										MainManager.instance.flags[349] = true;
									}
									else
									{
										this.flagvar[10] = Mathf.Clamp(Mathf.FloorToInt((float)(Convert.ToInt32(MainManager.itemdata[num7, MainManager.listvar[this.option], 4]) / 2)), 1, 999);
									}
								}
							}
							this.DestroyList();
						}
					}
					else
					{
						this.DestroyList();
					}
				}
				else if (MainManager.GetKey(5, false) && MainManager.listtype != 20)
				{
					MainManager.instance.multiselect = new List<int>();
					MainManager.PlaySound("Cancel", 10);
					MainManager.listcanceled = true;
					this.inputcooldown = 15f;
					MainManager.instance.skiptext = false;
					if (this.questboardobj != null)
					{
						this.CloseQuestBoard();
						MainManager.SaveCameraPosition(false);
					}
					else if (MainManager.battle != null)
					{
						MainManager.battle.CancelList();
					}
					else
					{
						MainManager.listredirect = new int?(MainManager.listcancel);
					}
					this.DestroyList();
				}
			}
			this.GetJoystick();
		}
	}

	// Token: 0x060004B6 RID: 1206 RVA: 0x00030A28 File Offset: 0x0002EC28
	public static IEnumerator TempColor(Color color, float frametime, SpriteRenderer render)
	{
		float a = 0f;
		Color c = render.material.color;
		while (render != null)
		{
			render.material.color = Color.Lerp(c, color, a / frametime);
			yield return null;
			a += MainManager.TieFramerate(1f);
			if (a >= frametime + 1f)
			{
				a = 0f;
				yield return null;
				while (render != null)
				{
					render.material.color = Color.Lerp(color, c, a / frametime);
					yield return null;
					a += MainManager.TieFramerate(1f);
					if (a >= frametime + 1f)
					{
						if (render != null)
						{
							render.material.color = c;
						}
						MainManager.templetter = null;
						yield break;
					}
				}
				yield break;
			}
		}
		yield break;
	}

	// Token: 0x060004B7 RID: 1207 RVA: 0x00030A48 File Offset: 0x0002EC48
	public static void SetFont(TextMesh letter, int fontid)
	{
		fontid = MainManager.FontID(fontid);
		if (fontid < MainManager.fonts.Length)
		{
			letter.font = MainManager.fonts[fontid];
		}
		MeshRenderer component = letter.GetComponent<MeshRenderer>();
		component.material = MainManager.fontmat[fontid];
		component.material.shader = MainManager.fontmat[0].shader;
	}

	// Token: 0x060004B8 RID: 1208 RVA: 0x00030A9D File Offset: 0x0002EC9D
	public static IEnumerator TempLetters(string text, int[] spacebreaks, bool rainbow, Vector3 posLeave0ToCenter, float showtime)
	{
		TextMesh[] roundletter = new TextMesh[text.Length];
		if (spacebreaks == null)
		{
			spacebreaks = new int[]
			{
				-1
			};
		}
		float space = 0f;
		List<int> s = new List<int>(spacebreaks);
		int num;
		for (int i = 0; i < roundletter.Length; i = num + 1)
		{
			if (text[i] != ' ')
			{
				if (s.Contains(i))
				{
					space += 1f;
				}
				roundletter[i] = MainManager.GetEmptyLetter();
				roundletter[i].text = (text[i].ToString() ?? "");
				DialogueAnim dialogueAnim = roundletter[i].gameObject.AddComponent<DialogueAnim>();
				if (rainbow)
				{
					roundletter[i].gameObject.AddComponent<FontEffects>().SetEffects(false, false, true, false, false, 2, i);
				}
				dialogueAnim.targetscale = Vector3.one * 2f;
				dialogueAnim.shrinkspeed = 0.15f;
				yield return new WaitForSeconds(0.05f);
			}
			num = i;
		}
		yield return new WaitForSeconds(showtime);
		for (int i = 0; i < roundletter.Length; i = num + 1)
		{
			if (roundletter[i] != null)
			{
				roundletter[i].GetComponent<DialogueAnim>().shrink = true;
				Object.Destroy(roundletter[i].gameObject, 2f);
				yield return new WaitForSeconds(0.05f);
			}
			num = i;
		}
		MainManager.templetter = null;
		yield break;
	}

	// Token: 0x060004B9 RID: 1209 RVA: 0x00030AC2 File Offset: 0x0002ECC2
	public static IEnumerator GradualColor(Renderer obj, Color target, float frametime)
	{
		Color s = obj.material.color;
		float a = 0f;
		do
		{
			obj.material.color = Color.Lerp(s, target, a / frametime);
			a += MainManager.TieFramerate(1f);
			yield return null;
		}
		while (a < frametime + 1f);
		yield break;
	}

	// Token: 0x060004BA RID: 1210 RVA: 0x00030ADF File Offset: 0x0002ECDF
	public static IEnumerator GradualColor(SpriteRenderer obj, Color target, float frametime, bool sprite)
	{
		Color s = obj.color;
		float a = 0f;
		do
		{
			obj.color = Color.Lerp(s, target, a / frametime);
			a += MainManager.TieFramerate(1f);
			yield return null;
		}
		while (a < frametime + 1f);
		yield break;
	}

	// Token: 0x060004BB RID: 1211 RVA: 0x00030AFC File Offset: 0x0002ECFC
	private static void UpdateQuestBoard()
	{
		MainManager.instance.questboardobj.GetChild(5).gameObject.SetActive(MainManager.listtype == 14);
		Transform child = MainManager.instance.questboardobj.GetChild(1);
		Renderer[] componentsInChildren = child.GetComponentsInChildren<Renderer>();
		int num = 0;
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].transform.parent == child)
			{
				if (num == MainManager.listtype - 14)
				{
					componentsInChildren[i].sortingOrder = 5;
				}
				else if (MainManager.listtype - 14 == 0)
				{
					componentsInChildren[i].sortingOrder = -20 - num * 2;
				}
				else
				{
					componentsInChildren[i].sortingOrder = -20 + num * 2;
				}
				Renderer[] componentsInChildren2 = componentsInChildren[i].transform.GetChild(0).GetComponentsInChildren<Renderer>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					componentsInChildren2[j].sortingOrder = componentsInChildren[i].sortingOrder + 1;
				}
				num++;
			}
		}
	}

	// Token: 0x060004BC RID: 1212 RVA: 0x00030BF0 File Offset: 0x0002EDF0
	private void CloseQuestBoard()
	{
		Object.Destroy(this.questboardobj.GetChild(3).gameObject);
		Object.Destroy(this.questboardobj.GetChild(4).gameObject);
		for (int i = 0; i < this.questboardobj.childCount; i++)
		{
			if (this.questboardobj.GetChild(i) != null)
			{
				MainManager.DestroyText(this.questboardobj.GetChild(i));
			}
		}
		ButtonSprite[] componentsInChildren = this.questboardobj.GetComponentsInChildren<ButtonSprite>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].transform.parent = this.questboardobj.GetChild(0);
		}
		DialogueAnim[] componentsInChildren2 = this.questboardobj.GetComponentsInChildren<DialogueAnim>();
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			componentsInChildren2[k].shrink = true;
		}
		Object.Destroy(this.questboardobj.gameObject, 0.1f);
		MainManager.player.actioncooldown = 20f;
		this.minipause = false;
		this.inlist = false;
	}

	// Token: 0x060004BD RID: 1213 RVA: 0x00030CF4 File Offset: 0x0002EEF4
	private void DestroyList()
	{
		this.itemlist.gameObject.AddComponent<DialogueAnim>().shrink = true;
		Object.Destroy(this.itemlist.gameObject, 0.5f);
		this.itemlist = null;
		Object.Destroy(MainManager.instance.cursor.gameObject);
		this.skiptext = false;
		MainManager.listoption = this.option;
		this.option = -1;
		MainManager.listY = -1;
	}

	// Token: 0x060004BE RID: 1214 RVA: 0x00030D68 File Offset: 0x0002EF68
	private void RefreshNumberPrompt()
	{
		MainManager.DestroyText(this.npromptholder);
		MainManager.instance.flagvar[4] = 0;
		string str = this.flagstring[0].PadRight(this.flagvar[10], '_');
		if (MainManager.instance.flagvar[0] == -555)
		{
			if (this.flagstring[MainManager.listtype] == null)
			{
				this.flagstring[MainManager.listtype] = "";
			}
			str = this.flagstring[MainManager.listtype].PadRight(this.flagvar[10], '_');
		}
		base.StartCoroutine(MainManager.SetText("|center|" + str, 0, null, false, false, Vector3.zero, Vector3.zero, Vector2.one, this.npromptholder, null));
	}

	// Token: 0x060004BF RID: 1215 RVA: 0x00030E2E File Offset: 0x0002F02E
	public static float ClampedAngle(float input, float max)
	{
		while (input < 0f)
		{
			input += max;
		}
		while (input > max)
		{
			input = max - input;
		}
		return input;
	}

	// Token: 0x060004C0 RID: 1216 RVA: 0x00030E4B File Offset: 0x0002F04B
	public static MainManager.BattleData GetEnemyData(int id)
	{
		return MainManager.GetEnemyData(id, false, false);
	}

	// Token: 0x060004C1 RID: 1217 RVA: 0x00030E55 File Offset: 0x0002F055
	public static MainManager.BattleData GetEnemyData(int id, bool createentity)
	{
		return MainManager.GetEnemyData(id, createentity, false);
	}

	// Token: 0x060004C2 RID: 1218 RVA: 0x00030E5F File Offset: 0x0002F05F
	public static float GetSoundDistance(Vector3 position)
	{
		return MainManager.GetSoundDistance(position, 25f);
	}

	// Token: 0x060004C3 RID: 1219 RVA: 0x00030E6C File Offset: 0x0002F06C
	public static float GetSoundDistance(Vector3 position, float maxdistance)
	{
		return 1f - Mathf.Clamp(Vector3.Distance(position, MainManager.MainCamera.transform.position), 0f, maxdistance) / maxdistance;
	}

	// Token: 0x060004C4 RID: 1220 RVA: 0x00030E96 File Offset: 0x0002F096
	public static float GetSoundDistance(float distance, float maxdistance)
	{
		return 1f - Mathf.Clamp(distance, 0f, maxdistance) / maxdistance;
	}

	// Token: 0x060004C5 RID: 1221 RVA: 0x00030EAC File Offset: 0x0002F0AC
	public static float GetSoundDistance(float distance)
	{
		return MainManager.GetSoundDistance(distance, 25f);
	}

	// Token: 0x060004C6 RID: 1222 RVA: 0x00030EB9 File Offset: 0x0002F0B9
	public static void PlayMoveSound(EntityControl entity)
	{
		MainManager.PlayMoveSound(entity.originalid, 9, entity.height > 0.1f);
	}

	// Token: 0x060004C7 RID: 1223 RVA: 0x00030ED8 File Offset: 0x0002F0D8
	public static void PlayMoveSound(int animid, int soundid, bool fly)
	{
		string text = null;
		float volume = 1f;
		float pitch = 1f;
		MainManager.AnimIDs animIDs = animid + MainManager.AnimIDs.Bee;
		if (animIDs <= MainManager.AnimIDs.Midge)
		{
			if (animIDs <= MainManager.AnimIDs.Spuder)
			{
				if (animIDs <= MainManager.AnimIDs.Mushroom)
				{
					if (animIDs == MainManager.AnimIDs.CordycepsAnt)
					{
						text = "Scuttle";
						volume = 0.8f;
						pitch = 0.5f;
						goto IL_1BC;
					}
					if (animIDs != MainManager.AnimIDs.Mushroom)
					{
						goto IL_1BC;
					}
					if (fly)
					{
						text = "Fly";
						pitch = 0.5f;
						goto IL_1BC;
					}
					goto IL_1BC;
				}
				else
				{
					if (animIDs == MainManager.AnimIDs.Armorpillar)
					{
						text = "FunnyStep";
						pitch = 0.9f;
						volume = 0.3f;
						goto IL_1BC;
					}
					if (animIDs != MainManager.AnimIDs.Spuder)
					{
						goto IL_1BC;
					}
					text = "Scuttle2";
					volume = 0.8f;
					pitch = 0.9f;
					goto IL_1BC;
				}
			}
			else if (animIDs <= MainManager.AnimIDs.Thief)
			{
				if (animIDs == MainManager.AnimIDs.Seedling)
				{
					goto IL_13E;
				}
				if (animIDs != MainManager.AnimIDs.Thief)
				{
					goto IL_1BC;
				}
				goto IL_155;
			}
			else if (animIDs != MainManager.AnimIDs.FlyTrap)
			{
				if (animIDs != MainManager.AnimIDs.Midge)
				{
					goto IL_1BC;
				}
				goto IL_155;
			}
		}
		else if (animIDs <= MainManager.AnimIDs.Flowering)
		{
			if (animIDs <= MainManager.AnimIDs.Scorpion)
			{
				if (animIDs == MainManager.AnimIDs.Abomihoney)
				{
					text = "PuddleMove";
					pitch = 1.15f;
					goto IL_1BC;
				}
				if (animIDs != MainManager.AnimIDs.Scorpion)
				{
					goto IL_1BC;
				}
				text = "Step";
				goto IL_1BC;
			}
			else if (animIDs != MainManager.AnimIDs.Krawler)
			{
				if (animIDs != MainManager.AnimIDs.Flowering)
				{
					goto IL_1BC;
				}
				goto IL_13E;
			}
		}
		else if (animIDs <= MainManager.AnimIDs.MimicSpider)
		{
			if (animIDs == MainManager.AnimIDs.Plumpling)
			{
				text = "ThumpSoft";
				volume = 0.65f;
				pitch = 0.65f;
				goto IL_1BC;
			}
			if (animIDs != MainManager.AnimIDs.MimicSpider)
			{
				goto IL_1BC;
			}
			text = "Scuttle2";
			volume = 0.8f;
			pitch = 1.1f;
			goto IL_1BC;
		}
		else
		{
			if (animIDs == MainManager.AnimIDs.Mantidfly)
			{
				goto IL_155;
			}
			if (animIDs != MainManager.AnimIDs.DivingSpider)
			{
				goto IL_1BC;
			}
			text = "WetStep";
			goto IL_1BC;
		}
		text = "Scuttle3";
		goto IL_1BC;
		IL_13E:
		if (fly)
		{
			text = "Toss2";
			volume = 0.5f;
			pitch = 1.1f;
			goto IL_1BC;
		}
		goto IL_1BC;
		IL_155:
		if (fly)
		{
			text = "BugWing";
		}
		IL_1BC:
		if (text != null)
		{
			MainManager.PlaySound(text, soundid, pitch, volume, true);
		}
	}

	// Token: 0x060004C8 RID: 1224 RVA: 0x000310B0 File Offset: 0x0002F2B0
	public static MainManager.BattleData GetEnemyData(int id, bool createentity, bool noexp)
	{
		MainManager.BattleData battleData = default(MainManager.BattleData);
		int num = Convert.ToInt32(MainManager.enemydata[id, 25]);
		battleData.eventondeath = Convert.ToInt32(MainManager.enemydata[id, 26]);
		bool flag = false;
		battleData.animid = id;
		if (createentity)
		{
			battleData.battleentity = EntityControl.CreateNewEntity("enemy" + id, 0, new Vector3(0f, -10f));
			battleData.battleentity.height = Convert.ToSingle(MainManager.enemydata[id, 20]);
			battleData.battleentity.bobspeed = Convert.ToSingle(MainManager.enemydata[id, 21]);
			battleData.battleentity.bobrange = Convert.ToSingle(MainManager.enemydata[id, 22]);
			switch (id)
			{
			case 105:
				battleData.battleentity.forcefire = true;
				id = 57;
				if (MainManager.map.mapid == MainManager.Maps.CaveOfTrials && !MainManager.instance.flags[664])
				{
					flag = true;
				}
				break;
			case 106:
				battleData.battleentity.forcefire = true;
				id = 61;
				if (MainManager.map.mapid == MainManager.Maps.CaveOfTrials && !MainManager.instance.flags[664])
				{
					flag = true;
				}
				break;
			case 107:
				battleData.battleentity.forcefire = true;
				id = 58;
				if (MainManager.map.mapid == MainManager.Maps.CaveOfTrials && !MainManager.instance.flags[664])
				{
					flag = true;
				}
				break;
			case 108:
				battleData.battleentity.inice = true;
				id = 57;
				break;
			case 109:
				id = 61;
				battleData.battleentity.inice = true;
				break;
			}
		}
		battleData.moves = Convert.ToInt32(MainManager.enemydata[id, 27]);
		battleData.notaunt = Convert.ToBoolean(MainManager.enemydata[id, 28]);
		battleData.cantfall = Convert.ToBoolean(MainManager.enemydata[id, 29]);
		battleData.notired = Convert.ToBoolean(MainManager.enemydata[id, 31]);
		battleData.fixedexp = Convert.ToBoolean(MainManager.enemydata[id, 30]);
		battleData.position = (BattleControl.BattlePosition)Enum.Parse(typeof(BattleControl.BattlePosition), MainManager.enemydata[id, 19]);
		if (num > -1)
		{
			battleData.animid = num;
			id = num;
		}
		if (MainManager.map.mapid == MainManager.Maps.CaveOfTrials && MainManager.instance.enemyencounter[id, 0] == 0)
		{
			flag = true;
		}
		battleData.entityname = (flag ? MainManager.menutext[59] : MainManager.enemynames[id]);
		battleData.holditem = -1;
		battleData.hp = Convert.ToInt32(MainManager.enemydata[id, 1]);
		battleData.def = Convert.ToInt32(MainManager.enemydata[id, 2]);
		if (MainManager.instance.partylevel < 27 && !MainManager.instance.flags[613])
		{
			battleData.exp = Convert.ToInt32(MainManager.enemydata[id, 3]);
			if (!battleData.fixedexp && !noexp)
			{
				battleData.exp = MainManager.GetEXP(battleData.exp, (MainManager.Enemies)battleData.animid);
			}
			else if (noexp)
			{
				battleData.exp = 0;
			}
		}
		battleData.money = Convert.ToInt32(MainManager.enemydata[id, 4]);
		battleData.cursoroffset = new Vector3(Convert.ToSingle(MainManager.enemydata[id, 5]), Convert.ToSingle(MainManager.enemydata[id, 6]), Convert.ToSingle(MainManager.enemydata[id, 7]));
		battleData.itemoffset = new Vector3(Convert.ToSingle(MainManager.enemydata[id, 39]), Convert.ToSingle(MainManager.enemydata[id, 40]), Convert.ToSingle(MainManager.enemydata[id, 41]));
		battleData.poisonres = Convert.ToInt32(MainManager.enemydata[id, 8]);
		battleData.freezeres = Convert.ToInt32(MainManager.enemydata[id, 9]);
		battleData.numbres = Convert.ToInt32(MainManager.enemydata[id, 10]);
		battleData.sleepres = Convert.ToInt32(MainManager.enemydata[id, 11]);
		battleData.size = Convert.ToSingle(MainManager.enemydata[id, 12]);
		battleData.deathtype = Convert.ToInt32(MainManager.enemydata[id, 33]);
		if (MainManager.enemydata[id, 34] != "-1")
		{
			string[] array = MainManager.enemydata[id, 34].Split(new char[]
			{
				';'
			});
			if (array.Length != 0)
			{
				battleData.chargeonotherenemy = new int[array.Length];
				for (int i = 0; i < battleData.chargeonotherenemy.Length; i++)
				{
					if (array[i].Length > 0)
					{
						battleData.chargeonotherenemy[i] = Convert.ToInt32(array[i]);
					}
					else
					{
						battleData.chargeonotherenemy[i] = -1;
					}
				}
			}
		}
		else
		{
			battleData.chargeonotherenemy = new int[0];
		}
		battleData.hidehp = Convert.ToBoolean(MainManager.enemydata[id, 32]);
		battleData.defenseonhit = Convert.ToInt32(MainManager.enemydata[id, 38]);
		battleData.eventonfall = Convert.ToInt32(MainManager.enemydata[id, 45]);
		battleData.notattle = (flag || Convert.ToBoolean(MainManager.enemydata[id, 44]));
		battleData.onhitaction = Convert.ToInt32(MainManager.enemydata[id, 46]);
		battleData.actimmobile = Convert.ToBoolean(MainManager.enemydata[id, 47]);
		battleData.sizeonfreeze = Convert.ToSingle(MainManager.enemydata[id, 48]);
		if (battleData.sizeonfreeze < 0.1f)
		{
			battleData.sizeonfreeze = battleData.size + 0.25f;
		}
		battleData.initialsize = battleData.size;
		if (MainManager.BadgeIsEquipped(11) || MainManager.instance.flags[166] || MainManager.instance.flags[614])
		{
			battleData.hardatk += Convert.ToInt32(MainManager.enemydata[id, 35]);
			battleData.hp += Convert.ToInt32(MainManager.enemydata[id, 36]);
			battleData.def += Convert.ToInt32(MainManager.enemydata[id, 37]);
			if (MainManager.instance.flags[614])
			{
				battleData.hardatk++;
				battleData.hp = Mathf.CeilToInt((float)battleData.hp * 1.15f);
				if (MainManager.instance.flags[300] && battleData.def >= 0)
				{
					battleData.def++;
				}
			}
			if (MainManager.instance.flags[166] && id != 103 && id != 72 && id != 102 && id != 101)
			{
				battleData.hp = Mathf.CeilToInt((float)battleData.hp * 1.5f);
				if (battleData.hp > 90)
				{
					battleData.hp = Mathf.CeilToInt((float)battleData.hp * 0.85f);
				}
				battleData.hardatk = Mathf.CeilToInt((float)battleData.hardatk * 1.5f);
				battleData.harddef = Mathf.CeilToInt((float)battleData.harddef * 1.5f);
				battleData.def = Mathf.Clamp(battleData.def, 1, 99);
			}
		}
		if (MainManager.instance.flags[162])
		{
			battleData.exp = Mathf.Clamp(Mathf.FloorToInt((float)battleData.exp * 0.2f), 0, 5);
		}
		battleData.maxhp = battleData.hp;
		if (battleData.position == BattleControl.BattlePosition.Random)
		{
			if (id != 1 || MainManager.instance.flags[24])
			{
				battleData.position = (BattleControl.BattlePosition)Random.Range(0, 2);
			}
			else
			{
				battleData.position = BattleControl.BattlePosition.Flying;
			}
		}
		if (createentity)
		{
			battleData.battleentity.animid = Convert.ToInt32(MainManager.enemydata[id, 0]);
			battleData.battleentity.freezesize = new Vector3(Convert.ToSingle(MainManager.enemydata[id, 13]), Convert.ToSingle(MainManager.enemydata[id, 14]), Convert.ToSingle(MainManager.enemydata[id, 15]));
			battleData.battleentity.freezeoffset = new Vector3(Convert.ToSingle(MainManager.enemydata[id, 16]), Convert.ToSingle(MainManager.enemydata[id, 17]), Convert.ToSingle(MainManager.enemydata[id, 18]));
			battleData.battleentity.overrridejump = true;
			battleData.battleentity.battle = true;
			battleData.battleentity.alwaysactive = true;
			battleData.battleentity.onground = true;
			if (flag || battleData.animid == 110)
			{
				battleData.battleentity.name = battleData.battleentity.name.Insert(0, "COT");
				battleData.battleentity.hologram = true;
				battleData.battleentity.cotunknown = true;
				battleData.battleentity.Invoke("RefreshCOT", 0.1f);
			}
			battleData.battleentity.tag = "Enemy";
			battleData.battleentity.gameObject.layer = 9;
			battleData.entity = battleData.battleentity;
			battleData.battleentity.height = ((battleData.position == BattleControl.BattlePosition.Flying && battleData.battleentity.height < 2f) ? 2f : battleData.battleentity.height);
			battleData.battleentity.initialheight = battleData.battleentity.height;
			battleData.battleentity.CreateHPBar();
			battleData.battleentity.emoticonoffset = battleData.cursoroffset;
			if (battleData.position == BattleControl.BattlePosition.Underground)
			{
				battleData.battleentity.InstantDig();
				battleData.battleentity.height = 0f;
				battleData.battleentity.initialheight = 0f;
			}
			if (!MainManager.instance.flags[166] && battleData.battleentity.hologram)
			{
				battleData.exp = Mathf.FloorToInt((float)battleData.exp * 0.1f);
			}
			if (Convert.ToBoolean(MainManager.enemydata[id, 42]))
			{
				battleData.battleentity.basestate = 13;
			}
		}
		if (battleData.entity != null)
		{
			switch (battleData.deathtype)
			{
			case 0:
			case 3:
				battleData.entity.destroytype = NPCControl.DeathType.SpinSmoke;
				break;
			case 1:
				battleData.entity.destroytype = NPCControl.DeathType.SpinNoSmoke;
				break;
			case 2:
			case 4:
				battleData.entity.destroytype = NPCControl.DeathType.KO;
				break;
			case 5:
				battleData.entity.destroytype = NPCControl.DeathType.SpinKO;
				break;
			case 6:
				battleData.entity.destroytype = NPCControl.DeathType.Shrink;
				break;
			case 7:
				battleData.entity.destroytype = NPCControl.DeathType.ShrinkNoSmoke;
				break;
			case 8:
				battleData.entity.destroytype = NPCControl.DeathType.None;
				break;
			case 9:
				battleData.entity.destroytype = NPCControl.DeathType.Sink;
				break;
			case 10:
				battleData.entity.destroytype = NPCControl.DeathType.ExplodeAnim;
				break;
			case 11:
				battleData.entity.destroytype = NPCControl.DeathType.DropSprites;
				break;
			}
		}
		battleData.condition = new List<int[]>();
		battleData.weakness = new List<BattleControl.AttackProperty>();
		string[] array2 = MainManager.enemydata[id, 23].Split(new char[]
		{
			'{'
		});
		if (Convert.ToInt32(array2[0]) > 0)
		{
			for (int j = 1; j < array2.Length; j++)
			{
				if (array2[j] != "")
				{
					battleData.weakness.Add((BattleControl.AttackProperty)Enum.Parse(typeof(BattleControl.AttackProperty), array2[j]));
				}
			}
		}
		battleData.weight = Convert.ToSingle(MainManager.enemydata[id, 24]);
		battleData.cantmove = -battleData.moves + 1;
		return battleData;
	}

	// Token: 0x060004C9 RID: 1225 RVA: 0x00031CC3 File Offset: 0x0002FEC3
	public static IEnumerator ForceFailsafe(Transform obj, Vector3 target, float cooldown)
	{
		while (cooldown > 0f)
		{
			cooldown -= MainManager.framestep;
			yield return null;
		}
		if (MainManager.GetDistance(obj.position, target) > 0.45000002f)
		{
			MainManager.DeathSmoke(obj.position);
			obj.position = target;
			MainManager.DeathSmoke(target);
		}
		yield break;
	}

	// Token: 0x060004CA RID: 1226 RVA: 0x00031CE0 File Offset: 0x0002FEE0
	public static int GetEXP(int input)
	{
		return MainManager.GetEXP(input, MainManager.instance.partylevel, null);
	}

	// Token: 0x060004CB RID: 1227 RVA: 0x00031D08 File Offset: 0x0002FF08
	public static int GetEXP(int input, int level)
	{
		return MainManager.GetEXP(input, level, null);
	}

	// Token: 0x060004CC RID: 1228 RVA: 0x00031D25 File Offset: 0x0002FF25
	public static int GetEXP(int input, MainManager.Enemies enemy)
	{
		return MainManager.GetEXP(input, MainManager.instance.partylevel, new MainManager.Enemies?(enemy));
	}

	// Token: 0x060004CD RID: 1229 RVA: 0x00031D40 File Offset: 0x0002FF40
	public static int GetEXP(int input, int lv, MainManager.Enemies? enemy)
	{
		float num = (MainManager.map != null) ? MainManager.map.expmulti : 1f;
		if (enemy != null && (enemy.Value == MainManager.Enemies.WaspTrooper || enemy.Value == MainManager.Enemies.WaspHealer))
		{
			if (MainManager.CurrentMap() == MainManager.Maps.MetalLake || MainManager.instance.areaid == 19)
			{
				input = Mathf.CeilToInt((float)input * 1.65f);
			}
			else if (MainManager.instance.areaid == 14)
			{
				input = Mathf.CeilToInt((float)input * 2.5f);
				if (enemy.Value == MainManager.Enemies.WaspHealer)
				{
					input += 10;
				}
			}
		}
		return Mathf.FloorToInt(Mathf.Clamp((float)input * num - ((lv > 1) ? ((float)(lv - 1) * 2.5f) : 0f), 1f, 99f)) - 1;
	}

	// Token: 0x060004CE RID: 1230 RVA: 0x00031E14 File Offset: 0x00030014
	public static void PlayBuzzer()
	{
		MainManager.PlaySound("Buzzer", 10);
	}

	// Token: 0x060004CF RID: 1231 RVA: 0x00031E23 File Offset: 0x00030023
	public static void Heal(bool noparticle, bool nosound)
	{
		MainManager.Heal(new MainManager.Healing[1], MainManager.instance.partyorder, noparticle, nosound);
	}

	// Token: 0x060004D0 RID: 1232 RVA: 0x00031E3C File Offset: 0x0003003C
	public static void Heal()
	{
		MainManager.Heal(new MainManager.Healing[1], MainManager.instance.partyorder, false, false);
	}

	// Token: 0x060004D1 RID: 1233 RVA: 0x00031E58 File Offset: 0x00030058
	public static void Heal(MainManager.Healing[] parameters, int[] partyids, bool noparticle, bool nosound)
	{
		if (!nosound)
		{
			MainManager.PlaySound("Heal");
		}
		MainManager.instance.hudcooldown = 100f;
		for (int i = 0; i < parameters.Length; i++)
		{
			for (int j = 0; j < partyids.Length; j++)
			{
				if (parameters[i] == MainManager.Healing.Full)
				{
					MainManager.instance.playerdata[j].hp = MainManager.instance.playerdata[j].maxhp;
					MainManager.instance.tp = MainManager.instance.maxtp;
				}
				else if (parameters[i] == MainManager.Healing.TPOnly)
				{
					MainManager.instance.tp = MainManager.instance.maxtp;
				}
				else if (parameters[i] == MainManager.Healing.FullHPOnly)
				{
					MainManager.instance.playerdata[j].hp = MainManager.instance.playerdata[j].maxhp;
				}
				if (parameters[i] != MainManager.Healing.TPOnly && !noparticle)
				{
					if (MainManager.battle)
					{
						MainManager.HealParticle(MainManager.instance.playerdata[j].battleentity.transform, Vector3.one, Vector3.up);
					}
					else
					{
						MainManager.HealParticle(MainManager.instance.playerdata[j].entity.transform, Vector3.one, Vector3.up);
					}
				}
			}
		}
	}

	// Token: 0x060004D2 RID: 1234 RVA: 0x00031FA4 File Offset: 0x000301A4
	public static int ConditionTurns(int playerid, int turns)
	{
		return turns;
	}

	// Token: 0x060004D3 RID: 1235 RVA: 0x00031FA8 File Offset: 0x000301A8
	public static bool HasWeakness(BattleControl.AttackProperty property, MainManager.BattleData entity)
	{
		BattleControl.AttackProperty[] array = entity.weakness.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == property)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060004D4 RID: 1236 RVA: 0x00031FD8 File Offset: 0x000301D8
	public static void SetCondition(MainManager.BattleCondition condition, ref MainManager.BattleData entity, int turns, int fromplayer)
	{
		if (condition == MainManager.BattleCondition.Shield)
		{
			entity.battleentity.shieldenabled = true;
		}
		for (int i = 0; i < entity.condition.Count; i++)
		{
			if (entity.condition[i][0] == (int)condition)
			{
				if (turns > 0 || MainManager.ConditionChance(condition, entity))
				{
					switch (condition)
					{
					case MainManager.BattleCondition.Freeze:
					case MainManager.BattleCondition.Numb:
					case MainManager.BattleCondition.Sleep:
						if (!entity.battleentity.CompareTag("Player"))
						{
							if (entity.isdefending)
							{
								entity.isdefending = false;
								goto IL_162;
							}
							goto IL_162;
						}
						else if (!(MainManager.battle != null) || MainManager.battle.currentchoice != BattleControl.Actions.Item)
						{
							if (entity.condition[i][1] < Mathf.Abs(turns))
							{
								goto IL_162;
							}
							goto IL_176;
						}
						break;
					case MainManager.BattleCondition.Poison:
						if (entity.battleentity.CompareTag("Player") && MainManager.BadgeIsEquipped(27, entity.trueid))
						{
							turns = 99999;
							goto IL_162;
						}
						break;
					case MainManager.BattleCondition.AttackUp:
					case MainManager.BattleCondition.DefenseUp:
					case MainManager.BattleCondition.AttackDown:
					case MainManager.BattleCondition.DefenseDown:
					case MainManager.BattleCondition.GradualHP:
					case MainManager.BattleCondition.GradualTP:
						break;
					case MainManager.BattleCondition.Topple:
					case MainManager.BattleCondition.Flipped:
					case MainManager.BattleCondition.Shield:
					case MainManager.BattleCondition.Taunted:
					case MainManager.BattleCondition.Sturdy:
					case MainManager.BattleCondition.Eaten:
					case MainManager.BattleCondition.EventStop:
						goto IL_162;
					case MainManager.BattleCondition.Fire:
						entity.condition[i][1] += Mathf.CeilToInt(Mathf.Abs((float)turns) / 2f);
						goto IL_176;
					default:
						goto IL_162;
					}
					entity.condition[i][1] += Mathf.Abs(turns);
					goto IL_176;
					IL_162:
					entity.condition[i][1] = Mathf.Abs(turns);
				}
				IL_176:
				entity.condition[i][1] = Mathf.Clamp(entity.condition[i][1], 0, 999999);
				MainManager.FixCondition(entity);
				return;
			}
		}
		switch (condition)
		{
		case MainManager.BattleCondition.Freeze:
		case MainManager.BattleCondition.Numb:
		case MainManager.BattleCondition.Sleep:
			if (!entity.battleentity.CompareTag("Player") && entity.isdefending)
			{
				entity.isdefending = false;
			}
			break;
		case MainManager.BattleCondition.Poison:
			if (entity.battleentity.CompareTag("Player") && MainManager.BadgeIsEquipped(27, entity.trueid))
			{
				turns = 99999;
			}
			break;
		}
		entity.condition.Add(new int[]
		{
			(int)condition,
			turns
		});
		MainManager.FixCondition(entity);
	}

	// Token: 0x060004D5 RID: 1237 RVA: 0x0003222C File Offset: 0x0003042C
	private static void FixCondition(MainManager.BattleData entity)
	{
		int[][] array = entity.condition.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			int num = entity.condition[i][1];
			if (num < 0)
			{
				entity.condition[i][1] = Mathf.Abs(num);
			}
			MainManager.BattleCondition battleCondition = (MainManager.BattleCondition)array[i][0];
			if (battleCondition != MainManager.BattleCondition.Numb)
			{
				if (battleCondition == MainManager.BattleCondition.Shield && entity.battleentity.CompareTag("Player"))
				{
					entity.condition[i][1] = 1;
				}
			}
			else
			{
				entity.isnumb = true;
			}
		}
	}

	// Token: 0x060004D6 RID: 1238 RVA: 0x000322B4 File Offset: 0x000304B4
	public static void SetCondition(MainManager.BattleCondition condition, ref MainManager.BattleData entity, int turns)
	{
		MainManager.SetCondition(condition, ref entity, turns, -1);
	}

	// Token: 0x060004D7 RID: 1239 RVA: 0x000322C0 File Offset: 0x000304C0
	public static bool ConditionChance(MainManager.BattleCondition condition, MainManager.BattleData entity)
	{
		float num = Random.Range(0f, 100f);
		switch (condition)
		{
		case MainManager.BattleCondition.Freeze:
			return num > (float)entity.freezeres;
		case MainManager.BattleCondition.Poison:
			return num > (float)entity.poisonres;
		case MainManager.BattleCondition.Numb:
			return num > (float)entity.numbres;
		case MainManager.BattleCondition.Sleep:
			return num > (float)entity.sleepres;
		default:
			return true;
		}
	}

	// Token: 0x060004D8 RID: 1240 RVA: 0x00032322 File Offset: 0x00030522
	public static int HasCondition(MainManager.BattleData entity)
	{
		if (MainManager.HasCondition(MainManager.BattleCondition.Freeze, entity) > 0 || MainManager.HasCondition(MainManager.BattleCondition.Numb, entity) > 0 || MainManager.HasCondition(MainManager.BattleCondition.Sleep, entity) > 0)
		{
			return 1;
		}
		return -1;
	}

	// Token: 0x060004D9 RID: 1241 RVA: 0x00032348 File Offset: 0x00030548
	public static int HasCondition(MainManager.BattleCondition condition, MainManager.BattleData entity)
	{
		for (int i = 0; i < entity.condition.Count; i++)
		{
			if (entity.condition[i][0] == (int)condition)
			{
				if (condition == MainManager.BattleCondition.Sleep)
				{
					entity.isasleep = true;
				}
				return entity.condition[i][1];
			}
		}
		return -1;
	}

	// Token: 0x060004DA RID: 1242 RVA: 0x00032398 File Offset: 0x00030598
	public static void RemoveCondition(MainManager.BattleCondition condition, MainManager.BattleData entity)
	{
		int num = -1;
		for (int i = 0; i < entity.condition.Count; i++)
		{
			if (entity.condition[i][0] == (int)condition)
			{
				if (condition != MainManager.BattleCondition.Freeze)
				{
					if (condition - MainManager.BattleCondition.Numb <= 1)
					{
						entity.battleentity.RefreshCondition();
					}
				}
				else if (entity.entity != null)
				{
					entity.entity.BreakIce();
				}
				num = i;
				break;
			}
		}
		if (num > -1)
		{
			entity.condition.RemoveAt(num);
		}
	}

	// Token: 0x060004DB RID: 1243 RVA: 0x00032411 File Offset: 0x00030611
	public static SpriteRenderer GetTransitionSprite()
	{
		return MainManager.GetTransitionSprite(0);
	}

	// Token: 0x060004DC RID: 1244 RVA: 0x00032419 File Offset: 0x00030619
	public static SpriteRenderer GetTransitionSprite(int id)
	{
		return MainManager.instance.transitionobj[id].GetComponent<SpriteRenderer>();
	}

	// Token: 0x060004DD RID: 1245 RVA: 0x0003242C File Offset: 0x0003062C
	public static void PlayScrollSound()
	{
		if (!MainManager.sounds[10].isPlaying)
		{
			MainManager.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/Scroll"), 10, 1f, 0.65f);
			if (MainManager.pausemenu != null && MainManager.pausemenu.windowid >= 4)
			{
				MainManager.sounds[10].volume = MainManager.pausemenu.svolume;
			}
		}
	}

	// Token: 0x060004DE RID: 1246 RVA: 0x00032495 File Offset: 0x00030695
	public void UpdateList(bool up)
	{
		this.UpdateList(up, 1);
	}

	// Token: 0x060004DF RID: 1247 RVA: 0x0003249F File Offset: 0x0003069F
	public void UpdateList(bool up, int skip)
	{
		this.UpdateList(up, skip, false);
	}

	// Token: 0x060004E0 RID: 1248 RVA: 0x000324AC File Offset: 0x000306AC
	public void UpdateList(bool up, int skip, bool nosound)
	{
		if (up)
		{
			if (this.option == 0)
			{
				for (int i = 0; i < this.maxoptions; i++)
				{
					this.UpdateList(MainManager.Directions.Down, nosound);
					MainManager.listY = -1;
				}
				return;
			}
			for (int j = 0; j < skip; j++)
			{
				this.UpdateList(MainManager.Directions.Up, nosound);
			}
			return;
		}
		else
		{
			if (this.option == this.maxoptions - 1)
			{
				for (int k = 0; k < this.maxoptions; k++)
				{
					this.UpdateList(MainManager.Directions.Up, nosound);
				}
				MainManager.listY = -1;
				return;
			}
			for (int l = 0; l < skip; l++)
			{
				this.UpdateList(MainManager.Directions.Down, nosound);
			}
			return;
		}
	}

	// Token: 0x060004E1 RID: 1249 RVA: 0x00032540 File Offset: 0x00030740
	public static void ReadSettings(string[] c)
	{
		int i;
		for (i = 0; i < InputIO.keys.Length; i++)
		{
			InputIO.keys[i] = (KeyCode)Enum.Parse(typeof(KeyCode), c[i]);
		}
		MainManager.resolutionindex = Convert.ToInt32(c[i]);
		i++;
		MainManager.fullscreen = Convert.ToBoolean(c[i]);
		i++;
		MainManager.fps = Convert.ToInt32(c[i]);
		i++;
		MainManager.lowshadows = Convert.ToBoolean(c[i]);
		i++;
		MainManager.lowtexture = Convert.ToBoolean(c[i]);
		i++;
		MainManager.musicvolume = Convert.ToSingle(c[i]);
		i++;
		MainManager.soundvolume = Convert.ToSingle(c[i]);
		i++;
		MainManager.MainCamera.GetComponent<FXAA>().enabled = Convert.ToBoolean(c[i]);
		i++;
		MainManager.languageid = Convert.ToInt32(c[i]);
		i++;
		MainManager.nowindeffect = Convert.ToBoolean(c[i]);
		i++;
		MainManager.enableoutline = Convert.ToInt32(c[i]);
		i++;
		MainManager.downsample = Convert.ToInt32(c[i]);
		i++;
		MainManager.particlelevel = Convert.ToInt32(c[i]);
		i++;
		string a = c[i];
		if (!(a == "True"))
		{
			if (a == "False")
			{
				c[i] = "0";
			}
		}
		else
		{
			c[i] = "1";
		}
		MainManager.usejoystick = Convert.ToInt32(c[i]);
		i++;
		MainManager.bleepvolume = Convert.ToSingle(c[i]);
		i++;
		MainManager.vsync = Convert.ToInt32(c[i]);
		i++;
		if (MainManager.usejoystick == 5 || MainManager.usejoystick == 4 || Convert.ToInt32(c[i]) < c.Length - 1)
		{
			MainManager.forcejoystick = Convert.ToInt32(c[i]);
		}
		i++;
		MainManager.keepmusicafterbattle = Convert.ToBoolean(c[i]);
		i++;
		MainManager.mashcommandalt = Convert.ToBoolean(c[i]);
		i++;
		if (i < c.Length)
		{
			MainManager.joybinds = new int[10];
			for (int j = 0; j < 10; j++)
			{
				MainManager.joybinds[j] = Convert.ToInt32(c[i + j]);
			}
			i += 10;
		}
		if (i < c.Length && c[i].Length > 0)
		{
			MainManager.monoaudio = Convert.ToBoolean(c[i]);
		}
		i++;
		if (i < c.Length && c[i].Length > 0)
		{
			string[] array = c[i].Split(new char[]
			{
				','
			});
			for (int k = 0; k < MainManager.secretunlocks.Length; k++)
			{
				MainManager.secretunlocks[k] = Convert.ToBoolean(array[k]);
			}
			i++;
		}
		if (i < c.Length && c[i].Length > 0)
		{
			MainManager.analog = Convert.ToInt32(c[i]);
		}
		i++;
		if (i < c.Length && c[i].Length > 0)
		{
			MainManager.pauseonfocus = Convert.ToBoolean(c[i]);
		}
		i++;
		if (i < c.Length && c[i].Length > 0)
		{
			MainManager.snapTo8 = Convert.ToBoolean(c[i]);
		}
		i++;
	}

	// Token: 0x060004E2 RID: 1250 RVA: 0x0003282C File Offset: 0x00030A2C
	public static string SaveSettings()
	{
		string text = "";
		for (int i = 0; i < InputIO.keys.Length; i++)
		{
			text = text + InputIO.keys[i].ToString() + "\n";
		}
		text = string.Concat(new object[]
		{
			text,
			MainManager.resolutionindex,
			"\n",
			MainManager.fullscreen.ToString(),
			"\n",
			MainManager.fps,
			"\n",
			MainManager.lowshadows.ToString(),
			"\n",
			MainManager.lowtexture.ToString(),
			"\n",
			MainManager.musicvolume,
			"\n",
			MainManager.soundvolume,
			"\n",
			MainManager.MainCamera.GetComponent<FXAA>().enabled.ToString(),
			"\n",
			MainManager.languageid,
			"\n",
			MainManager.nowindeffect.ToString(),
			"\n",
			MainManager.enableoutline,
			"\n",
			MainManager.downsample,
			"\n",
			MainManager.particlelevel,
			"\n",
			MainManager.usejoystick,
			"\n",
			MainManager.bleepvolume,
			"\n",
			MainManager.vsync,
			"\n",
			MainManager.forcejoystick,
			"\n",
			MainManager.keepmusicafterbattle.ToString(),
			"\n",
			MainManager.mashcommandalt.ToString(),
			"\n"
		});
		for (int j = 0; j < 10; j++)
		{
			text = text + MainManager.joybinds[j] + "\n";
		}
		return string.Concat(new object[]
		{
			text,
			MainManager.monoaudio.ToString(),
			"\n",
			MainManager.secretunlocks[0].ToString(),
			",",
			MainManager.secretunlocks[1].ToString(),
			",",
			MainManager.secretunlocks[2].ToString(),
			",",
			MainManager.secretunlocks[3].ToString(),
			",",
			MainManager.secretunlocks[4].ToString(),
			"\n",
			MainManager.analog,
			"\n",
			MainManager.pauseonfocus.ToString(),
			"\n",
			MainManager.snapTo8.ToString()
		});
	}

	// Token: 0x060004E3 RID: 1251 RVA: 0x00032B54 File Offset: 0x00030D54
	public static string SaveFile(Vector3? savepos)
	{
		string text = "";
		if (savepos != null)
		{
			text = string.Concat(new object[]
			{
				text,
				savepos.Value.x,
				",",
				savepos.Value.y,
				",",
				savepos.Value.z,
				","
			});
		}
		else
		{
			text = string.Concat(new object[]
			{
				text,
				MainManager.player.transform.position.x,
				",",
				MainManager.player.transform.position.y,
				",",
				MainManager.player.transform.position.z,
				","
			});
		}
		text = string.Concat(new string[]
		{
			text,
			MainManager.instance.flags[613].ToString(),
			",",
			MainManager.instance.flags[614].ToString(),
			",",
			MainManager.instance.flags[615].ToString(),
			",",
			MainManager.instance.flags[616].ToString(),
			",",
			MainManager.instance.flags[656].ToString(),
			",",
			MainManager.instance.flags[681].ToString(),
			",",
			MainManager.instance.flagstring[10],
			"\n"
		});
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			text = string.Concat(new object[]
			{
				text,
				MainManager.instance.playerdata[i].trueid,
				",",
				MainManager.instance.playerdata[i].hp,
				",",
				MainManager.instance.playerdata[i].maxhp,
				",",
				MainManager.instance.playerdata[i].basehp,
				",",
				MainManager.instance.playerdata[i].atk,
				",",
				MainManager.instance.playerdata[i].baseatk,
				",",
				MainManager.instance.playerdata[i].def,
				",",
				MainManager.instance.playerdata[i].basedef
			});
			if (i < MainManager.instance.playerdata.Length - 1)
			{
				text += "@";
			}
		}
		text += "\n";
		text = string.Concat(new object[]
		{
			text,
			MainManager.instance.partylevel,
			",",
			MainManager.instance.partyexp,
			",",
			MainManager.instance.neededexp,
			",",
			MainManager.instance.basetp,
			",",
			MainManager.instance.tp,
			",",
			MainManager.instance.money,
			",",
			MainManager.map.name,
			",",
			MainManager.instance.areaid,
			",",
			MainManager.instance.bp,
			",",
			MainManager.instance.maxbp,
			",",
			MainManager.instance.maxitems,
			",",
			MainManager.instance.maxstorage,
			",",
			MainManager.instance.clockhour,
			",",
			MainManager.instance.clockmin,
			",",
			MainManager.instance.clocksec,
			",",
			MainManager.SaveProgressIcons(),
			"\n"
		});
		for (int j = 0; j < MainManager.instance.avaliablebadgepool.Length; j++)
		{
			int[] array = MainManager.instance.avaliablebadgepool[j].ToArray();
			for (int k = 0; k < array.Length; k++)
			{
				text += array[k];
				if (k < array.Length - 1)
				{
					text += ",";
				}
			}
			if (j < MainManager.instance.avaliablebadgepool.Length - 1)
			{
				text += "@";
			}
		}
		text += "\n";
		for (int l = 0; l < MainManager.instance.badgeshops.Length; l++)
		{
			int[] array2 = MainManager.instance.badgeshops[l].ToArray();
			for (int m = 0; m < array2.Length; m++)
			{
				text += array2[m];
				if (m < array2.Length - 1)
				{
					text += ",";
				}
			}
			if (l < MainManager.instance.badgeshops.Length - 1)
			{
				text += "@";
			}
		}
		text += "\n";
		for (int n = 0; n < MainManager.instance.boardquests.Length; n++)
		{
			int[] array3 = MainManager.instance.boardquests[n].ToArray();
			for (int num = 0; num < array3.Length; num++)
			{
				text += array3[num];
				if (num < array3.Length - 1)
				{
					text += ",";
				}
			}
			if (n < MainManager.instance.boardquests.Length - 1)
			{
				text += "@";
			}
		}
		text += "\n";
		for (int num2 = 0; num2 < MainManager.instance.items.Length; num2++)
		{
			int[] array4 = MainManager.instance.items[num2].ToArray();
			for (int num3 = 0; num3 < array4.Length; num3++)
			{
				text += array4[num3];
				if (num3 < array4.Length - 1)
				{
					text += ",";
				}
			}
			if (num2 < MainManager.instance.items.Length - 1)
			{
				text += "@";
			}
		}
		text += "\n";
		for (int num4 = 0; num4 < MainManager.instance.badges.Count; num4++)
		{
			int[] array5 = MainManager.instance.badges[num4];
			for (int num5 = 0; num5 < array5.Length; num5++)
			{
				text += array5[num5];
				if (num5 < array5.Length - 1)
				{
					text += ",";
				}
			}
			if (num4 < MainManager.instance.badges.Count - 1)
			{
				text += "@";
			}
		}
		text += "\n";
		for (int num6 = 0; num6 < MainManager.instance.samiramusics.Count; num6++)
		{
			int[] array6 = MainManager.instance.samiramusics[num6];
			for (int num7 = 0; num7 < array6.Length; num7++)
			{
				text += array6[num7];
				if (num7 < array6.Length - 1)
				{
					text += ",";
				}
			}
			if (num6 < MainManager.instance.samiramusics.Count - 1)
			{
				text += "@";
			}
		}
		text += "\n";
		for (int num8 = 0; num8 < MainManager.instance.statbonus.Count; num8++)
		{
			int[] array7 = MainManager.instance.statbonus[num8];
			for (int num9 = 0; num9 < array7.Length; num9++)
			{
				text += array7[num9];
				if (num9 < array7.Length - 1)
				{
					text += ",";
				}
			}
			if (num8 < MainManager.instance.statbonus.Count - 1)
			{
				text += "@";
			}
		}
		text += "\n";
		for (int num10 = 0; num10 < MainManager.instance.librarystuff.GetLength(0); num10++)
		{
			for (int num11 = 0; num11 < MainManager.instance.librarystuff.GetLength(1); num11++)
			{
				text += MainManager.instance.librarystuff[num10, num11].ToString();
				if (num11 < MainManager.instance.librarystuff.GetLength(1) - 1)
				{
					text += ",";
				}
			}
			if (num10 < MainManager.instance.librarystuff.GetLength(0) - 1)
			{
				text += "@";
			}
		}
		text += "\n";
		for (int num12 = 0; num12 < MainManager.instance.flags.Length; num12++)
		{
			text += MainManager.instance.flags[num12].ToString();
			if (num12 < MainManager.instance.flags.Length - 1)
			{
				text += ",";
			}
		}
		text += "\n";
		for (int num13 = 0; num13 < MainManager.instance.flagstring.Length; num13++)
		{
			text += MainManager.instance.flagstring[num13];
			if (num13 < MainManager.instance.flagstring.Length - 1)
			{
				text += "|SPLIT|";
			}
		}
		text += "\n";
		for (int num14 = 0; num14 < MainManager.instance.flagvar.Length; num14++)
		{
			text += MainManager.instance.flagvar[num14];
			if (num14 < MainManager.instance.flagvar.Length - 1)
			{
				text += ",";
			}
		}
		text += "\n";
		for (int num15 = 0; num15 < MainManager.instance.regionalflags.Length; num15++)
		{
			text += MainManager.instance.regionalflags[num15].ToString();
			if (num15 < MainManager.instance.regionalflags.Length - 1)
			{
				text += ",";
			}
		}
		text += "\n";
		for (int num16 = 0; num16 < MainManager.instance.crystalbflags.Length; num16++)
		{
			text += MainManager.instance.crystalbflags[num16].ToString();
			if (num16 < MainManager.instance.crystalbflags.Length - 1)
			{
				text += ",";
			}
		}
		text += "\n";
		for (int num17 = 0; num17 < MainManager.instance.extrafollowers.Count; num17++)
		{
			text += MainManager.instance.extrafollowers[num17];
			if (num17 < MainManager.instance.extrafollowers.Count - 1)
			{
				text += ",";
			}
		}
		text += "\n";
		for (int num18 = 0; num18 < MainManager.instance.enemyencounter.GetLength(0); num18++)
		{
			text = string.Concat(new object[]
			{
				text,
				MainManager.instance.enemyencounter[num18, 0],
				",",
				MainManager.instance.enemyencounter[num18, 1]
			});
			if (num18 < MainManager.instance.enemyencounter.GetLength(0) - 1)
			{
				text += "@";
			}
		}
		return text;
	}

	// Token: 0x060004E4 RID: 1252 RVA: 0x0003383B File Offset: 0x00031A3B
	public void UpdateList(MainManager.Directions dir)
	{
		this.UpdateList(dir, false);
	}

	// Token: 0x060004E5 RID: 1253 RVA: 0x00033848 File Offset: 0x00031A48
	public void UpdateList(MainManager.Directions dir, bool nosound)
	{
		if (dir != MainManager.Directions.Up)
		{
			if (dir == MainManager.Directions.Down && this.option + 1 < this.maxoptions)
			{
				if (!nosound)
				{
					MainManager.PlayScrollSound();
				}
				this.option++;
				if (MainManager.listmax + 1 <= this.maxoptions && this.option == MainManager.listmax)
				{
					MainManager.listmax++;
					MainManager.listlow++;
					return;
				}
				MainManager.listcursor++;
				return;
			}
		}
		else if (this.option - 1 >= 0)
		{
			if (!nosound)
			{
				MainManager.PlayScrollSound();
			}
			this.option--;
			if (MainManager.listlow - 1 >= 0 && this.option == MainManager.listlow - 1)
			{
				MainManager.listmax--;
				MainManager.listlow--;
				return;
			}
			MainManager.listcursor--;
		}
	}

	// Token: 0x060004E6 RID: 1254 RVA: 0x00033929 File Offset: 0x00031B29
	public static bool Interval(float value, float min, float max, bool inclusive)
	{
		if (inclusive)
		{
			return value <= max && value >= min;
		}
		return value < max && value > min;
	}

	// Token: 0x060004E7 RID: 1255 RVA: 0x00033948 File Offset: 0x00031B48
	private void RefreshDiscovery()
	{
		if (this.discoverymessage == null)
		{
			this.discoverymessage = MainManager.NewUIObject("Discovery", MainManager.GUICamera.transform, new Vector3(-8f, -6f)).transform;
			GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Objects/logbookicon")) as GameObject;
			gameObject.transform.parent = this.discoverymessage;
			gameObject.transform.localPosition = new Vector3(0f, -1f, 10f);
			MainManager.instance.StartCoroutine(MainManager.SetText("|single||color,4||dropshadow,1,-1|" + MainManager.menutext[102], 1, null, false, false, new Vector3(1.2f, -1f, 10f), Vector3.zero, Vector2.one, this.discoverymessage, null));
			MainManager.instance.StartCoroutine(MainManager.SetText("|single||color,4||dropshadow,1,-1|" + MainManager.menutext[154], 1, null, false, false, new Vector3(1.2f, -1f, 10f), Vector3.zero, Vector2.one, this.discoverymessage, null));
			MainManager.instance.StartCoroutine(MainManager.SetText("|single||color,4||dropshadow,1,-1|" + MainManager.menutext[155], 1, null, false, false, new Vector3(1.2f, -1f, 10f), Vector3.zero, Vector2.one, this.discoverymessage, null));
			this.discoverymessage.GetChild(2).gameObject.SetActive(false);
			this.discoverymessage.GetChild(3).gameObject.SetActive(false);
		}
		if (this.discoveryhud > 0f)
		{
			this.discoverymessage.localPosition = Vector3.Lerp(this.discoverymessage.localPosition, new Vector3(-8f, -3f), MainManager.TieFramerate(0.1f));
			return;
		}
		this.discoverymessage.localPosition = Vector3.Lerp(this.discoverymessage.localPosition, new Vector3(-8f, -6f), MainManager.TieFramerate(0.1f));
	}

	// Token: 0x060004E8 RID: 1256 RVA: 0x00033B7C File Offset: 0x00031D7C
	public static void UpdateOutlines()
	{
		if (MainManager.map != null && MainManager.map.areaid == MainManager.Areas.MetalLake)
		{
			if (MainManager.enableoutline == 0)
			{
				Shader.SetGlobalFloat("GlobalOutline", 0f);
				return;
			}
			Shader.SetGlobalFloat("GlobalOutline", 0.1f);
			return;
		}
		else
		{
			switch (MainManager.enableoutline)
			{
			case 0:
				Shader.SetGlobalFloat("GlobalOutline", 0f);
				return;
			case 1:
				Shader.SetGlobalFloat("GlobalOutline", 0.1f);
				return;
			case 2:
				Shader.SetGlobalFloat("GlobalOutline", 0.3f);
				return;
			default:
				return;
			}
		}
	}

	// Token: 0x060004E9 RID: 1257 RVA: 0x00033C14 File Offset: 0x00031E14
	private void LateUpdate()
	{
		MainManager.messagebreak = ((MainManager.languageid <= 0) ? 9.75f : 10.5f);
		MainManager.itemdescbreak = ((MainManager.languageid > 0) ? 9.9f : 10.5f);
		MainManager.framestep = MainManager.TieFramerate(1f);
		if (MainManager.basicload)
		{
			MainManager.UpdateOutlines();
			MainManager.GUICamera.transform.localEulerAngles = Vector3.zero;
			if (this.globalcamdir != null)
			{
				this.globalcamdir.transform.eulerAngles = new Vector3(0f, MainManager.MainCamera.transform.eulerAngles.y, 0f);
			}
			if (this.globalcooldown > 0f)
			{
				this.globalcooldown -= MainManager.TieFramerate(1f);
			}
			if (MainManager.instance.playerdata != null && MainManager.instance.playerdata.Length != 0)
			{
				MainManager.RefreshHUD();
				this.RefreshDiscovery();
			}
			if (this.texttail != null)
			{
				if (this.tailtarget != null)
				{
					this.texttail.gameObject.SetActive(true);
					this.texttail.transform.position = this.tailtarget.position;
					float num = this.texttail.transform.localEulerAngles.z;
					if (num > 180f)
					{
						num -= 360f;
					}
					num = Mathf.Clamp(-1f - Mathf.Abs(num) / 60f, -1.7f, -1.1f);
					this.texttail.transform.localPosition = new Vector3(Mathf.Clamp(this.texttail.transform.localPosition.x, -3f, 3f), num, 0f);
					Vector2 vector = new Vector2(MainManager.MainCamera.WorldToViewportPoint(this.tailtarget.transform.position).x - MainManager.MainCamera.WorldToViewportPoint(this.texttail.transform.position).x, this.tailtarget.transform.position.y - this.texttail.transform.position.y);
					float value = vector.x / vector.y * -MainManager.MainCamera.WorldToViewportPoint(this.tailtarget.transform.position).z * 100f;
					this.texttail.transform.eulerAngles = new Vector3(this.texttail.transform.eulerAngles.x, this.globalcamdir.eulerAngles.y, Mathf.Clamp(value, -50f, 50f));
				}
				else
				{
					this.texttail.gameObject.SetActive(false);
				}
			}
			if (!this.started && MainManager.player != null)
			{
				MainManager.player.entity.DetectIgnoreSphere(true);
				this.started = true;
			}
			for (int i = 0; i < 2; i++)
			{
				if (!this.inbattle && this.inevent)
				{
					MainManager.letterbox[i].color = Color.Lerp(MainManager.letterbox[i].color, Color.black, MainManager.TieFramerate(0.15f));
				}
				else
				{
					MainManager.letterbox[i].color = Color.Lerp(MainManager.letterbox[i].color, Color.clear, MainManager.TieFramerate(0.15f));
				}
			}
			if (this.inputcooldown > 0f)
			{
				this.inputcooldown -= MainManager.framestep;
			}
			if (this.switchicon != null)
			{
				this.switchicon.color = Color.Lerp(this.switchicon.color, Color.clear, MainManager.TieFramerate(0.01f));
			}
			MainManager.stickholdx = (Mathf.Abs(InputIO.JoyStick(0)) < 0.5f && Mathf.Abs(InputIO.JoyStick(2)) < 0.5f);
			MainManager.stickholdy = (Mathf.Abs(InputIO.JoyStick(1)) < 0.5f && Mathf.Abs(InputIO.JoyStick(3)) < 0.5f);
			if (this.started)
			{
				if (this.flagvar != null && this.flagvar.Length >= 27)
				{
					if (this.flagvar[27] > 9999)
					{
						this.flagvar[27] = 9999;
					}
					if (this.flagvar[26] > 10000)
					{
						this.flagvar[26] = 10000;
					}
				}
				for (int j = 0; j < MainManager.bountyquests.Length; j++)
				{
					if (this.boardquests[2].Contains(MainManager.bountyquests[j]))
					{
						if (this.boardquests[0].Contains(MainManager.bountyquests[j]))
						{
							this.boardquests[0].Remove(MainManager.bountyquests[j]);
						}
						if (this.boardquests[1].Contains(MainManager.bountyquests[j]))
						{
							this.boardquests[1].Remove(MainManager.bountyquests[j]);
						}
					}
				}
				if (this.partylevel == 27)
				{
					this.partyexp = this.neededexp;
				}
			}
		}
	}

	// Token: 0x060004EA RID: 1258 RVA: 0x00034134 File Offset: 0x00032334
	public static int MixIngredients(int item1, int item2)
	{
		int result = 8;
		int length = MainManager.recipedata.GetLength(0);
		if ((item1 > item2 && item2 > -1) || item2 == 156)
		{
			int num = item1;
			item1 = item2;
			item2 = num;
		}
		if (item1 == 156 || item2 == 156)
		{
			int[] chances;
			if (item2 == 156)
			{
				RuntimeHelpers.InitializeArray(chances = new int[13], fieldof(<PrivateImplementationDetails>.0C5EBAEB48BD3A551E2691C622446FEFE7F28F40).FieldHandle);
			}
			else
			{
				RuntimeHelpers.InitializeArray(chances = new int[11], fieldof(<PrivateImplementationDetails>.D02090E6E4A9F6E9C7E22790BC27B0781608B416).FieldHandle);
			}
			return BattleControl.GetChance(chances);
		}
		for (int i = 0; i < length; i++)
		{
			if (MainManager.recipedata[i, 0] == item1 && MainManager.recipedata[i, 1] == item2)
			{
				result = MainManager.recipedata[i, 2];
				break;
			}
		}
		return result;
	}

	// Token: 0x060004EB RID: 1259 RVA: 0x000341E4 File Offset: 0x000323E4
	public static bool HasRecipe(int id)
	{
		for (int i = 0; i < MainManager.librarylimit[2]; i++)
		{
			if (MainManager.libraryorder[2, i] == id)
			{
				return MainManager.instance.librarystuff[2, i];
			}
		}
		return false;
	}

	// Token: 0x060004EC RID: 1260 RVA: 0x00034228 File Offset: 0x00032428
	public static int GetRecipeID(int id)
	{
		for (int i = 0; i < MainManager.librarylimit[2]; i++)
		{
			if (MainManager.libraryorder[2, i] == id)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x060004ED RID: 1261 RVA: 0x0003425C File Offset: 0x0003245C
	public static void ResetCamera()
	{
		MainManager.instance.camspeed = 0.1f;
		MainManager.instance.camoffsetspeed = 0.1f;
		MainManager.instance.changecamspeed = false;
		MainManager.instance.camanglechange = false;
		MainManager.instance.camoffset2 = Vector3.zero;
		MainManager.instance.camanglespeed = 0.1f;
		MainManager.instance.camtargetpos = null;
		MainManager.instance.camoffset = MainManager.defaultcamoffset;
		MainManager.instance.camangleoffset = MainManager.defaultcamangle;
		if (MainManager.map != null)
		{
			if (MainManager.map.camoffset.magnitude > 0.1f)
			{
				MainManager.instance.camoffset = MainManager.map.camoffset;
			}
			if (MainManager.map.camangle.magnitude > 0.1f)
			{
				MainManager.instance.camangleoffset = MainManager.map.camangle;
			}
		}
		if (MainManager.player != null)
		{
			MainManager.instance.camtarget = MainManager.player.transform;
			return;
		}
		MainManager.instance.camtarget = null;
	}

	// Token: 0x060004EE RID: 1262 RVA: 0x00034377 File Offset: 0x00032577
	public static void ResetCamera(bool instant)
	{
		MainManager.ResetCamera();
		if (instant)
		{
			MainManager.instance.camspeed = 1f;
			MainManager.instance.Invoke("ResetCamSpeed", 0.1f);
		}
	}

	// Token: 0x060004EF RID: 1263 RVA: 0x000343A4 File Offset: 0x000325A4
	private void ResetCamSpeed()
	{
		MainManager.instance.camspeed = 0.1f;
		MainManager.instance.camanglespeed = 0.1f;
	}

	// Token: 0x060004F0 RID: 1264 RVA: 0x000343C4 File Offset: 0x000325C4
	public static bool PartyIsNotMoving()
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].entity.forcemove)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x060004F1 RID: 1265 RVA: 0x00034407 File Offset: 0x00032607
	public static bool GetKey(int id)
	{
		return MainManager.GetKey(id, false);
	}

	// Token: 0x060004F2 RID: 1266 RVA: 0x00034410 File Offset: 0x00032610
	public static Vector2 GetPressedDirection(bool hold)
	{
		Vector2 a = Vector2.zero;
		if (MainManager.GetKey(0, hold))
		{
			a += Vector2.up;
		}
		else if (MainManager.GetKey(1, hold))
		{
			a += Vector2.down;
		}
		if (MainManager.GetKey(2, hold))
		{
			a += Vector2.left;
		}
		else if (MainManager.GetKey(3, hold))
		{
			a += Vector2.right;
		}
		return a.normalized;
	}

	// Token: 0x060004F3 RID: 1267 RVA: 0x00034484 File Offset: 0x00032684
	public static bool GetKey(int id, bool hold)
	{
		if (id == -4)
		{
			for (int i = 0; i < 10; i++)
			{
				if (MainManager.GetKey(i, hold))
				{
					return true;
				}
			}
			return false;
		}
		if (id == -3)
		{
			return MainManager.GetKey(4, hold) || MainManager.GetKey(5, hold) || MainManager.GetKey(6, hold) || MainManager.GetKey(7, hold);
		}
		if (id == -2)
		{
			if (!hold)
			{
				if (MainManager.joystick && MainManager.usejoystick > 0)
				{
					return (InputIO.JoyStick(0) != 0f && MainManager.stickholdx) || (InputIO.JoyStick(1) != 0f && MainManager.stickholdy) || (InputIO.JoyStick(2) != 0f && MainManager.stickholdx) || (InputIO.JoyStick(3) != 0f && MainManager.stickholdy);
				}
				return InputIO.GetKeyDown(0, false) || InputIO.GetKeyDown(1, false) || InputIO.GetKeyDown(2, false) || InputIO.GetKeyDown(3, false);
			}
			else
			{
				if (MainManager.joystick && MainManager.usejoystick > 0)
				{
					return InputIO.JoyStick(0) != 0f || InputIO.JoyStick(1) != 0f || InputIO.JoyStick(2) != 0f || InputIO.JoyStick(3) != 0f;
				}
				return InputIO.GetKey(0, false) || InputIO.GetKey(1, false) || InputIO.GetKey(2, false) || InputIO.GetKey(3, false);
			}
		}
		else if (id == -1)
		{
			if (!hold)
			{
				return InputIO.anyKeyDown();
			}
			return InputIO.anyKey();
		}
		else
		{
			if (!hold)
			{
				if (MainManager.usejoystick > 0)
				{
					if (id == 0 && (InputIO.JoyStick(1) < -0.5f || InputIO.JoyStick(3) > 0.5f) && MainManager.stickholdy)
					{
						return true;
					}
					if (id == 1 && (InputIO.JoyStick(1) > 0.5f || InputIO.JoyStick(3) < -0.5f) && MainManager.stickholdy)
					{
						return true;
					}
					if (id == 2 && (InputIO.JoyStick(0) < -0.5f || InputIO.JoyStick(2) < -0.5f) && MainManager.stickholdx)
					{
						return true;
					}
					if (id == 3 && (InputIO.JoyStick(0) > 0.5f || InputIO.JoyStick(2) > 0.5f) && MainManager.stickholdx)
					{
						return true;
					}
					if (id == 4 && InputIO.GetKeyDown(0, true))
					{
						return true;
					}
					if (id == 5 && InputIO.GetKeyDown(1, true))
					{
						return true;
					}
					if (id == 6 && InputIO.GetKeyDown(2, true))
					{
						return true;
					}
					if (id == 7 && InputIO.GetKeyDown(3, true))
					{
						return true;
					}
					if (id == 8 && InputIO.GetKeyDown(4, true))
					{
						return true;
					}
					if (id == 9 && InputIO.GetKeyDown(5, true))
					{
						return true;
					}
				}
				return InputIO.GetKeyDown(id, false);
			}
			if (MainManager.usejoystick > 0)
			{
				if (id == 0 && (InputIO.JoyStick(1) < 0f || InputIO.JoyStick(3) > 0f))
				{
					return true;
				}
				if (id == 1 && (InputIO.JoyStick(1) > 0f || InputIO.JoyStick(3) < 0f))
				{
					return true;
				}
				if (id == 2 && (InputIO.JoyStick(0) < 0f || InputIO.JoyStick(2) < 0f))
				{
					return true;
				}
				if (id == 3 && (InputIO.JoyStick(0) > 0f || InputIO.JoyStick(2) > 0f))
				{
					return true;
				}
				if (id == 4 && InputIO.GetKey(0, true))
				{
					return true;
				}
				if (id == 5 && InputIO.GetKey(1, true))
				{
					return true;
				}
				if (id == 6 && InputIO.GetKey(2, true))
				{
					return true;
				}
				if (id == 7 && InputIO.GetKey(3, true))
				{
					return true;
				}
				if (id == 8 && InputIO.GetKey(4, true))
				{
					return true;
				}
				if (id == 9 && InputIO.GetKey(5, true))
				{
					return true;
				}
			}
			return InputIO.GetKey(id, false);
		}
	}

	// Token: 0x060004F4 RID: 1268 RVA: 0x000347EB File Offset: 0x000329EB
	private void FixedUpdate()
	{
		if (MainManager.basicload)
		{
			this.RefreshCamera();
			this.LoopMusic();
		}
	}

	// Token: 0x060004F5 RID: 1269 RVA: 0x00034800 File Offset: 0x00032A00
	public static float DimishingReturns(float input, float decay)
	{
		return (input - 1f) * decay;
	}

	// Token: 0x060004F6 RID: 1270 RVA: 0x0003480C File Offset: 0x00032A0C
	public static float[][] LoopPoint()
	{
		string[] array = Resources.Load<TextAsset>("Data/LoopPoints").ToString().Split(new char[]
		{
			'\n'
		});
		List<float[]> list = new List<float[]>();
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[]
			{
				';'
			});
			list.Add(new float[]
			{
				Convert.ToSingle(array2[0]),
				Convert.ToSingle(array2[1])
			});
		}
		return list.ToArray();
	}

	// Token: 0x060004F7 RID: 1271 RVA: 0x0003488C File Offset: 0x00032A8C
	private void LoopMusic()
	{
		if (MainManager.musicloop == null || MainManager.musicloop.Length == 0)
		{
			MainManager.musicloop = MainManager.LoopPoint();
		}
		for (int i = 0; i < MainManager.music.Length; i++)
		{
			if ((MainManager.musicloop[MainManager.lastmusic][0] != 0f || MainManager.musicloop[MainManager.lastmusic][0] != 0f) && MainManager.music[i].time >= MainManager.musicloop[MainManager.lastmusic][0])
			{
				MainManager.music[i].time = MainManager.musicloop[MainManager.lastmusic][1];
			}
		}
	}

	// Token: 0x060004F8 RID: 1272 RVA: 0x00034920 File Offset: 0x00032B20
	public static int GetRandomMedal()
	{
		return MainManager.GetRandomMedal(false, false);
	}

	// Token: 0x060004F9 RID: 1273 RVA: 0x0003492C File Offset: 0x00032B2C
	public static int GetRandomMedal(bool dontremove, bool random)
	{
		if (MainManager.instance.flagstring[13].Length == 0)
		{
			MainManager.instance.flagstring[13] = "0,1,2,3,4,5,6,7,8,9";
		}
		string[] array = MainManager.instance.flagstring[13].Split(new char[]
		{
			','
		});
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = Convert.ToInt32(array[i]);
		}
		int num = 0;
		if (random)
		{
			List<int> list = new List<int>(array2);
			MainManager.RandomSort(ref list);
			array2 = list.ToArray();
			num = Random.Range(0, array2.Length);
		}
		int result = array2[num];
		MainManager.instance.flagstring[13] = "";
		for (int j = 0; j < array2.Length; j++)
		{
			if (j != num || dontremove)
			{
				string[] array3 = MainManager.instance.flagstring;
				int num2 = 13;
				array3[num2] += array2[j];
				if (j < array2.Length - 1)
				{
					string[] array4 = MainManager.instance.flagstring;
					int num3 = 13;
					array4[num3] += ",";
				}
			}
		}
		return result;
	}

	// Token: 0x060004FA RID: 1274 RVA: 0x00034A4B File Offset: 0x00032C4B
	public static GameObject NewUIObject(string objname, Transform parent, Vector3 pos)
	{
		return MainManager.NewUIObject(objname, parent, pos, Vector3.one, null, 0);
	}

	// Token: 0x060004FB RID: 1275 RVA: 0x00034A5C File Offset: 0x00032C5C
	public static GameObject NewUIObject(string objname, Transform parent, Vector3 pos, Vector3 size, Sprite sprite)
	{
		return MainManager.NewUIObject(objname, parent, pos, size, sprite, 0);
	}

	// Token: 0x060004FC RID: 1276 RVA: 0x00034A6C File Offset: 0x00032C6C
	public static GameObject NewUIObject(string objname, Transform parent, Vector3 pos, Vector3 size, Sprite sprite, int sortorder)
	{
		GameObject gameObject = new GameObject(objname);
		if (parent == null)
		{
			gameObject.transform.parent = MainManager.GUICamera.transform;
		}
		else
		{
			gameObject.transform.parent = parent;
		}
		gameObject.layer = 5;
		gameObject.transform.localEulerAngles = Vector3.zero;
		gameObject.transform.localPosition = pos;
		gameObject.transform.localScale = size;
		if (sprite != null)
		{
			SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = sprite;
			spriteRenderer.sortingOrder = sortorder;
		}
		return gameObject;
	}

	// Token: 0x060004FD RID: 1277 RVA: 0x00034AFC File Offset: 0x00032CFC
	public static bool CheckIfCanExist(int[] requires, int[] limit, int regionalflag)
	{
		if (limit != null && limit.Length != 0)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < limit.Length; i++)
			{
				if (limit[i] < -1)
				{
					if (MainManager.instance.flags[Mathf.Abs(limit[i])])
					{
						return true;
					}
				}
				else
				{
					list.Add(limit[i]);
				}
			}
			limit = list.ToArray();
		}
		return (requires != null && requires.Length != 0 && requires[0] > -1 && !MainManager.CheckAllBool(MainManager.instance.flags, requires, true)) || (limit != null && limit.Length != 0 && limit[0] > -1 && MainManager.CheckAllBool(MainManager.instance.flags, limit, true)) || (regionalflag > -1 && MainManager.map != null && MainManager.instance.regionalflags[regionalflag]);
	}

	// Token: 0x060004FE RID: 1278 RVA: 0x00034BB4 File Offset: 0x00032DB4
	private static Vector3 CamLimiter(Vector3 pos)
	{
		return new Vector3(Mathf.Clamp(pos.x, MainManager.map.camlimitneg.x, MainManager.map.camlimitpos.x), Mathf.Clamp(pos.y, MainManager.map.camlimitneg.y, MainManager.map.camlimitpos.y), Mathf.Clamp(pos.z, MainManager.map.camlimitneg.z, MainManager.map.camlimitpos.z));
	}

	// Token: 0x060004FF RID: 1279 RVA: 0x00034C44 File Offset: 0x00032E44
	public static Vector3 LimitRadius(Vector3 pos, Vector3 origin, float radius)
	{
		Vector3 vector = Vector3.ClampMagnitude(pos - origin, radius);
		return origin + new Vector3(vector.x, pos.y, vector.z);
	}

	// Token: 0x06000500 RID: 1280 RVA: 0x00034C7C File Offset: 0x00032E7C
	public static Vector3 LimitRadius(Vector3 pos, Vector3 origin, float radius, bool ignoreY)
	{
		Vector3 vector = Vector3.ClampMagnitude(pos - origin, radius);
		if (ignoreY)
		{
			Vector3 vector2 = new Vector3(origin.x, 0f, origin.z) + new Vector3(vector.x, 0f, vector.z);
			return new Vector3(vector2.x, pos.y, vector2.z);
		}
		return origin + new Vector3(vector.x, pos.y, vector.z);
	}

	// Token: 0x06000501 RID: 1281 RVA: 0x00034D04 File Offset: 0x00032F04
	private void RefreshCamera()
	{
		if (MainManager.MainCamera.transform.parent != null)
		{
			if (this.camtarget != null)
			{
				if (MainManager.map != null)
				{
					if (MainManager.map.tetherdistance > 0f && this.insideid == -1)
					{
						MainManager.MainCamera.transform.parent.position = Vector3.Lerp(MainManager.MainCamera.transform.parent.position, MainManager.LimitRadius(MainManager.CamLimiter(this.camtarget.transform.position), MainManager.map.actualcenter, MainManager.map.tetherdistance), this.camspeed);
					}
					else
					{
						MainManager.MainCamera.transform.parent.position = Vector3.Lerp(MainManager.MainCamera.transform.parent.position, MainManager.CamLimiter(this.camtarget.transform.position), this.camspeed);
					}
				}
				else
				{
					MainManager.MainCamera.transform.parent.position = Vector3.Lerp(MainManager.MainCamera.transform.parent.position, this.camtarget.transform.position, this.camspeed);
				}
			}
			else if (this.camtargetpos != null)
			{
				Vector3 value = this.camtargetpos.Value;
				if (MainManager.map != null)
				{
					if (MainManager.map.tetherdistance > 0f && this.insideid == -1)
					{
						MainManager.MainCamera.transform.parent.position = Vector3.Lerp(MainManager.MainCamera.transform.parent.position, MainManager.LimitRadius(MainManager.CamLimiter(this.camtargetpos.Value), MainManager.map.actualcenter, MainManager.map.tetherdistance), this.camspeed);
					}
					else
					{
						MainManager.MainCamera.transform.parent.position = Vector3.Lerp(MainManager.MainCamera.transform.parent.position, MainManager.CamLimiter(value), this.camspeed);
					}
				}
				else
				{
					MainManager.MainCamera.transform.parent.position = Vector3.Lerp(MainManager.MainCamera.transform.parent.position, value, this.camspeed);
				}
			}
		}
		Vector3 b = Vector3.zero;
		if (MainManager.screenshake != Vector3.zero)
		{
			b = MainManager.RandomVector(MainManager.screenshake);
		}
		MainManager.MainCamera.transform.localPosition = Vector3.Lerp(MainManager.MainCamera.transform.localPosition, this.camoffset + this.camoffset2, this.camspeed) + b;
		if (MainManager.map != null && MainManager.map.rotatecam && this.insideid == -1)
		{
			MainManager.MainCamera.transform.LookAt(MainManager.map.actualcenter);
			MainManager.MainCamera.transform.parent.LookAt(MainManager.map.actualcenter);
			if (MainManager.map.tieYtoplayer)
			{
				MainManager.MainCamera.transform.parent.transform.eulerAngles = new Vector3(0f, MainManager.MainCamera.transform.parent.transform.eulerAngles.y, MainManager.MainCamera.transform.parent.transform.eulerAngles.z);
				return;
			}
		}
		else
		{
			float t = this.camanglechange ? this.camanglespeed : this.camspeed;
			MainManager.MainCamera.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(MainManager.MainCamera.transform.localEulerAngles.x, 0f, t), Mathf.LerpAngle(MainManager.MainCamera.transform.localEulerAngles.y, 0f, t), Mathf.LerpAngle(MainManager.MainCamera.transform.localEulerAngles.z, 0f, t));
			MainManager.MainCamera.transform.parent.localEulerAngles = new Vector3(Mathf.LerpAngle(MainManager.MainCamera.transform.parent.localEulerAngles.x, this.camangleoffset.x, t), Mathf.LerpAngle(MainManager.MainCamera.transform.parent.localEulerAngles.y, this.camangleoffset.y, t), Mathf.LerpAngle(MainManager.MainCamera.transform.parent.localEulerAngles.z, this.camangleoffset.z, t));
		}
	}

	// Token: 0x06000502 RID: 1282 RVA: 0x000351C6 File Offset: 0x000333C6
	public static void SaveCameraPosition()
	{
		MainManager.SaveCameraPosition(true);
	}

	// Token: 0x06000503 RID: 1283 RVA: 0x000351CE File Offset: 0x000333CE
	public static void LoadCameraPosition()
	{
		MainManager.SaveCameraPosition(false);
	}

	// Token: 0x06000504 RID: 1284 RVA: 0x000351D8 File Offset: 0x000333D8
	public static void SaveCameraPosition(bool set)
	{
		if (set)
		{
			MainManager.tempcampos = MainManager.instance.camtargetpos;
			MainManager.tempcamoffset = MainManager.instance.camoffset;
			MainManager.tempcamspeed = MainManager.instance.camspeed;
			MainManager.tempcamangleoffset = MainManager.instance.camangleoffset;
			MainManager.tempcamtarget = MainManager.instance.camtarget;
			MainManager.tempmaplp = MainManager.map.camlimitpos;
			MainManager.tempmapln = MainManager.map.camlimitneg;
			return;
		}
		MainManager.instance.camtargetpos = MainManager.tempcampos;
		MainManager.instance.camspeed = MainManager.tempcamspeed;
		MainManager.instance.camoffset = MainManager.tempcamoffset;
		MainManager.instance.camangleoffset = MainManager.tempcamangleoffset;
		MainManager.instance.camtarget = MainManager.tempcamtarget;
		MainManager.map.camlimitpos = MainManager.tempmaplp;
		MainManager.map.camlimitneg = MainManager.tempmapln;
	}

	// Token: 0x06000505 RID: 1285 RVA: 0x000352BB File Offset: 0x000334BB
	public static Vector3 LerpVectorAngle(Vector3 input, Vector3 target, float ammount)
	{
		return new Vector3(Mathf.LerpAngle(input.x, target.x, ammount), Mathf.LerpAngle(input.y, target.y, ammount), Mathf.LerpAngle(input.z, target.z, ammount));
	}

	// Token: 0x06000506 RID: 1286 RVA: 0x000352F8 File Offset: 0x000334F8
	public static Vector3 LerpVectorAngleSmooth(Vector3 input, Vector3 target, Vector3 current, float ammount)
	{
		float x = current.x;
		float y = current.y;
		float z = current.z;
		Mathf.SmoothDampAngle(input.x, target.x, ref x, ammount);
		Mathf.SmoothDampAngle(input.y, target.y, ref y, ammount);
		Mathf.SmoothDampAngle(input.z, target.z, ref z, ammount);
		return new Vector3(x, y, z);
	}

	// Token: 0x06000507 RID: 1287 RVA: 0x00035364 File Offset: 0x00033564
	public static void RebuildHUD()
	{
		MainManager.ApplyBadges();
		MainManager.ApplyStatBonus();
		if (MainManager.instance.hud != null && MainManager.instance.hud.Length != 0)
		{
			Object.Destroy(MainManager.instance.hud[0].parent.gameObject);
		}
		MainManager.instance.hud = null;
	}

	// Token: 0x06000508 RID: 1288 RVA: 0x000353BC File Offset: 0x000335BC
	private static void CreateHUD()
	{
		MainManager.ApplyBadges();
		MainManager.ApplyStatBonus();
		MainManager.instance.hud = new Transform[5];
		MainManager.hudsprites = new SpriteRenderer[4];
		MainManager.instance.hudfont = new DynamicFont[5];
		GameObject gameObject = new GameObject("HUD");
		gameObject.transform.parent = MainManager.GUICamera.transform;
		gameObject.transform.localEulerAngles = Vector3.zero;
		gameObject.transform.localPosition = Vector3.zero;
		float num = -7f;
		for (int i = 0; i < 4; i++)
		{
			MainManager.NewHUDElement(i, new Vector2(num, 8.5f), gameObject.transform);
			num += 3.5f;
			if (i == 2)
			{
				num += 3.5f;
			}
		}
		MainManager.NewHUDElement(4, new Vector2(MainManager.instance.hud[3].transform.position.x, -MainManager.instance.hud[3].transform.position.y), gameObject.transform);
		MainManager.hudvalue = new float[]
		{
			6.5f,
			7.5f,
			8.5f,
			9.5f,
			-6.5f,
			-7f
		};
	}

	// Token: 0x06000509 RID: 1289 RVA: 0x000354DC File Offset: 0x000336DC
	private static void NewHUDElement(int hudid, Vector2 position, Transform parent)
	{
		GameObject gameObject = new GameObject("hud" + hudid);
		SpriteRenderer spriteRenderer = new GameObject("basesprite" + hudid).AddComponent<SpriteRenderer>();
		MainManager.instance.hud[hudid] = gameObject.transform;
		MainManager.instance.hud[hudid].parent = parent.transform;
		spriteRenderer.transform.parent = MainManager.instance.hud[hudid];
		spriteRenderer.gameObject.layer = 5;
		spriteRenderer.sprite = MainManager.guisprites[4];
		spriteRenderer.sortingOrder = 10;
		spriteRenderer.transform.localScale = new Vector3(0.55f, 0.65f, 1f);
		MainManager.instance.hud[hudid].transform.localPosition = new Vector3(position.x, position.y, 10f);
		SpriteRenderer spriteRenderer2 = new GameObject("facesprite").AddComponent<SpriteRenderer>();
		spriteRenderer2.transform.localScale = Vector3.one * 0.8f;
		spriteRenderer2.sortingOrder = spriteRenderer.sortingOrder + 2;
		spriteRenderer2.gameObject.layer = 5;
		spriteRenderer2.transform.parent = spriteRenderer.transform;
		spriteRenderer2.transform.localPosition = new Vector2(-2.3f, 0f);
		if (hudid < 3 && hudid < MainManager.instance.playerdata.Length)
		{
			spriteRenderer.color = MainManager.instance.charcolor[MainManager.instance.playerdata[hudid].animid];
			spriteRenderer2.sprite = MainManager.guisprites[MainManager.instance.playerdata[hudid].animid + 5];
			MainManager.hudsprites[hudid] = spriteRenderer;
			MainManager.NewUIObject("hpicon", spriteRenderer2.transform, new Vector3(0.45f, -0.6f), Vector3.one * 0.5f, MainManager.guisprites[24], spriteRenderer2.sortingOrder + 1);
			if (hudid < MainManager.instance.playerdata.Length)
			{
				MainManager.instance.hudfont[hudid] = DynamicFont.SetUp(MainManager.instance.playerdata[hudid].hpt.ToString().PadLeft(2, '0') + "/" + MainManager.instance.playerdata[hudid].maxhp.ToString().PadLeft(2, '0'), false, true, 2f, 2, spriteRenderer.sortingOrder + 1, Vector2.one * 1.75f, spriteRenderer.transform, new Vector3(-0.9f, -0.6f), Color.white, new Vector2?(new Vector2(3f, 0.85f)));
				MainManager.instance.hudfont[hudid].dropshadow = true;
			}
		}
		else if (hudid == 3)
		{
			MainManager.hudsprites[3] = spriteRenderer;
			spriteRenderer.color = MainManager.instance.menucolors[3];
			spriteRenderer2.sprite = MainManager.guisprites[28];
			MainManager.instance.hudfont[hudid] = DynamicFont.SetUp(MainManager.instance.tpt.ToString().PadLeft(2, '0') + "/" + MainManager.instance.maxtp.ToString().PadLeft(2, '0'), false, true, 2f, 2, spriteRenderer.sortingOrder + 1, Vector2.one * 1.75f, spriteRenderer.transform, new Vector3(-0.9f, -0.6f), Color.white, new Vector2?(new Vector2(3f, 0.85f)));
			MainManager.instance.hudfont[hudid].dropshadow = true;
		}
		else if (hudid == 4)
		{
			spriteRenderer.color = MainManager.instance.menucolors[0];
			spriteRenderer2.sprite = MainManager.guisprites[29];
			MainManager.instance.hudfont[hudid] = DynamicFont.SetUp(MainManager.instance.moneyt.ToString().PadLeft(3, '0'), false, true, 2f, 2, spriteRenderer.sortingOrder + 1, Vector2.one * 1.75f, spriteRenderer.transform, new Vector3(-0.75f, -0.6f), Color.white);
			MainManager.instance.hudfont[hudid].dropshadow = true;
		}
		MainManager.instance.hud[hudid].transform.localEulerAngles = Vector3.zero;
		spriteRenderer.transform.localEulerAngles = Vector3.zero;
	}

	// Token: 0x0600050A RID: 1290 RVA: 0x00035965 File Offset: 0x00033B65
	public static float QuadraticY(float x1, float x2, float ymax, float currentx)
	{
		return MainManager.QuadraticY(new Vector2(x1, 0f), new Vector2(x2, 0f), ymax, currentx);
	}

	// Token: 0x0600050B RID: 1291 RVA: 0x00035984 File Offset: 0x00033B84
	public static string GetItemString(bool emptyinv)
	{
		string text = "";
		for (int i = 0; i < MainManager.instance.items[0].Count; i++)
		{
			text += MainManager.instance.items[0][i].ToString();
			if (i < MainManager.instance.items[0].Count - 1)
			{
				text += ",";
			}
		}
		if (emptyinv)
		{
			MainManager.instance.items[0] = new List<int>();
		}
		return text;
	}

	// Token: 0x0600050C RID: 1292 RVA: 0x00035A0B File Offset: 0x00033C0B
	public static float QuadraticY(Vector2 x1, Vector2 x2, float ymax, float currentx)
	{
		return MainManager.BeizierCurve(x1, x2, ymax, MainManager.GetPercentage(x1.x, x2.x, currentx)).y;
	}

	// Token: 0x0600050D RID: 1293 RVA: 0x00035A2C File Offset: 0x00033C2C
	public static Vector2 BeizierCurve(Vector2 start, Vector2 end, float ymax, float t)
	{
		Vector2 a = new Vector2((start.x + end.x) / 2f, ymax);
		Vector2 a2 = (1f - t) * (1f - t) * start;
		Vector2 b = 2f * t * (1f - t) * a;
		Vector2 b2 = t * t * end;
		return a2 + b + b2;
	}

	// Token: 0x0600050E RID: 1294 RVA: 0x00035A98 File Offset: 0x00033C98
	public static bool EntitiesAreNotMoving(EntityControl[] entities)
	{
		for (int i = 0; i < entities.Length; i++)
		{
			if (entities[i].forcemove)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x0600050F RID: 1295 RVA: 0x00035AC0 File Offset: 0x00033CC0
	public static string GetBleep(EntityControl entity)
	{
		return MainManager.GetBleep(entity, 1f);
	}

	// Token: 0x06000510 RID: 1296 RVA: 0x00035AD0 File Offset: 0x00033CD0
	public static string GetBleep(EntityControl entity, float volume)
	{
		return string.Concat(new object[]
		{
			"|bleep,",
			entity.dialoguebleepid,
			",",
			entity.bleeppitch,
			",",
			volume,
			"|"
		});
	}

	// Token: 0x06000511 RID: 1297 RVA: 0x00035B30 File Offset: 0x00033D30
	public static Vector3 BeizierCurve3(Vector3 start, Vector3 end, float ymax, float t)
	{
		return MainManager.BeizierCurve3(start, end, new Vector3((start.x + end.x) / 2f, (ymax > 0f) ? ((start.y + end.y) / 2f + ymax) : ymax, (start.z + end.z) / 2f), t);
	}

	// Token: 0x06000512 RID: 1298 RVA: 0x00035B90 File Offset: 0x00033D90
	public static bool AsianLang()
	{
		return MainManager.languageid == 3;
	}

	// Token: 0x06000513 RID: 1299 RVA: 0x00035B9C File Offset: 0x00033D9C
	public static Vector3 BeizierCurve3(Vector3 start, Vector3 end, Vector3 mid, float t)
	{
		t = Mathf.Clamp01(t);
		return (1f - t) * (1f - t) * start + 2f * t * (1f - t) * mid + t * t * end;
	}

	// Token: 0x06000514 RID: 1300 RVA: 0x00035BEE File Offset: 0x00033DEE
	public static float BeizierFloat(float height, float t)
	{
		t = Mathf.Clamp01(t);
		return 2f * t * (1f - t) * height;
	}

	// Token: 0x06000515 RID: 1301 RVA: 0x00035C09 File Offset: 0x00033E09
	public static float QuadraticYSemiClamped(float currentx, float startx, float endx, float modifier)
	{
		return -((modifier + MainManager.GetDistance(startx, endx) / 10f) / MainManager.GetDistance(startx, endx)) * (currentx - endx) * (currentx - startx);
	}

	// Token: 0x06000516 RID: 1302 RVA: 0x00035C2B File Offset: 0x00033E2B
	public static float QuadraticYUnclamped(float currentx, float startx, float endx, float modifier)
	{
		return -modifier * (currentx - endx) * (currentx - startx);
	}

	// Token: 0x06000517 RID: 1303 RVA: 0x00035C37 File Offset: 0x00033E37
	public static Vector3 RandomVector(Vector3 input)
	{
		return new Vector3(Random.Range(-input.x, input.x), Random.Range(-input.y, input.y), Random.Range(-input.z, input.z));
	}

	// Token: 0x06000518 RID: 1304 RVA: 0x00035C74 File Offset: 0x00033E74
	public static Vector3 RandomVector(float randomx, float randomy, float randomz)
	{
		return new Vector3(Random.Range(-randomx, randomx), Random.Range(-randomy, randomy), Random.Range(-randomz, randomz));
	}

	// Token: 0x06000519 RID: 1305 RVA: 0x00035C93 File Offset: 0x00033E93
	public static Vector3 RandomVector(float input)
	{
		return new Vector3(Random.Range(-input, input), Random.Range(-input, input), Random.Range(-input, input));
	}

	// Token: 0x0600051A RID: 1306 RVA: 0x00035CB2 File Offset: 0x00033EB2
	public static Vector3 RandomVector(Vector2 input)
	{
		return new Vector3(Random.Range(-input.x, input.x), Random.Range(-input.y, input.y));
	}

	// Token: 0x0600051B RID: 1307 RVA: 0x00035CDD File Offset: 0x00033EDD
	public static Vector3 RandomVector(float randomx, float randomy)
	{
		return new Vector3(Random.Range(-randomx, randomx), Random.Range(-randomy, randomy));
	}

	// Token: 0x0600051C RID: 1308 RVA: 0x00035CF4 File Offset: 0x00033EF4
	public static float ColorDifference(Color a, Color b)
	{
		return new Vector3(a.r - b.r, a.g - b.g, a.b - b.b).magnitude;
	}

	// Token: 0x0600051D RID: 1309 RVA: 0x00035D38 File Offset: 0x00033F38
	public static void GlobalCommand(ref string text)
	{
		if (MainManager.map != null && MainManager.map.useglobalcommand && MainManager.map.currentline > -1)
		{
			string[] array = MainManager.map.commandlines[MainManager.map.currentline].Split(new char[]
			{
				';'
			});
			Regex regex = new Regex(Regex.Escape("@"));
			for (int i = 0; i < array.Length; i++)
			{
				text = regex.Replace(text, array[i], 1);
			}
		}
	}

	// Token: 0x0600051E RID: 1310 RVA: 0x00035DC0 File Offset: 0x00033FC0
	private static void ForceHUD()
	{
		if (MainManager.tphp != null)
		{
			for (int i = 0; i < MainManager.tphp.Length; i++)
			{
				MainManager.tphp[i] = -1;
			}
		}
		if (MainManager.ptmhp != null)
		{
			for (int j = 0; j < MainManager.ptmhp.Length; j++)
			{
				MainManager.ptmhp[j] = -1;
			}
		}
		if (MainManager.tmtp != null)
		{
			for (int k = 0; k < MainManager.tmtp.Length; k++)
			{
				MainManager.tmtp[k] = -1;
			}
		}
		MainManager.tempmoneh = -1;
	}

	// Token: 0x0600051F RID: 1311 RVA: 0x00035E38 File Offset: 0x00034038
	private static void RefreshHUD()
	{
		if (MainManager.instance.hud == null || MainManager.instance.hud.Length == 0)
		{
			MainManager.CreateHUD();
			MainManager.tphp = new int[MainManager.instance.playerdata.Length];
			MainManager.ptmhp = new int[MainManager.instance.playerdata.Length];
			MainManager.instance.hud[4].localPosition = new Vector3(MainManager.instance.hud[3].localPosition.x, MainManager.instance.hud[4].localPosition.y, MainManager.instance.hud[4].localPosition.z);
		}
		else if (MainManager.instance.hud.Length != 0 && MainManager.instance.hudcooldown > 0f)
		{
			if (!MainManager.instance.inbattle)
			{
				MainManager.instance.hudcooldown -= MainManager.framestep;
			}
			for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
			{
				if (MainManager.instance.playerdata[i].hpt > MainManager.instance.playerdata[i].hp)
				{
					MainManager.BattleData[] array = MainManager.instance.playerdata;
					int num = i;
					array[num].hpt = array[num].hpt - 1;
				}
				else if (MainManager.instance.playerdata[i].hpt < MainManager.instance.playerdata[i].hp)
				{
					MainManager.BattleData[] array2 = MainManager.instance.playerdata;
					int num2 = i;
					array2[num2].hpt = array2[num2].hpt + 1;
					if (MainManager.instance.playerdata[i].hpt != MainManager.instance.playerdata[i].hp && !MainManager.sounds[5].isPlaying)
					{
						MainManager.PlaySound("TP", 5, 1.15f + Random.Range(-0.1f, 0.1f), 1f);
					}
				}
				if (MainManager.tphp[i] != MainManager.instance.playerdata[i].hpt || MainManager.ptmhp[i] != MainManager.instance.playerdata[i].maxhp)
				{
					MainManager.instance.hudfont[i].text = string.Concat(MainManager.instance.playerdata[i].hpt).PadLeft(2, '0') + "/" + string.Concat(MainManager.instance.playerdata[i].maxhp).PadLeft(2, '0');
					MainManager.tphp[i] = MainManager.instance.playerdata[i].hpt;
					MainManager.ptmhp[i] = MainManager.instance.playerdata[i].maxhp;
				}
				if (MainManager.instance.playerdata[i].hp > 0 && MainManager.instance.playerdata[i].hp <= 4 && MainManager.ColorDifference(MainManager.hudsprites[i].color, MainManager.instance.charcolor[MainManager.instance.playerdata[i].trueid]) < 0.1f)
				{
					MainManager.hudsprites[i].color = Color.red;
				}
				else if (MainManager.instance.playerdata[i].hp == 0)
				{
					MainManager.hudsprites[i].color = Color.gray;
				}
				else
				{
					MainManager.hudsprites[i].color = Color.Lerp(MainManager.hudsprites[i].color, MainManager.instance.charcolor[MainManager.instance.playerdata[i].trueid], MainManager.framestep * 0.05f);
				}
			}
			if (MainManager.hudsprites.Length > 3)
			{
				MainManager.hudsprites[3].color = Color.Lerp(MainManager.hudsprites[3].color, MainManager.instance.menucolors[3], MainManager.framestep * 0.05f);
			}
			if (MainManager.instance.tpt > MainManager.instance.tp)
			{
				MainManager.instance.tpt--;
			}
			else if (MainManager.instance.tpt < MainManager.instance.tp)
			{
				MainManager.instance.tpt++;
				if (MainManager.instance.tpt != MainManager.instance.tp && !MainManager.sounds[5].isPlaying)
				{
					MainManager.PlaySound("TP", 5, 1f + Random.Range(-0.1f, 0.1f), 1f);
				}
			}
			if (MainManager.instance.tpt != MainManager.tmtp[0] || MainManager.instance.maxtp != MainManager.tmtp[1])
			{
				MainManager.instance.hudfont[3].text = string.Concat(MainManager.instance.tpt).PadLeft(2, '0') + "/" + string.Concat(MainManager.instance.maxtp).PadLeft(2, '0');
				MainManager.tmtp[0] = MainManager.instance.tpt;
				MainManager.tmtp[1] = MainManager.instance.maxtp;
			}
			MainManager.ShowHUD(true);
			if (MainManager.instance.hudcooldown <= 0f)
			{
				MainManager.instance.hudcooldown = -100f;
			}
		}
		else
		{
			MainManager.ShowHUD(false);
		}
		if (MainManager.instance.showmoney > 0f)
		{
			if (!MainManager.instance.message)
			{
				MainManager.instance.showmoney -= MainManager.framestep;
			}
			if (MainManager.instance.money != MainManager.instance.moneyt && !MainManager.sounds[4].isPlaying)
			{
				MainManager.PlaySound("Money", 4, ((MainManager.instance.money > MainManager.instance.moneyt) ? 1f : 0.7f) + Random.Range(-0.1f, 0.1f), 1f);
			}
		}
		if (MainManager.instance.discoveryhud > 0f)
		{
			MainManager.instance.discoveryhud -= MainManager.framestep;
		}
		if (MainManager.instance.moneyt > MainManager.instance.money)
		{
			MainManager.instance.moneyt--;
		}
		else if (MainManager.instance.moneyt < MainManager.instance.money)
		{
			MainManager.instance.moneyt++;
		}
		if (MainManager.tempmoneh != MainManager.instance.moneyt)
		{
			MainManager.instance.hudfont[4].text = string.Concat(MainManager.instance.moneyt).PadLeft(3, '0');
			MainManager.tempmoneh = MainManager.instance.moneyt;
		}
	}

	// Token: 0x06000520 RID: 1312 RVA: 0x00036524 File Offset: 0x00034724
	public static void ShowHUD(bool show)
	{
		if (show && !MainManager.hudvisible)
		{
			MainManager.ForceHUD();
			MainManager.hudvisible = true;
		}
		else if (!show)
		{
			MainManager.hudvisible = false;
		}
		float num = 6.5f;
		float num2 = 4.25f;
		if (show)
		{
			num2 = 6.5f;
			num = 4.25f;
		}
		float num3 = 1f;
		for (int i = 0; i < MainManager.instance.hud.Length; i++)
		{
			if (MainManager.instance.hud[i] != null)
			{
				if (i < 3)
				{
					if (i < MainManager.instance.playerdata.Length)
					{
						if (show)
						{
							MainManager.hudvalue[i] = Mathf.Lerp(MainManager.hudvalue[i], num, MainManager.TieFramerate(0.15f));
						}
						else
						{
							MainManager.hudvalue[i] = Mathf.Lerp(MainManager.hudvalue[i], num + (float)i, MainManager.TieFramerate(0.15f));
						}
					}
					else
					{
						MainManager.hudvalue[i] = 10f;
					}
				}
				if (i == 3)
				{
					if (show)
					{
						MainManager.hudvalue[i] = Mathf.Lerp(MainManager.hudvalue[i], num, MainManager.TieFramerate(0.15f));
					}
					else
					{
						MainManager.hudvalue[i] = Mathf.Lerp(MainManager.hudvalue[i], num + (float)i, MainManager.TieFramerate(0.15f));
					}
				}
				else if (i == 4)
				{
					num3 = -1f;
					if (!show)
					{
						num2 = 6.5f;
						num = 4.25f;
					}
					if (MainManager.instance.showmoney > 0f)
					{
						MainManager.hudvalue[i] = Mathf.Lerp(MainManager.hudvalue[i], num, MainManager.TieFramerate(0.15f));
					}
					else
					{
						MainManager.hudvalue[i] = Mathf.Lerp(MainManager.hudvalue[i], num + (float)i, MainManager.TieFramerate(0.15f));
					}
				}
				MainManager.instance.hud[i].localPosition = Vector3.Lerp(MainManager.instance.hud[i].localPosition, new Vector3(MainManager.instance.hud[i].localPosition.x, num3 * (MainManager.hudvalue[i] + (MainManager.hudvalue[i] - num) * (MainManager.hudvalue[i] - num2)), MainManager.instance.hud[i].localPosition.z), MainManager.TieFramerate(0.2f));
			}
		}
	}

	// Token: 0x06000521 RID: 1313 RVA: 0x00036744 File Offset: 0x00034944
	public static Vector3 MiddlePoint(Vector3[] inputs)
	{
		Vector3 a = Vector3.zero;
		for (int i = 0; i < inputs.Length; i++)
		{
			a += inputs[i];
		}
		return a / (float)inputs.Length;
	}

	// Token: 0x06000522 RID: 1314 RVA: 0x00036780 File Offset: 0x00034980
	public static int CheckIfMore(int value, int[] values)
	{
		int num = 0;
		for (int i = 0; i < values.Length; i++)
		{
			if (value >= values[i])
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x06000523 RID: 1315 RVA: 0x000367A8 File Offset: 0x000349A8
	public static Vector3[] GetEntitiesPos(EntityControl[] e)
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < e.Length; i++)
		{
			list.Add(e[i].transform.position);
		}
		return list.ToArray();
	}

	// Token: 0x06000524 RID: 1316 RVA: 0x000367E4 File Offset: 0x000349E4
	public static void RefreshSkills()
	{
		bool flag = MainManager.HasFollower(MainManager.AnimIDs.AntQueen) || MainManager.CurrentMap() == MainManager.Maps.TestRoom || MainManager.instance.flags[594] || MainManager.BadgeIsEquipped(80);
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			MainManager.instance.playerdata[i].skills = new List<int>();
			switch (MainManager.instance.playerdata[i].trueid)
			{
			case 0:
				if (MainManager.battle == null)
				{
					if (MainManager.instance.flags[11])
					{
						MainManager.instance.playerdata[i].skills.Add(34);
					}
					if (MainManager.instance.flags[21])
					{
						MainManager.instance.playerdata[i].skills.Add(35);
					}
					if (MainManager.instance.flags[19])
					{
						MainManager.instance.playerdata[i].skills.Add(36);
					}
				}
				MainManager.instance.playerdata[i].skills.Add(2);
				if (MainManager.instance.flags[21])
				{
					MainManager.instance.playerdata[i].skills.Add(18);
				}
				if (MainManager.BadgeIsEquipped(60))
				{
					MainManager.instance.playerdata[i].skills.Add(44);
				}
				if (MainManager.instance.partylevel >= 11)
				{
					MainManager.instance.playerdata[i].skills.Add(16);
				}
				if (MainManager.instance.partylevel >= 15)
				{
					MainManager.instance.playerdata[i].skills.Add(24);
				}
				if (MainManager.instance.flags[19])
				{
					MainManager.instance.playerdata[i].skills.Add(5);
				}
				if (MainManager.instance.flags[544])
				{
					MainManager.instance.playerdata[i].skills.Add(26);
				}
				if (MainManager.instance.flags[533])
				{
					MainManager.instance.playerdata[i].skills.Add(31);
				}
				if (MainManager.instance.partylevel >= 2)
				{
					MainManager.instance.playerdata[i].skills.Add(11);
				}
				if (MainManager.instance.flags[445])
				{
					MainManager.instance.playerdata[i].skills.Add(45);
				}
				if (MainManager.BadgeIsEquipped(51, MainManager.instance.playerdata[i].trueid))
				{
					MainManager.instance.playerdata[i].skills.Add(33);
				}
				if (MainManager.BadgeIsEquipped(73, MainManager.instance.playerdata[i].trueid))
				{
					MainManager.instance.playerdata[i].skills.Add(48);
				}
				if (flag)
				{
					MainManager.instance.playerdata[i].skills.Add(46);
				}
				break;
			case 1:
				if (MainManager.battle == null)
				{
					if (MainManager.instance.flags[17])
					{
						MainManager.instance.playerdata[i].skills.Add(37);
					}
					if (MainManager.instance.flags[699] && !MainManager.instance.flags[39])
					{
						MainManager.instance.playerdata[i].skills.Add(49);
					}
					if (MainManager.instance.flags[39])
					{
						MainManager.instance.playerdata[i].skills.Add(38);
					}
					if (MainManager.instance.flags[18])
					{
						MainManager.instance.playerdata[i].skills.Add(39);
					}
				}
				MainManager.instance.playerdata[i].skills.Add(3);
				if (MainManager.instance.partylevel >= 4)
				{
					MainManager.instance.playerdata[i].skills.Add(32);
				}
				if (MainManager.instance.flags[18])
				{
					MainManager.instance.playerdata[i].skills.Add(6);
				}
				if (MainManager.instance.flags[699])
				{
					MainManager.instance.playerdata[i].skills.Add(10);
				}
				if (MainManager.instance.flags[19])
				{
					MainManager.instance.playerdata[i].skills.Add(5);
				}
				if (MainManager.instance.partylevel >= 17)
				{
					MainManager.instance.playerdata[i].skills.Add(27);
				}
				if (MainManager.instance.flags[533])
				{
					MainManager.instance.playerdata[i].skills.Add(31);
				}
				if (MainManager.BadgeIsEquipped(13))
				{
					MainManager.instance.playerdata[i].skills.Add(9);
				}
				if (MainManager.BadgeIsEquipped(29))
				{
					MainManager.instance.playerdata[i].skills.Add(19);
				}
				if (MainManager.instance.flags[98])
				{
					MainManager.instance.playerdata[i].skills.Add(43);
				}
				if (MainManager.BadgeIsEquipped(51, MainManager.instance.playerdata[i].trueid))
				{
					MainManager.instance.playerdata[i].skills.Add(33);
				}
				if (MainManager.BadgeIsEquipped(73, MainManager.instance.playerdata[i].trueid))
				{
					MainManager.instance.playerdata[i].skills.Add(48);
				}
				if (flag)
				{
					MainManager.instance.playerdata[i].skills.Add(46);
				}
				break;
			case 2:
				if (MainManager.battle == null)
				{
					MainManager.instance.playerdata[i].skills.Add(40);
					if (MainManager.instance.flags[20])
					{
						MainManager.instance.playerdata[i].skills.Add(42);
					}
					if (MainManager.instance.flags[171])
					{
						MainManager.instance.playerdata[i].skills.Add(41);
					}
				}
				MainManager.instance.playerdata[i].skills.Add(4);
				if (MainManager.instance.partylevel >= 6)
				{
					MainManager.instance.playerdata[i].skills.Add(21);
				}
				if (MainManager.instance.flags[171])
				{
					MainManager.instance.playerdata[i].skills.Add(25);
				}
				if (MainManager.instance.partylevel >= 17)
				{
					MainManager.instance.playerdata[i].skills.Add(27);
				}
				if (MainManager.instance.flags[544])
				{
					MainManager.instance.playerdata[i].skills.Add(26);
				}
				if (MainManager.instance.flags[533])
				{
					MainManager.instance.playerdata[i].skills.Add(31);
				}
				if (MainManager.instance.flags[160])
				{
					MainManager.instance.playerdata[i].skills.Add(17);
				}
				if (MainManager.instance.flags[20])
				{
					MainManager.instance.playerdata[i].skills.Add(7);
				}
				if (MainManager.BadgeIsEquipped(52))
				{
					MainManager.instance.playerdata[i].skills.Add(8);
				}
				if (MainManager.BadgeIsEquipped(53))
				{
					MainManager.instance.playerdata[i].skills.Add(22);
				}
				if (MainManager.BadgeIsEquipped(10))
				{
					MainManager.instance.playerdata[i].skills.Add(14);
				}
				if (MainManager.BadgeIsEquipped(31))
				{
					MainManager.instance.playerdata[i].skills.Add(23);
				}
				if (MainManager.BadgeIsEquipped(39))
				{
					MainManager.instance.playerdata[i].skills.Add(15);
				}
				if (MainManager.BadgeIsEquipped(40))
				{
					MainManager.instance.playerdata[i].skills.Add(30);
				}
				if (MainManager.BadgeIsEquipped(37))
				{
					MainManager.instance.playerdata[i].skills.Add(28);
				}
				if (MainManager.BadgeIsEquipped(38))
				{
					MainManager.instance.playerdata[i].skills.Add(29);
				}
				if (MainManager.BadgeIsEquipped(15))
				{
					MainManager.instance.playerdata[i].skills.Add(12);
				}
				if (MainManager.BadgeIsEquipped(16))
				{
					MainManager.instance.playerdata[i].skills.Add(13);
				}
				if (MainManager.instance.partylevel >= 13)
				{
					MainManager.instance.playerdata[i].skills.Add(47);
				}
				if (MainManager.BadgeIsEquipped(51, MainManager.instance.playerdata[i].trueid))
				{
					MainManager.instance.playerdata[i].skills.Add(33);
				}
				if (MainManager.BadgeIsEquipped(73, MainManager.instance.playerdata[i].trueid))
				{
					MainManager.instance.playerdata[i].skills.Add(48);
				}
				if (flag)
				{
					MainManager.instance.playerdata[i].skills.Add(46);
				}
				break;
			}
		}
	}

	// Token: 0x06000525 RID: 1317 RVA: 0x00037248 File Offset: 0x00035448
	public static int[] PartyArray()
	{
		int[] array = new int[]
		{
			MainManager.instance.playerdata.Length
		};
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = MainManager.instance.playerdata[i].trueid;
		}
		return array;
	}

	// Token: 0x06000526 RID: 1318 RVA: 0x00037294 File Offset: 0x00035494
	public static void InventoryFromString(string inp, int itemtype, bool addd)
	{
		if (inp != null && inp.Length > 0)
		{
			if (!addd)
			{
				MainManager.instance.items[itemtype] = new List<int>();
			}
			string[] array = inp.Split(new char[]
			{
				','
			});
			if (array.Length != 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					MainManager.instance.items[itemtype].Add(Convert.ToInt32(array[i]));
				}
			}
		}
	}

	// Token: 0x06000527 RID: 1319 RVA: 0x000372FE File Offset: 0x000354FE
	public static IEnumerator DelayedObj(float delay, string path, Vector3 position, string sound, float destroy)
	{
		yield return new WaitForSeconds(delay);
		if (sound != null)
		{
			MainManager.PlaySound(sound);
		}
		GameObject obj = Object.Instantiate(Resources.Load(path), position, Quaternion.identity) as GameObject;
		if (destroy > 0f)
		{
			Object.Destroy(obj, destroy);
		}
		yield break;
	}

	// Token: 0x06000528 RID: 1320 RVA: 0x0003732C File Offset: 0x0003552C
	public static bool HasFollower(MainManager.AnimIDs followerid)
	{
		if (MainManager.map != null && MainManager.map.tempfollowers != null && MainManager.map.tempfollowers.Count > 0)
		{
			EntityControl[] array = MainManager.map.tempfollowers.ToArray();
			for (int i = 0; i < MainManager.map.tempfollowers.Count; i++)
			{
				if (array[i].originalid == followerid - MainManager.AnimIDs.Bee)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000529 RID: 1321 RVA: 0x000373A0 File Offset: 0x000355A0
	public static bool ForceMoving(EntityControl[] e)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i].forcemove)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600052A RID: 1322 RVA: 0x000373C8 File Offset: 0x000355C8
	public static void PlayTransition(int id, int data, float speed, Color color)
	{
		if (color.r > 0.95f && color.g > 0.95f && color.b > 0.95f)
		{
			color = new Color(0.95f, 0.95f, 0.95f, color.a);
		}
		if (MainManager.transition != null)
		{
			MainManager.instance.StopCoroutine(MainManager.transition);
		}
		if (MainManager.instance.transitionobj != null && MainManager.instance.transitionobj.Length != 0 && (id == 2 || id == 0 || id == 4 || id == 5))
		{
			for (int i = 0; i < MainManager.instance.transitionobj.Length; i++)
			{
				if (MainManager.instance.transitionobj[i] != null)
				{
					Object.Destroy(MainManager.instance.transitionobj[i].gameObject);
				}
			}
		}
		MainManager.transition = MainManager.instance.StartCoroutine(MainManager.Transition(id, data, speed, color));
	}

	// Token: 0x0600052B RID: 1323 RVA: 0x000374B0 File Offset: 0x000356B0
	public static void DestroyAllChildren(Transform parent)
	{
		for (int i = parent.childCount - 1; i >= 0; i--)
		{
			Object.Destroy(parent.GetChild(i).gameObject);
		}
	}

	// Token: 0x0600052C RID: 1324 RVA: 0x000374E4 File Offset: 0x000356E4
	public static void DestroyText(Transform parent)
	{
		for (int i = 0; i < parent.childCount; i++)
		{
			if (parent.GetChild(i).CompareTag("Text"))
			{
				MainManager.DisableLetter(parent.GetChild(i));
			}
		}
	}

	// Token: 0x0600052D RID: 1325 RVA: 0x00037524 File Offset: 0x00035724
	public static void DestroyText(Transform parent, bool destroy)
	{
		for (int i = 0; i < parent.childCount; i++)
		{
			if (parent.GetChild(i).CompareTag("Text"))
			{
				MainManager.DisableLetter(parent.GetChild(i), destroy);
			}
		}
	}

	// Token: 0x0600052E RID: 1326 RVA: 0x00037562 File Offset: 0x00035762
	private static void DisableLetter(Transform input)
	{
		MainManager.DisableLetter(input, true);
	}

	// Token: 0x0600052F RID: 1327 RVA: 0x0003756C File Offset: 0x0003576C
	private static void DisableLetter(Transform input, bool destroy)
	{
		for (int i = 0; i < input.childCount; i++)
		{
			if (input.GetChild(i).GetComponent<ButtonSprite>() != null)
			{
				Object.Destroy(input.GetChild(i).gameObject);
			}
			else if (input.GetChild(i).GetComponent<TextMesh>() != null)
			{
				MainManager.DisableLetter(input.GetChild(i).GetComponent<TextMesh>());
			}
		}
		if (destroy)
		{
			Object.Destroy(input.gameObject);
		}
	}

	// Token: 0x06000530 RID: 1328 RVA: 0x000375E4 File Offset: 0x000357E4
	private static void DisableLetter(TextMesh input)
	{
		if (input.CompareTag("Letter"))
		{
			input.text = "";
			if (input.GetComponent<FontEffects>() != null)
			{
				Object.Destroy(input.GetComponent<FontEffects>());
			}
			input.transform.parent = MainManager.instance.transform;
			input.tag = "Untagged";
			return;
		}
		Object.Destroy(input.gameObject);
	}

	// Token: 0x06000531 RID: 1329 RVA: 0x0003764E File Offset: 0x0003584E
	public static SpriteRenderer NewSolidColor(string name, Color color)
	{
		return MainManager.NewSolidColor(name, color, 100f, Vector3.zero, new Vector2(0.5f, 0.5f));
	}

	// Token: 0x06000532 RID: 1330 RVA: 0x00037670 File Offset: 0x00035870
	public static SpriteRenderer NewSolidColor(string name, Color color, float pixelsperunit, Vector3 position, Vector2 pivot)
	{
		SpriteRenderer spriteRenderer = new GameObject(name).AddComponent<SpriteRenderer>();
		Texture2D texture2D = new Texture2D(1, 1);
		texture2D.SetPixel(0, 0, color);
		texture2D.Apply();
		spriteRenderer.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, 1f, 1f), pivot, pixelsperunit);
		spriteRenderer.transform.position = position;
		return spriteRenderer;
	}

	// Token: 0x06000533 RID: 1331 RVA: 0x000376D3 File Offset: 0x000358D3
	public static void FadeOut()
	{
		MainManager.PlayTransition(1, 0, 0.1f, Color.black);
	}

	// Token: 0x06000534 RID: 1332 RVA: 0x000376E6 File Offset: 0x000358E6
	public static void FadeOut(float speed)
	{
		MainManager.PlayTransition(1, 0, speed, Color.black);
	}

	// Token: 0x06000535 RID: 1333 RVA: 0x000376F5 File Offset: 0x000358F5
	public static void FadeIn()
	{
		MainManager.PlayTransition(0, 0, 0.1f, Color.black);
	}

	// Token: 0x06000536 RID: 1334 RVA: 0x00037708 File Offset: 0x00035908
	public static void FadeIn(float speed)
	{
		MainManager.PlayTransition(0, 0, speed, Color.black);
	}

	// Token: 0x06000537 RID: 1335 RVA: 0x00037717 File Offset: 0x00035917
	public static void SetCameraInstant(Vector3 pos)
	{
		MainManager.SetCamera(pos, 1f);
	}

	// Token: 0x06000538 RID: 1336 RVA: 0x00037724 File Offset: 0x00035924
	public static void SetCamera(Vector3 pos)
	{
		MainManager.SetCamera(pos, 0.035f);
	}

	// Token: 0x06000539 RID: 1337 RVA: 0x00037731 File Offset: 0x00035931
	public static void FadeIn(float speed, Color color)
	{
		MainManager.PlayTransition(0, 0, speed, color);
	}

	// Token: 0x0600053A RID: 1338 RVA: 0x0003773C File Offset: 0x0003593C
	private static IEnumerator Transition(int id, int data, float speed, Color color)
	{
		MainManager.instance.intransition = true;
		int? overridesort = null;
		Vector3[] hp = new Vector3[]
		{
			new Vector3(-6.3f, 3.7f, 10f),
			new Vector3(0f, 3.7f, 10f),
			new Vector3(6.3f, 3.7f, 10f),
			new Vector3(-9.4f, -1.8f, 10f),
			new Vector3(-3.125f, -1.8f, 10f),
			new Vector3(3.125f, -1.8f, 10f),
			new Vector3(9.4f, -1.8f, 10f),
			new Vector3(-6.3f, -7.2f, 10f),
			new Vector3(0f, -7.2f, 10f),
			new Vector3(6.3f, -7.2f, 10f)
		};
		SpriteRenderer r;
		float failsafe;
		SpriteRenderer t;
		switch (id)
		{
		case 0:
			break;
		case 1:
			if (MainManager.instance.transitionobj != null && MainManager.instance.transitionobj.Length != 0)
			{
				r = MainManager.instance.transitionobj[0].GetComponent<SpriteRenderer>();
				failsafe = 600f;
				while (r.color.a > 0f && failsafe >= 0f)
				{
					r.color = Color.Lerp(r.color, Color.clear, MainManager.framestep * speed);
					failsafe -= MainManager.framestep;
					yield return null;
				}
				Object.Destroy(MainManager.instance.transitionobj[0].gameObject);
				goto IL_B94;
			}
			goto IL_B94;
		case 2:
		{
			bool hexagon = data == 4;
			MainManager.instance.transitionobj = new Transform[hexagon ? hp.Length : MainManager.leafpos.Length];
			for (int i = 0; i < MainManager.instance.transitionobj.Length; i++)
			{
				SpriteRenderer spriteRenderer = new GameObject("Leaf" + i).AddComponent<SpriteRenderer>();
				spriteRenderer.sprite = MainManager.leafsprites[data];
				spriteRenderer.color = color;
				spriteRenderer.gameObject.layer = 5;
				spriteRenderer.sortingOrder = 200 + i;
				spriteRenderer.transform.parent = MainManager.GUICamera.transform;
				if (!hexagon)
				{
					spriteRenderer.transform.localEulerAngles = new Vector3(0f, 0f, (float)Random.Range(0, 360));
					spriteRenderer.transform.localPosition = new Vector3(-20f, (float)Random.Range(-1, 1), 10f);
				}
				else
				{
					spriteRenderer.transform.localScale = Vector3.one * 0.75f;
					spriteRenderer.transform.localEulerAngles = Vector3.zero;
					spriteRenderer.transform.localPosition = hp[i] + new Vector3(0f, (float)(10 + 10 * i));
				}
				MainManager.instance.transitionobj[i] = spriteRenderer.transform;
			}
			while (MainManager.instance.transitionobj[0].localPosition != new Vector3(MainManager.leafpos[0].x, MainManager.leafpos[0].y, 10f))
			{
				for (int j = 0; j < MainManager.instance.transitionobj.Length; j++)
				{
					if (!hexagon)
					{
						MainManager.instance.transitionobj[j].localPosition = Vector3.Lerp(MainManager.instance.transitionobj[j].localPosition, new Vector3(MainManager.leafpos[j].x, MainManager.leafpos[j].y, 10f), MainManager.TieFramerate(speed));
						MainManager.instance.transitionobj[j].localEulerAngles = new Vector3(0f, 0f, MainManager.instance.transitionobj[j].localEulerAngles.z + MainManager.GetSqrDistance(MainManager.instance.transitionobj[j].localPosition, new Vector3(MainManager.leafpos[j].x, MainManager.leafpos[j].y, 10f)) / 20f);
					}
					else
					{
						MainManager.instance.transitionobj[j].localPosition = Vector3.Lerp(MainManager.instance.transitionobj[j].localPosition, hp[j], MainManager.TieFramerate(speed));
					}
				}
				yield return null;
			}
			goto IL_B94;
		}
		case 3:
		{
			bool hexagon = data == 4;
			if (MainManager.instance.transitionobj != null && MainManager.instance.transitionobj.Length != 0)
			{
				while (MainManager.GetDistance(MainManager.instance.transitionobj[0].localPosition, new Vector3(20f, 10f)) > 0.1f)
				{
					for (int k = 0; k < MainManager.instance.transitionobj.Length; k++)
					{
						if (hexagon)
						{
							MainManager.instance.transitionobj[k].localPosition = Vector3.Lerp(MainManager.instance.transitionobj[k].localPosition, hp[k] + new Vector3(0f, (float)(-20 - 10 * k)), MainManager.TieFramerate(speed));
						}
						else
						{
							MainManager.instance.transitionobj[k].localPosition = Vector3.Lerp(MainManager.instance.transitionobj[k].localPosition, new Vector3(20f, 10f), MainManager.TieFramerate(speed));
							MainManager.instance.transitionobj[k].localEulerAngles = new Vector3(0f, 0f, MainManager.instance.transitionobj[k].localEulerAngles.z + MainManager.GetSqrDistance(MainManager.instance.transitionobj[k].localPosition, new Vector3(20f, 10f)) / 50f);
						}
					}
					yield return null;
				}
				for (int l = 0; l < MainManager.instance.transitionobj.Length; l++)
				{
					if (MainManager.instance.transitionobj[l] != null)
					{
						Object.Destroy(MainManager.instance.transitionobj[l].gameObject);
					}
				}
				goto IL_B94;
			}
			goto IL_B94;
		}
		case 4:
		case 5:
			t = (Object.Instantiate(Resources.Load("Prefabs/Objects/RoundTransition"), MainManager.GUICamera.transform) as GameObject).GetComponent<SpriteRenderer>();
			if (id == 5)
			{
				t.material.color = color;
				speed /= 2f;
			}
			t.sortingOrder = data;
			overridesort = new int?(data);
			t.transform.localPosition = new Vector3(0f, 0f, 10f);
			t.transform.localEulerAngles = Vector3.zero;
			MainManager.instance.transitionobj = new Transform[]
			{
				t.transform
			};
			while ((id == 4 && t.material.color.a < 0.985f) || (id == 5 && t.material.color.a > 0.05f))
			{
				if (id == 4)
				{
					t.material.color = Color.Lerp(t.material.color, color, MainManager.TieFramerate(speed));
				}
				else
				{
					t.material.color = Color.Lerp(t.material.color, Color.clear, MainManager.TieFramerate(speed));
				}
				t.material.SetFloat("_Cutoff", 1f - t.material.color.a);
				yield return null;
			}
			Object.Destroy(t.gameObject, 0.2f);
			speed = 1f;
			if (id != 4)
			{
				goto IL_B94;
			}
			break;
		default:
			goto IL_B94;
		}
		MainManager.instance.transitionobj = new Transform[]
		{
			new GameObject("Dimmer").transform
		};
		r = MainManager.instance.transitionobj[0].gameObject.AddComponent<SpriteRenderer>();
		Texture2D texture2D = new Texture2D(1, 1);
		texture2D.SetPixel(0, 0, color);
		texture2D.Apply();
		r.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
		r.transform.localScale = new Vector3(3000f, 3000f, 1f);
		r.gameObject.layer = 5;
		if (overridesort == null)
		{
			r.sortingOrder = 9999;
		}
		else
		{
			r.sortingOrder = overridesort.Value;
		}
		r.transform.parent = MainManager.GUICamera.transform;
		r.transform.localPosition = new Vector3(0f, 0f, 10f);
		r.transform.localEulerAngles = Vector3.zero;
		r.color = Color.clear;
		failsafe = 600f;
		while (r.color.a < 1f)
		{
			if (failsafe < 0f)
			{
				break;
			}
			r.color = Color.Lerp(r.color, color, MainManager.framestep * speed);
			failsafe -= MainManager.framestep;
			yield return null;
		}
		IL_B94:
		r = null;
		t = null;
		MainManager.instance.intransition = false;
		yield return null;
		MainManager.transition = null;
		yield break;
	}

	// Token: 0x0600053B RID: 1339 RVA: 0x00037760 File Offset: 0x00035960
	public static void ForceAnim(EntityControl entity)
	{
		if (entity != null && entity.anim != null)
		{
			string text = "";
			string str = entity.animstate.ToString();
			if (entity.animstate < 100)
			{
				MainManager.Animations animstate = (MainManager.Animations)entity.animstate;
				str = animstate.ToString();
			}
			if (entity.height > 0.1f)
			{
				text += "f";
			}
			entity.anim.Play(str + text);
		}
	}

	// Token: 0x0600053C RID: 1340 RVA: 0x000377E0 File Offset: 0x000359E0
	public static void RefreshEntities(bool onlyplayer)
	{
		MainManager.RefreshEntities(false, false, onlyplayer);
	}

	// Token: 0x0600053D RID: 1341 RVA: 0x000377EA File Offset: 0x000359EA
	public static void RefreshEntities()
	{
		MainManager.RefreshEntities(false, false, false);
	}

	// Token: 0x0600053E RID: 1342 RVA: 0x000377F4 File Offset: 0x000359F4
	public static void RefreshEntities(bool forceanim, bool refreshmap)
	{
		MainManager.RefreshEntities(forceanim, refreshmap, false);
	}

	// Token: 0x0600053F RID: 1343 RVA: 0x0000448F File Offset: 0x0000268F
	public static void SetMonitor()
	{
	}

	// Token: 0x06000540 RID: 1344 RVA: 0x00037800 File Offset: 0x00035A00
	public static void RefreshEntities(bool forceanim, bool refreshmap, bool onlyplayer)
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].entity != null)
			{
				MainManager.instance.playerdata[i].entity.hitwall = false;
				MainManager.instance.playerdata[i].entity.animid = MainManager.instance.playerdata[i].animid;
				if (forceanim)
				{
					MainManager.ForceAnim(MainManager.instance.playerdata[i].entity);
				}
				MainManager.instance.playerdata[i].entity.UpdateSpriteMat();
			}
		}
		if (!onlyplayer)
		{
			EntityControl[] array = Object.FindObjectsOfType<EntityControl>();
			for (int j = 0; j < array.Length; j++)
			{
				array[j].oldid = -1;
				array[j].oldstate = -1;
				array[j].emoticoncooldown = 0f;
				array[j].hitwall = false;
				if (array[j].height > 0.1f)
				{
					array[j].oldfly = !array[j].flyinganim;
				}
				if ((array[j].item || MainManager.instance.inevent) && array[j].item)
				{
					array[j].UpdateItem();
				}
				if (forceanim)
				{
					MainManager.ForceAnim(array[j]);
				}
				if (refreshmap && array[j].npcdata != null)
				{
					array[j].npcdata.disguisecooldown = -1;
					if (array[j].npcdata.disguiseobj != null)
					{
						array[j].npcdata.disguiseobj.gameObject.SetActive(false);
						array[j].sprite.enabled = true;
					}
					if (array[j].npcdata.entitytype == NPCControl.NPCType.Object)
					{
						if (((array[j].npcdata.objecttype == NPCControl.ObjectTypes.Dropplet && (array[j].npcdata.data[2] == 0 || !array[j].npcdata.hit)) || array[j].npcdata.objecttype == NPCControl.ObjectTypes.PushRock) && array[j].rigid != null)
						{
							array[j].rigid.useGravity = true;
							array[j].rigid.isKinematic = false;
							if (array[j].npcdata.objecttype == NPCControl.ObjectTypes.Dropplet)
							{
								array[j].onground = false;
								array[j].rigid.velocity = Vector3.zero;
								if (array[j].originalmap == MainManager.map.transform)
								{
									array[j].transform.parent = MainManager.map.transform;
								}
							}
						}
						else if (array[j].npcdata.objecttype == NPCControl.ObjectTypes.Geizer && array[j].npcdata.hit && array[j].npcdata.internaltransform != null && array[j].npcdata.internaltransform.Length != 0)
						{
							array[j].npcdata.internaltransform[3].GetComponentInChildren<ParticleSystem>().Play();
						}
					}
				}
			}
			if (refreshmap && MainManager.map != null)
			{
				TrailRenderer[] array2 = Object.FindObjectsOfType<TrailRenderer>();
				for (int k = 0; k < array2.Length; k++)
				{
					array2[k].Clear();
				}
			}
		}
	}

	// Token: 0x06000541 RID: 1345 RVA: 0x00037B38 File Offset: 0x00035D38
	public static void SetEntityLastPos(bool setit)
	{
		EntityControl[] array = Object.FindObjectsOfType<EntityControl>();
		for (int i = 0; i < array.Length; i++)
		{
			if (setit)
			{
				array[i].lastpos = array[i].transform.position;
			}
			else
			{
				array[i].transform.position = array[i].lastpos;
				if (array[i].animid == -1)
				{
					array[i].rigid.useGravity = false;
					array[i].rigid.isKinematic = true;
				}
			}
		}
	}

	// Token: 0x06000542 RID: 1346 RVA: 0x00037BB0 File Offset: 0x00035DB0
	public static float GetSqrDistance(Vector3 a, Vector3 b)
	{
		return (a - b).sqrMagnitude * 1f;
	}

	// Token: 0x06000543 RID: 1347 RVA: 0x00037BD2 File Offset: 0x00035DD2
	public static float GetSqrDistance(Vector3 a, Vector3 b, bool ignorey)
	{
		if (!ignorey)
		{
			return MainManager.GetSqrDistance(a, b);
		}
		return MainManager.GetSqrDistance(new Vector3(a.x, 0f, a.z), new Vector3(b.x, 0f, b.z));
	}

	// Token: 0x06000544 RID: 1348 RVA: 0x00037C10 File Offset: 0x00035E10
	public static float GetDistance(Vector3 a, Vector3 b)
	{
		return (a - b).magnitude * 1f;
	}

	// Token: 0x06000545 RID: 1349 RVA: 0x00037C34 File Offset: 0x00035E34
	public static float GetDistance(Vector2 a, Vector2 b)
	{
		return (a - b).magnitude * 1f;
	}

	// Token: 0x06000546 RID: 1350 RVA: 0x00037C56 File Offset: 0x00035E56
	public static float GetDistance(Vector3 a, Vector3 b, bool ignoreY)
	{
		if (!ignoreY)
		{
			return MainManager.GetDistance(a, b);
		}
		return MainManager.GetDistance(new Vector3(a.x, 0f, a.z), new Vector3(b.x, 0f, b.z));
	}

	// Token: 0x06000547 RID: 1351 RVA: 0x00037C94 File Offset: 0x00035E94
	public static float GetDistance(float a, float b)
	{
		if (b > a)
		{
			float num = a;
			a = b;
			b = num;
		}
		return Mathf.Abs(a - b);
	}

	// Token: 0x06000548 RID: 1352 RVA: 0x00037CA8 File Offset: 0x00035EA8
	public static float LoopValue(float value, float min, float max)
	{
		return MainManager.LoopValue(value, min, min, false);
	}

	// Token: 0x06000549 RID: 1353 RVA: 0x00037CB3 File Offset: 0x00035EB3
	public static float LoopValue(float value, float min, float max, bool floor)
	{
		while (value < min)
		{
			value += max;
		}
		if (value > max)
		{
			value %= max;
		}
		if (floor)
		{
			value = (float)Mathf.FloorToInt(value);
		}
		return value;
	}

	// Token: 0x0600054A RID: 1354 RVA: 0x00037CD6 File Offset: 0x00035ED6
	public static GameObject PlayParticle(string name, string sound, Vector3 position)
	{
		return MainManager.PlayParticle(name, sound, position, new Vector3(-90f, 0f), 5f);
	}

	// Token: 0x0600054B RID: 1355 RVA: 0x00037CF4 File Offset: 0x00035EF4
	public static GameObject PlayParticle(string name, Vector3 position, float time)
	{
		return MainManager.PlayParticle(name, null, position, new Vector3(-90f, 0f), time);
	}

	// Token: 0x0600054C RID: 1356 RVA: 0x00037D0E File Offset: 0x00035F0E
	public static GameObject PlayParticle(string name, Vector3 position)
	{
		return MainManager.PlayParticle(name, null, position, new Vector3(-90f, 0f), 5f);
	}

	// Token: 0x0600054D RID: 1357 RVA: 0x00037D2C File Offset: 0x00035F2C
	public static GameObject PlayParticle(string name, string sound, Vector3 position, Vector3 angle, float alivetime)
	{
		return MainManager.PlayParticle(name, sound, position, angle, alivetime, 3000);
	}

	// Token: 0x0600054E RID: 1358 RVA: 0x00037D40 File Offset: 0x00035F40
	public static GameObject PlayParticle(string name, string sound, Vector3 position, Vector3 angle, float alivetime, int rendersort)
	{
		if (sound != null)
		{
			MainManager.PlaySound(sound);
		}
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Particles/" + name), position, Quaternion.Euler(angle)) as GameObject;
		Renderer component = gameObject.GetComponent<Renderer>();
		if (component != null)
		{
			component.material.renderQueue = rendersort;
		}
		if (alivetime > 0f)
		{
			Object.Destroy(gameObject, alivetime);
		}
		return gameObject;
	}

	// Token: 0x0600054F RID: 1359 RVA: 0x00037DA8 File Offset: 0x00035FA8
	public static Color RainbowColor(int variant)
	{
		return MainManager.RainbowColor(variant, 5.9f, 0f, 1f, 1f);
	}

	// Token: 0x06000550 RID: 1360 RVA: 0x00037DC4 File Offset: 0x00035FC4
	public static Color RainbowColor(int variant, float colorspeed)
	{
		return MainManager.RainbowColor(variant, colorspeed, 0f, 1f, 1f);
	}

	// Token: 0x06000551 RID: 1361 RVA: 0x00037DDC File Offset: 0x00035FDC
	public static Color RainbowColor(int variant, float colorspeed, float min, float limit, float alpha)
	{
		return new Color(Mathf.Clamp(Mathf.Sin((Time.time + (float)variant + 20f) * colorspeed) * 2f, min, limit), Mathf.Clamp(Mathf.Sin((Time.time + (float)variant) * colorspeed) * 2f, min, limit), Mathf.Clamp(Mathf.Sin((Time.time + (float)variant + 60f) * colorspeed) * 2f, min, limit), alpha);
	}

	// Token: 0x06000552 RID: 1362 RVA: 0x00037E50 File Offset: 0x00036050
	public static float ColorMagnitude(Color color)
	{
		return new Vector3(color.r, color.g, color.b).magnitude;
	}

	// Token: 0x06000553 RID: 1363 RVA: 0x00037E7C File Offset: 0x0003607C
	public static float Snap(in float value, in float step)
	{
		if (step != 0f)
		{
			return Mathf.Floor(value / step + 0.5f) * step;
		}
		return value;
	}

	// Token: 0x06000554 RID: 1364 RVA: 0x00037E9D File Offset: 0x0003609D
	public static Vector3 Snap(in Vector3 value, in Vector3 step)
	{
		return new Vector3(MainManager.Snap(value.x, step.x), MainManager.Snap(value.y, step.y), MainManager.Snap(value.z, step.z));
	}

	// Token: 0x06000555 RID: 1365 RVA: 0x00037ED7 File Offset: 0x000360D7
	public static IEnumerator LevelUpMessage()
	{
		string[] ld = Resources.Load<TextAsset>("Data/LevelData").ToString().Split(new char[]
		{
			'\n'
		});
		bool canmessage = true;
		int num9;
		for (int i = 0; i < ld.Length; i = num9 + 1)
		{
			string[] t = ld[i].Split(new char[]
			{
				','
			});
			if (Convert.ToInt32(t[0]) == MainManager.instance.partylevel)
			{
				yield return new WaitForSeconds(0.5f);
				int num = Convert.ToInt32(t[1]);
				int num2 = 141 + num;
				if (num == 0)
				{
					int num3 = Convert.ToInt32(t[2]);
					if (MainManager.HasPlayer(num3))
					{
						MainManager.instance.flagstring[0] = MainManager.menutext[num3 + 46];
						MainManager.instance.flagstring[1] = MainManager.skilldata[Convert.ToInt32(t[3]), 0];
					}
					else
					{
						canmessage = false;
					}
				}
				else if (num == 1)
				{
					int num4 = Convert.ToInt32(t[2]);
					MainManager.instance.flagstring[0] = MainManager.menutext[num4 + 46];
					MainManager.instance.flagstring[1] = t[4] + " ";
					switch (Convert.ToInt32(t[3]))
					{
					case 0:
					{
						string[] array = MainManager.instance.flagstring;
						int num5 = 1;
						array[num5] += MainManager.menutext[16];
						MainManager.AddStatBonus(MainManager.StatBonus.Attack, Convert.ToInt32(t[4]), Convert.ToInt32(t[2]));
						break;
					}
					case 1:
					{
						string[] array2 = MainManager.instance.flagstring;
						int num6 = 1;
						array2[num6] += MainManager.menutext[17];
						MainManager.AddStatBonus(MainManager.StatBonus.Defense, Convert.ToInt32(t[4]), Convert.ToInt32(t[2]));
						break;
					}
					case 2:
					{
						string[] array3 = MainManager.instance.flagstring;
						int num7 = 1;
						array3[num7] += MainManager.menutext[14];
						MainManager.AddStatBonus(MainManager.StatBonus.HP, Convert.ToInt32(t[4]), Convert.ToInt32(t[2]));
						break;
					}
					}
					canmessage = MainManager.HasPlayer(num4);
				}
				else if (num == 2)
				{
					int num8 = Convert.ToInt32(t[3]);
					if (Convert.ToInt32(t[2]) == 0)
					{
						MainManager.instance.flagstring[0] = num8 + " " + MainManager.menutext[15];
						MainManager.instance.tp += num8;
						MainManager.instance.maxtp += num8;
						if (num8 == 3)
						{
							MainManager.AddStatBonus(MainManager.StatBonus.TP, num8, -1);
						}
						else
						{
							for (int j = 0; j < num8; j++)
							{
								MainManager.AddStatBonus(MainManager.StatBonus.TP, 1, -1);
							}
						}
					}
					else
					{
						MainManager.instance.flagstring[0] = num8 + " " + MainManager.menutext[19];
						MainManager.instance.bp += num8;
						MainManager.instance.maxbp += num8;
						MainManager.AddStatBonus(MainManager.StatBonus.MP, num8, -1);
					}
				}
				else if (num == 3)
				{
					MainManager.instance.flagstring[0] = t[2];
					MainManager.instance.maxitems += Convert.ToInt32(t[2]);
					MainManager.instance.flagstring[1] = MainManager.instance.maxitems.ToString();
				}
				if (canmessage)
				{
					MainManager.instance.StartCoroutine(MainManager.SetText("|boxstyle,4||spd,0||halfline||center|" + MainManager.menutext[num2], true, Vector3.zero, null, null));
					while (MainManager.instance.message)
					{
						yield return null;
					}
					yield return null;
				}
			}
			t = null;
			num9 = i;
		}
		MainManager.ApplyStatBonus();
		yield return null;
		MainManager.instance.flagvar[2] = 1;
		yield break;
	}

	// Token: 0x06000556 RID: 1366 RVA: 0x00037EE0 File Offset: 0x000360E0
	public static void SetPlayers(Vector3[] newentitypos)
	{
		if (newentitypos != null)
		{
			for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
			{
				if (MainManager.instance.playerdata[i].entity != null)
				{
					Object.Destroy(MainManager.instance.playerdata[i].entity.gameObject);
				}
			}
		}
		MainManager.SetPlayers();
		if (newentitypos != null)
		{
			for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
			{
				if (MainManager.instance.playerdata[j].entity != null)
				{
					MainManager.instance.playerdata[j].entity.transform.position = newentitypos[j];
				}
			}
		}
	}

	// Token: 0x06000557 RID: 1367 RVA: 0x00037FA8 File Offset: 0x000361A8
	public static void SetPlayers()
	{
		EntityControl entityControl = null;
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			MainManager.instance.playerdata[i].entity = EntityControl.CreateNewEntity("Player " + i);
			MainManager.instance.playerdata[i].entity.animid = MainManager.instance.playerdata[i].animid;
			MainManager.instance.playerdata[i].entity.alwaysactive = true;
			MainManager.instance.playerdata[i].condition = new List<int[]>();
			if (entityControl == null)
			{
				MainManager.instance.playerdata[i].entity.gameObject.AddComponent<PlayerControl>();
				entityControl = MainManager.instance.playerdata[i].entity;
				MainManager.instance.camtarget = MainManager.instance.playerdata[i].entity.transform;
				MainManager.instance.playerdata[i].entity.tag = "Player";
			}
			else if (entityControl != null)
			{
				EntityControl following = entityControl;
				MainManager.instance.playerdata[i].entity.following = following;
				MainManager.instance.playerdata[i].entity.followoffset = (float)i * 0.1f;
				MainManager.instance.playerdata[i].entity.gameObject.layer = 9;
				MainManager.instance.playerdata[i].entity.transform.position = new Vector3(MainManager.instance.playerdata[i].entity.transform.position.x, MainManager.instance.playerdata[i].entity.transform.position.y, MainManager.instance.playerdata[i].entity.transform.position.z + MainManager.instance.playerdata[i].entity.followoffset);
				MainManager.instance.playerdata[i].entity.tag = "PFollower";
				MainManager.instance.playerdata[i].entity.mainparty = true;
				entityControl = MainManager.instance.playerdata[i].entity;
				MainManager.instance.playerdata[i - 1].entity.followedby = MainManager.instance.playerdata[i].entity.transform;
			}
		}
	}

	// Token: 0x06000558 RID: 1368 RVA: 0x00038288 File Offset: 0x00036488
	public static EntityControl[] GetPartyEntities()
	{
		List<EntityControl> list = new List<EntityControl>();
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			list.Add(MainManager.instance.playerdata[i].entity);
		}
		return list.ToArray();
	}

	// Token: 0x06000559 RID: 1369 RVA: 0x000382D4 File Offset: 0x000364D4
	public static EntityControl[] GetPartyEntities(bool idorder)
	{
		if (!idorder)
		{
			return MainManager.GetPartyEntities();
		}
		List<EntityControl> list = new List<EntityControl>();
		EntityControl[] array = new EntityControl[]
		{
			MainManager.GetEntity(-4),
			MainManager.GetEntity(-5),
			MainManager.GetEntity(-6)
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				list.Add(array[i]);
			}
		}
		return list.ToArray();
	}

	// Token: 0x0600055A RID: 1370 RVA: 0x0003833D File Offset: 0x0003653D
	private static IEnumerator LowerVolume(int musicid, float volume, float frametime, bool percent)
	{
		float vol = MainManager.music[musicid].volume;
		float target = percent ? (vol * volume) : volume;
		float a = 0f;
		do
		{
			MainManager.music[musicid].volume = Mathf.Lerp(vol, target, a / frametime);
			a += MainManager.framestep;
			yield return null;
		}
		while (a < frametime + 1f);
		yield break;
	}

	// Token: 0x0600055B RID: 1371 RVA: 0x00038361 File Offset: 0x00036561
	public static IEnumerator ChapterName(int chapterid)
	{
		SpriteRenderer dm = null;
		SpriteRenderer back = MainManager.NewUIObject("back", MainManager.GUICamera.transform, new Vector3(0f, 0.5f, 10f), Vector3.one * 1.2f, Resources.LoadAll<Sprite>("Sprites/GUI/textbox")[12], -1).GetComponent<SpriteRenderer>();
		back.color = Color.clear;
		float vol = MainManager.music[0].volume;
		MainManager.instance.StartCoroutine(MainManager.LowerVolume(0, 0.01f, 100f, true));
		if (chapterid < 5)
		{
			MainManager.PlayTransition(0, -5, 0.01f, new Color(1f, 1f, 1f, 0.5f));
			yield return null;
			dm = MainManager.GetTransitionSprite();
			while (dm.color.a < 0.35f)
			{
				yield return null;
			}
		}
		AudioSource audioSource = MainManager.PlaySound("ch" + (chapterid + 2));
		if (audioSource != null)
		{
			audioSource.volume = MainManager.musicvolume;
		}
		Coroutine c = MainManager.instance.StartCoroutine(MainManager.GradualColor(back, (chapterid == 5) ? Color.black : Color.white, 200f, true));
		string text = MainManager.menutext[173] + " " + (chapterid + 2);
		MainManager.instance.StartCoroutine(MainManager.SetText(string.Concat(new object[]
		{
			"|setbreak,9999,true||boxstyle,-1||size,1.3||spd,0||font,2||rainbow|",
			text,
			"|rainbow||line||halfline||font,0||size,1||dropshadow,0.075,-0.075|",
			(chapterid < 5) ? "|color,4|" : "|color,0|",
			"|fadeletter|              ",
			MainManager.menutext[176 + chapterid],
			"|fwait,",
			(audioSource == null) ? 5f : (audioSource.clip.length * 0.95f),
			"|"
		}), true, new Vector3(-5f, -2.75f, 10f), null, null));
		yield return null;
		MainManager.instance.blinker.transform.localPosition = new Vector3(0f, -6f, 0.5f);
		while (MainManager.instance.message)
		{
			yield return null;
		}
		MainManager.instance.StopCoroutine(c);
		MainManager.instance.StartCoroutine(MainManager.LowerVolume(0, vol, 30f, false));
		MainManager.instance.StartCoroutine(MainManager.GradualColor(back, Color.clear, 15f, true));
		Object.Destroy(back.gameObject, 1f);
		if (chapterid < 5)
		{
			MainManager.PlayTransition(1, 0, 0.025f, Color.black);
			yield return null;
			dm = MainManager.GetTransitionSprite();
			while (dm.color.a > 0.05f)
			{
				yield return null;
			}
		}
		MainManager.chaptername = null;
		yield break;
	}

	// Token: 0x0600055C RID: 1372 RVA: 0x00038370 File Offset: 0x00036570
	public static float TieFramerate(float value)
	{
		return value * Time.smoothDeltaTime * 60f;
	}

	// Token: 0x0600055D RID: 1373 RVA: 0x0003837F File Offset: 0x0003657F
	public static IEnumerator ShakeObject(Transform obj, Vector3 shake, float frametime, bool returntostart)
	{
		Vector3 p = obj.position;
		float a = 0f;
		do
		{
			obj.position = p + MainManager.RandomVector(shake);
			a += MainManager.TieFramerate(1f);
			yield return null;
		}
		while (a < frametime + 1f);
		if (returntostart)
		{
			obj.position = p;
		}
		yield break;
	}

	// Token: 0x0600055E RID: 1374 RVA: 0x000383A4 File Offset: 0x000365A4
	public static void LoadMap(int id, bool recreateplayers)
	{
		if (recreateplayers)
		{
			for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
			{
				if (MainManager.instance.playerdata[i].entity != null)
				{
					Object.Destroy(MainManager.instance.playerdata[i].entity.gameObject);
				}
			}
			MainManager.player = null;
		}
		MainManager.LoadMap(id);
	}

	// Token: 0x0600055F RID: 1375 RVA: 0x00038413 File Offset: 0x00036613
	public static void LoadMap()
	{
		MainManager.LoadMap(Convert.ToInt32(MainManager.map.name));
	}

	// Token: 0x06000560 RID: 1376 RVA: 0x0003842C File Offset: 0x0003662C
	public static void LoadMap(int id)
	{
		MainManager.instance.changecamspeed = false;
		MainManager.instance.camanglespeed = 0.1f;
		MainManager.instance.camanglechange = false;
		MainManager.instance.flags[400] = false;
		if (MainManager.map != null)
		{
			for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
			{
				if (MainManager.instance.playerdata[i].entity != null)
				{
					MainManager.instance.playerdata[i].entity.transform.parent = null;
					if (i == MainManager.instance.playerdata.Length - 1)
					{
						MainManager.instance.playerdata[i].entity.followedby = null;
					}
				}
			}
			EntityControl[] array = MainManager.map.tempfollowers.ToArray();
			if (array != null && array.Length != 0)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j] != null)
					{
						Object.Destroy(array[j].gameObject);
					}
				}
			}
			Object.Destroy(MainManager.map.gameObject);
			if (!MainManager.instance.inevent)
			{
				GC.Collect();
				Resources.UnloadUnusedAssets();
			}
			GameObject[] array2 = GameObject.FindGameObjectsWithTag("MapObj");
			for (int k = 0; k < array2.Length; k++)
			{
				Object.Destroy(array2[k].gameObject);
			}
		}
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Maps/" + (MainManager.Maps)id)) as GameObject;
		MainManager.map = gameObject.GetComponent<MapControl>();
		gameObject.name = id.ToString();
		if (MainManager.player == null)
		{
			MainManager.SetPlayers();
		}
		MainManager.instance.ForceLoadSprites();
		if (MainManager.player != null)
		{
			MainManager.player.entity.sound.loop = false;
			MainManager.player.entity.sound.Stop();
			MainManager.player.pausecooldown = 120f;
		}
	}

	// Token: 0x06000561 RID: 1377 RVA: 0x00038624 File Offset: 0x00036824
	public static void FixSamira()
	{
		if (MainManager.instance.samiramusics != null && MainManager.instance.samiramusics.Count > 0)
		{
			int[][] array = MainManager.instance.samiramusics.ToArray();
			int i = array.Length - 1;
			while (i >= 0)
			{
				MainManager.Musics musics = (MainManager.Musics)array[i][0];
				if (musics <= MainManager.Musics.Water)
				{
					if (musics == MainManager.Musics.Title || musics - MainManager.Musics.Wind <= 1)
					{
						goto IL_59;
					}
				}
				else if (musics == MainManager.Musics.MachineHum || musics == MainManager.Musics.Breathing)
				{
					goto IL_59;
				}
				IL_69:
				i--;
				continue;
				IL_59:
				MainManager.instance.samiramusics.RemoveAt(i);
				goto IL_69;
			}
		}
	}

	// Token: 0x06000562 RID: 1378 RVA: 0x000386A4 File Offset: 0x000368A4
	public static Transform Create9Box(Vector3 position, Vector2 size, int type, int sortorder, Color color, bool grow)
	{
		Transform transform = new GameObject("9Box").transform;
		SpriteRenderer spriteRenderer = transform.gameObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/GUI/9Box/box" + type);
		spriteRenderer.gameObject.layer = 5;
		spriteRenderer.color = color;
		spriteRenderer.sortingOrder = sortorder;
		spriteRenderer.size = size;
		spriteRenderer.drawMode = SpriteDrawMode.Tiled;
		spriteRenderer.tileMode = SpriteTileMode.Adaptive;
		if (grow)
		{
			transform.transform.localScale = Vector3.zero;
			transform.gameObject.AddComponent<DialogueAnim>();
		}
		else
		{
			transform.transform.localScale = Vector3.one;
		}
		transform.transform.parent = MainManager.GUICamera.transform;
		transform.transform.localEulerAngles = Vector3.zero;
		transform.transform.localPosition = position;
		return transform;
	}

	// Token: 0x06000563 RID: 1379 RVA: 0x0003877A File Offset: 0x0003697A
	public static IEnumerator LateAngle(Transform obj, Vector3 angle, bool local, WaitForSeconds time)
	{
		yield return time;
		if (local)
		{
			obj.localEulerAngles = angle;
		}
		else
		{
			obj.eulerAngles = angle;
		}
		yield break;
	}

	// Token: 0x06000564 RID: 1380 RVA: 0x000387A0 File Offset: 0x000369A0
	public static bool CheckAllBool(bool[] array, int[] values, bool state)
	{
		for (int i = 0; i < values.Length; i++)
		{
			if (array[values[i]] != state)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000565 RID: 1381 RVA: 0x000387C8 File Offset: 0x000369C8
	private static float GetLetterOffset(TextMesh letter, float size)
	{
		if (letter == null)
		{
			return 0.3f * size;
		}
		return (letter.GetComponent<MeshRenderer>().bounds.extents.x * 2f + -2f) * size;
	}

	// Token: 0x06000566 RID: 1382 RVA: 0x0003880C File Offset: 0x00036A0C
	public static int FontID(int id)
	{
		if (id < 0)
		{
			return Mathf.Abs(id + 1);
		}
		if (id == 2)
		{
			return 1;
		}
		return id;
	}

	// Token: 0x06000567 RID: 1383 RVA: 0x00038822 File Offset: 0x00036A22
	public static GameObject WaterSplash(Vector3 pos, Vector3 size)
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Objects/WaterSplash"), pos, Quaternion.identity) as GameObject;
		gameObject.transform.localScale = size;
		Object.Destroy(gameObject, 0.75f);
		return gameObject;
	}

	// Token: 0x06000568 RID: 1384 RVA: 0x00038855 File Offset: 0x00036A55
	public static void SystemText(string text, Transform parent, Vector3 pos)
	{
		MainManager.instance.StartCoroutine(MainManager.SetText(text, false, pos, parent, null));
	}

	// Token: 0x06000569 RID: 1385 RVA: 0x0003886C File Offset: 0x00036A6C
	public static float GetLetterOffset(char letter, int fontid, float size)
	{
		if (letter == ' ')
		{
			return 0.3f * size;
		}
		fontid = MainManager.FontID(fontid);
		CharacterInfo characterInfo = default(CharacterInfo);
		MainManager.fonts[fontid].RequestCharactersInTexture(letter.ToString() ?? "");
		if (MainManager.fonts[fontid].GetCharacterInfo(letter, out characterInfo))
		{
			return ((float)characterInfo.advance * 0.75f + -2f) * (size / (float)MainManager.fonts[fontid].fontSize);
		}
		return 0.3f * size;
	}

	// Token: 0x0600056A RID: 1386 RVA: 0x000388EF File Offset: 0x00036AEF
	public static Vector3 VectorFromString(string[] inputs)
	{
		return new Vector3(Convert.ToSingle(inputs[0]), Convert.ToSingle(inputs[1]), Convert.ToSingle(inputs[2]));
	}

	// Token: 0x0600056B RID: 1387 RVA: 0x0003890E File Offset: 0x00036B0E
	public static IEnumerator LightingBolt(Vector3 a, Vector3 b, int segments, float variant, Color color, float frametime, float lineduration)
	{
		GameObject t = Object.Instantiate(Resources.Load("Prefabs/Particles/LightingBolt"), a, Quaternion.identity) as GameObject;
		TrailRenderer component = t.GetComponent<TrailRenderer>();
		component.material.color = color;
		component.colorGradient = new Gradient
		{
			colorKeys = new GradientColorKey[]
			{
				new GradientColorKey(color, 0f),
				new GradientColorKey(color, 1f)
			}
		};
		component.time = lineduration;
		int amt = Mathf.Clamp(segments, 2, segments);
		int num;
		for (int i = 0; i < amt; i = num + 1)
		{
			Vector3 tp = t.transform.position;
			Vector3 pp = Vector3.Lerp(a, b, (float)i / (float)(amt - 1));
			if (i > 0 && i < amt - 1)
			{
				pp += MainManager.RandomVector(variant);
			}
			float c = frametime / (float)amt;
			float aa = 0f;
			do
			{
				t.transform.position = Vector3.Lerp(tp, pp, c);
				aa += MainManager.TieFramerate(1f);
				yield return null;
			}
			while (aa < c + 1f);
			tp = default(Vector3);
			pp = default(Vector3);
			num = i;
		}
		Object.Destroy(t, 1f);
		yield return null;
		yield break;
	}

	// Token: 0x0600056C RID: 1388 RVA: 0x0003894C File Offset: 0x00036B4C
	private static bool ContainsLine(string command)
	{
		if (!command.Contains("line"))
		{
			return false;
		}
		MainManager.Commands commands = (MainManager.Commands)Enum.Parse(typeof(MainManager.Commands), command, true);
		if (commands != MainManager.Commands.Shopline)
		{
			return commands != MainManager.Commands.Unpauseline || MainManager.pausemenu == null;
		}
		return MainManager.instance.inlist || (MainManager.player != null && MainManager.player.npc != null && MainManager.player.npc.Count > 0 && MainManager.player.npc[0].interacttype == NPCControl.Interaction.Shop);
	}

	// Token: 0x0600056D RID: 1389 RVA: 0x000389F4 File Offset: 0x00036BF4
	private static string ReplaceFunctions(string text)
	{
		string text2 = "";
		if (text != null)
		{
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '|')
				{
					string text3 = "|";
					int num = i + 1;
					while (num < text.Length && text[num] != '|')
					{
						text3 += text[num].ToString();
						num++;
					}
					if (text3.Contains("sstring"))
					{
						text2 += MainManager.instance.flagstring[Convert.ToInt32(text3.Replace("|sstring,", ""))];
					}
					else if (text3.Contains("|menu"))
					{
						string[] array = text3.Replace("|", "").Split(new char[]
						{
							','
						});
						if (array.Length == 2)
						{
							text2 += MainManager.menutext[Convert.ToInt32(array[1])];
						}
						else
						{
							array[1] = MainManager.menutext[Convert.ToInt32(array[1])];
							string a = array[2];
							if (!(a == "1"))
							{
								if (!(a == "2"))
								{
									if (a == "3")
									{
										text2 = text2 + array[1][0].ToString() + "-" + array[1].ToUpper();
									}
								}
								else
								{
									text2 += array[1].ToUpper();
								}
							}
							else
							{
								text2 = text2 + array[1][0].ToString() + "-" + array[1];
							}
						}
					}
					else
					{
						text2 = text2 + text3 + "|";
					}
					i += text3.Length;
				}
				else
				{
					text2 += text[i].ToString();
				}
			}
		}
		return text2;
	}

	// Token: 0x0600056E RID: 1390 RVA: 0x00038BD0 File Offset: 0x00036DD0
	private static string OrganizeLines(string text, float maxoffset, float size, int fontid)
	{
		if (MainManager.languageid == 3)
		{
			return text;
		}
		string text2 = "";
		string text3 = "";
		bool flag = false;
		float num = 0f;
		float num2 = 0f;
		text = MainManager.ReplaceFunctions(text);
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] != '\n')
			{
				if (text[i] == '|')
				{
					string text4 = "";
					int num3 = i + 1;
					while (num3 < text.Length && text[num3] != '|')
					{
						text4 += text[num3].ToString();
						num3++;
					}
					text3 = text3 + "|" + text4 + "|";
					bool flag2 = false;
					string[] array = text4.Split(new char[]
					{
						','
					});
					if (array[0].Contains("next") || array[0].Contains("blank") || MainManager.ContainsLine(array[0]))
					{
						if (num + num2 > maxoffset)
						{
							text2 += "\n";
						}
						text2 += text3;
						num = 0f;
						text3 = "";
						num2 = 0f;
					}
					else if (array[0].Contains("button"))
					{
						num += 0.7f;
						string text5 = InputIO.KeyboardString(Convert.ToInt32(text4.Replace("button,", "")));
						if (text5.Length > 1 && !text5.Contains("Arrow"))
						{
							num += 0.7f;
						}
						num2 = 0f;
					}
					else if (array[0].Contains("size") && array[0] != "battlesize")
					{
						if (array[1] != "multi")
						{
							size = Convert.ToSingle(array[1]);
						}
					}
					else if (array[0].Contains("singlebreak"))
					{
						flag = true;
						maxoffset = Convert.ToSingle(array[1]);
						if (MainManager.languageid == 6)
						{
							maxoffset *= 1.15f;
						}
					}
					if (!flag2)
					{
						i += text4.Length + 1;
					}
				}
				else if (text[i] != ' ')
				{
					text3 += text[i].ToString();
					num2 += (float)Mathf.FloorToInt(MainManager.GetLetterOffset(text[i], fontid, size) * 25f) / 25f * (flag ? 0.975f : 1f);
				}
				else
				{
					num += 0.3f * size;
					if (num + num2 > maxoffset)
					{
						if (flag)
						{
							text2 += "|line|";
						}
						else
						{
							text2 += "\n";
						}
						num = (((MainManager.languageid > 0 && !MainManager.AsianLang()) || (MainManager.languageid == 0 && MainManager.map != null && MainManager.map.englishbreakfix && MainManager.instance.message)) ? num2 : 0f);
					}
					else
					{
						num += num2;
					}
					text2 = text2 + text3 + " ";
					text3 = "";
					num2 = 0f;
				}
			}
		}
		if (num2 + num > maxoffset)
		{
			text2 += "\n";
		}
		text2 += text3;
		if (flag)
		{
			text2 = text2.Replace("\n", "|line|").Replace("|line||line|", "|line|");
		}
		return text2;
	}

	// Token: 0x0600056F RID: 1391 RVA: 0x00038F2B File Offset: 0x0003712B
	public static float GetDistance01(float start, float end, float multiplier)
	{
		return Mathf.Clamp01(1f - MainManager.GetDistance(start, end) * multiplier);
	}

	// Token: 0x06000570 RID: 1392 RVA: 0x00038F44 File Offset: 0x00037144
	public static float GetPercentage(float start, float end, float currentvalue)
	{
		float value = end - start;
		float num = end - currentvalue;
		return 1f - num / Mathf.Clamp(value, 0.001f, float.PositiveInfinity);
	}

	// Token: 0x06000571 RID: 1393 RVA: 0x00038F74 File Offset: 0x00037174
	public static void ScreamWaves(Vector3 pos)
	{
		MainManager.instance.StartCoroutine(MainManager.Waves(pos, 5, 20f, EventControl.quartersec, false, false, null));
	}

	// Token: 0x06000572 RID: 1394 RVA: 0x00038FA8 File Offset: 0x000371A8
	public static IEnumerator Waves(Vector3 pos, int ammount, float frametime, WaitForSeconds delay, bool invert, bool tridimentional, Color? color)
	{
		GameObject sphere = Resources.Load<GameObject>("Prefabs/Objects/SphereGlowEffect");
		Vector3 max = new Vector3(50f, 50f, (float)(tridimentional ? 50 : 1));
		int num;
		for (int i = 0; i < ammount; i = num + 1)
		{
			Transform transform = Object.Instantiate<GameObject>(sphere, pos, Quaternion.identity).transform;
			transform.localScale = (invert ? max : Vector3.zero);
			if (color != null)
			{
				transform.GetComponent<Renderer>().material.color = color.Value;
			}
			MainManager.instance.StartCoroutine(MainManager.GradualScale(transform, invert ? Vector3.zero : max, frametime, true));
			if (delay != null)
			{
				yield return delay;
			}
			num = i;
		}
		yield break;
	}

	// Token: 0x06000573 RID: 1395 RVA: 0x00038FE4 File Offset: 0x000371E4
	public static IEnumerator GradualScale(Transform obj, Vector3 target, float frametime, bool destroy)
	{
		float a = 0f;
		Vector3 ss = obj.localScale;
		do
		{
			obj.localScale = Vector3.Lerp(ss, target, a / frametime);
			a += MainManager.TieFramerate(1f);
			yield return null;
		}
		while (a < frametime + 1f);
		if (destroy)
		{
			Object.Destroy(obj.gameObject);
		}
		yield break;
	}

	// Token: 0x06000574 RID: 1396 RVA: 0x00039008 File Offset: 0x00037208
	public static IEnumerator SetText(string text, Vector3 position, Transform parent)
	{
		MainManager.instance.StartCoroutine(MainManager.SetText(text, 0, null, false, false, position, Vector3.zero, Vector2.one, parent, null));
		yield return null;
		yield break;
	}

	// Token: 0x06000575 RID: 1397 RVA: 0x00039025 File Offset: 0x00037225
	public static IEnumerator SetText(string text, Transform parent, NPCControl caller)
	{
		MainManager.instance.StartCoroutine(MainManager.SetText(text, 0, new float?(MainManager.messagebreak), true, false, Vector3.zero, Vector3.zero, Vector2.one, parent, caller));
		yield return null;
		yield break;
	}

	// Token: 0x06000576 RID: 1398 RVA: 0x00039042 File Offset: 0x00037242
	public static IEnumerator SetText(string text, bool dialogue, Vector3 position, Transform parent, NPCControl caller)
	{
		float? linebreak = new float?(MainManager.messagebreak);
		if (!dialogue)
		{
			linebreak = null;
		}
		MainManager.instance.StartCoroutine(MainManager.SetText(text, 0, linebreak, dialogue, false, position, Vector3.zero, Vector2.one, parent, caller));
		yield return null;
		yield break;
	}

	// Token: 0x06000577 RID: 1399 RVA: 0x0003906E File Offset: 0x0003726E
	public static IEnumerator InnSleep(NPCControl caller, Vector3? position, bool changemusic, bool nofadeout)
	{
		MainManager.instance.StartCoroutine(MainManager.InnSleep(caller, position, changemusic, nofadeout, null, null));
		yield return null;
		yield break;
	}

	// Token: 0x06000578 RID: 1400 RVA: 0x00039092 File Offset: 0x00037292
	public static IEnumerator InnSleep(NPCControl caller, Vector3? position, bool changemusic, bool nofadeout, Vector3? camn, Vector3? camp)
	{
		MainManager.instance.showmoney = -1f;
		string tm = (MainManager.music[0].clip != null) ? MainManager.music[0].clip.name : null;
		MainManager.PlaySound("Inn", 5);
		MainManager.FadeMusic(0.1f);
		if (caller != null)
		{
			caller.entity.talking = false;
		}
		MainManager.PlayTransition(4, 9999, 0.01f, Color.black);
		yield return new WaitForSeconds(0.1f);
		float failsafe = 700f;
		while (MainManager.transition != null && failsafe > 0f)
		{
			failsafe -= MainManager.framestep;
			yield return null;
		}
		if (position != null)
		{
			float ts = MainManager.instance.camspeed;
			MainManager.instance.camspeed = 1f;
			Vector3 value = position.Value;
			MainManager.player.transform.position = value;
			if (camn != null)
			{
				MainManager.map.camlimitneg = camn.Value;
			}
			if (camp != null)
			{
				MainManager.map.camlimitpos = camp.Value;
			}
			yield return new WaitForSeconds(0.1f);
			MainManager.TeleportFollowers(true);
			MainManager.instance.camspeed = ts;
		}
		failsafe = 60f;
		while (MainManager.sounds[5].isPlaying || failsafe > 0f)
		{
			failsafe -= MainManager.framestep;
			yield return null;
		}
		if (!nofadeout)
		{
			MainManager.PlayTransition(1, 0, 0.15f, Color.clear);
			yield return new WaitForSeconds(0.5f);
		}
		MainManager.Heal();
		MainManager.instance.hudcooldown = 150f;
		if (changemusic)
		{
			MainManager.ChangeMusic(tm);
		}
		MainManager.player.npc = new List<NPCControl>();
		if (caller != null)
		{
			caller.entity.emoticoncooldown = 0f;
		}
		MainManager.chaptername = null;
		yield break;
	}

	// Token: 0x06000579 RID: 1401 RVA: 0x000390C8 File Offset: 0x000372C8
	public static bool FrameDifference(int frames)
	{
		return Time.frameCount % Mathf.CeilToInt((float)((MainManager.vsync == 0) ? Application.targetFrameRate : Screen.currentResolution.refreshRate) / 60f) == 0;
	}

	// Token: 0x0600057A RID: 1402 RVA: 0x00039108 File Offset: 0x00037308
	public static string GetDialogueText(int id)
	{
		if (id < 0)
		{
			return MainManager.commondialogue[Mathf.Abs(id) - 1];
		}
		if (id > MainManager.map.dialogues.Length - 1)
		{
			if (MainManager.map.useglobalcommand)
			{
				MainManager.map.currentline = -1;
			}
			return null;
		}
		if (MainManager.map.useglobalcommand)
		{
			MainManager.map.currentline = id;
		}
		return MainManager.map.dialogues[id];
	}

	// Token: 0x0600057B RID: 1403 RVA: 0x00039175 File Offset: 0x00037375
	public static SpriteRenderer NewSpriteObject(Vector3 position, Transform parent, Sprite sprite)
	{
		return MainManager.NewSpriteObject("tempsprite", position, Vector3.zero, parent, sprite, MainManager.spritemat);
	}

	// Token: 0x0600057C RID: 1404 RVA: 0x00039190 File Offset: 0x00037390
	public static SpriteRenderer NewSpriteObject(string name, Vector3 position, Vector3 rotation, Transform parent, Sprite sprite, Material mat)
	{
		SpriteRenderer spriteRenderer = new GameObject(name).AddComponent<SpriteRenderer>();
		spriteRenderer.transform.parent = parent;
		spriteRenderer.transform.localPosition = position;
		spriteRenderer.transform.localEulerAngles = rotation;
		spriteRenderer.sprite = sprite;
		spriteRenderer.gameObject.layer = 14;
		spriteRenderer.material = mat;
		return spriteRenderer;
	}

	// Token: 0x0600057D RID: 1405 RVA: 0x000391E9 File Offset: 0x000373E9
	public static Sprite GetItemSprite(bool badge, int id)
	{
		return MainManager.itemsprites[Convert.ToInt32(badge), id];
	}

	// Token: 0x0600057E RID: 1406 RVA: 0x000391FC File Offset: 0x000373FC
	public static int CrystalBerryAmmount()
	{
		int num = 0;
		for (int i = 0; i < MainManager.instance.crystalbflags.Length; i++)
		{
			if (MainManager.instance.crystalbflags[i])
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x0600057F RID: 1407 RVA: 0x00039235 File Offset: 0x00037435
	public static IEnumerator FadeSound(AudioSource sound, float speed)
	{
		while (sound.volume > 0f && sound.isPlaying)
		{
			sound.volume -= MainManager.TieFramerate(speed);
			yield return null;
		}
		sound.Stop();
		yield break;
	}

	// Token: 0x06000580 RID: 1408 RVA: 0x0003924B File Offset: 0x0003744B
	public static void TeleportFollowers()
	{
		MainManager.TeleportFollowers(false);
	}

	// Token: 0x06000581 RID: 1409 RVA: 0x00039254 File Offset: 0x00037454
	public static void TeleportFollowers(bool all)
	{
		float num = 1f;
		for (int i = 1; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].entity != null)
			{
				MainManager.instance.playerdata[i].entity.transform.position = MainManager.player.transform.position + MainManager.instance.globalcamdir.forward.normalized * (num * 0.025f);
				MainManager.instance.playerdata[i].entity.rigid.velocity = Vector3.zero;
			}
		}
		if (all)
		{
			EntityControl[] array = MainManager.map.tempfollowers.ToArray();
			for (int j = 0; j < array.Length; j++)
			{
				if (array != null)
				{
					array[j].transform.position = MainManager.player.transform.position;
				}
			}
		}
		if (MainManager.map.chompy != null)
		{
			MainManager.map.chompy.DelayedPosition(MainManager.player.transform.position + MainManager.instance.globalcamdir.forward.normalized * 0.3f);
		}
	}

	// Token: 0x06000582 RID: 1410 RVA: 0x000393B8 File Offset: 0x000375B8
	public static GameObject Create6Wheel(Sprite[] sprites, Vector3 spritescale, bool gui)
	{
		return MainManager.Create6Wheel(new Color[]
		{
			Color.white,
			Color.white,
			Color.white,
			Color.white,
			Color.white,
			Color.white
		}, sprites, spritescale, gui);
	}

	// Token: 0x06000583 RID: 1411 RVA: 0x0003941C File Offset: 0x0003761C
	public static GameObject Create6Wheel(Sprite[] sprites, Vector3 spritescale)
	{
		return MainManager.Create6Wheel(new Color[]
		{
			Color.white,
			Color.white,
			Color.white,
			Color.white,
			Color.white,
			Color.white
		}, sprites, spritescale, false);
	}

	// Token: 0x06000584 RID: 1412 RVA: 0x00039480 File Offset: 0x00037680
	public static GameObject Create6Wheel(Color[] colors, Sprite[] sprites, Vector3 spritescale, bool gui)
	{
		GameObject gameObject = new GameObject("Wheel");
		SpriteRenderer spriteRenderer = MainManager.NewSpriteObject(new Vector3(0f, 0f, -0.3f), gameObject.transform, MainManager.guisprites[11]);
		spriteRenderer.transform.localScale = new Vector3(0.5f, 1f, 1f);
		if (gui)
		{
			spriteRenderer.gameObject.layer = 5;
		}
		spriteRenderer = MainManager.NewSpriteObject(Vector3.zero, gameObject.transform, MainManager.guisprites[121]);
		if (gui)
		{
			spriteRenderer.gameObject.layer = 5;
		}
		for (int i = 0; i < 6; i++)
		{
			spriteRenderer = MainManager.NewSpriteObject(new Vector3(0f, 0f, -0.1f), gameObject.transform, MainManager.guisprites[122]);
			spriteRenderer.material.color = colors[i];
			if (gui)
			{
				spriteRenderer.gameObject.layer = 5;
			}
			spriteRenderer = MainManager.NewSpriteObject(new Vector3(-0.05f, 0.8f, -0.1f), spriteRenderer.transform, sprites[i]);
			spriteRenderer.transform.localScale = spritescale;
			spriteRenderer.transform.parent.localEulerAngles = new Vector3(0f, 0f, (float)(60 * i));
			spriteRenderer.transform.parent.localScale = new Vector3(0.9f, 0.9f, 1f);
			if (gui)
			{
				spriteRenderer.gameObject.layer = 5;
			}
		}
		if (gui)
		{
			MainManager.SetParenting(gameObject.transform, MainManager.GUICamera.transform);
		}
		return gameObject;
	}

	// Token: 0x06000585 RID: 1413 RVA: 0x00039610 File Offset: 0x00037810
	public static void SetParenting(Transform t, Transform parent)
	{
		t.parent = parent;
		t.transform.localPosition = Vector3.zero;
		t.transform.localScale = Vector3.one;
		t.transform.localEulerAngles = Vector3.zero;
	}

	// Token: 0x06000586 RID: 1414 RVA: 0x00039649 File Offset: 0x00037849
	public static void TeleportFollowers(float distance, MainManager.TPDir dir, Transform caller)
	{
		MainManager.TeleportFollowers(distance, 1f, dir, caller.position);
	}

	// Token: 0x06000587 RID: 1415 RVA: 0x0003965D File Offset: 0x0003785D
	public static void TeleportFollowers(float distance, MainManager.TPDir dir, Vector3 caller)
	{
		MainManager.TeleportFollowers(distance, 1f, dir, caller);
	}

	// Token: 0x06000588 RID: 1416 RVA: 0x0003966C File Offset: 0x0003786C
	public static void TeleportFollowers(float distance, float offset, MainManager.TPDir dir, Vector3 caller, bool alsoTPextras)
	{
		for (int i = 1; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].entity != null && MainManager.instance.playerdata[i].entity.following != null && MainManager.GetDistance(MainManager.instance.playerdata[i].entity.transform.position, MainManager.instance.playerdata[i].entity.following.transform.position, false) > distance)
			{
				Vector3 a = Vector3.zero;
				if (dir == MainManager.TPDir.Right)
				{
					a = MainManager.instance.globalcamdir.right.normalized;
				}
				else if (dir == MainManager.TPDir.Left)
				{
					a = -MainManager.instance.globalcamdir.right.normalized;
				}
				else if (dir == MainManager.TPDir.Up)
				{
					a = MainManager.instance.globalcamdir.forward.normalized;
				}
				else if (dir == MainManager.TPDir.Down)
				{
					a = -MainManager.instance.globalcamdir.forward.normalized;
				}
				else if (dir == MainManager.TPDir.Away)
				{
					a = -(caller - MainManager.instance.playerdata[i].entity.following.transform.position).normalized + MainManager.MainCamera.transform.forward.normalized * 0.1f;
				}
				else if (dir == MainManager.TPDir.Center)
				{
					a = MainManager.instance.globalcamdir.forward.normalized * 0.1f * (float)(i + 1);
				}
				MainManager.instance.playerdata[i].entity.transform.position = MainManager.instance.playerdata[i].entity.following.transform.position + a * offset;
				MainManager.instance.playerdata[i].entity.rigid.velocity = Vector3.zero;
			}
		}
		if (MainManager.map != null && alsoTPextras)
		{
			if (MainManager.map.tempfollowers != null && MainManager.map.tempfollowers.Count > 0)
			{
				EntityControl[] array = MainManager.map.tempfollowers.ToArray();
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j] != null && MainManager.GetDistance(array[j].transform.position, caller) > distance)
					{
						Vector3 a2 = Vector3.zero;
						if (dir == MainManager.TPDir.Right)
						{
							a2 = MainManager.instance.globalcamdir.right.normalized;
						}
						else if (dir == MainManager.TPDir.Left)
						{
							a2 = -MainManager.instance.globalcamdir.right.normalized;
						}
						else if (dir == MainManager.TPDir.Up)
						{
							a2 = MainManager.instance.globalcamdir.forward.normalized;
						}
						else if (dir == MainManager.TPDir.Down)
						{
							a2 = -MainManager.instance.globalcamdir.forward.normalized;
						}
						else if (dir == MainManager.TPDir.Away)
						{
							a2 = -caller.normalized + MainManager.MainCamera.transform.forward.normalized * 0.1f;
						}
						else if (dir == MainManager.TPDir.Center)
						{
							a2 = MainManager.instance.globalcamdir.forward.normalized * 0.1f * (float)(j + 1);
						}
						array[j].transform.position = caller + a2 * offset;
						array[j].rigid.velocity = Vector3.zero;
					}
				}
				return;
			}
		}
		else if (MainManager.map != null && MainManager.map.chompy != null && MainManager.map.chompy.following != null && MainManager.GetDistance(MainManager.map.chompy.transform.position, MainManager.map.chompy.following.transform.position) > distance)
		{
			MainManager.map.chompy.transform.position = MainManager.map.chompy.following.transform.position + MainManager.instance.globalcamdir.forward.normalized * 0.1f;
		}
	}

	// Token: 0x06000589 RID: 1417 RVA: 0x00039B42 File Offset: 0x00037D42
	public static void TeleportFollowers(float distance, float offset, MainManager.TPDir dir, Vector3 caller)
	{
		MainManager.TeleportFollowers(distance, offset, dir, caller, false);
	}

	// Token: 0x0600058A RID: 1418 RVA: 0x00039B4E File Offset: 0x00037D4E
	public static void Reset()
	{
		SceneManager.LoadScene(0);
	}

	// Token: 0x0600058B RID: 1419 RVA: 0x00039B56 File Offset: 0x00037D56
	private static void BreakLine(ref float x, ref float y, float max, Vector2 size)
	{
		if (x >= max)
		{
			x = 0f;
			y -= 0.7f * size.y;
		}
	}

	// Token: 0x0600058C RID: 1420 RVA: 0x00039B78 File Offset: 0x00037D78
	private static EntityControl GetLastFollowing()
	{
		EntityControl result;
		if (MainManager.player.entity.followedby == null)
		{
			result = MainManager.player.entity;
		}
		else if (MainManager.instance.playerdata[MainManager.instance.playerdata.Length - 1].entity.followedby == null)
		{
			result = MainManager.instance.playerdata[MainManager.instance.playerdata.Length - 1].entity;
		}
		else
		{
			result = MainManager.map.tempfollowers[MainManager.map.tempfollowers.Count - 1];
		}
		return result;
	}

	// Token: 0x0600058D RID: 1421 RVA: 0x00039C24 File Offset: 0x00037E24
	public static void StopEntitiesMove()
	{
		EntityControl[] array = Object.FindObjectsOfType<EntityControl>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].StopForceMove();
		}
	}

	// Token: 0x0600058E RID: 1422 RVA: 0x00039C50 File Offset: 0x00037E50
	public static bool CheckAllBool(bool[] array, bool state)
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != state)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x0600058F RID: 1423 RVA: 0x00039C74 File Offset: 0x00037E74
	public static EntityControl AddFollower(EntityControl caller, int id)
	{
		Vector3 position = MainManager.player.transform.position + MainManager.instance.globalcamdir.forward.normalized * (float)MainManager.instance.playerdata.Length * 0.05f;
		if (caller != null)
		{
			caller.Unfix();
			id = caller.animid;
			position = caller.transform.position;
			MainManager.instance.extrafollowers.Add(id);
			Object.Destroy(caller.gameObject);
		}
		if (id > -1)
		{
			EntityControl lastFollowing = MainManager.GetLastFollowing();
			EntityControl entityControl = EntityControl.CreateNewEntity("Follower " + id, id, position, lastFollowing);
			if (MainManager.map != null)
			{
				entityControl.transform.parent = MainManager.map.transform;
			}
			entityControl.tempfollower = true;
			entityControl.tag = "PFollower";
			MainManager.player.npc = new List<NPCControl>();
			MainManager.map.tempfollowers.Add(entityControl);
			lastFollowing.followedby = entityControl.transform;
			return lastFollowing;
		}
		return null;
	}

	// Token: 0x06000590 RID: 1424 RVA: 0x00039D90 File Offset: 0x00037F90
	public static EntityControl GetExtraFollower(int id)
	{
		EntityControl result = null;
		EntityControl[] array = MainManager.map.tempfollowers.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].animid == id)
			{
				return array[i];
			}
		}
		return result;
	}

	// Token: 0x06000591 RID: 1425 RVA: 0x00039DCD File Offset: 0x00037FCD
	public static IEnumerator LateParent(Transform a, Transform b, float time)
	{
		yield return new WaitForSeconds(time);
		a.parent = b;
		yield break;
	}

	// Token: 0x06000592 RID: 1426 RVA: 0x00039DEA File Offset: 0x00037FEA
	private static void ResetDiag()
	{
		MainManager.tempdiag = "|size," + MainManager.fontdsize + "|";
	}

	// Token: 0x06000593 RID: 1427 RVA: 0x00039E0A File Offset: 0x0003800A
	public static void StopDig(bool delayed)
	{
		if (delayed)
		{
			MainManager.instance.StartCoroutine(MainManager.DigStop());
			return;
		}
		MainManager.StopDig();
	}

	// Token: 0x06000594 RID: 1428 RVA: 0x00039E25 File Offset: 0x00038025
	private static IEnumerator DigStop()
	{
		yield return new WaitForSeconds(0.1f);
		MainManager.StopDig();
		yield break;
	}

	// Token: 0x06000595 RID: 1429 RVA: 0x00039E2D File Offset: 0x0003802D
	public static void TeleportFollowers(MainManager.TPDir direction, Vector3 from)
	{
		MainManager.TeleportFollowers(1.25f, direction, from);
	}

	// Token: 0x06000596 RID: 1430 RVA: 0x00039E3C File Offset: 0x0003803C
	public static void StopDig()
	{
		EntityControl[] partyEntities = MainManager.GetPartyEntities();
		for (int i = 1; i < partyEntities.Length; i++)
		{
			partyEntities[i].LockRigid(false);
			partyEntities[i].spin = Vector3.zero;
			partyEntities[i].startscale = Vector3.one;
			partyEntities[i].sprite.transform.localScale = Vector3.one;
			partyEntities[i].overrideanim = false;
			partyEntities[i].ccol.enabled = true;
		}
	}

	// Token: 0x06000597 RID: 1431 RVA: 0x00039EAE File Offset: 0x000380AE
	private static void ResetDiag(bool soft)
	{
		MainManager.ResetDiag();
		if (!soft)
		{
			MainManager.diagstring = new List<string>();
			MainManager.currentdialogue = 0;
		}
	}

	// Token: 0x06000598 RID: 1432 RVA: 0x00039EC8 File Offset: 0x000380C8
	public static int GetValueFromString(string input)
	{
		if (input == "money")
		{
			return MainManager.instance.money;
		}
		if (!input.Contains("var") && !input.Contains("v"))
		{
			return Convert.ToInt32(input);
		}
		return MainManager.instance.flagvar[Convert.ToInt32(input.Replace("var", "").Replace("v", ""))];
	}

	// Token: 0x06000599 RID: 1433 RVA: 0x00039F3D File Offset: 0x0003813D
	public static IEnumerator Shrink(Transform obj, float frametime, bool deleteonend)
	{
		Vector3 s = obj.transform.localScale;
		float a = 0f;
		do
		{
			obj.transform.localScale = Vector3.Lerp(s, Vector3.zero, a / frametime);
			a += MainManager.TieFramerate(1f);
			yield return null;
		}
		while (a < frametime + 1f);
		if (deleteonend)
		{
			Object.Destroy(obj.gameObject);
		}
		yield break;
	}

	// Token: 0x0600059A RID: 1434 RVA: 0x00039F5C File Offset: 0x0003815C
	public static void FaceTowardsY(Transform t, Vector3 pos)
	{
		Vector3 eulerAngles = t.eulerAngles;
		t.LookAt(pos);
		t.eulerAngles = new Vector3(eulerAngles.x, t.eulerAngles.y, eulerAngles.z);
	}

	// Token: 0x0600059B RID: 1435 RVA: 0x00039F9C File Offset: 0x0003819C
	public static void Insert(int value, ref int[] array)
	{
		List<int> list = new List<int>();
		list.AddRange(array);
		list.Add(value);
		array = list.ToArray();
	}

	// Token: 0x0600059C RID: 1436 RVA: 0x00039FC8 File Offset: 0x000381C8
	public static IEnumerator SetText(string text, int fonttype, float? linebreak, bool dialogue, bool tridimensional, Vector3 position, Vector3 cameraoffset, Vector2 size, Transform parent, NPCControl caller)
	{
		float speed = 0f;
		float currentoffset = 0f;
		float currentline = 0f;
		float maxlenght = 0f;
		float bleeppitch = 1f;
		float bleepvolume = size.magnitude;
		float langOffset = 1f;
		int colorindex = 0;
		int sort = 0;
		int transferi = 0;
		int eventcall = -1;
		int writen = 0;
		int ignorenext = 0;
		int layer = 5;
		bool centralize = false;
		bool wavy = false;
		bool shaky = false;
		bool rainbow = false;
		bool glitchy = false;
		bool skipi = false;
		bool end = false;
		bool promptmenu = false;
		bool minibubble = false;
		bool tempoverf = MainManager.instance.overridefollower;
		bool questboardpromp = false;
		bool tempevent = false;
		bool fadeletter = false;
		bool fontlock = false;
		bool initialflip = false;
		bool ui3d = false;
		bool single = false;
		bool testing = false;
		bool asian = dialogue && MainManager.AsianLang();
		bool locksize = false;
		bool superglitch = false;
		if (MainManager.languageid == 6)
		{
			langOffset = 0.9f;
		}
		MainManager.notextbacktrack = false;
		Time.timeScale = 1f;
		Vector2? dropshadow = null;
		List<EntityControl> returnentitycol = new List<EntityControl>();
		Vector3 camtoffset = MainManager.instance.camoffset;
		MainManager.instance.camoffset2 = Vector3.zero;
		TextMesh ndd = null;
		TextMesh ds = null;
		Vector3? transfer = null;
		SpriteRenderer backbox = null;
		GameObject textholder = new GameObject("Text: " + text);
		GameObject textbox = null;
		Animator windowstyle = null;
		List<MiniBubble> bubbles = new List<MiniBubble>();
		List<GameObject> buts = new List<GameObject>();
		Transform tokenbox = null;
		AudioClip bleep = null;
		RigidbodyConstraints tcons = RigidbodyConstraints.FreezePosition;
		if (text != null)
		{
			text = text.Replace("\r\n", "\n");
		}
		textholder.transform.parent = parent;
		textholder.transform.localPosition = position;
		if (!tridimensional)
		{
			textholder.transform.localEulerAngles = Vector3.zero;
		}
		textholder.transform.localScale = Vector3.one;
		textholder.tag = "Text";
		if (fonttype == 0)
		{
			if (MainManager.languageid == 3)
			{
				fonttype = 3;
			}
			else if (MainManager.languageid == 6)
			{
				fonttype = 4;
			}
			else if (MainManager.languageid == 5)
			{
				fonttype = 5;
			}
		}
		if (linebreak != null)
		{
			text = MainManager.OrganizeLines(text, linebreak.Value, size.x, fonttype);
		}
		if (dialogue)
		{
			MainManager.instance.letterprompt = -1;
			MainManager.define = new List<string[]>();
			MainManager.tempdiag = string.Concat(new object[]
			{
				"|size,",
				size.x,
				",",
				size.y,
				"|"
			});
			if (asian)
			{
				text = text.Insert(0, MainManager.asiansize);
				speed = 0.03f;
			}
			else
			{
				speed = 0.02f;
			}
			MainManager.currentdialogue = 0;
			MainManager.instance.skiptext = false;
			MainManager.instance.isholdingskip = false;
			MainManager.instance.waitinput = false;
			MainManager.backtracking = false;
			MainManager.diagstring = new List<string>();
			MainManager.instance.discoveryhud = -1f;
			MainManager.noskip = false;
			MainManager.instance.inputcooldown = 10f;
			MainManager.instance.overridefollower = true;
			MainManager.instance.hudcooldown = 0f;
			if (caller != null)
			{
				if ((caller.interacttype == NPCControl.Interaction.Shop || caller.interacttype == NPCControl.Interaction.CaravanBadge) && (int)caller.shopkeeper.dialogues[1].y != 1)
				{
					MainManager.instance.showmoney = 10f;
				}
				else
				{
					MainManager.instance.showmoney = 0f;
				}
			}
			MainManager.instance.promptpick = -1;
			MainManager.instance.message = true;
			MainManager.instance.minipause = true;
			if (MainManager.player != null && !MainManager.instance.inevent)
			{
				if (caller != null && (caller.interacttype != NPCControl.Interaction.SavePoint || !MainManager.instance.inevent))
				{
					if (caller.interacttype == NPCControl.Interaction.Shop || caller.interacttype == NPCControl.Interaction.CaravanBadge)
					{
						MainManager.player.entity.FaceTowards(caller.shopkeeper.transform.position);
					}
					else
					{
						MainManager.player.entity.FaceTowards(caller.transform.position);
					}
				}
				initialflip = MainManager.player.entity.flip;
				MainManager.player.entity.StopMoving(0);
				tcons = MainManager.player.entity.rigid.constraints;
				if (caller != null)
				{
					MainManager.TeleportFollowers(2.55f, 1.5f, MainManager.TPDir.Away, caller.transform.position);
					yield return null;
				}
			}
			if (cameraoffset.magnitude > 0.1f)
			{
				MainManager.instance.camoffset += cameraoffset;
			}
			textbox = (Object.Instantiate(Resources.Load("Prefabs/Textbox")) as GameObject);
			MainManager.maintextbox = textbox;
			MainManager.instance.texttail = textbox.transform.GetChild(0);
			MainManager.instance.tailtarget = parent;
			if (parent != null && parent.GetComponent<EntityControl>() != null)
			{
				bleep = Resources.Load<AudioClip>("Audio/Sounds/Dialogue/Dialogue" + parent.GetComponent<EntityControl>().dialoguebleepid);
				bleeppitch = parent.GetComponent<EntityControl>().bleeppitch;
			}
			if (MainManager.instance.playerdata != null)
			{
				for (int j = 1; j < MainManager.instance.playerdata.Length; j++)
				{
					if (MainManager.instance.playerdata[j].entity != null && MainManager.instance.playerdata[j].entity.gameObject.activeInHierarchy)
					{
						MainManager.instance.playerdata[j].entity.StopForceMove(-1, false);
						MainManager.instance.playerdata[j].entity.sprite.enabled = true;
					}
				}
			}
			if (MainManager.map != null)
			{
				EntityControl[] array = MainManager.map.tempfollowers.ToArray();
				for (int k = 0; k < array.Length; k++)
				{
					if (array[k] != null)
					{
						array[k].StopForceMove(-1, false);
					}
				}
			}
			MainManager.GlobalCommand(ref text);
			textholder.transform.parent = textbox.transform;
			textbox.transform.parent = MainManager.GUICamera.transform;
			MainManager.instance.textbox = textholder.transform;
			textbox.transform.localEulerAngles = Vector3.zero;
			textbox.transform.localPosition = new Vector3(0f, 3.25f, 10f);
			if (position.magnitude > 0.1f)
			{
				textholder.transform.localPosition = position;
			}
			else
			{
				textholder.transform.localPosition = new Vector3(-5.5f, 0.9f);
			}
			textbox.transform.localScale = Vector3.zero;
			textbox.AddComponent<DialogueAnim>();
			MainManager.instance.blinker = textbox.transform.GetChild(1).GetComponent<SpriteRenderer>();
			windowstyle = textbox.GetComponent<Animator>();
			textholder.transform.localEulerAngles = Vector3.zero;
			if (MainManager.map != null)
			{
				MainManager.map.StopMovingEntities(null, -1);
			}
			if (MainManager.player != null)
			{
				while (MainManager.player.switchcooldown > 0f)
				{
					yield return null;
				}
			}
		}
		else
		{
			text = text.Insert(0, "|spd,0|");
		}
		int num2;
		for (int i = 0; i < text.Length; i = num2 + 1)
		{
			if (dialogue)
			{
				MainManager.fontdsize = size.x;
				MainManager.fontdtype = fonttype;
				MainManager.linebr = linebreak.Value;
				if (i < 10 && MainManager.player != null && !MainManager.instance.inevent)
				{
					MainManager.player.entity.flip = initialflip;
				}
			}
			if (text[i] == '\n')
			{
				currentoffset = 0f;
				currentline -= 0.7f * size.y * (asian ? 1.25f : 1f);
			}
			else if (text[i] == '|')
			{
				MainManager.SetTalk(dialogue, false);
				string command = "";
				int num = i + 1;
				while (num < text.Length && text[num] != '|')
				{
					command += text[num].ToString();
					num++;
				}
				string[] temp = command.Split(new char[]
				{
					','
				});
				MainManager.Commands com = (MainManager.Commands)Enum.Parse(typeof(MainManager.Commands), temp[0].Replace(".", ""), true);
				if (dialogue && (com == MainManager.Commands.Icon || com == MainManager.Commands.Button || com == MainManager.Commands.Size || com == MainManager.Commands.Shaky || com == MainManager.Commands.Wavy || com == MainManager.Commands.Rainbow || com == MainManager.Commands.Glitchy || com == MainManager.Commands.Halfline || com == MainManager.Commands.Quarterline))
				{
					MainManager.tempdiag = MainManager.tempdiag + "|" + command + "|";
				}
				if (ignorenext <= 0 && (!testing || (dialogue && (com == MainManager.Commands.Next || com == MainManager.Commands.Blank || com == MainManager.Commands.Speed || com == MainManager.Commands.Icon || com == MainManager.Commands.Menu || com == MainManager.Commands.Color || com == MainManager.Commands.Shaky || com == MainManager.Commands.Faketail || com == MainManager.Commands.Rainbow || com == MainManager.Commands.Glitchy || com == MainManager.Commands.Wavy || com == MainManager.Commands.Tail || com == MainManager.Commands.Tailextra || com == MainManager.Commands.Goto || com == MainManager.Commands.Call || com == MainManager.Commands.Minibubble || com == MainManager.Commands.Breakend || temp[0].Contains("line")))))
				{
					MainManager.Commands commands;
					switch (com)
					{
					case MainManager.Commands.String:
					case MainManager.Commands.Sstring:
					case MainManager.Commands.Menu:
					case MainManager.Commands.Call:
						goto IL_66C8;
					case MainManager.Commands.Var:
						if (temp.Length == 2)
						{
							string newValue = MainManager.instance.flagvar[Convert.ToInt32(temp[1])].ToString();
							text = text.Replace("|" + command + "|", newValue);
							num2 = i;
							i = num2 - 1;
							skipi = true;
							goto IL_A206;
						}
						if (temp[2] == "pad")
						{
							string newValue2 = MainManager.instance.flagvar[Convert.ToInt32(temp[1])].ToString().PadLeft(Convert.ToInt32(temp[3]), (temp.Length > 4) ? temp[4][0] : '0');
							text = text.Replace("|" + command + "|", newValue2);
							num2 = i;
							i = num2 - 1;
							skipi = true;
							goto IL_A206;
						}
						MainManager.instance.flagvar[Convert.ToInt32(temp[1])] = MainManager.GetValueFromString(temp[2]);
						goto IL_A206;
					case MainManager.Commands.Anstring:
					{
						int num3 = temp[1].Contains("var") ? MainManager.instance.flagvar[Convert.ToInt32(temp[1].Replace("var", ""))] : Convert.ToInt32(temp[1]);
						int num4 = (temp.Length > 2) ? Convert.ToInt32(temp[2]) : 0;
						text = text.Replace("|" + command + "|", (num4 == 0) ? MainManager.itemdata[0, num3, 3] : MainManager.badgedata[num3, 6]);
						num2 = i;
						i = num2 - 1;
						skipi = true;
						goto IL_A206;
					}
					case MainManager.Commands.Checkitem:
					case MainManager.Commands.Getitem:
					case MainManager.Commands.Buffer:
					case MainManager.Commands.LeafIn:
					case MainManager.Commands.LeafOut:
					case MainManager.Commands.Openboard:
					case MainManager.Commands.Librarybreak:
					case MainManager.Commands.Name:
						goto IL_A206;
					case MainManager.Commands.Prompt:
					{
						if (temp[1] == "yesno")
						{
							command = string.Concat(new string[]
							{
								"prompt,map,0.5,2,",
								temp[2],
								",",
								temp[3],
								",@",
								MainManager.menutext[5],
								",@",
								MainManager.menutext[6]
							});
							if (temp.Length > 4)
							{
								command = command + "," + temp[4];
							}
							temp = command.Split(new char[]
							{
								','
							});
						}
						float num5 = 0f;
						float num6 = 0f;
						string[] array2 = temp[2].Split(new char[]
						{
							';'
						});
						for (int l = 0; l < array2.Length; l++)
						{
							char c2 = array2[l][0];
							if (c2 != '$')
							{
								if (c2 == 'x')
								{
									num6 += Convert.ToSingle(array2[l].Replace("x", ""));
								}
							}
							else
							{
								num5 += Convert.ToSingle(array2[l].Replace("$", ""));
							}
						}
						MainManager.listcanceled = false;
						MainManager.instance.prompt = true;
						MainManager.instance.numberprompt = false;
						MainManager.instance.option = 0;
						MainManager.instance.maxoptions = Convert.ToInt32(temp[3]);
						float num7 = 0f;
						string[] array3 = new string[MainManager.instance.maxoptions];
						bool flag = temp[1] == "card";
						promptmenu = (temp[1] == "main" || temp[1] == "menu");
						for (int m = 0; m < MainManager.instance.maxoptions; m++)
						{
							if (temp[4 + MainManager.instance.maxoptions + m][0] == '@')
							{
								array3[m] = temp[4 + MainManager.instance.maxoptions + m].Remove(0, 1).Replace("}", ",").Replace("{", "|");
							}
							else if (flag)
							{
								array3[m] = MainManager.instance.cardgame.carddiag[Convert.ToInt32(temp[4 + MainManager.instance.maxoptions + m])];
							}
							else
							{
								array3[m] = MainManager.GetText(promptmenu, Convert.ToInt32(temp[4 + MainManager.instance.maxoptions + m]));
							}
							float textLenght = MainManager.GetTextLenght(array3[m], fonttype);
							if (textLenght > num7)
							{
								num7 = textLenght;
							}
						}
						if (temp[temp.Length - 1] == "none" || temp[temp.Length - 1] == "-1")
						{
							MainManager.listcancel = -1;
						}
						else if (temp[temp.Length - 1][0] == '$')
						{
							MainManager.listcancel = Convert.ToInt32(temp[temp.Length - 1].Replace("$", ""));
						}
						else
						{
							MainManager.listcancel = MainManager.instance.maxoptions - 1;
						}
						num7 /= 2.5f;
						num7 += 0.1f;
						if (num6 > num7)
						{
							num7 = num6;
						}
						MainManager.instance.promptbox = MainManager.Create9Box(new Vector3(0f, -0.6f * (float)MainManager.instance.maxoptions + ((MainManager.instance.maxoptions == 5) ? 0.65f : ((float)((MainManager.instance.maxoptions == 6) ? 1 : 0))) + num5, 10f), new Vector2(Mathf.Clamp(num7 * 2.1f + 0.75f, 2f, float.PositiveInfinity), 0.5f + (float)MainManager.instance.maxoptions), 0, -5, (textbox == null) ? Color.white : textbox.GetComponent<SpriteRenderer>().color, true);
						float num8 = 0.25f + 0.5f * (float)(MainManager.instance.maxoptions - 2);
						if (MainManager.instance.maxoptions > 3 && MainManager.battle == null)
						{
							MainManager.instance.promptbox.localPosition += new Vector3(MainManager.instance.promptbox.localPosition.x, 0.1f * (float)MainManager.instance.maxoptions);
						}
						MainManager.instance.promptpointers = new int[MainManager.instance.maxoptions];
						MainManager.CreateCursor(MainManager.instance.promptbox);
						for (int n = 0; n < MainManager.instance.maxoptions; n++)
						{
							MainManager.instance.StartCoroutine(MainManager.SetText(string.Concat(new object[]
							{
								array3[n],
								"|choicewave,",
								n,
								"|"
							}), 0, null, false, false, new Vector3(-num7 - 0.1f, num8, 0f), Vector3.zero, new Vector3(0.85f, 0.85f, 1f), MainManager.instance.promptbox, null));
							MainManager.instance.promptpointers[n] = Convert.ToInt32(Convert.ToInt32(temp[4 + n]));
							num8 -= 1f;
						}
						MainManager.instance.cursor.transform.localPosition = new Vector3(MainManager.instance.promptbox.GetChild(1).localPosition.x - 0.25f, MainManager.instance.promptbox.GetChild(1).localPosition.y + 0.25f, 10f);
						MainManager.instance.inputcooldown = 5f;
						goto IL_A206;
					}
					case MainManager.Commands.Line:
						goto IL_398E;
					case MainManager.Commands.Next:
						MainManager.instance.waitinput = true;
						MainManager.instance.skiptext = false;
						MainManager.noskip = false;
						if (MainManager.instance.tailtarget != null)
						{
							MainManager.instance.tailtarget.GetComponent<EntityControl>().talking = false;
						}
						while (MainManager.instance.waitinput)
						{
							yield return null;
						}
						yield return null;
						if (MainManager.currentdialogue == MainManager.diagstring.Count && !MainManager.backtracking)
						{
							MainManager.currentdialogue++;
							MainManager.diagstring.Add(MainManager.OrganizeLines(MainManager.tempdiag, linebreak.Value, size.x, fonttype));
							MainManager.tempdiag = string.Concat(new object[]
							{
								"|size,",
								size.x,
								",",
								size.y,
								"|"
							});
						}
						while (MainManager.currentdialogue < MainManager.diagstring.Count)
						{
							if (buts != null && buts.Count > 0)
							{
								for (int num9 = 0; num9 < buts.Count; num9++)
								{
									if (buts[num9] != null)
									{
										Object.Destroy(buts[num9].gameObject);
									}
								}
							}
							yield return null;
						}
						yield return null;
						MainManager.backtracking = false;
						goto IL_3CCD;
					case MainManager.Commands.End:
						goto IL_3E4A;
					case MainManager.Commands.Break:
						MainManager.instance.waitinput = true;
						MainManager.instance.skiptext = false;
						MainManager.noskip = false;
						if (MainManager.instance.tailtarget != null)
						{
							MainManager.instance.tailtarget.GetComponent<EntityControl>().talking = false;
						}
						while (MainManager.instance.waitinput)
						{
							yield return null;
						}
						goto IL_A206;
					case MainManager.Commands.Blank:
						goto IL_3CCD;
					case MainManager.Commands.Lock:
						MainManager.instance.message = true;
						MainManager.instance.minipause = true;
						goto IL_A206;
					case MainManager.Commands.Cancelaction:
						if (MainManager.player != null)
						{
							MainManager.player.CancelAction();
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Center:
						centralize = !centralize;
						if (temp.Length <= 1)
						{
							goto IL_A206;
						}
						currentline = 0f;
						if (position.magnitude > 0.1f)
						{
							textholder.transform.localPosition = position;
							goto IL_A206;
						}
						textholder.transform.localPosition = new Vector3(-5.5f, 0.9f);
						goto IL_A206;
					case MainManager.Commands.Halfline:
						temp = new string[]
						{
							"",
							"0.5"
						};
						goto IL_398E;
					case MainManager.Commands.Stopskip:
						MainManager.instance.skiptext = false;
						MainManager.instance.isholdingskip = fadeletter;
						goto IL_A206;
					case MainManager.Commands.Noskip:
						MainManager.instance.skiptext = false;
						MainManager.instance.isholdingskip = fadeletter;
						MainManager.noskip = true;
						goto IL_A206;
					case MainManager.Commands.Hide:
						if (textbox != null && textbox.GetComponent<DialogueAnim>() != null)
						{
							textbox.GetComponent<DialogueAnim>().shrink = !textbox.GetComponent<DialogueAnim>().shrink;
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Rainbow:
						rainbow = !rainbow;
						if (ndd != null)
						{
							FontEffects fontEffects = ndd.GetComponent<FontEffects>();
							if (fontEffects == null)
							{
								fontEffects = ndd.gameObject.AddComponent<FontEffects>();
							}
							fontEffects.SetEffects(false, false, rainbow, false, false, 0, 0);
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Shaky:
						shaky = !shaky;
						goto IL_A206;
					case MainManager.Commands.Wavy:
						wavy = !wavy;
						goto IL_A206;
					case MainManager.Commands.Glitchy:
						glitchy = !glitchy;
						superglitch = (temp.Length > 1 && temp[1] == "1");
						goto IL_A206;
					case MainManager.Commands.Overfollower:
						if (temp.Length > 1)
						{
							MainManager.instance.overridefollower = Convert.ToBoolean(temp[1]);
							goto IL_A206;
						}
						for (int num10 = 1; num10 < MainManager.instance.playerdata.Length; num10++)
						{
							MainManager.instance.playerdata[num10].entity.StopForceMove(-1, false);
						}
						MainManager.instance.overridefollower = !MainManager.instance.overridefollower;
						goto IL_A206;
					case MainManager.Commands.Choicewave:
						textholder.AddComponent<PromptAnim>().SetUp(Convert.ToInt32(temp[1]), temp.Length > 2);
						goto IL_A206;
					case MainManager.Commands.Spd:
					case MainManager.Commands.Speed:
					{
						float num11 = Convert.ToSingle(temp[1]);
						if (num11 == -1f)
						{
							speed = 0.02f;
							goto IL_A206;
						}
						speed = num11;
						goto IL_A206;
					}
					case MainManager.Commands.Color:
						colorindex = Convert.ToInt32(temp[1]);
						if (ndd != null)
						{
							ndd.GetComponent<MeshRenderer>().material.color = MainManager.instance.textcolors[colorindex];
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Anim:
						goto IL_417D;
					case MainManager.Commands.Sort:
						sort = Convert.ToInt32(temp[1]);
						if (ndd != null)
						{
							ndd.GetComponent<MeshRenderer>().sortingOrder = sort;
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Save:
						MainManager.roomtransition = true;
						yield return null;
						if (MainManager.Save(new Vector3?(caller.vectordata[0])))
						{
							MainManager.PlaySound("AtkSuccess", -1, 1f, 0.6f);
							text = MainManager.OrganizeLines("|blank|" + MainManager.menutext[8] + "|stopskip|", linebreak.Value, size.x, fonttype);
						}
						else
						{
							text = MainManager.OrganizeLines("|blank|" + MainManager.menutext[9] + "|stopskip|", linebreak.Value, size.x, fonttype);
						}
						yield return null;
						MainManager.roomtransition = false;
						i = -1;
						skipi = true;
						goto IL_A206;
					case MainManager.Commands.Parent:
						caller = MainManager.GetEntity(Convert.ToInt32(temp[1])).npcdata;
						goto IL_A206;
					case MainManager.Commands.Tail:
					case MainManager.Commands.Tailextra:
					case MainManager.Commands.Gettail:
						goto IL_5868;
					case MainManager.Commands.Flag:
						if (temp.Length > 3)
						{
							MainManager.instance.flags[MainManager.instance.flagvar[Convert.ToInt32(temp[2])]] = Convert.ToBoolean(temp[3]);
							goto IL_A206;
						}
						MainManager.instance.flags[Convert.ToInt32(temp[1])] = Convert.ToBoolean(temp[2]);
						goto IL_A206;
					case MainManager.Commands.Checkmoney:
					{
						int num12 = 2;
						int num13;
						if (temp[1].Contains("var"))
						{
							num13 = MainManager.instance.flagvar[Convert.ToInt32(temp[2])];
							num12 = 3;
						}
						else
						{
							num13 = Convert.ToInt32(temp[1]);
						}
						if (MainManager.instance.money < num13)
						{
							text = "|blank|" + MainManager.OrganizeLines(MainManager.GetDialogueText(Convert.ToInt32(temp[num12])), linebreak.Value, size.x, fonttype);
							i = -1;
							skipi = true;
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Checkinvqtd:
						if (temp[2] == "full")
						{
							int num14 = Convert.ToInt32(temp[1]);
							if ((num14 == 0 && MainManager.instance.items[num14].Count >= MainManager.instance.maxitems) || (num14 == 2 && MainManager.instance.items[num14].Count >= MainManager.instance.maxstorage))
							{
								text = MainManager.OrganizeLines("|blank|" + MainManager.GetDialogueText(Convert.ToInt32(temp[3])), linebreak.Value, size.x, fonttype);
								i = -1;
								skipi = true;
								goto IL_A206;
							}
							goto IL_A206;
						}
						else
						{
							if (temp[2] != "full" && MainManager.instance.items[Convert.ToInt32(temp[1])].Count <= Convert.ToInt32(temp[2]))
							{
								text = MainManager.OrganizeLines("|blank|" + MainManager.GetDialogueText(Convert.ToInt32(temp[3])), linebreak.Value, size.x, fonttype);
								i = -1;
								skipi = true;
								goto IL_A206;
							}
							goto IL_A206;
						}
						break;
					case MainManager.Commands.Additemtoss:
					{
						int num15 = Convert.ToInt32(temp[1]);
						int num16;
						if (temp[2].Contains("var"))
						{
							num16 = MainManager.instance.flagvar[Convert.ToInt32(temp[3])];
						}
						else
						{
							num16 = Convert.ToInt32(temp[2]);
						}
						if ((num15 != 0 || MainManager.instance.items[0].Count + 1 > MainManager.instance.maxitems) && num15 == 0)
						{
							if (caller != null)
							{
								caller.hit = false;
								caller.DestroyDescWindow();
								if (caller.data.Length > 1 && caller.data[1] > -1)
								{
									MainManager.eventtoss = caller.data[1];
								}
							}
							text = "|blank||boxstyle,4||spd,0|" + MainManager.menutext[3] + "|pickitem,0,1,true,false,-2,-3,5|";
							MainManager.instance.blinker.enabled = false;
							i = -1;
							skipi = true;
							goto IL_A206;
						}
						if (num15 != 2 && num15 != 3)
						{
							MainManager.instance.items[num15].Add(num16);
						}
						else if (num15 == 2)
						{
							MainManager.instance.badges.Add(new int[]
							{
								num16,
								-2
							});
						}
						if (caller != null)
						{
							caller.DestroyDescWindow();
							goto IL_3E4A;
						}
						goto IL_3E4A;
					}
					case MainManager.Commands.Additem:
					{
						if (MainManager.MultiItem())
						{
							int[] array4 = new int[]
							{
								(MainManager.listtype == 0) ? 2 : 0,
								(MainManager.listtype == 0) ? 0 : 2
							};
							int[] array5 = (from x in MainManager.instance.multiselect
							orderby x
							select x).ToArray<int>();
							for (int num17 = array5.Length - 1; num17 >= 0; num17--)
							{
								MainManager.instance.items[array4[0]].Add(MainManager.instance.items[array4[1]][array5[num17]]);
								MainManager.instance.items[array4[1]].RemoveAt(array5[num17]);
							}
							MainManager.instance.multiselect = new List<int>();
							goto IL_A206;
						}
						int item;
						if (temp[2].Contains("var"))
						{
							item = MainManager.instance.flagvar[Convert.ToInt32(temp[3])];
						}
						else
						{
							item = Convert.ToInt32(temp[2]);
						}
						MainManager.instance.items[Convert.ToInt32(temp[1])].Add(item);
						goto IL_A206;
					}
					case MainManager.Commands.Money:
						if (temp[1] == "var")
						{
							if (temp[2][0] == '-')
							{
								MainManager.instance.money -= MainManager.instance.flagvar[Convert.ToInt32(temp[3])];
							}
							else
							{
								MainManager.instance.money += MainManager.instance.flagvar[Convert.ToInt32(temp[2])];
							}
						}
						else
						{
							MainManager.instance.money += Convert.ToInt32(temp[1]);
						}
						MainManager.instance.money = Mathf.Clamp(MainManager.instance.money, 0, 999);
						goto IL_A206;
					case MainManager.Commands.Goto:
						goto IL_63B6;
					case MainManager.Commands.Currency:
					{
						string text2;
						if (temp[1] == "var")
						{
							text2 = MainManager.instance.flagvar[Convert.ToInt32(temp[2])].ToString();
						}
						else
						{
							text2 = temp[1];
						}
						if (MainManager.languageid == 6)
						{
							string text3 = text2;
							if (text2.Length > 2)
							{
								text3 = text2[text2.Length - 2].ToString() + text2[text2.Length - 1].ToString();
							}
							if (text3 == "11" || text3 == "12" || text3 == "13" || text3 == "14")
							{
								text2 = text2 + " " + MainManager.menutext[275];
							}
							else
							{
								switch (text3[text3.Length - 1])
								{
								case '1':
									text2 = text2 + " " + MainManager.menutext[277];
									break;
								case '2':
								case '3':
								case '4':
									text2 = text2 + " " + MainManager.menutext[278];
									break;
								default:
									text2 = text2 + " " + MainManager.menutext[275];
									break;
								}
							}
						}
						else
						{
							text2 = text2 + " " + MainManager.menutext[Convert.ToInt32(Convert.ToInt32(text2) != 1)];
						}
						text = text.Replace("|" + command + "|", text2);
						num2 = i;
						i = num2 - 1;
						skipi = true;
						goto IL_A206;
					}
					case MainManager.Commands.Kill:
					{
						EntityControl entity = MainManager.GetEntity(temp[1], caller.entity);
						if (entity.npcdata != null && (entity.npcdata.interacttype == NPCControl.Interaction.Shop || entity.npcdata.interacttype == NPCControl.Interaction.CaravanBadge) && entity.animid == 2)
						{
							if (entity.npcdata.interacttype == NPCControl.Interaction.CaravanBadge)
							{
								entity.npcdata.SetBadgeShop(true);
							}
							else
							{
								entity.npcdata.shopkeeper.SetBadgeShop(true);
							}
						}
						if (entity.npcdata != null && entity.npcdata.interacttype != NPCControl.Interaction.CaravanBadge)
						{
							entity.StartCoroutine(entity.Death(true));
						}
						if (MainManager.player != null)
						{
							MainManager.player.npc = new List<NPCControl>();
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Boxstyle:
					{
						SpriteRenderer component = textbox.GetComponent<SpriteRenderer>();
						if (temp[1] == "-1")
						{
							component.enabled = false;
							MainManager.instance.tailtarget = null;
						}
						else if (windowstyle != null)
						{
							windowstyle.Play(temp[1]);
						}
						if (temp.Length > 2)
						{
							component.sortingOrder = Convert.ToInt32(temp[2]);
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Pickitem:
						MainManager.instance.inputcooldown = 5f;
						MainManager.instance.multiselect = new List<int>();
						if (MainManager.battle == null)
						{
							MainManager.instance.inputcooldown = 5f;
						}
						if (temp.Length == 4)
						{
							MainManager.storeid = 0;
							MainManager.listtype = Convert.ToInt32(temp[1]);
							MainManager.listammount = 5;
							MainManager.listcancel = Convert.ToInt32(temp[3]);
							MainManager.listredirect = new int?(Convert.ToInt32(temp[2]));
							MainManager.listdesc = true;
							MainManager.listsell = false;
						}
						else
						{
							MainManager.storeid = Convert.ToInt32(temp[2]);
							MainManager.listcancel = Convert.ToInt32(temp[6]);
							MainManager.listredirect = new int?(Convert.ToInt32(temp[5]));
							if (temp.Length < 8)
							{
								MainManager.listammount = 5;
							}
							else
							{
								MainManager.listammount = Convert.ToInt32(temp[7]);
							}
							MainManager.listtype = Convert.ToInt32(temp[1]);
							MainManager.listdesc = Convert.ToBoolean(temp[3]);
							MainManager.listsell = Convert.ToBoolean(temp[4]);
						}
						MainManager.ShowItemList(MainManager.listtype, MainManager.defaultlistpos, MainManager.listdesc, MainManager.listsell);
						goto IL_A206;
					case MainManager.Commands.Checktrue:
					case MainManager.Commands.Checkflag:
					case MainManager.Commands.Battlewon:
						if (temp[1] == "var")
						{
							if (MainManager.instance.flagvar[Convert.ToInt32(temp[2])] != Convert.ToInt32(temp[3]))
							{
								text = MainManager.OrganizeLines(MainManager.GetDialogueText(Convert.ToInt32(temp[4])), linebreak.Value, size.x, fonttype);
								MainManager.instance.skiptext = false;
								skipi = true;
								i = -1;
								goto IL_A206;
							}
							goto IL_A206;
						}
						else
						{
							bool flag2 = false;
							string[] array6 = temp[1].Split(new char[]
							{
								'@'
							});
							if (array6.Length > 1)
							{
								bool[] array7 = new bool[array6.Length];
								for (int num18 = 0; num18 < array6.Length; num18++)
								{
									int num19 = Convert.ToInt32(array6[num18]);
									array7[num18] = (MainManager.instance.flags[Mathf.Abs(num19)] == num19 > 0);
								}
								flag2 = MainManager.CheckAllBool(array7, com == MainManager.Commands.Checktrue);
							}
							if (flag2 || (array6.Length == 1 && ((com == MainManager.Commands.Battlewon && !MainManager.battleresult) || (com != MainManager.Commands.Battlewon && MainManager.instance.flags[Convert.ToInt32(temp[1])] == ((com == MainManager.Commands.Checkflag) ? false : true)))))
							{
								text = MainManager.OrganizeLines(MainManager.GetDialogueText(Convert.ToInt32(temp[(com == MainManager.Commands.Battlewon) ? 1 : 2])), linebreak.Value, size.x, fonttype);
								MainManager.instance.skiptext = false;
								skipi = true;
								i = -1;
								goto IL_A206;
							}
							goto IL_A206;
						}
						break;
					case MainManager.Commands.Removeitem:
						if (!MainManager.MultiItem())
						{
							if (temp[1] == "3")
							{
								int[][] array8 = MainManager.instance.badges.ToArray();
								for (int num20 = 0; num20 < MainManager.instance.badges.Count; num20++)
								{
									if (array8[num20][0] == ((temp[2] == "var") ? MainManager.instance.flagvar[Convert.ToInt32(temp[3])] : Convert.ToInt32(temp[2])))
									{
										MainManager.instance.badges.RemoveAt(num20);
										break;
									}
								}
								goto IL_A206;
							}
							int[] array9 = new int[temp.Length - 1];
							for (int num21 = 1; num21 < temp.Length; num21++)
							{
								if (temp[num21].Contains("var"))
								{
									array9[num21 - 1] = MainManager.instance.flagvar[Convert.ToInt32(temp[num21 + 1])];
								}
								else
								{
									array9[num21 - 1] = Convert.ToInt32(temp[num21]);
								}
							}
							MainManager.instance.items[array9[0]].Remove(array9[1]);
							goto IL_A206;
						}
						else
						{
							if (MainManager.listsell)
							{
								MainManager.instance.multiselect = (from x in MainManager.instance.multiselect
								orderby x
								select x).ToList<int>();
								for (int num22 = MainManager.instance.multiselect.Count - 1; num22 >= 0; num22--)
								{
									MainManager.instance.items[0].RemoveAt(MainManager.instance.multiselect[num22]);
								}
								MainManager.instance.multiselect = new List<int>();
								goto IL_A206;
							}
							goto IL_A206;
						}
						break;
					case MainManager.Commands.Getstorage:
						text = text.Replace("|" + command + "|", (MainManager.instance.maxstorage - MainManager.instance.items[2].Count).ToString());
						num2 = i;
						i = num2 - 1;
						skipi = true;
						goto IL_A206;
					case MainManager.Commands.Button:
					{
						ButtonSprite buttonSprite = new GameObject(command).AddComponent<ButtonSprite>();
						int num23 = -1;
						if (temp.Length >= 3)
						{
							num23 = Convert.ToInt32(temp[2]);
						}
						string description = null;
						if (temp.Length >= 4)
						{
							description = temp[3];
						}
						int num24 = Convert.ToInt32(temp[1]);
						float num25 = 0.25f;
						bool flag3 = false;
						if (num23 != 1 && !InputIO.IsConsole && InputIO.LongButton(num24))
						{
							num25 += 0.3f;
							flag3 = true;
						}
						buttonSprite.tridimentional = tridimensional;
						buttonSprite.SetUp(num24, num23, description, new Vector2(currentoffset + num25, currentline + 0.225f * size.y), new Vector3(size.x / 2.2f, size.y / 2.2f, 1f), sort, null);
						if (!tridimensional)
						{
							buttonSprite.gameObject.layer = layer;
							buttonSprite.layer = layer;
						}
						buttonSprite.transform.parent = textholder.transform;
						buttonSprite.transform.localEulerAngles = Vector3.zero;
						buts.Add(buttonSprite.gameObject);
						currentoffset += 0.7f;
						if (flag3)
						{
							currentoffset += 0.7f;
						}
						if ((speed > 0f && !MainManager.instance.skiptext) || MainManager.noskip)
						{
							yield return new WaitForSeconds(speed);
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Movewait:
					{
						EntityControl temptail = MainManager.GetEntity(temp[1], (caller != null) ? caller.entity : null);
						while (temptail.forcemove)
						{
							yield return null;
						}
						temptail = null;
						goto IL_A206;
					}
					case MainManager.Commands.Move:
					case MainManager.Commands.Moveahead:
					{
						EntityControl entity2 = MainManager.GetEntity(caller, temp[1]);
						entity2.backsprite = false;
						float num26 = 0f;
						if (temp[3] != "null")
						{
							num26 = Convert.ToSingle(temp[3]);
						}
						else
						{
							entity2.ignorey = true;
						}
						float multiplier = 1f;
						int state = 1;
						int stopstate = 0;
						if (temp.Length > 5)
						{
							multiplier = Convert.ToSingle(temp[5]);
						}
						if (temp.Length > 6)
						{
							state = Convert.ToInt32(temp[6]);
						}
						if (temp.Length > 7)
						{
							stopstate = Convert.ToInt32(temp[7]);
						}
						entity2.MoveTowards(new Vector3(Convert.ToSingle(temp[2]), (com == MainManager.Commands.Move) ? num26 : 0f, Convert.ToSingle(temp[4])) + ((com == MainManager.Commands.Move) ? Vector3.zero : entity2.transform.position), multiplier, state, stopstate);
						goto IL_A206;
					}
					case MainManager.Commands.Forcewait:
					case MainManager.Commands.Fwait:
						if (MainManager.pausemenu == null)
						{
							yield return new WaitForSeconds(Convert.ToSingle(temp[1]));
							MainManager.instance.skiptext = false;
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Wait:
						if (!MainManager.instance.skiptext || minibubble)
						{
							yield return new WaitForSeconds(Convert.ToSingle(temp[1]));
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Face:
					{
						if (temp[1] == "party")
						{
							for (int num27 = 0; num27 < MainManager.instance.playerdata.Length; num27++)
							{
								if (caller != null)
								{
									MainManager.instance.playerdata[num27].entity.FaceTowards(MainManager.GetEntity(caller, temp[2]).transform.position);
								}
								else
								{
									MainManager.instance.playerdata[num27].entity.FaceTowards(MainManager.GetEntity(temp[2]).transform.position);
								}
							}
							goto IL_A206;
						}
						EntityControl entity3;
						EntityControl entity4;
						if (caller != null)
						{
							entity3 = MainManager.GetEntity(temp[1], caller.entity);
							entity4 = MainManager.GetEntity(temp[2], caller.entity);
						}
						else
						{
							entity3 = MainManager.GetEntity(temp[1]);
							entity4 = MainManager.GetEntity(temp[2]);
						}
						entity3.flip = (entity4.transform.position.x > entity3.transform.position.x);
						if (temp.Length > 3 && temp[3] == "true")
						{
							entity4.FaceTowards(entity3.transform.position);
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Camtarget:
						if (temp.Length > 2)
						{
							MainManager.instance.camtarget = null;
							MainManager.instance.camtargetpos = new Vector3?(new Vector3(Convert.ToSingle(temp[1]), Convert.ToSingle(temp[2]), Convert.ToSingle(temp[3])));
							goto IL_A206;
						}
						if (temp[1] == "null")
						{
							MainManager.instance.camtargetpos = null;
							MainManager.instance.camtarget = null;
							goto IL_A206;
						}
						MainManager.instance.camtargetpos = null;
						MainManager.instance.camtarget = MainManager.GetEntity(temp[1], (caller != null) ? caller.entity : null).transform;
						goto IL_A206;
					case MainManager.Commands.Flip:
					{
						if (temp[1] == "party")
						{
							bool? flag4 = null;
							if (temp.Length > 2)
							{
								flag4 = new bool?(Convert.ToBoolean(temp[2]));
							}
							for (int num28 = 0; num28 < MainManager.instance.playerdata.Length; num28++)
							{
								if (flag4 == null)
								{
									MainManager.instance.playerdata[num28].entity.flip = !MainManager.instance.playerdata[num28].entity.flip;
								}
								else
								{
									MainManager.instance.playerdata[num28].entity.flip = flag4.Value;
								}
							}
							goto IL_A206;
						}
						EntityControl entity5;
						if (caller != null)
						{
							entity5 = MainManager.GetEntity(temp[1], caller.entity);
						}
						else
						{
							entity5 = MainManager.GetEntity(temp[1]);
						}
						if (temp.Length == 2)
						{
							entity5.flip = !entity5.flip;
							goto IL_A206;
						}
						entity5.flip = Convert.ToBoolean(temp[2]);
						goto IL_A206;
					}
					case MainManager.Commands.Warp:
					case MainManager.Commands.Transfer:
						end = true;
						if (temp[1].Contains("var"))
						{
							transferi = MainManager.instance.flagvar[Convert.ToInt32(temp[1].Replace("var", ""))];
						}
						else
						{
							transferi = Convert.ToInt32(temp[1]);
						}
						if (transferi >= Enum.GetNames(typeof(MainManager.Maps)).Length)
						{
							transferi = 1;
						}
						if (temp.Length > 2)
						{
							transfer = new Vector3?(new Vector3(Convert.ToSingle(temp[2]), Convert.ToSingle(temp[3]), Convert.ToSingle(temp[4])));
							goto IL_A206;
						}
						transfer = new Vector3?(Vector3.zero);
						goto IL_A206;
					case MainManager.Commands.NumberPrompt:
					{
						MainManager.instance.letterprompt = -1;
						MainManager.listcanceled = false;
						MainManager.instance.inputcooldown = 5f;
						if (MainManager.instance.flagvar[0] == -555)
						{
							MainManager.instance.flagvar[0] = 0;
						}
						MainManager.instance.prompt = true;
						MainManager.instance.flagvar[5] = -1;
						MainManager.instance.numberprompt = true;
						MainManager.instance.flagstring[0] = "";
						MainManager.instance.option = 0;
						MainManager.instance.maxoptions = 11;
						MainManager.instance.promptbox = MainManager.Create9Box(new Vector3(0f, -1.2f, 10f), new Vector2(7f, 3f), 0, -3, (textbox == null) ? Color.white : textbox.GetComponent<SpriteRenderer>().color, true);
						MainManager.listtype = Convert.ToInt32(temp[1]);
						MainManager.instance.flagvar[10] = Convert.ToInt32(temp[2]);
						MainManager.listredirect = new int?(Convert.ToInt32(temp[3]));
						MainManager.listcancel = Convert.ToInt32(temp[4]);
						MainManager.instance.npromptholder = new GameObject("number holder").transform;
						MainManager.instance.npromptholder.transform.parent = MainManager.instance.promptbox.transform;
						MainManager.instance.npromptholder.transform.localPosition = new Vector3(0f, 0.5f, 0.05f);
						MainManager.instance.npromptholder.transform.localEulerAngles = Vector3.zero;
						MainManager.instance.npromptholder.transform.localScale = Vector3.one;
						float num29 = -2.25f;
						float num30 = -0.25f;
						for (int num31 = 0; num31 < MainManager.instance.maxoptions + 1; num31++)
						{
							string str = string.Concat(new object[]
							{
								num31,
								"|choicewave,",
								num31,
								",true|"
							});
							if (num31 == 10)
							{
								string text4 = "";
								if (MainManager.languageid == 6)
								{
									text4 = "|sizemulti,0.8,1|";
								}
								str = string.Concat(new object[]
								{
									text4,
									MainManager.menutext[42],
									"|choicewave,",
									num31,
									"|"
								});
							}
							else if (num31 == 11)
							{
								str = string.Concat(new object[]
								{
									MainManager.menutext[74],
									"|choicewave,",
									num31,
									"|"
								});
							}
							MainManager.instance.StartCoroutine(MainManager.SetText("|center|" + str, 0, null, false, false, new Vector3(num29, num30, 0f), Vector3.zero, new Vector3(0.85f, 0.85f, 1f), MainManager.instance.promptbox, null));
							num29 += 0.5f;
							if (num31 == 9)
							{
								num30 -= 0.75f;
								num29 = -1.5f;
							}
							else if (num31 > 9)
							{
								num29 = 1.5f;
							}
						}
						MainManager.instance.RefreshNumberPrompt();
						goto IL_A206;
					}
					case MainManager.Commands.Common:
						text = MainManager.OrganizeLines("|blank|" + MainManager.commondialogue[Convert.ToInt32(temp[1])], linebreak.Value, size.x, fonttype);
						i = -1;
						MainManager.instance.skiptext = false;
						skipi = true;
						goto IL_A206;
					case MainManager.Commands.Checkvar:
						if (temp[1] == "atleast")
						{
							if (MainManager.instance.flagvar[Convert.ToInt32(temp[2])] < (temp[3].Contains("var") ? MainManager.instance.flagvar[Convert.ToInt32(temp[3].Replace("var", ""))] : Convert.ToInt32(temp[3])))
							{
								text = MainManager.OrganizeLines("|blank|" + MainManager.GetDialogueText(Convert.ToInt32(temp[4])), linebreak.Value, size.x, fonttype);
								i = -1;
								MainManager.instance.skiptext = false;
								skipi = true;
								goto IL_A206;
							}
							goto IL_A206;
						}
						else if (temp[1] == "moreand")
						{
							if (MainManager.instance.flagvar[Convert.ToInt32(temp[2])] >= (temp[3].Contains("var") ? MainManager.instance.flagvar[Convert.ToInt32(temp[3].Replace("var", ""))] : Convert.ToInt32(temp[3])))
							{
								text = MainManager.OrganizeLines("|blank|" + MainManager.GetDialogueText(Convert.ToInt32(temp[4])), linebreak.Value, size.x, fonttype);
								i = -1;
								MainManager.instance.skiptext = false;
								skipi = true;
								goto IL_A206;
							}
							goto IL_A206;
						}
						else
						{
							if (MainManager.instance.flagvar[Convert.ToInt32(temp[1])] == Convert.ToInt32(temp[2]))
							{
								text = MainManager.OrganizeLines("|blank|" + MainManager.GetDialogueText(Convert.ToInt32(temp[3])), linebreak.Value, size.x, fonttype);
								i = -1;
								MainManager.instance.skiptext = false;
								skipi = true;
								goto IL_A206;
							}
							goto IL_A206;
						}
						break;
					case MainManager.Commands.FadeIn:
					case MainManager.Commands.FadeOut:
					{
						float y = 0.1f;
						int redirect = 0;
						int num32 = 0;
						if (temp.Length > 1)
						{
							y = Convert.ToSingle(temp[1]);
						}
						if (temp.Length > 2)
						{
							redirect = Convert.ToInt32(temp[2]);
						}
						if (temp.Length > 3 && char.IsNumber(temp[3][0]))
						{
							num32 = Convert.ToInt32(temp[3]);
						}
						MainManager.PlayTransition((int)Enum.Parse(typeof(MainManager.Transitions), temp[0], true), redirect, y, (num32 == 0) ? Color.black : MainManager.instance.textcolors[num32]);
						if (com == MainManager.Commands.FadeIn && temp.Length > 3 && temp[3] == "kill" && caller != null)
						{
							caller.entity.talking = false;
							yield return new WaitForSeconds(0.75f + (1f - y));
							Object.Destroy(caller.gameObject);
							MainManager.PlayTransition(1, redirect, y, Color.black);
							yield return new WaitForSeconds(0.75f + (1f - y));
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Align:
						if (caller != null)
						{
							EntityControl temptail = (MainManager.map.chompy != null) ? MainManager.map.chompy.following : null;
							bool a = temp[2] == "true";
							bool tmes = MainManager.instance.overridefollower;
							bool flipback = false;
							MainManager.instance.overridefollower = true;
							MainManager.TeleportFollowers(2.5f, MainManager.TPDir.Away, MainManager.player.transform);
							float d = caller.entity.ccol.radius * 3f + 0.1f;
							if (temp.Length >= 4)
							{
								if (char.IsNumber(temp[3][0]))
								{
									d = Convert.ToSingle(temp[3]);
								}
								else
								{
									flipback = Convert.ToBoolean(temp[3]);
								}
							}
							Vector3 a3 = caller.transform.position + MainManager.instance.globalcamdir.right.normalized * d;
							Vector3 a4 = MainManager.instance.globalcamdir.right.normalized;
							string a5 = temp[1];
							if (a5 == "left")
							{
								goto IL_8945;
							}
							if (!(a5 == "near"))
							{
								if (a5 == "front")
								{
									if (!caller.entity.flip)
									{
										goto IL_8945;
									}
								}
							}
							else if (MainManager.player.entity.campos.x < caller.entity.campos.x)
							{
								goto IL_8945;
							}
							IL_89E1:
							if (caller.pusher != null)
							{
								caller.pusher.enabled = false;
							}
							for (int num33 = 0; num33 < MainManager.instance.playerdata.Length; num33++)
							{
								if (MainManager.instance.playerdata[num33].entity.transform != MainManager.player.transform)
								{
									MainManager.instance.playerdata[num33].entity.forcejump = true;
								}
								MainManager.instance.playerdata[num33].entity.backsprite = false;
								MainManager.instance.playerdata[num33].entity.MoveTowards(a3 + a4 * (float)num33 * 1.3f, 1f, 1, 0, true, caller.transform.position);
								Physics.IgnoreCollision(MainManager.instance.playerdata[num33].entity.ccol, caller.entity.ccol, true);
							}
							if (MainManager.map.chompy != null)
							{
								MainManager.map.chompy.following = MainManager.instance.playerdata[MainManager.instance.playerdata.Length - 1].entity;
								MainManager.map.chompy.MoveTowards(MainManager.map.chompy.following.forcetarget + a4 * 0.95f + MainManager.instance.globalcamdir.forward * -0.45f, 1f, 1, 0, true);
								MainManager.map.chompy.forcejump = true;
								Physics.IgnoreCollision(MainManager.map.chompy.ccol, caller.entity.ccol, true);
							}
							if (a && textbox != null && textbox.GetComponent<DialogueAnim>() != null)
							{
								textbox.GetComponent<DialogueAnim>().shrink = !textbox.GetComponent<DialogueAnim>().shrink;
							}
							float y = 0f;
							while (!MainManager.PartyIsNotMoving())
							{
								for (int num34 = 0; num34 < MainManager.instance.playerdata.Length; num34++)
								{
									MainManager.instance.playerdata[num34].entity.backsprite = false;
									if (!MainManager.instance.playerdata[num34].entity.forcemove)
									{
										MainManager.instance.playerdata[num34].entity.FaceTowards(caller.transform.position);
									}
								}
								y += MainManager.framestep;
								if (y > 300f)
								{
									for (int num35 = 0; num35 < MainManager.instance.playerdata.Length; num35++)
									{
										MainManager.instance.playerdata[num35].entity.transform.position = MainManager.instance.playerdata[num35].entity.forcetarget;
									}
								}
								yield return null;
							}
							if (MainManager.map.chompy != null)
							{
								while (MainManager.map.chompy.forcemove)
								{
									if (y > 300f)
									{
										MainManager.map.chompy.transform.position = MainManager.map.chompy.forcetarget;
									}
									else
									{
										y += MainManager.framestep;
									}
									yield return null;
								}
								Physics.IgnoreCollision(MainManager.map.chompy.ccol, caller.entity.ccol, false);
								MainManager.map.chompy.FaceTowards(caller.transform.position);
								MainManager.map.chompy.animstate = 0;
							}
							if (a && textbox != null && textbox.GetComponent<DialogueAnim>() != null)
							{
								textbox.GetComponent<DialogueAnim>().shrink = !textbox.GetComponent<DialogueAnim>().shrink;
							}
							for (int num36 = 0; num36 < MainManager.instance.playerdata.Length; num36++)
							{
								Physics.IgnoreCollision(MainManager.instance.playerdata[num36].entity.ccol, caller.entity.ccol, false);
								MainManager.instance.playerdata[num36].entity.backsprite = false;
								MainManager.instance.playerdata[num36].entity.FaceTowards(caller.transform.position);
							}
							if (caller.pusher != null)
							{
								caller.pusher.enabled = true;
							}
							MainManager.instance.skiptext = false;
							MainManager.instance.overridefollower = tmes;
							initialflip = MainManager.player.entity.flip;
							if (flipback)
							{
								caller.entity.FaceTowards(MainManager.player.transform.position);
							}
							if (MainManager.map.chompy != null)
							{
								MainManager.map.chompy.following = temptail;
							}
							temptail = null;
							goto IL_A206;
							IL_8945:
							a3 = caller.transform.position - MainManager.instance.globalcamdir.right.normalized * d;
							a4 = -MainManager.instance.globalcamdir.right.normalized;
							goto IL_89E1;
						}
						goto IL_A206;
					case MainManager.Commands.Discovery:
					{
						if (temp[1] == "all")
						{
							for (int num37 = 0; num37 < MainManager.instance.librarystuff.GetLength(0); num37++)
							{
								for (int num38 = 0; num38 < MainManager.instance.librarystuff.GetLength(1); num38++)
								{
									MainManager.instance.librarystuff[num37, num38] = true;
								}
							}
							goto IL_A206;
						}
						int num39 = Convert.ToInt32(temp[1]);
						if (!MainManager.instance.librarystuff[0, num39])
						{
							MainManager.UpdateJounal(MainManager.Library.Discovery, num39);
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Size:
						break;
					case MainManager.Commands.Minibubble:
					{
						if (temp.Length == 1)
						{
							minibubble = true;
							bubbles.Add(parent.gameObject.GetComponent<MiniBubble>());
							goto IL_A206;
						}
						string text5;
						if (temp[1][0] == '@')
						{
							text5 = temp[1].Replace("}", ",").Replace("{", "|").Remove(0, 1);
						}
						else
						{
							text5 = MainManager.GetDialogueText(Convert.ToInt32(temp[1]));
						}
						Vector3 minibubblePos = new Vector3(0f, 1f, 10f);
						EntityControl entity6;
						if (testing)
						{
							entity6 = MainManager.player.entity;
						}
						else if (caller != null)
						{
							entity6 = MainManager.GetEntity(temp[2], caller.entity);
						}
						else
						{
							entity6 = MainManager.GetEntity(temp[2]);
						}
						if (entity6 != null)
						{
							if (temp.Length > 4)
							{
								minibubblePos = new Vector3(Convert.ToSingle(temp[3]), Convert.ToSingle(temp[4]), 10f);
							}
							else
							{
								float x2 = MainManager.MainCamera.WorldToViewportPoint(entity6.transform.position).x - 0.5f;
								float y2 = 0.85f;
								if (temp.Length == 4)
								{
									y2 = Convert.ToSingle(temp[3]);
								}
								minibubblePos = MainManager.GetMinibubblePos(x2, y2);
							}
							bubbles.Add(MiniBubble.SetUp(text5, entity6, minibubblePos, 10 + 3 * bubbles.Count));
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Destroyminibubble:
					{
						if (temp.Length == 1)
						{
							for (int num40 = 0; num40 < bubbles.Count; num40++)
							{
								if (bubbles[num40] != null)
								{
									bubbles[num40].DestroyThis();
								}
							}
							goto IL_A206;
						}
						int num41 = Convert.ToInt32(temp[1]);
						if (bubbles.Count <= num41 + 1 && bubbles[num41] != null)
						{
							bubbles[num41].DestroyThis();
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Halt:
						for (;;)
						{
							yield return null;
						}
						break;
					case MainManager.Commands.Waitminibubble:
					{
						if (temp.Length == 1)
						{
							while (bubbles.Count > 0)
							{
								for (int num42 = 0; num42 < bubbles.Count; num42++)
								{
									if (bubbles[num42] == null)
									{
										bubbles.RemoveAt(num42);
									}
								}
								yield return null;
							}
							goto IL_A206;
						}
						int redirect = Convert.ToInt32(temp[1]);
						while (bubbles[redirect] != null)
						{
							yield return null;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Showmoney:
						MainManager.instance.showmoney = 10f;
						goto IL_A206;
					case MainManager.Commands.Hidemoney:
						MainManager.instance.showmoney = -1f;
						goto IL_A206;
					case MainManager.Commands.Innsleep:
					{
						if (textbox != null && textbox.GetComponent<DialogueAnim>() != null)
						{
							textbox.GetComponent<DialogueAnim>().shrink = true;
						}
						Vector3? position2 = null;
						if (temp.Length > 1)
						{
							position2 = new Vector3?(MainManager.VectorFromString(new string[]
							{
								temp[1],
								temp[2],
								temp[3]
							}));
						}
						Vector3? camn = null;
						Vector3? camp = null;
						if (temp.Length > 5)
						{
							camn = new Vector3?(MainManager.VectorFromString(new string[]
							{
								temp[5],
								temp[6],
								temp[7]
							}));
							camp = new Vector3?(MainManager.VectorFromString(new string[]
							{
								temp[8],
								temp[9],
								temp[10]
							}));
						}
						MainManager.chaptername = MainManager.instance.StartCoroutine(MainManager.InnSleep(caller, position2, temp.Length > 4 && temp[4] == "true", false, camn, camp));
						while (MainManager.chaptername != null)
						{
							yield return null;
						}
						end = true;
						goto IL_A206;
					}
					case MainManager.Commands.Event:
						if (char.IsNumber(temp[1][0]))
						{
							MainManager.events.StartEvent(Convert.ToInt32(temp[1]), caller);
							tempoverf = true;
							tempevent = true;
							end = true;
							goto IL_A206;
						}
						MainManager.instance.inevent = Convert.ToBoolean(temp[1]);
						goto IL_A206;
					case MainManager.Commands.Checkregional:
						if ((temp.Length < 4 && !MainManager.instance.regionalflags[Convert.ToInt32(temp[1])]) || (temp.Length == 4 && MainManager.instance.regionalflags[Convert.ToInt32(temp[1])] == Convert.ToBoolean(temp[3])))
						{
							text = MainManager.OrganizeLines("|blank|" + MainManager.GetDialogueText(Convert.ToInt32(temp[2])), linebreak.Value, size.x, fonttype);
							MainManager.instance.skiptext = false;
							skipi = true;
							i = -1;
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Regionalflag:
						MainManager.instance.regionalflags[Convert.ToInt32(temp[1])] = Convert.ToBoolean(temp[2]);
						goto IL_A206;
					case MainManager.Commands.Createitem:
					{
						Vector3 position3 = MainManager.player.transform.position;
						int timer = 300;
						if (temp.Length > 3)
						{
							position3 = new Vector3(Convert.ToSingle(temp[3]), Convert.ToSingle(temp[4]), Convert.ToSingle(temp[5]));
						}
						if (temp.Length > 6)
						{
							timer = Convert.ToInt32(temp[6]);
						}
						NPCControl npccontrol = EntityControl.CreateItem(position3, Convert.ToInt32(temp[1]), Convert.ToInt32(temp[2]), Vector3.zero, timer);
						npccontrol.name = npccontrol.name.Insert(0, "Fixed");
						npccontrol.insideid = MainManager.instance.insideid;
						npccontrol.freezeconstraints = true;
						goto IL_A206;
					}
					case MainManager.Commands.Addboard:
					{
						int id;
						if (char.IsNumber(temp[1][0]))
						{
							id = Convert.ToInt32(temp[1]);
						}
						else
						{
							id = (int)Enum.Parse(typeof(MainManager.BoardQuests), temp[1]);
						}
						MainManager.ChangeBoardQuest(id);
						goto IL_A206;
					}
					case MainManager.Commands.Camangle:
						MainManager.instance.camangleoffset = new Vector3(Convert.ToSingle(temp[1]), Convert.ToSingle(temp[2]), Convert.ToSingle(temp[3]));
						goto IL_A206;
					case MainManager.Commands.Camoffset:
						MainManager.instance.camoffset = new Vector3(Convert.ToSingle(temp[1]), Convert.ToSingle(temp[2]), Convert.ToSingle(temp[3]));
						goto IL_A206;
					case MainManager.Commands.Completequest:
						MainManager.CompleteQuest(Convert.ToInt32(temp[1]));
						goto IL_A206;
					case MainManager.Commands.Activateselectedquest:
					{
						if (temp.Length > 1)
						{
							MainManager.instance.flagvar[0] = MainManager.GetValueFromString(temp[1]);
						}
						MainManager.instance.boardquests[0].Remove(MainManager.instance.flagvar[0]);
						MainManager.ChangeBoardQuest(MainManager.instance.flagvar[0], 1);
						int num43 = Convert.ToInt32(MainManager.boardquestdata[MainManager.instance.flagvar[0], 3]);
						if (num43 > -1)
						{
							MainManager.instance.flags[num43] = true;
						}
						if (MainManager.instance.boardquests[0].Count == 0)
						{
							MainManager.instance.boardquests[0].Add(0);
						}
						MainManager.instance.flagstring[11] = MainManager.BoardString();
						MainManager.instance.flags[2] = true;
						goto IL_A206;
					}
					case MainManager.Commands.Resetcamera:
						MainManager.ResetCamera();
						goto IL_A206;
					case MainManager.Commands.Quarterline:
						temp = new string[]
						{
							"",
							"0.25"
						};
						goto IL_398E;
					case MainManager.Commands.Questprompt:
						questboardpromp = true;
						goto IL_A206;
					case MainManager.Commands.Heal:
						if (temp.Length > 1 && temp[1] == "tp")
						{
							MainManager.Heal(new MainManager.Healing[]
							{
								MainManager.Healing.TPOnly
							}, MainManager.instance.partyorder, true, true);
							goto IL_A206;
						}
						MainManager.Heal();
						if (temp.Length > 1)
						{
							MainManager.instance.hudcooldown = 0f;
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Lockmovement:
						MainManager.player.entity.rigid.constraints = (RigidbodyConstraints)122;
						goto IL_A206;
					case MainManager.Commands.Teleportparty:
						if (temp.Length == 1)
						{
							MainManager.TeleportFollowers();
							goto IL_A206;
						}
						if (temp.Length == 2)
						{
							MainManager.TeleportFollowers(Convert.ToBoolean(temp[1]));
							goto IL_A206;
						}
						MainManager.TeleportFollowers(Convert.ToSingle(temp[1]), (temp.Length > 2) ? ((MainManager.TPDir)Enum.Parse(typeof(MainManager.TPDir), temp[2], true)) : MainManager.TPDir.Center, caller.transform);
						goto IL_A206;
					case MainManager.Commands.Flagvalue:
					{
						string newValue3;
						if (temp.Length > 2)
						{
							newValue3 = MainManager.instance.flags[MainManager.instance.flagvar[Convert.ToInt32(temp[2])]].ToString();
						}
						else
						{
							newValue3 = MainManager.instance.flags[Convert.ToInt32(temp[1])].ToString();
						}
						text = text.Replace("|" + command + "|", newValue3);
						num2 = i;
						i = num2 - 1;
						skipi = true;
						goto IL_A206;
					}
					case MainManager.Commands.Removebadgeshop:
					{
						int num44 = Convert.ToInt32(temp[1]);
						int num45;
						if (temp[2] == "var")
						{
							num45 = MainManager.instance.flagvar[Convert.ToInt32(temp[3])];
						}
						else
						{
							num45 = Convert.ToInt32(temp[2]);
						}
						int[] array10 = MainManager.instance.avaliablebadgepool[num44].ToArray();
						int num46 = 0;
						while (num46 < array10.Length)
						{
							if (array10[num46] == num45)
							{
								MainManager.instance.avaliablebadgepool[num44].RemoveAt(num46);
								MainManager.instance.avaliablebadgepool[num44].Insert(num46, -1);
								MainManager.instance.badgeshops[num44].Remove(num45);
								if (num44 == 1)
								{
									MainManager.instance.flagvar[66]++;
									break;
								}
								break;
							}
							else
							{
								num46++;
							}
						}
						goto IL_A206;
					}
					case MainManager.Commands.Savecamera:
						MainManager.SaveCameraPosition(true);
						goto IL_A206;
					case MainManager.Commands.Loadcamera:
						MainManager.SaveCameraPosition(false);
						goto IL_A206;
					case MainManager.Commands.Camspeed:
					{
						float num47 = Convert.ToSingle(temp[1]);
						if (num47 > 0f)
						{
							MainManager.instance.camspeed = num47;
							goto IL_A206;
						}
						MainManager.instance.camspeed = 0.1f;
						goto IL_A206;
					}
					case MainManager.Commands.Stars:
					{
						int num48 = Convert.ToInt32(temp[1]);
						for (int num49 = 0; num49 < num48; num49++)
						{
							SpriteRenderer spriteRenderer = new GameObject().AddComponent<SpriteRenderer>();
							spriteRenderer.sprite = MainManager.guisprites[100];
							spriteRenderer.transform.parent = textholder.transform;
							spriteRenderer.transform.localScale = ((MainManager.battle != null) ? new Vector3(0.5f, 0.25f, 1f) : (Vector3.one / 2f));
							spriteRenderer.transform.localEulerAngles = Vector3.zero;
							spriteRenderer.color = Color.white;
							spriteRenderer.gameObject.layer = 5;
							spriteRenderer.sortingOrder = sort + num49;
							spriteRenderer.transform.localPosition = new Vector3(currentoffset, currentline);
							currentoffset += spriteRenderer.sprite.bounds.extents.x - 0.15f;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Giveitem:
					{
						if (temp[1] == "all")
						{
							for (int num50 = 0; num50 < Enum.GetNames(typeof(MainManager.Items)).Length; num50++)
							{
								MainManager.instance.items[0].Add(num50);
							}
							goto IL_A206;
						}
						EntityControl temptail = (MainManager.instance.tailtarget != null) ? MainManager.instance.tailtarget.GetComponent<EntityControl>() : null;
						int itemtype = Convert.ToInt32(temp[1]);
						int redirect = 0;
						int id2 = -1;
						int num51;
						if (temp[2] == "var")
						{
							num51 = MainManager.instance.flagvar[Convert.ToInt32(temp[3])];
							redirect = Convert.ToInt32(temp[4]);
							if (temp.Length > 5)
							{
								id2 = Convert.ToInt32(temp[5]);
							}
						}
						else
						{
							num51 = Convert.ToInt32(temp[2]);
							redirect = Convert.ToInt32(temp[3]);
							if (temp.Length > 4)
							{
								id2 = Convert.ToInt32(temp[4]);
							}
						}
						MainManager.instance.flagstring[1] = MainManager.menutext[125];
						if (itemtype == 2 && MainManager.instance.flags[681] && num51 != 11)
						{
							num51 = MainManager.GetRandomMedal();
						}
						if (itemtype > -1 && itemtype < 3 && caller != null && caller.interacttype != NPCControl.Interaction.CaravanBadge && caller.interacttype != NPCControl.Interaction.Shop && (itemtype != 1 || MainManager.instance.insideid == -1 || num51 != 110))
						{
							caller.CreateDescWindow((itemtype == 2) ? 2 : 0, num51);
						}
						if ((itemtype == 0 && MainManager.instance.items[0].Count + 1 <= MainManager.instance.maxitems) || itemtype != 0)
						{
							DialogueAnim a2 = textbox.GetComponent<DialogueAnim>();
							a2.shrink = true;
							SpriteRenderer[] t = new SpriteRenderer[3];
							Sprite sprite;
							if (itemtype == 3)
							{
								sprite = MainManager.guisprites[83];
							}
							else if (itemtype > -1)
							{
								sprite = MainManager.GetItemSprite(itemtype == 2, num51);
							}
							else if (num51 >= 20)
							{
								sprite = MainManager.itemsprites[0, 186];
							}
							else if (num51 >= 5)
							{
								sprite = MainManager.itemsprites[0, 7];
							}
							else
							{
								sprite = MainManager.itemsprites[0, 6];
							}
							EntityControl ent = MainManager.GetEntity(id2);
							ent.rigid.velocity = Vector3.zero;
							t[0] = MainManager.NewSpriteObject("tempitem", ent.transform.position + new Vector3(0f, 2.75f, -0.1f), MainManager.instance.globalcamdir.eulerAngles, null, sprite, MainManager.spritemat);
							t[1] = MainManager.NewUIObject("fauxmessage", MainManager.GUICamera.transform, new Vector3(0f, 3f, 10f), Vector3.one, Resources.LoadAll<Sprite>("Sprites/GUI/textbox")[3]).GetComponent<SpriteRenderer>();
							t[0].gameObject.layer = 14;
							SpriteRenderer spriteRenderer2 = MainManager.NewSpriteObject("back", new Vector3(0f, 0f, 0.2f), Vector3.zero, t[0].transform, MainManager.guisprites[85], t[0].material);
							spriteRenderer2.transform.localScale = Vector3.zero;
							spriteRenderer2.gameObject.AddComponent<DialogueAnim>();
							spriteRenderer2.gameObject.layer = 14;
							if (itemtype == 0)
							{
								spriteRenderer2.material.color = new Color(0f, 0.7f, 0.7f);
							}
							else if (itemtype == 1)
							{
								spriteRenderer2.material.color = new Color(1f, 0.3f, 0.4f);
							}
							else
							{
								spriteRenderer2.material.color = new Color(1f, 0.5f, 0f);
							}
							int num52 = 106;
							if (itemtype > -1)
							{
								MainManager.instance.showmoney = 0f;
								MainManager.instance.moneyt = MainManager.instance.money;
							}
							if (itemtype == -1)
							{
								MainManager.instance.showmoney = 1f;
								MainManager.instance.money = Mathf.Clamp(MainManager.instance.money + num51, 0, 999);
								MainManager.instance.flagvar[0] = num51;
								num52 = 110;
								itemtype = 0;
							}
							else if (itemtype < 2)
							{
								MainManager.instance.flagstring[0] = MainManager.itemdata[0, num51, 0];
								MainManager.instance.flagstring[1] = MainManager.itemdata[0, num51, 3];
								MainManager.instance.items[itemtype].Add(num51);
							}
							else if (itemtype == 2)
							{
								MainManager.instance.flagstring[0] = MainManager.GetBadgeName(num51);
								MainManager.instance.flagstring[1] = MainManager.badgedata[num51, 6];
								MainManager.instance.badges.Add(new int[]
								{
									num51,
									-2
								});
							}
							else if (itemtype == 3)
							{
								MainManager.instance.flagvar[14]++;
								MainManager.instance.flagstring[0] = MainManager.menutext[112];
								MainManager.instance.crystalbflags[num51] = true;
							}
							MainManager.instance.StartCoroutine(MainManager.SetText("|sort,1||center||halfline|" + MainManager.menutext[num52], Vector3.zero, t[1].transform));
							ent.overrideanim = true;
							ent.animstate = 4;
							MainManager.PlaySound("ItemGet" + itemtype);
							yield return new WaitForSeconds(0.45f);
							t[2] = MainManager.NewUIObject("fauxcursor", t[1].transform, new Vector3(6f, -2f), Vector3.one, MainManager.cursorsprite[0], 20).GetComponent<SpriteRenderer>();
							t[2].gameObject.AddComponent<SpriteBounce>().SetUp(0.1f, 7f);
							t[2].transform.localEulerAngles = new Vector3(0f, 0f, -90f);
							MainManager.instance.waitinput = true;
							yield return null;
							while (MainManager.instance.waitinput)
							{
								yield return null;
							}
							for (int num53 = 0; num53 < t.Length; num53++)
							{
								Object.Destroy(t[num53].gameObject);
							}
							a2.shrink = false;
							MainManager.instance.waitinput = false;
							MainManager.instance.skiptext = false;
							ent.overrideanim = false;
							ent.animstate = 0;
							string str2 = "";
							if (!MainManager.instance.flags[31] && itemtype == 2)
							{
								str2 = string.Concat(new object[]
								{
									"|tail,null||destroydescbox||blank||boxstyle,4|",
									MainManager.commondialogue[31],
									"|flag,31,true||break||gettail,",
									temptail.originalid,
									"||boxstyle,0|"
								});
							}
							text = MainManager.OrganizeLines(str2 + "|destroydescbox||blank|" + MainManager.GetDialogueText(redirect), linebreak.Value, size.x, fonttype);
							skipi = true;
							i = -1;
							a2 = null;
							t = null;
							ent = null;
						}
						temptail = null;
						goto IL_A206;
					}
					case MainManager.Commands.Setvar:
						if (temp[1] == "add" || temp[1] == "sub")
						{
							MainManager.instance.flagvar[Convert.ToInt32(temp[2])] += ((temp[3] == "var") ? MainManager.instance.flagvar[Convert.ToInt32(temp[4])] : Convert.ToInt32(temp[3])) * ((temp[1] == "sub") ? -1 : 1);
							goto IL_A206;
						}
						MainManager.instance.flagvar[Convert.ToInt32(temp[1])] = Convert.ToInt32(temp[2]);
						goto IL_A206;
					case MainManager.Commands.Jump:
					{
						EntityControl entity7;
						if (caller != null)
						{
							entity7 = MainManager.GetEntity(temp[1], caller.entity);
						}
						else
						{
							entity7 = MainManager.GetEntity(Convert.ToInt32(temp[1]));
						}
						if (temp.Length == 2)
						{
							entity7.Jump();
							goto IL_A206;
						}
						entity7.Jump(Convert.ToSingle(temp[2]));
						goto IL_A206;
					}
					case MainManager.Commands.Position:
						textholder.transform.localPosition = new Vector3(textholder.transform.localPosition.x, Convert.ToSingle(temp[1]), textholder.transform.localPosition.z);
						goto IL_A206;
					case MainManager.Commands.Hidespeed:
						textbox.GetComponent<DialogueAnim>().shrinkspeed = Convert.ToSingle(temp[1]);
						goto IL_A206;
					case MainManager.Commands.Breakflag:
						if (!MainManager.instance.flags[Convert.ToInt32(temp[1])])
						{
							end = true;
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Bleep:
						if (temp.Length == 2)
						{
							if (temp[1][0] == '@')
							{
								EntityControl entity8 = MainManager.GetEntity(temp[1].Replace("@", ""));
								temp[1] = entity8.dialoguebleepid.ToString();
								bleeppitch = entity8.bleeppitch;
							}
							else
							{
								int num54 = Convert.ToInt32(temp[1]) + 1;
								temp[1] = MainManager.endata[num54].bleepid.ToString();
								bleeppitch = MainManager.endata[num54].bleeppitch;
							}
						}
						else
						{
							bleeppitch = Convert.ToSingle(temp[2]);
							if (temp.Length > 3)
							{
								bleepvolume = Convert.ToSingle(temp[3]);
							}
						}
						bleep = Resources.Load<AudioClip>("Audio/Sounds/Dialogue/Dialogue" + temp[1]);
						goto IL_A206;
					case MainManager.Commands.Exitgame:
						if (MainManager.pausemenu != null && MainManager.pausemenu.calledfrommain)
						{
							Application.Quit();
						}
						else
						{
							MainManager.FadeMusic(0.15f);
							MainManager.PlayTransition(0, 0, 0.1f, Color.black);
							yield return new WaitForSeconds(1f);
							SceneManager.LoadScene(0);
						}
						end = true;
						goto IL_A206;
					case MainManager.Commands.Openpause:
						if (MainManager.pausemenu != null)
						{
							MainManager.pausemenu.ChangeWindow(Convert.ToInt32(temp[1]));
						}
						end = true;
						goto IL_A206;
					case MainManager.Commands.Font:
						fonttype = Convert.ToInt32(temp[1]);
						if (temp.Length > 2)
						{
							fontlock = (temp[2] == "lock");
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Exp:
					case MainManager.Commands.Level:
					{
						bool flag5 = false;
						int num55;
						if (temp[1] == "add")
						{
							if (temp[2].Contains("var"))
							{
								num55 = MainManager.instance.flagvar[Convert.ToInt32(temp[2].Replace("var", ""))];
							}
							else
							{
								num55 = Convert.ToInt32(temp[2]);
							}
						}
						else if (temp[1] == "sub")
						{
							if (temp[2].Contains("var"))
							{
								num55 = -MainManager.instance.flagvar[Convert.ToInt32(temp[2].Replace("var", ""))];
							}
							else
							{
								num55 = -Convert.ToInt32(temp[2]);
							}
						}
						else
						{
							flag5 = true;
							if (temp[1].Contains("var"))
							{
								num55 = MainManager.instance.flagvar[Convert.ToInt32(temp[1].Replace("var", ""))];
							}
							else
							{
								num55 = Convert.ToInt32(temp[1]);
							}
						}
						if (flag5)
						{
							if (com == MainManager.Commands.Exp)
							{
								MainManager.instance.partyexp = Mathf.Clamp(num55, 0, MainManager.instance.neededexp - 1);
								goto IL_A206;
							}
							MainManager.instance.partylevel = Mathf.Clamp(num55, 1, 99);
							goto IL_A206;
						}
						else
						{
							if (com == MainManager.Commands.Exp)
							{
								MainManager.instance.partyexp = Mathf.Clamp(MainManager.instance.partyexp + num55, 0, MainManager.instance.neededexp - 1);
								goto IL_A206;
							}
							MainManager.instance.partylevel = Mathf.Clamp(MainManager.instance.partylevel + num55, 1, 99);
							goto IL_A206;
						}
						break;
					}
					case MainManager.Commands.Icon:
					{
						int num56 = Convert.ToInt32(temp[1]);
						float d2 = 1f;
						if (temp.Length > 2)
						{
							d2 = Convert.ToSingle(temp[2]);
						}
						SpriteRenderer component2 = MainManager.NewUIObject("icon" + num56, textholder.transform, new Vector3(currentoffset, currentline), new Vector3(size.x, size.y, 1f) * d2, MainManager.guisprites[num56], (temp.Length > 3) ? Convert.ToInt32(temp[3]) : sort).GetComponent<SpriteRenderer>();
						component2.transform.localPosition = new Vector3(currentoffset, currentline) + component2.sprite.pivot / (component2.sprite.pixelsPerUnit * 2f);
						if (num56 == 24)
						{
							component2.transform.localPosition += new Vector3(-0.1f, -0.25f);
						}
						component2.gameObject.tag = "Letter";
						buts.Add(component2.gameObject);
						currentoffset += component2.bounds.size.x;
						if (linebreak != null)
						{
							MainManager.BreakLine(ref currentoffset, ref currentline, linebreak.Value, size);
						}
						FontEffects fontEffects2 = component2.gameObject.AddComponent<FontEffects>();
						fontEffects2.SetEffects(shaky, wavy, rainbow, false, false, 0, i);
						if (num56 == 146)
						{
							fontEffects2.rotate = true;
						}
						if (single)
						{
							text = text.Replace("|" + command + "|", "\t");
							num2 = i;
							i = num2 - 1;
							skipi = true;
						}
						if (!dialogue)
						{
							goto IL_A206;
						}
						num2 = writen;
						writen = num2 + 1;
						MainManager.SetTalk(true, true);
						if (speed > 0f && !MainManager.instance.skiptext)
						{
							MainManager.PlayBleep(bleep, bleeppitch, bleepvolume, i);
							yield return new WaitForSeconds(speed);
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Dropshadow:
						if (temp.Length == 1)
						{
							dropshadow = null;
							goto IL_A206;
						}
						if (temp.Length == 2)
						{
							dropshadow = new Vector2?(Vector2.one * Convert.ToSingle(temp[1]));
							goto IL_A206;
						}
						dropshadow = new Vector2?(new Vector2(Convert.ToSingle(temp[1]), Convert.ToSingle(temp[2])));
						goto IL_A206;
					case MainManager.Commands.Clonestring:
						MainManager.instance.flagstring[Convert.ToInt32(temp[2])] = MainManager.instance.flagstring[Convert.ToInt32(temp[1])];
						goto IL_A206;
					case MainManager.Commands.Pauseline:
						if (MainManager.pausemenu != null && MainManager.pausemenu.windowid < 3)
						{
							goto IL_398E;
						}
						goto IL_A206;
					case MainManager.Commands.Addfollower:
						goto IL_1CF2;
					case MainManager.Commands.Fadeletter:
						fadeletter = !fadeletter;
						goto IL_A206;
					case MainManager.Commands.Setprize:
						if (temp[1] == "find")
						{
							int num57 = MainManager.IntFromString(temp[2]);
							for (int num58 = 0; num58 < MainManager.instance.prizeids.Length; num58++)
							{
								if (MainManager.instance.prizeids[num58] == num57 && MainManager.instance.flagvar[MainManager.instance.prizeflags[num58]] == 2)
								{
									MainManager.instance.flagvar[MainManager.instance.prizeflags[num58]] = 3;
									List<int> list = new List<int>(MainManager.caravanorder);
									list.Remove(MainManager.instance.prizeids[num58]);
									MainManager.caravanorder = list.ToArray();
									break;
								}
							}
							goto IL_A206;
						}
						if (temp[1] == "this" && caller != null)
						{
							for (int num59 = 0; num59 < MainManager.instance.prizeids.Length; num59++)
							{
								if (MainManager.instance.prizeids[num59] == caller.entity.animstate && MainManager.instance.flagvar[MainManager.instance.prizeflags[num59]] == 2)
								{
									MainManager.instance.flagvar[MainManager.instance.prizeflags[num59]] = 3;
									break;
								}
							}
							goto IL_A206;
						}
						MainManager.instance.flagvar[MainManager.instance.prizeflags[(temp[1] == "var") ? Convert.ToInt32(temp[2]) : Convert.ToInt32(temp[1])]] = 3;
						goto IL_A206;
					case MainManager.Commands.Libraryline:
						if (MainManager.pausemenu != null && MainManager.pausemenu.windowid == 3)
						{
							goto IL_398E;
						}
						goto IL_A206;
					case MainManager.Commands.Shopline:
						if (MainManager.FreePlayer())
						{
							goto IL_398E;
						}
						if (MainManager.instance.itemlist != null && MainManager.pausemenu == null)
						{
							goto IL_398E;
						}
						goto IL_A206;
					case MainManager.Commands.Shakecamera:
						MainManager.ShakeScreen(Vector3.one * Convert.ToSingle(temp[1]), Convert.ToSingle(temp[2]), temp.Length > 3 && temp[3] == "true");
						goto IL_A206;
					case MainManager.Commands.Removemaplimits:
						if (temp.Length == 1)
						{
							MainManager.map.RemoveLimit(false);
							goto IL_A206;
						}
						MainManager.map.RemoveLimit(Convert.ToBoolean(temp[1]));
						goto IL_A206;
					case MainManager.Commands.Resetmaplimits:
						if (temp.Length == 1)
						{
							MainManager.map.RestoreLimit(false);
							goto IL_A206;
						}
						MainManager.map.RestoreLimit(Convert.ToBoolean(temp[1]));
						goto IL_A206;
					case MainManager.Commands.Music:
					{
						float fadespeed = 0.1f;
						if (temp.Length > 2)
						{
							fadespeed = Convert.ToSingle(temp[2]);
						}
						if (temp.Length == 1 || temp[1] == "null")
						{
							MainManager.ChangeMusic(null, fadespeed);
							goto IL_A206;
						}
						MainManager.ChangeMusic(temp[1], fadespeed);
						goto IL_A206;
					}
					case MainManager.Commands.Sound:
					{
						float pitch = 1f;
						float volume = 1f;
						int id3 = -1;
						bool loop = false;
						if (temp.Length > 2)
						{
							id3 = Convert.ToInt32(temp[2]);
						}
						if (temp.Length > 3)
						{
							pitch = Convert.ToSingle(temp[3]);
						}
						if (temp.Length > 4)
						{
							volume = Convert.ToSingle(temp[4]);
						}
						if (temp.Length > 5)
						{
							loop = Convert.ToBoolean(temp[5]);
						}
						MainManager.PlaySound(temp[1], id3, pitch, volume, loop);
						goto IL_A206;
					}
					case MainManager.Commands.Destroydescbox:
						if ((caller != null && caller.interacttype != NPCControl.Interaction.CaravanBadge && caller.interacttype != NPCControl.Interaction.Shop) || (temp.Length > 1 && temp[1] == "1"))
						{
							caller.DestroyDescWindow();
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Resetregion:
						for (int num60 = 0; num60 < MainManager.instance.regionalflags.Length; num60++)
						{
							MainManager.instance.regionalflags[num60] = false;
						}
						goto IL_A206;
					case MainManager.Commands.Mapflag:
						if (temp.Length > 3)
						{
							MainManager.map.mapflags[MainManager.instance.flagvar[Convert.ToInt32(temp[2])]] = Convert.ToBoolean(temp[3]);
							goto IL_A206;
						}
						MainManager.map.mapflags[Convert.ToInt32(temp[1])] = Convert.ToBoolean(temp[2]);
						goto IL_A206;
					case MainManager.Commands.Checkmapflag:
						if (temp.Length > 3)
						{
							Convert.ToBoolean(temp[3]);
						}
						if (MainManager.map.mapflags[Convert.ToInt32(temp[1])])
						{
							text = MainManager.OrganizeLines(MainManager.GetDialogueText(Convert.ToInt32(temp[2])), linebreak.Value, size.x, fonttype);
							MainManager.instance.skiptext = false;
							skipi = true;
							i = -1;
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Kinematicplayer:
						if (temp[1] == "temp")
						{
							MainManager.player.entity.rigid.isKinematic = true;
							text += "|kinematicplayer,false|";
							goto IL_A206;
						}
						MainManager.player.entity.rigid.isKinematic = Convert.ToBoolean(temp[1]);
						goto IL_A206;
					case MainManager.Commands.Removefollower:
						if (temp.Length == 1)
						{
							MainManager.instance.extrafollowers = new List<int>();
							goto IL_A206;
						}
						MainManager.instance.extrafollowers.Remove(Convert.ToInt32(temp[1]));
						goto IL_A206;
					case MainManager.Commands.Shoppool:
						if (!(temp[1] == "reset"))
						{
							MainManager.instance.badgeshops[temp[1].Contains("var") ? MainManager.instance.flagvar[Convert.ToInt32(temp[1].Replace("var", ""))] : Convert.ToInt32(temp[1])].Add(temp[2].Contains("var") ? MainManager.instance.flagvar[Convert.ToInt32(temp[2].Replace("var", ""))] : Convert.ToInt32(temp[2]));
							goto IL_A206;
						}
						if (temp.Length == 2)
						{
							for (int num61 = 0; num61 < MainManager.instance.badgeshops.Length; num61++)
							{
								MainManager.instance.badgeshops[num61] = new List<int>();
							}
							goto IL_A206;
						}
						MainManager.instance.badgeshops[temp[2].Contains("var") ? MainManager.instance.flagvar[Convert.ToInt32(temp[2].Replace("var", ""))] : Convert.ToInt32(temp[2])] = new List<int>();
						goto IL_A206;
					case MainManager.Commands.Optiontovar:
						MainManager.instance.flagvar[(temp.Length == 1) ? 0 : Convert.ToInt32(temp[1])] = MainManager.instance.option;
						goto IL_A206;
					case MainManager.Commands.Setbreak:
						if (temp.Length <= 1)
						{
							goto IL_A206;
						}
						if (temp[1] == "null")
						{
							linebreak = null;
						}
						else
						{
							linebreak = new float?(Convert.ToSingle(temp[1]));
						}
						if (temp.Length > 2 && temp[2] == "true")
						{
							text = MainManager.OrganizeLines(text, linebreak.Value, size.x, fonttype);
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Librarybook:
						MainManager.instance.flagvar[15]++;
						Object.FindObjectOfType<LibraryShelf>().Refresh();
						goto IL_A206;
					case MainManager.Commands.Area:
						text = text.Replace("|area|", MainManager.areanames[MainManager.instance.areaid]);
						num2 = i;
						i = num2 - 1;
						skipi = true;
						goto IL_A206;
					case MainManager.Commands.Battle:
					{
						int[] array11;
						if (temp[1] == "all")
						{
							array11 = MainManager.GradualFill(Enum.GetNames(typeof(MainManager.Enemies)).Length);
						}
						else
						{
							string[] array12 = temp[1].Split(new char[]
							{
								'.'
							});
							array11 = new int[array12.Length];
							for (int num62 = 0; num62 < array11.Length; num62++)
							{
								array11[num62] = Convert.ToInt32(array12[num62]);
							}
						}
						MainManager.SaveCameraPosition();
						MainManager.instance.StartCoroutine(BattleControl.StartBattle(array11, (temp.Length > 3) ? Convert.ToInt32(temp[3]) : -1, -1, (temp.Length > 2) ? temp[2] : null, null, false));
						bool tmes = MainManager.instance.message;
						MainManager.instance.message = false;
						textbox.GetComponent<DialogueAnim>().shrink = true;
						yield return new WaitForSeconds(0.5f);
						while (MainManager.battle != null)
						{
							yield return null;
						}
						yield return new WaitForSeconds(0.5f);
						MainManager.instance.message = tmes;
						textbox.GetComponent<DialogueAnim>().shrink = false;
						goto IL_A206;
					}
					case MainManager.Commands.Removestat:
						MonoBehaviour.print(MainManager.GetValueFromString(temp[1]));
						MainManager.instance.statbonus.RemoveAt(MainManager.GetValueFromString(temp[1]));
						if (temp.Length > 2 && temp[2] == "true")
						{
							MainManager.ApplyStatBonus();
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Addstat:
						MainManager.AddStatBonus((MainManager.StatBonus)MainManager.GetValueFromString(temp[1]), MainManager.GetValueFromString(temp[2]), MainManager.GetValueFromString(temp[3]));
						if (temp.Length > 4 && temp[4] == "true")
						{
							MainManager.ApplyStatBonus();
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Checkpos:
					{
						if (!(MainManager.player != null))
						{
							goto IL_A206;
						}
						bool flag6 = false;
						string a5 = temp[1];
						if (!(a5 == "x"))
						{
							if (!(a5 == "y"))
							{
								if (a5 == "z")
								{
									flag6 = (MainManager.player.transform.position.z >= Convert.ToSingle(temp[2]));
								}
							}
							else
							{
								flag6 = (MainManager.player.transform.position.y >= Convert.ToSingle(temp[2]));
							}
						}
						else
						{
							flag6 = (MainManager.player.transform.position.x >= Convert.ToSingle(temp[2]));
						}
						if (!flag6)
						{
							text = MainManager.OrganizeLines(MainManager.GetDialogueText(Convert.ToInt32(temp[3])), linebreak.Value, size.x, fonttype);
							MainManager.instance.skiptext = false;
							skipi = true;
							i = -1;
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Triui:
						ui3d = !ui3d;
						goto IL_A206;
					case MainManager.Commands.Backline:
						currentoffset = 0f;
						currentline += 0.7f * size.y * ((temp.Length > 2) ? Convert.ToSingle(temp[2]) : 1f);
						if (dialogue && !MainManager.backtracking)
						{
							MainManager.tempdiag = MainManager.tempdiag + "|" + command + "|";
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Cardbattle:
					{
						CardGame cardGame = MainManager.MainCamera.gameObject.AddComponent<CardGame>();
						Transform tt = MainManager.instance.texttail;
						int opponentanimid = (temp.Length == 1 || temp[1] == "this") ? (caller.entity.originalid + 1) : Convert.ToInt32(temp[1]);
						int[] opponentdeck = null;
						if (temp.Length > 3)
						{
							if (temp[3][0] == '@')
							{
								opponentdeck = CardGame.pdecks[Convert.ToInt32(temp[3].Replace("@", ""))];
							}
							else
							{
								List<int> list2 = new List<int>();
								string[] array13 = temp[3].Split(new char[]
								{
									'@'
								});
								for (int num63 = 0; num63 < array13.Length; num63++)
								{
									list2.Add(Convert.ToInt32(array13[num63]));
								}
								opponentdeck = list2.ToArray();
							}
						}
						int mapid = (temp.Length > 2) ? Convert.ToInt32(temp[2]) : -1;
						cardGame.StartCoroutine(cardGame.StartCard(opponentanimid, mapid, opponentdeck));
						textbox.GetComponent<DialogueAnim>().shrink = true;
						MainManager.instance.message = false;
						yield return new WaitForSeconds(1f);
						while (MainManager.instance.cardgame != null)
						{
							yield return null;
						}
						MainManager.instance.message = true;
						MainManager.instance.texttail = tt;
						textbox.GetComponent<DialogueAnim>().shrink = false;
						if (MainManager.instance.blinker == null)
						{
							MainManager.instance.blinker = textbox.transform.GetChild(1).GetComponent<SpriteRenderer>();
						}
						tt = null;
						goto IL_A206;
					}
					case MainManager.Commands.Boxspeed:
						textbox.GetComponent<DialogueAnim>().shrinkspeed = Convert.ToSingle(temp[1]);
						goto IL_A206;
					case MainManager.Commands.Switch:
					{
						EntityControl entity9 = MainManager.GetEntity(Convert.ToInt32(temp[1]));
						if (temp.Length == 2)
						{
							entity9.npcdata.hit = !entity9.npcdata.hit;
							goto IL_A206;
						}
						entity9.npcdata.hit = Convert.ToBoolean(temp[2]);
						goto IL_A206;
					}
					case MainManager.Commands.Checkminibubble:
						if (Object.FindObjectsOfType<MiniBubble>().Length != 0)
						{
							text = MainManager.OrganizeLines(MainManager.GetDialogueText(Convert.ToInt32(temp[1])), linebreak.Value, size.x, fonttype);
							MainManager.instance.skiptext = false;
							skipi = true;
							i = -1;
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Breakend:
						text = text.Replace("|" + command + "|", "|break||destroyminibubble||end|");
						num2 = i;
						i = num2 - 1;
						skipi = true;
						goto IL_A206;
					case MainManager.Commands.DungeonGame:
					case MainManager.Commands.BeeGame:
					case MainManager.Commands.PartyGame:
					{
						textbox.GetComponent<DialogueAnim>().shrink = true;
						Color c = RenderSettings.fogColor;
						MainManager.FadeIn();
						string music = MainManager.music[0].clip.name;
						yield return new WaitForSeconds(1f);
						Vector3[] ml = new Vector3[]
						{
							MainManager.map.camlimitneg,
							MainManager.map.camlimitpos
						};
						MainManager.map.camlimitneg = Vector3.one * -999f;
						MainManager.map.camlimitpos = Vector3.one * 999f;
						Transform tt = null;
						commands = com;
						if (commands != MainManager.Commands.DungeonGame)
						{
							if (commands == MainManager.Commands.BeeGame)
							{
								tt = new GameObject().AddComponent<FlappyBee>().transform;
								MainManager.instance.flagvar[6] = 0;
							}
						}
						else
						{
							tt = new GameObject().AddComponent<MazeGame>().transform;
							MainManager.instance.flagvar[6] = 1;
						}
						tt.position = new Vector3(0f, 70f);
						MainManager.instance.camspeed = 1f;
						yield return null;
						MainManager.instance.camspeed = 0.1f;
						yield return null;
						while (tt != null)
						{
							yield return null;
						}
						MainManager.map.camlimitneg = ml[0];
						MainManager.map.camlimitpos = ml[1];
						MainManager.ResetCamera(true);
						RenderSettings.fogEndDistance = MainManager.map.fogend;
						RenderSettings.fogColor = c;
						MainManager.ChangeMusic(music);
						MainManager.FadeOut();
						yield return EventControl.sec;
						MainManager.instance.skiptext = false;
						textbox.GetComponent<DialogueAnim>().shrink = false;
						if (MainManager.instance.blinker == null)
						{
							MainManager.instance.blinker = textbox.transform.GetChild(1).GetComponent<SpriteRenderer>();
						}
						c = default(Color);
						music = null;
						ml = null;
						tt = null;
						goto IL_A206;
					}
					case MainManager.Commands.Copyvar:
						MainManager.instance.flagvar[Convert.ToInt32(temp[2])] = Convert.ToInt32(temp[1]);
						goto IL_A206;
					case MainManager.Commands.Addvar:
					{
						int num64 = MainManager.GetValueFromString(temp[2]);
						bool flag7 = false;
						if (temp.Length > 3)
						{
							string a5 = temp[3];
							if (!(a5 == "-"))
							{
								if (!(a5 == "*"))
								{
									if (a5 == "/")
									{
										flag7 = true;
										MainManager.instance.flagvar[Convert.ToInt32(temp[1])] /= num64;
									}
								}
								else
								{
									flag7 = true;
									MainManager.instance.flagvar[Convert.ToInt32(temp[1])] *= num64;
								}
							}
							else
							{
								num64 *= -1;
							}
						}
						if (!flag7)
						{
							MainManager.instance.flagvar[Convert.ToInt32(temp[1])] += num64;
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Showtokens:
						if (temp.Length == 1)
						{
							if (tokenbox != null)
							{
								Object.Destroy(tokenbox.gameObject);
							}
							SpriteRenderer component3 = MainManager.NewUIObject("TokenBar", MainManager.GUICamera.transform, new Vector3(15f, (MainManager.instance.showmoney > 0f) ? -3f : -4.25f, 10f), new Vector3(0.55f, 0.6f, 1f), MainManager.guisprites[4], 0).GetComponent<SpriteRenderer>();
							component3.color = new Color(0.9f, 0.5f, 0f);
							tokenbox = component3.transform;
							MainManager.instance.StartCoroutine(MainManager.SetText("|sort,10||color,4||dropshadow,0.1,-0.1|" + MainManager.instance.flagvar[27].ToString().PadLeft(4, '0'), 2, null, false, false, new Vector3(-0.75f, -0.4f), Vector3.zero, Vector2.one * 1.75f, component3.transform, null));
							component3 = MainManager.NewUIObject("Icon", tokenbox, new Vector3(-2.25f, 0.1f), Vector3.one * 2f, MainManager.itemsprites[0, 110], 1).GetComponent<SpriteRenderer>();
							DialogueAnim dialogueAnim = tokenbox.gameObject.AddComponent<DialogueAnim>();
							dialogueAnim.targetpos = new Vector3(7f, dialogueAnim.transform.localPosition.y, 10f);
							dialogueAnim.targetscale = new Vector3(0.55f, 0.6f, 1f);
							dialogueAnim.speed = 0.4f;
							goto IL_A206;
						}
						if (tokenbox != null && temp[1] == "null")
						{
							Object.Destroy(tokenbox.gameObject);
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Igcolmove:
					{
						EntityControl entity10 = MainManager.GetEntity(temp[1], caller.entity);
						if (entity10 != null && MainManager.player != null)
						{
							returnentitycol.Add(entity10);
							Physics.IgnoreCollision(entity10.ccol, MainManager.player.entity.ccol, true);
							if (entity10.npcdata != null)
							{
								if (entity10.npcdata.pusher != null)
								{
									Physics.IgnoreCollision(entity10.npcdata.pusher, MainManager.player.entity.ccol, true);
								}
								if (entity10.npcdata.scol != null)
								{
									Physics.IgnoreCollision(entity10.npcdata.scol, MainManager.player.entity.ccol, true);
								}
								if (entity10.npcdata.boxcol != null)
								{
									Physics.IgnoreCollision(entity10.npcdata.boxcol, MainManager.player.entity.ccol, true);
								}
							}
							MainManager.player.entity.hitwall = false;
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.LetterPrompt:
						MainManager.instance.inputcooldown = 5f;
						MainManager.instance.flagstring[2] = command;
						MainManager.instance.flagvar[5] = -1;
						textbox.GetComponent<DialogueAnim>().shrink = true;
						MainManager.CreateLetterPrompt(-1, textbox.GetComponent<SpriteRenderer>().color);
						goto IL_A206;
					case MainManager.Commands.Define:
						MainManager.define.Add(new string[]
						{
							temp[2],
							temp[1]
						});
						goto IL_A206;
					case MainManager.Commands.Loadmap:
						if (temp.Length == 1)
						{
							MainManager.LoadMap();
						}
						else
						{
							MainManager.LoadMap(Convert.ToInt32(temp[1]));
						}
						yield return null;
						goto IL_A206;
					case MainManager.Commands.Checkanim:
					{
						if (!(caller != null))
						{
							goto IL_A206;
						}
						EntityControl entity11 = MainManager.GetEntity(temp[1], caller.entity);
						bool flag8 = temp[2][0] == '!';
						int num65 = Convert.ToInt32(temp[2].Replace("!", ""));
						if ((flag8 && entity11.animstate != num65) || entity11.animstate == num65)
						{
							text = MainManager.OrganizeLines(MainManager.GetDialogueText(Convert.ToInt32(temp[3])), linebreak.Value, size.x, fonttype);
							MainManager.instance.skiptext = false;
							skipi = true;
							i = -1;
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Camlimit:
						MainManager.map.camlimitneg = MainManager.VectorFromString(new string[]
						{
							temp[1],
							temp[2],
							temp[3]
						});
						MainManager.map.camlimitneg = MainManager.VectorFromString(new string[]
						{
							temp[4],
							temp[5],
							temp[6]
						});
						goto IL_A206;
					case MainManager.Commands.Waitcn:
						while (MainManager.chaptername != null)
						{
							yield return null;
						}
						goto IL_A206;
					case MainManager.Commands.Faketail:
						if (textbox != null && textbox.GetComponent<DialogueAnim>() != null)
						{
							DialogueAnim a2 = textbox.GetComponent<DialogueAnim>();
							a2.shrink = true;
							yield return new WaitForSeconds(0.15f);
							a2.shrink = false;
							yield return new WaitForSeconds(0.15f);
							a2 = null;
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Unpauseline:
						if (MainManager.pausemenu == null)
						{
							goto IL_398E;
						}
						goto IL_A206;
					case MainManager.Commands.Cberrytotal:
					case MainManager.Commands.Medaltotal:
					{
						string newValue4 = (com == MainManager.Commands.Cberrytotal) ? MainManager.CrystalBerryAmmount().ToString() : MainManager.instance.badges.Count.ToString();
						text = text.Replace("|" + command + "|", newValue4);
						num2 = i;
						i = num2 - 1;
						skipi = true;
						goto IL_A206;
					}
					case MainManager.Commands.Single:
						if (temp.Length > 1)
						{
							single = Convert.ToBoolean(temp[1]);
							goto IL_A206;
						}
						single = !single;
						goto IL_A206;
					case MainManager.Commands.Tab:
						if (ndd != null)
						{
							ndd.tabSize = Convert.ToSingle(temp[1]);
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Singlebreak:
						if (single)
						{
							text = MainManager.OrganizeLines(text, 99999f, size.x, fonttype);
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Librarysize:
						if (!(MainManager.pausemenu != null) || MainManager.pausemenu.windowid != 3)
						{
							goto IL_A206;
						}
						break;
					case MainManager.Commands.Mothfly:
					{
						int num66 = (temp.Length > 1) ? Convert.ToInt32(temp[1]) : 1;
						string text6 = "";
						float num67 = (temp.Length > 2) ? Convert.ToSingle(temp[2]) : 0.35f;
						for (int num68 = 0; num68 < num66; num68++)
						{
							text6 = string.Concat(new object[]
							{
								text6,
								"|icon,146,",
								num67,
								"|"
							});
							if (writen < 2)
							{
								if (num68 == 0)
								{
									text6 += "  ";
								}
								if (num68 == 1)
								{
									text6 += " ";
								}
							}
						}
						text = text.Replace("|" + command + "|", text6);
						num2 = i;
						i = num2 - 1;
						skipi = true;
						goto IL_A206;
					}
					case MainManager.Commands.Chapterintro:
					{
						bool tmes = MainManager.instance.message;
						bool a = MainManager.instance.minipause;
						MainManager.chaptername = MainManager.instance.StartCoroutine(MainManager.ChapterName(Convert.ToInt32(temp[1])));
						while (MainManager.chaptername != null)
						{
							yield return null;
						}
						MainManager.instance.message = tmes;
						MainManager.instance.minipause = a;
						goto IL_A206;
					}
					case MainManager.Commands.Backbox:
						if (backbox == null)
						{
							backbox = MainManager.NewUIObject("backbox", textholder.transform, Vector3.zero, Vector3.one, MainManager.guisprites[0], sort - 2).GetComponent<SpriteRenderer>();
							Color color = Color.white;
							if (temp.Length > 1)
							{
								color = MainManager.instance.textcolors[Convert.ToInt32(temp[1])];
							}
							backbox.color = new Color(color.r, color.g, color.b, 0.4f);
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Testdiag:
						testing = true;
						goto IL_A206;
					case MainManager.Commands.Lore:
						MainManager.instance.flagstring[0] = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/LoreText").ToString().Split(new char[]
						{
							'\n'
						})[MainManager.instance.flagvar[0]].Split(new char[]
						{
							'@'
						})[1] + "|break||hide||fwait,0.05||boxstyle,3||fwait,0.05||hide||goto,1|";
						temp = new string[]
						{
							"string",
							"0",
							"true"
						};
						goto IL_66C8;
					case MainManager.Commands.Follow:
						temp = new string[]
						{
							"",
							"this",
							"true"
						};
						goto IL_1CF2;
					case MainManager.Commands.Transitionsort:
					{
						SpriteRenderer transitionSprite = MainManager.GetTransitionSprite();
						if (transitionSprite != null)
						{
							transitionSprite.sortingOrder = Convert.ToInt32(temp[1]);
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Addquest:
					{
						if (temp.Length == 2)
						{
							MainManager.AddQuest(Convert.ToInt32(temp[1]));
							goto IL_A206;
						}
						int num69 = Convert.ToInt32(temp[2]);
						if (num69 > 0)
						{
							MainManager.UpdateJounal();
						}
						MainManager.ChangeBoardQuest(Convert.ToInt32(temp[1]), num69);
						goto IL_A206;
					}
					case MainManager.Commands.Textangle:
						if (temp.Length == 1)
						{
							textholder.transform.localEulerAngles = Vector3.zero;
							goto IL_A206;
						}
						textholder.transform.localEulerAngles = MainManager.VectorFromString(new string[]
						{
							temp[1],
							temp[2],
							temp[3]
						});
						goto IL_A206;
					case MainManager.Commands.Particle:
						MainManager.PlayParticle(temp[1], MainManager.GetEntity(caller, temp[2]).transform.position + MainManager.VectorFromString(new string[]
						{
							temp[3],
							temp[4],
							temp[5]
						}));
						goto IL_A206;
					case MainManager.Commands.Itemname:
						if (temp.Length >= 3 && temp[2] == "1")
						{
							MainManager.instance.flagstring[(temp.Length > 2) ? Convert.ToInt32(temp[2]) : 0] = (MainManager.instance.flags[681] ? MainManager.menutext[59] : MainManager.badgedata[MainManager.IntFromString(temp[1]), 0]);
							goto IL_A206;
						}
						MainManager.instance.flagstring[(temp.Length > 2) ? Convert.ToInt32(temp[2]) : 0] = MainManager.itemdata[0, MainManager.IntFromString(temp[1]), 0];
						goto IL_A206;
					case MainManager.Commands.Addprize:
						MainManager.AddPrizeMedal(Convert.ToInt32(temp[1]));
						goto IL_A206;
					case MainManager.Commands.Entityalive:
					{
						EntityControl entity12 = MainManager.GetEntity(Convert.ToInt32(temp[1]));
						if (entity12 != null && entity12.gameObject.activeInHierarchy)
						{
							text = MainManager.OrganizeLines(MainManager.GetDialogueText(Convert.ToInt32(temp[2])), linebreak.Value, size.x, fonttype);
							MainManager.instance.skiptext = false;
							skipi = true;
							i = -1;
							goto IL_A206;
						}
						goto IL_A206;
					}
					case MainManager.Commands.Scorecheck:
					{
						MainManager.FadeIn();
						yield return EventControl.sec;
						MainManager.map.RemoveLimit(true);
						MainManager.SetCameraInstant(new Vector3(0f, 70f));
						bool a = MainManager.MainCamera.GetComponent<FXAA>().enabled;
						MainManager.MainCamera.GetComponent<FXAA>().enabled = true;
						MainManager.SetRenderTexture(2);
						SpriteRenderer t2 = MainManager.NewSolidColor("score", Color.Lerp(Color.white, Color.black, 0.75f), 0.01f, new Vector3(0f, 70f, 3f), new Vector2(0.5f, 0.5f));
						MainManager.instance.StartCoroutine(MainManager.SetText(FlappyBee.args + MainManager.menutext[237], 1, null, false, true, new Vector3(0f, 4.8f), Vector3.one, Vector3.one * 2f, t2.transform, null));
						Transform tt = new GameObject().transform;
						tt.parent = t2.transform;
						MainManager.FadeOut();
						yield return EventControl.sec;
						float y = 2f;
						for (int redirect = 0; redirect < 2; redirect = num2 + 1)
						{
							MainManager.instance.StartCoroutine(MainManager.SetText(FlappyBee.args + "|center|" + MainManager.menutext[210 + redirect], 1, null, false, true, new Vector3(-7f, y, -3f), Vector3.one, new Vector2(1.5f, 2f), t2.transform, null));
							yield return EventControl.halfsec;
							MainManager.instance.StartCoroutine(MainManager.SetText(FlappyBee.args + "|center|" + MainManager.instance.flagvar[28 + redirect].ToString().PadLeft(5, '0'), 1, null, false, true, new Vector3(4f, y, -3f), Vector3.one, Vector3.one * 2f, t2.transform, null));
							yield return EventControl.halfsec;
							y -= 1.65f;
							num2 = redirect;
						}
						MainManager.instance.StartCoroutine(MainManager.SetText(FlappyBee.args + MainManager.menutext[238], 1, null, false, true, new Vector3(0f, 0f, -3f), Vector3.one, new Vector2(1.5f, 2f), tt.transform, null));
						yield return EventControl.halfsec;
						while (!MainManager.GetKey(4) && !MainManager.GetKey(5))
						{
							tt.transform.localPosition = new Vector3(0f, (float)((Mathf.Sin(Time.time * 10f) > 0f) ? -2 : 999));
							yield return null;
						}
						tt.transform.position = new Vector3(0f, 999f);
						MainManager.FadeIn();
						yield return EventControl.sec;
						MainManager.SetRenderTexture(0);
						MainManager.MainCamera.GetComponent<FXAA>().enabled = a;
						Object.Destroy(t2.gameObject);
						MainManager.map.RestoreLimit(true);
						MainManager.ResetCamera(true);
						yield return null;
						MainManager.FadeOut();
						yield return EventControl.sec;
						t2 = null;
						tt = null;
						goto IL_A206;
					}
					case MainManager.Commands.Fademusic:
						if (temp.Length == 1)
						{
							MainManager.FadeMusic(0.025f);
							goto IL_A206;
						}
						MainManager.FadeMusic(Convert.ToSingle(temp[1]));
						goto IL_A206;
					case MainManager.Commands.Limit:
						if (temp[1] == "null")
						{
							linebreak = null;
							goto IL_A206;
						}
						linebreak = new float?(Convert.ToSingle(temp[1]));
						goto IL_A206;
					case MainManager.Commands.Termacadecheck:
						if (MainManager.instance.flagvar[28] > 9500 && MainManager.instance.flagvar[29] > 4500)
						{
							temp = new string[]
							{
								"",
								temp[2]
							};
							goto IL_63B6;
						}
						if (MainManager.instance.flagvar[28] > 9500 || MainManager.instance.flagvar[29] > 4500)
						{
							temp = new string[]
							{
								"",
								temp[1]
							};
							goto IL_63B6;
						}
						goto IL_A206;
					case MainManager.Commands.Removeitemat:
						if (!MainManager.MultiItem())
						{
							MainManager.instance.items[MainManager.IntFromString(temp[1])].RemoveAt((temp.Length == 2 || temp[2] == "opt") ? MainManager.listoption : MainManager.IntFromString(temp[2]));
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Unpausesize:
						if (!(MainManager.pausemenu == null) || !(MainManager.instance.questboardobj == null))
						{
							goto IL_A206;
						}
						break;
					case MainManager.Commands.Alwaysactive:
						MainManager.GetEntity(temp[1], caller.entity).alwaysactive = Convert.ToBoolean(temp[2]);
						goto IL_A206;
					case MainManager.Commands.Lockbacktrack:
						MainManager.notextbacktrack = true;
						goto IL_A206;
					case MainManager.Commands.Fixchompy:
						if (MainManager.map.chompy != null)
						{
							MainManager.map.chompy.following = MainManager.instance.playerdata[MainManager.instance.playerdata.Length - 1].entity;
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Updateanim:
						MainManager.GetEntity(temp[1], caller.entity).UpdateAnimSpecific();
						goto IL_A206;
					case MainManager.Commands.Checkallquests:
						if (MainManager.instance.boardquests[2].Count >= 60)
						{
							text = MainManager.OrganizeLines("|blank|" + MainManager.GetDialogueText(Convert.ToInt32(temp[1])), linebreak.Value, size.x, fonttype);
							i = -1;
							MainManager.instance.skiptext = false;
							skipi = true;
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Takeopenquests:
					{
						int[] array14 = MainManager.instance.boardquests[0].ToArray();
						List<int> list3 = new List<int>();
						if (array14.Length <= 1 && array14[0] == 0)
						{
							goto IL_63B6;
						}
						for (int num70 = 0; num70 < array14.Length; num70++)
						{
							if (array14[num70] != 23 && array14[num70] != 21 && array14[num70] != 9 && array14[num70] != 10 && array14[num70] != 8)
							{
								int num71 = Convert.ToInt32(MainManager.boardquestdata[array14[num70], 3]);
								if (num71 > -1)
								{
									MainManager.instance.flags[num71] = true;
								}
							}
							else
							{
								list3.Add(array14[num70]);
							}
						}
						if (array14.Length - list3.Count != 0)
						{
							List<int> list4 = new List<int>(array14);
							for (int num72 = list3.Count - 1; num72 >= 0; num72--)
							{
								list4.Remove(list3[num72]);
							}
							MainManager.instance.boardquests[1].AddRange(list4.ToArray());
							MainManager.instance.boardquests[1].Remove(0);
							MainManager.instance.boardquests[0] = new List<int>((list3.Count == 0) ? new int[1] : list3.ToArray());
							goto IL_A206;
						}
						goto IL_63B6;
					}
					case MainManager.Commands.Deathsmoke:
						if (temp.Length == 1)
						{
							MainManager.DeathSmoke(parent.transform.position);
							goto IL_A206;
						}
						if (temp.Length == 2)
						{
							MainManager.DeathSmoke(MainManager.GetEntity(temp[1], caller.entity).transform.position);
							goto IL_A206;
						}
						if (temp.Length == 3)
						{
							MainManager.DeathSmoke(MainManager.GetEntity(temp[1], caller.entity).transform.position, Vector3.one * Convert.ToSingle(temp[2]));
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Emoticon:
						MainManager.GetEntity(caller, temp[1]).Emoticon(Convert.ToInt32(temp[2]), Convert.ToInt32(temp[3]));
						goto IL_A206;
					case MainManager.Commands.Checksum:
						if (MainManager.GetValueFromString(temp[1]) + MainManager.GetValueFromString(temp[2]) > MainManager.GetValueFromString(temp[3]))
						{
							temp = new string[]
							{
								"",
								temp[4]
							};
							goto IL_63B6;
						}
						goto IL_A206;
					case MainManager.Commands.Questsize:
						if (!(MainManager.instance.questboardobj != null))
						{
							goto IL_A206;
						}
						break;
					case MainManager.Commands.Questbreak:
						if (MainManager.instance.questboardobj != null == (temp.Length == 1 && Convert.ToBoolean(temp[1])))
						{
							i = text.Length;
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Mapsize:
						if (!(MainManager.pausemenu != null) || MainManager.pausemenu.windowid != 6)
						{
							goto IL_A206;
						}
						break;
					case MainManager.Commands.Caravanmedal:
						if (MainManager.PrizeBadges(true).Length > 1)
						{
							temp = new string[]
							{
								"",
								temp[1],
								"keep"
							};
							goto IL_63B6;
						}
						goto IL_A206;
					case MainManager.Commands.Itemvalue:
						if (temp.Length >= 3 && temp[2] == "1")
						{
							MainManager.instance.flagvar[(temp.Length > 2) ? Convert.ToInt32(temp[2]) : 0] = (MainManager.instance.flags[681] ? 35 : Convert.ToInt32(MainManager.badgedata[MainManager.IntFromString(temp[1]), 5]));
							goto IL_A206;
						}
						MainManager.instance.flagvar[(temp.Length > 2) ? Convert.ToInt32(temp[2]) : 0] = Convert.ToInt32(MainManager.itemdata[0, MainManager.IntFromString(temp[1]), 4]);
						goto IL_A206;
					case MainManager.Commands.Sizemulti:
						goto IL_8FD0;
					case MainManager.Commands.Ignorenext:
						ignorenext = Convert.ToInt32(temp[1]);
						goto IL_A206;
					case MainManager.Commands.Rerollshops:
						if (caller != null)
						{
							caller.SetBadgeShop(true);
							goto IL_A206;
						}
						goto IL_A206;
					case MainManager.Commands.Maxmedals:
						text = text.Replace("|" + command + "|", string.Concat(120));
						num2 = i;
						i = num2 - 1;
						skipi = true;
						goto IL_A206;
					case MainManager.Commands.Battlesize:
						if (!(MainManager.battle != null))
						{
							goto IL_A206;
						}
						if (temp[1] == "multi")
						{
							temp = new string[]
							{
								"",
								temp[2],
								temp[3]
							};
							goto IL_8FD0;
						}
						break;
					case MainManager.Commands.Pausesize:
						if (!(MainManager.pausemenu != null) || !(MainManager.instance.questboardobj == null))
						{
							goto IL_A206;
						}
						break;
					case MainManager.Commands.GetFromMap:
						text = MainManager.OrganizeLines("|blank|" + MainManager.GetDialogueFromMap((MainManager.Maps)Convert.ToInt32(temp[1]), Convert.ToInt32(temp[2]), -1), linebreak.Value, size.x, fonttype);
						MainManager.GlobalCommand(ref text);
						i = -1;
						skipi = true;
						goto IL_A206;
					case MainManager.Commands.Listsize:
						if (!MainManager.instance.inlist || !(MainManager.pausemenu == null) || !(MainManager.battle == null))
						{
							goto IL_A206;
						}
						if (temp[1] == "multi")
						{
							temp = new string[]
							{
								"",
								temp[2],
								temp[3]
							};
							goto IL_8FD0;
						}
						break;
					case MainManager.Commands.Plural:
						if (MainManager.GetValueFromString(temp[1]) == 1)
						{
							text = text.Replace("|" + command + "|", temp[2]);
						}
						else
						{
							text = text.Replace("|" + command + "|", temp[3]);
						}
						num2 = i;
						i = num2 - 1;
						skipi = true;
						goto IL_A206;
					case MainManager.Commands.Layer:
						layer = Convert.ToInt32(temp[1]);
						goto IL_A206;
					default:
						goto IL_A206;
					}
					IL_9030:
					while (locksize && (temp.Length <= 3 || !(temp[3] == "force")))
					{
						if (temp.Length <= 3 || !(temp[3] == "unlock"))
						{
							goto IL_A206;
						}
						locksize = false;
					}
					if (temp[1] == "unpause" && MainManager.pausemenu == null && MainManager.instance.inlist)
					{
						size = new Vector2((temp[2] == "x") ? size.x : Convert.ToSingle(temp[2]), (temp[3] == "y") ? size.y : Convert.ToSingle(temp[(temp.Length == 3) ? 2 : 3]));
					}
					else if (temp[1] != "unpause")
					{
						if (temp.Length > 2)
						{
							size = new Vector2((temp[1] == "x") ? size.y : Convert.ToSingle(temp[1]), (temp[2] == "y") ? size.y : Convert.ToSingle(temp[2]));
						}
						else
						{
							size = new Vector2(Convert.ToSingle(temp[1]), Convert.ToSingle(temp[1]));
						}
						if (temp.Length > 3 && temp[3] == "lock")
						{
							locksize = true;
						}
					}
					if (ndd != null)
					{
						ndd.transform.localScale = new Vector3(size.x * langOffset, size.y, 1f) * 0.07f;
					}
					if (!MainManager.backtracking && dialogue)
					{
						MainManager.tempdiag = MainManager.tempdiag + "|" + command + "|";
					}
					bleepvolume = size.magnitude;
					goto IL_A206;
					IL_1CF2:
					if (!(temp[1] == "this"))
					{
						MainManager.instance.extrafollowers.Add((temp[1] == "var") ? MainManager.instance.flagvar[Convert.ToInt32(temp[2])] : Convert.ToInt32(temp[1]));
						goto IL_A206;
					}
					if (caller != null)
					{
						MainManager.AddFollower(caller.entity, -1);
					}
					if (temp.Length > 2 && temp[2] == "true")
					{
						textbox.GetComponent<DialogueAnim>().shrink = true;
						EventControl.temproutine = MainManager.instance.StartCoroutine(EventControl.PartyMover(false));
						while (EventControl.temproutine != null)
						{
							yield return null;
						}
						textbox.GetComponent<DialogueAnim>().shrink = false;
						goto IL_A206;
					}
					goto IL_A206;
					IL_398E:
					float num73 = 1f;
					if (temp.Length > 1)
					{
						num73 = Convert.ToSingle(temp[1]);
					}
					currentoffset = 0f;
					currentline -= 0.7f * size.y * num73;
					if (single)
					{
						ndd = null;
						ds = null;
						goto IL_A206;
					}
					if (dialogue && !MainManager.backtracking)
					{
						MainManager.tempdiag = MainManager.tempdiag + "|" + command + "|";
						goto IL_A206;
					}
					goto IL_A206;
					IL_3CCD:
					colorindex = 0;
					TextMesh[] componentsInChildren = textholder.GetComponentsInChildren<TextMesh>();
					for (int num74 = 0; num74 < componentsInChildren.Length; num74++)
					{
						MainManager.DisableLetter(componentsInChildren[num74]);
					}
					currentline = 0f;
					currentoffset = 0f;
					writen = 0;
					maxlenght = 0f;
					MainManager.ResetDiag();
					GameObject[] array15 = buts.ToArray();
					for (int num75 = 0; num75 < array15.Length; num75++)
					{
						Object.Destroy(array15[num75]);
					}
					buts = new List<GameObject>();
					yield return null;
					if (temp.Length > 1)
					{
						goto IL_5868;
					}
					goto IL_A206;
					IL_3E4A:
					end = true;
					if (caller != null && caller.objecttype == NPCControl.ObjectTypes.Item)
					{
						caller.StartCoroutine(caller.entity.Death());
						goto IL_A206;
					}
					goto IL_A206;
					IL_417D:
					if (MainManager.pausemenu == null)
					{
						int animstate;
						if (char.IsNumber(temp[2][0]))
						{
							animstate = Convert.ToInt32(temp[2]);
						}
						else
						{
							animstate = (int)Enum.Parse(typeof(MainManager.Animations), temp[2]);
						}
						EntityControl[] array16 = null;
						if (temp[1] == "this")
						{
							array16 = new EntityControl[]
							{
								MainManager.instance.tailtarget.GetComponent<EntityControl>()
							};
						}
						else if (temp[1] == "parent")
						{
							parent.GetComponent<EntityControl>();
						}
						else if (temp[1] == "party")
						{
							array16 = MainManager.GetPartyEntities();
						}
						else if (caller != null)
						{
							array16 = new EntityControl[]
							{
								MainManager.GetEntity(temp[1], caller.entity)
							};
						}
						else
						{
							array16 = new EntityControl[]
							{
								MainManager.GetEntity(temp[1])
							};
						}
						if (array16 != null)
						{
							for (int num76 = 0; num76 < array16.Length; num76++)
							{
								array16[num76].changedstate = true;
								array16[num76].animstate = animstate;
							}
						}
						yield return null;
						goto IL_A206;
					}
					goto IL_A206;
					IL_5868:
					if (!testing)
					{
						MainManager.ResetDiag(false);
						Transform y3 = MainManager.instance.tailtarget;
						if (MainManager.instance.tailtarget != null)
						{
							MainManager.instance.tailtarget.GetComponent<EntityControl>().talking = false;
						}
						bool a = temp.Length > 2 && temp[2] == "instant";
						EntityControl temptail = null;
						if (temp[1] == "caller" && caller != null)
						{
							temptail = caller.entity;
						}
						else if (temp[1] == "parent" && parent != null)
						{
							temptail = parent.GetComponent<EntityControl>();
						}
						else if (temp[1] != "null")
						{
							if (com == MainManager.Commands.Gettail)
							{
								int num77 = Convert.ToInt32(temp[1]);
								if (num77 <= 2 && MainManager.HasPlayer(num77))
								{
									temptail = MainManager.GetEntity(-(4 + num77));
								}
								else
								{
									EntityControl[] array17 = Object.FindObjectsOfType<EntityControl>();
									for (int num78 = 0; num78 < array17.Length; num78++)
									{
										if (array17[num78].originalid == num77)
										{
											temptail = array17[num78];
											break;
										}
									}
								}
							}
							else if (com == MainManager.Commands.Tailextra && MainManager.map != null)
							{
								temptail = MainManager.map.tempfollowers[Convert.ToInt32(temp[1])];
							}
							else if (caller != null)
							{
								temptail = MainManager.GetEntity(temp[1], caller.entity);
							}
							else
							{
								temptail = MainManager.GetEntity(temp[1]);
							}
						}
						else
						{
							MainManager.instance.tailtarget = null;
						}
						if (!a && (temptail == null || temptail.transform != y3))
						{
							textbox.GetComponent<DialogueAnim>().shrink = true;
							yield return new WaitForSeconds(0.035f);
						}
						if (temptail != null)
						{
							MainManager.instance.tailtarget = temptail.transform;
						}
						yield return null;
						if (temptail != null)
						{
							bleep = Resources.Load<AudioClip>("Audio/Sounds/Dialogue/Dialogue" + temptail.dialoguebleepid);
							bleeppitch = temptail.bleeppitch;
						}
						else
						{
							bleep = null;
						}
						yield return null;
						if (!a)
						{
							textbox.GetComponent<DialogueAnim>().shrink = false;
							if (temp.Length == 3)
							{
								goto IL_417D;
							}
						}
						temptail = null;
						goto IL_A206;
					}
					goto IL_A206;
					IL_63B6:
					if (temp[1] == "random")
					{
						text = MainManager.OrganizeLines("|blank|" + MainManager.GetDialogueText(Convert.ToInt32(temp[Random.Range(2, temp.Length)])), linebreak.Value, size.x, fonttype);
						if (MainManager.currentdialogue == MainManager.diagstring.Count && !MainManager.backtracking)
						{
							MainManager.currentdialogue++;
							MainManager.diagstring.Add(MainManager.OrganizeLines(MainManager.tempdiag, linebreak.Value, size.x, fonttype));
							MainManager.tempdiag = string.Concat(new object[]
							{
								"|size,",
								size.x,
								",",
								size.y,
								"|"
							});
						}
					}
					else
					{
						string text7 = "";
						if (temp.Length > 2)
						{
							for (int num79 = 2; num79 < temp.Length; num79++)
							{
								text7 = text7 + "|" + temp[num79] + "|";
							}
						}
						string str3 = (text7 == "|keep|") ? "" : "|blank|";
						text = str3 + MainManager.OrganizeLines(MainManager.GetDialogueText(Convert.ToInt32(temp[1])) + text7, linebreak.Value, size.x, fonttype);
						if (MainManager.currentdialogue == MainManager.diagstring.Count && !MainManager.backtracking)
						{
							MainManager.currentdialogue++;
							MainManager.diagstring.Add(MainManager.OrganizeLines(MainManager.tempdiag, linebreak.Value, size.x, fonttype));
							MainManager.tempdiag = string.Concat(new object[]
							{
								"|size,",
								size.x,
								",",
								size.y,
								"|"
							});
						}
					}
					MainManager.GlobalCommand(ref text);
					i = -1;
					skipi = true;
					goto IL_A206;
					IL_66C8:
					commands = com;
					string text8;
					if (commands != MainManager.Commands.Menu)
					{
						if (commands == MainManager.Commands.Call)
						{
							text8 = MainManager.GetDialogueText(Convert.ToInt32(temp[1]));
						}
						else
						{
							text8 = MainManager.instance.flagstring[Convert.ToInt32(temp[1])];
						}
					}
					else
					{
						text8 = MainManager.menutext[Convert.ToInt32(temp[1])];
					}
					text8 = text8.Replace("\r\n", "\n");
					if (temp.Length > 2 && temp[2] == "clamp" && MainManager.GetTextLenght(text8, fonttype) > Convert.ToSingle(temp[3]))
					{
						text8 = string.Concat(new object[]
						{
							"|sizemulti,",
							(temp.Length > 4) ? string.Concat(Convert.ToSingle(temp[4])) : "0.5",
							",1|",
							text8,
							"|size,",
							size.x,
							",",
							size.y,
							"|"
						});
					}
					text = text.Replace("|" + command + "|", text8);
					if (temp.Length > 2 && temp[2] == "true")
					{
						text = MainManager.OrganizeLines(text, linebreak.Value, size.x, fonttype);
					}
					num2 = i;
					i = num2 - 1;
					skipi = true;
					goto IL_A206;
					IL_8FD0:
					temp = new string[]
					{
						"",
						string.Concat(size.x * Convert.ToSingle(temp[1])),
						string.Concat(size.y * Convert.ToSingle(temp[2]))
					};
					goto IL_9030;
				}
				IL_A206:
				if (ignorenext > 0)
				{
					num2 = ignorenext;
					ignorenext = num2 - 1;
				}
				if (!skipi)
				{
					i += command.Length + 1;
				}
				skipi = false;
				command = null;
				temp = null;
			}
			else if (text[i] == ' ')
			{
				num2 = writen;
				writen = num2 + 1;
				MainManager.tempdiag += " ";
				currentoffset += 0.3f * size.x;
				if (dialogue)
				{
					MainManager.SetTalk(dialogue, true);
				}
				if (ndd != null)
				{
					TextMesh textMesh = ndd;
					textMesh.text += " ";
				}
			}
			else if (text[i] != '\r')
			{
				if (dialogue && speed == 0f)
				{
					MainManager.instance.inputcooldown = 5f;
				}
				int redirect = fonttype;
				if (!fontlock)
				{
					if ((MainManager.languageid == 3 && dialogue && !MainManager.instance.numberprompt) || Regex.IsMatch(text[i].ToString() ?? "", "\\p{IsHiragana}|\\p{IsKatakana}|\\p{IsKangxiRadicals}|\\p{IsCJKUnifiedIdeographs}"))
					{
						redirect = 3;
						fonttype = 3;
					}
					else if ((MainManager.languageid == 6 && dialogue && !MainManager.instance.numberprompt) || Regex.IsMatch(text[i].ToString() ?? "", "\\p{IsCyrillic}"))
					{
						redirect = 4;
						fonttype = 4;
					}
					else if ((MainManager.languageid == 5 && dialogue && !MainManager.instance.numberprompt) || Regex.IsMatch(text[i].ToString() ?? "", "\\p{IsHangulJamo}|\\p{IsHangulSyllables}|\\p{IsHangulCompatibilityJamo}"))
					{
						redirect = 5;
						fonttype = 5;
					}
				}
				if (!single)
				{
					num2 = writen;
					writen = num2 + 1;
					MainManager.SetTalk(dialogue, true);
					if (dialogue && i < text.Length - 1)
					{
						MainManager.PlayBleep(bleep, bleeppitch, bleepvolume, i);
						MainManager.tempdiag += text[i].ToString();
					}
					TextMesh emptyLetter = MainManager.GetEmptyLetter();
					if (emptyLetter != null)
					{
						if (ui3d)
						{
							emptyLetter.gameObject.layer = 15;
						}
						else if (!tridimensional)
						{
							emptyLetter.gameObject.layer = layer;
						}
						else
						{
							emptyLetter.gameObject.layer = 0;
						}
						emptyLetter.tag = "Letter";
						MainManager.SetLetter(ref emptyLetter, redirect, text[i].ToString() ?? "", textholder.transform, new Vector2(currentoffset, currentline - 0.1f) + MainManager.letteroffset, MainManager.instance.textcolors[colorindex], sort, new Vector3(size.x, size.y, 1f) * 0.07f);
						if (dropshadow != null)
						{
							MeshRenderer component4 = emptyLetter.GetComponent<MeshRenderer>();
							ds = MainManager.GetEmptyLetter();
							ds.gameObject.layer = emptyLetter.gameObject.layer;
							MainManager.SetLetter(ref ds, redirect, emptyLetter.text, textholder.transform, emptyLetter.transform.localPosition + dropshadow.Value, fadeletter ? Color.clear : new Color(0f, 0f, 0f, 0.5f), component4.sortingOrder, emptyLetter.transform.localScale);
							if (fadeletter)
							{
								MainManager.instance.StartCoroutine(MainManager.GradualColor(ds.GetComponent<MeshRenderer>(), new Color(0f, 0f, 0f, 0.5f), 200f));
							}
							MeshRenderer meshRenderer = component4;
							num2 = meshRenderer.sortingOrder;
							meshRenderer.sortingOrder = num2 + 1;
						}
						if (rainbow || wavy || glitchy || shaky || fadeletter)
						{
							FontEffects fontEffects3 = emptyLetter.gameObject.AddComponent<FontEffects>();
							fontEffects3.SetEffects(shaky, wavy, rainbow, glitchy, fadeletter, fonttype, i);
							fontEffects3.superglitch = superglitch;
						}
						currentoffset += MainManager.GetLetterOffset(text[i], redirect, size.x);
					}
					if (currentoffset > maxlenght)
					{
						maxlenght = currentoffset;
					}
					MainManager.textwidth = maxlenght;
					MainManager.lasttextcenter = new Vector3(position.x - maxlenght / 2f, position.y, position.z).x;
					if (centralize)
					{
						textholder.transform.localPosition = new Vector3(position.x - maxlenght / 2f, position.y, position.z);
					}
					if (minibubble || (dialogue && ((speed > 0f && !MainManager.instance.skiptext) || MainManager.noskip)))
					{
						yield return new WaitForSeconds(speed);
					}
					if (((dialogue && speed > 0f && !MainManager.instance.skiptext) || minibubble) && char.IsPunctuation(text[i]) && text[i] != '\'' && i + 1 < text.Length - 1 && text[i + 1] != '|' && text[i + 1] != ')' && text[i + 1] != '¿' && text[i + 1] != '¡' && text[i] != '\'' && text[i] != '/' && text[i] != '¿' && text[i] != ')' && text[i] != '¡' && (!char.IsPunctuation(text[i + 1]) || text[i + 1] != '.' || text[i + 1] != '!' || text[i + 1] != '?' || text[i + 1] != ')' || text[i + 1] != '？' || text[i + 1] != '、' || text[i + 1] != '。' || text[i + 1] != '！' || text[i + 1] != '¿' || text[i + 1] != '¡'))
					{
						yield return new WaitForSeconds(0.15f);
					}
				}
				else
				{
					if (ndd == null)
					{
						do
						{
							ndd = MainManager.GetEmptyLetter();
							if (ndd == null)
							{
								yield return null;
							}
						}
						while (ndd == null);
						MainManager.SetLetter(ref ndd, redirect, "", textholder.transform, Vector3.zero, MainManager.instance.textcolors[colorindex], sort, new Vector3(size.x * langOffset, size.y, 1f) * 0.07f);
						ndd.gameObject.layer = layer;
					}
					currentoffset += MainManager.GetLetterOffset(ndd, size.x * langOffset);
					TextMesh textMesh2 = ndd;
					textMesh2.text += text[i].ToString();
					ndd.transform.localPosition = new Vector3((!centralize) ? 0f : (-(MainManager.GetTextLenght(ndd.text, redirect) / 2f)), currentline) * langOffset + MainManager.letteroffset;
					if (dropshadow != null)
					{
						if (ds == null)
						{
							ds = MainManager.GetEmptyLetter();
							MainManager.SetLetter(ref ds, redirect, "", ndd.transform, dropshadow.Value, new Color(0f, 0f, 0f, 0.5f), sort - 1, Vector3.one);
						}
						ds.text = ndd.text;
						ds.gameObject.layer = ndd.gameObject.layer;
					}
				}
				if (backbox != null)
				{
					backbox.transform.localPosition = new Vector3(currentoffset / 2f, size.y / 2f + 0.1f, -5f);
					backbox.transform.localScale = new Vector3(currentoffset / 5f * size.x, size.y * 1.5f, 1f);
				}
			}
			if (dialogue)
			{
				MainManager.SetTalk(dialogue, false);
				while (MainManager.instance.prompt || MainManager.instance.itemlist != null)
				{
					yield return null;
				}
				if (MainManager.instance.promptpick > -1 && MainManager.instance.promptpointers.Length != 0)
				{
					string str4 = "";
					if (questboardpromp)
					{
						if (MainManager.instance.promptpick == 0)
						{
							str4 = "|activateselectedquest||break|" + ((!MainManager.instance.flags[64]) ? ("|tail,null||blank||boxstyle,4|" + MainManager.menutext[170] + "|flag,64,true||break|") : "") + "|loadcamera||end|";
						}
						else
						{
							str4 = "|break||loadcamera||end|";
						}
					}
					if (MainManager.instance.flagvar[0] == -555)
					{
						textbox.GetComponent<DialogueAnim>().shrink = false;
					}
					text = "|blank|" + MainManager.GetText(promptmenu, MainManager.instance.promptpointers[MainManager.instance.promptpick]);
					if (linebreak != null)
					{
						text = MainManager.OrganizeLines(text, linebreak.Value, size.x, fonttype) + str4;
					}
					DialogueAnim dialogueAnim2 = MainManager.instance.promptbox.GetComponent<DialogueAnim>();
					if (dialogueAnim2 == null)
					{
						dialogueAnim2 = MainManager.instance.promptbox.gameObject.AddComponent<DialogueAnim>();
					}
					dialogueAnim2.shrink = true;
					Object.Destroy(MainManager.instance.promptbox.gameObject, 0.5f);
					MainManager.instance.promptpick = -1;
					i = -1;
					yield return null;
				}
				if (MainManager.instance.inlist)
				{
					if (MainManager.listredirect != null && (MainManager.listredirect.Value == -2 || MainManager.listredirect.Value == -3))
					{
						caller.entity.ccol.enabled = true;
						caller.entity.rigid.useGravity = true;
						caller.entity.rigid.constraints = RigidbodyConstraints.FreezeRotation;
						caller.transform.parent = MainManager.map.transform;
						caller.bounces = 0;
						caller.StartCoroutine(caller.entity.LateVelocity(MainManager.RandomItemBounce(6f, 12f)));
						Object.Destroy(caller.entity.sprite.transform.GetChild(0).gameObject);
						caller.entity.Unfix(true);
						caller.touchcooldown = 70f;
						caller.tossed = true;
						caller.timer = 600f;
						if (MainManager.listredirect.Value == -2)
						{
							caller.entity.animstate = MainManager.instance.flagvar[1];
							caller.entity.itemstate = MainManager.instance.flagvar[1];
							caller.entity.basestate = MainManager.instance.flagvar[1];
							MainManager.instance.items[MainManager.listtype].Remove(MainManager.instance.flagvar[1]);
							MainManager.instance.items[MainManager.listtype].Add(MainManager.instance.flagvar[0]);
						}
						end = true;
						MainManager.listredirect = null;
						MainManager.player.actioncooldown = 30f;
						if (MainManager.eventtoss > -1)
						{
							eventcall = MainManager.eventtoss;
						}
					}
					else if (MainManager.listredirect != null && MainManager.listredirect.Value != -1)
					{
						text = MainManager.OrganizeLines("|blank|" + MainManager.GetDialogueText(MainManager.listredirect.Value), linebreak.Value, size.x, fonttype);
						i = -1;
					}
				}
				MainManager.SetTalk(dialogue, true);
				MainManager.instance.inlist = false;
				if (MainManager.player != null)
				{
					MainManager.player.npc = new List<NPCControl>();
				}
			}
			num2 = i;
		}
		if (dialogue)
		{
			if (transfer != null && caller != null)
			{
				caller.interactcd = 15f;
			}
			MainManager.SetTalk(dialogue, false);
			MainManager.instance.waitinput = true;
			MainManager.instance.skiptext = false;
			while (MainManager.instance.waitinput && !end)
			{
				yield return null;
			}
			if (MainManager.bleeps != null)
			{
				MainManager.bleeps.loop = false;
				MainManager.instance.StartCoroutine(MainManager.FadeSound(MainManager.bleeps, 0.1f));
			}
			TextMesh[] componentsInChildren2 = textholder.GetComponentsInChildren<TextMesh>();
			for (int num80 = 0; num80 < componentsInChildren2.Length; num80++)
			{
				MainManager.DisableLetter(componentsInChildren2[num80]);
			}
			Object.Destroy(textholder);
			if (!tempevent && !MainManager.instance.inevent)
			{
				MainManager.instance.minipause = false;
			}
			MainManager.instance.message = false;
			if (cameraoffset.magnitude > 0.1f)
			{
				MainManager.instance.camoffset = camtoffset;
			}
			MainManager.instance.overridefollower = tempoverf;
			if (textbox != null)
			{
				textbox.GetComponent<DialogueAnim>().shrink = true;
				Object.Destroy(textbox, 1f);
			}
			if (MainManager.player != null && !MainManager.instance.inevent)
			{
				MainManager.player.entity.rigid.constraints = tcons;
				MainManager.player.actioncooldown = 20f;
				EntityControl[] array18 = returnentitycol.ToArray();
				for (int num81 = 0; num81 < array18.Length; num81++)
				{
					Physics.IgnoreCollision(array18[num81].ccol, MainManager.player.entity.ccol, false);
					if (array18[num81].npcdata != null)
					{
						if (array18[num81].npcdata.pusher != null)
						{
							Physics.IgnoreCollision(array18[num81].npcdata.pusher, MainManager.player.entity.ccol, false);
						}
						if (array18[num81].npcdata.scol != null)
						{
							Physics.IgnoreCollision(array18[num81].npcdata.scol, MainManager.player.entity.ccol, false);
						}
						if (array18[num81].npcdata.boxcol != null)
						{
							Physics.IgnoreCollision(array18[num81].npcdata.boxcol, MainManager.player.entity.ccol, false);
						}
					}
				}
			}
			MainManager.instance.waitinput = false;
			if (!MainManager.instance.inevent && MainManager.battle == null)
			{
				MainManager.EndOfMessage();
			}
		}
		else if (minibubble && parent.GetComponent<MiniBubble>() != null)
		{
			parent.GetComponent<MiniBubble>().DestroyThis();
		}
		yield return null;
		if (transfer != null)
		{
			MainManager.instance.StartCoroutine(MainManager.TransferMap(transferi, MainManager.player.transform.position, transfer.Value, transfer.Value));
		}
		if (eventcall > -1)
		{
			MainManager.events.StartEvent(MainManager.eventtoss, caller);
			MainManager.eventtoss = -1;
		}
		if (tokenbox != null)
		{
			Object.Destroy(tokenbox.gameObject);
		}
		if (MainManager.instance.items != null)
		{
			MainManager.instance.items[1].Remove(110);
		}
		if (caller != null)
		{
			NPCControl.NPCType entitytype = caller.entitytype;
			if (entitytype == NPCControl.NPCType.Object && caller.objecttype == NPCControl.ObjectTypes.Item && caller.hit)
			{
				Object.Destroy(caller.gameObject);
			}
		}
		if (!MainManager.instance.inlist && !MainManager.instance.prompt)
		{
			MainManager.instance.flags[349] = false;
		}
		yield break;
	}

	// Token: 0x0600059D RID: 1437 RVA: 0x0003A028 File Offset: 0x00038228
	private static int IntFromString(string input)
	{
		if (input.Contains("var"))
		{
			return MainManager.instance.flagvar[Convert.ToInt32(input.Replace("var", ""))];
		}
		if (input.Contains("v"))
		{
			return MainManager.instance.flagvar[Convert.ToInt32(input.Replace("v", ""))];
		}
		return Convert.ToInt32(input);
	}

	// Token: 0x0600059E RID: 1438 RVA: 0x0003A098 File Offset: 0x00038298
	public static void EndOfMessage()
	{
		if (MainManager.instance.flags[347])
		{
			for (int i = 0; i < 2; i++)
			{
				if (MainManager.instance.badgeshops[i].Count == 0)
				{
					MainManager.instance.flags[587 + i] = true;
				}
			}
		}
		if (MainManager.instance.flags[470] && !MainManager.instance.librarystuff[0, 41])
		{
			MainManager.instance.librarystuff[0, 41] = true;
		}
		if (MainManager.instance.flags[351] && !MainManager.instance.librarystuff[0, 42])
		{
			MainManager.instance.librarystuff[0, 42] = true;
		}
		if (MainManager.instance.flags[605] && !MainManager.instance.boardquests[2].Contains(27))
		{
			MainManager.CompleteQuest(27);
		}
		if (MainManager.map != null)
		{
			MainManager.map.currentline = -1;
		}
		MainManager.instance.CheckAchievement();
	}

	// Token: 0x0600059F RID: 1439 RVA: 0x0003A1AC File Offset: 0x000383AC
	public static bool AllActive(EntityControl[] e)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i].npcdata != null && !e[i].npcdata.hit)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x060005A0 RID: 1440 RVA: 0x0003A1EC File Offset: 0x000383EC
	public static void SetLetter(ref TextMesh letter, int fontid, string text, Transform parent, Vector3 pos, Color color, int sortorder, Vector3 size)
	{
		MainManager.SetFont(letter, fontid);
		if (parent != null)
		{
			letter.transform.parent = parent;
		}
		letter.transform.localPosition = pos;
		letter.text = text;
		letter.anchor = TextAnchor.LowerLeft;
		letter.color = color;
		letter.transform.localEulerAngles = Vector3.zero;
		letter.transform.localScale = size;
		MeshRenderer component = letter.GetComponent<MeshRenderer>();
		component.material.color = color;
		component.sortingOrder = sortorder;
	}

	// Token: 0x060005A1 RID: 1441 RVA: 0x0003A278 File Offset: 0x00038478
	public static Vector3 GetMinibubblePos(Transform target, float y)
	{
		return MainManager.GetMinibubblePos(MainManager.MainCamera.WorldToViewportPoint(target.position).x - 0.5f, y);
	}

	// Token: 0x060005A2 RID: 1442 RVA: 0x0003A29B File Offset: 0x0003849B
	public static Vector3 GetMinibubblePos(Transform target)
	{
		return MainManager.GetMinibubblePos(MainManager.MainCamera.WorldToViewportPoint(target.position).x - 0.5f, 0.85f);
	}

	// Token: 0x060005A3 RID: 1443 RVA: 0x0003A2C2 File Offset: 0x000384C2
	public static Vector3 GetMinibubblePos(float x, float y)
	{
		return new Vector3(Mathf.Clamp(x * 33f, -5f, 5f), y, 10f);
	}

	// Token: 0x060005A4 RID: 1444 RVA: 0x0003A2E8 File Offset: 0x000384E8
	public static void ResetEntitySpeed()
	{
		for (int i = 0; i < MainManager.map.entities.Length; i++)
		{
			if (MainManager.map.entities[i] != null && MainManager.map.entities[i].anim != null)
			{
				MainManager.map.entities[i].anim.speed = 1f;
			}
		}
		for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
		{
			MainManager.instance.playerdata[j].entity.anim.speed = 1f;
		}
	}

	// Token: 0x060005A5 RID: 1445 RVA: 0x0003A390 File Offset: 0x00038590
	public static void ResetEntitySpeed(bool all)
	{
		if (!all)
		{
			MainManager.ResetEntitySpeed();
			return;
		}
		EntityControl[] array = Object.FindObjectsOfType<EntityControl>();
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].iskill && array[i].originalid > -1 && array[i].anim != null)
			{
				array[i].anim.speed = 1f;
			}
		}
	}

	// Token: 0x060005A6 RID: 1446 RVA: 0x0003A3F4 File Offset: 0x000385F4
	public static EntityControl FindEntity(int id)
	{
		EntityControl[] array = Object.FindObjectsOfType<EntityControl>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].animid == id)
			{
				return array[i];
			}
		}
		return null;
	}

	// Token: 0x060005A7 RID: 1447 RVA: 0x0003A425 File Offset: 0x00038625
	public static void SortLights()
	{
		MainManager.SortLights(null);
	}

	// Token: 0x060005A8 RID: 1448 RVA: 0x0003A430 File Offset: 0x00038630
	public static int MultipleKeys(bool hold)
	{
		int num = 0;
		for (int i = 0; i < InputIO.keys.Length; i++)
		{
			if (MainManager.GetKey(i, hold))
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x060005A9 RID: 1449 RVA: 0x0003A460 File Offset: 0x00038660
	public static void RefreshHUDValues()
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			MainManager.instance.playerdata[i].hpt = MainManager.instance.playerdata[i].hp;
		}
		MainManager.instance.tpt = MainManager.instance.tp;
		MainManager.instance.moneyt = MainManager.instance.money;
	}

	// Token: 0x060005AA RID: 1450 RVA: 0x0003A4D8 File Offset: 0x000386D8
	public static void SortLights(MeshRenderer[] r)
	{
		if (r == null)
		{
			r = Object.FindObjectsOfType<MeshRenderer>();
		}
		for (int i = 0; i < r.Length; i++)
		{
			if (r[i] != null && r[i].material.shader == MainManager.fakelight)
			{
				r[i].material.renderQueue = 2500 + (int)(MainManager.MainCamera.WorldToViewportPoint(r[i].transform.position).z * (float)((MainManager.battle == null) ? 100 : 10));
			}
		}
	}

	// Token: 0x060005AB RID: 1451 RVA: 0x0003A568 File Offset: 0x00038768
	public static void FixEntities()
	{
		EntityControl[] array = Object.FindObjectsOfType<EntityControl>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].npcdata == null || ((array[i].npcdata.entitytype == NPCControl.NPCType.NPC || array[i].npcdata.entitytype == NPCControl.NPCType.Enemy) && !array[i].item))
			{
				array[i].overrideflip = false;
				array[i].overrideanim = false;
				array[i].rigid.useGravity = true;
				array[i].rigid.isKinematic = false;
				array[i].onground = true;
				array[i].rigid.velocity = Vector3.zero;
				if (array[i].anim != null)
				{
					array[i].anim.speed = 1f;
				}
			}
		}
	}

	// Token: 0x060005AC RID: 1452 RVA: 0x0003A633 File Offset: 0x00038833
	private static char GetKoreanChar(int[] ids)
	{
		return Convert.ToChar(ids[0] * 588 + ids[1] * 28 + ids[2] + 44032);
	}

	// Token: 0x060005AD RID: 1453 RVA: 0x0003A654 File Offset: 0x00038854
	private void UpdateKoreanPrompt(int type)
	{
		for (int i = 3; i <= this.promptbox.childCount - 4; i++)
		{
			Renderer componentInChildren = this.promptbox.GetChild(i).GetComponentInChildren<Renderer>();
			if (componentInChildren != null)
			{
				if (MainManager.koreanHL.Contains(i))
				{
					componentInChildren.material.color = Color.red;
				}
				else if (i >= MainManager.koreanLimit[type].x && i <= MainManager.koreanLimit[type].y)
				{
					componentInChildren.material.color = Color.black;
				}
				else
				{
					componentInChildren.material.color = new Color(0f, 0f, 0f, 0.5f);
				}
			}
		}
	}

	// Token: 0x060005AE RID: 1454 RVA: 0x0003A718 File Offset: 0x00038918
	public static void ChangeLayer(Transform obj, int layer)
	{
		Transform[] componentsInChildren = obj.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
	}

	// Token: 0x060005AF RID: 1455 RVA: 0x0003A748 File Offset: 0x00038948
	private void ChangeLetterPrompt(int specific = -1)
	{
		if (specific > -1)
		{
			this.letterprompt = specific;
		}
		else
		{
			int num = this.letterprompt;
			if (num - 101 > 1)
			{
				this.letterprompt++;
				if (this.letterprompt >= MainManager.letterPromptHelp.Length)
				{
					this.letterprompt = 0;
				}
			}
			else
			{
				this.letterprompt = 4;
			}
		}
		MainManager.CreateLetterPrompt(this.letterprompt, MainManager.instance.promptbox.GetComponentInChildren<SpriteRenderer>().color);
	}

	// Token: 0x060005B0 RID: 1456 RVA: 0x0003A7BC File Offset: 0x000389BC
	private static void CreateLetterPrompt(int id, Color color)
	{
		color = Color.white;
		if (id == -1)
		{
			switch (MainManager.languageid)
			{
			case 3:
				id = 1;
				goto IL_3F;
			case 5:
				id = 4;
				goto IL_3F;
			case 6:
				id = 3;
				goto IL_3F;
			}
			id = 0;
		}
		IL_3F:
		bool flag = MainManager.instance.promptbox != null;
		if (flag && MainManager.instance.promptbox != null)
		{
			for (int i = 0; i < MainManager.instance.promptbox.childCount; i++)
			{
				MainManager.DestroyText(MainManager.instance.promptbox.GetChild(i), false);
			}
			Object.Destroy(MainManager.instance.promptbox.gameObject);
		}
		MainManager.instance.inputcooldown = 10f;
		string[] array = MainManager.instance.flagstring[2].Split(new char[]
		{
			','
		});
		MainManager.instance.prompt = true;
		MainManager.instance.numberprompt = true;
		MainManager.instance.letterprompt = id;
		MainManager.instance.flagvar[0] = -555;
		MainManager.instance.flagvar[1] = 0;
		MainManager.instance.flagstring[0] = "";
		MainManager.instance.flagvar[10] = Convert.ToInt32(array[4]);
		MainManager.instance.flagstring[1] = Resources.Load<TextAsset>("Data/LetterPrompt" + id).ToString();
		MainManager.instance.maxoptions = 0;
		MainManager.instance.option = 0;
		MainManager.instance.promptbox = MainManager.Create9Box(new Vector3(0f, -1.25f, 10f), new Vector2(15f, 7.5f), 1, -3, color, !flag);
		if (flag)
		{
			MainManager.instance.promptbox.transform.localScale = Vector3.one;
		}
		Transform transform = MainManager.Create9Box(new Vector3(0f, 3.75f, 10f), new Vector2(13f, 2f), 1, -3, color, false);
		transform.transform.parent = MainManager.instance.promptbox.transform;
		transform.transform.localScale = Vector3.one;
		transform.transform.localPosition = new Vector3(0f, 5f);
		MainManager.instance.StartCoroutine(MainManager.SetText("|center|" + ((array[3][0] == '@') ? array[3] : MainManager.GetDialogueText(Convert.ToInt32(array[3]))), new Vector3(0f, 4.75f, 9f), MainManager.instance.promptbox));
		MainManager.listtype = Convert.ToInt32(array[1]);
		MainManager.listredirect = new int?(Convert.ToInt32(array[2]));
		MainManager.listcancel = MainManager.listredirect.Value;
		MainManager.instance.npromptholder = new GameObject("letter holder").transform;
		MainManager.instance.npromptholder.transform.parent = MainManager.instance.promptbox.transform;
		MainManager.instance.npromptholder.transform.localPosition = new Vector3(0f, 2.7f, 0.05f);
		MainManager.instance.npromptholder.transform.localEulerAngles = Vector3.zero;
		MainManager.instance.npromptholder.transform.localScale = Vector3.one;
		List<int> list = new List<int>();
		float num = -6.35f;
		float num2 = 1.8f;
		bool flag2 = false;
		int num3 = 0;
		for (int j = 0; j < MainManager.instance.flagstring[1].Length; j++)
		{
			if (MainManager.instance.flagstring[1][j] == '\n')
			{
				flag2 = true;
				num3++;
				num2 -= ((MainManager.instance.letterprompt == 4 && num3 < 3) ? 1.1f : 0.75f);
				num = -6.35f;
			}
			else if (MainManager.instance.flagstring[1][j] == '{')
			{
				if (!flag2)
				{
					MainManager.instance.flagvar[1]++;
				}
				num += 0.65f;
				list.Add(MainManager.instance.maxoptions - 1);
			}
			else
			{
				if (!flag2)
				{
					MainManager.instance.flagvar[1]++;
				}
				string str = string.Concat(new object[]
				{
					MainManager.instance.flagstring[1][j].ToString(),
					"|choicewave,",
					MainManager.instance.maxoptions,
					",true|"
				});
				MainManager.instance.maxoptions++;
				MainManager.instance.StartCoroutine(MainManager.SetText("|center|" + str, 0, null, false, false, new Vector3(num, num2, 0f), Vector3.zero, new Vector3(1f, 1f, 1f), MainManager.instance.promptbox, null));
				num += 0.65f;
			}
		}
		MainManager.instance.flagstring[1] = MainManager.instance.flagstring[1].Replace("\n", "").Replace("{", "");
		MainManager.instance.multilist = list.ToArray();
		MainManager.instance.maxoptions += 3;
		MainManager.instance.StartCoroutine(MainManager.SetText(string.Concat(new object[]
		{
			"|center|",
			MainManager.menutext[74],
			"|choicewave,",
			MainManager.instance.maxoptions - 3,
			"|"
		}), 0, null, false, false, new Vector3(-5f, -3f, 0f), Vector3.zero, new Vector3(1f, 1f, 1f), MainManager.instance.promptbox, null));
		MainManager.instance.StartCoroutine(MainManager.SetText(string.Concat(new object[]
		{
			"|center|",
			MainManager.menutext[192],
			"|choicewave,",
			MainManager.instance.maxoptions - 2,
			"|"
		}), 0, null, false, false, new Vector3(0f, -3f, 0f), Vector3.zero, new Vector3(1f, 1f, 1f), MainManager.instance.promptbox, null));
		MainManager.instance.StartCoroutine(MainManager.SetText(string.Concat(new object[]
		{
			"|center|",
			(MainManager.languageid == 6) ? "|sizemulti,0.8,1|" : "",
			MainManager.menutext[42],
			"|choicewave,",
			MainManager.instance.maxoptions - 1,
			"|"
		}), 0, null, false, false, new Vector3(5f, -3f, 0f), Vector3.zero, new Vector3(1f, 1f, 1f), MainManager.instance.promptbox, null));
		new GameObject("button").AddComponent<ButtonSprite>().SetUp(6, -1, MainManager.letterPromptHelp[id], new Vector3(-2.25f, -1.9f), Vector3.one * 0.5f, 10, MainManager.instance.promptbox);
		if (MainManager.instance.letterprompt == 4)
		{
			MainManager.koreanHL = new int[]
			{
				-1,
				-1
			};
			MainManager.instance.flagvar[6] = 0;
			MainManager.instance.UpdateKoreanPrompt(0);
		}
		MainManager.instance.RefreshNumberPrompt();
	}

	// Token: 0x060005B1 RID: 1457 RVA: 0x0003AFB4 File Offset: 0x000391B4
	public static TextMesh GetEmptyLetter()
	{
		for (int i = 0; i < MainManager.letterpool.Length; i++)
		{
			if (MainManager.letterpool[i] == null)
			{
				MainManager.letterpool[i] = MainManager.NewLetter(i.ToString());
				return MainManager.letterpool[i];
			}
			if (MainManager.letterpool[i].text == "")
			{
				return MainManager.letterpool[i];
			}
		}
		return null;
	}

	// Token: 0x060005B2 RID: 1458 RVA: 0x0003B020 File Offset: 0x00039220
	public static bool AddExp(int ammount)
	{
		MainManager.instance.partyexp += ammount;
		if (!MainManager.sounds[4].isPlaying)
		{
			MainManager.PlaySound("Exp2", 4, 1.5f + Random.Range(-0.1f, 0.1f), 1f);
		}
		else if (!MainManager.sounds[5].isPlaying)
		{
			MainManager.PlaySound("Exp2", 5, 1.5f + Random.Range(-0.1f, 0.1f), 1f);
		}
		else if (!MainManager.sounds[6].isPlaying)
		{
			MainManager.PlaySound("Exp2", 6, 1.5f + Random.Range(-0.1f, 0.1f), 1f);
		}
		else
		{
			MainManager.PlaySound("Exp2", 7, 1.5f + Random.Range(-0.1f, 0.1f), 1f);
		}
		if (MainManager.instance.partyexp >= MainManager.instance.neededexp)
		{
			MainManager.instance.partyexp -= MainManager.instance.neededexp;
			return true;
		}
		return false;
	}

	// Token: 0x060005B3 RID: 1459 RVA: 0x0003B140 File Offset: 0x00039340
	public static void DeathSmoke(Vector3 pos, Vector3 size, int render)
	{
		MainManager.deathpart.transform.localScale = size;
		MainManager.deathpart.transform.position = pos;
		MainManager.deathpart.Emit(20);
		MainManager.deathpart.GetComponent<Renderer>().material.renderQueue = render;
	}

	// Token: 0x060005B4 RID: 1460 RVA: 0x0003B18E File Offset: 0x0003938E
	public static void DeathSmoke(Vector3 pos, Vector3 size)
	{
		MainManager.DeathSmoke(pos, size, 3000);
	}

	// Token: 0x060005B5 RID: 1461 RVA: 0x0003B19C File Offset: 0x0003939C
	public static void DeathSmoke(Vector3 pos)
	{
		MainManager.DeathSmoke(pos, Vector3.one);
	}

	// Token: 0x060005B6 RID: 1462 RVA: 0x0003B1A9 File Offset: 0x000393A9
	public static void HitPart(Vector3 pos)
	{
		MainManager.hitpart.transform.position = pos;
		MainManager.hitpart.Play();
	}

	// Token: 0x060005B7 RID: 1463 RVA: 0x0003B1C8 File Offset: 0x000393C8
	public static void PlayBleep(AudioClip bleep, float bleeppitch, float bleepvol, int i)
	{
		if (i % 2 == 0 && bleep != null && !MainManager.bleeps.isPlaying)
		{
			MainManager.bleeps.clip = bleep;
			MainManager.bleeps.volume = bleepvol * ((MainManager.pausemenu == null) ? MainManager.bleepvolume : MainManager.pausemenu.dvolume);
			MainManager.bleeps.pitch = bleeppitch + Random.Range(-0.05f, 0.05f);
			MainManager.bleeps.Play();
		}
	}

	// Token: 0x060005B8 RID: 1464 RVA: 0x0003B24C File Offset: 0x0003944C
	public static bool ArrayIsEmpty(object[] input, bool stringcheck)
	{
		if (stringcheck)
		{
			return MainManager.ArrayIsEmpty(input);
		}
		for (int i = 0; i < input.Length; i++)
		{
			if (input[i] != null)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x060005B9 RID: 1465 RVA: 0x0003B27C File Offset: 0x0003947C
	public static bool ArrayIsEmpty(object[] input)
	{
		for (int i = 0; i < input.Length; i++)
		{
			if (input[i] != null && input[i].ToString() != "null")
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x060005BA RID: 1466 RVA: 0x0003B2B3 File Offset: 0x000394B3
	public static void UpdateJounal()
	{
		MainManager.UpdateJounal(MainManager.Library.Discovery, -1);
	}

	// Token: 0x060005BB RID: 1467 RVA: 0x0003B2BC File Offset: 0x000394BC
	public static void UpdateJounal(MainManager.Library type, int variable)
	{
		if (type != MainManager.Library.Bestiary || !MainManager.instance.inbattle)
		{
			if (variable > -1)
			{
				MainManager.instance.librarystuff[(int)type, variable] = true;
			}
			MainManager.instance.discoveryhud = 350f;
			MainManager.instance.discoverymessage.GetComponentInChildren<Animator>().PlayInFixedTime((type == MainManager.Library.Logbook) ? "Arch" : "Disc");
			for (int i = 0; i < 3; i++)
			{
				MainManager.instance.discoverymessage.GetChild(i + 1).gameObject.SetActive((type == MainManager.Library.Logbook) ? (i + 1 == 3) : (i + 1 == 1));
			}
			return;
		}
		MainManager.instance.librarystuff[(int)type, variable] = true;
	}

	// Token: 0x060005BC RID: 1468 RVA: 0x0003B374 File Offset: 0x00039574
	private static EntityControl GetEntity(NPCControl caller, string args)
	{
		EntityControl entity;
		if (caller != null)
		{
			entity = MainManager.GetEntity(args, caller.entity);
		}
		else
		{
			entity = MainManager.GetEntity(Convert.ToInt32(args));
		}
		return entity;
	}

	// Token: 0x060005BD RID: 1469 RVA: 0x0003B3A8 File Offset: 0x000395A8
	private static void SetTalk(bool dialogue, bool talkstate)
	{
		if (dialogue && MainManager.instance.tailtarget != null)
		{
			MainManager.instance.tailtarget.GetComponent<EntityControl>().talking = talkstate;
			if (talkstate)
			{
				MainManager.instance.tailtarget.GetComponent<EntityControl>().backsprite = false;
			}
		}
	}

	// Token: 0x060005BE RID: 1470 RVA: 0x0003B3F8 File Offset: 0x000395F8
	public static float GetTextLenght(string text, int fontid)
	{
		float num = 0f;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == ' ')
			{
				num += 0.3f;
			}
			else
			{
				num += MainManager.GetLetterOffset(text[i], fontid, 1f);
			}
		}
		return num;
	}

	// Token: 0x060005BF RID: 1471 RVA: 0x0003B447 File Offset: 0x00039647
	public static void HealParticle(Transform parent, Vector3 size, Vector3 offset)
	{
		MainManager.HealParticle(parent, size, offset, false);
	}

	// Token: 0x060005C0 RID: 1472 RVA: 0x0003B454 File Offset: 0x00039654
	public static void HealParticle(Transform parent, Vector3 size, Vector3 offset, bool UI)
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Particles/Heal")) as GameObject;
		gameObject.transform.parent = parent;
		gameObject.transform.localScale = size;
		gameObject.transform.localPosition = offset;
		if (UI)
		{
			gameObject.layer = 5;
		}
		Object.Destroy(gameObject, 3f);
	}

	// Token: 0x060005C1 RID: 1473 RVA: 0x0003B4B0 File Offset: 0x000396B0
	public static void CreateCursor(Transform parent)
	{
		MainManager.instance.cursor = new GameObject("Cursor").AddComponent<SpriteRenderer>();
		MainManager.instance.cursor.sprite = MainManager.cursorsprite[0];
		MainManager.instance.cursor.GetComponent<SpriteRenderer>().sortingOrder = 1;
		MainManager.instance.cursor.transform.parent = parent;
		MainManager.instance.cursor.gameObject.layer = 5;
		MainManager.instance.cursor.transform.localScale = Vector3.one;
		MainManager.instance.cursor.transform.localEulerAngles = Vector3.zero;
		MainManager.instance.cursor.gameObject.AddComponent<SpriteBounce>().MessageBounce();
	}

	// Token: 0x060005C2 RID: 1474 RVA: 0x0003B578 File Offset: 0x00039778
	public static Vector3 RandomItemBounce(float range, float height)
	{
		range /= 2f;
		Vector3 vector = MainManager.ClampMagnitude(MainManager.RandomVector(range, range), range, range);
		return new Vector3(vector.x, height, vector.y);
	}

	// Token: 0x060005C3 RID: 1475 RVA: 0x0003B5AF File Offset: 0x000397AF
	public static void DestroyTemp(ref GameObject obj, float time)
	{
		if (obj != null)
		{
			obj.transform.position = new Vector3(0f, -9999f);
			Object.Destroy(obj.gameObject, time);
			obj = null;
		}
	}

	// Token: 0x060005C4 RID: 1476 RVA: 0x0003B5E6 File Offset: 0x000397E6
	public static void DestroyTemp(GameObject obj, float time)
	{
		if (obj != null)
		{
			obj.transform.position = new Vector3(0f, -9999f);
			Object.Destroy(obj.gameObject, time);
		}
	}

	// Token: 0x060005C5 RID: 1477 RVA: 0x0003B618 File Offset: 0x00039818
	private static int[] GetLibraryIDs(int index)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < MainManager.librarylimit[index]; i++)
		{
			list.Add(MainManager.libraryorder[index, i]);
		}
		return list.ToArray();
	}

	// Token: 0x060005C6 RID: 1478 RVA: 0x0003B655 File Offset: 0x00039855
	public static IEnumerator DelayedPosition(Transform obj, Vector3 pos, float delay, bool local)
	{
		if (delay > 0f)
		{
			yield return new WaitForSeconds(delay);
		}
		else
		{
			yield return null;
		}
		if (local)
		{
			obj.transform.localPosition = pos;
		}
		else
		{
			obj.transform.position = pos;
		}
		yield break;
	}

	// Token: 0x060005C7 RID: 1479 RVA: 0x0003B679 File Offset: 0x00039879
	public static void DisableRender(Renderer render, float tolerance, Vector3 offset)
	{
		if (MainManager.CheckIfCamera(render.transform.position, tolerance, offset))
		{
			render.shadowCastingMode = ShadowCastingMode.On;
			return;
		}
		render.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
	}

	// Token: 0x060005C8 RID: 1480 RVA: 0x0003B69E File Offset: 0x0003989E
	public static void DisableRender(GameObject render, float tolerance, Vector3 offset)
	{
		render.SetActive(MainManager.CheckIfCamera(render.transform.position, tolerance, offset));
	}

	// Token: 0x060005C9 RID: 1481 RVA: 0x0003B6B8 File Offset: 0x000398B8
	public static void ShakeScreen(Vector3 ammount, float time)
	{
		MainManager.ShakeScreen(ammount, time, false);
	}

	// Token: 0x060005CA RID: 1482 RVA: 0x0003B6C2 File Offset: 0x000398C2
	public static void ShakeScreen(float ammount, float time, bool dontreset)
	{
		MainManager.ShakeScreen(Vector3.one * ammount, time, dontreset);
	}

	// Token: 0x060005CB RID: 1483 RVA: 0x0003B6D8 File Offset: 0x000398D8
	public static void ShakeScreen(Vector3 ammount, float time, bool dontreset)
	{
		MainManager.camposshake = MainManager.instance.camoffset;
		MainManager.screenshake = new Vector3(ammount.x, ammount.y / 2f, 0f);
		if (time > 0f)
		{
			if (dontreset)
			{
				MainManager.instance.Invoke("StopScreenShake", time);
				return;
			}
			MainManager.instance.Invoke("StopScreenShakeReturn", time);
		}
	}

	// Token: 0x060005CC RID: 1484 RVA: 0x0003B741 File Offset: 0x00039941
	public static void ShakeScreen(float ammount, float time)
	{
		MainManager.ShakeScreen(Vector3.one * ammount, time);
	}

	// Token: 0x060005CD RID: 1485 RVA: 0x0003B754 File Offset: 0x00039954
	public static void ShakeScreen(float time)
	{
		MainManager.ShakeScreen(Vector3.one * 0.1f, time);
	}

	// Token: 0x060005CE RID: 1486 RVA: 0x0003B76B File Offset: 0x0003996B
	public static void ShakeScreen()
	{
		MainManager.ShakeScreen(Vector3.one * 0.1f, -1f);
	}

	// Token: 0x060005CF RID: 1487 RVA: 0x0003B786 File Offset: 0x00039986
	public static void ShakeScreen(Vector3 ammount)
	{
		MainManager.ShakeScreen(ammount, -1f);
	}

	// Token: 0x060005D0 RID: 1488 RVA: 0x0003B793 File Offset: 0x00039993
	public void StopScreenShakeReturn()
	{
		MainManager.screenshake = Vector3.zero;
		MainManager.MainCamera.transform.localPosition = MainManager.camposshake;
	}

	// Token: 0x060005D1 RID: 1489 RVA: 0x0003B7B3 File Offset: 0x000399B3
	public void StopScreenShake()
	{
		MainManager.screenshake = Vector3.zero;
	}

	// Token: 0x060005D2 RID: 1490 RVA: 0x0003B7BF File Offset: 0x000399BF
	public static void FadeMusic(float fadespeed)
	{
		MainManager.ChangeMusic(null, fadespeed);
	}

	// Token: 0x060005D3 RID: 1491 RVA: 0x0003B7C8 File Offset: 0x000399C8
	private static bool CheckIfCamera(Vector3 campos, float tolerance, Vector3 offset)
	{
		campos = MainManager.MainCamera.WorldToViewportPoint(campos + offset);
		return campos.x > -0.5f && campos.x < 1.5f && campos.y > -0.5f && campos.y < 1.5f && campos.z > tolerance;
	}

	// Token: 0x060005D4 RID: 1492 RVA: 0x0003B827 File Offset: 0x00039A27
	public static int GetTPCost(int player, int id)
	{
		return MainManager.GetTPCost(player, id, false);
	}

	// Token: 0x060005D5 RID: 1493 RVA: 0x0003B834 File Offset: 0x00039A34
	public static int GetTPCost(int player, int id, bool matchid)
	{
		if (matchid)
		{
			int[] array = MainManager.instance.playerdata[player].skills.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == id)
				{
					id = i;
					break;
				}
			}
		}
		id = MainManager.instance.playerdata[player].skills[id];
		int num = Convert.ToInt32(MainManager.skilldata[id, 2]);
		int num2 = Mathf.Abs(num);
		if (MainManager.BadgeIsEquipped(28) && (id == 16 || id == 2 || id == 18 || id == 24))
		{
			num2++;
		}
		if (MainManager.BadgeIsEquipped(72) && (id == 11 || id == 45))
		{
			num2 += MainManager.BadgeHowManyEquipped(72, MainManager.instance.playerdata[player].trueid);
		}
		if (num2 == 0)
		{
			return 0;
		}
		return Mathf.Clamp(num2 - MainManager.BadgeHowManyEquipped(25, MainManager.instance.playerdata[player].trueid) - MainManager.BadgeHowManyEquipped(72, MainManager.instance.playerdata[player].trueid), 1, 99) * ((num < 0) ? -1 : 1);
	}

	// Token: 0x060005D6 RID: 1494 RVA: 0x0003B954 File Offset: 0x00039B54
	public static int HowManyItem(int type, int id)
	{
		if (MainManager.instance.items[type] == null || MainManager.instance.items[type].Count == 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < MainManager.instance.items[type].Count; i++)
		{
			if (MainManager.instance.items[type][i] == id)
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x060005D7 RID: 1495 RVA: 0x0003B9BC File Offset: 0x00039BBC
	public static IEnumerator ArcMovement(GameObject obj, Vector3 startpos, Vector3 targetpos, Vector3 spin, float height, float frametime, bool destroyonend)
	{
		float a = 0f;
		while (!(obj == null))
		{
			obj.transform.Rotate(spin * MainManager.TieFramerate(1f));
			obj.transform.position = MainManager.BeizierCurve3(startpos, targetpos, height, a / frametime);
			a += MainManager.TieFramerate(1f);
			yield return null;
			if (a >= frametime + 1f)
			{
				if (obj != null)
				{
					if (destroyonend)
					{
						Object.Destroy(obj);
					}
					else
					{
						obj.transform.position = targetpos;
					}
				}
				yield return null;
				yield break;
			}
		}
		yield break;
	}

	// Token: 0x060005D8 RID: 1496 RVA: 0x0003B9F8 File Offset: 0x00039BF8
	public static string GetBadgeName(int id)
	{
		return MainManager.menutext[268].Replace("i", "replace1").Replace("m", "replace2").Replace("replace1", MainManager.badgedata[id, 0]).Replace("replace2", MainManager.menutext[159]);
	}

	// Token: 0x060005D9 RID: 1497 RVA: 0x0003BA59 File Offset: 0x00039C59
	public static IEnumerator ArcMovement(GameObject obj, Vector3 targetpos, float height, float frametime)
	{
		MainManager.instance.StartCoroutine(MainManager.ArcMovement(obj, obj.transform.position, targetpos, Vector3.zero, height, frametime, false));
		yield return null;
		yield break;
	}

	// Token: 0x060005DA RID: 1498 RVA: 0x0003BA80 File Offset: 0x00039C80
	public static void RefreshBadgeOrder()
	{
		List<int[]> list = new List<int[]>();
		for (int i = 0; i < MainManager.badgeorder.Length; i++)
		{
			for (int j = 0; j < MainManager.instance.badges.Count; j++)
			{
				if (MainManager.instance.badges[j][0] == MainManager.badgeorder[i])
				{
					list.Add(MainManager.instance.badges[j]);
				}
			}
		}
		MainManager.instance.badges = list;
	}

	// Token: 0x060005DB RID: 1499 RVA: 0x0003BAFC File Offset: 0x00039CFC
	public static int[] OrganizeArrayInt(int[] inputarray, int[] order)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < order.Length; i++)
		{
			for (int j = 0; j < inputarray.Length; j++)
			{
				if (inputarray[j] == order[i])
				{
					list.Add(inputarray[j]);
					break;
				}
			}
		}
		return list.ToArray();
	}

	// Token: 0x060005DC RID: 1500 RVA: 0x0003BB44 File Offset: 0x00039D44
	private static int[] GetBadgeIDs()
	{
		if (MainManager.instance.badges.Count != 0)
		{
			List<int> list = new List<int>();
			int[][] array = MainManager.instance.badges.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				list.Add(array[i][0]);
			}
			return list.ToArray();
		}
		if (!(MainManager.pausemenu != null))
		{
			return new int[]
			{
				-1
			};
		}
		return new int[0];
	}

	// Token: 0x060005DD RID: 1501 RVA: 0x0003BBB8 File Offset: 0x00039DB8
	public static int[] GetEquippedBadgeIDs()
	{
		if (MainManager.instance.badges.Count != 0)
		{
			List<int> list = new List<int>();
			int[][] array = MainManager.instance.badges.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i][1] > -2)
				{
					list.Add(i);
				}
			}
			return list.ToArray();
		}
		if (!(MainManager.pausemenu != null))
		{
			return new int[]
			{
				-1
			};
		}
		return new int[0];
	}

	// Token: 0x060005DE RID: 1502 RVA: 0x0003BC30 File Offset: 0x00039E30
	private static int[] GetLibraryOrder(int id)
	{
		int[] array = new int[MainManager.librarylimit[id]];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = MainManager.libraryorder[id, i];
		}
		return array;
	}

	// Token: 0x060005DF RID: 1503 RVA: 0x0003BC68 File Offset: 0x00039E68
	private static int[] GetOverralQuests()
	{
		List<int> list = new List<int>();
		int[] array = MainManager.instance.boardquests[1].ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != 0)
			{
				list.Add(array[i]);
			}
		}
		array = MainManager.instance.boardquests[2].ToArray();
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] != 0)
			{
				list.Add(-array[j]);
			}
		}
		return list.ToArray();
	}

	// Token: 0x060005E0 RID: 1504 RVA: 0x0003BCE0 File Offset: 0x00039EE0
	public static Vector3[] GetPartyPos(bool inorder)
	{
		List<Vector3> list = new List<Vector3>();
		if (inorder)
		{
			for (int i = 0; i < 3; i++)
			{
				EntityControl entity = MainManager.GetEntity(-(4 + i));
				if (entity != null)
				{
					list.Add(entity.transform.position);
				}
			}
		}
		else
		{
			for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
			{
				list.Add(MainManager.instance.playerdata[j].entity.transform.position);
			}
		}
		return list.ToArray();
	}

	// Token: 0x060005E1 RID: 1505 RVA: 0x0003BD6C File Offset: 0x00039F6C
	public static void DestroyPlayers(bool remake, bool inorder)
	{
		Vector3[] partyPos = MainManager.GetPartyPos(inorder);
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			Object.Destroy(MainManager.instance.playerdata[i].entity.gameObject);
		}
		if (remake)
		{
			MainManager.SetPlayers(partyPos);
		}
	}

	// Token: 0x060005E2 RID: 1506 RVA: 0x0003BDBF File Offset: 0x00039FBF
	public static Vector3 MultiplyVector(Vector3 a, Vector3 b)
	{
		return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	// Token: 0x060005E3 RID: 1507 RVA: 0x0003BDF0 File Offset: 0x00039FF0
	private static int[] GetQuestsBoard(int type)
	{
		int[] array = MainManager.instance.boardquests[type].ToArray();
		bool flag = type != 0 || Convert.ToInt32(MainManager.map.name) == 30;
		List<int> list = new List<int>();
		for (int i = 0; i < array.Length; i++)
		{
			if (Convert.ToInt32(MainManager.map.name) == 0 || (array[i] != 30 && array[i] != 26 && (array[i] < 11 || array[i] > 17) && (flag || (array[i] != 10 && array[i] != 9 && array[i] != 8 && array[i] != 23 && array[i] != 21))))
			{
				list.Add(array[i]);
			}
		}
		if (list.Count == 0)
		{
			list.Add(0);
		}
		return list.ToArray();
	}

	// Token: 0x060005E4 RID: 1508 RVA: 0x0003BEB0 File Offset: 0x0003A0B0
	public static int[] GetBosses()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < MainManager.instance.multilist.Length; i++)
		{
			if (MainManager.instance.multilist[i] == -2)
			{
				if (MainManager.instance.flags[555])
				{
					list.Add(-2);
				}
			}
			else if (MainManager.instance.multilist[i] == -1)
			{
				if (MainManager.instance.flags[118])
				{
					list.Add(-1);
				}
			}
			else if (MainManager.instance.enemyencounter[MainManager.instance.multilist[i], 0] > 0)
			{
				list.Add(MainManager.instance.multilist[i]);
			}
		}
		return list.ToArray();
	}

	// Token: 0x060005E5 RID: 1509 RVA: 0x0003BF6A File Offset: 0x0003A16A
	public static void PlaySoundAt(string sound, float volume, Vector3 position)
	{
		AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audio/Sounds/" + sound), position, MainManager.GetSoundDistance(position) * volume * MainManager.soundvolume);
	}

	// Token: 0x060005E6 RID: 1510 RVA: 0x0003BF90 File Offset: 0x0003A190
	public static int[] GradualFill(int startat, int ammount)
	{
		int[] array = new int[ammount];
		for (int i = 0; i < ammount; i++)
		{
			array[i] = startat + i;
		}
		return array;
	}

	// Token: 0x060005E7 RID: 1511 RVA: 0x0003BFB7 File Offset: 0x0003A1B7
	public static int[] GradualFill(int ammount)
	{
		return MainManager.GradualFill(0, ammount);
	}

	// Token: 0x060005E8 RID: 1512 RVA: 0x0003BFC0 File Offset: 0x0003A1C0
	public static int[] GetSettings()
	{
		List<int> list = new List<int>();
		list.AddRange(new int[]
		{
			0,
			1,
			2,
			21,
			19,
			20,
			3,
			4,
			6,
			7,
			8,
			9,
			11,
			10,
			16,
			12,
			23,
			13,
			17,
			22,
			25,
			18,
			14,
			15
		});
		if (MainManager.pausemenu.vsyc == 1)
		{
			list.Remove(12);
		}
		if (MainManager.pausemenu.joystick == 0)
		{
			list.Remove(22);
		}
		if (MainManager.pausemenu.joystick < 3 || MainManager.pausemenu.joystick > 4)
		{
			list.Remove(17);
		}
		if (MainManager.pausemenu.joystick != 5)
		{
			list.Remove(18);
		}
		return list.ToArray();
	}

	// Token: 0x060005E9 RID: 1513 RVA: 0x0003C058 File Offset: 0x0003A258
	public static string[] Controllers()
	{
		string[] joystickNames = Input.GetJoystickNames();
		List<string> list = new List<string>();
		for (int i = 0; i < joystickNames.Length; i++)
		{
			if (joystickNames[i].Length > 1)
			{
				list.Add(joystickNames[i]);
			}
		}
		return list.ToArray();
	}

	// Token: 0x060005EA RID: 1514 RVA: 0x0003C09C File Offset: 0x0003A29C
	public static int[] SamiraMissing()
	{
		int[][] array = MainManager.instance.samiramusics.ToArray();
		List<int> list = new List<int>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i][1] < 1)
			{
				list.Add(i);
			}
		}
		return list.ToArray();
	}

	// Token: 0x060005EB RID: 1515 RVA: 0x0003C0E4 File Offset: 0x0003A2E4
	public static void ShowItemList(int type, Vector2 position, bool showdescription, bool sell)
	{
		if (MainManager.instance.itemlist == null)
		{
			MainManager.listY = -1;
			MainManager.instance.multiselect = new List<int>();
		}
		MainManager.listpos = position;
		MainManager.listcanceled = false;
		int num = type;
		int num2 = type;
		int[,] array = null;
		switch (type)
		{
		case -3:
		case -2:
		case -1:
			MainManager.listvar = MainManager.instance.playerdata[Mathf.Abs(type) - 1].skills.ToArray();
			num = -1;
			MainManager.listredirect = null;
			goto IL_538;
		case 2:
			num = 0;
			break;
		case 3:
			MainManager.listvar = MainManager.GradualFill(MainManager.instance.badges.Count);
			if (MainManager.instance.pause)
			{
				MainManager.listredirect = null;
				goto IL_538;
			}
			goto IL_538;
		case 9:
			MainManager.listvar = new int[]
			{
				0,
				271,
				1,
				2,
				3
			};
			num = -1;
			MainManager.listredirect = null;
			goto IL_538;
		case 10:
		case 11:
		case 12:
		case 13:
			MainManager.listvar = MainManager.OrganizeArrayInt(MainManager.GetLibraryIDs(type - 10), MainManager.GetLibraryOrder(type - 10));
			MainManager.listredirect = null;
			goto IL_538;
		case 14:
		case 15:
		case 16:
			MainManager.listvar = MainManager.GetQuestsBoard(type - 14);
			MainManager.listredirect = null;
			goto IL_538;
		case 17:
			if (InputIO.IsConsole)
			{
				if (MainManager.pausemenu.calledfrommain)
				{
					MainManager.listvar = new int[]
					{
						0,
						1,
						2,
						19,
						20,
						9,
						22,
						25
					};
				}
				else
				{
					MainManager.listvar = new int[]
					{
						0,
						1,
						2,
						19,
						20,
						9,
						22,
						25,
						15
					};
				}
			}
			else
			{
				MainManager.listvar = MainManager.GetSettings();
			}
			MainManager.listredirect = null;
			goto IL_538;
		case 18:
		{
			num = -1;
			List<int> list = new List<int>();
			int[][] array2 = MainManager.instance.samiramusics.ToArray();
			if (MainManager.SamiraMissing().Length != 0)
			{
				list.Add(-1);
			}
			for (int i = 0; i < array2.Length; i++)
			{
				list.Add(array2[i][0]);
			}
			MainManager.listvar = list.ToArray();
			goto IL_538;
		}
		case 19:
		case 30:
		{
			List<int> list = new List<int>();
			for (int j = 0; j < InputIO.keys.Length; j++)
			{
				list.Add(j);
			}
			MainManager.listvar = list.ToArray();
			MainManager.listredirect = null;
			goto IL_538;
		}
		case 20:
			MainManager.listvar = new int[]
			{
				0,
				3,
				1,
				2,
				4,
				5,
				6
			};
			MainManager.listredirect = null;
			goto IL_538;
		case 21:
			MainManager.listvar = MainManager.GetOverralQuests();
			MainManager.listredirect = null;
			goto IL_538;
		case 22:
			MainManager.listvar = MainManager.GradualFill(MainManager.instance.flagvar[15]);
			MainManager.listredirect = new int?(-66);
			goto IL_538;
		case 23:
		{
			int[][] array3 = MainManager.instance.statbonus.ToArray();
			if (array3.Length == 0)
			{
				MainManager.listvar = new int[]
				{
					-1
				};
				goto IL_538;
			}
			MainManager.listvar = new int[array3.Length];
			array = new int[array3.Length, 2];
			for (int k = 0; k < array3.Length; k++)
			{
				MainManager.listvar[k] = array3[k][0];
				array[k, 0] = array3[k][1];
				array[k, 1] = array3[k][2];
			}
			goto IL_538;
		}
		case 24:
			MainManager.listvar = MainManager.GetBosses();
			goto IL_538;
		case 25:
			MainManager.listvar = MainManager.instance.multilist;
			goto IL_538;
		case 26:
			MainManager.listvar = MainManager.GradualFill(MainManager.termacadeprize.GetLength(0));
			goto IL_538;
		case 27:
			type = 0;
			num = 0;
			MainManager.listvar = MainManager.GradualFill(Enum.GetNames(typeof(MainManager.Items)).Length);
			goto IL_538;
		case 28:
			type = 3;
			num = 3;
			MainManager.listvar = MainManager.GradualFill(Enum.GetNames(typeof(MainManager.BadgeTypes)).Length);
			goto IL_538;
		case 29:
			type = -1;
			num = -1;
			MainManager.listvar = MainManager.GradualFill(Enum.GetNames(typeof(MainManager.Skills)).Length);
			goto IL_538;
		case 31:
			type = 31;
			num = 31;
			MainManager.listvar = MainManager.GradualFill(Enum.GetNames(typeof(MainManager.Maps)).Length);
			goto IL_538;
		case 32:
			type = 3;
			num = 3;
			MainManager.listvar = MainManager.instance.multilist;
			MainManager.listredirect = null;
			goto IL_538;
		case 33:
			MainManager.listvar = MainManager.OrganizeArrayInt(MainManager.instance.multilist, CardGame.order);
			goto IL_538;
		case 34:
			showdescription = !MainManager.instance.flags[681];
			MainManager.listvar = MainManager.caravanorder;
			goto IL_538;
		case 35:
			num = 0;
			type = 0;
			MainManager.listvar = MainManager.instance.multilist;
			goto IL_538;
		}
		num = 0;
		if (MainManager.instance.items[type].Count == 0)
		{
			MainManager.listvar = new int[]
			{
				-1
			};
		}
		else
		{
			MainManager.listvar = MainManager.instance.items[type].ToArray();
		}
		IL_538:
		MainManager.instance.maxoptions = MainManager.listvar.Length;
		if (MainManager.listlow != MainManager.listY || MainManager.listtype == 20)
		{
			MainManager.listY = MainManager.listlow;
			if (MainManager.instance.itemlist == null)
			{
				MainManager.listmax = MainManager.listammount;
				MainManager.listlow = 0;
				MainManager.instance.option = 0;
				MainManager.listcursor = 0;
				MainManager.CreateCursor(MainManager.GUICamera.transform);
				MainManager.instance.inlist = true;
				MainManager.instance.cursor.transform.position = MainManager.GUICamera.transform.position;
			}
			if (MainManager.instance.itemlist != null)
			{
				for (int l = 0; l < MainManager.instance.itemlist.childCount; l++)
				{
					MainManager.DestroyText(MainManager.instance.itemlist.GetChild(l), false);
				}
				Object.Destroy(MainManager.instance.itemlist.gameObject);
			}
			MainManager.instance.itemlist = new GameObject("ItemList").transform;
			if (MainManager.instance.questboardobj != null)
			{
				MainManager.instance.itemlist.parent = MainManager.instance.questboardobj;
			}
			else
			{
				MainManager.instance.itemlist.parent = MainManager.GUICamera.transform;
			}
			MainManager.instance.itemlist.localPosition = new Vector3(position.x, position.y, 10f);
			if (MainManager.listtype == 20)
			{
				MainManager.instance.StartCoroutine(MainManager.SetText("|color,4||center||sort,20|" + MainManager.languagehelp[MainManager.listvar[MainManager.instance.option]], new Vector3(1f, 4f), MainManager.instance.itemlist));
			}
			float num3 = 0.25f;
			if (MainManager.listlow > 0 && MainManager.instance.maxoptions > MainManager.listammount)
			{
				if (!MainManager.instance.pause && (type <= 16 || type == 18 || type >= 22 || type == 20))
				{
					SpriteRenderer spriteRenderer = new GameObject("UpArrow").AddComponent<SpriteRenderer>();
					spriteRenderer.gameObject.layer = 5;
					spriteRenderer.transform.parent = MainManager.instance.itemlist;
					if (type >= 14 && type <= 16)
					{
						spriteRenderer.sprite = MainManager.guisprites[1];
						spriteRenderer.transform.localPosition = new Vector2(8.8f, num3 + 0.2f);
					}
					else
					{
						spriteRenderer.sprite = MainManager.guisprites[3];
						spriteRenderer.transform.localPosition = new Vector2(1f, num3 + 0.7f);
					}
					spriteRenderer.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
					spriteRenderer.sortingOrder = 3;
					if (type == 20)
					{
						spriteRenderer.sortingOrder = 10;
					}
				}
				else if (MainManager.pausemenu != null)
				{
					SpriteRenderer component = MainManager.NewUIObject("uparrow", null, Vector3.zero, Vector3.one, MainManager.guisprites[1]).GetComponent<SpriteRenderer>();
					component.transform.parent = MainManager.instance.itemlist;
					component.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
					float x = 11f;
					if (MainManager.pausemenu.windowid == 3)
					{
						x = 9.5f;
					}
					else if (MainManager.pausemenu.windowid == 4)
					{
						x = 11.25f;
					}
					else if (MainManager.pausemenu.windowid == 5)
					{
						x = 8.5f;
					}
					else if (MainManager.pausemenu.windowid == 1)
					{
						x = 7f;
					}
					component.transform.localPosition = new Vector2(x, num3 + 0.3f);
					component.transform.localScale = Vector3.one * 1.25f;
				}
			}
			if (MainManager.overridedlist != null)
			{
				MainManager.LoadList(MainManager.overridedlist);
				MainManager.overridedlist = null;
			}
			string text = "";
			float x2 = 0.55f;
			if (MainManager.IsInMultiList())
			{
				new GameObject().AddComponent<ButtonSprite>().SetUp(7, -1, MainManager.menutext[281], new Vector3(-0.35f, -3.75f), Vector3.one * 0.4f, 99, MainManager.instance.itemlist);
				MainManager.NewUIObject("box", MainManager.instance.itemlist, new Vector3(1.35f, -3.75f), new Vector3(0.75f, 1.25f, 1f), MainManager.guisprites[0], -5).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.75f);
			}
			int num4 = MainManager.listlow;
			while (num4 < MainManager.listmax && num4 < MainManager.instance.maxoptions)
			{
				SpriteRenderer spriteRenderer2 = new GameObject("Bar" + num4).AddComponent<SpriteRenderer>();
				if (!MainManager.instance.pause && (type < 14 || type == 18 || type >= 22))
				{
					spriteRenderer2.sprite = MainManager.guisprites[0];
				}
				spriteRenderer2.gameObject.layer = 5;
				spriteRenderer2.transform.parent = MainManager.instance.itemlist;
				spriteRenderer2.sortingOrder = -1;
				if (!MainManager.instance.pause)
				{
					spriteRenderer2.transform.localScale = new Vector3(1.15f, 1f, 1f);
				}
				spriteRenderer2.transform.localPosition = new Vector2(1.4f, num3);
				spriteRenderer2.color = (MainManager.instance.multiselect.Contains(num4) ? MainManager.itemlistbg[1] : MainManager.itemlistbg[0]);
				num3 -= 0.7f;
				float x3 = -2f;
				float y = -0.15f;
				Vector2 one = new Vector2(0.75f, 0.75f);
				if (type == 22)
				{
					string[] array4 = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/LoreText").ToString().Split(new char[]
					{
						'\n'
					})[num4].Split(new char[]
					{
						'@'
					});
					text = array4[0];
					if (num4 == MainManager.instance.option)
					{
						MainManager.instance.flagstring[0] = array4[1];
					}
					x3 = -2.65f;
				}
				else if (type == 26)
				{
					bool flag = MainManager.termacadeprize[MainManager.listvar[num4], 0] == 2;
					int num5 = MainManager.termacadeprize[MainManager.listvar[num4], 1];
					SpriteRenderer spriteRenderer3 = new GameObject("itemsprite").AddComponent<SpriteRenderer>();
					spriteRenderer3.transform.parent = spriteRenderer2.transform;
					spriteRenderer3.transform.localPosition = new Vector2(-2.5f, 0f);
					spriteRenderer3.gameObject.layer = 5;
					spriteRenderer3.transform.localScale = new Vector3(x2, 0.6f, 1f);
					if (flag && MainManager.instance.flags[681])
					{
						spriteRenderer3.sprite = MainManager.guisprites[190];
						text = MainManager.menutext[59];
					}
					else
					{
						spriteRenderer3.sprite = MainManager.itemsprites[flag ? 1 : 0, num5];
						text = (flag ? MainManager.badgedata[num5, 0] : MainManager.itemdata[0, num5, 0]);
					}
					if (MainManager.termacadeprize[MainManager.listvar[num4], 3] == 1 && MainManager.instance.flags[MainManager.termacadeprize[MainManager.listvar[num4], 4]])
					{
						MainManager.instance.StartCoroutine(MainManager.SetText("|sort,10||size,0.6,0.75||color,1|" + MainManager.menutext[190], new Vector3(1f, -0.15f), spriteRenderer2.transform));
					}
					else
					{
						MainManager.instance.StartCoroutine(MainManager.SetText("|sort,10||size,0.6,0.75|" + MainManager.termacadeprize[MainManager.listvar[num4], 2], new Vector3(1.4f, -0.15f), spriteRenderer2.transform));
						MainManager.NewUIObject("tp", spriteRenderer2.transform, new Vector3(2.55f, 0f), new Vector3(0.6f, 0.65f, 1f), MainManager.itemsprites[0, 110], 10).GetComponent<SpriteRenderer>();
					}
				}
				else if (type >= 0 && type <= 2)
				{
					SpriteRenderer spriteRenderer4 = new GameObject("itemsprite").AddComponent<SpriteRenderer>();
					if (MainManager.listvar[num4] == -1)
					{
						text = "|color,1|" + MainManager.menutext[20];
					}
					else
					{
						spriteRenderer4.sprite = MainManager.itemsprites[num, MainManager.listvar[num4]];
						spriteRenderer4.transform.parent = spriteRenderer2.transform;
						spriteRenderer4.transform.localPosition = new Vector2(-2.5f, 0f);
						spriteRenderer4.gameObject.layer = 5;
						spriteRenderer4.transform.localScale = new Vector3(x2, 0.6f, 1f);
						text = MainManager.itemdata[num, MainManager.listvar[num4], 0];
						if (MainManager.instance.pause)
						{
							spriteRenderer2.transform.localPosition = new Vector3(1f, num3 + 0.7f, 0f);
							one = Vector2.one;
							spriteRenderer4.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
							spriteRenderer4.transform.localPosition = new Vector2(-2.75f, 0f);
							num3 -= 0.2f;
						}
					}
				}
				else if (type == 33)
				{
					x3 = -2.65f;
					text = MainManager.enemynames[MainManager.instance.cardgame.carddata[MainManager.listvar[num4]].enemyid];
					MainManager.instance.flagvar[6] = MainManager.instance.option;
				}
				else if (type == 31)
				{
					x3 = -2.65f;
					text = Enum.GetNames(typeof(MainManager.Maps))[MainManager.listvar[num4]];
				}
				else
				{
					if (type == 24)
					{
						x3 = -2.65f;
						int num6 = MainManager.listvar[num4];
						if (num6 <= 51)
						{
							if (num6 != -2)
							{
								if (num6 != -1)
								{
									if (num6 != 51)
									{
										goto IL_105A;
									}
									text = MainManager.map.dialogues[74];
								}
								else
								{
									text = MainManager.map.dialogues[13];
								}
							}
							else
							{
								text = MainManager.map.dialogues[87];
							}
						}
						else if (num6 != 74)
						{
							if (num6 != 85)
							{
								if (num6 != 92)
								{
									goto IL_105A;
								}
								text = MainManager.map.dialogues[76];
							}
							else
							{
								text = MainManager.map.dialogues[75];
							}
						}
						else
						{
							text = MainManager.map.dialogues[77];
						}
						IL_106A:
						MainManager.instance.flagvar[6] = MainManager.instance.option;
						goto IL_2EF5;
						IL_105A:
						text = MainManager.enemynames[MainManager.listvar[num4]];
						goto IL_106A;
					}
					if (type == 23)
					{
						x3 = -2.65f;
						if (MainManager.listvar[num4] == -1)
						{
							text = "|color,1|" + MainManager.menutext[20];
							MainManager.instance.flagvar[0] = -1;
						}
						else
						{
							text = string.Concat(new object[]
							{
								(MainManager.StatBonus)MainManager.listvar[num4],
								" x",
								array[num4, 0],
								" to ",
								(array[num4, 1] == -1) ? "All" : MainManager.menutext[46 + array[num4, 1]]
							});
							MainManager.instance.flagvar[1] = MainManager.instance.option;
						}
					}
					else if (type >= -3 && type <= -1)
					{
						int num7 = Mathf.Abs(type) - 1;
						int num8 = (num2 == 29) ? 0 : MainManager.GetTPCost(num7, num4);
						int tpcost = num8;
						string text2 = "";
						string text3 = "";
						bool flag2 = MainManager.BadgeIsEquipped(72, MainManager.instance.playerdata[num7].trueid) || num8 < 0;
						num8 = Mathf.Abs(num8);
						if (MainManager.instance.pause)
						{
							num3 -= 0.2f;
							y = -0.2f;
							one = Vector2.one;
							x3 = -0.75f;
							MainManager.instance.StartCoroutine(MainManager.SetText("|sort,10||font,0|" + ((num8 == 0) ? "  -" : num8.ToString().PadLeft(2, ' ')), new Vector3(6.9f, -0.2f), spriteRenderer2.transform));
							if (num8 != 0)
							{
								MainManager.NewUIObject("tp", spriteRenderer2.transform, new Vector3(8.45f, 0f), Vector3.one * 0.6f * (flag2 ? 0.65f : 1f), MainManager.guisprites[flag2 ? 24 : 28], 10).GetComponent<SpriteRenderer>();
							}
							text2 = "|single|";
						}
						else
						{
							if (!MainManager.HasSkillCost(tpcost, num7))
							{
								text2 = "|color,1|";
							}
							x3 = -2.65f;
							MainManager.instance.StartCoroutine(MainManager.SetText("|sort,10||size,0.75||font,0|" + text2 + num8.ToString().PadLeft(2, ' '), new Vector3(1.6f, -0.15f), spriteRenderer2.transform));
							MainManager.NewUIObject("tp", spriteRenderer2.transform, new Vector3(2.55f, 0f), new Vector3(0.45f, 0.5f, 1f) * (flag2 ? 0.65f : 1f), MainManager.guisprites[flag2 ? 24 : 28], 10).GetComponent<SpriteRenderer>();
							int num6 = MainManager.listvar[num4];
							if (num6 <= 11)
							{
								if (num6 == 2)
								{
									goto IL_13FE;
								}
								if (num6 != 3)
								{
									if (num6 != 11)
									{
										goto IL_148F;
									}
								}
								else
								{
									text3 += " |size,0.55,0.6|";
									if (MainManager.BadgeIsEquipped(67))
									{
										text3 += "|icon,195|";
										goto IL_148F;
									}
									goto IL_148F;
								}
							}
							else
							{
								if (num6 <= 18)
								{
									if (num6 != 16)
									{
										if (num6 != 18)
										{
											goto IL_148F;
										}
										goto IL_13FE;
									}
								}
								else if (num6 != 24)
								{
									if (num6 != 45)
									{
										goto IL_148F;
									}
									goto IL_1425;
								}
								text3 += " |size,0.55,0.6|";
								if (MainManager.BadgeIsEquipped(57))
								{
									text3 += "|icon,192|";
								}
								if (MainManager.BadgeIsEquipped(41))
								{
									text3 += "|icon,191|";
								}
								if (MainManager.BadgeIsEquipped(22))
								{
									text3 += "|icon,193|";
									goto IL_13FE;
								}
								goto IL_13FE;
							}
							IL_1425:
							text3 += " |size,0.55,0.6|";
							if (MainManager.BadgeIsEquipped(74, MainManager.instance.playerdata[MainManager.battle.currentturn].trueid))
							{
								text3 += "|icon,218|";
								goto IL_148F;
							}
							goto IL_148F;
							IL_13FE:
							text3 += " |size,0.55,0.6|";
							if (MainManager.BadgeIsEquipped(28))
							{
								text3 += "|icon,194|";
							}
						}
						IL_148F:
						text = text2 + MainManager.skilldata[MainManager.listvar[num4], 0] + text3;
					}
					else if ((type >= 10 && type <= 13) || type == 21)
					{
						x3 = -0.5f;
						one = Vector2.one;
						if (type == 21)
						{
							text = MainManager.boardquestdata[Mathf.Abs(MainManager.listvar[num4]), 0];
							float[] array5 = new float[]
							{
								6f,
								6f,
								6f,
								3f,
								6f,
								6f,
								6f,
								6f,
								6f,
								6f,
								6f,
								6f
							};
							if (MainManager.languageid == 4)
							{
								text = "|sizemulti,0.7,1|" + text;
							}
							if (MainManager.listvar[num4] < 0)
							{
								if (Mathf.Abs(MainManager.listvar[num4]) >= 11 && Mathf.Abs(MainManager.listvar[num4]) <= 17)
								{
									MainManager.NewUIObject("icon", spriteRenderer2.transform, new Vector3(-0.3f, 0.2f), Vector3.one * 0.75f, MainManager.guisprites[StartMenu.psprite[Mathf.Abs(MainManager.listvar[num4]) - 11]], 5);
									text = string.Concat(new object[]
									{
										"\t",
										text,
										"|tab,",
										array5[MainManager.languageid],
										"|"
									});
								}
								else
								{
									MainManager.NewUIObject("check", spriteRenderer2.transform, new Vector3(-0.3f, 0.2f), Vector3.one, MainManager.guisprites[113], 5).GetComponent<SpriteRenderer>().color = Color.green;
									text = string.Concat(new object[]
									{
										"\t",
										text,
										"|tab,",
										array5[MainManager.languageid] * 0.85f,
										"|"
									});
								}
							}
							else if (Mathf.Abs(MainManager.listvar[num4]) >= 11 && Mathf.Abs(MainManager.listvar[num4]) <= 17)
							{
								MainManager.NewUIObject("icon", spriteRenderer2.transform, new Vector3(-0.3f, 0.25f), Vector3.one * 0.75f, MainManager.guisprites[StartMenu.psprite[Mathf.Abs(MainManager.listvar[num4]) - 11]], 5).GetComponent<SpriteRenderer>().color = Color.black;
								text = string.Concat(new object[]
								{
									"\t",
									text,
									"|tab,",
									array5[MainManager.languageid],
									"|"
								});
							}
						}
						else
						{
							text = (num4 + 1).ToString().PadLeft(3, '0') + " - ";
							if (num - 10 == 1)
							{
								if (MainManager.instance.librarystuff[1, MainManager.listvar[num4]])
								{
									text += MainManager.enemynames[MainManager.listvar[num4]];
								}
								else
								{
									text += MainManager.menutext[59];
								}
							}
							else if (num - 10 == 2 && MainManager.instance.librarystuff[2, num4])
							{
								text += MainManager.itemdata[0, MainManager.libraryorder[2, num4], 0];
							}
							else if (num - 10 != 2 && MainManager.instance.librarystuff[num - 10, MainManager.libraryorder[num - 10, (num - 10 == 0 || num - 10 == 3) ? num4 : MainManager.listvar[num4]]])
							{
								text += MainManager.librarydata[num - 10, MainManager.libraryorder[num - 10, num4], 0];
							}
							else
							{
								text += MainManager.menutext[59];
							}
							if (MainManager.languageid == 4 && num - 10 == 0)
							{
								text = "|sizemulti,0.7,1|" + text;
							}
						}
						if (MainManager.AsianLang() && MainManager.pausemenu != null)
						{
							text = text.Insert(0, "|size,0.75,y,lock|");
						}
						text = text.Insert(0, "|single|");
					}
					else if (type >= 14 && type <= 16)
					{
						x3 = 0f;
						one = Vector2.one;
						text = MainManager.boardquestdata[MainManager.listvar[num4], 0];
						if (MainManager.pausemenu == null)
						{
							if (MainManager.languageid == 4)
							{
								text = "|size,0.5,0.8,lock|" + text;
							}
							else if (MainManager.AsianLang())
							{
								text = text.Insert(0, "|size,0.6,0.8,lock|");
							}
						}
					}
					else
					{
						if (type == 17)
						{
							if (MainManager.settingsindex[MainManager.listvar[num4]] == 36 && MainManager.pausemenu.calledfrommain)
							{
								text = MainManager.menutext[37];
							}
							else
							{
								text = MainManager.menutext[MainManager.settingsindex[MainManager.listvar[num4]]];
							}
							if (MainManager.settingsindex[MainManager.listvar[num4]] != 35 && MainManager.settingsindex[MainManager.listvar[num4]] != 36 && MainManager.settingsindex[MainManager.listvar[num4]] != 37 && MainManager.settingsindex[MainManager.listvar[num4]] != 231 && MainManager.settingsindex[MainManager.listvar[num4]] != 256)
							{
								float num9 = 3.75f;
								for (int m = 0; m < 2; m++)
								{
									SpriteRenderer component2 = MainManager.NewUIObject("slider" + m, spriteRenderer2.transform, new Vector3(num9, 0f), Vector3.one, MainManager.guisprites[1]).GetComponent<SpriteRenderer>();
									component2.sortingOrder = 10;
									if (m == 0)
									{
										component2.transform.localEulerAngles = new Vector3(0f, 0f, -90f);
									}
									else
									{
										component2.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
									}
									num9 += 5f;
								}
							}
							int num6 = MainManager.settingsindex[MainManager.listvar[num4]];
							int num10;
							if (num6 <= 183)
							{
								if (num6 > 116)
								{
									if (num6 <= 147)
									{
										if (num6 == 140)
										{
											num10 = 38;
											if (MainManager.nowindeffect)
											{
												num10 = 39;
											}
											MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[num10], new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
											goto IL_240F;
										}
										if (num6 != 147)
										{
											goto IL_240F;
										}
									}
									else
									{
										switch (num6)
										{
										case 156:
											break;
										case 157:
											num6 = MainManager.pausemenu.joystick;
											if (num6 != 4)
											{
												if (num6 != 5)
												{
													num10 = 218 + MainManager.pausemenu.joystick;
												}
												else
												{
													num10 = 230;
												}
											}
											else
											{
												num10 = 224;
											}
											MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[num10], new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
											goto IL_240F;
										case 158:
										case 159:
											goto IL_240F;
										case 160:
											goto IL_1BB3;
										default:
											if (num6 != 183)
											{
												goto IL_240F;
											}
											goto IL_20D7;
										}
									}
									num10 = 39;
									if ((MainManager.settingsindex[MainManager.listvar[num4]] == 147 && MainManager.enableoutline == 1) || (MainManager.settingsindex[MainManager.listvar[num4]] == 156 && MainManager.particlelevel == 1))
									{
										num10 = 40;
									}
									else if ((MainManager.settingsindex[MainManager.listvar[num4]] == 147 && MainManager.enableoutline == 2) || (MainManager.settingsindex[MainManager.listvar[num4]] == 156 && MainManager.particlelevel == 2))
									{
										num10 = 41;
									}
									MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[num10], new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
									goto IL_240F;
								}
								switch (num6)
								{
								case 28:
									MainManager.instance.StartCoroutine(MainManager.SetText(string.Concat(new object[]
									{
										"|center||size,0.75|",
										(int)MainManager.resolution[MainManager.pausemenu.resolutionid].x,
										"x",
										(int)MainManager.resolution[MainManager.pausemenu.resolutionid].y
									}), new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
									goto IL_240F;
								case 29:
									goto IL_20D7;
								case 30:
								case 31:
									num10 = 41;
									if ((MainManager.settingsindex[MainManager.listvar[num4]] == 31 && MainManager.pausemenu.lowtex) || (MainManager.settingsindex[MainManager.listvar[num4]] == 30 && MainManager.pausemenu.lowshadow))
									{
										num10 = 40;
									}
									MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[num10], new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
									goto IL_240F;
								case 32:
									MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + ((MainManager.downsample == 0) ? MainManager.menutext[39] : (MainManager.downsamples[MainManager.downsample] * 100f + "%")), new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
									goto IL_240F;
								case 33:
								case 34:
									break;
								default:
									if (num6 == 80)
									{
										num10 = 81;
										if (MainManager.pausemenu.fps == 1)
										{
											num10 = 82;
										}
										else if (MainManager.pausemenu.fps == 2)
										{
											num10 = 107;
										}
										MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[num10], new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
										goto IL_240F;
									}
									if (num6 != 116)
									{
										goto IL_240F;
									}
									num10 = 38;
									if (!MainManager.MainCamera.GetComponent<FXAA>().enabled)
									{
										num10 = 39;
									}
									MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[num10], new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
									goto IL_240F;
								}
								IL_1BB3:
								float num11 = 4.45f;
								for (int n = 0; n < 10; n++)
								{
									SpriteRenderer component3 = MainManager.NewUIObject("pip", spriteRenderer2.transform, new Vector3(num11, 0f), Vector3.one / 4f, MainManager.guisprites[59]).GetComponent<SpriteRenderer>();
									component3.sortingOrder = 10 + n;
									if ((MainManager.settingsindex[MainManager.listvar[num4]] == 33 && Mathf.RoundToInt(MainManager.pausemenu.mvolume * 10f) > n) || (MainManager.settingsindex[MainManager.listvar[num4]] == 34 && Mathf.RoundToInt(MainManager.pausemenu.svolume * 10f) > n) || (MainManager.settingsindex[MainManager.listvar[num4]] == 160 && Mathf.RoundToInt(MainManager.pausemenu.dvolume * 10f) > n))
									{
										component3.sprite = MainManager.guisprites[42];
										component3.color = Color.yellow;
										component3.transform.localScale = Vector3.one / 3f;
									}
									num11 += 0.4f;
								}
							}
							else if (num6 <= 245)
							{
								if (num6 != 222)
								{
									if (num6 != 239)
									{
										if (num6 == 245)
										{
											MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[MainManager.pausemenu.mash ? 247 : 246], new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
										}
									}
									else
									{
										MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[MainManager.keepmusicafterbattle ? 240 : 241], new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
									}
								}
								else if (MainManager.pausemenu.joystick == 4)
								{
									MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[MainManager.precjoystring[MainManager.pausemenu.joystickid]], new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
								}
								else
								{
									string text4 = "";
									if (MainManager.pausemenu.joystickid > -1)
									{
										try
										{
											string text5 = MainManager.Controllers()[MainManager.pausemenu.joystickid].Replace("Controller", "Contr.");
											for (int num12 = 0; num12 < 20; num12++)
											{
												if (num12 < text5.Length)
												{
													text4 += text5[num12].ToString();
												}
											}
										}
										catch
										{
											MainManager.pausemenu.joystickid = -1;
										}
									}
									MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + ((MainManager.pausemenu.joystickid == -1) ? MainManager.menutext[223] : ("|size,0.65,0.75|" + text4)), new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
								}
							}
							else if (num6 <= 261)
							{
								if (num6 == 255)
								{
									goto IL_20D7;
								}
								if (num6 == 261)
								{
									MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[MainManager.pausemenu.monoaudio ? 262 : 263], new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
								}
							}
							else if (num6 != 270)
							{
								if (num6 == 282)
								{
									num10 = 38;
									if (!MainManager.pausemenu.snap)
									{
										num10 = 39;
									}
									MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[num10], new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
								}
							}
							else
							{
								if (MainManager.pausemenu.analog > 2)
								{
									MainManager.pausemenu.analog = 0;
								}
								MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[(new int[]
								{
									39,
									40,
									41
								})[MainManager.pausemenu.analog]], new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
							}
							IL_240F:
							text = text.Insert(0, "|single|");
							goto IL_2EF5;
							IL_20D7:
							num10 = 38;
							if ((MainManager.settingsindex[MainManager.listvar[num4]] == 183 && MainManager.pausemenu.vsyc == 0) || (MainManager.settingsindex[MainManager.listvar[num4]] == 29 && !MainManager.pausemenu.fulls) || (MainManager.settingsindex[MainManager.listvar[num4]] == 255 && !MainManager.pausemenu.pauseunfocus))
							{
								num10 = 39;
							}
							MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[num10], new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
							goto IL_240F;
						}
						if (type == 19)
						{
							text = string.Concat(new object[]
							{
								"|button,",
								num4,
								",0| ",
								(MainManager.languageid == 6 && num4 >= 4 && num4 < 8) ? "|sizemulti,0.7,1|" : "",
								MainManager.menutext[num4 + 88]
							});
						}
						else if (type == 30)
						{
							string text6 = "";
							if (num4 == 0)
							{
								text = MainManager.menutext[254];
								float num13 = 0f;
								for (int num14 = 0; num14 < 6; num14++)
								{
									Transform transform = MainManager.NewUIObject("ic " + num14, spriteRenderer2.transform, new Vector3(num13, 0f), Vector3.one, MainManager.GetButtonSpriteD(num14), spriteRenderer2.sortingOrder + 1).transform;
									if (num4 == 0 || num4 == 5)
									{
										num13 += 0.75f;
									}
									else
									{
										num13 += 0.5f;
									}
								}
							}
							else if (num4 - 1 < 4)
							{
								int num15 = 0;
								int num16 = 0;
								switch (num4)
								{
								case 1:
									num15 = 166;
									num16 = 167;
									break;
								case 2:
									num15 = 164;
									num16 = 165;
									break;
								case 3:
									num15 = 21;
									num16 = 19;
									break;
								case 4:
									num15 = 12;
									num16 = 20;
									break;
								}
								text = string.Concat(new object[]
								{
									"|icon,",
									num15,
									",0.5||icon,",
									num16,
									",0.5||size,0.8,1| ",
									MainManager.menutext[num4 + 249]
								});
								if (MainManager.joybinds[num4 - 1] == -55)
								{
									text6 = "???";
								}
								else
								{
									text6 = MainManager.menutext[255] + " " + Mathf.Abs(MainManager.joybinds[num4 - 1]);
								}
							}
							else
							{
								int num17 = 13;
								if (MainManager.pausemenu.joystickid == 1)
								{
									num17 = 86;
								}
								else if (MainManager.pausemenu.joystickid == 2)
								{
									num17 = 127;
								}
								text = string.Concat(new object[]
								{
									"|icon,",
									num17 + num4 - 5,
									",0.5||size,0.6,1| ",
									MainManager.menutext[num4 + 87]
								});
								if (MainManager.joybinds[num4 - 1] == -55)
								{
									text6 = "???";
								}
								else
								{
									text6 = MainManager.menutext[256] + " " + Mathf.Abs(MainManager.joybinds[num4 - 1]);
								}
							}
							if (text6.Length > 0)
							{
								MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + text6, new Vector3(6.25f, -0.15f), spriteRenderer2.transform));
							}
						}
						else if (type == 20)
						{
							one = Vector2.one;
							text = "|sort,20||color,4|" + MainManager.languagenames[MainManager.listvar[num4]];
						}
						else if (type == 18)
						{
							x3 = -2.65f;
							if (MainManager.listvar[num4] == -1)
							{
								text = "|color,1|" + MainManager.commondialogue[152];
							}
							else
							{
								int[] array6 = MainManager.instance.samiramusics[num4 + (MainManager.listvar.Contains(-1) ? -1 : 0)];
								if (array6[1] == -1)
								{
									text = MainManager.musicnames[array6[0]];
								}
								else
								{
									text = "|color,3|" + MainManager.musicnames[array6[0]];
								}
							}
						}
						else if (type == 33 || type == 34)
						{
							if (MainManager.listvar[num4] > -1)
							{
								bool flag3 = type == 34 && MainManager.instance.flags[681];
								text = "|single|" + (flag3 ? MainManager.menutext[59] : MainManager.badgedata[MainManager.listvar[num4], 0]);
								SpriteRenderer spriteRenderer5 = new GameObject("itemsprite").AddComponent<SpriteRenderer>();
								spriteRenderer5.sprite = (flag3 ? MainManager.guisprites[190] : MainManager.itemsprites[1, MainManager.listvar[num4]]);
								spriteRenderer5.transform.parent = spriteRenderer2.transform;
								spriteRenderer5.gameObject.layer = 5;
								y = -0.2f;
								spriteRenderer5.transform.parent = spriteRenderer2.transform;
								spriteRenderer5.transform.localPosition = new Vector2(-2.5f, 0f);
								spriteRenderer5.gameObject.layer = 5;
								spriteRenderer5.transform.localScale = new Vector3(x2, 0.6f, 1f);
							}
						}
						else if (type == 3)
						{
							if (MainManager.listvar.Length == 0 || MainManager.listvar[num4] > -1)
							{
								int num18 = MainManager.instance.badges[MainManager.listvar[num4]][0];
								text = "|single|" + MainManager.badgedata[num18, 0];
								SpriteRenderer spriteRenderer6 = new GameObject("itemsprite").AddComponent<SpriteRenderer>();
								spriteRenderer6.sprite = MainManager.itemsprites[1, num18];
								spriteRenderer6.transform.parent = spriteRenderer2.transform;
								spriteRenderer6.gameObject.layer = 5;
								if (MainManager.instance.pause)
								{
									x3 = 0.35f;
									num3 -= 0.2f;
									y = -0.3f;
									spriteRenderer6.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
									spriteRenderer6.transform.localPosition = new Vector2(-0.25f, 0f);
									one = Vector2.one;
									if (MainManager.instance.badges[MainManager.listvar[num4]][1] > -2)
									{
										SpriteRenderer spriteRenderer7 = new GameObject("Bar" + num4).AddComponent<SpriteRenderer>();
										spriteRenderer7.sprite = MainManager.guisprites[0];
										spriteRenderer7.gameObject.layer = 5;
										spriteRenderer7.transform.parent = spriteRenderer2.transform;
										spriteRenderer7.sortingOrder = -1;
										spriteRenderer7.transform.localPosition = new Vector2(4.3f, 0f);
										spriteRenderer7.transform.localScale = new Vector3(1.8f, 1f, 1f);
										spriteRenderer7.color = new Color(1f, 1f, 1f, 0.5f);
										if (MainManager.instance.badges[MainManager.listvar[num4]][1] == -1)
										{
											spriteRenderer7.color = MainManager.instance.menucolors[3];
										}
										else
										{
											int num19 = MainManager.instance.badges[MainManager.listvar[num4]][1];
											MainManager.NewUIObject("charicon", spriteRenderer7.transform, new Vector3(1.2f, 0f), new Vector3(0.225f, 0.4f, 1f), MainManager.guisprites[5 + num19], 10).GetComponent<SpriteRenderer>();
											Color color = MainManager.instance.charcolor[num19];
											if (num19 == 1)
											{
												color = new Color(color.r - 0.1f, color.g - 0.1f, color.b - 0.1f);
											}
											spriteRenderer7.color = color;
										}
									}
								}
								else
								{
									y = -0.2f;
									spriteRenderer6.transform.parent = spriteRenderer2.transform;
									spriteRenderer6.transform.localPosition = new Vector2(-2.5f, 0f);
									spriteRenderer6.gameObject.layer = 5;
									spriteRenderer6.transform.localScale = new Vector3(x2, 0.6f, 1f);
								}
								if (!sell)
								{
									MainManager.instance.StartCoroutine(MainManager.SetText("|font,0|" + Mathf.Clamp(Convert.ToInt32(MainManager.badgedata[num18, 2]), 0, MainManager.instance.flags[613] ? 1 : 999), 0, null, false, false, new Vector3(7.5f, -0.2f), Vector3.zero, one, spriteRenderer2.transform, null));
									MainManager.NewUIObject("mpicon", spriteRenderer2.transform, new Vector3(8.8f, 0f), Vector3.one * 0.4f, MainManager.guisprites[61], 20);
								}
							}
							else
							{
								text = "|color,1|" + MainManager.menutext[23];
							}
						}
						else if (type == 9)
						{
							x3 = -2.6f;
							int[] array7 = new int[]
							{
								1,
								271,
								2,
								0,
								3
							};
							if ((num4 == 4 && !MainManager.battle.canflee) || ((num4 == 0 || num4 == 1) && !MainManager.AllPartyFree() && MainManager.GetAlivePlayerAmmount() > 1) || (num4 == 2 && MainManager.battle.disablespy))
							{
								text = "|color,1|" + MainManager.menutext[(array7[num4] > 3) ? array7[num4] : (65 + array7[num4])];
							}
							else
							{
								text = MainManager.menutext[(array7[num4] > 3) ? array7[num4] : (65 + array7[num4])];
								if (num4 == 0)
								{
									text += " |size,0.55,0.6||button,6|";
								}
								if (num4 == 3 && !MainManager.instance.playerdata[MainManager.battle.currentturn].didnothing)
								{
									text.Insert(0, "|single|");
									text += " |size,0.55,0.6|";
									if (MainManager.BadgeIsEquipped(62, MainManager.instance.playerdata[MainManager.battle.currentturn].trueid))
									{
										text += "|icon,187|";
									}
									if (MainManager.BadgeIsEquipped(56, MainManager.instance.playerdata[MainManager.battle.currentturn].trueid))
									{
										text += "|icon,188|";
									}
									if (MainManager.BadgeIsEquipped(61, MainManager.instance.playerdata[MainManager.battle.currentturn].trueid))
									{
										text += "|icon,189|";
									}
								}
								if (num4 == 4)
								{
									text.Insert(0, "|single|");
									text += " |size,0.55,0.6|";
									if (MainManager.BadgeIsEquipped(48))
									{
										text += "|icon,221|";
									}
									if (MainManager.BadgeIsEquipped(5))
									{
										text += "|icon,222|";
									}
								}
								if (num4 == 2 && MainManager.BadgeIsEquipped(17))
								{
									text.Insert(0, "|single|");
									text += " |size,0.55,0.6||icon,219|";
								}
							}
						}
					}
				}
				IL_2EF5:
				MainManager.instance.StartCoroutine(MainManager.SetText((MainManager.instance.pause ? ((type == 3 || type == 32) ? "  " : "") : "|size,0.6,0.8|") + text, 0, null, false, false, new Vector3(x3, y), Vector3.zero, one, spriteRenderer2.transform, null));
				num4++;
			}
			if (MainManager.instance.maxoptions > MainManager.listammount && MainManager.instance.option < MainManager.instance.maxoptions - 1 && MainManager.listlow < MainManager.instance.maxoptions - MainManager.listammount)
			{
				if (!MainManager.instance.pause && (type <= 16 || type == 18 || type >= 22 || type == 20))
				{
					SpriteRenderer spriteRenderer8 = new GameObject("DownArrow").AddComponent<SpriteRenderer>();
					spriteRenderer8.gameObject.layer = 5;
					spriteRenderer8.transform.parent = MainManager.instance.itemlist;
					if (type >= 14 && type <= 16)
					{
						spriteRenderer8.sprite = MainManager.guisprites[1];
						spriteRenderer8.transform.localPosition = new Vector2(8.8f, num3 + 0.5f);
					}
					else
					{
						spriteRenderer8.transform.localPosition = new Vector2(1f, num3);
						spriteRenderer8.sprite = MainManager.guisprites[3];
					}
					spriteRenderer8.sortingOrder = 3;
					if (type == 20)
					{
						spriteRenderer8.sortingOrder = 10;
					}
				}
				else if (MainManager.pausemenu != null)
				{
					SpriteRenderer component4 = MainManager.NewUIObject("downarrow", null, Vector3.zero, Vector3.one, MainManager.guisprites[1]).GetComponent<SpriteRenderer>();
					component4.transform.parent = MainManager.instance.itemlist;
					component4.transform.localEulerAngles = default(Vector3);
					float x4 = 11f;
					if (MainManager.pausemenu.windowid == 3)
					{
						x4 = 9.5f;
					}
					else if (MainManager.pausemenu.windowid == 4)
					{
						x4 = 11.25f;
					}
					else if (MainManager.pausemenu.windowid == 5)
					{
						x4 = 8.5f;
					}
					else if (MainManager.pausemenu.windowid == 1)
					{
						x4 = 7f;
					}
					component4.transform.localPosition = new Vector2(x4, num3 + 0.5f);
					component4.transform.localScale = Vector3.one * 1.25f;
				}
			}
		}
		if (showdescription && MainManager.listvar.Length != 0)
		{
			string text7 = "";
			string text8 = "";
			if (MainManager.listdescbox != null)
			{
				Object.Destroy(MainManager.listdescbox.gameObject);
			}
			if (type < 14 || type > 16)
			{
				bool flag4 = false;
				if ((type >= 0 && type < 9) || type == 33 || type == 34)
				{
					if (MainManager.listvar[MainManager.instance.option] > -1)
					{
						if (type == 33 || type == 34)
						{
							text7 = MainManager.badgedata[MainManager.listvar[MainManager.instance.option], 1];
							text8 = MainManager.badgedata[MainManager.listvar[MainManager.instance.option], 0];
						}
						else if (type == 3)
						{
							int num20 = MainManager.instance.badges[MainManager.listvar[MainManager.instance.option]][0];
							text7 = MainManager.badgedata[num20, 1];
							text8 = MainManager.badgedata[num20, 0];
						}
						else if (type == 33)
						{
							text7 = MainManager.badgedata[MainManager.listvar[MainManager.instance.option], 1];
							text8 = MainManager.badgedata[MainManager.listvar[MainManager.instance.option], 0];
						}
						else
						{
							text7 = MainManager.itemdata[num, MainManager.listvar[MainManager.instance.option], 2];
							text8 = MainManager.itemdata[num, MainManager.listvar[MainManager.instance.option], 0];
						}
					}
					else
					{
						flag4 = true;
					}
				}
				else if (type >= -3 && type <= -1)
				{
					text7 = MainManager.skilldata[MainManager.listvar[MainManager.instance.option], 1];
				}
				else if (type == 9)
				{
					int[] array8 = new int[]
					{
						1,
						272,
						2,
						0,
						3
					};
					text7 = MainManager.menutext[(array8[MainManager.instance.option] > 3) ? array8[MainManager.instance.option] : (69 + array8[MainManager.instance.option])];
				}
				else if (type == 17)
				{
					text7 = MainManager.menutext[0];
				}
				else if (type == 26)
				{
					if (MainManager.termacadeprize[MainManager.listvar[MainManager.instance.option], 0] == 2)
					{
						if (MainManager.instance.flags[681])
						{
							text7 = MainManager.menutext[59];
						}
						else
						{
							text7 = MainManager.badgedata[MainManager.termacadeprize[MainManager.listvar[MainManager.instance.option], 1], 1];
						}
					}
					else
					{
						text7 = MainManager.itemdata[0, MainManager.termacadeprize[MainManager.listvar[MainManager.instance.option], 1], 2];
					}
				}
				if (!flag4)
				{
					Transform transform2 = MainManager.Create9Box(Vector3.zero, new Vector2(11f, 3f), 0, -3, Color.white, true);
					transform2.parent = MainManager.instance.itemlist;
					transform2.transform.localEulerAngles = Vector3.zero;
					transform2.transform.localPosition = new Vector3(-7.5f, -4.25f);
					if (sell)
					{
						MainManager.instance.showmoney = 10f;
						MainManager.instance.flagvar[10] = Mathf.Clamp(Mathf.FloorToInt((float)(Convert.ToInt32(MainManager.itemdata[MainManager.listtype, MainManager.listvar[MainManager.instance.option], 4]) / 2)), 1, 999);
						text7 = string.Concat(new string[]
						{
							text8,
							" - ",
							MainManager.menutext[49],
							"|line|",
							text7
						});
					}
					MainManager.instance.StartCoroutine(MainManager.SetText(string.Concat(new object[]
					{
						"|single||singlebreak,",
						MainManager.itemdescbreak,
						"|",
						text7
					}), 0, null, false, false, new Vector3(-5.25f, 0.6f), Vector3.zero, new Vector2(0.75f, 0.75f), transform2, null));
					MainManager.listdescbox = transform2;
				}
			}
			else
			{
				MainManager.listdescbox = MainManager.NewChild("descbox", MainManager.instance.itemlist);
				if (MainManager.listvar[MainManager.instance.option] != 0)
				{
					MainManager.NewUIObject("Image", MainManager.listdescbox, new Vector3(10.5f, 0f), Vector3.one * 2f, MainManager.librarysprites[Convert.ToInt32(MainManager.boardquestdata[MainManager.listvar[MainManager.instance.option], 4])], 10).tag = "Text";
					MainManager.instance.StartCoroutine(MainManager.SetText(string.Concat(new string[]
					{
						"|size,0.75||sort,1|",
						MainManager.menutext[104],
						" ",
						MainManager.boardquestdata[MainManager.listvar[MainManager.instance.option], 2],
						"|line||halfline|",
						MainManager.menutext[105],
						" |stars,",
						MainManager.boardquestdata[MainManager.listvar[MainManager.instance.option], 5],
						"|"
					}), new Vector3(12f, 0.35f), MainManager.listdescbox));
				}
				MainManager.instance.StartCoroutine(MainManager.SetText("|sort,1||single|" + ((MainManager.languageid == 4) ? "|singlebreak,10||sizemulti,0.8,1|" : "|singlebreak,6|") + MainManager.boardquestdata[MainManager.listvar[MainManager.instance.option], 1].Split(new char[]
				{
					'}'
				})[0].Split(new char[]
				{
					'{'
				})[0], 0, null, false, false, new Vector3(9.9f, -1.75f), Vector3.zero, new Vector2(0.65f, 0.75f), MainManager.listdescbox, null));
			}
		}
		MainManager.instance.itemlist.localEulerAngles = Vector3.zero;
	}

	// Token: 0x060005EC RID: 1516 RVA: 0x0003F8A4 File Offset: 0x0003DAA4
	public static Vector3 ChildScale(Vector3 scale, Transform parent, bool swapZY)
	{
		if (swapZY)
		{
			return new Vector3(scale.x / parent.localScale.x, scale.y / parent.localScale.z, scale.z / parent.localScale.y);
		}
		return new Vector3(scale.x / parent.localScale.x, scale.y / parent.localScale.y, scale.z / parent.localScale.z);
	}

	// Token: 0x060005ED RID: 1517 RVA: 0x0003F92B File Offset: 0x0003DB2B
	public static Transform NewChild(string name, Transform parent)
	{
		Transform transform = new GameObject(name).transform;
		transform.parent = parent;
		transform.localPosition = Vector3.zero;
		transform.localEulerAngles = Vector3.zero;
		transform.localScale = Vector3.one;
		return transform;
	}

	// Token: 0x060005EE RID: 1518 RVA: 0x0003F960 File Offset: 0x0003DB60
	public static bool HasQuest(int questid)
	{
		for (int i = 0; i < MainManager.instance.boardquests.Length; i++)
		{
			if (MainManager.instance.boardquests[i].Contains(questid))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060005EF RID: 1519 RVA: 0x0003F99C File Offset: 0x0003DB9C
	public static int GetFreePlayerAmmount()
	{
		int num = 0;
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].cantmove <= 0 && MainManager.instance.playerdata[i].hp > 0 && MainManager.HasCondition(MainManager.BattleCondition.Sleep, MainManager.instance.playerdata[i]) <= 0 && MainManager.HasCondition(MainManager.BattleCondition.Numb, MainManager.instance.playerdata[i]) <= 0 && MainManager.HasCondition(MainManager.BattleCondition.Freeze, MainManager.instance.playerdata[i]) <= 0 && MainManager.HasCondition(MainManager.BattleCondition.EventStop, MainManager.instance.playerdata[i]) <= 0 && MainManager.HasCondition(MainManager.BattleCondition.Eaten, MainManager.instance.playerdata[i]) <= 0)
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x060005F0 RID: 1520 RVA: 0x0003FA84 File Offset: 0x0003DC84
	public static Vector3 CardinalSnap(Vector3 angle, bool cameratied)
	{
		Vector3 vector = Vector3.zero;
		if (angle.y > 45f && angle.y < 135f)
		{
			vector = Vector3.left;
		}
		else if (angle.y >= 135f && angle.y < 225f)
		{
			vector = Vector3.forward;
		}
		else if (angle.y >= 225f && angle.y < 315f)
		{
			vector = Vector3.right;
		}
		else
		{
			vector = Vector3.back;
		}
		if (cameratied)
		{
			return (vector + MainManager.instance.globalcamdir.forward.normalized).normalized;
		}
		return vector;
	}

	// Token: 0x060005F1 RID: 1521 RVA: 0x0003FB30 File Offset: 0x0003DD30
	public static Vector3 CardinalSnap8(Vector3 angle, bool cameratied)
	{
		Vector3 a = Vector3.zero;
		if (angle.y > 22.5f && angle.y < 67.5f)
		{
			a = Vector3.left + Vector3.back;
		}
		else if (angle.y >= 67.5f && angle.y < 112.5f)
		{
			a = Vector3.left;
		}
		else if (angle.y >= 122.5f && angle.y < 157.5f)
		{
			a = Vector3.left + Vector3.forward;
		}
		else if (angle.y >= 157.5f && angle.y < 202.5f)
		{
			a = Vector3.forward;
		}
		else if (angle.y >= 202.5f && angle.y < 247.5f)
		{
			a = Vector3.right + Vector3.forward;
		}
		else if (angle.y >= 247.5f && angle.y < 292.5f)
		{
			a = Vector3.right;
		}
		else if (angle.y >= 292.5f && angle.y < 337.5f)
		{
			a = Vector3.right + Vector3.back;
		}
		else
		{
			a = Vector3.back;
		}
		if (cameratied)
		{
			return (a + MainManager.instance.globalcamdir.forward.normalized).normalized;
		}
		return a.normalized;
	}

	// Token: 0x060005F2 RID: 1522 RVA: 0x0003FC9B File Offset: 0x0003DE9B
	public static float ClampToMinMax(float v, float min, float max)
	{
		return MainManager.ClampToMinMax(v, min, max, false);
	}

	// Token: 0x060005F3 RID: 1523 RVA: 0x0003FCA8 File Offset: 0x0003DEA8
	public static float ClampToMinMax(float v, float min, float max, bool lowest)
	{
		float num = Mathf.Lerp(min, max, 0.5f);
		if (lowest)
		{
			if (v < num)
			{
				return min;
			}
			return max;
		}
		else
		{
			if (v < num)
			{
				return max;
			}
			return min;
		}
	}

	// Token: 0x060005F4 RID: 1524 RVA: 0x0003FCD4 File Offset: 0x0003DED4
	public static bool InBetween(float v, float a, float b)
	{
		return v >= a && v < b;
	}

	// Token: 0x060005F5 RID: 1525 RVA: 0x0003FCE0 File Offset: 0x0003DEE0
	public static Vector3 CardinalSnap(Vector3 obj, int directions)
	{
		float num = 360f / (float)directions;
		return new Vector3(obj.x, (float)Mathf.RoundToInt(obj.y / num) * num, obj.z);
	}

	// Token: 0x060005F6 RID: 1526 RVA: 0x0003FD17 File Offset: 0x0003DF17
	public static void LookAt(Transform obj, Vector3 targetp)
	{
		obj.LookAt(targetp);
		obj.localEulerAngles = new Vector3(0f, obj.localEulerAngles.y, 0f);
	}

	// Token: 0x060005F7 RID: 1527 RVA: 0x0003FD40 File Offset: 0x0003DF40
	public static void LookAt(Transform obj, Vector3 targetp, bool keepangle)
	{
		if (!keepangle)
		{
			MainManager.LookAt(obj, targetp);
		}
		Vector3 localEulerAngles = obj.localEulerAngles;
		obj.LookAt(targetp);
		obj.localEulerAngles = new Vector3(localEulerAngles.x, obj.localEulerAngles.y, localEulerAngles.z);
	}

	// Token: 0x060005F8 RID: 1528 RVA: 0x0003FD87 File Offset: 0x0003DF87
	public static Vector3 CardinalSnap(Vector3 angle)
	{
		return MainManager.CardinalSnap(angle, false);
	}

	// Token: 0x060005F9 RID: 1529 RVA: 0x0003FD90 File Offset: 0x0003DF90
	public static float CardinalSnap(float angle)
	{
		return MainManager.CardinalSnap(angle, 4);
	}

	// Token: 0x060005FA RID: 1530 RVA: 0x0003FD99 File Offset: 0x0003DF99
	public static Vector3 CardinalSnap(Transform obj, int directions)
	{
		return MainManager.CardinalSnap(obj.transform.eulerAngles, directions);
	}

	// Token: 0x060005FB RID: 1531 RVA: 0x0003FDAC File Offset: 0x0003DFAC
	public static Vector3 CardinalSnap(Transform obj)
	{
		return MainManager.CardinalSnap(obj, 4);
	}

	// Token: 0x060005FC RID: 1532 RVA: 0x0003FDB8 File Offset: 0x0003DFB8
	public static float CardinalSnap(float angle, int directions)
	{
		float num = (float)(360 / directions);
		return (float)Mathf.RoundToInt(angle / num) * num;
	}

	// Token: 0x060005FD RID: 1533 RVA: 0x0003FDDC File Offset: 0x0003DFDC
	public static int GetFreePlayerAmmount(bool hponly)
	{
		if (hponly)
		{
			int num = 0;
			for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
			{
				if (MainManager.instance.playerdata[i].hp > 0)
				{
					num++;
				}
			}
			return num;
		}
		return MainManager.GetFreePlayerAmmount();
	}

	// Token: 0x060005FE RID: 1534 RVA: 0x0003FE28 File Offset: 0x0003E028
	public static int GetAlivePlayerAmmount()
	{
		int num = 0;
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].hp > 0 && MainManager.instance.playerdata[i].eatenby == null)
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x060005FF RID: 1535 RVA: 0x0003FE88 File Offset: 0x0003E088
	public static void ApplySettings()
	{
		if (MainManager.music != null)
		{
			for (int i = 0; i < MainManager.music.Length; i++)
			{
				if (MainManager.instance.inmusicrange == -1)
				{
					MainManager.music[i].volume = MainManager.musicvolume;
				}
			}
		}
		if (MainManager.sounds != null)
		{
			for (int j = 0; j < MainManager.sounds.Length; j++)
			{
				MainManager.sounds[j].volume = MainManager.soundvolume;
			}
		}
		QualitySettings.vSyncCount = ((MainManager.vsync != 0) ? Mathf.Clamp(Mathf.FloorToInt((float)Screen.currentResolution.refreshRate / 60f), 1, 4) : 0);
		if (MainManager.fps == 0)
		{
			Application.targetFrameRate = 30;
		}
		else if (MainManager.fps == 1)
		{
			Application.targetFrameRate = 60;
		}
		else if (MainManager.fps == 2)
		{
			Application.targetFrameRate = -1;
		}
		if (MainManager.lowshadows)
		{
			QualitySettings.shadowResolution = ShadowResolution.Low;
		}
		else
		{
			QualitySettings.shadowResolution = ShadowResolution.High;
		}
		if (MainManager.lowtexture)
		{
			QualitySettings.masterTextureLimit = 1;
		}
		else
		{
			QualitySettings.masterTextureLimit = 0;
		}
		if (MainManager.usejoystick == 5)
		{
			MainManager.joyid = MainManager.forcejoystick;
		}
		else if ((MainManager.forcejoystick > -1 && MainManager.usejoystick == 3) || (MainManager.usejoystick > 0 && MainManager.joystick))
		{
			InputIO.GetJoyButtons();
			MainManager.forcecontrollerupdate = true;
		}
		if (AudioSettings.speakerMode != (MainManager.monoaudio ? AudioSpeakerMode.Mono : AudioSpeakerMode.Stereo))
		{
			string text = (MainManager.music[0].clip != null && MainManager.music[0].isPlaying) ? MainManager.music[0].clip.name : null;
			float time = (text != null) ? MainManager.music[0].time : 0f;
			AudioSettings.speakerMode = (MainManager.monoaudio ? AudioSpeakerMode.Mono : AudioSpeakerMode.Stereo);
			if (text != null)
			{
				MainManager.ChangeMusic(text);
				MainManager.music[0].Play();
				MainManager.music[0].time = time;
			}
		}
		if (!InputIO.IsConsole)
		{
			Application.runInBackground = !MainManager.pauseonfocus;
			if (Application.isMobilePlatform)
			{
				Screen.SetResolution(Screen.resolutions[0].width, Screen.resolutions[0].height, false);
			}
			else
			{
				Screen.SetResolution((int)MainManager.resolution[MainManager.resolutionindex].x, (int)MainManager.resolution[MainManager.resolutionindex].y, MainManager.fullscreen);
			}
		}
		else
		{
			Application.runInBackground = false;
		}
		if (MainManager.map != null)
		{
			MainManager.map.RefreshSoundVolume();
		}
		if (MainManager.analog > 2)
		{
			MainManager.analog = 0;
		}
	}

	// Token: 0x06000600 RID: 1536 RVA: 0x000400F8 File Offset: 0x0003E2F8
	public static bool AllPartyFree()
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].cantmove > 0)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000601 RID: 1537 RVA: 0x00040137 File Offset: 0x0003E337
	public static int BadgeHowManyEquipped(int id)
	{
		return MainManager.BadgeHowManyEquipped(id, -1);
	}

	// Token: 0x06000602 RID: 1538 RVA: 0x00040140 File Offset: 0x0003E340
	public static int BadgeHowManyEquipped(int id, int playerid)
	{
		int num = 0;
		for (int i = 0; i < MainManager.instance.badges.Count; i++)
		{
			if (MainManager.instance.badges[i][0] == id && MainManager.instance.badges[i][1] > -2 && playerid == MainManager.instance.badges[i][1])
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x06000603 RID: 1539 RVA: 0x000401AE File Offset: 0x0003E3AE
	public static bool BadgeIsEquipped(int id)
	{
		return MainManager.BadgeIsEquipped(id, -1);
	}

	// Token: 0x06000604 RID: 1540 RVA: 0x000401B8 File Offset: 0x0003E3B8
	public static bool CheckActiveEntities(int[] ids)
	{
		for (int i = 0; i < ids.Length; i++)
		{
			if (ids[i] >= 0)
			{
				if (!MainManager.map.entities[ids[i]].npcdata.hit)
				{
					return false;
				}
			}
			else if (MainManager.map.entities[Mathf.Abs(ids[i])].npcdata.hit)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000605 RID: 1541 RVA: 0x00040218 File Offset: 0x0003E418
	public static bool BadgeIsEquipped(int id, int playerid)
	{
		bool result = false;
		for (int i = 0; i < MainManager.instance.badges.Count; i++)
		{
			if (MainManager.instance.badges[i][0] == id && MainManager.instance.badges[i][1] > -2)
			{
				if (playerid == -1)
				{
					return true;
				}
				if (playerid == MainManager.instance.badges[i][1])
				{
					result = true;
				}
			}
		}
		return result;
	}

	// Token: 0x06000606 RID: 1542 RVA: 0x0004028C File Offset: 0x0003E48C
	public static void ReloadSave()
	{
		MainManager.StopSound("CrowdChatter");
		MainManager.FadeMusic(0.1f);
		if (MainManager.battle != null)
		{
			MainManager.battle.StopAllCoroutines();
			Object.Destroy(MainManager.battle.battlemap.gameObject);
		}
		MainManager.events.StopAllCoroutines();
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (MainManager.instance.playerdata[i].entity != null)
			{
				Object.Destroy(MainManager.instance.playerdata[i].entity.gameObject);
			}
		}
		RenderSettings.skybox = null;
		if (MainManager.map != null)
		{
			Object.Destroy(MainManager.map.gameObject);
		}
		MainManager.events.StartEvent(22, null);
	}

	// Token: 0x06000607 RID: 1543 RVA: 0x00040364 File Offset: 0x0003E564
	public static void ApplyBadges()
	{
		MainManager.instance.speedup = true;
		MainManager.instance.maxtp = MainManager.instance.basetp;
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			MainManager.ResetPlayerStat(i);
		}
		int[][] array = MainManager.instance.badges.ToArray();
		for (int j = 0; j < array.GetLength(0); j++)
		{
			if (array[j][1] > -2)
			{
				string[] array2 = MainManager.badgedata[array[j][0], 4].Split(new char[]
				{
					';'
				});
				for (int k = 0; k < array2.Length; k++)
				{
					string[] array3 = array2[k].Split(new char[]
					{
						','
					});
					if (array[j][1] > -1)
					{
						for (int l = 0; l < MainManager.instance.playerdata.Length; l++)
						{
							if (MainManager.instance.playerdata[l].trueid == array[j][1])
							{
								switch ((MainManager.BadgeEffects)Enum.Parse(typeof(MainManager.BadgeEffects), array3[0]))
								{
								case MainManager.BadgeEffects.HPUP:
								{
									MainManager.BattleData[] array4 = MainManager.instance.playerdata;
									int num = l;
									array4[num].maxhp = array4[num].maxhp + Convert.ToInt32(array3[1]);
									break;
								}
								case MainManager.BadgeEffects.AttackUp:
								{
									MainManager.BattleData[] array5 = MainManager.instance.playerdata;
									int num2 = l;
									array5[num2].atk = array5[num2].atk + Convert.ToInt32(array3[1]);
									break;
								}
								case MainManager.BadgeEffects.DefenseUp:
								{
									MainManager.BattleData[] array6 = MainManager.instance.playerdata;
									int num3 = l;
									array6[num3].def = array6[num3].def + Convert.ToInt32(array3[1]);
									break;
								}
								case MainManager.BadgeEffects.LockSkills:
									MainManager.instance.playerdata[l].lockskills = true;
									break;
								case MainManager.BadgeEffects.PoisonRes:
								{
									MainManager.BattleData[] array7 = MainManager.instance.playerdata;
									int num4 = l;
									array7[num4].poisonres = array7[num4].poisonres + Convert.ToInt32(array3[1]);
									break;
								}
								case MainManager.BadgeEffects.SleepRes:
								{
									MainManager.BattleData[] array8 = MainManager.instance.playerdata;
									int num5 = l;
									array8[num5].sleepres = array8[num5].sleepres + Convert.ToInt32(array3[1]);
									break;
								}
								case MainManager.BadgeEffects.NumbRes:
								{
									MainManager.BattleData[] array9 = MainManager.instance.playerdata;
									int num6 = l;
									array9[num6].numbres = array9[num6].numbres + Convert.ToInt32(array3[1]);
									break;
								}
								case MainManager.BadgeEffects.FreezeRes:
								{
									MainManager.BattleData[] array10 = MainManager.instance.playerdata;
									int num7 = l;
									array10[num7].freezeres = array10[num7].freezeres + Convert.ToInt32(array3[1]);
									break;
								}
								case MainManager.BadgeEffects.AttackMultiply:
								{
									MainManager.BattleData[] array11 = MainManager.instance.playerdata;
									int num8 = l;
									array11[num8].atk = array11[num8].atk * Mathf.FloorToInt(Convert.ToSingle(array3[1]));
									break;
								}
								case MainManager.BadgeEffects.LockItems:
									MainManager.instance.playerdata[l].lockitems = true;
									break;
								case MainManager.BadgeEffects.LockRelay:
									MainManager.instance.playerdata[l].locktri = true;
									break;
								case MainManager.BadgeEffects.LockRelayPass:
									MainManager.instance.playerdata[l].lockrelayreceive = true;
									break;
								}
							}
						}
					}
					else
					{
						MainManager.BadgeEffects badgeEffects = (MainManager.BadgeEffects)Enum.Parse(typeof(MainManager.BadgeEffects), array3[0]);
						if (badgeEffects != MainManager.BadgeEffects.TPUP)
						{
							if (badgeEffects != MainManager.BadgeEffects.AttackUp)
							{
								if (badgeEffects == MainManager.BadgeEffects.SpeedUp)
								{
									MainManager.instance.speedup = true;
								}
							}
							else
							{
								for (int m = 0; m < MainManager.instance.playerdata.Length; m++)
								{
									MainManager.BattleData[] array12 = MainManager.instance.playerdata;
									int num9 = m;
									array12[num9].atk = array12[num9].atk + Convert.ToInt32(array3[1]);
								}
							}
						}
						else
						{
							MainManager.instance.maxtp += Convert.ToInt32(array3[1]);
						}
					}
				}
			}
		}
		MainManager.instance.tp = Mathf.Clamp(MainManager.instance.tp, 0, MainManager.instance.maxtp);
		for (int n = 0; n < MainManager.instance.playerdata.Length; n++)
		{
			MainManager.instance.playerdata[n].hp = Mathf.Clamp(MainManager.instance.playerdata[n].hp, 0, MainManager.instance.playerdata[n].maxhp);
		}
	}

	// Token: 0x06000608 RID: 1544 RVA: 0x000407C8 File Offset: 0x0003E9C8
	public static GameObject CreateRock(Vector3 pos, Vector3 size, Vector3 rotation)
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Objects/CrackedRock")) as GameObject;
		gameObject.transform.position = pos;
		gameObject.transform.localEulerAngles = rotation;
		gameObject.transform.localScale = size;
		Object.Destroy(gameObject.GetComponent<MeshCollider>());
		Object.Destroy(gameObject.GetComponent<Fader>());
		return gameObject;
	}

	// Token: 0x06000609 RID: 1545 RVA: 0x00040824 File Offset: 0x0003EA24
	public static void CrackRock(Transform parent, bool destroyparent)
	{
		(Object.Instantiate(Resources.Load("Prefabs/Objects/CrackRockBreak"), parent.position, Quaternion.Euler(parent.eulerAngles)) as GameObject).transform.localScale = parent.localScale;
		if (destroyparent)
		{
			Object.Destroy(parent.gameObject);
		}
	}

	// Token: 0x0600060A RID: 1546 RVA: 0x00040874 File Offset: 0x0003EA74
	public static void AddBadge(int id)
	{
		MainManager.instance.badges.Add(new int[]
		{
			id,
			-2
		});
	}

	// Token: 0x0600060B RID: 1547 RVA: 0x00040894 File Offset: 0x0003EA94
	public static void SetUpList(int type, bool showdescription, bool sell)
	{
		MainManager.storeid = 0;
		MainManager.listtype = type;
		MainManager.listammount = 6;
		MainManager.listdesc = showdescription;
		MainManager.listsell = sell;
		MainManager.instance.inputcooldown = 5f;
	}

	// Token: 0x0600060C RID: 1548 RVA: 0x000408C4 File Offset: 0x0003EAC4
	private static void ResetPlayerStat(int id)
	{
		MainManager.instance.playerdata[id].lockitems = false;
		MainManager.instance.playerdata[id].lockskills = false;
		MainManager.instance.playerdata[id].locktri = false;
		MainManager.instance.playerdata[id].lockrelayreceive = false;
		MainManager.instance.playerdata[id].maxhp = MainManager.instance.playerdata[id].basehp;
		MainManager.instance.playerdata[id].atk = MainManager.instance.playerdata[id].baseatk;
		MainManager.instance.playerdata[id].def = MainManager.instance.playerdata[id].basedef;
		MainManager.instance.playerdata[id].poisonres = 0;
		MainManager.instance.playerdata[id].sleepres = 0;
		MainManager.instance.playerdata[id].freezeres = 0;
		MainManager.instance.playerdata[id].numbres = 0;
	}

	// Token: 0x0600060D RID: 1549 RVA: 0x000409FF File Offset: 0x0003EBFF
	public static void ResetList()
	{
		MainManager.listcursor = 0;
		MainManager.instance.option = 0;
		MainManager.listlow = 0;
		MainManager.listmax = MainManager.listammount;
		MainManager.listY = -1;
	}

	// Token: 0x0600060E RID: 1550 RVA: 0x00040A28 File Offset: 0x0003EC28
	public static int[] SaveList()
	{
		return new int[]
		{
			MainManager.instance.option,
			MainManager.listcursor,
			MainManager.listlow,
			MainManager.listmax,
			MainManager.listoption
		};
	}

	// Token: 0x0600060F RID: 1551 RVA: 0x00040A5D File Offset: 0x0003EC5D
	public static void LoadList(int[] v)
	{
		MainManager.instance.option = v[0];
		MainManager.listcursor = v[1];
		MainManager.listlow = v[2];
		MainManager.listmax = v[3];
		MainManager.listoption = v[4];
	}

	// Token: 0x06000610 RID: 1552 RVA: 0x00040A8C File Offset: 0x0003EC8C
	public static MainManager.LoadData? Load(int file, bool lite)
	{
		try
		{
			string[] array = new string[]
			{
				""
			};
			if (InputIO.IsConsole)
			{
				array = InputIO.ReadFile("save" + file + ".dat").Split(new char[]
				{
					'\n'
				});
			}
			else
			{
				try
				{
					if (!Directory.Exists("Saves"))
					{
						Directory.CreateDirectory("Saves");
						for (int i = 0; i < 3; i++)
						{
							if (File.Exists("save" + i + ".dat"))
							{
								File.Move("save" + i + ".dat", "Saves/save" + i + ".dat");
							}
						}
					}
					array = InputIO.Encrypt(InputIO.ReadFile("Saves/save" + file + ".dat")).Split(new char[]
					{
						'\n'
					});
				}
				catch
				{
					return null;
				}
			}
			MainManager.LoadData loadData = default(MainManager.LoadData);
			string[] array2 = array[0].Split(new char[]
			{
				','
			});
			if (!lite)
			{
				loadData.loadpos = new Vector3(Convert.ToSingle(array2[0]), Convert.ToSingle(array2[1]), Convert.ToSingle(array2[2]));
				array2 = array[7].Split(new char[]
				{
					'@'
				});
				MainManager.instance.badges = new List<int[]>();
				for (int j = 0; j < array2.Length; j++)
				{
					string[] array3 = array2[j].Split(new char[]
					{
						','
					});
					List<int> list = new List<int>();
					if (array3.Length != 0)
					{
						for (int k = 0; k < array3.Length; k++)
						{
							if (array3[k] != "")
							{
								list.Add(Convert.ToInt32(array3[k]));
							}
						}
					}
					if (list.Count > 0)
					{
						MainManager.instance.badges.Add(list.ToArray());
					}
				}
				array2 = array[9].Split(new char[]
				{
					'@'
				});
				MainManager.instance.statbonus = new List<int[]>();
				for (int l = 0; l < array2.Length; l++)
				{
					string[] array4 = array2[l].Split(new char[]
					{
						','
					});
					List<int> list2 = new List<int>();
					if (array4.Length != 0)
					{
						for (int m = 0; m < array4.Length; m++)
						{
							if (array4[m] != "")
							{
								list2.Add(Convert.ToInt32(array4[m]));
							}
						}
					}
					if (list2.Count > 0)
					{
						MainManager.instance.statbonus.Add(list2.ToArray());
					}
				}
				array2 = array[1].Split(new char[]
				{
					'@'
				});
				int[] array5 = new int[array2.Length];
				for (int n = 0; n < array5.Length; n++)
				{
					array5[n] = Convert.ToInt32(array2[n].Split(new char[]
					{
						','
					})[0]);
				}
				MainManager.ChangeParty(array5, false);
				MainManager.instance.playerdata = new MainManager.BattleData[array5.Length];
				for (int num = 0; num < array2.Length; num++)
				{
					string[] array6 = array2[num].Split(new char[]
					{
						','
					});
					MainManager.instance.playerdata[num].trueid = array5[num];
					MainManager.instance.playerdata[num].animid = array5[num];
					MainManager.instance.playerdata[num].hp = Convert.ToInt32(array6[1]);
					MainManager.instance.playerdata[num].maxhp = Convert.ToInt32(array6[2]);
					MainManager.instance.playerdata[num].basehp = Convert.ToInt32(array6[3]);
					MainManager.instance.playerdata[num].atk = Convert.ToInt32(array6[4]);
					MainManager.instance.playerdata[num].baseatk = Convert.ToInt32(array6[5]);
					MainManager.instance.playerdata[num].def = Convert.ToInt32(array6[6]);
					MainManager.instance.playerdata[num].basedef = Convert.ToInt32(array6[7]);
					MainManager.instance.playerdata[num].entityname = MainManager.menutext[46 + array5[num]];
				}
			}
			else
			{
				loadData.challenges = new bool[6];
				try
				{
					if (array2.Length > 3)
					{
						for (int num2 = 0; num2 < loadData.challenges.Length; num2++)
						{
							loadData.challenges[num2] = (array2[num2 + 3] == "True");
						}
					}
					if (array2.Length < 10)
					{
						loadData.filename = "|color,1|SLOT " + (file + 1) + " - NO FILE NAME";
					}
					else
					{
						loadData.filename = array2[9];
					}
				}
				catch
				{
					loadData.filename = "|color,1|SLOT " + (file + 1) + " - NO FILE NAME";
				}
			}
			array2 = array[2].Split(new char[]
			{
				','
			});
			if (!lite)
			{
				MainManager.instance.partylevel = Convert.ToInt32(array2[0]);
				MainManager.instance.partyexp = Convert.ToInt32(array2[1]);
				MainManager.instance.neededexp = Convert.ToInt32(array2[2]);
				MainManager.instance.basetp = Convert.ToInt32(array2[3]);
				MainManager.instance.tp = Convert.ToInt32(array2[4]);
				MainManager.instance.money = Convert.ToInt32(array2[5]);
				MainManager.instance.bp = Convert.ToInt32(array2[8]);
				MainManager.instance.maxbp = Convert.ToInt32(array2[9]);
				MainManager.instance.maxitems = Convert.ToInt32(array2[10]);
				MainManager.instance.maxstorage = Convert.ToInt32(array2[11]);
				MainManager.instance.clockhour = Convert.ToInt32(array2[12]);
				MainManager.instance.clockmin = Convert.ToInt32(array2[13]);
				MainManager.instance.clocksec = Convert.ToInt32(array2[14]);
				MainManager.instance.areaid = Convert.ToInt32(array2[7]);
			}
			loadData.level = Convert.ToInt32(array2[0]);
			loadData.mapid = Convert.ToInt32(array2[6]);
			loadData.areaid = Convert.ToInt32(array2[7]);
			loadData.timeh = Convert.ToInt32(array2[12]);
			loadData.timem = Convert.ToInt32(array2[13]);
			loadData.times = Convert.ToInt32(array2[14]);
			loadData.progression = Convert.ToInt32(array2[15]);
			if (!lite)
			{
				array2 = array[3].Split(new char[]
				{
					'@'
				});
				MainManager.instance.avaliablebadgepool = new List<int>[array2.Length];
				for (int num3 = 0; num3 < array2.Length; num3++)
				{
					MainManager.instance.avaliablebadgepool[num3] = new List<int>();
					string[] array7 = array2[num3].Split(new char[]
					{
						','
					});
					if (array7.Length != 0)
					{
						for (int num4 = 0; num4 < array7.Length; num4++)
						{
							if (array7[num4] != "")
							{
								MainManager.instance.avaliablebadgepool[num3].Add(Convert.ToInt32(array7[num4]));
							}
						}
					}
				}
				array2 = array[4].Split(new char[]
				{
					'@'
				});
				MainManager.instance.badgeshops = new List<int>[array2.Length];
				for (int num5 = 0; num5 < array2.Length; num5++)
				{
					MainManager.instance.badgeshops[num5] = new List<int>();
					string[] array8 = array2[num5].Split(new char[]
					{
						','
					});
					if (array8.Length != 0)
					{
						for (int num6 = 0; num6 < array8.Length; num6++)
						{
							if (array8[num6] != "")
							{
								MainManager.instance.badgeshops[num5].Add(Convert.ToInt32(array8[num6]));
							}
						}
					}
				}
				array2 = array[5].Split(new char[]
				{
					'@'
				});
				MainManager.instance.boardquests = new List<int>[array2.Length];
				for (int num7 = 0; num7 < array2.Length; num7++)
				{
					MainManager.instance.boardquests[num7] = new List<int>();
					string[] array9 = array2[num7].Split(new char[]
					{
						','
					});
					if (array9.Length != 0)
					{
						for (int num8 = 0; num8 < array9.Length; num8++)
						{
							if (array9[num8] != "")
							{
								MainManager.instance.boardquests[num7].Add(Convert.ToInt32(array9[num8]));
							}
						}
					}
				}
				array2 = array[6].Split(new char[]
				{
					'@'
				});
				MainManager.instance.items = new List<int>[array2.Length];
				for (int num9 = 0; num9 < array2.Length; num9++)
				{
					MainManager.instance.items[num9] = new List<int>();
					string[] array10 = array2[num9].Split(new char[]
					{
						','
					});
					if (array10.Length != 0)
					{
						for (int num10 = 0; num10 < array10.Length; num10++)
						{
							if (array10[num10] != "")
							{
								MainManager.instance.items[num9].Add(Convert.ToInt32(array10[num10]));
							}
						}
					}
				}
				array2 = array[8].Split(new char[]
				{
					'@'
				});
				MainManager.instance.samiramusics = new List<int[]>();
				for (int num11 = 0; num11 < array2.Length; num11++)
				{
					string[] array11 = array2[num11].Split(new char[]
					{
						','
					});
					List<int> list3 = new List<int>();
					if (array11.Length != 0)
					{
						for (int num12 = 0; num12 < array11.Length; num12++)
						{
							if (array11[num12] != "")
							{
								list3.Add(Convert.ToInt32(array11[num12]));
							}
						}
					}
					if (list3.Count > 0)
					{
						MainManager.instance.samiramusics.Add(list3.ToArray());
					}
				}
				array2 = array[10].Split(new char[]
				{
					'@'
				});
				MainManager.instance.librarystuff = new bool[array2.Length, 256];
				for (int num13 = 0; num13 < array2.Length; num13++)
				{
					string[] array12 = array2[num13].Split(new char[]
					{
						','
					});
					for (int num14 = 0; num14 < array12.Length; num14++)
					{
						MainManager.instance.librarystuff[num13, num14] = Convert.ToBoolean(array12[num14]);
					}
				}
				array2 = array[11].Split(new char[]
				{
					','
				});
				MainManager.instance.flags = new bool[750];
				for (int num15 = 0; num15 < array2.Length; num15++)
				{
					if (num15 < MainManager.instance.flags.Length)
					{
						MainManager.instance.flags[num15] = Convert.ToBoolean(array2[num15]);
					}
				}
				array2 = array[12].Replace("|SPLIT|", "¬").Split(new char[]
				{
					'¬'
				});
				MainManager.instance.flagstring = new string[15];
				for (int num16 = 0; num16 < MainManager.instance.flagstring.Length; num16++)
				{
					if (num16 < array2.Length)
					{
						MainManager.instance.flagstring[num16] = array2[num16];
					}
				}
				if (MainManager.instance.flagstring.Length <= 2)
				{
					MainManager.instance.flagstring = new string[15];
				}
				array2 = array[13].Split(new char[]
				{
					','
				});
				MainManager.instance.flagvar = new int[70];
				for (int num17 = 0; num17 < array2.Length; num17++)
				{
					MainManager.instance.flagvar[num17] = Convert.ToInt32(array2[num17]);
				}
				array2 = array[14].Split(new char[]
				{
					','
				});
				MainManager.instance.regionalflags = new bool[array2.Length];
				for (int num18 = 0; num18 < array2.Length; num18++)
				{
					MainManager.instance.regionalflags[num18] = Convert.ToBoolean(array2[num18]);
				}
				array2 = array[15].Split(new char[]
				{
					','
				});
				MainManager.instance.crystalbflags = new bool[array2.Length];
				for (int num19 = 0; num19 < array2.Length; num19++)
				{
					MainManager.instance.crystalbflags[num19] = Convert.ToBoolean(array2[num19]);
				}
				array2 = array[16].Split(new char[]
				{
					','
				});
				MainManager.instance.extrafollowers = new List<int>();
				for (int num20 = 0; num20 < array2.Length; num20++)
				{
					if (array2[num20] != "")
					{
						MainManager.instance.extrafollowers.Add(Convert.ToInt32(array2[num20]));
					}
				}
				array2 = array[17].Split(new char[]
				{
					'@'
				});
				MainManager.instance.enemyencounter = new int[array2.Length, array2[0].Split(new char[]
				{
					','
				}).Length];
				for (int num21 = 0; num21 < array2.Length; num21++)
				{
					string[] array13 = array2[num21].Split(new char[]
					{
						','
					});
					for (int num22 = 0; num22 < array13.Length; num22++)
					{
						MainManager.instance.enemyencounter[num21, num22] = Convert.ToInt32(array13[num22]);
					}
				}
				MainManager.ApplyBadges();
			}
			return new MainManager.LoadData?(loadData);
		}
		catch
		{
			Debug.Log("Save file " + file + " is corrupted.");
		}
		return null;
	}

	// Token: 0x06000611 RID: 1553 RVA: 0x00041848 File Offset: 0x0003FA48
	public static float[] Divisions(int divisions)
	{
		float num = 1f / (float)(divisions + 1);
		List<float> list = new List<float>();
		for (int i = 1; i <= divisions; i++)
		{
			list.Add(num * (float)i);
		}
		return list.ToArray();
	}

	// Token: 0x06000612 RID: 1554 RVA: 0x00041882 File Offset: 0x0003FA82
	public static IEnumerator LatePos(Transform obj, Vector3 pos, float delay, bool keeptrying)
	{
		if (keeptrying)
		{
			for (float i = 0f; i < delay; i += MainManager.framestep)
			{
				if (obj != null)
				{
					obj.position = pos;
				}
				yield return null;
			}
		}
		else
		{
			yield return new WaitForSeconds(delay);
			if (obj != null)
			{
				obj.position = pos;
			}
		}
		yield break;
	}

	// Token: 0x06000613 RID: 1555 RVA: 0x000418A8 File Offset: 0x0003FAA8
	public static int SaveProgressIcons()
	{
		int num = 0;
		if (MainManager.instance.flags[41])
		{
			num++;
		}
		if (MainManager.instance.flags[88])
		{
			num++;
		}
		if (MainManager.instance.flags[299])
		{
			num++;
		}
		if (MainManager.instance.flags[345])
		{
			num++;
		}
		if (MainManager.instance.flags[347])
		{
			num++;
		}
		if (MainManager.instance.flags[346])
		{
			num++;
		}
		if (MainManager.instance.flags[555])
		{
			num++;
		}
		return num;
	}

	// Token: 0x06000614 RID: 1556 RVA: 0x0004194C File Offset: 0x0003FB4C
	public static bool Save(Vector3? savepos)
	{
		return InputIO.Save(savepos);
	}

	// Token: 0x06000615 RID: 1557 RVA: 0x00041954 File Offset: 0x0003FB54
	public static string PromptYesNo(int yes, int no)
	{
		return string.Concat(new object[]
		{
			"|prompt,map,0.5,2,",
			yes,
			",",
			no,
			",@",
			MainManager.menutext[5],
			",@",
			MainManager.menutext[6],
			"|"
		});
	}

	// Token: 0x06000616 RID: 1558 RVA: 0x000419BB File Offset: 0x0003FBBB
	public static string PromptYesNo()
	{
		return MainManager.PromptYesNo(-11, -11);
	}

	// Token: 0x06000617 RID: 1559 RVA: 0x000419C8 File Offset: 0x0003FBC8
	private static void GetRewardTokens(int score)
	{
		if (!MainManager.battleresult)
		{
			MainManager.instance.flagvar[1] = Mathf.FloorToInt((float)score / 200f);
		}
		else if (MainManager.instance.flagvar[6] == 0)
		{
			MainManager.instance.flagvar[1] = Mathf.FloorToInt((float)score / 95f);
		}
		else
		{
			MainManager.instance.flagvar[1] = Mathf.FloorToInt((float)score / 140f);
		}
		MainManager.instance.flagvar[1] = Mathf.Clamp(MainManager.instance.flagvar[1], 2, 999999);
	}

	// Token: 0x06000618 RID: 1560 RVA: 0x00041A5C File Offset: 0x0003FC5C
	public static bool AnyKeyButThis(int id, bool hold)
	{
		for (int i = 0; i < InputIO.keys.Length; i++)
		{
			if (i != id && MainManager.GetKey(i, hold))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000619 RID: 1561 RVA: 0x00041A8B File Offset: 0x0003FC8B
	public static IEnumerator TransferMap(int targetmap, Vector3 targetpos)
	{
		MainManager.instance.StartCoroutine(MainManager.TransferMap(targetmap, MainManager.player.transform.position, targetpos, targetpos, null));
		yield return null;
		yield break;
	}

	// Token: 0x0600061A RID: 1562 RVA: 0x00041AA1 File Offset: 0x0003FCA1
	public static IEnumerator TransferMap(int targetmap, Vector3 moveto, Vector3 tppos, Vector3 othermovepos)
	{
		MainManager.instance.StartCoroutine(MainManager.TransferMap(targetmap, moveto, tppos, othermovepos, null));
		yield return null;
		yield break;
	}

	// Token: 0x0600061B RID: 1563 RVA: 0x00041AC5 File Offset: 0x0003FCC5
	public static IEnumerator TransferMap(int targetmap, Vector3 moveto, Vector3 tppos, Vector3 othermovepos, NPCControl caller)
	{
		MainManager.roomtransition = true;
		Vector3[] tdata = null;
		int[] tdataa = null;
		float jump = 0f;
		if (caller != null)
		{
			if (caller.data.Length > 1)
			{
				tdata = new Vector3[]
				{
					caller.vectordata[3],
					caller.vectordata[4],
					caller.vectordata[5],
					caller.vectordata[6]
				};
				tdataa = caller.data;
			}
			jump = caller.entity.emoticonoffset.x;
		}
		if (MainManager.map != null && MainManager.map.entities != null)
		{
			for (int i = 0; i < MainManager.map.entities.Length; i++)
			{
				if (MainManager.map.entities[i] != null)
				{
					MainManager.map.entities[i].BreakIce();
					if (MainManager.map.entities[i].npcdata != null && MainManager.map.entities[i].npcdata.boxcol != null)
					{
						MainManager.map.entities[i].npcdata.boxcol.enabled = false;
					}
				}
			}
		}
		MainManager.instance.minipause = true;
		MainManager.player.CancelAction();
		while (MainManager.pausemenu != null)
		{
			yield return null;
		}
		MainManager.PlayTransition(0, 0, 0.1f, Color.black);
		if (!(caller != null) || caller.data.Length < 5 || caller.data[4] != 1)
		{
			MainManager.player.entity.MoveTowards(moveto, 1f, 1, 0, true);
			MainManager.instance.camtargetpos = new Vector3?(MainManager.player.transform.position);
			MainManager.instance.camtarget = null;
			while (MainManager.player.entity.forcemove)
			{
				yield return null;
			}
		}
		else
		{
			for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
			{
				MainManager.instance.playerdata[j].entity.rigid.useGravity = false;
				MainManager.instance.playerdata[j].entity.rigid.isKinematic = true;
			}
			yield return null;
		}
		SpriteRenderer t = MainManager.instance.transitionobj[0].GetComponent<SpriteRenderer>();
		while (t.color.a < 0.9f)
		{
			yield return null;
		}
		t.color = new Color(t.color.r, t.color.g, t.color.b, 1f);
		yield return null;
		MainManager.LoadMap(targetmap);
		if (!MainManager.instance.inevent)
		{
			MainManager.instance.insideid = -1;
		}
		MainManager.instance.globalcooldown = 30f;
		MainManager.instance.inmusicrange = -1;
		yield return null;
		MainManager.MainCamera.transform.position = MainManager.instance.playerdata[0].entity.transform.position + MainManager.instance.camoffset;
		float tempsped = MainManager.instance.camspeed;
		MainManager.instance.camspeed = 1f;
		yield return null;
		for (int k = 0; k < MainManager.instance.playerdata.Length; k++)
		{
			if (MainManager.instance.playerdata[k].entity != null)
			{
				MainManager.instance.playerdata[k].entity.transform.position = tppos + MainManager.MainCamera.transform.forward * ((float)k / 10f);
				MainManager.instance.playerdata[k].entity.rigid.velocity = Vector3.zero;
				MainManager.instance.playerdata[k].entity.rigid.useGravity = true;
				MainManager.instance.playerdata[k].entity.rigid.isKinematic = false;
				MainManager.instance.playerdata[k].entity.onground = false;
			}
		}
		yield return null;
		for (int l = 0; l < MainManager.map.entities.Length; l++)
		{
			if (MainManager.map.entities[l] != null)
			{
				MainManager.map.entities[l].CheckNear();
			}
		}
		yield return null;
		MainManager.instance.ForceLoadSprites();
		yield return new WaitForSeconds(0.1f);
		for (int m = 0; m < MainManager.instance.playerdata.Length; m++)
		{
			MainManager.instance.playerdata[m].entity.FaceTowards(othermovepos);
		}
		MainManager.instance.camtarget = MainManager.player.transform;
		if (tdataa != null && tdataa.Length > 1)
		{
			if (tdataa[1] == 1)
			{
				MainManager.instance.camoffset = tdata[0];
			}
			if (tdataa[2] == 1)
			{
				MainManager.instance.camangleoffset = tdata[1];
			}
			if (tdataa[3] == 1)
			{
				MainManager.map.camlimitpos = tdata[2];
				MainManager.map.camlimitneg = tdata[3];
			}
		}
		yield return new WaitForSeconds(0.3f);
		MainManager.instance.camspeed = tempsped;
		yield return null;
		MainManager.PlayTransition(1, 0, 0.1f, Color.black);
		yield return null;
		if (jump > 0.1f)
		{
			MainManager.player.jumproutine = MainManager.player.StartCoroutine(MainManager.player.JumpTo(othermovepos, jump));
			while (MainManager.player.jumproutine != null)
			{
				for (int n = 0; n < MainManager.instance.playerdata.Length; n++)
				{
					MainManager.instance.playerdata[n].entity.animstate = 3;
					MainManager.instance.playerdata[n].entity.onground = false;
				}
				yield return null;
			}
		}
		else
		{
			MainManager.player.entity.MoveTowards(othermovepos, 1f, 1, 0, true);
			while (MainManager.player.entity.forcemove)
			{
				yield return null;
			}
		}
		MainManager.player.entity.DetectIgnoreSphere(true);
		MainManager.instance.minipause = false;
		MainManager.player.lastpos = MainManager.player.transform.position;
		MainManager.player.lastloadzone = MainManager.player.transform.position;
		yield return null;
		MainManager.player.entity.hitwall = false;
		MainManager.player.npc = new List<NPCControl>();
		MainManager.player.pausecooldown = 7f;
		MainManager.roomtransition = false;
		yield break;
	}

	// Token: 0x0600061C RID: 1564 RVA: 0x00041AF1 File Offset: 0x0003FCF1
	public void ForceLoadSprites()
	{
		MainManager.instance.StartCoroutine(MainManager.FLS());
	}

	// Token: 0x0600061D RID: 1565 RVA: 0x00041B03 File Offset: 0x0003FD03
	private static IEnumerator FLS()
	{
		if (MainManager.map.entities != null)
		{
			SpriteRenderer[] r = new SpriteRenderer[MainManager.map.entities.Length];
			for (int i = 0; i < r.Length; i++)
			{
				if (MainManager.map.entities[i] != null && MainManager.map.entities[i].sprite != null && MainManager.map.entities[i].sprite.sprite != null)
				{
					r[i] = new GameObject().AddComponent<SpriteRenderer>();
					r[i].transform.parent = MainManager.map.transform;
					r[i].transform.position = MainManager.player.transform.position;
					r[i].sprite = MainManager.map.entities[i].sprite.sprite;
				}
			}
			yield return null;
			for (int j = 0; j < r.Length; j++)
			{
				if (r[j] != null)
				{
					Object.Destroy(r[j].gameObject);
				}
			}
			r = null;
		}
		yield break;
	}

	// Token: 0x0600061E RID: 1566 RVA: 0x00041B0B File Offset: 0x0003FD0B
	private static string GetText(bool menu, int id)
	{
		if (!menu)
		{
			return MainManager.GetDialogueText(id);
		}
		return MainManager.menutext[id];
	}

	// Token: 0x0600061F RID: 1567 RVA: 0x00041B20 File Offset: 0x0003FD20
	public static int GetEnemyPortrait(int id)
	{
		int num = Convert.ToInt32(MainManager.enemydata[id, 43]);
		if (MainManager.instance.flags[664])
		{
			switch (id)
			{
			case 57:
				return 226;
			case 58:
				return 225;
			case 61:
				return 227;
			}
		}
		if (num > -1)
		{
			return num;
		}
		return id;
	}

	// Token: 0x06000620 RID: 1568 RVA: 0x00041B8C File Offset: 0x0003FD8C
	public static Vector2 GetDirection(in Vector2 a, in Vector2 b)
	{
		return (a - b).normalized;
	}

	// Token: 0x06000621 RID: 1569 RVA: 0x00041BB4 File Offset: 0x0003FDB4
	public static Vector3 GetDirection(in Vector3 a, in Vector3 b)
	{
		return (a - b).normalized;
	}

	// Token: 0x06000622 RID: 1570 RVA: 0x00041BDC File Offset: 0x0003FDDC
	public static Vector3 GetDirection(in Vector3 a, in Vector3 b, bool ignoreY)
	{
		if (!ignoreY)
		{
			return MainManager.GetDirection(a, b);
		}
		Vector3 vector = new Vector3(a.x, 0f, a.z);
		Vector3 vector2 = new Vector3(b.x, 0f, b.z);
		return MainManager.GetDirection(vector, vector2);
	}

	// Token: 0x06000623 RID: 1571 RVA: 0x00041C2C File Offset: 0x0003FE2C
	public static Vector3 GetDirection4(Vector3 a, Vector3 b, bool ignoreY)
	{
		if (ignoreY)
		{
			a = new Vector3(a.x, 0f, a.z);
			b = new Vector3(b.x, 0f, b.z);
		}
		Vector3 vector = new Vector3(a.x, 0f, a.z);
		Vector3 vector2 = new Vector3(b.x, 0f, b.z);
		return MainManager.GetDirection(vector, vector2);
	}

	// Token: 0x06000624 RID: 1572 RVA: 0x00041CA3 File Offset: 0x0003FEA3
	public static IEnumerator MoveTowards(Transform obj, Vector3 target, float frametime, bool smooth, bool local)
	{
		float a = 0f;
		Vector3 s = local ? obj.localPosition : obj.localPosition;
		do
		{
			if (local)
			{
				if (smooth)
				{
					obj.localPosition = MainManager.SmoothLerp(s, target, a / frametime);
				}
				else
				{
					obj.localPosition = Vector3.Lerp(s, target, a / frametime);
				}
			}
			else if (smooth)
			{
				obj.position = MainManager.SmoothLerp(s, target, a / frametime);
			}
			else
			{
				obj.position = Vector3.Lerp(s, target, a / frametime);
			}
			a += MainManager.TieFramerate(1f);
			yield return null;
		}
		while (a < frametime);
		yield break;
	}

	// Token: 0x06000625 RID: 1573 RVA: 0x00041CCF File Offset: 0x0003FECF
	public static IEnumerator MoveTowards(Transform obj, Vector3 start, Vector3 end, float frametime, bool smooth, Action<bool> caller)
	{
		MainManager.instance.StartCoroutine(MainManager.MoveTowards(obj, start, end, frametime, 0f, 0f, -1f, smooth, caller, null));
		yield return null;
		yield break;
	}

	// Token: 0x06000626 RID: 1574 RVA: 0x00041D04 File Offset: 0x0003FF04
	public static IEnumerator MoveTowards(Transform obj, Vector3 start, Vector3 end, float frametime, float startdelay, float shrink, float destroytime, bool smooth, Action<bool> caller)
	{
		MainManager.instance.StartCoroutine(MainManager.MoveTowards(obj, start, end, frametime, startdelay, shrink, destroytime, smooth, caller, null));
		yield return null;
		yield break;
	}

	// Token: 0x06000627 RID: 1575 RVA: 0x00041D5C File Offset: 0x0003FF5C
	public static IEnumerator MoveTowards(Transform obj, Vector3 start, Vector3 end, float frametime, float startdelay, float shrink, float destroytime, bool smooth, Action<bool> caller, string soundatend)
	{
		if (startdelay > 0f)
		{
			yield return new WaitForSeconds(startdelay);
		}
		float a = 0f;
		do
		{
			if (smooth)
			{
				obj.transform.position = MainManager.SmoothLerp(start, end, a / frametime);
			}
			else
			{
				obj.transform.position = Vector3.Lerp(start, end, a / frametime);
			}
			a += MainManager.TieFramerate(1f);
			yield return null;
		}
		while (a < frametime + 1f);
		obj.transform.position = end;
		if (soundatend != null)
		{
			MainManager.PlaySound(soundatend);
		}
		yield return null;
		if (shrink > 0f)
		{
			DialogueAnim dialogueAnim = obj.gameObject.AddComponent<DialogueAnim>();
			dialogueAnim.shrink = true;
			dialogueAnim.shrinkspeed = shrink;
		}
		if (destroytime > -1f)
		{
			if (destroytime == 0f)
			{
				Object.Destroy(obj.gameObject);
			}
			else
			{
				Object.Destroy(obj.gameObject, destroytime);
			}
		}
		if (caller != null)
		{
			caller(false);
		}
		yield break;
	}

	// Token: 0x06000628 RID: 1576 RVA: 0x00041DBC File Offset: 0x0003FFBC
	public static Vector3 GlobalScale(Vector3 desiredscale, Vector3 parentscale, bool flipZY)
	{
		if (flipZY)
		{
			return new Vector3(desiredscale.x / parentscale.x, desiredscale.z / parentscale.z, desiredscale.y / parentscale.y);
		}
		return new Vector3(desiredscale.x / parentscale.x, desiredscale.y / parentscale.y, desiredscale.z / parentscale.z);
	}

	// Token: 0x06000629 RID: 1577 RVA: 0x00041E25 File Offset: 0x00040025
	public static void DialogueText(string text, Transform tailtarget, NPCControl caller)
	{
		MainManager.instance.StartCoroutine(MainManager.SetText(text, true, Vector3.zero, tailtarget, caller));
	}

	// Token: 0x0600062A RID: 1578 RVA: 0x00041E40 File Offset: 0x00040040
	public static IEnumerator OpenQuestBoard(EntityControl caretaker, NPCControl caller)
	{
		MainManager.instance.discoveryhud = 0f;
		MainManager.SaveCameraPosition(true);
		MainManager.instance.flags[2] = true;
		MainManager.instance.option = 0;
		MainManager.instance.showmoney = 0f;
		MainManager.instance.hudcooldown = 0f;
		MainManager.instance.minipause = true;
		if (MainManager.instance.boardquests[0].Count > 1)
		{
			MainManager.instance.boardquests[0].Remove(0);
		}
		yield return null;
		if (caretaker != null)
		{
			caretaker.FaceTowards(MainManager.player.transform.position);
		}
		if (caller != null)
		{
			for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
			{
				MainManager.instance.playerdata[i].entity.FaceTowards(caller.transform.position);
			}
			MainManager.instance.boardcaller = caller;
			MainManager.tempcamspeed = MainManager.instance.camspeed;
			MainManager.tempcamoffset = MainManager.instance.camoffset;
			MainManager.tempcamangleoffset = MainManager.instance.camangleoffset;
			if (caller.vectordata[0].magnitude > 0.1f)
			{
				MainManager.instance.camoffset = caller.vectordata[0];
			}
			if (caller.vectordata[1].magnitude > 0.1f)
			{
				MainManager.instance.camangleoffset = caller.vectordata[1];
			}
			if (caller.vectordata[2].x > 0.01f)
			{
				MainManager.instance.camspeed = caller.vectordata[2].x;
			}
			yield return new WaitForSeconds(caller.vectordata[2].y);
			for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
			{
				MainManager.instance.playerdata[j].entity.FaceTowards(caller.transform.position);
			}
		}
		if (caretaker != null)
		{
			caretaker.FaceTowards(MainManager.player.transform.position);
		}
		MainManager.instance.questboardobj = MainManager.NewUIObject("QuestBoard", null, new Vector3(0f, 0f, 10f)).transform;
		DialogueAnim dialogueAnim = MainManager.NewUIObject("LeafSprite", MainManager.instance.questboardobj, new Vector3(0f, -1.75f), Vector2.one, Resources.Load<Sprite>("Sprites/GUI/bigleaf")).AddComponent<DialogueAnim>();
		dialogueAnim.transform.localScale = Vector3.zero;
		dialogueAnim.targetscale = Vector3.one * 2f;
		dialogueAnim.GetComponent<SpriteRenderer>().color = new Color(0.9f, 0.7f, 0.1f);
		dialogueAnim.GetComponent<SpriteRenderer>().sortingOrder = -50;
		MainManager.Create9Box(new Vector3(-3.75f, -0.3f, 10f), new Vector2(8f, 7.5f), 4, -10, Color.yellow, true).transform.parent = MainManager.instance.questboardobj;
		MainManager.Create9Box(new Vector3(4.75f, -0.3f, 10f), new Vector2(8.25f, 9f), 4, -5, Color.white, true).transform.parent = MainManager.instance.questboardobj;
		for (int k = 0; k < 2; k++)
		{
			MainManager.NewUIObject("arrow" + k, MainManager.instance.questboardobj, (k == 0) ? new Vector3(0.2f, 4f, 10f) : new Vector3(-7.7f, 4f, 10f), Vector3.one, MainManager.guisprites[1], 20).transform.localEulerAngles = ((k == 0) ? new Vector3(0f, 0f, 90f) : new Vector3(0f, 0f, 270f));
		}
		for (int l = 0; l < 2; l++)
		{
			int buttonid = 0;
			string description = MainManager.AsianLang() ? "|size,0.7,1|" : "";
			Vector3 zero = Vector3.zero;
			if (l == 0)
			{
				buttonid = 4;
				description = "|sort,2|" + MainManager.menutext[83];
				zero = new Vector3(-8f, -4.5f, 10f);
			}
			else if (l == 1)
			{
				buttonid = 5;
				description = "|sort,2|" + MainManager.menutext[43];
				zero = new Vector3(-2.8f, -4.5f, 10f);
			}
			new GameObject("button " + l).AddComponent<ButtonSprite>().SetUp(buttonid, -1, description, zero, Vector3.one * 0.5f, 10, null).transform.parent = MainManager.instance.questboardobj;
		}
		float num = -1.6f;
		for (int m = 0; m < 3; m++)
		{
			SpriteRenderer component = MainManager.NewUIObject("tab" + m, MainManager.instance.questboardobj.GetChild(1), new Vector3(num, 4.25f), new Vector3(0.75f, 0.75f, 1f), MainManager.guisprites[4]).GetComponent<SpriteRenderer>();
			component.color = MainManager.instance.questcolors[m];
			component.sortingOrder = -5 - m * 2;
			num += 1.55f;
			float x = -0.5f;
			int num2 = MainManager.languageid;
			if (num2 != 1)
			{
				if (num2 == 3)
				{
					x = 0f;
				}
			}
			else
			{
				x = -0.75f;
			}
			MainManager.instance.StartCoroutine(MainManager.SetText(string.Concat(new object[]
			{
				MainManager.AsianLang() ? "|size,0.8,1.15|" : ((MainManager.languageid == 6) ? "|size,1.2,1.5|" : "|size,1.5|"),
				"|single||quarterline||center||sort,",
				component.sortingOrder + 1,
				"|",
				MainManager.menutext[84 + m]
			}), new Vector3(x, -0.25f), component.transform));
		}
		MainManager.SetUpList(14, true, false);
		MainManager.listammount = 10;
		MainManager.ShowItemList(14, new Vector2(-8.5f, 2.55f), true, false);
		MainManager.UpdateQuestBoard();
		yield break;
	}

	// Token: 0x0600062B RID: 1579 RVA: 0x0002D9A9 File Offset: 0x0002BBA9
	public static void ChangeBoardQuest(int id)
	{
		MainManager.ChangeBoardQuest(id, 0);
	}

	// Token: 0x0600062C RID: 1580 RVA: 0x00041E58 File Offset: 0x00040058
	private static string BoardString()
	{
		string text = "";
		for (int i = 0; i < MainManager.instance.boardquests[0].Count; i++)
		{
			text = text + MainManager.instance.boardquests[0][i] + ",";
		}
		return text;
	}

	// Token: 0x0600062D RID: 1581 RVA: 0x00041EAC File Offset: 0x000400AC
	public static void ChangeBoardQuest(int id, int type)
	{
		if (!MainManager.instance.boardquests[type].Contains(id))
		{
			MainManager.instance.boardquests[type].Remove(0);
			MainManager.instance.boardquests[type].Add(id);
			if (type == 0 && id > 0)
			{
				MainManager.instance.flags[2] = false;
			}
		}
	}

	// Token: 0x0600062E RID: 1582 RVA: 0x00041F08 File Offset: 0x00040108
	public static void CompleteQuest(int id)
	{
		MainManager.instance.boardquests[1].Remove(id);
		MainManager.instance.boardquests[2].Add(id);
		MainManager.instance.boardquests[2].Remove(0);
		if (MainManager.instance.boardquests[1].Count == 0)
		{
			MainManager.instance.boardquests[1].Add(0);
		}
		MainManager.instance.discoveryhud = 350f;
		MainManager.instance.flagvar[47]++;
		for (int i = 0; i < 3; i++)
		{
			MainManager.instance.discoverymessage.GetChild(i + 1).gameObject.SetActive(i + 1 == 2);
		}
		MainManager.instance.discoverymessage.GetComponentInChildren<Animator>().PlayInFixedTime("Quest");
	}

	// Token: 0x0600062F RID: 1583 RVA: 0x00041FDE File Offset: 0x000401DE
	public static MainManager.ItemUse GetItemUse(int id)
	{
		return MainManager.GetItemUse(id, 0);
	}

	// Token: 0x06000630 RID: 1584 RVA: 0x00041FE8 File Offset: 0x000401E8
	public static MainManager.ItemUse GetItemUse(int id, int itemtype)
	{
		MainManager.ItemUse itemUse = default(MainManager.ItemUse);
		string[] array = MainManager.itemdata[itemtype, id, 5].Split(new char[]
		{
			';'
		});
		itemUse.usetype = new MainManager.ItemUsage[array.Length];
		itemUse.values = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[]
			{
				','
			});
			itemUse.usetype[i] = (MainManager.ItemUsage)Enum.Parse(typeof(MainManager.ItemUsage), array2[0]);
			itemUse.values[i] = Convert.ToInt32(array2[1]);
		}
		return itemUse;
	}

	// Token: 0x06000631 RID: 1585 RVA: 0x0004208C File Offset: 0x0004028C
	public static int? GetItemHeal(int id)
	{
		MainManager.ItemUse itemUse = MainManager.GetItemUse(id);
		for (int i = 0; i < itemUse.usetype.Length; i++)
		{
			if (itemUse.usetype[i] == MainManager.ItemUsage.HPRecover || itemUse.usetype[i] == MainManager.ItemUsage.Revive || itemUse.usetype[i] == MainManager.ItemUsage.AutoRevive)
			{
				return new int?(itemUse.values[i]);
			}
			if (itemUse.usetype[i] == MainManager.ItemUsage.HPRecoverAll || itemUse.usetype[i] == MainManager.ItemUsage.ReviveAll)
			{
				return new int?(-itemUse.values[i]);
			}
			if (itemUse.usetype[i] == MainManager.ItemUsage.HPRecoverFull)
			{
				return new int?(999);
			}
		}
		return null;
	}

	// Token: 0x06000632 RID: 1586 RVA: 0x0004212C File Offset: 0x0004032C
	public static int DoItemEffect(MainManager.ItemUsage type, int value, int? characterid)
	{
		switch (type)
		{
		case MainManager.ItemUsage.HPRecover:
		case MainManager.ItemUsage.Revive:
			break;
		case MainManager.ItemUsage.TPRecover:
			goto IL_3D3;
		case MainManager.ItemUsage.HPRecoverAll:
		case MainManager.ItemUsage.ReviveAll:
			goto IL_2B7;
		case MainManager.ItemUsage.HPRecoverFull:
		case MainManager.ItemUsage.TPRecoverFull:
		case MainManager.ItemUsage.Battle:
		case MainManager.ItemUsage.AutoRevive:
		case MainManager.ItemUsage.TPto1:
		case MainManager.ItemUsage.HPto1All:
		case MainManager.ItemUsage.DefUpStat:
		case MainManager.ItemUsage.AtkUpStat:
		case MainManager.ItemUsage.Sturdy:
			goto IL_1044;
		case MainManager.ItemUsage.HPUP:
			MainManager.PlaySound("StatUp");
			MainManager.AddStatBonus(MainManager.StatBonus.HP, value, characterid.Value);
			MainManager.ApplyStatBonus();
			goto IL_1044;
		case MainManager.ItemUsage.TPUP:
			MainManager.PlaySound("StatUp");
			MainManager.AddStatBonus(MainManager.StatBonus.TP, value, -1);
			MainManager.ApplyStatBonus();
			goto IL_1044;
		case MainManager.ItemUsage.AttackUp:
			MainManager.PlaySound("StatUp");
			MainManager.AddStatBonus(MainManager.StatBonus.Attack, value, characterid.Value);
			MainManager.ApplyStatBonus();
			if (MainManager.instance.inbattle && MainManager.BadgeIsEquipped(24, MainManager.instance.playerdata[characterid.Value].trueid))
			{
				goto IL_66B;
			}
			goto IL_1044;
		case MainManager.ItemUsage.DefenseUp:
			MainManager.PlaySound("StatUp");
			MainManager.AddStatBonus(MainManager.StatBonus.Defense, value, characterid.Value);
			MainManager.ApplyStatBonus();
			if (MainManager.instance.inbattle && MainManager.BadgeIsEquipped(24, MainManager.instance.playerdata[characterid.Value].trueid))
			{
				goto IL_66B;
			}
			goto IL_1044;
		case MainManager.ItemUsage.CurePoison:
			MainManager.PlaySound("Heal3");
			MainManager.RemoveCondition(MainManager.BattleCondition.Poison, MainManager.instance.playerdata[characterid.Value]);
			goto IL_1044;
		case MainManager.ItemUsage.CureFreeze:
			MainManager.PlaySound("Heal3");
			MainManager.RemoveCondition(MainManager.BattleCondition.Freeze, MainManager.instance.playerdata[characterid.Value]);
			if (MainManager.instance.inbattle)
			{
				MainManager.instance.playerdata[characterid.Value].battleentity.BreakIce();
				goto IL_1044;
			}
			goto IL_1044;
		case MainManager.ItemUsage.CureNumb:
			MainManager.PlaySound("Heal3");
			MainManager.instance.playerdata[characterid.Value].isnumb = false;
			MainManager.RemoveCondition(MainManager.BattleCondition.Numb, MainManager.instance.playerdata[characterid.Value]);
			goto IL_1044;
		case MainManager.ItemUsage.CureSleep:
			MainManager.PlaySound("Heal3");
			MainManager.instance.playerdata[characterid.Value].isasleep = false;
			MainManager.RemoveCondition(MainManager.BattleCondition.Sleep, MainManager.instance.playerdata[characterid.Value]);
			goto IL_1044;
		case MainManager.ItemUsage.CureParty:
			MainManager.PlaySound("Heal3");
			for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
			{
				if (MainManager.instance.playerdata[i].hp > 0 && MainManager.instance.playerdata[i].eatenby == null)
				{
					MainManager.RemoveCondition(MainManager.BattleCondition.Poison, MainManager.instance.playerdata[i]);
					MainManager.RemoveCondition(MainManager.BattleCondition.Sleep, MainManager.instance.playerdata[i]);
					MainManager.RemoveCondition(MainManager.BattleCondition.Freeze, MainManager.instance.playerdata[i]);
					MainManager.RemoveCondition(MainManager.BattleCondition.Numb, MainManager.instance.playerdata[i]);
					MainManager.RemoveCondition(MainManager.BattleCondition.Fire, MainManager.instance.playerdata[i]);
					MainManager.RemoveCondition(MainManager.BattleCondition.Sticky, MainManager.instance.playerdata[i]);
					MainManager.RemoveCondition(MainManager.BattleCondition.Inked, MainManager.instance.playerdata[i]);
					if (MainManager.instance.inbattle)
					{
						if (MainManager.instance.playerdata[i].battleentity.firepart != null)
						{
							Object.Destroy(MainManager.instance.playerdata[i].battleentity.firepart.gameObject);
						}
						MainManager.instance.playerdata[i].battleentity.BreakIce();
						MainManager.instance.playerdata[i].isasleep = false;
						MainManager.instance.playerdata[i].isnumb = false;
					}
				}
			}
			goto IL_1044;
		case MainManager.ItemUsage.AddPoison:
			goto IL_66B;
		case MainManager.ItemUsage.AddSleep:
			if (!MainManager.instance.inbattle || MainManager.HasCondition(MainManager.BattleCondition.Sturdy, MainManager.instance.playerdata[characterid.Value]) > 0)
			{
				goto IL_1044;
			}
			MainManager.PlaySound("Sleep");
			if (!MainManager.BadgeIsEquipped(12, MainManager.instance.playerdata[characterid.Value].trueid) && !MainManager.BadgeIsEquipped(66, MainManager.instance.playerdata[characterid.Value].trueid))
			{
				MainManager.SetCondition(MainManager.BattleCondition.Sleep, ref MainManager.instance.playerdata[characterid.Value], value);
				goto IL_1044;
			}
			goto IL_1044;
		case MainManager.ItemUsage.AddNumb:
			if (!MainManager.instance.inbattle || MainManager.HasCondition(MainManager.BattleCondition.Sturdy, MainManager.instance.playerdata[characterid.Value]) > 0)
			{
				goto IL_1044;
			}
			MainManager.PlaySound("Shock");
			if (!MainManager.BadgeIsEquipped(21, MainManager.instance.playerdata[characterid.Value].trueid) && !MainManager.BadgeIsEquipped(66, MainManager.instance.playerdata[characterid.Value].trueid))
			{
				MainManager.SetCondition(MainManager.BattleCondition.Numb, ref MainManager.instance.playerdata[characterid.Value], value);
				goto IL_1044;
			}
			goto IL_1044;
		case MainManager.ItemUsage.AddFreeze:
			if (!MainManager.instance.inbattle || MainManager.HasCondition(MainManager.BattleCondition.Sturdy, MainManager.instance.playerdata[characterid.Value]) > 0)
			{
				goto IL_1044;
			}
			MainManager.PlaySound("Freeze");
			if (!MainManager.BadgeIsEquipped(33, MainManager.instance.playerdata[characterid.Value].trueid) && !MainManager.BadgeIsEquipped(66, MainManager.instance.playerdata[characterid.Value].trueid))
			{
				MainManager.SetCondition(MainManager.BattleCondition.Freeze, ref MainManager.instance.playerdata[characterid.Value], value);
				goto IL_1044;
			}
			goto IL_1044;
		case MainManager.ItemUsage.HPto1:
			if (!MainManager.instance.inbattle)
			{
				MainManager.PlaySound("Damage0");
			}
			MainManager.instance.playerdata[characterid.Value].hp = 1;
			goto IL_1044;
		case MainManager.ItemUsage.GradualHP:
			if (MainManager.instance.inbattle)
			{
				MainManager.PlaySound("Heal3");
				MainManager.SetCondition(MainManager.BattleCondition.GradualHP, ref MainManager.instance.playerdata[characterid.Value], value);
				if (MainManager.instance.inbattle && MainManager.BadgeIsEquipped(24, MainManager.instance.playerdata[characterid.Value].trueid))
				{
					goto IL_66B;
				}
				goto IL_1044;
			}
			break;
		case MainManager.ItemUsage.GradualTP:
			if (!MainManager.instance.inbattle)
			{
				goto IL_3D3;
			}
			MainManager.PlaySound("Heal3");
			MainManager.SetCondition(MainManager.BattleCondition.GradualTP, ref MainManager.instance.playerdata[characterid.Value], value);
			if (!MainManager.instance.inbattle || !MainManager.BadgeIsEquipped(24, MainManager.instance.playerdata[characterid.Value].trueid))
			{
				goto IL_1044;
			}
			goto IL_66B;
		case MainManager.ItemUsage.GradualHPParty:
			if (!MainManager.instance.inbattle)
			{
				goto IL_2B7;
			}
			MainManager.PlaySound("Heal3");
			for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
			{
				if (MainManager.instance.playerdata[j].hp > 0)
				{
					MainManager.SetCondition(MainManager.BattleCondition.GradualHP, ref MainManager.instance.playerdata[j], value);
				}
			}
			if (MainManager.instance.inbattle && MainManager.BadgeIsEquipped(24, MainManager.instance.playerdata[characterid.Value].trueid))
			{
				goto IL_66B;
			}
			goto IL_1044;
		case MainManager.ItemUsage.HPUPAll:
			MainManager.PlaySound("StatUp");
			MainManager.AddStatBonus(MainManager.StatBonus.HP, value, -1);
			MainManager.ApplyStatBonus();
			goto IL_1044;
		case MainManager.ItemUsage.MPUP:
			MainManager.PlaySound("StatUp");
			MainManager.AddStatBonus(MainManager.StatBonus.MP, value, -1);
			MainManager.instance.maxbp += value;
			MainManager.instance.bp += value;
			MainManager.ApplyStatBonus();
			goto IL_1044;
		case MainManager.ItemUsage.ChargeUp:
			MainManager.instance.playerdata[characterid.Value].charge = Mathf.Clamp(MainManager.instance.playerdata[characterid.Value].charge + 1, 0, 3);
			if (MainManager.instance.inbattle && MainManager.BadgeIsEquipped(24, MainManager.instance.playerdata[characterid.Value].trueid))
			{
				goto IL_66B;
			}
			goto IL_1044;
		case MainManager.ItemUsage.AtkDownAfter:
			MainManager.instance.playerdata[characterid.Value].atkdownonloseatkup = true;
			goto IL_1044;
		case MainManager.ItemUsage.CureFire:
			MainManager.PlaySound("Heal3");
			MainManager.RemoveCondition(MainManager.BattleCondition.Fire, MainManager.instance.playerdata[characterid.Value]);
			if (MainManager.instance.inbattle && MainManager.instance.playerdata[characterid.Value].battleentity.firepart != null)
			{
				Object.Destroy(MainManager.instance.playerdata[characterid.Value].battleentity.firepart.gameObject);
				goto IL_1044;
			}
			goto IL_1044;
		case MainManager.ItemUsage.CureAll:
			MainManager.PlaySound("Heal3");
			MainManager.RemoveCondition(MainManager.BattleCondition.Poison, MainManager.instance.playerdata[characterid.Value]);
			MainManager.RemoveCondition(MainManager.BattleCondition.Sleep, MainManager.instance.playerdata[characterid.Value]);
			MainManager.RemoveCondition(MainManager.BattleCondition.Freeze, MainManager.instance.playerdata[characterid.Value]);
			MainManager.RemoveCondition(MainManager.BattleCondition.Numb, MainManager.instance.playerdata[characterid.Value]);
			MainManager.RemoveCondition(MainManager.BattleCondition.Fire, MainManager.instance.playerdata[characterid.Value]);
			MainManager.RemoveCondition(MainManager.BattleCondition.Inked, MainManager.instance.playerdata[characterid.Value]);
			MainManager.RemoveCondition(MainManager.BattleCondition.Sticky, MainManager.instance.playerdata[characterid.Value]);
			if (MainManager.instance.inbattle)
			{
				if (MainManager.instance.playerdata[characterid.Value].battleentity.firepart != null)
				{
					Object.Destroy(MainManager.instance.playerdata[characterid.Value].battleentity.firepart.gameObject);
				}
				MainManager.instance.playerdata[characterid.Value].battleentity.BreakIce();
				MainManager.instance.playerdata[characterid.Value].isasleep = false;
				MainManager.instance.playerdata[characterid.Value].isnumb = false;
				goto IL_1044;
			}
			goto IL_1044;
		case MainManager.ItemUsage.TurnNextTurn:
			MainManager.PlaySound("Heal3");
			if (MainManager.instance.inbattle)
			{
				MainManager.BattleData[] array = MainManager.instance.playerdata;
				int value2 = characterid.Value;
				array[value2].moreturnnextturn = array[value2].moreturnnextturn + 1;
				goto IL_1044;
			}
			goto IL_1044;
		case MainManager.ItemUsage.HPorDamage:
			if (MainManager.battle == null)
			{
				if (characterid != null)
				{
					value += MainManager.BadgeHowManyEquipped(74, MainManager.lastitemuser);
				}
				if (Random.Range(0, 100) > 40)
				{
					MainManager.PlaySound("Heal");
					MainManager.instance.playerdata[characterid.Value].hp = Mathf.Clamp(MainManager.instance.playerdata[characterid.Value].hp + value, 0, MainManager.instance.playerdata[characterid.Value].maxhp);
				}
				else
				{
					MainManager.PlaySound("Damage0");
					MainManager.instance.playerdata[characterid.Value].hp = Mathf.Clamp(MainManager.instance.playerdata[characterid.Value].hp - value, 1, MainManager.instance.playerdata[characterid.Value].maxhp);
				}
			}
			if (MainManager.instance.inbattle && MainManager.BadgeIsEquipped(24, MainManager.instance.playerdata[characterid.Value].trueid))
			{
				goto IL_66B;
			}
			goto IL_1044;
		case MainManager.ItemUsage.CurePoisonAll:
			MainManager.PlaySound("Heal3");
			for (int k = 0; k < MainManager.instance.playerdata.Length; k++)
			{
				MainManager.RemoveCondition(MainManager.BattleCondition.Poison, MainManager.instance.playerdata[k]);
			}
			goto IL_1044;
		default:
			goto IL_1044;
		}
		MainManager.PlaySound("Heal");
		if (type == MainManager.ItemUsage.GradualHP)
		{
			value *= 2;
		}
		if (characterid != null && MainManager.instance.inbattle)
		{
			value += MainManager.BadgeHowManyEquipped(74, MainManager.lastitemuser);
		}
		MainManager.instance.playerdata[characterid.Value].hp = Mathf.Clamp(MainManager.instance.playerdata[characterid.Value].hp + value, 0, MainManager.instance.playerdata[characterid.Value].maxhp);
		if (MainManager.instance.inbattle && MainManager.BadgeIsEquipped(24, MainManager.instance.playerdata[characterid.Value].trueid))
		{
			goto IL_66B;
		}
		goto IL_1044;
		IL_2B7:
		if (type == MainManager.ItemUsage.GradualHPParty)
		{
			value *= 2;
		}
		MainManager.PlaySound("Heal");
		if (characterid != null && MainManager.instance.inbattle)
		{
			value += MainManager.BadgeHowManyEquipped(74, MainManager.lastitemuser);
		}
		for (int l = 0; l < MainManager.instance.playerdata.Length; l++)
		{
			if (MainManager.instance.playerdata[l].eatenby == null && (type == MainManager.ItemUsage.ReviveAll || MainManager.instance.playerdata[l].hp > 0))
			{
				MainManager.instance.playerdata[l].hp = Mathf.Clamp(MainManager.instance.playerdata[l].hp + value, 0, MainManager.instance.playerdata[l].maxhp);
				if (MainManager.instance.inbattle && MainManager.BadgeIsEquipped(24, MainManager.instance.playerdata[l].trueid))
				{
					MainManager.DoItemEffect(MainManager.ItemUsage.AddPoison, 2, new int?(l));
				}
			}
		}
		goto IL_1044;
		IL_3D3:
		if (type == MainManager.ItemUsage.GradualTP)
		{
			value *= 2;
		}
		MainManager.PlaySound("Heal2");
		MainManager.instance.tp = Mathf.Clamp(MainManager.instance.tp + value, 0, MainManager.instance.maxtp);
		if (!MainManager.instance.inbattle || !MainManager.BadgeIsEquipped(24, MainManager.instance.playerdata[characterid.Value].trueid))
		{
			goto IL_1044;
		}
		IL_66B:
		if (MainManager.instance.inbattle && MainManager.HasCondition(MainManager.BattleCondition.Sturdy, MainManager.instance.playerdata[characterid.Value]) <= 0)
		{
			MainManager.PlaySound("Poison");
			if (!MainManager.BadgeIsEquipped(7, MainManager.instance.playerdata[characterid.Value].trueid) && !MainManager.BadgeIsEquipped(66, MainManager.instance.playerdata[characterid.Value].trueid))
			{
				MainManager.SetCondition(MainManager.BattleCondition.Poison, ref MainManager.instance.playerdata[characterid.Value], MainManager.BadgeIsEquipped(27, MainManager.instance.playerdata[characterid.Value].trueid) ? 9999999 : (((type == MainManager.ItemUsage.AddPoison) ? value : 2) + (MainManager.BadgeIsEquipped(24, MainManager.instance.playerdata[characterid.Value].trueid) ? 1 : 0)));
			}
		}
		IL_1044:
		if (MainManager.battle != null)
		{
			MainManager.battle.UpdateConditionIcons();
		}
		return value;
	}

	// Token: 0x06000633 RID: 1587 RVA: 0x00043198 File Offset: 0x00041398
	public static void IgnoreRadius(Collider col, bool ignore)
	{
		NPCControl[] array = Object.FindObjectsOfType<NPCControl>();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				Physics.IgnoreCollision(col, array[i].scol, ignore);
			}
		}
	}

	// Token: 0x06000634 RID: 1588 RVA: 0x000431CC File Offset: 0x000413CC
	private static Sprite GetButtonSpriteD(int id)
	{
		switch (MainManager.forcejoystick)
		{
		default:
			return null;
		}
	}

	// Token: 0x06000635 RID: 1589 RVA: 0x000431F4 File Offset: 0x000413F4
	public static void SwitchParty(bool battle)
	{
		if (!battle && MainManager.instance.inevent)
		{
			return;
		}
		int num = 0;
		if (MainManager.battle == null)
		{
			MainManager.PlaySound("Switch");
		}
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			if (i == 0)
			{
				if (battle)
				{
					num = MainManager.battle.partypointer[i];
				}
				else
				{
					num = MainManager.instance.playerdata[i].animid;
				}
			}
			int num2 = i + 1;
			if (num2 >= MainManager.instance.playerdata.Length)
			{
				num2 = 0;
			}
			if (!battle)
			{
				MainManager.instance.playerdata[i].animid = MainManager.instance.playerdata[num2].animid;
				if (i == MainManager.instance.playerdata.Length - 1)
				{
					MainManager.instance.playerdata[i].animid = num;
				}
			}
			else
			{
				MainManager.battle.partypointer[i] = MainManager.battle.partypointer[num2];
				if (i == MainManager.instance.playerdata.Length - 1)
				{
					MainManager.battle.partypointer[i] = num;
				}
			}
			if (!battle)
			{
				if (MainManager.instance.switchicon != null)
				{
					MainManager.instance.switchicon.sprite = MainManager.guisprites[MainManager.instance.playerdata[0].animid + 94];
					MainManager.instance.switchicon.color = new Color(1f, 1f, 1f, 0.5f);
				}
				MainManager.instance.playerdata[0].entity.emoticonoffset = new Vector3(0f, 1.8f + 0.25f * (float)MainManager.instance.playerdata[0].animid, -0.1f);
			}
		}
	}

	// Token: 0x06000636 RID: 1590 RVA: 0x000433CC File Offset: 0x000415CC
	public static EntityControl GetEntity(string id)
	{
		return MainManager.GetEntity(id, null);
	}

	// Token: 0x06000637 RID: 1591 RVA: 0x000433D8 File Offset: 0x000415D8
	public static EntityControl GetEntity(string id, EntityControl caller)
	{
		if (!MainManager.instance.inbattle)
		{
			if (id == "this")
			{
				return MainManager.instance.tailtarget.GetComponent<EntityControl>();
			}
			if (id == "caller")
			{
				return caller;
			}
			if (MainManager.define != null && MainManager.define.Count > 0)
			{
				string[][] array = MainManager.define.ToArray();
				for (int i = 0; i < MainManager.define.Count; i++)
				{
					if (array[i][0].ToLower() == id.ToLower())
					{
						return MainManager.GetEntity(Convert.ToInt32(array[i][1]));
					}
				}
			}
		}
		return MainManager.GetEntity(Convert.ToInt32(id));
	}

	// Token: 0x06000638 RID: 1592 RVA: 0x00043488 File Offset: 0x00041688
	public static int[] PrizeBadges(bool caravan)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < MainManager.instance.prizeflags.Length; i++)
		{
			if ((caravan && MainManager.instance.flagvar[MainManager.instance.prizeflags[i]] == 2) || (!caravan && MainManager.instance.flagvar[MainManager.instance.prizeflags[i]] == 1))
			{
				list.Add(caravan ? MainManager.instance.prizeids[i] : MainManager.instance.prizeflags[i]);
			}
		}
		if (list.Count > 0)
		{
			return list.ToArray();
		}
		return null;
	}

	// Token: 0x06000639 RID: 1593 RVA: 0x00043520 File Offset: 0x00041720
	public static int GetEnemyPrizeID(int flagvalue)
	{
		for (int i = 0; i < MainManager.instance.prizeflags.Length; i++)
		{
			if (MainManager.instance.prizeflags[i] == flagvalue)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x0600063A RID: 1594 RVA: 0x00043556 File Offset: 0x00041756
	public static EntityControl[] GetEntities(int[] ids)
	{
		return MainManager.GetEntities(ids, null);
	}

	// Token: 0x0600063B RID: 1595 RVA: 0x00043560 File Offset: 0x00041760
	public static EntityControl[] GetEntities(int[] ids, EntityControl[] ads)
	{
		List<EntityControl> list = new List<EntityControl>();
		for (int i = 0; i < ids.Length; i++)
		{
			list.Add(MainManager.GetEntity(ids[i]));
		}
		if (ads != null)
		{
			list.AddRange(ads);
		}
		return list.ToArray();
	}

	// Token: 0x0600063C RID: 1596 RVA: 0x0004359F File Offset: 0x0004179F
	public static Vector3 SmoothLerp(Vector3 a, Vector3 b, float t)
	{
		return MainManager.SmoothLerp(a, b, t, 0f, 0f);
	}

	// Token: 0x0600063D RID: 1597 RVA: 0x000435B4 File Offset: 0x000417B4
	public static Vector3 SmoothLerp(Vector3 a, Vector3 b, float t, float onlythrough, float onlyafter)
	{
		if (onlythrough > 0f && t < onlythrough)
		{
			return Vector3.Lerp(a, b, t);
		}
		if (onlyafter > 0f && t > onlyafter)
		{
			return Vector3.Lerp(a, b, t);
		}
		return new Vector3(Mathf.SmoothStep(a.x, b.x, t), Mathf.SmoothStep(a.y, b.y, t), Mathf.SmoothStep(a.z, b.z, t));
	}

	// Token: 0x0600063E RID: 1598 RVA: 0x00043628 File Offset: 0x00041828
	public static EntityControl GetEntity(int id)
	{
		if (id >= 1000)
		{
			return MainManager.map.tempfollowers[id - 1000];
		}
		if (!MainManager.instance.inbattle)
		{
			if (id == -1)
			{
				if (MainManager.instance.playerdata.Length != 0 && MainManager.instance.playerdata[0].entity != null)
				{
					return MainManager.instance.playerdata[0].entity;
				}
			}
			else if (id == -2)
			{
				if (MainManager.instance.playerdata.Length > 1 && MainManager.instance.playerdata[1].entity != null)
				{
					return MainManager.instance.playerdata[1].entity;
				}
			}
			else if (id == -3)
			{
				if (MainManager.instance.playerdata.Length > 2 && MainManager.instance.playerdata[2].entity != null)
				{
					return MainManager.instance.playerdata[2].entity;
				}
			}
			else if (id == -4)
			{
				for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
				{
					if (MainManager.instance.playerdata[i].entity.animid == 0)
					{
						return MainManager.instance.playerdata[i].entity;
					}
				}
			}
			else if (id == -5)
			{
				for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
				{
					if (MainManager.instance.playerdata[j].entity.animid == 1)
					{
						return MainManager.instance.playerdata[j].entity;
					}
				}
			}
			else if (id == -6)
			{
				for (int k = 0; k < MainManager.instance.playerdata.Length; k++)
				{
					if (MainManager.instance.playerdata[k].entity.animid == 2)
					{
						return MainManager.instance.playerdata[k].entity;
					}
				}
			}
			else if (MainManager.map != null && id < MainManager.map.entities.Length && id >= 0 && MainManager.map.entities[id] != null)
			{
				return MainManager.map.entities[id];
			}
		}
		else if (id < 0)
		{
			if (MainManager.battle.extraentities != null || MainManager.battle.extraentities.Length != 0)
			{
				return MainManager.battle.extraentities[Mathf.Abs(id + 1)];
			}
		}
		else
		{
			if (id < MainManager.instance.playerdata.Length)
			{
				return MainManager.instance.playerdata[id].battleentity;
			}
			return MainManager.battle.enemydata[id - MainManager.instance.playerdata.Length].battleentity;
		}
		return null;
	}

	// Token: 0x040003F6 RID: 1014
	public static readonly string[] languagenames = new string[]
	{
		"English (US)",
		"Español (LA)",
		"Português (BR)",
		"日本語 (Japanese)",
		"Deutsch (German)",
		"한국어 (Korean)",
		"Русский (Russian)"
	};

	// Token: 0x040003F7 RID: 1015
	public static string[] languagehelp;

	// Token: 0x040003F8 RID: 1016
	public static MainManager instance;

	// Token: 0x040003F9 RID: 1017
	public static bool[] secretunlocks = new bool[5];

	// Token: 0x040003FA RID: 1018
	public static MapControl map;

	// Token: 0x040003FB RID: 1019
	public static Camera GUICamera;

	// Token: 0x040003FC RID: 1020
	public static Camera MainCamera;

	// Token: 0x040003FD RID: 1021
	public static PlayerControl player;

	// Token: 0x040003FE RID: 1022
	public static BattleControl battle;

	// Token: 0x040003FF RID: 1023
	public SpriteRenderer cursor;

	// Token: 0x04000400 RID: 1024
	public SpriteRenderer blinker;

	// Token: 0x04000401 RID: 1025
	public SpriteRenderer switchicon;

	// Token: 0x04000402 RID: 1026
	public static Coroutine transition;

	// Token: 0x04000403 RID: 1027
	public static Coroutine hudmovement;

	// Token: 0x04000404 RID: 1028
	public static Coroutine musiccoroutine;

	// Token: 0x04000405 RID: 1029
	public static Shader fakelight;

	// Token: 0x04000406 RID: 1030
	public static Shader shadowcaster;

	// Token: 0x04000407 RID: 1031
	public static PauseMenu pausemenu;

	// Token: 0x04000408 RID: 1032
	private static TextMesh[] letterpool;

	// Token: 0x04000409 RID: 1033
	private static float[] keyhold = new float[2];

	// Token: 0x0400040A RID: 1034
	public static int[] joybinds = new int[]
	{
		-55,
		-55,
		-55,
		-55,
		-55,
		-55,
		-55,
		-55,
		-55,
		-55
	};

	// Token: 0x0400040B RID: 1035
	public static int[] caravanorder;

	// Token: 0x0400040C RID: 1036
	public static Font[] fonts;

	// Token: 0x0400040D RID: 1037
	public static Material[] fontmat;

	// Token: 0x0400040E RID: 1038
	public MainManager.BattleData[] playerdata;

	// Token: 0x0400040F RID: 1039
	private static SpriteRenderer[] letterbox;

	// Token: 0x04000410 RID: 1040
	private static SpriteRenderer[] hudsprites;

	// Token: 0x04000411 RID: 1041
	public List<MainManager.BattleData> reserveplayers;

	// Token: 0x04000412 RID: 1042
	public static Material spritemat;

	// Token: 0x04000413 RID: 1043
	public static Material holosprite;

	// Token: 0x04000414 RID: 1044
	public static Material spritematlit;

	// Token: 0x04000415 RID: 1045
	public static Material spritedefaultunity;

	// Token: 0x04000416 RID: 1046
	public static Material grayscale;

	// Token: 0x04000417 RID: 1047
	public static Material outlinemain;

	// Token: 0x04000418 RID: 1048
	public static Material Main3D;

	// Token: 0x04000419 RID: 1049
	public static Material Fade3D;

	// Token: 0x0400041A RID: 1050
	public static Material mainPlane;

	// Token: 0x0400041B RID: 1051
	public static Material fadePlane;

	// Token: 0x0400041C RID: 1052
	public static Material windShader;

	// Token: 0x0400041D RID: 1053
	public static Material emptymat;

	// Token: 0x0400041E RID: 1054
	public static PhysicMaterial defaultpmat;

	// Token: 0x0400041F RID: 1055
	public Transform camtarget;

	// Token: 0x04000420 RID: 1056
	public Transform promptbox;

	// Token: 0x04000421 RID: 1057
	public Transform globalcamdir;

	// Token: 0x04000422 RID: 1058
	public Transform texttail;

	// Token: 0x04000423 RID: 1059
	public Transform tailtarget;

	// Token: 0x04000424 RID: 1060
	public Transform itemlist;

	// Token: 0x04000425 RID: 1061
	public Transform npromptholder;

	// Token: 0x04000426 RID: 1062
	public Transform questboardobj;

	// Token: 0x04000427 RID: 1063
	public Transform discoverymessage;

	// Token: 0x04000428 RID: 1064
	public Transform[] hud;

	// Token: 0x04000429 RID: 1065
	public Transform[] transitionobj;

	// Token: 0x0400042A RID: 1066
	private DynamicFont[] hudfont;

	// Token: 0x0400042B RID: 1067
	public static ParticleSystem hitpart;

	// Token: 0x0400042C RID: 1068
	public static ParticleSystem deathpart;

	// Token: 0x0400042D RID: 1069
	public NPCControl boardcaller;

	// Token: 0x0400042E RID: 1070
	public List<int>[] items;

	// Token: 0x0400042F RID: 1071
	public List<int>[] boardquests;

	// Token: 0x04000430 RID: 1072
	public List<int>[] badgeshops;

	// Token: 0x04000431 RID: 1073
	public List<int>[] avaliablebadgepool;

	// Token: 0x04000432 RID: 1074
	public List<int> extrafollowers;

	// Token: 0x04000433 RID: 1075
	public List<int> lastdefeated;

	// Token: 0x04000434 RID: 1076
	public List<int> multiselect;

	// Token: 0x04000435 RID: 1077
	public List<int[]> badges;

	// Token: 0x04000436 RID: 1078
	public List<int[]> samiramusics;

	// Token: 0x04000437 RID: 1079
	public List<int[]> statbonus;

	// Token: 0x04000438 RID: 1080
	public static int[] overridedlist = null;

	// Token: 0x04000439 RID: 1081
	public static int[] discoveryicons;

	// Token: 0x0400043A RID: 1082
	public static int[] achiveicons;

	// Token: 0x0400043B RID: 1083
	public static int[] librarylimit = new int[]
	{
		50,
		92,
		70,
		30,
		100
	};

	// Token: 0x0400043C RID: 1084
	public static int[] listvar;

	// Token: 0x0400043D RID: 1085
	public static int[] badgeorder;

	// Token: 0x0400043E RID: 1086
	public static int[] musicids;

	// Token: 0x0400043F RID: 1087
	public static int[] settingsindex = new int[]
	{
		33,
		34,
		160,
		28,
		29,
		32,
		116,
		30,
		31,
		147,
		140,
		156,
		80,
		157,
		35,
		36,
		183,
		222,
		231,
		239,
		245,
		261,
		270,
		255,
		256,
		282
	};

	// Token: 0x04000440 RID: 1088
	public static int[,] libraryorder;

	// Token: 0x04000441 RID: 1089
	public static int[,] recipedata;

	// Token: 0x04000442 RID: 1090
	public static int[,] termacadeprize;

	// Token: 0x04000443 RID: 1091
	private static int[][] questchecks;

	// Token: 0x04000444 RID: 1092
	public static string[] menutext;

	// Token: 0x04000445 RID: 1093
	public static string[] enemynames;

	// Token: 0x04000446 RID: 1094
	public static string[] commondialogue;

	// Token: 0x04000447 RID: 1095
	public static string[] commandhelptext;

	// Token: 0x04000448 RID: 1096
	public static string[] areanames;

	// Token: 0x04000449 RID: 1097
	public static string[] musicnames;

	// Token: 0x0400044A RID: 1098
	public static string[,] skilldata;

	// Token: 0x0400044B RID: 1099
	public static string[,] badgedata;

	// Token: 0x0400044C RID: 1100
	public static string[,] boardquestdata;

	// Token: 0x0400044D RID: 1101
	public static string[,] enemydata;

	// Token: 0x0400044E RID: 1102
	public static string[,,] itemdata;

	// Token: 0x0400044F RID: 1103
	public static string[,,] librarydata;

	// Token: 0x04000450 RID: 1104
	public static Sprite[] guisprites;

	// Token: 0x04000451 RID: 1105
	public static Sprite[] cursorsprite;

	// Token: 0x04000452 RID: 1106
	public static Sprite[] grasssprite;

	// Token: 0x04000453 RID: 1107
	public static Sprite[] leafsprites;

	// Token: 0x04000454 RID: 1108
	public static Sprite[] battlemessage;

	// Token: 0x04000455 RID: 1109
	public static Sprite[] librarysprites;

	// Token: 0x04000456 RID: 1110
	private static Sprite[] textboxsprites;

	// Token: 0x04000457 RID: 1111
	public static Sprite[,] itemsprites;

	// Token: 0x04000458 RID: 1112
	public Sprite[] joybuttons;

	// Token: 0x04000459 RID: 1113
	public Sprite[] joybuttonsps;

	// Token: 0x0400045A RID: 1114
	public Sprite[] dynamicfont;

	// Token: 0x0400045B RID: 1115
	public Sprite[] conditionsprites;

	// Token: 0x0400045C RID: 1116
	public Sprite[] projectilepsrites;

	// Token: 0x0400045D RID: 1117
	public static Sprite shadowsprite;

	// Token: 0x0400045E RID: 1118
	public Color[] textcolors = new Color[]
	{
		Color.black,
		Color.red,
		Color.green,
		Color.blue,
		Color.white,
		Color.gray,
		Color.yellow
	};

	// Token: 0x0400045F RID: 1119
	public Color[] charcolor = new Color[]
	{
		Color.yellow,
		Color.green,
		new Color(0f, 0.75f, 0.9f, 1f)
	};

	// Token: 0x04000460 RID: 1120
	public Color[] menucolors;

	// Token: 0x04000461 RID: 1121
	public Color[] questcolors = new Color[]
	{
		Color.green,
		Color.yellow,
		Color.red
	};

	// Token: 0x04000462 RID: 1122
	private static char[] letters;

	// Token: 0x04000463 RID: 1123
	public static GameObject maintextbox;

	// Token: 0x04000464 RID: 1124
	public static AudioSource[] sounds;

	// Token: 0x04000465 RID: 1125
	public static AudioSource[] music;

	// Token: 0x04000466 RID: 1126
	private static AudioSource bleeps;

	// Token: 0x04000467 RID: 1127
	public const int totalenemies = 4;

	// Token: 0x04000468 RID: 1128
	public const int flaglimit = 10;

	// Token: 0x04000469 RID: 1129
	public const int diaglimit = 20;

	// Token: 0x0400046A RID: 1130
	public const int maxdata = 10;

	// Token: 0x0400046B RID: 1131
	public const int max9box = 5;

	// Token: 0x0400046C RID: 1132
	public const int maxitemtypes = 1;

	// Token: 0x0400046D RID: 1133
	public const int totalitems = 256;

	// Token: 0x0400046E RID: 1134
	public const int moneymax = 999;

	// Token: 0x0400046F RID: 1135
	public const int hpmax = 999;

	// Token: 0x04000470 RID: 1136
	public const int tpmax = 99;

	// Token: 0x04000471 RID: 1137
	public const int regionalammount = 100;

	// Token: 0x04000472 RID: 1138
	public const int maxdamage = 99;

	// Token: 0x04000473 RID: 1139
	public const int chargelimit = 3;

	// Token: 0x04000474 RID: 1140
	public const int maxfpsv = 2;

	// Token: 0x04000475 RID: 1141
	public const int saveslots = 3;

	// Token: 0x04000476 RID: 1142
	public const int maxletters = 500;

	// Token: 0x04000477 RID: 1143
	public const int itemtime = 600;

	// Token: 0x04000478 RID: 1144
	public const int levelcap = 27;

	// Token: 0x04000479 RID: 1145
	public const int lowhp = 4;

	// Token: 0x0400047A RID: 1146
	public const int defaultpartqueue = 3000;

	// Token: 0x0400047B RID: 1147
	public const int beehs = 4500;

	// Token: 0x0400047C RID: 1148
	public const int mkhs = 9500;

	// Token: 0x0400047D RID: 1149
	public const int maxmedals = 120;

	// Token: 0x0400047E RID: 1150
	public const float kerning = -2f;

	// Token: 0x0400047F RID: 1151
	public const float lineheight = 0.7f;

	// Token: 0x04000480 RID: 1152
	public const float spacing = 0.3f;

	// Token: 0x04000481 RID: 1153
	public const float punctuationdelay = 0.15f;

	// Token: 0x04000482 RID: 1154
	public const float defaultcamspeed = 0.1f;

	// Token: 0x04000483 RID: 1155
	public const float defaultcamoffsetspeed = 0.1f;

	// Token: 0x04000484 RID: 1156
	public const float defaultcamanglespeed = 0.1f;

	// Token: 0x04000485 RID: 1157
	public const float letterfixer = 0.07f;

	// Token: 0x04000486 RID: 1158
	public const float wordmulti = 25f;

	// Token: 0x04000487 RID: 1159
	public const float cookspeed = 2.5f;

	// Token: 0x04000488 RID: 1160
	public static Vector2[] resolution = new Vector2[]
	{
		new Vector2(1024f, 576f),
		new Vector2(1152f, 648f),
		new Vector2(1280f, 720f),
		new Vector2(1366f, 768f),
		new Vector2(1600f, 900f),
		new Vector2(1920f, 1080f),
		new Vector2(2560f, 1440f),
		new Vector2(3840f, 2160f)
	};

	// Token: 0x04000489 RID: 1161
	public static Vector2[] leafpos;

	// Token: 0x0400048A RID: 1162
	public static Vector2 defaultlistpos = new Vector2(4f, 0.5f);

	// Token: 0x0400048B RID: 1163
	public static Vector2 listpos;

	// Token: 0x0400048C RID: 1164
	public static Vector2 letteroffset = new Vector2(0f, -0.1f);

	// Token: 0x0400048D RID: 1165
	public static Vector3 bubblepos = new Vector3(0f, 4.15f, 10f);

	// Token: 0x0400048E RID: 1166
	public static Vector3 battlecampos = new Vector3(0f, 3f, -9.25f);

	// Token: 0x0400048F RID: 1167
	public static Vector3 battlecamangle = new Vector3(5f, 0f, 0f);

	// Token: 0x04000490 RID: 1168
	public static Vector3 tempcamangleoffset;

	// Token: 0x04000491 RID: 1169
	public static Vector3 tempcamoffset;

	// Token: 0x04000492 RID: 1170
	public static Vector3 defaultcamoffset = new Vector3(0f, 2.25f, -8.25f);

	// Token: 0x04000493 RID: 1171
	public static Vector3 defaultcamangle = new Vector3(10f, 0f);

	// Token: 0x04000494 RID: 1172
	public static Vector3 screenshake;

	// Token: 0x04000495 RID: 1173
	public static Vector3 entityactive = new Vector3(0.6f, 1.25f, -1f);

	// Token: 0x04000496 RID: 1174
	public static Vector3 tempmaplp;

	// Token: 0x04000497 RID: 1175
	public static Vector3 tempmapln;

	// Token: 0x04000498 RID: 1176
	public static Vector3? tempcampos;

	// Token: 0x04000499 RID: 1177
	public static Transform tempcamtarget;

	// Token: 0x0400049A RID: 1178
	public static Transform listdescbox;

	// Token: 0x0400049B RID: 1179
	public static Transform gamemonitor;

	// Token: 0x0400049C RID: 1180
	public static EventControl events;

	// Token: 0x0400049D RID: 1181
	public CardGame cardgame;

	// Token: 0x0400049E RID: 1182
	private static int lastmusic;

	// Token: 0x0400049F RID: 1183
	public static int languageid = -1;

	// Token: 0x040004A0 RID: 1184
	public static int saveslot;

	// Token: 0x040004A1 RID: 1185
	public static int resolutionindex;

	// Token: 0x040004A2 RID: 1186
	public static int listlow;

	// Token: 0x040004A3 RID: 1187
	public static int listmax;

	// Token: 0x040004A4 RID: 1188
	public static int listammount;

	// Token: 0x040004A5 RID: 1189
	public static int storeid;

	// Token: 0x040004A6 RID: 1190
	public static int listcancel;

	// Token: 0x040004A7 RID: 1191
	public static int listcursor;

	// Token: 0x040004A8 RID: 1192
	public static int listtype;

	// Token: 0x040004A9 RID: 1193
	public static int joyid;

	// Token: 0x040004AA RID: 1194
	public static int fps = 1;

	// Token: 0x040004AB RID: 1195
	public static int musicchannel;

	// Token: 0x040004AC RID: 1196
	public static int listoption;

	// Token: 0x040004AD RID: 1197
	public static int enableoutline = 2;

	// Token: 0x040004AE RID: 1198
	public static int eventtoss = -1;

	// Token: 0x040004AF RID: 1199
	public static int downsample;

	// Token: 0x040004B0 RID: 1200
	public static int lastinside;

	// Token: 0x040004B1 RID: 1201
	public static int particlelevel = 2;

	// Token: 0x040004B2 RID: 1202
	public static int lastsoundid;

	// Token: 0x040004B3 RID: 1203
	public static int vsync = 0;

	// Token: 0x040004B4 RID: 1204
	public static int basefont = 0;

	// Token: 0x040004B5 RID: 1205
	public static int listY;

	// Token: 0x040004B6 RID: 1206
	public static int usejoystick = 1;

	// Token: 0x040004B7 RID: 1207
	public static int forcejoystick = -1;

	// Token: 0x040004B8 RID: 1208
	public static int lastitemuser;

	// Token: 0x040004B9 RID: 1209
	public static int analog = 2;

	// Token: 0x040004BA RID: 1210
	public static int lastevent;

	// Token: 0x040004BB RID: 1211
	public static int? listredirect;

	// Token: 0x040004BC RID: 1212
	public static bool savelastlist;

	// Token: 0x040004BD RID: 1213
	public static bool notextbacktrack;

	// Token: 0x040004BE RID: 1214
	public static bool listdesc;

	// Token: 0x040004BF RID: 1215
	public static bool noskip;

	// Token: 0x040004C0 RID: 1216
	public static bool listsell;

	// Token: 0x040004C1 RID: 1217
	public static bool joystick;

	// Token: 0x040004C2 RID: 1218
	public static bool stickholdx;

	// Token: 0x040004C3 RID: 1219
	public static bool stickholdy;

	// Token: 0x040004C4 RID: 1220
	public static bool fullscreen;

	// Token: 0x040004C5 RID: 1221
	public static bool lowshadows;

	// Token: 0x040004C6 RID: 1222
	public static bool lowtexture;

	// Token: 0x040004C7 RID: 1223
	public static bool halt;

	// Token: 0x040004C8 RID: 1224
	public static bool nowindeffect;

	// Token: 0x040004C9 RID: 1225
	public static bool listcanceled;

	// Token: 0x040004CA RID: 1226
	public static bool battleresult;

	// Token: 0x040004CB RID: 1227
	public static bool battlelossevent;

	// Token: 0x040004CC RID: 1228
	public static bool battlefled;

	// Token: 0x040004CD RID: 1229
	public static bool battlenoexp;

	// Token: 0x040004CE RID: 1230
	public static bool haltbattleload;

	// Token: 0x040004CF RID: 1231
	public static bool battleenemyfled;

	// Token: 0x040004D0 RID: 1232
	public static bool forcecontrollerupdate;

	// Token: 0x040004D1 RID: 1233
	public static bool keepmusicafterbattle = true;

	// Token: 0x040004D2 RID: 1234
	public static bool mashcommandalt;

	// Token: 0x040004D3 RID: 1235
	public static bool debugenalbed;

	// Token: 0x040004D4 RID: 1236
	public static bool monoaudio;

	// Token: 0x040004D5 RID: 1237
	public static bool pauseonfocus;

	// Token: 0x040004D6 RID: 1238
	public static bool roomtransition;

	// Token: 0x040004D7 RID: 1239
	public static bool hudvisible;

	// Token: 0x040004D8 RID: 1240
	public static bool snapTo8 = true;

	// Token: 0x040004D9 RID: 1241
	public static float lasttextcenter;

	// Token: 0x040004DA RID: 1242
	public static float textwidth;

	// Token: 0x040004DB RID: 1243
	public static float tempcamspeed;

	// Token: 0x040004DC RID: 1244
	public static float musicvolume = 0.4f;

	// Token: 0x040004DD RID: 1245
	public static float soundvolume = 0.5f;

	// Token: 0x040004DE RID: 1246
	public static float bleepvolume = 0.5f;

	// Token: 0x040004DF RID: 1247
	public static float framestep;

	// Token: 0x040004E0 RID: 1248
	public static float musicresume = -1f;

	// Token: 0x040004E1 RID: 1249
	public static float messagebreak = 10.5f;

	// Token: 0x040004E2 RID: 1250
	public static float itemdescbreak = 10.5f;

	// Token: 0x040004E3 RID: 1251
	private static float[] hudvalue;

	// Token: 0x040004E4 RID: 1252
	public int tp;

	// Token: 0x040004E5 RID: 1253
	public int tpt;

	// Token: 0x040004E6 RID: 1254
	public int maxtp;

	// Token: 0x040004E7 RID: 1255
	public int basetp;

	// Token: 0x040004E8 RID: 1256
	public int partylevel = 1;

	// Token: 0x040004E9 RID: 1257
	public int partyexp;

	// Token: 0x040004EA RID: 1258
	public int neededexp = 100;

	// Token: 0x040004EB RID: 1259
	public int areaid;

	// Token: 0x040004EC RID: 1260
	public int insideid;

	// Token: 0x040004ED RID: 1261
	public int option;

	// Token: 0x040004EE RID: 1262
	public int maxoptions;

	// Token: 0x040004EF RID: 1263
	public int maxitems;

	// Token: 0x040004F0 RID: 1264
	public int maxstorage = 35;

	// Token: 0x040004F1 RID: 1265
	public int money;

	// Token: 0x040004F2 RID: 1266
	public int bp;

	// Token: 0x040004F3 RID: 1267
	public int maxbp;

	// Token: 0x040004F4 RID: 1268
	public int promptpick;

	// Token: 0x040004F5 RID: 1269
	public int battlestage;

	// Token: 0x040004F6 RID: 1270
	public int moneyt;

	// Token: 0x040004F7 RID: 1271
	public int inmusicrange = -1;

	// Token: 0x040004F8 RID: 1272
	public int entitytouchevent = -1;

	// Token: 0x040004F9 RID: 1273
	public int letterprompt = -1;

	// Token: 0x040004FA RID: 1274
	public int lastPrompt;

	// Token: 0x040004FB RID: 1275
	public int[] flagvar;

	// Token: 0x040004FC RID: 1276
	public int[] promptpointers;

	// Token: 0x040004FD RID: 1277
	public int[] partyorder = new int[]
	{
		0,
		1,
		2
	};

	// Token: 0x040004FE RID: 1278
	public bool[] flags;

	// Token: 0x040004FF RID: 1279
	public bool[] regionalflags;

	// Token: 0x04000500 RID: 1280
	public bool[] crystalbflags;

	// Token: 0x04000501 RID: 1281
	public bool[,] librarystuff;

	// Token: 0x04000502 RID: 1282
	public string[] flagstring;

	// Token: 0x04000503 RID: 1283
	public Vector3 camoffset;

	// Token: 0x04000504 RID: 1284
	public Vector3 camangleoffset;

	// Token: 0x04000505 RID: 1285
	public Vector3 camoffset2;

	// Token: 0x04000506 RID: 1286
	public Vector3[] vectorflags;

	// Token: 0x04000507 RID: 1287
	public Vector3? camtargetpos;

	// Token: 0x04000508 RID: 1288
	public bool prompt;

	// Token: 0x04000509 RID: 1289
	public bool pause;

	// Token: 0x0400050A RID: 1290
	public bool minipause;

	// Token: 0x0400050B RID: 1291
	public bool message;

	// Token: 0x0400050C RID: 1292
	public bool waitinput;

	// Token: 0x0400050D RID: 1293
	public bool inbattle;

	// Token: 0x0400050E RID: 1294
	public bool skiptext;

	// Token: 0x0400050F RID: 1295
	public bool started;

	// Token: 0x04000510 RID: 1296
	public bool intransition;

	// Token: 0x04000511 RID: 1297
	public bool inlist;

	// Token: 0x04000512 RID: 1298
	public bool inevent;

	// Token: 0x04000513 RID: 1299
	public bool overridefollower;

	// Token: 0x04000514 RID: 1300
	public bool hudstats;

	// Token: 0x04000515 RID: 1301
	public bool numberprompt;

	// Token: 0x04000516 RID: 1302
	public bool speedup;

	// Token: 0x04000517 RID: 1303
	public bool camanglechange;

	// Token: 0x04000518 RID: 1304
	public bool changecamspeed;

	// Token: 0x04000519 RID: 1305
	public bool isholdingskip;

	// Token: 0x0400051A RID: 1306
	public bool firstbattleaction;

	// Token: 0x0400051B RID: 1307
	public bool itempicked;

	// Token: 0x0400051C RID: 1308
	public float hudcooldown;

	// Token: 0x0400051D RID: 1309
	public float camspeed;

	// Token: 0x0400051E RID: 1310
	public float showmoney;

	// Token: 0x0400051F RID: 1311
	public float discoveryhud;

	// Token: 0x04000520 RID: 1312
	public float globalcooldown;

	// Token: 0x04000521 RID: 1313
	public float camoffsetspeed;

	// Token: 0x04000522 RID: 1314
	public float camanglespeed;

	// Token: 0x04000523 RID: 1315
	public float inputcooldown;

	// Token: 0x04000524 RID: 1316
	public int[,] enemyencounter;

	// Token: 0x04000525 RID: 1317
	public static float[] downsamples = new float[]
	{
		1f,
		0.9f,
		0.8f,
		0.75f,
		0.6f,
		0.5f,
		0.4f
	};

	// Token: 0x04000526 RID: 1318
	private static List<string> diagstring;

	// Token: 0x04000527 RID: 1319
	private static int currentdialogue;

	// Token: 0x04000528 RID: 1320
	private RenderTexture rtex;

	// Token: 0x04000529 RID: 1321
	private static AudioClip[] asounds;

	// Token: 0x0400052A RID: 1322
	private static AudioClip[] dsounds;

	// Token: 0x0400052B RID: 1323
	private static AudioClip[] msounds;

	// Token: 0x0400052C RID: 1324
	private static Texture[] parttex;

	// Token: 0x0400052D RID: 1325
	private static Sprite[] spritepart;

	// Token: 0x0400052E RID: 1326
	private static GameObject[] partfab;

	// Token: 0x0400052F RID: 1327
	public static bool basicload;

	// Token: 0x04000530 RID: 1328
	public static MainManager.Entity_Data[] endata;

	// Token: 0x04000531 RID: 1329
	public int[] prizeflags;

	// Token: 0x04000532 RID: 1330
	public int[] prizeids;

	// Token: 0x04000533 RID: 1331
	public int[] prizeenemyids;

	// Token: 0x04000534 RID: 1332
	private static bool joytest;

	// Token: 0x04000535 RID: 1333
	public int clocksec;

	// Token: 0x04000536 RID: 1334
	public int clockmin;

	// Token: 0x04000537 RID: 1335
	public int clockhour;

	// Token: 0x04000538 RID: 1336
	public static bool timeddemo;

	// Token: 0x04000539 RID: 1337
	private int demotimer;

	// Token: 0x0400053A RID: 1338
	private Transform textbox;

	// Token: 0x0400053B RID: 1339
	public static readonly string asiansize = "|size,0.8,0.9|";

	// Token: 0x0400053C RID: 1340
	private static string tempdiag;

	// Token: 0x0400053D RID: 1341
	private static float linebr;

	// Token: 0x0400053E RID: 1342
	private static float fontdsize;

	// Token: 0x0400053F RID: 1343
	private static int fontdtype;

	// Token: 0x04000540 RID: 1344
	private static bool backtracking;

	// Token: 0x04000541 RID: 1345
	public static Coroutine templetter;

	// Token: 0x04000542 RID: 1346
	private static int[] bountyquests = new int[]
	{
		9,
		23,
		8,
		21,
		10
	};

	// Token: 0x04000543 RID: 1347
	public static int[] preconfigjoy = new int[]
	{
		0,
		1,
		2,
		3,
		4,
		5,
		6,
		7,
		8
	};

	// Token: 0x04000544 RID: 1348
	public static int[] precjoystring = new int[]
	{
		225,
		226,
		227,
		232,
		233,
		228,
		229,
		242,
		269
	};

	// Token: 0x04000545 RID: 1349
	public static float[][] musicloop;

	// Token: 0x04000546 RID: 1350
	private static List<int> existCheck = new List<int>();

	// Token: 0x04000547 RID: 1351
	private static int[] tphp;

	// Token: 0x04000548 RID: 1352
	private static int[] ptmhp;

	// Token: 0x04000549 RID: 1353
	private static int[] tmtp = new int[2];

	// Token: 0x0400054A RID: 1354
	private static int tempmoneh;

	// Token: 0x0400054B RID: 1355
	public const float distancemultiplier = 1f;

	// Token: 0x0400054C RID: 1356
	public const float holdkeytime = 7f;

	// Token: 0x0400054D RID: 1357
	private const float colormultiplier = 2f;

	// Token: 0x0400054E RID: 1358
	private const float deadzone = 0.5f;

	// Token: 0x0400054F RID: 1359
	public const float defaultcolorspeed = 5.9f;

	// Token: 0x04000550 RID: 1360
	public const float defaultsounddistance = 25f;

	// Token: 0x04000551 RID: 1361
	public static Coroutine chaptername;

	// Token: 0x04000552 RID: 1362
	private static List<string[]> define;

	// Token: 0x04000553 RID: 1363
	private static readonly Vector2Int[] koreanLimit = new Vector2Int[]
	{
		new Vector2Int(3, 21),
		new Vector2Int(24, 44),
		new Vector2Int(45, 72)
	};

	// Token: 0x04000554 RID: 1364
	private static int[] koreanHL = new int[]
	{
		-1,
		-1
	};

	// Token: 0x04000555 RID: 1365
	private static readonly string[] letterPromptHelp = new string[]
	{
		"ひらがな",
		"カタカナ",
		"Русский",
		"한국어",
		"✰ ♡ ♩ $",
		"ABC/123/?!ß"
	};

	// Token: 0x04000556 RID: 1366
	private static Vector3 camposshake;

	// Token: 0x04000557 RID: 1367
	private static readonly Color[] itemlistbg = new Color[]
	{
		new Color(1f, 1f, 1f, 0.75f),
		new Color(0f, 1f, 0f, 0.75f)
	};

	// Token: 0x04000558 RID: 1368
	public int[] multilist;

	// Token: 0x020001FD RID: 509
	private enum Fonts
	{
		// Token: 0x04001693 RID: 5779
		BubblegumSans,
		// Token: 0x04001694 RID: 5780
		D3Streetism,
		// Token: 0x04001695 RID: 5781
		UNUSED,
		// Token: 0x04001696 RID: 5782
		Uzura,
		// Token: 0x04001697 RID: 5783
		BalsamiqSans,
		// Token: 0x04001698 RID: 5784
		ONEMobilePOP
	}

	// Token: 0x020001FE RID: 510
	public enum Directions
	{
		// Token: 0x0400169A RID: 5786
		Up,
		// Token: 0x0400169B RID: 5787
		Down,
		// Token: 0x0400169C RID: 5788
		Left,
		// Token: 0x0400169D RID: 5789
		Right,
		// Token: 0x0400169E RID: 5790
		Confirm,
		// Token: 0x0400169F RID: 5791
		Cancel,
		// Token: 0x040016A0 RID: 5792
		Switch,
		// Token: 0x040016A1 RID: 5793
		Select,
		// Token: 0x040016A2 RID: 5794
		Start,
		// Token: 0x040016A3 RID: 5795
		Help
	}

	// Token: 0x020001FF RID: 511
	public enum Animations
	{
		// Token: 0x040016A5 RID: 5797
		Idle,
		// Token: 0x040016A6 RID: 5798
		Walk,
		// Token: 0x040016A7 RID: 5799
		Jump,
		// Token: 0x040016A8 RID: 5800
		Fall,
		// Token: 0x040016A9 RID: 5801
		ItemGet,
		// Token: 0x040016AA RID: 5802
		Angry,
		// Token: 0x040016AB RID: 5803
		Sad,
		// Token: 0x040016AC RID: 5804
		Upset,
		// Token: 0x040016AD RID: 5805
		Happy,
		// Token: 0x040016AE RID: 5806
		Surprized,
		// Token: 0x040016AF RID: 5807
		Flustered,
		// Token: 0x040016B0 RID: 5808
		Hurt,
		// Token: 0x040016B1 RID: 5809
		Death,
		// Token: 0x040016B2 RID: 5810
		BattleIdle,
		// Token: 0x040016B3 RID: 5811
		Sleep,
		// Token: 0x040016B4 RID: 5812
		Fallen,
		// Token: 0x040016B5 RID: 5813
		HurtFallen,
		// Token: 0x040016B6 RID: 5814
		WeakBattleIdle,
		// Token: 0x040016B7 RID: 5815
		KO,
		// Token: 0x040016B8 RID: 5816
		PickAction,
		// Token: 0x040016B9 RID: 5817
		WeakPickAction,
		// Token: 0x040016BA RID: 5818
		Woobly,
		// Token: 0x040016BB RID: 5819
		HurtWooble,
		// Token: 0x040016BC RID: 5820
		Chase,
		// Token: 0x040016BD RID: 5821
		Block,
		// Token: 0x040016BE RID: 5822
		SleepFallen,
		// Token: 0x040016BF RID: 5823
		AirTackle,
		// Token: 0x040016C0 RID: 5824
		ItemWalk,
		// Token: 0x040016C1 RID: 5825
		TossItem,
		// Token: 0x040016C2 RID: 5826
		Sit,
		// Token: 0x040016C3 RID: 5827
		FakeHurt,
		// Token: 0x040016C4 RID: 5828
		Dig,
		// Token: 0x040016C5 RID: 5829
		DigMove
	}

	// Token: 0x02000200 RID: 512
	public enum Transitions
	{
		// Token: 0x040016C7 RID: 5831
		FadeIn,
		// Token: 0x040016C8 RID: 5832
		FadeOut,
		// Token: 0x040016C9 RID: 5833
		LeafIn,
		// Token: 0x040016CA RID: 5834
		LeafOut,
		// Token: 0x040016CB RID: 5835
		Circle,
		// Token: 0x040016CC RID: 5836
		CircleOut,
		// Token: 0x040016CD RID: 5837
		HexagonIn,
		// Token: 0x040016CE RID: 5838
		HexagonOut
	}

	// Token: 0x02000201 RID: 513
	public enum ItemUsage
	{
		// Token: 0x040016D0 RID: 5840
		None,
		// Token: 0x040016D1 RID: 5841
		HPRecover,
		// Token: 0x040016D2 RID: 5842
		TPRecover,
		// Token: 0x040016D3 RID: 5843
		HPRecoverAll,
		// Token: 0x040016D4 RID: 5844
		HPRecoverFull,
		// Token: 0x040016D5 RID: 5845
		TPRecoverFull,
		// Token: 0x040016D6 RID: 5846
		HPUP,
		// Token: 0x040016D7 RID: 5847
		TPUP,
		// Token: 0x040016D8 RID: 5848
		AttackUp,
		// Token: 0x040016D9 RID: 5849
		DefenseUp,
		// Token: 0x040016DA RID: 5850
		Battle,
		// Token: 0x040016DB RID: 5851
		Revive,
		// Token: 0x040016DC RID: 5852
		ReviveAll,
		// Token: 0x040016DD RID: 5853
		AutoRevive,
		// Token: 0x040016DE RID: 5854
		CurePoison,
		// Token: 0x040016DF RID: 5855
		CureFreeze,
		// Token: 0x040016E0 RID: 5856
		CureNumb,
		// Token: 0x040016E1 RID: 5857
		CureSleep,
		// Token: 0x040016E2 RID: 5858
		CureParty,
		// Token: 0x040016E3 RID: 5859
		AddPoison,
		// Token: 0x040016E4 RID: 5860
		AddSleep,
		// Token: 0x040016E5 RID: 5861
		AddNumb,
		// Token: 0x040016E6 RID: 5862
		AddFreeze,
		// Token: 0x040016E7 RID: 5863
		HPto1,
		// Token: 0x040016E8 RID: 5864
		TPto1,
		// Token: 0x040016E9 RID: 5865
		HPto1All,
		// Token: 0x040016EA RID: 5866
		GradualHP,
		// Token: 0x040016EB RID: 5867
		GradualTP,
		// Token: 0x040016EC RID: 5868
		GradualHPParty,
		// Token: 0x040016ED RID: 5869
		DefUpStat,
		// Token: 0x040016EE RID: 5870
		AtkUpStat,
		// Token: 0x040016EF RID: 5871
		Sturdy,
		// Token: 0x040016F0 RID: 5872
		HPUPAll,
		// Token: 0x040016F1 RID: 5873
		MPUP,
		// Token: 0x040016F2 RID: 5874
		ChargeUp,
		// Token: 0x040016F3 RID: 5875
		AtkDownAfter,
		// Token: 0x040016F4 RID: 5876
		CureFire,
		// Token: 0x040016F5 RID: 5877
		CureAll,
		// Token: 0x040016F6 RID: 5878
		TurnNextTurn,
		// Token: 0x040016F7 RID: 5879
		HPorDamage,
		// Token: 0x040016F8 RID: 5880
		CurePoisonAll
	}

	// Token: 0x02000202 RID: 514
	public enum LibraryPages
	{
		// Token: 0x040016FA RID: 5882
		Discoveries,
		// Token: 0x040016FB RID: 5883
		Bestiary,
		// Token: 0x040016FC RID: 5884
		Recipes,
		// Token: 0x040016FD RID: 5885
		Logbook,
		// Token: 0x040016FE RID: 5886
		Map
	}

	// Token: 0x02000203 RID: 515
	public enum BadgeEffects
	{
		// Token: 0x04001700 RID: 5888
		None,
		// Token: 0x04001701 RID: 5889
		HPUP,
		// Token: 0x04001702 RID: 5890
		TPUP,
		// Token: 0x04001703 RID: 5891
		HPRecover,
		// Token: 0x04001704 RID: 5892
		TPRecover,
		// Token: 0x04001705 RID: 5893
		AttackUp,
		// Token: 0x04001706 RID: 5894
		DefenseUp,
		// Token: 0x04001707 RID: 5895
		LockSkills,
		// Token: 0x04001708 RID: 5896
		Detect,
		// Token: 0x04001709 RID: 5897
		SpeedUp,
		// Token: 0x0400170A RID: 5898
		PoisonRes,
		// Token: 0x0400170B RID: 5899
		SleepRes,
		// Token: 0x0400170C RID: 5900
		NumbRes,
		// Token: 0x0400170D RID: 5901
		FreezeRes,
		// Token: 0x0400170E RID: 5902
		PoisonAttack,
		// Token: 0x0400170F RID: 5903
		PoisonDefense,
		// Token: 0x04001710 RID: 5904
		SleepDefense,
		// Token: 0x04001711 RID: 5905
		NumbDefense,
		// Token: 0x04001712 RID: 5906
		FreezeDefense,
		// Token: 0x04001713 RID: 5907
		AttackMultiply,
		// Token: 0x04001714 RID: 5908
		DefenseMuliply,
		// Token: 0x04001715 RID: 5909
		LockItems,
		// Token: 0x04001716 RID: 5910
		LockRelay,
		// Token: 0x04001717 RID: 5911
		LockRelayPass
	}

	// Token: 0x02000204 RID: 516
	public enum BattleCondition
	{
		// Token: 0x04001719 RID: 5913
		Freeze,
		// Token: 0x0400171A RID: 5914
		Poison,
		// Token: 0x0400171B RID: 5915
		Numb,
		// Token: 0x0400171C RID: 5916
		Sleep,
		// Token: 0x0400171D RID: 5917
		AttackUp,
		// Token: 0x0400171E RID: 5918
		DefenseUp,
		// Token: 0x0400171F RID: 5919
		AttackDown,
		// Token: 0x04001720 RID: 5920
		DefenseDown,
		// Token: 0x04001721 RID: 5921
		Topple,
		// Token: 0x04001722 RID: 5922
		Flipped,
		// Token: 0x04001723 RID: 5923
		Shield,
		// Token: 0x04001724 RID: 5924
		Taunted,
		// Token: 0x04001725 RID: 5925
		Sturdy,
		// Token: 0x04001726 RID: 5926
		GradualHP,
		// Token: 0x04001727 RID: 5927
		GradualTP,
		// Token: 0x04001728 RID: 5928
		Eaten,
		// Token: 0x04001729 RID: 5929
		EventStop,
		// Token: 0x0400172A RID: 5930
		Fire,
		// Token: 0x0400172B RID: 5931
		Inked,
		// Token: 0x0400172C RID: 5932
		Sticky,
		// Token: 0x0400172D RID: 5933
		Reflection
	}

	// Token: 0x02000205 RID: 517
	public enum Emoticons
	{
		// Token: 0x0400172F RID: 5935
		None,
		// Token: 0x04001730 RID: 5936
		Talk,
		// Token: 0x04001731 RID: 5937
		QuestionMark,
		// Token: 0x04001732 RID: 5938
		Exclamation,
		// Token: 0x04001733 RID: 5939
		DotsLong,
		// Token: 0x04001734 RID: 5940
		Detector,
		// Token: 0x04001735 RID: 5941
		Pushable
	}

	// Token: 0x02000206 RID: 518
	public enum Library
	{
		// Token: 0x04001737 RID: 5943
		Discovery,
		// Token: 0x04001738 RID: 5944
		Bestiary,
		// Token: 0x04001739 RID: 5945
		Recipes,
		// Token: 0x0400173A RID: 5946
		Logbook,
		// Token: 0x0400173B RID: 5947
		Map
	}

	// Token: 0x02000207 RID: 519
	public enum BattleMaps
	{
		// Token: 0x0400173D RID: 5949
		Grasslands1,
		// Token: 0x0400173E RID: 5950
		Desert1,
		// Token: 0x0400173F RID: 5951
		Snakemouth1,
		// Token: 0x04001740 RID: 5952
		AssociationHQ,
		// Token: 0x04001741 RID: 5953
		Snakemouth2,
		// Token: 0x04001742 RID: 5954
		Snakemouth3,
		// Token: 0x04001743 RID: 5955
		Snakemouth4,
		// Token: 0x04001744 RID: 5956
		Theater,
		// Token: 0x04001745 RID: 5957
		GoldenBattle1,
		// Token: 0x04001746 RID: 5958
		OutskirtsLow,
		// Token: 0x04001747 RID: 5959
		Cave0,
		// Token: 0x04001748 RID: 5960
		GoldenSettlementArena,
		// Token: 0x04001749 RID: 5961
		GoldenBattle2,
		// Token: 0x0400174A RID: 5962
		GoldenHillsBoss,
		// Token: 0x0400174B RID: 5963
		GoldenBattle4,
		// Token: 0x0400174C RID: 5964
		GoldenBattle5,
		// Token: 0x0400174D RID: 5965
		HBsLab,
		// Token: 0x0400174E RID: 5966
		Bakery,
		// Token: 0x0400174F RID: 5967
		FactoryP,
		// Token: 0x04001750 RID: 5968
		FactoryS,
		// Token: 0x04001751 RID: 5969
		FactoryC,
		// Token: 0x04001752 RID: 5970
		FactoryS2,
		// Token: 0x04001753 RID: 5971
		SandCastle,
		// Token: 0x04001754 RID: 5972
		SandCastleIce,
		// Token: 0x04001755 RID: 5973
		HideoutBattle,
		// Token: 0x04001756 RID: 5974
		HideoutAstotheles,
		// Token: 0x04001757 RID: 5975
		SandCastleDark,
		// Token: 0x04001758 RID: 5976
		SandCastleDarkIce,
		// Token: 0x04001759 RID: 5977
		SandCastleRoof,
		// Token: 0x0400175A RID: 5978
		SandCastleBoss,
		// Token: 0x0400175B RID: 5979
		AntPalace,
		// Token: 0x0400175C RID: 5980
		PlazaAttack,
		// Token: 0x0400175D RID: 5981
		BridgeAttack,
		// Token: 0x0400175E RID: 5982
		CastleAttack,
		// Token: 0x0400175F RID: 5983
		Grasslands2,
		// Token: 0x04001760 RID: 5984
		FarGrasslands,
		// Token: 0x04001761 RID: 5985
		Swamplands,
		// Token: 0x04001762 RID: 5986
		BarrenLands,
		// Token: 0x04001763 RID: 5987
		ChomperCaves,
		// Token: 0x04001764 RID: 5988
		ChomperCavesBoss,
		// Token: 0x04001765 RID: 5989
		WaspPrison,
		// Token: 0x04001766 RID: 5990
		WaspThrone,
		// Token: 0x04001767 RID: 5991
		TermiteColiseum,
		// Token: 0x04001768 RID: 5992
		AbandonedTent,
		// Token: 0x04001769 RID: 5993
		Broodmother,
		// Token: 0x0400176A RID: 5994
		KaliShop,
		// Token: 0x0400176B RID: 5995
		MetalLake,
		// Token: 0x0400176C RID: 5996
		StreamMountain,
		// Token: 0x0400176D RID: 5997
		StreamMountainBoss,
		// Token: 0x0400176E RID: 5998
		BugariaCommercial,
		// Token: 0x0400176F RID: 5999
		CaveOfTrials,
		// Token: 0x04001770 RID: 6000
		UpperSnek,
		// Token: 0x04001771 RID: 6001
		UpperSnekBoss,
		// Token: 0x04001772 RID: 6002
		RubberPrisonInside,
		// Token: 0x04001773 RID: 6003
		RubberPrisonOutside,
		// Token: 0x04001774 RID: 6004
		WaspKingdomPrison,
		// Token: 0x04001775 RID: 6005
		CarminaRoom,
		// Token: 0x04001776 RID: 6006
		RubberPrisonBoss,
		// Token: 0x04001777 RID: 6007
		MysteryIslandInside,
		// Token: 0x04001778 RID: 6008
		GiantLair1,
		// Token: 0x04001779 RID: 6009
		GiantLair2,
		// Token: 0x0400177A RID: 6010
		GiantLairFridge,
		// Token: 0x0400177B RID: 6011
		FinalBoss1,
		// Token: 0x0400177C RID: 6012
		FinalBoss2,
		// Token: 0x0400177D RID: 6013
		PitcherPlant,
		// Token: 0x0400177E RID: 6014
		AntBridge,
		// Token: 0x0400177F RID: 6015
		CardTourney,
		// Token: 0x04001780 RID: 6016
		UndergroundBar,
		// Token: 0x04001781 RID: 6017
		GiantLair3,
		// Token: 0x04001782 RID: 6018
		GiantLair4,
		// Token: 0x04001783 RID: 6019
		DefiantRootBattle,
		// Token: 0x04001784 RID: 6020
		HBsLab2
	}

	// Token: 0x02000208 RID: 520
	public enum Healing
	{
		// Token: 0x04001786 RID: 6022
		Full,
		// Token: 0x04001787 RID: 6023
		FullHPOnly,
		// Token: 0x04001788 RID: 6024
		TPOnly
	}

	// Token: 0x02000209 RID: 521
	public enum Musics
	{
		// Token: 0x0400178A RID: 6026
		Field0,
		// Token: 0x0400178B RID: 6027
		Battle0,
		// Token: 0x0400178C RID: 6028
		Cave0,
		// Token: 0x0400178D RID: 6029
		Battle1,
		// Token: 0x0400178E RID: 6030
		Calm,
		// Token: 0x0400178F RID: 6031
		Inside0,
		// Token: 0x04001790 RID: 6032
		LevelUp,
		// Token: 0x04001791 RID: 6033
		Theater,
		// Token: 0x04001792 RID: 6034
		Tension,
		// Token: 0x04001793 RID: 6035
		Chef0,
		// Token: 0x04001794 RID: 6036
		Moth,
		// Token: 0x04001795 RID: 6037
		Beetle,
		// Token: 0x04001796 RID: 6038
		Title,
		// Token: 0x04001797 RID: 6039
		Field1,
		// Token: 0x04001798 RID: 6040
		Inside1,
		// Token: 0x04001799 RID: 6041
		Inside2,
		// Token: 0x0400179A RID: 6042
		Dungeon0,
		// Token: 0x0400179B RID: 6043
		Mothiva,
		// Token: 0x0400179C RID: 6044
		Festival,
		// Token: 0x0400179D RID: 6045
		Battle2,
		// Token: 0x0400179E RID: 6046
		Venus,
		// Token: 0x0400179F RID: 6047
		Field2,
		// Token: 0x040017A0 RID: 6048
		Dungeon1,
		// Token: 0x040017A1 RID: 6049
		Miniboss,
		// Token: 0x040017A2 RID: 6050
		Field3,
		// Token: 0x040017A3 RID: 6051
		Battle3,
		// Token: 0x040017A4 RID: 6052
		Wind,
		// Token: 0x040017A5 RID: 6053
		Water,
		// Token: 0x040017A6 RID: 6054
		Dungeon2,
		// Token: 0x040017A7 RID: 6055
		Chef1,
		// Token: 0x040017A8 RID: 6056
		Chef2,
		// Token: 0x040017A9 RID: 6057
		Bee,
		// Token: 0x040017AA RID: 6058
		Battle4,
		// Token: 0x040017AB RID: 6059
		Dungeon2b,
		// Token: 0x040017AC RID: 6060
		Sad,
		// Token: 0x040017AD RID: 6061
		MothivaCalm,
		// Token: 0x040017AE RID: 6062
		BeeQ,
		// Token: 0x040017AF RID: 6063
		Tension2,
		// Token: 0x040017B0 RID: 6064
		MachineHum,
		// Token: 0x040017B1 RID: 6065
		Battle5,
		// Token: 0x040017B2 RID: 6066
		Dungeon3,
		// Token: 0x040017B3 RID: 6067
		Dungeon4,
		// Token: 0x040017B4 RID: 6068
		Field4,
		// Token: 0x040017B5 RID: 6069
		Battle6,
		// Token: 0x040017B6 RID: 6070
		Cave1,
		// Token: 0x040017B7 RID: 6071
		Sad2,
		// Token: 0x040017B8 RID: 6072
		Battle7,
		// Token: 0x040017B9 RID: 6073
		Field5,
		// Token: 0x040017BA RID: 6074
		Submarine,
		// Token: 0x040017BB RID: 6075
		Termite,
		// Token: 0x040017BC RID: 6076
		Breathing,
		// Token: 0x040017BD RID: 6077
		TermiteLoop,
		// Token: 0x040017BE RID: 6078
		WaspHive,
		// Token: 0x040017BF RID: 6079
		Dungeon5,
		// Token: 0x040017C0 RID: 6080
		Invasion,
		// Token: 0x040017C1 RID: 6081
		MetalIsland,
		// Token: 0x040017C2 RID: 6082
		Bounty,
		// Token: 0x040017C3 RID: 6083
		Centipede,
		// Token: 0x040017C4 RID: 6084
		Lab,
		// Token: 0x040017C5 RID: 6085
		FlyingBee,
		// Token: 0x040017C6 RID: 6086
		Battle8,
		// Token: 0x040017C7 RID: 6087
		Battle9,
		// Token: 0x040017C8 RID: 6088
		Alert,
		// Token: 0x040017C9 RID: 6089
		Giant1,
		// Token: 0x040017CA RID: 6090
		Giant2,
		// Token: 0x040017CB RID: 6091
		Giant3,
		// Token: 0x040017CC RID: 6092
		Final1,
		// Token: 0x040017CD RID: 6093
		Final2,
		// Token: 0x040017CE RID: 6094
		MiteKnight,
		// Token: 0x040017CF RID: 6095
		Field6,
		// Token: 0x040017D0 RID: 6096
		Pier,
		// Token: 0x040017D1 RID: 6097
		Credits,
		// Token: 0x040017D2 RID: 6098
		Tension3,
		// Token: 0x040017D3 RID: 6099
		Field7,
		// Token: 0x040017D4 RID: 6100
		TeamSnek
	}

	// Token: 0x0200020A RID: 522
	public enum StatBonus
	{
		// Token: 0x040017D6 RID: 6102
		HP,
		// Token: 0x040017D7 RID: 6103
		Attack,
		// Token: 0x040017D8 RID: 6104
		Defense,
		// Token: 0x040017D9 RID: 6105
		TP,
		// Token: 0x040017DA RID: 6106
		MP
	}

	// Token: 0x0200020B RID: 523
	public enum BadgeTypes
	{
		// Token: 0x040017DC RID: 6108
		HPPlus,
		// Token: 0x040017DD RID: 6109
		TPPlus,
		// Token: 0x040017DE RID: 6110
		Detector,
		// Token: 0x040017DF RID: 6111
		AttackUp,
		// Token: 0x040017E0 RID: 6112
		DefenseUp,
		// Token: 0x040017E1 RID: 6113
		SpeedUp,
		// Token: 0x040017E2 RID: 6114
		PoisonAttacker,
		// Token: 0x040017E3 RID: 6115
		PoisonResistance,
		// Token: 0x040017E4 RID: 6116
		Berserker,
		// Token: 0x040017E5 RID: 6117
		PoisonDefender,
		// Token: 0x040017E6 RID: 6118
		Empower,
		// Token: 0x040017E7 RID: 6119
		DoublePain,
		// Token: 0x040017E8 RID: 6120
		SleepyResistance,
		// Token: 0x040017E9 RID: 6121
		MightyPeeble,
		// Token: 0x040017EA RID: 6122
		SpikeBod,
		// Token: 0x040017EB RID: 6123
		Depower1,
		// Token: 0x040017EC RID: 6124
		Depower2,
		// Token: 0x040017ED RID: 6125
		HPScope,
		// Token: 0x040017EE RID: 6126
		BumpAttack,
		// Token: 0x040017EF RID: 6127
		SuperBlock,
		// Token: 0x040017F0 RID: 6128
		FavoriteOne,
		// Token: 0x040017F1 RID: 6129
		NumbResis,
		// Token: 0x040017F2 RID: 6130
		PoisonNeedle,
		// Token: 0x040017F3 RID: 6131
		StrongStart,
		// Token: 0x040017F4 RID: 6132
		WeakStomach,
		// Token: 0x040017F5 RID: 6133
		TPSaver,
		// Token: 0x040017F6 RID: 6134
		ReversePoison,
		// Token: 0x040017F7 RID: 6135
		EternalPoison,
		// Token: 0x040017F8 RID: 6136
		Beemerang2,
		// Token: 0x040017F9 RID: 6137
		MightierPebble,
		// Token: 0x040017FA RID: 6138
		DoublePainReal,
		// Token: 0x040017FB RID: 6139
		Empower2,
		// Token: 0x040017FC RID: 6140
		LifeSteal,
		// Token: 0x040017FD RID: 6141
		FreezeResistance,
		// Token: 0x040017FE RID: 6142
		ShockTrooper,
		// Token: 0x040017FF RID: 6143
		FrontSupport,
		// Token: 0x04001800 RID: 6144
		BackSupport,
		// Token: 0x04001801 RID: 6145
		Emfeeble,
		// Token: 0x04001802 RID: 6146
		Emfeeble2,
		// Token: 0x04001803 RID: 6147
		Fortify,
		// Token: 0x04001804 RID: 6148
		Fortify2,
		// Token: 0x04001805 RID: 6149
		NumbNeedle,
		// Token: 0x04001806 RID: 6150
		EXPBoost,
		// Token: 0x04001807 RID: 6151
		StatusBoost,
		// Token: 0x04001808 RID: 6152
		PoisonTouch,
		// Token: 0x04001809 RID: 6153
		RelayTransfer,
		// Token: 0x0400180A RID: 6154
		FrostBite,
		// Token: 0x0400180B RID: 6155
		HeavySleeper,
		// Token: 0x0400180C RID: 6156
		SecurePouch,
		// Token: 0x0400180D RID: 6157
		PowerExchange,
		// Token: 0x0400180E RID: 6158
		DefenseExchange,
		// Token: 0x0400180F RID: 6159
		Tardigrade,
		// Token: 0x04001810 RID: 6160
		ChargeUp,
		// Token: 0x04001811 RID: 6161
		ChargeUp2,
		// Token: 0x04001812 RID: 6162
		LeafCloak,
		// Token: 0x04001813 RID: 6163
		RandomStart,
		// Token: 0x04001814 RID: 6164
		Meditation,
		// Token: 0x04001815 RID: 6165
		ElecNeedles,
		// Token: 0x04001816 RID: 6166
		NeedlePincer,
		// Token: 0x04001817 RID: 6167
		FreezeTime,
		// Token: 0x04001818 RID: 6168
		HeavyThrow,
		// Token: 0x04001819 RID: 6169
		Reflection,
		// Token: 0x0400181A RID: 6170
		Prayer,
		// Token: 0x0400181B RID: 6171
		AntlionJaws,
		// Token: 0x0400181C RID: 6172
		HappyHeart,
		// Token: 0x0400181D RID: 6173
		HappyTP,
		// Token: 0x0400181E RID: 6174
		ResistAll,
		// Token: 0x0400181F RID: 6175
		TauntPlus,
		// Token: 0x04001820 RID: 6176
		MiracleMatter,
		// Token: 0x04001821 RID: 6177
		VictoryBuzz,
		// Token: 0x04001822 RID: 6178
		HealingBuzz,
		// Token: 0x04001823 RID: 6179
		CrazyPrepared,
		// Token: 0x04001824 RID: 6180
		HPFunnel,
		// Token: 0x04001825 RID: 6181
		HardCharge,
		// Token: 0x04001826 RID: 6182
		HealPlus,
		// Token: 0x04001827 RID: 6183
		StatusMirror,
		// Token: 0x04001828 RID: 6184
		LuckierDay,
		// Token: 0x04001829 RID: 6185
		Plating,
		// Token: 0x0400182A RID: 6186
		Seedling,
		// Token: 0x0400182B RID: 6187
		BerryFinder,
		// Token: 0x0400182C RID: 6188
		RoyalDecree,
		// Token: 0x0400182D RID: 6189
		BadDream,
		// Token: 0x0400182E RID: 6190
		Hook,
		// Token: 0x0400182F RID: 6191
		HoloCloak,
		// Token: 0x04001830 RID: 6192
		Pip,
		// Token: 0x04001831 RID: 6193
		Helper,
		// Token: 0x04001832 RID: 6194
		Ambush,
		// Token: 0x04001833 RID: 6195
		LastWind,
		// Token: 0x04001834 RID: 6196
		ItemRecycle,
		// Token: 0x04001835 RID: 6197
		BombPlus,
		// Token: 0x04001836 RID: 6198
		HelperBoost
	}

	// Token: 0x0200020C RID: 524
	public enum Skills
	{
		// Token: 0x04001838 RID: 6200
		RESERVED,
		// Token: 0x04001839 RID: 6201
		RESERVED2,
		// Token: 0x0400183A RID: 6202
		BeeRangMultiHit,
		// Token: 0x0400183B RID: 6203
		BeetleTaunt,
		// Token: 0x0400183C RID: 6204
		Icefall,
		// Token: 0x0400183D RID: 6205
		BeeFly,
		// Token: 0x0400183E RID: 6206
		BeetleDig,
		// Token: 0x0400183F RID: 6207
		BubbleShield,
		// Token: 0x04001840 RID: 6208
		Empower,
		// Token: 0x04001841 RID: 6209
		PeebleToss,
		// Token: 0x04001842 RID: 6210
		HornDash,
		// Token: 0x04001843 RID: 6211
		SecretStash,
		// Token: 0x04001844 RID: 6212
		DefenseBreak1,
		// Token: 0x04001845 RID: 6213
		DefenseBreakAll,
		// Token: 0x04001846 RID: 6214
		AttackUp,
		// Token: 0x04001847 RID: 6215
		DefenseUp,
		// Token: 0x04001848 RID: 6216
		NeedleToss,
		// Token: 0x04001849 RID: 6217
		BubbleShieldLite,
		// Token: 0x0400184A RID: 6218
		HurricaneBeemerang,
		// Token: 0x0400184B RID: 6219
		PebbleTossPlus,
		// Token: 0x0400184C RID: 6220
		RevivalMassage,
		// Token: 0x0400184D RID: 6221
		FrigidCoffin,
		// Token: 0x0400184E RID: 6222
		ChargeUpPlus,
		// Token: 0x0400184F RID: 6223
		EmpowerPlus,
		// Token: 0x04001850 RID: 6224
		NeedlePincer,
		// Token: 0x04001851 RID: 6225
		IceRain,
		// Token: 0x04001852 RID: 6226
		IceBeemerang,
		// Token: 0x04001853 RID: 6227
		IceDrill,
		// Token: 0x04001854 RID: 6228
		AttackDown,
		// Token: 0x04001855 RID: 6229
		AttackDownPlus,
		// Token: 0x04001856 RID: 6230
		DefenseUpPlus,
		// Token: 0x04001857 RID: 6231
		IceSphere,
		// Token: 0x04001858 RID: 6232
		HeavyStrike,
		// Token: 0x04001859 RID: 6233
		Sturdy,
		// Token: 0x0400185A RID: 6234
		FieldBeemerang,
		// Token: 0x0400185B RID: 6235
		FieldHalt,
		// Token: 0x0400185C RID: 6236
		FieldFly,
		// Token: 0x0400185D RID: 6237
		FieldHorn,
		// Token: 0x0400185E RID: 6238
		FieldDash,
		// Token: 0x0400185F RID: 6239
		FieldDig,
		// Token: 0x04001860 RID: 6240
		FieldFreeze,
		// Token: 0x04001861 RID: 6241
		FieldIcecle,
		// Token: 0x04001862 RID: 6242
		FieldShield,
		// Token: 0x04001863 RID: 6243
		PepTalk,
		// Token: 0x04001864 RID: 6244
		HeavyThrow,
		// Token: 0x04001865 RID: 6245
		SharingStash,
		// Token: 0x04001866 RID: 6246
		RoyalDecree,
		// Token: 0x04001867 RID: 6247
		Cleanse,
		// Token: 0x04001868 RID: 6248
		HardCharge,
		// Token: 0x04001869 RID: 6249
		FirstDash
	}

	// Token: 0x0200020D RID: 525
	public enum BoardQuests
	{
		// Token: 0x0400186B RID: 6251
		None,
		// Token: 0x0400186C RID: 6252
		InnQuest,
		// Token: 0x0400186D RID: 6253
		ChuckQuest,
		// Token: 0x0400186E RID: 6254
		TheaterQuest,
		// Token: 0x0400186F RID: 6255
		ToyQuest,
		// Token: 0x04001870 RID: 6256
		LadybugQuest,
		// Token: 0x04001871 RID: 6257
		UndergroundBar,
		// Token: 0x04001872 RID: 6258
		CableCar,
		// Token: 0x04001873 RID: 6259
		SeedlingKing,
		// Token: 0x04001874 RID: 6260
		FalseMonarch,
		// Token: 0x04001875 RID: 6261
		MotherChomper,
		// Token: 0x04001876 RID: 6262
		Prologue,
		// Token: 0x04001877 RID: 6263
		Chapter1,
		// Token: 0x04001878 RID: 6264
		Chapter2,
		// Token: 0x04001879 RID: 6265
		Chapter3,
		// Token: 0x0400187A RID: 6266
		Chapter4,
		// Token: 0x0400187B RID: 6267
		Chapter5,
		// Token: 0x0400187C RID: 6268
		Chapter6,
		// Token: 0x0400187D RID: 6269
		Crisbee,
		// Token: 0x0400187E RID: 6270
		Kut,
		// Token: 0x0400187F RID: 6271
		Fry,
		// Token: 0x04001880 RID: 6272
		Sandwyrm,
		// Token: 0x04001881 RID: 6273
		Butomo,
		// Token: 0x04001882 RID: 6274
		PeacockSpider,
		// Token: 0x04001883 RID: 6275
		Tanjerin,
		// Token: 0x04001884 RID: 6276
		ZaspDoll,
		// Token: 0x04001885 RID: 6277
		Leif,
		// Token: 0x04001886 RID: 6278
		LibraryantRed,
		// Token: 0x04001887 RID: 6279
		Madeleine1,
		// Token: 0x04001888 RID: 6280
		Venus,
		// Token: 0x04001889 RID: 6281
		Bee,
		// Token: 0x0400188A RID: 6282
		PowerPlant,
		// Token: 0x0400188B RID: 6283
		CardGame,
		// Token: 0x0400188C RID: 6284
		CicadaBook,
		// Token: 0x0400188D RID: 6285
		Vivi,
		// Token: 0x0400188E RID: 6286
		MenderQuest,
		// Token: 0x0400188F RID: 6287
		Kali,
		// Token: 0x04001890 RID: 6288
		Isau,
		// Token: 0x04001891 RID: 6289
		Bomby,
		// Token: 0x04001892 RID: 6290
		Madeleine2,
		// Token: 0x04001893 RID: 6291
		GenEri,
		// Token: 0x04001894 RID: 6292
		Mun,
		// Token: 0x04001895 RID: 6293
		BanditHunt,
		// Token: 0x04001896 RID: 6294
		ArtBee,
		// Token: 0x04001897 RID: 6295
		TermiteLunch,
		// Token: 0x04001898 RID: 6296
		Alex,
		// Token: 0x04001899 RID: 6297
		Mayor,
		// Token: 0x0400189A RID: 6298
		SeedlingHunt,
		// Token: 0x0400189B RID: 6299
		Layna,
		// Token: 0x0400189C RID: 6300
		Eetl,
		// Token: 0x0400189D RID: 6301
		Farmer,
		// Token: 0x0400189E RID: 6302
		RizSis,
		// Token: 0x0400189F RID: 6303
		Wizard,
		// Token: 0x040018A0 RID: 6304
		Eremi,
		// Token: 0x040018A1 RID: 6305
		MoleCricket,
		// Token: 0x040018A2 RID: 6306
		Maki,
		// Token: 0x040018A3 RID: 6307
		BadBook,
		// Token: 0x040018A4 RID: 6308
		WaspTwins,
		// Token: 0x040018A5 RID: 6309
		BlacksmithGuy,
		// Token: 0x040018A6 RID: 6310
		WorkerTermite,
		// Token: 0x040018A7 RID: 6311
		Beetle,
		// Token: 0x040018A8 RID: 6312
		Rebecca,
		// Token: 0x040018A9 RID: 6313
		Roach,
		// Token: 0x040018AA RID: 6314
		StratosDelilah
	}

	// Token: 0x0200020E RID: 526
	public enum Enemies
	{
		// Token: 0x040018AC RID: 6316
		CordycepsAnt,
		// Token: 0x040018AD RID: 6317
		Mushroom,
		// Token: 0x040018AE RID: 6318
		Spuder,
		// Token: 0x040018AF RID: 6319
		Zasp,
		// Token: 0x040018B0 RID: 6320
		Cactus,
		// Token: 0x040018B1 RID: 6321
		Pseudoscorpion,
		// Token: 0x040018B2 RID: 6322
		Thief,
		// Token: 0x040018B3 RID: 6323
		Bandit,
		// Token: 0x040018B4 RID: 6324
		ArmoredPoly,
		// Token: 0x040018B5 RID: 6325
		Seedling,
		// Token: 0x040018B6 RID: 6326
		FlyingSeedling,
		// Token: 0x040018B7 RID: 6327
		MakiTutorial,
		// Token: 0x040018B8 RID: 6328
		MothWeb,
		// Token: 0x040018B9 RID: 6329
		SpuderReal,
		// Token: 0x040018BA RID: 6330
		Sneil,
		// Token: 0x040018BB RID: 6331
		Mothiva1,
		// Token: 0x040018BC RID: 6332
		Acornling,
		// Token: 0x040018BD RID: 6333
		Weevil,
		// Token: 0x040018BE RID: 6334
		MrTester,
		// Token: 0x040018BF RID: 6335
		AngryPlant,
		// Token: 0x040018C0 RID: 6336
		FlyTrap,
		// Token: 0x040018C1 RID: 6337
		Acolyte,
		// Token: 0x040018C2 RID: 6338
		AcolyteVine,
		// Token: 0x040018C3 RID: 6339
		Beetle,
		// Token: 0x040018C4 RID: 6340
		VenusBoss,
		// Token: 0x040018C5 RID: 6341
		WaspTrooper,
		// Token: 0x040018C6 RID: 6342
		WaspBomber,
		// Token: 0x040018C7 RID: 6343
		WaspDriller,
		// Token: 0x040018C8 RID: 6344
		WaspHealer,
		// Token: 0x040018C9 RID: 6345
		Midge,
		// Token: 0x040018CA RID: 6346
		Underling,
		// Token: 0x040018CB RID: 6347
		Scarlet,
		// Token: 0x040018CC RID: 6348
		GoldenSeedling,
		// Token: 0x040018CD RID: 6349
		Sandworm,
		// Token: 0x040018CE RID: 6350
		Carmina,
		// Token: 0x040018CF RID: 6351
		SeedlingKing,
		// Token: 0x040018D0 RID: 6352
		MidgeBroodmother,
		// Token: 0x040018D1 RID: 6353
		Plumpling,
		// Token: 0x040018D2 RID: 6354
		Flowering,
		// Token: 0x040018D3 RID: 6355
		Burglar,
		// Token: 0x040018D4 RID: 6356
		BanditLeader,
		// Token: 0x040018D5 RID: 6357
		MotherChomper,
		// Token: 0x040018D6 RID: 6358
		Ahoneynation,
		// Token: 0x040018D7 RID: 6359
		BeeBot,
		// Token: 0x040018D8 RID: 6360
		BeeTurret,
		// Token: 0x040018D9 RID: 6361
		ShockWorm,
		// Token: 0x040018DA RID: 6362
		BeeBoss,
		// Token: 0x040018DB RID: 6363
		MenderBot,
		// Token: 0x040018DC RID: 6364
		Abomihoney,
		// Token: 0x040018DD RID: 6365
		Scorpion,
		// Token: 0x040018DE RID: 6366
		SandWyrm,
		// Token: 0x040018DF RID: 6367
		Kali,
		// Token: 0x040018E0 RID: 6368
		Zombee,
		// Token: 0x040018E1 RID: 6369
		Zombeetle,
		// Token: 0x040018E2 RID: 6370
		ZombieRoach,
		// Token: 0x040018E3 RID: 6371
		PeacockSpider,
		// Token: 0x040018E4 RID: 6372
		Bloatshroom,
		// Token: 0x040018E5 RID: 6373
		Krawler,
		// Token: 0x040018E6 RID: 6374
		Cape,
		// Token: 0x040018E7 RID: 6375
		SandWall,
		// Token: 0x040018E8 RID: 6376
		IceWall,
		// Token: 0x040018E9 RID: 6377
		CursedSkull,
		// Token: 0x040018EA RID: 6378
		WaspKingIntermission,
		// Token: 0x040018EB RID: 6379
		JumpingSpider,
		// Token: 0x040018EC RID: 6380
		MimicSpider,
		// Token: 0x040018ED RID: 6381
		LeafbugNinja,
		// Token: 0x040018EE RID: 6382
		LeafbugArcher,
		// Token: 0x040018EF RID: 6383
		LeafbugClubber,
		// Token: 0x040018F0 RID: 6384
		SkullCaterpillar,
		// Token: 0x040018F1 RID: 6385
		Centipede,
		// Token: 0x040018F2 RID: 6386
		ChomperBrute,
		// Token: 0x040018F3 RID: 6387
		Mantidfly,
		// Token: 0x040018F4 RID: 6388
		WaspGeneral,
		// Token: 0x040018F5 RID: 6389
		WildChomper,
		// Token: 0x040018F6 RID: 6390
		TermiteSoldier,
		// Token: 0x040018F7 RID: 6391
		TermiteNasute,
		// Token: 0x040018F8 RID: 6392
		PrimalWeevil,
		// Token: 0x040018F9 RID: 6393
		FalseMonarch,
		// Token: 0x040018FA RID: 6394
		Mothfly,
		// Token: 0x040018FB RID: 6395
		MothflyCluster,
		// Token: 0x040018FC RID: 6396
		Ironclad,
		// Token: 0x040018FD RID: 6397
		ToeBiter,
		// Token: 0x040018FE RID: 6398
		Ruffian,
		// Token: 0x040018FF RID: 6399
		Strider,
		// Token: 0x04001900 RID: 6400
		DivingSpider,
		// Token: 0x04001901 RID: 6401
		Cenn,
		// Token: 0x04001902 RID: 6402
		Pisci,
		// Token: 0x04001903 RID: 6403
		DeadLanderA,
		// Token: 0x04001904 RID: 6404
		DeadLanderB,
		// Token: 0x04001905 RID: 6405
		DeadLanderG,
		// Token: 0x04001906 RID: 6406
		WaspKing,
		// Token: 0x04001907 RID: 6407
		EverlastingKing,
		// Token: 0x04001908 RID: 6408
		Maki,
		// Token: 0x04001909 RID: 6409
		Kina,
		// Token: 0x0400190A RID: 6410
		Yin,
		// Token: 0x0400190B RID: 6411
		UltimaxTank,
		// Token: 0x0400190C RID: 6412
		Zommoth,
		// Token: 0x0400190D RID: 6413
		Fisherman,
		// Token: 0x0400190E RID: 6414
		Pitcher,
		// Token: 0x0400190F RID: 6415
		SandWyrmTail,
		// Token: 0x04001910 RID: 6416
		PisciWall,
		// Token: 0x04001911 RID: 6417
		KeyR,
		// Token: 0x04001912 RID: 6418
		KeyL,
		// Token: 0x04001913 RID: 6419
		Tablet,
		// Token: 0x04001914 RID: 6420
		PitcherFlytrap,
		// Token: 0x04001915 RID: 6421
		FireKrawler,
		// Token: 0x04001916 RID: 6422
		FireWarden,
		// Token: 0x04001917 RID: 6423
		FireCape,
		// Token: 0x04001918 RID: 6424
		IceKrawler,
		// Token: 0x04001919 RID: 6425
		IceWarden,
		// Token: 0x0400191A RID: 6426
		TANGYBUG,
		// Token: 0x0400191B RID: 6427
		Stratos,
		// Token: 0x0400191C RID: 6428
		Delilah,
		// Token: 0x0400191D RID: 6429
		HoloVi,
		// Token: 0x0400191E RID: 6430
		HoloKabbu,
		// Token: 0x0400191F RID: 6431
		HoloLeif
	}

	// Token: 0x0200020F RID: 527
	public enum Areas
	{
		// Token: 0x04001921 RID: 6433
		BugariaOutskirts,
		// Token: 0x04001922 RID: 6434
		BugariaCity,
		// Token: 0x04001923 RID: 6435
		Snakemouth,
		// Token: 0x04001924 RID: 6436
		Desert,
		// Token: 0x04001925 RID: 6437
		GoldenHills,
		// Token: 0x04001926 RID: 6438
		GoldenWay,
		// Token: 0x04001927 RID: 6439
		GoldenSettlement,
		// Token: 0x04001928 RID: 6440
		BarrenLands,
		// Token: 0x04001929 RID: 6441
		FarGrasslands,
		// Token: 0x0400192A RID: 6442
		WildGrasslands,
		// Token: 0x0400192B RID: 6443
		DefiantRoot,
		// Token: 0x0400192C RID: 6444
		SandCastle,
		// Token: 0x0400192D RID: 6445
		Beehive,
		// Token: 0x0400192E RID: 6446
		HoneyFactory,
		// Token: 0x0400192F RID: 6447
		RubberPrison,
		// Token: 0x04001930 RID: 6448
		GiantLair,
		// Token: 0x04001931 RID: 6449
		MetalLake,
		// Token: 0x04001932 RID: 6450
		MetalIsland,
		// Token: 0x04001933 RID: 6451
		TermiteCity,
		// Token: 0x04001934 RID: 6452
		WaspKingdom,
		// Token: 0x04001935 RID: 6453
		BanditHideout,
		// Token: 0x04001936 RID: 6454
		StreamMountain,
		// Token: 0x04001937 RID: 6455
		ChomperCaves,
		// Token: 0x04001938 RID: 6456
		FishingVillage,
		// Token: 0x04001939 RID: 6457
		UpperSnakemouth
	}

	// Token: 0x02000210 RID: 528
	public enum Maps
	{
		// Token: 0x0400193B RID: 6459
		TestRoom,
		// Token: 0x0400193C RID: 6460
		NearSnakemouth,
		// Token: 0x0400193D RID: 6461
		OutsideSnakemouth,
		// Token: 0x0400193E RID: 6462
		AntTunnels,
		// Token: 0x0400193F RID: 6463
		DesertEntrance,
		// Token: 0x04001940 RID: 6464
		DesertBadlands,
		// Token: 0x04001941 RID: 6465
		DesertBookArea,
		// Token: 0x04001942 RID: 6466
		DesertRockFormation,
		// Token: 0x04001943 RID: 6467
		DesertTrenchSouth,
		// Token: 0x04001944 RID: 6468
		BugariaMainPlaza,
		// Token: 0x04001945 RID: 6469
		BugariaCommercial,
		// Token: 0x04001946 RID: 6470
		SnakemouthBridgeRoom,
		// Token: 0x04001947 RID: 6471
		SnakemouthDoorRoom,
		// Token: 0x04001948 RID: 6472
		SnakemouthFallRoom,
		// Token: 0x04001949 RID: 6473
		SnakemouthLake,
		// Token: 0x0400194A RID: 6474
		SnakemouthEmpty,
		// Token: 0x0400194B RID: 6475
		BugariaOutskirtsOutsideCity,
		// Token: 0x0400194C RID: 6476
		BugariaOutskitsSnakemouthCorridor1,
		// Token: 0x0400194D RID: 6477
		BugariaOutskirtsSnakemouthCorridor2,
		// Token: 0x0400194E RID: 6478
		SnakemouthUndergrondDoor,
		// Token: 0x0400194F RID: 6479
		SnakemouthMushroomPit,
		// Token: 0x04001950 RID: 6480
		SnakemouthTreasureRoom,
		// Token: 0x04001951 RID: 6481
		SnakemouthUndergroundRightA,
		// Token: 0x04001952 RID: 6482
		SnakemouthUndergroundRightB,
		// Token: 0x04001953 RID: 6483
		SnakemouthUndergroundLeftA,
		// Token: 0x04001954 RID: 6484
		SnakemouthUndergroundLeftB,
		// Token: 0x04001955 RID: 6485
		BugariaTheater,
		// Token: 0x04001956 RID: 6486
		ChucksAbode,
		// Token: 0x04001957 RID: 6487
		BugariaResidential,
		// Token: 0x04001958 RID: 6488
		GoldenHillsCableCar,
		// Token: 0x04001959 RID: 6489
		UndergroundBar,
		// Token: 0x0400195A RID: 6490
		AntPalace1,
		// Token: 0x0400195B RID: 6491
		AntPalace2,
		// Token: 0x0400195C RID: 6492
		AntBridge,
		// Token: 0x0400195D RID: 6493
		AntPalaceLibrary,
		// Token: 0x0400195E RID: 6494
		GoldenPathTunnel,
		// Token: 0x0400195F RID: 6495
		BOGoldenPath,
		// Token: 0x04001960 RID: 6496
		AntPalaceWarRoom,
		// Token: 0x04001961 RID: 6497
		GoldenHillsPath2,
		// Token: 0x04001962 RID: 6498
		GoldenSettlementEntrance,
		// Token: 0x04001963 RID: 6499
		GoldenSettlement1,
		// Token: 0x04001964 RID: 6500
		GoldenSettlement1Night,
		// Token: 0x04001965 RID: 6501
		GoldenSettlement2,
		// Token: 0x04001966 RID: 6502
		GoldenSettlement2Night,
		// Token: 0x04001967 RID: 6503
		GoldenHillsPath3,
		// Token: 0x04001968 RID: 6504
		GoldenHillsDungeonEntrance,
		// Token: 0x04001969 RID: 6505
		GoldenHillsDungeonLeftMain,
		// Token: 0x0400196A RID: 6506
		GoldenHillsDungeonCrankLeft,
		// Token: 0x0400196B RID: 6507
		GoldenHillsDungeonRightCrank,
		// Token: 0x0400196C RID: 6508
		GoldenHillsLowerRightCrank,
		// Token: 0x0400196D RID: 6509
		GoldenHillsDungeonLeftCrankHalf,
		// Token: 0x0400196E RID: 6510
		GoldenHillsDungeonUpperMain,
		// Token: 0x0400196F RID: 6511
		GoldenHillsDungeonUpperSide,
		// Token: 0x04001970 RID: 6512
		GoldenHillsDungeonBoss,
		// Token: 0x04001971 RID: 6513
		BugariaPier,
		// Token: 0x04001972 RID: 6514
		BugariaOutskirtsEast1,
		// Token: 0x04001973 RID: 6515
		BugariaOutskirtsEast2,
		// Token: 0x04001974 RID: 6516
		BOLostSandsEntrance,
		// Token: 0x04001975 RID: 6517
		DefiantRoot1,
		// Token: 0x04001976 RID: 6518
		DefiantRootWell,
		// Token: 0x04001977 RID: 6519
		DefiantRoot2,
		// Token: 0x04001978 RID: 6520
		DefiantRoot3,
		// Token: 0x04001979 RID: 6521
		BeehiveOutside,
		// Token: 0x0400197A RID: 6522
		BeehiveThroneRoom,
		// Token: 0x0400197B RID: 6523
		BeehiveScannerRoom,
		// Token: 0x0400197C RID: 6524
		GoldenSettlement3,
		// Token: 0x0400197D RID: 6525
		GoldenSettlement3Night,
		// Token: 0x0400197E RID: 6526
		BeehiveMainArea,
		// Token: 0x0400197F RID: 6527
		HBsLab,
		// Token: 0x04001980 RID: 6528
		BeehiveBalcony,
		// Token: 0x04001981 RID: 6529
		HoneycombsLab,
		// Token: 0x04001982 RID: 6530
		JaunesGallery,
		// Token: 0x04001983 RID: 6531
		HoneyFactoryEntrance,
		// Token: 0x04001984 RID: 6532
		AntMinesBreakRoom,
		// Token: 0x04001985 RID: 6533
		HoneyFactoryWorkerRooms,
		// Token: 0x04001986 RID: 6534
		HoneyFactoryCore,
		// Token: 0x04001987 RID: 6535
		DesertDREastEntrance,
		// Token: 0x04001988 RID: 6536
		DesertFGBorder,
		// Token: 0x04001989 RID: 6537
		DesertDRSouthEntrance,
		// Token: 0x0400198A RID: 6538
		DesertBadgeAlcove,
		// Token: 0x0400198B RID: 6539
		DesertCaravanMap,
		// Token: 0x0400198C RID: 6540
		DesertSandPitArea,
		// Token: 0x0400198D RID: 6541
		DesertBeforeGH,
		// Token: 0x0400198E RID: 6542
		FactoryProcessingFirstRoom,
		// Token: 0x0400198F RID: 6543
		FactoryProcessing2,
		// Token: 0x04001990 RID: 6544
		FactoryProcessingPump,
		// Token: 0x04001991 RID: 6545
		FactoryProcessingPuzzle1,
		// Token: 0x04001992 RID: 6546
		FactoryProcessingPuzzle2,
		// Token: 0x04001993 RID: 6547
		FactoryProcessingPuzzle3,
		// Token: 0x04001994 RID: 6548
		FactoryProcessingMalbee,
		// Token: 0x04001995 RID: 6549
		FactoryStorageMaze,
		// Token: 0x04001996 RID: 6550
		FactoryStorageElevator,
		// Token: 0x04001997 RID: 6551
		FactoryStorageMiniboss,
		// Token: 0x04001998 RID: 6552
		FactoryStorageOverseer,
		// Token: 0x04001999 RID: 6553
		MetalIsland1,
		// Token: 0x0400199A RID: 6554
		DesertRoachVillage,
		// Token: 0x0400199B RID: 6555
		DesertOasis,
		// Token: 0x0400199C RID: 6556
		DesertOasisEntrance,
		// Token: 0x0400199D RID: 6557
		DesertWestDunes,
		// Token: 0x0400199E RID: 6558
		HideoutEntrance,
		// Token: 0x0400199F RID: 6559
		HideoutCell,
		// Token: 0x040019A0 RID: 6560
		HideoutCentralRoom,
		// Token: 0x040019A1 RID: 6561
		HideoutLeftA,
		// Token: 0x040019A2 RID: 6562
		HideoutStairsRoom,
		// Token: 0x040019A3 RID: 6563
		HideoutGarden,
		// Token: 0x040019A4 RID: 6564
		HideoutWestStorage,
		// Token: 0x040019A5 RID: 6565
		HideoutRightA,
		// Token: 0x040019A6 RID: 6566
		DesertSandCastle,
		// Token: 0x040019A7 RID: 6567
		DesertMountain,
		// Token: 0x040019A8 RID: 6568
		DesertTrenchMiddle,
		// Token: 0x040019A9 RID: 6569
		DesertJumpPuzzle,
		// Token: 0x040019AA RID: 6570
		DesertSouthern,
		// Token: 0x040019AB RID: 6571
		DesertScorpion,
		// Token: 0x040019AC RID: 6572
		DesertEastmost,
		// Token: 0x040019AD RID: 6573
		GoldenSMinigame,
		// Token: 0x040019AE RID: 6574
		Blank,
		// Token: 0x040019AF RID: 6575
		SandCastleEntrance,
		// Token: 0x040019B0 RID: 6576
		SandCastleSlidePuzzle,
		// Token: 0x040019B1 RID: 6577
		SandCastleStatueRoom,
		// Token: 0x040019B2 RID: 6578
		SandCastleBasement,
		// Token: 0x040019B3 RID: 6579
		SandCastleRoof,
		// Token: 0x040019B4 RID: 6580
		SandCastleMainRoom,
		// Token: 0x040019B5 RID: 6581
		SandCastleBossKeyRoom,
		// Token: 0x040019B6 RID: 6582
		BugariaPlazaAttack,
		// Token: 0x040019B7 RID: 6583
		BugariaBridgeAttack,
		// Token: 0x040019B8 RID: 6584
		BugariaCastleAttack,
		// Token: 0x040019B9 RID: 6585
		SandCastlePressurePuzzle,
		// Token: 0x040019BA RID: 6586
		SandCastleRockRoom,
		// Token: 0x040019BB RID: 6587
		SandCastleBossRoom,
		// Token: 0x040019BC RID: 6588
		SandCastleTreasureRoom,
		// Token: 0x040019BD RID: 6589
		BugariaAssociationAttack,
		// Token: 0x040019BE RID: 6590
		MetalIsland2,
		// Token: 0x040019BF RID: 6591
		StreamMountain1,
		// Token: 0x040019C0 RID: 6592
		StreamMountain2,
		// Token: 0x040019C1 RID: 6593
		StreamMountain3,
		// Token: 0x040019C2 RID: 6594
		FGCave,
		// Token: 0x040019C3 RID: 6595
		SeedlingHaven,
		// Token: 0x040019C4 RID: 6596
		FarGrasslands1,
		// Token: 0x040019C5 RID: 6597
		FarGrasslandsOutsideCave,
		// Token: 0x040019C6 RID: 6598
		FarGrasslandsWizard,
		// Token: 0x040019C7 RID: 6599
		FarGrasslands2,
		// Token: 0x040019C8 RID: 6600
		FarGrasslandsLake,
		// Token: 0x040019C9 RID: 6601
		FarGrasslandsOutsideVillage,
		// Token: 0x040019CA RID: 6602
		FarGrasslands3,
		// Token: 0x040019CB RID: 6603
		FishingVillage,
		// Token: 0x040019CC RID: 6604
		SwamplandsEntrance,
		// Token: 0x040019CD RID: 6605
		FGOutsideSwamplands,
		// Token: 0x040019CE RID: 6606
		WaspKingdomOutside,
		// Token: 0x040019CF RID: 6607
		Swamplands2,
		// Token: 0x040019D0 RID: 6608
		BarrenLandsEntrance,
		// Token: 0x040019D1 RID: 6609
		BarrenLandsCD,
		// Token: 0x040019D2 RID: 6610
		Swamplands3,
		// Token: 0x040019D3 RID: 6611
		SwamplandsBridge,
		// Token: 0x040019D4 RID: 6612
		FarGrasslands4,
		// Token: 0x040019D5 RID: 6613
		SwamplandsBoss,
		// Token: 0x040019D6 RID: 6614
		ChomperCave1,
		// Token: 0x040019D7 RID: 6615
		ChomperCaves2,
		// Token: 0x040019D8 RID: 6616
		ChomperCaves3,
		// Token: 0x040019D9 RID: 6617
		Swamplands4,
		// Token: 0x040019DA RID: 6618
		Swamplands5,
		// Token: 0x040019DB RID: 6619
		Swamplands6,
		// Token: 0x040019DC RID: 6620
		Swamplands7,
		// Token: 0x040019DD RID: 6621
		Swamplands8,
		// Token: 0x040019DE RID: 6622
		WaspKingdom1,
		// Token: 0x040019DF RID: 6623
		WaspKingdom2,
		// Token: 0x040019E0 RID: 6624
		WaspKingdom3,
		// Token: 0x040019E1 RID: 6625
		WaspKingdom4,
		// Token: 0x040019E2 RID: 6626
		WaspKingdom5,
		// Token: 0x040019E3 RID: 6627
		WaspKingdomPrison,
		// Token: 0x040019E4 RID: 6628
		WaspKingdomJayde,
		// Token: 0x040019E5 RID: 6629
		WaspKingdomMainHall,
		// Token: 0x040019E6 RID: 6630
		WaspKingdomThrone,
		// Token: 0x040019E7 RID: 6631
		WaspKingdomQueen,
		// Token: 0x040019E8 RID: 6632
		TermiteOutside,
		// Token: 0x040019E9 RID: 6633
		TermiteMainPlaza,
		// Token: 0x040019EA RID: 6634
		TermiteRoyalChamber,
		// Token: 0x040019EB RID: 6635
		TermiteIndustrial,
		// Token: 0x040019EC RID: 6636
		TermitePier,
		// Token: 0x040019ED RID: 6637
		TermiteColiseum1,
		// Token: 0x040019EE RID: 6638
		TermiteColiseum2,
		// Token: 0x040019EF RID: 6639
		BarrenLandsBeefly,
		// Token: 0x040019F0 RID: 6640
		BarrenLandsAntTunnel,
		// Token: 0x040019F1 RID: 6641
		BarrenLandsMiniboss,
		// Token: 0x040019F2 RID: 6642
		MetalLake,
		// Token: 0x040019F3 RID: 6643
		SnakemouthTop,
		// Token: 0x040019F4 RID: 6644
		CaveOfTrials,
		// Token: 0x040019F5 RID: 6645
		WizardTowerBasement,
		// Token: 0x040019F6 RID: 6646
		WizardTowerStairs,
		// Token: 0x040019F7 RID: 6647
		WizardTowerAttic,
		// Token: 0x040019F8 RID: 6648
		BarrenLandsPinkSpider,
		// Token: 0x040019F9 RID: 6649
		BarrenLandsTanks,
		// Token: 0x040019FA RID: 6650
		BarrenLandsMushrooms,
		// Token: 0x040019FB RID: 6651
		AbandonedCity,
		// Token: 0x040019FC RID: 6652
		BarrenLandsPumpkins,
		// Token: 0x040019FD RID: 6653
		BarrenLandsCloud,
		// Token: 0x040019FE RID: 6654
		BarrenLandsRock,
		// Token: 0x040019FF RID: 6655
		AbandonedCityTent,
		// Token: 0x04001A00 RID: 6656
		PowerPlant,
		// Token: 0x04001A01 RID: 6657
		BroodmotherLair,
		// Token: 0x04001A02 RID: 6658
		BarrenLandsSideGPT,
		// Token: 0x04001A03 RID: 6659
		GoldenPathTunnel2,
		// Token: 0x04001A04 RID: 6660
		FGClearing,
		// Token: 0x04001A05 RID: 6661
		StreamMountain4,
		// Token: 0x04001A06 RID: 6662
		GoldenPitcher1,
		// Token: 0x04001A07 RID: 6663
		StreamMountain5,
		// Token: 0x04001A08 RID: 6664
		GoldenPitcher2,
		// Token: 0x04001A09 RID: 6665
		MysteryIsland,
		// Token: 0x04001A0A RID: 6666
		MysteryIslandInside,
		// Token: 0x04001A0B RID: 6667
		UpperSnekEntrance,
		// Token: 0x04001A0C RID: 6668
		UpperSnekTransition,
		// Token: 0x04001A0D RID: 6669
		UpperSnekSwitchPuzzle,
		// Token: 0x04001A0E RID: 6670
		UpperSnekBeforeBoss,
		// Token: 0x04001A0F RID: 6671
		UpperSnekPressurePlateRoom,
		// Token: 0x04001A10 RID: 6672
		UpperSnekBossRoom,
		// Token: 0x04001A11 RID: 6673
		UpperSnekMiddleRoom,
		// Token: 0x04001A12 RID: 6674
		UpperSnekPlatformRoom,
		// Token: 0x04001A13 RID: 6675
		UpperSnekRiverPuzzle,
		// Token: 0x04001A14 RID: 6676
		UpperSnekGeizerRoom,
		// Token: 0x04001A15 RID: 6677
		RubberPrisonPier,
		// Token: 0x04001A16 RID: 6678
		RubberPrisonCheckpointCorridor,
		// Token: 0x04001A17 RID: 6679
		RubberPrisonSpikeRoom,
		// Token: 0x04001A18 RID: 6680
		RubberPrisonCells1,
		// Token: 0x04001A19 RID: 6681
		RubberPrisonCells2,
		// Token: 0x04001A1A RID: 6682
		RubberPrisonLibrary,
		// Token: 0x04001A1B RID: 6683
		RubberPrisonCafeteria,
		// Token: 0x04001A1C RID: 6684
		RubberPrisonGym,
		// Token: 0x04001A1D RID: 6685
		RubberPrisonSecurity,
		// Token: 0x04001A1E RID: 6686
		HermitCave,
		// Token: 0x04001A1F RID: 6687
		MetalIslandAuditorium,
		// Token: 0x04001A20 RID: 6688
		RubberPrisonOffice,
		// Token: 0x04001A21 RID: 6689
		RubberPrisonThirdFloor,
		// Token: 0x04001A22 RID: 6690
		RubberPrisonGiantLairBridge,
		// Token: 0x04001A23 RID: 6691
		GiantLairEntrance,
		// Token: 0x04001A24 RID: 6692
		GiantLairDeadLands1,
		// Token: 0x04001A25 RID: 6693
		GiantLairDeadLands2,
		// Token: 0x04001A26 RID: 6694
		GiantLairFridgeOutside,
		// Token: 0x04001A27 RID: 6695
		GiantLairFridgeInside,
		// Token: 0x04001A28 RID: 6696
		GiantLairRoachVillage,
		// Token: 0x04001A29 RID: 6697
		GiantLairSaplingPlains,
		// Token: 0x04001A2A RID: 6698
		PitcherPlantArena,
		// Token: 0x04001A2B RID: 6699
		BugariaEndPlaza,
		// Token: 0x04001A2C RID: 6700
		BugariaEndBridge,
		// Token: 0x04001A2D RID: 6701
		BugariaEndThrone,
		// Token: 0x04001A2E RID: 6702
		WaspKingdomDrillRoom,
		// Token: 0x04001A2F RID: 6703
		GiantLairBeforeBoss,
		// Token: 0x04001A30 RID: 6704
		GiantLairBeforeBoss2
	}

	// Token: 0x02000211 RID: 529
	public enum Items
	{
		// Token: 0x04001A32 RID: 6706
		None = -1,
		// Token: 0x04001A33 RID: 6707
		CrunchyLeaf,
		// Token: 0x04001A34 RID: 6708
		HoneyDrop,
		// Token: 0x04001A35 RID: 6709
		VitalitySeed,
		// Token: 0x04001A36 RID: 6710
		GenerousSeed,
		// Token: 0x04001A37 RID: 6711
		VigorousSeed,
		// Token: 0x04001A38 RID: 6712
		BurlySeed,
		// Token: 0x04001A39 RID: 6713
		MoneySmall,
		// Token: 0x04001A3A RID: 6714
		MoneyMedium,
		// Token: 0x04001A3B RID: 6715
		Mistake,
		// Token: 0x04001A3C RID: 6716
		CookedLeaf,
		// Token: 0x04001A3D RID: 6717
		HoneydLeaf,
		// Token: 0x04001A3E RID: 6718
		MagicDrops,
		// Token: 0x04001A3F RID: 6719
		ClearWater,
		// Token: 0x04001A40 RID: 6720
		Mushroom,
		// Token: 0x04001A41 RID: 6721
		CookedShroom,
		// Token: 0x04001A42 RID: 6722
		GlazedShroom,
		// Token: 0x04001A43 RID: 6723
		LeafSalad,
		// Token: 0x04001A44 RID: 6724
		AphidEgg,
		// Token: 0x04001A45 RID: 6725
		Omelet,
		// Token: 0x04001A46 RID: 6726
		HeartyBreakfast,
		// Token: 0x04001A47 RID: 6727
		GlazedHoney,
		// Token: 0x04001A48 RID: 6728
		HoneyShroom,
		// Token: 0x04001A49 RID: 6729
		HoneyDanger,
		// Token: 0x04001A4A RID: 6730
		HardSeed,
		// Token: 0x04001A4B RID: 6731
		DotBall,
		// Token: 0x04001A4C RID: 6732
		GBugRangerPlushie,
		// Token: 0x04001A4D RID: 6733
		DangerShroom,
		// Token: 0x04001A4E RID: 6734
		ExplorerPermit,
		// Token: 0x04001A4F RID: 6735
		CookedDanger,
		// Token: 0x04001A50 RID: 6736
		SweetCrystal,
		// Token: 0x04001A51 RID: 6737
		SpicyBomb,
		// Token: 0x04001A52 RID: 6738
		PoisonBomb,
		// Token: 0x04001A53 RID: 6739
		KingDinner,
		// Token: 0x04001A54 RID: 6740
		MushroomStick,
		// Token: 0x04001A55 RID: 6741
		RoastBerry,
		// Token: 0x04001A56 RID: 6742
		Abomihoney,
		// Token: 0x04001A57 RID: 6743
		ClearBomb,
		// Token: 0x04001A58 RID: 6744
		AntCompass,
		// Token: 0x04001A59 RID: 6745
		ChomperSeed,
		// Token: 0x04001A5A RID: 6746
		BerryJuice,
		// Token: 0x04001A5B RID: 6747
		NumbDart,
		// Token: 0x04001A5C RID: 6748
		Map,
		// Token: 0x04001A5D RID: 6749
		Ice,
		// Token: 0x04001A5E RID: 6750
		Battery,
		// Token: 0x04001A5F RID: 6751
		FrostBomb,
		// Token: 0x04001A60 RID: 6752
		NumbBomb,
		// Token: 0x04001A61 RID: 6753
		SleepBomb,
		// Token: 0x04001A62 RID: 6754
		ShavedIce,
		// Token: 0x04001A63 RID: 6755
		AphidMilk,
		// Token: 0x04001A64 RID: 6756
		HoneyIceCream,
		// Token: 0x04001A65 RID: 6757
		HoneyMilk,
		// Token: 0x04001A66 RID: 6758
		IceCream,
		// Token: 0x04001A67 RID: 6759
		LoreBook,
		// Token: 0x04001A68 RID: 6760
		FrozenSalad,
		// Token: 0x04001A69 RID: 6761
		FlowerKey,
		// Token: 0x04001A6A RID: 6762
		OfferingA,
		// Token: 0x04001A6B RID: 6763
		OfferingB,
		// Token: 0x04001A6C RID: 6764
		MothivaDoll,
		// Token: 0x04001A6D RID: 6765
		GHCrank,
		// Token: 0x04001A6E RID: 6766
		CrankHalfA,
		// Token: 0x04001A6F RID: 6767
		MainCrank,
		// Token: 0x04001A70 RID: 6768
		CrankHalfB,
		// Token: 0x04001A71 RID: 6769
		Abombhoney,
		// Token: 0x04001A72 RID: 6770
		SpyData,
		// Token: 0x04001A73 RID: 6771
		PoisonSpud,
		// Token: 0x04001A74 RID: 6772
		BakedYam,
		// Token: 0x04001A75 RID: 6773
		FrenchFries,
		// Token: 0x04001A76 RID: 6774
		BurlyChips,
		// Token: 0x04001A77 RID: 6775
		FlourBag,
		// Token: 0x04001A78 RID: 6776
		YamBread,
		// Token: 0x04001A79 RID: 6777
		SpicyCandy,
		// Token: 0x04001A7A RID: 6778
		BurlyCandy,
		// Token: 0x04001A7B RID: 6779
		DryBread,
		// Token: 0x04001A7C RID: 6780
		NutCake,
		// Token: 0x04001A7D RID: 6781
		PoisonCake,
		// Token: 0x04001A7E RID: 6782
		ShockCandy,
		// Token: 0x04001A7F RID: 6783
		PlainTea,
		// Token: 0x04001A80 RID: 6784
		TangyBerry,
		// Token: 0x04001A81 RID: 6785
		TangyJam,
		// Token: 0x04001A82 RID: 6786
		TangyJuice,
		// Token: 0x04001A83 RID: 6787
		SpicyTea,
		// Token: 0x04001A84 RID: 6788
		BurlyTea,
		// Token: 0x04001A85 RID: 6789
		FrostPie,
		// Token: 0x04001A86 RID: 6790
		HeartBerry,
		// Token: 0x04001A87 RID: 6791
		BellBerry,
		// Token: 0x04001A88 RID: 6792
		TangyPie,
		// Token: 0x04001A89 RID: 6793
		Donut,
		// Token: 0x04001A8A RID: 6794
		TangyCarpaccio,
		// Token: 0x04001A8B RID: 6795
		PoisonDart,
		// Token: 0x04001A8C RID: 6796
		BedBug,
		// Token: 0x04001A8D RID: 6797
		JellyBean,
		// Token: 0x04001A8E RID: 6798
		CookedJellyBean,
		// Token: 0x04001A8F RID: 6799
		DesertKey,
		// Token: 0x04001A90 RID: 6800
		QuestBook,
		// Token: 0x04001A91 RID: 6801
		ChomperRibbon,
		// Token: 0x04001A92 RID: 6802
		BeeCard,
		// Token: 0x04001A93 RID: 6803
		ShockShroom,
		// Token: 0x04001A94 RID: 6804
		ShellOil,
		// Token: 0x04001A95 RID: 6805
		CrimsonOre,
		// Token: 0x04001A96 RID: 6806
		BeeHat,
		// Token: 0x04001A97 RID: 6807
		BlankCard,
		// Token: 0x04001A98 RID: 6808
		SpadeCard,
		// Token: 0x04001A99 RID: 6809
		DualCard,
		// Token: 0x04001A9A RID: 6810
		TriadCard,
		// Token: 0x04001A9B RID: 6811
		FullCard,
		// Token: 0x04001A9C RID: 6812
		YinKey,
		// Token: 0x04001A9D RID: 6813
		YangKey,
		// Token: 0x04001A9E RID: 6814
		LonglegSummoner,
		// Token: 0x04001A9F RID: 6815
		StolenSilk,
		// Token: 0x04001AA0 RID: 6816
		TrialKey,
		// Token: 0x04001AA1 RID: 6817
		GameToken,
		// Token: 0x04001AA2 RID: 6818
		HideoutKey,
		// Token: 0x04001AA3 RID: 6819
		TanjerinHorn,
		// Token: 0x04001AA4 RID: 6820
		SandCastleKey,
		// Token: 0x04001AA5 RID: 6821
		SandCastleSmallKey,
		// Token: 0x04001AA6 RID: 6822
		SandCastleBossKey,
		// Token: 0x04001AA7 RID: 6823
		SnakemouthKey,
		// Token: 0x04001AA8 RID: 6824
		BombyHat,
		// Token: 0x04001AA9 RID: 6825
		SeedlingCrystal,
		// Token: 0x04001AAA RID: 6826
		WaspKey,
		// Token: 0x04001AAB RID: 6827
		JaydeStew,
		// Token: 0x04001AAC RID: 6828
		BlackCherry,
		// Token: 0x04001AAD RID: 6829
		CherryPie,
		// Token: 0x04001AAE RID: 6830
		MiracleShake,
		// Token: 0x04001AAF RID: 6831
		BerrySmoothie,
		// Token: 0x04001AB0 RID: 6832
		Squash,
		// Token: 0x04001AB1 RID: 6833
		SquashCandy,
		// Token: 0x04001AB2 RID: 6834
		SophiePetal,
		// Token: 0x04001AB3 RID: 6835
		SucculentCookie,
		// Token: 0x04001AB4 RID: 6836
		Pudding,
		// Token: 0x04001AB5 RID: 6837
		SquashSoda,
		// Token: 0x04001AB6 RID: 6838
		HPPotion,
		// Token: 0x04001AB7 RID: 6839
		ATKPotion,
		// Token: 0x04001AB8 RID: 6840
		DEFPotion,
		// Token: 0x04001AB9 RID: 6841
		TPPotion,
		// Token: 0x04001ABA RID: 6842
		SuperHPPotion,
		// Token: 0x04001ABB RID: 6843
		SuperTPPotion,
		// Token: 0x04001ABC RID: 6844
		MPPotion,
		// Token: 0x04001ABD RID: 6845
		ShadyNote,
		// Token: 0x04001ABE RID: 6846
		BlackPaint,
		// Token: 0x04001ABF RID: 6847
		RedPaint,
		// Token: 0x04001AC0 RID: 6848
		DefRootCloak,
		// Token: 0x04001AC1 RID: 6849
		AntToy,
		// Token: 0x04001AC2 RID: 6850
		MIToy,
		// Token: 0x04001AC3 RID: 6851
		MushroomCandy,
		// Token: 0x04001AC4 RID: 6852
		TermiteLunch,
		// Token: 0x04001AC5 RID: 6853
		CrownCrystal,
		// Token: 0x04001AC6 RID: 6854
		DrowsyCake,
		// Token: 0x04001AC7 RID: 6855
		LeafCroisant,
		// Token: 0x04001AC8 RID: 6856
		Package,
		// Token: 0x04001AC9 RID: 6857
		RainbowCrystal,
		// Token: 0x04001ACA RID: 6858
		FangCrystal,
		// Token: 0x04001ACB RID: 6859
		CherryBomb,
		// Token: 0x04001ACC RID: 6860
		SquashPuree,
		// Token: 0x04001ACD RID: 6861
		MiteBurg,
		// Token: 0x04001ACE RID: 6862
		ProteinShake,
		// Token: 0x04001ACF RID: 6863
		Guarana,
		// Token: 0x04001AD0 RID: 6864
		SmallGear,
		// Token: 0x04001AD1 RID: 6865
		MedGear,
		// Token: 0x04001AD2 RID: 6866
		BigGear,
		// Token: 0x04001AD3 RID: 6867
		LabCard,
		// Token: 0x04001AD4 RID: 6868
		PrisonKey,
		// Token: 0x04001AD5 RID: 6869
		HustleSeed,
		// Token: 0x04001AD6 RID: 6870
		PrisonBookA,
		// Token: 0x04001AD7 RID: 6871
		PrisonBookB,
		// Token: 0x04001AD8 RID: 6872
		PrisonBookC,
		// Token: 0x04001AD9 RID: 6873
		PrisonBookD,
		// Token: 0x04001ADA RID: 6874
		LeafUmbrella,
		// Token: 0x04001ADB RID: 6875
		PoisonRibbon,
		// Token: 0x04001ADC RID: 6876
		NumbRibbon,
		// Token: 0x04001ADD RID: 6877
		SleepRibbon,
		// Token: 0x04001ADE RID: 6878
		BigMistake,
		// Token: 0x04001ADF RID: 6879
		BurlyBomb,
		// Token: 0x04001AE0 RID: 6880
		BerryShake,
		// Token: 0x04001AE1 RID: 6881
		BadBook,
		// Token: 0x04001AE2 RID: 6882
		MechArm,
		// Token: 0x04001AE3 RID: 6883
		PlatinumCard,
		// Token: 0x04001AE4 RID: 6884
		Coffee,
		// Token: 0x04001AE5 RID: 6885
		CoffeeCandy,
		// Token: 0x04001AE6 RID: 6886
		SquashPie,
		// Token: 0x04001AE7 RID: 6887
		PlumplingPie,
		// Token: 0x04001AE8 RID: 6888
		DangerDish,
		// Token: 0x04001AE9 RID: 6889
		HoneyPancake,
		// Token: 0x04001AEA RID: 6890
		FlameRock,
		// Token: 0x04001AEB RID: 6891
		CardTrophy,
		// Token: 0x04001AEC RID: 6892
		RedRibbon,
		// Token: 0x04001AED RID: 6893
		MoneyBig
	}

	// Token: 0x02000212 RID: 530
	public enum AnimIDs
	{
		// Token: 0x04001AEF RID: 6895
		None,
		// Token: 0x04001AF0 RID: 6896
		Bee,
		// Token: 0x04001AF1 RID: 6897
		Beetle,
		// Token: 0x04001AF2 RID: 6898
		Moth,
		// Token: 0x04001AF3 RID: 6899
		LadybugKnight,
		// Token: 0x04001AF4 RID: 6900
		ButterflyGirl,
		// Token: 0x04001AF5 RID: 6901
		MessengerAnt,
		// Token: 0x04001AF6 RID: 6902
		MinerAnt1,
		// Token: 0x04001AF7 RID: 6903
		SleepyMinerAnt,
		// Token: 0x04001AF8 RID: 6904
		OverseerMinerAnt,
		// Token: 0x04001AF9 RID: 6905
		BeeMinerAnt,
		// Token: 0x04001AFA RID: 6906
		Mothiva,
		// Token: 0x04001AFB RID: 6907
		FuzzyMoth,
		// Token: 0x04001AFC RID: 6908
		MaskedMoth,
		// Token: 0x04001AFD RID: 6909
		CordycepsAnt,
		// Token: 0x04001AFE RID: 6910
		Mushroom,
		// Token: 0x04001AFF RID: 6911
		TestCube,
		// Token: 0x04001B00 RID: 6912
		TestButton,
		// Token: 0x04001B01 RID: 6913
		OldBoringBeetle,
		// Token: 0x04001B02 RID: 6914
		FlyChef,
		// Token: 0x04001B03 RID: 6915
		Jaune,
		// Token: 0x04001B04 RID: 6916
		Zasp,
		// Token: 0x04001B05 RID: 6917
		DrNeolith,
		// Token: 0x04001B06 RID: 6918
		Cactus,
		// Token: 0x04001B07 RID: 6919
		TestSign,
		// Token: 0x04001B08 RID: 6920
		SavePoint,
		// Token: 0x04001B09 RID: 6921
		DigMound,
		// Token: 0x04001B0A RID: 6922
		AntInnkeeper,
		// Token: 0x04001B0B RID: 6923
		AntKid,
		// Token: 0x04001B0C RID: 6924
		MothKid,
		// Token: 0x04001B0D RID: 6925
		Pillbug,
		// Token: 0x04001B0E RID: 6926
		OGBeetle,
		// Token: 0x04001B0F RID: 6927
		GenericAnt,
		// Token: 0x04001B10 RID: 6928
		AntSoldier1,
		// Token: 0x04001B11 RID: 6929
		Samira,
		// Token: 0x04001B12 RID: 6930
		BadgeBeetle,
		// Token: 0x04001B13 RID: 6931
		BounceShroom,
		// Token: 0x04001B14 RID: 6932
		SwitchCrystal,
		// Token: 0x04001B15 RID: 6933
		SodaCap,
		// Token: 0x04001B16 RID: 6934
		Armorpillar,
		// Token: 0x04001B17 RID: 6935
		AncientPressurePlate,
		// Token: 0x04001B18 RID: 6936
		PushRock,
		// Token: 0x04001B19 RID: 6937
		CoilyVine,
		// Token: 0x04001B1A RID: 6938
		Spuder,
		// Token: 0x04001B1B RID: 6939
		CrystalBerry,
		// Token: 0x04001B1C RID: 6940
		Seedling,
		// Token: 0x04001B1D RID: 6941
		Kina,
		// Token: 0x04001B1E RID: 6942
		Maki,
		// Token: 0x04001B1F RID: 6943
		Gen,
		// Token: 0x04001B20 RID: 6944
		Eri,
		// Token: 0x04001B21 RID: 6945
		ShielderAnt,
		// Token: 0x04001B22 RID: 6946
		TrappedMoth,
		// Token: 0x04001B23 RID: 6947
		MantisAccountant,
		// Token: 0x04001B24 RID: 6948
		AncientPlatform,
		// Token: 0x04001B25 RID: 6949
		LongAncientPlatform,
		// Token: 0x04001B26 RID: 6950
		BigCrystalSwitch,
		// Token: 0x04001B27 RID: 6951
		SmallAncientPlatform,
		// Token: 0x04001B28 RID: 6952
		Chubee,
		// Token: 0x04001B29 RID: 6953
		Thief,
		// Token: 0x04001B2A RID: 6954
		Bandit,
		// Token: 0x04001B2B RID: 6955
		SneilEnemy,
		// Token: 0x04001B2C RID: 6956
		Crickerly,
		// Token: 0x04001B2D RID: 6957
		CaravanSmolBug,
		// Token: 0x04001B2E RID: 6958
		LadybugGirl,
		// Token: 0x04001B2F RID: 6959
		LadybugBoy,
		// Token: 0x04001B30 RID: 6960
		Mar,
		// Token: 0x04001B31 RID: 6961
		Genow,
		// Token: 0x04001B32 RID: 6962
		Trist,
		// Token: 0x04001B33 RID: 6963
		OrangeBeetle,
		// Token: 0x04001B34 RID: 6964
		Acornling,
		// Token: 0x04001B35 RID: 6965
		Weevil,
		// Token: 0x04001B36 RID: 6966
		FlyTrapPlatform,
		// Token: 0x04001B37 RID: 6967
		CommonSticcBug,
		// Token: 0x04001B38 RID: 6968
		KungFuMantis,
		// Token: 0x04001B39 RID: 6969
		MrTester,
		// Token: 0x04001B3A RID: 6970
		AngryPlant,
		// Token: 0x04001B3B RID: 6971
		OmaBug,
		// Token: 0x04001B3C RID: 6972
		Madeleine,
		// Token: 0x04001B3D RID: 6973
		FlyTrap,
		// Token: 0x04001B3E RID: 6974
		MantisAcolyte,
		// Token: 0x04001B3F RID: 6975
		CurledVineGround,
		// Token: 0x04001B40 RID: 6976
		Venus,
		// Token: 0x04001B41 RID: 6977
		Bae,
		// Token: 0x04001B42 RID: 6978
		Barkeeper,
		// Token: 0x04001B43 RID: 6979
		EdgeBeetle,
		// Token: 0x04001B44 RID: 6980
		OrangeBarBug,
		// Token: 0x04001B45 RID: 6981
		Shades,
		// Token: 0x04001B46 RID: 6982
		RoyalGuard,
		// Token: 0x04001B47 RID: 6983
		MosquitoGal,
		// Token: 0x04001B48 RID: 6984
		StickShopkeeper,
		// Token: 0x04001B49 RID: 6985
		BeetleInnkeeper,
		// Token: 0x04001B4A RID: 6986
		ShyBee,
		// Token: 0x04001B4B RID: 6987
		ArrogantBee,
		// Token: 0x04001B4C RID: 6988
		BeeKid,
		// Token: 0x04001B4D RID: 6989
		ContestBee,
		// Token: 0x04001B4E RID: 6990
		AntSoldier2,
		// Token: 0x04001B4F RID: 6991
		AntCapitain,
		// Token: 0x04001B50 RID: 6992
		AntQueen,
		// Token: 0x04001B51 RID: 6993
		Libraryant1,
		// Token: 0x04001B52 RID: 6994
		Libraryant2,
		// Token: 0x04001B53 RID: 6995
		WaspTrooper,
		// Token: 0x04001B54 RID: 6996
		ScrewSwitch,
		// Token: 0x04001B55 RID: 6997
		Aphid,
		// Token: 0x04001B56 RID: 6998
		WoolyAphid,
		// Token: 0x04001B57 RID: 6999
		Cochinael,
		// Token: 0x04001B58 RID: 7000
		FatMinerAnt,
		// Token: 0x04001B59 RID: 7001
		WoodenSwitch,
		// Token: 0x04001B5A RID: 7002
		WoodenPlatform,
		// Token: 0x04001B5B RID: 7003
		MantisBarkeeper,
		// Token: 0x04001B5C RID: 7004
		SticcGirl,
		// Token: 0x04001B5D RID: 7005
		LadybugGHRed,
		// Token: 0x04001B5E RID: 7006
		LadybugGHOrange,
		// Token: 0x04001B5F RID: 7007
		SleepyStickBug,
		// Token: 0x04001B60 RID: 7008
		MantisChef,
		// Token: 0x04001B61 RID: 7009
		FarmerAnt1,
		// Token: 0x04001B62 RID: 7010
		FarmerAnt2,
		// Token: 0x04001B63 RID: 7011
		FarmerAnt3,
		// Token: 0x04001B64 RID: 7012
		BeeGuard,
		// Token: 0x04001B65 RID: 7013
		ProfHoneycomb,
		// Token: 0x04001B66 RID: 7014
		Hawk,
		// Token: 0x04001B67 RID: 7015
		Mayor,
		// Token: 0x04001B68 RID: 7016
		Midge,
		// Token: 0x04001B69 RID: 7017
		VenusGuardian,
		// Token: 0x04001B6A RID: 7018
		OldAnt,
		// Token: 0x04001B6B RID: 7019
		GenericAnt2,
		// Token: 0x04001B6C RID: 7020
		ButterflyGuy,
		// Token: 0x04001B6D RID: 7021
		FortuneTeller,
		// Token: 0x04001B6E RID: 7022
		Stratos,
		// Token: 0x04001B6F RID: 7023
		Delilah,
		// Token: 0x04001B70 RID: 7024
		BlackStickBug,
		// Token: 0x04001B71 RID: 7025
		OfferingAltar,
		// Token: 0x04001B72 RID: 7026
		Underling,
		// Token: 0x04001B73 RID: 7027
		Scarlet,
		// Token: 0x04001B74 RID: 7028
		Tanjerin,
		// Token: 0x04001B75 RID: 7029
		TanjerinHorn,
		// Token: 0x04001B76 RID: 7030
		GoldenSeedling,
		// Token: 0x04001B77 RID: 7031
		Pseudoscorp,
		// Token: 0x04001B78 RID: 7032
		Sandworm,
		// Token: 0x04001B79 RID: 7033
		Carmina,
		// Token: 0x04001B7A RID: 7034
		Burglar,
		// Token: 0x04001B7B RID: 7035
		MotherChomper,
		// Token: 0x04001B7C RID: 7036
		Sirfy,
		// Token: 0x04001B7D RID: 7037
		Isau,
		// Token: 0x04001B7E RID: 7038
		CricketShopkeeper,
		// Token: 0x04001B7F RID: 7039
		FakeLegStickBug,
		// Token: 0x04001B80 RID: 7040
		DragonflyGuy,
		// Token: 0x04001B81 RID: 7041
		CricketGuy1,
		// Token: 0x04001B82 RID: 7042
		CricketGuy2,
		// Token: 0x04001B83 RID: 7043
		FortuneSister,
		// Token: 0x04001B84 RID: 7044
		BeetleMerchantBag,
		// Token: 0x04001B85 RID: 7045
		MothMerchantBag,
		// Token: 0x04001B86 RID: 7046
		ButterflyGuyDR,
		// Token: 0x04001B87 RID: 7047
		StickBugDR,
		// Token: 0x04001B88 RID: 7048
		TermiteGirl,
		// Token: 0x04001B89 RID: 7049
		WaspInnkeeper,
		// Token: 0x04001B8A RID: 7050
		MuseumMoth,
		// Token: 0x04001B8B RID: 7051
		BumbleBarkeeper,
		// Token: 0x04001B8C RID: 7052
		CoolMosquito,
		// Token: 0x04001B8D RID: 7053
		RoachShaman,
		// Token: 0x04001B8E RID: 7054
		QueenBee,
		// Token: 0x04001B8F RID: 7055
		DocHB,
		// Token: 0x04001B90 RID: 7056
		HBAssistant,
		// Token: 0x04001B91 RID: 7057
		FashionBee,
		// Token: 0x04001B92 RID: 7058
		ToughBee,
		// Token: 0x04001B93 RID: 7059
		SmugBeeKid,
		// Token: 0x04001B94 RID: 7060
		TolBee,
		// Token: 0x04001B95 RID: 7061
		EdgeArtBee,
		// Token: 0x04001B96 RID: 7062
		FashionMoth,
		// Token: 0x04001B97 RID: 7063
		GuideBee,
		// Token: 0x04001B98 RID: 7064
		GiftShopBee,
		// Token: 0x04001B99 RID: 7065
		ChompyChan,
		// Token: 0x04001B9A RID: 7066
		BeeOverseer,
		// Token: 0x04001B9B RID: 7067
		WorkerBee1,
		// Token: 0x04001B9C RID: 7068
		WorkerBee2,
		// Token: 0x04001B9D RID: 7069
		Maldibee,
		// Token: 0x04001B9E RID: 7070
		GlassesMantis,
		// Token: 0x04001B9F RID: 7071
		BeeBot,
		// Token: 0x04001BA0 RID: 7072
		SmallMothGuy,
		// Token: 0x04001BA1 RID: 7073
		Abomihoney,
		// Token: 0x04001BA2 RID: 7074
		Turret,
		// Token: 0x04001BA3 RID: 7075
		Denmuki,
		// Token: 0x04001BA4 RID: 7076
		BeeBoss,
		// Token: 0x04001BA5 RID: 7077
		WaspScout,
		// Token: 0x04001BA6 RID: 7078
		BigSailorGuy,
		// Token: 0x04001BA7 RID: 7079
		SmallSailorGuy,
		// Token: 0x04001BA8 RID: 7080
		SailorGirl,
		// Token: 0x04001BA9 RID: 7081
		PierMantisCook,
		// Token: 0x04001BAA RID: 7082
		FatScubaAnt,
		// Token: 0x04001BAB RID: 7083
		ThinScubaAnt,
		// Token: 0x04001BAC RID: 7084
		Ahoneynation,
		// Token: 0x04001BAD RID: 7085
		Menderbot,
		// Token: 0x04001BAE RID: 7086
		ElectroPlatform,
		// Token: 0x04001BAF RID: 7087
		HoneyGrate,
		// Token: 0x04001BB0 RID: 7088
		MadeleineButler,
		// Token: 0x04001BB1 RID: 7089
		BLANK,
		// Token: 0x04001BB2 RID: 7090
		NerdyCicada,
		// Token: 0x04001BB3 RID: 7091
		Astotheles,
		// Token: 0x04001BB4 RID: 7092
		Eophi,
		// Token: 0x04001BB5 RID: 7093
		SmallMinerAnt,
		// Token: 0x04001BB6 RID: 7094
		FarmerMinerAnt,
		// Token: 0x04001BB7 RID: 7095
		ButterflyCMaster,
		// Token: 0x04001BB8 RID: 7096
		ArcadeTermite,
		// Token: 0x04001BB9 RID: 7097
		Scorpion,
		// Token: 0x04001BBA RID: 7098
		BankerAnt,
		// Token: 0x04001BBB RID: 7099
		Bulkbee,
		// Token: 0x04001BBC RID: 7100
		MantisMerchant,
		// Token: 0x04001BBD RID: 7101
		Krawler,
		// Token: 0x04001BBE RID: 7102
		Cape,
		// Token: 0x04001BBF RID: 7103
		CursedSkull,
		// Token: 0x04001BC0 RID: 7104
		IcePillarObj,
		// Token: 0x04001BC1 RID: 7105
		OldMoth,
		// Token: 0x04001BC2 RID: 7106
		PinkMoth,
		// Token: 0x04001BC3 RID: 7107
		Watcher,
		// Token: 0x04001BC4 RID: 7108
		icepillar,
		// Token: 0x04001BC5 RID: 7109
		SandPillar,
		// Token: 0x04001BC6 RID: 7110
		WaspKing,
		// Token: 0x04001BC7 RID: 7111
		Eremi,
		// Token: 0x04001BC8 RID: 7112
		RollingRock,
		// Token: 0x04001BC9 RID: 7113
		Abombhoney,
		// Token: 0x04001BCA RID: 7114
		WormBeetle,
		// Token: 0x04001BCB RID: 7115
		SnailBeetle,
		// Token: 0x04001BCC RID: 7116
		Kali,
		// Token: 0x04001BCD RID: 7117
		Bomby,
		// Token: 0x04001BCE RID: 7118
		DragonflyLady,
		// Token: 0x04001BCF RID: 7119
		CardGuard,
		// Token: 0x04001BD0 RID: 7120
		HotelRecep,
		// Token: 0x04001BD1 RID: 7121
		MIMosquito,
		// Token: 0x04001BD2 RID: 7122
		Alex,
		// Token: 0x04001BD3 RID: 7123
		Butomo,
		// Token: 0x04001BD4 RID: 7124
		HaughtyAnt,
		// Token: 0x04001BD5 RID: 7125
		SeedlingKing,
		// Token: 0x04001BD6 RID: 7126
		Yin,
		// Token: 0x04001BD7 RID: 7127
		Flowering,
		// Token: 0x04001BD8 RID: 7128
		Plumpling,
		// Token: 0x04001BD9 RID: 7129
		JumpingSpider,
		// Token: 0x04001BDA RID: 7130
		LeafbugNinja,
		// Token: 0x04001BDB RID: 7131
		LeafbugArcher,
		// Token: 0x04001BDC RID: 7132
		LeafbugClubber,
		// Token: 0x04001BDD RID: 7133
		Patton,
		// Token: 0x04001BDE RID: 7134
		MimicSpider,
		// Token: 0x04001BDF RID: 7135
		SkullCaterpillar,
		// Token: 0x04001BE0 RID: 7136
		LongLegs,
		// Token: 0x04001BE1 RID: 7137
		Centipede,
		// Token: 0x04001BE2 RID: 7138
		Lilypad,
		// Token: 0x04001BE3 RID: 7139
		ChomperBrute,
		// Token: 0x04001BE4 RID: 7140
		WoodenPPlate,
		// Token: 0x04001BE5 RID: 7141
		RopePlatform,
		// Token: 0x04001BE6 RID: 7142
		Mantidfly,
		// Token: 0x04001BE7 RID: 7143
		WaspDriller,
		// Token: 0x04001BE8 RID: 7144
		WaspBomber,
		// Token: 0x04001BE9 RID: 7145
		Jayde,
		// Token: 0x04001BEA RID: 7146
		WaspGeneral,
		// Token: 0x04001BEB RID: 7147
		WaspQueen,
		// Token: 0x04001BEC RID: 7148
		Futes,
		// Token: 0x04001BED RID: 7149
		HungryAnt,
		// Token: 0x04001BEE RID: 7150
		TraitorWasp,
		// Token: 0x04001BEF RID: 7151
		WildChomper,
		// Token: 0x04001BF0 RID: 7152
		Submarine,
		// Token: 0x04001BF1 RID: 7153
		TermiteScientist,
		// Token: 0x04001BF2 RID: 7154
		GazingTermite,
		// Token: 0x04001BF3 RID: 7155
		EdgeTermite,
		// Token: 0x04001BF4 RID: 7156
		SwordTermite,
		// Token: 0x04001BF5 RID: 7157
		TermiteInnkeeper,
		// Token: 0x04001BF6 RID: 7158
		TermiteSoldier,
		// Token: 0x04001BF7 RID: 7159
		TermiteNasute,
		// Token: 0x04001BF8 RID: 7160
		Zombee,
		// Token: 0x04001BF9 RID: 7161
		Zombeetle,
		// Token: 0x04001BFA RID: 7162
		Bloatshroom,
		// Token: 0x04001BFB RID: 7163
		TermiteShopkeeper,
		// Token: 0x04001BFC RID: 7164
		TermiteBarkeeper,
		// Token: 0x04001BFD RID: 7165
		TermiteQuestgiver,
		// Token: 0x04001BFE RID: 7166
		TermiteKing,
		// Token: 0x04001BFF RID: 7167
		TermiteQueen,
		// Token: 0x04001C00 RID: 7168
		ButterflyGirl2,
		// Token: 0x04001C01 RID: 7169
		TiredLadybug,
		// Token: 0x04001C02 RID: 7170
		StickBug2,
		// Token: 0x04001C03 RID: 7171
		WeirdTermite,
		// Token: 0x04001C04 RID: 7172
		ShortTermite,
		// Token: 0x04001C05 RID: 7173
		ShortTermite2,
		// Token: 0x04001C06 RID: 7174
		TermiteGirl2,
		// Token: 0x04001C07 RID: 7175
		BandanaTermite,
		// Token: 0x04001C08 RID: 7176
		CherryMerchant,
		// Token: 0x04001C09 RID: 7177
		WorkingTermite,
		// Token: 0x04001C0A RID: 7178
		PierTermite,
		// Token: 0x04001C0B RID: 7179
		PoorTermite,
		// Token: 0x04001C0C RID: 7180
		PoorTermiteSister,
		// Token: 0x04001C0D RID: 7181
		ScarfTermite,
		// Token: 0x04001C0E RID: 7182
		TermiteCashier,
		// Token: 0x04001C0F RID: 7183
		TrialRoach,
		// Token: 0x04001C10 RID: 7184
		ScientistRoach,
		// Token: 0x04001C11 RID: 7185
		PrimalWeevil,
		// Token: 0x04001C12 RID: 7186
		TangySeller,
		// Token: 0x04001C13 RID: 7187
		ToyMerchant,
		// Token: 0x04001C14 RID: 7188
		Wizard,
		// Token: 0x04001C15 RID: 7189
		ColiseumTermite,
		// Token: 0x04001C16 RID: 7190
		RoyalTermiteWorker,
		// Token: 0x04001C17 RID: 7191
		SittingAnt,
		// Token: 0x04001C18 RID: 7192
		TermiteOwner,
		// Token: 0x04001C19 RID: 7193
		TermiteScientist2,
		// Token: 0x04001C1A RID: 7194
		Gachapon,
		// Token: 0x04001C1B RID: 7195
		MaskedMF1,
		// Token: 0x04001C1C RID: 7196
		MaskedMF2,
		// Token: 0x04001C1D RID: 7197
		MaskedMF3,
		// Token: 0x04001C1E RID: 7198
		MaskedMF4,
		// Token: 0x04001C1F RID: 7199
		FalseMonarch,
		// Token: 0x04001C20 RID: 7200
		Mothfly,
		// Token: 0x04001C21 RID: 7201
		MothflyCluster,
		// Token: 0x04001C22 RID: 7202
		Ironclad,
		// Token: 0x04001C23 RID: 7203
		ToeBiter,
		// Token: 0x04001C24 RID: 7204
		MidgeBroodmother,
		// Token: 0x04001C25 RID: 7205
		FLMinerAnt,
		// Token: 0x04001C26 RID: 7206
		FGMinerAnt,
		// Token: 0x04001C27 RID: 7207
		RPMinerAnt,
		// Token: 0x04001C28 RID: 7208
		CloakAnt,
		// Token: 0x04001C29 RID: 7209
		CicadaGuy2,
		// Token: 0x04001C2A RID: 7210
		StickbugGuy2,
		// Token: 0x04001C2B RID: 7211
		VeilBee,
		// Token: 0x04001C2C RID: 7212
		BumblebeeGirl,
		// Token: 0x04001C2D RID: 7213
		SmallBeetleGuy,
		// Token: 0x04001C2E RID: 7214
		LeafAnt,
		// Token: 0x04001C2F RID: 7215
		BookAnt,
		// Token: 0x04001C30 RID: 7216
		SmugWeakGuy,
		// Token: 0x04001C31 RID: 7217
		Layna,
		// Token: 0x04001C32 RID: 7218
		LaynaPet,
		// Token: 0x04001C33 RID: 7219
		BrotherTermite,
		// Token: 0x04001C34 RID: 7220
		SeedlingTermite,
		// Token: 0x04001C35 RID: 7221
		JojoTermite,
		// Token: 0x04001C36 RID: 7222
		WrappedTermite,
		// Token: 0x04001C37 RID: 7223
		Strider,
		// Token: 0x04001C38 RID: 7224
		DivingSpider,
		// Token: 0x04001C39 RID: 7225
		Cenn,
		// Token: 0x04001C3A RID: 7226
		Pisci,
		// Token: 0x04001C3B RID: 7227
		Ruffian,
		// Token: 0x04001C3C RID: 7228
		SandWyrm,
		// Token: 0x04001C3D RID: 7229
		SandWyrmTail,
		// Token: 0x04001C3E RID: 7230
		StagBeetle,
		// Token: 0x04001C3F RID: 7231
		PisciWall,
		// Token: 0x04001C40 RID: 7232
		Riz,
		// Token: 0x04001C41 RID: 7233
		RizSister,
		// Token: 0x04001C42 RID: 7234
		RizGrandpa,
		// Token: 0x04001C43 RID: 7235
		BackerStickBug,
		// Token: 0x04001C44 RID: 7236
		Zommoth,
		// Token: 0x04001C45 RID: 7237
		FitTermite,
		// Token: 0x04001C46 RID: 7238
		TermiteNasuteSmol,
		// Token: 0x04001C47 RID: 7239
		PeacockSpider,
		// Token: 0x04001C48 RID: 7240
		PrisonGate,
		// Token: 0x04001C49 RID: 7241
		PrisonGateLocal,
		// Token: 0x04001C4A RID: 7242
		SteelSwitch,
		// Token: 0x04001C4B RID: 7243
		MoleCricketGuy,
		// Token: 0x04001C4C RID: 7244
		MoleCricketGirl,
		// Token: 0x04001C4D RID: 7245
		UltimaxTank,
		// Token: 0x04001C4E RID: 7246
		Kenny,
		// Token: 0x04001C4F RID: 7247
		DeadLanderA,
		// Token: 0x04001C50 RID: 7248
		DeadLanderB,
		// Token: 0x04001C51 RID: 7249
		DeadLanderC,
		// Token: 0x04001C52 RID: 7250
		RoachElder,
		// Token: 0x04001C53 RID: 7251
		BuffRoachGuy,
		// Token: 0x04001C54 RID: 7252
		WalkingRoachGuy,
		// Token: 0x04001C55 RID: 7253
		BeeButler,
		// Token: 0x04001C56 RID: 7254
		BigBeetle,
		// Token: 0x04001C57 RID: 7255
		CowboyStickbug,
		// Token: 0x04001C58 RID: 7256
		RichAnt,
		// Token: 0x04001C59 RID: 7257
		RichKid,
		// Token: 0x04001C5A RID: 7258
		ShadyLadybug,
		// Token: 0x04001C5B RID: 7259
		RichMoth,
		// Token: 0x04001C5C RID: 7260
		TeaMoth,
		// Token: 0x04001C5D RID: 7261
		MaskStickbug,
		// Token: 0x04001C5E RID: 7262
		WindUp,
		// Token: 0x04001C5F RID: 7263
		PierGirl,
		// Token: 0x04001C60 RID: 7264
		KeyR,
		// Token: 0x04001C61 RID: 7265
		KeyL,
		// Token: 0x04001C62 RID: 7266
		Tablet,
		// Token: 0x04001C63 RID: 7267
		EverlastingKing,
		// Token: 0x04001C64 RID: 7268
		YinMoth,
		// Token: 0x04001C65 RID: 7269
		Pitcher,
		// Token: 0x04001C66 RID: 7270
		PitcherSummon,
		// Token: 0x04001C67 RID: 7271
		Poppy,
		// Token: 0x04001C68 RID: 7272
		BookWaspGuy,
		// Token: 0x04001C69 RID: 7273
		CardBumblebee,
		// Token: 0x04001C6A RID: 7274
		Effo,
		// Token: 0x04001C6B RID: 7275
		DragonflyBlacksmith,
		// Token: 0x04001C6C RID: 7276
		WaspTwinA,
		// Token: 0x04001C6D RID: 7277
		WaspTwinB,
		// Token: 0x04001C6E RID: 7278
		WaspBoyfriend,
		// Token: 0x04001C6F RID: 7279
		BombMaster,
		// Token: 0x04001C70 RID: 7280
		BombFanatic,
		// Token: 0x04001C71 RID: 7281
		OldSailorMantis,
		// Token: 0x04001C72 RID: 7282
		CardWasp,
		// Token: 0x04001C73 RID: 7283
		CardBumble,
		// Token: 0x04001C74 RID: 7284
		CardBee,
		// Token: 0x04001C75 RID: 7285
		CardJudge,
		// Token: 0x04001C76 RID: 7286
		Comfy,
		// Token: 0x04001C77 RID: 7287
		Soto,
		// Token: 0x04001C78 RID: 7288
		RoachGirl,
		// Token: 0x04001C79 RID: 7289
		RichMantis,
		// Token: 0x04001C7A RID: 7290
		RichStagBeetle,
		// Token: 0x04001C7B RID: 7291
		RichRhinoBeetle,
		// Token: 0x04001C7C RID: 7292
		CardStickbug,
		// Token: 0x04001C7D RID: 7293
		MasterSlice,
		// Token: 0x04001C7E RID: 7294
		BounceShroom2,
		// Token: 0x04001C7F RID: 7295
		FireKrawler,
		// Token: 0x04001C80 RID: 7296
		FireCape,
		// Token: 0x04001C81 RID: 7297
		FireWarden,
		// Token: 0x04001C82 RID: 7298
		TermiteGirl3,
		// Token: 0x04001C83 RID: 7299
		Cerise,
		// Token: 0x04001C84 RID: 7300
		WoodenPPlate2,
		// Token: 0x04001C85 RID: 7301
		RecipeGuy,
		// Token: 0x04001C86 RID: 7302
		Roy
	}

	// Token: 0x02000213 RID: 531
	private enum Commands
	{
		// Token: 0x04001C88 RID: 7304
		String,
		// Token: 0x04001C89 RID: 7305
		Var,
		// Token: 0x04001C8A RID: 7306
		Anstring,
		// Token: 0x04001C8B RID: 7307
		Checkitem,
		// Token: 0x04001C8C RID: 7308
		Prompt,
		// Token: 0x04001C8D RID: 7309
		Getitem,
		// Token: 0x04001C8E RID: 7310
		Line,
		// Token: 0x04001C8F RID: 7311
		Next,
		// Token: 0x04001C90 RID: 7312
		End,
		// Token: 0x04001C91 RID: 7313
		Break,
		// Token: 0x04001C92 RID: 7314
		Blank,
		// Token: 0x04001C93 RID: 7315
		Lock,
		// Token: 0x04001C94 RID: 7316
		Cancelaction,
		// Token: 0x04001C95 RID: 7317
		Center,
		// Token: 0x04001C96 RID: 7318
		Halfline,
		// Token: 0x04001C97 RID: 7319
		Stopskip,
		// Token: 0x04001C98 RID: 7320
		Noskip,
		// Token: 0x04001C99 RID: 7321
		Hide,
		// Token: 0x04001C9A RID: 7322
		Rainbow,
		// Token: 0x04001C9B RID: 7323
		Shaky,
		// Token: 0x04001C9C RID: 7324
		Wavy,
		// Token: 0x04001C9D RID: 7325
		Glitchy,
		// Token: 0x04001C9E RID: 7326
		Buffer,
		// Token: 0x04001C9F RID: 7327
		Overfollower,
		// Token: 0x04001CA0 RID: 7328
		Choicewave,
		// Token: 0x04001CA1 RID: 7329
		Spd,
		// Token: 0x04001CA2 RID: 7330
		Speed,
		// Token: 0x04001CA3 RID: 7331
		Color,
		// Token: 0x04001CA4 RID: 7332
		Anim,
		// Token: 0x04001CA5 RID: 7333
		Sort,
		// Token: 0x04001CA6 RID: 7334
		Save,
		// Token: 0x04001CA7 RID: 7335
		Parent,
		// Token: 0x04001CA8 RID: 7336
		Tail,
		// Token: 0x04001CA9 RID: 7337
		Flag,
		// Token: 0x04001CAA RID: 7338
		Checkmoney,
		// Token: 0x04001CAB RID: 7339
		Checkinvqtd,
		// Token: 0x04001CAC RID: 7340
		Additemtoss,
		// Token: 0x04001CAD RID: 7341
		Additem,
		// Token: 0x04001CAE RID: 7342
		Money,
		// Token: 0x04001CAF RID: 7343
		Goto,
		// Token: 0x04001CB0 RID: 7344
		Currency,
		// Token: 0x04001CB1 RID: 7345
		Kill,
		// Token: 0x04001CB2 RID: 7346
		Boxstyle,
		// Token: 0x04001CB3 RID: 7347
		Pickitem,
		// Token: 0x04001CB4 RID: 7348
		Checktrue,
		// Token: 0x04001CB5 RID: 7349
		Removeitem,
		// Token: 0x04001CB6 RID: 7350
		Getstorage,
		// Token: 0x04001CB7 RID: 7351
		Button,
		// Token: 0x04001CB8 RID: 7352
		Movewait,
		// Token: 0x04001CB9 RID: 7353
		Move,
		// Token: 0x04001CBA RID: 7354
		Forcewait,
		// Token: 0x04001CBB RID: 7355
		Wait,
		// Token: 0x04001CBC RID: 7356
		Face,
		// Token: 0x04001CBD RID: 7357
		Camtarget,
		// Token: 0x04001CBE RID: 7358
		Flip,
		// Token: 0x04001CBF RID: 7359
		Warp,
		// Token: 0x04001CC0 RID: 7360
		Transfer,
		// Token: 0x04001CC1 RID: 7361
		NumberPrompt,
		// Token: 0x04001CC2 RID: 7362
		Common,
		// Token: 0x04001CC3 RID: 7363
		Checkvar,
		// Token: 0x04001CC4 RID: 7364
		Fwait,
		// Token: 0x04001CC5 RID: 7365
		FadeIn,
		// Token: 0x04001CC6 RID: 7366
		FadeOut,
		// Token: 0x04001CC7 RID: 7367
		LeafIn,
		// Token: 0x04001CC8 RID: 7368
		LeafOut,
		// Token: 0x04001CC9 RID: 7369
		Align,
		// Token: 0x04001CCA RID: 7370
		Discovery,
		// Token: 0x04001CCB RID: 7371
		Size,
		// Token: 0x04001CCC RID: 7372
		Minibubble,
		// Token: 0x04001CCD RID: 7373
		Destroyminibubble,
		// Token: 0x04001CCE RID: 7374
		Halt,
		// Token: 0x04001CCF RID: 7375
		Waitminibubble,
		// Token: 0x04001CD0 RID: 7376
		Showmoney,
		// Token: 0x04001CD1 RID: 7377
		Hidemoney,
		// Token: 0x04001CD2 RID: 7378
		Innsleep,
		// Token: 0x04001CD3 RID: 7379
		Event,
		// Token: 0x04001CD4 RID: 7380
		Checkregional,
		// Token: 0x04001CD5 RID: 7381
		Checkflag,
		// Token: 0x04001CD6 RID: 7382
		Regionalflag,
		// Token: 0x04001CD7 RID: 7383
		Createitem,
		// Token: 0x04001CD8 RID: 7384
		Addboard,
		// Token: 0x04001CD9 RID: 7385
		Openboard,
		// Token: 0x04001CDA RID: 7386
		Camangle,
		// Token: 0x04001CDB RID: 7387
		Camoffset,
		// Token: 0x04001CDC RID: 7388
		Completequest,
		// Token: 0x04001CDD RID: 7389
		Activateselectedquest,
		// Token: 0x04001CDE RID: 7390
		Resetcamera,
		// Token: 0x04001CDF RID: 7391
		Quarterline,
		// Token: 0x04001CE0 RID: 7392
		Questprompt,
		// Token: 0x04001CE1 RID: 7393
		Heal,
		// Token: 0x04001CE2 RID: 7394
		Lockmovement,
		// Token: 0x04001CE3 RID: 7395
		Teleportparty,
		// Token: 0x04001CE4 RID: 7396
		Flagvalue,
		// Token: 0x04001CE5 RID: 7397
		Removebadgeshop,
		// Token: 0x04001CE6 RID: 7398
		Savecamera,
		// Token: 0x04001CE7 RID: 7399
		Loadcamera,
		// Token: 0x04001CE8 RID: 7400
		Camspeed,
		// Token: 0x04001CE9 RID: 7401
		Stars,
		// Token: 0x04001CEA RID: 7402
		Giveitem,
		// Token: 0x04001CEB RID: 7403
		Tailextra,
		// Token: 0x04001CEC RID: 7404
		Setvar,
		// Token: 0x04001CED RID: 7405
		Gettail,
		// Token: 0x04001CEE RID: 7406
		Jump,
		// Token: 0x04001CEF RID: 7407
		Position,
		// Token: 0x04001CF0 RID: 7408
		Hidespeed,
		// Token: 0x04001CF1 RID: 7409
		Breakflag,
		// Token: 0x04001CF2 RID: 7410
		Bleep,
		// Token: 0x04001CF3 RID: 7411
		Exitgame,
		// Token: 0x04001CF4 RID: 7412
		Openpause,
		// Token: 0x04001CF5 RID: 7413
		Font,
		// Token: 0x04001CF6 RID: 7414
		Exp,
		// Token: 0x04001CF7 RID: 7415
		Icon,
		// Token: 0x04001CF8 RID: 7416
		Dropshadow,
		// Token: 0x04001CF9 RID: 7417
		Level,
		// Token: 0x04001CFA RID: 7418
		Clonestring,
		// Token: 0x04001CFB RID: 7419
		Pauseline,
		// Token: 0x04001CFC RID: 7420
		Addfollower,
		// Token: 0x04001CFD RID: 7421
		Fadeletter,
		// Token: 0x04001CFE RID: 7422
		Setprize,
		// Token: 0x04001CFF RID: 7423
		Libraryline,
		// Token: 0x04001D00 RID: 7424
		Shopline,
		// Token: 0x04001D01 RID: 7425
		Shakecamera,
		// Token: 0x04001D02 RID: 7426
		Removemaplimits,
		// Token: 0x04001D03 RID: 7427
		Resetmaplimits,
		// Token: 0x04001D04 RID: 7428
		Music,
		// Token: 0x04001D05 RID: 7429
		Sound,
		// Token: 0x04001D06 RID: 7430
		Destroydescbox,
		// Token: 0x04001D07 RID: 7431
		Resetregion,
		// Token: 0x04001D08 RID: 7432
		Mapflag,
		// Token: 0x04001D09 RID: 7433
		Checkmapflag,
		// Token: 0x04001D0A RID: 7434
		Kinematicplayer,
		// Token: 0x04001D0B RID: 7435
		Removefollower,
		// Token: 0x04001D0C RID: 7436
		Shoppool,
		// Token: 0x04001D0D RID: 7437
		Optiontovar,
		// Token: 0x04001D0E RID: 7438
		Librarybreak,
		// Token: 0x04001D0F RID: 7439
		Setbreak,
		// Token: 0x04001D10 RID: 7440
		Librarybook,
		// Token: 0x04001D11 RID: 7441
		Area,
		// Token: 0x04001D12 RID: 7442
		Sstring,
		// Token: 0x04001D13 RID: 7443
		Menu,
		// Token: 0x04001D14 RID: 7444
		Battle,
		// Token: 0x04001D15 RID: 7445
		Removestat,
		// Token: 0x04001D16 RID: 7446
		Addstat,
		// Token: 0x04001D17 RID: 7447
		Checkpos,
		// Token: 0x04001D18 RID: 7448
		Triui,
		// Token: 0x04001D19 RID: 7449
		Backline,
		// Token: 0x04001D1A RID: 7450
		Cardbattle,
		// Token: 0x04001D1B RID: 7451
		Boxspeed,
		// Token: 0x04001D1C RID: 7452
		Battlewon,
		// Token: 0x04001D1D RID: 7453
		Switch,
		// Token: 0x04001D1E RID: 7454
		Checkminibubble,
		// Token: 0x04001D1F RID: 7455
		Breakend,
		// Token: 0x04001D20 RID: 7456
		DungeonGame,
		// Token: 0x04001D21 RID: 7457
		BeeGame,
		// Token: 0x04001D22 RID: 7458
		Copyvar,
		// Token: 0x04001D23 RID: 7459
		Addvar,
		// Token: 0x04001D24 RID: 7460
		Showtokens,
		// Token: 0x04001D25 RID: 7461
		Call,
		// Token: 0x04001D26 RID: 7462
		Igcolmove,
		// Token: 0x04001D27 RID: 7463
		PartyGame,
		// Token: 0x04001D28 RID: 7464
		LetterPrompt,
		// Token: 0x04001D29 RID: 7465
		Define,
		// Token: 0x04001D2A RID: 7466
		Loadmap,
		// Token: 0x04001D2B RID: 7467
		Checkanim,
		// Token: 0x04001D2C RID: 7468
		Camlimit,
		// Token: 0x04001D2D RID: 7469
		Waitcn,
		// Token: 0x04001D2E RID: 7470
		Faketail,
		// Token: 0x04001D2F RID: 7471
		Unpauseline,
		// Token: 0x04001D30 RID: 7472
		Cberrytotal,
		// Token: 0x04001D31 RID: 7473
		Medaltotal,
		// Token: 0x04001D32 RID: 7474
		Single,
		// Token: 0x04001D33 RID: 7475
		Tab,
		// Token: 0x04001D34 RID: 7476
		Singlebreak,
		// Token: 0x04001D35 RID: 7477
		Librarysize,
		// Token: 0x04001D36 RID: 7478
		Mothfly,
		// Token: 0x04001D37 RID: 7479
		Chapterintro,
		// Token: 0x04001D38 RID: 7480
		Backbox,
		// Token: 0x04001D39 RID: 7481
		Testdiag,
		// Token: 0x04001D3A RID: 7482
		Lore,
		// Token: 0x04001D3B RID: 7483
		Follow,
		// Token: 0x04001D3C RID: 7484
		Transitionsort,
		// Token: 0x04001D3D RID: 7485
		Moveahead,
		// Token: 0x04001D3E RID: 7486
		Addquest,
		// Token: 0x04001D3F RID: 7487
		Textangle,
		// Token: 0x04001D40 RID: 7488
		Particle,
		// Token: 0x04001D41 RID: 7489
		Itemname,
		// Token: 0x04001D42 RID: 7490
		Addprize,
		// Token: 0x04001D43 RID: 7491
		Entityalive,
		// Token: 0x04001D44 RID: 7492
		Scorecheck,
		// Token: 0x04001D45 RID: 7493
		Fademusic,
		// Token: 0x04001D46 RID: 7494
		Limit,
		// Token: 0x04001D47 RID: 7495
		Termacadecheck,
		// Token: 0x04001D48 RID: 7496
		Removeitemat,
		// Token: 0x04001D49 RID: 7497
		Unpausesize,
		// Token: 0x04001D4A RID: 7498
		Alwaysactive,
		// Token: 0x04001D4B RID: 7499
		Lockbacktrack,
		// Token: 0x04001D4C RID: 7500
		Fixchompy,
		// Token: 0x04001D4D RID: 7501
		Updateanim,
		// Token: 0x04001D4E RID: 7502
		Checkallquests,
		// Token: 0x04001D4F RID: 7503
		Takeopenquests,
		// Token: 0x04001D50 RID: 7504
		Deathsmoke,
		// Token: 0x04001D51 RID: 7505
		Emoticon,
		// Token: 0x04001D52 RID: 7506
		Checksum,
		// Token: 0x04001D53 RID: 7507
		Questsize,
		// Token: 0x04001D54 RID: 7508
		Questbreak,
		// Token: 0x04001D55 RID: 7509
		Mapsize,
		// Token: 0x04001D56 RID: 7510
		Caravanmedal,
		// Token: 0x04001D57 RID: 7511
		Name,
		// Token: 0x04001D58 RID: 7512
		Itemvalue,
		// Token: 0x04001D59 RID: 7513
		Sizemulti,
		// Token: 0x04001D5A RID: 7514
		Ignorenext,
		// Token: 0x04001D5B RID: 7515
		Rerollshops,
		// Token: 0x04001D5C RID: 7516
		Maxmedals,
		// Token: 0x04001D5D RID: 7517
		Battlesize,
		// Token: 0x04001D5E RID: 7518
		Pausesize,
		// Token: 0x04001D5F RID: 7519
		GetFromMap,
		// Token: 0x04001D60 RID: 7520
		Listsize,
		// Token: 0x04001D61 RID: 7521
		Plural,
		// Token: 0x04001D62 RID: 7522
		Layer
	}

	// Token: 0x02000214 RID: 532
	public struct ItemUse
	{
		// Token: 0x04001D63 RID: 7523
		public MainManager.ItemUsage[] usetype;

		// Token: 0x04001D64 RID: 7524
		public int[] values;
	}

	// Token: 0x02000215 RID: 533
	public struct BattleData
	{
		// Token: 0x04001D65 RID: 7525
		public int hp;

		// Token: 0x04001D66 RID: 7526
		public int maxhp;

		// Token: 0x04001D67 RID: 7527
		public int basehp;

		// Token: 0x04001D68 RID: 7528
		public int lv;

		// Token: 0x04001D69 RID: 7529
		public int atk;

		// Token: 0x04001D6A RID: 7530
		public int def;

		// Token: 0x04001D6B RID: 7531
		public int exp;

		// Token: 0x04001D6C RID: 7532
		public int freezeres;

		// Token: 0x04001D6D RID: 7533
		public int poisonres;

		// Token: 0x04001D6E RID: 7534
		public int numbres;

		// Token: 0x04001D6F RID: 7535
		public int sleepres;

		// Token: 0x04001D70 RID: 7536
		public int animid;

		// Token: 0x04001D71 RID: 7537
		public int money;

		// Token: 0x04001D72 RID: 7538
		public int hpt;

		// Token: 0x04001D73 RID: 7539
		public int id;

		// Token: 0x04001D74 RID: 7540
		public int baseatk;

		// Token: 0x04001D75 RID: 7541
		public int basedef;

		// Token: 0x04001D76 RID: 7542
		public int cantmove;

		// Token: 0x04001D77 RID: 7543
		public int tired;

		// Token: 0x04001D78 RID: 7544
		public int charge;

		// Token: 0x04001D79 RID: 7545
		public int trueid;

		// Token: 0x04001D7A RID: 7546
		public int eventondeath;

		// Token: 0x04001D7B RID: 7547
		public int moves;

		// Token: 0x04001D7C RID: 7548
		public int deathtype;

		// Token: 0x04001D7D RID: 7549
		public int hardhp;

		// Token: 0x04001D7E RID: 7550
		public int hardatk;

		// Token: 0x04001D7F RID: 7551
		public int harddef;

		// Token: 0x04001D80 RID: 7552
		public int holditem;

		// Token: 0x04001D81 RID: 7553
		public int defenseonhit;

		// Token: 0x04001D82 RID: 7554
		public int eventonfall;

		// Token: 0x04001D83 RID: 7555
		public int onhitaction;

		// Token: 0x04001D84 RID: 7556
		public int turnssincedeath;

		// Token: 0x04001D85 RID: 7557
		public int turnsalive;

		// Token: 0x04001D86 RID: 7558
		public int moreturnnextturn;

		// Token: 0x04001D87 RID: 7559
		public int pointer;

		// Token: 0x04001D88 RID: 7560
		public int turnsnodamage;

		// Token: 0x04001D89 RID: 7561
		public int blockTimes;

		// Token: 0x04001D8A RID: 7562
		public int[] chargeonotherenemy;

		// Token: 0x04001D8B RID: 7563
		public float size;

		// Token: 0x04001D8C RID: 7564
		public float weight;

		// Token: 0x04001D8D RID: 7565
		public float sizeonfreeze;

		// Token: 0x04001D8E RID: 7566
		public float initialsize;

		// Token: 0x04001D8F RID: 7567
		public Vector3 cursoroffset;

		// Token: 0x04001D90 RID: 7568
		public Vector3 battlepos;

		// Token: 0x04001D91 RID: 7569
		public Vector3 itemoffset;

		// Token: 0x04001D92 RID: 7570
		public List<int[]> condition;

		// Token: 0x04001D93 RID: 7571
		public List<int> skills;

		// Token: 0x04001D94 RID: 7572
		public List<int> delayedcondition;

		// Token: 0x04001D95 RID: 7573
		public int[] data;

		// Token: 0x04001D96 RID: 7574
		public SpriteRenderer helditem;

		// Token: 0x04001D97 RID: 7575
		public EntityControl entity;

		// Token: 0x04001D98 RID: 7576
		public EntityControl battleentity;

		// Token: 0x04001D99 RID: 7577
		public EntityControl ate;

		// Token: 0x04001D9A RID: 7578
		public EntityControl eatenby;

		// Token: 0x04001D9B RID: 7579
		public BattleControl.BattlePosition position;

		// Token: 0x04001D9C RID: 7580
		public List<BattleControl.AttackProperty> weakness;

		// Token: 0x04001D9D RID: 7581
		public Transform[] extrastuff;

		// Token: 0x04001D9E RID: 7582
		public string entityname;

		// Token: 0x04001D9F RID: 7583
		public ParticleSystem tiredpart;

		// Token: 0x04001DA0 RID: 7584
		public ParticleSystem frostbitep;

		// Token: 0x04001DA1 RID: 7585
		public bool lockskills;

		// Token: 0x04001DA2 RID: 7586
		public bool lockitems;

		// Token: 0x04001DA3 RID: 7587
		public bool locktri;

		// Token: 0x04001DA4 RID: 7588
		public bool haspassed;

		// Token: 0x04001DA5 RID: 7589
		public bool lockrelayreceive;

		// Token: 0x04001DA6 RID: 7590
		public bool cantfall;

		// Token: 0x04001DA7 RID: 7591
		public bool notaunt;

		// Token: 0x04001DA8 RID: 7592
		public bool noblock;

		// Token: 0x04001DA9 RID: 7593
		public bool fixedexp;

		// Token: 0x04001DAA RID: 7594
		public bool notired;

		// Token: 0x04001DAB RID: 7595
		public bool hidehp;

		// Token: 0x04001DAC RID: 7596
		public bool isdefending;

		// Token: 0x04001DAD RID: 7597
		public bool fled;

		// Token: 0x04001DAE RID: 7598
		public bool isasleep;

		// Token: 0x04001DAF RID: 7599
		public bool notattle;

		// Token: 0x04001DB0 RID: 7600
		public bool hitaction;

		// Token: 0x04001DB1 RID: 7601
		public bool actimmobile;

		// Token: 0x04001DB2 RID: 7602
		public bool isnumb;

		// Token: 0x04001DB3 RID: 7603
		public bool didnothing;

		// Token: 0x04001DB4 RID: 7604
		public bool diebyitself;

		// Token: 0x04001DB5 RID: 7605
		public bool noexpatstart;

		// Token: 0x04001DB6 RID: 7606
		public bool atkdownonloseatkup;

		// Token: 0x04001DB7 RID: 7607
		public bool lockposition;

		// Token: 0x04001DB8 RID: 7608
		public bool plating;

		// Token: 0x04001DB9 RID: 7609
		public bool alreadycounted;

		// Token: 0x04001DBA RID: 7610
		public bool destroyentity;

		// Token: 0x04001DBB RID: 7611
		public bool frozenlastturn;

		// Token: 0x04001DBC RID: 7612
		public bool lockcantmove;
	}

	// Token: 0x02000216 RID: 534
	public struct Entity_Data
	{
		// Token: 0x04001DBD RID: 7613
		public Vector3 freezesize;

		// Token: 0x04001DBE RID: 7614
		public Vector3 freezeoffset;

		// Token: 0x04001DBF RID: 7615
		public Vector3 freezeflipoffset;

		// Token: 0x04001DC0 RID: 7616
		public Vector3 modeloffset;

		// Token: 0x04001DC1 RID: 7617
		public Vector3 startscale;

		// Token: 0x04001DC2 RID: 7618
		public Vector3 modelscale;

		// Token: 0x04001DC3 RID: 7619
		public bool ismodel;

		// Token: 0x04001DC4 RID: 7620
		public bool diganim;

		// Token: 0x04001DC5 RID: 7621
		public bool freezenofall;

		// Token: 0x04001DC6 RID: 7622
		public bool shakeondrop;

		// Token: 0x04001DC7 RID: 7623
		public bool noshadow;

		// Token: 0x04001DC8 RID: 7624
		public bool dontoverridejump;

		// Token: 0x04001DC9 RID: 7625
		public bool hasiceanim;

		// Token: 0x04001DCA RID: 7626
		public bool noflyanim;

		// Token: 0x04001DCB RID: 7627
		public bool forceshadow;

		// Token: 0x04001DCC RID: 7628
		public bool Object;

		// Token: 0x04001DCD RID: 7629
		public int bleepid;

		// Token: 0x04001DCE RID: 7630
		public int basestate;

		// Token: 0x04001DCF RID: 7631
		public int basewalk;

		// Token: 0x04001DD0 RID: 7632
		public float shadowsize;

		// Token: 0x04001DD1 RID: 7633
		public float bleeppitch;

		// Token: 0x04001DD2 RID: 7634
		public float minheight;

		// Token: 0x04001DD3 RID: 7635
		public float startheight;

		// Token: 0x04001DD4 RID: 7636
		public float startbobspd;

		// Token: 0x04001DD5 RID: 7637
		public float startbobfreq;

		// Token: 0x04001DD6 RID: 7638
		public string[] preloaddata;

		// Token: 0x04001DD7 RID: 7639
		public EntityControl.WalkType walktype;
	}

	// Token: 0x02000217 RID: 535
	public enum TPDir
	{
		// Token: 0x04001DD9 RID: 7641
		Right,
		// Token: 0x04001DDA RID: 7642
		Left,
		// Token: 0x04001DDB RID: 7643
		Up,
		// Token: 0x04001DDC RID: 7644
		Down,
		// Token: 0x04001DDD RID: 7645
		Away,
		// Token: 0x04001DDE RID: 7646
		Center
	}

	// Token: 0x02000218 RID: 536
	public struct LoadData
	{
		// Token: 0x04001DDF RID: 7647
		public Vector3 loadpos;

		// Token: 0x04001DE0 RID: 7648
		public int mapid;

		// Token: 0x04001DE1 RID: 7649
		public int areaid;

		// Token: 0x04001DE2 RID: 7650
		public int level;

		// Token: 0x04001DE3 RID: 7651
		public int times;

		// Token: 0x04001DE4 RID: 7652
		public int timem;

		// Token: 0x04001DE5 RID: 7653
		public int timeh;

		// Token: 0x04001DE6 RID: 7654
		public int progression;

		// Token: 0x04001DE7 RID: 7655
		public string filename;

		// Token: 0x04001DE8 RID: 7656
		public bool[] challenges;
	}
}
