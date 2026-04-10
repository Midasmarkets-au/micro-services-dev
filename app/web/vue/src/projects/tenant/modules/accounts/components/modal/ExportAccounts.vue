<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('action.export')"
    width="900"
    align-center
  >
    <el-form label-width="auto">
      <div class="row row-cols-2">
        <el-form-item :label="$t('tip.searchKeyWords')"
          ><el-input
            v-model="criteria.searchText"
            clearable
            :disabled="isLoading"
        /></el-form-item>
        <el-form-item :label="t('status.showClosedAccount')">
          <el-checkbox
            v-model="criteria.includeClosed"
            size="large"
            :disabled="isLoading"
          />
        </el-form-item>
      </div>
      <div class="row row-cols-2">
        <el-form-item label="MT">
          <el-select
            class="w-150px"
            v-model="criteria.serviceId"
            :disabled="isLoading"
          >
            <el-option
              v-for="item in ServiceList"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>

        <el-form-item :label="t('fields.status')">
          <el-select
            class="w-150px"
            v-model="criteria.status"
            :disabled="isLoading"
          >
            <el-option label="-- All --" value="" />
            <el-option label="Activated" :value="AccountStatusTypes.Activate" />
            <el-option label="Paused" :value="AccountStatusTypes.Pause" />
            <el-option
              label="Inactivated"
              :value="AccountStatusTypes.Inactivated"
            />
          </el-select>
        </el-form-item>
      </div>
      <div class="row row-cols-2">
        <el-form-item :label="$t('fields.site')">
          <el-select
            class="w-150px"
            v-model="criteria.siteId"
            :disabled="isLoading"
          >
            <el-option label="-- All --" value="" />

            <el-option
              v-for="item in ConfigSiteTypesSelections"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item :label="$t('fields.role')">
          <el-select
            class="w-150px"
            v-model="criteria.role"
            :disabled="isLoading"
          >
            <el-option label="-- All --" value="" />
            <el-option label="Client" :value="AccountRoleTypes.Client" />
            <el-option label="IB" :value="AccountRoleTypes.IB" />
            <el-option label="Sales" :value="AccountRoleTypes.Sales" />
          </el-select>
        </el-form-item>
      </div>
      <div class="row row-cols-2">
        <el-form-item label="Child Accounts">
          <el-input
            class="w-150px"
            v-model="criteria.parentAccountUid"
            :disabled="isLoading"
            clearable
          />
        </el-form-item>
        <el-form-item label="Sales Child Only">
          <el-input
            class="w-150px"
            v-model="criteria.salesUid"
            clearable
            :disabled="isLoading"
          />
        </el-form-item>
      </div>
      <div class="row row-cols-2">
        <el-form-item :label="$t('fields.ibGroup')">
          <el-input
            class="w-150px"
            v-model="criteria.group"
            clearable
            :disabled="isLoading"
          />
        </el-form-item>
        <el-form-item :label="$t('fields.salesCode')">
          <el-input
            class="w-150px"
            v-model="criteria.code"
            clearable
            :disabled="isLoading"
          />
        </el-form-item>
      </div>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disabled="isLoading">{{
          $t("action.cancel")
        }}</el-button>
        <el-button
          type="success"
          :loading="isLoading"
          @click="exportAccounts()"
        >
          {{ $t("action.export") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import i18n from "@/core/plugins/i18n";
import AccountService from "../../services/AccountService";
import {
  AccountRoleTypes,
  AccountStatusTypes,
} from "@/core/types/AccountInfos";
import { ConfigSiteTypesSelections } from "@/core/types/SiteTypes";
import { ElNotification } from "element-plus";
import { ReportRequestTypes } from "@/core/types/ReportRequestTypes";
import moment from "moment";

const t = i18n.global.t;
const isLoading = ref(false);
const dialogRef = ref(false);
const criteria = ref({} as any);

const currentDate = moment().format("YYYY-MM-DD");

const ServiceList = ref<any>([
  { label: "-- All --", value: "" },
  { label: "No Server", value: 0 },
  { label: "MDM-Real", value: 10 },
  { label: "MDM-Real", value: 20 },
  { label: "MDM-MT5", value: 30 },
  { label: "MDM-MT5", value: 31 },
]);

const show = (_criteria: any) => {
  dialogRef.value = true;
  criteria.value = {
    searchText: _criteria.searchText,
    includeClosed: _criteria.includeClosed,
    serviceId: _criteria.serviceId,
    status: _criteria.status,
    siteId: _criteria.siteId,
    role: _criteria.role,
    parentAccountUid: _criteria.parentAccountUid,
    salesUid: _criteria.salesUid,
    group: _criteria.group,
    code: _criteria.code,
  };
};

const exportAccounts = async () => {
  isLoading.value = true;
  try {
    await AccountService.exportAccounts({
      type: ReportRequestTypes.AccountSearchForTenant,
      name: "Accounts Report-" + currentDate,
      query: criteria.value,
    });
    ElNotification.success({
      title: "Success",
      message: "Exporting accounts report success",
    });
    criteria.value = {};
  } catch (e) {
    console.error(e);
    ElNotification.error({
      title: "Error",
      message: e.message,
    });
  } finally {
    isLoading.value = false;
  }
};

defineExpose({
  show,
});
</script>
