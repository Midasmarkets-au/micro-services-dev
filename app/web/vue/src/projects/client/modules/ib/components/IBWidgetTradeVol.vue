<template>
  <div class="card h-100" v-if="!isMobile">
    <div class="card-header border-0">
      <div class="card-title-noicon">
        <span class="rebate-svg">
          <!-- <svg
            width="12"
            height="12"
            viewBox="0 0 12 12"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M1.86108 4.05077H2.01747C2.61957 4.05077 3.11219 4.54339 3.11219 5.14549V10.6191C3.11219 11.2212 2.61957 11.7138 2.01747 11.7138H1.86108C1.25898 11.7138 0.766357 11.2212 0.766357 10.6191V5.14549C0.766357 4.54339 1.25898 4.05077 1.86108 4.05077ZM6.23997 0.766602C6.84207 0.766602 7.33469 1.25923 7.33469 1.86132V10.6191C7.33469 11.2212 6.84207 11.7138 6.23997 11.7138C5.63787 11.7138 5.14525 11.2212 5.14525 10.6191V1.86132C5.14525 1.25923 5.63787 0.766602 6.23997 0.766602ZM10.6189 7.02216C11.221 7.02216 11.7136 7.51478 11.7136 8.11688V10.6191C11.7136 11.2212 11.221 11.7138 10.6189 11.7138C10.0168 11.7138 9.52414 11.2212 9.52414 10.6191V8.11688C9.52414 7.51478 10.0168 7.02216 10.6189 7.02216Z"
              fill="white"
            />
          </svg> -->
          <SvgIcon name="trade" path="ibCenter" />
        </span>
        <h4 class="rebate-title">{{ $t("title.todayTradeVol") }}</h4>
      </div>
    </div>
    <div class="card-body pt-3">
      <div class="tab-pane fade show active" id="today-rebate">
        <div class="rebate-content">
          <div class="rebate-amount">
            <h1 class="fw-semibold balance-show">{{ todayTrdVol / 100 }}</h1>
            <router-link to="/ib/trade" class="view-more" v-if="false">
              {{ $t("action.viewMore") }}
              <inline-svg
                src="/images/icons/general/show_more.svg"
                class="ml-2"
              ></inline-svg>
            </router-link>
          </div>
          <span class="">
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
    </div>
  </div>
  <div
    v-else
    class="text-center d-flex flex-column align-items-center justify-content-center"
  >
    <img
      src="/images/icons/ibCenter/rebate.svg"
      style="width: 35px; height: 35px"
    />
    <h1>{{ todayTrdVol / 100 }}</h1>
    <router-link to="/ib/trade"
      ><p style="color: #717171">Trade Volume</p></router-link
    >
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useStore } from "@/store";
import IbReportService from "../services/IbReportService";
import { isMobile } from "@/core/config/WindowConfig";

const store = useStore();
const agentAccount = computed(() => store.state.AgentModule.agentAccount);

const todayTrdVol = ref(0);

onMounted(async () => {
  const res = await IbReportService.getTradeTodayVolume();
  todayTrdVol.value = res.volume;
});
</script>
