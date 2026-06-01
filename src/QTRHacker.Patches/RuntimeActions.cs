using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.Map;

namespace QTRHacker.Patches
{
	public static class RuntimeActions
	{
		public static bool UnlockAllDuplicationsRequested;
		public static bool RevealTheWholeMapRequested;
		public static int UnlockAllDuplicationsCompleted;
		public static int RevealTheWholeMapCompleted;
		public static int LastErrorCode;

		public static void ApplyQueuedActions()
		{
			if (UnlockAllDuplicationsRequested)
			{
				UnlockAllDuplicationsRequested = false;
				Run(UnlockAllDuplications, ref UnlockAllDuplicationsCompleted);
			}
			if (RevealTheWholeMapRequested)
			{
				RevealTheWholeMapRequested = false;
				Run(RevealTheWholeMap, ref RevealTheWholeMapCompleted);
			}
		}

		private static void UnlockAllDuplications()
		{
			if (Main.gameMenu || Main.LocalPlayer?.creativeTracker?.ItemSacrifices == null)
				return;

			var sacrifices = Main.LocalPlayer.creativeTracker.ItemSacrifices;
			for (int itemId = 0; itemId < ItemID.Count; itemId++)
				sacrifices.RegisterItemSacrifice(itemId, 9999);
		}

		private static void RevealTheWholeMap()
		{
			if (Main.gameMenu || Main.Map == null)
				return;

			int margin = WorldMap.BlackEdgeWidth;
			int maxX = Math.Max(margin, Main.maxTilesX - margin);
			int maxY = Math.Max(margin, Main.maxTilesY - margin);
			for (int x = margin; x < maxX; x++)
			{
				for (int y = margin; y < maxY; y++)
					Main.Map.UpdateLighting(x, y, byte.MaxValue);
			}
			Main.refreshMap = true;
		}

		private static void Run(Action action, ref int completed)
		{
			try
			{
				action();
				completed++;
				LastErrorCode = 0;
			}
			catch
			{
				LastErrorCode = 1;
			}
		}
	}
}
