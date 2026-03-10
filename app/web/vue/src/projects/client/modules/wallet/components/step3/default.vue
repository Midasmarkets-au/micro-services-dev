<template>
  <div style="width: 100%">
    <div class="step-title">
      {{ $t("title.depositInToCurrentAccount") }}
    </div>

    <!-- Amount -->
    <div class="d-flex flex-column mb-5 fv-row">
      <label class="fs-5 fw-semobold mb-2 required"
        >{{ $t("fields.amount") }} ({{
          $t("type.currency." + paymentRequireData.currencyId)
        }})</label
      >

      <Field
        class="form-control form-control-solid"
        placeholder=""
        name="amount"
        v-model.number="paymentRequireData.amount"
        @keyup="calculateTargetAmount()"
      />

      <div class="fv-plugins-message-container">
        <div class="fv-help-block">
          <ErrorMessage name="amount"> </ErrorMessage>
        </div>
      </div>
      <div v-if="amountError" style="color: #900000">
        {{ $t("error.amountRule") }} ${{
          isInitial ? paymentService.initialValue : paymentService.minValue
        }}
        - ${{ paymentService.maxValue }}
      </div>
    </div>

    <div class="d-flex flex-column mb-5 fv-row">
      <label class="fs-5 fw-semobold mb-2"
        >{{ $t("fields.depositAmount") }} -
        {{ showDetailInfo.paymentServiceInfo["name"] }}</label
      >

      <Field
        class="form-control form-control-solid"
        placeholder=""
        name="targetAmount"
        v-model.number="paymentRequireData.targetAmount"
        disabled
      />
      <div>
        {{ $t("fields.exchangeRate") }}:
        {{ paymentRequireData.exchangeRate }}
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const props = defineProps<{
  item: object;
  isSelected: boolean;
}>();
const item = ref<any>({
  ...props.item,
});
</script>

<style scoped lang="scss">
.wallet-icon {
  position: absolute;
  top: 5px;
  right: 5px;
  img {
    width: 50px;
    height: 50px;
    opacity: 0.6;
  }
}
</style>
