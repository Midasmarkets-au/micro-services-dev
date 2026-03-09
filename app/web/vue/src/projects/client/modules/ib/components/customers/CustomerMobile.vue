<template>
  <div v-if="isLoading" class="d-flex justify-content-center">
    <LoadingRing />
  </div>
  <div
    class="d-flex justify-content-center"
    v-else-if="!isLoading && customerAccounts.length === 0"
  >
    <NoDataBox />
  </div>
  <div v-else class="px-3">
    <el-card
      v-for="(item, index) in customerAccounts"
      :key="index"
      class="mb-3"
    >
      <div class="d-flex justify-content-between align-items-center">
        <div class="d-flex align-items-center">
          <UserAvatar
            :avatar="item.user?.avatar"
            :name="getUserName(item)"
            size="40px"
            class="me-3"
            side="client"
            rounded
          />
          <div class="d-flex">
            <div>
              <div>{{ getUserName(item) }}</div>
              <span>{{ item.user?.email }}</span>
            </div>
            <div class="d-flex flex-column gap-2 justify-content-end">
              <div v-if="currentRole == null" class="text-center">
                <span :class="getRoleType(item).class">
                  {{ getRoleType(item).label }}
                </span>
              </div>
              <CustomerDropDownMobile :item="item" />
            </div>
          </div>
        </div>

        <!-- <div>
          <div class="d-flex justify-content-end">
            <div class="w-100">
              <CustomerDropDownMobile :item="item" />
            </div>
          </div>
        </div> -->
      </div>
    </el-card>
  </div>
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import i18n from "@/core/plugins/i18n";
import CustomerDropDownMobile from "./CustomerDropDownMobile.vue";
const t = i18n.global.t;
const customerAccounts = inject<any>("customerAccounts");
const isLoading = inject<any>("isLoading");
const currentRole = inject<any>("currentRole");
const getUserName = inject<any>("getUserName");

const getRoleType = (item: any) => {
  switch (item.role) {
    case AccountRoleTypes.Client:
      return { class: "badge badge-primary", label: t("fields.client") };
    case AccountRoleTypes.IB:
      return { class: "badge badge-success", label: t("fields.ib") };
  }
};
</script>
