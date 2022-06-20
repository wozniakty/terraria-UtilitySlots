using Terraria;
using Terraria.ModLoader;
using UtilitySlots.UI;

namespace UtilitySlots {
    internal class GlobalWingItem : GlobalItem {
        public override bool CanEquipAccessory(Item item, Player player, int slot) {
            if(item.wingSlot <= 0) return base.CanEquipAccessory(item, player, slot);

            WingSlotUI ui = UtilitySlots.UI;

            return ui.Panel.ContainsPoint(Main.MouseScreen) ||
                   (UtilitySlotsConfig.Instance.AllowAccessorySlots && UtilitySlots.UI.EquipSlot.Item.IsAir);

        }

        public override bool CanRightClick(Item item) {
            return item.wingSlot > 0 &&
                   !UtilitySlots.OverrideRightClick() &&
                   (!UtilitySlotsConfig.Instance.AllowAccessorySlots ||
                    !UtilitySlots.UI.EquipSlot.Item.IsAir ||
                    Main.LocalPlayer.wingTimeMax == 0);
        }

        public override void RightClick(Item item, Player player) {
            if(item.wingSlot <= 0) return;

            player.GetModPlayer<UtilitySlotsPlayer>().EquipItem(
                item,
                KeyboardUtils.Shift ? UtilitySlotsPlayer.EquipType.Social : UtilitySlotsPlayer.EquipType.Accessory,
                true);
        }
    }
}
