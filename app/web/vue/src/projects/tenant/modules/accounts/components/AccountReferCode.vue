<template>
  <div class="card">
    <div class="card-header">
      <div
        class="card-title d-flex align-item-center justify-content-end"
        style="width: 100%"
      >
        <div>
          <el-button
            v-if="$can('TenantAdmin')"
            type="primary"
            :loading="isUpdating"
            @click="addReferralCode()"
          >
            Add Referral Code
          </el-button>
        </div>
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-3">
        <thead class="text-muted fw-bold fs-7 text-uppercase gs-0">
          <tr>
            <td>{{ $t("fields.linkName") }}</td>
            <td>{{ $t("fields.referCode") }}</td>
            <td>{{ $t("fields.accountType") }}</td>
            <td>{{ $t("fields.linkType") }}</td>
            <td>{{ $t("fields.lang") }}</td>
            <td>{{ $t("fields.createdOn") }}</td>
            <td>{{ $t("action.action") }}</td>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in data" :key="index">
            <td>{{ item.name }}</td>
            <td>{{ item.code }}</td>
            <td class="">
              <template v-if="item.serviceType != ReferralServiceType.Client">
                <div>
                  <span
                    v-for="(acc, index) in item.displaySummary.schema"
                    class="accountBadge"
                    :class="['type1', 'type2', 'type3'][index % 3]"
                    :key="'type_' + index"
                    >{{ $t("type.shortAccount." + acc.accountType) }}</span
                  >
                </div>
              </template>

              <template v-if="item.serviceType == ReferralServiceType.Client">
                <div>
                  <span
                    v-for="(acc, index) in item.displaySummary
                      .allowAccountTypes"
                    class="accountBadge"
                    :class="['type1', 'type2', 'type3'][index % 3]"
                    :key="'type_' + index"
                    >{{ $t("type.shortAccount." + acc.accountType) }}</span
                  >
                </div>
              </template>
            </td>
            <td>
              <div class="d-flex align-items-center">
                {{ $t("type.accountRole." + item.serviceType) }}
                <div
                  v-if="
                    item.serviceType == AccountRoleTypes.Client &&
                    item.isDefault == 1
                  "
                  class="ms-1"
                >
                  <el-tag type="success" size="small">
                    {{ $t("status.default") }}</el-tag
                  >
                </div>
              </div>
            </td>

            <td class="">
              {{
                item.displaySummary.language == undefined
                  ? "undefined"
                  : $t("type.language." + item.displaySummary.language)
              }}
            </td>
            <td>
              <TimeShow :date-iso-string="item.createdOn" />
            </td>
            <td>
              <el-button
                v-if="
                  item.serviceType == ReferralServiceType.Client &&
                  item.isDefault == 0
                "
                type="success"
                class="small"
                plain
                :loading="isUpdating"
                @click="setDefaultClient(item)"
              >
                {{ $t("action.setDefault") }}
              </el-button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <CreateReferralCodeForAccount
      ref="createReferralCodeRef"
      @fetchData="fetchData"
    />
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import notification from "@/core/plugins/notification";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { ReferralServiceType } from "@/core/types/ReferralServiceType";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import CreateReferralCodeForAccount from "@/projects/tenant/modules/accounts/components/modal/CreateReferralCodeForAccount.vue";

const data = ref<any>([]);
const isLoading = ref(false);
const isUpdating = ref(false);
const createReferralCodeRef =
  ref<InstanceType<typeof CreateReferralCodeForAccount>>();
const accountDetails = inject(AccountInjectionKeys.ACCOUNT_DETAILS);

const setDefaultClient = async (item: any) => {
  isUpdating.value = true;
  try {
    await AccountService.setDefaultClient(item.accountId, item.code);
    notification.success();
    fetchData();
  } catch (error) {
    notification.danger();
    console.log(error);
  }
  isUpdating.value = false;
};

const addReferralCode = async () => {
  createReferralCodeRef.value?.show();
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await AccountService.getReferCode(accountDetails.value.id);
    data.value = res;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

onMounted(() => {
  fetchData();
});
</script>
