# vintage-coffee
Coffee Mod ☕ for Vintage Story

by KritterBizkit

Brew real coffee in Vintage Story: find wild shrubs, prune for beans, roast, grind, and brew a hot drink that actually warms you up.

Features

Wild Coffee Shrubs

Spawn in temperate, rainy biomes (new chunks).

Three growth states: empty → flowering → ripe.

Prune ripe shrubs to collect beans; shrubs regrow over time.

Bean Lifecycle

Green Beans → roast → Roasted Beans → grind → Coffee Grounds.

Brewing (Cookpot)

Put 1× Bowl of Water (waterportion) and 1× Coffee Grounds into the pot.

Heat until cooked, then serve into bowls.

Served hot (shows as a liquid in the bowl).

Warmth Effect

Drinking hot coffee gives an instant warmth bump and a lingering slow-cool effect (via the small CoffeeEffects code mod). (COMING SOON) Only vanilla warmth right now.


HOW TO USE
Find shrubs

Temperate, rainy areas; under trees and on the surface.

Spawn only in newly generated chunks (explore new areas or start a fresh world).

Get beans

Prune a ripe shrub to get Green Coffee Beans.

Roast & Grind

Roast: 1× green bean → 1× roasted bean (grid, 1×1).

Grind: 3× roasted beans → 1× coffee grounds (3×3 shaped).

Brew

Put 1× Bowl of Water and 1× Coffee Grounds in a Cookpot.

Heat until done; serve into bowls. Drink while hot for best warmth.

WORLDGEN DETAILS

File: coffee/assets/coffee/worldgen/blockpatches/coffeebushes.json

Default band (edit to taste):

"minTemp": 8, "maxTemp": 30,
"minRain": 0.35, "maxRain": 1.0,
"minForest": 0.2, "maxForest": 0.9,
"minY": 0.0, "maxY": 0.75,
"chance": 0.006,
"quantity": { "avg": 5, "var": 2 }


Existing worlds: shrubs only appear in new chunks. Travel to unexplored areas or plant some manually:

/giveblock coffee:coffeebush-coffee-ripe 5
