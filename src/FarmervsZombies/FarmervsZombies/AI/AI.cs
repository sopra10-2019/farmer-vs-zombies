using System;
using System.Collections.Generic;
using System.Linq;
using FarmervsZombies.GameObjects;
using FarmervsZombies.Managers;
using FarmervsZombies.Pathfinding;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.AI
{
    internal static class Ai
    {
        private static Necromancer sNecromancer;
        private static readonly List<List<Zombie>> sWaves = new List<List<Zombie>>();
        private static Vector2 sAiBasePosition;
        private static readonly Random sRandom = new Random();
        private static double sResources = 100000;
        private static int sAvailableSpawns;
        private const double ZombieChance = 0.02;
        private const double SpawnChance = 0;
        private const double SpawnPrice = 10000;
        private const double AbilityCostPerWheat = 20000;
        private const double SpoilChance = 0.4;
        private const float AbilityCooldown = 500f;
        private static float sTimeSinceAbility;
        private const float WaveCooldown = 60f;
        private static float sTimeSinceWave = 55f;
        private static bool sGathering;

        public static void Reset()
        {
            // TODO: "Reset" game time
            sNecromancer = null;
            sWaves.Clear();
            sResources = 100000;
            sAvailableSpawns = 0;
            sTimeSinceAbility = 0;
            sTimeSinceWave = Game1.Difficulty * 10;
        }

        public static void BuildGraveyard(Vector2 position)
        {
            ObjectManager.Instance.Add(new Graveyard((int)position.X, (int)position.Y));
            sAiBasePosition = position;

            for (var i = 0; i < Game1.Difficulty; i++)
            {
                ObjectManager.Instance.Add(new Zombie(sAiBasePosition.X - 0, sAiBasePosition.Y - 1 - i, true, 3f));
                ObjectManager.Instance.Add(new Zombie(sAiBasePosition.X - 1 - i, sAiBasePosition.Y + 0, true, 3f));
                ObjectManager.Instance.Add(new Zombie(sAiBasePosition.X + 1 + i, sAiBasePosition.Y - 0, true, 3f));
                ObjectManager.Instance.Add(new Zombie(sAiBasePosition.X + 0, sAiBasePosition.Y + 1 + i, true, 3f));
            }


            // Decoration
            ObjectManager.Instance.Add(new Graveyardasset(sAiBasePosition.X - 5, sAiBasePosition.Y - 5));
            ObjectManager.Instance.Add(new Graveyardasset(sAiBasePosition.X + 5, sAiBasePosition.Y - 5));
            ObjectManager.Instance.Add(new Graveyardasset(sAiBasePosition.X - 5, sAiBasePosition.Y + 5));
            ObjectManager.Instance.Add(new Graveyardasset(sAiBasePosition.X + 5, sAiBasePosition.Y + 5));
                                                                                                                      
            ObjectManager.Instance.Add(new GraveyardCross(sAiBasePosition.X - 2, sAiBasePosition.Y + 3));
            ObjectManager.Instance.Add(new GraveyardCross(sAiBasePosition.X - 1, sAiBasePosition.Y + 6));
            ObjectManager.Instance.Add(new GraveyardCross(sAiBasePosition.X + 0, sAiBasePosition.Y + 3));
            ObjectManager.Instance.Add(new GraveyardCross(sAiBasePosition.X + 1, sAiBasePosition.Y + 6));
            ObjectManager.Instance.Add(new GraveyardCross(sAiBasePosition.X + 2, sAiBasePosition.Y + 3));
                                                                                                                      
            ObjectManager.Instance.Add(new GraveyardCross(sAiBasePosition.X - 2, sAiBasePosition.Y - 3));
            ObjectManager.Instance.Add(new GraveyardCross(sAiBasePosition.X - 1, sAiBasePosition.Y - 6));
            ObjectManager.Instance.Add(new GraveyardCross(sAiBasePosition.X + 0, sAiBasePosition.Y - 3));
            ObjectManager.Instance.Add(new GraveyardCross(sAiBasePosition.X + 1, sAiBasePosition.Y - 6));
            ObjectManager.Instance.Add(new GraveyardCross(sAiBasePosition.X + 2, sAiBasePosition.Y - 3));

            // More Decoration
            for (var i = -9; i < 10; i++)
            {
                for (var j = -9; j < 10; j++)
                {
                    if (i*i * j * j < 666 && Game1.sTileMap.GetTileType((int)position.X + i, (int)position.Y + j) != Tile.Water)
                    {
                        Game1.sTileMap.PlantWasteland((int)position.X + i , (int)position.Y + j);
                    }
                }
            }
        }

        public static void AddWave(List<Zombie> wave)
        {
            sWaves.Add(wave);
        }

        private static void SpawnZombie()
        {
            if (sRandom.NextDouble() >  SpawnChance || sAvailableSpawns <= 0) return;

            var zombie = new Zombie(sAiBasePosition.X + sRandom.Next(1, 5),
                sAiBasePosition.Y + sRandom.Next(1, 5),
                true,
                10) { Aggressive = true };
            ObjectManager.Instance.Add(zombie);
            sAvailableSpawns--;
        }

        private static void SpawnWave(int size, double passedTime)
        {
            if (sAvailableSpawns < size) return;

            var wave = new List<Zombie>();
            var squareSize = (int)Math.Ceiling(Math.Sqrt(size));
            for (var i = 0; i < size; i++)
            {
                var zombie = new Zombie(sAiBasePosition.X + (i % squareSize), (int)sAiBasePosition.Y + i / squareSize);
                wave.Add(zombie);
                ObjectManager.Instance.Add(zombie);
            }

            SetWaveTactic(wave, passedTime);

            sWaves.Add(wave);
            sAvailableSpawns-= size;
            sTimeSinceWave = 0;
            sGathering = false;
        }

        public static void SpawnNecromancer(Vector2 position)
        {
            sNecromancer = new Necromancer(position.X, position.Y);
            ObjectManager.Instance.Add(sNecromancer);
            LastStand();
        }

        private static void LastStand()
        {
            var waveSize = (int) (sResources / SpawnPrice) + sAvailableSpawns;
            sAvailableSpawns = 0;
            var finalWave = new List<Zombie>();
            var spawnBox = (int)Math.Sqrt(waveSize);

            for (var i = 0; i < waveSize; i++)
            {
                var position = new Vector2(sRandom.Next((int)sAiBasePosition.X - spawnBox / 2, (int)sAiBasePosition.X + spawnBox / 2), 
                    sRandom.Next((int)sAiBasePosition.Y - spawnBox / 2, (int)sAiBasePosition.Y + spawnBox / 2));
                while (Game1.sTileMap.GetTileType((int) position.X, (int) position.Y) == Tile.Water ||
                       ObjectManager.Instance.CheckTile(position, typeof(IPathCollidable)).Any(obj => !(obj is Graveyard)))
                {
                    position = new Vector2(sRandom.Next((int)sAiBasePosition.X - spawnBox / 2, (int)sAiBasePosition.X + spawnBox / 2),
                        sRandom.Next((int)sAiBasePosition.Y - spawnBox / 2, (int)sAiBasePosition.Y + spawnBox / 2));
                }

                var zombie = new Zombie(position.X, position.Y);
                finalWave.Add(zombie);
                ObjectManager.Instance.Add(zombie);
                zombie.Charge(ObjectManager.Instance.GetFarmer().Position);
                zombie.PreferredTargetType = typeof(Farmer);
            }

            sWaves.Add(finalWave);
        }

        private static void SetWaveTactic(IReadOnlyCollection<Zombie> wave, double passedTime)
        {
            IAttackable target = null;
            var position = wave.First().Position;

            // Avoid multiple zombies attacking the same target and blocking themselves
            var targets = new IAttackable[(int)Math.Ceiling((float)wave.Count / 2)];
            var flockTargets = false;
            

            // Choose tactic: attack economy vs. trying to finish
            if (sRandom.NextDouble() < passedTime / (passedTime + 5) * Game1.Difficulty || !ObjectManager.Instance.GetAnimals().Any())
            {
                UnitPath farmerPath = null;
                UnitPath farmhousePath = null;

                if (ObjectManager.Instance.GetFarmer() != null &&
                    Pathfinder.ExistsPath(position, ObjectManager.Instance.GetFarmer().Position))
                    farmerPath = Pathfinder.GetPath(position, ObjectManager.Instance.GetFarmer().Position);
                if (ObjectManager.Instance.GetFarmhouse() != null &&
                    Pathfinder.ExistsPath(position, ObjectManager.Instance.GetFarmhouse().Position))
                    farmhousePath = Pathfinder.GetPath(position, ObjectManager.Instance.GetFarmhouse().Position);

                if (farmerPath != null && (farmhousePath == null || farmerPath.Length <= farmhousePath.Length))
                {
                    target = ObjectManager.Instance.GetFarmer();
                }
                else if (farmhousePath != null)
                {
                    target = ObjectManager.Instance.GetFarmhouse();
                }
                else
                {
                    foreach (var tower in ObjectManager.Instance.GetTowers())
                    {
                        if (target == null || Vector2.Distance(position, tower.Position) <
                            Vector2.Distance(position, target.Position))
                            target = tower;
                    }

                    if (target == null || !Pathfinder.ExistsPath(position, target.Position))
                    {
                        flockTargets = true;

                        var fences = ObjectManager.Instance.GetFences().ToArray();
                        var maxIndex = 0;
                        for (var i = 0; i < targets.Length && i < fences.Length; i++)
                        {
                            targets[i] = fences[i];
                            if (Vector2.Distance(position, fences[i].Position) <
                                Vector2.Distance(position, targets[maxIndex].Position))
                            {
                                maxIndex = i;
                            }
                        }

                        for (var i = targets.Length; i < fences.Length; i++)
                        {
                            if (!(Vector2.Distance(position, fences[i].Position) <
                                  Vector2.Distance(position, targets[maxIndex].Position))) continue;

                            targets[maxIndex] = fences[i];
                            for (var j = 0; j < targets.Length; j++)
                            {
                                if (Vector2.Distance(position, targets[j].Position) >
                                    Vector2.Distance(position, targets[maxIndex].Position))
                                {
                                    maxIndex = j;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                flockTargets = true;

                var animals = ObjectManager.Instance.GetAnimals().ToArray();
                var maxIndex = 0;
                for (var i = 0; i < targets.Length && i < animals.Length; i++)
                {
                    targets[i] = animals[i];
                    if (Vector2.Distance(position, animals[i].Position) <
                        Vector2.Distance(position, targets[maxIndex].Position))
                    {
                        maxIndex = i;
                    }
                }

                for (var i = targets.Length; i < animals.Length; i++)
                {
                    if (!(Vector2.Distance(position, animals[i].Position) <
                          Vector2.Distance(position, targets[maxIndex].Position))) continue;

                    targets[maxIndex] = animals[i];
                    for (var j = 0; j < targets.Length; j++)
                    {
                        if (Vector2.Distance(position, targets[j].Position) >
                            Vector2.Distance(position, targets[maxIndex].Position))
                        {
                            maxIndex = j;
                        }
                    }
                }
            }

            if (flockTargets)
            {
                var flockedTargets = targets.Where(t => t != null).ToArray();
                for (var i = 0; i < wave.Count; i++)
                {
                    wave.ElementAt(i).Target = flockedTargets[i % flockedTargets.Length];
                }
            }
            else
            {
                foreach (var zombie in wave)
                {
                    zombie.Target = target;

                    if (!zombie.PathEmpty) continue;
                    var closestPosition = target.Position;
                    while (!Pathfinder.ExistsPath(position, closestPosition))
                        closestPosition = 0.9f * (closestPosition - zombie.Position) + zombie.Position;
                    zombie.Charge(closestPosition);
                    zombie.PreferredTargetType = target.GetType();
                }
            }
        }

        private static bool IsWaveBusy(IReadOnlyCollection<Zombie> wave)
        {
            return wave.Sum(zombie => zombie.HasPath || zombie.HasTarget ? 1 : 0) > wave.Count / 2;
        }

        public static void RemoveZombie(Zombie zombie)
        {
            foreach (var wave in sWaves)
                wave.Remove(zombie);
        }

        private static void SpoilLand()
        {
            var wheat = ObjectManager.Instance.GetWheat().ToList();
            if (wheat.Count < 5 || sResources < AbilityCostPerWheat * SpoilChance * wheat.Count) return;

            foreach (var w in wheat)
            {
                if (sRandom.NextDouble() >= SpoilChance) continue;

                Game1.sTileMap.PlantWasteland((int)w.Position.X, (int)w.Position.Y);
                for (var i = (int) w.Position.X - 1; i < (int) w.Position.X + 2; i++)
                {
                    for (var j = (int) w.Position.Y - 1; j < (int) w.Position.Y + 2; j++)
                    {
                        if (sRandom.NextDouble() < SpoilChance / 4)
                            Game1.sTileMap.PlantWasteland(i, j);
                    }
                }
            }

            sResources -= AbilityCostPerWheat * SpoilChance * wheat.Count;
            sTimeSinceAbility = 0;
        }

        public static void Update(GameTime gameTime)
        {
            sResources += gameTime.ElapsedGameTime.TotalMilliseconds * Math.Sqrt(gameTime.TotalGameTime.TotalMinutes) * Game1.Difficulty;

            if (sRandom.NextDouble() < ZombieChance && sResources >= SpawnPrice)
            {
                sAvailableSpawns++;
                sResources -= SpawnPrice;
            }

            if (sTimeSinceAbility >= AbilityCooldown)
            {
                SpoilLand();
            }

            var deadWaves = new List<List<Zombie>>();
            foreach (var wave in sWaves)
            {
                if (!wave.Any())
                {
                    deadWaves.Add(wave);
                    continue;
                }

                if (!IsWaveBusy(wave))
                {
                    SetWaveTactic(wave, gameTime.TotalGameTime.TotalMinutes);
                }
            }

            foreach (var wave in deadWaves)
            {
                sWaves.Remove(wave);
            }

            if (!sGathering)
            {
                if (sTimeSinceWave >= WaveCooldown / (Game1.Difficulty == 1 ? 1 : Game1.Difficulty - 1))
                    sGathering = true;
                else
                    SpawnZombie();
            }
            else if (10 + 2 * gameTime.TotalGameTime.TotalMinutes <= sAvailableSpawns)
            {
                SpawnWave(10 + 2 * (int)gameTime.TotalGameTime.TotalMinutes, gameTime.TotalGameTime.TotalMinutes);
            }

            sTimeSinceWave += gameTime.ElapsedGameTime.Milliseconds / 1000f;
            sTimeSinceAbility += gameTime.ElapsedGameTime.Milliseconds / 1000f;
        }
    }
}
