using System;
using UnityEngine;

namespace Sirvival
{
    public enum Rarity { Common, Rare, Epic }

    /// <summary>One draftable upgrade: metadata (incl. rarity + icon) + a stat mutation.</summary>
    public class Upgrade
    {
        public string Id;
        public string Title;
        public string Desc;
        public string IconId;      // maps to Assets/Art/Sprites/Sirvival/ui/ic_<IconId>.png
        public Rarity Rarity;
        private readonly Action<PlayerStats> _apply;

        public Upgrade(string id, string title, string desc, string iconId, Rarity rarity, Action<PlayerStats> apply)
        {
            Id = id; Title = title; Desc = desc; IconId = iconId; Rarity = rarity; _apply = apply;
        }

        public void Apply(PlayerStats s) => _apply(s);

        public static string RarityLabel(Rarity r) => r == Rarity.Epic ? "EPIC" : r == Rarity.Rare ? "RARE" : "COMMON";

        public static Color RarityColor(Rarity r) =>
            r == Rarity.Epic ? new Color(0.61f, 0.42f, 0.88f) :
            r == Rarity.Rare ? new Color(0.31f, 0.61f, 1f) :
                                new Color(0.54f, 0.54f, 0.58f);
    }
}
