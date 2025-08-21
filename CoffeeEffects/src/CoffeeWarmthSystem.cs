using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace CoffeeMod
{
    public class CoffeeWarmthSystem : ModSystem
    {
        ICoreServerAPI sapi;

        public override void Start(ICoreAPI api)
        {
            // Expose our custom coffee item class
            api.RegisterItemClass("ItemBrewCoffee", typeof(ItemBrewCoffee));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
            // Tick 5x/sec
            sapi.World.RegisterGameTickListener(OnServerTick, 200);
        }

        void OnServerTick(float dt)
        {
            double now = sapi.World.Calendar.TotalSeconds;

            foreach (var plr in sapi.World.AllOnlinePlayers)
            {
                var eplr = plr.Entity;
                var attrs = eplr.WatchedAttributes;

                // --- Warmth trickle while active ---
                double warmUntil = attrs.GetDouble("coffeeWarmthUntil", 0);
                if (warmUntil > now)
                {
                    float boostPerSec = attrs.GetFloat("coffeeBoostPerSec", 0f);
                    if (boostPerSec > 0f)
                    {
                        var tempBh = eplr.GetBehavior<EntityBehaviorBodyTemperature>();
                        if (tempBh != null)
                        {
                            tempBh.CurBodyTemperature = GameMath.Clamp(
                                tempBh.CurBodyTemperature + boostPerSec * dt, -20, 40
                            );
                        }
                    }
                }

                // --- Hunger slowdown: add back ~ (1-mul) of baseline loss ---
                double hungUntil = attrs.GetDouble("coffeeHungerUntil", 0);
                if (hungUntil > now)
                {
                    float mul = GameMath.Clamp(attrs.GetFloat("coffeeHungerMul", 0.9f), 0.1f, 2f);
                    float basePerHr = attrs.GetFloat("coffeeHungerBaseSatPerHr", 60f);

                    float restorePerSec = (1f - mul) * basePerHr / 3600f;
                    if (restorePerSec > 0f)
                    {
                        var hunger = eplr.GetBehavior<EntityBehaviorHunger>();
                        if (hunger != null)
                        {
                            // Call hunger.Satiate(...) via reflection (API signatures can vary)
                            var m = hunger.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                           .FirstOrDefault(x => x.Name == "Satiate");
                            if (m != null)
                            {
                                try { m.Invoke(hunger, new object[] { restorePerSec * dt, EnumFoodCategory.Unknown, 1f }); }
                                catch { try { m.Invoke(hunger, new object[] { restorePerSec * dt }); } catch { /* ignore */ } }
                            }
                        }
                    }
                }
            }
        }
    }
}
