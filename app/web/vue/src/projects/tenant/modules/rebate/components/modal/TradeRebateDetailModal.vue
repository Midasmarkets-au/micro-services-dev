<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.tradeRebateDetails')"
    :is-loading="isLoading"
    :width="1000"
    :before-close="handleBeforeClose"
    disable-footer
  >
    <LoadingCentralBox v-if="isLoading" />
    <div v-if="!isLoading && item">
      <el-descriptions
        class="margin-top"
        :title="$t('title.trade')"
        :column="3"
        border
      >
        <template #extra>
          <div class="d-flex gap-2"></div>
          <UserInfo
            :user="item?.sourceTradeAccount?.user"
            disable-comment-view
          />
        </template>

        <el-descriptions-item>
          <template #label>
            <div class="cell-item">
              {{ $t("fields.accountNo") }}
            </div>
          </template>
          {{ item?.sourceTradeAccount?.accountNumber }}
        </el-descriptions-item>

        <el-descriptions-item>
          <template #label>
            <div class="cell-item">
              {{ $t("fields.ticket") }}
            </div>
          </template>
          {{ item.ticket }}
        </el-descriptions-item>

        <el-descriptions-item>
          <template #label>
            <div class="cell-item">
              {{ $t("fields.platform") }}
            </div>
          </template>
          {{ platformName }}
        </el-descriptions-item>

        <el-descriptions-item>
          <template #label>
            <div class="cell-item">
              {{ $t("fields.symbol") }}
            </div>
          </template>
          {{ item.symbol }}
        </el-descriptions-item>

        <el-descriptions-item>
          <template #label>
            <div class="cell-item">
              {{ $t("fields.currency") }}
            </div>
          </template>
          {{ $t(`type.currency.${item.currencyId}`) }}
        </el-descriptions-item>

        <el-descriptions-item>
          <template #label>
            <div class="cell-item">
              {{ $t("fields.volume") }}
            </div>
          </template>
          {{ item.volume / 100 }}
        </el-descriptions-item>

        <el-descriptions-item>
          <template #label>
            <div class="cell-item">
              {{ $t("fields.currentStatus") }}
            </div>
          </template>
          {{ $t(`type.tradeRebateStatus.${item.status}`) }}
        </el-descriptions-item>

        <el-descriptions-item>
          <template #label>
            <div class="cell-item">
              {{ $t("fields.openTime") }}
            </div>
          </template>
          <!-- <TimeShow type="exactTime" :date-iso-string="item.openedOn" />
          <br /> -->
          <TimeShow
            type="exactTimeGMT"
            :date-iso-string="item.openedOn"
            :gmt-option="{ isconvert: false, islocal: true }"
          />
        </el-descriptions-item>

        <el-descriptions-item>
          <template #label>
            <div class="cell-item">
              {{ $t("fields.closeTime") }}
            </div>
          </template>
          <!-- <TimeShow type="exactTime" :date-iso-string="item.closedOn" />
          <br /> -->
          <TimeShow
            type="exactTimeGMT"
            :date-iso-string="item.closedOn"
            :gmt-option="{ isconvert: false, islocal: true }"
          />
        </el-descriptions-item>
      </el-descriptions>
      <el-divider></el-divider>
      <div class="d-flex justify-content-between align-items-center">
        <h4 class="mt-5">
          Rebate Target Accounts<b style="color: #f1416c">(GMT+0)</b>
        </h4>
        <div>
          <el-button
            type="primary"
            plain
            class="me-2"
            :icon="Search"
            @click="showCheck()"
            >{{ $t("action.check") }}</el-button
          >
          <el-button
            type="success"
            plain
            class="me-5"
            :icon="Promotion"
            @click="reset()"
            v-if="resendShow"
            >Resend</el-button
          >
        </div>
      </div>
      <div>
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th>{{ $t("fields.user") }}</th>
              <th>UID</th>
              <th>{{ $t("fields.group") }}</th>
              <th>{{ $t("fields.amount") }}</th>
              <th>Target Wallet ID</th>
              <th>{{ $t("fields.postedOn") }}</th>
              <th>Released On</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && item.rebates.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold">
            <tr v-for="(rebateItem, index) in item.rebates" :key="index">
              <td><UserInfo :user="rebateItem.user" disable-comment-view /></td>
              <td>{{ rebateItem.targetAccount.uid }}</td>
              <td>{{ rebateItem.targetAccount.group }}</td>
              <td>
                <BalanceShow
                  :balance="rebateItem.amount"
                  :currency-id="rebateItem.currencyId"
                />
              </td>
              <td>{{ rebateItem.targetWalletId }}</td>
              <td>
                存储时间：<TimeShow
                  type="exactTimeGMT"
                  :date-iso-string="rebateItem.postedOn"
                  :gmt-option="{ isconvert: false, islocal: false }"
                /><br />
                转换时间：
                <TimeShow
                  type="exactTimeGMT"
                  :date-iso-string="rebateItem.postedOn"
                  :gmt-option="{ isconvert: true, islocal: true }"
                />
              </td>
              <td>
                存储时间：
                <TimeShow
                  type="exactTime"
                  :date-iso-string="rebateItem.releasedOn"
                />
                <br />
                转换时间：
                <TimeShow
                  type="exactTimeGMT"
                  :date-iso-string="rebateItem.releasedOn"
                  :gmt-option="{ isconvert: true, islocal: true }"
                />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="check">
        <el-divider></el-divider>
        <h4 class="mt-5">Check Rebate Data</h4>
        <div>
          <table
            class="table align-middle table-row-dashed fs-6 gy-5"
            id="table_accounts_requests"
          >
            <thead>
              <tr
                class="text-start text-muted fw-bold fs-7 text-uppercase gs-0"
              >
                <th class="">{{ $t("fields.user") }}</th>
                <th class="">UID</th>
                <th class="">{{ $t("fields.group") }}</th>
                <th class="">{{ $t("fields.amount") }}</th>
                <th class="">{{ $t("fields.postedOn") }}</th>
              </tr>
            </thead>

            <tbody v-if="checkLoading">
              <LoadingRing />
            </tbody>
            <tbody v-else-if="!checkLoading && checkData.length === 0">
              <NoDataBox />
            </tbody>

            <tbody v-else class="fw-semibold text-black">
              <tr v-for="(item, index) in checkData" :key="index">
                <td><UserInfo :user="item.user" disable-comment-view /></td>
                <td>{{ item.targetAccount.uid }}</td>
                <td>{{ item.targetAccount.group }}</td>
                <td>
                  <BalanceShow
                    :balance="item.amount"
                    :currency-id="item.currencyId"
                  />
                </td>
                <td>
                  <TimeShow type="exactTime" :date-iso-string="item.postedOn" />
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, computed, inject } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import RebateService from "@/projects/tenant/modules/rebate/services/RebateService";
import moment from "moment";
import TimeShow from "@/components/TimeShow.vue";
import UserInfo from "@/components/UserInfo.vue";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import BalanceShow from "@/components/BalanceShow.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Search, Promotion } from "@element-plus/icons-vue";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import { ElNotification } from "element-plus";
import {
  convertToLocalTime,
  convertToLocalGMT,
} from "@/core/plugins/TimerService";
const isLoading = ref(true);
const checkLoading = ref(false);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const item = ref<any>({});
const check = ref(false);
const resendShow = ref(false);
const services = ref<any>(null);
const checkData = ref<any>({});
const platformName = computed(
  () =>
    services.value?.find((s: any) => s.id === item.value.tradeServiceId)
      ?.name ?? ""
);
const show = async (_item: any) => {
  modalRef.value?.show();

  try {
    isLoading.value = true;
    item.value = await RebateService.getTradeRebateDetails(_item.id);
    if (services.value === null) {
      services.value = await AccountService.getServices();
    }
    item.value.tradeGMT = Array.isArray(item.value.tradeGMT)
      ? item.value.tradeGMT
      : [];
    item.value.rebateGMT = Array.isArray(item.value.rebateGMT)
      ? item.value.rebateGMT
      : [];

    item.value.tradeGMT.push(
      convertToLocalGMT(item.value.openedOn, "America/Los_Angeles")
    );
    item.value.tradeGMT.push(
      convertToLocalGMT(item.value.closedOn, "America/Los_Angeles")
    );
    if (item.value.rebates?.length > 0) {
      item.value.rebateGMT.push(
        convertToLocalGMT(item.value.rebates[0].postedOn, "America/Los_Angeles")
      );
      item.value.rebateGMT.push(
        convertToLocalGMT(
          item.value.rebates[0].releasedOn,
          "America/Los_Angeles"
        )
      );
    }
    item.value.tradeGMT = item.value.tradeGMT.filter((gmt, index) => {
      return item.value.tradeGMT.indexOf(gmt) === index;
    });
    item.value.rebateGMT = item.value.rebateGMT.filter((gmt, index) => {
      return item.value.rebateGMT.indexOf(gmt) === index;
    });
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};
const openConfirmModal = inject<any>(
  TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL
);

const reset = async () => {
  openConfirmModal?.(() => RebateService.resendRebate(item.value.id));
};
const showCheck = async () => {
  check.value = true;

  try {
    checkLoading.value = true;
    checkData.value = await RebateService.getRebateCheck(item.value.id);
    resendShow.value = true;
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    checkLoading.value = false;
  }
};
const handleBeforeClose = () => {
  check.value = false;
  resendShow.value = false;
  checkData.value = {};
};

defineExpose({
  show,
  hide: () => modalRef.value?.hide(),
});
</script>

<style scoped></style>
