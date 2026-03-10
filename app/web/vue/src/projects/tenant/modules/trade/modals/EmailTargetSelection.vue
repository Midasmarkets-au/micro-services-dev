<template>
  <div
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    aria-modal="true"
    role="dialog"
    ref="EmailTargetSelectionRef"
  >
    <div class="modal-dialog modal-dialog-centered" style="min-width: 600px">
      <div class="modal-content px-10 py-10">
        <div class="border-bottom mb-10 pb-5">
          <div class="d-flex justify-content-between">
            <p class="fs-1">Send Credit Below Letter</p>
          </div>
        </div>
        <table
          v-if="isLoading"
          class="table align-middle table-row-dashed fs-6 gy-3 table-hover"
        >
          <tbody>
            <LoadingRing />
          </tbody>
        </table>
        <div v-else class="pt-5 fs-5">
          <div class="d-flex align-items-center mb-7">
            <div style="font-weight: 800; min-width: 100px">Account</div>
            <el-input
              v-model="formData.accountNumber"
              placeholder="Account Number"
              disabled
            ></el-input>
          </div>

          <div class="d-flex align-items-center mb-7">
            <div style="font-weight: 800; min-width: 100px">Email</div>
            <el-input
              v-model="formData.email"
              placeholder="Email"
              disabled
            ></el-input>
          </div>

          <div class="d-flex align-items-center mb-7">
            <div style="font-weight: 800; min-width: 100px">Language</div>
            <el-radio-group v-model="formData.language">
              <el-radio key="en-us" value="en-us" label="en-us" />
              <el-radio key="zh-cn" value="zh-cn" label="zh-cn" />
            </el-radio-group>
          </div>

          <div class="d-flex align-items-center mb-7">
            <div style="font-weight: 800; min-width: 100px">BCCs</div>

            <div>
              <div class="d-flex align-items-center">
                <input
                  v-model="formData.bccEmails"
                  type="checkbox"
                  id="sales"
                  :value="item.salesEmail"
                  style="width: 15px; height: 15px"
                  :disabled="!item.salesEmail"
                />
                <strong class="ms-3 me-5">Sales Email:</strong
                >{{ item.salesEmail }}
              </div>
              <div class="d-flex align-items-center mt-2">
                <input
                  v-model="formData.bccEmails"
                  type="checkbox"
                  id="ib"
                  :value="item.agentEmail"
                  style="width: 15px; height: 15px"
                  :disabled="!item.agentEmail"
                />
                <strong class="ms-3 me-5">IB Email:</strong
                >{{ item.agentEmail }}
              </div>
              <div class="d-flex align-items-center mt-2">
                <input
                  v-model="formData.bccEmails"
                  type="checkbox"
                  id="ib"
                  value="dealing@midasmkts.com"
                  style="width: 15px; height: 15px"
                />
                <strong class="ms-3 me-5">Dealing Dept:</strong
                >dealing@midasmkts.com
              </div>
            </div>
          </div>

          <div class="d-flex align-items-center mb-7">
            <div style="font-weight: 800; width: 100px">Date</div>
            <el-date-picker
              v-model="formData.date"
              type="date"
              placeholder="Pick a day"
            />
          </div>

          <div class="text-end mt-3">
            <el-button type="success" :disable="isLoading" @click="send()">
              {{ $t("action.submit") }}
            </el-button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { ref, reactive } from "vue";
import { FormInstance } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Field } from "vee-validate";
import { hideModal, showModal } from "@/core/helpers/dom";
import SystemService from "@/projects/tenant/modules/system/services/SystemService";
import TradeServices from "../services/TradeServices";

const { t } = useI18n();
const isLoading = ref(true);
const item = ref<any>(null);
const EmailTargetSelectionRef = ref(null);

const formData = ref({
  accountNumber: "",
  email: "",
  language: "",
  bccEmails: [] as any,
  date: "",
});

const send = async () => {
  isLoading.value = true;

  if (!formData.value.language) {
    MsgPrompt.warning("Please select a language");
    isLoading.value = false;
    return;
  }

  // formData.value = {
  //   accountNumber: "123456789",
  //   email: "hanktsou@bacera.com",
  //   language: "en-us",
  //   bccEmails: ["hanktsou@bacera.com"],
  //   date: "2024-07-21",
  // };

  try {
    const res = await TradeServices.sendEquityBelowCreditEmail(formData.value);
    MsgPrompt.success(res.message).then(() => {
      close();
    });
  } catch (error) {
    MsgPrompt.error(error);
  }

  isLoading.value = false;
};

const show = (_item: any) => {
  isLoading.value = true;

  item.value = _item;
  formData.value = {
    accountNumber: _item.accountNumber,
    email: _item.email,
    language: "",
    bccEmails: [],
    date: getCurrentDate(),
  };
  showModal(EmailTargetSelectionRef.value);

  isLoading.value = false;
};

const getCurrentDate = () => {
  const date = new Date();
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0"); // Months are zero-based
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
};

const close = () => {
  hideModal(EmailTargetSelectionRef.value);
};

defineExpose({ show });
</script>
