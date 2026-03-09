<template>
  <div
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    aria-modal="true"
    role="dialog"
    ref="paymentCreateShowRef"
  >
    <div class="modal-dialog modal-dialog-centered" style="min-width: 950px">
      <div
        class="modal-content px-10 py-10"
        v-loading="isLoading"
        element-loading-background="rgba(122, 122, 122, 0.3)"
      >
        <div class="border-bottom mb-10">
          <p class="fs-1">Add Payment Service</p>
        </div>

        <!-- <div v-if="isLoading" class="d-flex justify-content-center">
          <LoadingRing />
        </div> -->
        <div>
          <el-form :model="formData" ref="paymentFormRef" label-position="top">
            <div class="row">
              <div class="col-8">
                <div class="d-flex">
                  <el-form-item label="Activate" prop="isActivated">
                    <el-switch
                      v-model="formData.isActivated"
                      :active-value="1"
                      :inactive-value="0"
                    ></el-switch>
                  </el-form-item>

                  <el-form-item
                    label="High Dollar"
                    prop="isHighDollarEnabled"
                    class="ms-9"
                  >
                    <el-switch
                      v-model="formData.isHighDollarEnabled"
                      :active-value="1"
                      :inactive-value="0"
                    ></el-switch>
                  </el-form-item>

                  <el-form-item
                    label="Auto Deposit"
                    prop="isAutoDepositEnabed"
                    class="ms-9"
                  >
                    <el-switch
                      v-model="formData.isAutoDepositEnabed"
                      :active-value="1"
                      :inactive-value="0"
                    ></el-switch>
                  </el-form-item>
                </div>
              </div>
              <div class="col-4">
                <el-form-item label="Method Type"
                  ><div class="d-flex align-items-center">
                    <Field
                      v-model="formData.methodType"
                      class="form-check-input widget-9-check me-3"
                      type="radio"
                      name="Deposit"
                      value="Deposit"
                    />
                    <label class="me-9" for="Deposit">Deposit</label>
                    <Field
                      v-model="formData.methodType"
                      class="form-check-input widget-9-check me-3"
                      type="radio"
                      name="Withdrawal"
                      value="Withdrawal"
                    />
                    <label class="me-9" for="Withdrawal">Withdrawal</label>
                  </div>
                </el-form-item>
              </div>
            </div>

            <div class="row">
              <el-form-item label="Name (Required)" prop="name" class="col-4">
                <el-input v-model="formData.name"></el-input>
              </el-form-item>
              <el-form-item label="Code" prop="commentCode" class="col-4">
                <el-input v-model="formData.commentCode"></el-input>
              </el-form-item>
              <el-form-item label="Percentage" prop="percentage" class="col-4">
                <el-input v-model="formData.percentage"
                  ><template #append>%</template></el-input
                >
              </el-form-item>
            </div>
            <div class="row">
              <el-form-item
                label="Platform (Required)"
                prop="platform"
                class="col-4"
              >
                <el-select v-model="formData.platform" placeholder="Select">
                  <el-option
                    v-for="(value, index) in platformList"
                    :key="index"
                    :label="t(`type.paymentTypes.${value}`)"
                    :value="value"
                  />
                </el-select>
              </el-form-item>
              <el-form-item
                label="Category (Required)"
                prop="group"
                class="col-4"
              >
                <el-input v-model="formData.group"></el-input>
                <!-- <el-select v-model="formData.group" placeholder="Select">
                  <el-option
                    v-for="(value, index) in categoryList"
                    :key="index"
                    :label="index"
                    :value="index"
                  />
                </el-select> -->
              </el-form-item>
              <el-form-item
                label="Currency (Required)"
                prop="currencyId"
                class="col-4"
              >
                <el-select v-model="formData.currencyId" placeholder="Select">
                  <el-option
                    v-for="(value, i) in currencyList"
                    :key="i"
                    :label="$t(`type.currency.${value}`)"
                    :value="value"
                  />
                </el-select>
              </el-form-item>
            </div>

            <div class="row">
              <el-form-item
                label="Initial Minimum Amount"
                prop="initialValue"
                class="col-4"
              >
                <el-input
                  type="number"
                  v-model="formData.initialValue"
                ></el-input>
              </el-form-item>
              <el-form-item
                label="Minimum Amount"
                prop="minValue"
                class="col-4"
              >
                <el-input type="number" v-model="formData.minValue"></el-input>
              </el-form-item>
              <el-form-item
                label="Maximum Amount"
                prop="maxValue"
                class="col-4"
              >
                <el-input type="number" v-model="formData.maxValue"></el-input>
              </el-form-item>
            </div>

            <el-form-item label="Description" prop="description">
              <el-input v-model="formData.description"></el-input>
            </el-form-item>

            <el-form-item label="Upload Payment Logo" prop="image">
              <UploadImage ref="uploadImageRef" />
            </el-form-item>

            <div class="text-end mt-15">
              <button class="btn btn-secondary me-5" @click.prevent="close">
                {{ $t("action.close") }}
              </button>
              <button
                class="btn btn-primary"
                :disable="isLoading"
                @click.prevent="addPaymentService()"
              >
                {{ $t("action.submit") }}
              </button>
            </div>
          </el-form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";
import { hideModal, showModal } from "@/core/helpers/dom";
import { type FormRules, FormInstance } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import PaymentService from "../services/PaymentService";
import { Field } from "vee-validate";
import UploadImage from "./UploadImage.vue";

const { t } = useI18n();
const isLoading = ref(false);
const paymentCreateShowRef = ref(null);
const paymentFormRef = ref<FormInstance>();
const categoryList = ref([]);
const platformList = ref([] as any);
const tab = ref("Deposit");
const store = useStore();
const uploadImageRef = ref<any>(null);
const currencyList = store.state.AuthModule.config.currencyAvailable;

const initFormData = ref({
  isActivated: 0,
  isHighDollarEnabled: 0,
  isAutoDepositEnabed: 0,
  methodType: "Deposit",
  name: "",
  commentCode: "",
  percentage: 100,
  description: "",
  currencyId: "",
  canDeposit: 0,
  canWithdraw: 0,
  initialValue: 0,
  minValue: 0,
  maxValue: 0,
  platform: "",
  group: "",
});

const formData = ref({
  isActivated: 0,
  isHighDollarEnabled: 0,
  isAutoDepositEnabed: 0,
  methodType: "Deposit",
  name: "",
  commentCode: "",
  percentage: 100,
  description: "",
  currencyId: "",
  canDeposit: 0,
  canWithdraw: 0,
  initialValue: 0,
  minValue: 0,
  maxValue: 0,
  platform: "",
  group: "",
  logo: "",
});

const addPaymentService = async () => {
  if (
    formData.value.name == "" ||
    formData.value.platform == "" ||
    formData.value.group == "" ||
    formData.value.currencyId == ""
  ) {
    MsgPrompt.error("Name, Platform, Category and Currency are required");
    return;
  }

  formData.value.initialValue = parseFloat(formData.value.initialValue);
  formData.value.minValue = parseFloat(formData.value.minValue);
  formData.value.maxValue = parseFloat(formData.value.maxValue);

  isLoading.value = true;
  try {
    await uploadImageRef.value.uploadImage().then(() => {
      const res = uploadImageRef.value.imageUrl;
      if (res[0] != undefined) {
        formData.value.logo = res[0];
      }
    });
    console.log("formData.value", formData.value);
    await PaymentService.postPaymentServices(formData.value);
    MsgPrompt.success(t("tip.submitSuccess")).then(() => {
      emits("eventSubmit");
      close();
    });
    formData.value = ref(JSON.parse(JSON.stringify(initFormData)));
  } catch (error) {
    MsgPrompt.error("Failed to submit");
  }

  isLoading.value = false;
};

const show = (data, _platform, _tab) => {
  platformList.value = _platform;
  categoryList.value = data;
  tab.value = _tab;

  formData.value = JSON.parse(JSON.stringify(initFormData.value));
  formData.value.methodType = tab.value;
  showModal(paymentCreateShowRef.value);
};

const close = () => {
  hideModal(paymentCreateShowRef.value);
};

const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();

defineExpose({ show });
</script>
