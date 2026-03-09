<template>
  <!-- <SalesCenterMenu activeMenuItem="link" /> -->
  <SalesLayout activeMenuItem="link">
    <div class="border-0 flex-1">
      <div
        v-if="projectConfig?.rebateEnabled"
        class="flex-1 h-full d-flex flex-column"
      >
        <div class="card mb-2 px-4 round-bl-br">
          <div class="my-3 px-0 d-flex gap-1">
            <span
              class="basic-tab btn btn-light btn-bordered"
              :class="{
                'active-tab btn-primary': activeTab === tab.manageLink,
              }"
              @click="activeTab = tab.manageLink"
            >
              {{ t("title.manageLink") }}
            </span>
            <span
              class="basic-tab btn btn-light btn-bordered"
              :class="{
                'active-tab  btn-primary': activeTab === tab.addNewLink,
              }"
              @click="activeTab = tab.addNewLink"
            >
              {{ t("title.addLink") }}
            </span>
          </div>
        </div>
        <div class="card card-body round-tl-tr">
          <div v-if="activeTab == tab.manageLink">
            <SalesManageLink ref="salesManageLinkRef" />
          </div>

          <div v-if="activeTab == tab.addNewLink">
            <SalesAddNewLink @refresh="refresh" />
          </div>
        </div>
      </div>
      <SalesManageLinkSimp v-else />
    </div>
  </SalesLayout>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { isMobile } from "@/core/config/WindowConfig";
import SalesCenterMenu from "../components/SalesCenterMenu.vue";
import SalesManageLink from "../components/SalesManageLink.vue";
import SalesAddNewLink from "../components/SalesAddNewLink.vue";
import SalesManageLinkSimp from "../components/SalesManageLinkSimp.vue";
import SalesLayout from "../components/SalesLayout.vue";

const { t } = useI18n();
const store = useStore();
const projectConfig: PublicSetting = store.state.AuthModule.config;
const salesManageLinkRef = ref<InstanceType<typeof SalesManageLink>>();

const activeTab = ref("manageLink");
const tab = ref({
  manageLink: "manageLink",
  addNewLink: "addNewLink",
});

const refresh = () => {
  activeTab.value = "manageLink";
  salesManageLinkRef.value?.fetchData();
};
</script>

<style scoped lang="scss">
.basic-tab {
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 15px;
  // border-radius: 5px 5px 0 0;
  // width: 150px;
  // height: 40px;
  //border: 2px solid #ffd400;
  cursor: pointer;
  border-bottom: 0;
  transition: background-color 0.3s;

  @media (max-width: 768px) {
    flex: 1;
  }
}
@media (max-width: 768px) {
  .basic-tab {
    font-size: 12px;
  }
}
.active-tab {
  background-color: #000f32;
  color: #fff;
}
</style>
