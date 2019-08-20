using System.Globalization;
using System.IO;
using System.Xml;
using FarmervsZombies.Managers;

namespace FarmervsZombies
{
    internal sealed class Statistics
    {
        public int Animals { get; private set; }
        private int AnimalsRound { get; set; }
        public int ZombiesKilled { get; private set; }
        private int ZombiesKilledRound { get; set; }
        private int AnimalsAlive { get; set; }
        public int GameLost { get; private set; }
        public int GameWon { get; private set; }
        public int Points { get; set; }
        public int OldGold { get; private set; }
        public int OldAnimalsAlive { get; private set; }
        public float OldGameTime { get; private set; }
        public int OldPoints { get; private set; }

        public int mHighScore1;
        public int mHighScore2;
        public int mHighScore3;

        public void SetAnimalsLiving(int count)
        {
            if (count < AnimalsAlive)
            {
                Game1.sAchievements.AnimalsDied = true;
            }
            AnimalsAlive = count;
            if (AnimalsAlive == 20 && !Game1.sAchievements.WahrerFarmer)
            {
                Game1.sAchievements.WahrerFarmer = true;
                NotificationManager.AddNotification("Achievement freigeschaltet!", "Du hast das Achievement \"Wahrer Farmer\" erreicht.", 6.0f);
            }
        }

        public void IncreaseAnimals()
        {
            Animals += 1;
            if (!Game1.sAchievements.ErsteGeburt)
            {
                Game1.sAchievements.ErsteGeburt = true;
                NotificationManager.AddNotification("Achievement freigeschaltet!", "Du hast das Achievement \"Erste Geburt\" erreicht.", 6.0f);
            }
            AnimalsRound++;
        }

        public void IncreaseZombiesKilled()
        {
            ZombiesKilled += 1;
            ZombiesKilledRound++;
            if (ZombiesKilled == 5 && !Game1.sAchievements.DerAnfangVomEnde)
            {
                Game1.sAchievements.DerAnfangVomEnde = true;
                NotificationManager.AddNotification("Achievement freigeschaltet!", "Du hast das Achievement \"Der Anfang vom Ende\" erreicht.", 6.0f);
            }
            else if (ZombiesKilled == 50 && !Game1.sAchievements.VomGejagtenZumJäger)
            {
                Game1.sAchievements.VomGejagtenZumJäger = true;
                NotificationManager.AddNotification("Achievement freigeschaltet!", "Du hast das Achievement \"Vom Gejagten zum Jäger\" erreicht.", 6.0f);
            }
        }

        public void Lost()
        {
            GameLost++;
            if (Game1.sAchievements.Gehirnlos) return;
            Game1.sAchievements.Gehirnlos = true;
            NotificationManager.AddNotification("Achievement freigeschaltet!", "Du hast das Achievement \"Gehirnlos\" erreicht.", 6.0f);
        }

        public void Win()
        {
            GameWon++;
            if (!Game1.sAchievements.DieApokalypseIstVorbei)
            {
                Game1.sAchievements.DieApokalypseIstVorbei = true;
                NotificationManager.AddNotification("Achievement freigeschaltet!", "Du hast das Achievement \"Die Apokalypse ist vorbei\" erreicht.", 6.0f);
            }

            if (!Game1.sAchievements.AnimalsDied)
            {
                Game1.sAchievements.SieHattenKeineChance = true;
                NotificationManager.AddNotification("Achievement freigeschaltet!", "Du hast das Achievement \"Sie hatten keine Chance\" erreicht.", 6.0f);
            }
        }

        public void Update()
        {
            Points = (int)(Game1.mTime * (ZombiesKilledRound + (float)EconomyManager.Instance.GoldAmount / 20 + AnimalsRound));
        }


        public void Save()
        {
            if (File.Exists("stats.xml"))
            {
                File.Delete("stats.xml");
            }

            if (Points > mHighScore1)
            {
                mHighScore3 = mHighScore2;
                mHighScore2 = mHighScore1;
                mHighScore1 = Points;
            }
            else if (Points > mHighScore2 && Points < mHighScore1)
            {
                mHighScore3 = mHighScore2;
                mHighScore2 = Points;
            }
            else if (Points < mHighScore2 && Points > mHighScore3)
            {
                mHighScore3 = Points;
            }

            var saveState = new XmlDocument();
            var xmlRoot = saveState.CreateElement("GameObjects");

            // storing gold

            xmlRoot.SetAttribute("gold",
                OldGold >= EconomyManager.Instance.GoldAmount ? OldGold.ToString(CultureInfo.InvariantCulture) : EconomyManager.Instance.GoldAmount.ToString());

            // storing killed zombies
            xmlRoot.SetAttribute("zombieskilled", ZombiesKilled.ToString(CultureInfo.InvariantCulture));
            // storing max alive animals 
            xmlRoot.SetAttribute("maxLivingAnimals",
                OldAnimalsAlive >= AnimalsAlive ? OldAnimalsAlive.ToString(CultureInfo.InvariantCulture) : AnimalsAlive.ToString(CultureInfo.InvariantCulture));
            AnimalsAlive = 0;
            // storing max points 
            xmlRoot.SetAttribute("points", OldPoints >= Points ? OldPoints.ToString(CultureInfo.InvariantCulture) : Points.ToString(CultureInfo.InvariantCulture));
            // bought animals
            xmlRoot.SetAttribute("animals",Animals.ToString(CultureInfo.InvariantCulture));

            // in game time
            xmlRoot.SetAttribute("gametime", ((int)OldGameTime + (int)Game1.mTime).ToString(CultureInfo.InvariantCulture));

            xmlRoot.SetAttribute("lost", (GameLost).ToString());

            xmlRoot.SetAttribute("win", (GameWon).ToString());

            xmlRoot.SetAttribute("highscore1", mHighScore1.ToString());
            xmlRoot.SetAttribute("highscore2", mHighScore2.ToString());
            xmlRoot.SetAttribute("highscore3", mHighScore3.ToString());
            

            saveState.AppendChild(xmlRoot);
            saveState.Save("stats.xml");
        }

        public void Load()
        {
            if (File.Exists("stats.xml"))
            { 
                var doc = new XmlDocument();
                doc.Load("stats.xml");
                if (doc.DocumentElement?.ParentNode == null) return;
                foreach (XmlNode node in doc.DocumentElement.ParentNode)
                {
                    if (node.Attributes == null) continue;
                    for (var i = 0; i < node.Attributes.Count; i++)
                    {
                            if (node.Attributes.Item(i).Name == "gold")
                            {
                                OldGold = int.Parse(node.Attributes.Item(i).Value);
                            }

                            if (node.Attributes.Item(i).Name == "zombieskilled")
                            {
                                ZombiesKilled = int.Parse(node.Attributes.Item(i).Value);
                            }

                            if (node.Attributes.Item(i).Name == "maxLivingAnimals")
                            {
                                OldAnimalsAlive = int.Parse(node.Attributes.Item(i).Value);
                            }

                            if (node.Attributes.Item(i).Name == "animals")
                            {
                                Animals = int.Parse(node.Attributes.Item(i).Value);
                            }

                            if (node.Attributes.Item(i).Name == "gametime")
                            {
                                OldGameTime = float.Parse(node.Attributes.Item(i).Value);
                            }

                            if (node.Attributes.Item(i).Name == "points")
                            {
                                OldPoints = int.Parse(node.Attributes.Item(i).Value);
                            }

                            if (node.Attributes.Item(i).Name == "lost")
                            {
                                GameLost= int.Parse(node.Attributes.Item(i).Value);
                            }

                            if (node.Attributes.Item(i).Name == "win")
                            {
                                GameWon = int.Parse(node.Attributes.Item(i).Value);
                            }

                            if (node.Attributes.Item(i).Name == "highscore1")
                            {
                                mHighScore1 = int.Parse(node.Attributes.Item(i).Value);
                            }

                            if (node.Attributes.Item(i).Name == "highscore2")
                            {
                                mHighScore2 = int.Parse(node.Attributes.Item(i).Value);
                            }

                            if (node.Attributes.Item(i).Name == "highscore3")
                            {
                                mHighScore3 = int.Parse(node.Attributes.Item(i).Value);
                            }

                    }
                }
            }
            else
            {
                OldGold = 0;
                OldGameTime = 0;
                OldAnimalsAlive = 0;
                OldGameTime = 0;
                OldPoints = 0;
            }
        }
    }
}
