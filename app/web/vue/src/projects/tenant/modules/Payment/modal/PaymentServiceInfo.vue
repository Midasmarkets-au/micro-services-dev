<template>
  <!-- <div v-if="isLoading">
    <LoadingRing />
  </div>
  <div v-else-if="!isLoading && serviceDetail.length === 0">
    <NoDataBox />
  </div> -->
  <div
    v-loading="isLoading"
    element-loading-background="rgba(122, 122, 122, 0.3)"
  >
    <div class="d-flex justify-content-between">
      <h3 style="text-decoration: underline">
        Payment Service Detail: {{ serviceDetail.name }}
      </h3>
      <div>
        <el-button
          v-if="!editMode"
          @click="editMode = true"
          type="primary"
          plain
          >Edit</el-button
        >

        <template v-else>
          <el-button type="success" plain @click="updatePayment"
            >Save</el-button
          >
          <el-button @click="editMode = false" plain>Cancel</el-button>
        </template>
      </div>
    </div>
    <div class="fs-5 mt-4">
      <div class="row row-cols-12">
        <p class="col col-3">
          <span class="fw-bold me-2">Name:</span>
          <span v-if="!editMode">{{ serviceDetail.name }}</span>
          <el-input v-else v-model="serviceDetail.name" class="w-150px" />
        </p>
        <!-- <p class="col">
          <span class="fw-bold me-2">Description:</span>
          <span v-if="!editMode">{{ serviceDetail.description }}</span>
          <el-input
            v-else
            v-model="serviceDetail.description"
            class="w-150px"
          />
        </p> -->
        <p class="col col-3">
          <span class="fw-bold me-2">{{ $t("fields.category") }}:</span>
          <span v-if="!editMode">
            {{ serviceDetail.group }}
          </span>

          <el-select
            v-else
            class="w-150px"
            v-model="serviceDetail.group"
            :placeholder="$t('fields.category')"
          >
            <el-option
              v-for="(value, index) in sortedServices"
              :key="index"
              :label="index"
              :value="index"
            />
          </el-select>
        </p>
        <p class="col col-6">
          <span class="fw-bold me-2"
            >{{ $t("fields.availableCurrencies") }}:</span
          >
          <template v-if="!editMode">
            <span
              v-for="currency in serviceDetail.availableCurrencies"
              :key="currency"
              class="me-2"
            >
              {{ $t(`type.currency.${currency}`) }}
            </span>
          </template>
          <el-checkbox-group v-model="serviceDetail.availableCurrencies" v-else>
            <el-checkbox
              v-for="currency in availableCurrenciesOptions"
              :key="currency.value"
              :label="currency.value"
              >{{ currency.label }}</el-checkbox
            >
          </el-checkbox-group>
        </p>
      </div>
      <div class="row row-cols-12">
        <p class="col col-3">
          <span class="fw-bold me-2">{{ $t("fields.activate") }}:</span>
          <span v-if="!editMode">
            {{ serviceDetail.status ? "Yes" : "No" }}
          </span>

          <el-select v-else class="w-75px" v-model="serviceDetail.status">
            <el-option label="Yes" :value="10" />
            <el-option label="No" :value="0" />
          </el-select>
        </p>
        <!-- <p class="col">
          <span class="fw-bold me-2">{{ $t("fields.deposit") }}:</span>
          <span v-if="!editMode">
            {{ serviceDetail.canDeposit ? "Yes" : "No" }}
          </span>
          <el-select v-else class="w-75px" v-model="serviceDetail.canDeposit">
            <el-option label="Yes" :value="1" />
            <el-option label="No" :value="0" />
          </el-select>
        </p>
        <p class="col">
          <span class="fw-bold me-2">{{ $t("fields.withdraw") }}:</span>
          <span v-if="!editMode">
            {{ serviceDetail.canWithdraw ? "Yes" : "No" }}
          </span>
          <el-select v-else class="w-75px" v-model="serviceDetail.canWithdraw">
            <el-option label="Yes" :value="1" />
            <el-option label="No" :value="0" />
          </el-select>
        </p> -->
        <p class="col col-3">
          <span class="fw-bold me-2">{{ "Comment Code" }}:</span>
          <span v-if="!editMode">{{ serviceDetail.commentCode }}</span>
          <el-input v-else v-model="serviceDetail.commentCode" class="w-50px" />
        </p>

        <p class="col">
          <span class="fw-bold me-2">{{ $t("fields.currency") }}:</span>
          <span>
            {{ $t(`type.currency.${serviceDetail.currencyId}`) }}
          </span>
        </p>
      </div>
      <div class="row row-cols-12">
        <p class="col col-3">
          <span class="fw-bold me-2">{{ $t("fields.initMinAmount") }}:</span>
          <span v-if="!editMode">
            {{ serviceDetail.initialValue }}
          </span>
          <el-input
            v-else
            class="w-75px"
            type="number"
            v-model="serviceDetail.initialValue"
          ></el-input>
        </p>
        <p class="col col-3">
          <span class="fw-bold me-2">{{ $t("fields.minAmount") }}:</span>
          <span v-if="!editMode">
            {{ serviceDetail.minValue }}
          </span>

          <el-input
            v-else
            class="w-75px"
            type="number"
            v-model="serviceDetail.minValue"
          ></el-input>
        </p>
        <p class="col">
          <span class="fw-bold me-2">{{ $t("fields.maxAmount") }}:</span>
          <span v-if="!editMode">
            {{ serviceDetail.maxValue }}
          </span>

          <el-input
            v-else
            class="w-75px"
            type="number"
            v-model="serviceDetail.maxValue"
          ></el-input>
        </p>
      </div>

      <div v-if="!editMode">
        <p class="fw-bold me-2">Logo</p>
        <div>
          <img
            v-if="serviceDetail.logo"
            :src="serviceDetail.logo"
            class="w-175px border border-2"
          />
          <div v-else>No Logo Uploaded</div>
        </div>
      </div>
      <div v-else>
        <p class="fw-bold me-2">Logo</p>
        <UploadImage ref="uploadImageRef" />
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { inject, onMounted, ref, watch, nextTick } from "vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import PaymentService from "../services/PaymentService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
// import { useStore } from "@/store";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import tenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import UploadImage from "../components/UploadImage.vue";
// const store = useStore();
const availableCurrenciesOptions = ref([
  { label: "AUD", value: CurrencyTypes.AUD },
  { label: "USD", value: CurrencyTypes.USD },
  { label: "CNY/RMB", value: CurrencyTypes.CNY },
  { label: "VND", value: CurrencyTypes.VND },
  { label: "THB", value: CurrencyTypes.THB },
  { label: "IDR", value: CurrencyTypes.IDR },
  { label: "INR", value: CurrencyTypes.INR },
  { label: "MXN", value: CurrencyTypes.MXN },
  { label: "KRW", value: CurrencyTypes.KRW },
  { label: "JPY", value: CurrencyTypes.JPY },
  { label: "BRL", value: CurrencyTypes.BRL },
  { label: "PHP", value: CurrencyTypes.PHP },
  { label: "MYR", value: CurrencyTypes.MYR },
]);
const isLoading = ref(true);
const serviceDetail = ref({} as any);
const editMode = ref(false);
const uploadImageRef = ref<any>(null);
const props = defineProps<{
  paymentServiceId: number;
  sortedServices: any;
  closeFunction: () => void;
}>();
const emits = defineEmits<{
  (e: "update"): void;
}>();
const fetchData = async () => {
  isLoading.value = true;
  try {
    serviceDetail.value = await PaymentService.getPaymentMethodById(
      props.paymentServiceId
    );
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const updatePayment = async () => {
  openConfirmBox?.(
    async () => {
      isLoading.value = true;
      // await PaymentService.putPaymentMethodByIdV2(props.paymentServiceId, {
      //   ...serviceDetail.value,
      // });

      // 如果logo有更新，則上傳圖片
      if (serviceDetail.value.logo !== uploadImageRef.value.imageUrl) {
        await uploadImageRef.value.uploadImage().then(() => {
          const res = uploadImageRef.value.imageUrl;
          if (res[0] != undefined) {
            serviceDetail.value.logo = res[0];
          }
        });
      }

      await PaymentService.updatePaymentMethodDetailById(
        props.paymentServiceId,
        {
          ...serviceDetail.value,
        }
      );

      serviceDetail.value = await PaymentService.getPaymentMethodById(
        props.paymentServiceId
      );

      emits("update");
      isLoading.value = false;
      editMode.value = false;
    },
    () => void 0,
    {
      confirmText: "Are you sure to update the payment service?",
    }
  );
};

const openConfirmBox = inject(tenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL);

watch(
  () => editMode.value,
  async (newVal) => {
    if (newVal === true) {
      await nextTick();
      uploadImageRef.value.imageUrl = serviceDetail.value.logo;
    }
  },
  { immediate: true }
);

onMounted(async () => {
  await fetchData();
});
</script>
