﻿using System;
using CustomSlot;
using CustomSlot.UI;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace UtilitySlots {
    internal class UtilitySlotsPlayer : ModPlayer {
        public enum EquipType {
            Accessory,
            Social,
            Dye
        }

        private const string PanelXTag = "panelx";
        private const string PanelYTag = "panely";
        private const string HiddenTag = "hidden";
        private const string WingsTag = "wings";
        private const string SocialWingsTag = "vanitywings";
        private const string WingsDyeTag = "wingdye";

        public Item EquippedWings { get; set; }
        public Item SocialWings { get; set; }
        public Item WingsDye { get; set; }
        public bool WingsVisible { get; set; }

        public override void Initialize() {
            EquippedWings = new Item();
            SocialWings = new Item();
            WingsDye = new Item();
            WingsVisible = true;

            EquippedWings.SetDefaults();
            SocialWings.SetDefaults();
            WingsDye.SetDefaults();
        }

        public override void OnEnterWorld(Player player) {
            EquipItem(EquippedWings, EquipType.Accessory, false);
            EquipItem(SocialWings, EquipType.Social, false);
            EquipItem(WingsDye, EquipType.Dye, false);
            UtilitySlots.UI.EquipSlot.ItemVisible = WingsVisible;
        }

        public override void clientClone(ModPlayer clientClone) {
            UtilitySlotsPlayer clone = clientClone as UtilitySlotsPlayer;

            if(clone == null) {
                return;
            }

            clone.EquippedWings = EquippedWings.Clone();
            clone.SocialWings = SocialWings.Clone();
            clone.WingsDye = WingsDye.Clone();
        }

        //public override void SendClientChanges(ModPlayer clientPlayer) {
        //    WingSlotPlayer oldClone = clientPlayer as WingSlotPlayer;

        //    if(oldClone == null) {
        //        return;
        //    }

        //    if(oldClone.EquippedWings.IsNotTheSameAs(EquippedWings)) {
        //        SendSingleItemPacket(PacketMessageType.EquipSlot, EquippedWings, -1, player.whoAmI);
        //    }

        //    if(oldClone.SocialWings.IsNotTheSameAs(SocialWings)) {
        //        SendSingleItemPacket(PacketMessageType.VanitySlot, SocialWings, -1, player.whoAmI);
        //    }

        //    if(oldClone.WingsDye.IsNotTheSameAs(WingsDye)) {
        //        SendSingleItemPacket(PacketMessageType.DyeSlot, WingsDye, -1, player.whoAmI);
        //    }
        //}

        //internal void SendSingleItemPacket(PacketMessageType message, Item item, int toWho, int fromWho) {
        //    ModPacket packet = mod.GetPacket();
        //    packet.Write((byte)message);
        //    packet.Write((byte)player.whoAmI);
        //    ItemIO.Send(item, packet);
        //    packet.Send(toWho, fromWho);
        //}

        // TODO: fix sending packets to other players
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)PacketMessageType.All);
            packet.Write((byte)player.whoAmI);
            ItemIO.Send(EquippedWings, packet);
            ItemIO.Send(SocialWings, packet);
            ItemIO.Send(WingsDye, packet);
            packet.Send(toWho, fromWho);
        }

        public override void ModifyDrawInfo(ref PlayerDrawInfo drawInfo) {
            if(WingsDye.stack > 0 && (EquippedWings.stack > 0 || SocialWings.stack > 0)) {
                drawInfo.wingShader = WingsDye.dye;
            }
        }

        /// <summary>
        /// Update player with the equipped wings.
        /// </summary>
        public override void UpdateEquips(ref bool wallSpeedBuff, ref bool tileSpeedBuff, ref bool tileRangeBuff) {
            if(UtilitySlots.UI == null) return;

            if(EquippedWings.stack > 0) {
                player.VanillaUpdateAccessory(player.whoAmI, EquippedWings, !WingsVisible, ref wallSpeedBuff, 
                                              ref tileSpeedBuff, ref tileRangeBuff);
                player.VanillaUpdateEquip(EquippedWings);
            }

            if(SocialWings.stack > 0) {
                player.VanillaUpdateVanityAccessory(SocialWings);
            }
        }

        /// <summary>
        /// Since there is no tModLoader hook in UpdateDyes, we use PreUpdateBuffs which is right after that.
        /// </summary>
        public override void PreUpdateBuffs() {
            // Cleaned up vanilla code
            if(UtilitySlots.UI == null) return;

            if(WingsDye.stack <= 0) 
                return;

            if(SocialWings.stack > 0 || (EquippedWings.stack > 0 && WingsVisible)) {
                player.cWings = WingsDye.dye;
            }
        }

        /// <summary>
        /// Drop items if the player character is Medium or Hardcore.
        /// </summary>
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
            if(player.difficulty == 0) return;

            player.QuickSpawnClonedItem(EquippedWings);
            player.QuickSpawnClonedItem(SocialWings);
            player.QuickSpawnClonedItem(WingsDye);

            EquipItem(new Item(), EquipType.Accessory, false);
            EquipItem(new Item(), EquipType.Social, false);
            EquipItem(new Item(), EquipType.Dye, false);
        }

        /// <summary>
        /// Save player settings.
        /// </summary>
        public override TagCompound Save() {
            return new TagCompound {
                { PanelXTag, UtilitySlots.UI.PanelCoordinates.X },
                { PanelYTag, UtilitySlots.UI.PanelCoordinates.Y },
                { HiddenTag, WingsVisible },
                { WingsTag, ItemIO.Save(EquippedWings) },
                { SocialWingsTag, ItemIO.Save(SocialWings) },
                { WingsDyeTag, ItemIO.Save(WingsDye) }
            };
        }

        /// <summary>
        /// Load the mod settings.
        /// </summary>
        public override void Load(TagCompound tag) {
            if(tag.ContainsKey(WingsTag))
                EquippedWings = ItemIO.Load(tag.GetCompound(WingsTag));

            if(tag.ContainsKey(SocialWingsTag))
                SocialWings = ItemIO.Load(tag.GetCompound(SocialWingsTag));

            if(tag.ContainsKey(WingsDyeTag))
                WingsDye = ItemIO.Load(tag.GetCompound(WingsDyeTag));

            if(tag.ContainsKey(HiddenTag))
                WingsVisible = tag.GetBool(HiddenTag);

            if(tag.ContainsKey(PanelXTag))
                UtilitySlots.UI.PanelCoordinates.X = tag.GetFloat(PanelXTag);

            if(tag.ContainsKey(PanelYTag))
                UtilitySlots.UI.PanelCoordinates.Y = tag.GetFloat(PanelYTag);
        }

        /// <summary>
        /// Equip either wings or a dye in a slot.
        /// </summary>
        /// <param name="item">item to equip</param>
        /// <param name="type">what type of slot to equip in</param>
        /// <param name="fromInventory">whether the item is being equipped from the inventory</param>
        public void EquipItem(Item item, EquipType type, bool fromInventory) {
            if(item == null) return;

            CustomItemSlot slot;

            if(type == EquipType.Dye) {
                slot = UtilitySlots.UI.DyeSlot;

                if(WingsDye.IsNotTheSameAs(item))
                    WingsDye = item.Clone();
            }
            else if(type == EquipType.Social) {
                slot = UtilitySlots.UI.SocialSlot;

                if(SocialWings.IsNotTheSameAs(item))
                    SocialWings = item.Clone();
            }
            else {
                slot = UtilitySlots.UI.EquipSlot;

                if(EquippedWings.IsNotTheSameAs(item))
                    EquippedWings = item.Clone();
            }

            if(fromInventory) {
                item.favorited = false;

                int fromSlot = Array.FindIndex(player.inventory, i => i == item);

                if(fromSlot < 0) return;

                player.inventory[fromSlot] = slot.Item.Clone();
                Main.PlaySound(SoundID.Grab);
                Recipe.FindRecipes();
            }

            slot.SetItem(item);
        }

        /// <summary>
        /// Fires when the item in a slot is changed.
        /// </summary>
        public void ItemChanged(CustomItemSlot slot, ItemChangedEventArgs e) {
            if(slot.Context == ItemSlot.Context.EquipAccessory)
                EquippedWings = e.NewItem.Clone();
            else if(slot.Context == ItemSlot.Context.EquipAccessoryVanity)
                SocialWings = e.NewItem.Clone();
            else
                WingsDye = e.NewItem.Clone();
        }

        /// <summary>
        /// Fires when the visibility of an item in a slot is toggled.
        /// </summary>
        public void ItemVisibilityChanged(CustomItemSlot slot, ItemVisibilityChangedEventArgs e) {
            WingsVisible = e.Visibility;
        }
    }
}