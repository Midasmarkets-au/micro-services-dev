<template>
  <div v-if="showPaypal">
    <div
      class="mb-4 d-flex align-items-center gap-2"
      style="color: #409eff; font-size: 16px"
    >
      {{ $t("action.redirectToPayPal") }} <el-icon><Bottom /></el-icon>
    </div>
    <div id="paypal-button-container"></div>
  </div>
  <div v-else>
    <div class="d-flex align-items-center">
      <div class="me-7">
        <img src="/images/walletSuccess.png" alt="" />
      </div>
      <div v-if="!isDirectSuccess">
        <h4 class="mb-2">{{ $t("tip.orderCreated") }}</h4>
        <div>{{ $t("tip.depositSuccessTip") }}</div>
      </div>
      <div>
        <h4 style="color: #67c23a">{{ $t("tip.paymentSuccess") }}</h4>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { loadScript } from "@paypal/paypal-js";
import clientGlobalService from "@/projects/client/services/ClientGlobalService";
import { Bottom } from "@element-plus/icons-vue";
let paypal;

const showPaypal = ref(true);
const isDirectSuccess = ref(true);
const props = defineProps<{
  form: any;
  currency: string;
}>();

const loadPaypal = async () => {
  try {
    paypal = await loadScript({
      clientId:
        "AcMexhbeCXd8nI-2azk4FTOjSf52rX-dYHo_uEjoovrDyCIrkEi2ZOdHlInBtIESrBQOD1KXfYwi4TMU",
      currency: props.currency,
      disableFunding: ["card", "credit"],
      // enableFunding: ["credit"],
    });
  } catch (error) {
    console.error("failed to load the PayPal JS SDK script", error);
  }

  if (paypal) {
    try {
      await paypal
        .Buttons({
          async createOrder() {
            return props.form.id;
          },
          fundingSource: paypal.FUNDING.PAYPAL,

          onApprove: async (data, actions) => {
            showPaypal.value = false;
            try {
              await clientGlobalService.postPaypalCallback(data);
            } catch (error) {
              isDirectSuccess.value = false;
            }
          },
        })
        .render("#paypal-button-container");
    } catch (error) {
      console.error("failed to render the PayPal Buttons", error);
    }
  }
};

onMounted(() => {
  console.log("mounted: ", props.form);
  loadPaypal();
});
</script>
