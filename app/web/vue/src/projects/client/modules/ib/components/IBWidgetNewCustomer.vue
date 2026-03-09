<template>
  <div class="card h-100" v-if="!isMobile">
    <div class="card-header border-0">
      <div class="card-title-noicon">
        <span class="rebate-svg">
          <!-- <svg
            width="16"
            height="17"
            viewBox="0 0 16 17"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <g clip-path="url(#clip0_162_4050)">
              <path
                fill-rule="evenodd"
                clip-rule="evenodd"
                d="M11.1132 9.00293C12.0265 9.62293 12.6665 10.4629 12.6665 11.5829V13.5829H14.6665C15.0332 13.5829 15.3332 13.2829 15.3332 12.9163V11.5829C15.3332 10.1296 12.9532 9.2696 11.1132 9.00293Z"
                fill="white"
              />
              <path
                d="M5.99992 8.24984C7.47268 8.24984 8.66659 7.05593 8.66659 5.58317C8.66659 4.11041 7.47268 2.9165 5.99992 2.9165C4.52716 2.9165 3.33325 4.11041 3.33325 5.58317C3.33325 7.05593 4.52716 8.24984 5.99992 8.24984Z"
                fill="white"
              />
              <path
                fill-rule="evenodd"
                clip-rule="evenodd"
                d="M9.99995 8.24984C11.4733 8.24984 12.6666 7.0565 12.6666 5.58317C12.6666 4.10984 11.4733 2.9165 9.99995 2.9165C9.68661 2.9165 9.39328 2.98317 9.11328 3.0765C9.66661 3.76317 9.99995 4.6365 9.99995 5.58317C9.99995 6.52984 9.66661 7.40317 9.11328 8.08984C9.39328 8.18317 9.68661 8.24984 9.99995 8.24984Z"
                fill="white"
              />
              <path
                fill-rule="evenodd"
                clip-rule="evenodd"
                d="M5.99996 8.9165C4.21996 8.9165 0.666626 9.80984 0.666626 11.5832V12.9165C0.666626 13.2832 0.966626 13.5832 1.33329 13.5832H10.6666C11.0333 13.5832 11.3333 13.2832 11.3333 12.9165V11.5832C11.3333 9.80984 7.77996 8.9165 5.99996 8.9165Z"
                fill="white"
              />
            </g>
            <defs>
              <clipPath id="clip0_162_4050">
                <rect
                  width="16"
                  height="16"
                  fill="white"
                  transform="translate(0 0.25)"
                />
              </clipPath>
            </defs>
          </svg> -->
          <SvgIcon name="customer" path="ibCenter" />
        </span>
        <h4 class="rebate-title">{{ $t("title.todayNewCustomer") }}</h4>
      </div>
    </div>
    <div class="card-body pt-3">
      <div class="tab-pane fade show active" id="today-rebate">
        <div class="rebate-content">
          <div class="rebate-amount">
            <h1 class="fw-semibold mb-6 balance-show">
              + {{ todayNewCustomers }}
            </h1>
            <router-link to="/ib/new-customers" class="view-more">
              {{ $t("action.viewMore") }}
              <inline-svg
                src="/images/icons/general/show_more.svg"
                class="ml-2"
              ></inline-svg>
            </router-link>
          </div>
          <span class="">
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
  <div v-else>
    <div
      class="text-center d-flex flex-column align-items-center justify-content-center"
    >
      <img
        src="/images/ibCenter/customer.svg"
        style="width: 30px; height: 22px; margin-bottom: 4px"
      />
      <h1>+ {{ todayNewCustomers }}</h1>
      <router-link to="/ib/new-customers"
        ><p style="color: #717171">New Customers</p></router-link
      >
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { RouterLink } from "vue-router";
import { useStore } from "@/store";
import { isMobile } from "@/core/config/WindowConfig";
import IbReportService from "../services/IbReportService";

const todayNewCustomers = ref(0);
const store = useStore();

onMounted(async () => {
  const res = await IbReportService.getTodayAccountCreationCount();
  todayNewCustomers.value = res.count;
});
</script>
