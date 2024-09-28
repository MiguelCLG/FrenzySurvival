using System;
using System.Collections.Generic;
using Godot;


namespace GeneratedData
{
    public static class Generator
    {
        public static Random r = new(111190);
        public static string GenerateName(int len)
        {
            Random r = new Random();
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
            string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
            string Name = "";
            Name += consonants[r.Next(consonants.Length)].ToUpper();
            Name += vowels[r.Next(vowels.Length)];
            int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
            while (b < len)
            {
                Name += consonants[r.Next(consonants.Length)];
                b++;
                Name += vowels[r.Next(vowels.Length)];
                b++;
            }

            return Name;
        }

        public static int NumberBetween(int a, int b)
        {
            return r.Next(a, b);
        }

        public static string PickUnitTextureName()
        {
            List<string> unitTextureNames = new List<string>
            {
                "res://Assets/Units/deep_elf_annihilator.png",
                "res://Assets/Units/deep_elf_blademaster.png",
                "res://Assets/Units/deep_elf_death_mage.png",
                "res://Assets/Units/deep_elf_demonologist.png",
                "res://Assets/Units/deep_elf_fighter_new.png",
                "res://Assets/Units/deep_elf_high_priest.png",
                "res://Assets/Units/deep_elf_knight_new.png",
                "res://Assets/Units/deep_elf_mage.png",
                "res://Assets/Units/deep_elf_master_archer.png",
                "res://Assets/Units/deep_elf_priest.png",
                "res://Assets/Units/deep_elf_soldier.png",
                "res://Assets/Units/deep_elf_summoner.png",
            };
            GD.Print("GENERETED TEXTURE: " + unitTextureNames[NumberBetween(0, unitTextureNames.Count)].ToString());
            return unitTextureNames[NumberBetween(0, unitTextureNames.Count)].ToString();
        }
    }
}


