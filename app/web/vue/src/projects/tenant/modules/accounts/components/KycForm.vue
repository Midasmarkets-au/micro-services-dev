<template>
  <div>
    <div class="card mb-5">
      <div class="card-header">
        <h3 class="card-title">
          {{ $t("tip.kycForm") }}
        </h3>
      </div>

      <div class="card-body">
        <el-form
          ref="ruleFormRef"
          :rules="rules"
          label-width="150px"
          class="demo-ruleForm"
          size="default"
          status-icon
        >
          <el-form-item label="Account" prop="account">
            <el-input v-model="kycInfos.accountNumber" disabled />
          </el-form-item>

          <el-form-item label="First Name" prop="firstName">
            <el-input v-model="kycInfos.firstName" disabled />
          </el-form-item>

          <el-form-item label="LastName" prop="lastName">
            <el-input v-model="kycInfos.lastName" disabled />
          </el-form-item>

          <el-form-item label="ID Issue Date" prop="idIssueDate">
            <el-date-picker
              v-model="kycInfos.idIssuedOn"
              type="date"
              label="Pick a date"
              :placeholder="$t('action.select')"
              style="width: 100%"
            />
          </el-form-item>

          <el-form-item label="Issue Office" prop="issueOffice">
            <el-input v-model="kycInfos.idIssuer" />
          </el-form-item>

          <el-form-item label="Expiration Data" prop="expirationDate">
            <el-date-picker
              v-model="kycInfos.idExpireOn"
              type="date"
              label="Pick a date"
              :placeholder="$t('action.select')"
              style="width: 100%"
            />
          </el-form-item>

          <el-form-item label="Address" prop="address">
            <el-input v-model="kycInfos.address" />
          </el-form-item>

          <el-form-item>
            <el-button :loading="isLoading" type="primary" @click="submitForm"
              >Submit</el-button
            >
            <el-button>Preview</el-button>
          </el-form-item>
        </el-form>
      </div>
    </div>

    <div class="card">
      <div class="card-header">
        <h3 class="card-title">
          {{ $t("tip.kycRecord") }}
        </h3>
      </div>

      <div class="card-body">
        <table
          class="table table-row-dashed fs-6 gy-5 my-0"
          id="kt_ecommerce_add_product_reviews"
        >
          <thead>
            <tr
              class="text-start text-gray-400 fw-bold fs-7 text-uppercase gs-0"
            >
              <th class="text-start">{{ $t("fields.accountNo") }}</th>
              <th class="text-start">{{ $t("fields.staff") }}</th>
              <th class="text-start">{{ $t("fields.complianceStaff") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && kycRecords.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, index) in kycRecords" :key="index">
              <td class="text-start">{{ item.accountNumber }}</td>
              <td class="text-start">{{ item.staffPartyId }}</td>
              <td class="text-start">{{ item.complianceName }}</td>
            </tr>
          </tbody>
        </table>
        <TableFooter :page-change="fetchKycHistory" :criteria="criteria" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { inject, onMounted, reactive, ref, Ref } from "vue";
import type { FormInstance, FormRules } from "element-plus";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import AccountService from "../services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserService from "@/projects/tenant/modules/users/services/UserService";
import { ElNotification } from "element-plus";
const emits = defineEmits<{
  (e: "submit"): void;
}>();

const isLoading = ref(true);

const getUserInfos = inject<() => object>(
  AccountInjectionKeys.GET_USER_INFOS,
  () => ({})
);
const userInfos = ref<any>({});

const accountDetails = inject<Ref>(
  AccountInjectionKeys.ACCOUNT_DETAILS,
  ref<any>({})
);

const ruleFormRef = ref<FormInstance>();

const kycInfos = ref({} as any);

const rules = reactive<FormRules>({});

const submitForm = async () => {
  try {
    /**
     * if kycRecords exists, means the user has submitted the form before,
     * update the form to awaiting review status
     * allowing compliance staff to re-review the form
     */
    if (kycInfos.value.emptyKycForm && kycRecords.value.length === 0) {
      try {
        await AccountService.createKycForm(
          userInfos.value.partyId,
          kycInfos.value
        );
      } catch (error) {
        MsgPrompt.error(error);
      }
    } else {
      await AccountService.updateKycFormToAwaitingReview(
        userInfos.value.partyId
      );
    }
    ElNotification({
      title: "Success",
      message: "KYC Form Submit Success",
      type: "success",
    });
    // MsgPrompt.success("Submit Success");
  } catch (e) {
    ElNotification.error({
      title: "Error",
      message: e.response.data.message
        ? e.response.data.message
        : "Error Submit",
      type: "error",
    });
    // MsgPrompt.error(error);
  }
  emits("submit");
};

const resetForm = async () => {
  if (!ruleFormRef.value) return;
  const _kycInfo = await UserService.getPredefinedKycInfos(
    userInfos.value.partyId
  );
  kycInfos.value = {
    ..._kycInfo,
    accountNumber: accountDetails.value.tradeAccount?.accountNumber,
  };
};

const kycRecords = ref(Array<any>());

const criteria = ref({
  page: 1,
  size: 10,
});
const fetchKycHistory = async (_page: number) => {
  criteria.value.page = _page;
  kycRecords.value = await AccountService.getUserKycHistory(
    userInfos.value.partyId
  );
};

onMounted(async () => {
  userInfos.value = await getUserInfos();
  await resetForm();
  await fetchKycHistory(1);
  isLoading.value = false;
});
</script>

<style scoped></style>
