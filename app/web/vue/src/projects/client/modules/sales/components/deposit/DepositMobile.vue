<template>
  <div v-if="filterRef?.isLoading" class="d-flex justify-content-center">
    <LoadingRing />
  </div>
  <div
    v-else-if="!filterRef?.isLoading && filterRef?.data.length == 0"
    class="d-flex justify-content-center card mt-2"
  >
    <div class="d-flex justify-content-center">
      <NoDataBox />
    </div>
  </div>

  <div v-else class="card mt-2 p-3">
    <div class="d-flex justify-content-between align-items-center">
      <div>
        <div class="fw-semibold fs-7">
          {{
            $t("action.showing") +
            " " +
            (filterRef?.criteria.total ?? "0") +
            " " +
            $t("title.results")
          }}
        </div>
        <div class="d-flex">
          <div class="me-2">{{ $t("title.total") }}:</div>
          <BalanceShow
            :balance="filterRef?.criteria.totalAmount"
            :currency="filterRef?.data[0]?.currencyId"
          />
        </div>
      </div>

      <el-switch
        v-model="isSelectingClient"
        @change="onIsSelectingClientChange"
        width="65"
        size="large"
        inline-prompt
        style="--el-switch-on-color: #0053ad; --el-switch-off-color: #ffc420"
        :active-text="$t('fields.client')"
        :inactive-text="$t('fields.ib')"
      />
    </div>

    <el-card
      v-for="(item, index) in filterRef?.getData()"
      :key="index"
      class="mt-3"
      @click="depositDetailMobileRef?.show(item)"
    >
      <div class="d-flex justify-content-between align-items-center">
        <div>
          <span>
            {{
              item.user.nativeName ||
              item.user.displayName ||
              `${item.user.firstName} ${item.user.lastNameName}`
            }}
          </span>
          <div>
            {{ item.user.email }}
          </div>

          <div>
            {{ $t("fields.target") }} -
            {{ item.targetTradeAccount.accountNumber }}
          </div>
        </div>
        <div class="text-end">
          <div>
            <span
              class="badge"
              :class="`badge-${
                {
                  [TransactionStateType.DepositCreated]: 'primary',
                  [TransactionStateType.DepositCompleted]: 'success',
                  [TransactionStateType.DepositTenantRejected]: 'danger',
                  [TransactionStateType.DepositTenantApproved]: 'warning',
                }[item.stateId] ?? 'info'
              }`"
              >{{
                $t(`type.transactionState.${item.stateId}`).replace(
                  /^deposit\s+/i,
                  ""
                )
              }}</span
            >
          </div>
          <BalanceShow :balance="item.amount" :currencyId="item.currencyId" />
          <div>
            <TimeShow
              :date-iso-string="item.createdOn"
              style="font-size: 12px; color: rgb(113, 113, 113)"
            />
          </div>
        </div>
      </div>
    </el-card>
  </div>

  <DepositDetailMobile ref="depositDetailMobileRef" />
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { TransactionStateType } from "@/core/types/StateInfos";
import DepositDetailMobile from "./DepositDetailMobile.vue";

const depositDetailMobileRef = ref<any>(null);
const filterRef = inject<any>("filterRef");
const isSelectingClient = ref(true);
const onIsSelectingClientChange = () => {
  //
  filterRef.value.criteria.role = isSelectingClient.value
    ? AccountRoleTypes.Client
    : AccountRoleTypes.IB;
  filterRef.value?.fetchData(1);
};
</script>
<style scoped>
.badge {
  --bs-badge-font-size: 0.55rem !important;
}
</style>
