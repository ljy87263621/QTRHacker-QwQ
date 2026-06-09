using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;

namespace QTRHacker.Patches
{
	public static class PlayerToggles
	{
		public static bool InfiniteLife;
		public static bool InfiniteMana;
		public static bool InfiniteOxygen;
		public static bool InfiniteMinion;
		public static bool InfiniteAmmo;
		public static bool InfiniteFlyTime;
		public static bool CreativeMenu;
		public static bool ImmuneToDebuffs;
		public static bool SlowFall;
		public static bool FastSpeed;
		public static bool SuperGrabRange;
		public static bool CoinPortalDropsBags;
		public static bool FishCratesOnly;
		public static bool BonusTwoSlots;
		public static bool HighLight;
		public static bool SuperRange;
		public static bool FastTileAndWallPlacingSpeed;
		public static bool MechanicalRuler;
		public static bool MechanicalLens;
		public static bool RightClickToTP;
		public static bool EnableAllRecipes;
		public static bool StrengthenVampireKnives;
		private static int CreativeMenuOriginalDifficulty = -1;
		private static float OriginalGlobalBrightness = float.NaN;
		private static bool[] ProcessedVampireKnives = Array.Empty<bool>();

		public static void Apply()
		{
			ReadSharedState();
			ApplyHighLight();
			if (Main.gameMenu)
			{
				RestoreCreativeMenuDifficulty();
				return;
			}

			Player player = Main.LocalPlayer;
			if (player == null || !player.active)
			{
				RestoreCreativeMenuDifficulty();
				return;
			}

			Apply(player, normalizePlacementSpeed: true);
		}

		private static void Apply(Player player, bool normalizePlacementSpeed)
		{
			ApplyPlayerPropertyInfluenceOverrides(player);
			ApplyCreativeMenu(player);
			if (InfiniteLife && player.statLifeMax2 > 0)
				player.statLife = player.statLifeMax2;
			if (InfiniteMana && player.statManaMax2 > 0)
				player.statMana = player.statManaMax2;
			if (InfiniteOxygen && player.breathMax > 0)
				player.breath = player.breathMax;
			if (InfiniteMinion)
			{
				player.maxMinions = 9999;
				player.maxTurrets = 9999;
			}
			if (InfiniteAmmo)
				TopOffAmmo(player);
			if (InfiniteFlyTime)
			{
				if (player.wingTimeMax > 0)
					player.wingTime = player.wingTimeMax;
				if (player.rocketTimeMax > 0)
					player.rocketTime = player.rocketTimeMax;
			}
			if (ImmuneToDebuffs)
				ClearDebuffs(player);
			if (SlowFall)
				player.slowFall = true;
			if (FastSpeed)
				player.moveSpeed = 10f;
			if (SuperGrabRange)
			{
				Player.defaultItemGrabRange = 1000;
				player.goldRing = true;
				player.manaMagnet = true;
				player.lifeMagnet = true;
				player.treasureMagnet = true;
			}
			if (CoinPortalDropsBags)
				ReplaceCoinPortalCoins();
			if (FishCratesOnly)
				ForceFishingCrates();
			if (BonusTwoSlots)
				player.extraAccessory = true;
			if (SuperRange)
			{
				Player.tileRangeX = 0x1000;
				Player.tileRangeY = 0x1000;
				player.lastTileRangeX = 0x1000;
				player.lastTileRangeY = 0x1000;
			}
			if (FastTileAndWallPlacingSpeed)
			{
				if (normalizePlacementSpeed)
				{
					player.wallSpeed = 1f / 3f;
					player.tileSpeed = 1f / 3f;
				}
				else
				{
					player.wallSpeed = 3f;
					player.tileSpeed = 3f;
				}
			}
			if (MechanicalRuler)
			{
				player.rulerGrid = true;
				player.rulerLine = true;
				SetBuilderAccVisible(player, 0);
				SetBuilderAccVisible(player, 1);
			}
			if (MechanicalLens)
			{
				player.InfoAccMechShowWires = true;
				SetBuilderAccVisible(player, 4);
				SetBuilderAccVisible(player, 5);
				SetBuilderAccVisible(player, 6);
				SetBuilderAccVisible(player, 7);
				SetBuilderAccVisible(player, 9);
				SetBuilderAccVisible(player, 8);
			}
			if (RightClickToTP)
				TryTeleportFromFullscreenMap(player);
			if (EnableAllRecipes)
				KeepAllRecipesAvailable();
			if (StrengthenVampireKnives)
				StrengthenOwnedVampireKnives();
			ApplyPlayerPropertyFinalOverrides(player);
		}

		public static void ApplyPlayerPropertyOverrides(Player player)
		{
			ApplyPlayerPropertyInfluenceOverrides(player);
			ApplyPlayerPropertyFinalOverrides(player);
		}

		public static unsafe void ApplyPlayerPropertyInfluenceOverrides(Player player)
		{
			PatchState.State* state = PatchState.Shared;

			if (PatchState.GetBool(state->Override_CoinLuck_Enabled))
				player.coinLuck = state->Override_CoinLuck_Value;
			if (PatchState.GetBool(state->Override_KiteLuckLevel_Enabled))
				player.kiteLuckLevel = ClampByte(state->Override_KiteLuckLevel_Value);
			if (PatchState.GetBool(state->Override_LadyBugLuckTimeLeft_Enabled))
				player.ladyBugLuckTimeLeft = state->Override_LadyBugLuckTimeLeft_Value;
			if (PatchState.GetBool(state->Override_BrokenMirrorBadLuckTime_Enabled))
				player.brokenMirrorBadLuckTime = state->Override_BrokenMirrorBadLuckTime_Value;
		}

		public static unsafe void ApplyPlayerPropertyFinalOverrides(Player player)
		{
			PatchState.State* state = PatchState.Shared;

			if (PatchState.GetBool(state->Override_StatDefense_Enabled))
				player.statDefense = state->Override_StatDefense_Value;
			if (PatchState.GetBool(state->Override_ArmorPenetration_Enabled))
				player.armorPenetration = state->Override_ArmorPenetration_Value;
			if (PatchState.GetBool(state->Override_MeleeCrit_Enabled))
				player.meleeCrit = state->Override_MeleeCrit_Value;
			if (PatchState.GetBool(state->Override_RangedCrit_Enabled))
				player.rangedCrit = state->Override_RangedCrit_Value;
			if (PatchState.GetBool(state->Override_MagicCrit_Enabled))
				player.magicCrit = state->Override_MagicCrit_Value;
			if (PatchState.GetBool(state->Override_MeleeDamage_Enabled))
				player.meleeDamage = state->Override_MeleeDamage_Value;
			if (PatchState.GetBool(state->Override_RangedDamage_Enabled))
				player.rangedDamage = state->Override_RangedDamage_Value;
			if (PatchState.GetBool(state->Override_MagicDamage_Enabled))
				player.magicDamage = state->Override_MagicDamage_Value;
			if (PatchState.GetBool(state->Override_MinionDamage_Enabled))
				player.minionDamage = state->Override_MinionDamage_Value;
			if (PatchState.GetBool(state->Override_RocketDamage_Enabled))
				player.rocketDamage = state->Override_RocketDamage_Value;
			if (PatchState.GetBool(state->Override_Endurance_Enabled))
				player.endurance = state->Override_Endurance_Value;
			if (PatchState.GetBool(state->Override_Thorns_Enabled))
				player.thorns = state->Override_Thorns_Value;

			if (PatchState.GetBool(state->Override_MoveSpeed_Enabled))
				player.moveSpeed = state->Override_MoveSpeed_Value;
			if (PatchState.GetBool(state->Override_MaxRunSpeed_Enabled))
				player.maxRunSpeed = state->Override_MaxRunSpeed_Value;
			if (PatchState.GetBool(state->Override_AccRunSpeed_Enabled))
				player.accRunSpeed = state->Override_AccRunSpeed_Value;
			if (PatchState.GetBool(state->Override_RunAcceleration_Enabled))
				player.runAcceleration = state->Override_RunAcceleration_Value;
			if (PatchState.GetBool(state->Override_JumpSpeedBoost_Enabled))
				player.jumpSpeedBoost = state->Override_JumpSpeedBoost_Value;
			if (PatchState.GetBool(state->Override_WingTime_Enabled))
				player.wingTime = state->Override_WingTime_Value;
			if (PatchState.GetBool(state->Override_WingTimeMax_Enabled))
				player.wingTimeMax = state->Override_WingTimeMax_Value;
			if (PatchState.GetBool(state->Override_RocketTime_Enabled))
				player.rocketTime = state->Override_RocketTime_Value;
			if (PatchState.GetBool(state->Override_RocketTimeMax_Enabled))
				player.rocketTimeMax = state->Override_RocketTimeMax_Value;
			if (PatchState.GetBool(state->Override_GravDir_Enabled))
				player.gravDir = state->Override_GravDir_Value;

			if (PatchState.GetBool(state->Override_MaxMinions_Enabled))
				player.maxMinions = state->Override_MaxMinions_Value;
			if (PatchState.GetBool(state->Override_MaxTurrets_Enabled))
				player.maxTurrets = state->Override_MaxTurrets_Value;
			if (PatchState.GetBool(state->Override_TileRangeX_Enabled))
			{
				Player.tileRangeX = state->Override_TileRangeX_Value;
				player.lastTileRangeX = state->Override_TileRangeX_Value;
			}
			if (PatchState.GetBool(state->Override_TileRangeY_Enabled))
			{
				Player.tileRangeY = state->Override_TileRangeY_Value;
				player.lastTileRangeY = state->Override_TileRangeY_Value;
			}
			if (PatchState.GetBool(state->Override_TileSpeed_Enabled))
				player.tileSpeed = state->Override_TileSpeed_Value;
			if (PatchState.GetBool(state->Override_WallSpeed_Enabled))
				player.wallSpeed = state->Override_WallSpeed_Value;
			if (PatchState.GetBool(state->Override_PickSpeed_Enabled))
				player.pickSpeed = state->Override_PickSpeed_Value;
			if (PatchState.GetBool(state->Override_BlockRange_Enabled))
				player.blockRange = state->Override_BlockRange_Value;
		}

		private static byte ClampByte(int value)
		{
			if (value < byte.MinValue)
				return byte.MinValue;
			if (value > byte.MaxValue)
				return byte.MaxValue;
			return (byte)value;
		}

		private static void SetBuilderAccVisible(Player player, int index)
		{
			if (player.builderAccStatus == null || index < 0 || index >= player.builderAccStatus.Length)
				return;
			player.builderAccStatus[index] = 0;
		}

		private static void ApplyCreativeMenu(Player player)
		{
			if (!CreativeMenu)
			{
				RestoreCreativeMenuDifficulty(player);
				return;
			}

			if (CreativeMenuOriginalDifficulty < 0)
				CreativeMenuOriginalDifficulty = player.difficulty;
			player.difficulty = 3;
		}

		private static void RestoreCreativeMenuDifficulty(Player player = null)
		{
			if (CreativeMenuOriginalDifficulty < 0)
				return;

			if (player == null)
				player = Main.LocalPlayer;
			if (player != null)
				player.difficulty = (byte)CreativeMenuOriginalDifficulty;
			CreativeMenuOriginalDifficulty = -1;
		}

		private static void ApplyHighLight()
		{
			if (!HighLight)
			{
				RestoreHighLight();
				return;
			}

			if (float.IsNaN(OriginalGlobalBrightness))
				OriginalGlobalBrightness = Lighting.GlobalBrightness;
			Lighting.GlobalBrightness = 100f;
		}

		private static void RestoreHighLight()
		{
			if (float.IsNaN(OriginalGlobalBrightness))
				return;
			Lighting.GlobalBrightness = OriginalGlobalBrightness;
			OriginalGlobalBrightness = float.NaN;
		}

		private static void TopOffAmmo(Player player)
		{
			if (player.inventory == null)
				return;

			int max = Math.Min(player.inventory.Length, 59);
			for (int i = 0; i < max; i++)
			{
				Item item = player.inventory[i];
				if (item != null && item.ammo > 0 && item.stack > 0)
					item.stack = Math.Max(item.maxStack, 999);
			}
		}

		private static unsafe void ReadSharedState()
		{
			PatchState.State* state = PatchState.Shared;
			InfiniteLife = PatchState.GetBool(state->InfiniteLife);
			InfiniteMana = PatchState.GetBool(state->InfiniteMana);
			InfiniteOxygen = PatchState.GetBool(state->InfiniteOxygen);
			InfiniteMinion = PatchState.GetBool(state->InfiniteMinion);
			InfiniteAmmo = PatchState.GetBool(state->InfiniteAmmo);
			InfiniteFlyTime = PatchState.GetBool(state->InfiniteFlyTime);
			CreativeMenu = PatchState.GetBool(state->CreativeMenu);
			ImmuneToDebuffs = PatchState.GetBool(state->ImmuneToDebuffs);
			SlowFall = PatchState.GetBool(state->SlowFall);
			FastSpeed = PatchState.GetBool(state->FastSpeed);
			SuperGrabRange = PatchState.GetBool(state->SuperGrabRange);
			CoinPortalDropsBags = PatchState.GetBool(state->CoinPortalDropsBags);
			FishCratesOnly = PatchState.GetBool(state->FishCratesOnly);
			BonusTwoSlots = PatchState.GetBool(state->BonusTwoSlots);
			HighLight = PatchState.GetBool(state->HighLight);
			SuperRange = PatchState.GetBool(state->SuperRange);
			FastTileAndWallPlacingSpeed = PatchState.GetBool(state->FastTileAndWallPlacingSpeed);
			MechanicalRuler = PatchState.GetBool(state->MechanicalRuler);
			MechanicalLens = PatchState.GetBool(state->MechanicalLens);
			RightClickToTP = PatchState.GetBool(state->RightClickToTP);
			EnableAllRecipes = PatchState.GetBool(state->EnableAllRecipes);
			StrengthenVampireKnives = PatchState.GetBool(state->StrengthenVampireKnives);
		}

		private static void TryTeleportFromFullscreenMap(Player player)
		{
			if (!Main.mapFullscreen || !Main.mouseRight || !Main.mouseRightRelease)
				return;

			Main.mapFullscreen = false;
			Main.mouseRightRelease = false;
			float targetX = ((Main.mouseX - Main.screenWidth / 2f) / Main.mapFullscreenScale + Main.mapFullscreenPos.X) * 16f;
			float targetY = ((Main.mouseY - Main.screenHeight / 2f) / Main.mapFullscreenScale + Main.mapFullscreenPos.Y) * 16f;
			player.position.X = targetX;
			player.position.Y = targetY;
		}

		private static void KeepAllRecipesAvailable()
		{
			if (Main.availableRecipe == null)
				return;

			int count = Math.Min(Recipe.maxRecipes, Main.availableRecipe.Length);
			Main.numAvailableRecipes = count;
			for (int i = 0; i < count; i++)
				Main.availableRecipe[i] = i;
		}

		private static void ForceFishingCrates()
		{
			if (Main.projectile == null)
				return;

			for (int i = 0; i < Main.projectile.Length; i++)
			{
				Projectile bobber = Main.projectile[i];
				if (bobber == null || !bobber.active || !bobber.bobber || bobber.owner != Main.myPlayer)
					continue;
				if (bobber.ai == null || bobber.localAI == null || bobber.ai.Length <= 1 || bobber.localAI.Length <= 1)
					continue;
				if (bobber.ai[1] >= 0f)
					continue;

				int itemType = (int)bobber.localAI[1];
				if (itemType <= 0 || IsFishingCrate(itemType))
					continue;

				bobber.localAI[1] = FishCratesOnlyHook.SelectFallbackCrate(bobber);
				bobber.netUpdate = true;
			}
		}

		private static bool IsFishingCrate(int itemType)
		{
			return itemType > 0
				&& ItemID.Sets.IsFishingCrate != null
				&& itemType < ItemID.Sets.IsFishingCrate.Length
				&& ItemID.Sets.IsFishingCrate[itemType];
		}


		private static void ReplaceCoinPortalCoins()
		{
			if (Main.netMode == 1 || Main.projectile == null || Main.item == null)
				return;

			for (int i = 0; i < Main.item.Length; i++)
			{
				WorldItem item = Main.item[i];
				if (item == null || !item.active || !IsCoin(item.type))
					continue;
				if (item.timeSinceItemSpawned > 10)
					continue;
				if (!IsNearOwnedCoinPortal(item.Center))
					continue;

				item.SetDefaults(ItemID.MoonLordBossBag);
				item.stack = 1;
				item.velocity = Vector2.UnitY.RotatedByRandom(Math.PI * 2) * new Vector2(3f, 2f)
					* (Main.rand.NextFloat() * 0.5f + 0.5f) - Vector2.UnitY;
				if (Main.netMode == 2)
					NetMessage.SendData(21, -1, -1, null, i);
			}
		}

		private static bool IsCoin(int itemType)
		{
			return itemType == ItemID.CopperCoin
				|| itemType == ItemID.SilverCoin
				|| itemType == ItemID.GoldCoin
				|| itemType == ItemID.PlatinumCoin;
		}

		private static bool IsNearOwnedCoinPortal(Vector2 position)
		{
			for (int i = 0; i < Main.projectile.Length; i++)
			{
				Projectile projectile = Main.projectile[i];
				if (projectile == null || !projectile.active || projectile.type != ProjectileID.CoinPortal)
					continue;
				if (projectile.owner != Main.myPlayer)
					continue;
				if (Vector2.DistanceSquared(projectile.Center, position) <= 1600f)
					return true;
			}
			return false;
		}

		private static void StrengthenOwnedVampireKnives()
		{
			if (Main.netMode == 2 || Main.projectile == null)
				return;

			EnsureProcessedVampireKnives();
			for (int i = 0; i < Main.projectile.Length; i++)
			{
				Projectile projectile = Main.projectile[i];
				if (projectile == null || !projectile.active || projectile.type != ProjectileID.VampireKnife)
				{
					ProcessedVampireKnives[i] = false;
					continue;
				}
				if (projectile.owner != Main.myPlayer)
				{
					ProcessedVampireKnives[i] = false;
					continue;
				}
				if (ProcessedVampireKnives[i])
					continue;

				ProcessedVampireKnives[i] = true;
				SpawnVampireKnifeRing(projectile);
			}
		}

		private static void EnsureProcessedVampireKnives()
		{
			if (ProcessedVampireKnives.Length == Main.projectile.Length)
				return;
			ProcessedVampireKnives = new bool[Main.projectile.Length];
		}

		private static void SpawnVampireKnifeRing(Projectile source)
		{
			Vector2 baseVelocity = source.velocity;
			if (baseVelocity.LengthSquared() < 0.01f)
				baseVelocity = Vector2.UnitX * 12f;
			float speed = Math.Max(baseVelocity.Length(), 10f);
			int damage = Math.Max(source.damage, 1);
			float knockBack = source.knockBack;
			for (int i = 0; i < 12; i++)
			{
				Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 12f) * speed;
				int index = Projectile.NewProjectile(
					source.GetProjectileSource_FromThis(),
					source.Center,
					velocity,
					ProjectileID.VampireKnife,
					damage,
					knockBack,
					source.owner);
				if (index >= 0 && index < Main.projectile.Length)
				{
					Projectile clone = Main.projectile[index];
					if (index < ProcessedVampireKnives.Length)
						ProcessedVampireKnives[index] = true;
					clone.netUpdate = true;
				}
			}
		}

		private static void ClearDebuffs(Player player)
		{
			if (player.buffType == null || player.buffTime == null)
				return;

			int count = Math.Min(Player.maxBuffs, Math.Min(player.buffType.Length, player.buffTime.Length));
			for (int i = 0; i < count; i++)
			{
				int type = player.buffType[i];
				if (type > 0 && type < Main.debuff.Length && Main.debuff[type])
				{
					player.buffType[i] = 0;
					player.buffTime[i] = 0;
				}
			}
		}
	}
}
