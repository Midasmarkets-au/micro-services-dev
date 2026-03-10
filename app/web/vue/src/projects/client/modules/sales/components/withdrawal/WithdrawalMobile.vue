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

  <div v-else class="card mt-2">
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
      @click="withdrawalDetailMobileRef?.show(item)"
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
            {{ item.source.displayNumber }}
            <TinyCopyBox
              class="ms-1"
              :val="item.source.displayNumber.toString()"
            ></TinyCopyBox>
          </div>
        </div>
        <div class="text-end">
          <div>
            <span
              class="badge"
              :class="`badge-${
                {
                  [TransactionStateType.WithdrawalCreated]: 'primary',
                  [TransactionStateType.WithdrawalCompleted]: 'success',
                  [TransactionStateType.WithdrawalTenantRejected]: 'danger',
                  [TransactionStateType.WithdrawalTenantApproved]: 'warning',
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
  <WithdrawalDetailMobile ref="withdrawalDetailMobileRef" />
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import { TransactionStateType } from "@/core/types/StateInfos";
import TinyCopyBox from "@/components/TinyCopyBox.vue";
import WithdrawalDetailMobile from "./WithdrawalDetailMobile.vue";

const withdrawalDetailMobileRef = ref<any>(null);
const filterRef = inject<any>("filterRef");
const isSelectingClient = inject<boolean>("isSelectingClient");
const onIsSelectingClientChange = inject<any>("onIsSelectingClientChange");
</script>
<style scoped>
.badge {
  --bs-badge-font-size: 0.55rem !important;
}
</style>
