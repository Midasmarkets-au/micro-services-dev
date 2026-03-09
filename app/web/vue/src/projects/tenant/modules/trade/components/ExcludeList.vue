<template>
  <div
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    aria-modal="true"
    role="dialog"
    ref="ExcludelistCreateRef"
  >
    <div class="modal-dialog modal-dialog-centered" style="min-width: 950px">
      <div class="modal-content px-10 py-10">
        <div class="border-bottom mb-10 pb-5">
          <div class="d-flex justify-content-between">
            <p class="fs-1">Excluded List</p>
          </div>
        </div>
        <div class="pt-5">
          <el-input
            v-model="formData.accountNumbers"
            placeholder="Account Numbers ( Seperate By ' , ' )"
            type="textarea"
            clearable
          ></el-input>
          <div class="text-end mt-3">
            <el-button type="success" :disable="isLoading" @click="addItem()">
              {{ $t("action.submit") }}
            </el-button>
          </div>
        </div>

        <hr class="mt-7 mb-7" />

        <table
          class="table align-middle table-row-dashed fs-6 gy-2 table-hover"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th>{{ $t("fields.accountNumber") }}</th>
              <th class="text-center">{{ $t("fields.action") }}</th>
            </tr>
          </thead>
          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && excludeList.length === 0">
            <NoDataBox />
          </tbody>
          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, index) in excludeList" :key="index">
              <td>{{ item }}</td>
              <td class="text-center">
                <el-button
                  type="danger"
                  size="small"
                  @click="openConfirmPanel(item)"
                  >{{ $t("action.delete") }}</el-button
                >
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { ref, inject } from "vue";
import { FormInstance } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
// import ToolServices from "../services/ToolServices";
import { hideModal, showModal } from "@/core/helpers/dom";
import SystemService from "@/projects/tenant/modules/system/services/SystemService";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";

const { t } = useI18n();
const isLoading = ref(false);
const ExcludelistCreateRef = ref(null);
const excludeList = ref<any>([]);
const configData = ref<any>({});
const openConfirmBox = inject(TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL);

const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();

const formData = ref<any>({
  accountNumbers: "",
});

const updateExcludedList = async () => {
  try {
    await SystemService.putExcludeEquityBelowCredit(configData.value);
    fetchData();
  } catch (error) {
    MsgPrompt.error(t("tip.submitError"));
  }
};

const addItem = async () => {
  isLoading.value = true;

  const newItems = formData.value.accountNumbers.split(",").map(Number);
  excludeList.value = [...excludeList.value, ...newItems];
  configData.value["value"] = excludeList.value;
  updateExcludedList();

  formData.value.accountNumbers = "";
};

const deleteItem = async (item: any) => {
  isLoading.value = true;

  excludeList.value = excludeList.value.filter((i) => i !== item);
  configData.value["value"] = excludeList.value;
  updateExcludedList();
};

const openConfirmPanel = (item: any) => {
  openConfirmBox?.(() => deleteItem(item), void 0, {
    confirmTitle: "Delete Account",
    confirmText: "Confirm to delete this account - " + item,
  });
};

const fetchData = async () => {
  isLoading.value = true;

  try {
    configData.value = await SystemService.getExcludeEquityBelowCredit();

    excludeList.value = configData.value["value"];
  } catch (error) {
    console.log(error);
  }

  isLoading.value = false;
};

const show = () => {
  fetchData();
  showModal(ExcludelistCreateRef.value);
};

const close = () => {
  hideModal(ExcludelistCreateRef.value);
};

defineExpose({ show });
</script>
