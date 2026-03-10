<template>
  <div class="card mb-5 mb-xl-8">
    <div class="card-header border-0">
      <div class="card-title">
        <h3 class="fw-bold m-0">
          {{ $t("account.name") }}: {{ props.fields.accountNumber }} ({{
            $t("fields.platforms." + props.account.serviceId)
          }})
        </h3>
      </div>
    </div>
    <!--begin::Card body-->
    <div class="card-body">
      <div>
        <CurrencyBadge :currency="props.account?.currencyId" />

        {{ $t("account.balance") }}: ${{
          item?.tradeAccountStatus?.balance ?? "--"
        }}
        {{ $t("fields.leverage") }}:
        {{ item?.tradeAccountStatus?.leverage ?? "--" }}:1
        <button class="btn btn-primary" @click="showDetail">
          {{ $t("action.show_detail") }}
        </button>
      </div>
      <div v-if="isShowDetail">
        {{ $t("account.volume") }}:
        {{ item?.tradeAccountStatus?.equity ?? " -- " }}
        {{ $t("account.lots") }} {{ $t("account.profit") }}:
        {{ item?.tradeAccountStatus?.balance ?? "--" }}:1
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import { useStore } from "@/store";
import ErrorMsg from "@/components/ErrorMsg";
import CurrencyBadge from "@/components/CurrencyBadge";
import { AccountService as svc } from "../services/AccountService";
import { TradeAccount } from "../model/TradeAccount";
import filters from "@/core/helpers/filters";

const props = defineProps<{
  account: TradeAccount;
}>();
const isLoading = ref(true);
const isShowDetail = ref(false);
const item = ref<TradeAccount>();
const user = useStore().state.AuthModule.user;
function toCurrency(amount: number, currencyId: number): string {
  return filters.toCurrency(amount, currencyId, user.language);
}

function toDateTime(date): string {
  return filters.toDateTime(date, user.language);
}
const showDetail = async () => {
  isShowDetail.value = true;
  isLoading.value = true;
  svc
    .getTradeAccount(props.account.id)
    .then((data) => {
      item.value = data;
      isLoading.value = false;
    })
    .catch(({ response }) => {
      ErrorMsg.show(response);
      isLoading.value = false;
    });
};
</script>
