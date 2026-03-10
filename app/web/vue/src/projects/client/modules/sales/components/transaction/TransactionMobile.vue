<template>
  <div v-if="tradeFilterRef?.isLoading" class="d-flex justify-content-center">
    <LoadingRing />
  </div>
  <div
    v-else-if="!tradeFilterRef?.isLoading && tradeFilterRef?.data.length == 0"
    class="d-flex justify-content-center card mt-2"
  >
    <div class="d-flex justify-content-center">
      <NoDataBox />
    </div>
  </div>
  <div v-else class="card mt-2">
    <div class="d-flex justify-content-between align-items-center mt-3">
      <div>
        <div class="fw-semibold fs-7">
          {{
            $t("action.showing") +
            " " +
            (tradeFilterRef?.criteria.total ?? "0") +
            " " +
            $t("title.results")
          }}
        </div>
        <div class="d-flex">
          <div class="me-2">{{ $t("title.total") }}:</div>

          <BalanceShow
            :balance="tradeFilterRef?.criteria.totalAmount"
            :currency="tradeFilterRef?.data[0]?.currencyId"
          />
        </div>
      </div>
      <el-switch
        class="me-50px"
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
      v-for="(item, index) in tradeFilterRef?.getData()"
      :key="index"
      class="mt-3"
      @click="transactionDetailMobileRef?.show(item)"
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
        </div>
        <div class="text-end">
          <div>
            <span
              class="badge"
              :class="`badge-${
                {
                  [TransactionStateType.TransferCreated]: 'primary',
                  [TransactionStateType.TransferCompleted]: 'success',
                  [TransactionStateType.TransferRejected]: 'danger',
                  [TransactionStateType.TransferApproved]: 'warning',
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

  <TransactionDetailMobile ref="transactionDetailMobileRef" />
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import { TransactionStateType } from "@/core/types/StateInfos";
import TransactionDetailMobile from "./TransactionDetailMobile.vue";

const transactionDetailMobileRef = ref<any>(null);
const tradeFilterRef = inject<any>("tradeFilterRef");
const isSelectingClient = inject<boolean>("isSelectingClient");
const onIsSelectingClientChange = inject<any>("onIsSelectingClientChange");
</script>
<style scoped>
.badge {
  --bs-badge-font-size: 0.55rem !important;
}
</style>
