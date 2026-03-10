<template>
  <div v-if="accountUid === -1">
    <div v-if="isLoading" class="d-flex justify-content-center">
      <LoadingRing />
    </div>
    <div
      class="d-flex justify-content-center"
      v-else-if="!isLoading && data.length === 0"
    >
      <NoDataBox />
    </div>
    <div v-else>
      <el-card v-for="(item, index) in data" :key="index" class="mb-3">
        <div class="d-flex justify-content-between">
          <div
            class="d-flex align-items-center w-100"
            @click="showCustomerDetail(item)"
          >
            <UserAvatar
              :avatar="item.user?.avatar"
              :name="getUserName(item)"
              size="40px"
              class="me-3"
              side="client"
              rounded
            />
            <div>
              <div class="d-flex align-items-center gap-2">
                <div>{{ getUserName(item) }}</div>
                <div v-if="currentRole == null">
                  <span :class="getRoleType(item).class">
                    {{ getRoleType(item).label }}
                  </span>
                </div>
              </div>
              <span>{{ item.user?.email }}</span>
            </div>
          </div>
          <div>
            <div class="d-flex justify-content-end" style="margin-top: -12px">
              <div class="w-100">
                <CustomerDropDownMobile :item="item" />
              </div>
            </div>
            <div @click="showCustomerDetail(item)">
              <div>
                <span>
                  {{
                    item.role == AccountRoleTypes.Client
                      ? item.accountNumber == 0
                        ? t("fields.noTradeAccount")
                        : item.accountNumber
                      : item.uid
                  }}</span
                >
              </div>
              <div class="mt-1 text-end">
                <BalanceShow
                  v-if="item.role != AccountRoleTypes.IB"
                  :balance="item?.tradeAccount?.balanceInCents"
                  :currency-id="item?.tradeAccount?.currencyId"
                />
                <div v-else>
                  {{ item.group }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </el-card>
    </div>
    <TableFooter
      v-if="accountUid === -1"
      @page-change="fetchData"
      :criteria="criteria"
    />
  </div>
  <SalesCustomerDetail
    v-if="accountUid !== -1"
    :service-map="serviceMap"
    :customer-accounts="data"
  />
  <SalesCustomerDetailMobile ref="detailRef" />
</template>
<script setup lang="ts">
import i18n from "@/core/plugins/i18n";
import { ref, inject } from "vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import SalesCustomerDetailMobile from "./SalesCustomerDetailMobile.vue";
import SalesCustomerDetail from "../../views/SalesCustomerDetail.vue";
import CustomerDropDownMobile from "./CustomerDropDownMobile.vue";
const t = i18n.global.t;
const data = inject<any>("data");
const serviceMap = inject("serviceMap");
const isLoading = inject("isLoading");
const currentRole = inject("currentRole");
const getUserName = inject<any>("getUserName");
const accountUid = inject("accountUid");
const fetchData = inject("fetchData");
const criteria = inject("criteria");
const detailRef = ref<any>(null);

const showCustomerDetail = (item: any) => {
  detailRef.value?.show(item);
};

const getRoleType = (item: any) => {
  switch (item.role) {
    case AccountRoleTypes.Client:
      return { class: "badge badge-primary", label: t("fields.client") };
    case AccountRoleTypes.IB:
      return { class: "badge badge-success", label: t("fields.ib") };
    case AccountRoleTypes.Sales:
      return { class: "badge badge-danger", label: t("fields.sales") };
  }
};
</script>
