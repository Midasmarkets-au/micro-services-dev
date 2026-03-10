<template>
  <div class="card h-100" v-if="!isMobile">
    <div class="card-header border-0">
      <div class="card-title-noicon">
        <span class="rebate-svg">
          <!-- <svg
            width="10"
            height="17"
            viewBox="0 0 10 17"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M5.35016 7.34514C3.31143 6.81525 2.6558 6.2674 2.6558 5.41419C2.6558 4.43524 3.5629 3.75267 5.08073 3.75267C6.67938 3.75267 7.27214 4.51607 7.32603 5.63872H9.31087C9.248 4.09395 8.30498 2.67492 6.42791 2.21688V0.25H3.73355V2.18994C1.99119 2.56715 0.590128 3.69878 0.590128 5.43215C0.590128 7.5068 2.30554 8.53964 4.81129 9.14138C7.05659 9.68025 7.50565 10.4706 7.50565 11.3059C7.50565 11.9256 7.06557 12.9135 5.08073 12.9135C3.2306 12.9135 2.50312 12.0872 2.40433 11.0274H0.428467C0.536241 12.9943 2.00916 14.099 3.73355 14.4672V16.4161H6.42791V14.4852C8.17924 14.1529 9.57132 13.138 9.57132 11.2969C9.57132 8.74621 7.38889 7.87503 5.35016 7.34514Z"
              fill="white"
            />
          </svg> -->
          <SvgIcon name="funding" path="ibCenter" />
        </span>
        <h4 class="rebate-title">{{ $t("title.todayFunding") }}</h4>
      </div>
    </div>
    <div class="card-body pt-3">
      <div class="tab-pane fade show active" id="today-rebate">
        <div class="rebate-content">
          <div class="rebate-amount">
            <h1 class="fw-semibold balance-show">
              <div v-for="(todayFund, index) in todayFundings" :key="index">
                {{ todayFund.amount > 0 ? "+" : "" }}
                <BalanceShow
                  :balance="todayFund.amount"
                  :currency-id="todayFund.currencyId"
                />
              </div>
            </h1>
            <router-link to="/ib/trade" class="view-more" v-if="false">
              {{ $t("action.viewMore") }}
              <inline-svg
                src="/images/icons/general/show_more.svg"
                class="ml-2"
              ></inline-svg>
            </router-link>
          </div>
          <div
            class=""
            style="
              position: relative;
              width: 100px;
              height: 50px;
              overflow: hidden;
            "
          >
            <span class="" style="position: absolute; top: 0px; left: -72px">
              <svg
                width="174"
                height="47"
                viewBox="0 0 174 47"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
              >
                <rect
                  x="117.668"
                  y="47"
                  width="47"
                  height="5.33287"
                  rx="2.66644"
                  transform="rotate(-90 117.668 47)"
                  fill="#E9EDF7"
                />
                <rect
                  x="117.668"
                  y="47"
                  width="38.6846"
                  height="5.33288"
                  rx="2.66644"
                  transform="rotate(-90 117.668 47)"
                  fill="url(#paint0_linear_162_4080)"
                />
                <rect
                  x="132.001"
                  y="47"
                  width="47"
                  height="5.33286"
                  rx="2.66643"
                  transform="rotate(-90 132.001 47)"
                  fill="#E9EDF7"
                />
                <rect
                  x="132.001"
                  y="47"
                  width="32.5385"
                  height="5.33287"
                  rx="2.66643"
                  transform="rotate(-90 132.001 47)"
                  fill="url(#paint1_linear_162_4080)"
                />
                <rect
                  x="146.334"
                  y="47"
                  width="47"
                  height="5.33281"
                  rx="2.66641"
                  transform="rotate(-90 146.334 47)"
                  fill="#E9EDF7"
                />
                <rect
                  x="146.334"
                  y="47"
                  width="42.6615"
                  height="5.33287"
                  rx="2.66643"
                  transform="rotate(-90 146.334 47)"
                  fill="url(#paint2_linear_162_4080)"
                />
                <rect
                  x="160.667"
                  y="47"
                  width="47"
                  height="5.33286"
                  rx="2.66643"
                  transform="rotate(-90 160.667 47)"
                  fill="#E9EDF7"
                />
                <rect
                  x="160.667"
                  y="47"
                  width="11.2077"
                  height="5.33286"
                  rx="2.66643"
                  transform="rotate(-90 160.667 47)"
                  fill="url(#paint3_linear_162_4080)"
                />
                <defs>
                  <linearGradient
                    id="paint0_linear_162_4080"
                    x1="117.668"
                    y1="47"
                    x2="119.111"
                    y2="57.4668"
                    gradientUnits="userSpaceOnUse"
                  >
                    <stop stop-color="#FFEF9F" />
                    <stop offset="1" stop-color="#FEB228" />
                  </linearGradient>
                  <linearGradient
                    id="paint1_linear_162_4080"
                    x1="132.001"
                    y1="47"
                    x2="133.704"
                    y2="57.3867"
                    gradientUnits="userSpaceOnUse"
                  >
                    <stop stop-color="#FFEF9F" />
                    <stop offset="1" stop-color="#FEB228" />
                  </linearGradient>
                  <linearGradient
                    id="paint2_linear_162_4080"
                    x1="146.334"
                    y1="47"
                    x2="147.647"
                    y2="57.5016"
                    gradientUnits="userSpaceOnUse"
                  >
                    <stop stop-color="#FFEF9F" />
                    <stop offset="1" stop-color="#FEB228" />
                  </linearGradient>
                  <linearGradient
                    id="paint3_linear_162_4080"
                    x1="160.667"
                    y1="47"
                    x2="164.805"
                    y2="55.6967"
                    gradientUnits="userSpaceOnUse"
                  >
                    <stop stop-color="#FFEF9F" />
                    <stop offset="1" stop-color="#FEB228" />
                  </linearGradient>
                </defs>
              </svg>
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div
    v-else
    class="text-center d-flex flex-column align-items-center justify-content-center"
  >
    <img
      src="/images/icons/ibCenter/funding.svg"
      style="width: 35px; height: 35px"
    />
    <h1>
      <div v-for="(todayFund, index) in todayFundings" :key="index">
        {{ todayFund.amount > 0 ? "+" : "" }}
        <BalanceShow
          :balance="todayFund.amount"
          :currency-id="todayFund.currencyId"
        />
      </div>
    </h1>
    <router-link to="/ib/trade"
      ><p style="color: #717171">Funding</p></router-link
    >
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { RouterLink } from "vue-router";
import BalanceShow from "@/components/BalanceShow.vue";
import { useStore } from "@/store";
import IbReportService from "../services/IbReportService";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import { isMobile } from "@/core/config/WindowConfig";
import SvgIcon from "@/projects/client/components/SvgIcon.vue";

const store = useStore();
const agentAccount = computed(() => store.state.AgentModule.agentAccount);
const isPositive = ref("+");
const currencyId = ref(CurrencyTypes.USD);
interface Funding {
  amount: number;
  currencyId: number;
}
const todayFundings = ref<Funding[]>([]);

onMounted(async () => {
  const res = await IbReportService.getDepositTodayValue();
  console.log("res", res);
  todayFundings.value = res;
  // if (res.length > 0) {
  //   currencyId.value = res[0].currencyId;
  //   todayFundings.value = res.reduce(
  //     (acc: number, cur: any) => acc + cur.amount,
  //     0
  //   );
  //   if (todayFundings.value < 0) {
  //     isPositive.value = "";
  //   }
  // }
});
</script>
