using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using FarmervsZombies.AI;
using FarmervsZombies.GameObjects;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.SaveAndLoad
{
    internal sealed class SaveLoad
    {
        public void Save(IEnumerable<BGameObject> currentState, string path)
        {
            var saveState = new XmlDocument();
            var xmlRoot = saveState.CreateElement("GameObjects");
            saveState.AppendChild(xmlRoot);

            Game1.sTileMap.ToXml(saveState, xmlRoot);
            Game1.sFog.ToXml(saveState, xmlRoot);

            foreach (var ob in currentState)
            {
                ob.ToXml(saveState, xmlRoot);
            }

            saveState.Save(path);
        }

        public void Load(string path)
        {
            if (!File.Exists(path))
            {
                Debug.WriteLine($"Spielstand \" {path}\" konnte nicht geladen werden. Bitte überprüfen.\n");
            }

            string c = "";
            try
            {
                ObjectManager.Instance.UnloadAll();
                var doc = new XmlDocument();
                doc.Load(path);
                Debug.WriteLine("-----------------------------------------------");
                if (doc.DocumentElement != null)
                    foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                    {
                        c = node.Name;
                        if (node.Attributes == null) continue;
                        if (node.Name == "Farmer")
                        {

                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var money = float.Parse(node.Attributes.Item(3).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var seed1 = int.Parse(node.Attributes.Item(4).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var seed2 = int.Parse(node.Attributes.Item(5).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var seed3 = int.Parse(node.Attributes.Item(6).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var points = int.Parse(node.Attributes.Item(7).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var time = float.Parse(node.Attributes.Item(8).Value,
                                System.Globalization.CultureInfo.InvariantCulture);


                            ObjectManager.Instance.Add(new Farmer(x,y));
                            ObjectManager.Instance.GetFarmer().Health = (int)health;
                            EconomyManager.Instance.GoldAmount = (int)money;
                            EconomyManager.Instance.SeedAmount1 = seed1;
                            EconomyManager.Instance.SeedAmount2 = seed2;
                            EconomyManager.Instance.SeedAmount3 = seed3;
                            Game1.sStatistics.Points = points;
                            Game1.mTime = time;
                            Debug.WriteLine("Farmer Loaded"+ points);
                        }

                        else if (node.Name == "Fence")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value.PadLeft(2),
                                System.Globalization.CultureInfo.InvariantCulture);

                            var level = int.Parse(node.Attributes.Item(3).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            ObjectManager.Instance.Add(new Fence((int)x,(int)y, level:level, loadhealth:(int)health));
                            Debug.WriteLine("Fence Loaded");
                        }

                        else if (node.Name == "FogOfWar")
                        {
                            Game1.sFog.Reset();
                            for (var i = 0; i < node.Attributes.Count; i++)
                            {
                                var visiblePoint = node.Attributes.Item(i).Value.Split(',');
                                var end = visiblePoint[1].Length - 1;
                                var x = int.Parse(visiblePoint[0].Remove(0, 1));
                                var y = int.Parse(visiblePoint[1].Remove(end));
                                Game1.sFog.SetField(x,y);
                            }
                        }

                        else if (node.Name == "Tower")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var level = int.Parse(node.Attributes.Item(3).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            ObjectManager.Instance.Add(new Tower((int)x, (int)y, loadhealth:(int)health, level:level));
                            Debug.WriteLine("Tower Loaded");
                        }

                        else if (node.Name == "Chicken")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var level = int.Parse(node.Attributes.Item(3).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            ObjectManager.Instance.Add(new Chicken((int)x, (int)y, loadhealth: (int)health, level:level));
                            Debug.WriteLine("Chicken Loaded");
                        }

                        else if (node.Name == "Cow")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var level = int.Parse(node.Attributes.Item(3).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            ObjectManager.Instance.Add(new Cow((int)x, (int)y, loadhealth: (int)health, level:level));
                            Debug.WriteLine("Cow Loaded");
                        }

                        else if (node.Name == "AttackCow")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            ObjectManager.Instance.Add(new AttackCow((int)x, (int)y, loadhealth: (int)health));
                            Debug.WriteLine("Attackcow Loaded");
                        }

                        else if (node.Name == "AttackChicken")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            ObjectManager.Instance.Add(new AttackChicken((int)x, (int)y, loadhealth: (int)health));
                            Debug.WriteLine("Attackchicken Loaded");
                        }

                        else if (node.Name == "AttackPig")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            ObjectManager.Instance.Add(new AttackPig((int)x, (int)y, loadhealth: (int)health));
                            Debug.WriteLine("Attackpig Loaded");
                        }

                        else if (node.Name == "Trap")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            ObjectManager.Instance.Add(new Trap((int)x, (int)y));
                            Debug.WriteLine("Trap Loaded");
                        }

                        else if (node.Name == "FarmerHouse")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value,
                                System.Globalization.CultureInfo.InvariantCulture);


                            ObjectManager.Instance.Add(new Farmhouse((int)x, (int)y, loadhealth:(int)health));
                        }
                        
                        else if (node.Name == "Necromancer")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            ObjectManager.Instance.Add(new Necromancer((int)x, (int)y, loadhealth: (int)health));
                        }

                        else if (node.Name == "Pig")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var level = int.Parse(node.Attributes.Item(3).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            ObjectManager.Instance.Add(new Pig((int)x, (int)y, loadhealth: (int)health, level));
                            Debug.WriteLine("pig Loaded");
                        }

                        else if (node.Name == "Tree")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            ObjectManager.Instance.Add(new Tree((int)x, (int)y, loadhealh: (int)health));
                            Debug.WriteLine("Tree Loaded");
                        }

                        else if (node.Name == "Boulder")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            ObjectManager.Instance.Add(new Boulder((int)x, (int)y));
                            Debug.WriteLine("Boulder Loaded");
                        }

                        else if (node.Name == "Zombie")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            if (health > 0)
                            {
                                ObjectManager.Instance.Add(new Zombie((int)x, (int)y, loadhealth: (int)health));
                                Debug.WriteLine("Zombie Loaded");
                            }
                        }

                        else if (node.Name == "Graveyard")
                        {
                            var x = float.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = float.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(2).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            ObjectManager.Instance.Add(new Graveyard((int)x, (int)y, (int)health));
                            Debug.WriteLine("graveyard Loaded");
                        }

                        else if (node.Name == "Wheat")
                        {
                            var x = int.Parse(node.Attributes.Item(0).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var y = int.Parse(node.Attributes.Item(1).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var type = node.Attributes.Item(2).Value;
                            Wheat.WheatType tmp;

                            if (type == "Corn")
                            {
                                tmp = Wheat.WheatType.Corn;
                            } else if (type == "Wheat1")
                            {
                                tmp = Wheat.WheatType.Wheat1;
                            } else
                            {
                                tmp = Wheat.WheatType.Wheat2;
                            }

                            var stage = int.Parse(node.Attributes.Item(3).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var time = float.Parse(node.Attributes.Item(4).Value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            var health = float.Parse(node.Attributes.Item(5).Value,
                                System.Globalization.CultureInfo.InvariantCulture);


                            Game1.sTileMap.mPlowedTiles.Remove(new Vector2(x, y));
                            ObjectManager.Instance.Add(new Wheat(x, y, tmp, stage:stage, loadinghealth : (int)health, time:time));
                            Game1.sTileMap.SetTile(x, y, Tile.Dirt);
                            Debug.WriteLine("wheat Loaded");
                        }

                        else if (node.Name == "Map")
                        {
                            // first planting grass everywhere
                            for (var i = 0; i < Game1.MapWidth; i++)
                            {
                                for (var j = 0; j < Game1.MapHeight; j++)
                                {
                                    Game1.sTileMap.PlantGrass(i, j, true);
                                }
                            }
                            // then loading all fields which are not grass
                            for (var i = 0; i < node.Attributes.Count; i++)
                            {
                                var visiblePoint = node.Attributes.Item(i).Value.Split(',');
                                var x = int.Parse(visiblePoint[0]);
                                var y = int.Parse(visiblePoint[1]);

                                if (int.Parse(node.Attributes.Item(i).Name.Split('X')[1]) == 1)
                                {
                                    Game1.sTileMap.PlowTile(x, y);
                                }

                                if (int.Parse(node.Attributes.Item(i).Name.Split('X')[1]) == 2)
                                {
                                    Game1.sTileMap.PlantWater(x, y);
                                }

                                if (int.Parse(node.Attributes.Item(i).Name.Split('X')[1]) == 3)
                                {
                                    Game1.sTileMap.PlantWasteland(x, y);
                                }
                            }
                        }
                    }

                Ai.AddWave(ObjectManager.Instance.GetZombies().ToList());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (c != null)
                {
                    Debug.WriteLine(c + "error");
                }
                throw;
            }
        }
    }
}