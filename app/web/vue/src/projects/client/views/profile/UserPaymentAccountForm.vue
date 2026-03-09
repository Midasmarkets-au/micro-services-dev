<template>
  <div
    class="modal fade"
    id="kt_modal_create_deposit"
    tabindex="-1"
    aria-hidden="true"
    ref="createPaymentMethodRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-900px">
      <div class="modal-content rounded-3">
        <div
          class="form fv-plugins-bootstrap5 fv-plugins-framework"
          style="max-height: 90vh; overflow: auto"
        >
          <!------------------------------------------------------------------- Modal Header -->
          <div class="modal-header">
            <h2 class="fs-2">{{ title }}</h2>
            <div data-bs-dismiss="modal">
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>

          <!------------------------------------------------------------------- Modal Header -->
          <div class="p-9">
            <usdtForm
              v-if="paymentPlatform == paymentPlateformList.USDT"
              ref="usdtFormRef"
              @submit="submit"
              :data="data"
            />
            <wireDefaultForm
              v-else
              ref="wireFormRef"
              @submit="submit"
              :data="data"
            />

            <!-- <wireForm
              v-if="paymentPlatform == paymentPlateformList.Bank"
              ref="wireFormRef"
              @submit="submit"
            /> -->

            <!-- <payPalForm
              v-else-if="paymentPlatform == paymentPlateformList.PayPal"
              ref="payPalFormRef"
              @submit="submit"
            /> -->
          </div>
          <!-- ------------------------------------------------------ Footer -->
          <div class="modal-footer">
            <div class="d-flex flex-stack">
              <div>
                <button
                  class="btn btn-primary btn-sm btn-radius"
                  @click="handleSubmit"
                  :disabled="submited || isLoading"
                >
                  <span v-if="isLoading">
                    {{ $t("action.waiting") }}
                    <span
                      class="spinner-border h-15px w-15px align-middle text-gray-400"
                    ></span>
                  </span>

                  <span v-else>{{ $t("action.submit") }} </span>
                </button>
              </div>
            </div>
          </div>
          <!-- ------------------------------------------------------ (END) Footer -->
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick, computed } from "vue";
import wireDefaultForm from "./components/form/WireDefaultForm.vue";
import payPalForm from "./components/form/PayPalForm.vue";
import usdtForm from "./components/form/UsdtForm.vue";
import GlobalService from "../../services/ClientGlobalService";
import { showModal, hideModal } from "@/core/helpers/dom";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import i18n from "@/core/plugins/i18n";
const { t } = i18n.global;
const props = defineProps<{
  paymentPlateformList?: any;
}>();

const _id = ref<number>(0);
const submited = ref(false);
const isLoading = ref(false);
const serviceSelected = ref(false);
const paymentPlatform = ref(100);
const wireFormRef = ref<InstanceType<typeof wireDefaultForm>>();
const payPalFormRef = ref<InstanceType<typeof payPalForm>>();
const usdtFormRef = ref<InstanceType<typeof usdtForm>>();
const createPaymentMethodRef = ref<null | HTMLElement>(null);
const data = ref<any>([]);
const isEdit = ref(false);
const emits = defineEmits<{
  (e: "fetchData"): void;
}>();
const title = computed(() => {
  return isEdit.value
    ? t("title.editPaymentAccount")
    : t("title.addPaymentAccount");
});
const reset = () => {
  _id.value = 0;
  paymentPlatform.value = 100;
  serviceSelected.value = false;
  isEdit.value = false;
  data.value = [];
};

const show = async (item?: any) => {
  reset();
  await nextTick(); // wait until reset form is done
  if (item) {
    data.value = item;
    paymentPlatform.value = item?.paymentPlatform;
    isEdit.value = true;
  } else {
    isEdit.value = false;
  }
  showModal(createPaymentMethodRef.value);
};

const showUSDT = async () => {
  reset();
  await nextTick(); // wait until reset form is done
  paymentPlatform.value = props.paymentPlateformList.USDT;
  showModal(createPaymentMethodRef.value);
};
const handleSubmit = () => {
  switch (paymentPlatform.value) {
    case props.paymentPlateformList.Bank:
      wireFormRef.value?.returnFormData();
      break;
    case props.paymentPlateformList.PayPal:
      payPalFormRef.value?.returnFormData();
      break;
    case props.paymentPlateformList.USDT:
      usdtFormRef.value?.returnFormData();
      break;
  }
};

const submit = async (_info) => {
  isLoading.value = true;

  try {
    if (isEdit.value) {
      await editSubmit();
    } else {
      await GlobalService.createUserPaymentInfo({
        paymentPlatform: paymentPlatform.value,
        name: _info.name,
        info: _info,
      });
    }
    MsgPrompt.success(t("tip.formSubmitSuccess")).then(() => {
      emits("fetchData");
      hide();
      wireFormRef.value?.resetForm();
    });
  } catch (error) {
    MsgPrompt.error(error);
  }

  isLoading.value = false;
};
const editSubmit = async () => {
  await GlobalService.updateUserPaymentInfo(data.value.id, {
    name: data.value.name,
    info: data.value.info,
  });
};
const hide = () => {
  hideModal(createPaymentMethodRef.value);
};

defineExpose({
  hide,
  show,
  showUSDT,
});
</script>
<style scoped>
.stepper-icon-round {
  border-radius: 100% !important;
}

#kt_modal_create_deposit .step-title {
  font-size: 18px;
  font-family: Lato;
  margin-bottom: 24px;
}

#kt_modal_create_deposit .btn {
  font-size: 14px;
  font-family: Lato;
}
#kt_modal_create_deposit .review-wrapper .outline {
  border: 1px solid #e4e6ef;
}
#kt_modal_create_deposit .review-wrapper .title-item {
  color: #0053ad;
  font-size: 14px;
  font-family: Lato;
  background-color: #f5f7fa;
  padding: 16px 24px;
}

#kt_modal_create_deposit .review-wrapper .content-item {
  color: black;
  font-size: 14px;
  font-family: Lato;
  padding: 16px 24px;
}
</style>
