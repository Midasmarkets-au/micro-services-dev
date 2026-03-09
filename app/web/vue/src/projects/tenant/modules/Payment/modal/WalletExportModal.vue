<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('action.export')"
    :is-loading="isLoading"
    :width="500"
    disable-footer
  >
    <div
      class="d-flex flex-column justify-content-center align-items-center gap-5"
    >
      <div class="d-flex flex-column">
        <label class="required mb-1">{{ $t("title.recordName") }}</label>
        <el-input class="w-400px" v-model="formData.name" />
      </div>

      <div class="d-flex flex-column">
        <label for="" class="required mb-1">{{ $t("fields.type") }}</label>
        <el-select v-model="formData.type" class="w-400px">
          <el-option
            :label="$t('fields.walletOverview')"
            :value="ReportRequestTypes.WalletOverviewForTenant"
          />
          <el-option
            :label="$t('fields.walletTransactionDetails')"
            :value="ReportRequestTypes.WalletTransactionForTenant"
          />
        </el-select>
      </div>

      <div
        class="d-flex flex-column"
        v-if="formData.type === ReportRequestTypes.WalletTransactionForTenant"
      >
        <label class="required mb-1" style="">
          {{ $t("fields.period") }}
        </label>
        <el-date-picker
          class="w-400px"
          v-model="period"
          type="datetimerange"
          :start-placeholder="$t('fields.startDate')"
          :end-placeholder="$t('fields.endDate')"
          format="YYYY-MM-DD HH:mm:ss"
          value-format="YYYY-MM-DD HH:mm:ss"
        />
      </div>
      <div v-else class="w-100 px-10">
        <el-divider></el-divider>
        <div class="d-flex align-items-center justify-content-between mb-3">
          <h4>Conditions</h4>
          <el-button @click="resetCriteria">Reset Conditions</el-button>
        </div>

        <el-form label-width="auto" class="border">
          <el-form-item label="Search Type">
            <el-select
              v-model="selectedOption"
              placeholder="Select option"
              :disabled="exporting"
            >
              <el-option label="Wallet ID" value="id"></el-option>
              <el-option label="Email" value="email"></el-option>
              <el-option label="Account UID" value="accountUid"></el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="Keywords">
            <el-input v-model="inputValue" :disabled="exporting"> </el-input>
          </el-form-item>
          <el-form-item label="Currency">
            <el-select
              class="w-125px"
              v-model="criteria.currencyId"
              :placeholder="$t('fields.currency')"
              :disabled="exporting"
            >
              <el-option label="-- All --" value="" />
              <el-option
                v-for="item in currencySelections"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-form-item>
          <el-form-item label="Fund Type">
            <el-select
              class="w-125px"
              v-model="criteria.fundType"
              :placeholder="$t('fields.fundType')"
              :disabled="exporting"
            >
              <el-option label="-- All --" value="" />
              <el-option
                v-for="item in fundTypeSelections"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-form-item>
        </el-form>
      </div>
      <div class="d-flex mt-5">
        <el-button
          type="primary"
          :loading="exporting"
          @click="submit"
          :disabled="!formData.type || !formData.name"
        >
          {{ $t("action.export") }}
        </el-button>
        <el-button @click="hide">{{ $t("action.cancel") }}</el-button>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import SimpleForm from "@/components/SimpleForm.vue";
import { nextTick, ref } from "vue";
import { ReportRequestTypes } from "@/core/types/ReportRequestTypes";
import TenantGlobalService, {
  CreateReportSpec,
} from "@/projects/tenant/services/TenantGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { FundTypes, getAllFundTypeSelections } from "@/core/types/FundTypes";
const modalRef = ref();
const isLoading = ref(false);
const formData = ref({
  type: ReportRequestTypes.WalletOverviewForTenant,
} as any);
const exporting = ref(false);
const period = ref([] as any);
const selectedOption = ref("id");
const inputValue = ref("");
const criteria = ref({
  currencyId: "",
  fundType: "",
});
const currencySelections = ref([]);
const fundTypeSelections = getAllFundTypeSelections([
  FundTypes.Wire,
  FundTypes.Ips,
]);

const submit = async () => {
  const submitHandler = {
    [ReportRequestTypes.WalletTransactionForTenant]: submitReportRequestJob,
    [ReportRequestTypes.WalletOverviewForTenant]: submitReportRequestDownload,
  }[formData.value.type];

  await submitHandler?.();
};

const convertToISO = (prd) => {
  const val = prd.map((x) => (x = x.replace(" ", "T") + "Z"));
  return { from: val ? val[0] : null, to: val ? val[1] : null };
};

const submitReportRequestJob = async () => {
  // const criteria = parsePeriodIntoCriteria(period.value);
  const datesRange = convertToISO(period.value);
  const spec: CreateReportSpec = {
    type: ReportRequestTypes.WalletTransactionForTenant,
    name: formData.value.name,
    query: datesRange,
  };
  try {
    exporting.value = true;
    await TenantGlobalService.createReportRequest(spec);
    MsgPrompt.success(
      'Report request has been created, go to "Report", "Report Record" to download'
    ).then(hide);
    // const media = await TenantGlobalService.createReportRequestDownload(spec);
    // await TenantGlobalService.downloadFileByGuid(media.guid, media.fileName);
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    exporting.value = false;
  }
};
const submitReportRequestDownload = async () => {
  await configCriteria();
  const spec: CreateReportSpec = {
    type: ReportRequestTypes.WalletOverviewForTenant,
    name: formData.value.name,
    query: {
      ...criteria.value,
    },
  };
  try {
    exporting.value = true;
    const media = await TenantGlobalService.createReportRequestDownload(spec);
    await TenantGlobalService.downloadFileByGuid(media.guid, media.fileName);
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    exporting.value = false;
  }
};

const resetCriteria = () => {
  criteria.value.currencyId = "";
  criteria.value.fundType = "";
  selectedOption.value = "id";
  inputValue.value = "";
};

const configCriteria = async () => {
  delete criteria.value.id;
  delete criteria.value.email;
  delete criteria.value.accountUid;
  if (inputValue.value !== null && inputValue.value.trim() !== "") {
    criteria.value[selectedOption.value] = inputValue.value;
  }
};

const show = async (_criteria?: any, _currencySelections?: any) => {
  fillCriteria(_criteria, _currencySelections);
  await nextTick();
  modalRef.value?.show();
};

const fillCriteria = (_criteria, _currencySelections) => {
  if (_criteria) {
    criteria.value.currencyId = _criteria.currencyId;
    criteria.value.fundType = _criteria.fundType;
    if (_criteria.id === 0 || _criteria.id === null) {
      selectedOption.value = _criteria.email === null ? "accountUid" : "email";
      inputValue.value =
        _criteria.email === null ? _criteria.accountUid : _criteria.email;
    } else {
      selectedOption.value = "id";
      inputValue.value = _criteria.id;
    }
  }
  if (_currencySelections) {
    currencySelections.value = _currencySelections;
  }
};

const hide = () => {
  modalRef.value?.hide();
};

defineExpose({
  show,
  hide,
});
</script>

<style scoped>
.border {
  border: 1px solid #dedeee !important;
  border-radius: 5px;
  padding: 10px;
}
</style>
