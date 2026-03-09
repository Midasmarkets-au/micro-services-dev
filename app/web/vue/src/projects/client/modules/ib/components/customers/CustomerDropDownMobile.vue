<template>
  <div
    v-if="item.role == AccountRoleTypes.Client"
    style="min-width: 82px"
    class="ps-2"
  >
    <router-link
      :to="'/ib/customers/' + item.uid"
      style="color: #7c8fa2; min-width: 100px"
    >
      {{ $t("action.viewDetails") }}
    </router-link>
  </div>
  <el-dropdown trigger="click" v-else>
    <el-button class="btn btn-xs btn-bordered" style="font-size: 12px">
      {{ $t("action.action")
      }}<el-icon class="el-icon--right"><arrow-down /></el-icon>
    </el-button>
    <template #dropdown>
      <el-dropdown-menu>
        <template
          v-for="(dropdownItem, index) in getDropdownItems(props.item)"
          :key="index"
        >
          <el-dropdown-item
            v-if="dropdownItem.condition"
            @click="dropdownItem.action && dropdownItem.action()"
          >
            <span>
              {{ dropdownItem.label }}
            </span>
          </el-dropdown-item>
        </template>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { useStore } from "@/store";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import i18n from "@/core/plugins/i18n";
import { ArrowDown } from "@element-plus/icons-vue";

const props = defineProps<{
  item: any;
}>();

const t = i18n.global.t;
const store = useStore();
const projectConfig: PublicSetting = store.state.AuthModule.config;
const IbSearch = inject<any>("IbSearch");
const showRebateStat = inject<any>("showRebateStat");
const showEditSchema = inject<any>("showEditSchema");
const selectedIbAccountsChain = inject<any>("selectedIbAccountsChain");

const getDropdownItems = (item) => {
  return [
    {
      condition: true,
      isLink: false,
      action: () => IbSearch(item),
      label: t("action.viewAccounts"),
    },
    {
      condition: true,
      isLink: false,
      action: () => showRebateStat(item),
      label: t("title.viewTradeStatistics"),
    },
    {
      condition:
        projectConfig?.rebateEnabled &&
        selectedIbAccountsChain?.value.length == 0,
      isLink: false,
      action: () => showEditSchema(item),
      label: t("action.editSchema"),
    },
  ];
};
</script>
