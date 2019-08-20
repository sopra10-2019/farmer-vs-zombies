using System.Diagnostics;
using System.IO;
using System.Xml;
using FarmervsZombies.Managers;

namespace FarmervsZombies
{
    internal sealed class Achievements
    {
        public bool ErsteGeburt { get; set; }

        public bool WahrerFarmer { get; set; }

        public bool FarmingSimulator { get; private set; }

        public bool DerAnfangVomEnde { get; set; }

        public bool VomGejagtenZumJäger { get; set; }

        public bool Gehirnlos { get; set; }

        public bool Obdachlos { get; set; }

        public bool DasWarErstDerAnfang { get; set; }

        public bool DieLangeNacht { get; set; }

        public bool DieApokalypseIstVorbei { get; set; }

        public bool SieHattenKeineChance { get; set; }

        public bool AnimalsDied { get; set; }

        private float mGoldEarned;

        public void GoldIncrease(int increase)
        {
            mGoldEarned += increase;
            if (!(mGoldEarned >= 1000)) return;
            if (FarmingSimulator) return;
            FarmingSimulator = true;
            NotificationManager.AddNotification("Achievement freigeschaltet!", "Du hast das Achievement \"FarmingSimulator\" erreicht.", 6.0f);
        }

        public void Save()
        {
            if (File.Exists("achievments.xml"))
            {
                File.Delete("achievments.xml");
            }


            var saveState = new XmlDocument();
            var xmlRoot = saveState.CreateElement("GameObjects");

            // storing gold
            if (ErsteGeburt)
            {
                xmlRoot.SetAttribute("ErsteGeburt", null);

            }

            if (WahrerFarmer)
            {
                xmlRoot.SetAttribute("WahrerFarmer", null);

            }

            if (FarmingSimulator)
            {
                xmlRoot.SetAttribute("FarmingSimulator", null);

            }

            if (DerAnfangVomEnde)
            {
                xmlRoot.SetAttribute("DerAnfangVomEnde", null);

            }

            if (VomGejagtenZumJäger)
            {
                xmlRoot.SetAttribute("VomGejagtenZumJäger", null);

            }

            if (Gehirnlos)
            {
                xmlRoot.SetAttribute("Gehirnlos", null);

            }

            if (Obdachlos)
            {
                xmlRoot.SetAttribute("Obdachlos", null);
            }

            if (DasWarErstDerAnfang)
            {
                xmlRoot.SetAttribute("DasWarErstDerAnfang", null);

            }

            if (DieLangeNacht)
            {
                xmlRoot.SetAttribute("DieLangeNacht", null);

            }

            if (DieApokalypseIstVorbei)
            {
                xmlRoot.SetAttribute("DieApokalypseIstVorbei", null);
            }

            if (SieHattenKeineChance)
            {
                xmlRoot.SetAttribute("SieHattenKeineChance", null);
            }
            saveState.AppendChild(xmlRoot);
            saveState.Save("achievments.xml");
        }

        public void Load()
        {
            if (File.Exists("achievments.xml"))
            {
                var doc = new XmlDocument();
                doc.Load("achievments.xml");
                if (doc.DocumentElement?.ParentNode == null) return;
                foreach (XmlNode node in doc.DocumentElement.ParentNode)
                {
                    if (node.Attributes == null) continue;
                    for (var i = 0; i < node.Attributes.Count; i++)
                    {
                        if (node.Attributes.Item(i).Name == "ErsteGeburt")
                        {
                            ErsteGeburt = true;
                        }

                        if (node.Attributes.Item(i).Name == "WahrerFarmer")
                        {
                            WahrerFarmer = true;
                        }

                        if (node.Attributes.Item(i).Name == "FarmingSimulator")
                        {
                            FarmingSimulator = true;
                        }

                        if (node.Attributes.Item(i).Name == "DerAnfangVomEnde")
                        {
                            DerAnfangVomEnde = true;
                        }

                        if (node.Attributes.Item(i).Name == "VomGejagtenZumJäger")
                        {
                            VomGejagtenZumJäger = true;
                        }

                        if (node.Attributes.Item(i).Name == "Gehirnlos")
                        {
                            Gehirnlos = true;
                        }

                        if (node.Attributes.Item(i).Name == "Obdachlos")
                        {
                            Obdachlos = true;
                        }

                        if (node.Attributes.Item(i).Name == "DasWarErstDerAnfang")
                        {
                            DasWarErstDerAnfang = true;
                        }

                        if (node.Attributes.Item(i).Name == "DieLangeNacht")
                        {
                            DieLangeNacht = true;
                        }

                        if (node.Attributes.Item(i).Name == "DieApokalypseIstVorbei")
                        {
                            DieApokalypseIstVorbei = true;
                        }

                        if (node.Attributes.Item(i).Name == "SieHattenKeineChance")
                        {
                            SieHattenKeineChance = true;
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("Loading stats is not possible please Play a Game to generate");
            }
        }

    }
}