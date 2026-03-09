<template>
  <div class="flex-lg-row-fluid ms-lg-15">
    <ul
      class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
    >
      <!--begin:::Tab item-->
      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4 active"
          data-bs-toggle="tab"
          href="#kt_customer_view_overview_tab"
          >Overview</a
        >
      </li>
    </ul>
    <LiveAccountDetail
      :account="item"
      v-for="(item, index) in items"
      :key="index"
    ></LiveAccountDetail>
  </div>
</template>
<script lang="ts" setup>
import LiveAccountDetail from "./LiveAccountDetail.vue";
import { ref, onMounted } from "vue";
import ErrorMsg from "@/components/ErrorMsg";
import { AccountService as svc } from "../services/AccountService";
import { TradeAccount, TradeAccountCriteria } from "../model/TradeAccount";
const isLoading = ref(true);
const items = ref<TradeAccount[]>([]);
const criteria = ref<TradeAccountCriteria>({} as TradeAccountCriteria);

onMounted(async () => {
  svc
    .queryTradeAccounts(criteria.value)
    .then((res) => {
      items.value = res.data;
      criteria.value = res.criteria;
    })
    .catch(({ response }) => {
      ErrorMsg.show(response);
      isLoading.value = false;
    });
});
</script>
