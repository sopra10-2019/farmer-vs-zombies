
# :man_farmer: Farmer vs Zombies

This project was created at the [University of Freiburg](http://www.uni-freiburg.de/) during a lecture of the [Chair of Software Engineering](http://swt.informatik.uni-freiburg.de/). The game is about a farmer surviving in a zombie apocalypse.

![screenshot](https://user-images.githubusercontent.com/45404400/65511772-35e4cc80-ded8-11e9-8cfb-1414a41dd80d.png)  

### [Game Design Document (german)](https://github.com/sopra10-2019/farmer-vs-zombies/blob/master/farmer-vs-zombies_GDD.pdf)  

## Download ![version](https://img.shields.io/badge/version-1.0-blue)
[Latest Release](https://github.com/sopra10-2019/farmer-vs-zombies/releases/latest)

## Key assignments

### Normal key assignments:

| Key               | Action        |
| -------------     | ------------- |
| Right-click | Indirect control for selected unit |
| Right-click (on enemy unit) | Selected units attacks clicked opponent |
| Left-click | Menu control (harvest, sow, buy, activate skills, upgrade, ...) |
| LShift + left-click | Indirect control for selected unit |
| Space | Follow farmer/Don't follow farmer anymore |
| Arrow keys | Move camera |
| F5 | Window mode On/Off |
| X | Stop selected units |

### Key assignments for cheat mode (can be activated in the options menu):

| Key               | Action        |
| -------------     | ------------- |
| C | Spawn chicken |
| V | Spawn cow |
| B | Spawn pig |
| N | Spawn fighting chicken |
| M | Spawn fighting cow |
| J | Spawn fighting pig |
| Z | Spawn the necromancer |
| W, A, S, D | Direct farmer control |
| F3 | FPS counter on/off |
| F | FogOfWar On/Off |
| T | QuadTree visualization On/Off |

## Cheat mode:
  The cheat mode can be activated in the options menu. If the cheat mode is enabled, there are additional options in the ingame menus:  
- Gold cheat button in farmhouse menu: 1000 Gold  
- Buttons for map editing
    
## Actions of the AI:
- Create a wave of zombies, if there are enough resources available
- Plan a strategy based on game state (attack economy or kill farmer)
- Summon the Necromancer when the graveyard is attacked or the gravestone is destroyed
- Summon a last contingent of zombies when the graveyard is attacked or the gravestone is destroyed
- If a lot of grain has been planted, cause a disease that has a small chance of destroying the farmer's crop and spoiling the soil
