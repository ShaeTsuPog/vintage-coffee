using System;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace CoffeeMod
{
    public class CoffeeWarmthSystem : ModSystem
    {
        ICoreServerAPI sapi;

        public override void Start(ICoreAPI api)
        {
            api.RegisterCollectibleBehaviorClass("CoffeeBuff", typeof(CollectibleBehaviorCoffeeBuff));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
            sapi.World.RegisterGameTickListener(OnServerTick, 200); // 5x/sec
        }

        /// Helper: e.GetBehavior<T>() using reflection, trying both assembly names
        static object GetBehaviorReflect(EntityAgent e, string typeA, string typeB)
        {
            var behType = Type.GetType(typeA) ?? Type.GetType(typeB);
            if (behType == null) return null;

            var m = typeof(EntityAgent).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                       .FirstOrDefault(x => x.Name == "GetBehavior" && x.IsGenericMethodDefinition && x.GetParameters().Length == 0);
            if (m == null) return null;

            var gm = m.MakeGenericMethod(behType);
            return gm.Invoke(e, null);
        }

        void OnServerTick(float dt)
        {
            double now = sapi.World.Calendar.TotalHours * 3600.0;  // older API: use hours

            foreach (var plr in sapi.World.AllOnlinePlayers)
            {
                var eplr = plr.Entity;
                var attrs = eplr.WatchedAttributes;

                // ---- Warmth trickle while active ----
                double warmUntil = attrs.GetDouble("coffeeWarmthUntil", 0);
                if (warmUntil > now)
                {
                    float boostPerSec = attrs.GetFloat("coffeeBoostPerSec", 0f);
                    if (boostPerSec > 0f)
                    {
                        var tempBeh = GetBehaviorReflect(
                            eplr,
                            "Vintagestory.GameContent.EntityBehaviorBodyTemperature, VSSurvivalMod",
                            "Vintagestory.GameContent.EntityBehaviorBodyTemperature, Vintagestory.GameContent"
                        );
                        if (tempBeh != null)
                        {
                            var prop = tempBeh.GetType().GetProperty("CurBodyTemperature");
                            if (prop != null)
                            {
                                float cur = (float)(prop.GetValue(tempBeh) ?? 0f);
                                float next = GameMath.Clamp(cur + boostPerSec * dt, -20, 40);
                                prop.SetValue(tempBeh, next);
                            }
                        }
                    }
                }

                // ---- Hunger slowdown: add back (1-mul)*baseline each second ----
                double hungUntil = attrs.GetDouble("coffeeHungerUntil", 0);
                if (hungUntil > now)
                {
                    float mul = GameMath.Clamp(attrs.GetFloat("coffeeHungerMul", 0.9f), 0.1f, 2f);
                    float basePerHr = attrs.GetFloat("coffeeHungerBaseSatPerHr", 60f);
                    float restorePerSec = (1f - mul) * basePerHr / 3600f;

                    if (restorePerSec > 0f)
                    {
                        var hungerBeh = GetBehaviorReflect(
                            eplr,
                            "Vintagestory.GameContent.EntityBehaviorHunger, VSSurvivalMod",
                            "Vintagestory.GameContent.EntityBehaviorHunger, Vintagestory.GameContent"
                        );
                        if (hungerBeh != null)
                        {
                            // Satiate(float) OR Satiate(float, EnumFoodCategory, float)
                            var m1 = hungerBeh.GetType().GetMethod("Satiate",
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                null, new Type[] { typeof(float) }, null);

                            if (m1 != null)
                            {
                                m1.Invoke(hungerBeh, new object[] { restorePerSec * dt });
                            }
                            else
                            {
                                var m2 = hungerBeh.GetType().GetMethod("Satiate",
                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                if (m2 != null && m2.GetParameters().Length == 3)
                                {
                                    // enum value 0 is fine here
                                    m2.Invoke(hungerBeh, new object[] { restorePerSec * dt, 0, 1f });
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
