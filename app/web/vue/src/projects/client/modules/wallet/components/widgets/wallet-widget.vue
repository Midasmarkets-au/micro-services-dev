<template>
  <!--begin::Mixed Widget 12-->
  <div
    id="wallet_cards"
    :class="widgetClasses"
    class="card theme-dark-bg-body"
    :style="`border: 2px solid var(--background-${widgetColor})`"
  >
    <!--begin::Body-->
    <div class="card-body d-flex flex-column">
      <!--begin::Wrapper-->
      <div
        class="d-flex flex-row flex-grow-1"
        style="justify-content: space-between"
      >
        <a href="#" class="text-dark text-hover-primary fw-bold fs-3">{{
          title
        }}</a>

        <img
          :src="'/images/currency/' + currency + '.svg'"
          class="me-4 w-25px"
          style="border-radius: 4px"
          alt=""
        />
      </div>
      <!--end::Wrapper-->

      <!--begin::Stats-->
      <div class="pt-5 pb-3">
        <div class="text-dark fw-bold fs-1 me-2 lh-0">
          <BalanceShow
            :currency-id="Number(currency)"
            :balance="Number(number)"
          />
        </div>
      </div>
      <!--end::Stats-->
    </div>
    <!--end::Body-->
  </div>
  <!--end::Mixed Widget 12-->
</template>

<script lang="ts">
import { defineComponent } from "vue";
import BalanceShow from "@/components/BalanceShow.vue";

export default defineComponent({
  name: "widget-12",
  props: {
    widgetClasses: String,
    widgetColor: Number,
    chartHeight: String,
    title: String,
    number: String,
    currency: String,
  },
  components: {
    BalanceShow,
  },
  // eslint-disable-next-line @typescript-eslint/no-empty-function
  setup() {},
});
</script>

<style>
:root {
  --outline-0: #c9e2ff;
  --outline-1: #d0d7ff;
  --outline-2: #e3d7ff;
  --outline-3: #ffd4e5;
  --outline-4: #ffd2d2;
  --outline-5: #ffe3b4;
  --outline-6: #fff3cd;
  --outline-7: #d4edda;
  --outline-8: #d1ecf1;
  --outline-9: #d1ecf1;
  --outline-10: #f8f9fa;
  --background-0: #0d6efd;
  --background-1: #6610f2;
  --background-2: #6f42c1;
  --background-3: #d63384;
  --background-4: #dc3545;
  --background-5: #fd7e14;
  --background-6: #ffc107;
  --background-7: #198754;
  --background-8: #20c997;
  --background-9: #0dcaf0;
  --background-10: #6c757d;
}
#wallet_cards {
  box-shadow: rgba(0, 0, 0, 0.16) 0px 1px 4px;
}

#wallet_cards:hover {
  cursor: pointer;
  box-shadow: rgba(0, 0, 0, 0.24) 0px 3px 8px;
}
</style>
