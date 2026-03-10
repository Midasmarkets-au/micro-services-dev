<template>
  <div class="card account-card b-0 mx-auto">
    <div
      class="card-header account-card-header align-items-center border-0 ps-5 pt-5 pe-4"
      :class="{
        'account-real-card': props.isReal,
        'account-demo-card': !props.isReal,
      }"
    >
      <h3 class="card-title align-items-start flex-column">
        <div v-if="props.isReal && props.account?.hasTradeAccount">
          <span class="badge badge-warning fw-bold fs-7 p-2 me-2 fst-italic">
            {{
              props.serviceMap[props.account?.tradeAccount?.serviceId]
                ?.serverName
            }}
          </span>
          <span class="badge badge-success fw-bold fs-7 p-2 me-2 fst-italic">
            {{
              props.serviceMap[props.account?.tradeAccount?.serviceId]
                ?.platformName
            }}
          </span>
        </div>
        <div v-if="props.isReal && !props.account?.hasTradeAccount">
          <span class="badge badge-warning fw-bold fs-7 p-2 me-2 fst-italic">
            {{
              props.serviceMap[props.account?.supplement?.serviceId]?.serverName
            }}
          </span>
          <span class="badge badge-success fw-bold fs-7 p-2 me-2 fst-italic">
            {{
              props.serviceMap[props.account?.supplement?.serviceId]
                ?.platformName
            }}
          </span>
        </div>
        <div v-if="!props.isReal">
          <span class="badge badge-warning fw-bold fs-7 p-2 me-2 fst-italic">
            {{ props.serviceMap[props.account?.serviceId]?.serverName }}
          </span>
          <span class="badge badge-success fw-bold fs-7 p-2 me-2 fst-italic">
            {{ props.serviceMap[props.account?.serviceId]?.platformName }}
          </span>
        </div>
      </h3>
      <div class="card-toolbar">
        <!--begin::Menu-->
        <!-- <button
          type="button"
          class="btn btn-sm btn-icon btn-color-primary"
          data-kt-menu-trigger="click"
          data-kt-menu-placement="bottom-end"
          data-kt-menu-flip="top-end"
        >
          <span class="svg-icon svg-icon-2">
            <inline-svg src="/images/icons/general/gen063.svg" />
          </span>
        </button> -->
        <!-- <Dropdown1></Dropdown1> -->
        <!--end::Menu-->
      </div>
      <div
        class="account-balance d-block pt-1 ps-2 mb-4"
        v-if="props.account.hasTradeAccount"
      >
        <span class="text-black text-white fs-5 d-block">
          {{ $t("fields.balance") }}
        </span>
        <div class="text-white fs-1">
          <BalanceShow
            :balance="props.fields.tradeAccount?.balanceInCents"
            :currency-id="props.fields.tradeAccount?.currencyId"
          />
        </div>
      </div>
      <div v-else class="account-balance d-block pt-1 ps-2 mb-4">
        <span class="text-black text-white fs-5 d-block"> </span>
        <div class="text-white fs-1">Pedding</div>
      </div>
      <div class="account-currency">
        <div
          class="symbol symbol-circle symbol-25px"
          v-if="props.isReal && props.account.hasTradeAccount"
        >
          <img
            :src="
              '/images/currency/' +
              props.fields.tradeAccount?.currencyId +
              '.svg'
            "
            class="me-2 w-25px"
            style="border-radius: 50%"
          />
        </div>
        <div
          class="symbol symbol-circle symbol-25px"
          v-if="props.isReal && !props.account.hasTradeAccount"
        >
          <img
            :src="
              '/images/currency/' +
              props.account.supplement?.currencyId +
              '.svg'
            "
            class="me-2 w-25px"
            style="border-radius: 50%"
          />
        </div>
        <div class="symbol symbol-circle symbol-25px" v-if="!props.isReal">
          <img
            :src="'/images/currency/' + props.account.currencyId + '.svg'"
            class="me-2 w-25px"
            style="border-radius: 50%"
          />
        </div>
      </div>
    </div>
    <div class="card-body p-4">
      <div class="row mt-5">
        <div class="col-7">
          <div
            v-if="props.isReal && props.account.hasTradeAccount"
            class="fs-4"
          >
            <CopyBox
              :val="props.fields.tradeAccount?.accountNumber.toString()"
              :hasAni="true"
              :hasIcon="true"
            ></CopyBox>
          </div>
          <div
            v-if="props.isReal && !props.account.hasTradeAccount"
            class="fs-4"
          >
            ********
          </div>
          <div v-if="!props.isReal" class="fs-4">
            <CopyBox
              :val="props.fields.accountNumber.toString()"
              :hasAni="true"
              :hasIcon="true"
            ></CopyBox>
          </div>
          <div class="text-uppercase fs-8 text-gray-600">account number</div>
        </div>
        <div class="col-5 text-end">
          <div>500:1</div>
          <div class="text-uppercase fs-8 text-gray-600">Leverage</div>
        </div>
      </div>
      <div class="row mt-5">
        <div class="col-7">
          <div v-if="props.account.hasTradeAccount">
            {{ $t(`type.account.${props.account.type}`) }}
          </div>
          <div v-else>
            {{ $t(`type.account.${props.account.supplement?.accountType}`) }}
          </div>
          <div class="text-uppercase fs-8 text-gray-600">Type</div>
        </div>
        <div class="col-5 text-end"></div>
      </div>
    </div>
    <div class="card-footer p-1 text-end">
      <router-link
        v-if="isReal && props.account.hasTradeAccount"
        class="d-inline-block text-uppercase border-0 bg-light fs-6 px-6 py-2 rounded-3 bg-secondary bg-hover-light"
        style="color: #00254d"
        :to="'/account/' + props.account.tradefields.accountNumber"
      >
        Detail
      </router-link>
      <button
        v-if="isReal && !props.account.hasTradeAccount"
        class="text-uppercase border-0 bg-none fs-6 px-6 py-2 rounded-3"
        disabled
        style="color: #00254d"
      >
        Pedding...
      </button>
      <button
        v-if="!isReal"
        class="text-uppercase border-0 bg-none fs-6 px-6 py-2 rounded-3"
        disabled
        style="color: #00254d"
      >
        Demo
      </button>
    </div>
  </div>
</template>
<script setup lang="ts">
import { defineProps } from "vue";
import BalanceShow from "@/components/BalanceShow.vue";
import CopyBox from "@/components/CopyBox.vue";

const props = defineProps<{
  account: object;
  isReal: boolean;
  serviceMap: object;
}>();
</script>
<style scoped>
.account-card {
  width: 294px;
  height: 344px;
}

.account-card-header {
  width: 294px;
  /* display: block; */
  /* min-height: 70px; */
  height: 148px;
}
.account-balance {
  min-width: 120px;
}
.account-real-card {
  background-size: cover;
  background-image: url("/images/bg/account-real-bg.png");
  background-repeat: no-repeat;
}

.account-demo-card {
  background-size: cover;
  background-image: url("/images/bg/account-demo-bg.png");
  background-repeat: no-repeat;
}

.account-currency {
  position: absolute;
  top: 124px;
  right: 123px;
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: rgba(253, 202, 31, 0.2);
  padding: 12px 12px;
}
</style>
