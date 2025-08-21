using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace CoffeeMod
{
    // Runs when the player finishes drinking a bowl of coffee
    public class ItemBrewCoffee : ItemMeal
    {
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);
            if (byEntity.World.Side != EnumAppSide.Server) return;
            if (byEntity.Controls.HandUse != EnumHandInteract.Eat) return;
            if (secondsUsed < 0.95f) return;

            var eplr = byEntity as EntityPlayer;
            if (eplr == null) return;

            // Warmth
            float instantDegrees = Attributes?["coffeeInstantDegrees"].AsFloat(2.5f) ?? 2.5f;
            float durationSec    = Attributes?["coffeeDurationSec"].AsFloat(480f) ?? 480f;
            float boostPerSec    = Attributes?["coffeeBoostPerSec"].AsFloat(0.03f) ?? 0.03f;

            var tempBh = eplr.GetBehavior<EntityBehaviorBodyTemperature>();
            if (tempBh != null)
            {
                tempBh.CurBodyTemperature = GameMath.Clamp(tempBh.CurBodyTemperature + instantDegrees, -20, 40);
            }

            double now = byEntity.World.Calendar.TotalSeconds;
            var wattr = eplr.WatchedAttributes;

            double existingUntil = wattr.GetDouble("coffeeWarmthUntil", 0);
            double newUntil = System.Math.Max(existingUntil, now + durationSec);
            wattr.SetDouble("coffeeWarmthUntil", newUntil);
            wattr.SetFloat ("coffeeBoostPerSec", boostPerSec);

            // Hunger slowdown
            float hungerMul    = Attributes?["coffeeHungerMul"].AsFloat(0.9f) ?? 0.9f;   // 10% slower
            float hours        = Attributes?["coffeeHungerHours"].AsFloat(3f) ?? 3f;     // 3 hours
            float baseSatPerHr = Attributes?["coffeeHungerSatPerHourBase"].AsFloat(60f) ?? 60f;

            wattr.SetDouble("coffeeHungerUntil", now + hours * 3600f);
            wattr.SetFloat ("coffeeHungerMul", hungerMul);
            wattr.SetFloat ("coffeeHungerBaseSatPerHr", baseSatPerHr);
            eplr.WatchedAttributes.MarkDirty();

            // Feedback message while testing
            (byEntity.World as IServerWorldAccessor)?.BroadcastMessageToPlayer(
                eplr.Player, $"[Coffee] Warmth +{instantDegrees:0.0}°, hunger drain {((1f-hungerMul)*100):0}% slower for {hours:0.#}h."
            );
        }
    }
}
