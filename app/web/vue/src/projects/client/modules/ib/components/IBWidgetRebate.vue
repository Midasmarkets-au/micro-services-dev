<template>
  <div class="card h-100">
    <div class="card-header border-0">
      <div class="card-title-noicon">
        <span class="rebate-svg">
          <!-- <svg
            width="16"
            height="15"
            viewBox="0 0 16 15"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M14.6466 11.8724V12.6543C14.6466 13.5145 13.9428 14.2183 13.0827 14.2183H2.13532C1.26735 14.2183 0.571411 13.5145 0.571411 12.6543V1.70698C0.571411 0.846826 1.26735 0.143066 2.13532 0.143066H13.0827C13.9428 0.143066 14.6466 0.846826 14.6466 1.70698V2.48893H7.609C6.74103 2.48893 6.0451 3.19269 6.0451 4.05284V10.3085C6.0451 11.1686 6.74103 11.8724 7.609 11.8724H14.6466ZM7.609 10.3085H15.4286V4.05284H7.609V10.3085ZM10.7368 8.35359C10.0878 8.35359 9.56389 7.82968 9.56389 7.18066C9.56389 6.53164 10.0878 6.00773 10.7368 6.00773C11.3858 6.00773 11.9098 6.53164 11.9098 7.18066C11.9098 7.82968 11.3858 8.35359 10.7368 8.35359Z"
              fill="white"
            />
          </svg> -->
          <SvgIcon name="rebate" path="ibCenter" />
        </span>
        <h4 class="rebate-title">{{ $t("title.rebate") }}</h4>
      </div>
      <div class="card-toolbar">
        <div class="nav account-tabs nav-pills nav-pills-custom">
          <button
            class="account-tab nav-item active"
            data-bs-toggle="pill"
            href="#today-rebate"
          >
            {{ $t("fields.today") }}
          </button>
          <button
            class="account-tab nav-item"
            data-bs-toggle="pill"
            href="#all-rebate"
          >
            {{ $t("fields.all") }}
          </button>
        </div>
      </div>
    </div>
    <div class="card-body pt-3 d-flex justify-content-between align-center">
      <div class="tab-content">
        <div class="tab-pane fade show active" id="today-rebate">
          <h1 class="fw-semibold mb-6 balance-show">
            {{ isPositive }}
            <BalanceShow
              :balance="rebateTodayAmount"
              :currency-id="currencyId"
            />
          </h1>
          <router-link to="/ib/rebate" class="view-more">
            {{ $t("action.viewMore") }}
            <inline-svg
              src="/images/icons/general/show_more.svg"
              class="ml-2"
            ></inline-svg>
          </router-link>
        </div>
        <div class="tab-pane fade" id="all-rebate">
          <!-- style="
              background: linear-gradient(to right, #e82a6c, #f99f1d);
              -webkit-background-clip: text;
              -webkit-text-fill-color: transparent;
            "-->
          <h1 class="fw-semibold mb-6 balance-show">
            <BalanceShow :balance="rebateAllAmount" />
          </h1>
          <router-link to="/ib/rebate" class="view-more">
            {{ $t("action.viewMore") }}
            <inline-svg
              src="/images/icons/general/show_more.svg"
              class="ml-2"
            ></inline-svg>
          </router-link>
        </div>
      </div>

      <span class="pt-3">
        <svg
          width="62"
          height="32"
          viewBox="0 0 62 32"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            d="M1.5 29.4541C1.5 29.4541 7.64585 31.2449 13.7916 29.4541C28.1144 25.2805 23.5 -1.25 34.5 5.74999C47.6784 14.1363 58.1562 13.6888 60 1.75"
            stroke="url(#paint0_linear_322_5193)"
            stroke-width="3"
            stroke-linecap="round"
            stroke-linejoin="round"
          />
          <defs>
            <linearGradient
              id="paint0_linear_322_5193"
              x1="-35.5"
              y1="37.75"
              x2="58.4452"
              y2="-17.1601"
              gradientUnits="userSpaceOnUse"
            >
              <stop stop-color="#FFD400" />
              <stop offset="1" stop-color="#FFD400" stop-opacity="0" />
            </linearGradient>
          </defs>
        </svg>
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import BalanceShow from "@/components/BalanceShow.vue";
import { ref, onMounted, computed } from "vue";
import { RouterLink } from "vue-router";
import IbReportService from "../services/IbReportService";
import { useStore } from "@/store";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import { TimeZoneService } from "@/core/plugins/TimerService";
import SvgIcon from "@/projects/client/components/SvgIcon.vue";

const rebateTodayAmount = ref(0);
const rebateAllAmount = ref(0);
const currencyId = ref(CurrencyTypes.USD);
const isPositive = ref("+");
const store = useStore();
const agentAccount = computed(() => store.state.AgentModule.agentAccount);
const timeZoneOffsetInHour = TimeZoneService.getTimeZoneOffsetInHours();

onMounted(async () => {
  const res = await IbReportService.getRebateTodayValue(timeZoneOffsetInHour);
  if (res.length > 0) {
    currencyId.value = res[0].currencyId;
    rebateTodayAmount.value = res.reduce(
      (acc: number, cur: any) => acc + cur.amount,
      0
    );
    if (rebateTodayAmount.value < 0) {
      isPositive.value = "";
    }
  }

  const res2 = await IbReportService.getRebateTotalValue();
  if (res2.length > 0) {
    rebateAllAmount.value = res2.reduce(
      (acc: number, cur: any) => acc + cur.amount,
      0
    );
  }
});
</script>

<style scoped lang="scss">
.account-tabs {
  background-color: #fafbfd;
  border-radius: 8px;
  padding: 1px;
  border: 1px solid #f2f4f7;
}
.account-tabs .account-tab {
  padding: 8px 30px;
  border-radius: 8px;
  cursor: pointer;
  border: 0px;
  background: none;
  font-size: 14px;
}
.account-tabs .active {
  background-color: #f2f4f7;
  &::after {
    background: none;
  }
}
</style>
