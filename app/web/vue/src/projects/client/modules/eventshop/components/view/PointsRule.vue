<template>
  <div class="modal-body w-100 overflow-auto card md:mt-5 md:my-6 ps-3">
    <div class="w-100 notice-content card-body">
      <div v-for="(option, optionIndex) in options" :key="optionIndex">
        <div class="w-100 notice-content" v-html="pointsRule[option.key]"></div>
        <!-- <el-divider class="my-3" /> -->
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, inject, onMounted } from "vue";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import { useStore } from "@/store";
import { RoleTypes } from "@/core/types/RoleTypes";
import i18n from "@/core/plugins/i18n";

const t = i18n.global.t;
const store = useStore();
const eventDetail = inject(ClientGlobalInjectionKeys.EVENT_SHOP_DETAIL);
const user = store.state.AuthModule.user;
const userRoles = user.roles;
const activeType = ref("all");
const options = ref([
  { label: t("fields.all"), value: "All", key: "all" },
  { label: t("fields.agent"), value: "IB", key: "agent" },
  { label: t("fields.client"), value: "Client", key: "client" },
  { label: t("fields.sales"), value: "Sales", key: "sales" },
]);

const pointsRule = ref(eventDetail.value?.instruction.pointsRule);

const changeTab = (type: string) => {
  activeType.value = type;
};

const filterOptions = () => {
  if (!Object.values(userRoles).includes(RoleTypes.Sales)) {
    options.value = options.value.filter((option) => option.value !== "Sales");
  }
  if (!Object.values(userRoles).includes(RoleTypes.Client)) {
    options.value = options.value.filter((option) => option.value !== "Client");
  }
  if (!Object.values(userRoles).includes(RoleTypes.IB)) {
    options.value = options.value.filter((option) => option.value !== "IB");
  }
};
onMounted(() => {
  filterOptions();
});
</script>
<style lang="scss" scoped>
.active-nav {
  color: #f7b93f;
}
</style>
