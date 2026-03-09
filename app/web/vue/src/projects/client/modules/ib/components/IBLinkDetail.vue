<template>
  <div
    class="modal fade ibLinkDetailBackdrop"
    id="kt_modal_iblibk_detail"
    tabindex="-1"
    aria-hidden="true"
    ref="IBLinkDetailModalRef"
  >
    <div class="modal-dialog modal-dialog-centered modal-fullscreen-sm-down">
      <div class="modal-content rounded-3">
        <div class="modal-header" id="kt_modal_new_address_header">
          <h2 class="fs-2 modal-title-text">{{ $t("title.ibLinkDetail") }}</h2>
          <div data-bs-dismiss="modal" class="close-btn">
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <!-- ------------------------------------------------------------------ -->
        <div class="form-outer">
          <table
            v-if="isLoading"
            class="table table-row-dashed table-row-gray-200 align-middle"
          >
            <tbody>
              <LoadingRing />
            </tbody>
          </table>

          <template v-else>
            <div v-if="ibLinkDetail.summary.distributionType == 3" class="p-4">
              <!-- 表单行：移动端垂直排列 -->
              <div class="form-row">
                <div class="form-item">
                  <div class="form-label">
                    {{ $t("fields.referralCode") }}
                  </div>
                  <Field
                    class="form-control form-control-solid form-input"
                    name="ibLinkCode"
                    :value="ibLinkDetail.code"
                    disabled
                  />
                </div>

                <div class="form-item">
                  <div class="form-label">
                    {{ $t("fields.linkName") }}
                  </div>
                  <Field
                    class="form-control form-control-solid form-input"
                    name="ibLinkName"
                    :value="ibLinkDetail.summary.name"
                    disabled
                  />
                </div>
              </div>

              <div class="form-row">
                <div class="form-item">
                  <div class="form-label">
                    {{ $t("fields.language") }}
                  </div>
                  <Field
                    class="form-control form-control-solid form-input"
                    name="ibLinkLanguage"
                    :value="ibLinkDetail.summary.language"
                    disabled
                  />
                </div>

                <div class="form-item">
                  <div class="form-label">
                    {{ $t("title.recordName") }}
                  </div>
                  <Field
                    class="form-control form-control-solid form-input"
                    name="ibLinkOptionName"
                    :value="ibLinkDetail.summary.percentageSchema.optionName"
                    disabled
                  />
                </div>
              </div>

              <hr
                v-if="ibLinkDetail.serviceType == ReferralServiceType.Agent"
              />

              <div v-if="ibLinkDetail.serviceType == ReferralServiceType.Agent">
                <div
                  v-for="(item, index) in ibLinkDetail.summary.percentageSchema
                    .percentageSetting"
                  :key="index"
                  class="level-item"
                >
                  <label class="form-label">Level {{ index + 1 }}</label>
                  <div class="d-flex align-items-center">
                    <input
                      class="form-control form-control-solid form-input"
                      :name="'ibLinkSetting' + index"
                      :value="item"
                      disabled
                    />
                    <span class="ms-3">%</span>
                  </div>
                </div>
              </div>
            </div>
            <div v-else>
              <div v-if="ibLinkDetail.serviceType == ReferralServiceType.Agent">
                <table
                  v-if="ibLinkDetail.summary?.schema.length == 0"
                  class="table table-row-dashed table-row-gray-200 align-middle"
                >
                  <tbody>
                    <NoDataBox />
                  </tbody>
                </table>

                <div
                  v-else
                  v-for="(account, index) in ibLinkDetail.summary.schema"
                  :key="index"
                  class="account-section"
                >
                  <div class="account-header">
                    <div class="vertical-line"></div>
                    <div class="fw-500 fs-4">
                      {{ $t("type.account." + account.accountType) }}
                    </div>
                  </div>

                  <div class="table-wrapper">
                    <table class="rebate-detail-table">
                      <thead>
                        <tr class="text-center">
                          <th class="table-title-gray">
                            {{ $t("title.category") }}
                          </th>
                          <th class="table-title-gray">
                            {{ $t("title.totalRebate") }}
                          </th>
                          <th class="table-title-gray">
                            {{ $t("title.personalRebate") }}
                          </th>
                          <th class="table-title-gray">
                            {{ $t("title.remainRebate") }}
                          </th>
                          <th
                            v-if="isRoot && account.pips"
                            class="table-title-gray"
                          >
                            {{ $t("title.pips") }}
                          </th>
                          <th
                            v-if="isRoot && account.commission"
                            class="table-title-gray"
                          >
                            {{ $t("title.commission") }}
                          </th>
                          <th
                            v-if="
                              (account.pips || account.commission) &&
                              account.percentage != 0
                            "
                            class="table-title-blue"
                          >
                            %
                          </th>
                        </tr>
                      </thead>
                      <tbody class="fw-semibold text-gray-600">
                        <tr
                          class="text-center"
                          v-for="(item, idx) in account.items"
                          :key="idx"
                        >
                          <td>
                            {{
                              $t(
                                "type.productCategory." +
                                  props.productCategory.find(
                                    (obj) => obj.key === item.cid
                                  ).value
                              )
                            }}
                          </td>
                          <td>
                            {{
                              props.currentAccountRebateRule[
                                account.accountType
                              ].items[item.cid]
                            }}
                          </td>
                          <td>
                            {{ item.r }}
                          </td>
                          <td>
                            {{
                              props.currentAccountRebateRule[
                                account.accountType
                              ].items[item.cid] < item.r
                                ? 0
                                : calculate(
                                    props.currentAccountRebateRule[
                                      account.accountType
                                    ].items[item.cid],
                                    item.r
                                  )
                            }}
                          </td>
                          <td v-if="isRoot && account.pips">
                            {{
                              defaultLevelSetting[account.accountType]?.find(
                                (acc: any) =>
                                  acc.optionName == account.optionName
                              )?.allowPipSetting[account.pips].items[
                                item.cid
                              ] ?? "-"
                            }}
                          </td>
                          <td v-if="isRoot && account.commission">
                            {{
                              defaultLevelSetting[account.accountType]?.find(
                                (acc: any) =>
                                  acc.optionName == account.optionName
                              )?.allowCommissionSetting[account.commission]
                                .items[item.cid] ?? "-"
                            }}
                          </td>
                          <td
                            v-if="
                              idx == 0 &&
                              (account.pips || account.commission) &&
                              account.percentage != 0
                            "
                            :rowspan="props.productCategory.length"
                            class="percentage-cell"
                          >
                            <el-input
                              class="w-60px"
                              v-model="account.percentage"
                              disabled
                            />
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>

              <!-- Client 类型账户卡片 -->
              <div
                v-if="ibLinkDetail.serviceType == ReferralServiceType.Client"
                class="client-accounts-wrapper"
              >
                <div
                  class="account-card"
                  v-for="(schema, index) in ibLinkDetail.summary
                    .allowAccountTypes"
                  :key="index"
                >
                  <div class="account-card-content">
                    <div class="account-info">
                      <inline-svg
                        src="/images/icons/general/checked.svg"
                        class="check-icon"
                      ></inline-svg>
                      <div class="account-name">
                        <label class="fs-5">{{
                          $t("type.account." + schema.accountType)
                        }}</label>
                        <span class="accountBadge type1">{{
                          schema.optionName == "alpha"
                            ? $t("type.shortAccount.alpha")
                            : schema.optionName
                        }}</span>
                      </div>
                    </div>
                    <div class="account-values">
                      <div class="value-item">
                        <span class="badge badge-success">{{
                          $t("title.pips")
                        }}</span>
                        <inline-svg
                          class="arrow-icon"
                          src="/images/icons/arrows/arrow-right.svg"
                        ></inline-svg>
                        <span class="badge badge-success">{{
                          schema.pips == null || schema.pips == 0
                            ? "0"
                            : schema.pips
                        }}</span>
                      </div>
                      <span class="divider">|</span>
                      <div class="value-item">
                        <span class="badge badge-danger">{{
                          $t("title.commission")
                        }}</span>
                        <inline-svg
                          class="arrow-icon"
                          src="/images/icons/arrows/arrow-right.svg"
                        ></inline-svg>
                        <span class="badge badge-danger">{{
                          schema.commission == null || schema.commission == 0
                            ? "0"
                            : schema.commission
                        }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { showModal } from "@/core/helpers/dom";

import Decimal from "decimal.js";
import { processKeysToCamelCase } from "@/core/services/api.client";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import { ReferralServiceType } from "@/core/types/ReferralServiceType";
import { Field, ErrorMessage, useForm } from "vee-validate";

const props = defineProps<{
  productCategory?: any;
  defaultLevelSetting?: any;
  currentAccountRebateRule?: any;
}>();

const isRoot = ref(false);
const ibLinkDetail = ref({} as any);
const isLoading = ref(true);
const defaultLevelSetting = ref({});
const IBLinkDetailModalRef = ref<null | HTMLElement>(null);

const show = async (_code: string, _isRoot: any) => {
  isLoading.value = true;
  isRoot.value = _isRoot;

  showModal(IBLinkDetailModalRef.value);

  if (props.defaultLevelSetting) {
    defaultLevelSetting.value = JSON.parse(
      JSON.stringify(props.defaultLevelSetting)
    );
  }

  try {
    ibLinkDetail.value = await GlobalService.getReferralCodeSupplement(_code);
    ibLinkDetail.value = processKeysToCamelCase(ibLinkDetail.value);
  } catch (error) {
    // console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const calculate = (a, b) => {
  return new Decimal(a).minus(new Decimal(b)).toString();
};

defineExpose({
  show,
});
</script>

<style scoped>
.ibLinkDetailBackdrop {
  background-color: rgba(0, 0, 0, 0.5);
}

.modal-header {
  padding: 16px 20px;
}

.modal-title-text {
  font-size: 18px;
}

.close-btn {
  cursor: pointer;
  padding: 8px;
}

.form-outer {
  max-height: 70vh;
  overflow-y: auto;
}

/* 表单布局 */
.form-row {
  display: flex;
  gap: 20px;
  margin-bottom: 16px;
}

.form-item {
  flex: 1;
  min-width: 0;
}

.form-label {
  font-size: 14px;
  font-weight: 500;
  color: #606266;
  margin-bottom: 8px;
}

.form-input {
  width: 100%;
  height: 48px;
}

.level-item {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 16px;
}

.level-item .form-label {
  width: 80px;
  margin-bottom: 0;
  flex-shrink: 0;
}

.level-item .form-input {
  flex: 1;
  max-width: 200px;
}

/* 账户区块 */
.account-section {
  margin-bottom: 24px;
}

.account-header {
  display: flex;
  align-items: center;
  padding: 12px 16px;
}

.vertical-line {
  border-left: 3px solid #800020;
  height: 16px;
  margin-right: 12px;
}

/* 表格包装器 */
.table-wrapper {
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
  padding: 0 16px;
}

.rebate-detail-table {
  width: 100%;
  min-width: 500px;
  border-collapse: collapse;
}

.rebate-detail-table th,
.rebate-detail-table td {
  padding: 10px 8px;
  text-align: center;
  white-space: nowrap;
  border-bottom: 1px dashed #e4e6ef;
}

.table-title-gray {
  background-color: #f8f9fa !important;
  font-weight: 600;
  font-size: 13px;
}

.table-title-blue {
  color: white !important;
  background-color: #0053ad !important;
}

.percentage-cell {
  border-left: 1px solid #f5f5f5;
}

/* Client 账户卡片 */
.client-accounts-wrapper {
  padding: 16px;
}

.account-card {
  background-color: #fafbfd;
  border-radius: 10px;
  padding: 14px;
  border: 1px solid #f2f4f7;
  margin-bottom: 12px;
}

.account-card-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 12px;
}

.account-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.check-icon {
  width: 16px;
  height: 16px;
  flex-shrink: 0;
}

.account-name {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.account-values {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.value-item {
  display: flex;
  align-items: center;
  gap: 6px;
}

.arrow-icon {
  width: 12px;
  height: 12px;
}

.divider {
  color: #909399;
}

.accountBadge {
  display: inline-block;
  border-radius: 8px;
  padding: 2px 8px;
  font-size: 10px;
  font-weight: 700;
}

.type1 {
  background: rgba(88, 168, 255, 0.1);
  color: #4196f0;
}

.type2 {
  background: rgba(255, 164, 0, 0.15);
  color: #ffa400;
}

.type3 {
  background: rgba(123, 97, 255, 0.1);
  color: #7b61ff;
}

/* 移动端适配 */
@media (max-width: 768px) {
  .modal-dialog {
    margin: 0;
  }

  .modal-content {
    border-radius: 0 !important;
    min-height: 100vh;
  }

  .modal-title-text {
    font-size: 16px;
  }

  .form-row {
    flex-direction: column;
    gap: 12px;
  }

  .form-input {
    height: 44px;
  }

  .level-item {
    flex-wrap: wrap;
  }

  .level-item .form-label {
    width: 100%;
  }

  .level-item .form-input {
    max-width: none;
  }

  .rebate-detail-table {
    font-size: 13px;
  }

  .rebate-detail-table th,
  .rebate-detail-table td {
    padding: 8px 6px;
  }

  .account-card-content {
    flex-direction: column;
    align-items: flex-start;
  }

  .account-values {
    width: 100%;
    justify-content: flex-start;
    margin-top: 8px;
  }

  .value-item .badge {
    font-size: 11px;
    padding: 4px 6px;
  }
}

@media (max-width: 480px) {
  .form-outer {
    max-height: calc(100vh - 60px);
  }

  .rebate-detail-table {
    font-size: 12px;
    min-width: 400px;
  }

  .table-title-gray {
    font-size: 12px;
  }

  .account-values {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }

  .divider {
    display: none;
  }
}
</style>
