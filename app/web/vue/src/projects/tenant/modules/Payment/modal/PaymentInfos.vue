<template>
  <div>
    <SimpleForm
      ref="paymentInfosFormRef"
      :title="$t('title.paymentInfos')"
      :is-loading="isSubmitting"
      :submit="approve"
      :discard="reject"
      :save-title="$t('action.approve')"
      :discard-title="$t('action.reject')"
      :submit-color="'primary'"
      :discard-color="'danger'"
    >
      <!-- :disable-submit="paymentInfos.status !== PaymentStatusTypes.Executing" -->
      <div
        v-if="!isLoading"
        class="card-rounded bg-body mt-n10 position-relative card-px py-15"
      >
        <!--begin::Row-->
        <div class="row g-0 mb-7">
          <!--begin::Col-->
          <div class="col mx-5">
            <div class="fs-6 text-gray-400">{{ $t("fields.amount") }}</div>
            <div class="fs-2 fw-bold text-gray-800">
              <BalanceShow
                :balance="paymentInfos.amount"
                :currency-id="paymentInfos.currencyId"
              />
            </div>
          </div>
          <!--end::Col--><!--begin::Col-->
          <div class="col mx-5">
            <div class="fs-6 text-gray-400">{{ $t("fields.currency") }}</div>
            <div class="fs-2 fw-bold text-gray-800">
              {{ $t(`type.currency.${paymentInfos.currencyId}`) }}
            </div>
          </div>
          <!--end::Col-->
        </div>
        <!--end::Row--><!--begin::Row-->
        <div class="row g-0">
          <!--begin::Col-->
          <div class="col mx-5">
            <div class="fs-6 text-gray-400">
              {{ $t("title.paymentService") }}
            </div>
            <div class="fs-2 fw-bold text-gray-800">
              {{ paymentInfos.paymentService?.name }}
            </div>
          </div>
          <!--end::Col--><!--begin::Col-->
          <div class="col mx-5">
            <div class="fs-6 text-gray-400">{{ $t("fields.createdOn") }}</div>
            <div class="fs-2 fw-bold text-gray-800">
              <TimeShow :date-iso-string="paymentInfos.createdOn" />
            </div>
          </div>
          <!--end::Col-->
        </div>
        <!--end::Row-->

        <!--begin::Timeline-->
        <div class="bg-light-primary mt-10 px-9 py-7 card-rounded w-100">
          <div class="d-flex justify-content-between align-items-center">
            <h3 class="card-title fw-bold text-primary">
              {{ $t("fields.status") }}
            </h3>
            <button
              class="btn btn-sm btn-light-info fw-bold"
              @click="executePayment"
              v-if="paymentInfos.status === PaymentStatusTypes.Pending"
            >
              Put Payment to Executing
            </button>
          </div>
          <div class="timeline-label my-10">
            <div
              class="timeline-item"
              v-if="paymentInfos.status >= PaymentStatusTypes.Pending"
            >
              <div class="timeline-label fw-bold text-gray-800 fs-6">
                <TimeShow
                  :date-iso-string="paymentInfos.createdOn"
                  :format="'MM-DD'"
                />
              </div>

              <div class="timeline-badge">
                <i
                  class="fa fa-genderless fs-1"
                  :class="{
                    ['text-primary']:
                      paymentInfos.status === PaymentStatusTypes.Pending,
                  }"
                ></i>
              </div>

              <div class="timeline-content fw-bold text-gray-800 ps-3">
                {{ $t("type.paymentStatus.0") }}
              </div>
            </div>

            <div
              class="timeline-item"
              v-if="paymentInfos.status >= PaymentStatusTypes.Executing"
            >
              <div class="timeline-label fw-bold text-gray-800 fs-6">
                <TimeShow
                  :date-iso-string="paymentInfos.createdOn"
                  :format="'MM-DD'"
                />
              </div>

              <div class="timeline-badge">
                <i
                  class="fa fa-genderless fs-1"
                  :class="{
                    ['text-primary']:
                      paymentInfos.status === PaymentStatusTypes.Executing,
                  }"
                ></i>
              </div>

              <div class="timeline-content fw-bold text-gray-800 ps-3">
                {{ $t("type.paymentStatus.1") }}
              </div>
            </div>

            <div
              class="timeline-item"
              v-if="paymentInfos.status > PaymentStatusTypes.Executing"
            >
              <div class="timeline-label fw-bold text-gray-800 fs-6">
                <TimeShow
                  :date-iso-string="paymentInfos.createdOn"
                  :format="'MM-DD'"
                />
              </div>

              <div class="timeline-badge">
                <i
                  class="fa fa-genderless text-success fs-1"
                  :class="{
                    ['text-success']:
                      paymentInfos.status === PaymentStatusTypes.Completed,
                    ['text-danger']:
                      paymentInfos.status === PaymentStatusTypes.Failed ||
                      paymentInfos.status === PaymentStatusTypes.Cancelled,
                  }"
                ></i>
              </div>

              <div class="timeline-content fw-bold text-gray-800 ps-3">
                {{ $t(`type.paymentStatus.${paymentInfos.status}`) }}
              </div>
            </div>
          </div>
        </div>
        <!--end::Timeline-->
      </div>
      <!-- <div>{{ paymentInfos }}</div> -->
    </SimpleForm>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import PaymentService from "../services/PaymentService";
import { PaymentInfo, PaymentStatusTypes } from "@/core/types/PaymentTypes";
import TimeShow from "@/components/TimeShow.vue";
import BalanceShow from "@/components/BalanceShow.vue";

const isLoading = ref(true);
const isSubmitting = ref(true);
const paymentInfosFormRef = ref<InstanceType<typeof SimpleForm>>();
const paymentInfos = ref<PaymentInfo>({} as PaymentInfo);

//合併 dealing 操作
const itemID = ref<number>(-1);

const approve = async () => {
  isSubmitting.value = true;
  // 當財務部門批准時，直接將 dealing 付款狀態改為已完成，讓dealing 到下一個 Tab 批准
  await PaymentService.completePaymentById(paymentInfos.value.id).then(
    async () => {
      if (itemID.value !== -1) {
        //-直接將 dealing 付款狀態改為已完成-
        await PaymentService.completeDepositById(itemID.value);
      }
    }
  );
  isSubmitting.value = false;
  paymentInfosFormRef.value?.hide();
};

const reject = async () => {
  isSubmitting.value = true;
  await PaymentService.cancelPaymentById(paymentInfos.value.id);
  isSubmitting.value = false;
  paymentInfosFormRef.value?.hide();
};

const executePayment = async () => {
  isSubmitting.value = true;
  await PaymentService.executePaymentById(paymentInfos.value.id);
  isSubmitting.value = false;
  paymentInfosFormRef.value?.hide();
};

defineExpose({
  async show(paymentId: number, id?: number) {
    paymentInfosFormRef.value?.show();
    itemID.value = id || -1;
    paymentInfos.value = await PaymentService.getPaymentInfosById(paymentId);

    isLoading.value = false;
    isSubmitting.value = false;
  },
  hide() {
    paymentInfosFormRef.value?.hide();
  },
});
</script>

<style scoped></style>
