using QHackLib;
using QHackLib.Memory;
using QTRHacker.Core.GameObjects.ValueTypeRedefs.Xna;

namespace QTRHacker.Core.GameObjects.Terraria;

/// <summary>
/// Wrapper for Terraria.Item
/// </summary>
public partial class Item : Entity
{
	public Item(GameContext ctx, HackObject obj) : base(ctx, obj)
	{
	}

	public void SetDefaults(int type)
	{
		Context.RunByHookUpdate(TypedInternalObject.GetMethodCall("Terraria.Item.SetDefaults(Int32, Terraria.GameContent.Items.ItemVariant)")
			.Call(true, null, null, new object[] { type, (nuint)0 }));
	}

	public void SetPrefix(int prefix)
	{
		if (prefix == 0)
		{
			ResetPrefixSafely();
			return;
		}

		if (!HasValidTypeForGamePrefixLookup())
		{
			Prefix = 0;
			return;
		}

		if (!CanApplyPrefix(prefix))
		{
			ResetPrefixByGame();
			return;
		}

		ApplyPrefixByGame(prefix);
	}

	public void SetDefaultsAndPrefix(int type, int prefix)
	{
		SetDefaults(type);
		SetPrefix(prefix);
	}


	public static int NewItem(GameContext Context, int X, int Y, int Width, int Height, int Type, int Stack = 1,
		bool noBroadcast = false, int pfix = 0, bool noGrabDelay = false)
	{
		using MemoryAllocation ret = new(Context.HContext);

		Context.RunByHookUpdate(
			new HackMethod(Context.HContext,
				Context.GameModuleHelper.GetClrMethodBySignature("Terraria.Item",
				"Terraria.Item.NewItem(Terraria.DataStructures.IEntitySource, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, Int32, Int32, Boolean, Int32, Boolean)"))
			.Call(null)
			.Call(true, null, ret.AllocationBase, new object[] { 0, new Vector2(X, Y), new Vector2(Width, Height), Type, Stack, noBroadcast, pfix, noGrabDelay }));

		return Context.HContext.DataAccess.Read<int>(ret.AllocationBase);
	}
}
